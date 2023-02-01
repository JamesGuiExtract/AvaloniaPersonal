using Extract.Imaging.Utilities;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Extract.Testing.Utilities.ImageUtils;

namespace ESConvertToPDF.Test
{
    /// <summary>
    /// Class for testing the ESConvertToPDF utility application
    /// </summary>
    [TestFixture]
    [Category("ESConvertToPDF")]
    public class TestESConvertToPDF
    {
        static readonly string _ESConvertToPDF = Path.Combine(FileSystemMethods.CommonComponentsPath, "ESConvertToPDF.exe");
        static readonly string _IMAGE_FORMAT_CONVERTER_PATH = Path.Combine(FileSystemMethods.CommonComponentsPath, "ImageFormatConverter.exe");
        const string _ANNOTATED_PDF = "Resources.Annotated.pdf";
        const string _TESTIMAGE001_TIF = "Resources.TestImage001.tif";
        const string _TEXT_BASED_PDF = "Resources.plagiarizers_paper.pdf";
        const string _EXAMPLE05_TIF = "Resources.Example05.tif";
        const string _EXAMPLE05_WITH_ROTATED_PAGE_TIF = "Resources.Example05_WithRotatedPage.tif";
        const string _EXAMPLE05_WITH_ROTATED_PAGE_PDF = "Resources.Example05_WithRotatedPage.pdf";
        const string _TALL_IMAGE_PDF = "Resources.TallImage.pdf";
        const string _WIDE_IMAGE_PDF = "Resources.WideImage.pdf";


        static TestFileManager<TestESConvertToPDF> _testFiles;

        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestESConvertToPDF>();
            UnlockLeadtools.UnlockPdfSupport(false);
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
        /// Tests whether there is searchable text added to a TIF
        /// </summary>
        [Test, Category("Automated")]
        public static void TextTest_AddTextToTifSource()
        {
            string testImage = _testFiles.GetFile(_TESTIMAGE001_TIF);
            using var outputFile = new TemporaryFile(".searchable.pdf", false);
            string output = outputFile.FileName;

            File.Delete(output);
            SystemMethods.RunExecutable(_ESConvertToPDF, new[] { testImage, output }, createNoWindow: true);

            Assert.IsTrue(File.Exists(output));

            using var pdfDocument = PdfReader.Open(output);
            Assert.AreEqual(1, pdfDocument.PageCount);

            var text = pdfDocument.GetPageText(1);
            StringAssert.Contains("999-11-5555", text);
        }

        /// <summary>
        /// Tests that bookmarks, annotations are not lost when adding document text
        /// https://extract.atlassian.net/browse/ISSUE-11940
        /// https://extract.atlassian.net/browse/ISSUE-17138
        /// </summary>
        [Test, Category("Automated")]
        public static void MetadataTest_PreservePDFMetadata()
        {
            string testImage = _testFiles.GetFile(_ANNOTATED_PDF);
            using var outputFile = new TemporaryFile(".searchable.pdf", false);
            string output = outputFile.FileName;
            File.Delete(output);

            SystemMethods.RunExecutable(_ESConvertToPDF, new[] { testImage, output }, createNoWindow: true);

            Assert.IsTrue(File.Exists(output));

            using var pdfOutput = PdfReader.Open(output);
            Assert.AreEqual(4, pdfOutput.PageCount);

            var pageText = Enumerable.Range(1, pdfOutput.PageCount)
                .ToDictionary(p => p, p => pdfOutput.GetPageText(p));
            Assert.AreEqual(4, pageText.Count);
            StringAssert.Contains("DOE, JOHN", pageText[1]);
            StringAssert.Contains("BRADENTON", pageText[2]);
            StringAssert.Contains("JANE DOE", pageText[3]);
            StringAssert.Contains("1992", pageText[4]);

            // Ensure there are annotations
            var annotationCount = Enumerable.Range(0, pdfOutput.PageCount)
                .Sum(p => pdfOutput.Pages[p].Annotations.Count);
            Assert.Greater(annotationCount, 0);

            // Ensure there are bookmarks
            var outlineCollection = ((PdfDictionary)pdfOutput.Internals.Catalog.Elements.GetObject("/Outlines"));
            var outlineCount = outlineCollection.Elements.Count;
            Assert.Greater(outlineCount, 0);
        }

        /// <summary>
        /// Check that image comparison algorithm is valid
        /// Comparing identical documents where one has a rotated page will result in a large error value
        /// </summary>
        [Test, Category("Automated")]
        public static void ImageTest_ComparisonSanity()
        {
            using var nonRotated = new TIF(_testFiles.GetFile(_EXAMPLE05_TIF));
            using var rotated = new TIF(_testFiles.GetFile(_EXAMPLE05_WITH_ROTATED_PAGE_TIF));

            double errors = ComparePagesAsImages(nonRotated, rotated);
            Assert.Greater(errors, 1E+6);
        }

