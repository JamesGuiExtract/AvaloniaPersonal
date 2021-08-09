using Extract.Testing.Utilities;
using Moq;
using MvvmGen.Events;
using NUnit.Framework;
using System;

namespace Extract.Utilities.SqlCompactToSqliteConverter.Test
{
    // Tests in this class must be run serially unless [FixtureLifeCycle(LifeCycle.InstancePerTestCase)] is used (upgrade nunit)
    [TestFixture]
    [Category("SqlCompactToSqliteConverter")]
    public class TestMainViewModel
    {
        private Mock<IEventAggregator> _eventAggregatorMock;
        private MainViewModel _mainViewModel;

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

            _mainViewModel = new(_eventAggregatorMock.Object,
                new(_eventAggregatorMock.Object,
                    new Mock<IMessageDialogService>().Object,
                    new Mock<IFileBrowserDialogService>().Object,
                    new Mock<IDatabaseConverter>().Object));
        }

        /// Confirm that the view model subscribes to the ArgumentsEvent
        [Test]
        [Category("Automated")]
        public void ShouldSubscribeToArgumentsEvent()
        {
            _eventAggregatorMock.Verify(ea => ea.RegisterSubscriber(
                It.Is<IEventSubscriber<ArgumentsEvent>>(es => es == _mainViewModel)), Times.Once());
        }

        /// Confirm that the view model publishes a DatabaseInputOutputEvent in response to arguments
        [Test]
        [Category("Automated")]
        public void ShouldPublishDatabaseInputOutputEventInResponseToArguments()
        {
            // Publish event
            _mainViewModel.OnEvent(new ArgumentsEvent(new[] { "input.db", "output.db" }));

            // Confirm another event is published with the input/output paths
            _eventAggregatorMock.Verify(ea => ea.Publish(It.Is<DatabaseInputOutputEvent>(paths =>
                paths.InputDatabasePath == "input.db" && paths.OutputDatabasePath == "output.db")), Times.Once);
        }

        /// Confirm that the view model doesn't publish an event when no arguments are supplied
        [Test]
        [Category("Automated")]
        public void ShouldNotPublishDatabaseInputOutputEventWhenNoArguments()
        {
            // Publish event
            _mainViewModel.OnEvent(new ArgumentsEvent(Array.Empty<string>()));

            // Confirm no DatabaseInputOutputEvent event is published
            _eventAggregatorMock.Verify(ea => ea.Publish(It.IsAny<DatabaseInputOutputEvent>()), Times.Never);
        }

        /// Confirm that the view model will fill in the output path if omitted
        [Test]
        [Category("Automated")]
        public void ShouldPublishDatabaseInputOutputEventInResponseToSingleArgument()
        {
            _mainViewModel.OnEvent(new ArgumentsEvent(new[] { "input.db" }));
            _eventAggregatorMock.Verify(ea => ea.Publish(It.Is<DatabaseInputOutputEvent>(paths =>
                paths.InputDatabasePath == "input.db" && paths.OutputDatabasePath == "input.sqlite")), Times.Once);
        }
    }
}