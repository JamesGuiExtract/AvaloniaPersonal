using System;
using System.Windows;

namespace Extract.Utilities.WPF
{
    public class GlobalErrorHandler : IObserver<Exception>
    {
        readonly string _eliCode;
        readonly Func<Window?> _getMainWindow;

        /// <summary>
        /// Create an error handler that will display exceptions
        /// </summary>
        /// <param name="eliCode">The extract code to use for displaying exceptions</param>
        public GlobalErrorHandler(string eliCode)
            : this(eliCode, () => null)
        {
        }

        /// <summary>
        /// Create an error handler that will display exceptions by invoking
        /// on the dispatcher of the specified window
        /// </summary>
        /// <param name="eliCode">The extract code to use for displaying exceptions</param>
        /// <param name="getMainWindow">A function to get a window via which to invoke the exceptions</param>
        public GlobalErrorHandler(string eliCode, Func<Window?> getMainWindow)
        {
            _eliCode = eliCode;
            _getMainWindow = getMainWindow;
        }

        /// <inheritdoc/>
        public void OnNext(Exception value)
        {
            if (_getMainWindow() is Window window)
            {
                window.Dispatcher.BeginInvoke(() =>
                    value.ExtractDisplay(_eliCode));
            }
            else
            {
                value.ExtractDisplay(_eliCode);
            }
        }

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            try
            {
                error.ExtractLog("ELI53779");
            }
            catch { }
        }

        /// <inheritdoc/>
        public void OnCompleted() { }
    }
}
