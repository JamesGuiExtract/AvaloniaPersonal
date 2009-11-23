using System;
using System.Threading;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents a mechanism for synchronizing access to PDF loading and saving.
    /// </summary>
    sealed class PdfLock : IDisposable
    {
        #region Fields

        /// <summary>
        /// Locked if a PDF is being loaded or saved; otherwise unlocked.
        /// </summary>
        static readonly object _lock = new object();

        /// <summary>
        /// <see langword="true"/> if this <see cref="PdfLock"/> owns an exclusive lock to PDF 
        /// loading and saving; <see langword="false"/> if this <see cref="PdfLock"/> does not 
        /// own an exclusive lock.
        /// </summary>
        bool _isLocked;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLock"/> class.
        /// </summary>
        public PdfLock()
            : this(true)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLock"/> class.
        /// </summary>
        /// <param name="isLocked"><see langword="true"/> to gain exclusive access to PDF loading 
        /// and saving until <see cref="Dispose()"/> is called; <see langword="false"/> to do 
        /// nothing.</param>
        public PdfLock(bool isLocked)
        {
            if (isLocked)
            {
                Monitor.Enter(_lock);
                _isLocked = true;
            }
        }

        #endregion Constructors

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="PdfLock"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="PdfLock"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="PdfLock"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_isLocked)
                {
                    _isLocked = false;
                    Monitor.Exit(_lock);
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

    }
}