using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Linq;

namespace Extract.Dashboard.Forms.Test
{
    [Category("TestGridDetailConfiguration")]
    [TestFixture]
    public class TestGridDetailConfiguration
    {
        #region Constants

        #endregion

        #region Fields

        #endregion

        #region Overhead

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
        }

        #endregion

        #region Unit Tests

        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        public static void TestDefaults()
        {
            GridDetailConfiguration gridDetailConfiguration = new GridDetailConfiguration();

            Assert.IsNotNull(gridDetailConfiguration, "GridDetailConfiguration object should have been created.");
            Assert.IsNotNull(gridDetailConfiguration.DashboardLinks, "DashboardLinks set should be valid.");
            Assert.IsNull(gridDetailConfiguration.DashboardGridName, "DashboardGridName should be null.");
            Assert.IsNull(gridDetailConfiguration.RowQuery, "RowQuery should be null.");
            Assert.IsNull(gridDetailConfiguration.DataMemberUsedForFileName,
                          "DataMemberUsedForFileName should be null.");
        }


        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        public static void GetHashCodeTest()
        {
            var gridDetailConfiguration1 = CreateGridDetailConfiguration("TestGridName",
                                                                         "SELECT * FROM TEST",
                                                                         "TestItem",
                                                                         "Test1,Test2");

            var firstCode = gridDetailConfiguration1.GetHashCode();
            Assert.AreEqual(firstCode,
                            gridDetailConfiguration1.GetHashCode(),
                            "GetHashCode should always return the same result for same object.");

            gridDetailConfiguration1.DashboardGridName = "TestGridName2";
            Assert.AreNotEqual(firstCode,
                               gridDetailConfiguration1.GetHashCode(),
                               "GetHashCode should return different value for different DashboardGridName");

            gridDetailConfiguration1.DashboardGridName = "TestGridName";
            gridDetailConfiguration1.DashboardLinks.Remove("Test2");
            Assert.AreNotEqual(firstCode,
                               gridDetailConfiguration1.GetHashCode(),
                               "GetHashCode should return different value for different DashboardLinks");

            gridDetailConfiguration1.DashboardLinks.Add("Test2");
            gridDetailConfiguration1.RowQuery = "SELECT * FROM TEST1";
            Assert.AreNotEqual(firstCode,
                               gridDetailConfiguration1.GetHashCode(),
                               "GetHashCode should return different value for different RowQuery");

            gridDetailConfiguration1.RowQuery = "SELECT * FROM TEST";
            gridDetailConfiguration1.DataMemberUsedForFileName = "TestItem1";
            Assert.AreNotEqual(firstCode,
                               gridDetailConfiguration1.GetHashCode(),
                               "GetHashCode should return different value for different DataMemberUsedForFileName");


            var gridDetailConfiguration2 = CreateGridDetailConfiguration("TestGridName",
                                                                         "SELECT * FROM TEST",
                                                                         "TestItem",
                                                                         "Test1,Test2");
            Assert.AreEqual(firstCode,
                            gridDetailConfiguration2.GetHashCode(),
                            "Different GridDetailConfigurations with same values should be the same.");
        }

        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        [Description(" This is testing the Equals(object) member")]
        public static void EqualsGridObjectTest()
        {
            var gridDetailConfiguration1 = CreateGridDetailConfiguration("TestGridName",
                                                                         "SELECT * FROM TEST",
                                                                         "TestItem",
                                                                         "Test1,Test2");

            Assert.IsFalse(gridDetailConfiguration1.Equals((object)null), "Equals(object) should not be equal to null");

            Assert.IsFalse(gridDetailConfiguration1.Equals("test with string"),
                           "GridDetailConfiguration should not equal a different object.");

            Assert.IsTrue(gridDetailConfiguration1.Equals((object)gridDetailConfiguration1), "Should equal itself");

            var gridDetailConfiguration2 = CreateGridDetailConfiguration("TestGridName",
                                                                         "SELECT * FROM TEST",
                                                                         "TestItem",
                                                                         "Test1,Test2");

            Assert.IsTrue(gridDetailConfiguration1.Equals((object)gridDetailConfiguration2),
                          "Should equal different GridDetailConfiguration object with same values.");

            gridDetailConfiguration2.DashboardGridName = "DifferentName";
            Assert.IsFalse(gridDetailConfiguration1.Equals((object)gridDetailConfiguration2),
                           "Should not be equal if different values");
        }

        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        [Description(" This is testing the Equals(GridDetailConfiguration) member")]
        public static void EqualsGridTest()
        {
            var gridDetailConfiguration1 = CreateGridDetailConfiguration("TestGridName",
                                                                         "SELECT * FROM TEST",
                                                                         "TestItem",
                                                                         "Test1,Test2");

            Assert.IsFalse(gridDetailConfiguration1.Equals(null),
                           "Equals(GridDetailConfiguration) should not be equal to null");

            Assert.IsTrue(gridDetailConfiguration1.Equals(gridDetailConfiguration1), "Should equal itself");

            var gridDetailConfiguration2 = CreateGridDetailConfiguration("TestGridName",
                                                                         "SELECT * FROM TEST",
                                                                         "TestItem",
                                                                         "Test1,Test2");

            Assert.IsTrue(gridDetailConfiguration1.Equals(gridDetailConfiguration2),
                          "Should equal different GridDetailConfiguration object with same values.");

            gridDetailConfiguration2.DashboardGridName = "DifferentName";
            Assert.IsFalse(gridDetailConfiguration1.Equals(gridDetailConfiguration2),
                           "Should not be equal if different values");
        }

        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        [Description(" This is testing the == operator")]
        public static void EqualityOperatorGridTest()
        {
            var gridDetailConfiguration1 = CreateGridDetailConfiguration("TestGridName",
                                                                         "SELECT * FROM TEST",
                                                                         "TestItem",
                                                                         "Test1,Test2");

            Assert.IsFalse(gridDetailConfiguration1 == null, "== should not be equal to null");

            Assert.IsFalse(null == gridDetailConfiguration1, "== should not be equal to null");

            var gridDetailConfiguration2 = gridDetailConfiguration1;

            Assert.IsTrue(gridDetailConfiguration1 == gridDetailConfiguration2, "Should equal itself");

            gridDetailConfiguration2 = CreateGridDetailConfiguration("TestGridName",
                                                                     "SELECT * FROM TEST",
                                                                     "TestItem",
                                                                     "Test1,Test2");

            Assert.IsTrue(gridDetailConfiguration1 == gridDetailConfiguration2,
                          "Should equal different GridDetailConfiguration object with same values.");

            gridDetailConfiguration2.DashboardGridName = "DifferentName";
            Assert.IsFalse(gridDetailConfiguration1 == gridDetailConfiguration2,
                           "Should not be equal if different values");
        }


        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        [Description(" This is testing the != operator ")]
        public static void NotEqualsOperatorGridTest()
        {
            var gridDetailConfiguration1 = CreateGridDetailConfiguration("TestGridName",
                                                                         "SELECT * FROM TEST",
                                                                         "TestItem",
                                                                         "Test1,Test2");

            Assert.IsTrue(gridDetailConfiguration1 != null);

            Assert.IsTrue(null != gridDetailConfiguration1);

            var gridDetailConfiguration2 = gridDetailConfiguration1;

            Assert.IsFalse(gridDetailConfiguration1 != gridDetailConfiguration2, "Should equal itself");

            gridDetailConfiguration2 = CreateGridDetailConfiguration("TestGridName",
                                                                     "SELECT * FROM TEST",
                                                                     "TestItem",
                                                                     "Test1,Test2");

            Assert.IsFalse(gridDetailConfiguration1 != gridDetailConfiguration2,
                           "Should equal different GridDetailConfiguration object with same values.");

            gridDetailConfiguration2.DashboardGridName = "DifferentName";
            Assert.IsTrue(gridDetailConfiguration1 != gridDetailConfiguration2,
                          "Should not be equal if different values");
        }

        #endregion


        #region Helper methods

        private static GridDetailConfiguration CreateGridDetailConfiguration(string dashboardGridName,
                                                                             string rowQuery,
                                                                             string dataMemberUsedForFileName,
                                                                             string gridDetailCsv)
        {
            var gridDetail = new GridDetailConfiguration
            {
                DashboardGridName = dashboardGridName,
                RowQuery = rowQuery,
                DataMemberUsedForFileName = dataMemberUsedForFileName
            };
            var links = gridDetailCsv.Split(',');
            links.ToList().ForEach(v => gridDetail.DashboardLinks.Add(v));

            return gridDetail;
        }
        #endregion
    }
}
