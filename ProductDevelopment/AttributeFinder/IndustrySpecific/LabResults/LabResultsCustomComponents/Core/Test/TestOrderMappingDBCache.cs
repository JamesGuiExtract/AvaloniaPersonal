using Extract.Testing.Utilities;
using NUnit.Framework;
using System.IO;
using Extract.Utilities;

namespace Extract.LabResultsCustomComponents.Test
{
    [TestFixture, Category("OrderMappingDBCache"), Category("Automated")]
    public class TestOrderMappingDBCache
    {
        static TestFileManager<TestOrderMappingDBCache> _testFiles;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new();
        }

        [OneTimeTearDown]
        public static void Teardown()
        {
            _testFiles.Dispose();
        }

        [Test]
        public static void TestGoodDatabase()
        {
            string databaseFile = _testFiles.GetFile("Resources.OrderMappingDB.sqlite");

            using OrderMappingDBCache cache = new(new(), databaseFile);

            Assert.Pass();
        }

        [Test]
        public static void TestMissingDatabase()
        {
            string databaseFile = _testFiles.GetFile("Resources.OrderMappingDB.sqlite");
            File.Delete(databaseFile);
            Assume.That(File.Exists(databaseFile), Is.False);

            ExtractException uex = Assert.Throws<ExtractException>(() => new OrderMappingDBCache(new(), databaseFile));

            Assert.AreEqual(databaseFile, uex.Data["Database File"]);
        }

        [Test]
        public static void TestBadDatabase()
        {
            using TemporaryFile tmpFile = new(false);
            File.WriteAllText(tmpFile.FileName, "Not database contents");
            string databaseFile = tmpFile.FileName;

            ExtractException uex = Assert.Throws<ExtractException>(() => new OrderMappingDBCache(new(), databaseFile));

            Assert.AreEqual(databaseFile, uex.Data["Database File"]);
        }
    }
}
