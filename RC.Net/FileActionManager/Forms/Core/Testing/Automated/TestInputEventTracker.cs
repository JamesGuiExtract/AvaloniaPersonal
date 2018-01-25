using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extract;
using Extract.Testing.Utilities;
using NUnit.Framework;
using Extract.FileActionManager.Database.Test;
using UCLID_FILEPROCESSINGLib;
using Extract.Utilities.Forms;
using System.Diagnostics;
using System.Threading;
using System.Globalization;


namespace Extract.FileActionManager.Forms.Test
{
    /// <summary>
    /// Class to test InputEventTracker
    /// </summary>
    [TestFixture]
    [Category("TestInputEventTracker")]
    public class TestInputEventTracker
    {
        #region Fields

        /// <summary>
        /// Manager for databases for testign
        /// </summary>
        static FAMTestDBManager<TestInputEventTracker> _testDbManager;

        #endregion

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestInputEventTracker>();

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

        #region Tests

        [Test, Category("Automated")]
        public static void TestActivityTracking()
        {
            string testDBName = "TestActivityTrackingDB";
            try
            {
                FileProcessingDB fam = _testDbManager.GetNewDatabase(testDBName);
                int actionId = fam.DefineNewAction("Test");
                InputEventTracker inputTracker = new InputEventTracker(fam, actionId);
                inputTracker.Active = true;
                Stopwatch testSW = new Stopwatch();

                double secondsOfActivity= RunTestActivity(inputTracker, 5, 10, 1, testSW);
                Assert.That(secondsOfActivity > testSW.Elapsed.TotalSeconds - 1.0,
                    string.Format(CultureInfo.InvariantCulture,
                        "Activity time: {0}, expected within 2 seconds: {1}", secondsOfActivity, testSW.Elapsed.TotalSeconds));

                secondsOfActivity = RunTestActivity(inputTracker, 2, 10, 5, testSW);
                Assert.That(Math.Abs(secondsOfActivity) < 0.001,
                     string.Format(CultureInfo.InvariantCulture,
                        "Activity time: {0}, expected close to 0", secondsOfActivity, testSW.Elapsed.TotalSeconds));
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }

        }

        static double RunTestActivity(InputEventTracker inputTracker, int timeout, 
            int runtime, int timeBetweenInput, Stopwatch testSW)
        {
            inputTracker.StartActivityTimer(timeout);
            testSW.Restart();
            while (testSW.Elapsed.TotalSeconds < runtime)
            {
                Thread.Sleep(timeBetweenInput*1000);
                inputTracker.NotifyOfInputEvent();
            }
            double returnValue = inputTracker.StopActivityTimer();
            testSW.Stop();
            return returnValue;
        }

        #endregion

    }
}
