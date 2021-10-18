using Extract.Testing.Utilities;
using Extract.Utilities.Forms;
using NUnit.Framework;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using static Extract.DataEntry.LabDE.Test.CommonTestMethods;
using static System.FormattableString;

namespace Extract.DataEntry.LabDE.Test
{
    [TestFixture]
    [NUnit.Framework.Category("DataEntryQueries")]
    public class TestOrderAndEncounterTables
    {
        #region Constants        

        /// <summary>
        /// Patient constants for patients A, B, and radiology.
        /// </summary>
        private const string PatientA = "00000001";
        private const string PatientB = "00000002";
        private const string RadiologyPatient = "00000006";

        static readonly string EncounterMsg_1 = "Resources.EncounterMsg_1.txt";
        static readonly string EncounterMsg_2 = "Resources.EncounterMsg_2.txt";

        static readonly string RadiologyOrder1 = "Resources.RadiologyOrder.txt";
        static readonly string RadiologyOrder2 = "Resources.RadiologyOrderUpdate.txt";

        #endregion Constants

        #region Fields        
        /// <summary>
        /// Manages the test text files used by these tests (HL7 messages)
        /// </summary>
        static TestFileManager<TestOrderAndEncounterTables> _testTextFiles;

        static private SqlConnection _dbConnection;

        static private string _AdtDirectory;
        static private string _OrmDirectory;

        #endregion Fields

        #region Setup and Teardown

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testTextFiles = new TestFileManager<TestOrderAndEncounterTables>();

            // There are two copies of the config file, one for nunit use and one for VS debugging use.
            // The nunit config file is here: 
            //      \Engineering\RC.Net\Core\Testing\Automated\Extract-all.config
            // The config file used when debugging is here: 
            //      \engineering\RC.Net\DataEntry\LabDE\Core\Testing\Automated\app.config
            //

            string dbConnectString =
                ConfigurationManager.ConnectionStrings["LabDECoreTestingAutomatedConnectionString"].ConnectionString;

            _AdtDirectory = ConfigurationManager.AppSettings["AdtDropDirectory"];
            _OrmDirectory = ConfigurationManager.AppSettings["OrmDropDirectory"];

            CustomizableMessageBox cmb = new CustomizableMessageBox();
            cmb.Caption = "Check configuration settings";
            cmb.Text = Invariant($"Warning: Verify test configuration!\nADT directory: {_AdtDirectory}\n") +
                       Invariant($"ORM directory: {_OrmDirectory}\n") +
                       Invariant($"DB Connect string: {dbConnectString}\n\n");

            const int thirtySeconds = 30 * 1000;
            cmb.Timeout = thirtySeconds;
            cmb.UseDefaultOkButton = true;

            cmb.Show();

            _dbConnection = new SqlConnection(dbConnectString);
            _dbConnection.Open();

            ProgressStatus progressStatus = new ProgressStatus();
            if (null != progressStatus)
            {
                FileProcessingDB fileProcessingDB = new FileProcessingDB();
                string connect =
                    ConfigurationManager.ConnectionStrings["LabDECoreTestingAutomatedConnectionString"].ConnectionString;

                // The connectionString looks like this:
                // connectionString="server=VM-MAIN;database=Demo_LabDE;Integrated Security=True"
                string[] parts = connect.Split(';');
                Assert.That(parts.Length >= 2);

                const int serverIndex = 0;
                const int databaseIndex = 1;

                string server = ValueFromNameValuePair(parts[serverIndex]);
                fileProcessingDB.DatabaseServer = server;

                string dbName = ValueFromNameValuePair(parts[databaseIndex]);
                fileProcessingDB.DatabaseName = dbName;

                // This shouldn't cause the entire test to fail...
                try
                {
                    fileProcessingDB.UpgradeToCurrentSchema(progressStatus);
                }
                catch (Exception)
                {
                }
            }
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            _dbConnection.Close();
            _dbConnection.Dispose();
        }
        #endregion Setup and Teardown

        #region Public Test Functions

        // These tests use the LabDE stored procedure LabDEAddOrUpdateEncounter, and
        // only test the LabDEEncounter table.
        #region LabDEAddOrUpdateEncounter

        [Test, Category("Interactive")]
        public static void Test1()
        {
            CleanupDb();

            // drop ADT message that should populate the LabDEEncounter table.
            CopyToDropDirectory(_AdtDirectory, EncounterMsg_1);

            // read the LabDEEncounter table
            var query = MakeEncounterQuery(PatientA);
            var result = GetSqlResults(query, _dbConnection);

            // compare results to expected
            var expected = ExpectedEncounterText(CSN: "12345",
                                                 PatientMRN: "00000001",
                                                 EncounterDateTime: "9/8/2016 8:51:35 AM",
                                                 Department: "64",
                                                 EncounterType: "R",
                                                 EncounterProvider: "02046");

            Assert.That(ExpectedMatch(result, expected));
        }

