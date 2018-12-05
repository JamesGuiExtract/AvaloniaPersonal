using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.Utilities;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using static System.DateTime;


namespace Extract.ETL.Test
{
    [Category("ETL")]
    [Category("DatabaseService")]
    [TestFixture]
    public class DatabaseServiceManagerTest
    {
        class TestDatabaseService : DatabaseService
        {
            bool _processing = false;
            public static AutoResetEvent FinishEvent = new AutoResetEvent(false);
            public static AutoResetEvent StartedEvent = new AutoResetEvent(false);

            public override bool Processing => _processing;

            public override int Version { get => 1; protected set { } }

            public override void Process(CancellationToken cancelToken)
            {
                _processing = true;
                var status = GetLastOrCreateStatus(() => new TestDatabaseServiceStatus());
                StartedEvent.Set();
                FinishEvent.WaitOne();
                SaveStatus(status);
                _processing = false;
            }

            public class TestDatabaseServiceStatus : DatabaseServiceStatus
            {
                public override int Version { get => 1; protected set { } }
            }
        }

        static FAMTestDBManager<DatabaseServiceManagerTest> _testDbManager;

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<DatabaseServiceManagerTest>();
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

        [Test]
        [Category("Automated")]
        [Category("DatabaseService")]
        public static void DatabaseServiceTest()
        {
            string testDBName = "DatabaseService_test";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase("Resources.ExpandAttributes.bak", testDBName);

                TestDatabaseService testDatabaseService = new TestDatabaseService();
                testDatabaseService.DatabaseName = fileProcessingDb.DatabaseName;
                testDatabaseService.DatabaseServer = fileProcessingDb.DatabaseServer;
                testDatabaseService.Description = "Test";
                testDatabaseService.DatabaseServiceID = 1;
                testDatabaseService.Schedule = new ScheduledEvent
                {
                    Start = Now,
                    RecurrenceUnit = DateTimeUnit.Minute
                };

                testDatabaseService.UpdateDatabaseServiceSettings();

                // Build the connection string from the settings
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
                sqlConnectionBuild.DataSource = testDatabaseService.DatabaseServer;
                sqlConnectionBuild.InitialCatalog = testDatabaseService.DatabaseName;

                sqlConnectionBuild.IntegratedSecurity = true;
                sqlConnectionBuild.NetworkLibrary = "dbmssocn";
                sqlConnectionBuild.MultipleActiveResultSets = true;

                // This should start the  processing
                using (DatabaseServiceManager manager = new DatabaseServiceManager("ETL:Test", testDatabaseService.DatabaseServer,
                    testDatabaseService.DatabaseName, new List<string> { "ETL:Test" }, 1))
                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {



                    connection.Open();

                    string databaseServiceQuery = @"
                        Select 
                            [LastFileTaskSessionIDProcessed]
                            ,[StartTime]
                            ,[LastWrite]
                            ,[EndTime]
                            ,[MachineID]
                            ,[Exception]
                            ,[ActiveServiceMachineID]
                            ,[NextScheduledRunTime]
                            ,[ActiveFAMID]
                            ,[ActiveMachine].[MachineName] ActiveMachineName
                            ,[Machine].MachineName
                                    
                        FROM DatabaseService 
                            LEFT JOIN Machine ActiveMachine on ActiveMachine.ID = DatabaseService.ActiveServiceMachineID 
                            LEFT JOIN Machine ON Machine.ID = DatabaseService.MachineID
                        Where DatabaseService.ID = 1";
                    IDataRecord first;

                    using (var serviceCmd = connection.CreateCommand())
                    {
                        serviceCmd.CommandText = databaseServiceQuery;
                        first = serviceCmd.ExecuteReader().Cast<IDataRecord>().Single();
                    }


                    // Check that the machine name is this machine
                    Assert.AreEqual(Environment.MachineName, first["ActiveMachineName"], "Active machine is current machine");
                    var nextScheduled = first["NextScheduledRunTime"];
                    Assert.That(nextScheduled != DBNull.Value, "NextScheduledRunTime should be set.");

                    var nextOccurence = testDatabaseService.Schedule.GetNextOccurrence();

                    Assert.That(nextOccurence != null, "There is not a next occurrence for the schedule.");

                    TimeSpan ts = (DateTime)testDatabaseService.Schedule.GetNextOccurrence() - (DateTime)nextScheduled;

                    Assert.That(Math.Abs(ts.TotalSeconds) < 1, "NextScheduledRunTime should match the Schedule next occurrence.");

                    // The Process method should be called within a minute (allowing for up to 5 seconds longer)
                    Assert.That(TestDatabaseService.StartedEvent.WaitOne(65000), "Process should start within 1 minute");

                    IDataRecord whileRunning;
                    using (var afterCmd = connection.CreateCommand())
                    {
                        afterCmd.CommandText = databaseServiceQuery;
                        whileRunning = afterCmd.ExecuteReader().Cast<IDataRecord>().Single();
                    }
                    nextScheduled = whileRunning["NextScheduledRunTime"];
                    Assert.That(nextScheduled != DBNull.Value, "NextScheduledRunTime should be set.");
                    Assert.That(nextOccurence != null, "There is not a next occurrence for the schedule.");

                    ts = (DateTime)testDatabaseService.Schedule.GetNextOccurrence() - (DateTime)nextScheduled;

                    Assert.That(Math.Abs(ts.TotalSeconds) < 1, "NextScheduledRunTime should match the Schedule next occurrence.");
                    Assert.AreNotEqual(DBNull.Value, whileRunning["StartTime"], "StartTime should be set");

                    ts = Now - (DateTime)whileRunning["StartTime"];
                    Assert.That(Math.Abs(ts.TotalSeconds) < 5, "StartTime should be recent ");
                    Assert.AreEqual(DBNull.Value, whileRunning["LastWrite"], "LastWrite should be null");
                    Assert.AreEqual(DBNull.Value, whileRunning["EndTime"], "EndTime should be null");
                    Assert.AreEqual(whileRunning["ActiveMachineName"], whileRunning["MachineName"], "ActiveMachineName and machineName should be the same");
                    Assert.AreNotEqual(DBNull.Value, whileRunning["ActiveFAMID"], "ActiveFAMId is not null");
                    Assert.AreEqual(first["ActiveFAMID"], whileRunning["ActiveFAMID"], "ActiveFAMID is the same as before");

                    TestDatabaseService.FinishEvent.Set();
                    DateTime afterFinish = Now;

                    // wait to make sure the last run is complete
                    Thread.Sleep(1000);

                    IDataRecord afterFinishedData;
                    using (var afterFinishedCmd = connection.CreateCommand())
                    {
                        afterFinishedCmd.CommandText = databaseServiceQuery;
                        afterFinishedData = afterFinishedCmd.ExecuteReader().Cast<IDataRecord>().Single();
                    }

                    Assert.AreEqual(whileRunning["StartTime"], afterFinishedData["StartTime"], "StartTime while running should be the same after running");
                    Assert.AreNotEqual(DBNull.Value, afterFinishedData["EndTime"]);
                    ts = afterFinish - (DateTime)afterFinishedData["EndTime"];
                    Assert.That(Math.Abs(ts.TotalSeconds) < 5, "EndTime should be recent");
                    Assert.That((DateTime)afterFinishedData["StartTime"] < (DateTime)afterFinishedData["LastWrite"]
                        && (DateTime)afterFinishedData["LastWrite"] < (DateTime)afterFinishedData["EndTime"],
                        "LastWriteTime should be between StartTime and EndTime");

                    nextScheduled = afterFinishedData["NextScheduledRunTime"];
                    Assert.That(nextScheduled != DBNull.Value, "NextScheduledRunTime should be set.");
                    Assert.That(nextOccurence != null, "There is not a next occurrence for the schedule.");

                    ts = (DateTime)testDatabaseService.Schedule.GetNextOccurrence() - (DateTime)nextScheduled;
                    Assert.That(Math.Abs(ts.TotalSeconds) < 1, "NextScheduledRunTime should match the Schedule next occurrence.");


                    // Clear the Active FAM table
                    using (var clearActiveFAMCmd = connection.CreateCommand())
                    {
                        clearActiveFAMCmd.CommandText = "DELETe FROM ActiveFAM";
                        clearActiveFAMCmd.ExecuteNonQuery();
                    }

                    // Wait for the next run
                    Assert.That(TestDatabaseService.StartedEvent.WaitOne(65000), "Process should start withing 1 minute");
                    IDataRecord afterDeleteActive;
                    using (var afterDeleteActiveCmd = connection.CreateCommand())
                    {
                        afterDeleteActiveCmd.CommandText = databaseServiceQuery;
                        afterDeleteActive = afterDeleteActiveCmd.ExecuteReader().Cast<IDataRecord>().Single();
                    }

                    Assert.AreNotEqual(DBNull.Value, afterDeleteActive["ActiveFAMID"], "ActiveFAMID is not null");
                    Assert.AreNotEqual(afterFinishedData["ActiveFAMID"], afterDeleteActive["ActiveFAMID"], "There should be a new ActiveFAMID");
                    TestDatabaseService.FinishEvent.Set();

                    manager.Stop();

                    manager.StoppedWaitHandle.WaitOne();

                    IDataRecord afterStop;
                    using (var afterStopCmd = connection.CreateCommand())
                    {
                        afterStopCmd.CommandText = databaseServiceQuery;
                        afterStop = afterStopCmd.ExecuteReader().Cast<IDataRecord>().Single();
                    }

                    Assert.AreEqual(DBNull.Value, afterStop["ActiveFAMID"], "ActiveFAMID should be null after stop");
                    Assert.AreEqual(DBNull.Value, afterStop["NextScheduledRunTime"], "NextScheduledRunTime should be null after stop");
                    Assert.AreEqual(DBNull.Value, afterStop["ActiveMachineName"], "The Active machine should be null after stop");
                }

            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        #endregion
    }
}
