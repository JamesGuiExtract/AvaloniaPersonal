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


/*
NOTES
Notation in comments:
* Step is not expected to be possible at Duke
[] Event that, for DUKE, will either not be sent to us or that we will ignore via Corepoint.

Configuration: 
	- Receive all ADT events? (DUKE: no)
	- Ignore patients w/o order? (DUKE: yes)
	- Update patient info for all ADT messages? (DUKE: yes but only receiving udpate messages...)
	- Handle patient deletes? (DUKE: no)

See the config file for:
    1) connection string
    2) ADT and ORM "drop" directories
*/

namespace Extract.DataEntry.LabDE.Test
{
    [TestFixture]
    [NUnit.Framework.Category("DataEntryQueries")]    
    public class TestAdtMergeUnmerge
    {

        #region Constants        
        /// <summary>
        /// Patient constants for patients A, B, and C.
        /// </summary>
        private const string PatientA = "00000033";
        private const string PatientB = "00000034";
        private const string PatientC = "00000035";

        /// <summary>
        /// Order constants that correspond to patients A, B, and C.
        /// </summary>
        private const string OrderA = "33000001";
        private const string OrderB = "34000001";
        private const string OrderC = "35000001";

        /// <summary>
        /// The patient query template, with one parameter (MRN).
        /// </summary>
        private const string PatientQuery =
            "select MRN, FirstName, LastName, DOB, Gender, MergedInto, CurrentMRN from LabDEPatient where MRN='{0}';";

        /// <summary>
        /// The order query template, with one parameter (MRN).
        /// </summary>
        private const string OrderQuery =
            "select OrderNumber, OrderCode, PatientMRN from LabDEOrder where PatientMRN='{0}';";

        /// <summary>
        /// Re-usable "expected" Patient text. Patient data retrieved from the patient table will
        /// match this template, with four parameterized values.
        /// e.g. {0} = '00000033', {1} = '00000033', {2} = '33000001', {3} = 00000033
        /// so parameter 0 is MRN, 1 is DOB, 2 is MergedInto (MRN), 3 is CurrentMRN                
        /// </summary>
        private const string ORM_PatientText = "{0}, CHAD, CHAN, {1}, M, {2}, {3}";

        /// <summary>
        /// re-usable "expected" order text. Order data retrieved from the order table will
        /// match this template, with two parameterized values.
        /// {0} is OrderNumber, {1} is PatientMRN.        
        /// </summary>
        private const string ORM_OrderText = "{0}, GLU, {1}";

        /// <summary>
        /// Following are the HL7 test message resource files.
        /// </summary>
        static readonly string _Merge_AtoB = "Resources.Merge_AtoB.txt";
        static readonly string _Merge_AtoC = "Resources.Merge_AtoC.txt";
        static readonly string _Merge_BtoC = "Resources.Merge_BtoC.txt";
        static readonly string _Merge_CtoA = "Resources.Merge_CtoA.txt";
        static readonly string _Merge_CtoB = "Resources.Merge_CtoB.txt";

        static readonly string _ORM_A = "Resources.ORM_A.txt";
        static readonly string _ORM_A_UpdateA = "Resources.ORM_A_UpdateA.txt";
        static readonly string _ORM_B = "Resources.ORM_B.txt";
        static readonly string _ORM_B_UpdateB = "Resources.ORM_B_UpdateB.txt";

        static readonly string _Unmerge_A = "Resources.Unmerge_A.txt";
        static readonly string _Unmerge_B = "Resources.Unmerge_B.txt";

        #endregion Constants

        #region Fields        
        /// <summary>
        /// Manages the test text files used by these tests (HL7 messages)
        /// </summary>
        static TestFileManager<TestAdtMergeUnmerge> _testTextFiles;

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

            _testTextFiles = new TestFileManager<TestAdtMergeUnmerge>();

            string dbConnectString = 
                ConfigurationManager.ConnectionStrings["LabDECoreTestingAutomatedConnectionString"].ConnectionString;

