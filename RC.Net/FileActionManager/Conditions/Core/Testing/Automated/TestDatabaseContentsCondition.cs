using Extract.Database.Sqlite;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.IO;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions.Test
{
    /// <summary>
    ///  NOTE: It was pointed out in review that a better way to structure the below tests
    ///  would be to use TestCaseData/TestCaseSource to pass appropriately configured
    ///  DatabaseContentsCondition instances rather than building up a condition based on
    ///  test parameters.
    ///  While it is probably not worth the time to refactor right now, if more work is
    ///  done on this class such a refactor should be considered.
    /// </summary>
    [Category("TestDatabaseContentsCondition")]
    [TestFixture]
    public class TestDatabaseContentsCondition
    {
        #region Constants

        const int _CURRENT_WORKFLOW = -1;
        const string _LABDE_EMPTY_DB = "Resources.Demo_LabDE_Empty";
        const string _TEST_OMDB = "Resources.OrderMappingDB.sqlite";
        const string _TEST_FILE1 = "Resources.TestImage001.tif";

        const string _TEST_DB_NAME = "Test_DatabaseContentsCondition";
        const string _SQLITE_SOURCE = "SQLite";
        const string _SQLITE_PROVIDER = "SQLite Data Provider";
        
        const string _ACTION1 = "A01_ExtractData";
        const string _ACTION1_LOWERCASE = "a01_extractdata";
        const string _ACTION1_MISSPELL = "A01_ExtractZata";
        const int _ACTION_ID1 = 1;

        #endregion Constants

        #region Fields

        static TestFileManager<TestDatabaseContentsCondition> _testFiles;
        static FAMTestDBManager<TestDatabaseContentsCondition> _testDbManager;
        static FileProcessingDB _fileProcessingDb;
        static FileRecord _fileRecord;
        static FAMTagManager _tagManager;
        static string _sqliteFileName;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestDatabaseContentsCondition>();
            _testDbManager = new FAMTestDBManager<TestDatabaseContentsCondition>();

            string testFileName = _testFiles.GetFile(_TEST_FILE1);
            _fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, _TEST_DB_NAME);

            _fileRecord = _fileProcessingDb.AddFile(
                testFileName, _ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                EActionStatus.kActionPending, true, out var _, out var _);

            _tagManager = new FAMTagManagerClass();
            _tagManager.FPSFileDir = Path.GetDirectoryName(_fileRecord.Name);
            _tagManager.FPSFileName = Path.Combine(_tagManager.FPSFileDir, "Test.fps");

            _sqliteFileName = _testFiles.GetFile(_TEST_OMDB);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
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

        [CLSCompliant(false)]
        [Test, Category("Automated")]
        [TestCase(true, true, "ActiveFAM", null, DatabaseContentsConditionRowCount.Zero, TestName = "FAMDbTableZeroRowsTrue")]
        [TestCase(false, true, "FAMFile", null, DatabaseContentsConditionRowCount.Zero, TestName = "FAMDbTableZeroRowsFalse")]
        [TestCase(true, true, "FAMFile", null, DatabaseContentsConditionRowCount.ExactlyOne, TestName = "FAMDbTableExactlyOneTrue")]
        [TestCase(false, true, "ActiveFAM", null, DatabaseContentsConditionRowCount.ExactlyOne, TestName = "FAMDbTableExactlyOneFalse")]
        [TestCase(false, true, "Action", null, DatabaseContentsConditionRowCount.ExactlyOne, TestName = "FAMDbTableExactlyOneFalse")]
        [TestCase(true, true, "Action", null, DatabaseContentsConditionRowCount.AtLeastOne, TestName = "FAMDbTableAtLeastOneTrue")]
        [TestCase(false, true, "ActiveFAM", null, DatabaseContentsConditionRowCount.AtLeastOne, TestName = "FAMDbTableAtLeastOneFalse")]
        [TestCase(true, true, null, "SELECT [ASCName] FROM [Action] WHERE [ASCName] LIKE 'A01%'", 
            DatabaseContentsConditionRowCount.ExactlyOne, TestName = "FAMDbQueryExactlyOneTrue")]
        [TestCase(false, true, null, "SELECT [ASCName] FROM [Action]",
            DatabaseContentsConditionRowCount.ExactlyOne, TestName = "FAMDbQueryExactlyOneFalse")]
        [TestCase(true, true, "FAMFile", null, DatabaseContentsConditionRowCount.ExactlyOne,
            "FileName", "<SourceDocName>", TestName = "FAMDbTableResultNameExactlyOneTrue")]
        [TestCase(true, true, null, "SELECT [ASCName] AS [Name] FROM [Action]",
            DatabaseContentsConditionRowCount.ExactlyOne, "Name", _ACTION1, TestName = "FAMDbQueryResultNameExactlyOneTrue")]
        [TestCase(false, true, null, "SELECT [ASCName] AS [Name] FROM [Action] WHERE [ASCName] LIKE 'A02%'",
            DatabaseContentsConditionRowCount.ExactlyOne, "Name", _ACTION1, TestName = "FAMDbQueryResultNameExactlyOneFalse")]
        [TestCase(true, true, "Action", null, DatabaseContentsConditionRowCount.ExactlyOne,
            "ASCName", _ACTION1_LOWERCASE, false, TestName = "FAMDbTableResultNameExactlyOneCaseSensitiveTrue")]
        [TestCase(false, true, "Action", null, DatabaseContentsConditionRowCount.ExactlyOne,
            "ASCName", _ACTION1_LOWERCASE, true, TestName = "FAMDbTableResultNameExactlyOneCaseSensitiveFalse")]
        [TestCase(true, true, "Action", null, DatabaseContentsConditionRowCount.ExactlyOne,
            "ASCName", _ACTION1_MISSPELL, false, true, TestName = "FAMDbTableResultNameExactlyOneFuzzyTrue")]
        [TestCase(false, true, "Action", null, DatabaseContentsConditionRowCount.ExactlyOne,
            "ASCName", _ACTION1_MISSPELL, false, false, TestName = "FAMDbTableResultNameExactlyOneFuzzyFalse")]
        [TestCase(true, false, "TypicalOrderHeading", null, DatabaseContentsConditionRowCount.Zero, TestName = "SQLiteTableZeroRowsTrue")]
        [TestCase(false, false, "SmartTag", null, DatabaseContentsConditionRowCount.Zero, TestName = "SQLiteTableZeroRowsFalse")]
        [TestCase(true, false, "SmartTag", null, DatabaseContentsConditionRowCount.ExactlyOne, TestName = "SQLiteTableExactlyOneTrue")]
        [TestCase(false, false, "TypicalOrderHeading", null, DatabaseContentsConditionRowCount.ExactlyOne, TestName = "SQLiteTableExactlyOneFalse")]
        [TestCase(false, false, "Gender", null, DatabaseContentsConditionRowCount.ExactlyOne, TestName = "SQLiteTableExactlyOneFalse")]
        [TestCase(true, false, "Gender", null, DatabaseContentsConditionRowCount.AtLeastOne, TestName = "SQLiteTableAtLeastOneTrue")]
        [TestCase(false, false, "TypicalOrderHeading", null, DatabaseContentsConditionRowCount.AtLeastOne, TestName = "SQLiteTableAtLeastOneFalse")]
        [TestCase(true, false, null, "SELECT [Value] FROM [Settings] WHERE [Name] = 'OrderMapperSchemaVersion'",
            DatabaseContentsConditionRowCount.ExactlyOne, TestName = "SQLiteQueryExactlyOneTrue")]
        [TestCase(false, false, null, "SELECT [Value] FROM [Settings]",
            DatabaseContentsConditionRowCount.ExactlyOne, TestName = "SQLiteQueryExactlyOneFalse")]
        [TestCase(true, false, "Settings", null, DatabaseContentsConditionRowCount.ExactlyOne,
            "Value", "2", TestName = "SQLiteTableResultNameExactlyOneTrue")]
        [TestCase(true, false, null, "SELECT [Value] FROM [Settings]",
            DatabaseContentsConditionRowCount.ExactlyOne, "Value", "2", TestName = "SQLiteQueryResultNameExactlyOneTrue")]
        [TestCase(false, false, null, "SELECT [Value] FROM [Settings]",
            DatabaseContentsConditionRowCount.ExactlyOne, "Value", "OrderMapperSchemaVersion", TestName = "SQLiteQueryResultNameExactlyOneFalse")]
        [TestCase(true, false, "Gender", null, DatabaseContentsConditionRowCount.ExactlyOne,
            "Gender", "f", false, TestName = "SQLiteTableResultNameExactlyOneCaseSensitiveTrue")]
        [TestCase(false, false, "Gender", null, DatabaseContentsConditionRowCount.ExactlyOne,
            "Gender", "f", true, TestName = "SQLiteTableResultNameExactlyOneCaseSensitiveFalse")]
        [TestCase(true, false, "SmartTag", null, DatabaseContentsConditionRowCount.ExactlyOne,
            "TagName", "coments", false, true, TestName = "SQLiteTableResultNameExactlyOneFuzzyTrue")]
        [TestCase(false, false, "SmartTag", null, DatabaseContentsConditionRowCount.ExactlyOne,
            "TagName", "coments", false, false, TestName = "SQLiteTableResultNameExactlyOneFuzzyFalse")]
        public static void TestCondition(bool expectedValue, bool useFAMDB, string table, string query, 
            DatabaseContentsConditionRowCount rowCountCondition,
            string searchField = null, string searchValue = null, bool caseSensitive = false, bool fuzzy = false)
        {
            using DatabaseContentsCondition condition = new();
            if (useFAMDB)
            {
                condition.UseFAMDBConnection = true;
            }
            else
            {
                condition.UseFAMDBConnection = false;
                condition.DataSourceName = _SQLITE_SOURCE;
                condition.DataProviderName = _SQLITE_PROVIDER;
                condition.DataConnectionString = SqliteMethods.BuildConnectionString(_sqliteFileName);
            }

            ExtractException.Assert("ELI51917", "Invalid test configuration",
                string.IsNullOrWhiteSpace(table) != string.IsNullOrWhiteSpace(query));

            if (!string.IsNullOrWhiteSpace(table))
            {
                condition.UseQuery = false;
                condition.Table = table;
            }
            else
            {
                condition.UseQuery = true;
                condition.Query = query;
            }

            if (searchField != null)
            {
                condition.CheckFields = true;

                IVariantVector fieldVector = new VariantVector();
                fieldVector.PushBack(searchField);
                fieldVector.PushBack(searchValue);
                fieldVector.PushBack(caseSensitive);
                fieldVector.PushBack(fuzzy);

                condition.SearchFields = new IUnknownVector();
                condition.SearchFields.PushBack(fieldVector);
            }
            else
            {
                condition.CheckFields = true;

            }

            condition.RowCountCondition = rowCountCondition;

            Assert.AreEqual(expectedValue,
                condition.FileMatchesFAMCondition(_fileRecord, _fileProcessingDb, _ACTION_ID1, _tagManager));

        }

        #endregion Tests
    }
}
