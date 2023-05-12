using AlertManager;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using AlertManager.ViewModels;
using AlertManager.Views;
using Avalonia.Controls;
using DynamicData;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.AlertManager.Test.MockData;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using Moq;
using NUnit.Framework.Internal;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class EventsOverallViewModelUnitTests
    {
        [SetUp]
        public void Init()
        {

        }

        #region Constructor Testing
        [Test]
        public void TestConstructorInits([ValueSource(nameof(EventsSource))] EventDto eventObject)
        {
            Mock<IWindowService> mockWindow = new();
            Mock<IElasticSearchLayer> mockElastic = new();

            List<EventDto> events = new();
            events.Add(eventObject);

            mockElastic.Setup(m => m.GetAllEvents(0)).Returns(events);
            mockElastic.Setup(m => m.GetAllAlerts(0)).Returns( new List<AlertsObject>() );
            mockElastic.Setup(m => m.GetMaxEventPages()).Returns(1);

            EventsOverallViewModel sut = new(mockWindow.Object, mockElastic.Object, eventObject);

            Assert.Multiple(() =>
            {
                Assert.That(sut.GetEvent, Is.EqualTo(eventObject));
                Assert.That(sut.GetService, Is.Not.Null);
            });
        }

        [Test]
        public void TestNullConstructors()
        {
            Assert.Throws<UnhandledErrorException>(
                delegate { EventsOverallViewModel sut = new(null, null, null); });
        }

        #endregion Constructor Testing



        #region Sources

        public static IEnumerable<AlertsObject> AlertsSource()
        {
            yield return new();
        }

        public static IEnumerable<EventDto> EventsSource()
        {
            yield return new();
        }

        #endregion Sources
    }
}
