using System;
using System.Security;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient
{
    public class EmailManagementConfiguration : IDisposable
    {
        private bool disposedValue;

        public string UserName { get; set; }
        public SecureString Password { get; set; }
        public FileProcessingDB FileProcessingDB { get; set; }
        public string SharedEmailAddress { get; set; }
        public string QueuedMailFolderName { get; set; }
        public string InputMailFolderName { get; set; }
        public string FilepathToDownloadEmails { get; set; }

        ~EmailManagementConfiguration()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Dispose is public.")]
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }
                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                if (this.Password != null)
                {
                    this.Password.Dispose();
                    this.Password = null;
                }

                // The thread will keep running as long as the process runs if it isn't stopped        
                disposedValue = true;
            }
        }
    }
}