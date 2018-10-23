using Extract.AttributeFinder;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.UtilityApplications.TrainingDataCollector;
using Extract.UtilityApplications.TrainingDataCollector.Test;
using LearningMachineTrainer;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.UtilityApplications.MLModelTrainer.Test
{
    /// <summary>
    /// Unit tests for MLModelTrainer class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("MLModelTrainer")]
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

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]

        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestMLModelTrainer>();
            _testDbManager = new FAMTestDBManager<TestMLModelTrainer>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
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

        // Helper function to put resource test files into a DB
        // These images are from Demo_FlexIndex
        private static void CreateDatabase()
        {
            // Create DB
            var fileProcessingDB = _testDbManager.GetNewDatabase(_DB_NAME);
            fileProcessingDB.DefineNewAction("a");
            fileProcessingDB.DefineNewMLModel(_MODEL_NAME);
            fileProcessingDB.AddFileNoQueue("dummy", 0, 0, EFilePriority.kPriorityNormal, -1);
            fileProcessingDB.CloseAllDBConnections();

            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
            {
                DataSource = fileProcessingDB.DatabaseServer,
                InitialCatalog = fileProcessingDB.DatabaseName,
                IntegratedSecurity = true,
                NetworkLibrary = "dbmssocn"
            };

            var connection = new SqlConnection(sqlConnectionBuild.ConnectionString);
            connection.Open();

            // Add record for DatabaseService so that there's a valid ID
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "INSERT DatabaseService (Description, Settings) VALUES('ML Model Trainer test', '')";
                cmd.ExecuteNonQuery();
            }

            var rng = new Random();
            foreach (int i in Enumerable.Range(0, 100))
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO MLData(MLModelID, FileID, IsTrainingData, DateTimeStamp, Data)
                    SELECT MLModel.ID, FAMFile.ID, @IsTrainingData, GETDATE(), @Data
                    FROM MLModel, FAMFILE WHERE MLModel.Name = @ModelName AND FAMFile.FileName = @FileName";
                    cmd.Parameters.AddWithValue("@IsTrainingData", (rng.Next(2) == 0));
                    cmd.Parameters.AddWithValue("@Data", UtilityMethods.FormatInvariant($"{i}\r\n"));
                    cmd.Parameters.AddWithValue("@ModelName", _MODEL_NAME);
                    cmd.Parameters.AddWithValue("@FileName", "dummy");

                    cmd.ExecuteNonQuery();
                }
            }

            _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Directory.CreateDirectory(_inputFolder.Last());
        }

        #endregion Overhead

        #region Tests

        // Test that the training process runs without error
        [Test, Category("MLModelTrainer")]
        public static void DummyTrainingCommand()
        {
            try
            {
                CreateDatabase();

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
                        DatabaseName = _DB_NAME
                    };

                    trainer.Process(CancellationToken.None);

                    var expected = "Training\r\n";
                    var trainingOutput = File.ReadAllText(dest.FileName);
                    Assert.AreEqual(expected, trainingOutput);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Test that the testing process runs without error
        [Test, Category("MLModelTrainer")]
        public static void DummyTestingCommand()
        {
            try
            {
                CreateDatabase();

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
                        DatabaseName = _DB_NAME
                    };

                    trainer.Process(CancellationToken.None);

                    var expected = "Testing\r\n";
                    var testingOutput = File.ReadAllText(dest.FileName);
                    Assert.AreEqual(expected, testingOutput);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Test that both training and testing processes run without error
        [Test, Category("MLModelTrainer")]
        public static void DummyCommands()
        {
            try
            {
                CreateDatabase();

                var trainingExe = Path.Combine(_inputFolder.Last(), "train.bat");
                var testingExe = Path.Combine(_inputFolder.Last(), "test2.bat");
                _testFiles.GetFile("Resources.train.bat", trainingExe);
                _testFiles.GetFile("Resources.test2.bat", testingExe);
                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new MLModelTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\"",
                        TestingCommand = testingExe.Quote() + " \"<TempModelPath>\"",
                        MaximumTrainingRecords = 10000,
                        MaximumTestingRecords = 10000,
                        DatabaseServer = "(local)",
                        DatabaseName = _DB_NAME
                    };

                    trainer.Process(CancellationToken.None);

                    var expected = "Training Result:\r\nTraining\r\n";
                    var testingOutput = File.ReadAllText(dest.FileName);
                    Assert.AreEqual(expected, testingOutput);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Test exit code handling
        [Test, Category("MLModelTrainer")]
        public static void FailedTrainingCommand()
        {
            try
            {
                CreateDatabase();

                var trainingExe = Path.Combine(_inputFolder.Last(), "train.bad.bat");
                _testFiles.GetFile("Resources.train.bad.bat", trainingExe);
                var dest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                var trainer = new MLModelTrainer
                {
                    ModelName = _MODEL_NAME,
                    ModelDestination = dest,
                    TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\"",
                    TestingCommand = null,
                    MaximumTrainingRecords = 10000,
                    DatabaseServer = "(local)",
                    DatabaseName = _DB_NAME
                };

                var ex = Assert.Throws<ExtractException>(() => trainer.Process(CancellationToken.None));
                Assert.AreEqual("Training failed", ex.Message);
                Assert.False(File.Exists(dest));
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Test exit code handling
        [Test, Category("MLModelTrainer")]
        public static void FailedTestingCommand()
        {
            try
            {
                CreateDatabase();

                var testingExe = Path.Combine(_inputFolder.Last(), "test.bad.bat");
                _testFiles.GetFile("Resources.test.bad.bat", testingExe);
                var dest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                var trainer = new MLModelTrainer
                {
                    ModelName = _MODEL_NAME,
                    ModelDestination = dest,
                    TrainingCommand = null,
                    TestingCommand = testingExe.Quote() + " \"<TempModelPath>\"",
                    MaximumTestingRecords = 10000,
                    DatabaseServer = "(local)",
                    DatabaseName = _DB_NAME
                };

                var ex = Assert.Throws<ExtractException>(() => trainer.Process(CancellationToken.None));
                Assert.AreEqual("Testing failed", ex.Message);
                Assert.False(File.Exists(dest));
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Confirm that the process correctly retrieves data from the DB
        [Test, Category("MLModelTrainer")]
        public static void GetDataFromDB()
        {
            try
            {
                TestTrainingDataCollector.Setup();
                TestTrainingDataCollector.CreateDatabase();
                TestTrainingDataCollector.Process();

                _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                Directory.CreateDirectory(_inputFolder.Last());

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
                        DatabaseName = TestTrainingDataCollector.DBName,
                        MinimumF1Score = 0
                    };

                    trainer.Process(CancellationToken.None);

                    var trainingOutput = File.ReadAllText(dest.FileName);
                    Assert.AreEqual(18305, trainingOutput.Length);
                    Assert.AreEqual("Washington County , Oregon 1 000 123456 ", trainingOutput.Substring(0, 40));
                }
            }
            finally
            {
                TestTrainingDataCollector.FinalCleanup();
            }
        }

        // Test that an encrypted output file is created
        [Test, Category("MLModelTrainer")]
        public static void EncryptedOutput()
        {
            try
            {
                CreateDatabase();

                var trainingExe = Path.Combine(_inputFolder.Last(), "train.bat");
                _testFiles.GetFile("Resources.train.bat", trainingExe);
                using (var dest = new TemporaryFile(".etf", false))
                {
                    var trainer = new MLModelTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\"",
                        TestingCommand = null,
                        MaximumTrainingRecords = 10000,
                        DatabaseServer = "(local)",
                        DatabaseName = _DB_NAME
                    };

                    trainer.Process(CancellationToken.None);

                    var expected = new byte [] { 134, 229, 5, 229, 22, 201, 81, 37, 94, 70, 57, 40, 127, 77, 225, 36 };
                    var trainingOutput = File.ReadAllBytes(dest.FileName);
                    CollectionAssert.AreEqual(expected, trainingOutput);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Train a learning machine classifier
        [Test, Category("MLModelTrainer")]
        public static void TestLearningMachine()
        {
            try
            {
                TestTrainingDataCollector.Setup();
                TestTrainingDataCollector.CreateDatabase();
                TestTrainingDataCollector.Process(learningMachine: true);

                _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                Directory.CreateDirectory(_inputFolder.Last());

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
                    DatabaseName = TestTrainingDataCollector.DBName
                };

                trainer.Process(CancellationToken.None);

                lm = LearningMachine.Load(dest);
                Assert.That(lm.IsTrained);
            }
            finally
            {
                // Remove the modified LM
                _testFiles.RemoveFile("Resources.docClassifier.lm");

                TestTrainingDataCollector.FinalCleanup();
            }
        }

        // Check to make sure the different classifier types are loadable by the trainer app
        // https://extract.atlassian.net/browse/ISSUE-15486
        [Test, Category("MLModelTrainer")]
        public static void TestLearningMachineSerialization()
        {
            try
            {
                TestTrainingDataCollector.Setup();
                TestTrainingDataCollector.CreateDatabase();
                TestTrainingDataCollector.Process(learningMachine: true);

                _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                Directory.CreateDirectory(_inputFolder.Last());

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
                        DatabaseName = TestTrainingDataCollector.DBName
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

                TestTrainingDataCollector.FinalCleanup();
            }
        }

        // Train a learning machine classifier and then changing
        // a document type
        [Test, Category("MLModelTrainer")]
        public static void LearningMachineRenameDocType()
        {
            try
            {
                TestTrainingDataCollector.Setup();
                TestTrainingDataCollector.CreateDatabase();
                TestTrainingDataCollector.Process(learningMachine: true);

                _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                Directory.CreateDirectory(_inputFolder.Last());

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
                    DatabaseName = TestTrainingDataCollector.DBName
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

                TestTrainingDataCollector.FinalCleanup();
            }
        }

        static void SimulateTrainingAndTesting(double lastF1Score, double minimumF1Score, double allowableAccuracyDrop, bool expectSuccess = true, bool interactive = false)
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
                    , DatabaseName = _DB_NAME
                };

                if (interactive)
                {
                    UtilityMethods.ShowMessageBox("Configure/confirm email settings with EmailFile.exe /c", "", false);
                    UtilityMethods.ShowMessageBox("Fill in email address and subject fields on the next screen and click OK", "", false);
                    using (var form = new MLModelTrainerConfigurationDialog(trainer, "(local)", _DB_NAME))
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
            try
            {
                CreateDatabase();

                // Stays the same
                SimulateTrainingAndTesting(lastF1Score: 0.5116, minimumF1Score: 0.04, allowableAccuracyDrop: 0.05);

                // Drops 0.05
                SimulateTrainingAndTesting(lastF1Score: 0.5616, minimumF1Score: 0.04, allowableAccuracyDrop: 0.05);

                // Drops to minimum
                SimulateTrainingAndTesting(lastF1Score: 0.5616, minimumF1Score: 0.5116, allowableAccuracyDrop: 0.1);

                // Increases
                SimulateTrainingAndTesting(lastF1Score: 0.5115, minimumF1Score: 0.04, allowableAccuracyDrop: 0.05);
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Test unacceptable result: f1 score drops more than the allowable amount or drops below the minimum allowed
        // train.bat will write the word "Training" to the <TempModelPath>
        // if the testing result is deemed acceptable, this file will be copied to the destination
        [Test, Category("MLModelTrainer")]
        public static void UnacceptableResult()
        {
            try
            {
                CreateDatabase();

                // Drops 0.051
                SimulateTrainingAndTesting(lastF1Score: 0.5626, minimumF1Score: 0.04, allowableAccuracyDrop: 0.05, expectSuccess: false);

                // Drops below minimum
                SimulateTrainingAndTesting(lastF1Score: 0.5116, minimumF1Score: 0.5117, allowableAccuracyDrop: 0.05, expectSuccess: false);
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Test unacceptable result: f1 score drops more than the allowable amount or drops below the minimum allowed
        // train.bat will write the word "Training" to the <TempModelPath>
        // if the testing result is deemed acceptable, this file will be copied to the destination
        [Test, Category("Interactive")]
        public static void Interactive_UnacceptableResult()
        {
            try
            {
                CreateDatabase();

                // Drops 0.051
                SimulateTrainingAndTesting(lastF1Score: 0.5626, minimumF1Score: 0.04, allowableAccuracyDrop: 0.05, expectSuccess: false, interactive: true);
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        [Test, Category("MLModelTrainer")]
        public static void SimulateOutOfMemory()
        {
            try
            {
                CreateDatabase();
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
                        , DatabaseName = _DB_NAME
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
                _testDbManager.RemoveDatabase(_DB_NAME);
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
    }
}
