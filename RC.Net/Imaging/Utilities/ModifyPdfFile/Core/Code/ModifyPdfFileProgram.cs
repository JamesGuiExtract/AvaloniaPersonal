using Extract.Imaging;
using Extract.Licensing;
using PegasusImaging.WinForms.PdfXpress3;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Extract.Utilities
{
    /// <summary>
    /// Application that takes a source pdf and destination pdf, modifies the source and
    /// saves it to the destination
    /// </summary>
    static class ModifyPdfFileProgram
    {
        #region Constants

        /// <summary>
        /// The XFDF text as a <see cref="T:byte[]"/>.
        /// </summary>
        static readonly byte[] _xfdfBytes = BuildEmptyXfdfStream();

        // Unlock codes for the PdfXpress engine
        static readonly int[] _ul = new int[] { 352502263, 995632770, 1963445779, 32594 };

        #endregion Constants

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The args.</param>
        [STAThread]
        static void Main(string[] args)
        {
            string exceptionFile = null;
            try
            {
                int argumentCount = args.Length;

                if (argumentCount == 1
                    && args[0].Equals("/?", StringComparison.OrdinalIgnoreCase))
                {
                    DisplayUsage(null);
                    return;
                }

                if (argumentCount < 2 || argumentCount > 6)
                {
                    DisplayUsage("Incorrect number of arguments.");
                    return;
                }

                string pdfSource = Path.GetFullPath(args[0]);
                string pdfDest = Path.GetFullPath(args[1]);
                bool removeAnnotations = false;
                bool overWrite = false;

                for (int i = 2; i < argumentCount; i++)
                {
                    string temp = args[i];
                    if (temp.Equals("/?", StringComparison.OrdinalIgnoreCase))
                    {
                        DisplayUsage(null);
                        return;
                    }
                    else if (temp.Equals("/ra", StringComparison.OrdinalIgnoreCase))
                    {
                        removeAnnotations = true;
                    }
                    else if (temp.Equals("/o", StringComparison.OrdinalIgnoreCase))
                    {
                        overWrite = true;
                    }
                    else if (temp.Equals("/ef", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((++i) >= argumentCount)
                        {
                            DisplayUsage("No exception file specified.");
                            return;
                        }

                        exceptionFile = Path.GetFullPath(args[i]);
                    }
                    else
                    {
                        DisplayUsage("Unrecognized argument: " + temp);
                        return;
                    }
                }

                // Load and validate licenses
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.PegasusPdfxpressModifyPdf,
                    "ELI31883", "Modify PDF File");

                // Ensure a file name was specified and that it exists.
                ExtractException.Assert("ELI31877", "Source file does not exist.",
                    File.Exists(pdfSource), "PDF Source", pdfSource);

                // Ensure the file is a PDF file [LRCAU #5729]
                ExtractException.Assert("ELI31878", "Source file is not a pdf.",
                    ImageMethods.IsPdf(pdfSource), "PDF Source", pdfSource);

                if (!overWrite && File.Exists(pdfDest))
                {
                    var ee = new ExtractException("ELI31879", "Destination file already exists.");
                    ee.AddDebugData("Destination File", pdfDest, false);
                    throw ee;
                }

                using (var tempFile = new TemporaryFile(".pdf"))
                {
                    if (removeAnnotations)
                    {
                        RemoveAnnotationsFromFile(pdfSource, tempFile.FileName);
                    }

                    File.Copy(tempFile.FileName, pdfDest, true);
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(exceptionFile))
                {
                    ex.ExtractLog("ELI31880", exceptionFile);
                }
                else
                {
                    ex.ExtractDisplay("ELI31881");
                }
            }
        }

        #region Methods

        /// <summary>
        /// Displays the usage.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        static void DisplayUsage(string errorMessage)
        {
            bool error = !string.IsNullOrWhiteSpace(errorMessage);

            var sb = new StringBuilder(error ? errorMessage : "", 1024);
            if (error)
            {
                sb.AppendLine();
                sb.AppendLine();
            }
            sb.Append(Environment.GetCommandLineArgs()[0]);
            sb.AppendLine(" <PDFSource> <PDFDestination> [/ra] [/o] [/ef <ExceptionFile>]");
            sb.AppendLine();
            sb.AppendLine("Usage:");
            sb.AppendLine("-------------------");
            sb.AppendLine("<PDFSource>: The source pdf file.");
            sb.AppendLine("<PDFDestination>: The destination pdf file.");
            sb.AppendLine("/ra: Indicates that annotations should be removed from the PDF file.");
            sb.AppendLine("/o: Will overwrite the destination file if it exists.");
            sb.AppendLine("/ef <ExceptionFile>: Log any exceptions to the specified");
            sb.AppendLine("    file rather than display them.");

            UtilityMethods.ShowMessageBox(sb.ToString(), error ? "Error" : "Usage", error);
        }

        /// <summary>
        /// Builds an empty xfdf byte array.
        /// </summary>
        /// <returns>A <see cref="T:byte[]"/> containing an empty xfdf string.</returns>
        static byte[] BuildEmptyXfdfStream()
        {
            // The xfdf string to remove annotations
            string xfdfText = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><xfdf xmlns="
                + "\"http://ns.adobe.com/xfdf/\" xml:space=\"preserve\"><annots/><ids original="
                + "\"0FF75CC9984F9D5DB0952D7ABA83CDE4\" modified=\"0FF75CC9984F9D5DB0952D7ABA83CDE4\"/>"
                + "</xfdf>";

            // Convert the string to a byte array
            byte[] xfdf = ASCIIEncoding.ASCII.GetBytes(xfdfText);

            return xfdf;
        }

        /// <summary>
        /// Removes the annotations from PDF file.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destFile">The dest file.</param>
        static void RemoveAnnotationsFromFile(string sourceFile, string destFile)
        {
            Mutex mutex = null;
            PdfXpress express = null;
            try
            {
                // Get the global PdfXpress mutex and block
                mutex = ThreadingMethods.GetGlobalNamedMutex(ImageConstants.PdfXpressMutex);
                mutex.WaitOne();

                // Initialize the PdfXpress engine
                express = new PdfXpress();
                express.Initialize();
                express.Licensing.UnlockRuntime(_ul[0], _ul[1], _ul[2], _ul[3]);

                // Open the file
                using (var document = new Document(express, sourceFile))
                {
                    // Create the Xfdf options (set all annotations on all pages and allow delete)
                    XfdfOptions xfdfoptions = new XfdfOptions();
                    xfdfoptions.WhichAnnotation = XfdfOptions.AllAnnotations;
                    xfdfoptions.WhichPage = XfdfOptions.AllPages;
                    xfdfoptions.CanDeleteAnnotations = true;

                    // Import the xfdf bytes containing no annotations (this will remove
                    // the annotations from the document)
                    document.ImportXfdf(xfdfoptions, _xfdfBytes);

                    // Set the save file options to save to the temp file
                    SaveOptions sfo = new SaveOptions();
                    sfo.Filename = destFile;
                    sfo.Overwrite = true;

                    // Save the document
                    document.Save(sfo);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31882");
            }
            finally
            {
                if (express != null)
                {
                    express.Dispose();
                    express = null;
                }
                if (mutex != null)
                {
                    // Ensure the mutex is released (do this as the last step after
                    // the PdfXpress engine is released)
                    mutex.ReleaseMutex();
                    mutex.Dispose();
                }
            }
        }

        #endregion Methods
    }
}
