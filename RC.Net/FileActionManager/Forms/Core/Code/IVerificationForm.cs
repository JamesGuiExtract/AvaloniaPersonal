using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Event args for a <see cref="IVerificationForm.FileComplete"/> event.
    /// </summary>
    [CLSCompliant(false)]
    public class FileCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Specifies under what circumstances verification of the file completed.
        /// </summary>
        readonly EFileProcessingResult _fileProcessingResult;

        /// <summary>
        /// Initializes a new <see cref="FileCompleteEventArgs"/> instance.
        /// </summary>
        /// <param name="fileProcessingResult">
        /// <see cref="EFileProcessingResult.kProcessingSuccessful"/> if verification of the
        /// document completed successfully, <see cref="EFileProcessingResult.kProcessingCancelled"/>
        /// if verification of the document was cancelled by the user or
        /// <see cref="EFileProcessingResult.kProcessingSkipped"/> if verification of the current file
        /// was skipped, but the user wishes to continue viewing subsequent documents.</param>
        public FileCompleteEventArgs(EFileProcessingResult fileProcessingResult)
            : base()
        {
            _fileProcessingResult = fileProcessingResult;
        }

        /// <summary>
        /// The processing result of the file being shown.
        /// </summary>
        /// <returns></returns>
        public EFileProcessingResult FileProcessingResult
        {
            get
            {
                return _fileProcessingResult;
            }
        }
    }

    /// <summary>
    /// Event args for a <see cref="IVerificationForm.FileRequested"/> event.
    /// </summary>
    [CLSCompliant(false)]
    public class FileRequestedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new <see cref="FileRequestedEventArgs"/> instance.
        /// </summary>
        /// <param name="fileID">The ID of the file being requested for verification.</param>
        public FileRequestedEventArgs(int fileID)
            : base()
        {
            FileID = fileID;
        }

        /// <summary>
        /// Gets the ID of the file being requested for verification.
        /// </summary>
        public int FileID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets whether the file is currently "processing" in the verification task
        /// (waiting on another thread in prefetch).
        /// </summary>
        public bool FileIsAvailable
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Event args for a <see cref="IVerificationForm.FileDelayed"/> event.
    /// </summary>
    [CLSCompliant(false)]
    public class FileDelayedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new <see cref="FileDelayedEventArgs"/> instance.
        /// </summary>
        /// <param name="fileID">The ID of the file whose processing is being delayed.</param>
        public FileDelayedEventArgs(int fileID)
            : base()
        {
            FileID = fileID;
        }

        /// <summary>
        /// Gets the ID of the file whose processing is being delayed.
        /// </summary>
        public int FileID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets whether the file was currently "processing" in the verification task
        /// (waiting on another thread in prefetch).
        /// </summary>
        public bool FileIsAvailable
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Delegate for the <see cref="IVerificationForm.Open"/> method.
    /// </summary>
    /// <param name="fileName">Specifies the filename of the document image to open.</param>
    /// <param name="fileID">The ID of the file being processed.</param>
    /// <param name="actionID">The ID of the action being processed.</param>
    /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
    /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
    [CLSCompliant(false)]
    public delegate void VerificationFormOpen(string fileName, int fileID, int actionID,
        FAMTagManager tagManager, FileProcessingDB fileProcessingDB);

    /// <summary>
    /// Delegate for the <see cref="IVerificationForm.Standby"/> method.
    /// </summary>
    public delegate bool VerificationFormStandby();

    /// <summary>
    /// Represents a <see cref="Form"/> that verifies files in a multi-threaded environment.
    /// </summary>
    [CLSCompliant(false)]
    public interface IVerificationForm
    {
        /// <summary>
        /// Occurs when a file has completed verification.
        /// </summary>
        event EventHandler<FileCompleteEventArgs> FileComplete;

        /// <summary>
        /// Raised when the task requests that a specific file be provided ahead of the files
        /// currently waiting in the task from different threads (prefetched).
        /// </summary>
        event EventHandler<FileRequestedEventArgs> FileRequested;

        /// <summary>
        /// Raised when the task request that processing of a specific file be delayed (returned to
        /// the FPRecordManager queue).
        /// </summary>
        event EventHandler<FileDelayedEventArgs> FileDelayed;

        /// <summary>
        /// Gets whether the control styles of the current Windows theme should be used for the
        /// verification form.
        /// </summary>
        /// <returns><see langword="true"/> to use the control styles of the current Windows theme;
        /// <see langword="false"/> to use Window's classic theme to draw controls.</returns>
        bool UseVisualStyles
        {
            get;
        }

        /// <summary>
        /// A thread-safe method that opens a document for verification.
        /// </summary>
        /// <param name="fileName">The filename of the document to open.</param>
        /// <param name="fileID">The ID of the file being processed.</param>
        /// <param name="actionID">The ID of the action being processed.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        void Open(string fileName, int fileID, int actionID, FAMTagManager tagManager,
            FileProcessingDB fileProcessingDB);

        /// <summary>
        /// A thread-safe method that allows loading of data prior to the <see cref="Open"/> call
        /// so as to reduce the time the <see cref="Open"/> call takes once it is called.
        /// <para><b>Note</b></para>
        /// It can be assumed that once Prefetch is called for a document, <see cref="Open"/> will
        /// be called unless the processing is stopped. 
        /// <para><b>Note</b></para>
        /// It will be up to each implementation of IVerificationForm to determine whether a
        /// separate thread should be used to do the prefetch or whether it should not attempt to
        /// enter the lock section until prefetch is complete. If prefetch is done synchronously,
        /// that could allow another document to "jump ahead" of a document that spent a long time
        /// in prefetch.
        /// </summary>
        /// <param name="fileName">The filename of the document for which to prefetch data.</param>
        /// <param name="fileID">The ID of the file being prefetched.</param>
        /// <param name="actionID">The ID of the action being prefetched.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        void Prefetch(string fileName, int fileID, int actionID, FAMTagManager tagManager,
            FileProcessingDB fileProcessingDB);

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
        /// it also means the form may be opened or closed while the Standby call is still ocurring.
        /// If this happens, the return value of Standby will be ignored; however, Standby should
        /// promptly return in this case to avoid needlessly keeping a thread alive.
        /// </summary>
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        bool Standby();

        /// <summary>
        /// Delays processing of the current file allowing the next file in the queue to be brought
        /// up in its place, though if there are no more files in the queue this will cause the same
        /// file to be re-displayed.
        /// </summary>
        void DelayFile();
    }
}
