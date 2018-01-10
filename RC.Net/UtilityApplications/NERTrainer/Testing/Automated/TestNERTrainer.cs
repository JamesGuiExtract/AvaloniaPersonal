﻿using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.UtilityApplications.NERDataCollector.Test;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.NERTrainer.Test
{
    /// <summary>
    /// Unit tests for NERTrainer class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("NERTrainer")]
    public class TestNERTrainer
    {
        #region Fields

        /// <summary>
        /// Manages the test data files
        /// </summary>
        static TestFileManager<TestNERTrainer> _testFiles;
        static List<string> _inputFolder = new List<string>();

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestNERTrainer> _testDbManager;

        static readonly string _DB_NAME = "_TestNERTrainer_14394B59-A748-4418-B11A-A5682E3C5A5B";
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
            _testFiles = new TestFileManager<TestNERTrainer>();
            _testDbManager = new FAMTestDBManager<TestNERTrainer>();
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
        [Test, Category("NERTrainer")]
        public static void DummyTrainingCommand()
        {
            try
            {
                CreateDatabase();

                var trainingExe = Path.Combine(_inputFolder.Last(), "train.bat");
                _testFiles.GetFile("Resources.train.bat", trainingExe);
                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new NERTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\"",
                        MaximumTrainingDocuments = 10000
                    };

                    trainer.Process("(local)", _DB_NAME);

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
        [Test, Category("NERTrainer")]
        public static void DummyTestingCommand()
        {
            try
            {
                CreateDatabase();

                var testingExe = Path.Combine(_inputFolder.Last(), "test1.bat");
                _testFiles.GetFile("Resources.test1.bat", testingExe);
                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new NERTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        TestingCommand = testingExe.Quote() + " \"<TempModelPath>\"",
                        MaximumTestingDocuments = 10000
                    };

                    trainer.Process("(local)", _DB_NAME);

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
        [Test, Category("NERTrainer")]
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
                    var trainer = new NERTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\"",
                        TestingCommand = testingExe.Quote() + " \"<TempModelPath>\"",
                        MaximumTrainingDocuments = 10000,
                        MaximumTestingDocuments = 10000
                    };

                    trainer.Process("(local)", _DB_NAME);

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
        [Test, Category("NERTrainer")]
        public static void FailedTrainingCommand()
        {
            try
            {
                CreateDatabase();

                var trainingExe = Path.Combine(_inputFolder.Last(), "train.bad.bat");
                _testFiles.GetFile("Resources.train.bad.bat", trainingExe);
                var dest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                var trainer = new NERTrainer
                {
                    ModelName = _MODEL_NAME,
                    ModelDestination = dest,
                    TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\"",
                    MaximumTrainingDocuments = 10000
                };

                var ex = Assert.Throws<ExtractException>(() => trainer.Process("(local)", _DB_NAME));
                Assert.AreEqual("Training failed", ex.Message);
                Assert.False(File.Exists(dest));
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Test exit code handling
        [Test, Category("NERTrainer")]
        public static void FailedTestingCommand()
        {
            try
            {
                CreateDatabase();

                var testingExe = Path.Combine(_inputFolder.Last(), "test.bad.bat");
                _testFiles.GetFile("Resources.test.bad.bat", testingExe);
                var dest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                var trainer = new NERTrainer
                {
                    ModelName = _MODEL_NAME,
                    ModelDestination = dest,
                    TestingCommand = testingExe.Quote() + " \"<TempModelPath>\"",
                    MaximumTestingDocuments = 10000

                };

                var ex = Assert.Throws<ExtractException>(() => trainer.Process("(local)", _DB_NAME));
                Assert.AreEqual("Testing failed", ex.Message);
                Assert.False(File.Exists(dest));
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Confirm that the process correctly retrieves data from the DB
        [Test, Category("NERTrainer")]
        public static void GetDataFromDB()
        {
            try
            {
                TestNERDataCollector.Setup();
                TestNERDataCollector.CreateDatabase();
                TestNERDataCollector.Process();

                _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                Directory.CreateDirectory(_inputFolder.Last());

                var trainingExe = Path.Combine(_inputFolder.Last(), "copy.bat");
                _testFiles.GetFile("Resources.copy.bat", trainingExe);
                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new NERTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        TrainingCommand = trainingExe.Quote() + "\"<DataFile>\" \"<TempModelPath>\"",
                        MaximumTrainingDocuments = 10000

                    };

                    trainer.Process("(local)", TestNERDataCollector.DBName);

                    var trainingOutput = File.ReadAllText(dest.FileName);
                    Assert.AreEqual(18346, trainingOutput.Length);
                    Assert.AreEqual("Washington County , Oregon 1 000 123456 ", trainingOutput.Substring(0, 40));
                }
            }
            finally
            {
                TestNERDataCollector.FinalCleanup();
            }
        }

        // Test that an encrypted output file is created
        [Test, Category("NERTrainer")]
        public static void EncryptedOutput()
        {
            try
            {
                CreateDatabase();

                var trainingExe = Path.Combine(_inputFolder.Last(), "train.bat");
                _testFiles.GetFile("Resources.train.bat", trainingExe);
                using (var dest = new TemporaryFile(".etf", false))
                {
                    var trainer = new NERTrainer
                    {
                        ModelName = _MODEL_NAME,
                        ModelDestination = dest.FileName,
                        TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\"",
                        MaximumTrainingDocuments = 10000
                    };

                    trainer.Process("(local)", _DB_NAME);

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

        static void SimulateTrainingAndTesting(double lastF1Score, double minimumF1Score, double allowableAccuracyDrop, bool expectSuccess = true, bool interactive = false)
        {
            var trainingExe = Path.Combine(_inputFolder.Last(), "train.bat");
            _testFiles.GetFile("Resources.train.bat", trainingExe);
            using (var testingExe = new TemporaryFile(".bat", false))
            using (var dest = new TemporaryFile(false))
            {
                var trainer = new NERTrainer
                {
                    ModelName = _MODEL_NAME
                    , ModelDestination = dest.FileName
                    , TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\""
                    , TestingCommand = testingExe.FileName.Quote() + " \"<TempModelPath>\""
                    , MinimumF1Score = minimumF1Score
                    , LastF1Score = lastF1Score
                    , AllowableAccuracyDrop = allowableAccuracyDrop
                    , MaximumTrainingDocuments = 10000
                    , MaximumTestingDocuments = 10000
                };

                if (interactive)
                {
                    UtilityMethods.ShowMessageBox("Configure/confirm email settings with EmailFile.exe /c", "", false);
                    UtilityMethods.ShowMessageBox("Fill in email address and subject fields on the next screen and click OK", "", false);
                    using (var form = new NERTrainerConfigurationDialog(trainer, "(local)", _DB_NAME))
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

                trainer.Process("(local)", _DB_NAME);

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
        [Test, Category("NERTrainer")]
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
        [Test, Category("NERTrainer")]
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

        [Test, Category("NERTrainer")]
        public static void SimulateOutOfMemory()
        {
            try
            {
                CreateDatabase();
                using (var trainingExe = new TemporaryFile(".bat", false))
                using (var testingExe = new TemporaryFile(".bat", false))
                using (var dest = new TemporaryFile(false))
                {
                    var trainer = new NERTrainer
                    {
                        ModelName = _MODEL_NAME
                        , ModelDestination = dest.FileName
                        , TrainingCommand = trainingExe.FileName.Quote() + " \"<DataFile>\" \"<TempModelPath>\""
                        , TestingCommand = testingExe.FileName.Quote() + " \"<DataFile>\""
                        , MaximumTrainingDocuments = 10000
                        , MaximumTestingDocuments = 10000
                        , LastIDProcessed = 50
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

                    trainer.Process("(local)", _DB_NAME);

                    // Destination file will be empty if testing result was not acceptable
                    var expected = "Training\r\n";
                    var testingOutput = File.ReadAllText(dest.FileName);

                    Assert.AreEqual(expected, testingOutput);

                    // Maximum training files will have been reduced
                    Assert.LessOrEqual(trainer.MaximumTrainingDocuments, 30);

                    // Maximum testing files will have been reduced
                    Assert.LessOrEqual(trainer.MaximumTestingDocuments, 30);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // Test loading/saving from JSON
        [Test, Category("NERTrainer")]
        public static void LoadingSavingFromJson()
        {
            var trainerSettings = _testFiles.GetFile("Resources.NERTrainerSettings.txt");
            var trainer = NERTrainer.FromJson(File.ReadAllText(trainerSettings));

            var jsonSettings = trainer.ToJson();
            trainer.EmailSubject = "DUMMY_SUBJECT";
            var updatedJsonSettings = trainer.ToJson();

            Assert.AreNotEqual(jsonSettings, updatedJsonSettings);

            var updatedtrainer = (NERTrainer)JsonConvert.DeserializeObject(updatedJsonSettings,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });

            Assert.AreEqual("DUMMY_SUBJECT", updatedtrainer.EmailSubject);

            // Verify that other, persisted settings are the same
            Assert.AreEqual(trainer.AllowableAccuracyDrop, updatedtrainer.AllowableAccuracyDrop);
            Assert.AreEqual(trainer.Description, updatedtrainer.Description);
            Assert.AreEqual(trainer.EmailAddressesToNotifyOnFailure, updatedtrainer.EmailAddressesToNotifyOnFailure);
            Assert.AreEqual(trainer.LastF1Score, updatedtrainer.LastF1Score);
            Assert.AreEqual(trainer.LastIDProcessed, updatedtrainer.LastIDProcessed);
            Assert.AreEqual(trainer.MaximumTestingDocuments, updatedtrainer.MaximumTestingDocuments);
            Assert.AreEqual(trainer.MaximumTrainingDocuments, updatedtrainer.MaximumTrainingDocuments);
            Assert.AreEqual(trainer.MinimumF1Score, updatedtrainer.MinimumF1Score);
            Assert.AreEqual(trainer.ModelDestination, updatedtrainer.ModelDestination);
            Assert.AreEqual(trainer.ModelName, updatedtrainer.ModelName);
            Assert.AreEqual(trainer.TestingCommand, updatedtrainer.TestingCommand);
            Assert.AreEqual(trainer.TrainingCommand, updatedtrainer.TrainingCommand);
        }

        #endregion Tests
    }
}
