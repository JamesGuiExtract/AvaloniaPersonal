using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Creates an <see cref="IVerificationForm"/>.
    /// </summary>
    /// <returns>An <see cref="IVerificationForm"/>.</returns>
    [CLSCompliant(false)]
    public delegate IVerificationForm CreateForm();

    /// <summary>
    /// Represents a <see cref="Form"/> that can verify documents in a multi-threaded environment.
    /// </summary>
    /// <typeparam name="TForm">The type of the <see cref="Form"/>.</typeparam>
    [CLSCompliant(false)]
    public class VerificationForm<TForm> : IDisposable where TForm : Form, IVerificationForm
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
        /// An event to indicate a document is done processing.
        /// </summary>
        EventWaitHandle _fileCompletedEvent = new AutoResetEvent(false);

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
        /// An event indicating when a file has finished loading.
        /// </summary>
        EventWaitHandle _fileLoadedEvent = new ManualResetEvent(false);

        /// <summary>
        /// If <see langword="true"/>, the <see cref="_form"/> is in the process of being closed. 
        /// <see cref="ShowDocument"/> should not be called when in this state and any call to 
        /// <see cref="ShowForm"/> needs to wait for the previous form to finish closing.
        /// </summary>
        volatile bool _closing;

        /// <summary>
        /// The processing result of the file being shown.
        /// </summary>
        EFileProcessingResult _fileProcessingResult;

        /// <summary>
        /// Used to protect access to <see cref="VerificationForm{TForm}"/>.
        /// </summary>
        static object _lock = new object();
		
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI24047",
                    Application.ProductName);
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
        public void ValidateForm(CreateForm creator)
        {
            try
            {
                Thread validationThread = CreateUserInterfaceThread(ValidationThread, creator);

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
        /// <param name="creator">Creates the <see cref="IVerificationForm"/>.</param>
        /// <returns>A thread with a single-threaded apartment.</returns>
        static Thread CreateUserInterfaceThread(ParameterizedThreadStart threadStart, 
            CreateForm creator)
        {
            // Create thread
            Thread thread = new Thread(threadStart);

            // [DataEntry:292] Some .Net control functionality such as clipboard and 
            // auto-complete depends upon the STA threading model.
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(creator);

            return thread;
        }

        /// <summary>
        /// Creates a <see cref="_form"/> instance running in a separate thread
        /// that callers from all threads will share.
        /// </summary>
        public void ShowForm(CreateForm creator)
        {
            lock (_lock)
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
                        _fileCompletedEvent.Reset();
                        _canceledEvent.Reset();
                        _exceptionThrownEvent.Reset();
                        _closedEvent.Reset();

                        _uiThread = 
                            CreateUserInterfaceThread(VerificationApplicationThread, creator);

                        // Wait until the form is initialized (or an error interrupts initialization) 
                        // before returning.
                        WaitForEvent(_initializedEvent);
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
        }

        /// <summary>
        /// Displays the document image specified.
        /// </summary>
        /// <param name="fileName">Specifies the filename of the document image to open.</param>
        /// <param name="fileID">The ID of the file being processed.</param>
        /// <param name="actionID">The ID of the action being processed.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <returns><see cref="EFileProcessingResult.kProcessingSuccessful"/> if verification of the
        /// document completed successfully, <see cref="EFileProcessingResult.kProcessingCancelled"/>
        /// if verification of the document was cancelled by the user or
        /// <see cref="EFileProcessingResult.kProcessingSkipped"/> if processing of the current file
        /// was skipped, but the user wishes to continue viewing subsequent documents.
        /// </returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ShowDocument(string fileName, int fileID, int actionID,
            FAMTagManager tagManager, FileProcessingDB fileProcessingDB)
        {
            if (this.Canceled)
            {
                return EFileProcessingResult.kProcessingCancelled;
            }

            bool haveLock = false;
            EFileProcessingResult fileProcessingResult;

            try
            {
                // Attempt to get the lock for the verification UI thread, but don't block at this
                // point if its not available.
                haveLock = Monitor.TryEnter(_lock);
                if (!haveLock)
                {
                    // Wait until the UI thread has finished loading its document before
                    // pre-fetcthing. Even with multiple cores, disk I/O from the prefectch can
                    // cause the UI thread to load slower.
                    _fileLoadedEvent.WaitOne();

                    // While waiting for the verification UI thread, prefetch data so that
                    // MainForm.Open call on this thread will have less work to do and execute
                    // faster.
                    MainForm.Prefetch(fileName, fileID, actionID, tagManager, fileProcessingDB);

                    // Now request the lock for the verification UI thread again, but this time
                    // block until it is available.
                    Monitor.Enter(_lock);
                    haveLock = true;
                }

                _fileLoadedEvent.Reset();

                if (this.Canceled)
                {
                    return EFileProcessingResult.kProcessingCancelled;
                }

                // Ensure the verification form has been properly initialized.
                EnsureInitialization();

                // Open the file
                MainForm.Open(fileName, fileID, actionID, tagManager, fileProcessingDB);

                _fileLoadedEvent.Set();

                // Wait until the document is either saved or the verification form is closed.
                WaitForEvent(_fileCompletedEvent);

                if (this.Canceled)
                {
                    return EFileProcessingResult.kProcessingCancelled;
                }

                // The file processing result needs to be noted before exiting the locked block,
                // but we need to exit the lock block before returning so that Sleep is called
                // to prevent one or more files from becoming stuck on other threads.
                fileProcessingResult = _fileProcessingResult;
            }
            catch (Exception ex)
            {
                // Ensure that _fileLoadedEvent gets set so that prefetch threads don't hang.
                _fileLoadedEvent.Set();

                ExtractException ee = ExtractException.AsExtractException("ELI23970", ex);
                ee.AddDebugData("Filename", fileName, false);
                throw ee;
            }
            finally
            {
                if (haveLock)
                {
                    Monitor.Exit(_lock);
                }
            }

            // Sleep to allow other threads waiting on the _lock to proceed, otherwise when running
            // multiple threads on a single-threaded machine this thread is likely to re-enter the
            // _lock section before Windows gives the waiting thread(s) an opportunity to proceed.
            Thread.Sleep(0);

            return fileProcessingResult;
        }

        /// <summary>
        /// Waits for the specified event to be signaled or for user cancellation or for an error 
        /// condition.
        /// </summary>
        /// <param name="waitHandle">The wait handle of the event for which to wait.</param>
        void WaitForEvent(EventWaitHandle waitHandle)
        {
            // Wait for the specified handle, cancellation, or exception
            WaitHandle[] waitHandles =
                new WaitHandle[] { waitHandle, _canceledEvent, _exceptionThrownEvent };

            WaitHandle.WaitAny(waitHandles);

            // Throw any caught exceptions
            HandleExceptions();
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
                lock (_lock)
                {
                    // Close the verification form.  false so any exceptions ending the thread are thrown.
                    EndVerificationApplicationThread(false);
                }
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
                    _initializedEvent.Dispose();
                    _initializedEvent = null;
                }
                if (_fileCompletedEvent != null)
                {
                    _fileCompletedEvent.Dispose();
                    _fileCompletedEvent = null;
                }
                if (_canceledEvent != null)
                {
                    _canceledEvent.Dispose();
                    _canceledEvent = null;
                }
                if (_exceptionThrownEvent != null)
                {
                    _exceptionThrownEvent.Dispose();
                    _exceptionThrownEvent = null;
                }
                if (_closedEvent != null)
                {
                    _closedEvent.Dispose();
                    _closedEvent = null;
                }
                if (_fileLoadedEvent != null)
                {
                    _fileLoadedEvent.Dispose();
                    _fileLoadedEvent = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region VerificationForm Event Handlers

        /// <summary>
        /// Handles the <see cref="IVerificationForm.FileComplete"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="IVerificationForm.FileComplete"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="IVerificationForm.FileComplete"/> event.</param>
        void HandleFileComplete(object sender, FileCompleteEventArgs e)
        {
            _fileProcessingResult = e.FileProcessingResult;

            if (e.FileProcessingResult == EFileProcessingResult.kProcessingCancelled)
            {
                _canceledEvent.Set();
            }
            else
            {
                _fileCompletedEvent.Set();
            }
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
        /// Handles the <see cref="Form.Closed"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Form.Closed"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Form.Closed"/> event.</param>
        void HandleFormClosed(object sender, FormClosedEventArgs e)
        {
            // Reset _initializedEvent to indicate the form is no longer initialized
            _initializedEvent.Reset();
        }

        #endregion VerificationForm Event Handlers

        #region VerificationForm Private Properties

        /// <summary>
        /// Gets or sets whether the current file has been signaled for completion.
        /// </summary>
        /// <returns><see langword="true"/> if the current file is signaled as complete;
        /// <see langword="false"/> if the current file is not signaled as complete.</returns>
        public bool Completed
        {
            get
            {
                return _fileCompletedEvent.WaitOne(0, false);
            }
        }

        /// <summary>
        /// Indicates whether or not the <see cref="_form"/> is initialzed and ready for use.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="_form"/> is intialized, 
        /// <see langword="false"/> if it is not.</returns>
        bool Initialized
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
        void ValidationThread(object obj)
        {
            try
            {
                CreateForm creator = (CreateForm)obj;

                // Attempt to create an instance of verification form, then return.
                TForm form = CreateTForm(creator);
                form.Dispose();
            }
            catch (Exception ex)
            {
                StoreException(ex);
            }
        }

        /// <summary>
        /// Thread which creates and displays the verification form.
        /// </summary>
        void VerificationApplicationThread(object obj)
        {
            try
            {
                // Create the verification form in FAM mode.
                MainForm = CreateTForm((CreateForm) obj);

                // Register events
                MainForm.Shown += HandleVerificationFormShown;
                MainForm.FileComplete += HandleFileComplete;
                MainForm.FormClosed += HandleFormClosed;

                Application.Run(MainForm);
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

                StoreException(ex);
            }
            finally
            {
                _canceledEvent.Set();

                MainForm = null;
            }
        }

        /// <summary>
        /// Creates a <typeparamref name="TForm"/> using the specified <see cref="CreateForm"/> 
        /// delegate instance.
        /// </summary>
        /// <param name="creator">Used to create the <typeparamref name="TForm"/>.</param>
        /// <returns>A <typeparamref name="TForm"/> using the specified <see cref="CreateForm"/>
        /// delegate instance.</returns>
        static TForm CreateTForm(CreateForm creator)
        {
            // Prepare the application to display the verification form
            Application.SetCompatibleTextRenderingDefault(false);

            TForm formInstance = (TForm)creator();
            if (formInstance.UseVisualStyles)
            {
                Application.EnableVisualStyles();
            }

            return formInstance;
        }

        /// <summary>
        /// Stores the specified exception in <see cref="_lastException"/> so it can be thrown by
        /// the calling thread.
        /// </summary>
        /// <param name="ex">The exception to store.</param>
        void StoreException(Exception ex)
        {
            if (_lastException != null)
            {
                _lastException.Log();
            }

            _lastException = ExtractException.AsExtractException("ELI23988", ex);
            _exceptionThrownEvent.Set();
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
                while (_uiThread != null && _uiThread.IsAlive)
                {
                    // Attempt to end the thread cleanly by closing the form if it still exists.
                    // [DataEntry:308, 254]
                    // In response to attempting to close the verification UI, the user may still
                    // be presented with an opportunity to save. If the user initially canceled
                    // the save prompt to make further adjustments, the UI will remain (despite 
                    // Initialized returning true). If any _fileCompletedEvent events are fired,
                    // re-attempt to close the UI.
                    // This code will probably need to be re-worked with #254.
                    if (Initialized || Completed)
                    {
                        // Indicate the the form is no longer initialized now that we have asked
                        // it to close.
                        _initializedEvent.Reset();

                        // Call Close via Invoke (Synchronous call)
                        MainForm.Invoke(new ParameterlessDelegate(MainForm.Close));
                    }

                    Thread.Sleep(100);
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
