using System;
using System.IO;

namespace Extract.SharePoint
{
    /// <summary>
    /// Class for managing a lock file on disk. The lock file can be used
    /// to enforce a singleton behavior among threads or for thread
    /// synchronization.
    /// </summary>
    internal class LockFileManager : IDisposable
    {
        #region Fields

        /// <summary>
        /// Handle to the filestream being opened.
        /// </summary>
        FileStream _lockedFile;

        /// <summary>
        /// The name of the file that was opened.
        /// </summary>
        string _lockedFileName;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Attempts to create the specified lock file.
        /// </summary>
        /// <param name="fileName">The lock file to create.</param>
        /// <returns><see langword="true"/> if the lock file was created
        /// and <see langword="false"/> otherwise.</returns>
        public bool TryCreateLockFile(string fileName)
        {
            try
            {
                string directory = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(fileName))
                {
                    _lockedFile = new FileStream(fileName, FileMode.CreateNew,
                        FileAccess.ReadWrite, FileShare.None);
                    _lockedFileName = fileName;
                    return true;
                }

                return false;
            }
            // If the exception is an IO exception, just eat it, this indicates
            // the file already exists
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// Closes the opened stream for the lock file and deletes the file.
        /// If no lock file has been opened this method does nothing.
        /// </summary>
        public void CloseAndDeleteLockFile()
        {
            if (_lockedFile != null)
            {
                _lockedFile.Dispose();
                _lockedFile = null;
                File.Delete(_lockedFileName);
                _lockedFileName = string.Empty;
            }

        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Disposes of both managed and unmanaged objects.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of both managed and unmanaged objects.
        /// </summary>
        /// <param name="disposing">If true, disposes of managed objects.</param>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseAndDeleteLockFile();
            }
        }

        #endregion
    }
}
