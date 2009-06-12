using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// A thread-safe class used to funnel calls from multiple threads to a single 
    /// <see cref="DataEntryApplicationForm"/> instance and be able to route exceptions back to
    /// to the calling thread.
    /// </summary>
    class DataEntryApplicationFormManager : IDisposable
    {
        /// <summary>
        /// If an exception forces the UI thread to be aborted, this is the number of milliseconds
        /// the thread should be given to close cleanly upon request.  If the UI thread has not 
        /// exited after this amount of time, it will be forcefully aborted.
        /// </summary>
        private static readonly int _THREAD_TIMEOUT = 10000;

        #region Fields

        /// <summary>
        /// The thread for the <see cref="DataEntryApplicationForm"/>'s UI.
        /// </summary>
        private Thread _dataEntryApplicationThread;

        /// <summary>
        /// The DataEntryApplicationForm instance to be used for the 
        /// <see cref="IFileProcessingTask"/> interface.
        /// </summary>
        volatile DataEntryApplicationForm _dataEntryForm;

        /// <summary>
        /// The last exception passed out of the <see cref="DataEntryApplicationForm"/> via the
        /// ExceptionGenerated event.
        /// </summary>
        private volatile ExtractException _lastException;

        /// <summary>
        /// An event to indicate the <see cref="DataEntryApplicationForm"/> has been created.
        /// </summary>
        private EventWaitHandle _initializedEvent = new ManualResetEvent(false);

        /// <summary>
        /// An event to indicate a document has been saved (output) in the 
        /// <see cref="DataEntryApplicationForm"/>.
        /// </summary>
        private EventWaitHandle _documentSavedEvent = new AutoResetEvent(false);

        /// <summary>
        /// An event to indicate processing has been cancelled either via 
        /// IFileProcessingTask.Cancel or as a result of the 
        /// <see cref="DataEntryApplicationForm"/> being closed.
        /// </summary>
        private EventWaitHandle _cancelledEvent = new ManualResetEvent(false);

        /// <summary>
        /// An event to indicate an exception was passed out of the 
        /// <see cref="DataEntryApplicationForm"/> via the ExceptionGenerated event.
        /// </summary>
        private EventWaitHandle _exceptionThrownEvent = new ManualResetEvent(false);

        /// <summary>
        /// An event to indicate that the <see cref="DataEntryApplicationForm"/> has been 
        /// successfully closed and is ready to be re-initialized.
        /// </summary>
        private EventWaitHandle _closedEvent = new ManualResetEvent(false);

        /// <summary>
        /// If <see langword="true"/>, the <see cref="DataEntryApplicationForm"/> is in the process
        /// of being closed.  ShowDocument should not be called when in this state and 
        /// any call to ShowForm needs to wait for the previous form to finish closing
        /// </summary>
        private volatile bool _closing;

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Delegate for a function that takes a single <see langref="string"/> as a paramater.
        /// </summary>
        /// <param name="value">The parameter for the delegate method.</param>
        private delegate void StringParameterDelegate(string value);

        /// <summary>
        /// Delegate for a function that does not take any parameters.
        /// </summary>
        private delegate void ParameterlessDelegate();

        #endregion Delegates

        #region Contructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryApplicationFormManager"/> instance.
        /// </summary>
        public DataEntryApplicationFormManager()
        {
            try
            {
                // Initialize licensing (load from folder since caller is likely to be using
                // this class as a static and it may therefore initialize before the caller's
                // constructor code).
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects,
                    "ELI24047", Application.ProductName);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23791", ex);
            }
        }

        #endregion Contructors

        #region Methods

        /// <summary>
        /// Ensures the specified config file has valid settings by attempting to initialize an
        /// instance of the DEP using them.
        /// </summary>
        /// <param name="configFileName">The name of the configuration file to verify.</param>
        public void ValidateForm(string configFileName)
        {
            try
            {
                // Use the DataEntryFormValidationThread which simply initializes a DEP instance
                // and returns.
                Thread validationThread =
                    new Thread(new ParameterizedThreadStart(DataEntryFormValidationThread));

                // [DataEntry:292] Some .Net control functionality such as clipboard and 
                // auto-complete depends upon the STA threading model.
                validationThread.SetApartmentState(ApartmentState.STA);
                validationThread.Start(configFileName);

                validationThread.Join();

                // Throw any exceptions that were caught when creating the data entry form.
                HandleExceptions();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26224", ex);
            }
        }

        /// <summary>
        /// Creates a <see cref="DataEntryApplicationForm"/> instance running in a separate thread
        /// that callers from all threads will share.
        /// </summary>
        /// <param name="configFileName">The name of the configuration file used to supply settings 
        /// for the <see cref="DataEntryApplicationForm"/>. NOTE: If a DataEntryApplication form
        /// is already running, it will be used even if it was launched using a different config
        /// file.</param>
        public void ShowForm(string configFileName)
        {
            try
            {
                if (_closing)
                {
                    ExtractException.Assert("ELI24002", 
                        "Unable to access existing data entry form!",
                        _closedEvent.WaitOne(_THREAD_TIMEOUT, false));
                }

                if (_dataEntryApplicationThread == null)
                {
                    // Create and start the data entry form thread if it doesn't already exist.
                    _closing = false;
                    _cancelledEvent.Reset();
                    _exceptionThrownEvent.Reset();
                    _closedEvent.Reset();

                    _dataEntryApplicationThread =
                        new Thread(new ParameterizedThreadStart(DataEntryApplicationThread));

                    // [DataEntry:292] Some .Net control functionality such as clipboard and 
                    // auto-complete depends upon the STA threading model.
                    _dataEntryApplicationThread.SetApartmentState(ApartmentState.STA);
                    _dataEntryApplicationThread.Start(configFileName);

                    // Wait until the form is initialized (or an error interupts initialization) 
                    // before returning.
                    WaitHandle[] waitHandles = new WaitHandle[] 
                        { _initializedEvent, _cancelledEvent, _exceptionThrownEvent };

                    WaitHandle.WaitAny(waitHandles);

                    // Notify any interested listeners of exceptions that were caught when showing
                    // the data entry form.
                    HandleExceptions();
                }
                else
                {
                    // If the data entry thread already exists, simply check to see that it is 
                    // properly initialized.
                    EnsureInitialization();
                }
            }
            catch (Exception ex)
            {
                // If there was a problem, end the data entry form.  true so any further exceptions 
                // will be logged and not thrown
                EndDataEntryApplicationThread(true);

                throw ExtractException.AsExtractException("ELI23977", ex);
            }
        }

        /// <summary>
        /// Displays the document image specified and loads any corresponding 
        /// <see cref="IAttribute"/> data into the <see cref="DataEntryControlHost"/>'s controls.
        /// </summary>
        /// <param name="fileName">Specifies the filename of the document image to open.</param>
        public void ShowDocument(string fileName)
        {
            try
            {
                // Ensure the data entry form has been properly initialized.
                EnsureInitialization();

                // Call OpenDocument via BeginInvoke (Asynchronous call)
                _dataEntryForm.BeginInvoke(new StringParameterDelegate(_dataEntryForm.OpenDocument),
                    new object[] { fileName });

                // Wait until the document is either saved or the data entry form is closed.
                WaitHandle[] waitHandles =
                    new WaitHandle[] { _documentSavedEvent, _cancelledEvent, _exceptionThrownEvent };

                WaitHandle.WaitAny(waitHandles);

                // Notify any interested listeners of exceptions that were caught during the time
                // the document was displayed or was being saved.
                HandleExceptions();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23970", ex);
                ee.AddDebugData("Filename", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Closes any currently open document.
        /// </summary>
        public void Cancel()
        {
            try
            {
                _cancelledEvent.Set();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24012", ex);
            }
        }

        /// <summary>
        /// Closes the <see cref="DataEntryApplicationForm"/> and ends the thread it was running in.
        /// </summary>
        public void CloseForm()
        {
            try
            {
                // Close the data entry form.  false so any exceptions ending the thread are thrown.
                EndDataEntryApplicationThread(false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24039", ex);
            }
            finally
            {
                // Whether or not there were any errors closing the data entry form,
                // cut it loose at this point so that a new one is able to be created.
                _dataEntryApplicationThread = null;
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Indicates whether work has been cancelled.
        /// </summary>
        /// <returns><see langword="true"/> if work has been cancelled since the 
        /// <see cref="DataEntryApplicationForm"/> was shown, <see langword="false"/> if it has not.
        /// </returns>
        public bool Cancelled
        {
            get
            {
                return _cancelledEvent.WaitOne(0, false);
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <overloads>Releases resources used by the 
        /// <see cref="DataEntryApplicationFormManager"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the 
        /// <see cref="DataEntryApplicationFormManager"/>.
        /// </summary>  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        /// <summary>
        /// Releases all unmanaged resources used by the 
        /// <see cref="DataEntryApplicationFormManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param> 
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dataEntryApplicationThread != null)
                {
                    // This will lead to _dataEntryForm being disposed.
                    // true so exceptions will be logged instead of thrown.
                    EndDataEntryApplicationThread(true);
                }

                // Close the event handles
                if (_initializedEvent != null)
                {
                    _initializedEvent.Close();
                    _initializedEvent = null;
                }
                if (_documentSavedEvent != null)
                {
                    _documentSavedEvent.Close();
                    _documentSavedEvent = null;
                }
                if (_cancelledEvent != null)
                {
                    _cancelledEvent.Close();
                    _cancelledEvent = null;
                }
                if (_exceptionThrownEvent != null)
                {
                    _exceptionThrownEvent.Close();
                    _exceptionThrownEvent = null;
                }
                if (_closedEvent != null)
                {
                    _closedEvent.Close();
                    _closedEvent = null;
                }
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handles the case that an exception was generated in the 
        /// <see cref="DataEntryApplicationForm"/> that should be passed out to the calling thread. 
        /// Within a short time of an event being handled, the exception will be thrown to the 
        /// <see cref="DataEntryApplicationFormManager"/> method that lead to the exception.
        /// </summary>
        /// <param name="sender">The <see cref="DataEntryApplicationFormManager"/> that threw the
        /// exception.
        /// </param>
        /// <param name="e">An <see cref="ExtractExceptionEventArgs"/> instance that contains the 
        /// exception to be thrown.</param>
        private void HandleExceptionGenerated(object sender, ExtractExceptionEventArgs e)
        {
            if (_lastException != null)
            {
                // If there was an previous exception that has not yet been thrown, log it so there
                // will be a record of it.
                _lastException.Log();
            }

            // Record the exception, then fire the _exceptionThrownEvent so any waiting threads move
            // on.
            _lastException = e.Exception;
            _exceptionThrownEvent.Set();
        }

        /// <summary>
        /// Handles the case that the user saved (output) the data from the current document.
        /// In this case, the caller can be notified that processing has completed on the current
        /// document.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="EventArgs"/> event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleDocumentSaved(object sender, EventArgs e)
        {
            _documentSavedEvent.Set();
        }

        /// <summary>
        /// Handles the case that the <see cref="DataEntryApplicationForm"/> has been displayed.
        /// At this point the form can be considered initialized and the _initializedEvent will be
        /// set.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="EventArgs"/> event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleDataEntryFormShown(object sender, EventArgs e)
        {
            // Notify any waiting threads that the data entry form been initialized.
            _initializedEvent.Set();
        }

        /// <summary>
        /// Handles the case that the <see cref="DataEntryApplicationForm"/> is closing. At this
        /// point the form is no longer considered initialized and ready to use, so the
        /// _initializedEvent is reset.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="EventArgs"/> event.</param>
        /// <param name="e">An <see cref="FormClosingEventArgs"/> that contains the event data.</param>
        void HandleFormClosing(object sender, FormClosingEventArgs e)
        {
            // Reset _initializedEvent to inidicate the form is no longer initialized
            _initializedEvent.Reset();
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Indicates whether or not the <see cref="DataEntryApplicationForm"/> is initialzed and
        /// ready for use.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="DataEntryApplicationForm"/> is intialized,
        /// <see langword="false"/> if it is not.</returns>
        private bool IsFormInitialized
        {
            get
            {
                return _initializedEvent.WaitOne(0, false);
            }
        }

        /// <summary>
        /// Checks to be sure the data entry form has been properly initialized. If initialization
        /// is still in progress it will wait for initialization to complete before returning.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if the data entry thread form is not initialized
        /// and ready for use.</throws>
        private void EnsureInitialization()
        {
            ExtractException.Assert("ELI24010", "Data entry form could not be initialized!",
                !_closing && _initializedEvent.WaitOne(_THREAD_TIMEOUT, false));
        }

        /// <summary>
        /// Checks to see if any exceptions were passed out by the 
        /// <see cref="DataEntryApplicationForm"/>, and, if so, throws it out on the current thread.
        /// </summary>
        private void HandleExceptions()
        {
            if (_lastException != null)
            {
                ExtractException ee = _lastException;
                _lastException = null;
                _exceptionThrownEvent.Reset();

                throw ee;
            }
        }

        /// <summary>
        /// Creates a <see cref="DataEntryApplicationForm"/> using the specified configuration file..
        /// </summary>
        /// <param name="configFileNameObject">A <see langword="string"/> specifying the name of the
        /// configuration file used to supply settings for the <see cref="DataEntryApplicationForm"/>.
        /// </param>
        private void DataEntryFormValidationThread(Object configFileNameObject)
        {
            try
            {
                string configFileName = configFileNameObject as string;
                ExtractException.Assert("ELI26223", "Null argument Exception!",
                    configFileName != null);

                // Prepare the application to display the data entry form
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Attempt to create an instance of data entry form, then return.
                using (new DataEntryApplicationForm(configFileName))
                {
                }
            }
            catch (Exception ex)
            {
                if (_lastException != null)
                {
                    _lastException.Log();
                }

                _lastException = ExtractException.AsExtractException("ELI26225", ex);
                _exceptionThrownEvent.Set();
            }
        }

        /// <summary>
        /// Initialized a thread in which to run the <see cref="DataEntryApplicationForm"/>.
        /// </summary>
        /// <param name="configFileNameObject">A <see langword="string"/> specifying the name of the
        /// configuration file used to supply settings for the <see cref="DataEntryApplicationForm"/>.
        /// </param>
        private void DataEntryApplicationThread(Object configFileNameObject)
        {
            // Make sure to keep track of the data entry form in a separate variable from _dataEntryForm
            // so that we can be sure to dispose of the form used by this thread even if 
            // the creator of this thread has already cleared _dataEntryForm.
            DataEntryApplicationForm dataEntryForm = null;

            try
            {
                string configFileName = configFileNameObject as string;
                ExtractException.Assert("ELI25480", "Null argument Exception!", 
                    configFileName != null);

                // Prepare the application to display the data entry form
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Create the data entry form in FAM mode.
                dataEntryForm = new DataEntryApplicationForm(configFileName);
                _dataEntryForm = dataEntryForm;

                dataEntryForm.StandAloneMode = false;

                // Register events
                dataEntryForm.ExceptionGenerated += HandleExceptionGenerated;
                dataEntryForm.Shown += HandleDataEntryFormShown;
                dataEntryForm.DocumentSaved += HandleDocumentSaved;
                dataEntryForm.FormClosing += HandleFormClosing;

                Application.Run(dataEntryForm);
            }
            catch (ThreadAbortException threadAbortException)
            {
                // If the data entry form's thread was aborted, it cannot be considered initialized.
                _initializedEvent.Reset();

                ExtractException.Log("ELI23870", threadAbortException);
            }
            catch (Exception ex)
            {
                // If the data entry form's thread was aborted, it cannot be considered initialized.
                _initializedEvent.Reset();

                if (_lastException != null)
                {
                    _lastException.Log();
                }

                _lastException = ExtractException.AsExtractException("ELI23988", ex);
                _exceptionThrownEvent.Set();
            }
            finally
            {
                _cancelledEvent.Set();
                
                if (dataEntryForm != null)
                {
                    dataEntryForm.Dispose();
                }
            }
        }

        /// <summary>
        /// Attempts to cleanly end the _dataEntryApplicationThread if possible, but will force
        /// kill it if necessary.
        /// </summary>
        private void EndDataEntryApplicationThread(bool logExceptions)
        {
            try
            {
                if (_closing)
                {
                    // If another thread has already started closing the thread, just wait and
                    // ensure it does end.
                    if (!_closedEvent.WaitOne(_THREAD_TIMEOUT, false))
                    {
                        new ExtractException("ELI24036",
                            "Timeout waiting for data entry form to close!").Log();
                    }

                    return;
                }

                // Set flag so that subsequent calls into this instance know that the form is in
                // the process of closing.
                _closing = true;

                // Check if the UI thread needs to be taken down.
                if (_dataEntryApplicationThread != null)
                {
                    // Attempt to end the thread cleanly by closing the form if it still exists.
                    if (this.IsFormInitialized)
                    {
                        // Call Close via BeginInvoke (Asynchronous call)
                        _dataEntryForm.BeginInvoke(new ParameterlessDelegate(_dataEntryForm.Close));
                    }

                    _dataEntryApplicationThread.Join();

                    // TODO: [DataEntry:293] Add timeout back, but in a way that is aware of prompts
                    // displayed for the user.
                }

                // Notify any interested listeners of exceptions that were caught during the time
                // the form was being closed.
                HandleExceptions();
            }
            catch (Exception ex)
            {
                // If any exception was thrown while trying to cleanly exit the UI thread, 
                // forcefully end it at this point.

                _dataEntryApplicationThread.Abort();
                if (logExceptions)
                {
                    ExtractException.Log("ELI23898", ex);
                }
                else
                {
                    throw ExtractException.AsExtractException("ELI24006", ex);
                }
            }
            finally
            {
                // Ensure the form and thread are set to null so that they will be re-initialized
                // on the next call to Init.
                _dataEntryApplicationThread = null;
                _dataEntryForm = null;
                _closedEvent.Set();
            }
        }

        #endregion Private Methods
    }
}
