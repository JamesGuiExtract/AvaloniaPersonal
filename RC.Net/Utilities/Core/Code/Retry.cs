using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Extract.Utilities
{
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
        EventWaitHandle _eventToWaitFor;

        #endregion

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
            _eventToWaitFor = null;
        }

        /// <summary>
        /// Initializes the Retry instance
        /// </summary>
        /// <param name="retryCount">Number of time to attempt a method call</param>
        /// <param name="timeBetweenRetries">Number of milliseconds to wait between method call attempts</param>
        /// <param name="eventToWaitFor">Event to wait for between method call attempts and if signalled
        /// will cause no further attempts to be made.</param>
        public Retry(int retryCount, int timeBetweenRetries, EventWaitHandle eventToWaitFor)
        {
            _retryCount = retryCount;
            _timeBetweenRetries = timeBetweenRetries;
            _eventToWaitFor = eventToWaitFor;
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
                catch (TExceptionType ex)
                {
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
                    if (_eventToWaitFor == null)
                    {
                        Thread.Sleep(_timeBetweenRetries);
                    }
                    else if (_eventToWaitFor.WaitOne(_timeBetweenRetries))
                    {
                        ExtractException ee = new ExtractException("ELI32546",
                            "Application Trace: Event signaled while waiting to retry.", ex);
                        throw ee;
                    }
                }
            }
            while (true);
        }
        
        #endregion
    }
}
