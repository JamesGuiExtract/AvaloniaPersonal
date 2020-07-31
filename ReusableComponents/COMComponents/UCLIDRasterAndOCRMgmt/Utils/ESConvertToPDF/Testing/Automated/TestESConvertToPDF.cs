using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using System.Linq;

namespace ESConvertToPDF.Test
{
    /// <summary>
    /// Class for testing the ESConvertToPDF utility application
    /// </summary>
    [TestFixture]
    [Category("ESConvertToPDF")]
    public class TestESConvertToPDF
    {
        static string _ESConvertToPDF = Path.Combine(FileSystemMethods.CommonComponentsPath, "ESConvertToPDF.exe");
        static readonly string _ANNOTATED_PDF = "Resources.Annotated.pdf";
        static readonly string _TESTIMAGE001_TIF = "Resources.TestImage001.tif";

        static TestFileManager<TestESConvertToPDF> _testFiles;

        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestESConvertToPDF>();
        }

        /// <summary>
        /// Performs tear down needed after entire test run.
        /// </summary>
        [OneTimeTearDown]
        public static void Teardown()
        {
            _testFiles?.Dispose();
            _testFiles = null;
        }

        /// <summary>
        /// Tests whether or not the enumerator returns the same lines as the collection.
        /// </summary>
        [Test, Category("Automated")]
        public static void TestAddTextToTifSource()
        {
            string testImage = _testFiles.GetFile(_TESTIMAGE001_TIF);
            using (var outputFile = new TemporaryFile(".searchable.pdf", false))
            {
                string output = outputFile.FileName;

                File.Delete(output);
                SystemMethods.RunExecutable(_ESConvertToPDF, new[] { testImage, output }, createNoWindow: true);

                Assert.IsTrue(File.Exists(output));

                using (var pdfDocument = PdfReader.Open(output))
                {
                    Assert.AreEqual(1, pdfDocument.PageCount);

                    var text = pdfDocument.GetPageText(1);
                    Assert.IsTrue(text.Contains("999-11-5555"));

                    pdfDocument.Close();
                }
            }
        }

        /// <summary>
        /// Tests that bookmarks, annotations are not lost when adding document text
        /// https://extract.atlassian.net/browse/ISSUE-11940
        /// https://extract.atlassian.net/browse/ISSUE-17138
        /// </summary>
        [Test, Category("Automated")]
        public static void TestPreservePDFMetadata()
        {
            string testImage = _testFiles.GetFile(_ANNOTATED_PDF);
            using (var outputFile = new TemporaryFile(".searchable.pdf", false))
            {
                string output = outputFile.FileName;
                File.Delete(output);

                SystemMethods.RunExecutable(_ESConvertToPDF, new[] { testImage, output }, createNoWindow: true);

                Assert.IsTrue(File.Exists(output));

                using (var pdfOutput = PdfReader.Open(output))
                {
                    Assert.AreEqual(4, pdfOutput.PageCount);

                    var pageText = Enumerable.Range(1, pdfOutput.PageCount)
                        .ToDictionary(p => p, p => pdfOutput.GetPageText(p));
                    Assert.AreEqual(4, pageText.Count);
                    Assert.IsTrue(pageText[1].Contains("DOE, JOHN"));
                    Assert.IsTrue(pageText[2].Contains("BRADENTON"));
                    Assert.IsTrue(pageText[3].Contains("JANE DOE"));
                    Assert.IsTrue(pageText[4].Contains("1992"));

                    // Ensure there are annotations
                    var annotationCount = Enumerable.Range(0, pdfOutput.PageCount)
                        .Sum(p => pdfOutput.Pages[p].Annotations.Count);
                    Assert.Greater(annotationCount, 0);

                    // Ensure there are bookmarks
                    var outlineCollection = ((PdfDictionary)pdfOutput.Internals.Catalog.Elements.GetObject("/Outlines"));
                    var outlineCount = outlineCollection.Elements.Count;
                    Assert.Greater(outlineCount, 0);

                    pdfOutput.Close();
                }
            }
        }
    }
}
