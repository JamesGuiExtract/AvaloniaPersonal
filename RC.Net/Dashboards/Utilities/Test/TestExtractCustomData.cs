using DevExpress.Utils.Extensions;
using Extract.Dashboard.Forms;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

using static System.FormattableString;

namespace Extract.Dashboard.Utilities.Test
{
    [Category("TestExtractUserData")]
    [TestFixture]
    public class TestExtractCustomData
    {
        #region Constants

        private const string NoUserDataDashboard = "Resources.NoUserDataDashboard.esdx";
        private const string Version1UserDataDashboard = "Resources.Version1UserDataDashboard.esdx";
        private const string Version1UserDataDashboardNoDashboardLinks = "Resources.Version1UserDataDashboardNoDashboardLinks.esdx";
        private const string Version1UserDataDashboardNoFileNameColumnNoDataLinks = "Resources.Version1UserDataDashboardNoFileNameColumnNoDataLinks.esdx";
        private const string Version2UserDataDashboardNotCore = "Resources.Version2UserDataDashboardNotCore.esdx";
        private const string Version2UserDataDashboardCore = "Resources.Version2UserDataDashboardCore.esdx";

        #endregion

        #region Fields
        /// <summary>
        /// Managers test files
        /// </summary>
        private static TestFileManager<TestExtractCustomData> testFileManager;

        #endregion

        #region Overhead

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            testFileManager = new TestFileManager<TestExtractCustomData>();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            testFileManager?.Dispose();
            testFileManager = null;
        }

        #endregion

