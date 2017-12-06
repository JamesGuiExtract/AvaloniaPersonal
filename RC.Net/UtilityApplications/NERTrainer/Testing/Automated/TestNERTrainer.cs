using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.UtilityApplications.NERDataCollector.Test;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            fileProcessingDB.CloseAllDBConnections();

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
                        TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\""
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
                        TestingCommand = testingExe.Quote() + " \"<TempModelPath>\""
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
                        TestingCommand = testingExe.Quote() + " \"<TempModelPath>\""
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
                    TestingCommand = testingExe.Quote() + " \"<TempModelPath>\""
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
                        TrainingCommand = trainingExe.Quote() + "\"<DataFile>\" \"<TempModelPath>\""
                    };

                    trainer.Process("(local)", TestNERDataCollector.DBName);

                    var testingOutput = File.ReadAllText(dest.FileName);
                    Assert.AreEqual(18346, testingOutput.Length);
                    Assert.AreEqual("Washington County , Oregon 1 000 123456 ", testingOutput.Substring(0, 40));
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
                        TrainingCommand = trainingExe.Quote() + " \"<TempModelPath>\""
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

        #endregion Tests
    }
}
