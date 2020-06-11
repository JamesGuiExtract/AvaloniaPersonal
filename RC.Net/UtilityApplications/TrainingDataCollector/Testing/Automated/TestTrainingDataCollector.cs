using AttributeDbMgrComponentsLib;
using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileProcessors;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.UtilityApplications.NERAnnotation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.MachineLearning.Test
{
    /// <summary>
    /// Unit tests for TrainingDataCollector class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("TrainingDataCollector")]
    public class TestTrainingDataCollector
    {
        #region Fields

        /// <summary>
        /// Manages the test data files
        /// </summary>
        static TestFileManager<TestTrainingDataCollector> _testFiles;
        static List<string> _inputFolder = new List<string>();

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestTrainingDataCollector> _testDbManager;

        static readonly string _DB_NAME = "_TestTrainingDataCollector_2DB1BD2B-2352-4F4D-AA62-AB215603B1C3";
        static readonly string _ATTRIBUTE_SET_NAME = "Expected";
        static readonly string _STORE_ATTRIBUTE_GUID = typeof(StoreAttributesInDBTask).GUID.ToString();
        static readonly string _MODEL_NAME = "Test";

        static readonly string _GET_MLDATA =
            @"SELECT Data FROM MLData
            JOIN MLModel ON MLData.MLModelID = MLModel.ID
                WHERE Name = @Name
                AND IsTrainingData = @IsTrainingData
                AND CanBeDeleted = 'False'";

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]

        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestTrainingDataCollector>();
            _testDbManager = new FAMTestDBManager<TestTrainingDataCollector>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            // Dispose of the test image manager
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }

            // The first temp folder exists after it has been deleted (until I close nunit) so to
            // safe, remove them from the list so as not to attempt to delete them more than once if you run
            // test twice.
            for (int i = _inputFolder.Count; i > 0;)
            {
                Directory.Delete(_inputFolder[--i], true);
                _inputFolder.RemoveAt(i);
            }

            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
        }

        #endregion Overhead

        #region Helper Methods

        /// <summary>
        /// Put resource test files into a DB. These images are from Demo_FlexIndex
        /// </summary>
        /// <returns>The name of the created database</returns>
        public static string CreateDatabase([CallerMemberName] string dbSuffix = "")
        {
            string dbName = _DB_NAME + dbSuffix;
            var fileProcessingDB = _testDbManager.GetNewDatabase(dbName);

            try
            {
                // Create DB
                fileProcessingDB.DefineNewAction("a");
                fileProcessingDB.DefineNewMLModel(_MODEL_NAME);
                var attributeDBMgr = new AttributeDBMgr
                {
                    FAMDB = fileProcessingDB
                };
                attributeDBMgr.CreateNewAttributeSetName(_ATTRIBUTE_SET_NAME);
                var afutility = new UCLID_AFUTILSLib.AFUtility();
                fileProcessingDB.RecordFAMSessionStart("DUMMY", "a", true, true);

                // Populate DB
                _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                Directory.CreateDirectory(_inputFolder.Last());

                var tokenFile = Path.Combine(_inputFolder.Last(), "en-token.nlp.etf");
                _testFiles.GetFile("Resources.en-token.nlp.etf", tokenFile);
                var sentenceFile = Path.Combine(_inputFolder.Last(), "en-sent.nlp.etf");
                _testFiles.GetFile("Resources.en-sent.nlp.etf", sentenceFile);

                int numFiles = 10;
                for (int i = 1; i <= numFiles; i++)
                {
                    var baseResourceName = "Resources.Example{0:D2}.tif{1}";
                    var baseName = "Example{0:D2}.tif{1}";

                    string resourceName = string.Format(CultureInfo.CurrentCulture, baseResourceName, i, "");
                    string fileName = string.Format(CultureInfo.CurrentCulture, baseName, i, "");
                    string path = Path.Combine(_inputFolder.Last(), fileName);
                    _testFiles.GetFile(resourceName, path);

                    var rec = fileProcessingDB.AddFile(path, "a", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false,
                        out var _, out var _);

                    resourceName = string.Format(CultureInfo.CurrentCulture, baseResourceName, i, ".uss");
                    fileName = string.Format(CultureInfo.CurrentCulture, baseName, i, ".uss");
                    path = Path.Combine(_inputFolder.Last(), fileName);
                    _testFiles.GetFile(resourceName, path);

                    resourceName = string.Format(CultureInfo.CurrentCulture, baseResourceName, i, ".evoa");
                    fileName = string.Format(CultureInfo.CurrentCulture, baseName, i, ".evoa");
                    path = Path.Combine(_inputFolder.Last(), fileName);
                    _testFiles.GetFile(resourceName, path);

                    var voaData = afutility.GetAttributesFromFile(path);
                    int fileTaskSessionID = fileProcessingDB.StartFileTaskSession(_STORE_ATTRIBUTE_GUID, rec.FileID, rec.ActionID);
                    attributeDBMgr.CreateNewAttributeSetForFile(fileTaskSessionID, _ATTRIBUTE_SET_NAME, voaData, false, true, true,
                        closeConnection: i == numFiles);

                    fileProcessingDB.EndFileTaskSession(fileTaskSessionID, 0, 0, 0);
                }

                fileProcessingDB.RecordFAMSessionStop();
                fileProcessingDB.CloseAllDBConnections();

                // Add record for DatabaseService so that there's a valid ID
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
                {
                    DataSource = fileProcessingDB.DatabaseServer,
                    InitialCatalog = fileProcessingDB.DatabaseName,
                    IntegratedSecurity = true,
                    NetworkLibrary = "dbmssocn"
                };

                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "INSERT DatabaseService (Description, Settings) VALUES('Unit tests'' service', '')";
                        cmd.ExecuteNonQuery();
                    }
                }

                return dbName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45129");
            }
        }

        /// <summary>
        /// Modifies the date stamp of the stored data
        /// </summary>
        public static void ModifyDateAttributesStored(string dbName, DateTime date)
        {
            try
            {
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
                {
                    DataSource = "(local)",
                    InitialCatalog = dbName,
                    IntegratedSecurity = true,
                    NetworkLibrary = "dbmssocn"
                };

                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE FileTaskSession SET DateTimeStamp = @Date";
                        cmd.Parameters.AddWithValue("@Date", date.ToString("s", CultureInfo.InvariantCulture));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46490");
            }
        }

        /// <summary>
        /// Retrieves MLData
        /// </summary>
        /// <param name="trainingData">Whether to retrieve training data (if <c>true</c>) or testing data (if <c>false</c>)</param>
        private static string GetDataFromDB(string dbName, bool trainingData)
        {
            string recordSeparator = "\r\n";
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
            {
                DataSource = "(local)",
                InitialCatalog = dbName,
                IntegratedSecurity = true,
                NetworkLibrary = "dbmssocn"
            };

            string trainingOutput = null;
            using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = _GET_MLDATA;
                    cmd.Parameters.AddWithValue("@Name", _MODEL_NAME);
                    cmd.Parameters.AddWithValue("@IsTrainingData", trainingData);

                    var lines = new List<string>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lines.Add(reader.GetString(0));
                        }

                        trainingOutput = string.Join(recordSeparator, lines);
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return trainingOutput;
        }

        /// <summary>
        /// Runs the data collector process
        /// </summary>
        public static void Process(string dbName, bool learningMachine = false)
        {
            try
            {
                if (learningMachine)
                {
                    var learningMachinePath = Path.Combine(_inputFolder.Last(), "docClassifier.lm");
                    _testFiles.GetFile("Resources.docClassifier.lm", learningMachinePath);

                    var collectorSettings = Path.Combine(_inputFolder.Last(), "collectorSettings.txt");
                    _testFiles.GetFile("Resources.collectorSettings.txt", collectorSettings);
                    var collector = TrainingDataCollector.FromJson(File.ReadAllText(collectorSettings));
                    collector.DataGeneratorPath = learningMachinePath;
                    collector.ModelType = ModelType.LearningMachine;
                    collector.DatabaseServer = "(local)";
                    collector.DatabaseName = dbName;
                    collector.UseRandomSeedFromDataGenerator = true;

                    collector.Process(System.Threading.CancellationToken.None);
                }
                else
                {
                    var annotatorSettingsPath = Path.Combine(_inputFolder.Last(), "opennlp.annotator");
                    _testFiles.GetFile("Resources.opennlp.annotator", annotatorSettingsPath);

                    var collectorSettings = Path.Combine(_inputFolder.Last(), "collectorSettings.txt");
                    _testFiles.GetFile("Resources.collectorSettings.txt", collectorSettings);
                    var collector = TrainingDataCollector.FromJson(File.ReadAllText(collectorSettings));
                    collector.DataGeneratorPath = annotatorSettingsPath;
                    collector.DatabaseServer = "(local)";
                    collector.DatabaseName = dbName;
                    collector.UseRandomSeedFromDataGenerator = true;

                    collector.Process(System.Threading.CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45128");
            }
        }

        private static void CompareExpectedFileToFoundText(string expectedFile, string output)
        {
            var expected = File.ReadAllText(expectedFile);
            Assert.AreEqual(expected, output);
        }

        #endregion Helper Methods

        #region Tests

        // Test Database mode
        [Test, Category("TrainingDataCollector")]
        public static void AllFilesExist()
        {
            string dbName = CreateDatabase();
            try
            {
                Process(dbName);

                // Verify tags
                var expectedFile = _testFiles.GetFile("Resources.opennlp.train.txt");

                string trainingOutput = GetDataFromDB(dbName, trainingData: true);
                CompareExpectedFileToFoundText(expectedFile, trainingOutput);

                expectedFile = _testFiles.GetFile("Resources.opennlp.test.txt");
                var testingOutput = GetDataFromDB(dbName, trainingData: false);
                CompareExpectedFileToFoundText(expectedFile, testingOutput);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Image files need not exist
        [Test, Category("TrainingDataCollector")]
        public static void NoImageFilesExist()
        {
            string dbName = CreateDatabase();
            try
            {
                foreach(var fileName in Directory.GetFiles(_inputFolder.Last(), "*.tif"))
                {
                    File.Delete(fileName);
                }

                Process(dbName);

                // Verify tags
                var expectedFile = _testFiles.GetFile("Resources.opennlp.train.txt");

                string trainingOutput = GetDataFromDB(dbName, trainingData: true);
                CompareExpectedFileToFoundText(expectedFile, trainingOutput);

                expectedFile = _testFiles.GetFile("Resources.opennlp.test.txt");
                var testingOutput = GetDataFromDB(dbName, trainingData: false);
                CompareExpectedFileToFoundText(expectedFile, testingOutput);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);

                // Reset test files object to avoid complaints about deleted files
                _testFiles.Dispose();
                _testFiles = new TestFileManager<TestTrainingDataCollector>();
            }
        }

        // Missing USS files = no data
        [Test, Category("TrainingDataCollector")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "USS")]
        public static void NoUSSFilesExist()
        {
            string dbName = CreateDatabase();
            try
            {
                foreach(var fileName in Directory.GetFiles(_inputFolder.Last(), "*.uss"))
                {
                    File.Delete(fileName);
                }

                Process(dbName);

                // Verify empty data
                var expected = "";

                string trainingOutput = GetDataFromDB(dbName, trainingData: true);
                Assert.AreEqual(expected, trainingOutput);

                var testingOutput = GetDataFromDB(dbName, trainingData: false);
                Assert.AreEqual(expected, testingOutput);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
                // Reset test files object to avoid complaints about deleted files
                _testFiles.Dispose();
                _testFiles = new TestFileManager<TestTrainingDataCollector>();
            }
        }

        // Missing USS files = no data
        // This exercises a different code path than the previous test due to no training/testing set division
        [Test, Category("TrainingDataCollector")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "USS")]
        public static void NoUSSFilesExist2()
        {
            string dbName = CreateDatabase();
            try
            {
                foreach(var fileName in Directory.GetFiles(_inputFolder.Last(), "*.uss"))
                {
                    File.Delete(fileName);
                }

                var annotatorSettingsPath = Path.Combine(_inputFolder.Last(), "opennlp.annotator");
                _testFiles.GetFile("Resources.opennlp.annotator", annotatorSettingsPath);

                // Update settings to change code-path used to handle missing uss file
                var annotatorSettings = NERAnnotatorSettings.LoadFrom(annotatorSettingsPath);
                annotatorSettings.PercentToUseForTestingSet = 0;
                annotatorSettings.SaveTo(annotatorSettingsPath);

                var collectorSettings = Path.Combine(_inputFolder.Last(), "collectorSettings.txt");
                _testFiles.GetFile("Resources.collectorSettings.txt", collectorSettings);
                var collector = TrainingDataCollector.FromJson(File.ReadAllText(collectorSettings));
                collector.DataGeneratorPath = annotatorSettingsPath;
                collector.DatabaseServer = "(local)";
                collector.DatabaseName = dbName;

                collector.Process(System.Threading.CancellationToken.None);

                // Verify empty data
                var expected = "";

                string trainingOutput = GetDataFromDB(dbName, trainingData: true);
                Assert.AreEqual(expected, trainingOutput);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
                // Reset test files object to avoid complaints about deleted files
                _testFiles.Dispose();
                _testFiles = new TestFileManager<TestTrainingDataCollector>();
            }
        }

        // Test with some USS files missing
        [Test, Category("TrainingDataCollector")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "USS")]
        public static void SomeUSSFilesExist()
        {
            string dbName = CreateDatabase();
            try
            {
                var files = Directory.GetFiles(_inputFolder.Last(), "*.uss");
                CollectionMethods.Shuffle(files, new Random(0));
                foreach(var fileName in files.Take(5))
                {
                    File.Delete(fileName);
                }

                Process(dbName);

                // Verify that there is some data, but less than if all files existed
                var expectedFile = _testFiles.GetFile("Resources.opennlp.train.txt");
                var expected = File.ReadAllText(expectedFile);

                string trainingOutput = GetDataFromDB(dbName, trainingData: true);
                Assert.Less(0, trainingOutput.Length);
                Assert.Greater(expected.Length, trainingOutput.Length);

                expectedFile = _testFiles.GetFile("Resources.opennlp.test.txt");
                expected = File.ReadAllText(expectedFile);
                var testingOutput = GetDataFromDB(dbName, trainingData: false);
                Assert.Less(0, testingOutput.Length);
                Assert.Greater(expected.Length, testingOutput.Length);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
                // Reset test files object to avoid complaints about deleted files
                _testFiles.Dispose();
                _testFiles = new TestFileManager<TestTrainingDataCollector>();
            }
        }

        // Test LearningMachine data collection
        [Test, Category("TrainingDataCollector")]
        public static void LearningMachineDataCollection()
        {
            string dbName = CreateDatabase();
            try
            {
                Process(dbName, learningMachine: true);

                var expectedFile = _testFiles.GetFile("Resources.learningMachine.train.txt");
                string trainingOutput = GetDataFromDB(dbName, trainingData: true);
                CompareExpectedFileToFoundText(expectedFile, trainingOutput);

                expectedFile = _testFiles.GetFile("Resources.learningMachine.test.txt");
                string testingOutput = GetDataFromDB(dbName, trainingData: false);
                CompareExpectedFileToFoundText(expectedFile, testingOutput);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test loading/saving from JSON
        [Test, Category("TrainingDataCollector")]
        public static void LoadingSavingFromJson()
        {
            var collectorSettings = _testFiles.GetFile("Resources.collectorSettings.txt");
            var collector = TrainingDataCollector.FromJson(File.ReadAllText(collectorSettings));

            var jsonSettings = collector.ToJson();
            collector.DataGeneratorPath = "DUMMY_PATH";
            var updatedJsonSettings = collector.ToJson();

            Assert.AreNotEqual(jsonSettings, updatedJsonSettings);

            var updatedCollector = (TrainingDataCollector)JsonConvert.DeserializeObject(updatedJsonSettings,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });

            Assert.AreEqual("DUMMY_PATH", updatedCollector.DataGeneratorPath);

            // Verify that other, persisted settings are the same
            Assert.AreEqual(collector.DataGeneratorPath, updatedCollector.DataGeneratorPath);
            Assert.AreEqual(collector.AttributeSetName, updatedCollector.AttributeSetName);
            Assert.AreEqual(collector.Description, updatedCollector.Description);
            Assert.AreEqual(collector.LastIDProcessed, updatedCollector.LastIDProcessed);
            Assert.AreEqual(collector.ModelName, updatedCollector.ModelName);
        }

        // Tests MarkAllDataForDeletion() behavior and
        // tests that attributes stored more than 30 days ago are ignored by default
        [Test, Category("TrainingDataCollector")]
        public static void SkipOldFiles()
        {
            string dbName = CreateDatabase();
            try
            {
                Process(dbName);

                // Verify that there is data
                string trainingOutput = GetDataFromDB(dbName, trainingData: true);
                Assert.Greater(trainingOutput.Length, 0);

                var testingOutput = GetDataFromDB(dbName, trainingData: false);
                Assert.Greater(testingOutput.Length, 0);

                var collector = new TrainingDataCollector
                {
                    DatabaseName = dbName,
                    DatabaseServer = "(local)",
                    ModelName = _MODEL_NAME
                };

                // Mark all the data for this model for deletion
                collector.MarkAllDataForDeletion();

                // Verify that there is no data
                trainingOutput = GetDataFromDB(dbName, trainingData: true);
                Assert.AreEqual(0, trainingOutput.Length);

                testingOutput = GetDataFromDB(dbName, trainingData: false);
                Assert.AreEqual(0, testingOutput.Length);

                // Change date data stored so that it will be ignored
                ModifyDateAttributesStored(dbName, DateTime.Now.Subtract(TimeSpan.FromDays(30)));

                Process(dbName);

                // Verify that there is no data
                trainingOutput = GetDataFromDB(dbName, trainingData: true);
                Assert.AreEqual(0, trainingOutput.Length);

                testingOutput = GetDataFromDB(dbName, trainingData: false);
                Assert.AreEqual(0, testingOutput.Length);

                // Change date stored so that it will not be ignored
                ModifyDateAttributesStored(dbName, DateTime.Now.Subtract(TimeSpan.FromDays(29)));

                Process(dbName);

                // Verify that there is data
                trainingOutput = GetDataFromDB(dbName, trainingData: true);
                Assert.Greater(trainingOutput.Length, 0);

                testingOutput = GetDataFromDB(dbName, trainingData: false);
                Assert.Greater(testingOutput.Length, 0);

                // Confirm that marking data for deletion doesn't affect the data if the model name is different
                collector.ModelName = _MODEL_NAME + "_";
                collector.MarkAllDataForDeletion();
                trainingOutput = GetDataFromDB(dbName, trainingData: true);
                Assert.Greater(trainingOutput.Length, 0);

                testingOutput = GetDataFromDB(dbName, trainingData: false);
                Assert.Greater(testingOutput.Length, 0);

            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test that the status column is used for LastIDProcessed
        // https://extract.atlassian.net/browse/ISSUE-15366
        [Test, Category("TrainingDataCollector")]
        public static void StatusColumn()
        {
            string dbName = CreateDatabase();
            try
            {
                var collector = new TrainingDataCollector
                {
                    ModelName = _MODEL_NAME,
                    LastIDProcessed = 5,
                };

                int id = collector.AddToDatabase("(local)", dbName);

                string statusJson = collector.Status.ToJson();

                // Disconnect from the DB and change values
                collector.DatabaseServiceID = 0;
                collector.LastIDProcessed = 6;

                string modifiedStatusJson = collector.Status.ToJson();
                Assert.AreNotEqual(statusJson, modifiedStatusJson);

                Assert.AreEqual(6, collector.LastIDProcessed);

                // Reconnect to the DB and confirm values are reset
                collector.DatabaseServiceID = id;
                string resetStatusJson = collector.Status.ToJson();
                Assert.AreEqual(statusJson, resetStatusJson);

                // Clear the status column
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
                {
                    DataSource = "(local)",
                    InitialCatalog = dbName,
                    IntegratedSecurity = true,
                    NetworkLibrary = "dbmssocn"
                };

                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE DatabaseService SET Status = NULL";
                        cmd.ExecuteNonQuery();
                    }
                }

                // Refresh the status and confirm values are defaults
                collector.RefreshStatus();
                Assert.AreEqual(0, collector.LastIDProcessed);
                string clearedStatusJson = collector.Status.ToJson();
                string defaultStatusJson = new TrainingDataCollector.TrainingDataCollectorStatus().ToJson();
                Assert.AreEqual(defaultStatusJson, clearedStatusJson);

                // Set back to original values
                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE DatabaseService SET Status = @Status";
                        cmd.Parameters.AddWithValue("@Status", statusJson);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Refresh the status and confirm values are back to original values
                collector.RefreshStatus();
                Assert.AreEqual(5, collector.LastIDProcessed);
                string resetToOriginalStatusJson = collector.Status.ToJson();
                Assert.AreEqual(statusJson, resetToOriginalStatusJson);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        #endregion Tests
    }
}