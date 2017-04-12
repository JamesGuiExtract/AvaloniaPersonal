using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    /// Testing class for the workflow management related methods in IFileProcessingDB.
    /// </summary>
    [Category("TestFAMDBConfig")]
    [TestFixture]
    public class TestFAMDBConfig
    {
        #region Constants

        static readonly string _LABDE_EMPTY_DB = "Resources.Demo_LabDE_Empty";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestFAMFileProcessing> _testDbManager;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
        }

        #endregion Overhead

        #region Test Methods

        /// <summary>
        /// Tests the LoginUser method.
        /// </summary>
        [Test, Category("Automated")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login")]
        public static void LoginUser()
        {
            string testDbName = "Test_LoginUser";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Good password
                fileProcessingDb.LoginUser("admin", "a");

                // Bad password
                Assert.Throws<COMException>(() => fileProcessingDb.LoginUser("admin", "BadPassword"));

                // Unknown user
                Assert.Throws<COMException>(() => fileProcessingDb.LoginUser("UnknownUser", "a"));

                // User without password set
                fileProcessingDb.AddLoginUser("UserWithNoPassword");
                Assert.Throws<COMException>(() => fileProcessingDb.LoginUser("UserWithNoPassword", ""));
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        #endregion Test Methods
    }
}
