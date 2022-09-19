using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;

namespace Extract.Utilities
{
    /// <summary>
    /// An <see cref="EventArgs"/> override that provides information for a 
    /// <see cref="Retry{TExceptionType}.AttemptFailed"/> event.
    /// </summary>
    public class AttemptFailedEventArgs<TExceptionType> : EventArgs where TExceptionType : Exception
    {
        /// <summary>
        /// The <see typeparam="TExceptionType"/> from the failure.
        /// </summary>
        TExceptionType _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttemptFailedEventArgs{TExceptionType}"/>
        /// class.
        /// </summary>
        /// <param name="exception">The <see typeparam="TExceptionType"/> from the failure.</param>
        public AttemptFailedEventArgs(TExceptionType exception)
        {
            try
            {
                _exception = exception;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI34092",
                    "Failed to initialize AttemptFailedEventArgs!", ex);
            }
        }

        /// <summary>
        /// Gets the <see typeparam="TExceptionType"/> that occurred.
        /// </summary>
        /// <returns>The <see typeparam="TExceptionType"/> that occurred.</returns>
        public TExceptionType Exception
        {
            get
            {
                return _exception;
            }
        }
    }

    /// <summary>
    /// Class to be used to retry method calls.  When the method that is being retried throws an
    /// exception of the type TException the method will be retried until it no longer throws
    /// a TException or the numbe of retries specified in the constructor have been exceeded.
    /// </summary>
    /// <typeparam name="TExceptionType">Exception type to retry the method call</typeparam>
    public class Retry<TExceptionType> where TExceptionType : Exception
    {
        #region Fields

        /// <summary>
        /// Maximum number of retries
        /// </summary>
        int _retryCount;

        /// <summary>
        /// The time to wait between retries
        /// </summary>
        int _timeBetweenRetries;

        /// <summary>
        /// Optional event to wait for
        /// </summary>
        WaitHandle _handleToWaitFor;

        #endregion Fields

        #region Events

        /// <summary>
        /// Raised when an attempt fails.
        /// <para><b>Note</b></para>
        /// Any exceptions thrown from this handler will be thrown immediatly from
        /// <see cref="DoRetry"/> thereby preventing additional retry attempts.
        /// </summary>
        public event EventHandler<AttemptFailedEventArgs<TExceptionType>> AttemptFailed;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes the Retry instance
        /// </summary>
        /// <param name="retryCount">Number of time to attempt a method call</param>
        /// <param name="timeBetweenRetries">Number of milliseconds to wait between method call attempts</param>
        public Retry(int retryCount, int timeBetweenRetries)
        {
            _retryCount = retryCount;
            _timeBetweenRetries = timeBetweenRetries;
            _handleToWaitFor = null;
        }

        /// <summary>
        /// Initializes the Retry instance
        /// </summary>
        /// <param name="retryCount">Number of time to attempt a method call</param>
        /// <param name="timeBetweenRetries">Number of milliseconds to wait between method call attempts</param>
        /// <param name="handleToWaitFor">Event to wait for between method call attempts and if signalled
        /// will cause no further attempts to be made.</param>
        public Retry(int retryCount, int timeBetweenRetries, WaitHandle handleToWaitFor)
        {
            _retryCount = retryCount;
            _timeBetweenRetries = timeBetweenRetries;
            _handleToWaitFor = handleToWaitFor;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retries the <see paramref="function"/> until
        ///     - No exception of type TException is thrown
        ///     - Number of retries specified in constructor have been tried
        ///     - EventWaitHandle, if specfied in the constructor, is signaled
        /// </summary>
        /// <param name="function">The <see cref="Func{T}"/> to retry.</param>
        /// <typeparam name="T">Return type of <see paramref="function"/>.</typeparam>
        /// <returns>The return value of the <see paramref="function"/>.</returns>
        public T DoRetry<T>(Func<T> function)
        {
            return (T)SharedRetry(function);
        }

        /// <summary>
        /// Retries the <see paramref="action"/> until
        ///     - No exception of type TException is thrown
        ///     - Number of retries specified in constructor have been tried
        ///     - EventWaitHandle, if specfied in the constructor, is signaled
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to retry.</param>
        public void DoRetry(Action action)
        {
            SharedRetry(action);
        }

        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Method that invokes the delegate.
        /// </summary>
        /// <param name="anyDelegate">The delegate to invoke.</param>
        /// <returns>Return value of the delegate.</returns>
        object SharedRetry(MulticastDelegate anyDelegate)
        {
            int numberOfRetries = 0;
            do
            {
                try
                {
                    object returnObect = anyDelegate.DynamicInvoke();
                    if (numberOfRetries > 0)
                    {
                        ExtractException ee = new ExtractException("ELI32553", "Application Trace: Retry was successful.");
                        ee.AddDebugData("RetriesAttempted", numberOfRetries, false);
                        ee.Log();
                    }
                    return returnObect;
                }
                catch (TargetInvocationException invokeExn) when (invokeExn.InnerException is TExceptionType ex)
                {
                    // This is intentionally not wrapped in a try/catch block. Any exceptions in the
                    // handler should abort addtional retry attempts.
                    OnAttemptFailed(ex);

                    // If the _retryCount is 0 there is nothing else to do so rethrow the exception.
                    if (_retryCount == 0)
                    {
                        throw ex.AsExtract("ELI32638");
                    }

                    if (numberOfRetries == 0)
                    {
                        // Add application trace in exception log that retry is happening
                        ExtractException retryStarting = new ExtractException("ELI32552",
                            "Application Trace: Attempting to retry operation.", ex);
                        retryStarting.Log();
                    }

                    if (numberOfRetries >= _retryCount)
                    {
                        ExtractException ee = new ExtractException("ELI32545", "Retry attempt was unsuccessful.", ex);
                        ee.AddDebugData("RetryAttempts", numberOfRetries, false);
                        throw ee;
                    }

                    numberOfRetries++;
                    if (_handleToWaitFor == null)
                    {
                        Thread.Sleep(_timeBetweenRetries);
                    }
                    else if (_handleToWaitFor.WaitOne(_timeBetweenRetries))
                    {
                        ExtractException ee = new ExtractException("ELI32546",
                            "Application Trace: Event signaled while waiting to retry.", ex);
                        throw ee;
                    }
                }
            }
            while (true);
        }

        /// <summary>
        /// Raises the <see cref="AttemptFailed"/> event.
        /// </summary>
        /// <param name="ex">The <see typeparam="TExceptionType"/> from the failure.</param>
        void OnAttemptFailed(TExceptionType ex)
        {
            if (AttemptFailed != null)
            {
                AttemptFailed(this, new AttemptFailedEventArgs<TExceptionType>(ex));
            }
        }
        
        #endregion Private Methods
    }
}
