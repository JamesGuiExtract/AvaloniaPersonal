using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Services;
using AlertManager.ViewModels;
using Extract.ErrorHandling;
using Moq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    ///VERY IMPORTANT README!!!: the files being tested for are the expected from dummy data, if data is changed then the tests will fail
    ///if there are issues with failing files, check the filepath first, then compare the expected data
    ///todo, rn the dummy data isn't set properly fix in future
    public class DBServiceUnitTests
    {
        //NOTE TO SELF, uses configuration manager for filepath, so need to modify that for db service

        [SetUp]
        public void Init()
        {
            
        }


        [Test]
        public void TestReadAllAlertsNull()
        {
            string readEventFileLocation = "";
            DBService dbService = new DBService();
            dbService.ErrorFileLocation = readEventFileLocation;

            dbService.AlertFileLocation = null;
            Assert.DoesNotThrow(() => 
            {
                dbService.ReadAlertObjects();
            });
        }


        [Test]
        public void TestReadAllErrorsNull()
        {
            DBService dbService = new DBService();

            dbService.ErrorFileLocation = null;

            Assert.Throws<ExtractException>( () => dbService.ReadAllErrors());
        }

        [Test]
        public void TestGetDocumentTotal()
        {
            string readAlertFileLocation = ""; //TODO add filepath to the mock data i created
            string readEventFileLocation = "";
            int numberOfDocumentsExpected = 25;
            DBService dbService = new DBService();
            dbService.ErrorFileLocation = readEventFileLocation;
            dbService.AlertFileLocation = readAlertFileLocation;

            Assert.That(dbService.GetDocumentTotal(), Is.EqualTo(numberOfDocumentsExpected));
        }

        [Test]
        public void TestGetDocumentTotalNull()
        {
            string readAlertFileLocation = ""; //TODO add filepath to the mock data i created
            string readEventFileLocation = "";
            DBService dbService = new DBService();
            dbService.ErrorFileLocation = readEventFileLocation;
            dbService.AlertFileLocation = readAlertFileLocation;

            //i mean its hard coded right now so it'll never fail...
            dbService.ErrorFileLocation = null;
            dbService.AlertFileLocation = null;

            Assert.Multiple(() =>
            {
                Assert.Throws<ExtractException>(() => dbService.GetDocumentTotal());
            });
            
        }


        [Test]
        public void TestReturnFromDatabaseNull()
        {
            DBService dbService = new DBService();


            dbService.ErrorFileLocation = null;
            dbService.AlertFileLocation = null;

            Assert.DoesNotThrow(() =>
            {
                dbService.ReturnFromDatabase(-1);
            });
        }

        [Test]
        [Ignore("will moq up the expected values and insert expected")]
        public void TestAllIssueIds()
        {
            string readAlertFileLocation = ""; //TODO add filepath to the mock data i created
            string readEventFileLocation = "";
            DBService dbService = new DBService();
            dbService.ErrorFileLocation = readEventFileLocation;
            dbService.AlertFileLocation = readAlertFileLocation;

            List<int> expectedList = new();
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => { dbService.AllIssueIds(); });
                Assert.That(dbService.AllIssueIds(), Is.EqualTo(expectedList)); ;
            });
        }

        [Test]
        public void TestAllIssueIdsNull()
        {
            DBService dbService = new DBService();

            dbService.ErrorFileLocation = null;
            dbService.AlertFileLocation = null;

            Assert.Throws<ExtractException>(() => dbService.AllIssueIds());
        }

        [Test]
        [TestCaseSource(nameof(AlertsSource))]
        [Ignore("todo finish this test when its better set up")]
        public void TestAddAlertToDatabase(AlertsObject alertObject)
        {
            //todo 
        }

        [Test]
        [TestCaseSource(nameof(AlertsSource))]
        public void TestAddAlertToDatabaseNull(AlertsObject alertObject)
        {
            DBService dbService = new DBService();

            dbService.ErrorFileLocation = null;
            dbService.AlertFileLocation = null;

            //maybe i should check if it does throw a error on this one..., or maybe creates a new filepath?, not fully created so not sure
            Assert.DoesNotThrow(() =>
            {
                dbService.AddAlertToDatabase(new());
            });
        }


        public static IEnumerable<AlertsObject> AlertsSource()
        {
            yield return new();
            yield return new AlertsObject(1, "AlertId", "TestAction2", "TestType2", "TestAlertName", "testconfig2", new DateTime(2008, 5, 1, 8, 30, 52), "testUser2", "testMachine", "testResolution", TypeOfResolutionAlerts.Snoozed, new DateTime(2008, 5, 1, 8, 30, 52), "testingAlertHistory");
        }

        public static IEnumerable<DataNeededForPage> DataSource()
        {
            yield return new();
            yield return new();
        }

        public static IEnumerable<int> ListValueSource()
        {
            yield return 0;
            yield return 1;
        }
    }
}
