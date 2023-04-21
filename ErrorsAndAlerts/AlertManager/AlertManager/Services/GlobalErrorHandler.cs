using Avalonia.Controls;
using Extract.ErrorHandling;
using MessageBox.Avalonia;
using System;

namespace AlertManager.Services
{
    /// <summary>
    /// Handle uncaught exceptions by displaying a message box dialog
    /// </summary>
    public class GlobalErrorHandler : IObserver<Exception>
    {
        readonly Func<Window?> _getMainWindow;

        public GlobalErrorHandler(Func<Window?> getMainWindow)
        {
            _getMainWindow = getMainWindow;
        }

        public void OnNext(Exception value)
        {
            var messageBoxStandardWindow = MessageBoxManager
              .GetMessageBoxStandardWindow("Error", value?.Message);

            value?.AsExtractException("ELI54069").Log();

            if (_getMainWindow() is Window window)
            {
                messageBoxStandardWindow.ShowDialog(window);
            }
            else
            {
                messageBoxStandardWindow.Show();
            }
        }

        public void OnError(Exception error) { }

        public void OnCompleted() { }
    }
}
