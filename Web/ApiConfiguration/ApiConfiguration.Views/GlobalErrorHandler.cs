using System;

namespace Extract.Web.ApiConfiguration.Views
{
    /// <summary>
    /// Handle uncaught exceptions by displaying as ExtractException
    /// </summary>
    public class GlobalErrorHandler : IObserver<Exception>
    {
        public void OnNext(Exception value)
        {
            value.ExtractDisplay("ELI53778");
        }

        public void OnError(Exception error)
        {
            try
            {
                error.ExtractLog("ELI53779");
            }
            catch { }
        }

        public void OnCompleted() { }
    }
}
