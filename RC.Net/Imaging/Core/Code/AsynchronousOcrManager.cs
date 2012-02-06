using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCRLib;
using UCLID_COMUTILSLib;

namespace Extract.Imaging
{
    /// <summary>
    /// Specifies the set of OCR tradeoff values.
    /// </summary>
    public enum OcrTradeoff
    {
        /// <summary>
        /// The most accurate, but the slowest.
        /// </summary>
        Accurate,

        /// <summary>
        /// A balance between accuracy and speed.
        /// </summary>
        Balanced,

        /// <summary>
        /// The quickest, but the least accurate.
        /// </summary>
        Fast,
    }

    /// <summary>
    /// Provides data for the <see cref="AsynchronousOcrManager.OcrProgressUpdate"/> event.
    /// </summary>
    public class OcrProgressUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// The percentage of OCR progress that has completed. 
        /// </summary>
        double _progressPercent;

        /// <summary>
        /// Initializes a new instance of the <see cref="OcrProgressUpdateEventArgs"/> class.
        /// </summary>
        /// <param name="progressPercent">The percentage of OCR progress that has completed.</param>
        public OcrProgressUpdateEventArgs(double progressPercent)
        {
            try
            {
                _progressPercent = progressPercent;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI22036",
                    "Failed to initialize OcrProgressUpdateEventArgs!", ex);
            }
        }

        /// <summary>
        /// Gets the percentage of OCR progress.
        /// </summary>
        /// <returns>The percentage of OCR progress.</returns>
        public double ProgressPercent
        {
            get
            {
                return _progressPercent;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="AsynchronousOcrManager.OcrProgressError"/> event.
    /// </summary>
    public class OcrProgressErrorEventArgs : ExtractExceptionEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OcrProgressErrorEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The <see cref="ExtractException"/> that occurred.</param>
        public OcrProgressErrorEventArgs(ExtractException exception)
            : base(exception)
        {
        }
    }

    /// <summary>
    /// Provides data for the <see cref="AsynchronousOcrManager.OcrError"/> event.
    /// </summary>
    public class OcrErrorEventArgs : ExtractExceptionEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OcrErrorEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The <see cref="ExtractException"/> that occurred.</param>
        public OcrErrorEventArgs(ExtractException exception)
            : base(exception)
        {
        }
    }

    /// <summary>
    /// Provides a way to wait for and access the results of an asynchronous OCR operation.
    /// <para><b>Note</b></para>
    /// While this interface implements <see cref="IDisposable"/>, do not dispose of an instance
    /// if <see cref="IAsyncResult.IsCompleted"/> is <see langword="false"/>.
    /// </summary>
    public interface IAsyncOcrResult : IAsyncResult, IDisposable
    {
        /// <summary>
        /// The data from the OCR operation.
        /// </summary>
        string OcrData
        {
            get;
        }
    }

    /// <summary>
    /// A class to manage launching OCR operations in a separate thread.  Currently
    /// only supports OCR of one document at a time.  If
    /// <see cref="OcrFile(string, int, int, CancellationToken?)"/> is called multiple times it
    /// will abort the currently running <see cref="Thread"/> and spawn a new one to OCR the newly
    /// specified file.
    /// <para><b>Note:</b></para>
    /// <see cref="Thread.Abort()"/> is non-deterministic, but once the thread acts on the
    /// signal the SSOCR2 instance will be cleaned up.
    /// </summary>
    public class AsynchronousOcrManager : IDisposable
    {
        #region Constants

        /// <summary>
        /// A constant representing the value returned in the
        /// <see cref="OcrProgressUpdateEventArgs"/> to indicate the OCR process was canceled.
        /// </summary>
        public static readonly double OcrCanceledProgressStatusValue = 42.0;

        /// <summary>
        /// A constant for how long to wait for OCR timeout when waiting for
        /// the OCR thread to complete.
        /// </summary>
        static readonly int _OCR_THREAD_COMPLETION_TIMEOUT = 60000;

        /// <summary>
        /// The number of milliseconds to allow a new operation to initialize before attempting to
        /// retrieve progress data or allowing it to be cancelled.
        /// </summary>
        static readonly int _OPERATION_INIT_TIME = 250;

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(AsynchronousOcrManager).ToString();

        #endregion Constants

        #region ASyncOcrResult

        /// <summary>
        /// Provides a way for callers wait for and access the results of an asynchronous OCR
        /// operation.
        /// </summary>
        class ASyncOcrResult : IAsyncOcrResult
        {
            #region Fields

            /// <summary>
            /// The data from the OCR operation.
            /// </summary>
            public string _ocrData;

            /// <summary>
            /// Indicates whether the OCR operation is complete (including failed/cancelled).
            /// </summary>
            public ManualResetEvent _completeEvent = new ManualResetEvent(false);

            #endregion Fields

            #region IAsyncOcrResult Members

            /// <summary>
            /// The data from the OCR operation.
            /// </summary>
            public string OcrData
            {
                get
                {
                    return _ocrData;
                }
            }

            #endregion IAsyncOcrResult Members

            #region IAsyncResult Members

            /// <summary>
            /// Gets the data from the OCR operation.
            /// </summary>
            /// <returns>A user-defined object that qualifies or contains information about an
            /// asynchronous operation.</returns>
            public object AsyncState
            {
                get
                {
                    return _ocrData;
                }
            }

            /// <summary>
            /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an
            /// asynchronous operation to complete.
            /// </summary>
            /// <returns>A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an
            /// asynchronous operation to complete.</returns>
            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    return _completeEvent;
                }
            }

            /// <summary>
            /// Gets a value that indicates whether the asynchronous operation completed
            /// synchronously
            /// <para><b>Note</b></para>
            /// Not used by this implementation.
            /// </summary>
            /// <returns><see langword="true"/> if the asynchronous operation completed synchronously;
            /// otherwise, <see langword="false"/>.</returns>
            public bool CompletedSynchronously
            {
                get
                {
                    return false;
                }
            }

            /// <summary>
            /// Gets a value that indicates whether the asynchronous operation has completed.
            /// </summary>
            /// <returns><see langword="true"/>  if the operation is complete; otherwise,
            /// <see langword="false"/>.</returns>
            public bool IsCompleted
            {
                get
                {
                    return (_completeEvent.WaitOne(0));
                }
            }

            #endregion IAsyncResult Members

            #region IDisposable Members

            /// <summary>
            /// Releases all resources used by the <see cref="ASyncOcrResult"/>.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <overloads>Releases resources used by the <see cref="ASyncOcrResult"/>.
            /// </overloads>
            /// <summary>
            /// Releases all unmanaged resources used by the <see cref="ASyncOcrResult"/>.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
            protected virtual void Dispose(bool disposing)
            {
                // Dispose managed objects
                if (disposing)
                {
                    if (_completeEvent != null)
                    {
                        _completeEvent.Close();
                        _completeEvent = null;
                    }
                }

                // No unmanaged resources to free
            }

            #endregion IDisposable Members
        }

        #endregion ASyncOcrResult

        #region Fields

        /// <summary>
        /// The tradeoff setting between accuracy and speed
        /// </summary>
        OcrTradeoff _tradeoff = OcrTradeoff.Balanced;

        /// <summary>
        /// Flag that indicates if the current OCR process has begun.
        /// </summary>
        volatile bool _currentlyInOcr;

        /// <summary>
        /// Holds a stringized byte stream representation of the SpatialString returned
        /// from the most recently completed OCR process.
        /// </summary>
        volatile string _ocrTextStream;

        /// <summary>
        /// The name of the file for the OCR thread to perform text recognition on.
        /// </summary>
        volatile string _fileToOcr;

        /// <summary>
        /// The <see cref="ASyncOcrResult"/> for the active operation.
        /// </summary>
        ASyncOcrResult _result;

        /// <summary>
        /// Mutex object
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// Mutex to ensure only on OCR operation is started or canceled at the same time.
        /// </summary>
        object _operationInitLock = new object();

        /// <summary>
        /// Handle to the thread that is performing the OCR process.
        /// </summary>
        Thread _ocrThread;

        /// <summary>
        /// The first page to OCR.
        /// </summary>
        volatile int _startPage;

        /// <summary>
        /// The last page to OCR.
        /// </summary>
        volatile int _endPage;

        /// <summary>
        /// The logical area of an image page that should be OCR'd
        /// </summary>
        Rectangle? _zonalOcrArea;

        /// <summary>
        /// Holds an instance of the ScansoftOcrClass so that we can use the same
        /// COM object throughout the life of the <see cref="AsynchronousOcrManager"/> class.
        /// </summary>
        ScansoftOCRClass _ssocr;

        /// <summary>
        /// An <see cref="AutoResetEvent"/> indicating that a new document has been flagged
        /// for OCR.
        /// </summary>
        EventWaitHandle _newOcrEvent = new AutoResetEvent(false);

        /// <summary>
        /// An <see cref="ManualResetEvent"/> indicating that the OCR operation has
        /// completed.
        /// </summary>
        EventWaitHandle _ocrDocumentCompleteEvent = new ManualResetEvent(true);

        /// <summary>
        /// An <see cref="ManualResetEvent"/> inidicating that the OCR thread should
        /// exit.
        /// </summary>
        EventWaitHandle _endThreadEvent = new ManualResetEvent(false);

        /// <summary>
        /// An <see cref="ManualResetEvent"/> indicating that the current OCR event
        /// has been canceled.
        /// </summary>
        EventWaitHandle _ocrCanceledEvent = new ManualResetEvent(false);

        /// <summary>
        /// A <see cref="ManualResetEvent"/> indicating that a new operation can be started. Will
        /// not be set while a previous operation is initializing.
        /// </summary>
        EventWaitHandle _canStartNewOperation = new ManualResetEvent(true);

        /// <summary>
        /// Active <see cref="CancellationToken"/> that can be used to cancel the currently running
        /// operation in lieu of calling <see cref="CancelOcrOperation"/>.
        /// </summary>
        CancellationToken? _cancelToken;

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when OCR progress needs to be updated.
        /// </summary>
        public event EventHandler<OcrProgressUpdateEventArgs> OcrProgressUpdate;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> occurs in the OCR progress update thread.
        /// <para><b>NOTE:</b></para>
        /// If there is no event handler registered for this event, then the
        /// <see cref="ExtractException"/> will be automatically logged.  If there is
        /// an event handler registered then it is up to the event handler
        /// to do something with the <see cref="ExtractException"/>.
        /// </summary>
        public event EventHandler<OcrProgressErrorEventArgs> OcrProgressError;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> occurs in the OCR thread.
        /// <para><b>NOTE:</b></para>
        /// If there is no event handler registered for this event, then the
        /// <see cref="ExtractException"/> will be automatically logged.  If there is
        /// an event handler registered then it is up to the event handler
        /// to do something with the <see cref="ExtractException"/>.
        /// </summary>
        public event EventHandler<OcrErrorEventArgs> OcrError;

        #endregion Events

        #region Constructors

        /// <overloads>Initializes a new instance of the <see cref="AsynchronousOcrManager"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="AsynchronousOcrManager"/> class.
        /// </summary>
        public AsynchronousOcrManager()
            : this(OcrTradeoff.Balanced)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsynchronousOcrManager"/> class with the
        /// specified speed-accuracy tradeoff.
        /// </summary>
        public AsynchronousOcrManager(OcrTradeoff tradeoff)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.OcrOnClientFeature, "ELI23120",
					_OBJECT_NAME);

                // Set the tradeoff
                _tradeoff = tradeoff;

                // Create the ocr thread
                _ocrThread = new Thread(new ThreadStart(OcrFileThread));

                // Start the thread
                _ocrThread.Start();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23121", ex);
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="OcrProgressUpdate"/> event.
        /// </summary>
        /// <param name="e">A <see cref="OcrProgressUpdateEventArgs"/>
        /// that contains event data.</param>
        protected void OnOcrProgressUpdate(OcrProgressUpdateEventArgs e)
        {
            try
            {
                // Check for a handler
                if (OcrProgressUpdate != null)
                {
                    // Raise the event
                    OcrProgressUpdate(this, e);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22039", ex);
                ee.AddDebugData("Event Arguments", e, false);
                throw ee;
            }
        }

        /// <summary>
        /// Raises the <see cref="OcrProgressError"/> event.
        /// </summary>
        /// <param name="e">A <see cref="OcrProgressErrorEventArgs"/>
        /// that contains the event data.</param>
        protected void OnOcrProgressError(OcrProgressErrorEventArgs e)
        {
            try
            {
                // If no event handler is registered then just log the exception
                if (OcrProgressError != null)
                {
                    OcrProgressError(this, e);
                }
                else
                {
                    e.Exception.Log();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22041", ex);
                ee.AddDebugData("Event Arguments", e, false);
                throw ee;
            }
        }

        /// <summary>
        /// Raises the <see cref="OcrError"/> event.
        /// </summary>
        /// <param name="e">A <see cref="OcrErrorEventArgs"/>
        /// that contains the event data.</param>
        protected void OnOcrError(OcrErrorEventArgs e)
        {
            try
            {
                // If no event handler is registered then just log the exception
                if (OcrError != null)
                {
                    OcrError(this, e);
                }
                else
                {
                    e.Exception.Log();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22040", ex);
                ee.AddDebugData("Event Arguments", e, false);
                throw ee;
            }
        }

        #endregion Event Handlers

        #region Delegates

        /// <summary>
        /// Delegate method for the <see cref="Timer"/>. Will raise the
        /// <see cref="OcrProgressUpdate"/> event.
        /// </summary>
        /// <param name="state">A ProgressStatus object that can be queried
        /// to get the current progress status.</param>
        protected void Tick(object state)
        {
            try
            {
                // [FlexIDSCore:5043]
                // After the initial _OPERATION_INIT_TIME, consider this operation initialized and
                // allow it to be cancelled in favor of another.
                if (!_canStartNewOperation.WaitOne(0))
                {
                    _canStartNewOperation.Set();
                }

                // If there is a valid ProgressStatus object, update progress.
                ProgressStatus progressStatus = state as ProgressStatus;
                if(progressStatus != null)
                {
                    OnOcrProgressUpdate(new OcrProgressUpdateEventArgs(
                        progressStatus.GetProgressPercent()));
                }
            }
            catch (Exception ex)
            {
                // Wrap the exception as an ExtractException
                ExtractException ee = ExtractException.AsExtractException("ELI22042", ex);

                // Raise the OnOcrProgressError event
                OnOcrProgressError(new OcrProgressErrorEventArgs(ee));
            }
        }

        /// <summary>
        /// Delegate method for the OCR <see cref="Thread"/>. Will perform
        /// an OCR operation on the provided file name.
        /// </summary>
        void OcrFileThread()
        {
            try
            {
                try
                {
                    // Create an array of wait handles for the OCR thread to spin on
                    WaitHandle[] waitHandles = new WaitHandle[] { _newOcrEvent, _endThreadEvent };

                    // WaitAny will return the index of the event that became signaled from the
                    // array of WaitHandles that was passed in.
                    // Index 1 indicates the _endThreadEvent, this will cause the OcrFileThread
                    // method to exit which will in turn end the thread.
                    while (WaitHandle.WaitAny(waitHandles) != 1)
                    {
                        // Gather the information about this task before _canStartNewOperation is
                        // set.
                        string fileToOcr;
                        int startPage;
                        int endPage;
                        Rectangle? zonalOcrArea = null;
                        ASyncOcrResult result = null;
                        lock (_lock)
                        {
                            fileToOcr = _fileToOcr;
                            startPage = _startPage;
                            endPage = _endPage;
                            zonalOcrArea = _zonalOcrArea;
                            result = _result;
                        }

                        try
                        {
                            ProgressStatus progressStatus = new ProgressStatus();

                            // Create and start a timer to update progress status.  Add a small
                            // delay to allow OCR to begin, this will help with the UI refreshing
                            // with meaningful data sooner.
                            // NOTE: FxCop will complain if you set the interval time < 1000
                            using (Timer timer = new Timer(new TimerCallback(Tick),
                                progressStatus, _OPERATION_INIT_TIME, 1000))
                            {
                                if (_ssocr == null)
                                {
                                    // Get a new ScanSoftOCR class object
                                    _ssocr = new ScansoftOCRClass();

                                    // Init the private license
                                    _ssocr.InitPrivateLicense(GetSpecialOcrValue());
                                }

                                SpatialString ocrText = null;
                                if (!OcrCanceled)
                                {
                                    // Set the currently in OCR flag to true
                                    _currentlyInOcr = true;

                                    if (zonalOcrArea == null)
                                    {
                                        // If _zonalOcrArea has not been specified, OCR the entire
                                        // area each page.
                                        ocrText = _ssocr.RecognizeTextInImage(fileToOcr,
                                        startPage, endPage, EFilterCharacters.kNoFilter, "",
                                        (EOcrTradeOff)this.Tradeoff, true, progressStatus);
                                    }
                                    else
                                    {
                                        // If _zonalOcrArea has been specified, OCR only the area
                                        // within this logical area of this rectangle on each page.
                                        // [LegacyRCAndUtils:5033] TODO: This currently does not
                                        // respect this.Tradeoff.
                                        LongRectangleClass zonalOCRRectangle = new LongRectangleClass();
                                        zonalOCRRectangle.SetBounds(zonalOcrArea.Value.Left,
                                                                    zonalOcrArea.Value.Top,
                                                                    zonalOcrArea.Value.Right,
                                                                    zonalOcrArea.Value.Bottom);

                                        ocrText = _ssocr.RecognizeTextInImageZone(fileToOcr,
                                            startPage, endPage, zonalOCRRectangle, 0,
                                            EFilterCharacters.kNoFilter, "", false, false, true, null);
                                    }

                                    // OCR complete, set currently in OCR flag to false
                                    _currentlyInOcr = false;
                                }

                                // Stop the progress timer
                                timer.Change(Timeout.Infinite, Timeout.Infinite);

                                // Mutex over changes to class variables
                                lock (_lock)
                                {
                                    if (OcrCanceled)
                                    {
                                        // Set empty output if canceled so a call doesn't mistake it
                                        // for valid output.
                                        _ocrTextStream = "";
                                        result._ocrData = null;
                                    }
                                    else
                                    {
                                        // Get the SpatialString as a stringized byte stream
                                        MiscUtils miscUtils = new MiscUtils();
                                        _ocrTextStream =
                                            miscUtils.GetObjectAsStringizedByteStream(ocrText);
                                        result._ocrData = _ocrTextStream;
                                    }
                                }
                            }

                            // Send last progress update message to indicate completion or canceled.
                            if (OcrCanceled)
                            {
                                // Canceled, send the cancel update
                                OnOcrProgressUpdate(new OcrProgressUpdateEventArgs(
                                    OcrCanceledProgressStatusValue));
                            }
                            else
                            {
                                // Completed, send 100% update
                                OnOcrProgressUpdate(new OcrProgressUpdateEventArgs(1.0));
                            }
                        }
                        catch (Exception ex)
                        {
                            // Mutex over changes to member variable
                            lock (_lock)
                            {
                                // Set the OCR text stream to empty string
                                _ocrTextStream = "";
                                result._ocrData = "";
                            }

                            // Wrap the exception as an ExtractException
                            // Do not add a new top level exception, just wrap current exception
                            // [IDShieldDesktop #28]
                            ExtractException ee =
                                ExtractException.AsExtractException("ELI22031", ex);

                            // Raise the OcrError event
                            OnOcrError(new OcrErrorEventArgs(ee));
                        }
                        finally
                        {
                            // The OcrOperation is complete/failed/cancelled. Ensure
                            // _canStartNewOperation is set.
                            if (!_canStartNewOperation.WaitOne(0))
                            {
                                _canStartNewOperation.Set();
                            }
                        }

                        // Set the OCR document complete event to signaled
                        _ocrDocumentCompleteEvent.Set();
                        result._completeEvent.Set();
                    }
                }
                finally
                {
                    // Ensure _ocrDocumentCompleteEvent gets set even if an error occurs
                    _ocrDocumentCompleteEvent.Set();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI22145", ex);
            }
        }

        #endregion Delegates

        #region Methods

        /// <summary>
        /// Launches a thread to OCR the specified file.
        /// <para><b>Note:</b></para>
        /// Multiple calls to this function will result in the canceling of the current
        /// OCR operation and then beginning a new OCR operation on the new document.
        /// The current design only supports OCR of one document at a time.
        /// Also, all data related to the last OCR operation will be reset 
        /// (<see cref="OcrOutput"/>, etc).
        /// </summary>
        /// <param name="fileName">The file to OCR.</param>
        /// <param name="startPage">The first page of the image to OCR.</param>
        /// <param name="endPage">The last page of the image to OCR, or -1 to OCR to the end of 
        /// the document.</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> that can be used to cancel
        /// the currently running operation in lieu of calling <see cref="CancelOcrOperation"/>.
        /// Can be <see langword="null"/> if there is no cancel token to use.</param>
        /// <returns>An <see cref="IAsyncOcrResult"/> to allow waiting for and accessing the results
        /// of the asynchronous OCR operation.
        /// <para><b>Note</b></para>
        /// Do not dispose of <see cref="IAsyncOcrResult"/> if
        /// <see cref="IAsyncResult.IsCompleted"/> is <see langword="false"/>.
        /// </returns>
        public IAsyncOcrResult OcrFile(string fileName, int startPage, int endPage,
            CancellationToken? cancelToken)
        {
            return OcrImageArea(fileName, startPage, endPage, null, cancelToken);
        }

        /// <summary>
        /// Launches a thread to OCR the specified area of the specified image page(s).
        /// <para><b>Note:</b></para>
        /// Multiple calls to this function will result in the canceling of the current
        /// OCR operation and then beginning a new OCR operation on the new document.
        /// The current design only supports OCR of one document at a time.
        /// Also, all data related to the last OCR operation will be reset 
        /// (<see cref="OcrOutput"/>, etc).
        /// </summary>
        /// <param name="fileName">The file to OCR.</param>
        /// <param name="startPage">The first page of the image to OCR.</param>
        /// <param name="endPage">The last page of the image to OCR, or -1 to OCR to the end of 
        /// the document.</param>
        /// <param name="imageArea">The logical area of each which should be OCR'd.
        /// If null, the entirety of each page will be OCR'd</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> that can be used to cancel
        /// the currently running operation in lieu of calling <see cref="CancelOcrOperation"/>.
        /// Can be <see langword="null"/> if there is no cancel token to use.</param>
        /// <returns>An <see cref="IAsyncOcrResult"/> to allow waiting for and accessing the results
        /// of the asynchronous OCR operation.
        /// <para><b>Note</b></para>
        /// Do not dispose of <see cref="IAsyncOcrResult"/> if
        /// <see cref="IAsyncResult.IsCompleted"/> is <see langword="false"/>.
        /// </returns>
        public IAsyncOcrResult OcrImageArea(string fileName, int startPage, int endPage,
            Rectangle? imageArea, CancellationToken? cancelToken)
        {
            ASyncOcrResult result = new ASyncOcrResult();

            try
            {
                // Ensure that a valid file name has been passed in.
                ExtractException.Assert("ELI22032", "Filename may not be null or empty!",
                    !string.IsNullOrEmpty(fileName));
                ExtractException.Assert("ELI22033", "File is not valid!",
                    File.Exists(fileName), "Ocr File Name", fileName);
                
                // [FlexIDSCore:4944]
                // Lock _operationInitLock immediately to prevent another OCR event from being
                // started or cancelled until this one has been started.
                lock (_operationInitLock)
                {
                    // Check if OCR is currently complete
                    if (!_ocrDocumentCompleteEvent.WaitOne(0, false))
                    {
                        // Signal OCR canceled
                        _ocrCanceledEvent.Set();

                        // Give the last OCR operation a chance to initialize before trying to kill it.
                        _canStartNewOperation.WaitOne(_OPERATION_INIT_TIME);

                        // If currently performing an OCR task then kill the OCR engine
                        if (_currentlyInOcr)
                        {
                            // Kill the ocr engine and wait
                            _ssocr.WhackOCREngine();
                        }

                        // Wait for the current ocr to complete (if it times out there was a
                        // problem, throw an exception)
                        if (!_ocrDocumentCompleteEvent.WaitOne(_OCR_THREAD_COMPLETION_TIMEOUT,
                            false))
                        {
                            throw new ExtractException("ELI22151",
                                "Error: Timeout occurred while waiting for current OCR process to end!");
                        }
                    }

                    // Reset progress status
                    OnOcrProgressUpdate(new OcrProgressUpdateEventArgs(0.0));

                    // Mutex around member variable changes
                    lock (_lock)
                    {
                        // Reset any old Ocr text
                        _ocrTextStream = "";

                        // Set the file to OCR
                        _fileToOcr = fileName;

                        // Set the start and end page
                        _startPage = startPage;
                        _endPage = endPage;

                        // Set the image area to OCR
                        _zonalOcrArea = imageArea;

                        _result = result;
                    }

                    // Before launching the OCR process, ensure the operaion has not already been
                    // cancelled.
                    if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                    {
                        result._ocrData = null;
                        result._completeEvent.Set();
                        return result;
                    }

                    // [FlexIDSCore:5043]
                    // Prevent this operation from being cancelled until it has had time to
                    // initialize. The intent is to prevent exceptions from cancelling the operation
                    // while it is in the process of initializing.
                    _canStartNewOperation.Reset();

                    // [DotNetRCAndUtils:302] Set the OCR complete event to unsignaled
                    // This needs to be reset here and not within OcrFileThread in order to
                    // prevent a race condition.
                    _ocrDocumentCompleteEvent.Reset();

                    // Set the OCR canceled event to unsignaled
                    _ocrCanceledEvent.Reset();

                    try
                    {
                        if (cancelToken != null)
                        {
                            // If a cancel token has been provided, spawn a thread to watch for
                            // cancelation.
                            _cancelToken = cancelToken;
                            Thread cancelTokenWatchThread =
                                new Thread(new ThreadStart(CancelTokenWatchThread));
                            cancelTokenWatchThread.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        // [FlexIDSCore:4944]
                        // Ensure we don't reset _ocrDocumentCompleteEvent without _newOcrEvent ever
                        // getting set. That will result in an instance that is "stuck" and not able
                        // to OCR anymore since all new events will be waiting on
                        // _ocrDocumentCompleteEvent.
                        _ocrDocumentCompleteEvent.Set();

                        throw ex.AsExtract("ELI34141");
                    }

                    // Signal the New OCR event
                    _newOcrEvent.Set();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22050", ex);
            }

            return result;
        }

        /// <summary>
        /// Cancels the current OCR operation and sets the canceled flag.
        /// </summary>
        public void CancelOcrOperation()
        {
            try
            {
                // [FlexIDSCore:4944]
                // Lock _operationInitLock immediately to prevent another OCR event from being
                // started until this one has been cancelled.
                lock (_operationInitLock)
                {
                    // If OCR is not complete, then cancel the OCR process
                    if (!_ocrDocumentCompleteEvent.WaitOne(0, false))
                    {
                        // Signal OCR canceled
                        _ocrCanceledEvent.Set();

                        // Give the last OCR operation a chance to initialize before trying to kill it.
                        _canStartNewOperation.WaitOne(_OPERATION_INIT_TIME);

                        // If currently performing an OCR task then kill the OCR engine
                        if (_currentlyInOcr)
                        {
                            // Kill the ocr engine and wait
                            _ssocr.WhackOCREngine();
                        }

                        // Wait for the current ocr to complete
                        _ocrDocumentCompleteEvent.WaitOne();

                        // Reset progress status
                        OnOcrProgressUpdate(new OcrProgressUpdateEventArgs(0.0));

                        // Mutex around member variable changes
                        lock (_lock)
                        {
                            // Reset any old Ocr text
                            _ocrTextStream = "";

                            // Set the file to OCR to empty string
                            _fileToOcr = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22051", ex);
            }
        }

        /// <summary>
        /// Blocks until the OCR thread has completed processing.  If it
        /// has already completed processing this method will return immediately.
        /// </summary>
        public void WaitForOcrCompletion()
        {
            try
            {
                // Block until _ocrDocumentCompleteEvent is signaled
                _ocrDocumentCompleteEvent.WaitOne();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22034", ex);
            }
        }

        /// <summary>
        /// Watches for the _cancelToken to be signaled and, if it is, cancels the currently running
        /// operation.
        /// </summary>
        void CancelTokenWatchThread()
        {
            try
            {
                ExtractException.Assert("ELI32623", "OCR cancel token not initialized.",
                        _cancelToken != null);

                // Wait for either the _cancelToken or _ocrDocumentCompleteEvent handles to be signaled.
                WaitHandle[] waitHandles =
                    new WaitHandle[] { _cancelToken.Value.WaitHandle, _ocrDocumentCompleteEvent };

                // If the _cancelToken handle was signaled, cancel the running operation.
                if (WaitHandle.WaitAny(waitHandles) == 0)
                {
                    CancelOcrOperation();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI34373");
            }
        }

        /// <summary>
        /// Gets the private license code for licensing the OCR engine.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the private license
        /// key for licensing the OCR engine.</returns>
        static string GetSpecialOcrValue()
        {
            return LicenseUtilities.GetMapLabelValue(new MapLabel());
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets a flag indicating whether the current OCR operation is complete yet.
        /// </summary>
        /// <returns><see langword="true"/> if OCR is complete and
        /// <see langword="false"/> if OCR is not complete.</returns>
        public bool OcrFinished
        {
            get
            {
                // Check if the OCR has completed (by setting timeout to 0, this
                // method will return immediately indicating whether _ocrDocumentCompleteEvent
                // has been signaled or not)
                return _ocrDocumentCompleteEvent.WaitOne(0, false);
            }
        }

        /// <summary>
        /// Gets a <see cref="string"/> containing a <see cref="SpatialString"/>
        /// converted to a stringized byte stream that represents the result of
        /// the most recent OCR operation.
        /// </summary>
        /// <returns>A <see cref="string"/> containing a stringized byte stream
        /// version of a <see cref="SpatialString"/>.</returns>
        public string OcrOutput
        {
            get
            {
                lock (_lock)
                {
                    return _ocrTextStream;
                }
            }
        }

        /// <summary>
        /// Returns a SpatialString containing the OCR output.
        /// </summary>
        /// <returns>SpatialString containing the OCR output, or
        /// <see langword="null"/> if the OcrOutput was empty.</returns>
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SpatialString GetOcrSpatialString()
        {
            try
            {
                SpatialString ocrOutput = null;
                if (OcrFinished)
                {
                    // Get the stringized bytestream of the SpatialString object
                    string ocrOutputByteStream = OcrOutput;

                    if (!string.IsNullOrEmpty(ocrOutputByteStream))
                    {
                        // Get a MiscUtils object
                        MiscUtils miscUtils = new MiscUtils();

                        // Convert the stringized bytestream into a SpatialString
                        ocrOutput = (SpatialString)
                            miscUtils.GetObjectFromStringizedByteStream(ocrOutputByteStream);

                        return ocrOutput;
                    }
                }

                return ocrOutput;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26493", ex);
            }
        }

        /// <summary>
        /// Gets the OCR output data as a text string.
        /// </summary>
        /// <returns>A string containing the OCR output as a text string.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetOcrText()
        {
            try
            {
                // Get the SpatialString from the OCR output
                SpatialString ocrOutput = GetOcrSpatialString();

                string ocrText = "";
                if (ocrOutput != null)
                {
                    ocrText = ocrOutput.String;
                }

                return ocrText;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26494", ex);
            }
        }


        /// <summary>
        /// Gets whether the last OCR operation was canceled.
        /// </summary>
        /// <returns><see lanword="true"/> if the last OCR opertaion was canceled
        /// and <see langword="false"/> otherwise.</returns>
        public bool OcrCanceled
        {
            get
            {
                return _ocrCanceledEvent.WaitOne(0, false);
            }
        }

        /// <summary>
        /// Gets or sets the speed-accuracy tradeoff.
        /// </summary>
        /// <value>The speed-accuracy tradeoff.</value>
        /// <returns>The speed-accuracy tradeoff.</returns>
        // This property just gets and sets the tradeoff value, it should never
        // throw an exception.
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public OcrTradeoff Tradeoff
        {
            get
            {
                lock (_lock)
                {
                    return _tradeoff;
                }
            }
            set
            {
                lock (_lock)
                {
                    _tradeoff = value;
                }
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="AsynchronousOcrManager"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="AsynchronousOcrManager"/>.
        /// </overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="AsynchronousOcrManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        // FxCop does not like ther we have created exceptions in Dispose.  You should not throw
        // exceptions from the Dispose method, but we are merely logging them if they occur.
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        protected virtual void Dispose(bool disposing)
        {
            // Dispose managed objects
            if (disposing)
            {
                // Signal the OCR thread to end
                if (_endThreadEvent != null)
                {
                    _endThreadEvent.Set();
                }

                // Check for current OCR operation, kill engine if necessary
                if (_ssocr != null && _currentlyInOcr)
                {
                    _ssocr.WhackOCREngine();
                }

                // Wait for the ocrCompleteEvent to be signaled
                // (add a timeout for sanity sake, this should get signaled
                // no matter what by the time the OCR thread has finished
                if (_ocrThread != null)
                {
                    if (!_ocrThread.Join(_OCR_THREAD_COMPLETION_TIMEOUT))
                    {
                        // If timeout occurs log an exception as this indicates something
                        // very bad has occurred.
                        ExtractException ee = new ExtractException("ELI22049",
                            "Timed out waiting for OCR thread to end!");
                        ee.Log();

                        // As a precaution, abort the thread
                        _ocrThread.Abort();
                    }

                    _ocrThread = null;
                }

                // Release the SSOCR COM object so that SSOCR2.exe goes away
                if (_ssocr != null)
                {
                    // Ensure that SSOCR2 is cleaned up
                    _ssocr.WhackOCREngine();
                    _ssocr = null;
                }

                // Close the event handles
                if (_endThreadEvent != null)
                {
                    _endThreadEvent.Close();
                    _endThreadEvent = null;
                }
                if (_ocrDocumentCompleteEvent != null)
                {
                    _ocrDocumentCompleteEvent.Close();
                    _ocrDocumentCompleteEvent = null;
                }
                if (_newOcrEvent != null)
                {
                    _newOcrEvent.Close();
                    _newOcrEvent = null;
                }
                if (_ocrCanceledEvent != null)
                {
                    _ocrCanceledEvent.Close();
                    _ocrCanceledEvent = null;
                }
                if (_canStartNewOperation != null)
                {
                    _canStartNewOperation.Close();
                    _canStartNewOperation = null;
                }
            }

            // No unmanaged resources to free
        }

        #endregion
    }
}
