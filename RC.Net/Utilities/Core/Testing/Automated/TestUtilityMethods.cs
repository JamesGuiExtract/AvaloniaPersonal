using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
        [TestFixtureSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            _interfaceOnlyDll = Path.Combine(path, _TEST_INTERFACE_ONLY_DLL.Replace("Resources.", ""));
            _noInterfaceDll = Path.Combine(path, _TEST_NO_INTERFACE_DLL.Replace("Resources.", ""));
            _oneClassInterfaceDll = Path.Combine(path, _TEST_ONE_CLASS_INTERFACE_DLL.Replace("Resources.", ""));

            // Write the embedded dll's to the disk. These files will be cleaned
            // up when the unit tests are exited
            GeneralMethods.WriteEmbeddedResourceToFile<TestUtilityMethods>(
                _TEST_INTERFACE_ONLY_DLL, _interfaceOnlyDll);
            GeneralMethods.WriteEmbeddedResourceToFile<TestUtilityMethods>(
                _TEST_NO_INTERFACE_DLL, _noInterfaceDll);
            GeneralMethods.WriteEmbeddedResourceToFile<TestUtilityMethods>(
                _TEST_ONE_CLASS_INTERFACE_DLL, _oneClassInterfaceDll);
        }

        /// <summary>
        /// Performs post test execution cleanup.
        /// </summary>
        [TestFixtureTearDown]
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

        #endregion TestMethods
    }
}
