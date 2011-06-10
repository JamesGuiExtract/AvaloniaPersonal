using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Extract.Utilities
{
    #region Delegates

    /// <summary>
    /// Delegate for function call that has no return value and not arguments
    /// </summary>
    public delegate void FunctionToRunNoArgsNoReturnValue();

    /// <summary>
    /// Delegate for function call that has no return value and one argument
    /// </summary>
    /// <typeparam name="TFirstTArgumentType">Type of first argument</typeparam>
    /// <param name="first">First argument</param>
    public delegate void FunctionToRunOneArgNoReturnValue<TFirstTArgumentType>(TFirstTArgumentType first);

    /// <summary>
    /// Delegate for function call that has no return value and two arguments
    /// </summary>
    /// <typeparam name="TFirstTArgumentType">Type of first argument</typeparam>
    /// <typeparam name="TSecondTArgumentType">Type of second argument</typeparam>
    /// <param name="first">First argument</param>
    /// <param name="second">Second argument</param>
    public delegate void FunctionToRunTwoArgsNoReturnValue<TFirstTArgumentType, TSecondTArgumentType>
       (TFirstTArgumentType first, TSecondTArgumentType second);

    /// <summary>
    /// Delegate for function call that has no return value and three arguemnts
    /// </summary>
    /// <typeparam name="TFirstTArgumentType">Type of first argument</typeparam>
    /// <typeparam name="TSecondTArgumentType">Type of second argument</typeparam>
    /// <typeparam name="TThirdTArgumentType">Type of third argument</typeparam>
    /// <param name="first">First argument</param>
    /// <param name="second">Second argument</param>
    /// <param name="third">Third argument</param>
    // Suppressing message since the idea is to be able to retry even a method that takes 3 arguments
    // this should not be too confusing since the delegate that is "used" should be handled implicitly
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public delegate void
        FunctionToRunThreeArgsNoReturnValue<TFirstTArgumentType, TSecondTArgumentType, TThirdTArgumentType>
        (TFirstTArgumentType first, TSecondTArgumentType second, TThirdTArgumentType third);

    /// <summary>
    /// Delegate for function call that has a return value and no arguments
    /// </summary>
    /// <typeparam name="TReturnType">Return type</typeparam>
    /// <returns>Return value</returns>
    public delegate TReturnType FunctionToRunNoArgs<TReturnType>();

    /// <summary>
    /// Delegate for function call that has a return value and one argument
    /// </summary>
    /// <typeparam name="TReturnType">Return type</typeparam>
    /// <typeparam name="TFirstTArgumentType">First argument type</typeparam>
    /// <param name="first">First argument</param>
    /// <returns>Return value</returns>
    public delegate TReturnType FunctionToRunOneArg<TReturnType, TFirstTArgumentType>(TFirstTArgumentType first);

    /// <summary>
    /// Delegate for function call that has a return value and two arguments
    /// </summary>
    /// <typeparam name="TReturnType">Return type</typeparam>
    /// <typeparam name="TFirstTArgumentType">Type of first argument</typeparam>
    /// <typeparam name="TSecondTArgumentType">Type of second argument</typeparam>
    /// <param name="first">First argument</param>
    /// <param name="second">Second argument</param>
    /// <returns></returns>
    // Suppressing message since the idea is to be able to retry even a method that takes 3 arguments
    // this should not be too confusing since the delegate that is "used" should be handled implicitly
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public delegate TReturnType FunctionToRunTwoArgs<TReturnType, TFirstTArgumentType, TSecondTArgumentType>
        (TFirstTArgumentType first, TSecondTArgumentType second);

    /// <summary>
    /// Delegate for function call that has a return value and three arguments
    /// </summary>
    /// <typeparam name="TReturnType">Return type</typeparam>
    /// <typeparam name="TFirstTArgumentType">First argument type</typeparam>
    /// <typeparam name="TSecondTArgumentType">Second argument type</typeparam>
    /// <typeparam name="TThirdTArgumentType">Third argument type</typeparam>
    /// <param name="first">First argument</param>
    /// <param name="second">Second argument</param>
    /// <param name="third">Third argument</param>
    /// <returns>Return value</returns>
    // Suppressing message since the idea is to be able to retry even a method that takes 3 arguments
    // this should not be too confusing since the delegate that is "used" should be handled implicitly
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public delegate TReturnType
        FunctionToRunThreeArgs<TReturnType, TFirstTArgumentType, TSecondTArgumentType, TThirdTArgumentType>
        (TFirstTArgumentType first, TSecondTArgumentType second, TThirdTArgumentType third);

    #endregion

    /// <summary>
    /// Class to be used to retry method calls.  When the method that is being retried throws an
    /// exception of the type TException the method will be retried until it no longer throws
    /// a TException or the numbe of retries specified in the constructor have been exceeded.
    /// 
    /// The class includes delegates to allow the retry of any method that takes 0 to 3 arguments
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
        /// Retries the method call for method that returns a value and has no arguments until
        ///     - No exception of type TException is thrown
        ///     - Number of retries specified in constructor have been tried
        ///     - EventWaitHandle, if specfied in the constructor, is signaled
        /// </summary>
        /// <typeparam name="TReturnType">Return type of the method delegate being called</typeparam> 
        /// <param name="delegateToUse">Method delegate that will be retried</param>
        public TReturnType DoRetry<TReturnType>(FunctionToRunNoArgs<TReturnType> delegateToUse)
        {
            return (TReturnType)SharedRetry(delegateToUse);
        }

        /// <summary>
        /// Retries the method call for method that does not returns a value and has no arguments until
        ///     - No exception of type TException is thrown
        ///     - Number of retries specified in constructor have been tried
        ///     - EventWaitHandle, if specfied in the constructor, is signaled
        /// </summary>
        /// <param name="delegateToUse">Method delegate that will be retried</param>
        public void DoRetryNoReturnValue(FunctionToRunNoArgsNoReturnValue delegateToUse)
        {
            SharedRetry(delegateToUse);
        }

        /// <summary>
        /// Retries the method call for method that returns a value and has one argument until
        ///     - No exception of type TException is thrown
        ///     - Number of retries specified in constructor have been tried
        ///     - EventWaitHandle, if specfied in the constructor, is signaled
        /// </summary>
        /// <typeparam name="TReturnType">Return type of the method delegate being called</typeparam> 
        /// <typeparam name="TFirstTArgumentType">Type of first argument</typeparam>
        /// <param name="delegateToUse">Method delegate that will be retried</param>
        /// <param name="first">First argument</param>
        public TReturnType DoRetry<TReturnType, TFirstTArgumentType>
            (FunctionToRunOneArg<TReturnType, TFirstTArgumentType> delegateToUse, TFirstTArgumentType first)
        {
            return (TReturnType)SharedRetry(delegateToUse, first);
        }

        /// <summary>
        /// Retries the method call for method that does not return a value and has one argument until
        ///     - No exception of type TException is thrown
        ///     - Number of retries specified in constructor have been tried
        ///     - EventWaitHandle, if specfied in the constructor, is signaled
        /// </summary>
        /// <typeparam name="TFirstTArgumentType">Type of first argument</typeparam>
        /// <param name="delegateToUse">Method delegate that will be retried</param>
        /// <param name="first">First argument</param>
        public void DoRetryNoReturnValue<TFirstTArgumentType>
            (FunctionToRunOneArgNoReturnValue<TFirstTArgumentType> delegateToUse, TFirstTArgumentType first)
        {
            SharedRetry(delegateToUse, first);
        }


        /// <summary>
        /// Retries the method call for method that returns a value and has two arguments until
        ///     - No exception of type TException is thrown
        ///     - Number of retries specified in constructor have been tried
        ///     - EventWaitHandle, if specfied in the constructor, is signaled
        /// </summary>
        /// <typeparam name="TReturnType">Return type of the method delegate being called</typeparam> 
        /// <typeparam name="TFirstTArgumentType">Type of first argument</typeparam>
        /// <typeparam name="TSecondTArgumentType">Type of second argument</typeparam>
        /// <param name="delegateToUse">Method delegate that will be retried</param>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        public TReturnType DoRetry<TReturnType, TFirstTArgumentType, TSecondTArgumentType>
            (FunctionToRunTwoArgs<TReturnType, TFirstTArgumentType, TSecondTArgumentType> delegateToUse,
            TFirstTArgumentType first, TSecondTArgumentType second)
        {
            try
            {
                return (TReturnType)SharedRetry(delegateToUse, first, second);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32634");
            }
        }

        /// <summary>
        /// Retries the method call for method that does not returns a value and has two arguments until
        ///     - No exception of type TException is thrown
        ///     - Number of retries specified in constructor have been tried
        ///     - EventWaitHandle, if specfied in the constructor, is signaled
        /// </summary>
        /// <typeparam name="TFirstTArgumentType">Type of first argument</typeparam>
        /// <typeparam name="TSecondTArgumentType">Type of second argument</typeparam>
        /// <param name="delegateToUse">Method delegate that will be retried</param>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        public void DoRetryNoReturnValue<TFirstTArgumentType, TSecondTArgumentType>
            (FunctionToRunTwoArgsNoReturnValue<TFirstTArgumentType, TSecondTArgumentType> delegateToUse,
            TFirstTArgumentType first, TSecondTArgumentType second)
        {
            try
            {
                SharedRetry(delegateToUse, first, second);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32635");
            }
        }

        /// <summary>
        /// Retries the method call for method that returns a value and has three arguments until
        ///     - No exception of type TException is thrown
        ///     - Number of retries specified in constructor have been tried
        ///     - EventWaitHandle, if specfied in the constructor, is signaled
        /// </summary>
        /// <typeparam name="TReturnType">Return type of the method delegate being called</typeparam>
        /// <typeparam name="TFirstTArgumentType">Type of first argument</typeparam>
        /// <typeparam name="TSecondTArgumentType">Type of second argument</typeparam>
        /// <typeparam name="TThirdTArgumentType"></typeparam>
        /// <param name="delegateToUse">Method delegate that will be retried</param>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        /// <param name="third">Third argument</param>
        /// <returns>Return value of the delegate called</returns>
        public TReturnType DoRetry<TReturnType, TFirstTArgumentType, TSecondTArgumentType, TThirdTArgumentType>
            (FunctionToRunThreeArgs<TReturnType, TFirstTArgumentType, TSecondTArgumentType, TThirdTArgumentType> delegateToUse,
            TFirstTArgumentType first, TSecondTArgumentType second, TThirdTArgumentType third)
        {
            try
            {
                return (TReturnType)SharedRetry(delegateToUse, first, second, third);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32636");
            }
        }

        /// <summary>
        /// Retries the method call for method that does not return a value and has three arguments until
        ///     - No exception of type TException is thrown
        ///     - Number of retries specified in constructor have been tried
        ///     - EventWaitHandle, if specfied in the constructor, is signaled
        /// </summary>
        /// <typeparam name="TFirstTArgumentType">Type of first argument</typeparam>
        /// <typeparam name="TSecondTArgumentType">Type of second argument</typeparam>
        /// <typeparam name="TThirdTArgumentType"></typeparam>
        /// <param name="delegateToUse">Method delegate that will be retried</param>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        /// <param name="third">Third argument</param>
        public void DoRetryNoReturnValue<TFirstTArgumentType, TSecondTArgumentType, TThirdTArgumentType>
            (FunctionToRunThreeArgsNoReturnValue<TFirstTArgumentType, TSecondTArgumentType, TThirdTArgumentType> delegateToUse,
            TFirstTArgumentType first, TSecondTArgumentType second, TThirdTArgumentType third)
        {
            try
            {
                SharedRetry(delegateToUse, first, second, third);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32637");
            }
        }

        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Method that invokes the delegate using the parameters passed
        /// </summary>
        /// <param name="anyDelegate">Any of the defined delegates</param>
        /// <param name="parameterList">List of arguments to pass to the method called</param>
        /// <returns>Return value of the method</returns>
        object SharedRetry(MulticastDelegate anyDelegate, params object[] parameterList)
        {
            int numberOfRetries = 0;
            do
            {
                try
                {
                    object returnObect = anyDelegate.DynamicInvoke(parameterList);
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