        // This test builds on the first by updating the Encounter info established in test 1 - 
        // hence it calls Test1, rather than celaring the DB.
        // This test updates the the department id (rom 64 to 63), and the encounter type (from R to A)
        [Test, Category("Interactive")]
        public static void Test2()
        {
            // Setup initial condition for this test, which updates LabDEEncounter.CSN value
            Test1();

            // drop ADT message that should update the LabDEEncounter table.
            CopyToDropDirectory(_AdtDirectory, EncounterMsg_2);

            // read the LabDEEncounter table
            const string CsnValue = "12345";
            var query = MakeEncounterQuery(PatientA, CsnValue);
            var result = GetSqlResults(query, _dbConnection);

            // compare results to expected
            var expected = ExpectedEncounterText(CSN: CsnValue,
                                                 PatientMRN: "00000001",
                                                 EncounterDateTime: "9/8/2016 8:51:35 AM",
                                                 Department: "63",
                                                 EncounterType: "A",
                                                 EncounterProvider: "02046");

            Assert.That(ExpectedMatch(result, expected));
        }

        #endregion LabDEAddOrUpdateEncounter

        // These tests use the LabDE LabDEAddOrUpdateOrderWithEncounter stored procedure, and test
        // both the LabDEOrder and LabDEEncounter tables.
        #region LabDEAddOrUpdateOrderWithEncounter

        /// <summary>
        /// This test adds a new order with corresponding add of associated encounter.
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test3()
        {
            CleanupTables(RadiologyPatient);

            // drop ORM message that should populate the LabDEEncounter table.
            CopyToDropDirectory(_OrmDirectory, RadiologyOrder1);

            // read the LabDEEncounter table
            var query = MakeEncounterQuery(RadiologyPatient);
            var result = GetSqlResults(query, _dbConnection);

            var expected = ExpectedEncounterText(CSN: "3CSN009",
                                                 PatientMRN: "00000006",
                                                 EncounterDateTime: "9/8/2016 12:00:00 AM",
                                                 Department: "ED",
                                                 EncounterType: "R",
                                                 EncounterProvider: "01438");

            Assert.That(ExpectedMatch(result, expected));

            // Read the LabDEOrder table
            var queryOrder = MakeOrderQuery(encounterID: "3CSN009");
            var resultOrder = GetSqlResults(queryOrder, _dbConnection);

            var expectedOrder = ExpectedOrderText(orderNumber: "40000004",
                                                  orderCode: "RAD1010",
                                                  patientMRN: "00000006",
                                                  referenceDateTime: "9/8/2016 1:10:35 PM",
                                                  encounterID: "3CSN009");

            Assert.That(ExpectedMatch(resultOrder, expectedOrder));
        }

        [Test, Category("Interactive")]
        public static void Test4()
        {
            Test3();

            // drop the update ORM message - update the values stored during test3
            CopyToDropDirectory(_OrmDirectory, RadiologyOrder2);

            // read the LabDEEncounter table
            var query = MakeEncounterQuery(RadiologyPatient, CSN: "3CSN009");
            var result = GetSqlResults(query, _dbConnection);

            var expected = ExpectedEncounterText(CSN: "3CSN009",
                                                 PatientMRN: "00000006",
                                                 EncounterDateTime: "9/8/2016 12:00:00 AM",
                                                 Department: "ED",
                                                 EncounterType: "E",
                                                 EncounterProvider: "01438");

            Assert.That(ExpectedMatch(result, expected));

            // Read the LabDEOrder table
            var queryOrder = MakeOrderQuery(encounterID: "3CSN009");
            var resultOrder = GetSqlResults(queryOrder, _dbConnection);

            var expectedOrder = ExpectedOrderText(orderNumber: "40000004",
                                                  orderCode: "RAD1010",
                                                  patientMRN: "00000006",
                                                  referenceDateTime: "9/9/2016 1:11:35 PM",
                                                  encounterID: "3CSN009");

            Assert.That(ExpectedMatch(resultOrder, expectedOrder));
        }

        #endregion LabDEAddOrUpdateOrderWithEncounter

        #endregion Public Test Functions

        #region Private Test functions

