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
        DBService dbService;
        private string readAlertFileLocation = "something"; //TODO add filepath to the mock data i created
        private string readEventFileLocation = "something";
        private string writeAlertFileLocation = "something";
        private string writeEventFileLocation = "something";
        private int numberOfDocumentsExpected = 25;
        private int fileNumberReturnedOnError = -1; //should have this as a thing in dbService and just check there

        [SetUp]
        public void Init()
        {
            
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
