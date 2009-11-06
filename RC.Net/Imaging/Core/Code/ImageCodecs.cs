using Extract.Licensing;
using Extract.Utilities;
using Leadtools;
using Leadtools.Codecs;
using System;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents software libraries for encoding and decoding image files.
    /// </summary>
    public sealed class ImageCodecs : IDisposable
    {
        #region Constants

        static readonly string _PDF_INITIALIZATION_DIRECTORY = GetPdfInitializationDirectory();

        static readonly string _OBJECT_NAME = typeof(ImageCodecs).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if <see cref="ImageCodecs"/> has been disposed; 
        /// <see langword="false"/> if <see cref="ImageCodecs"/> has not been disposed.
        /// </summary>
        bool _disposed;

        /// <summary>
        /// <see langword="true"/> if PDF documents should be loaded as bitonal images; 
        /// <see langword="false"/> if PDF documents should be loaded in full color depth.
        /// </summary>
        /// <remarks>Leadtools does not support anti-aliasing for non-bitonal PDF documents.
        /// </remarks>
        readonly bool _loadPdfAsBitonal;

        /// <summary>
        /// Extract licensing.
        /// </summary>
        static readonly LicenseStateCache _license =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCodecs"/> class.
        /// </summary>
        public ImageCodecs()
        {
            try
            {
                _license.Validate("ELI28485");

                // Load Leadtools libraries
                RasterCodecs.Startup();

                // Load pdfs as bitonal images if bitonal support is unlocked
                _loadPdfAsBitonal = !RasterSupport.IsLocked(RasterSupportType.Bitonal);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI28478",
                    "Unable to initialize image codecs.", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Creates a <see cref="ImageReader"/> to read the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file to read.</param>
        /// <returns>An <see cref="ImageReader"/> to read <paramref name="fileName"/>.</returns>
        public ImageReader CreateReader(string fileName)
        {
            try
            {
                RasterCodecs codecs = GetCodecs();

                return new ImageReader(fileName, codecs);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28479",
                    "Unable to create image reader.", ex);
                ee.AddDebugData("File name", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Creates a <see cref="ImageWriter"/> to write to the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file to write to.</param>
        /// <param name="format">The format of <paramref name="fileName"/>.</param>
        /// <returns>An <see cref="ImageWriter"/> to write to <paramref name="fileName"/>.</returns>
        public ImageWriter CreateWriter(string fileName, RasterImageFormat format)
        {
            try
            {
                RasterCodecs codecs = GetCodecs();

                return new ImageWriter(fileName, codecs, format);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28480",
                    "Unable to create image writer", ex);
                ee.AddDebugData("File name", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Creates the codecs used to encode or decode an image.
        /// </summary>
        /// <returns>Codecs used to encode or decode an image.</returns>
        RasterCodecs GetCodecs()
        {
            RasterCodecs codecs = new RasterCodecs();
            SetOptions(codecs.Options);

            return codecs;
        }

        /// <summary>
        /// Sets the options used when loading or saving image files.
        /// </summary>
        void SetOptions(CodecsOptions options)
        {
            options.Pdf.InitialPath = _PDF_INITIALIZATION_DIRECTORY;
            options.Pdf.Save.UseImageResolution = true;
            options.Tiff.Load.IgnoreViewPerspective = true;

            // Load PDF as bitonal images if necessary
            if (_loadPdfAsBitonal)
            {
                options.Pdf.Load.DisplayDepth = 1;

                // Use high dpi to preserve image quality
                options.Pdf.Load.XResolution = 300;
                options.Pdf.Load.YResolution = 300;
            }
        }

        /// <summary>
        /// Gets the PDF initialization directory (the directory that contains
        /// the Lib, Resource, and Fonts directories).
        /// </summary>
        /// <returns>The PDF initialization directory.</returns>
        static string GetPdfInitializationDirectory()
        {
            const string pdfDirectory = 
#if DEBUG
                @"\..\..\ReusableComponents\APIs\LeadTools_16.5\PDF";
#else
                @".\PDF";
#endif

            return FileSystemMethods.GetAbsolutePath(pdfDirectory);
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ImageCodecs"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ImageCodecs"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ImageCodecs"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                // Dispose of managed objects
                RasterCodecs.Shutdown();

                _disposed = true;
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}