        #region Unit Tests

        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        public static void TestDefaults()
        {
            ExtractCustomData extractUserData = new ExtractCustomData();
            Assert.IsFalse(extractUserData.CoreLicensed, "Default CoreLicensed should be false");
            Assert.AreEqual(0, extractUserData.CustomGridValues.Count, "Default CustomGridValues should be empty");
        }

        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        public static void GetHashCodeTest()
        {
            ExtractCustomData extractCustomData1 = new ExtractCustomData();

            Assert.AreEqual(extractCustomData1.GetHashCode(),
                            new ExtractCustomData().GetHashCode(),
                            "New default ExtractCustomData objects should have the same hash code.");

            extractCustomData1.CoreLicensed = !extractCustomData1.CoreLicensed;
            Assert.AreNotEqual(extractCustomData1.GetHashCode(),
                               new ExtractCustomData().GetHashCode(),
                               "different ExtractCustomData objects should not have the same hash code.");

            extractCustomData1 = CreateCustomData(true, "Test1,Test2", "FileNameTest");
            var extractCustomData2 = CreateCustomData(true, "Test1,Test2", "FileNameTest");
            Assert.AreEqual(extractCustomData2.GetHashCode(),
                            extractCustomData1.GetHashCode(),
                            "Two ExtractCustomData objects with same data should have the same hash code.");

            extractCustomData2 = CreateCustomData(true, "Test1Diff,Test2Diff", "FileNameTest2");
            Assert.AreNotEqual(extractCustomData2.GetHashCode(),
                               extractCustomData1.GetHashCode(),
                               "Two ExtractCustomData objects with different data should have different hash codes.");
        }

        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        [Description("Test the Equals(object) member")]
        public static void EqualsExtractCustomDataObjectTest()
        {
            var extractCustomData1 = CreateCustomData(true, "Test1,Test2", "FileNameTest");
            var extractCustomData2 = CreateCustomData(true, "Test1,Test2", "FileNameTest");

            Assert.IsFalse(extractCustomData1.Equals((object)null), "Equals(object) should not be equal to null");
            Assert.IsFalse(extractCustomData1.Equals("Test against string"),
                           "ExtractCustomData should not equal another object");

            Assert.IsTrue(extractCustomData1.Equals((object)extractCustomData1), "Should equal itself");

            Assert.IsTrue(extractCustomData1.Equals((object)extractCustomData2),
                          "Should equal different instance that has the same values");

            extractCustomData2 = CreateCustomData(true, "Test1Diff,Test2Diff", "FileNameTest2");
            Assert.IsFalse(extractCustomData1.Equals((object)extractCustomData2),
                           "Should not equal if different values");
        }

        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        [Description("Test the Equals(ExtractCustomData) member")]
        public static void EqualsExtractCustomDataTest()
        {
            var extractCustomData1 = CreateCustomData(true, "Test1,Test2", "FileNameTest");
            var extractCustomData2 = CreateCustomData(true, "Test1,Test2", "FileNameTest");

            Assert.IsFalse(extractCustomData1.Equals(null), "Equals(ExtractCustomData) should not be equal to null");

            Assert.IsTrue(extractCustomData1.Equals(extractCustomData1), "Should equal itself");

            Assert.IsTrue(extractCustomData1.Equals(extractCustomData2),
                          "Should equal different instance that has the same values");

            extractCustomData2 = CreateCustomData(true, "Test1Diff,Test2Diff", "FileNameTest2");
            Assert.IsFalse(extractCustomData1.Equals(extractCustomData2), "Should not equal if different values");
        }


        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        public static void EqualityOperatorExtractCustomDataTest()
        {
            var extractCustomData1 = CreateCustomData(true, "Test1,Test2", "FileNameTest");
            var extractCustomData2 = extractCustomData1;

            Assert.IsFalse(extractCustomData1 == null, "== should not be equal to null");
            Assert.IsFalse(null == extractCustomData1, "== should not be equal to null");

            Assert.IsTrue(extractCustomData1 == extractCustomData2, "Should equal itself");


            extractCustomData2 = CreateCustomData(true, "Test1,Test2", "FileNameTest");
            Assert.IsTrue(extractCustomData1 == extractCustomData2,
                          "Should equal different instance that has the same values");

            extractCustomData2 = CreateCustomData(true, "Test1Diff,Test2Diff", "FileNameTest2");
            Assert.IsFalse(extractCustomData1 == extractCustomData2, "Should not equal if different values");
        }


        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        public static void NotEqualsOperatorExtractCustomDataTest()
        {
            var extractCustomData1 = CreateCustomData(true, "Test1,Test2", "FileNameTest");
            var extractCustomData2 = extractCustomData1;

            Assert.IsTrue(extractCustomData1 != null);
            Assert.IsTrue(null != extractCustomData1);

            Assert.IsFalse(extractCustomData1 != extractCustomData2, "Should equal itself");


            extractCustomData2 = CreateCustomData(true, "Test1,Test2", "FileNameTest");
            Assert.IsFalse(extractCustomData1 != extractCustomData2,
                           "Should equal different instance that has the same values");

            extractCustomData2 = CreateCustomData(true, "Test1Diff,Test2Diff", "FileNameTest2");
            Assert.IsTrue(extractCustomData1 != extractCustomData2, "Should not equal if different values");
        }

        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        public static void TestNoUserData()
        {
            string testFile = testFileManager.GetFile(NoUserDataDashboard);
            XDocument dashboardDefinition = XDocument.Load(testFile);
            ExtractCustomData extractUserData = new ExtractCustomData(dashboardDefinition);

            Assert.IsFalse(extractUserData.CoreLicensed, "CoreLicensed should be false");
            Assert.AreEqual(0, extractUserData.CustomGridValues.Count, "CustomGridValues should be empty");
        }

        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        public static void TestDashboardAddExtractCustomDataToDashboardXmlFromVersion()
        {
            string testFile = testFileManager.GetFile(NoUserDataDashboard);
            XDocument dashboardDefinition = XDocument.Load(testFile);
            ExtractCustomData extractCustomData = new ExtractCustomData();
            extractCustomData.CoreLicensed = true;

            var updatedXml = extractCustomData.AddExtractCustomDataToDashboardXml(dashboardDefinition);
            var dataElement = updatedXml.XPathSelectElement("/Dashboard/UserData");
            Assert.IsNotNull(dataElement, "UserData element should exist.");

            dataElement = updatedXml.XPathSelectElement("/Dashboard/UserData/ExtractCustomData/Version");
            Assert.AreEqual(ExtractCustomData.ExtractUserDataVersion,
                            int.Parse(dataElement.Value, CultureInfo.InvariantCulture),
                            Invariant($"Version should be {ExtractCustomData.ExtractUserDataVersion}"));

            // Test conversion back
            ExtractCustomData fromUpdated = new ExtractCustomData(updatedXml);
            Assert.IsTrue(fromUpdated.CoreLicensed, "CoreLicensed should be true.");
        }


        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        public static void TestVersion1DashboardConvert()
        {
            string testFile = testFileManager.GetFile(Version1UserDataDashboard);
            XDocument dashboardDefinition = XDocument.Load(testFile);
            ExtractCustomData extractCustomData = new ExtractCustomData(dashboardDefinition);
            extractCustomData.CoreLicensed = true;

            var updatedXml = extractCustomData.AddExtractCustomDataToDashboardXml(dashboardDefinition);
            var dataElement = updatedXml.XPathSelectElement("/Dashboard/UserData");
            Assert.IsNotNull(dataElement, "UserData element should exist.");

            dataElement = updatedXml.XPathSelectElement("/Dashboard/UserData/ExtractCustomData/Version");
            Assert.AreEqual(ExtractCustomData.ExtractUserDataVersion,
                            int.Parse(dataElement.Value, CultureInfo.InvariantCulture),
                            Invariant($"Version should be {ExtractCustomData.ExtractUserDataVersion}"));

            ExtractCustomData updatedCustomData = new ExtractCustomData(updatedXml);
            Assert.AreEqual(extractCustomData, updatedCustomData, "Updated custom data should be the same as added.");
        }


