using Aspose.Pdf;
using Aspose.Pdf.InteractiveFeatures;
using Aspose.Pdf.InteractiveFeatures.Annotations;
using Extract;
using Extract.Imaging;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Utilities
{
    /// <summary>
    /// Application that takes a source pdf and destination pdf, modifies the source and
    /// saves it to the destination
    /// </summary>
    static class ModifyPdfFileProgram
    {
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

                if (argumentCount < 2)
                {
                    DisplayUsage("Incorrect number of arguments.");
                    return;
                }

                string pdfSource = Path.GetFullPath(args[0]);
                string pdfDest = Path.GetFullPath(args[1]);
                bool removeAnnotations = false;
                string[] hyperlinkAttributes = null;
                string hyperlinkAddress = "";
                string dataFileName = "";
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
                    else if (temp.Equals("/h", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((++i) >= argumentCount)
                        {
                            DisplayUsage("No attribute names specified.");
                            return;
                        }

                        hyperlinkAttributes =
                            args[i].Split(new[] { ',', ';', ' '}, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else if (temp.Equals("/ha", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((++i) >= argumentCount)
                        {
                            DisplayUsage("No hyperlink address specified.");
                            return;
                        }

                        hyperlinkAddress = args[i];
                    }
                    else if (temp.Equals("/voa", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((++i) >= argumentCount)
                        {
                            DisplayUsage("No VOA filename specified.");
                            return;
                        }

                        dataFileName = args[i];
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ModifyPdf, "ELI31883",
                    "Modify PDF File");

                // Ensure a file name was specified and that it exists.
                ExtractException.Assert("ELI31877", "Source file does not exist.",
                    File.Exists(pdfSource), "PDF Source", pdfSource);

                // Ensure the file is a PDF file [LRCAU #5729]
                ExtractException.Assert("ELI31878", "Source file is not a pdf.",
                    ImageMethods.IsPdf(pdfSource), "PDF Source", pdfSource);

                ExtractException.Assert("ELI36286", "No PDF modifications have been configured.",
                    removeAnnotations || hyperlinkAttributes != null);

                ExtractException.Assert("ELI36268", "Data file for hyperlinks not specified.",
                    hyperlinkAttributes == null || !string.IsNullOrWhiteSpace(dataFileName),
                    "Data file", dataFileName);

                if (!overWrite && File.Exists(pdfDest))
                {
                    var ee = new ExtractException("ELI31879", "Destination file already exists.");
                    ee.AddDebugData("Destination File", pdfDest, false);
                    throw ee;
                }

                using (var tempFile = new TemporaryFile(".pdf", true))
                {
                    bool modified = false;
                    // Not all exception modifying the PDF will be treated as errors that should
                    // prevent output from being generated. Keep a list of all such errors.
                    var exceptions = new List<ExtractException>();

                    using (Document pdfDocument = new Document(pdfSource))
                    {
                        if (removeAnnotations)
                        {
                            modified |= RemoveAnnotationsFromFile(pdfDocument, exceptions);
                        }

                        if (hyperlinkAttributes != null)
                        {
                            modified |= AddHyperlinks(pdfDocument, dataFileName,
                                hyperlinkAttributes,hyperlinkAddress, exceptions);
                        }

                        // Save output as long as either something has been modified or no
                        // exceptions of any kind were encountered.
                        if (modified || exceptions.Count == 0)
                        {
                            pdfDocument.Save(tempFile.FileName);
                            File.Copy(tempFile.FileName, pdfDest, true);
                        }
                    }

                    // After outputting the document, throw any exceptions that were collected.
                    if (exceptions.Count > 0)
                    {
                        var ee = new ExtractException("ELI36275", modified
                            ? "PDF file was modified with errors."
                            : "Failed to modify PDF file.",
                            exceptions.AsAggregateException());
                        throw ee;
                    }
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
            sb.AppendLine(" <PDFSource> <PDFDestination> [/ra] [/h <AttributeNames> /voa " +
                "<DataFilename> [/ha <HyperlinkAddress>]] [/o] [/ef <ExceptionFile>]");
            sb.AppendLine();
            sb.AppendLine("Usage:");
            sb.AppendLine("-------------------");
            sb.AppendLine("<PDFSource>: The source pdf file.");
            sb.AppendLine("<PDFDestination>: The destination pdf file.");
            sb.AppendLine("/ra: Indicates that annotations should be removed from the PDF file.");
            sb.AppendLine("/h <AttributeNames>: Indicates that hyperlinks should be added for " +
                "attributes whose names are indicated in a comma separated list.");
            sb.AppendLine("/ha <HyperlinkAddress>: Specify the address that should be used for " +
                "hyperlinks. If not specified, the value of the attributes will be used.");
            sb.AppendLine("/voa <DataFileName>: The name of the datafile containing the " +
                "attributes to use for hyperlinks.");
            sb.AppendLine("/o: Will overwrite the destination file if it exists.");
            sb.AppendLine("/ef <ExceptionFile>: Log any exceptions to the specified");
            sb.AppendLine("    file rather than display them.");

            UtilityMethods.ShowMessageBox(sb.ToString(), error ? "Error" : "Usage", error);
        }

        /// <summary>
        /// Removes all annotations and hyperlinks from the <see paramref="pdfDocument"/>.
        /// </summary>
        /// <param name="pdfDocument">The <see cref="Document"/> instance for which annotations
        /// should be removed.</param>
        /// <param name="exceptions">Collects any exceptions modifying the PDF that should not be
        /// treated as errors that should prevent output from being generated.</param>
        /// <returns><see langword="true"/> if the <see paramref="pdfDocument"/> was modified;
        /// otherwise, <see langword="true"/>.</returns>
        static bool RemoveAnnotationsFromFile(Document pdfDocument, List<ExtractException> exceptions)
        {
            bool modified = false;
            var removalExceptions = new List<ExtractException>();

            foreach (Page page in pdfDocument.Pages)
            {
                try
                {
                    if (page.Annotations.Count > 0)
                    {
                        page.Annotations.Delete();
                        modified = true;
                    }
                }
                catch (Exception ex)
                {
                    // Go ahead and allow output to be generated even if there are errors deleting
                    // annotations as long as at least some annotations were successfully deleted.
                    removalExceptions.Add(ex.AsExtract("ELI36276"));
                }
            }

            if (removalExceptions.Count > 0)
            {
                var ee = new ExtractException("ELI36277",
                    string.Format(CultureInfo.CurrentCulture,
                        "Failed to remove annotations on {0} of {1} page(s).",
                        removalExceptions.Count, pdfDocument.Pages.Count),
                    removalExceptions.AsAggregateException());

                exceptions.Add(ee);
            }

            return modified;
        }


        /// <summary>
        /// Adds hyperlinks to the the <see paramref="pdfDocument"/>.
        /// </summary>
        /// <param name="pdfDocument">The <see cref="Document"/> instance to which hyperlinks
        /// should be added.</param>
        /// <param name="dataFileName">The name of the VOA providing that <see cref="IAttribute"/>s to
        /// use to define where hyperlinks are added.</param>
        /// <param name="hyperlinkAttributes">A list of attribute names that should be used from
        /// the <see paramref="dataFileName"/>.</param>
        /// <param name="hyperlinkAddress">The address that should be used for all hyperlinks. If
        /// not specified, the value of each <see cref="IAttribute"/> will be used as the link
        /// address.</param>
        /// <param name="exceptions">Collects any exceptions modifying the PDF that should not be
        /// treated as errors that should prevent output from being generated.</param>
        /// <returns><see langword="true"/> if the <see paramref="pdfDocument"/> was modified;
        /// otherwise, <see langword="true"/>.</returns>
        static bool AddHyperlinks(Document pdfDocument, string dataFileName,
            string[] hyperlinkAttributes, string hyperlinkAddress, List<ExtractException> exceptions)
        {
            bool modified = false;

            // Load a list of all attributes to use from the data file.
            IUnknownVector attributes = new IUnknownVector();
            attributes.LoadFrom(dataFileName, false);
            string attributeQuery = string.Join("|", hyperlinkAttributes);
            AFUtility afUtility = new AFUtility();
            IUnknownVector selectedAttributes =
                afUtility.QueryAttributes(attributes, attributeQuery, false);

            int attributeCount = 0;
            var hyperlinkExceptions = new List<ExtractException>();

            // Add a hyperlink for each selected attribute.
            foreach (var attribute in selectedAttributes.ToIEnumerable<IAttribute>())
            {
                attributeCount++;

                try
                {
                    ExtractException.Assert("ELI36272",
                        "Hyperlinks cannot be added for non-spatial attributes.",
                        attribute.Value.HasSpatialInfo());

                    // If an attribute spans pages, add a hyperlink for the attribute area on each
                    // page the attribute spans.
                    foreach (var pageValue in
                        attribute.Value.GetPages().ToIEnumerable<SpatialString>())
                    {
                        // Get the correct page from the PDF file.
                        int pageNum = pageValue.GetFirstPageNumber();
                        Page page = pdfDocument.Pages[pageNum];

                        // Convert the SpatialString coordinates to Aspose API coordinates.
                        Aspose.Pdf.Rectangle asposeRect = page.GetPageRect(false);
                        LongRectangle attributePageBounds =
                            pageValue.GetOriginalImagePageBounds(pageNum);
                        double xFactor = asposeRect.Width / (double)attributePageBounds.Right;
                        double yFactor = asposeRect.Height / (double)attributePageBounds.Bottom;

                        LongRectangle attributeBounds = pageValue.GetOriginalImageBounds();

                        // NOTE: The coordinate origin in the Apose API is the bottom left.
                        double left = attributeBounds.Left * xFactor;
                        double top = asposeRect.URY - (attributeBounds.Top * yFactor);
                        double right = attributeBounds.Right * xFactor;
                        double bottom = asposeRect.URY - (attributeBounds.Bottom * yFactor);

                        // Create the hyperlink annotation.
                        LinkAnnotation link = new LinkAnnotation(
                            page, new Aspose.Pdf.Rectangle(left, bottom, right, top));
                        link.Border = new Border(link);
                        link.Border.Style = BorderStyle.Underline;
                        link.Color = Aspose.Pdf.Color.FromRgb(System.Drawing.Color.Blue);
                        link.Action = new GoToURIAction(
                            string.IsNullOrWhiteSpace(hyperlinkAddress)
                                ? attribute.Value.String
                                : hyperlinkAddress);

                        page.Annotations.Add(link);
                        modified = true;
                    }
                }
                catch (Exception ex)
                {
                    // Go ahead and allow output to be generated even if there are errors adding
                    // hyperlinks as long as at least some hyperlinks were successfully added.
                    hyperlinkExceptions.Add(ex.AsExtract("ELI36269"));
                }
            }

            if (hyperlinkExceptions.Count > 0)
            {
                var ee = new ExtractException("ELI36270",
                    string.Format(CultureInfo.CurrentCulture,
                        "Failed to add {0} of {1} hyperlink(s).",
                        hyperlinkExceptions.Count, attributeCount),
                    hyperlinkExceptions.AsAggregateException());

                exceptions.Add(ee);
            }

            return modified;
        }

        #endregion Methods
    }
}
