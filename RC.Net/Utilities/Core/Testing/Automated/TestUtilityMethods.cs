using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Class for testing the <see cref="UtilityMethods"/> class.
    /// </summary>
    [TestFixture, Category("UtilityMethods")]
    public class TestUtilityMethods
    {
        #region Constants

        // Strings for retrieving the embedded dll's from the resources
        const string _TEST_INTERFACE_ONLY_DLL = "Resources.TestUtilitiesInterfaceOnly.dll";
        const string _TEST_NO_INTERFACE_DLL = "Resources.NoTestInterface.dll";
        const string _TEST_ONE_CLASS_INTERFACE_DLL = "Resources.OneClassInterface.dll";

        // Constants string for test interface type
        const string _TEST_INTERFACE_TYPE = "Extract.Utilities.Test.ITestInterface";

        #endregion

        #region Fields

        /// <summary>
        /// Paths to the dlls to load to test the interface searching
        /// </summary>
        static string _interfaceOnlyDll;
        static string _noInterfaceDll;
        static string _oneClassInterfaceDll;

        #endregion Fields

        #region TestSetup

        /// <summary>
        /// Initializes the test fixture for testing these methods
        /// </summary>
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            _interfaceOnlyDll = Path.Combine(path, _TEST_INTERFACE_ONLY_DLL.Replace("Resources.", ""));
            _noInterfaceDll = Path.Combine(path, _TEST_NO_INTERFACE_DLL.Replace("Resources.", ""));
            _oneClassInterfaceDll = Path.Combine(path, _TEST_ONE_CLASS_INTERFACE_DLL.Replace("Resources.", ""));

            // Write the embedded dll's to the disk. These files will be cleaned
            // up when the unit tests are exited
            if (!File.Exists(_interfaceOnlyDll))
            {
                GeneralMethods.WriteEmbeddedResourceToFile<TestUtilityMethods>(
                    _TEST_INTERFACE_ONLY_DLL, _interfaceOnlyDll);
            }
            if (!File.Exists(_noInterfaceDll))
            {
                GeneralMethods.WriteEmbeddedResourceToFile<TestUtilityMethods>(
                    _TEST_NO_INTERFACE_DLL, _noInterfaceDll);
            }
            if (!File.Exists(_oneClassInterfaceDll))
            {
                GeneralMethods.WriteEmbeddedResourceToFile<TestUtilityMethods>(
                    _TEST_ONE_CLASS_INTERFACE_DLL, _oneClassInterfaceDll);
            }
        }

        /// <summary>
        /// Performs post test execution cleanup.
        /// </summary>
        [OneTimeTearDown]
        public static void Cleanup()
        {
            // Set file names back to null
            _interfaceOnlyDll = null;
            _noInterfaceDll = null;
            _oneClassInterfaceDll = null;
        }

        #endregion TestSetup

        #region TestMethods

        #region Swap

        /// <summary>
        /// Tests swapping <see cref="byte"/>.
        /// </summary>
        /// <param name="a">The first item to swap.</param>
        /// <param name="b">The second item to swap.</param>
        [Test, Category("Swap"), Sequential]
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        public static void SwapByte([Values(1, 3, 42, byte.MaxValue)] byte a, [Values(2, 4, 6, byte.MinValue)] byte b)
        {
            byte aBefore = a;
            byte bBefore = b;

            UtilityMethods.Swap(ref a, ref b);
            Assert.That(aBefore == b && bBefore == a);
        }

        /// <summary>
        /// Tests swapping <see cref="short"/>.
        /// </summary>
        /// <param name="a">The first item to swap.</param>
        /// <param name="b">The second item to swap.</param>
        [Test, Category("Swap"), Sequential]
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        public static void SwapInt16([Values(1, 3, 42, short.MaxValue)] short a, [Values(2, 4, 6, short.MinValue)] short b)
        {
            short aBefore = a;
            short bBefore = b;

            UtilityMethods.Swap(ref a, ref b);
            Assert.That(aBefore == b && bBefore == a);
        }

        /// <summary>
        /// Tests swapping <see cref="int"/>.
        /// </summary>
        /// <param name="a">The first item to swap.</param>
        /// <param name="b">The second item to swap.</param>
        [Test, Category("Swap"), Sequential]
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        public static void SwapInt32([Values(1, 3, 42, int.MaxValue)] int a, [Values(2, 4, 6, int.MinValue)] int b)
        {
            int aBefore = a;
            int bBefore = b;

            UtilityMethods.Swap(ref a, ref b);
            Assert.That(aBefore == b && bBefore == a);
        }

        /// <summary>
        /// Tests swapping <see cref="long"/>.
        /// </summary>
        /// <param name="a">The first item to swap.</param>
        /// <param name="b">The second item to swap.</param>
        [Test, Category("Swap"), Sequential]
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        public static void SwapInt64([Values(1, 3, 42, long.MaxValue)] long a, [Values(2, 4, 6, long.MinValue)] long b)
        {
            long aBefore = a;
            long bBefore = b;

            UtilityMethods.Swap(ref a, ref b);
            Assert.That(aBefore == b && bBefore == a);
        }

        /// <summary>
        /// Tests swapping <see cref="float"/>.
        /// </summary>
        /// <param name="a">The first item to swap.</param>
        /// <param name="b">The second item to swap.</param>
        [Test, Category("Swap"), Sequential]
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        public static void SwapSingle([Values(1.0f, 3.0f, 42.3f, float.MaxValue)] float a, [Values(2.3f, 4.3f, 6.9f, float.MinValue)] float b)
        {
            float aBefore = a;
            float bBefore = b;

            UtilityMethods.Swap(ref a, ref b);
            Assert.That(aBefore == b && bBefore == a);
        }

        /// <summary>
        /// Tests swapping <see cref="double"/>.
        /// </summary>
        /// <param name="a">The first item to swap.</param>
        /// <param name="b">The second item to swap.</param>
        [Test, Category("Swap"), Sequential]
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        public static void SwapDouble([Values(1.0, 3.0, 42.3, double.MaxValue)] double a, [Values(2.3, 4.3, 6.9, double.MinValue)] double b)
        {
            double aBefore = a;
            double bBefore = b;

            UtilityMethods.Swap(ref a, ref b);
            Assert.That(aBefore == b && bBefore == a);
        }

        /// <summary>
        /// Tests swapping <see cref="decimal"/>.
        /// </summary>
        /// <param name="a">The first item to swap.</param>
        /// <param name="b">The second item to swap.</param>
        [Test, Category("Swap"), Sequential]
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        public static void SwapDecimal([Values(1.0903434d, -3.01238980412d, 42.3d, 0.0)] double a,
            [Values(2.3802193d, 4.3721831d, 6.9d, 0.0)] double b)
        {
            decimal c = (a == 0.0 ? decimal.MaxValue : (decimal)a);
            decimal d = (b == 0.0 ? decimal.MinValue : (decimal)b);
            decimal cBefore = c;
            decimal dBefore = d;

            UtilityMethods.Swap(ref c, ref d);
            Assert.That(cBefore == d && dBefore == c);
        }

        /// <summary>
        /// Tests swapping <see cref="char"/>.
        /// </summary>
        /// <param name="a">The first item to swap.</param>
        /// <param name="b">The second item to swap.</param>
        [Test, Category("Swap"), Sequential]
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        public static void SwapChar([Values('a', '3', '&', char.MaxValue)] char a, [Values('2', 'Z', '^', char.MinValue)] char b)
        {
            char aBefore = a;
            char bBefore = b;

            UtilityMethods.Swap(ref a, ref b);
            Assert.That(aBefore == b && bBefore == a);
        }

        #endregion Swap

        #region TypesThatImplementInterface

        /// <summary>
        /// Tests that no types are returned for an assembly only containing the desired interface.
        /// </summary>
        [Test, Category("TypesThatImplementInterface")]
        public static void NoTypesInterfaceOnly()
        {
            var assembly = Assembly.ReflectionOnlyLoadFrom(_interfaceOnlyDll);
            var type = assembly.GetType(_TEST_INTERFACE_TYPE);
            var list = UtilityMethods.AllTypesThatImplementInterface(type, assembly);

            Assert.That(list.Length == 0);
        }

        /// <summary>
        /// Tests that no types are returned for an assembly that does not contain any classes
        /// that implement the desired interface.
        /// </summary>
        [Test, Category("TypesThatImplementInterface")]
        public static void NoTypesImplementInterface()
        {
            var interfaceAssembly = Assembly.ReflectionOnlyLoadFrom(_interfaceOnlyDll);
            var type = interfaceAssembly.GetType(_TEST_INTERFACE_TYPE);
            var assembly = Assembly.ReflectionOnlyLoadFrom(_noInterfaceDll);

            var list = UtilityMethods.AllTypesThatImplementInterface(type, assembly);

            Assert.That(list.Length == 0);
        }

        /// <summary>
        /// Tests that only 1 type is returned for an assembly that contains multiple classes
        /// but only 1 that implements the desired interface.
        /// </summary>
        [Test, Category("TypesThatImplementInterface")]
        public static void OneTypeImplementInterface()
        {
            var interfaceAssembly = Assembly.ReflectionOnlyLoadFrom(_interfaceOnlyDll);
            var type = interfaceAssembly.GetType(_TEST_INTERFACE_TYPE);
            var assembly = Assembly.ReflectionOnlyLoadFrom(_oneClassInterfaceDll);

            var list = UtilityMethods.AllTypesThatImplementInterface(type, assembly);

            Assert.That(list.Length == 1);
        }

        /// <summary>
        /// Tests that only 1 type is returned for a collection of assemblies of which only 1
        /// contains a class that implements the desired interface.
        /// </summary>
        [Test, Category("TypesThatImplementInterface")]
        public static void OnlyOneAssemblyContainsTypeImplementInterfaceCommaValues()
        {
            var interfaceAssembly = Assembly.ReflectionOnlyLoadFrom(_interfaceOnlyDll);
            var type = interfaceAssembly.GetType(_TEST_INTERFACE_TYPE);
            var oneImplementer = Assembly.ReflectionOnlyLoadFrom(_oneClassInterfaceDll);
            var noImplementers = Assembly.ReflectionOnlyLoadFrom(_noInterfaceDll);

            var list = UtilityMethods.AllTypesThatImplementInterface(type, interfaceAssembly,
                oneImplementer, noImplementers);

            Assert.That(list.Length == 1);
        }

        /// <summary>
        /// Tests that only 1 type is returned for a collection of assemblies of which only 1
        /// contains a class that implements the desired interface.
        /// </summary>
        [Test, Category("TypesThatImplementInterface")]
        public static void OnlyOneAssemblyContainsTypeImplementInterfaceList()
        {
            var assemblies = new List<Assembly>();
            var interfaceAssembly = Assembly.ReflectionOnlyLoadFrom(_interfaceOnlyDll);
            assemblies.Add(interfaceAssembly);
            var type = interfaceAssembly.GetType(_TEST_INTERFACE_TYPE);
            assemblies.Add(Assembly.ReflectionOnlyLoadFrom(_oneClassInterfaceDll));
            assemblies.Add(Assembly.ReflectionOnlyLoadFrom(_noInterfaceDll));

            var list = UtilityMethods.AllTypesThatImplementInterface(type, assemblies.ToArray());

            Assert.That(list.Length == 1);
        }

        /// <summary>
        /// Tests that only no types are returned for a collection of assemblies of which none
        /// contains a class that implements the desired interface.
        /// </summary>
        [Test, Category("TypesThatImplementInterface")]
        public static void NoAssemblyContainsTypeImplementInterface()
        {
            var assemblies = new List<Assembly>();
            var interfaceAssembly = Assembly.ReflectionOnlyLoadFrom(_interfaceOnlyDll);
            assemblies.Add(interfaceAssembly);
            var type = interfaceAssembly.GetType(_TEST_INTERFACE_TYPE);
            assemblies.Add(Assembly.ReflectionOnlyLoadFrom(_noInterfaceDll));

            var list = UtilityMethods.AllTypesThatImplementInterface(type, assemblies.ToArray());

            Assert.That(list.Length == 0);
        }

        #endregion TypesThatImplementInterface

        #region Shuffle

        /// <summary>
        /// Tests shuffling an array with a fixed random seed.
        /// </summary>
        [Test, Category("Shuffle")]
        [CLSCompliant(false)]
        public static void ShuffleWithSeed()
        {
            var rng = new Random(0);
            int[] array = { 1, 2, 3, 4 };
            int[] expected = { 3, 4, 2, 1 };
            CollectionMethods.Shuffle(array, rng);
            CollectionAssert.AreEqual(expected, array);
        }

        /// <summary>
        /// Tests shuffling two arrays with a fixed random seed.
        /// </summary>
        [Test, Category("Shuffle")]
        [CLSCompliant(false)]
        public static void Shuffle2WithSeed()
        {
            var rng = new Random(0);
            int[] array1 = { 1, 2, 3, 4 };
            int[] expected1 = { 3, 4, 2, 1 };
            string[] array2 = { "one", "two", "three", "four" };
            string[] expected2 = { "three", "four", "two", "one" };
            CollectionMethods.Shuffle(array1, array2, rng);
            CollectionAssert.AreEqual(expected1, array1);
            CollectionAssert.AreEqual(expected2, array2);
        }

        /// <summary>
        /// Tests shuffling an array with no specified seed.
        /// </summary>
        [Test, Category("Shuffle")]
        [CLSCompliant(false)]
        public static void ShuffleWithoutSeed()
        {
            Dictionary<string, int> results = (new[] { 1, 2, 3, 4 }).GetPermutations(4)
                .Select(a => Tuple.Create(String.Join("", a), 0))
                .ToDictionary(t => t.Item1, t => t.Item2);

            for (int i = 0; i < 600000; i++)
            {
                int[] array = { 1, 2, 3, 4 };
                CollectionMethods.Shuffle(array);
                string result = String.Join("", array);
                results[result]++;
            }

            // This assertion is to make sure that the generated permutations are distributed fairly evenly
            Assert.That(results.Values.Select(count => Math.Abs(25000 - count)).All(difference => difference < 1000),
                "There is a (slight) chance of the random distributions being very unbalanced so this assertion might fail.");

            // Repeat the process in order to test that the same distribution does not happen each time
            Dictionary<string, int> results2 = (new[] { 1, 2, 3, 4 }).GetPermutations(4)
                .Select(a => Tuple.Create(String.Join("", a), 0))
                .ToDictionary(t => t.Item1, t => t.Item2);

            for (int i = 0; i < 600000; i++)
            {
                int[] array = { 1, 2, 3, 4 };
                CollectionMethods.Shuffle(array);
                string result = String.Join("", array);
                results2[result]++;
            }
            
            // If different permutation frequencies occurred then the shuffle algorithm is not biased
            var permutationsOrderedByCount = results.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToArray();
            var permutationsOrderedByCount2 = results2.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToArray();
            CollectionAssert.AreNotEqual(permutationsOrderedByCount, permutationsOrderedByCount2,
                "There is a chance that this assertion will fail occasionally.");
        }

        /// <summary>
        /// Tests shuffling two arrays with no specified seed.
        /// </summary>
        [Test, Category("Shuffle")]
        [CLSCompliant(false)]
        public static void Shuffle2WithoutSeed()
        {
            Dictionary<string, int> results1 = (new[] { 1, 2, 3, 4 }).GetPermutations(4)
                .Select(a => Tuple.Create(String.Join("", a), 0))
                .ToDictionary(t => t.Item1, t => t.Item2);

            Dictionary<string, int> results2 = (new[] { "one", "two", "three", "four" }).GetPermutations(4)
                .Select(a => Tuple.Create(String.Join("", a), 0))
                .ToDictionary(t => t.Item1, t => t.Item2);

            for (int i = 0; i < 600000; i++)
            {
                int[] array1 = { 1, 2, 3, 4 };
                string[] array2 = { "one", "two", "three", "four" };
                CollectionMethods.Shuffle(array1, array2);
                string result1 = String.Join("", array1);
                string result2 = String.Join("", array2);
                results1[result1]++;
                results2[result2]++;
            }
            Assert.That(results1.Values.Select(count => Math.Abs(25000 - count)).All(difference => difference < 1000),
                "There is a (slight) chance of the random distributions being very unbalanced so this assertion might fail.");
            CollectionAssert.AreEqual(results1.Values, results2.Values);
        }

        #endregion Shuffle

        #region GetRandomString

        /// <summary>
        /// Tests generating a random string in a loop to make sure numbers aren't repeated very much.
        /// </summary>
        [Test, Category("GetRandomString")]
        [CLSCompliant(false)]
        public static void GetRandomStrings()
        {
            string[] results = new string[600000];

            for (int i = 0; i < 600000; i++)
            {
                results[i] = UtilityMethods.GetRandomString(10, true, false, true);
            }
            Assert.That(results.Distinct().Count() == 600000,
                "There is a (slight) chance of generating repeated random strings so this assertion might fail.");
        }

        #endregion GetRandomString

        #region ValidatePageNumbers

        /// <summary>
        /// Tests ValidatePageNumbers on valid strings
        /// </summary>
        [Test, Category("ValidatePageNumbers")]
        [CLSCompliant(false)]
        public static void ValidPageNumbers()
        {
            Assert.DoesNotThrow(() => UtilityMethods.ValidatePageNumbers("1-3"));
            Assert.DoesNotThrow(() => UtilityMethods.ValidatePageNumbers("-3"));
            Assert.DoesNotThrow(() => UtilityMethods.ValidatePageNumbers("3-"));
            Assert.DoesNotThrow(() => UtilityMethods.ValidatePageNumbers("1,2,3-,-1,4,5,1-10"));
            Assert.DoesNotThrow(() => UtilityMethods.ValidatePageNumbers("3-3"));
        }

        /// <summary>
        /// Tests ValidatePageNumbers on invalid strings
        /// </summary>
        [Test, Category("ValidatePageNumbers")]
        [CLSCompliant(false)]
        public static void InvalidPageNumbers()
        {
            Assert.Throws<ExtractException>(() => UtilityMethods.ValidatePageNumbers("1--3"));
            Assert.Throws<ExtractException>(() => UtilityMethods.ValidatePageNumbers("1--"));
            Assert.Throws<ExtractException>(() => UtilityMethods.ValidatePageNumbers("--1"));
            Assert.Throws<ExtractException>(() => UtilityMethods.ValidatePageNumbers(""));
            Assert.Throws<ExtractException>(() => UtilityMethods.ValidatePageNumbers("abc"));
            Assert.Throws<ExtractException>(() => UtilityMethods.ValidatePageNumbers("1A-3"));
            Assert.Throws<ExtractException>(() => UtilityMethods.ValidatePageNumbers("A-B"));
        }

        #endregion ValidatePageNumbers

        #region PagesAndStringConversions

        /// <summary>
        /// Tests generating page numbers from range string
        /// </summary>
        [Test, Category("PagesAndStringConversions")]
        [CLSCompliant(false)]
        public static void GetSortedPageNumberFromString()
        {
            var result = UtilityMethods.GetSortedPageNumberFromString("1-3", 3, false);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("1-3", 2, false);
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("1-", 3, false);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("-1", 3, false);
            CollectionAssert.AreEquivalent(new[] { 3 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("-2", 3, false);
            CollectionAssert.AreEquivalent(new[] { 2, 3 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("-3", 3, false);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("3-", 3, false);
            CollectionAssert.AreEquivalent(new[] { 3 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("4-", 3, false);
            CollectionAssert.AreEquivalent(new int[0], result);

            result = UtilityMethods.GetSortedPageNumberFromString("-4", 3, false);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("1-1", 2, false);
            CollectionAssert.AreEquivalent(new[] { 1 }, result);
        }

        /// <summary>
        /// Tests generating page numbers from multiple ranges
        /// </summary>
        [Test, Category("PagesAndStringConversions")]
        [CLSCompliant(false)]
        public static void GetMultiplePageRangesFromString()
        {
            var result = UtilityMethods.GetSortedPageNumberFromString("1,-1", 4, false);
            CollectionAssert.AreEquivalent(new[] { 1, 4 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("1,1-3", 4, false);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("1,1-3", 2, false);
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("3,1-3", 4, false);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("-1, 3, 10", 300, false);
            CollectionAssert.AreEquivalent(new[] { 3, 10, 300 }, result);

            result = UtilityMethods.GetSortedPageNumberFromString("271-, 3, 10", 300, false);
            CollectionAssert.AreEquivalent(new[] { 3, 10, 271, 272, 273, 274, 275, 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300 }, result);
        }

        [Test, Category("PagesAndStringConversions")]
        [CLSCompliant(false)]
        public static void GetPageNumbersFromString()
        {
            var result = UtilityMethods.GetPageNumbersFromString("1,-1", 4, false);
            CollectionAssert.AreEquivalent(new[] { 1, 4 }, result);

            result = UtilityMethods.GetPageNumbersFromString("1,1-3", 4, false);
            CollectionAssert.AreEquivalent(new[] { 1, 1, 2, 3 }, result);

            result = UtilityMethods.GetPageNumbersFromString("1,1-3", 2, false);
            CollectionAssert.AreEquivalent(new[] { 1, 1, 2 }, result);

            result = UtilityMethods.GetPageNumbersFromString("3,1-3", 4, false);
            CollectionAssert.AreEquivalent(new[] { 3, 1, 2, 3 }, result);

            result = UtilityMethods.GetPageNumbersFromString("-1, 3, 10", 300, false);
            CollectionAssert.AreEquivalent(new[] { 300, 3, 10, }, result);

            result = UtilityMethods.GetPageNumbersFromString("271-, 3, 10", 300, false);
            CollectionAssert.AreEquivalent(new[] { 271, 272, 273, 274, 275, 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 3, 10 }, result);
        }

        [Test, Category("PagesAndStringConversions")]
        [CLSCompliant(false)]
        public static void GetPageNumbersFromStringNoPageTotal()
        {
            Assert.Throws<ExtractException>(() => UtilityMethods.GetPageNumbersFromString("1-2", -1, true));

            var result = UtilityMethods.GetPageNumbersFromString("1-2", -1, false);
            CollectionAssert.AreEquivalent(new[] { 1, 2, }, result);

            Assert.Throws<ExtractException>(() => UtilityMethods.GetPageNumbersFromString("1-", -1, false));

            Assert.Throws<ExtractException>(() => UtilityMethods.GetPageNumbersFromString("-2", -1, false));
        }

        /// <summary>
        /// Tests exception handling generating page numbers from formatted string
        /// </summary>
        [Test, Category("PagesAndStringConversions")]
        [CLSCompliant(false)]
        public static void GetSortedPageNumberFromStringExceptions()
        {
            var ex = Assert.Throws<ExtractException>(() => UtilityMethods.GetPageNumbersFromString("4-2", 3, false));
            Assert.That( ex.Message, Is.EqualTo("Start page number must be less than or equal to the end page number.") );

            ex = Assert.Throws<ExtractException>(() => UtilityMethods.GetPageNumbersFromString("-0", 3, false));
            Assert.That( ex.Message, Is.EqualTo("Ending page cannot be zero.") );

            ex = Assert.Throws<ExtractException>(() => UtilityMethods.GetPageNumbersFromString("2-4", 3, true));
            Assert.That( ex.Message, Is.EqualTo("Specified end page number is out of range.") );

            ex = Assert.Throws<ExtractException>(() => UtilityMethods.GetPageNumbersFromString("-4", 3, true));
            Assert.That( ex.Message, Is.EqualTo("Specified page number is out of range.") );

            ex = Assert.Throws<ExtractException>(() => UtilityMethods.GetPageNumbersFromString("4-", 3, true));
            Assert.That( ex.Message, Is.EqualTo("Specified start page number is out of range.") );
        }

        [Test, Category("PagesAndStringConversions")]
        [CLSCompliant(false)]
        public static void GetPageNumbersAsString()
        {
            Assert.AreEqual("", UtilityMethods.GetPageNumbersAsString(new int[0]));
            Assert.AreEqual("1", UtilityMethods.GetPageNumbersAsString(new[] { 1 }));
            Assert.AreEqual("1,3", UtilityMethods.GetPageNumbersAsString(new[] { 1, 3 }));

            Assert.AreEqual("1-3", UtilityMethods.GetPageNumbersAsString(new[] { 1, 2, 3 }));
            Assert.AreEqual("3,2,1", UtilityMethods.GetPageNumbersAsString(new[] { 3, 2, 1 }));

            Assert.AreEqual("1-3,5", UtilityMethods.GetPageNumbersAsString(new[] { 1, 2, 3, 5 }));
            Assert.AreEqual("5,2,1-3", UtilityMethods.GetPageNumbersAsString(new[] { 5, 2, 1, 2, 3 }));

            Assert.AreEqual("1-3,5,8-11", UtilityMethods.GetPageNumbersAsString(new[] { 1, 2, 3, 5, 8, 9, 10, 11 }));

            Assert.AreEqual("5,8-11", UtilityMethods.GetPageNumbersAsString(new[] { 5, 8, 9, 10, 11 }));
        }

        #endregion PagesStringConversions

        #endregion TestMethods
    }
}
