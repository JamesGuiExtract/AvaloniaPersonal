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
        string testCSV1Path, testCSV2Path, testCSVGrantoreePath, testCSVAddressPath;

        [SetUp]
        public void Setup()
        {
            sut = new();
            outputFolder = MakeTempDirectory("EAVWriterTestOutput");
            inputFolder = MakeTempDirectory("EAVWriterTestInput");
            testCSV1Path = Path.Combine(inputFolder, "EAVWriterTest1.csv");
            testCSV2Path = Path.Combine(inputFolder, "EAVWriterTest2.csv");
            testCSVGrantoreePath = Path.Combine(inputFolder, "EAVWriterTestGrantorGrantee.csv");
            testCSVAddressPath = Path.Combine(inputFolder, "EAVWriterTestAddress.csv");
            File.WriteAllText(testCSV1Path, 
                "12345\n" +
                "54321\r" +
                "\r" +
                "999");
            File.WriteAllText(testCSV2Path,
                //true,"woah"
                "true,\"woah\"\n" +
                //false,"dontprint"
                "false,\"dontprint\"\n" +
                //true,"wo\nah\r"
                "true,\"wo\nah\"\n");
            File.WriteAllText(testCSVGrantoreePath,
                "ID, GranteeFirst, GranteeLast, GrantorFirst, GrantorLast\n" +
                "001, Jack, Sprat, John, Doe\n" +
                "001, His, Wife, Jane, Doe\n" +
                "002, Herby, , Kirby, ");
            File.WriteAllText(testCSVAddressPath,
                "ID, Address\n" +
                "001, 123 Fake St\n" +
                "002, None");
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
                    Is.EqualTo("processing started"));
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
                    Is.EqualTo("invalid output folder"));
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
                Assert.That(sut.ProcessNextLine(), Is.EqualTo("att1|12345\n"));
                // should handle \n  
                Assert.That(sut.ProcessNextLine(), Is.EqualTo("att1|54321\n"));
                // should handle \r
                Assert.That(sut.ProcessNextLine(), Is.EqualTo("att1|999\n"));
                // should handle quotation mark delimiters
                Assert.That(sut.ProcessNextLine(), Is.EqualTo("att2|woah\n.att2Child|Ima Child\n"));
                // conditional should fail
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(""));
                // \n should be replaced with \\n when inside string
                Assert.That(sut.ProcessNextLine(), Is.EqualTo("att2|wo\\nah\n.att2Child|Ima Child\n"));
                // end of processing
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(
                    "Grantee|N/A\n" +
                    ".First|GranteeFirst\n" +
                    ".Last|GranteeLast\n" +
                    "Grantor|N/A\n" +
                    ".First|GrantorFirst\n" +
                    ".Last|GrantorLast\n"));
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(
                    "Grantee|N/A\n" +
                    ".First|Jack\n" +
                    ".Last|Sprat\n" +
                    "Grantor|N/A\n" +
                    ".First|John\n" +
                    ".Last|Doe\n"));
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(
                    "Grantee|N/A\n" +
                    ".First|His\n" +
                    ".Last|Wife\n" +
                    "Grantor|N/A\n" +
                    ".First|Jane\n" +
                    ".Last|Doe\n"));
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(
                    "Grantee|N/A\n" +
                    ".First|Herby\n" +
                    "Grantor|N/A\n" +
                    ".First|Kirby\n"));
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(
                    "ReturnAddress|Address\n"));
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(
                    "ReturnAddress|123 Fake St\n"));
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(
                    "ReturnAddress|None\n"));
                //end of processing
                Assert.That(sut.ProcessNextLine(), Is.EqualTo(""));
            });
        }

        [Test]
        public void ProcessNextAttributeTest()
        {
            sut.StartProcessing(GetTestAttList(), outputFolder);

            Assert.Multiple(() =>
            {
                Assert.That(sut.ProcessNextFile(), Is.EqualTo("EAVWriterTest1.csv"));
                Assert.That(sut.ProcessNextFile(), Is.EqualTo("EAVWriterTest2.csv"));
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
                        testCSV1Path, ',', new Guid()),
                    OutputFileName: "%1",
                    IsConditional: false);
            string[] inputs = new string[3] { "one", "two", "three" };

            Assert.That(sut.GetEAVText(attribute, inputs), Is.EqualTo("att1|one"));
        }

        [Test]
        public void FormatNewLineTest()
        {
            string noHits = "there is nothing to replace here";
            string someHits = "there a\re some thi\ngs to replace here";
            string someHitsResult = "there a\\re some thi\\ngs to replace here";

            Assert.Multiple(() =>
            {
                Assert.That(EAVWriter.FormatNewlines(noHits), Is.EqualTo(noHits));
                Assert.That(EAVWriter.FormatNewlines(someHits), Is.EqualTo(someHitsResult));
            });
        }

        [Test]
        public void AttributeHasChildrenTest()
        {
            sut.StartProcessing(GetTestAttList(), outputFolder);

            Assert.Multiple(() =>
            {
                Assert.That(sut.AttributeHasChildren(0), Is.EqualTo(false));
                Assert.That(sut.AttributeHasChildren(1), Is.EqualTo(true));
                Assert.That(sut.AttributeHasChildren(2), Is.EqualTo(false));
            });
        }

        [Test]
        public void EAVWrittenToRightFolderTest()
        {
            sut.StartProcessing(GetTestAttList(), outputFolder);
            sut.ProcessNextLine();

            Assert.That(File.Exists(Path.Combine(outputFolder, "12345.eav")), Is.EqualTo(true));
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
            FileListItem testCSV1 = new(testCSV1Path, ',', new Guid());
            FileListItem testCSV2 = new(testCSV2Path, ',', new Guid());
            FileListItem testCSVGG = new(testCSVGrantoreePath, ',', new Guid());
            FileListItem testCSVRA = new(testCSVAddressPath, ',', new Guid());

            List<AttributeListItem> toReturn = new()
            {
                new AttributeListItem(
                    Name: "att1",
                    Value: "%1",
                    Type: "",
                    File: testCSV1,
                    OutputFileName: "%1",
                    IsConditional: false,
                    ConditionType: null,
                    LeftCondition: null,
                    RightCondition: null),
                new AttributeListItem(
                    Name: "att2",
                    Value: "%2",
                    Type: "",
                    File: testCSV2,
                    OutputFileName: "%1",
                    IsConditional: true,
                    ConditionType: true,
                    LeftCondition: "%1",
                    RightCondition: "true"),
                new AttributeListItem(
                    Name: ".att2Child",
                    Value: "Ima Child",
                    Type: "",
                    File: testCSV2,
                    OutputFileName: "%1",
                    IsConditional: true,
                    ConditionType: true,
                    LeftCondition: "%1",
                    RightCondition: "true"),
                new AttributeListItem(
                    Name: "Grantee",
                    Value: "N/A",
                    Type: "",
                    File: testCSVGG,
                    OutputFileName: "%1",
                    IsConditional: false),
                new AttributeListItem(
                    Name: ".First",
                    Value: "%2",
                    Type: "",
                    File: testCSVGG,
                    OutputFileName: "%1",
                    IsConditional: true,
                    ConditionType: false,
                    LeftCondition: "%2",
                    RightCondition: ""),
                new AttributeListItem(
                    Name: ".Last",
                    Value: "%3",
                    Type: "",
                    File: testCSVGG,
                    OutputFileName: "%1",
                    IsConditional: true,
                    ConditionType: false,
                    LeftCondition: "%3",
                    RightCondition: ""),
                new AttributeListItem(
                    Name: "Grantor",
                    Value: "N/A",
                    Type: "",
                    File: testCSVGG,
                    OutputFileName: "%1",
                    IsConditional: false),
                new AttributeListItem(
                    Name: ".First",
                    Value: "%4",
                    Type: "",
                    File: testCSVGG,
                    OutputFileName: "%1",
                    IsConditional: true,
                    ConditionType: false,
                    LeftCondition: "%4",
                    RightCondition: ""),
                new AttributeListItem(
                    Name: ".Last",
                    Value: "%5",
                    Type: "",
                    File: testCSVGG,
                    OutputFileName: "%1",
                    IsConditional: true,
                    ConditionType: false,
                    LeftCondition: "%5",
                    RightCondition: ""),
                new AttributeListItem(
                    Name: "ReturnAddress",
                    Value: "%2",
                    Type: "",
                    File: testCSVRA,
                    OutputFileName: "%1",
                    IsConditional: true,
                    ConditionType: false,
                    LeftCondition: "%2",
                    RightCondition: "")
            };

            return toReturn;
        }
    }
}
