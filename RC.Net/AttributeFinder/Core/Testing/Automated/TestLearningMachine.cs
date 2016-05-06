using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Unit tests for learning machine data class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("LearningMachine")]
    public class TestLearningMachine
    {
        #region Constants

        /// <summary>
        /// The name of an embedded resource folder
        /// </summary>

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestLearningMachine> _testFiles;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestLearningMachine>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            // Dispose of the test image manager
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }
        }

        #endregion Overhead

        #region Tests

        // Test GetIndexesOfSubsetsByCategory to make sure it is randomly selecting appropriately sized subsets
        [Test, Category("LearningMachine")]
        public static void GetIndexesOfSubsetsByCategoryOneCategory()
        {
            foreach (int size in new[] { 1, 1000})
            {
                int[] originalIndexes = new int[size];

                // All one category for this test
                int[] categories = new int[size];
                for (int i = 0; i < size; i++)
                {
                    originalIndexes[i] = i;
                }
                var fractions = new HashSet<double>();
                fractions.Add(1);
                for (double d = 1; d <= 100; d++)
                {
                    for (double n = 1; n < d; n++)
                    {
                        fractions.Add(n / d);
                    }
                }
                var rng = new Random();
                foreach (var subset1Fraction in fractions)
                {
                    int seed = rng.Next();
                    System.Collections.Generic.List<int> subset1Indexes, subset2Indexes;
                    LearningMachine.GetIndexesOfSubsetsByCategory(categories, subset1Fraction, out subset1Indexes, out subset2Indexes, new Random(seed));

                    // Check size of subset1
                    Assert.AreEqual(Math.Max(Math.Round(size * subset1Fraction), 1), subset1Indexes.Count);
                    // Check size of subset2
                    Assert.AreEqual(Math.Max(size - Math.Round(size * subset1Fraction), 1), subset2Indexes.Count);

                    // Subsets should overlap by at most one item
                    Assert.LessOrEqual(subset1Indexes.Intersect(subset2Indexes).Count(), 1);

                    // These next rely on implementation details so may fail if implementation changes

                    // Shuffle a copy of the original indexes
                    var expected = originalIndexes.ToArray();
                    Utilities.CollectionMethods.Shuffle(expected, new Random(seed));

                    // Since the same random generator was used, subset1 indexes should be the same as first of the shuffled indexes
                    CollectionAssert.AreEqual(expected.Take(subset1Indexes.Count), subset1Indexes);

                    // Subset2 indexes should be the same as last of the shuffled indexes
                    CollectionAssert.AreEqual(expected.Skip(size - subset2Indexes.Count), subset2Indexes);
                }
            }
        }

        // Test GetIndexesOfSubsetsByCategory with multiple categories to make sure no items are
        // missing from both subsets and few if any items are present in both subsets with a variety
        // of category sizes
        [Test, Category("LearningMachine")]
        public static void GetIndexesOfSubsetsByCategoryMultipleCategories()
        {
            foreach (int size in new[] { 1, 1000})
            {
                var numberOfCategories = 10;
                int[] data = new int[size];
                int[] categories = new int[size];
                var rng = new Random();
                for (int i = 0; i < size; i++)
                {
                    data[i] = i;
                    categories[i] = rng.Next(0, numberOfCategories);
                }
                var fractions = new HashSet<double>();
                fractions.Add(1);
                for (double d = 1; d <= 100; d++)
                {
                    for (double n = 1; n < d; n++)
                    {
                        fractions.Add(n / d);
                    }
                }
                foreach (var subset1Fraction in fractions)
                {
                    System.Collections.Generic.List<int> subset1Indexes, subset2Indexes;
                    LearningMachine.GetIndexesOfSubsetsByCategory(categories, subset1Fraction, out subset1Indexes, out subset2Indexes);

                    // Check if counts make sense
                    Assert.GreaterOrEqual(subset1Indexes.Count + subset2Indexes.Count, size);
                    Assert.LessOrEqual(subset1Indexes.Count + subset2Indexes.Count, size + numberOfCategories);

                    // Check to make sure overlap is minimal
                    Assert.LessOrEqual(subset1Indexes.Intersect(subset2Indexes).Count(), numberOfCategories);
                }
            }
        }

        #endregion Tests
    }
}
