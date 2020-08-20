using Extract.Testing.Utilities;
using NUnit.Framework;
using System.IO;

namespace Extract.FileActionManager.FileProcessors.Test
{
    [TestFixture]
    [Category("TransformXmlTask")]
    public class TestTransformXmlTask
    {
        const string _INPUT = "Resources.TransformXmlTask.FullText.xml";
        const string _DESCENDING_SORT_STYLESHEET = "Resources.TransformXmlTask.alphaSortDescendingTransform.xslt";
        const string _DEFAULT_SORT_EXPECTED = "Resources.TransformXmlTask.FullText.sorted.xml";
        const string _DESCENDING_SORT_EXPECTED ="Resources.TransformXmlTask.FullText.sortedDescending.xml";

        static TestFileManager<TestTransformXmlTask> _testFiles;

        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestTransformXmlTask>("56BE3FF6-1EDC-4F64-9FFD-B9A29EF822D3");
        }

        /// <summary>
        /// Performs tear down needed after entire test run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            _testFiles?.Dispose();
        }

        /// <summary>
        /// Test default options
        /// </summary>
        [Test, Category("Automated")]
        public static void DefaultOptions()
        {
            try
            {
                var task = new TransformXmlTask();
                Assert.AreEqual("<SourceDocName>.xml", task.InputPath);
                Assert.AreEqual(null, task.StyleSheet);
                Assert.AreEqual("<SourceDocName>.xml", task.OutputPath);
                Assert.IsFalse(task.UseSpecifiedStyleSheet);

                var inputPath = _testFiles.GetFile(_INPUT);
                var sourceDocName = Path.ChangeExtension(inputPath, null);
                var outputPath = inputPath;
                var x = GetResult(sourceDocName, task, _DEFAULT_SORT_EXPECTED, outputPath);

                Assert.AreEqual(x.Expected, x.Output);
            }
            finally
            {
                // Remove input file so as not to pollute the other tests
                _testFiles.RemoveFile(_INPUT);
            }
        }

        /// <summary>
        /// Test specified stylesheet
        /// </summary>
        [Test, Category("Automated")]
        public static void SpecifiedStyleSheet()
        {
            try
            {
                var styleSheet = _testFiles.GetFile(_DESCENDING_SORT_STYLESHEET);
                var task = new TransformXmlTask
                {
                    StyleSheet = styleSheet,
                    UseSpecifiedStyleSheet = true
                };

                var inputPath = _testFiles.GetFile(_INPUT);
                var sourceDocName = Path.ChangeExtension(inputPath, null);
                var outputPath = inputPath;
                var x = GetResult(sourceDocName, task, _DESCENDING_SORT_EXPECTED, outputPath);

                Assert.AreEqual(x.Expected, x.Output);
            }
            finally
            {
                // Remove input file so as not to pollute the other tests
                _testFiles.RemoveFile(_INPUT);
            }
        }

        /// <summary>
        /// Test alternate input/output paths that use path tags and require folder creation
        /// </summary>
        [Test, Category("Automated")]
        public static void PathTagInputOutput()
        {
            string inputFolder = null;
            string outputPath = null;
            try
            {
                var styleSheet = _testFiles.GetFile(_DESCENDING_SORT_STYLESHEET);
                var task = new TransformXmlTask
                {
                    InputPath = @"$DirOf(<SourceDocName>)\InputXML\$FileOf(<SourceDocName>).xml",
                    OutputPath = @"$DirOf(<SourceDocName>)\OutputXML\$FileOf(<SourceDocName>).xml",
                };
                var inputPath = _testFiles.GetFile(_INPUT);
                var sourceDocName = Path.ChangeExtension(inputPath, null);

                // Move the input XML into a subfolder
                var newInputPath = Path.Combine(Path.GetDirectoryName(inputPath), "InputXML", Path.GetFileName(inputPath));
                inputFolder = Path.GetDirectoryName(newInputPath);
                Directory.CreateDirectory(inputFolder);
                if (File.Exists(newInputPath))
                {
                    File.Delete(newInputPath);
                }
                File.Move(inputPath, newInputPath);
                inputPath = newInputPath;

                // Setup the output path but don't create the folder
                outputPath = inputPath.Replace(@"\InputXML\", @"\OutputXML\");
                Assert.IsFalse(Directory.Exists(outputPath));

                var x = GetResult(sourceDocName, task, _DEFAULT_SORT_EXPECTED, outputPath);

                Assert.AreEqual(x.Expected, x.Output);
            }
            finally
            {
                // Remove input file so as not to pollute the other tests
                _testFiles.RemoveFile(_INPUT);

                if (inputFolder != null)
                {
                    Directory.Delete(inputFolder, true);
                }
                if (outputPath != null)
                {
                    var outputDir = Path.GetDirectoryName(outputPath);
                    if (Directory.Exists(outputDir))
                    {
                        Directory.Delete(outputDir, true);
                    }
                }
            }
        }

        static TestOutput GetResult(
            string sourceDocName,
            TransformXmlTask task,
            string expectedResource,
            string outputPath)
        {
            var expectedPath = _testFiles.GetFile(expectedResource);
            var expected = File.ReadAllText(expectedPath);

            task.Execute(sourceDocName);
            var output = File.ReadAllText(outputPath);

            return new TestOutput(expected, output);
        }

        class TestOutput
        {
            public string Expected { get; }
            public string Output { get; }
            public TestOutput(string expected, string actual)
            {
                Expected = expected;
                Output = actual;
            }
        }

        /// <summary>
        /// Tests to see whether or not the specified <see paramref="sourceDocName"/> fails
        /// validation based on the configuration of <see paramref="validateTask"/>.
        /// </summary>
        /// <param name="validateTask">The <see cref="TransformXmlTask"/> to use to validate
        /// <see paramref="sourceDocName"/>.</param>
        /// <param name="sourceDocName">The filename <see paramref="validateTask"/> should be run
        /// against.</param>
        /// <param name="expectedFailureELI">An ELI code expected to be in the exception stack the
        /// test will fail with or <see langword="null"/> if the test is expected to succeed.
        /// </param>
        /// <returns><see langword="true"/> if the execution of <see paramref="validateTask"/>
        /// failed/succeeded as expected.</returns>
        static bool TestTaskForFailure(TransformXmlTask validateTask, string sourceDocName,
            string expectedFailureELI)
        {
            try
            {
                validateTask.Execute(sourceDocName);
            }
            catch (ExtractException ee)
            {
                if (string.IsNullOrWhiteSpace(expectedFailureELI))
                {
                    return false;
                }
                else
                {
                    for (ExtractException eeIterator = ee; eeIterator != null;
                        eeIterator = eeIterator.InnerException as ExtractException)
                    {
                        if (eeIterator.EliCode == expectedFailureELI)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(expectedFailureELI);
        }

    }
}