        [Test]
        [Category("Automated")]
        [Category("Dashboard")]
        [CLSCompliant(false)]
        [TestCase(false, "File Details", Version1UserDataDashboard, 1, "FileNameTest", TestName = "Version1 All settings available")]
        [TestCase(false,"", Version1UserDataDashboardNoDashboardLinks, 1, "FileNameTest", TestName = "Version1 No Dashboard links")]
        [TestCase(false, "", Version1UserDataDashboardNoFileNameColumnNoDataLinks, 1, "FileName", TestName = "Version 1 No filename or link")]
        [TestCase(true, "File Details", Version2UserDataDashboardCore, 2, "FileNameTest", TestName = "Core licensed")]
        [TestCase(false, "File Details", Version2UserDataDashboardNotCore, 2, "FileNameTest", TestName = "Not core licensed")]
        public static void TestDashboardCore(bool coreLicenseValue,
                                             string dashboardLinks,
                                             string testFileName,
                                             int versionTested,
                                             string fileNameMember)
        {
            string testFile = testFileManager.GetFile(testFileName);
            XDocument dashboardDefinition = XDocument.Load(testFile);
            ExtractCustomData extractCustomData = new ExtractCustomData(dashboardDefinition);

            var expectedCustomData = CreateCustomData(coreLicenseValue, dashboardLinks, fileNameMember);

            Assert.AreEqual(expectedCustomData, extractCustomData, "Custom data should match expected.");

            // Test that the version in the XML is the same as the version of the class
            var version = dashboardDefinition.XPathSelectElement("/Dashboard/UserData/ExtractCustomData/Version");
            if (versionTested == 1)
            {
                Assert.IsNull(version, "Version should be null");
            }
            else
            {
                Assert.IsNotNull(version, "Version should not be null");

                Assert.AreEqual(versionTested,
                                int.Parse(version.Value, CultureInfo.InvariantCulture),
                                Invariant($"Version should be {ExtractCustomData.ExtractUserDataVersion}"));
            }
        }
        #endregion

        #region Helper methods

        private static ExtractCustomData CreateCustomData(bool coreLicenseValue,
                                                          string dashboardLinks,
                                                          string fileNameMember)
        {
            var customData = new ExtractCustomData { CoreLicensed = coreLicenseValue };
            customData.CustomGridValues
                      .Add("gridDashboardItem1",
                           new GridDetailConfiguration
                {
                    DashboardGridName = "gridDashboardItem1",
                    RowQuery = "SELECT @_FileID = ID FROM FAMFile WHERE FileName = @FileName",
                    DataMemberUsedForFileName = fileNameMember
                });

            var links = dashboardLinks?.Split(',')?.Where(s => !string.IsNullOrWhiteSpace(s));
            if (links != null && links.Count() > 0)
            {
                customData.CustomGridValues["gridDashboardItem1"].DashboardLinks.AddRange(links);
            }
            return customData;
        }
        #endregion
    }
}