        /// <summary>
        /// Check that image comparison algorithm is sensitive
        /// Converting from TIF to PDF with default nuance settings will result in a small but significant error value
        /// </summary>
        [Test, Category("Automated")]
        public static void ImageTest_ComparisonCalibration()
        {
            using var pair = ConvertTifResourceToPdf(_EXAMPLE05_WITH_ROTATED_PAGE_TIF);

            double errors = ComparePagesAsImages(pair.Input, pair.Output);
            Assert.Greater(errors, 0.1, "Less is probably OK. When test was written errors = 0.10149283330365146");
            Assert.Less(errors, 0.11);
        }

        /// <summary>
        /// Compare the original tif images to the output pdf images
        /// </summary>
        [Test, Category("Automated")]
        public static void ImageTest_TifToPdf()
        {
            using var pair = ConvertTifResourceToSearchable(_TESTIMAGE001_TIF);

            // Check that the output rendered as images is very similar to the original
            double errors = ComparePagesAsImages(pair.Input, pair.Output);
            Assert.Less(errors, 0.06); // Conversion is lossy but not bad
        }

        /// <summary>
        /// Compare the original pdf images to the output pdf images
        /// </summary>
        [Test, Category("Automated")]
        public static void ImageTest_PdfToPdf()
        {
            using var pair = ConvertPdfResourceToSearchable(_TEXT_BASED_PDF);

            // Check that the output rendered as images is the same as the original
            double errors = ComparePagesAsImages(pair.Input, pair.Output);
            Assert.Less(errors, 1E-6, "No conversion should have happened");
        }

        /// <summary>
        /// Ensure rotated TIF pages do not change visually
        /// </summary>
        [Test, Category("Automated")]
        public static void ImageTest_RotatedTifToPdf()
        {
            using var pair = ConvertTifResourceToSearchable(_EXAMPLE05_WITH_ROTATED_PAGE_TIF);

            // Check that the output rendered as images is very similar to the original
            // Same errors as above, calibration (IFC) test
            double errors = ComparePagesAsImages(pair.Input, pair.Output);
            Assert.Less(errors, 0.11);
        }

        /// <summary>
        /// Ensure rotated TIF pages OCR correctly
        /// https://extract.atlassian.net/browse/ISSUE-16740
        /// </summary>
        [Test, Category("Automated")]
        public static void TextTest_RotatedTifToPdf()
        {
            using var pair = ConvertTifResourceToSearchable(_EXAMPLE05_WITH_ROTATED_PAGE_TIF);

            var text = GetText(pair.Output, 2);
            StringAssert.Contains("WITNESS my hand and official seal", text);
            StringAssert.Contains("My Commission Expires:", text);
        }

        /// <summary>
        /// Ensure rotated PDF pages do not change visually
        /// </summary>
        [Test, Category("Automated")]
        public static void ImageTest_RotatedPdfToPdf()
        {
            using var pair = ConvertPdfResourceToSearchable(_EXAMPLE05_WITH_ROTATED_PAGE_PDF);

            // Check that the output rendered as images is the same as the original
            double errors = ComparePagesAsImages(pair.Input, pair.Output);
            Assert.Less(errors, 1E-6, "No conversion should have happened");
        }

        /// <summary>
        /// Ensure rotated PDF pages OCR correctly
        /// https://extract.atlassian.net/browse/ISSUE-16740
        /// </summary>
        [Test, Category("Automated")]
        public static void TextTest_RotatedPdfToPdf()
        {
            using var pair = ConvertPdfResourceToSearchable(_EXAMPLE05_WITH_ROTATED_PAGE_PDF);

            var text = GetText(pair.Output, 2);
            StringAssert.Contains("WITNESS my hand and official seal", text);
            StringAssert.Contains("My Commission Expires:", text);
        }

        /// <summary>
        /// Compare original pdf text to the output pdf text
        /// </summary>
        [Test, Category("Automated")]
        public static void TextTest_TextPdfToPdf()
        {
            using var pair = ConvertPdfResourceToSearchable(_TEXT_BASED_PDF);

            // Check that the output text is, almost, the same as the original (new text is the "Cross Check" image, badly recognized)
            var origText = GetText(pair.Input);
            var newText = GetText(pair.Output);
            var editDistance = UtilityMethods.LevenshteinDistance(origText, newText);
            Assert.Less(editDistance, 20, "This value was 16 when test was written");
        }

