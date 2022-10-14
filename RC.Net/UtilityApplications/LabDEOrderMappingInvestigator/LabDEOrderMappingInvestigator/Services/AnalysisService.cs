using LabDEOrderMappingInvestigator.Models;
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
        string AnalyzeESComponentMap(AnalyzeESComponentMapArgs args);
    }

    /// <inheritdoc/>
    public class AnalysisService : IAnalysisService
    {
        readonly ILabOrderService _labOrderService;

        public AnalysisService(ILabOrderService labOrderService)
        {
            _labOrderService = labOrderService;
        }

        /// <summary>
        /// Analyze the mappings of a customer database wrt a single lab document with expected data
        /// </summary>
        public string AnalyzeESComponentMap(AnalyzeESComponentMapArgs args)
        {
            _ = args ?? throw new ArgumentNullException(nameof(args));

            string customerOMDBPath = Path.Combine(args.ProjectFolder, "Solution", "Database Files", "OrderMappingDB.sqlite");

            IList<LabOrderActual>? expectedOrders;
            if (File.Exists(args.ExpectedDataPath))
            {
                expectedOrders = _labOrderService.LoadFromFile(args.ExpectedDataPath, customerOMDBPath);
            }
            else
            {
                return "Expected data file does not exist!";
            }

            if (expectedOrders is null || expectedOrders.Count == 0)
            {
                return "No expected orders found";
            }

            return AnalyzeExpectedOrders(expectedOrders);
        }

        // Build a text result describing the expected orders
        static string AnalyzeExpectedOrders(IList<LabOrderActual> expectedOrders)
        {
            List<LabOrderActual> unknownOrders = new();
            List<LabOrderDefinition> knownOrders = new();
            foreach (var x in expectedOrders)
            {
                if (x.LabOrderDefinition.HasValue)
                {
                    knownOrders.Add(x.LabOrderDefinition.Value);
                }
                else
                {
                    unknownOrders.Add(x);
                }
            }

            StringBuilder sb = new();
            if (knownOrders.Count > 0)
            {
                AnalyzeKnownOrders(knownOrders, sb);
                sb.AppendLine();
            }

            if (unknownOrders.Count > 0)
            {
                AnalyzeUnknownOrders(unknownOrders, sb);
            }

            return sb.ToString();
        }

        // Add info about the recognized orders
        static void AnalyzeKnownOrders(IList<LabOrderDefinition> knownOrders, StringBuilder sb)
        {
            string label = knownOrders.Count > 1 ? "orders were" : "order was";
            sb.AppendLine(CultureInfo.InvariantCulture, $"{knownOrders.Count} {label} found in the database");
            sb.AppendLine();

            var testsMissingMaps = knownOrders
                .SelectMany(order => order.MandatoryTests.Concat(order.OptionalTests).Where(x => x.ESComponentCodes.Count == 0))
                .DistinctBy(test => test.Code)
                .ToList();

            if (testsMissingMaps.Count > 0)
            {
                sb.AppendLine("The following tests are missing URS mappings:");
                foreach (var test in testsMissingMaps)
                {
                    sb.Append(CultureInfo.InvariantCulture, $"  Name: {test.OfficialName}, Code: {test.Code}, AKAs: ");
                    sb.AppendJoin("; ", test.AKAs);
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("No expected tests are missing URS mappings");
            }
        }

        // Add info about any unrecognized orders
        static void AnalyzeUnknownOrders(IList<LabOrderActual> unknownOrders, StringBuilder sb)
        {
            string label = unknownOrders.Count > 1 ? "orders were" : "order was";
            sb.AppendLine(CultureInfo.InvariantCulture, $"{unknownOrders.Count} {label} not found in the database:");
            foreach (var order in unknownOrders)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"  Name: {order.Name}, Code: {order.Code}, TestCount: {order.Tests.Count}");
            }
        }
    }
}
