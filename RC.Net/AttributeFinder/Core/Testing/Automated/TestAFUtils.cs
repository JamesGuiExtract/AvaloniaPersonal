using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Linq;
using System.Threading;


namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Test cases for AFUtils methods. This is not meant to be comprehensive at this time but to include
    /// test cases for new features.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("AFUtility")]
    public class TestAFUtility
    {
        #region Fields
        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
        }

        #endregion Setup and Teardown

        #region Public Test Functions

        /// <summary>
        /// Test GetComponentDataFolder2 run in parallel
        /// </summary>
        [Test, Category("AFUtility")]        
        public static void Test01()
        {
            ThreadLocal<UCLID_AFUTILSLib.AFUtility> afutility = new ThreadLocal<UCLID_AFUTILSLib.AFUtility>(() => new UCLID_AFUTILSLib.AFUtility());
            Enumerable.Range(1, 10000).AsParallel().ForAll(i =>
            {
                var componentDataDir = afutility.Value.GetComponentDataFolder2("Latest", null);
                Assert.NotNull(componentDataDir);
            });
        }

        #endregion Public Test Functions

    }
}
