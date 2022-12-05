using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Services;
using AlertManager.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
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
        DBService dbService;
        private string readAlertFileLocation = ""; //TODO add filepath to the mock data i created
        private string readEventFileLocation = "";
        private string writeAlertFileLocation = "";
        private string writeEventFileLocation = "";
        private int numberOfDocumentsExpected = 25;
        private int fileNumberReturnedOnError = -1; //should have this as a thing in dbService and just check there



        [SetUp]
        public void Init()
        {

            dbService = new DBService();
            dbService.ErrorFileLocation = readEventFileLocation;
            dbService.AlertFileLocation = readAlertFileLocation;
        }

        [Test]
        public void TestReadAllAlerts()
        {
            //todo expected list
            List<LogAlert> expectedList = new();
            Assert.Multiple( () => 
            {
                Assert.DoesNotThrow( () => { dbService.ReadAllAlerts(); });
                Assert.That(dbService.ReadAllAlerts(), Is.EqualTo(expectedList));
            });
        }

        [Test]
        public void TestReadAllAlertsNull()
        {
            dbService.AlertFileLocation = null;
            Assert.DoesNotThrow(() => 
            {
                dbService.ReadAlertObjects();
            });
        }

        [Test]
        public void TestReadAllErrors()
        {
            List<LogError> expectedList = new(); 
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => { dbService.ReadAllErrors(); });
                Assert.That(dbService.ReadAllErrors(), Is.EqualTo(expectedList));
            });
        }

        [Test]
        public void TestReadAllErrorsNull()
        {
            dbService.ErrorFileLocation = null;
            Assert.DoesNotThrow(() =>
            {
                dbService.ReadAllErrors();
            });
        }

        [Test]
        public void TestGetDocumentTotal()
        {
            Assert.That(dbService.GetDocumentTotal(), Is.EqualTo(numberOfDocumentsExpected));
        }

        [Test]
        public void TestGetDocumentTotalNull()
        {
            //i mean its hard coded right now so it'll never fail...
            dbService.ErrorFileLocation = null;
            dbService.AlertFileLocation = null;

            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() =>
                {
                    dbService.GetDocumentTotal();
                });

                Assert.That(dbService.GetDocumentTotal, Is.EqualTo(fileNumberReturnedOnError));
            });
            
        }

        [Test]
        [Ignore("now obsolete, done with elasticsearchimplimentation")]
        public void TestReturnFromDatabase([ValueSource(nameof(DataSource))] DataNeededForPage data, [ValueSource(nameof(ListValueSource))] int listNumber)
        {
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => { dbService.ReturnFromDatabase(listNumber); });
                Assert.That(dbService.ReturnFromDatabase(listNumber), Is.EqualTo(data));
            });
        }

        [Test]
        public void TestReturnFromDatabaseNull()
        {
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
            dbService.ErrorFileLocation = null;
            dbService.AlertFileLocation = null;

            Assert.DoesNotThrow(() =>
            {
                dbService.GetDocumentTotal();
            });
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
