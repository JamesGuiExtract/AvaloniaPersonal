using Extract.Testing.Utilities;
using Moq;
using MvvmGen.Events;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.Utilities.SqlCompactToSqliteConverter.Test
{
    // Tests in this class must be run serially unless [FixtureLifeCycle(LifeCycle.InstancePerTestCase)] is used (upgrade nunit)
    [TestFixture]
    [Category("SqlCompactToSqliteConverter")]
    public class TestDatabaseConverterViewModel
    {
        Mock<IEventAggregator> _eventAggregatorMock;
        Mock<IMessageDialogService> _messageDialogServiceMock;
        Mock<IFileBrowserDialogService> _fileBrowserDialogServiceMock;
        Mock<IDatabaseConverter> _databaseConverterMock;
        DatabaseConverterViewModel _viewModel;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        // Per-test setup
        [SetUp]
        public void Init()
        {
            _eventAggregatorMock = new();
            _messageDialogServiceMock = new();
            _fileBrowserDialogServiceMock = new();
            _databaseConverterMock = new();

            _viewModel = new(
                _eventAggregatorMock.Object,
                _messageDialogServiceMock.Object,
                _fileBrowserDialogServiceMock.Object,
                _databaseConverterMock.Object);
        }

        /// Confirm that the view model subscribes to the DatabaseInputOutputEvent update event
        [Test]
        [Category("Automated")]
        public void ShouldSubscribeToDatabaseInputOutputEvent()
        {
            _eventAggregatorMock.Verify(ea => ea.RegisterSubscriber(
                    It.Is<IEventSubscriber<DatabaseInputOutputEvent>>(es => es == _viewModel)), Times.Once());
        }

        /// Confirm that the DatabaseInputOutputEvent event updates the view model
        [Test, Combinatorial]
        [Category("Automated")]
        public void DatabaseInputOutputEvent_ShouldUpdateTheViewModel(
            [Values(null, "", "input.db")] string inputPath,
            [Values(null, "", "output.db")] string outputPath)
        {
            // Verify initial state
            Assert.IsNull(_viewModel.InputDatabasePath);
            Assert.IsNull(_viewModel.OutputDatabasePath);

            // Publish event
            _viewModel.OnEvent(new DatabaseInputOutputEvent(inputPath, outputPath));

            // Confirm that both paths have been set
            Assert.AreEqual(inputPath, _viewModel.InputDatabasePath);
            Assert.AreEqual(outputPath, _viewModel.OutputDatabasePath);
        }

        /// Confirm that the DatabaseInputOutputEvent event fires change notification
        [Test]
        [Category("Automated")]
        public void DatabaseInputOutputEvent_ShouldUpdateConvertCommandCanExecute()
        {
            Assert.IsFalse(_viewModel.ConvertCommand.CanExecute(null));

            _viewModel.OnEvent(new DatabaseInputOutputEvent("input.db", "output.db"));

            Assert.IsTrue(_viewModel.ConvertCommand.CanExecute(null));
        }

        /// Confirm that the the convert command only enabled if all paths are not empty
        [Test, Combinatorial]
        [Category("Automated")]
        public void ConvertCommand_ShouldBeEnabledWhenPathsAreNotEmpty(
            [Values(null, "", " ", "\r", "\n", "\t", "\"", "input.db")] string inputPath,
            [Values(null, "", " ", "\r", "\n", "\t", "\"", "output.db")] string outputPath)
        {
            _viewModel.InputDatabasePath = inputPath;
            _viewModel.OutputDatabasePath = outputPath;

            bool expectedValue =
                   !string.IsNullOrEmpty(inputPath?.Trim(' ', '\r', '\n', '\t', '"'))
                && !string.IsNullOrEmpty(outputPath?.Trim(' ', '\r', '\n', '\t', '"'));

            Assert.AreEqual(expectedValue, _viewModel.ConvertCommand.CanExecute(null));
        }

        /// Confirm that the the convert command runs the IDatabaseConverter.Convert method
        [Test]
        [Category("Automated")]
        public void ConvertCommand_CallsConvert()
        {
            // Run convert and verify that Convert was called on the mock
            _viewModel.ConvertCommand.Execute(null);
            _databaseConverterMock.Verify(x => x.Convert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()),
                Times.Once);
        }

        /// Confirm that the the convert command trims quotes from the input path
        [Test]
        [Category("Automated")]
        public void ConvertCommand_TrimsQuotesFromViewModelInputPath()
        {
            _viewModel.InputDatabasePath = @"""input.sdf""";
            _viewModel.OutputDatabasePath = @"output.sqlite";

            bool propertyChangedFired = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(DatabaseConverterViewModel.InputDatabasePath))
                {
                    propertyChangedFired = true;
                }
            };

            // Run convert
            _viewModel.ConvertCommand.Execute(null);

            // Verify that the path has been updated and the PropertyChanged event called
            Assert.AreEqual("input.sdf", _viewModel.InputDatabasePath);
            Assert.IsTrue(propertyChangedFired);
        }

        /// Confirm that the the convert command trims quotes from the output path
        [Test]
        [Category("Automated")]
        public void ConvertCommand_TrimsQuotesFromViewModelOutputPath()
        {
            _viewModel.InputDatabasePath = @"input.sdf";
            _viewModel.OutputDatabasePath = @"""output.sqlite""";

            bool propertyChangedFired = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(DatabaseConverterViewModel.OutputDatabasePath))
                {
                    propertyChangedFired = true;
                }
            };

            // Run convert
            _viewModel.ConvertCommand.Execute(null);

            // Verify that the path has been updated and the PropertyChanged event called
            Assert.AreEqual("output.sqlite", _viewModel.OutputDatabasePath);
            Assert.IsTrue(propertyChangedFired);
        }

        /// Confirm that the the convert command is not enabled while the convert command is running
        [Test]
        [Category("Automated")]
        public void ConvertCommand_ShouldBeDisabledWhenConvertIsExecuting()
        {
            _viewModel.InputDatabasePath = "input.db";
            _viewModel.OutputDatabasePath = "output.db";

            // Verify precondition
            Assert.IsTrue(_viewModel.ConvertCommand.CanExecute(null));

            using ManualResetEvent stopConversion = new(false);
            try
            {
                // Mock a convert to run a task until stop is signaled
                _databaseConverterMock
                    .Setup(x => x.Convert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                    .Returns(() => Task.Run(() => stopConversion.WaitOne()));

                // Start conversion and confirm the command is disabled
                _viewModel.ConvertCommand.Execute(null);
                Assert.IsFalse(_viewModel.ConvertCommand.CanExecute(null));
            }
            finally
            {
                // let conversion task finish
                stopConversion.Set();
            }
        }

        /// Confirm that the the convert command is enabled after the convert command finishes
        [Test]
        [Category("Automated")]
        public void ConvertCommand_ShouldBeEnabledWhenConvertIsComplete()
        {
            _viewModel.InputDatabasePath = "input.db";
            _viewModel.OutputDatabasePath = "output.db";

            // Run convert and verify that Convert was called on the mock
            _viewModel.ConvertCommand.Execute(null);
            _databaseConverterMock.Verify(x => x.Convert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()),
                Times.Once);

            // Confirm that the convert command is enabled
            Assert.IsTrue(_viewModel.ConvertCommand.CanExecute(null));
        }

        /// Confirm that the the convert command updates the status message
        [Test]
        [Category("Automated")]
        public async Task ConvertCommand_UpdatesStatusMessage()
        {
            // Mock a converter that sets some status messages
            _databaseConverterMock
                .Setup(x => x.Convert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns((Func<string, string, Action<string>, Task>)
                    ((_, _, logger) =>
                        Task.Run(() =>
                        {
                            logger("message1");
                            logger("\nmessage2");
                        })));

            _viewModel.ConvertCommand.Execute(null);

            // Let the async Convert function finish
            await Task.Yield();

            Assert.IsFalse(_viewModel.IsExecuting);

            // Verify the status message on the view model
            Assert.AreEqual("message1\nmessage2", _viewModel.StatusMessage);
        }

        /// Confirm that the convert command prompts about overwriting an existing database
        [Test]
        [Category("Automated")]
        public void ConvertCommand_PromptsBeforeOverwrite(
            [Values(MessageDialogResult.Yes, MessageDialogResult.No)] MessageDialogResult dialogResult)
        {
            using TemporaryFile inputDB = new(".sdf", false);
            using TemporaryFile outputDB = new(".sqlite", false);
            _viewModel.InputDatabasePath = inputDB.FileName;
            _viewModel.OutputDatabasePath = outputDB.FileName;

            FileAssert.Exists(outputDB.FileName);

            _messageDialogServiceMock.Setup(x => x.ShowYesNoDialog(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(dialogResult);

            // Execute the command
            _viewModel.ConvertCommand.Execute(null);

            // Verify that the prompt was shown
            _messageDialogServiceMock.Verify(x => x.ShowYesNoDialog(It.IsAny<string>(), "Overwrite existing database file?"),
                Times.Once);

            // Verify that Convert was called only if result was Yes
            _databaseConverterMock.Verify(x => x.Convert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()),
                dialogResult == MessageDialogResult.Yes
                ? Times.Once
                : Times.Never);
        }

        /// Confirm that the select input database command calls the file browser service
        [Test]
        [Category("Automated")]
        public void SelectInputDatabase_CallsFileBrowserService()
        {
            _viewModel.SelectInputDatabaseCommand.Execute(null);
            _fileBrowserDialogServiceMock.Verify(x => x.SelectExistingFile(
                "Select input database",
                "SQL Compact database (*.sdf)|*.sdf|All files|*.*"), Times.Once);
        }

        /// Confirm that the select output database command calls the file browser service
        [Test]
        [Category("Automated")]
        public void SelectOutputDatabase_CallsFileBrowserService()
        {
            _viewModel.SelectOutputDatabaseCommand.Execute(null);
            _fileBrowserDialogServiceMock.Verify(x => x.SelectFile(
                "Select output database",
                "SQLite database (*.sqlite)|*.sqlite|All files|*.*"), Times.Once);
        }

        /// Confirm that the select input database command updates the view model
        [Test]
        [Category("Automated")]
        public void SelectInputDatabase_UpdatesTheViewModelWhenPathIsSelected()
        {
            _fileBrowserDialogServiceMock.Setup(x => x.SelectExistingFile(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("input.sdf");

            // Verify that the paths are null
            Assert.IsNull(_viewModel.InputDatabasePath);
            Assert.IsNull(_viewModel.OutputDatabasePath);

            // Run the select input database command
            _viewModel.SelectInputDatabaseCommand.Execute(null);

            // Verify that the input path has been updated and the output path remains null
            Assert.AreEqual("input.sdf", _viewModel.InputDatabasePath);
            Assert.IsNull(_viewModel.OutputDatabasePath);
        }

        /// Confirm that the select input database command doesn't update the view model when the dialog returns null
        [Test]
        [Category("Automated")]
        public void SelectInputDatabase_DoesNotUpdateTheViewModelWhenNoPathIsSelected()
        {
            _fileBrowserDialogServiceMock.Setup(x => x.SelectExistingFile(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string)null);

            // Set an initial state for the paths
            _viewModel.InputDatabasePath = "input.sdf";
            _viewModel.OutputDatabasePath = "output.sqlite";

            // Run the select input database command
            _viewModel.SelectInputDatabaseCommand.Execute(null);

            // Verify that nothing has changed
            Assert.AreEqual("input.sdf", _viewModel.InputDatabasePath);
            Assert.AreEqual("output.sqlite", _viewModel.OutputDatabasePath);
        }

        /// Confirm that the select output database command updates the view model
        [Test]
        [Category("Automated")]
        public void SelectOutputDatabase_UpdatesTheViewModel()
        {
            _fileBrowserDialogServiceMock.Setup(x => x.SelectFile(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("output.sqlite");

            // Verify that the paths are null
            Assert.IsNull(_viewModel.InputDatabasePath);
            Assert.IsNull(_viewModel.OutputDatabasePath);

            // Run the select output database command
            _viewModel.SelectOutputDatabaseCommand.Execute(null);

            // Verify that the output path has been updated and the input path remains null
            Assert.AreEqual("output.sqlite", _viewModel.OutputDatabasePath);
            Assert.IsNull(_viewModel.InputDatabasePath);
        }

        /// Confirm that the select output database command doesn't update the view model when the dialog returns null
        [Test]
        [Category("Automated")]
        public void SelectOutputDatabase_DoesNotUpdateTheViewModelWhenNoPathIsSelected()
        {
            _fileBrowserDialogServiceMock.Setup(x => x.SelectFile(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string)null);

            // Set an initial state for the paths
            _viewModel.InputDatabasePath = "input.sdf";
            _viewModel.OutputDatabasePath = "output.sqlite";

            // Run the select output database command
            _viewModel.SelectOutputDatabaseCommand.Execute(null);

            // Verify that nothing has changed
            Assert.AreEqual("input.sdf", _viewModel.InputDatabasePath);
            Assert.AreEqual("output.sqlite", _viewModel.OutputDatabasePath);
        }
    }
}