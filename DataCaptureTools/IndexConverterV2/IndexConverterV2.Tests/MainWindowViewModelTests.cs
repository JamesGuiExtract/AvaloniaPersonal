﻿using IndexConverterV2.Models;
using IndexConverterV2.ViewModels;
using IndexConverterV2.Views;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace IndexConverterV2.Tests
{
    [TestFixture]
    public class MainWindowViewModelTests
    {
        //System Under Test
        MainWindowViewModel sut;

        [SetUp]
        public void Init() 
        {
            var viewMock = new Mock<IView>();
            var dialogMock = new Mock<IDialogService>();

            sut = new MainWindowViewModel(viewMock.Object, dialogMock.Object);
        }

        [Test]
        public void AddFileTest() 
        {
            AddSampleFile("inputfile1");

            ObservableCollection<FileListItem> testList = new()
            {
                new FileListItem("inputfile1", ',', '"')
            };

            Assert.That(sut.InputFiles, Is.EqualTo(testList));
        }

        [Test]
        public void RemoveFileTest()
        {
            AddSampleAttribute("inputfile1", "attribute1");

            ObservableCollection<FileListItem> emptyFileList = new();
            ObservableCollection<AttributeListItem> emptyAttributeList = new();

            sut.FileListSelectedIndex = 0;
            sut.RemoveFile();

            Assert.Multiple(() => 
            {
                Assert.That(sut.InputFiles, Is.EqualTo(emptyFileList));
                Assert.That(sut.Attributes, Is.EqualTo(emptyAttributeList));
            });
        }

        [Test]
        public void AddAttributeTest() 
        {
            AddSampleAttribute("inputfile1", "attribute1");

            FileListItem file = new("inputfile1", ',', '"');

            ObservableCollection<AttributeListItem> testList = new()
            {
                new AttributeListItem("attribute1", "val", "", file, "%1", false, null, null, null)
            };

            Assert.That(sut.Attributes, Is.EqualTo(testList));
        }

        [Test]
        public void RemoveAttributeTest() 
        {
            AddSampleAttribute("inputfile1", "attribute1");
            ObservableCollection<AttributeListItem> testList = new();
            sut.AttributeListSelectedIndex = 0;
            sut.RemoveAttribute();

            Assert.That(sut.Attributes, Is.EqualTo(testList));
        }

        [Test]
        public void MoveAttributesTest() 
        {
            AddSampleAttribute("inputfile1", "attribute1");
            AddSampleAttribute("inputfile2", "attribute2");

            ObservableCollection<AttributeListItem> testListInc = new()
            {
                new AttributeListItem(
                    Name: "attribute1",
                    Value: "val",
                    Type: "",
                    File: new FileListItem("inputfile1", ',', '"'),
                    OutputFileName: "%1",
                    IsConditional: false),

                new AttributeListItem(
                    Name: "attribute2",
                    Value: "val",
                    Type: "",
                    File: new FileListItem("inputfile2", ',', '"'),
                    OutputFileName: "%1",
                    IsConditional: false)
            };

            ObservableCollection<AttributeListItem> testListDec = new()
            {
                new AttributeListItem(
                    Name: "attribute2",
                    Value: "val",
                    Type: "",
                    File: new FileListItem("inputfile2", ',', '"'),
                    OutputFileName: "%1",
                    IsConditional: false),

                new AttributeListItem(
                    Name: "attribute1",
                    Value: "val",
                    Type: "",
                    File: new FileListItem("inputfile1", ',', '"'),
                    OutputFileName: "%1",
                    IsConditional: false)
            };

            Assert.Multiple(() => 
            {
                //collection is the same upon creation
                Assert.That(sut.Attributes, Is.EqualTo(testListInc));

                sut.AttributeListSelectedIndex = 0;
                sut.MoveAttributeUp(); //Should not be able to move index 0 up
                Assert.That(sut.Attributes, Is.EqualTo(testListInc));

                sut.AttributeListSelectedIndex = 1;
                sut.MoveAttributeDown(); //Should not be able to move last index down
                Assert.That(sut.Attributes, Is.EqualTo(testListInc));

                sut.MoveAttributeUp(); //sut Attributes should now be descending order
                Assert.That(sut.Attributes, Is.EqualTo(testListDec));

                sut.AttributeListSelectedIndex = 0;
                sut.MoveAttributeDown(); //sut Attributes should now be back to ascending order
                Assert.That(sut.Attributes, Is.EqualTo(testListInc));
            });
        }

        [Test]
        public void LoadTest()
        {
            //Arranging a sample model for comparison to
            MainWindowModel sampleModel = new(
                InputFiles: new List<FileListItem>()
                {
                    new FileListItem("File1", ',', '"'),
                    new FileListItem("File2", ',', '"')
                },
                Attributes: new List<AttributeListItem>()
                {
                    new AttributeListItem(
                        Name: "Att1",
                        Value: "Val1",
                        Type: "Type1",
                        File: new FileListItem("File1", ',', '"'),
                        OutputFileName: "%1",
                        IsConditional: false,
                        ConditionType: null,
                        LeftCondition: null,
                        RightCondition: null
                    ),
                    new AttributeListItem(
                        Name: "Att2",
                        Value: "Val2",
                        Type: "",
                        File: new FileListItem("File2", ',', '"'),
                        OutputFileName: "%1",
                        IsConditional: true,
                        ConditionType: true,
                        LeftCondition: "%3",
                        RightCondition: "test"
                    )
                },
                OutputFolder: "test"
            ); 

            string loadTestFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "LoadTest.txt");
            string? loadTestFileDirectory = Path.GetDirectoryName(loadTestFilePath);
            if (loadTestFileDirectory is null)
                throw new Exception("Invalid directory");

            try
            {
                //Serializing sample model to use for loading
                System.IO.Directory.CreateDirectory(loadTestFileDirectory);
                File.WriteAllText(loadTestFilePath, JsonSerializer.Serialize(sampleModel));

                //Loading the serialized sample model
                sut.LoadConfig(loadTestFilePath);

                Assert.Multiple(() => 
                {
                    Assert.That(sut.InputFiles, Is.EqualTo(sampleModel.InputFiles));
                    Assert.That(sut.Attributes, Is.EqualTo(sampleModel.Attributes));
                });
            }
            finally 
            {
                Directory.Delete(loadTestFileDirectory, true);  
            }
        }

        [Test]
        public void SaveTest()
        {
            string tempPath = Path.GetTempPath() + "SaveTest.txt";
            AddSampleAttribute("file1", "attribute1");
            AddSampleAttribute("file2", "attribute2");
            sut.SaveConfig(tempPath);

            StreamReader sr = new(tempPath);
            string actualJson = sr.ReadToEnd();
            sr.Close();
            string expectedJson = JsonSerializer.Serialize(new MainWindowModel(sut.InputFiles, sut.Attributes, sut.OutputFolder));

            try 
            {
                Assert.That(actualJson, Is.EqualTo(expectedJson));
            }
            finally 
            {
                File.Delete(tempPath);
            }
        }

        private void AddSampleFile(string fileName) 
        {
            sut.InputFileName = fileName;
            sut.Delimiter = ",";
            sut.Qualifier = "\"";
            sut.AddFile();
        }

        private void AddSampleAttribute(string fileName, string attributeName)
        {
            AddSampleFile(fileName);
            sut.AttributeName = attributeName;
            sut.AttributeValue = "val";
            sut.AttributeFileSelectedIndex = sut.InputFiles.Count - 1;
            sut.AttributeOutputFileName = "%1";
            sut.AddAttribute();
        }
    }
}