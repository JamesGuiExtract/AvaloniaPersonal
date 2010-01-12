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

                // If the start handle has not been signaled yet, signal it
                // (this will allow the processing threads to exit so that the
                // FAMProcess.exe's will be cleaned up)
                if (_startThreads != null && !_startThreads.WaitOne(0))
                {
                    _startThreads.Set();
                }

                if (_stopProcessing != null)
                {
                    // Check if processing has been stopped, if not stop it
                    if (!_stopProcessing.WaitOne(0, false))
                    {
                        _stopProcessing.Set();
                    }

                    if (_threadsStopped != null)
                    {
                        // Now wait for each thread to exit (set timeout to an hour)
                        _threadsStopped.WaitOne(3600000);
                    }

                    _stopProcessing.Close();
                    _stopProcessing = null;
                }

                if (_threadsStopped != null)
                {
                    // Mutex around closing the event
                    lock (_lock)
                    {
                        _threadsStopped.Close();
                        _threadsStopped = null;
                    }
                }

                if (_startThreads != null)
                {
                    _startThreads.Close();
                    _startThreads = null;
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
