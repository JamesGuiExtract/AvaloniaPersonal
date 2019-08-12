using Microsoft.Win32.SafeHandles;
using System;
using System.Threading;

namespace Extract.UtilityApplications.PaginationUtility
{
    public partial class DataEntryPanelContainer
    {
        /// <summary>
        /// Manages the threads spawned for <see cref="DataEntryPanelContainer.UpdateDocumentData"/>
        /// in order to ensure the threads all cleanly exit when the form is closed.
        /// </summary>
        class ThreadManager : IDisposable
        {
            /// <summary>
            /// Signals that all <see cref="StartUpdateDocumentStatus"/> threads should be stopped.
            /// </summary>
            ManualResetEvent _stopSignaledEvent = new ManualResetEvent(false);

            /// <summary>
            /// An event that indicates when the document status threads have all finished.
            /// This will start with an initial count of 1 as the event cannot otherwise be incremented.
            /// StopDocumentStatusUpdateThreads will decrement to ensure the counter will go to zero
            /// once all threads are stopped.
            /// </summary>
            CountdownEvent _documentStatusThreadsFinishedEvent = new CountdownEvent(1);

            /// <summary>
            /// Initializes a new instance of the <see cref="ThreadManager"/> class.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public ThreadManager(DataEntryPanelContainer owner)
            {
                Owner = owner;
            }

            /// <summary>
            /// Gets the owning <see cref="DataEntryPanelContainer"/>. By default, the current
            /// container, but for instances spawned into an UpdateDocumentStatus thread, the owner
            /// will be the instance that spawned the thread.
            /// </summary>
            public DataEntryPanelContainer Owner
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets a value indicating whether the threads are currently stopping.
            /// </summary>
            /// <value><c>true</c> if the threads are currently stopping; otherwise, <c>false</c>.
            /// </value>
            public bool StoppingThreads
            {
                get
                {
                    return _stopSignaledEvent.WaitOne(0);
                }
            }

            /// <summary>
            /// Gets a <see cref="WaitHandle"/> that is signaled when <see cref="StopThreads"/> is
            /// called.
            /// </summary>
            public WaitHandle StopEvent
            {
                get
                {
                    return _stopSignaledEvent;
                }
            }

            /// <summary>
            /// Tries to register thread.
            /// </summary>
            /// <returns><c>true</c> if the thread was successfully registered; otherwise, <c>false</c>.
            /// </returns>
            public bool TryRegisterThread()
            {
                return _documentStatusThreadsFinishedEvent.TryAddCount();
            }

            /// <summary>
            /// Signals that a registered thread has ended.
            /// </summary>
            public void SignalThreadEnded()
            {
                _documentStatusThreadsFinishedEvent.Signal();
            }

            /// <summary>
            /// Stops all currently registered threads.
            /// </summary>
            /// <param name="timeout">The timeout.</param>
            public bool StopThreads(int timeout)
            {
                if (!StoppingThreads)
                {
                    _stopSignaledEvent.Set();
                    _documentStatusThreadsFinishedEvent.Signal();
                }

                if (_documentStatusThreadsFinishedEvent.Wait(timeout))
                {
                    _stopSignaledEvent.Reset();
                    _documentStatusThreadsFinishedEvent.Reset(1);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            #region IDisposable Members

            /// <summary>
            /// Releases all resources used by the <see cref="ThreadManager"/>. Also deletes
            /// the temporary file being managed by this class.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <overloads>Releases resources used by the <see cref="ThreadManager"/>.</overloads>
            /// <summary>
            /// Releases all unmanaged resources used by the <see cref="ThreadManager"/>. Also
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
                        if (_documentStatusThreadsFinishedEvent != null)
                        {
                            // Wait up to 10 seconds for any document status threads to finish
                            if (!StopThreads(10000))
                            {
                                ExtractException.Log("ELI41657", "Thread timeout");
                            }

                            _documentStatusThreadsFinishedEvent.Dispose();
                            _documentStatusThreadsFinishedEvent = null;
                        }

                        if (_stopSignaledEvent != null)
                        {
                            _stopSignaledEvent.Dispose();
                            _stopSignaledEvent = null;
                        }
                    }
                    catch { }
                }

                // Dispose of unmanaged resources
            }

            #endregion IDisposable Members
        }
    }
}