        /// <summary>
        /// Cleans up the database - removes all the test entries from the encounter table.
        /// </summary>
        private static void CleanupDb()
        {
            string stmt = Invariant($"delete from LabDEEncounter where PatientMRN='{PatientA}' OR ") +
                            Invariant($"PatientMRN='{PatientB}';");

            using SqlCommand cmd = new SqlCommand(stmt, _dbConnection);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Cleans up the database tables used by the LabDEAddOrUpdateOrderWithEncounter tests. 
        /// </summary>
        /// <param name="patientMrn">the PatentMRN value of the rows to remove</param>
        private static void CleanupTables(string patientMrn)
        {
            var cmd1 = new SqlCommand(Invariant($"delete from LabDEOrder where[PatientMRN] = '{patientMrn}';"),
                                      _dbConnection);
            cmd1.ExecuteNonQuery();

            var cmd2 = new SqlCommand(Invariant($"delete from LabDEEncounter where[PatientMRN] = '{patientMrn}';"),
                                      _dbConnection);
            cmd2.ExecuteNonQuery();
        }

        /// <summary>
        /// Copies the specified file to the specified "drop" directory. The "drop" directory
        /// is one which Corepoint has been configured to receive files into.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="targetFileName">Name of the target resource file.</param>
        private static void CopyToDropDirectory(string targetDirectory, string targetFileName)
        {
            string testFile = _testTextFiles.GetFile(targetFileName);
            Assert.That(File.Exists(testFile));

            Assert.That(Directory.Exists(targetDirectory));
            string copyFileTo = Path.Combine(targetDirectory, Path.GetFileName(targetFileName));
            File.Copy(testFile, copyFileTo);

            // Wait for Corepoint to move the file
            Debug.WriteLine("");

            const int oneSecond = 1 * 1000;
            while (File.Exists(copyFileTo))
            {
                Thread.Sleep(oneSecond);
                Debug.Write(".");
            }
        }

        /// <summary>
        /// Makes the patient query for the specified MRN.
        /// </summary>
        /// <param name="MRN">The MRN of the patient to query for</param>
        /// <param name="CSN">an optional CSN to query for</param>
        /// <returns>returns patient query</returns>
        private static string MakeEncounterQuery(string MRN, string CSN = null)
        {
            string encounterQuery = "select [CSN], [PatientMRN], [EncounterDateTime], [Department], " +
                                    "[EncounterType], [EncounterProvider] from " +
                                    Invariant($"[dbo].[LabDEEncounter] where [PatientMRN]='{MRN}'");
            string csnClause = ";";
            if (!String.IsNullOrWhiteSpace(CSN))
            {
                csnClause = Invariant($" AND [CSN]='{CSN}';");
            }

            return encounterQuery + csnClause;
        }

        /// <summary>
        /// Makes the order query for the specified patient MRN.
        /// </summary>
        /// <param name="MRN">The MRN of the patient to query for</param>
        /// <returns></returns>
        private static string MakeOrderQuery(string encounterID)
        {
            string orderQuery = "select [OrderNumber], [OrderCode], [PatientMRN], [ReferenceDateTime], " +
                                Invariant($"[EncounterID] from [dbo].[LabDEOrder] where [EncounterID]='{encounterID}';");
            return orderQuery;
        }

        /// <summary>
        /// Creates the expected text for an encounter.
        /// </summary>
        /// <param name="CSN"></param>
        /// <param name="PatientMRN"></param>
        /// <param name="EncounterDateTime"></param>
        /// <param name="Department"></param>
        /// <param name="EncounterType"></param>
        /// <param name="EncounterProvider"></param>
        /// <returns></returns>
        private static string ExpectedEncounterText(string CSN,
                                                    string PatientMRN,
                                                    string EncounterDateTime,
                                                    string Department,
                                                    string EncounterType,
                                                    string EncounterProvider)
        {
            return Invariant($"{CSN}, {PatientMRN}, {EncounterDateTime}, {Department}, {EncounterType}, {EncounterProvider}");
        }


        /// <summary>
        /// Creates the expected text for an order.
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <param name="orderCode"></param>
        /// <param name="patientMRN"></param>
        /// <param name="referenceDateTime"></param>
        /// <param name="encounterID"></param>
        /// <returns></returns>
        private static string ExpectedOrderText(string orderNumber,
                                                string orderCode,
                                                string patientMRN,
                                                string referenceDateTime,
                                                string encounterID)
        {
            return Invariant($"{orderNumber}, {orderCode}, {patientMRN}, {referenceDateTime}, {encounterID}");
        }


        #endregion Private Test functions

    }

}
