using Extract.Utilities;
using GdPicture14;
using System;
using System.IO;

namespace Extract.GdPicture
{
    public class GdPictureUtility : IDisposable
    {
        const string _LICENSE_KEY = "JdMc805vjgV2_0ds1xxMlyVasmUA5Tm4AWpnwGeGsYIp4W7XkSerQFD_opMlgASTJN2SufUgbYvfUGb9eCTiv9IOGsE4khmUh-84YYGmvXuxF3M67U1g0Pkd_8b24v_WMVzHYrVpxMnKuRf4cw55TT8aK2p9665KBP_9U1cQmxrf4crIn6zDpEKU6kWzednqipmsGqsdK3JcWPQFGWh6VZo-00LhDAvNAUyAblLvbQphGjbylZuTYWfeyDbaE_bPWckekol2qOOCTgq47PbC8bSCPUHi3olFnvnJtIgd0l_pKKQU3hD0jcB10SQJ2Qx7l2FNgjZFbrWmY2b6QVdJ3e6i_cfJNNgtTWcB_TY11Z-BzuwC5_JZ1UiodFDdbra2UYJ45kO69hJhVjWxjYDyVeh1BtWqQfNI2wfERgqtv8BDdks6HIl7kjmY1vD8u36ro_a-ZK90nDEk1aez65fpkM7MiM2a4KA3PbwpoibAv5hAo7oBK_UxqzF_VajIhnSxShWBvIKySrvxs_vxJWJB5z40z6gns6YiZM89AnvDExyUkv6RD-5pbRKiHU_k4olkoXWs52QwL1PGIXlkb3bA4xeJMw_F3z43PiCLgdKErHlRL3d0oF9UxIxcy8NpOgG4cYCmk7nhPQQPQKx201LBFvGkgubghY9lzJY74va59FvLSn6vZ-zm0mR2PfkP9hYsQ0YMU2aprXUslW3opxXE8b5h5U2DB0N_xzqGezpLoonv4FtMdeLjni5_-ABEwZkNWsHTOIeeuChTteKcWY4ydSIAa2Rd6M3chkFuFPC0U5Wv_SLgUPURyqAlbiIbjQhwI48Qlvt714bTZrcZ3u-kvP7XGXBjwGYYU8JOxld9J6jYL4w8tL9FfkeY3eF5nVa78J4o_cMGT8eHomfoqBpdATbmAxLwo_8g1xUGBqF_LYhKlEmt4YxUBEHIzZZArquq2iTRaKrkFpsQAKZ4D39FYGkf869CNkkLpyACX61ORCA=";

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

        public static Version GdPictureVersion { get; }

        static GdPictureUtility()
        {
            try
            {
                _licenseManager.RegisterKEY(_LICENSE_KEY);

                GdPictureVersion = typeof(GdPictureOCR).Assembly.GetName().Version;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI54219");
            }
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
                foreach (var data in debugData.AdditionalDebugData)
                {
                    ue.AddDebugData(data.Key, data.Value);
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
