using IndexConverterV2.Models;
using IndexConverterV2.Services;
using Microsoft.VisualBasic.FileIO;
using NUnit.Framework;

namespace IndexConverterV2.Tests
{
    [TestFixture]
    public class EAVWriterTests
    {
        //System Under Test
        EAVWriter sut;

        string outputFolder, inputFolder;
        string testCSV1, testCSV2;

        [SetUp]
        public void Setup()
        {
            sut = new();
            outputFolder = MakeTempDirectory("EAVWriterTestOutput");
            inputFolder = MakeTempDirectory("EAVWriterTestInput");
            testCSV1 = Path.Combine(inputFolder, "EAVWriterTest1.csv");
            testCSV2 = Path.Combine(inputFolder, "EAVWriterTest2.csv");
            File.WriteAllText(testCSV1, "12345\n54321\r\r999");
            File.WriteAllText(testCSV2, 
                //true,"woah"
                "true,\"woah\"\n" +
                //false,"dontprint"
                "false,\"dontprint\"\n" +
                //true,"wo\nah\r"
                "true,\"wo\nah\"\n");
        }

        [TearDown]
        public void Teardown()
        {
            sut.StopProcessing();
            Directory.Delete(outputFolder, true);
            Directory.Delete(inputFolder, true);
        }

        [Test]
        public void StartProcessingTest()
        {
            Assert.Multiple(() => 
            {
                Assert.That(
                    sut.StartProcessing(GetTestAttList(), outputFolder), 
                    Is.EqualTo(true));
                Assert.That(sut.Processing, Is.EqualTo(true));
            });
        }

