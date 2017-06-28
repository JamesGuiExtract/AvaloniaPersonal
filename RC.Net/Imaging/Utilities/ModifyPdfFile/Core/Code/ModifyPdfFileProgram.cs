﻿using Aspose.Pdf;
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
using System.Linq;
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
        /// The width (in pixels) the underlines for hyperlinks should be drawn with.
        /// </summary>
        const int _HYPERLINK_UNDERLINE_WIDTH = 2;
        
        /// <summary>
        /// The distance below the attribute zone the underline should be drawn (in inches).
        /// This offset is to prevent the underline from being drawn across the bottom of the text
        /// being underlined.
        /// </summary>
        const double _HYPERLINK_UNDERLINE_OFFSET = .03;

        /// <summary>
        /// The distance below the attribute zone the underline should be drawn (in Aspose image
        /// coordinates). This offset is to prevent the underline from being drawn across the bottom
        /// of the text being underlined.
        /// </summary>
        static double? _hyperlinkUnderlineOffset;

        /// <summary>
        /// Gets the distance below the attribute zone the underline should be drawn (in Aspose
        /// image coordinates). This offset is to prevent the underline from being drawn across the
        /// bottom of the text being underlined.
        /// </summary>
        static double HyperlinkUnderlineOffset
        {
            get
            {
                if (!_hyperlinkUnderlineOffset.HasValue)
                {
                    // In order to calculate the distance in Aspose coordinates an underline should 
                    // be drawn, we need to know the DPI Aspose is using for the image.
                    // Unfortunately, I can't find a way to get this number directly. The Aspose
                    // forums indicate that 72 can be assumed... I think one step better than this
                    // assumption is to assume that the default VectorGraphicsRenderingDPI value for
                    // an Aspose.Pdf.Generator.Image is the same DPI for the pages in the pdfDocument.
                    var testImage = new Aspose.Pdf.Generator.Image();
                    double defaultDPI = (double)testImage.VectorGraphicsRenderingDPI;
                    _hyperlinkUnderlineOffset = _HYPERLINK_UNDERLINE_OFFSET * defaultDPI;
                }

                return _hyperlinkUnderlineOffset.Value;
            }
        }

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
                string[] highlightAttributes = null;
                string[] textAttributes = null;
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
                    else if (temp.Equals("/l", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((++i) >= argumentCount)
                        {
                            DisplayUsage("No attribute names specified.");
                            return;
                        }

                        hyperlinkAttributes =
                            args[i].Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else if (temp.Equals("/la", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((++i) >= argumentCount)
                        {
                            DisplayUsage("No hyperlink address specified.");
                            return;
                        }

                        hyperlinkAddress = args[i];
                    }
                    else if (temp.Equals("/h", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((++i) >= argumentCount)
                        {
                            DisplayUsage("No attribute names specified.");
                            return;
                        }

                        highlightAttributes =
                            args[i].Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else if (temp.Equals("/txt", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((++i) >= argumentCount)
                        {
                            DisplayUsage("No attribute names specified.");
                            return;
                        }

                        textAttributes =
                            args[i].Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

                bool addingAnnotations =
                    (hyperlinkAttributes != null ||
                    highlightAttributes != null ||
                    textAttributes != null);

                ExtractException.Assert("ELI36286", "No PDF modifications have been configured.",
                    removeAnnotations || addingAnnotations);

                ExtractException.Assert("ELI36268", "Data file for annotations not specified.",
                    !addingAnnotations || 
                    !string.IsNullOrWhiteSpace(dataFileName), "Data file", dataFileName);

                if (!overWrite && File.Exists(pdfDest))
                {
                    var ee = new ExtractException("ELI31879", "Destination file already exists.");
                    ee.AddDebugData("Destination File", pdfDest, false);
                    throw ee;
                }

                // Load the license for the Aspose PDF API
                License license = new License();
                license.SetLicense(
                    Path.Combine(FileSystemMethods.CommonComponentsPath, "Aspose.Pdf.lic"));

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
                            AnnotationCreatorDelegate hyperlinkCreator = (page, rectangle, attribute) =>
                                HyperlinkCreator(page, rectangle, attribute, hyperlinkAddress);

                            modified |= AddAnnotations(pdfDocument, dataFileName,
                                hyperlinkAttributes, "hyperlink", hyperlinkCreator, exceptions);
                        }

                        if (highlightAttributes != null)
                        {
                            modified |= AddAnnotations(pdfDocument, dataFileName,
                                highlightAttributes, "highlight", HighlightCreator, exceptions);
                        }

                        if (textAttributes != null)
                        {
                            modified |= AddAnnotations(pdfDocument, dataFileName,
                                textAttributes, "text annotation", FreeTextAnnotationCreator, exceptions);
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
            sb.AppendLine(" <PDFSource> <PDFDestination> [/ra] \r\n" +
                "[/l <AttributeNames>] [/la <LinkAddress>] [/h <AttributeNames>] \r\n" +
                "[/txt <AttributeNames>] [/voa <DataFilename>] [/o] \r\n" +
                "[/ef <ExceptionFile>]");
            sb.AppendLine();
            sb.AppendLine("Usage:");
            sb.AppendLine("-------------------");
            sb.AppendLine("<PDFSource>: The source pdf file.");
            sb.AppendLine("<PDFDestination>: The destination pdf file.");
            sb.AppendLine("/ra: Indicates that annotations should be removed from the PDF file.");
            sb.AppendLine("/l <AttributeNames>: Indicates that hyperlinks should be added for " +
                "attributes whose names are indicated in a comma separated list.");
            sb.AppendLine("/la <LinkAddress>: Specify the address that should be used for " +
                "hyperlinks. If not specified, the value of the attributes will be used.");
            sb.AppendLine("/h <AttributeNames>: Indicates that hyperlinks should be added for " +
                "attributes whose names are indicated in a comma separated list.");
            sb.AppendLine("/txt <AttributeNames>: Indicates that text annotations should be added " +
                "for attributes whose names are indicated in a comma separated list.");
            sb.AppendLine("/voa <DataFileName>: The name of the datafile containing the " +
                "attributes to use for annotations, highlights or hyperlinks.");
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
        /// Adds annotations to the <see paramref="pdfDocument"/>.
        /// </summary>
        /// <param name="pdfDocument">The <see cref="Document"/> instance to which annotations
        /// should be added.</param>
        /// <param name="dataFileName">The name of the VOA providing that <see cref="IAttribute"/>s to
        /// use to define where annotations are added.</param>
        /// <param name="annotationAttributes">A list of attribute names that should be used from
        /// the <see paramref="dataFileName"/>.</param>
        /// <param name="annotationTypeName">The name of annotation type being added.</param>
        /// <param name="annotationCreatorDelegate">A <see cref="AnnotationCreatorDelegate"/> that
        /// will create each <see paramref="annotationTypeName"/> annotation.</param>
        /// <param name="exceptions">Collects any exceptions modifying the PDF that should not be
        /// treated as errors that should prevent output from being generated.</param>
        /// <returns><see langword="true"/> if the <see paramref="pdfDocument"/> was modified;
        /// otherwise, <see langword="true"/>.</returns>
        static bool AddAnnotations(Document pdfDocument, string dataFileName,
            string[] annotationAttributes, string annotationTypeName,
            AnnotationCreatorDelegate annotationCreatorDelegate, List<ExtractException> exceptions)
        {
            bool modified = false;

            // Load a list of all attributes to use from the data file.
            IUnknownVector attributes = new IUnknownVector();
            attributes.LoadFrom(dataFileName, false);
            string attributeQuery = string.Join("|", annotationAttributes);
            AFUtility afUtility = new AFUtility();
            IUnknownVector selectedAttributes =
                afUtility.QueryAttributes(attributes, attributeQuery, false);

            int attributeCount = 0;
            var annotationExceptions = new List<ExtractException>();

            // Add an annotation for each selected attribute.
            foreach (var attribute in selectedAttributes.ToIEnumerable<IAttribute>())
            {
                attributeCount++;

                try
                {
                    ExtractException.Assert("ELI36272",
                        "Annotations cannot be added for non-spatial attributes.",
                        attribute.Value.HasSpatialInfo());

                    // If an attribute spans pages, add an annotation for the attribute area on each
                    // page the attribute spans.
                    foreach (var line in
                        attribute.Value.GetLines().ToIEnumerable<SpatialString>())
                    {
                        // Get the correct page from the PDF file.
                        int pageNum = line.GetFirstPageNumber();
                        Page page = pdfDocument.Pages[pageNum];

                        // Convert the SpatialString coordinates to Aspose API coordinates.
                        Aspose.Pdf.Rectangle asposeRect = page.GetPageRect(false);
                        LongRectangle attributePageBounds =
                            line.GetOriginalImagePageBounds(pageNum);
                        double xFactor = asposeRect.Width / (double)attributePageBounds.Right;
                        double yFactor = asposeRect.Height / (double)attributePageBounds.Bottom;

                        LongRectangle attributeBounds = line.GetOriginalImageBounds();

                        // NOTE: The coordinate origin in the Aspose API is the bottom left.
                        // https://extract.atlassian.net/browse/ISSUE-2292
                        // For reasons that are unclear, the X-coordinates needs to be compensated
                        // by the left coordinate of asposeRect, while the Y-coordinates should not
                        // be.
                        double left = attributeBounds.Left * xFactor + asposeRect.LLX;
                        double top = asposeRect.URY - (attributeBounds.Top * yFactor);
                        double right = attributeBounds.Right * xFactor + asposeRect.LLX;
                        double bottom = asposeRect.URY - (attributeBounds.Bottom * yFactor);
                        var rectangle = new Aspose.Pdf.Rectangle(left, bottom, right, top);

                        Annotation annotation = annotationCreatorDelegate(page, rectangle, attribute);
                        page.Annotations.Add(annotation);
                        modified = true;
                    }
                }
                catch (Exception ex)
                {
                    // Go ahead and allow output to be generated even if there are errors adding
                    // annotations as long as at least some were successfully added.
                    annotationExceptions.Add(ex.AsExtract("ELI36269"));
                }
            }

            if (annotationExceptions.Count > 0)
            {
                var ee = new ExtractException("ELI36270",
                    string.Format(CultureInfo.CurrentCulture,
                        "Failed to add {0} of {1} {2}(s).",
                        annotationExceptions.Count, attributeCount, annotationTypeName),
                    annotationExceptions.AsAggregateException());

                exceptions.Add(ee);
            }

            return modified;
        }

        /// <summary>
        /// A delegate used to create <see cref="Annotation"/>s of the required type within
        /// <see cref="AddAnnotations"/>.
        /// </summary>
        /// <param name="page">The <see cref="Aspose.Pdf.Page"/> the annotation should be added to.
        /// </param>
        /// <param name="rectangle">A <see cref="Aspose.Pdf.Rectangle"/> describing the bounds of the
        /// annotation.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> from the voa file the annotation is
        /// associated with.</param>
        /// <returns>The <see cref="Annotation"/>.</returns>
        delegate Annotation AnnotationCreatorDelegate(Aspose.Pdf.Page page,
            Aspose.Pdf.Rectangle rectangle, IAttribute attribute);

        /// <summary>
        /// Creates a hyperlink annotation.
        /// </summary>
        /// <param name="page">The <see cref="Aspose.Pdf.Page"/> the hyperlink should be added to.
        /// </param>
        /// <param name="rectangle">A <see cref="Aspose.Pdf.Rectangle"/> describing the bounds of the
        /// hyperlink.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> from the voa file the hyperlink is
        /// associated with.</param>
        /// <param name="hyperlinkAddress">The address to assign to the hyperlink (if not provided
        /// by the <see paramref="attribute"/> value.</param>
        /// <returns>The <see cref="Annotation"/>.</returns>
        static Annotation HyperlinkCreator(Aspose.Pdf.Page page, Aspose.Pdf.Rectangle rectangle,
            IAttribute attribute, string hyperlinkAddress)
        {
            try
            {
                Aspose.Pdf.Rectangle pageRect = page.GetPageRect(false);
                SpatialPageInfo pageInfo = attribute.Value.GetPageInfo(page.Number);

                // Expand the zone down so that the underline appears under the zone rather than on
                // top of it.
                rectangle.LLY = Math.Max(rectangle.LLY - HyperlinkUnderlineOffset, 0);

                // If the page is rotated, we will need to add a border on all sides rather
                // than just an underline (so the "underline" doesn't end up on the right,
                // top or bottom). Accordingly, expand the other boundaries out by the
                // HyperlinkUnderlineOffset distance so the border is not drawn on top of
                // the text.
                if (pageInfo.Orientation != EOrientation.kRotNone)
                {
                    rectangle.LLX = Math.Max(rectangle.LLX - HyperlinkUnderlineOffset, 0);
                    rectangle.URY = Math.Min(rectangle.URY + HyperlinkUnderlineOffset, pageRect.Height);
                    rectangle.URX = Math.Min(rectangle.URX + HyperlinkUnderlineOffset, pageRect.Width);
                }

                // Create the hyperlink annotation.
                LinkAnnotation link = new LinkAnnotation(page, rectangle);
                link.Border = new Border(link);

                // If the page is rotated, we will need to add a border on all sides rather
                // than just an underline (so the "underline" doesn't end up on the right,
                // top or bottom).
                link.Border.Style = (pageInfo.Orientation == EOrientation.kRotNone)
                    ? BorderStyle.Underline
                    : BorderStyle.Solid;
                link.Border.Width = _HYPERLINK_UNDERLINE_WIDTH;
                link.Color = Aspose.Pdf.Color.FromRgb(System.Drawing.Color.Blue);
                link.Action = new GoToURIAction(
                    string.IsNullOrWhiteSpace(hyperlinkAddress)
                        ? attribute.Value.String
                        : hyperlinkAddress);

                return link;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38368");
            }
        }

        /// <summary>
        /// Creates a highlight annotation.
        /// </summary>
        /// <param name="page">The <see cref="Aspose.Pdf.Page"/> the highlight should be added to.
        /// </param>
        /// <param name="rectangle">A <see cref="Aspose.Pdf.Rectangle"/> describing the bounds of the
        /// highlight.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> from the voa file the highlight is
        /// associated with.</param>
        /// <returns>The <see cref="Annotation"/>.</returns>
        static Annotation HighlightCreator(Aspose.Pdf.Page page, Aspose.Pdf.Rectangle rectangle,
            IAttribute attribute)
        {
            try
            {
                HighlightAnnotation highlight = new HighlightAnnotation(page, rectangle);
                highlight.Color = Color.Yellow;

                return highlight;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38369");
            }
        }

        /// <summary>
        /// Creates a free text annotation.
        /// </summary>
        /// <param name="page">The <see cref="Aspose.Pdf.Page"/> the highlight should be added to.
        /// </param>
        /// <param name="rectangle">A <see cref="Aspose.Pdf.Rectangle"/> describing the bounds of the
        /// highlight.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> from the voa file the highlight is
        /// associated with.</param>
        /// <returns>The <see cref="Annotation"/>.</returns>
        static Annotation FreeTextAnnotationCreator(Aspose.Pdf.Page page, Aspose.Pdf.Rectangle rectangle,
            IAttribute attribute)
        {
            try
            {
                TextAnnotation textAnnotation = new TextAnnotation(page, rectangle);
                textAnnotation.Title = attribute
                    .SubAttributes
                    .ToIEnumerable<IAttribute>()
                    .Where(a => a.Name == "Title")
                    .Select(a => a.Value.String)
                    .FirstOrDefault()
                    ?? attribute.Name.Replace('_', ' ');
                textAnnotation.Contents = attribute.Value.String;

                return textAnnotation;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43554");
            }
        }

        #endregion Methods
    }
}
