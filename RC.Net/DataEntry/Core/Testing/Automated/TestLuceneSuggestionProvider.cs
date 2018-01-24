using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;


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
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestLuceneSuggestionProvider>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
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

            // Dtermine how many key presses are required to bring the target
            // test name into the top 3 spots, top 2 spots and top 1 spot.
            // Don't use a fixed-seed rng so that all the data will be used, eventually
            int testCases = testData.Count;
            var keysToTheTopX = new List<int>[]
            {
                new List<int>(testCases),
                new List<int>(testCases),
                new List<int>(testCases),
            };

            int numSpots = keysToTheTopX.Length;
            for(int testCase=0; testCase < testCases; testCase++)
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
                var words = aka.Split(' ');
                CollectionMethods.Shuffle(words, new Random(0));
                aka = string.Join(" ", words);
                for (int keyPresses = 1; keyPresses <= aka.Length; keyPresses++)
                {
                    var suggestions = provider.GetSuggestions(aka.Substring(0, keyPresses), spots.Length).ToList();
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
            var medians = keysToTheTopX.Select(a => a[testCases / 2]).ToList();
            var avgs = keysToTheTopX.Select(a => a.Sum()/(double)testCases).ToList();

            Assert.LessOrEqual(medians[0], 5);
            Assert.LessOrEqual(medians[1], 3);
            Assert.LessOrEqual(medians[2], 3);

            Assert.LessOrEqual(avgs[0], 6.25);
            Assert.LessOrEqual(avgs[1], 4.75);
            Assert.LessOrEqual(avgs[2], 4);
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

            // Determine how many key presses are required to bring the target
            // lab name into the top 1, 2, etc spots
            int testCases = testData.Count;
            var keysToTheTopX = new List<int>[]
            {
                new List<int>(testCases),
                new List<int>(testCases),
                new List<int>(testCases),
                new List<int>(testCases),
                new List<int>(testCases),
            };

            int numSpots = keysToTheTopX.Length;
            for(int testCase=0; testCase < testCases; testCase++)
            {
                var name = testData[testCase];
                var spots = new bool[numSpots];

                // Init lists for this case
                for (int i=0; i<numSpots;i++)
                {
                    // Initialize spots with the number of characters in the name
                    keysToTheTopX[i].Add(name.Length);
                }

                // Pick a random word to start typing from
                // Use a fixed-seed rng so that the unit test will always pass
                var words = name.Split(' ');
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
            var medians = keysToTheTopX.Select(a => a[testCases / 2]).ToList();
            var avgs = keysToTheTopX.Select(a => a.Sum()/(double)testCases).ToList();

            Assert.LessOrEqual(medians[0], 8);
            Assert.LessOrEqual(medians[1], 6);
            Assert.LessOrEqual(medians[2], 6);
            Assert.LessOrEqual(medians[3], 5);
            Assert.LessOrEqual(medians[4], 5);

            Assert.LessOrEqual(avgs[0], 8.5);
            Assert.LessOrEqual(avgs[1], 7);
            Assert.LessOrEqual(avgs[2], 6.5);
            Assert.LessOrEqual(avgs[3], 5.75);
            Assert.LessOrEqual(avgs[4], 5.5);
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

        #endregion
    }
}