        /// <summary>
        /// Compare original pdf text to the output PDFA text
        /// </summary>
        /// <remarks>
        /// Outputting PDFA is a different code path in ESConvertToPDF
        /// Currently this test is broken. The code in ESConvertToPDF should be fixed so that it preserves existing text
        /// when using the legacy method. The same code path is used as a fall-back method for (so this could affect non-password protected, non-PDFA outputs too).
        /// </remarks>
        [Test, Category("Automated")]
        public static void TextTest_TextPdfToPdfa()
        {
            using var pair = ConvertPdfResourceToSearchable(_TEXT_BASED_PDF, "/pdfa");

            // Check that the output text is, almost, the same as the original
            var origText = GetText(pair.Input);
            var newText = GetText(pair.Output);
            var editDistance = UtilityMethods.LevenshteinDistance(origText, newText);
            Assert.Less(editDistance, 70, "I saw a value of 67 when test was written");

            var fixedText = newText.Replace(" \r\n", "\r\n");
            editDistance = UtilityMethods.LevenshteinDistance(origText, fixedText);
            Assert.Less(editDistance, 2, "I saw a value of 1 when test was written");
        }

        /// <summary>
        /// Ensure rotated PDF pages OCR correctly when result is PDFA
        /// https://extract.atlassian.net/browse/ISSUE-16740
        /// </summary>
        /// <remarks>
        /// Outputting PDFA is a different code path in ESConvertToPDF
        /// </remarks>
        [Test, Category("Automated")]
        public static void TextTest_RotatedPdfToPdfa()
        {
            using var pair = ConvertPdfResourceToSearchable(_EXAMPLE05_WITH_ROTATED_PAGE_PDF, "/pdfa");

            var text = GetText(pair.Output, 2);
            StringAssert.Contains("WITNESS my hand and official seal", text);
            StringAssert.Contains("My Commission Expires:", text);
        }

        /// <summary>
        /// Ensure rotated TIF pages OCR correctly when result is PDFA
        /// https://extract.atlassian.net/browse/ISSUE-16740
        /// </summary>
        /// <remarks>
        /// Outputting PDFA is a different code path in ESConvertToPDF
        /// </remarks>
        [Test, Category("Automated")]
        public static void TextTest_RotatedTifToPdfa()
        {
            using var pair = ConvertTifResourceToSearchable(_EXAMPLE05_WITH_ROTATED_PAGE_TIF, "/pdfa");

            var text = GetText(pair.Output, 2);
            StringAssert.Contains("WITNESS my hand and official seal", text);
            StringAssert.Contains("My Commission Expires:", text);
        }

        /// <summary>
        /// Ensure rotated PDF pages do not change visually when using PDFA format
        /// </summary>
        /// <remarks>
        /// If outputting PDFA there is a different code path in ESConvertToPDF
        /// </remarks>
        [Test, Category("Automated")]
        public static void ImageTest_RotatedPdfToPdfa()
        {
            using var pair = ConvertPdfResourceToSearchable(_EXAMPLE05_WITH_ROTATED_PAGE_PDF, "/pdfa");

            // Check that the output rendered as images is the same as the original
            double errors = ComparePagesAsImages(pair.Input, pair.Output);
            Assert.Less(errors, 1E-6, "No conversion should have happened");
        }

        /// <summary>
        /// Ensure rotated TIF pages do not change visually when using PDFA format
        /// </summary>
        [Test, Category("Automated")]
        public static void ImageTest_RotatedTifToPdfa()
        {
            using var pair = ConvertTifResourceToSearchable(_EXAMPLE05_WITH_ROTATED_PAGE_TIF, "/pdfa");
            double errors = ComparePagesAsImages(pair.Input, pair.Output);
            Assert.Less(errors, 0.11);
        }

        /// <summary>
        /// Ensure large PDFs can be converted
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_LargePdfToPdf()
        {
            using var tall = ConvertPdfResourceToSearchable(_TALL_IMAGE_PDF);
            using var wide = ConvertPdfResourceToSearchable(_WIDE_IMAGE_PDF);

            // Check that the output rendered as images is the same as the original
            double errors = ComparePagesAsImages(tall.Input, tall.Output);
            Assert.Less(errors, 1E-6, "No conversion should have happened");

            errors = ComparePagesAsImages(wide.Input, wide.Output);
            Assert.Less(errors, 1E-6, "No conversion should have happened");

            // Text should be about the same for tall as wide (wide is tall rotated 270 degrees)
            var tallText = GetText(tall.Output);
            var wideText = GetText(wide.Output);
            var editDistance = UtilityMethods.LevenshteinDistance(tallText, wideText);
            Assert.Less(editDistance, 10, "This value was 2 when test was written");

            // Check that there is something recognized
            StringAssert.Contains("Header", wideText);
            StringAssert.Contains("Contract Date", wideText);
        }

