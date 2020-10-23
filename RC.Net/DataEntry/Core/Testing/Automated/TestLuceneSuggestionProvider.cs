using Extract.Testing.Utilities;
using Extract.Utilities;
using Lucene.Net.Support.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
        public static void TestLabDEComponentAKAs()
        {
            var testDataCSV = _testFiles.GetFile("Resources.LuceneSuggestionProvider.urs_names_and_akas.csv");
            var testData = GetTestData(testDataCSV);
            var testDataDict = testData.ToLookup(a => a[0], a => a.Skip(1));
            var provider = new LuceneSuggestionProvider<IGrouping<string, IEnumerable<string>>>(
                testDataDict.AsEnumerable(),
                s => s.Key,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s.Key), 1)
                    .Concat(s.SelectMany(akas => akas.Select(aka => new KeyValuePair<string, string>("AKA", aka)))));

            var keysToTheTopX = GetTestNamesResults(provider, testData);

            int testCases = testData.Count;
            var medians = keysToTheTopX.Select(a => a[testCases / 2]).ToList();
            var avgs = keysToTheTopX.Select(a => (double)a.Sum() / testCases).ToList();

            Assert.LessOrEqual(medians[0], 5);
            Assert.LessOrEqual(medians[1], 3);
            Assert.LessOrEqual(medians[2], 3);

            Assert.LessOrEqual(avgs[0], 6.5);
            Assert.LessOrEqual(avgs[1], 4.75);
            Assert.LessOrEqual(avgs[2], 4);
        }


        /// <summary>
        /// Test that typing LabDE Lab Display Names results in the target appearing near the top of the list
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]
        public static void TestIndividual()
        {
            var testDataText = _testFiles.GetFile("Resources.LuceneSuggestionProvider.demo_order_names.txt");
            IEnumerable<string> testData = File.ReadAllLines(testDataText);
            var provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s), 1));
            var suggestions = provider.GetSuggestions("Electro", excludeLowScoring: true).ToList();
            Assert.AreEqual("ELECTROLYTE PANEL", suggestions[0]);

            var testDataCSV = _testFiles.GetFile("Resources.LuceneSuggestionProvider.demo_lab_display_names.csv");
            testData = GetTestData(testDataCSV).Select(l => l[0]).ToList();
            provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s.Replace("MEMORIAL", "LAIROMEM")), 1));
            suggestions = provider.GetSuggestions("AMERICAN", excludeLowScoring: true).ToList();
            var suggestions2 = provider.GetSuggestions("AMERICAN ", excludeLowScoring: true).ToList();
            Assert.AreEqual(suggestions[0], suggestions2[0]);
            Assert.AreEqual("AMERICAN", suggestions[0].Substring(0, 8));
        }

        /// <summary>
        /// Test that typing LabDE Lab Display Names results in the target appearing near the top of the list
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]        
        public static void TestLabDELabNames()
        {
            var testDataCSV = _testFiles.GetFile("Resources.LuceneSuggestionProvider.demo_lab_display_names.csv");
            var testData = GetTestData(testDataCSV).Select(l => l[0]).ToList();
            var provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s), 1));
            int testCases = testData.Count;

            var keysToTheTopX = GetResults(provider, testData);
            var medians = keysToTheTopX.Select(a => a[testCases / 2]).ToList();
            var avgs = keysToTheTopX.Select(a => (double)a.Sum()/testCases).ToList();

            var ver11Avgs = new[]
            {
                8.4678571428571434,
                6.9767857142857146,
                6.1875,
                5.7089285714285714,
                5.4357142857142859,
            };
            var ver11Total = ver11Avgs.Sum(l => l * testCases);
            var thisTotal = keysToTheTopX.SelectMany(l => l).Sum();
            Assert.Less(thisTotal, ver11Total);

            Assert.LessOrEqual(medians[0], 7);
            Assert.LessOrEqual(medians[1], 6);
            Assert.LessOrEqual(medians[2], 5);
            Assert.LessOrEqual(medians[3], 4);
            Assert.LessOrEqual(medians[4], 4);

            Assert.LessOrEqual(avgs[0], 8.2);
            Assert.LessOrEqual(avgs[1], 6.6);
            Assert.LessOrEqual(avgs[2], 5.7);
            Assert.LessOrEqual(avgs[3], 5.3);
            Assert.LessOrEqual(avgs[4], 5.0);
        }

        /// <summary>
        /// Test that the suggestion list doesn't change wildly after typing a space after a first or second word
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]        
        public static void TestSuggestionStability()
        {
            var testDataCSV = _testFiles.GetFile("Resources.LuceneSuggestionProvider.demo_lab_display_names.csv");
            var testData = GetTestData(testDataCSV).Select(l => l[0]).ToList();
            var provider = new LuceneSuggestionProvider<string>(
                testData,
                s => s,
                s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s), 1));

            var (variances, variancesTop5, variancesTop1) = GetStabilityResults(provider, testData);
            var avgOneWordChanges = variances[0].Average();
            var avgTwoWordChanges = variances[1].Average();
            var avgOneWordChangesTop5 = variancesTop5[0].Average();
            var avgTwoWordChangesTop5 = variancesTop5[1].Average();
            var avgOneWordChangesTop1 = variancesTop1[0].Average();
            var avgTwoWordChangesTop1 = variancesTop1[1].Average();

            // The whole list changes around somewhat
            Assert.Less(avgOneWordChanges, 1.356);
            Assert.Less(avgTwoWordChanges, 1.908);

            // The top five suggestions change less
            Assert.Less(avgOneWordChangesTop5, 0.404);
            Assert.Less(avgTwoWordChangesTop5, 0.556);

            // The top suggestion should not change very often at all
            Assert.Less(avgOneWordChangesTop1, 0.079);
            Assert.Less(avgTwoWordChangesTop1, 0.009);
        }

        /// <summary>
        /// Test that a method called via the LuceneSuggestionProvider constructor is thread-safe
        /// NOTE: This test often passes even with the non-thread-safe caching but if you run it, e.g.,
        /// 100 times it tends to fail half a dozen times
        /// </summary>
        [Test, Category("LuceneSuggestionProvider")]
        public static void TestGetCanonicalPath()
        {
            int i = 0;
            Parallel.For(0, 5000, _ =>
            {
                Interlocked.Increment(ref i);
                var tempDirPath = Path.Combine(Path.GetTempPath(), "SuggestionProvider", Path.GetRandomFileName());
                var tempDirInfo = new DirectoryInfo(tempDirPath);
                Assert.DoesNotThrow(() => tempDirInfo.GetCanonicalPath(), message: UtilityMethods.FormatInvariant($"Iteration: {i}"));
            });
        }

        [Test, Category("LuceneAutoSuggest"), Category("Interactive")]
        public static void TestAutoSuggestControls()
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

        static List<int>[] GetTestNamesResults<TProvider>(
            LuceneSuggestionProvider<TProvider> provider,
            List<string[]> testData)
        {
            int numTestCases = testData.Count;

            // Determine how many key presses are required to bring the target
            // test name into the top 3 spots, top 2 spots and top 1 spot.
            var keysToTheTopX = new List<int>[]
            {
                new List<int>(numTestCases),
                new List<int>(numTestCases),
                new List<int>(numTestCases),
            };

            int numSpots = keysToTheTopX.Length;
            for(int testCase=0; testCase < numTestCases; testCase++)
            {
                var pair = testData[testCase];
                var officialName = pair[0];
                var aka = pair[1];
                var spots = new bool[numSpots];

                // Init lists for this case
                for (int i=0; i<numSpots;i++)
                {
                    // Initialize spots with the number of characters in the AKA
                    // Some AKAs are ambiguous so targets will never reach the top spot...
                    // This shouldn't have too great an impact on the numbers, though
                    keysToTheTopX[i].Add(aka.Length);
                }

                // Pick a random word to start typing from
                // Use a fixed-seed rng so that the unit test will always pass
                var words = aka.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                CollectionMethods.Shuffle(words, new Random(0));
                aka = string.Join(" ", words);
                for (int keyPresses = 1; keyPresses <= aka.Length; keyPresses++)
                {
                    var suggestions = provider.GetSuggestions(aka.Substring(0, keyPresses), spots.Length, true).ToList();
                    var matchIdx = suggestions.IndexOf(officialName);
                    if (matchIdx >= 0)
                    {
                        for (int i = matchIdx; i < spots.Length; i++)
                        {
                            if (!spots[i])
                            {
                                spots[i] = true;
                                keysToTheTopX[i][testCase] = keyPresses;
                            }
                        }

                        if (matchIdx == 0)
                        {
                            break;
                        }
                    }
                }
            }
            foreach(var spot in keysToTheTopX)
            {
                spot.Sort();
            }
            return keysToTheTopX;
        }

        static List<int>[] GetResults<TProvider>(
            LuceneSuggestionProvider<TProvider> provider,
            List<string> testData)
        {
            int numTestCases = testData.Count;

            // Determine how many key presses are required to bring the target
            // lab name into the top 1, 2, etc spots
            var keysToTheTopX = new List<int>[]
            {
                new List<int>(numTestCases),
                new List<int>(numTestCases),
                new List<int>(numTestCases),
                new List<int>(numTestCases),
                new List<int>(numTestCases),
            };

            int numSpots = keysToTheTopX.Length;
            for (int testCase = 0; testCase < numTestCases; testCase++)
            {
                var name = testData[testCase];
                var spots = new bool[numSpots];

                // Init lists for this case
                for (int i = 0; i < numSpots; i++)
                {
                    // Initialize spots with the number of characters in the name
                    keysToTheTopX[i].Add(name.Length);
                }

                // Pick a random word to start typing from
                // Use a fixed-seed rng so that the unit test will always pass
                var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                CollectionMethods.Shuffle(words, new Random(0));
                var jumbled = string.Join(" ", words);

                for (int keyPresses = 1; keyPresses <= jumbled.Length; keyPresses++)
                {
                    var suggestions = provider.GetSuggestions(jumbled.Substring(0, keyPresses), spots.Length).ToList();
                    var matchIdx = suggestions.IndexOf(name);
                    if (matchIdx >= 0)
                    {
                        for (int i = matchIdx; i < spots.Length; i++)
                        {
                            if (!spots[i])
                            {
                                spots[i] = true;
                                keysToTheTopX[i][testCase] = keyPresses;
                            }
                        }

                        if (matchIdx == 0)
                        {
                            break;
                        }
                    }
                }
            }
            foreach(var spot in keysToTheTopX)
            {
                spot.Sort();
            }
            return keysToTheTopX;
        }

        static (int[][] variances, int[][] variancesTop5, int[][] variancesTop1) GetStabilityResults<TProvider>(
            LuceneSuggestionProvider<TProvider> provider,
            List<string> testData)
        {
            int numTestCases = testData.Count;

            // Determine how much the list varies after completing a first or second word with a space
            var variances = new int[][]
            {
                new int[numTestCases],
                new int[numTestCases]
            };

            var variancesTop5 = new int[][]
            {
                new int[numTestCases],
                new int[numTestCases]
            };

            var variancesTop1 = new int[][]
            {
                new int[numTestCases],
                new int[numTestCases]
            };

            for (int testCase = 0; testCase < numTestCases; testCase++)
            {
                var name = testData[testCase];
                variances[0][testCase] =
                    variances[1][testCase] =
                    variancesTop5[0][testCase] =
                    variancesTop5[1][testCase] =
                    variancesTop1[0][testCase] =
                    variancesTop1[1][testCase] = 0;

                var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (int wordsStarted = 1; wordsStarted <= words.Length && wordsStarted < 3; wordsStarted++)
                {
                    var searchString = string.Join(" ", words.Take(wordsStarted));
                    var suggestionsIncomplete = provider.GetSuggestions(searchString, excludeLowScoring: true).ToList();
                    var suggestionsComplete = provider.GetSuggestions(searchString + " ", excludeLowScoring: true).ToList();

                    var sections = DiffLib.Diff.CalculateSections(suggestionsIncomplete, suggestionsComplete);
                    var diffCount = sections.Count(s => !s.IsMatch);

                    var sectionsTop5 = DiffLib.Diff.CalculateSections(
                        suggestionsIncomplete.Take(5).ToList(),
                        suggestionsComplete.Take(5).ToList());
                    var diffCountTop5 = sectionsTop5.Count(s => !s.IsMatch);
                    variances[wordsStarted-1][testCase] = diffCount;
                    variancesTop5[wordsStarted-1][testCase] = diffCountTop5;
                    variancesTop1[wordsStarted - 1][testCase] = suggestionsIncomplete[0] == suggestionsComplete[0]
                        ? 0
                        : 1;
                }
            }

            return (variances, variancesTop5, variancesTop1);
        }

        #endregion
    }
}
