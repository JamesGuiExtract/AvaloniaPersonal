using AlertManager.Interfaces;
using AlertManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    internal class AppUnitTests
    {
        IDBService dbService = new DBService(new FileProcessingDB());

        [OneTimeSetUp] //if i use a database
        public void OneTimeSetUp()
        {
        }

        [SetUp]
        [Ignore("Not yet valid to test")]
        public void Init()
        {
            //initialize backends
        }

        [TearDown]
        public void Clear()
        {

        }

        [Test]
        [Ignore("Not yet valid to test")]
        public void TestInitialize()
        {

        }

        [Test]
        [Ignore("Not yet valid to test")]
        public void TestRegestration()
        {

        }
    }
}
