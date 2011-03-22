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

        static readonly string _OBJECT_NAME = typeof(ImageCodecs).ToString();

        static readonly int _DEFAULT_PDF_DISPLAY_DEPTH = 24;

        static readonly int _DEFAULT_PDF_RESOLUTION = 300;

        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if <see cref="ImageCodecs"/> has been disposed; 
        /// <see langword="false"/> if <see cref="ImageCodecs"/> has not been disposed.
        /// </summary>
        volatile bool _disposed;

        /// <summary>
        /// <see langword="true"/> if PDF documents should be loaded as bitonal images; 
        /// <see langword="false"/> if PDF documents should be loaded in full color depth.
        /// </summary>
        /// <remarks>Leadtools does not support anti-aliasing for non-bitonal PDF documents.
        /// </remarks>
        readonly bool _loadPdfAsBitonal;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCodecs"/> class.
        /// </summary>
        public ImageCodecs()
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28485",
                    _OBJECT_NAME);

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
            RasterCodecs codecs = null;
            try
            {
                codecs = GetCodecs();

                return new ImageReader(fileName, codecs, _loadPdfAsBitonal);
            }
            catch (Exception ex)
            {
                if (codecs != null)
                {
                    codecs.Dispose();
                }

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
            RasterCodecs codecs = null;
            try
            {
                codecs = GetCodecs();

                return new ImageWriter(fileName, codecs, format);
            }
            catch (Exception ex)
            {
                if (codecs != null)
                {
                    codecs.Dispose();
                }

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
            ExtractException.Assert("ELI30000", "Codecs has been disposed.", !_disposed);

            RasterCodecs codecs = new RasterCodecs();
            SetOptions(codecs.Options);

            return codecs;
        }

        /// <summary>
        /// Sets the options used when loading or saving image files.
        /// </summary>
        static void SetOptions(CodecsOptions options)
        {
            options.Pdf.Save.UseImageResolution = true;
            options.Tiff.Load.IgnoreViewPerspective = true;

            // Use default DPI and display depth
            options.Pdf.Load.DisplayDepth = _DEFAULT_PDF_DISPLAY_DEPTH;
            options.Pdf.Load.XResolution = _DEFAULT_PDF_RESOLUTION;
            options.Pdf.Load.YResolution = _DEFAULT_PDF_RESOLUTION;
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
                _disposed = true;
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}