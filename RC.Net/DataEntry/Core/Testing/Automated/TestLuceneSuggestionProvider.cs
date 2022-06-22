using Extract.Testing.Utilities;
using Extract.Utilities;
using Lucene.Net.Support.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.DataEntry.Test
{
    /// <summary>
    /// Test cases for LuceneSuggestionProvider. This is not meant to be comprehensive at this time but to include
    /// test cases for new features.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("LuceneSuggestionProvider")]
    public class TestLuceneSuggestionProvider
    {
        #region Fields

        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestLuceneSuggestionProvider> _testFiles;

        #endregion Fields

        #region Setup and Teardown

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestLuceneSuggestionProvider>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }
        }

        #endregion Setup and Teardown

        #region Public Test Functions


        /// <summary>
        /// Test that typing LabDE URS component AKAs results in the target name being near the top of the suggestion list
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "AKAs")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "AKAs")]
        [Test, Category("LuceneSuggestionProvider")]
        public static void LabDEComponentAKAs()
        {
            var testDataCSV = _testFiles.GetFile("Resources.LuceneSuggestionProvider.urs_names_and_akas.csv");
            var testData = GetTestData(testDataCSV);
            var testDataDict = testData.ToLookup(a => a[0], a => a.Skip(1));
            var provider = new LuceneSuggestionProvider<IGrouping<string, IEnumerable<string>>>(
                testDataDict.AsEnumerable(),
                s => s.Key,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s.Key), 1)
                    .Concat(s.SelectMany(akas => akas.Select(aka => new KeyValuePair<string, string>("AKA", aka)))));

            var results = GetTestNamesResults(provider, testData);
            WriteResultsIfDebug(results);

            var medians = results.Medians;
            var avgs = results.Averages;

            Assert.LessOrEqual(medians[0], 4);
            Assert.LessOrEqual(medians[1], 3);
            Assert.LessOrEqual(medians[2], 3);

            Assert.LessOrEqual(avgs[0], 6.1);
            Assert.LessOrEqual(avgs[1], 4.4);
            Assert.LessOrEqual(avgs[2], 3.7);
        }

        /// <summary>
        /// Test that typing LabDE Lab Display Names results in the target appearing near the top of the list
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]
        public static void Individual()
        {
            var testDataText = _testFiles.GetFile("Resources.LuceneSuggestionProvider.demo_order_names.txt");
            IEnumerable<string> testData = File.ReadAllLines(testDataText);
            var provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s), 1));
            var suggestions = provider.GetSuggestions("Electro", excludeLowScoring: true);
            Assert.AreEqual("ELECTROLYTE PANEL", suggestions[0]);

            var testDataCSV = _testFiles.GetFile("Resources.LuceneSuggestionProvider.demo_lab_display_names.csv");
            testData = GetTestData(testDataCSV).Select(l => l[0]);
            provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s.Replace("MEMORIAL", "LAIROMEM")), 1));
            suggestions = provider.GetSuggestions("AMERICAN", excludeLowScoring: true);
            var suggestions2 = provider.GetSuggestions("AMERICAN ", excludeLowScoring: true);
            Assert.AreEqual(suggestions[0], suggestions2[0]);
            Assert.AreEqual("AMERICAN", suggestions[0].Substring(0, 8));
        }

        /// <summary>
        /// Test that typing LabDE Lab Display Names results in the target appearing near the top of the list
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]
        public static void LabDELabNames()
        {
            var testDataCSV = _testFiles.GetFile("Resources.LuceneSuggestionProvider.demo_lab_display_names.csv");
            var testData = GetTestData(testDataCSV).Select(l => l[0]).ToList();
            var provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s), 1));
            int testCases = testData.Count;

            var results = GetResults(provider, testData);
            WriteResultsIfDebug(results);
            var medians = results.Medians;
            var avgs = results.Averages;

            var ver11Avgs = new[]
            {
                8.4678571428571434,
                6.9767857142857146,
                6.1875,
                5.7089285714285714,
                5.4357142857142859,
            };
            var ver11Total = ver11Avgs.Sum(l => l * testCases);
            var thisTotal = results.Total;
            Assert.Less(thisTotal, ver11Total);

            Assert.LessOrEqual(medians[0], 7);
            Assert.LessOrEqual(medians[1], 6);
            Assert.LessOrEqual(medians[2], 5);
            Assert.LessOrEqual(medians[3], 4);
            Assert.LessOrEqual(medians[4], 4);

            Assert.LessOrEqual(avgs[0], 8.1);
            Assert.LessOrEqual(avgs[1], 6.5);
            Assert.LessOrEqual(avgs[2], 5.7);
            Assert.LessOrEqual(avgs[3], 5.3);
            Assert.LessOrEqual(avgs[4], 5.0);
        }

        /// <summary>
        /// Test that typing part of specific document type results in the target appearing at the top of the list
        /// https://extract.atlassian.net/browse/ISSUE-17302
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]
        public static void SpecificNemoursDocumentTypes()
        {
            var testDataText = _testFiles.GetFile("Resources.LuceneSuggestionProvider.doctypes.txt");
            IEnumerable<string> testData = File.ReadAllLines(testDataText);
            var provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s), 1));
            var suggestions = provider.GetSuggestions("diagnosti", excludeLowScoring: true);
            Assert.AreEqual("Diagnostic Studies", suggestions[0]);
        }

        /// <summary>
        /// Test that typing all but the first char of a specific department results in the target appearing at the top of the list
        /// https://extract.atlassian.net/browse/ISSUE-17302
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]
        public static void SpecificNemoursDepartments()
        {
            var testDataText = _testFiles.GetFile("Resources.LuceneSuggestionProvider.departments.txt");
            IEnumerable<string> testData = File.ReadAllLines(testDataText);
            var provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s), 1));
            var suggestions = provider.GetSuggestions("klaud", excludeLowScoring: true);
            Assert.AreEqual("LKLAUD", suggestions[0]);
        }

        /// <summary>
        /// Test that typing document types results in the target appearing near the top of the list
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]
        public static void NemoursDocumentTypes()
        {
            var testDataText = _testFiles.GetFile("Resources.LuceneSuggestionProvider.doctypes.txt");
            var testData = File.ReadAllLines(testDataText);
            var provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s), 1));

            var results = GetResults(provider, testData);
            WriteResultsIfDebug(results);
            var medians = results.Medians;
            var avgs = results.Averages;

            Assert.LessOrEqual(medians[0], 6);
            Assert.LessOrEqual(medians[1], 4);
            Assert.LessOrEqual(medians[2], 3);
            Assert.LessOrEqual(medians[3], 3);
            Assert.LessOrEqual(medians[4], 3);

            Assert.LessOrEqual(avgs[0], 8.2);
            Assert.LessOrEqual(avgs[1], 6.6);
            Assert.LessOrEqual(avgs[2], 5.6);
            Assert.LessOrEqual(avgs[3], 5.1);
            Assert.LessOrEqual(avgs[4], 4.8);
        }

        /// <summary>
        /// Test that the suggestion list doesn't change wildly after typing a space after a first or second word
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]
        public static void SuggestionStability()
        {
            var testDataCSV = _testFiles.GetFile("Resources.LuceneSuggestionProvider.demo_lab_display_names.csv");
            var testData = GetTestData(testDataCSV).Select(l => l[0]).ToList();
            var provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s), 1));

            var results = GetStabilityResults(provider, testData);
            WriteResultsIfDebug(results);

            // The whole list changes around somewhat
            Assert.Less(results.AvgOneWordChanges, 1.342);
            Assert.Less(results.AvgTwoWordChanges, 2.127);

            // The top five suggestions change less
            Assert.Less(results.AvgOneWordChangesTop5, 0.377);
            Assert.Less(results.AvgTwoWordChangesTop5, 0.551);

            // The top suggestion should not change very often at all
            Assert.Less(results.AvgOneWordChangesTop1, 0.077);
            Assert.Less(results.AvgTwoWordChangesTop1, 0.004);
        }

        /// <summary>
        /// Test that a method called via the LuceneSuggestionProvider constructor is thread-safe
        /// NOTE: This test often passes even with the non-thread-safe caching but if you run it, e.g.,
        /// 100 times it tends to fail half a dozen times
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]
        public static void GetCanonicalPath()
        {
            int i = 0;
            Parallel.For(0, 5000, _ =>
            {
                Interlocked.Increment(ref i);
                var tempDirInfo = FileSystemMethods.GetTemporaryFolder(Path.Combine(Path.GetTempPath(), "SuggestionProvider"), true);
                Assert.DoesNotThrow(() => tempDirInfo.GetCanonicalPath(), message: UtilityMethods.FormatInvariant($"Iteration: {i}"));
            });
        }

        [Test, Category("LuceneAutoSuggest"), Category("Interactive")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        [RequiresThread(ApartmentState.STA)]
        public static void AutoSuggestControls()
        {
            using var demo = new AutoSuggestDemo();
            demo.Run();
        }

        #endregion

        #region Private Methods

        private static List<string[]> GetTestData(string testDataCSV)
        {
            var testDataList = new List<string[]>();
            using (var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(testDataCSV))
            {
                csvReader.Delimiters = new[] { "," };
                while (!csvReader.EndOfData)
                {
                    var fields = csvReader.ReadFields();
                    testDataList.Add(fields);
                }
            }
            return testDataList;
        }


        static KeysToTopResults GetTestNamesResults<TProvider>(
            LuceneSuggestionProvider<TProvider> provider,
            List<string[]> testData)
        {
            int numTestCases = testData.Count;
            int numSpots = 3;

            // Determine how many key presses are required to bring the target
            // test name into the top 3 spots, top 2 spots and top 1 spot.
            var results = new KeysToTopResults(numTestCases, numSpots);

            for (int testCase = 0; testCase < numTestCases; testCase++)
            {
                var pair = testData[testCase];
                var officialName = pair[0];
                var aka = pair[1];
                var spots = new bool[numSpots];

                // Init lists for this case
                for (int i = 0; i < numSpots; i++)
                {
                    // Initialize spots with the number of characters in the AKA
                    // Some AKAs are ambiguous so targets will never reach the top spot...
                    // This shouldn't have too great an impact on the numbers, though
                    results.KeysToTheTopX[i].Add(aka.Length);
                }

                // Pick a random word to start typing from
                // Use a fixed-seed rng so that the unit test will always pass
                var words = aka.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                CollectionMethods.Shuffle(words, new Random(0));
                aka = string.Join(" ", words);
                for (int keyPresses = 1; keyPresses <= aka.Length; keyPresses++)
                {
                    var suggestions = provider.GetSuggestions(aka.Substring(0, keyPresses), spots.Length, true);
                    var matchIdx = suggestions.IndexOf(officialName);
                    if (matchIdx >= 0)
                    {
                        for (int i = matchIdx; i < spots.Length; i++)
                        {
                            if (!spots[i])
                            {
                                spots[i] = true;
                                results.KeysToTheTopX[i][testCase] = keyPresses;
                            }
                        }

                        if (matchIdx == 0)
                        {
                            break;
                        }
                    }
                }
            }

            results.Calculate();
            return results;
        }

        static KeysToTopResults GetResults<TProvider>(
            LuceneSuggestionProvider<TProvider> provider,
            IList<string> testData)
        {
            int numTestCases = testData.Count;

            // Determine how many key presses are required to bring the target
            // lab name into the top 1, 2, etc spots
            int numSpots = 5;
            var results = new KeysToTopResults(numTestCases, numSpots);
            for (int testCase = 0; testCase < numTestCases; testCase++)
            {
                var name = testData[testCase];
                var spots = new bool[numSpots];

                // Init lists for this case
                for (int i = 0; i < numSpots; i++)
                {
                    // Initialize spots with the number of characters in the name
                    results.KeysToTheTopX[i].Add(name.Length);
                }

                // Pick a random word to start typing from
                // Use a fixed-seed rng so that the unit test will always pass
                var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                CollectionMethods.Shuffle(words, new Random(0));
                var jumbled = string.Join(" ", words);

                for (int keyPresses = 1; keyPresses <= jumbled.Length; keyPresses++)
                {
                    var suggestions = provider.GetSuggestions(jumbled.Substring(0, keyPresses), spots.Length);
                    var matchIdx = suggestions.IndexOf(name);
                    if (matchIdx >= 0)
                    {
                        for (int i = matchIdx; i < spots.Length; i++)
                        {
                            if (!spots[i])
                            {
                                spots[i] = true;
                                results.KeysToTheTopX[i][testCase] = keyPresses;
                            }
                        }

                        if (matchIdx == 0)
                        {
                            break;
                        }
                    }
                }
            }

            results.Calculate();
            return results;
        }

        static SuggestionStabilityResults GetStabilityResults<TProvider>(
            LuceneSuggestionProvider<TProvider> provider,
            List<string> testData)
        {
            int numTestCases = testData.Count;

            // Determine how much the list varies after completing a first or second word with a space
            var results = new SuggestionStabilityResults(numTestCases);

            for (int testCase = 0; testCase < numTestCases; testCase++)
            {
                var name = testData[testCase];
                results.Variances[0][testCase] =
                    results.Variances[1][testCase] =
                    results.VariancesTop5[0][testCase] =
                    results.VariancesTop5[1][testCase] =
                    results.VariancesTop1[0][testCase] =
                    results.VariancesTop1[1][testCase] = 0;

                var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (int wordsStarted = 1; wordsStarted <= words.Length && wordsStarted < 3; wordsStarted++)
                {
                    var searchString = string.Join(" ", words.Take(wordsStarted));
                    var suggestionsIncomplete = provider.GetSuggestions(searchString, excludeLowScoring: true);
                    var suggestionsComplete = provider.GetSuggestions(searchString + " ", excludeLowScoring: true);

                    var sections = DiffLib.Diff.CalculateSections(suggestionsIncomplete, suggestionsComplete);
                    var diffCount = sections.Count(s => !s.IsMatch);

                    var sectionsTop5 = DiffLib.Diff.CalculateSections(
                        suggestionsIncomplete.Take(5).ToList(),
                        suggestionsComplete.Take(5).ToList());
                    var diffCountTop5 = sectionsTop5.Count(s => !s.IsMatch);
                    results.Variances[wordsStarted - 1][testCase] = diffCount;
                    results.VariancesTop5[wordsStarted - 1][testCase] = diffCountTop5;
                    results.VariancesTop1[wordsStarted - 1][testCase] = suggestionsIncomplete[0] == suggestionsComplete[0]
                        ? 0
                        : 1;
                }
            }

            results.Calculate();
            return results;
        }

        // Write out json file to track test results and to aid in updating the expected numbers in the tests
        static void WriteResultsIfDebug<T>(T results, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFile = "")
        {
#if DEBUG
            var outputPath = Path.Combine(Path.GetDirectoryName(sourceFile), "CurrentTestResults", memberName + ".json");
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(results, Formatting.Indented));
#endif
        }

        #endregion

        #region Private Classes

        class KeysToTopResults
        {
            [JsonIgnore]
            public List<int>[] KeysToTheTopX { get; }

            public List<int> Medians { get; private set; }
            public List<double> Averages { get; private set; }
            public int Total { get; private set; }

            public KeysToTopResults(int numTestCases, int numSpots)
            {
                KeysToTheTopX = new List<int>[numSpots];
                for (int i = 0; i < numSpots; i++)
                {
                    KeysToTheTopX[i] = new List<int>(numTestCases);
                }
            }

            public void Calculate()
            {
                foreach (var spot in KeysToTheTopX)
                {
                    spot.Sort();
                }
                Medians = KeysToTheTopX.Select(a => a[a.Count / 2]).ToList();
                Averages = KeysToTheTopX.Select(a => (double)a.Sum() / a.Count).ToList();
                Total = KeysToTheTopX.SelectMany(l => l).Sum();
            }
        }

        class SuggestionStabilityResults
        {
            [JsonIgnore]
            public int[][] Variances { get; }
            [JsonIgnore]
            public int[][] VariancesTop5 { get; }
            [JsonIgnore]
            public int[][] VariancesTop1 { get; }

            public double AvgOneWordChanges { get; private set; }
            public double AvgTwoWordChanges { get; private set; }
            public double AvgOneWordChangesTop5 { get; private set; }
            public double AvgTwoWordChangesTop5 { get; private set; }
            public double AvgOneWordChangesTop1 { get; private set; }
            public double AvgTwoWordChangesTop1 { get; private set; }

            public SuggestionStabilityResults(int numTestCases)
            {
                Variances = MakeArray(numTestCases);
                VariancesTop5 = MakeArray(numTestCases);
                VariancesTop1 = MakeArray(numTestCases);
            }

            public void Calculate()
            {
                AvgOneWordChanges = Variances[0].Average();
                AvgTwoWordChanges = Variances[1].Average();
                AvgOneWordChangesTop5 = VariancesTop5[0].Average();
                AvgTwoWordChangesTop5 = VariancesTop5[1].Average();
                AvgOneWordChangesTop1 = VariancesTop1[0].Average();
                AvgTwoWordChangesTop1 = VariancesTop1[1].Average();
            }

            static int[][] MakeArray(int numTestCases)
            {
                return new int[][]
                {
                    new int[numTestCases],
                    new int[numTestCases]
                };
            }
        }

        #endregion
    }
}
