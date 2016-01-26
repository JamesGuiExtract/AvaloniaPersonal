using System;
using UCLID_COMUTILSLib;

namespace Extract.Utilities
{
    /// <summary>
    /// https://extract.atlassian.net/browse/ISSUE-13573
    /// This class is used to protect a file against modification from another Extract Systems process,
    /// thread or task by using a hidden file with the same name the target file but ending ".ExtractLock"
    /// <para><b>Note:</b></para>
    /// Presently this class will only actually attempt to lock a file if being used internally at
    ///	Extract Systems.
    /// A ".ExtractLock" file will not be produced locked if it is read-only.
    /// A file can be locked even if it does not yet exist to protect against a race condition for access
    /// to a file that is about to be written.
    /// FileSupplyingMgmtRole as been hardcoded to ignore ".ExtractLock" files to ensure they are not
    /// inadvertently queued.
    /// </summary>
    public class ExtractFileLock : IDisposable
    {
        #region Fields

        /// <summary>
        /// Allows access to c++ ExtractFileLock class which provides the underlying implementation.
        /// </summary>
        IMiscUtils _miscUtils = new MiscUtils();

        /// <summary>
        /// Pointer used by _miscUtils to reference the c++ ExtractFileLock instance that is
        /// providing the file lock implementation.
        /// </summary>
        IntPtr _nativeFileLock;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractFileLock"/> class without taking out
        /// a lock on any file.
        /// </summary>
        public ExtractFileLock()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractFileLock"/> class by taking out a
        /// lock on <see paramref="fileName"/> if possible. If another processes already has the
        /// file locked, an exception will be thrown.
        /// </summary>
        /// <param name="fileName">Name of the file to lock.</param>
        public ExtractFileLock(string fileName) : this (fileName, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractFileLock"/> class by taking out a
        /// lock on <see paramref="fileName"/> if possible. If another processes already has the
        /// file locked, an exception will be thrown.
        /// </summary>
        /// <param name="fileName">Name of the file to lock.</param>
        /// <param name="context">What should be reported to other processes as having the file
        /// locked.</param>
        public ExtractFileLock(string fileName, string context)
        {
            try
            {
                _nativeFileLock = _miscUtils.CreateExtractFileLock(fileName, context);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39248");
            }
        }

        #endregion Constructors

        #region Finalizer

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ExtractFileLock"/> is reclaimed by garbage collection.
        /// </summary>
        ~ExtractFileLock()
        {
            Dispose(false);
        }

        #endregion Finalizer

        #region Methods

        /// <summary>
        /// Attempts to lock <see paramref="fileName"/> if possible. If another processes already
        /// has the file locked, an exception will be thrown. Any other file that is currently
        /// locked by this instance will be released. Can be called repeatedly for the same file
        /// without first releasing the lock.
        /// </summary>
        /// <param name="fileName">Name of the file to lock.</param>
        public void GetLock(string fileName)
        {
            GetLock(fileName, "");
        }

        /// <summary>
        /// Attempts to lock <see paramref="fileName"/> if possible. If another processes already
        /// has the file locked, an exception will be thrown. Any other file that is currently
        /// locked by this instance will be released. Can be called repeatedly for the same file
        /// without first releasing the lock.
        /// </summary>
        /// <param name="fileName">Name of the file to lock.</param>
        /// <param name="context">What should be reported to other processes as having the file
        /// locked.</param>
        public void GetLock(string fileName, string context)
        {
            try
            {
                if (_nativeFileLock != IntPtr.Zero)
                {
                    if (_miscUtils.IsExtractFileLockForFile(_nativeFileLock, fileName))
                    {
                        return;
                    }
                    else
                    {
                        ReleaseLock();
                    }
                }
                
                _nativeFileLock = _miscUtils.CreateExtractFileLock(fileName, context);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI39249");
                ee.AddDebugData("Filename", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Releases any lock currently held by this instance.
        /// </summary>
        public void ReleaseLock()
        {
            try
            {
                if (_nativeFileLock != IntPtr.Zero)
                {
                    IntPtr nativeFileLock = _nativeFileLock;
                    _nativeFileLock = IntPtr.Zero;
                    _miscUtils.DeleteExtractFileLock(nativeFileLock);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39250");
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ExtractFileLock"/>. This will release any
        /// lock currently held by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ExtractFileLock"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ExtractFileLock"/>. This will
        /// release any lock currently held by this instance.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            try
            {
                // Dispose of managed resources
                if (disposing)
                {
                }

                // Dispose of unmanaged resources
                ReleaseLock();
            }
            catch { }
        }

        #endregion IDisposable Members
    }
}
