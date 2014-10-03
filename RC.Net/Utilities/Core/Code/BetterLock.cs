using Extract.Licensing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Extract.Utilities
{
    /// <summary>
    /// A class that can be used in place of a basic object to lock. Provides expanded abilities
    /// such as the ability to wait for the lock but be able to cancel the wait when an event is
    /// signaled.
    /// </summary>
    public class BetterLock : IDisposable
    {
        #region Constants

        /// <summary>
        /// The object name used in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(BetterLock).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The underlying object that will be used to provide absolute enforcement of the lock.
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// An event that protects access to _lock to allow for expanded locking behaviors.
        /// </summary>
        AutoResetEvent _lockEvent = new AutoResetEvent(true);

        /// <summary>
        /// The ID of the thread that currently owns the lock. No other thread is permitted access
        /// to the lock while this thread owns the lock.
        /// </summary>
        int? _lockingThreadID;

        /// <summary>
        /// The number of Lock calls that need corresponding Unlock calls before the lock is
        /// relinquished.
        /// </summary>
        int _referenceCount;

        /// <summary>
        /// <see langword="true"/> if the object has been disposed; otherwise,
        /// <see langword="false"/>.
        /// </summary>
        bool _disposed;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterLock"/> class.
        /// </summary>
        public BetterLock()
        {
            try
            {
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI37523", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37524");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this <see cref="BetterLock"/> is currently locked.
        /// </summary>
        /// <value><see langword="true"/> if this instance is locked; otherwise,
        /// <see langword="false"/>.</value>
        public bool Locked
        {
            get
            {
                return _lockingThreadID.HasValue;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Obtains the lock if currently available, otherwise returns <see langword="false"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the lock was obtained, <see langword="false"/> if
        /// another thread already owns the lock.</returns>
        public bool TryLock()
        {
            try
            {
                bool lockTaken = false;
                return InnerLock(ref lockTaken, 0);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37525");
            }
        }

        /// <summary>
        /// Blocks until either the lock can be obtained or one of the specified
        /// <see paramref="abortEvents"/> is signaled.
        /// </summary>
        /// <param name="cancelWaitEvents">One of more <see cref="EventWaitHandle"/>s that should
        /// cancel the wait for the lock if signaled.</param>
        /// <returns><see langword="true"/> if the lock is acquired; otherwise, the output is
        /// <see langword="false"/>.</returns>
        public bool Lock(params EventWaitHandle[] cancelWaitEvents)
        {
            try
            {
                bool lockTaken = false;
                return InnerLock(ref lockTaken, Timeout.Infinite, cancelWaitEvents);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37538");
            }
        }

        /// <summary>
        /// Blocks until either the lock can be obtained or one of the specified
        /// <see paramref="abortEvents"/> is signaled.
        /// </summary>
        /// <param name="lockTaken">The input must be <see langword="false"/>. The output is
        /// <see langword="true"/> if the lock is acquired; otherwise, the output is
        /// <see langword="false"/>. The output is set even if an exception occurs during the
        /// attempt to acquire the lock.</param>
        /// <param name="cancelWaitEvents">One of more <see cref="EventWaitHandle"/>s that should
        /// cancel the wait for the lock if signaled.</param>
        /// <returns><see langword="true"/> if the lock is acquired; otherwise, the output is
        /// <see langword="false"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        public bool Lock(ref bool lockTaken, params EventWaitHandle[] cancelWaitEvents)
        {
            try
            {
                return InnerLock(ref lockTaken, Timeout.Infinite, cancelWaitEvents);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37526");
            }
        }

        /// <summary>
        /// Releases the lock if the calling thread owns the lock, and has made an equal number of
        /// Lock and Unlock calls.
        /// </summary>
        public void Unlock()
        {
            try
            {
                InnerUnlock();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37527");
            }
        }

        /// <summary>
        /// Gets a <see cref="IDisposable"/> object that can be used to guarantee the release of a
        /// <see cref="BetterLock"/> instance when going out of scope.
        /// </summary>
        /// <returns>The <see cref="IDisposable"/> object that will release the lock when disposed.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IDisposable GetDisposableScopeLock()
        {
            try
            {
                return new BetterLockScopeLock(this);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37528");
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="BetterLock"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="BetterLock"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BetterLock"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                try
                {
                    _disposed = true;

                    if (!_disposed)
                    {
                        if (Locked && _lockingThreadID == Thread.CurrentThread.ManagedThreadId)
                        {
                            _lockingThreadID = null;
                            _referenceCount = 0;

                            Monitor.Exit(_lock);

                            _lockEvent.Set();
                        }

                        _lockEvent.Dispose();
                        _lockEvent = null;
                    }
                }
                catch { }
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Blocks until the lock can be obtained, the specified timeout is reached or one of the
        /// specified <see paramref="abortEvents"/> is signaled.
        /// </summary>
        /// <param name="lockTaken">The input must be <see langword="false"/>. The output is
        /// <see langword="true"/> if the lock is acquired; otherwise, the output is
        /// <see langword="false"/>. The output is set even if an exception occurs during the
        /// attempt to acquire the lock.</param>
        /// <param name="timeout">The number of milliseconds to wait to obtain the lock or
        /// <see cref="Timeout.Infinite"/> to wait indefinitely.</param>
        /// <param name="cancelWaitEvents">One of more <see cref="EventWaitHandle"/>s that should
        /// cancel the wait for the lock if signaled.</param>
        bool InnerLock(ref bool lockTaken, int timeout, params EventWaitHandle[] cancelWaitEvents)
        {
            ExtractException.Assert("ELI37529", "Invalid initial state lock.", !lockTaken);
            ExtractException.Assert("ELI37530", "Unable to lock disposed object.", !_disposed);

            // If this thread already owns the lock, simply increment the reference count and return
            // immediately.
            if (Locked && _lockingThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                _referenceCount++;
                lockTaken = true;
                return true;
            }

            // Block until the lock is available or one of the cancelWaitEvents is signaled.
            EventWaitHandle[] events = new[] { _lockEvent }.Union(cancelWaitEvents).ToArray();
            if (EventWaitHandle.WaitAny(events, timeout) == 0)
            {
                Monitor.TryEnter(_lock, ref lockTaken);
                // Since _lockEvent is an AutoResetEvent, only a single thread should be allowed in
                // this block at once. _lock should be available.
                ExtractException.Assert("ELI37531", "Unable to obtain lock.", lockTaken);

                _lockingThreadID = Thread.CurrentThread.ManagedThreadId;
                _referenceCount++;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Releases the lock if the calling thread owns the lock, and has made an equal number of
        /// Lock and Unlock calls.
        /// </summary>
        void InnerUnlock()
        {
            ExtractException.Assert("ELI37532", "Unable to release lock that is not locked.", Locked);
            ExtractException.Assert("ELI37533", "Unable to release lock obtained on a different thread.",
                _lockingThreadID == Thread.CurrentThread.ManagedThreadId);

            if (_referenceCount > 1)
            {
                // More Unlock calls are needed before the lock is relinquished.
                _referenceCount--;
            }
            else if (_referenceCount == 1)
            {
                _lockingThreadID = null;
                _referenceCount = 0;
                Monitor.Exit(_lock);

                // Set _lockEvent to allow the next thread seeking the lock to get it.
                _lockEvent.Set();
            }
            else
            {
                ExtractException.ThrowLogicException("ELI37534");
            }
        }

        #endregion Private Members
    }

    /// <summary>
    /// Provides an <see cref="IDisposable"/> object that can be used to guarantee the release of a
    /// <see cref="BetterLock"/> instance when going out of scope.
    /// </summary>
    public class BetterLockScopeLock : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="BetterLock"/> instance whose lock status is being managed.
        /// </summary>
        BetterLock _betterLock;

        /// <summary>
        /// <see langword="true"/> if the lock of <see cref="_betterLock"/> is currently owned.
        /// </summary>
        bool _lockTaken;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterLockScopeLock"/> class.
        /// </summary>
        /// <param name="betterLock">The <see cref="BetterLock"/> instance whose lock status is
        /// being managed.</param>
        public BetterLockScopeLock(BetterLock betterLock)
        {
            try
            {
                _betterLock = betterLock;
                _betterLock.Lock(ref _lockTaken);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37535");
            }
        }

        #endregion Constructors

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="BetterLockScopeLock"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BetterLockScopeLock"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                if (_lockTaken)
                {
                    _lockTaken = false;
                    _betterLock.Unlock();
                }
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable Members
    }
}
