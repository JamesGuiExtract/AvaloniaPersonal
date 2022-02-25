using System;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// Helper class to automatically end a file task session on Dispose
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(false)]
    public class DisposableFileTaskSession : IDisposable
    {
        private bool _isDisposed;
        readonly IFileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the FileTaskSession that this instance represents
        /// </summary>
        public int SessionID { get; }

        /// <summary>
        /// Create an instance with an existing file task session
        /// </summary>
        public DisposableFileTaskSession(IFileProcessingDB fileProcessingDB, int sessionID)
        {
            _fileProcessingDB = fileProcessingDB;
            SessionID = sessionID;
        }

        // Close the session whether disposing or finalizing
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                try
                {
                    _fileProcessingDB?.EndFileTaskSession(SessionID, 0, 0, false);
                }
                catch { }

                _isDisposed = true;
            }
        }

        ~DisposableFileTaskSession()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
