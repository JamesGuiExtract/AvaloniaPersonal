using Extract.Utilities;
using GdPicture14;
using System;
using System.IO;

namespace Extract.GdPicture
{
    public class GdPictureUtility : IDisposable
    {
        const string _LICENSE_KEY = "21180688892504565151613356179259628324";

        static readonly LicenseManager _licenseManager = new();

        static readonly string _TESS_DATA_PATH = Path.Combine(FileSystemMethods.CommonComponentsPath, "tessdata");

        readonly Lazy<GdPicturePDF> _pdfAPI;
        readonly Lazy<GdPictureImaging> _imagingAPI;
        readonly Lazy<GdPictureOCR> _ocrAPI;
        readonly Lazy<GdPictureDocumentConverter> _documentConverter;
        bool _isDisposed;

        /// <summary>
        /// Get the lazily-instantiated <see cref="GdPictureImaging"/> instance managed by this object
        /// </summary>
        public GdPictureImaging ImagingAPI => _imagingAPI.Value;

        /// <summary>
        /// Get the lazily-instantiated <see cref="GdPictureOCR"/> instance managed by this object
        /// </summary>
        public GdPictureOCR OcrAPI => _ocrAPI.Value;

        /// <summary>
        /// Get the lazily-instantiated <see cref="GdPicturePDF"/> instance managed by this object
        /// </summary>
        public GdPicturePDF PdfAPI => _pdfAPI.Value;

        /// <summary>
        /// Get the lazily-instantiated <see cref="GdPictureDocumentConverter"/> instance managed by this object
        /// </summary>
        public GdPictureDocumentConverter DocumentConverter => _documentConverter.Value;

        static GdPictureUtility()
        {
            _licenseManager.RegisterKEY(_LICENSE_KEY);
        }

        /// <summary>
        /// Create an instance, initilize GdPicture API instances
        /// </summary>
        public GdPictureUtility()
        {
            try
            {
                _imagingAPI = new(() => new());
                _pdfAPI = new(() => new());
                _ocrAPI = new(() => new() { ResourceFolder = _TESS_DATA_PATH });
                _documentConverter = new(() => new());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53805");
            }
        }

        // Build an exception with debug data for the current document, etc
        static ExtractException BuildException(string eliCode, string message, GdPictureStatus status, DebugData? debugData)
        {
            var ue = new ExtractException(eliCode, message);
            if (debugData is not null)
            {
                if (!String.IsNullOrWhiteSpace(debugData.FilePath))
                {
                    ue.AddDebugData("File path", debugData.FilePath);
                }
                if (debugData.PageNumber is int pageNumber && pageNumber > 0)
                {
                    ue.AddDebugData("Page number", pageNumber);
                }
            }
            ue.AddDebugData("Status code", status);

            return ue;
        }

        /// <summary>
        /// Throw an exception if the status is not OK
        /// </summary>
        public static void ThrowIfStatusNotOK(GdPictureStatus status, string eliCode, string message, DebugData? debugData = null)
        {
            if (status != GdPictureStatus.OK)
            {
                throw BuildException(eliCode, message, status, debugData);
            }
        }

        /// <summary>
        /// Log an exception if the status-generating function throws or the resulting status is not OK
        /// </summary>
        public static void LogIfStatusNotOK(Func<GdPictureStatus> statusFun, string eliCode, string message, DebugData? debugData = null)
        {
            _ = statusFun ?? throw new ArgumentNullException(nameof(statusFun));

            try
            {
                GdPictureStatus status = statusFun();

                if (status != GdPictureStatus.OK)
                {
                    BuildException(eliCode, message, status, debugData).Log();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog(eliCode);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_imagingAPI.IsValueCreated)
                    {
                        _imagingAPI.Value.Dispose();
                    }
                    if (_pdfAPI.IsValueCreated)
                    {
                        _pdfAPI.Value.Dispose();
                    }
                    if (_ocrAPI.IsValueCreated)
                    {
                        _ocrAPI.Value.Dispose();
                    }
                    if (_documentConverter.IsValueCreated)
                    {
                        _documentConverter.Value.Dispose();
                    }
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
