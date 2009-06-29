using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a <see cref="Form"/> that can verify documents in a multi-threaded environment.
    /// </summary>
    /// <typeparam name="TForm">The type of the <see cref="Form"/>.</typeparam>
    public class VerificationForm<TForm> : IDisposable where TForm : Form, IVerificationForm, new()
    {
        #region VerificationForm Constants
	
	    /// <summary>
        /// If an exception forces the UI thread to be aborted, this is the number of milliseconds
        /// the thread should be given to close cleanly upon request. If the UI thread has not 
        /// exited after this amount of time, it will be forcefully aborted.
        /// </summary>
        static readonly int _THREAD_TIMEOUT = 10000;
		
	    #endregion VerificationForm Constants

        #region VerificationForm Fields
	
        /// <summary>
        /// The <typeparamref name="TForm"/> instance.
        /// </summary>
        volatile TForm _form;

	    /// <summary>
        /// The thread that creates and directly acts upon the <see cref="_form"/>.
        /// </summary>
        Thread _uiThread;

        /// <summary>
        /// The last exception thrown by the <see cref="_form"/>.
        /// </summary>
        volatile ExtractException _lastException;

        /// <summary>
        /// An event to indicate the <see cref="_form"/> has been created.
        /// </summary>
        EventWaitHandle _initializedEvent = new ManualResetEvent(false);

        /// <summary>
        /// An event to indicate a document has been verified.
        /// </summary>
        EventWaitHandle _fileVerified = new AutoResetEvent(false);

        /// <summary>
        /// An event to indicate processing has been cancelled either via <see cref="Cancel"/>
        /// or <see cref="CloseForm"/>.
        /// </summary>
        EventWaitHandle _canceledEvent = new ManualResetEvent(false);

        /// <summary>
        /// An event to indicate an exception was thrown from the <see cref="_form"/>.
        /// </summary>
        EventWaitHandle _exceptionThrownEvent = new ManualResetEvent(false);

        /// <summary>
        /// An event to indicate that the <see cref="_form"/> has been successfully closed and is 
        /// ready to be re-initialized.
        /// </summary>
        EventWaitHandle _closedEvent = new ManualResetEvent(false);

        /// <summary>
        /// If <see langword="true"/>, the <see cref="_form"/> is in the process of being closed. 
        /// <see cref="ShowDocument"/> should not be called when in this state and any call to 
        /// <see cref="ShowForm"/> needs to wait for the previous form to finish closing.
        /// </summary>
        volatile bool _closing;
		
	    #endregion VerificationForm Fields

        #region VerificationForm Delegates
	
        /// <summary>
        /// Delegate for a function that does not take any parameters.
        /// </summary>
        delegate void ParameterlessDelegate();
		
	    #endregion VerificationForm Delegates

        #region VerificationForm Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationForm{TForm}"/> class.
        /// </summary>
        public VerificationForm()
        {
            try
            {
                // Initialize licensing (load from folder since caller could be using this class as 
                // a static and it may therefore initialize before the caller's constructor code).
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI24047", Application.ProductName);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23791", ex);
            }
        }

        #endregion VerificationForm Constructors

        #region VerificationForm Properties

        /// <summary>
        /// Indicates whether work has been canceled.
        /// </summary>
        /// <returns><see langword="true"/> if work has been canceled since the 
        /// <see cref="_form"/> was shown, <see langword="false"/> if it has not.
        /// </returns>
        public bool Canceled
        {
            get
            {
                return _canceledEvent.WaitOne(0, false);
            }
        }

        #endregion VerificationForm Properties

        #region VerificationForm Methods

        /// <summary>
        /// Ensures the specified config file has valid settings by attempting to initialize an
        /// instance of the DEP using them.
        /// </summary>
        public void ValidateForm()
        {
            try
            {
                Thread validationThread = CreateUserInterfaceThread(ValidationThread);

                validationThread.Join();

                // Throw any exceptions that were caught when creating the verification form.
                HandleExceptions();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26224", ex);
            }
        }

        /// <summary>
        /// Creates and starts a thread with a single-threaded apartment.
        /// </summary>
        /// <param name="threadStart">A delegate that represents the functionality of the thread.
        /// </param>
        /// <returns>A thread with a single-threaded apartment.</returns>
        static Thread CreateUserInterfaceThread(ThreadStart threadStart)
        {
            // Use the ValidationThread which simply initializes a DEP instance and returns.
            Thread validationThread = new Thread(threadStart);

            // [DataEntry:292] Some .Net control functionality such as clipboard and 
            // auto-complete depends upon the STA threading model.
            validationThread.SetApartmentState(ApartmentState.STA);
            validationThread.Start();

            return validationThread;
        }

        /// <summary>
        /// Creates a <see cref="_form"/> instance running in a separate thread
        /// that callers from all threads will share.
        /// </summary>
        public void ShowForm()
        {
            try
            {
                if (_closing)
                {
                    ExtractException.Assert("ELI24002",
                        "Unable to access existing verification form.",
                        _closedEvent.WaitOne(_THREAD_TIMEOUT, false));
                }

                if (_uiThread == null)
                {
                    // Create and start the verification form thread if it doesn't already exist.
                    _closing = false;
                    _canceledEvent.Reset();
                    _exceptionThrownEvent.Reset();
                    _closedEvent.Reset();

                    _uiThread = CreateUserInterfaceThread(VerificationApplicationThread);

                    // Wait until the form is initialized (or an error interupts initialization) 
                    // before returning.
                    WaitHandle[] waitHandles = new WaitHandle[] { _initializedEvent, _canceledEvent, _exceptionThrownEvent };

                    WaitHandle.WaitAny(waitHandles);

                    // Notify any interested listeners of exceptions that were caught when showing
                    // the verification form.
                    HandleExceptions();
                }
                else
                {
                    // If the verification thread already exists, simply check to see that it is 
                    // properly initialized.
                    EnsureInitialization();
                }
            }
            catch (Exception ex)
            {
                // If there was a problem, end the verification form.  true so any further exceptions 
                // will be logged and not thrown
                EndVerificationApplicationThread(true);

                throw ExtractException.AsExtractException("ELI23977", ex);
            }
        }

        /// <summary>
        /// Displays the document image specified.
        /// </summary>
        /// <param name="fileName">Specifies the filename of the document image to open.</param>
        public void ShowDocument(string fileName)
        {
            try
            {
                // Ensure the verification form has been properly initialized.
                EnsureInitialization();

                // Open the file
                MainForm.Open(fileName);

                // Wait until the document is either saved or the verification form is closed.
                WaitHandle[] waitHandles =
                    new WaitHandle[] { _fileVerified, _canceledEvent, _exceptionThrownEvent };

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
                _canceledEvent.Set();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24012", ex);
            }
        }

        /// <summary>
        /// Closes the <see cref="_form"/> and ends the thread it was running in.
        /// </summary>
        public void CloseForm()
        {
            try
            {
                // Close the verification form.  false so any exceptions ending the thread are thrown.
                EndVerificationApplicationThread(false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24039", ex);
            }
            finally
            {
                // Whether or not there were any errors closing the verification form,
                // cut it loose at this point so that a new one is able to be created.
                _uiThread = null;
            }
        }

        #endregion VerificationForm Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="VerificationForm{TForm}"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="VerificationForm{TForm}"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="VerificationForm{TForm}"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_uiThread != null)
                {
                    // This will lead to _form being disposed.
                    // true so exceptions will be logged instead of thrown.
                    EndVerificationApplicationThread(true);
                }

                // Close the event handles
                if (_initializedEvent != null)
                {
                    _initializedEvent.Close();
                    _initializedEvent = null;
                }
                if (_fileVerified != null)
                {
                    _fileVerified.Close();
                    _fileVerified = null;
                }
                if (_canceledEvent != null)
                {
                    _canceledEvent.Close();
                    _canceledEvent = null;
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

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region VerificationForm Event Handlers

        /// <summary>
        /// Handles the <see cref="IVerificationForm.FileVerified"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="IVerificationForm.FileVerified"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="IVerificationForm.FileVerified"/> event.</param>
        void HandleFileVerified(object sender, EventArgs e)
        {
            _fileVerified.Set();
        }

        /// <summary>
        /// Handles the <see cref="Form.Shown"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Form.Shown"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Form.Shown"/> event.</param>
        void HandleVerificationFormShown(object sender, EventArgs e)
        {
            // Notify any waiting threads that the verification form been initialized.
            _initializedEvent.Set();
        }

        /// <summary>
        /// Handles the <see cref="Form.Closing"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Form.Closing"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Form.Closing"/> event.</param>
        void HandleFormClosing(object sender, FormClosingEventArgs e)
        {
            // Reset _initializedEvent to inidicate the form is no longer initialized
            _initializedEvent.Reset();
        }

        #endregion VerificationForm Event Handlers

        #region VerificationForm Private Properties

        /// <summary>
        /// Indicates whether or not the <see cref="_form"/> is initialzed and ready for use.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="_form"/> is intialized, 
        /// <see langword="false"/> if it is not.</returns>
        bool IsFormInitialized
        {
            get
            {
                return _initializedEvent.WaitOne(0, false);
            }
        }

        /// <summary>
        /// Gets or sets the verification form.
        /// </summary>
        /// <value>The verification form.</value>
        /// <returns>The verification form.</returns>
        TForm MainForm
        {
            get
            {
                if (_form == null)
                {
                    _form = new TForm();
                }

                return _form;
            }
            set
            {
                if (_form != null)
                {
                    _form.Dispose();
                }

                _form = value;
            }
        }
        
        #endregion VerificationForm Private Properties

        #region Private Methods

        /// <summary>
        /// Checks to be sure the verification form has been properly initialized. If initialization
        /// is still in progress it will wait for initialization to complete before returning.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if the verification thread form is not initialized
        /// and ready for use.</throws>
        void EnsureInitialization()
        {
            ExtractException.Assert("ELI24010", "Verification form could not be initialized.",
                !_closing && _initializedEvent.WaitOne(_THREAD_TIMEOUT, false));
        }

        /// <summary>
        /// Checks to see if any exceptions were passed out by the <see cref="_form"/>, and, if so, 
        /// throws it out on the current thread.
        /// </summary>
        void HandleExceptions()
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
        /// Creates a <typeparamref name="TForm"/> using the specified configuration file.
        /// </summary>
        void ValidationThread()
        {
            try
            {
                // Prepare the application to display the verification form
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Attempt to create an instance of verification form, then return.
                using (new TForm())
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
        /// Thread which creates and displays the verification form.
        /// </summary>
        void VerificationApplicationThread()
        {
            // Make sure to keep track of the verification form in a separate variable from _form
            // so that we can be sure to dispose of the form used by this thread even if 
            // the creator of this thread has already cleared _form.
            TForm verificationForm = null;

            try
            {
                // Prepare the application to display the verification form
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Create the verification form in FAM mode.
                verificationForm = new TForm();

                // Register events
                verificationForm.Shown += HandleVerificationFormShown;
                verificationForm.FileVerified += HandleFileVerified;
                verificationForm.FormClosing += HandleFormClosing;

                MainForm = verificationForm;

                Application.Run(verificationForm);
            }
            catch (ThreadAbortException threadAbortException)
            {
                // If the verification form's thread was aborted, it cannot be considered initialized.
                _initializedEvent.Reset();

                ExtractException.Log("ELI23870", threadAbortException);
            }
            catch (Exception ex)
            {
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
                _canceledEvent.Set();

                if (verificationForm != null)
                {
                    verificationForm.Dispose();
                }
            }
        }

        /// <summary>
        /// Attempts to cleanly end the _uiThread if possible, but will force
        /// kill it if necessary.
        /// </summary>
        void EndVerificationApplicationThread(bool logExceptions)
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
                            "Timeout waiting for verification form to close.").Log();
                    }

                    return;
                }

                // Set flag so that subsequent calls into this instance know that the form is in
                // the process of closing.
                _closing = true;

                // Check if the UI thread needs to be taken down.
                if (_uiThread != null)
                {
                    // Attempt to end the thread cleanly by closing the form if it still exists.
                    if (this.IsFormInitialized)
                    {
                        // Call Close via BeginInvoke (Asynchronous call)
                        MainForm.BeginInvoke(new ParameterlessDelegate(MainForm.Close));
                    }

                    _uiThread.Join();

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

                _uiThread.Abort();
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
                _uiThread = null;
                MainForm = null;
                _closedEvent.Set();
            }
        }

        #endregion Private Methods
    }
}
