using System.Threading;

namespace Extract.FileActionManager.Utilities
{
    partial class ESFAMService
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_stopProcessing != null)
                {
                    // Check if processing has been stopped, if not stop it
                    if (!_stopProcessing.WaitOne(0, false))
                    {
                        _stopProcessing.Set();

                        if (_threadStopped != null && _threadStopped.Length > 0)
                        {
                            // Now wait for each thread to exit (set timeout to an hour)
                            WaitHandle.WaitAll(_threadStopped, 3600000);
                        }
                    }

                    _stopProcessing.Close();
                    _stopProcessing = null;
                }

                if (_threadStopped != null)
                {
                    // Mutex around closing the events
                    lock (_lock)
                    {
                        for (int i = 0; i < _threadStopped.Length; i++)
                        {
                            if (_threadStopped[i] != null)
                            {
                                _threadStopped[i].Close();
                                _threadStopped[i] = null;
                            }
                        }
                    }
                }

                if (_dnsStarted != null)
                {
                    _dnsStarted.Close();
                    _dnsStarted = null;
                }
                if (_netLogonStarted != null)
                {
                    _netLogonStarted.Close();
                    _netLogonStarted = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.ServiceName = "Extract Systems FAM Service";
        }

        #endregion
    }
}
