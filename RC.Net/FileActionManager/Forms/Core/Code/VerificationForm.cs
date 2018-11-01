using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        /// An event to indicate the <see cref="_form"/> failed to initialize properly.
        /// </summary>
        EventWaitHandle _initializationFailedEvent = new ManualResetEvent(false);

        /// <summary>
        /// An event to indicate a document is done processing.
        /// </summary>
        EventWaitHandle _fileCompletedEvent = new ManualResetEvent(false);

        /// <summary>
        /// An event to indicate the verification session has been aborted.
        /// </summary>
        EventWaitHandle _abortedEvent = new ManualResetEvent(false);

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
        /// An event indicating when at least one file has been queued for processing.
        /// </summary>
        EventWaitHandle _fileProcessingEvent = new ManualResetEvent(false);

        /// <summary>
        /// An event indicating when a file has finished loading.
        /// </summary>
        EventWaitHandle _fileLoadedEvent = new ManualResetEvent(false);

        /// <summary>
        /// An event used to hold pre-fetched files while waiting on a requested file.
        /// </summary>
        EventWaitHandle _waitRequestedFile = new ManualResetEvent(true);

        /// <summary>
        /// An event used to awake threads waiting to display files to see if the file to be
        /// displayed is a file that has been delayed.
        /// </summary>
        EventWaitHandle _fileDelayedEvent = new ManualResetEvent(false);

        /// <summary>
        /// Tracks whether <see cref="Cancel"/> has been called (and prevents multiple calls to it).
        /// </summary>
        int _canceledCalled = 0;

        /// <summary>
        /// If <see langword="true"/>, the <see cref="_form"/> is in the process of being closed. 
        /// <see cref="ShowDocument"/> should not be called when in this state and any call to 
        /// ShowForm needs to wait for the previous form to finish closing.
        /// </summary>
        volatile bool _closing;

        /// <summary>
        /// If <see langword="true"/>, the FormClosing event has been received.
        /// <see cref="ShowDocument"/> should not be called when in this state and any call to 
        /// ShowForm needs to wait for the previous form to finish closing.
        /// </summary>
        volatile bool _formIsClosing;

        /// <summary>
        /// Indicates whether, in the case that processing failed due to the <see cref="_form"/>
        /// raising an <see cref="IVerificationForm.ExceptionGenerated"/> event, that the user
        /// should be given the option to continue processing on the next document.
        /// </summary>
        volatile bool _promptToContinueOnError;

        /// <summary>
        /// The handling of an <see cref="IVerificationForm.ExceptionGenerated"/> requires that any
        /// additional exceptions that may pop up on the main verification form windows message loop
        /// be suppressed so that the verification form can be cleanly closed programmatically
        /// if need be. This member keeps track of the thread for which exception displays are being
        /// blocked so that the block can be removed if need be.
        /// </summary>
        volatile int _exceptionDisplayBlockThreadId = -1;

        /// <summary>
        /// A reference count of the number of files currently in <see cref="ShowDocument"/>.
        /// </summary>
        long _processingFileCount;

        /// <summary>
        /// The IDs of the file currently displayed for verification.
        /// </summary>
        ConcurrentDictionary<int, int> _currentFileIDs = new ConcurrentDictionary<int, int>();

        /// <summary>
        /// Specifies the ID of a file that has been requested to be displayed ahead of all others.
        /// </summary>
        volatile int _requestedFileID = -1;

        /// <summary>
        /// The queue of IDs of files currently being held in the prefetch stage ("processing" in
        /// the verification task on threads other than the thread for which the currently displayed
        /// file is running on). The files are in ascending order of the time that they entered the
        /// queue.
        /// </summary>
        List<int> _waitingFileIDQueue = new List<int>();

        /// <summary>
        /// The IDs of all files for which a request has been made to delay processing.
        /// </summary>
        HashSet<int> _delayedFiles = new HashSet<int>();

        /// <summary>
        /// The IDs of files that have been completed with the associated <see cref="EFileProcessingResult"/>
        /// for each completion.
        /// </summary>
        ConcurrentDictionary<int, EFileProcessingResult> _completedFiles = new ConcurrentDictionary<int, EFileProcessingResult>();

        /// <summary>
        /// Used to protect access to <see cref="VerificationForm{TForm}"/>.
        /// </summary>
        static BetterLock _lock = new BetterLock();

        /// <summary>
        /// Used to protect access to assignment and disposal of <see cref="MainForm"/>.
        /// </summary>
        static object _lockFormChange = new object();

        /// <summary>
        /// Protects access to the objects used to handle requests for a specific file to be
        /// displayed or to delay processing of specific files.
        /// </summary>
        static object _lockFileRequest = new object();

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
        /// Gets or sets whether the verification session has been aborted.
        /// </summary>
        /// <returns><see langword="true"/> if processing has been aborted since the 
        /// <see cref="_form"/> was shown, <see langword="false"/> if it has not.
        /// </returns>
        public bool Aborted
        {
            get
            {
                return _abortedEvent.WaitOne(0, false);
            }
        }

        /// <summary>
        /// Gets a value indicating whether there are any files currently being processed in
        /// <see cref="ShowDocument"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if any files currently being processed; otherwise,
        /// <see langword="false"/>.</value>
        public bool IsFileProcessing
        {
            get
            {
                return _fileProcessingEvent.WaitOne(0, false);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is UI ready and available to display a
        /// document.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is UI ready; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsUIReady
        {
            get
            {
                return (!_closing && !_formIsClosing && !Aborted &&
                        _uiThread != null && _uiThread.IsAlive && MainForm != null);
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
                Thread validationThread = CreateUserInterfaceThread(ValidationThread, creator, 0);

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
        /// <param name="minStackSize">The minimum stack size needed for the thread in which this
        /// verification form is to be run (0 for default)</param>
        /// <returns>A thread with a single-threaded apartment.</returns>
        static Thread CreateUserInterfaceThread(ParameterizedThreadStart threadStart, 
            CreateForm creator, int minStackSize)
        {
            // Create thread
            Thread thread = new Thread(threadStart, minStackSize);

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
            ShowForm(creator, 0);
        }

        /// <summary>
        /// Creates a <see cref="_form"/> instance running in a separate thread
        /// that callers from all threads will share.
        /// </summary>
        /// <param name="creator">Used to create the <typeparamref name="TForm"/>.</param>
        /// <param name="minStackSize">The minimum stack size needed for the thread in which this
        /// verification form is to be run (0 for default)</param>
        public void ShowForm(CreateForm creator, int minStackSize)
        {
            using (_lock.GetDisposableScopeLock())
            {
                try
                {
                    if (_closing || _formIsClosing)
                    {
                        ExtractException.Assert("ELI24002",
                            "Unable to access existing verification form.",
                            _closedEvent.WaitOne(_THREAD_TIMEOUT, false));
                    }

                    if (_uiThread == null)
                    {
                        // Create and start the verification form thread if it doesn't already exist.
                        _closing = false;
                        _formIsClosing = false;
                        _canceledCalled = 0;
                        _fileCompletedEvent.Reset();
                        _abortedEvent.Reset();
                        _exceptionThrownEvent.Reset();
                        _closedEvent.Reset();
                        _lastException = null;
                        _requestedFileID = -1;
                        _waitRequestedFile.Set();
                        _waitingFileIDQueue.Clear();
                        _completedFiles.Clear();

                        _uiThread = CreateUserInterfaceThread(VerificationApplicationThread,
                            creator, minStackSize);

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
                    _initializationFailedEvent.Set();
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
            if (!IsUIReady)
            {
                // Either a Cancel request was received or the user closed the verification form.
                return EFileProcessingResult.kProcessingCancelled;
            }

            bool haveLock = false;
            EFileProcessingResult fileProcessingResult = EFileProcessingResult.kProcessingSuccessful;

            try
            {
                if (Interlocked.Increment(ref _processingFileCount) == 1)
                {
                    _fileProcessingEvent.Set();
                }

                // If the form can support multiple documents, send the document straight to the
                // Open call. Otherwise, using synchronizaton mechanisms to send all but one file to
                // Prefetch and to hold there while taking requested and delayed files into
                // consideration.
                if (MainForm.SupportsMultipleDocuments)
                {
                    _currentFileIDs.TryAdd(fileID, 0);
                }
                else
                {
                    // Attempt to get the lock for the verification UI thread, but don't block at this
                    // point if its not available. Although unlikely, from my reading it appears the
                    // lock could attempt here could allow a new file to sneak past already waiting
                    // files. Therefore, only try for the lock if we know of no currently waiting files.
                    haveLock = _waitingFileIDQueue.Count == 0 && _lock.TryLock();

                    if (!haveLock)
                    {
                        lock (_lockFileRequest)
                        {
                            _waitingFileIDQueue.Add(fileID);
                        }

                        // Beyond opening files, there are a couple of other circumstances in which the lock
                        // may be taken (Standby, RequestFile, DelayFile). If there are no other files processing
                        // and the current one is at the front of the queue, wait here for the lock without
                        // first going through prefetch.
                        // https://extract.atlassian.net/browse/ISSUE-15323
                        if (_currentFileIDs.Count == 0
                            && (_requestedFileID == -1 || _requestedFileID == fileID))
                        {
                            _lock.Lock(ref haveLock, _abortedEvent, _exceptionThrownEvent);
                        }

                        if (!haveLock)
                        {
                            // Wait until the UI thread has finished loading its document before
                            // pre-fetching. Even with multiple cores, disk I/O from the prefetch can
                            // cause the UI thread to load slower.
                            _fileLoadedEvent.WaitOne();

                            // Need to log exception when loading prefetched file
                            try
                            {
                                // While waiting for the verification UI thread, prefetch data so that
                                // MainForm.Open call on this thread will have less work to do and execute
                                // faster.
                                MainForm.Prefetch(fileName, fileID, actionID, tagManager, fileProcessingDB);
                            }
                            catch (Exception ex)
                            {
                                // Just log this exception another exception will be thrown when the file
                                // is loaded into the UI
                                ExtractException ee = new ExtractException("ELI37889", "Unable to prefetch file.", ex);
                                ee.AddDebugData("Filename", fileName, false);
                                ee.Log();
                            }
                        }

                        // Loop in case we need to wait on a requested file or a request has been made
                        // to delay processing of currently waiting files.
                        while (!haveLock && IsUIReady)
                        {
                            // Now request the lock for the verification UI thread again. We won't get
                            // out of this loop until we have the lock, the file has been delayed or
                            // processing is stopped.
                            _lock.Lock(ref haveLock, _fileDelayedEvent);

                            // Check to see if the lock was released in order to release a delayed file.
                            lock (_lockFileRequest)
                            {
                                if (_delayedFiles.Contains(fileID))
                                {
                                    _waitingFileIDQueue.Remove(fileID);
                                    _delayedFiles.Remove(fileID);

                                    // If all files requested to be released have been released, reset
                                    // _fileReleasedEvent so that files stop spinning until the next file
                                    // is needed or delayed.
                                    if (_delayedFiles.Count == 0)
                                    {
                                        _fileDelayedEvent.Reset();
                                    }
                                    return EFileProcessingResult.kProcessingDelayed;
                                }
                            }

                            if (!haveLock)
                            {
                                // Ensure against a tight loop
                                Thread.Sleep(100);
                                continue;
                            }

                            // If we have the lock, but a specific file is requested and this is not
                            // that file or there is another waiting file that came in before this one,
                            // release the lock and remain in this loop so that the appropriate file can
                            // get through.
                            if ((_requestedFileID != -1 && _requestedFileID != fileID) ||
                                (_requestedFileID == -1 && _waitingFileIDQueue[0] != fileID))
                            {
                                _lock.Unlock();
                                haveLock = false;

                                if (_requestedFileID != -1)
                                {
                                    // In case the requested file is not already waiting, wait until it
                                    // comes in before trying to obtain the lock again.
                                    WaitForEvent(_waitRequestedFile);
                                }
                                else
                                {
                                    // Ensure against a tight loop.
                                    Thread.Sleep(100);
                                }
                            }
                        }
                    }

                    // If currently waiting on a request file, checks that the requested file is fileID.
                    if (!CheckForRequestedFile(fileID))
                    {
                        return EFileProcessingResult.kProcessingDelayed;
                    }

                    // Needs to be set before file is removed from _prefetchSequencer so RequestFile
                    // can know for sure whether a requested file is available.
                    _currentFileIDs.TryAdd(fileID, 0);

                    // If this file is the currently requested file ID, clear the request so that other
                    // files that are being held are free to continue processing after this one.
                    lock (_lockFileRequest)
                    {
                        if (fileID == _requestedFileID)
                        {
                            _requestedFileID = -1;
                            _waitRequestedFile.Set();
                        }

                        _waitingFileIDQueue.Remove(fileID);
                    }
                }

                _fileLoadedEvent.Reset();

                // Protect against the form being closed or disposed of until the document has been
                // loaded.
                lock (_lockFormChange)
                {
                    if (IsUIReady)
                    {
                        // Whenever a new file is to be loaded, ensure the PreventSaveOfDirtyData and
                        // PreventCloseCancel properties are reset so that any behavior intended for
                        // the last document do not carry over to the this document.
                        MainForm.PreventSaveOfDirtyData = false;
                        MainForm.PreventCloseCancel = false;

                        if (_exceptionDisplayBlockThreadId != -1)
                        {
                            if (_exceptionDisplayBlockThreadId == _uiThread.ManagedThreadId)
                            {
                                // EndBlockExceptionDisplays is thread specific so it needs to be
                                // invoked on MainForm's thread.
                                MainForm.Invoke((MethodInvoker)(() =>
                                    ExtractException.EndBlockExceptionDisplays()));
                            }
                            _exceptionDisplayBlockThreadId = -1;
                        }
                    }
                    else
                    {
                        // Either a Cancel request was received or the user closed the verification form.
                        return EFileProcessingResult.kProcessingCancelled;
                    }

                    // Ensure the verification form has been properly initialized.
                    EnsureInitialization();
                }

                // Open the file
                MainForm.Open(fileName, fileID, actionID, tagManager, fileProcessingDB);

                _fileLoadedEvent.Set();
                
                do
                {
                    // When files are completed in when MainForm.SupportsMultipleDocuments, protect
                    // against a tight loop while other threads are checking if they are completed
                    Thread.Sleep(100);

                    // Wait until the document is either saved or the verification form is closed.
                    WaitForEvent(_fileCompletedEvent);
                }
                // Loop because in case of SupportsMultipleDocuments, multiple docouments may be
                // waiting in this loop-- only exit if it was this thread's document that completed.
                while (IsUIReady && !_completedFiles.TryRemove(fileID, out fileProcessingResult));
            }
            catch (Exception ex)
            {
                // Ensure that _fileLoadedEvent gets set so that prefetch threads don't hang.
                _fileLoadedEvent.Set();

                // If the FileID for the current file is in the _waitingFileQueue - remove it
                _waitingFileIDQueue.Remove(fileID);

                // Always display exceptions that arise from a verification task.
                ex.ExtractDisplay("ELI37834");

                // The file was not cleanly handled and will be failed with an exception; it is
                // not appropriate to display any prompts to save the document on close as such
                // attempts would likely meet with similar errors (which may not be able to be 
                // handled cleanly in context of a form close).
                MainForm.PreventSaveOfDirtyData = true;

                if (IsUIReady &&
                    (!_promptToContinueOnError || MessageBox.Show(
                    "An error was encountered while attempting to process the document '" +
                    fileName + "'.\r\n\r\nDo you wish to continue processing?",
                    "Error: Continue processing?", MessageBoxButtons.YesNo, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, 0) == DialogResult.No))
                {
                    _abortedEvent.Set();
                }

                ExtractException ee = ExtractException.AsExtractException("ELI23970", ex);
                ee.AddDebugData("Filename", fileName, false);
                throw ee;
            }
            finally
            {
                _currentFileIDs.TryRemove(fileID, out int _);
                _completedFiles.TryRemove(fileID, out var _);

                // Only reset _fileCompletedEvent once every thread has had the opportunity to see
                // if it was the one completed.
                if (_completedFiles.Count == 0)
                {
                    _fileCompletedEvent.Reset();
                }

                if (haveLock)
                {
                    _lock.Unlock();
                }

                if (Interlocked.Decrement(ref _processingFileCount) == 0)
                {
                    _fileProcessingEvent.Reset();
                }
            }

            // Sleep to allow other threads waiting on the _lock to proceed, otherwise when running
            // multiple threads on a single-threaded machine this thread is likely to re-enter the
            // _lock section before Windows gives the waiting thread(s) an opportunity to proceed.
            Thread.Sleep(0);

            return Aborted
                ? EFileProcessingResult.kProcessingCancelled
                : fileProcessingResult;
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
                new WaitHandle[] { waitHandle, _abortedEvent, _exceptionThrownEvent };

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
                // Only the first call to cancel per verification session need be handled.
                if (Interlocked.Increment(ref _canceledCalled) == 1)
                {
                    MainForm.Invoke((MethodInvoker)(() =>
                    {
                        // Don't call close if form is already closing
                        // https://extract.atlassian.net/browse/ISSUE-13125
                        if (!_formIsClosing)
                        {
                            // The FAM framework is not able to handle that a task can refuse the
                            // cancel request so force that the user cannot choose to cancel if
                            // prompted to save changes.
                            MainForm.PreventCloseCancel = true;
                            MainForm.Close();
                        }
                    }));
                }
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
                using (_lock.GetDisposableScopeLock())
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

                // Reset closing flag
                // https://extract.atlassian.net/browse/ISSUE-13854
                _closing = false;
            }
        }

        /// <summary>
        /// Called to notify the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <see langword="true"/>. If the processor wants to cancel processing,
        ///	it should return <see langword="false"/>. If the processor does not immediately know
        ///	whether processing should be cancelled right away, it may block until it does know,
        ///	and return at that time.
        /// <para><b>Note</b></para>
        /// This call will be made on a different thread than the other calls, so the Standby call
        /// must be thread-safe. This allows the file processor to block on the Standby call, but
        /// it also means that call to <see cref="ShowDocument"/> or <see cref="CloseForm"/> may
        /// come while the Standby call is still occurring. If this happens, the return value of
        /// Standby will be ignored; however, Standby should promptly return in this case to avoid
        /// needlessly keeping a thread alive.
        /// </summary>
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        public bool Standby()
        {
            try
            {
                if (!IsUIReady)
                {
                    // If the UI is gone or closing, abort processing.
                    _abortedEvent.Set();
                    return false;
                }
                else if (IsFileProcessing)
                {
                    // If a new file has been supplied on a different thread, standby for one on
                    // this thread.
                    return true;
                }

                using (ManualResetEvent formClosedEvent = new ManualResetEvent(false))
                {
                    // Allows the formClosedDelegate to know if the call to standby ended before it
                    // was called.
                    bool standybyEnded = false;

                    // Create a delegate which will signal formClosedEvent when the verification
                    // form is closed.
                    FormClosedEventHandler formClosedDelegate = (sender, eventArgs) =>
                    {
                        try
                        {
                            if (!standybyEnded)
                            {
                                // Lock to ensure standybyEnded doesn't get set and formClosedEvent
                                // disposed after the initial check of standybyEnded, but before
                                // signaling the event.
                                lock (_lockFormChange)
                                {
                                    if (!standybyEnded)
                                    {
                                        formClosedEvent.Set();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI33929");
                        }
                    };

                    bool registeredForFormClosing = false;
                    bool standby;

                    try
                    {
                        // Lock to prevent a race condition in Standby whereby the form could close
                        // after checking that the UI thread and form are still there but before
                        // accessing the form.
                        using (_lock.GetDisposableScopeLock())
                        lock (_lockFormChange)
                        {
                            // Once in the lock, double-check that the UI is still there and not
                            // processing before subscribing to FormClosed.
                            if (!IsUIReady)
                            {
                                // If the UI is gone or closing, abort processing.
                                _abortedEvent.Set();
                                return false;
                            }
                            else if (IsFileProcessing)
                            {
                                // If a new file has been supplied on a different thread, standby
                                // for one on this thread.
                                return true;
                            }

                            // Subscribe to FormClosed so that false can be returned from Standby
                            // to cancel processing if the verification form is closed.
                            MainForm.FormClosed += formClosedDelegate;

                            registeredForFormClosing = true;

                            // Allow the form itself to have the first crack at handling standby.
                            standby = MainForm.Standby();
                        }

                        // If the form approves of standing by, block until either another file is
                        // supplied, or the form is closed.
                        if (standby)
                        {
                            WaitHandle[] waitHandles =
                                new WaitHandle[] { formClosedEvent, _fileProcessingEvent };

                            standby = (WaitHandle.WaitAny(waitHandles) == 1);
                        }
                    }
                    finally
                    {
                        lock (_lockFormChange)
                        {
                            // Though we unsubscribe to the FormClosed event before exiting, the
                            // FormClosed event may have already fired. Set standybyEnded to notify the
                            // handler that it should not attempt to call Set on the disposed waitHandle.
                            standybyEnded = true;

                            if (registeredForFormClosing && MainForm != null)
                            {
                                MainForm.FormClosed -= formClosedDelegate;
                            }
                        }
                    }

                    // If this task will not standby, abort verification.
                    if (!standby)
                    {
                        _abortedEvent.Set();
                    }

                    return standby;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33924");
                return true;
            }
        }

        /// <summary>
        /// Requests the specified <see paramref="fileID"/> to be the next file displayed. The file
        /// should be allowed to jump ahead of any other files currently "processing" in the
        /// verification task on other threads (prefetch).
        /// </summary>
        /// <param name="fileID">The file ID.</param>
        /// <returns><see langword="true"/> if the file is currently processing in the verification
        /// task and confirmed to be available, <see langword="false"/> if the task is not currently
        /// holding the file; the requested file will be expected to be the next file in the queue.
        /// </returns>
        public bool RequestFile(int fileID)
        {
            try
            {
                lock (_lockFileRequest)
                {
                    _requestedFileID = fileID;

                    if (_currentFileIDs.ContainsKey(fileID) || _waitingFileIDQueue.Contains(fileID))
                    {
                        // The form is already holding the requested file. Return true to indicate
                        // it is available and will be guaranteed to be the next file displayed.
                        return true;
                    }
                    else
                    {
                        // If the requested file ID is not one of the currently waiting files, reset
                        // _waitRequestedFile to force all waiting files to wait until the requested
                        // file comes in. It is assumed that it has been arranged such that the
                        // requested file is the next file in the queue.
                        _waitRequestedFile.Reset();
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37495");
            }
        }

        /// <summary>
        /// Delays processing of the specified <see paramref="fileID"/> if the file is currently
        /// "processing" in the task, but not currently displayed (in prefetch).
        /// </summary>
        /// <param name="fileID">The ID of the file to delay.</param>
        /// <returns><see langword="true"/> if the file was "processing" but not displayed, thus
        /// able to be delayed; otherwise <see langword="false"/>.</returns>
        public bool DelayFile(int fileID)
        {
            try
            {
                lock (_lockFileRequest)
                {
                    if (_waitingFileIDQueue.Contains(fileID))
                    {
                        _delayedFiles.Add(fileID);

                        // Set _fileDelayedEvent to prompt threads with waiting files to see if the
                        // file each is holding is delayed.
                        _fileDelayedEvent.Set();
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37501");
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
                if (_initializationFailedEvent != null)
                {
                    _initializationFailedEvent.Dispose();
                    _initializationFailedEvent = null;
                }
                if (_fileCompletedEvent != null)
                {
                    _fileCompletedEvent.Dispose();
                    _fileCompletedEvent = null;
                }
                if (_abortedEvent != null)
                {
                    _abortedEvent.Dispose();
                    _abortedEvent = null;
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
                if (_fileProcessingEvent != null)
                {
                    _fileProcessingEvent.Dispose();
                    _fileProcessingEvent = null;
                }
                if (_waitRequestedFile != null)
                {
                    _waitRequestedFile.Dispose();
                    _waitRequestedFile = null;
                }
                if (_fileDelayedEvent != null)
                {
                    _fileDelayedEvent.Dispose();
                    _fileDelayedEvent = null;
                }
                if (_lock != null)
                {
                    _lock.Dispose();
                    _lock = null;
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
            _completedFiles[e.FileId] = e.FileProcessingResult;

            if (e.FileProcessingResult == EFileProcessingResult.kProcessingCancelled)
            {
                // Once a file has been canceled, the verification session should be aborted.
                _abortedEvent.Set();
            }
            else
            {
                _fileCompletedEvent.Set();
            }
        }

        /// <summary>
        /// Handles the <see cref="IVerificationForm.Initialized"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="IVerificationForm.Initialized"/> event.</param>
        /// <param name="e">The event data associated with the
        /// <see cref="IVerificationForm.Initialized"/>n event.</param>
        void HandleVerificationFormInitialized(object sender, EventArgs e)
        {
            // Notify any waiting threads that the verification form been initialized.
            _initializedEvent.Set();
        }

        /// <summary>
        /// Handles the form closing.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleFormClosing(object sender, FormClosingEventArgs e)
        {
            bool gotLock = false;

            try
            {
                // [FlexIDSCore:5008]
                // If the close has been canceled, there is nothing to do here.
                if (e.Cancel)
                {
                    return;
                }

                _formIsClosing = true;

                // If _closing is not set, the user has closed the form as opposed to it being
                // programmatically closed. Signal that further processing should be aborted.
                if (!_closing)
                {
                    _abortedEvent.Set();
                }

                // Note the time to ensure processing doesn't hang here for an unreasonable amount
                // of time.
                DateTime closingTime = DateTime.Now;

                // It is possible the form may be closing but there is still a thread in Prefetch.
                // Spin the message loop here until all files have exited processing (in case
                // Prefetch or Standby may need to use the message loop for processing).
                while (IsFileProcessing)
                {
                    Thread.Sleep(50);
                    Application.DoEvents();
                    ExtractException.Assert("ELI33950",
                        "Timeout waiting for the verification window to close.",
                        (DateTime.Now - closingTime).TotalMilliseconds < _THREAD_TIMEOUT);
                }

                // Lock to prevent a race condition in Standby whereby the form could be disposed
                // after checking that the UI thread and form are still there but before it accesses
                // the form. Spin the message loop heres to prevent a deadlock since the UI may need
                // to execute code to exit from an existing _lockFormChange lock.
                gotLock = Monitor.TryEnter(_lockFormChange);
                while (!gotLock)
                {
                    Thread.Sleep(50);
                    Application.DoEvents();
                    ExtractException.Assert("ELI33951",
                        "Timeout waiting for the verification window to close.",
                        (DateTime.Now - closingTime).TotalMilliseconds < _THREAD_TIMEOUT);
                    gotLock = Monitor.TryEnter(_lockFormChange);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33946");
            }
            finally
            {
                if (gotLock)
                {
                    Monitor.Exit(_lockFormChange);
                }
            }
        }

        /// <summary>
        /// Handles the Form.Closed event.
        /// </summary>
        /// <param name="sender">The object that sent the  Form.Closed event.</param>
        /// <param name="e">The event data associated with the  Form.Closed event.</param>
        void HandleFormClosed(object sender, FormClosedEventArgs e)
        {
            // Reset _initializedEvent to indicate the form is no longer initialized
            _initializedEvent.Reset();
            _initializationFailedEvent.Reset();
        }

        /// <summary>
        /// Handles the <see cref="IVerificationForm.FileRequested"/> event of
        /// <see cref="MainForm"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileRequestedEventArgs"/> instance containing the event
        /// data.</param>
        void HandleMainForm_FileRequested(object sender, FileRequestedEventArgs e)
        {
            try
            {
                e.FileIsAvailable = RequestFile(e.FileID);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37491");
            }
        }

        /// <summary>
        /// Handles the <see cref="IVerificationForm.FileDelayed"/> event of
        /// <see cref="MainForm"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileDelayedEventArgs"/> instance containing the event
        /// data.</param>
        void HandleMainForm_FileDelayed(object sender, FileDelayedEventArgs e)
        {
            try
            {
                e.FileIsAvailable = DelayFile(e.FileID);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37502");
            }
        }

        /// <summary>
        /// Handles the case that an exception was generated in <see cref="MainForm"/> that should
        /// be passed out to the calling thread.
        /// </summary>
        /// <param name="sender">The <see cref="IVerificationForm"/> that threw the
        /// exception.
        /// </param>
        /// <param name="e">An <see cref="VerificationExceptionGeneratedEventArgs"/> instance that
        /// contains the exception to be thrown.</param>
        void HandleMainForm_ExceptionGenerated(object sender,
            VerificationExceptionGeneratedEventArgs e)
        {
            try
            {
                StoreException(e.Exception, e.CanProcessingContinue);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37835");
            }
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
        /// Indicates whether or not the <see cref="_form"/> is initialized and ready for use.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="_form"/> is initialized, 
        /// <see langword="false"/> if it is not.</returns>
        bool Initialized
        {
            get
            {
                return _initializedEvent.WaitOne(0, false);
            }
        }

        /// <summary>
        /// Gets a value indicating whether initialization of the <see cref="_form"/> has failed.
        /// </summary>
        /// <value><see langword="true"/> if initialization of the <see cref="_form"/> has failed;
        /// otherwise, <see langword="false"/>.
        /// </value>
        bool InitializationFailed
        {
            get
            {
                return _initializationFailedEvent.WaitOne(0, false);
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
                try
                {
                    // Lock to prevent a race condition in Standby whereby the form could be disposed
                    // after checking that the UI thread and form are still there but before it accesses
                    // the form.
                    lock (_lockFormChange)
                    {
                        if (value != _form)
                        {
                            if (!_formIsClosing && _form != null)
                            {
                                try
                                {
                                    // [FlexIDSCore:4965]
                                    // Close the form here rather than calling dispose. Close is
                                    // safer because close can be called a second time in case of a
                                    // race condition, but still results in the form being disposed.
                                    _form.BeginInvoke(new ParameterlessDelegate(_form.Close));
                                }
                                catch (Exception ex)
                                {
                                    // [FlexIDSCore:4965]
                                    // If the form is now disposed, ignore the exception.
                                    if (!_form.IsDisposed)
                                    {
                                        throw ex.AsExtract("ELI34267");
                                    }
                                }

                                _formIsClosing = true;
                            }

                            _form = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34265");
                }
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
                StoreException(ex, false);
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
                MainForm.Initialized += HandleVerificationFormInitialized;
                MainForm.FileComplete += HandleFileComplete;
                MainForm.FormClosing += HandleFormClosing;
                MainForm.FormClosed += HandleFormClosed;
                MainForm.FileRequested += HandleMainForm_FileRequested;
                MainForm.FileDelayed += HandleMainForm_FileDelayed;
                MainForm.ExceptionGenerated += HandleMainForm_ExceptionGenerated;

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

                StoreException(ex, false);
            }
            finally
            {
                try
                {
                    _abortedEvent.Set();

                    try
                    {
                        MainForm.DisposeThread();
                        MainForm = null;
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI34270");
                    }

                    _closedEvent.Set();
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI34271");
                }
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
        /// <param name="canProcessingContinue"><see langword="true"/> if the user should be given
        /// the option to continue verification on the next document; <see langword="false"/> if the
        /// error should prevent the possibility of continuing the verification session.</param>
        void StoreException(Exception ex, bool canProcessingContinue)
        {
            if (_lastException != null)
            {
                _lastException.Log();
            }

            if (IsUIReady)
            {
                // EndBlockExceptionDisplays is thread specific so it needs to be
                // invoked on MainForm's thread.
                MainForm.Invoke((MethodInvoker)(() => ExtractException.BlockExceptionDisplays()));
                _exceptionDisplayBlockThreadId = _uiThread.ManagedThreadId;
            }

            _lastException = ex.AsExtract("ELI37836");
            _promptToContinueOnError = canProcessingContinue;
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
                            "Application trace: Timeout waiting for verification form to close.").Log();
                    }

                    return;
                }

                // Set flag so that subsequent calls into this instance know that the form is in
                // the process of closing.
                _closing = true;

                // Check if the UI thread needs to be taken down.
                while (!_formIsClosing && _uiThread != null && _uiThread.IsAlive)
                {
                    // Attempt to end the thread cleanly by closing the form if it still exists.
                    // [DataEntry:308, 254]
                    // In response to attempting to close the verification UI, the user may still
                    // be presented with an opportunity to save. If the user initially canceled
                    // the save prompt to make further adjustments, the UI will remain (despite 
                    // Initialized returning true). If any _fileCompletedEvent events are fired,
                    // re-attempt to close the UI.
                    // This code will probably need to be re-worked with #254.
                    if (Initialized || InitializationFailed || Completed)
                    {
                        // Indicate the form is no longer initialized now that we have asked
                        // it to close.
                        _initializedEvent.Reset();
                        _initializationFailedEvent.Reset();

                        lock (_lockFormChange)
                        {
                            // [FlexIDSCore:4965]
                            // Ensure MainForm hasn't already been disposed before attempting to
                            // close it.
                            if (_formIsClosing || MainForm == null)
                            {
                                break;
                            }

                            try
                            {
                                // Call Close via BeginInvoke (Asynchronous call)
                                MainForm.BeginInvoke(new ParameterlessDelegate(MainForm.Close));

                                _formIsClosing = true;
                            }
                            catch (Exception ex)
                            {
                                // [FlexIDSCore:4965]
                                // If the form is now disposed, ignore the exception.
                                if (!MainForm.IsDisposed)
                                {
                                    throw ex.AsExtract("ELI34268");
                                }
                            }
                        }
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
                // Ensure the thread is set to null so that it will be re-initialized on the next call to Init.
                _uiThread = null;
            }
        }

        /// <summary>
        /// Checks that the specified file ID is the currently requested file ID if we are waiting
        /// on the requested file ID.
        /// </summary>
        /// <param name="fileID">The file ID to ensure is the file we are waiting on (if waiting on
        /// a file).</param>
        /// <returns><c>true</c> if there is no requested file or the request file is
        /// <see paramref="fileID"/>; <c>false</c> if there is a requested file that is not
        /// <c>fileID</c>.</returns>
        bool CheckForRequestedFile(int fileID)
        {
            lock (_lockFileRequest)
            {
                // Are we waiting on the requested file ID?
                if (!_waitRequestedFile.WaitOne(0))
                {
                    // If the specified file is not the file we are waiting for, display an
                    // exception and load the file anyway. This ensures verification won't spin
                    // through all files in the queue looking for a file that isn't there.
                    if (_requestedFileID == -1)
                    {
                        _waitRequestedFile.Set();
                        var ee = new ExtractException("ELI37496", "Error waiting for requested file.");
                        ee.Display();
                    }

                    if (_requestedFileID != fileID
                        && !_waitingFileIDQueue.Contains(_requestedFileID)
                        && !_currentFileIDs.ContainsKey(_requestedFileID))
                    {
                        var ee = new ExtractException("ELI37497", "Requested file not available");
                        ee.AddDebugData("FileID", fileID, false);
                        ee.AddDebugData("RequestedID", _requestedFileID, false);
                        ee.Log();

                        _waitRequestedFile.Set();

                        return false;
                    }
                }
            }

            return true;
        }

        #endregion Private Methods
    }
}