        [Test]
        public void StartProcessingTestBadOutputFolder()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    sut.StartProcessing(GetTestAttList(), "this isn't a proper path"), 
                    Is.EqualTo(false));
                Assert.That(sut.Processing, Is.EqualTo(false));
            });
        }

        [Test]
        public void StopProcessingTest()
        {
            sut.StopProcessing();

            Assert.That(sut.Processing, Is.EqualTo(false));
        }

        [Test]
        public void ProcessNextLineTest()
        {
            sut.StartProcessing(GetTestAttList(), outputFolder);
            
            Assert.Multiple(() => 
            {
                // should get first line
                Assert.That(sut.ProcessNextLine(), Is.EqualTo("att1|12345|"));
                // should handle \n  
                Assert.That(sut.ProcessNextLine(), Is.EqualTo("att1|54321|"));
                // should handle \r
                Assert.That(sut.ProcessNextLine(), Is.EqualTo("att1|999|"));
                // should handle quotation mark delimiters
                Assert.That(sut.ProcessNextLine(), Is.EqualTo("att2|woah|"));
                // conditional should fail
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(""));
                // \n should be replaced with \\n when inside string
                Assert.That(sut.ProcessNextLine(), Is.EqualTo("att2|wo\\nah|"));
                // end of processing
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(""));
            });
        }

        [Test]
        public void ProcessNextAttributeTest()
        {
            sut.StartProcessing(GetTestAttList(), outputFolder);

            Assert.Multiple(() => 
            {
                Assert.That(sut.ProcessNextAttribute(), Is.EqualTo("att1"));
                Assert.That(sut.ProcessNextAttribute(), Is.EqualTo("att2"));
            });
        }

        [Test]
        public void ProcessAllTest()
        {
            sut.StartProcessing(GetTestAttList(), outputFolder);

            Assert.That(sut.ProcessAll(), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(AttributeConditionCases))]
        public void AttributeConditionShouldPrintTest(
            AttributeListItem testAtt, 
            string[] inputs,
            bool result) 
        {
            Assert.That(
                sut.AttributeConditionShouldPrint(testAtt, inputs), 
                Is.EqualTo(result));
        }

        public static IEnumerable<object[]> AttributeConditionCases()
        {
            yield return new object[] {
                new AttributeListItem(
                   Name: "testAtt",
                   Value: "%1",
                   Type: "",
                   File: new FileListItem("", ',', new Guid()),
                   OutputFileName: "%1",
                   IsConditional: true,
                   ConditionType: true,
                   LeftCondition: "%1",
                   RightCondition: "12345"),
                new string[] { "12345", "54321", "999"},
                true
            };

            yield return new object[] {
                new AttributeListItem(
                   Name: "testAtt",
                   Value: "%1",
                   Type: "",
                   File: new FileListItem("", ',', new Guid()),
                   OutputFileName: "%1",
                   IsConditional: true,
                   ConditionType: true,
                   LeftCondition: "%1",
                   RightCondition: "12345"),
                new string[] { "54321", "12345", "999"},
                false
            };

            yield return new object[] {
                new AttributeListItem(
                   Name: "testAtt",
                   Value: "%1",
                   Type: "",
                   File: new FileListItem("", ',', new Guid()),
                   OutputFileName: "%1",
                   IsConditional: false),
                new string[] { "54321", "12345", "999"},
                true
            };
        }

        [Test]
        public void ReplacePercentsTest()
        {
            string[] inputs = new string[3];
            inputs[0] = "doesn't matter";
            inputs[1] = "please ignore";
            inputs[2] = "GET THIS ONE";

            Assert.Multiple(() =>
            {
                Assert.That(sut.ReplacePercents("%3", inputs), 
                    Is.EqualTo("GET THIS ONE"));
                Assert.That(sut.ReplacePercents("123%3after", inputs), 
                    Is.EqualTo("123GET THIS ONEafter"));
                Assert.That(sut.ReplacePercents("%3blah%3", inputs), 
                    Is.EqualTo("GET THIS ONEblahGET THIS ONE"));
            });
        }

        [Test]
        public void GetEAVTextTest() 
        {
            AttributeListItem attribute = new(
                    Name: "att1",
                    Value: "%1",
                    Type: "",
                    File: new FileListItem(
                        testCSV1, ',', new Guid()),
                    OutputFileName: "%1",
                    IsConditional: false);
            string[] inputs = new string[3] { "one", "two", "three"};

            Assert.That(sut.GetEAVText(attribute, inputs), Is.EqualTo("att1|one|"));
        }

        [Test]
        public void FormatNewLineTest()
        {
            string noHits = "there is nothing to replace here";
            string someHits = "there a\re some thi\ngs to replace here";
            string someHitsResult = "there a\\re some thi\\ngs to replace here";

            Assert.Multiple(() =>
            {
                Assert.That(EAVWriter.FormatNewline(noHits), Is.EqualTo(noHits));
                Assert.That(EAVWriter.FormatNewline(someHits), Is.EqualTo(someHitsResult));
            });
        }
        


        private static string MakeTempDirectory(string directoryName)
        {
            string? directoryPath = 
                Path.GetDirectoryName(
                    Path.Combine(
                        Path.GetTempPath(), 
                        Guid.NewGuid().ToString(), 
                        directoryName));

            if (directoryPath is null)
                throw new Exception("Invalid directory");
            else
                System.IO.Directory.CreateDirectory(directoryPath);

            return directoryPath;
        }

        private List<AttributeListItem> GetTestAttList() 
        {
            List<AttributeListItem> toReturn = new()
            {
                new AttributeListItem(
                    Name: "att1",
                    Value: "%1",
                    Type: "",
                    File: new FileListItem(
                        testCSV1, ',', new Guid()),
                    OutputFileName: "%1",
                    IsConditional: false,
                    ConditionType: null,
                    LeftCondition: null,
                    RightCondition: null),
                new AttributeListItem(
                    Name: "att2",
                    Value: "%2",
                    Type: "",
                    File: new FileListItem(
                        testCSV2, ',', new Guid()),
                    OutputFileName: "%1",
                    IsConditional: true,
                    ConditionType: true,
                    LeftCondition: "%1",
                    RightCondition: "true")
            };

            return toReturn;
        }
    }
}