            _AdtDirectory = ConfigurationManager.AppSettings["AdtDropDirectory"];
            _OrmDirectory = ConfigurationManager.AppSettings["OrmDropDirectory"];

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Warning: Verify test configuration!\nADT directory: {0}\nORM directory: {1}\n" +
                            "DB Connect string: {2}\n\n" +
                            "Corepoint server required set up:\n" +
                            "Receive all ADT events? (DUKE: no)\n" +
                            "Ignore patients w/o order? (DUKE: yes)\n" +
                            "Update patient info for all ADT messages? (DUKE: yes)\n" +
                            "Handle patient deletes? (DUKE: no)",
                            _AdtDirectory,
                            _OrmDirectory,
                            dbConnectString);

            CustomizableMessageBox cmb = new CustomizableMessageBox();
            cmb.Caption = "Check configuration settings";
            cmb.Text = sb.ToString();
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

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            _dbConnection.Close();
            _dbConnection.Dispose();
        }
        #endregion Setup and Teardown

        #region Public Test Functions
        /// <summary>
        ///        	ORM(A)
        ///	        Merge A->B
        ///	        Expect: ORM = B
        /// </summary>
        [Test, Category("Interactive")]        
        public static void Test1()
        {
            StartTest();

            CopyToDropDirectory(_OrmDirectory, _ORM_A);
            string patientResult = GetSqlResults(MakePatientQuery(PatientA));
            string orderResult = GetSqlResults(MakeOrderQuery(PatientA));
            string result = patientResult + ", " + orderResult;

            // Expected text returned from query:
            // "00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000033, 33000001, GLU, 00000033"
            string expectedText = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", "", PatientA) + ", " + 
                                  ExpectedOrderText(OrderA, PatientA);
            Assert.That(ExpectedMatch(result, expectedText));

            CopyToDropDirectory(_AdtDirectory, _Merge_AtoB);

            // Expected text after merge:
            // 00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000034, 00000034, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, NULL, 00000034, 
            // 33000001, GLU, 00000033"
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + p2Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", PatientB, PatientB) + ", " +
                               ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", "", PatientB) + ", " + 
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        ///        	ORM(B)
        ///        	Merge A->B 
        ///	        Expect: ORM = B
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test2()
        {
            StartTest();

            CopyToDropDirectory(_OrmDirectory, _ORM_B);
            string patientResult = GetSqlResults(MakePatientQuery(PatientB));
            string orderResult = GetSqlResults(MakeOrderQuery(PatientB));
            string result = patientResult + ", " + orderResult;

            // Expected text returned from query:
            // "00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000034, 34000001, GLU, 00000034"
            string expectedText = ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", "", PatientB) + ", " +
                                  ExpectedOrderText(OrderB, PatientB);
            Assert.That(ExpectedMatch(result, expectedText));

            CopyToDropDirectory(_AdtDirectory, _Merge_AtoB);

            // Expected text after merge:
            // same as above
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientB));
            string result2 = p2Result + ", " + order2Result;

            Assert.That(ExpectedMatch(result2, expectedText));
        }

        //
        // Where is Test3? Test4 is almost identical to Test3; the difference is that Test4 updates PatientA
        // in the unmerge operation. Hence Test3 has been omitted.
        //

        /// <summary>
        ///        	ORM(A)
        ///	        Merge A->B
        ///         Unmerge A
        ///	        Expect: ORM = A, with updated data
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test4()
        {
            Test1();

            CopyToDropDirectory(_AdtDirectory, _Unmerge_A);

            // Expected text after unmerge:
            // "00000033, CHAD, CHAN, 12/10/1970 12:00:00 AM, M, NULL, 00000033, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, NULL, 00000034, 
            // 33000001, GLU, 00000033"
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + p2Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/10/1970 12:00:00 AM", "", PatientA) + ", " +
                               ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", "", PatientB) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// ORM(A)
        /// Merge A->B
        ///	ORM(A) (update A)
        ///	Expect: ORM = B, A has been updated
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test5A()
        {
            Test1();

            CopyToDropDirectory(_OrmDirectory, _ORM_A_UpdateA);

            // Expected text:
            // "00000033, CHAD, CHAN, 12/10/1970 12:00:00 AM, M, 00000034, 00000034, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000034, 
            // 33000001, GLU, 00000033"
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + p2Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/10/1970 12:00:00 AM", PatientB, PatientB) + ", " +
                               ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", "", PatientB) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// ORM(A)
        /// Merge A->B
        ///	ORM(A) (update A)
        ///	Expect: ORM = B, A has been updated
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test5B()
        {
            Test1();

            CopyToDropDirectory(_OrmDirectory, _ORM_B_UpdateB);

            // Expected text:
            // "00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000034, 00000034, 
            // 00000034, CHAD, CHAN, 12/10/1970 12:00:00 AM, M, , 00000034, 
            // 33000001, GLU, 00000033"
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + p2Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", PatientB, PatientB) + ", " +
                               ExpectedPatientText(PatientB, "12/10/1970 12:00:00 AM", "", PatientB) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// [Merge A->B]
        /// ORM(B) 
        /// Expect: ORM = B
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test6()
        {
            StartTest();

            CopyToDropDirectory(_AdtDirectory, _Merge_AtoB);
            string patientResult = GetSqlResults(MakePatientQuery(PatientA));
            string orderResult = GetSqlResults(MakeOrderQuery(PatientA));
            string result = patientResult + orderResult;

            // Expected text returned from query: empty string
            Assert.That(String.IsNullOrWhiteSpace(result));

            CopyToDropDirectory(_OrmDirectory, _ORM_B);

            // Expected text after merge:
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000034, 
            // 34000001, GLU, 00000034"
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientB));
            string result2 = p2Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", "", PatientB) + ", " +
                               ExpectedOrderText(OrderB, PatientB);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// [Merge A->B]
        /// ORM(B)
        /// [Unmerge A]
        /// Expect: ORM = B
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test7()
        {
            Test6();

            CopyToDropDirectory(_AdtDirectory, _Unmerge_A);
            Assert.That(String.Empty == GetSqlResults(MakePatientQuery(PatientA)));
            string patientResult = GetSqlResults(MakePatientQuery(PatientB));
            string orderResult = GetSqlResults(MakeOrderQuery(PatientB));
            string result = patientResult + ", " + orderResult;

            string expected = ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", "", PatientB) + ", " +
                               ExpectedOrderText(OrderB, PatientB);

            // Expected text returned from query: empty string
            Assert.That(ExpectedMatch(result, expected));
        }

        /// <summary>
        /// [Unmerge A]
        /// ORM(A)
        /// [Unmerge A]
        /// Expect: ORM = A
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test8()
        {
            StartTest();

            CopyToDropDirectory(_AdtDirectory, _Unmerge_A);
            string pResult = GetSqlResults(MakePatientQuery(PatientA));
            string oResult = GetSqlResults(MakeOrderQuery(PatientA));
            Assert.That(String.IsNullOrWhiteSpace(pResult) && String.IsNullOrWhiteSpace(oResult));

            CopyToDropDirectory(_OrmDirectory, _ORM_A);
            string patientResult = GetSqlResults(MakePatientQuery(PatientA));
            string orderResult = GetSqlResults(MakeOrderQuery(PatientA));
            string result = patientResult + ", " + orderResult;

            // Expected text returned from query:
            // "00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000033, 33000001, GLU, 00000033"
            string expectedText = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", "", PatientA) + ", " +
                                  ExpectedOrderText(OrderA, PatientA);
            Assert.That(ExpectedMatch(result, expectedText));

            CopyToDropDirectory(_AdtDirectory, _Unmerge_A);

            // Expected text after merge has not changed except that Patient A has updated DOB
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/10/1970 12:00:00 AM", "", PatientA) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// ORM(A) - adds A to tables
        /// Merge A->B - merges A and B
        /// Merge B->C
        /// Expect: ORM = C
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test9()
        {
            Test1();

            CopyToDropDirectory(_AdtDirectory, _Merge_BtoC);

            // Expected text after merge:
            // "00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000034, 00000035, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000035, 00000035, 
            // 00000035, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000035, 
            // 33000001, GLU, 00000033"
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string p3Result = GetSqlResults(MakePatientQuery(PatientC));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + p2Result + ", " + p3Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", PatientB, PatientC) + ", " +
                               ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", PatientC, PatientC) + ", " +
                               ExpectedPatientText(PatientC, "12/12/1970 12:00:00 AM", "", PatientC) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// ORM(A) - adds A to tables
        /// Merge A->B - merges A and B
        /// Merge C->B
        /// [Merge C->B]
        /// Expect: ORM = B
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test10()
        {
            Test1();

            // Note: this updates Patient B DOB
            CopyToDropDirectory(_AdtDirectory, _Merge_CtoB);

            // Expected text after merge:
            // "00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000034, 00000034, 
            // 00000034, CHAD, CHAN, 12/10/1970 12:00:00 AM, M, , 00000034, 
            // 33000001, GLU, 00000033"
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string p3Result = GetSqlResults(MakePatientQuery(PatientC));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + p2Result + ", " + p3Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", PatientB, PatientB) + ", " +
                               ExpectedPatientText(PatientB, "12/10/1970 12:00:00 AM", "", PatientB) + ", " +
                               ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// ORM(A) - adds A to tables
        /// Merge A->B - merges A and B
        /// Merge A->B
        /// Merge A->C
        /// Expect: ORM = C
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test11()
        {
            Test1();

            CopyToDropDirectory(_AdtDirectory, _Merge_AtoC);

            // Expected text after merge:
            // "00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000035, 00000035, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000034,
            // 00000035, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000035,  
            // 33000001, GLU, 00000033"
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string p3Result = GetSqlResults(MakePatientQuery(PatientC));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + p2Result + ", " + p3Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", PatientC, PatientC) + ", " +
                               ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", "", PatientB) + ", " +
                               ExpectedPatientText(PatientC, "12/12/1970 12:00:00 AM", "", PatientC) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// [Merge A->B]
        /// ORM(B) 
        /// [*Merge A->C]
        /// Expect: ORM = B
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test12()
        {
            Test6();

            CopyToDropDirectory(_AdtDirectory, _Merge_AtoC);
            // Expected text after merge:
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000034, 
            // 34000001, GLU, 00000034"
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientB));
            string result2 = p2Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", "", PatientB) + ", " +
                               ExpectedOrderText(OrderB, PatientB);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// ORM(A)
        /// Merge A->B
        /// Merge B->C
        /// Unmerge B
        /// Expect: ORM = B
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test13()
        {
            Common13_14_15();

            CopyToDropDirectory(_AdtDirectory, _Unmerge_B);

            // Expected text after unmerge:
            // 00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000034, 00000034, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, NULL, 00000034, 
            // 00000035, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, NULL, 00000035, 
            // 33000001, GLU, 00000033"
            string p1Res = GetSqlResults(MakePatientQuery(PatientA));
            string p2Res = GetSqlResults(MakePatientQuery(PatientB));
            string p3Res = GetSqlResults(MakePatientQuery(PatientC));
            string order2Res = GetSqlResults(MakeOrderQuery(PatientA));
            string result = p1Res + ", " + p2Res + ", " + p3Res + ", " + order2Res;

            string expected = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", PatientB, PatientB) + ", " +
                               ExpectedPatientText(PatientB, "12/10/1970 12:00:00 AM", "", PatientB) + ", " +
                               ExpectedPatientText(PatientC, "12/12/1970 12:00:00 AM", "", PatientC) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result, expected));            
        }

        /// <summary>
        /// ORM(A)
        /// Merge A->B
        /// Merge B->C
        /// Unmerge A
        /// Expect: ORM = A
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test14()
        {
            Common13_14_15();

            // NOTE: this operation updates Patient A DOB
            CopyToDropDirectory(_AdtDirectory, _Unmerge_A);

            // Expected text after unmerge A:
            // 00000033, CHAD, CHAN, 12/10/1970 12:00:00 AM, M, NULL, 00000033, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000035, 00000035, 
            // 00000035, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, NULL, 00000035, 
            // 33000001, GLU, 00000033"
            string p1Res = GetSqlResults(MakePatientQuery(PatientA));
            string p2Res = GetSqlResults(MakePatientQuery(PatientB));
            string p3Res = GetSqlResults(MakePatientQuery(PatientC));
            string order2Res = GetSqlResults(MakeOrderQuery(PatientA));
            string result = p1Res + ", " + p2Res + ", " + p3Res + ", " + order2Res;

            string expected = ExpectedPatientText(PatientA, "12/10/1970 12:00:00 AM", "", PatientA) + ", " +
                               ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", PatientC, PatientC) + ", " +
                               ExpectedPatientText(PatientC, "12/12/1970 12:00:00 AM", "", PatientC) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result, expected));
        }

        /// <summary>
        /// ORM(A)
        /// Merge A->B
        /// Merge B->C
        /// *Merge C->A
        /// Expect: ORM = A
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test15()
        {
            Common13_14_15();

            CopyToDropDirectory(_AdtDirectory, _Merge_CtoA);

            // Expected text after merge C to A (will fail, leaving state the same as at end of Common13_14_15):
            // 00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000034, 00000035, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000035, 00000035, 
            // 00000035, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, NULL, 00000035, 
            // 33000001, GLU, 00000033"
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string p3Result = GetSqlResults(MakePatientQuery(PatientC));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + p2Result + ", " + p3Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", PatientB, PatientC) + ", " +
                               ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", PatientC, PatientC) + ", " +
                               ExpectedPatientText(PatientC, "12/12/1970 12:00:00 AM", "", PatientC) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// [Merge A->B]
        /// ORM(B) - create B
        /// Merge B->C - create C, merge B to C
        /// *Merge C->A
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test16()
        {
            Test6();

            CopyToDropDirectory(_AdtDirectory, _Merge_BtoC);

            // Expected text after merge B to C
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000035, 00000035, 
            // 00000035, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, NULL, 00000035, 
            // 34000001, GLU, 00000034"
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string p3Result = GetSqlResults(MakePatientQuery(PatientC));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientB));
            string result2 = p2Result + ", " + p3Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", PatientC, PatientC) + ", " +
                               ExpectedPatientText(PatientC, "12/12/1970 12:00:00 AM", "", PatientC) + ", " +
                               ExpectedOrderText(OrderB, PatientB);

            Assert.That(ExpectedMatch(result2, expected2));

            // Note that in this message, Patient A is created with DOB 12/10/1970.
            CopyToDropDirectory(_AdtDirectory, _Merge_CtoA);

            // Expected text after merge C to A
            // 00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000033, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000035, 00000033, 
            // 00000035, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000033, 00000033, 
            // 34000001, GLU, 00000034"
            string p1Res = GetSqlResults(MakePatientQuery(PatientA));
            string p2Res = GetSqlResults(MakePatientQuery(PatientB));
            string p3Res = GetSqlResults(MakePatientQuery(PatientC));
            string order2Res = GetSqlResults(MakeOrderQuery(PatientB));
            string result = p1Res + ", " + p2Res + ", " + p3Res + ", " + order2Res;

            string expected = ExpectedPatientText(PatientA, "12/10/1970 12:00:00 AM", "", PatientA) + ", " +
                              ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", PatientC, PatientA) + ", " +
                              ExpectedPatientText(PatientC, "12/12/1970 12:00:00 AM", PatientA, PatientA) + ", " +
                              ExpectedOrderText(OrderB, PatientB);

            Assert.That(ExpectedMatch(result, expected));
        }

        /// <summary>
        /// ORM(A)
        /// [Merge B->C]
        /// *Merge A->B
        /// 
        /// Expect: Same as Test 1, as [Merge B->C] will be ignored by Corepoint.
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test17()
        {
            StartTest();

            CopyToDropDirectory(_OrmDirectory, _ORM_A);
            string patientResult = GetSqlResults(MakePatientQuery(PatientA));
            string orderResult = GetSqlResults(MakeOrderQuery(PatientA));
            string result = patientResult + ", " + orderResult;

            // Expected text returned from query:
            // "00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000033, 33000001, GLU, 00000033"
            string expectedText = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", "", PatientA) + ", " +
                                  ExpectedOrderText(OrderA, PatientA);
            Assert.That(ExpectedMatch(result, expectedText));

            CopyToDropDirectory(_AdtDirectory, _Merge_BtoC);
            string pRes = GetSqlResults(MakePatientQuery(PatientA));
            string oRes = GetSqlResults(MakeOrderQuery(PatientA));
            string res = pRes + ", " + oRes;

            // Expected text returned from query:
            // "00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, , 00000033, 33000001, GLU, 00000033"
            string expected = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", "", PatientA) + ", " +
                              ExpectedOrderText(OrderA, PatientA);
            Assert.That(ExpectedMatch(res, expected));

            CopyToDropDirectory(_AdtDirectory, _Merge_AtoB);

            // Expected text after merge:
            // "00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000034, 00000034, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, NULL, 00000034, 
            // 33000001, GLU, 00000033"
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + p2Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", PatientB, PatientB) + ", " +
                               ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", "", PatientB) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }
        #endregion Public Test Functions

        #region Private Functions

        /// <summary>
        /// Extracts the value from name value pair, delimited by '='.
        /// </summary>
        /// <param name="nameValuePair">The name value pair.</param>
        /// <returns>The requested value.</returns>
        /// NOTE: This function asserts if the input is null or empty, 
        /// if there isn't an '=' character in the input,
        /// or if the value is empty.
        private static string ValueFromNameValuePair(string nameValuePair)
        {
            Assert.That(!String.IsNullOrWhiteSpace(nameValuePair));

            const int notFound = -1;
            int startOfValue = nameValuePair.IndexOf('=');
            Assert.That(startOfValue != notFound);

            startOfValue += 1;

            var value = nameValuePair.Substring(startOfValue);
            Assert.That(!String.IsNullOrWhiteSpace(value));

            return value;
        }

        /// <summary>
        /// Makes the patient query for the specified MRN.
        /// </summary>
        /// <param name="MRN">The MRN of the patient to query for</param>
        /// <returns>returns patient query</returns>
        private static string MakePatientQuery(string MRN)
        {
            return String.Format(PatientQuery, MRN);
        }

        /// <summary>
        /// Makes the order query for the specified MRN.
        /// </summary>
        /// <param name="MRN">The MRN of the order to query for</param>
        /// <returns>returns order query</returns>
        private static string MakeOrderQuery(string MRN)
        {
            return String.Format(OrderQuery, MRN);
        }

        /// <summary>
        /// Makes the table text.
        /// </summary>
        /// <param name="MRN">The expected patient MRN.</param>
        /// <param name="DOB">The expected patient dob.</param>
        /// <param name="mergedInto">The expected MRN that should be present in the mergedinto column.</param>
        /// <param name="currentMRN">The expected MRN that should be present in the currentMRN column.</param>
        /// <returns></returns>
        private static string ExpectedPatientText(string MRN,
                                                  string DOB,
                                                  string mergedInto,
                                                  string currentMRN)
        {
            return String.Format(ORM_PatientText, MRN, DOB, mergedInto, currentMRN);
        }

        /// <summary>
        /// Makes the expected order text.
        /// </summary>
        /// <param name="orderNumber">The expected order number.</param>
        /// <param name="patientMRN">The expected patient MRN.</param>
        /// <returns></returns>
        private static string ExpectedOrderText(string orderNumber, string patientMRN)
        {
            return String.Format(ORM_OrderText, orderNumber, patientMRN);
        }

        /// <summary>
        /// Copies the specified file to the specified "drop" directory. The "drop" directory
        /// is one which Corepoint has been configured to receive files into. For this test,
        /// there are two such directories, one for ADT messages (files), another for ORM messages.
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
        /// Gets the SQL results using the specified query.
        /// </summary>
        /// <param name="query">The specified query. 
        /// Note that the query is assumed to only return a single row.</param>
        /// <returns>A string with all of the column values pasted together with commas. 
        /// There is no comma on the final column.</returns>
        /// NOTE: This function waits for an amount of time to ensure that prior operations have had time 
        /// to be written into the DB tables.
        private static string GetSqlResults(string query)
        {
            const int twoSeconds = 2 * 1000;
            Thread.Sleep(twoSeconds);

            SqlCommand cmd = new SqlCommand(query, _dbConnection);
            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                    return "";

                StringBuilder sb = new StringBuilder();

                while (reader.Read())
                {
                    int numberOfColumns = reader.FieldCount;
                    for (int column = 0; column < numberOfColumns; ++column)
                    {
                        string value = reader[column].ToString();

                        if (column < numberOfColumns - 1)
                            sb.AppendFormat("{0}, ", value);
                        else
                            sb.AppendFormat("{0}", value);
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// This function is primarily a logical description of the operation, and a good 
        /// location for displaying the result and expected strings for trouble-shooting.
        /// </summary>
        /// <param name="result">The result (read from the DB tables).</param>
        /// <param name="expected">The expected value.</param>
        /// <returns></returns>
        private static bool ExpectedMatch(string result, string expected)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\n  Result: {0}\nExpected: {1}", result, expected);
            Debug.WriteLine(sb.ToString());

            return result == expected;
        }

        /// <summary>
        /// This test is the common base for tests 13, 14, and 15
        /// ORM(A)
        /// Merge A->B
        /// Merge B->C
        /// </summary>
        private static void Common13_14_15()
        {
            Test1();

            CopyToDropDirectory(_AdtDirectory, _Merge_BtoC);

            // Expected text after merge:
            // 00000033, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000034, 00000035, 
            // 00000034, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, 00000035, 00000035, 
            // 00000035, CHAD, CHAN, 12/12/1970 12:00:00 AM, M, NULL, 00000035, 
            // 33000001, GLU, 00000033"
            string p1Result = GetSqlResults(MakePatientQuery(PatientA));
            string p2Result = GetSqlResults(MakePatientQuery(PatientB));
            string p3Result = GetSqlResults(MakePatientQuery(PatientC));
            string order2Result = GetSqlResults(MakeOrderQuery(PatientA));
            string result2 = p1Result + ", " + p2Result + ", " + p3Result + ", " + order2Result;

            string expected2 = ExpectedPatientText(PatientA, "12/12/1970 12:00:00 AM", PatientB, PatientC) + ", " +
                               ExpectedPatientText(PatientB, "12/12/1970 12:00:00 AM", PatientC, PatientC) + ", " +
                               ExpectedPatientText(PatientC, "12/12/1970 12:00:00 AM", "", PatientC) + ", " +
                               ExpectedOrderText(OrderA, PatientA);

            Assert.That(ExpectedMatch(result2, expected2));
        }

        /// <summary>
        /// Cleans up the database - removes all the test entries from the patient table, 
        /// and any associated orders (via cascade delete).
        /// </summary>
        private static void CleanupDb()
        {
            string stmt = String.Format("delete from LabDEPatient where MRN='{0}' OR MRN='{1}' OR MRN='{2}';",
                                        PatientA,
                                        PatientB,
                                        PatientC);

            using SqlCommand cmd = new SqlCommand(stmt, _dbConnection);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// This is a commonly used test preamble.
        /// </summary>
        private static void StartTest()
        {
            CleanupDb();
        }

        #endregion Private Functions
    }
}
