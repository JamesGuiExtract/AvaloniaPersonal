using ADODB;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE.Test
{
    /// <summary>
    /// Class to test LabDE stored procedures
    /// </summary>
    [Category("TestLabDEStoredProcedures")]
    [TestFixture]
    public class TestLabDEStoredProcedures
    {
        #region Constants/Read-only fields

        /// <summary>
        /// Test database
        /// </summary>
        static readonly string Demo_LabDE_DB = "Resources.Demo_LabDE.bak";

        /// <summary>
        /// Field names for the LabDEEncounter table
        /// </summary>
        static readonly List<string> _ENCOUNTER_FIELDS = new List<string>
        {
            "CSN"
            , "PatientMRN"
            , "EncounterDateTime"
            , "Department"
            , "EncounterType"
            , "EncounterProvider"
            , "ADTMessage"
            , "AdmissionDate"
            , "DischargeDate"
        };

        /// <summary>
        /// Field names for the LabDEOrder table
        /// </summary>
        static readonly List<string> _ORDER_FIELDS = new List<string>
        {
            "OrderNumber"
            , "OrderCode"
            , "PatientMRN"
            , "OrderStatus"
            , "ReferenceDateTime"
            , "ORMMessage"
            , "EncounterID"
            , "AccessionNumber"
        };

        /// <summary>
        /// Base query to return records from LabDEEncounter table with XML as string
        /// </summary>
        const string _LABDE_ENCOUNTER_QUERY =
            "SELECT CSN, " +
            "   PatientMRN, " +
            "   EncounterDateTime,  " +
            "   Department, " +
            "   EncounterType, " +
            "   EncounterProvider," +
            "   CONVERT(nvarchar(MAX), [ADTMessage]) as [ADTMessage],  " +
            "   AdmissionDate, " +
            "   DischargeDate " +
            "FROM [LabDEEncounter] ";

        /// <summary>
        /// Base query to return records from LabDEOrder table with XML as string
        /// </summary>
        const string _LABDE_ORDER_QUERY =
            "SELECT [OrderNumber]" +
            "      ,[OrderCode]" +
            "      ,[PatientMRN]" +
            "      ,[OrderStatus]" +
            "      ,[ReferenceDateTime]" +
            "      ,CONVERT(nvarchar(MAX), [ORMMessage]) as [ORMMessage]" +
            "      ,[EncounterID]" +
            "      ,[AccessionNumber] " +
            "FROM[dbo].[LabDEOrder] "; 

        #endregion
        
        #region Fields

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<TestLabDEStoredProcedures> _testDbManager;

        #endregion

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestLabDEStoredProcedures>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
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
        /// Test LabDEAddOrUpdateEncounter stored procedure
        /// </summary>
        [Test, Category("Automated")]
        public void LabDEAddOrUpdateEncounterTest()
        {
            string testDBName = "Test_LabDEAddOrUpdateEncounter";
            IFileProcessingDB _famDB = null;

            try
            {
                _famDB = CreateTestDatabase(testDBName);

                // Set up the data to use for the test
                var dataDictionary = new Dictionary<string, Tuple<string, string>>();
                LabDEEncounterValues(dataDictionary,
                    "11111111111111111111",
                    "0000000000000000001",
                    "2017/11/15 09:39:00 AM",
                    "TEST",
                    "TESTENC",
                    "PROVIDER",
                    "<unused>unused</unused>",
                    "NULL",
                    "NULL");

                LabDEAddOrUpdateEncounter(_famDB, dataDictionary);

                string encounterQuery = _LABDE_ENCOUNTER_QUERY + " WHERE CSN = '11111111111111111111' ";

                CheckResults(_famDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);

                // Call again to update data
                LabDEEncounterValues(dataDictionary,
                    "11111111111111111111",
                    "0000000000000000002",
                    "2017/11/14 09:39:00 AM",
                    "TEST2",
                    "TESTENC2",
                    "PROVIDER2",
                    "<unused>unused2</unused>",
                    "NULL",
                    "NULL");
                

                LabDEAddOrUpdateEncounter(_famDB, dataDictionary);

                CheckResults(_famDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _famDB = null;
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Test the LabDEAddOrUpdateOrder stored procedure
        /// </summary>
        [Test, Category("Automated")]
        public void LabDEAddOrUpdateOrderTest()
        {
            string testDBName = "Test_LabDEAddOrUpdateOrder";
            IFileProcessingDB _famDB = null;

            try
            {
                _famDB = CreateTestDatabase(testDBName);
                
                // Set up the data to use for the test
                var dataDictionary = new Dictionary<string, Tuple<string, string>>();
                LabDEOrderValues(dataDictionary,
                    "12345678901234567890123456789012345678901234567890",
                    "GLU",
                    "0000000000000000001",
                    "A",
                    "2010/01/14 09:39:00 AM",
                    "<unused>unused</unused>",
                    "NULL",
                    "NULL");

                LabDEAddOrUpdateOrder(_famDB, dataDictionary);

                string orderQuery = _LABDE_ORDER_QUERY + " WHERE OrderNumber = '12345678901234567890123456789012345678901234567890' ";

                CheckResults(_famDB, dataDictionary.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);

                // Call again to update data
                LabDEOrderValues(dataDictionary,
                    "12345678901234567890123456789012345678901234567890",
                    "GLYH",
                    "0000000000000000002",
                    "C",
                    "2011/01/14 09:39:00 AM",
                    "<unused>unused2</unused>",
                    "NULL",
                    "NULL");

                LabDEAddOrUpdateOrder(_famDB, dataDictionary);

                CheckResults(_famDB, dataDictionary.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _famDB = null;
                _testDbManager.RemoveDatabase(testDBName);
            }
        }


        /// <summary>
        /// Test the LabDEAddOrUpdateOrderWithAccession stored procedure
        /// </summary>
        [Test, Category("Automated")]
        public void LabDEAddOrUpdateOrderWithAccessionTest()
        {
            string testDBName = "Test_LabDEAddOrUpdateOrderWithAccession";
            IFileProcessingDB _famDB = null;

            try
            {
                _famDB = CreateTestDatabase(testDBName);

                // Set up the data to use for the test
                var dataDictionary = new Dictionary<string, Tuple<string, string>>();
                LabDEOrderValues(dataDictionary,
                    "12345678901234567890123456789012345678901234567890",
                    "GLU",
                    "0000000000000000001",
                    "A",
                    "2010/01/14 09:39:00 AM",
                    "<unused>unused</unused>",
                    "NULL",
                    "12345678901234567890123456789012345678901234567890");

                LabDEAddOrUpdateOrderWithAccession(_famDB, dataDictionary);

                string orderQuery = _LABDE_ORDER_QUERY + " WHERE OrderNumber = '12345678901234567890123456789012345678901234567890' ";

                CheckResults(_famDB, dataDictionary.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);

                // Call again to update data
                LabDEOrderValues(dataDictionary,
                    "12345678901234567890123456789012345678901234567890",
                    "GLYH",
                    "0000000000000000002",
                    "C",
                    "2011/01/14 09:39:00 AM",
                    "<unused>unused2</unused>",
                    "NULL",
                    "78901234567890123456789012345678901234567890123456");

                LabDEAddOrUpdateOrderWithAccession(_famDB, dataDictionary);

                CheckResults(_famDB, dataDictionary.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _famDB = null;
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Test the LabDEAddOrUpdateEncounterAndIPDates stored procedure
        /// </summary>
        [Test, Category("Automated")]
        public void LabDEAddOrUpdateEncounterAndIPDatesTest()
        {
            string testDBName = "Test_LabDEAddOrUpdateEncounterAndIPDates";
            string testDBNameNew = "Test_LabDEAddOrUpdateEncounterAndIPDates_NewDB";
            FileProcessingDB _famDB = null;
            IFileProcessingDB _NewFAMDB = null;

            try
            {
                _famDB = _testDbManager.GetDatabase(Demo_LabDE_DB, testDBName);  
                AddTestPatients(_famDB);

                _NewFAMDB = CreateTestDatabase(testDBNameNew);

                // Set up the data to use for the test
                var dataDictionary = new Dictionary<string, Tuple<string, string>>();
                LabDEEncounterValues(dataDictionary,
                    "11111111111111111111",
                    "0000000000000000001",
                    "2017/11/14 09:39:00 AM",
                    "TEST",
                    "TESTENC",
                    "PROVIDER",
                    "<unused>unused</unused>",
                    "2017/11/14 09:39:00 AM",
                    "NULL");

                LabDEAddOrUpdateEncounterAndIPDates(_famDB, dataDictionary);
                LabDEAddOrUpdateEncounterAndIPDates(_NewFAMDB, dataDictionary);

                string encounterQuery = _LABDE_ENCOUNTER_QUERY + " WHERE CSN = '11111111111111111111' ";

                CheckResults(_famDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);
                CheckResults(_NewFAMDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);

                // Call again to update data
                LabDEEncounterValues(dataDictionary,
                    "11111111111111111111",
                    "0000000000000000002",
                    "2017/11/14 09:39:00 AM",
                    "TEST2",
                    "TESTENC2",
                    "PROVIDER2",
                    "<unused>unused2</unused>",
                    "2017/11/14 09:39:00 AM",
                    "2017/11/15 09:39:00 PM");

                LabDEAddOrUpdateEncounterAndIPDates(_famDB, dataDictionary);

                CheckResults(_famDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);

                LabDEAddOrUpdateEncounterAndIPDates(_NewFAMDB, dataDictionary);

                CheckResults(_NewFAMDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);

                var saveData = new Dictionary<string, Tuple<string, string>>(dataDictionary);

                // Test that passing null values keeps the original dates
                dataDictionary["AdmissionDate"] = new Tuple<string, string>("@AdmissionDate", "NULL");
                dataDictionary["DischargeDate"] = new Tuple<string, string>("@DischargeDate", "NULL");

                LabDEAddOrUpdateEncounterAndIPDates(_famDB, dataDictionary);

                CheckResults(_famDB, saveData.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);


                LabDEAddOrUpdateEncounterAndIPDates(_NewFAMDB, dataDictionary);

                CheckResults(_NewFAMDB, saveData.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _famDB = null;
                _NewFAMDB?.CloseAllDBConnections();
                _NewFAMDB = null;
                _testDbManager.RemoveDatabase(testDBName);
                _testDbManager.RemoveDatabase(testDBNameNew);
            }
        }


        /// <summary>
        ///  Test the LabDEAddOrUpdateOrderWithEncounter
        /// </summary>
        [Test, Category("Automated")]
        public void LabDEAddOrUpdateOrderWithEncounterTest()
        {
            string testDBName = "Test_LabDEAddOrUpdateOrderWithEncounter";
            IFileProcessingDB _famDB = null;

            try
            {
                _famDB = CreateTestDatabase(testDBName);

                // Set up the data to use for the test
                var dataDictionary = new Dictionary<string, Tuple<string, string>>();
                LabDEEncounterValues(dataDictionary,
                    "11111111111111111111",
                    "0000000000000000001",
                    "2017/11/15 09:39:00 AM",
                    "TEST",
                    "TESTENC",
                    "PROVIDER",
                    "<unused>unused</unused>",
                    "NULL",
                    "NULL");

                LabDEOrderValues(dataDictionary,
                    "12345678901234567890123456789012345678901234567890",
                    "GLU",
                    "0000000000000000001",
                    "A",
                    "2010/01/14 09:39:00 AM",
                    dataDictionary["ADTMessage"].Item2,
                    "11111111111111111111",
                    "NULL");

                LabDEAddOrUpdateOrderWithEncounter(_famDB, dataDictionary);

                string encounterQuery = _LABDE_ENCOUNTER_QUERY + " WHERE CSN = '11111111111111111111' ";
                CheckResults(_famDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);

                string orderQuery = _LABDE_ORDER_QUERY + " WHERE OrderNumber = '12345678901234567890123456789012345678901234567890' ";
                CheckResults(_famDB, dataDictionary.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);

                // Call again to update data
                LabDEEncounterValues(dataDictionary,
                    "11111111111111111111",
                    "0000000000000000002",
                    "2017/11/14 09:39:00 AM",
                    "TEST2",
                    "TESTENC2",
                    "PROVIDER2",
                    "<unused>unused2</unused>",
                    "NULL",
                    "NULL");
                
                // Call again to update data
                LabDEOrderValues(dataDictionary,
                    "12345678901234567890123456789012345678901234567890",
                    "GLYH",
                    "0000000000000000002",
                    "C",
                    "2011/01/14 09:39:00 AM",
                    dataDictionary["ADTMessage"].Item2,
                    "11111111111111111111",
                    "NULL");

                LabDEAddOrUpdateOrderWithEncounter(_famDB, dataDictionary);

                CheckResults(_famDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);

                CheckResults(_famDB, dataDictionary.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _famDB = null;
                _testDbManager.RemoveDatabase(testDBName);
            }
        }


        /// <summary>
        /// Test the LabDEAddOrUpdateOrderWithEncounterAndIPDates
        /// </summary>
        [Test, Category("Automated")]
        public void LabDEAddOrUpdateOrderWithEncounterAndIPDatesTest()
        {
            string testDBName = "Test_LabDEAddOrUpdateOrderWithEncounterAndIPDates";
            string testDBNameNew = "Test_LabDEAddOrUpdateOrderWithEncounterAndIPDates_NewDB";
            FileProcessingDB _famDB = null;
            IFileProcessingDB _NewFAMDB = null;

            try
            {
                _famDB = _testDbManager.GetDatabase(Demo_LabDE_DB, testDBName);
                AddTestPatients(_famDB);

                _NewFAMDB = CreateTestDatabase(testDBNameNew);

                // Set up the data to use for the test
                var dataDictionary = new Dictionary<string, Tuple<string, string>>();
                LabDEEncounterValues(dataDictionary,
                    "11111111111111111111",
                    "0000000000000000001",
                    "2017/11/14 09:39:00 AM",
                    "TEST",
                    "TESTENC",
                    "PROVIDER",
                    "<unused>unused</unused>",
                    "2017/11/14 09:39:00 AM",
                    "NULL");

                LabDEOrderValues(dataDictionary,
                    "12345678901234567890123456789012345678901234567890",
                    "GLU",
                    "0000000000000000001",
                    "A",
                    "2010/01/14 09:39:00 AM",
                    dataDictionary["ADTMessage"].Item2,
                    "11111111111111111111",
                    "12345678901234567890123456789012345678901234567890");

                LabDEAddOrUpdateOrderWithEncounterAndIPDates(_famDB, dataDictionary);
                LabDEAddOrUpdateOrderWithEncounterAndIPDates(_NewFAMDB, dataDictionary);

                string encounterQuery = _LABDE_ENCOUNTER_QUERY + " WHERE CSN = '11111111111111111111' ";
                CheckResults(_famDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);
                CheckResults(_NewFAMDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);

                string orderQuery = _LABDE_ORDER_QUERY + " WHERE OrderNumber = '12345678901234567890123456789012345678901234567890' ";
                CheckResults(_famDB, dataDictionary.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);
                CheckResults(_NewFAMDB, dataDictionary.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);

                // Call again to update data
                LabDEEncounterValues(dataDictionary,
                    "11111111111111111111",
                    "0000000000000000002",
                    "2017/11/14 09:39:00 AM",
                    "TEST2",
                    "TESTENC2",
                    "PROVIDER2",
                    "<unused>unused2</unused>",
                    "2017/11/14 09:39:00 AM",
                    "2017/11/15 09:39:00 PM");

                // Call again to update data
                LabDEOrderValues(dataDictionary,
                    "12345678901234567890123456789012345678901234567890",
                    "GLYH",
                    "0000000000000000002",
                    "C",
                    "2011/01/14 09:39:00 AM",
                    dataDictionary["ADTMessage"].Item2,
                    "11111111111111111111",
                    "78901234567890123456789012345678901234567890123456");

                LabDEAddOrUpdateOrderWithEncounterAndIPDates(_famDB, dataDictionary);
                LabDEAddOrUpdateOrderWithEncounterAndIPDates(_NewFAMDB, dataDictionary);

                CheckResults(_famDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);
                CheckResults(_NewFAMDB, dataDictionary.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);

                CheckResults(_famDB, dataDictionary.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);
                CheckResults(_NewFAMDB, dataDictionary.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);

                var saveData = new Dictionary<string, Tuple<string, string>>(dataDictionary);

                // Test that passing null values keeps the original dates
                dataDictionary["AdmissionDate"] = new Tuple<string, string>("@AdmissionDate", "NULL");
                dataDictionary["DischargeDate"] = new Tuple<string, string>("@DischargeDate", "NULL");

                LabDEAddOrUpdateOrderWithEncounterAndIPDates(_famDB, dataDictionary);
                LabDEAddOrUpdateOrderWithEncounterAndIPDates(_NewFAMDB, dataDictionary);

                CheckResults(_famDB, saveData.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);
                CheckResults(_NewFAMDB, saveData.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key)), encounterQuery);

                CheckResults(_famDB, saveData.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);
                CheckResults(_NewFAMDB, saveData.Where(s => _ORDER_FIELDS.Contains(s.Key)), orderQuery);
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _famDB = null;
                _NewFAMDB?.CloseAllDBConnections();
                _NewFAMDB = null;
                _testDbManager.RemoveDatabase(testDBName);
                _testDbManager.RemoveDatabase(testDBNameNew);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test <see cref="FileProcessingDB"/> to use for testing
        /// </summary>
        /// <param name="DBName">The name of the database to create</param>
        /// <returns></returns>
        static IFileProcessingDB CreateTestDatabase(string DBName)
        {
            var fileProcessingDB = _testDbManager.GetNewDatabase(DBName);

            AddTestPatients(fileProcessingDB);

            return fileProcessingDB;
        }

        /// <summary>
        /// Adds 2 test patients to the database
        /// </summary>
        /// <param name="fileProcessingDB">Database to add patients to</param>
        private static void AddTestPatients(FileProcessingDB fileProcessingDB)
        {
            fileProcessingDB.ExecuteCommandQuery(
                "INSERT INTO [dbo].[LabDEPatient] \r\n" +
                "([MRN] \r\n" +
                ",[FirstName] \r\n" +
                ",[MiddleName] \r\n" +
                ",[LastName] \r\n" +
                ",[Suffix] \r\n" +
                ",[DOB] \r\n" +
                ",[Gender] \r\n" +
                ",[MergedInto] \r\n" +
                ",[CurrentMRN]) \r\n" +
                " VALUES \r\n" +
                "('0000000000000000001', 'John', '', 'Doe', '', '1970/01/01 10:00:00 AM', 'M', NULL, '0000000000000000001'), \r\n" +
                "('0000000000000000002', 'John', 'M', 'Doe', '', '1970/01/01 10:00:00 AM', 'M', NULL, '0000000000000000002')");
        }

        /// <summary>
        /// Compares the data returned by the query with the expected data
        /// </summary>
        /// <param name="_famDB">The <see cref="FileProcessingDB"/> to get the results from</param>
        /// <param name="expected">The expected results</param>
        /// <param name="resultsQuery">The query to retrieve the results from _famDB</param>
        static void CheckResults(IFileProcessingDB _famDB, IEnumerable<KeyValuePair<string, Tuple<string, string>>> expected, string resultsQuery)
        {
            var result = _famDB.GetResultsForQuery(resultsQuery);

            // There should be only one record
            Assert.That(result.RecordCount == 1, "There should be exactly one record.");

            var errorString = "";
            Assert.That(CompareWithExpected(result, expected, ref errorString),
                $"The data should match expected. {errorString}");

            result.Close();
            result = null;
        }

        /// <summary>
        /// Compares the results data in result with the expected data
        /// </summary>
        /// <param name="result">The <see cref="Recordset"/> that contains the results records</param>
        /// <param name="expected">The expected data</param>
        /// <param name="errorString">The string that indicates what field was not the same as the expected</param>
        /// <returns><see langword="true"/> if all results match; otherwise, <see langword="false"/>.</returns>
        static bool CompareWithExpected(Recordset result, IEnumerable<KeyValuePair<string, Tuple<string, string>>> expected, ref string errorString)
        {
            // List to identify date types
            List<DataTypeEnum> dateTypes = new List<DataTypeEnum>
                { DataTypeEnum.adDate, DataTypeEnum.adDBDate, DataTypeEnum.adDBTime, DataTypeEnum.adDBTimeStamp };

            // Iterate through the expected
            foreach (var v in expected)
            {
                // Get the result
                var f = result.Fields[v.Key];

                // Check if the result is a null value
                if (f.Value == DBNull.Value)
                {
                    // Check if the result should be null
                    if (v.Value.Item2 == "NULL")
                    {
                        continue;
                    }
                    else
                    {
                        errorString = $"{v.Key} did not match";
                        return false;
                    }
                }

                // Check for a date type
                if (dateTypes.Contains(f.Type))
                {
                    // Convert to a DateTime value to compare 
                    DateTime d = (DateTime)f.Value;
                    DateTime e = DateTime.Parse(v.Value.Item2);
                    if (d != e)
                    {
                        errorString = $"{v.Key} did not match";
                        return false;
                    }
                }
                else
                {
                    if (f.Value.ToString() != v.Value.Item2)
                    {
                        errorString = $"{v.Key} did not match";
                        return false;
                    }
                }
            }
            errorString = "";
            return true;
        }

        /// <summary>
        /// Puts the data in a dictionary with the correct field names and parameter names for LabDEEncounter table values
        /// </summary>
        /// <param name="dict">The dictionary to contain the data"/></param>
        /// <param name="CSN"></param>
        /// <param name="PatientMRN"></param>
        /// <param name="EncounterDateTime"></param>
        /// <param name="Department"></param>
        /// <param name="EncounterType"></param>
        /// <param name="EncounterProvider"></param>
        /// <param name="ADTMessage"></param>
        /// <param name="AdmissionDate"></param>
        /// <param name="DischargeDate"></param>
        /// <returns>The dictionary with the field data</returns>
        static Dictionary<string, Tuple<string, string>> LabDEEncounterValues(Dictionary<string, Tuple<string, string>> dict,
            string CSN, string PatientMRN, string EncounterDateTime, string Department, string EncounterType,
            string EncounterProvider, string ADTMessage, string AdmissionDate, string DischargeDate)
        {
            dict["CSN"] = new Tuple<string, string>("@EncounterID", CSN);
            dict["PatientMRN"] = new Tuple<string, string>("@PatientMRN", PatientMRN);
            dict["EncounterDateTime"] = new Tuple<string, string>("@EncounterDateTime", EncounterDateTime);
            dict["Department"] = new Tuple<string, string>("@Department", Department);
            dict["EncounterType"] = new Tuple<string, string>("@EncounterType", EncounterType);
            dict["EncounterProvider"] = new Tuple<string, string>("@EncounterProvider", EncounterProvider);
            dict["ADTMessage"] = new Tuple<string, string>("@ADTMessage", ADTMessage);
            dict["AdmissionDate"] = new Tuple<string, string>("@AdmissionDate", AdmissionDate);
            dict["DischargeDate"] = new Tuple<string, string>("@DischargeDate", DischargeDate);

            return dict;
        }

        /// <summary>
        /// Puts the data in a dictionary with the correct field names and parameter names for LabDEOrder table values
        /// </summary>
        /// <param name="dict">he dictionary to contain the data"/></param>
        /// <param name="OrderNumber"></param>
        /// <param name="OrderCode"></param>
        /// <param name="PatientMRN"></param>
        /// <param name="OrderStatus"></param>
        /// <param name="ReferenctDateTime"></param>
        /// <param name="ORMMessage"></param>
        /// <param name="EncounterID"></param>
        /// <param name="AccessionNumber"></param>
        /// <returns>The dictionary with the field data</returns>
        static Dictionary<string, Tuple<string, string>> LabDEOrderValues(Dictionary<string, Tuple<string, string>> dict, 
            string OrderNumber, 
            string OrderCode, 
            string PatientMRN, 
            string OrderStatus, 
            string ReferenctDateTime, 
            string ORMMessage, 
            string EncounterID, 
            string AccessionNumber)
        {
            dict["OrderNumber"] = new Tuple<string, string>("@OrderNumber", OrderNumber);
            dict["OrderCode"] = new Tuple<string, string>("@OrderCode", OrderCode);
            dict["PatientMRN"] = new Tuple<string, string>("@PatientMRN", PatientMRN);
            dict["OrderStatus"] = new Tuple<string, string>("@OrderStatus", OrderStatus);
            dict["ReferenceDateTime"] = new Tuple<string, string>("@ReferenceDateTime", ReferenctDateTime);
            dict["ORMMessage"] = new Tuple<string, string>("@ORMMessage", ORMMessage);
            dict["EncounterID"] = new Tuple<string, string>("@EncounterID", EncounterID);
            dict["AccessionNumber"] = new Tuple<string, string>("@AccessionNumber", AccessionNumber);

            return dict;
        }

        /// <summary>
        /// Executes the LabDEAddOrUpdateEncounter stored procedure on the given database with the dataValues
        /// </summary>
        /// <param name="dB">The <see cref="FileProcessingDB"/> to execute the stored procedure on</param>
        /// <param name="dataValues">The parameter values to use for the stored procedure call</param>
        static void LabDEAddOrUpdateEncounter(IFileProcessingDB dB, Dictionary<string, Tuple<string, string>> dataValues)
        {
            List<string> fieldsToExclude = new List<string> { "AdmissionDate", "DischargeDate" };
            List<string> fieldsToInclude = _ENCOUNTER_FIELDS.Except(fieldsToExclude).ToList();

            var data = dataValues.Where(s => fieldsToInclude.Contains(s.Key));

            dB.ExecuteCommandQuery(BuildProcCall("[dbo].[LabDEAddOrUpdateEncounter]", data));
        }

        /// <summary>
        /// Executes the LabDEAddOrUpdateEncounterAndIPDates stored procedure on the given database with the dataValues
        /// </summary>
        /// <param name="dB">The <see cref="FileProcessingDB"/> to execute the stored procedure on</param>
        /// <param name="dataValues">The parameter values to use for the stored procedure call</param>
        static void LabDEAddOrUpdateEncounterAndIPDates(IFileProcessingDB dB, Dictionary<string, Tuple<string, string>> dataValues)
        {
            var data = dataValues.Where(s => _ENCOUNTER_FIELDS.Contains(s.Key));
            dB.ExecuteCommandQuery(BuildProcCall("[dbo].[LabDEAddOrUpdateEncounterAndIPDates]", data));
        }

        /// <summary>
        /// Executes the LabDEAddOrUpdateOrderWithEncounter stored procedure on the given database with the dataValues
        /// </summary>
        /// <param name="dB">The <see cref="FileProcessingDB"/> to execute the stored procedure on</param>
        /// <param name="dataValues">The parameter values to use for the stored procedure call</param>
        static void LabDEAddOrUpdateOrderWithEncounter(IFileProcessingDB dB, Dictionary<string, Tuple<string, string>> dataValues)
        {
            List<string> fieldsToExclude = new List<string> { "CSN", "AdmissionDate", "DischargeDate", "ORMMessage", "AccessionNumber" };
            List<string> fieldsToInclude = _ENCOUNTER_FIELDS.Union(_ORDER_FIELDS).Except(fieldsToExclude).ToList();

            var data = dataValues.Where(s => fieldsToInclude.Contains(s.Key));
            dB.ExecuteCommandQuery(BuildProcCall("[dbo].[LabDEAddOrUpdateOrderWithEncounter]", data));
        }

        /// <summary>
        /// Executes the LabDEAddOrUpdateOrderWithEncounterAndIPDates stored procedure on the given database with the dataValues
        /// </summary>
        /// <param name="dB">The <see cref="FileProcessingDB"/> to execute the stored procedure on</param>
        /// <param name="dataValues">The parameter values to use for the stored procedure call</param>
        static void LabDEAddOrUpdateOrderWithEncounterAndIPDates(IFileProcessingDB dB, Dictionary<string, Tuple<string, string>> dataValues)
        {
            List<string> fieldsToExclude = new List<string> { "CSN", "ORMMessage" };
            List<string> fieldsToInclude = _ENCOUNTER_FIELDS.Union(_ORDER_FIELDS).Except(fieldsToExclude).ToList();

            var data = dataValues.Where(s => fieldsToInclude.Contains(s.Key));
            dB.ExecuteCommandQuery(BuildProcCall("[dbo].[LabDEAddOrUpdateOrderWithEncounterAndIPDates]", data));
        }

        /// <summary>
        /// Executes the LabDEAddOrUpdateOrder stored procedure on the given database with the dataValues
        /// </summary>
        /// <param name="dB">The <see cref="FileProcessingDB"/> to execute the stored procedure on</param>
        /// <param name="dataValues">The parameter values to use for the stored procedure call</param>
        static void LabDEAddOrUpdateOrder(IFileProcessingDB dB, Dictionary<string, Tuple<string, string>> dataValues)
        {
            List<string> fieldsToExclude = new List<string> { "EncounterID", "AccessionNumber" };
            List<string> fieldsToInclude = _ORDER_FIELDS.Except(fieldsToExclude).ToList();

            var data = dataValues.Where(s => fieldsToInclude.Contains(s.Key));
            dB.ExecuteCommandQuery(BuildProcCall("[dbo].[LabDEAddOrUpdateOrder]", data));
        }

        /// <summary>
        /// Executes the LabDEAddOrUpdateOrderWithAccession stored procedure on the given database with the dataValues
        /// </summary>
        /// <param name="dB">The <see cref="FileProcessingDB"/> to execute the stored procedure on</param>
        /// <param name="dataValues">The parameter values to use for the stored procedure call</param>
        static void LabDEAddOrUpdateOrderWithAccession(IFileProcessingDB dB, Dictionary<string, Tuple<string, string>> dataValues)
        {
            List<string> fieldsToExclude = new List<string> { "EncounterID" };
            List<string> fieldsToInclude = _ORDER_FIELDS.Except(fieldsToExclude).ToList();

            var data = dataValues.Where(s => fieldsToInclude.Contains(s.Key));
            dB.ExecuteCommandQuery(BuildProcCall("[dbo].[LabDEAddOrUpdateOrderWithAccession]", data));
        }

        /// <summary>
        /// Builds the sql query to call the stored procedures
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        static string BuildProcCall(string procName, IEnumerable<KeyValuePair<string, Tuple<string, string>>> parameters)
        {
            string paramList = string.Join(", ",
                parameters.Aggregate("", (str, p) =>
                    str + p.Value.Item1
                        + " = "
                        + ((p.Value.Item2 == "NULL") ? p.Value.Item2 + ", " : "'" + p.Value.Item2 + "', ")));
            return string.Format("{0} {1}", procName, paramList).TrimEnd(" ,".ToCharArray());
        } 

        #endregion
    }

}
