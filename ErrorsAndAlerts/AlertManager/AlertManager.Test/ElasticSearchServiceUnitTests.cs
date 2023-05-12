using AlertManager.Services;
using Extract.ErrorHandling;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class ElasticSearchServiceUnitTests
    {
        //Test indices have 1,000,000 documents
        //Page size in our application is 25
        //Therefore max pages is 1,000,000 / 25 = 40,000
        const int maxPages = 40000;

        //system under test
        ElasticSearchService sut = new ElasticSearchService();

        [SetUp]
        public void Init()
        {

        }

        [Test]
        public void TestConstructor()
        {
            //Purposefully left blank
        }

        [Test]
        [Category("Broken")]
        [Ignore("Test indices store hits as array, production indices do not")]
        public void TestGetAllAlerts_PositiveInput_ShouldBePositiveOrZero() 
        {
            //Is failing due to problems deserializing in ElasticAlertToLocalAlertObject
            //in tests logAlert.Hits.ToString() gives "System.Collections.Generic.List`1[System.Object]" instead of anything usable
            //Would like to use json deserialization, but it seems to break when field names are mismatched from what is expected

            var result0 = sut.GetAllAlerts(0);
            var result1 = sut.GetAllAlerts(1);

            Assert.Multiple(() => 
            {
                Assert.That(result0.Count, Is.GreaterThanOrEqualTo(0));
                Assert.That(result1.Count, Is.GreaterThanOrEqualTo(0));
            });
        }

        [Test]
        public void TestGetAllAlerts_MaxPageInput_ShouldError()
        {
            Assert.Throws<ExtractException>(() =>
            {
                var result = sut.GetAllAlerts(maxPages);
            });
        }

        [Test]
        public void TestGetAllAlerts_NegativeInput_ShouldError()
        {
            Assert.Throws<ExtractException>(() => 
            {
                var result = sut.GetAllAlerts(-1);
            });
        }

        [Test]
        public void TestGetAllEvents_PositiveInput_ShouldBePositiveOrZero()
        {
            var result0 = sut.GetAllEvents(0);
            var result1 = sut.GetAllEvents(1);

            Assert.Multiple(() =>
            {
                Assert.That(result0.Count, Is.GreaterThanOrEqualTo(0));
                Assert.That(result1.Count, Is.GreaterThanOrEqualTo(0));
            });
        }

        [Test]
        public void TestGetAllEvents_MaxPageInput_ShouldError()
        {
            Assert.Throws<ExtractException>(() =>
            {
                var result = sut.GetAllEvents(maxPages);
            });
        }

        [Test]
        public void TestGetAllEvents_NegativeInput_ShouldError()
        {
            Assert.Throws<ExtractException>(() =>
            {
                var result = sut.GetAllEvents(-1);
            });
        }

        [Test]
        public void TestGetUnresolvedAlerts_PositiveInput_ShouldBePositive()
        {
            var result0 = sut.GetUnresolvedAlerts(0);
            var result1 = sut.GetUnresolvedAlerts(1);

            Assert.Multiple(() =>
            {
                Assert.That(result0.Count, Is.GreaterThanOrEqualTo(0));
                Assert.That(result1.Count, Is.GreaterThanOrEqualTo(0));
            });
        }

        [Test]
        public void TestGetEnvInfoWithContextAndEntity_ShouldGetNoMatches()
        {
            var result = sut.GetEnvInfoWithContextAndEntity(DateTime.Now, "fake", "notreal");

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestGetEnvInfoWithContextAndEntity_ShouldGetMatches()
        {
            var result = sut.GetEnvInfoWithContextAndEntity(DateTime.Now, "Machine", "Server 1");

            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void TestGetEventsInTimeframe_ShouldGetMatches()
        {
            var result = sut.GetEventsInTimeframe(new DateTime(2023, 4, 1), DateTime.Now);

            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void TestGetEventsInTimeframe_ShouldGetNoMatches()
        {
            var result = sut.GetEventsInTimeframe(new DateTime(1999, 1, 1), new DateTime(1999, 1, 2));

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestGetEventsInTimeframe_InvalidTimeframe_ShouldError()
        {
            Assert.Throws<ExtractException>(() =>
            {
                var result = sut.GetEventsInTimeframe(DateTime.Now, new DateTime(2023, 4, 1));
            });
        }

        [Test]
        public void TestGetMaxAlertPages_ShouldBe40000()
        {
            var result = sut.GetMaxAlertPages();

            Assert.That(result, Is.EqualTo(maxPages));
        }

        [Test]
        public void TestGetMaxEventPages_ShouldBe40000()
        {
            var result = sut.GetMaxAlertPages();

            Assert.That(result, Is.EqualTo(maxPages));
        }

        [Test]
        [Ignore("Tested function does not work but is not used yet")]
        public void TestTryGetEnvInfoWithDataEntry_ShouldGetResult() 
        {
            var result = sut.TryGetEnvInfoWithDataEntry(DateTime.Now, "CPU %", "81");
            var resultDoc = result.ElementAt(0);
            bool resultDataHasPair = resultDoc.Data.Contains(new KeyValuePair<string, string>("CPU %", "81"));

            Assert.That(resultDataHasPair, Is.EqualTo(true));
        }

        [Test]
        [Ignore("Tested function does not work but is not used yet")]
        public void TestTryGetEnvInfoWithDataEntry_ShouldGetEmpty()
        {
            var result = sut.TryGetEnvInfoWithDataEntry(DateTime.Now, "fake", "not real");

            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}