        /// <summary>
        /// Ensure large PDFs can be converted
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_LargeTifToPdf()
        {
            // Convert the PDFs to TIFs
            // BONUS: This tests that the IFC can handle large files too
            using var tallIFC = ConvertPdfResourceToTif(_TALL_IMAGE_PDF);
            using var wideIFC = ConvertPdfResourceToTif(_WIDE_IMAGE_PDF);

            using var tall = ConvertTifToSearchable(tallIFC.Output.Path);
            using var wide = ConvertTifToSearchable(wideIFC.Output.Path);

            // Check that the output rendered as images is similar to the original
            double errors = ComparePagesAsImages(tall.Input, tall.Output);
            Assert.Less(errors, 0.03);

            errors = ComparePagesAsImages(wide.Input, wide.Output);
            Assert.Less(errors, 0.03);

            // Text should be about the same for tall as wide (wide is tall rotated 270 degrees)
            var tallText = GetText(tall.Output);
            var wideText = GetText(wide.Output);
            var editDistance = UtilityMethods.LevenshteinDistance(tallText, wideText);
            Assert.Less(editDistance, 10, "This value was 2 when test was written");

            // Check that there is something recognized
            StringAssert.Contains("Header", wideText);
            StringAssert.Contains("Contract Date", wideText);
        }


        // Convert a TIF to a searchable PDF
        static FilePair<TIF, PDF> ConvertTifToSearchable(string inputPath, params string[] options)
        {
            StringAssert.EndsWith(".tif", inputPath);
            var outputFile = new TemporaryFile(".searchable.pdf", false);
            var outputPath = outputFile.FileName;
            File.Delete(outputPath);

            var args = new List<string> { inputPath, outputPath }
                .Concat(options)
                .ToArray();

            SystemMethods.RunExecutable(_ESConvertToPDF, args, createNoWindow: true);

            return new FilePair<TIF, PDF>(new TIF(inputPath), new PDF(outputPath), outputFile);
        }

        // Convert a TIF embedded resource to a searchable PDF
        static FilePair<TIF, PDF> ConvertTifResourceToSearchable(string resourceName, params string[] options)
        {
            StringAssert.EndsWith(".tif", resourceName);
            var inputPath = _testFiles.GetFile(resourceName);
            return ConvertTifToSearchable(inputPath, options);
        }

        // Convert a TIF embedded resource to a searchable PDF
        static FilePair<PDF, PDF> ConvertPdfResourceToSearchable(string resourceName, params string[] options)
        {
            StringAssert.EndsWith(".pdf", resourceName);
            var inputFile = _testFiles.GetFile(resourceName);
            var outputFile = new TemporaryFile(".searchable.pdf", false);
            var outputPath = outputFile.FileName;
            File.Delete(outputPath);

            var args = new List<string> { inputFile, outputPath }
                .Concat(options)
                .ToArray();

            SystemMethods.RunExecutable(_ESConvertToPDF, args, createNoWindow: true);

            return new FilePair<PDF, PDF>(new PDF(inputFile), new PDF(outputPath), outputFile);
        }

        // Convert PDF embedded resource to TIF
        static FilePair<PDF, TIF> ConvertPdfResourceToTif(string resourceName)
        {
            StringAssert.EndsWith(".pdf", resourceName);
            var testImage = _testFiles.GetFile(resourceName);
            var outputFile = new TemporaryFile(".tif", false);
            var outputPath = outputFile.FileName;
            File.Delete(outputPath);

            SystemMethods.RunExecutable(_IMAGE_FORMAT_CONVERTER_PATH,
                new[] { testImage, outputPath, "/tif", "/am", "/color" },
                createNoWindow: true);

            return new FilePair<PDF, TIF>(new PDF(testImage), new TIF(outputPath), outputFile);
        }

        // Convert TIF embedded resource to non-searchable PDF
        static FilePair<TIF, PDF> ConvertTifResourceToPdf(string resourceName)
        {
            StringAssert.EndsWith(".tif", resourceName);
            var testImage = _testFiles.GetFile(resourceName);
            var outputFile = new TemporaryFile(".pdf", false);
            var output = outputFile.FileName;
            File.Delete(output);

            SystemMethods.RunExecutable(_IMAGE_FORMAT_CONVERTER_PATH,
                new[] { testImage, output, "/pdf", "/am", "/color" },
                createNoWindow: true);

            return new FilePair<TIF, PDF>(new TIF(testImage), new PDF(output), outputFile);
        }

        //--------------------------------------------------------------------------------
        // A disposable tuple to make tests less verbose
        class FilePair<TInput, TOutput> : IDisposable
            where TInput : IDisposable
            where TOutput : IDisposable
        {
            readonly TemporaryFile tempFile;
            bool disposedValue;

            public FilePair(TInput input, TOutput output, TemporaryFile tempFile)
            {
                Input = input;
                Output = output;
                this.tempFile = tempFile;
            }

            public TInput Input { get; }
            public TOutput Output { get; }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        Input.Dispose();
                        Output.Dispose();
                        tempFile.Dispose();
                    }

                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
