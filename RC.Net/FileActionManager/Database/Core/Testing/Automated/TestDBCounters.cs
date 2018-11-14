using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;
using UCLID_COMUTILSLib;
using System.Linq;
using Extract.Interfaces;

namespace Extract.FileActionManager.Database.Test
{
    [Category("TestDBCounters")]
    [TestFixture]
    public class TestDBCounters
    {
        #region Constants

        static readonly string _DB_WITH_COUNTERS = "Resources.DBWithCounters.bak";
        static readonly string _DB_WITH_COUNTERS_V171 = "Resources.DBWithCountersVer171.bak";


        #endregion

        #region Fields

        /// <summary>
        /// Manages test files.
        /// </summary>
        static TestFileManager<TestDBCounters> _testFiles;

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestDBCounters> _testDbManager;

        #endregion
        
        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestDBCounters>();
            _testDbManager = new FAMTestDBManager<TestDBCounters>();
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
                _testFiles = null;
            }

            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
        }

        #endregion Overhead

        #region Tests

        [Test, Category("Automated")]
        public static void RestoreDatabaseCorruptCounters()
        {
            string testDBName = "Test_RestoreDatabaseCorrupt";
            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_DB_WITH_COUNTERS_V171, testDBName);

                IIUnknownVector secureCounters = fileProcessingDb.GetSecureCounters(true);
                var secureCounterList = secureCounters.ToIEnumerable<ISecureCounter>().ToList();

                var invalidcounters = secureCounterList
                    .Where(c => c.IsValid == false);

                Assert.AreEqual(secureCounterList.Count(), invalidcounters.Count(), "Counters should all be corrupt.");
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        [Test, Category("Automated_ADMIN")]
        [TestCase(8,1, TestName = "Standard Time")]
        [TestCase(1,8, TestName = "Daylight Savings Time")]
        public static void TimeChanges(short createMonth, short testMonth)
        {
            string testDBName = "Test_StandardTime";
            SystemTime savedTime = NativeMethods.GetWin32SystemTime();
            try
            {
                short year = savedTime.Year;
                if (createMonth <= testMonth)
                {
                    year -= 1;
                }
                ChangeMonthYear(savedTime, createMonth, year);

                // The _DB_WITH_COUNTERS database is version 170 which converts to DB with counters in a valid state
                var fileProcessingDb = _testDbManager.GetDatabase(_DB_WITH_COUNTERS, testDBName);

                ChangeMonthYear(savedTime, testMonth, savedTime.Year);

                IIUnknownVector secureCounters = fileProcessingDb.GetSecureCounters(true);
                var secureCounterList = secureCounters.ToIEnumerable<ISecureCounter>().ToList();

                var invalidcounters = secureCounterList
                    .Where(c => c.IsValid == false);

                Assert.AreEqual(0, invalidcounters.Count(), "Counters are not valid.");
            }
            finally
            {
                NativeMethods.SetWin32SystemTime(savedTime);
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        #endregion

        #region Helper Methods

        static void ChangeMonthYear(SystemTime currentTime, short month, short year)
        {
            SystemTime changedTime = new SystemTime(currentTime);
            changedTime.Month = month;
            changedTime.Year = year;
            Assert.IsTrue(NativeMethods.SetWin32SystemTime(changedTime), "Unable to perform test. Time was not changed. Must be admin to run this test");
            System.DateTime dateTime = System.DateTime.UtcNow;
            Assert.AreEqual(changedTime.Month, dateTime.Month, "Unable to perform test. Time was not changed. Must be admin to run this test");
        }

        #endregion
    }
}
