using LabDEOrderMappingInvestigator.Models;
using LabDEOrderMappingInvestigator.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LabDEOrderMappingInvestigator.Services
{
    /// <summary>
    /// Service with methods to read/analyze order/test data from VOA files and Sqlite databases
    /// </summary>
    public interface IAnalysisService
    {
        /// <summary>
        /// Analyze the mappings of a customer database wrt a single lab document with expected data
        /// </summary>
        OutputMessageViewModelBase AnalyzeESComponentMap(AnalyzeESComponentMapArgs args);
    }

    /// <inheritdoc/>
    public class AnalysisService : IAnalysisService
    {
        readonly ILabOrderFileService _labOrderFileService;
        readonly ILabOrderDatabaseService _labOrderDatabaseService;
        readonly ILabTestMappingSuggestionService _labTestMappingSuggestionService;
        readonly ILabTestMatchListViewModelFactory _labTestMatchListViewModelFactory;
        readonly IRulesAccuracyService _rulesAccuracyService;

        /// <summary>
        /// Create an instance of the analysis service
        /// </summary>
        public AnalysisService(
            ILabOrderFileService labOrderFileService,
            ILabOrderDatabaseService labOrderDatabaseService,
            ILabTestMappingSuggestionService labTestMappingSuggestionService,
            ILabTestMatchListViewModelFactory labTestMatchListViewModelFactory,
            IRulesAccuracyService rulesAccuracyService)
        {
            _labOrderFileService = labOrderFileService;
            _labOrderDatabaseService = labOrderDatabaseService;
            _labTestMappingSuggestionService = labTestMappingSuggestionService;
            _labTestMatchListViewModelFactory = labTestMatchListViewModelFactory;
            _rulesAccuracyService = rulesAccuracyService;
        }

        /// <summary>
        /// Analyze the mappings of a customer database wrt a single lab document with expected data
        /// </summary>
        public OutputMessageViewModelBase AnalyzeESComponentMap(AnalyzeESComponentMapArgs args)
        {
            _ = args ?? throw new ArgumentNullException(nameof(args));

            IList<LabOrderActual>? expectedOrders;
            if (File.Exists(args.ExpectedDataPath))
            {
                expectedOrders = _labOrderDatabaseService.UpdateDefinitions(
                    _labOrderFileService.LoadLabOrdersFromFile(args.ExpectedDataPath, args.CustomerOMDBPath),
                    args.CustomerOMDBPath);
            }
            else
            {
                return new ErrorOutputMessageViewModel("Expected data file does not exist!");
            }

            if (expectedOrders is null || expectedOrders.Count == 0)
            {
                return new ErrorOutputMessageViewModel("No expected orders found");
            }

            IList<LabOrderActual>? foundOrders = null;
            if (File.Exists(args.FoundDataPath))
            {
                foundOrders = _labOrderDatabaseService.UpdateDefinitions(
                    _labOrderFileService.LoadLabOrdersFromFile(args.FoundDataPath, args.CustomerOMDBPath),
                    args.CustomerOMDBPath);
            }

            IList<LabTestExtract> extractTests = _labOrderDatabaseService.LoadLabTestsFromExtractDatabase(args.ExtractOMDBPath, args.CustomerOMDBPath);
            IList<(LabTestActual, IList<LabTestMatch>)> suggestions = SuggestNewMappings(extractTests, expectedOrders, foundOrders);

            string textResult = AnalyzeExpectedOrders(expectedOrders);

            RulesAccuracyResult? rulesAccuracyResult = null;
            if (foundOrders is not null)
            {
                rulesAccuracyResult = _rulesAccuracyService.CalculateAccuracy(expectedOrders, foundOrders);

                if (rulesAccuracyResult.IsAnythingMissingOrIncorrect)
                {
                    StringBuilder sb = new();
                    sb.AppendLine(textResult);
                    sb.Append(CultureInfo.InvariantCulture, $"Missed orders: {rulesAccuracyResult.MissedOrders.Count}");
                    sb.Append(CultureInfo.InvariantCulture, $", Incorrect orders: {rulesAccuracyResult.IncorrectOrders.Count}");
                    sb.Append(CultureInfo.InvariantCulture, $", Missed tests: {rulesAccuracyResult.MissedTests.Count}");
                    sb.Append(CultureInfo.InvariantCulture, $", Incorrect tests: {rulesAccuracyResult.IncorrectTests.Count}");

                    textResult = sb.ToString();
                }
                else
                {
                    textResult += Environment.NewLine + "No missing/incorrect orders or tests";
                }
            }

            if (suggestions.Any())
            {
                return new MappingSuggestionsOutputMessageViewModel(
                    textResult,
                    rulesAccuracyResult,
                    suggestions.Select(matchInfo =>
                    {
                        var (customerTest, labTestMatches) = matchInfo;
                        return _labTestMatchListViewModelFactory.Create(customerTest, labTestMatches);
                    }),
                    args.InitialLabTestFilter);
            }

            return new TextOutputMessageViewModel(textResult);
        }

        // Compute a list of suggested additions to the ComponentToESComponent map table
        IList<(LabTestActual, IList<LabTestMatch>)> SuggestNewMappings(
            IList<LabTestExtract> extractTests,
            IList<LabOrderActual> expectedOrders,
            IList<LabOrderActual>? foundOrders)
        {
            List<LabOrderActual> knownOrders = expectedOrders.Where(x => x.LabOrderDefinition.HasValue).ToList();
            if (foundOrders is not null)
            {
                knownOrders.AddRange(foundOrders);
            }

            List<LabTestActual> testsInOrders =
                knownOrders.SelectMany(o => o.Tests)
                .Where(t => t.LabTestDefinition.HasValue)
                .DistinctBy(t => t.LabTestDefinition.Value.Code)
                .OrderBy(t => t.LabTestDefinition.Value.Code)
                .ToList();

            var suggestions = _labTestMappingSuggestionService.GetSuggestions(extractTests, testsInOrders, 10);

            var testsAndSuggestions = testsInOrders.Zip(suggestions).ToList();

            // Make sure that any existing mappings show up in the results, even if they are not a top-10 suggestion
            return AddExistingMappingsAsSuggestedMatches(extractTests, testsAndSuggestions);
        }

        // Ensure that any existing ESComponentCodes for a test are represented in the suggested matches
        static IList<(LabTestActual, IList<LabTestMatch>)> AddExistingMappingsAsSuggestedMatches(
            IList<LabTestExtract> extractTests,
            IList<(LabTestActual, IList<LabTestMatch>)> testsAndSuggestions)
        {
            Dictionary<string, LabTestExtract> extractTestCodeLookup = extractTests.ToDictionary(x => x.Code);
            return testsAndSuggestions
                .Select(tuple =>
                {
                    var (customerTest, matches) = tuple;
                    var missing = customerTest.LabTestDefinition.Value.ESComponentCodes
                        .Except(matches.Select(m => m.ExtractTest.Code))
                        .Where(code => extractTestCodeLookup.ContainsKey(code))
                        .Select(code => new LabTestMatch(customerTest, extractTestCodeLookup[code], double.NaN))
                        .ToList();

                    if (missing.Any())
                    {
                        return (customerTest, matches.Concat(missing).ToList());
                    }
                    else
                    {
                        return tuple;
                    }
                })
                .ToList();
        }

        // Build a text result describing the expected orders
        static string AnalyzeExpectedOrders(IList<LabOrderActual> expectedOrders)
        {
            List<LabOrderActual> unknownOrders = new();
            List<LabOrderActual> knownOrders = new();
            foreach (var x in expectedOrders)
            {
                if (x.LabOrderDefinition.HasValue)
                {
                    knownOrders.Add(x);
                }
                else
                {
                    unknownOrders.Add(x);
                }
            }

            StringBuilder result = new();
            result.Append("Analysis result:");
            if (knownOrders.Count > 0)
            {
                result.AppendLine();
                result.Append(AnalyzeKnownOrders(knownOrders));
            }

            if (unknownOrders.Count > 0)
            {
                result.AppendLine();
                result.Append(AnalyzeUnknownOrders(unknownOrders));
            }

            return result.ToString();
        }

        // Add info about the recognized orders
        static string AnalyzeKnownOrders(IList<LabOrderActual> knownOrders)
        {
            StringBuilder result = new();

            string label = knownOrders.Count > 1 ? "orders" : "order";
            result.Append(CultureInfo.InvariantCulture, $"{knownOrders.Count} {label} found in the database");

            var testsMissingMaps = knownOrders
                .SelectMany(order => order.Tests.Where(x => x.LabTestDefinition.HasValue && x.LabTestDefinition.Value.ESComponentCodes.Count == 0))
                .DistinctBy(test => test.Code)
                .ToList();

            if (testsMissingMaps.Count > 0)
            {
                label = testsMissingMaps.Count > 1 ? "expected tests" : "expected test";
                result.AppendLine();
                result.Append(CultureInfo.InvariantCulture, $"{testsMissingMaps.Count} {label} missing URS mappings");
            }
            else
            {
                result.AppendLine();
                result.Append("No expected tests missing URS mappings");
            }

            return result.ToString();
        }

        // Add info about any unrecognized orders
        static string AnalyzeUnknownOrders(IList<LabOrderActual> unknownOrders)
        {
            StringBuilder result = new();

            string label = unknownOrders.Count > 1 ? "orders" : "order";
            result.Append(CultureInfo.InvariantCulture, $"{unknownOrders.Count} {label} not found in the database:");
            foreach (var order in unknownOrders)
            {
                result.AppendLine();
                result.Append(CultureInfo.InvariantCulture, $"  Name: {order.Name}, Code: {order.Code}, TestCount: {order.Tests.Count}");
            }

            return result.ToString();
        }
    }
}
