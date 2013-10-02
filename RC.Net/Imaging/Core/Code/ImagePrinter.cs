using Extract.Imaging.Utilities;
using Extract.Licensing;
using Leadtools;
using Leadtools.Annotations;
using Leadtools.Drawing;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extract.Imaging
{
    /// <summary>
    /// Allows printing of images.
    /// </summary>
    public class ImagePrinter : IDisposable
    {
        #region Constants

        /// <summary>
        /// The object name used in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ImagePrinter).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="ImageReader"/> used to load the image from disk.
        /// </summary>
        ImageReader _imageReader;

        /// <summary>
        /// Indicates whether image annotations should be printed.
        /// </summary>
        bool _printAnnotations;

        /// <summary>
        /// Indicates the current page number being printed.
        /// </summary>
        int _currentPageNumber;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="ImagePrinter"/> class from being created.
        /// </summary>
        private ImagePrinter()
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                        "ELI36011", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36012");
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Prints the specified file.
        /// <para><b>Note</b></para>
        /// Any annotations on the image will be printed. If a failure occurs trying to print
        /// annotations, the document will still be printed, but an exception will be logged.
        /// </summary>
        /// <param name="fileName">Name of the file to print.</param>
        public static void Print(string fileName)
        {
            try
            {
                var imagePrinter = new ImagePrinter();
                imagePrinter.PrintHelper(fileName, null, true, true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36010");
            }
        }

        /// <summary>
        /// Prints the specified file.
        /// </summary>
        /// <param name="fileName">Name of the file to print.</param>
        /// <param name="printerName">Name of the printer to print to or <see langword="null"/> to
        /// use the default printer.</param>
        /// <param name="useInstalledPrinterList"><see langword="true"/> if
        /// <see paramref="printerName"/> should be explicitly validated against the installed
        /// printers list; <see langword="false"/> to allow the printer to be discovered on the
        /// network even when it is not currently on the installed printers list.</param>
        /// <param name="printAnnotations"> <see langword="true"/> to print annotations annotations;
        /// otherwise, <see langword="false"/>.
        /// <para><b>Note</b></para>
        /// If a failure occurs trying to print annotations, the document will still be printed, but
        /// an exception will be logged.
        /// </param>
        public static void Print(string fileName, string printerName, bool useInstalledPrinterList,
            bool printAnnotations)
        {
            try
            {
                var imagePrinter = new ImagePrinter();
                imagePrinter.PrintHelper(fileName, printerName, useInstalledPrinterList,
                    printAnnotations);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36013");
            }
        }

        #endregion Methods

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the <see cref="ImagePrinter"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ImagePrinter"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ImagePrinter"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources

                if (_imageReader != null)
                {
                    _imageReader.Dispose();
                    _imageReader = null;
                }
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="PrintDocument.PrintPage"/> event of a <see cref="PrintDocument"/>
        /// instance.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PrintPageEventArgs"/> instance containing the event data.
        /// </param>
        void HandlePrintPage(object sender, PrintPageEventArgs e)
        {
            _currentPageNumber++;
            e.HasMorePages = _currentPageNumber < e.PageSettings.PrinterSettings.ToPage;

            using (RasterImage pageImage = _imageReader.ReadPage(_currentPageNumber))
            {
                // [IDSD #318], [DotNetRCAndUtils:1078]
                // The margin bounds should be the margin rectangle intersected with the
                // printable area offset by the inverse of the top-left of the printable area.
                var printableArea = Rectangle.Round(e.PageSettings.PrintableArea);
                Rectangle marginBounds = e.MarginBounds;
                marginBounds.Intersect(printableArea);
                marginBounds.Offset(-printableArea.X, -printableArea.Y);
                Rectangle sourceBounds = new Rectangle(0, 0, pageImage.Width, pageImage.Height);

                // Calculate the scale so the image fits exactly within the bounds of the page
                float scale = (float)Math.Min(
                    marginBounds.Width / (double)sourceBounds.Width,
                    marginBounds.Height / (double)sourceBounds.Height);

                // Calculate the horizontal and vertical padding
                PointF padding = new PointF(
                    (marginBounds.Width - sourceBounds.Width * scale) / 2.0F,
                    (marginBounds.Height - sourceBounds.Height * scale) / 2.0F);

                // Calculate the destination rectangle
                Rectangle destinationBounds = new Rectangle(
                    (int)(marginBounds.Left + padding.X), (int)(marginBounds.Top + padding.Y),
                    (int)(marginBounds.Width - padding.X * 2), (int)(marginBounds.Height - padding.Y * 2));

                using (var image = RasterImageConverter.ConvertToImage(pageImage,
                    ConvertToImageOptions.None))
                {
                    e.Graphics.DrawImage(image, destinationBounds, sourceBounds, GraphicsUnit.Pixel);

                    // If printing annotations, apply them now.
                    if (_printAnnotations)
                    {
                        DrawAnnotations(e, sourceBounds, destinationBounds, scale);
                    }
                }
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Prints the specified file.
        /// </summary>
        /// <param name="fileName">Name of the file to print.</param>
        /// <param name="printerName">Name of the printer to print to or <see langword="null"/> to
        /// use the default printer.</param>
        /// <param name="useInstalledPrinterList"><see langword="true"/> if
        /// <see paramref="printerName"/> should be explicitly validated against the installed
        /// printers list; <see langword="false"/> to allow the printer to be discovered on the
        /// network even when it is not currently on the installed printers list.</param>
        /// <param name="printAnnotations"> <see langword="true"/> to print annotations annotations;
        /// otherwise, <see langword="false"/>.</param>
        void PrintHelper(string fileName, string printerName, bool useInstalledPrinterList,
            bool printAnnotations)
        {
            if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                UnlockLeadtools.UnlockPdfSupport(true);
            }

            using (ImageCodecs imageCodecs = new ImageCodecs())
            {
                _imageReader = imageCodecs.CreateReader(fileName);
                _printAnnotations = printAnnotations;

                PrintDocument printDocument = new PrintDocument();
                printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                printDocument.DefaultPageSettings.Color = true;
                printDocument.DocumentName = Path.GetFileName(fileName);
                printDocument.PrinterSettings.PrintFileName = Path.GetFileName(fileName);
                printDocument.PrinterSettings.PrintRange = PrintRange.AllPages;
                printDocument.PrinterSettings.FromPage = 1;
                printDocument.PrinterSettings.ToPage = _imageReader.PageCount;
                    
                if (!string.IsNullOrEmpty(printerName))
                {
                    // [DotNetRCAndUtils:1089]
                    // Interpret printer names that end with " on [MachineName]" to be equal to
                    // "\\[MachineName]\[PrinterName]".
                    if (!printerName.StartsWith(@"\\", StringComparison.Ordinal))
                    {
                        Regex printerPathRegex = new Regex(@"(\son\s)([\s\S]+)$");
                        Match machineNameMatch = printerPathRegex.Match(printerName);
                        if (machineNameMatch.Success)
                        {
                            string machineName = machineNameMatch.Groups[2].Value;
                            printerName = string.Format(@"\\{0}\{1}", machineName,
                                printerName.Substring(0, machineNameMatch.Index));
                        }
                    }

                    if (useInstalledPrinterList &&
                        !PrinterSettings.InstalledPrinters
                            .OfType<string>()
                            .Any(printer =>
                                printer.Equals(printerName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var ee = new ExtractException("ELI36176",
                            "The specified printer not in the local installed printers list.");
                        ee.AddDebugData("Printer name", printerName, false);
                        throw ee;
                    }

                    printDocument.PrinterSettings.PrinterName = printerName;
                    ExtractException.Assert("ELI36007", "Printer not found.",
                        printDocument.PrinterSettings.IsValid, "Printer name", printerName);
                }
                    
                printDocument.PrintPage += HandlePrintPage;
                printDocument.Print();
            }
        }

        /// <summary>
        /// Draws the annotations to the page indicated by <see paramref="printPageEventArgs"/>.
        /// </summary>
        /// <param name="printPageEventArgs">The <see cref="PrintPageEventArgs"/> for the page
        /// currently being printed.</param>
        /// <param name="sourceBounds">A <see cref="Rectangle"/> describing the bounds of the source
        /// image.</param>
        /// <param name="destinationBounds">A <see cref="Rectangle"/> describing the bounds of the
        /// page being printed.</param>
        /// <param name="scale">Factor by which source image coordinates should be multiplied to
        /// arrive at coordinates in printed page.</param>
        void DrawAnnotations(PrintPageEventArgs printPageEventArgs, Rectangle sourceBounds,
            Rectangle destinationBounds, float scale)
        {
            try
            {
                // Document support needs to be unlocked to print annotations.
                UnlockLeadtools.UnlockDocumentSupport(true);

                // Load annotations from file
                RasterTagMetadata tag = _imageReader.ReadTagOnPage(_currentPageNumber);
                if (tag != null)
                {
                    // Initialize a new annotations container and associate it with the image viewer.
                    AnnContainer annotations = new AnnContainer();

                    annotations.Bounds =
                        new AnnRectangle(0, 0, sourceBounds.Width, sourceBounds.Height, AnnUnit.Pixel);
                    annotations.Visible = true;

                    // Ensure the unit converter is the same DPI as the output graphics.
                    annotations.UnitConverter = new AnnUnitConverter(printPageEventArgs.Graphics.DpiX, printPageEventArgs.Graphics.DpiY);

                    // Load the annotations from the Tiff tag.
                    AnnCodecs annCodecs = new AnnCodecs();
                    annCodecs.LoadFromTag(tag, annotations);

                    // Make all Leadtools annotations visible
                    foreach (AnnObject annotation in annotations.Objects)
                    {
                        annotation.Visible = true;
                    }

                    // Create matrix to map logical (image) coordinates to printer coordinates
                    using (annotations.Transform = GetTransformMatrix(sourceBounds.Location,
                        scale, destinationBounds.Location))
                    {
                        annotations.Draw(printPageEventArgs.Graphics);
                    }
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI36009", "Unable to print annotations.", ex);
                ee.Log();
            }
        }

        /// <summary>
        /// Creates a 3x3 affine matrix that maps the unrotated image using the specified offset,
        /// scale, and padding.
        /// </summary>
        /// <param name="offset">The offset applied to the original image in logical (image) 
        /// coordinates.</param>
        /// <param name="scale">The scale to apply to the original image in logical (image) 
        /// coordinates.</param>
        /// <param name="padding">The padding to apply to the original image in logical (image) 
        /// coordinates.</param>
        /// <returns>A 3x3 affine matrix that maps the unrotated image to a rotated destination 
        /// rectangle with the specified scale factor and margin padding.
        /// </returns>
        static Matrix GetTransformMatrix(PointF offset, float scale, PointF padding)
        {
            // Create the matrix
            Matrix printMatrix = new Matrix();

            // Translate the origin by the specified offset
            printMatrix.Translate(-offset.X, -offset.Y);

            // Scale the matrix
            printMatrix.Scale(scale, scale, MatrixOrder.Append);

            // Translate the matrix so the image is centered with the specified padding
            printMatrix.Translate(padding.X, padding.Y, MatrixOrder.Append);

            // Return the rotated the matrix
            return printMatrix;
        }
        
        #endregion Private Members
    }
}
