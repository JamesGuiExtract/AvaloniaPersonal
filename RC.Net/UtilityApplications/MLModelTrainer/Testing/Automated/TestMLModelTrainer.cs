using AttributeDbMgrComponentsLib;
using Extract.AttributeFinder;
using Extract.AttributeFinder.Rules;
using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileProcessors;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using LearningMachineTrainer;
using Newtonsoft.Json;
using NUnit.Framework;
using opennlp.tools.namefind;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.UtilityApplications.MachineLearning.Test
{
    /// <summary>
    /// Unit tests for MLModelTrainer class
    /// </summary>
    [TestFixture]
    public class TestMLModelTrainer
    {
        #region Fields

        /// <summary>
        /// Manages the test data files
        /// </summary>
        static TestFileManager<TestMLModelTrainer> _testFiles;
        static List<string> _inputFolder = new List<string>();

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestMLModelTrainer> _testDbManager;

        static readonly string _DB_NAME = "_TestMLModelTrainer_14394B59-A748-4418-B11A-A5682E3C5A5B";
        static readonly string _MODEL_NAME = "Test";
        static readonly string _ATTRIBUTE_SET_NAME = "Expected";
        static readonly string _STORE_ATTRIBUTE_GUID = typeof(StoreAttributesInDBTask).GUID.ToString();

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]

        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestMLModelTrainer>();
            _testDbManager = new FAMTestDBManager<TestMLModelTrainer>();
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

        #region Tests

        // Test that the training process runs without error
        [Test, Category("MLModelTrainer")]
        public static void DummyTrainingCommand()
        {
            string dbName = CreateDatabase();
            try
            {

                var trainingExe = Path.Combine(_inputFolder.Last(), "train.bat");
                _testFiles.GetFile("Resources.train.bat", trainingExe);
                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new MLModelTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\"",
                        TestingCommand = null,
                        MaximumTrainingRecords = 10000,
                        DatabaseServer = "(local)",
                        DatabaseName = dbName
                    };

                    trainer.Process(CancellationToken.None);

                    var expected = "Training\r\n";
                    var trainingOutput = File.ReadAllText(dest.FileName);
                    Assert.AreEqual(expected, trainingOutput);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test that the testing process runs without error
        [Test, Category("MLModelTrainer")]
        public static void DummyTestingCommand()
        {
            string dbName = CreateDatabase();
            try
            {
                var testingExe = Path.Combine(_inputFolder.Last(), "test1.bat");
                _testFiles.GetFile("Resources.test1.bat", testingExe);
                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new MLModelTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        TrainingCommand = null,
                        TestingCommand = testingExe.Quote() + " \"<TempModelPath>\"",
                        MaximumTestingRecords = 10000,
                        DatabaseServer = "(local)",
                        DatabaseName = dbName
                    };

                    trainer.Process(CancellationToken.None);

                    var expected = "Testing\r\n";
                    var testingOutput = File.ReadAllText(dest.FileName);
                    Assert.AreEqual(expected, testingOutput);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test that both training and testing processes run without error
        [Test, Category("MLModelTrainer")]
        public static void NERTrainingAndTesting()
        {
            var dataFile = _testFiles.GetFile("Resources.ComponentTrainingTestingData.txt");
            var data = File.ReadAllLines(dataFile);
            string dbName = CreateDatabase(data);
            try
            {

                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new MLModelTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        MaximumTrainingRecords = 10000,
                        MaximumTestingRecords = 10000,
                        DatabaseServer = "(local)",
                        DatabaseName = dbName,
                        MinimumF1Score = 0.5,
                        LastF1Score = 0
                    };

                    trainer.Process(CancellationToken.None);

                    using (var zipArchive = ZipFile.Open(dest.FileName, ZipArchiveMode.Read))
                    {
                        var model = zipArchive.GetEntry("nameFinder.model");
                        Assert.NotNull(model);
                    }
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Container class to simulate how this will work with ETL (or from command line)
        class FakeContainer : ETL.ITrainingCoordinator
        {
            public string ProjectName => "";
            public string RootDir { get; set; }
            public int NumberOfBackupModelsToKeep { get; set; }
        }
        // Test that relative paths can be used for feature gen xml and params file
        [Test, Category("MLModelTrainer")]
        public static void AllowRelativePaths()
        {
            var dataFile = _testFiles.GetFile("Resources.ComponentTrainingTestingData.txt");
            var data = File.ReadAllLines(dataFile);
            string dbName = CreateDatabase(data);

            try
            {
                var featureGenFile = "myFeatureGen.xml";
                _testFiles.GetFile("Resources.myFeatureGen.xml", Path.Combine(_inputFolder.Last(), featureGenFile));
                // Confirm this file doesn't exist in current dir
                Assert.False(File.Exists(featureGenFile));

                var paramsFile = "myParams.txt";
                _testFiles.GetFile("Resources.myParams.txt", Path.Combine(_inputFolder.Last(), paramsFile));
                // Confirm this file doesn't exist in current dir
                Assert.False(File.Exists(paramsFile));

                var exe = @"<CommonComponentsDir>\opennlp.ikvm.exe";
                var modelTag = @"<TempModelPath>";
                var dataFileTag = @"<DataFile>";
                var trainingCommand = UtilityMethods.FormatInvariant(
                    $"\"{exe}\" TokenNameFinderTrainer -model \"{modelTag}\"",
                    $" -lang en -data \"{dataFileTag}\" -resources . -featuregen {featureGenFile}",
                    $" -params {paramsFile}");

                // This dir will be made the current dir when the training command is run
                var container = new FakeContainer { RootDir = _inputFolder.Last() };

                // This is not the current dir now
                var currentDir = Directory.GetCurrentDirectory();
                Assert.AreNotEqual(container.RootDir, currentDir);

                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new MLModelTrainer
                    {
                        Container = container,
                        ModelType = ModelType.NamedEntityRecognition,
                        TrainingCommand = trainingCommand,
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        MaximumTrainingRecords = 10000,
                        MaximumTestingRecords = 10000,
                        DatabaseServer = "(local)",
                        DatabaseName = dbName,
                        MinimumF1Score = 0.5,
                        LastF1Score = 0
                    };

                    trainer.Process(CancellationToken.None);

                    using (var zipArchive = ZipFile.Open(dest.FileName, ZipArchiveMode.Read))
                    {
                        var model = zipArchive.GetEntry("nameFinder.model");
                        Assert.NotNull(model);
                    }

                    // The directory has been changed back to prev dir
                    Assert.AreEqual(currentDir, Directory.GetCurrentDirectory());
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test that it is possible, if a little convoluted, to pass in a path other than '.' for the resources dir
        // It is necessary to give a resources dir for some types of features that depend on other files.
        // When a resources dir is specified, then the featuregen xml file and its dependencies will be resolved based on that dir.
        // OpenNLP requires this path to be quoted and end in a backslash and since this command line gets split and rebuilt it is necessary
        // to escape the quotes, e.g., \".\res\\"
        [Test, Category("MLModelTrainer")]
        public static void AllowRelativePaths2()
        {
            var dataFile = _testFiles.GetFile("Resources.ComponentTrainingTestingData.txt");
            var data = File.ReadAllLines(dataFile);
            string dbName = CreateDatabase(data);

            try
            {
                var featureGenFile = "myFeatureGen.xml";
                var resourceDir = Path.Combine(_inputFolder.Last(), "res");
                Directory.CreateDirectory(resourceDir);
                _testFiles.GetFile("Resources.myFeatureGen.xml", Path.Combine(resourceDir, featureGenFile));
                // Confirm this file doesn't exist in current dir
                Assert.False(File.Exists(featureGenFile));
                Assert.False(File.Exists("res\\" + featureGenFile));

                var paramsFile = "myParams.txt";
                _testFiles.GetFile("Resources.myParams.txt", Path.Combine(_inputFolder.Last(), paramsFile));
                // Confirm this file doesn't exist in current dir
                Assert.False(File.Exists(paramsFile));

                var exe = @"<CommonComponentsDir>\opennlp.ikvm.exe";
                var modelTag = @"<TempModelPath>";
                var dataFileTag = @"<DataFile>";
                var trainingCommand = UtilityMethods.FormatInvariant(
                    $@"""{exe}"" TokenNameFinderTrainer -model ""{modelTag}""",
                    $@" -lang en -data ""{dataFileTag}"" -resources \"".\res\\"" -featuregen {featureGenFile}",
                    $@" -params {paramsFile}");

                // This dir will be made the current dir when the training command is run
                var container = new FakeContainer { RootDir = _inputFolder.Last() };

                // This is not the current dir now
                var currentDir = Directory.GetCurrentDirectory();
                Assert.AreNotEqual(container.RootDir, currentDir);

                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new MLModelTrainer
                    {
                        Container = container,
                        ModelType = ModelType.NamedEntityRecognition,
                        TrainingCommand = trainingCommand,
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        MaximumTrainingRecords = 10000,
                        MaximumTestingRecords = 10000,
                        DatabaseServer = "(local)",
                        DatabaseName = dbName,
                        MinimumF1Score = 0.5,
                        LastF1Score = 0
                    };

                    trainer.Process(CancellationToken.None);

                    using (var zipArchive = ZipFile.Open(dest.FileName, ZipArchiveMode.Read))
                    {
                        var model = zipArchive.GetEntry("nameFinder.model");
                        Assert.NotNull(model);
                    }

                    // The directory has been changed back to prev dir
                    Assert.AreEqual(currentDir, Directory.GetCurrentDirectory());
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test that the status column is used for LastIDProcessed, MaximumTrainingDocuments, etc
        // https://extract.atlassian.net/browse/ISSUE-15366
        [Test, Category("MLModelTrainer")]
        public static void StatusColumn()
        {
            string dbName = CreateDatabase();
            try
            {
                var trainer = new MLModelTrainer
                {
                    ModelName = _MODEL_NAME,
                    MaximumTrainingRecords = 1,
                    MaximumTestingRecords = 2,
                    MinimumF1Score = 0.3,
                    LastF1Score = 0.4,
                    LastIDProcessed = 5
                };
                string serverName = "(local)";
                int id = trainer.AddToDatabase(serverName, dbName);

                string statusJson = trainer.Status.ToJson();

                // Disconnect from the DB and change values
                trainer.DatabaseServiceID = 0;
                trainer.MaximumTrainingRecords = 10;
                trainer.MaximumTestingRecords = 20;
                trainer.MinimumF1Score = 0.4;
                trainer.LastF1Score = 0.5;
                trainer.LastIDProcessed = 6;

                string modifiedStatusJson = trainer.Status.ToJson();
                Assert.AreNotEqual(statusJson, modifiedStatusJson);

                Assert.AreEqual(10, trainer.MaximumTrainingRecords);
                Assert.AreEqual(20, trainer.MaximumTestingRecords);
                Assert.AreEqual(0.4, trainer.MinimumF1Score);
                Assert.AreEqual(0.5, trainer.LastF1Score);
                Assert.AreEqual(6, trainer.LastIDProcessed);

                // Reconnect to the DB and confirm values are reset
                trainer.DatabaseServiceID = id;
                string resetStatusJson = trainer.Status.ToJson();
                Assert.AreEqual(statusJson, resetStatusJson);

                using var connection = new ExtractRoleConnection(serverName, dbName);
                connection.Open();
                using var cmd = connection.CreateCommand();
                     
                cmd.CommandText = "UPDATE DatabaseService SET Status = NULL";
                cmd.ExecuteNonQuery();

                // Refresh the status and confirm values are defaults
                trainer.RefreshStatus();
                Assert.AreEqual(0, trainer.LastIDProcessed);
                string clearedStatusJson = trainer.Status.ToJson();
                string defaultStatusJson = new MLModelTrainer.MLModelTrainerStatus().ToJson();
                Assert.AreEqual(defaultStatusJson, clearedStatusJson);

                // Set back to original values
                using var restoreStatusCmd = connection.CreateCommand();
                restoreStatusCmd.CommandText = "UPDATE DatabaseService SET Status = @Status";
                restoreStatusCmd.Parameters.AddWithValue("@Status", statusJson);
                restoreStatusCmd.ExecuteNonQuery();

                // Refresh the status and confirm values are back to original values
                trainer.RefreshStatus();
                Assert.AreEqual(5, trainer.LastIDProcessed);
                string resetToOriginalStatusJson = trainer.Status.ToJson();
                Assert.AreEqual(statusJson, resetToOriginalStatusJson);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test exit code handling
        [Test, Category("MLModelTrainer")]
        public static void FailedTrainingCommand()
        {
            string dbName = CreateDatabase();
            try
            {
                var trainingExe = Path.Combine(_inputFolder.Last(), "train.bad.bat");
                _testFiles.GetFile("Resources.train.bad.bat", trainingExe);
                var dest = FileSystemMethods.GetTemporaryFileName();
                File.Delete(dest);
                var trainer = new MLModelTrainer
                {
                    ModelName = _MODEL_NAME,
                    ModelDestination = dest,
                    TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\"",
                    TestingCommand = null,
                    MaximumTrainingRecords = 10000,
                    DatabaseServer = "(local)",
                    DatabaseName = dbName
                };

                var ex = Assert.Throws<ExtractException>(() => trainer.Process(CancellationToken.None));
                Assert.AreEqual("Training failed", ex.Message);
                Assert.False(File.Exists(dest));
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test exit code handling
        [Test, Category("MLModelTrainer")]
        public static void FailedTestingCommand()
        {
            string dbName = CreateDatabase();
            try
            {
                var testingExe = Path.Combine(_inputFolder.Last(), "test.bad.bat");
                _testFiles.GetFile("Resources.test.bad.bat", testingExe);
                var dest = FileSystemMethods.GetTemporaryFileName();
                File.Delete(dest);
                var trainer = new MLModelTrainer
                {
                    ModelName = _MODEL_NAME,
                    ModelDestination = dest,
                    TrainingCommand = null,
                    TestingCommand = testingExe.Quote() + " \"<TempModelPath>\"",
                    MaximumTestingRecords = 10000,
                    DatabaseServer = "(local)",
                    DatabaseName = dbName
                };

                var ex = Assert.Throws<ExtractException>(() => trainer.Process(CancellationToken.None));
                Assert.AreEqual("Testing failed", ex.Message);
                Assert.False(File.Exists(dest));
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Confirm that the process correctly retrieves data from the DB
        [Test, Category("MLModelTrainer")]
        public static void GetDataFromDB()
        {
            var dbName = CreateDatabaseForDataCollector(false);
            try
            {
                CreateTrainingData(dbName, false);

                _inputFolder.Add(FileSystemMethods.GetTemporaryFolderName());

                var trainingExe = Path.Combine(_inputFolder.Last(), "copy.bat");
                _testFiles.GetFile("Resources.copy.bat", trainingExe);
                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new MLModelTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        TrainingCommand = trainingExe.Quote() + "\"<DataFile>\" \"<TempModelPath>\"",
                        TestingCommand = null,
                        MaximumTrainingRecords = 10000,
                        DatabaseServer = "(local)",
                        DatabaseName = dbName,
                        MinimumF1Score = 0
                    };

                    trainer.Process(CancellationToken.None);

                    var trainingOutput = File.ReadAllText(dest.FileName);
                    Assert.AreEqual(17721, trainingOutput.Length);
                    Assert.AreEqual("Washington County , Oregon 1 000 123456 ", trainingOutput.Substring(0, 40));
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test that an encrypted output file is created
        [Test, Category("MLModelTrainer")]
        public static void EncryptedOutput()
        {
            var dataFile = _testFiles.GetFile("Resources.ComponentTrainingTestingData.txt");
            var data = File.ReadAllLines(dataFile);
            string dbName = CreateDatabase(data);

            try
            {
                using (var dest = new TemporaryFile(".etf", false))
                {
                    var trainer = new MLModelTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        MaximumTrainingRecords = 10000,
                        MaximumTestingRecords = 10000,
                        DatabaseServer = "(local)",
                        DatabaseName = dbName,
                        MinimumF1Score = 0.5,
                        LastF1Score = 0
                    };

                    trainer.Process(CancellationToken.None);

                    // Confirm that the output is not a model file (is not a valid zip file)
                    Assert.Throws(typeof(InvalidDataException), delegate
                    {
                        using (var zipArchive = ZipFile.Open(dest.FileName, ZipArchiveMode.Read)) { }
                    });

                    // Confirm that it can be loaded by the NERF (and so does contain an encrypted model)
                    var model = NERFinder.GetModel(dest.FileName, strm => new TokenNameFinderModel(strm));
                    Assert.NotNull(model);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Train a learning machine classifier
        [Test, Category("MLModelTrainer")]
        public static void TestLearningMachine()
        {
            var dbName = CreateDatabaseForDataCollector(true);
            try
            {
                CreateTrainingData(dbName, true);

                var dest = _testFiles.GetFile("Resources.docClassifier.lm");
                LearningMachine lm = LearningMachine.Load(dest);
                Assert.That(!lm.IsTrained);

                var trainer = new MLModelTrainer
                {
                    ModelType = ModelType.LearningMachine,
                    ModelName = _MODEL_NAME,
                    ModelDestination = dest,
                    MaximumTrainingRecords = 10000,
                    MaximumTestingRecords = 10000,
                    DatabaseServer = "(local)",
                    DatabaseName = dbName
                };

                trainer.Process(CancellationToken.None);

                lm = LearningMachine.Load(dest);
                Assert.That(lm.IsTrained);
            }
            finally
            {
                // Remove the modified LM
                _testFiles.RemoveFile("Resources.docClassifier.lm");
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Check to make sure the different classifier types are loadable by the trainer app
        // https://extract.atlassian.net/browse/ISSUE-15486
        [Test, Category("MLModelTrainer")]
        public static void TestLearningMachineSerialization()
        {
            var dbName = CreateDatabaseForDataCollector(true);
            try
            {
                CreateTrainingData(dbName, true);

                var dest = _testFiles.GetFile("Resources.docClassifier.lm");
                LearningMachine lm = LearningMachine.Load(dest);

                void train()
                {

                    Assert.That(!lm.IsTrained);
                    var trainer = new MLModelTrainer
                    {
                        ModelType = ModelType.LearningMachine,
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest,
                        MaximumTrainingRecords = 10000,
                        MaximumTestingRecords = 10000,
                        DatabaseServer = "(local)",
                        DatabaseName = dbName
                    };

                    trainer.Process(CancellationToken.None);

                    lm = LearningMachine.Load(dest);
                    Assert.That(lm.IsTrained);
                }

                Assert.That(lm.Classifier is IMulticlassSupportVectorMachineModel);
                train();

                lm.Classifier = new MultilabelSupportVectorMachineClassifier();
                lm.Save(dest);
                lm = LearningMachine.Load(dest);
                train();

                lm.Classifier = new NeuralNetworkClassifier();
                lm.Save(dest);
                lm = LearningMachine.Load(dest);
                train();
            }
            finally
            {
                // Remove the modified LM
                _testFiles.RemoveFile("Resources.docClassifier.lm");

                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Train a learning machine classifier and then changing
        // a document type
        [Test, Category("MLModelTrainer")]
        public static void LearningMachineRenameDocType()
        {
            var dbName = CreateDatabaseForDataCollector(true);
            try
            {
                CreateTrainingData(dbName, true);

                var dest = _testFiles.GetFile("Resources.docClassifier.lm");
                var uss = _testFiles.GetFile("Resources.Example02.tif.uss");

                LearningMachine lm = LearningMachine.Load(dest);
                Assert.That(!lm.IsTrained);

                var trainer = new MLModelTrainer
                {
                    ModelType = ModelType.LearningMachine,
                    ModelName = _MODEL_NAME,
                    ModelDestination = dest,
                    MaximumTrainingRecords = 10000,
                    MaximumTestingRecords = 10000,
                    DatabaseServer = "(local)",
                    DatabaseName = dbName
                };

                trainer.Process(CancellationToken.None);

                lm = LearningMachine.Load(dest);
                Assert.That(lm.IsTrained);

                var doc = new SpatialStringClass();
                var voa = new IUnknownVectorClass();
                doc.LoadFrom(uss, false);
                lm.ComputeAnswer(doc, voa, false);
                Assert.AreEqual("Mortgage", ((IAttribute)voa.At(0)).Value.String);

                trainer.ChangeAnswer("Mortgage", "Egagtrom", true);

                LearningMachine.ComputeAnswer(dest, doc, voa, false);
                Assert.AreEqual("Egagtrom", ((IAttribute)voa.At(0)).Value.String);

                // Verify that even if the machine isn't updated that it will be trainable with the
                // modified data
                // Overwrite the updated machine and sleep for a few ms to allow cache time to update
                lm.Save(dest);
                Thread.Sleep(100);

                // Verify old doctype is predicted
                LearningMachine.ComputeAnswer(dest, doc, voa, false);
                Assert.AreEqual("Mortgage", ((IAttribute)voa.At(0)).Value.String);

                // Retrain and confirm new doctype is predicted
                trainer.Process(CancellationToken.None);
                LearningMachine.ComputeAnswer(dest, doc, voa, false);
                Assert.AreEqual("Egagtrom", ((IAttribute)voa.At(0)).Value.String);
            }
            finally
            {
                // Remove the modified LM
                _testFiles.RemoveFile("Resources.docClassifier.lm");
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        static void SimulateTrainingAndTesting(string dbName, double lastF1Score, double minimumF1Score, double allowableAccuracyDrop, bool expectSuccess = true, bool interactive = false)
        {
            var trainingExe = Path.Combine(_inputFolder.Last(), "train.bat");
            _testFiles.GetFile("Resources.train.bat", trainingExe);
            using (var testingExe = new TemporaryFile(".bat", false))
            using (var dest = new TemporaryFile(false))
            {
                var trainer = new MLModelTrainer
                {
                    Description = "Unit Test"
                    , ModelName = _MODEL_NAME
                    , ModelDestination = dest.FileName
                    , TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\""
                    , TestingCommand = testingExe.FileName.Quote() + " \"<TempModelPath>\""
                    , MinimumF1Score = minimumF1Score
                    , LastF1Score = lastF1Score
                    , AllowableAccuracyDrop = allowableAccuracyDrop
                    , MaximumTrainingRecords = 10000
                    , MaximumTestingRecords = 10000
                    , DatabaseServer = "(local)"
                    , DatabaseName = dbName
                };

                if (interactive)
                {
                    UtilityMethods.ShowMessageBox("Configure/confirm email settings with EmailFile.exe /c", "", false);
                    UtilityMethods.ShowMessageBox("Fill in email address and subject fields on the next screen and click OK", "", false);
                    using (var form = new MLModelTrainerConfigurationDialog(trainer, "(local)", dbName))
                    {
                        Application.Run(form);
                        var result = form.DialogResult;
                        Assert.That(result == DialogResult.OK);
                    }
                }

                // Simulate a testing run...
                File.WriteAllLines(testingExe.FileName, new[] {
                    "@ECHO OFF"
                    ,"SETLOCAL"
                    ,"ECHO Loading Token Name Finder model ... done (0.301s)"
                    ,"ECHO."
                    ,"ECHO."
                    ,"ECHO Average: 392.3 sent/s"
                    ,"ECHO Total: 51 sent"
                    ,"ECHO Runtime: 0.13s"
                    ,"ECHO."
                    ,"ECHO Evaluated 50 samples with 28 entities; found: 15 entities; correct: 11."
                    ,"ECHO        TOTAL: precision:   73.33%%;  recall:   39.29%%; F1:   51.16%%."
                    ,"ECHO        Party: precision:   73.33%%;  recall:   39.29%%; F1:   51.16%%. [target:  28; tp:  11; fp:   4]"
                    ,"ECHO."
                    ,"ECHO Execution time: 0.540 seconds"
                });

                try
                {
                    trainer.Process(CancellationToken.None);
                }
                catch (Exception ex)
                when (expectSuccess == false && ex.Message.Contains("failed to produce an adequate model"))
                { }

                // Destination file will be empty if testing result was not acceptable
                var expected = expectSuccess ? "Training\r\n" : "";
                var testingOutput = File.ReadAllText(dest.FileName);

                Assert.AreEqual(expected, testingOutput);

                if (interactive)
                {
                    UtilityMethods.ShowMessageBox("Confirm that you received an email with an exception log attached", "", false);
                }
            }
        }

        // Test acceptable test result: f1 score stays the same, drops an allowable amount or increases
        // train.bat will write the word "Training" to the <TempModelPath>
        // if the testing result is deemed acceptable, this file will be copied to the destination
        [Test, Category("MLModelTrainer")]
        public static void AcceptableResult()
        {
            string dbName = CreateDatabase();
            try
            {
                // Stays the same
                SimulateTrainingAndTesting(dbName, lastF1Score: 0.5116, minimumF1Score: 0.04, allowableAccuracyDrop: 0.05);

                // Drops 0.05
                SimulateTrainingAndTesting(dbName, lastF1Score: 0.5616, minimumF1Score: 0.04, allowableAccuracyDrop: 0.05);

                // Drops to minimum
                SimulateTrainingAndTesting(dbName, lastF1Score: 0.5616, minimumF1Score: 0.5116, allowableAccuracyDrop: 0.1);

                // Increases
                SimulateTrainingAndTesting(dbName, lastF1Score: 0.5115, minimumF1Score: 0.04, allowableAccuracyDrop: 0.05);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test unacceptable result: f1 score drops more than the allowable amount or drops below the minimum allowed
        // train.bat will write the word "Training" to the <TempModelPath>
        // if the testing result is deemed acceptable, this file will be copied to the destination
        [Test, Category("MLModelTrainer")]
        public static void UnacceptableResult()
        {
            string dbName = CreateDatabase();
            try
            {
                // Drops 0.051
                SimulateTrainingAndTesting(dbName, lastF1Score: 0.5626, minimumF1Score: 0.04, allowableAccuracyDrop: 0.05, expectSuccess: false);

                // Drops below minimum
                SimulateTrainingAndTesting(dbName, lastF1Score: 0.5116, minimumF1Score: 0.5117, allowableAccuracyDrop: 0.05, expectSuccess: false);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test unacceptable result: f1 score drops more than the allowable amount or drops below the minimum allowed
        // train.bat will write the word "Training" to the <TempModelPath>
        // if the testing result is deemed acceptable, this file will be copied to the destination
        [Test, Category("Interactive")]
        public static void Interactive_UnacceptableResult()
        {
            string dbName = CreateDatabase();
            try
            {
                // Drops 0.051
                SimulateTrainingAndTesting(dbName, lastF1Score: 0.5626, minimumF1Score: 0.04, allowableAccuracyDrop: 0.05, expectSuccess: false, interactive: true);
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("MLModelTrainer")]
        public static void SimulateOutOfMemory()
        {
            string dbName = CreateDatabase();
            try
            {
                using (var trainingExe = new TemporaryFile(".bat", false))
                using (var testingExe = new TemporaryFile(".bat", false))
                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new MLModelTrainer
                    {
                        ModelName = _MODEL_NAME
                        , ModelDestination = dest.FileName
                        , TrainingCommand = trainingExe.FileName.Quote() + " \"<DataFile>\" \"<TempModelPath>\""
                        , TestingCommand = testingExe.FileName.Quote() + " \"<DataFile>\""
                        , MaximumTrainingRecords = 10000
                        , MaximumTestingRecords = 10000
                        , LastIDProcessed = 50
                        , MinimumF1Score = 0.5
                        , DatabaseServer = "(local)"
                        , DatabaseName = dbName
                    };

                    // Simulate running out of memory if number of records is greater than 30
                    var lines = new[] {
                        "@ECHO OFF"
                        ,"SETLOCAL"
                        ,"ECHO Performing 300 iterations."
                        ,"ECHO   1:  . (2708/2770) 0.9776173285198556"
                        ,@"FOR /f %%C in ('Find /V /C """" ^< %1') do set Count=%%C"
                        ,"ECHO %Count%"
                        ,"IF %Count% GTR 30 ("
                        ,@"  ECHO Exception in thread ""main"" java.lang.OutOfMemoryError: GC overhead limit exceeded 1>&2"
                        ,@"  ECHO        at java.util.Arrays.copyOf(Unknown Source^) 1>&2"
                        ,@"  EXIT /B 9999"
                        ,")\r\n"
                    };
                    File.WriteAllLines(trainingExe.FileName, lines);
                    File.AppendAllText(trainingExe.FileName, "ECHO Training> %2");

                    File.WriteAllLines(testingExe.FileName, lines);
                    File.AppendAllText(testingExe.FileName, "ECHO        TOTAL: precision:   73.33%%;  recall:   39.29%%; F1:   51.16%%.");

                    trainer.Process(CancellationToken.None);

                    // Destination file will be empty if testing result was not acceptable
                    var expected = "Training\r\n";
                    var testingOutput = File.ReadAllText(dest.FileName);

                    Assert.AreEqual(expected, testingOutput);

                    // Maximum training files will have been reduced
                    Assert.LessOrEqual(trainer.MaximumTrainingRecords, 30);

                    // Maximum testing files will have been reduced
                    Assert.LessOrEqual(trainer.MaximumTestingRecords, 30);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        // Test loading/saving from JSON
        [Test, Category("MLModelTrainer")]
        public static void LoadingSavingFromJson()
        {
            var trainerSettings = _testFiles.GetFile("Resources.MLModelTrainerSettings.txt");
            var trainer = MLModelTrainer.FromJson(File.ReadAllText(trainerSettings));

            var jsonSettings = trainer.ToJson();
            trainer.EmailSubject = "DUMMY_SUBJECT";
            var updatedJsonSettings = trainer.ToJson();

            Assert.AreNotEqual(jsonSettings, updatedJsonSettings);

            var updatedtrainer = (MLModelTrainer)JsonConvert.DeserializeObject(updatedJsonSettings,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });

            Assert.AreEqual("DUMMY_SUBJECT", updatedtrainer.EmailSubject);

            // Verify that other, persisted settings are the same
            Assert.AreEqual(trainer.AllowableAccuracyDrop, updatedtrainer.AllowableAccuracyDrop);
            Assert.AreEqual(trainer.Description, updatedtrainer.Description);
            Assert.AreEqual(trainer.EmailAddressesToNotifyOnFailure, updatedtrainer.EmailAddressesToNotifyOnFailure);
            Assert.AreEqual(trainer.LastF1Score, updatedtrainer.LastF1Score);
            Assert.AreEqual(trainer.LastIDProcessed, updatedtrainer.LastIDProcessed);
            Assert.AreEqual(trainer.MaximumTestingRecords, updatedtrainer.MaximumTestingRecords);
            Assert.AreEqual(trainer.MaximumTrainingRecords, updatedtrainer.MaximumTrainingRecords);
            Assert.AreEqual(trainer.MinimumF1Score, updatedtrainer.MinimumF1Score);
            Assert.AreEqual(trainer.ModelDestination, updatedtrainer.ModelDestination);
            Assert.AreEqual(trainer.ModelName, updatedtrainer.ModelName);
            Assert.AreEqual(trainer.TestingCommand, updatedtrainer.TestingCommand);
            Assert.AreEqual(trainer.TrainingCommand, updatedtrainer.TrainingCommand);
        }

        #endregion Tests

        #region Private Methods

        // Helper function to put resource test files into a DB
        // These images are from Demo_FlexIndex
        private static string CreateDatabase(IEnumerable<string> data = null, [CallerMemberName] string dbSuffix = "")
        {
            // Create DB
            string dbName = _DB_NAME + dbSuffix;
            var fileProcessingDB = _testDbManager.GetNewDatabase(dbName);

            try
            {
                fileProcessingDB.DefineNewAction("a");
                fileProcessingDB.DefineNewMLModel(_MODEL_NAME);
                fileProcessingDB.AddFileNoQueue("dummy", 0, 0, EFilePriority.kPriorityNormal, -1);
                fileProcessingDB.CloseAllDBConnections();

                using var connection = new ExtractRoleConnection("(local)", dbName);
                connection.Open();

                // Add record for DatabaseService so that there's a valid ID
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT DatabaseService (Description, Settings) VALUES('ML Model Trainer test', '')";
                    cmd.ExecuteNonQuery();
                }

                var rng = new Random();
                if (data == null)
                {
                    data = Enumerable.Range(0, 100).Select(i => UtilityMethods.FormatInvariant($"{i}\r\n"));
                }

                foreach (string s in data)
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO MLData(MLModelID, FileID, IsTrainingData, DateTimeStamp, Data)
                    SELECT MLModel.ID, FAMFile.ID, @IsTrainingData, GETDATE(), @Data
                    FROM MLModel, FAMFILE WHERE MLModel.Name = @ModelName AND FAMFile.FileName = @FileName";
                        cmd.Parameters.AddWithValue("@IsTrainingData", (rng.Next(2) == 0));
                        cmd.Parameters.AddWithValue("@Data", s);
                        cmd.Parameters.AddWithValue("@ModelName", _MODEL_NAME);
                        cmd.Parameters.AddWithValue("@FileName", "dummy");

                        cmd.ExecuteNonQuery();
                    }
                }

                _inputFolder.Add(FileSystemMethods.GetTemporaryFolderName());

                return dbName;
            }
            catch
            {
                _testDbManager.RemoveDatabase(dbName);
                throw;
            }
        }

        private static string CreateDatabaseForDataCollector(bool forDocClassifier, [CallerMemberName] string dbSuffix = "")
        {
            string dbName = _DB_NAME + dbSuffix;
            var fileProcessingDB = _testDbManager.GetNewDatabase(dbName);

            try
            {
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
                _inputFolder.Add(FileSystemMethods.GetTemporaryFolderName());

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

                    fileProcessingDB.EndFileTaskSession(fileTaskSessionID, 0, 0, false);
                }

                if (forDocClassifier)
                {
                    var dups = new List<string>();
                    foreach (var fileName in Directory.GetFiles(_inputFolder.Last()))
                    {
                        var newName = Path.Combine(Path.GetDirectoryName(fileName), "Copy_" + Path.GetFileName(fileName));
                        File.Copy(fileName, newName);

                        if (newName.EndsWith(".tif", StringComparison.Ordinal))
                        {
                            dups.Add(newName);
                        }
                    }
                    foreach (var (tif, i) in dups.Select((path, i) => (path, i + numFiles)))
                    {
                        var rec = fileProcessingDB.AddFile(tif, "a", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false,
                            out var _, out var _);

                        var voa = tif + ".evoa";
                        var voaData = afutility.GetAttributesFromFile(voa);
                        int fileTaskSessionID = fileProcessingDB.StartFileTaskSession(_STORE_ATTRIBUTE_GUID, rec.FileID, rec.ActionID);
                        attributeDBMgr.CreateNewAttributeSetForFile(fileTaskSessionID, _ATTRIBUTE_SET_NAME, voaData, false, true, true,
                            closeConnection: i == numFiles * 2);

                        fileProcessingDB.EndFileTaskSession(fileTaskSessionID, 0, 0, false);
                    }
                }

                fileProcessingDB.RecordFAMSessionStop();
                fileProcessingDB.CloseAllDBConnections();

                using var connection = new ExtractRoleConnection("(local)", dbName);
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT DatabaseService (Description, Settings) VALUES('Unit tests'' service', '')";
                cmd.ExecuteNonQuery();

                return dbName;
            }
            catch (Exception ex)
            {
                _testDbManager.RemoveDatabase(dbName);
                throw ex.AsExtract("ELI45129");
            }
        }

        private static void CreateTrainingData(string dbName, bool forDocClassifier)
        {
            if (forDocClassifier)
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

                collector.Process(CancellationToken.None);
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

                collector.Process(CancellationToken.None);
            }
        }

        #endregion Private Methods
    }
}
