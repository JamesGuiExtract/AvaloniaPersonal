using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlertManager;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using Moq;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    //using moq
    [TestFixture]
    internal class AlertStatusUnitTests
    {


        [SetUp]
        public void Init()
        {

        }

        //note, can't really test this well b/c of how its set up, will have basic tests to see that its can fetch data
        [Test]
        [Ignore("will not work due to setup fix in jira https://extract.atlassian.net/browse/ISSUE-18827")]
        public void TestGetAllAlerts()
        {
            Mock<LoggingTargetElasticsearch> mockStatus = new();
            Assert.DoesNotThrow(() => mockStatus.Object.GetAllAlerts(0)); //right now this fails b/c cloud portion is not dp injected so i can't manipulate it
        }

        [Test]
        [Ignore("will not work due to setup fix in jira https://extract.atlassian.net/browse/ISSUE-18827")]
        public void TestGetAllEvents()
        {
            Mock<LoggingTargetElasticsearch> mockStatus = new();
            Assert.DoesNotThrow(() => mockStatus.Object.GetAllEvents(0));
        }

    }
}
