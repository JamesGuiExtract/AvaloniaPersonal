using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
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
        /// This event indicates the verification form has been initialized and is ready to load a
        /// document.
        /// </summary>
        event EventHandler<EventArgs> Initialized;

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
        /// Raised when exceptions are raised from the verification UI that should result in the
        /// document failing. Generally this will be raised as a result of errors loading or saving
        /// the document as opposed to interacting with a successfully loaded document.
        /// </summary>
        event EventHandler<VerificationExceptionGeneratedEventArgs> ExceptionGenerated;

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
        /// Gets or sets a value indicating whether the verification form should prevent
        /// any attempts to save dirty data. This may be used after experiencing an error or
        /// when the form is being programmatically closed. (when prompts to save in response to
        /// events that occur are not appropriate)
        /// </summary>
        /// <value><see langword="true"/> if the verification form should prevent any
        /// attempts to save dirty data; otherwise, <see langword="false"/>.</value>
        bool PreventSaveOfDirtyData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether any cancellation of a form closing event should
        /// be disallowed. This is used to ensure that if the FAM requests a verification task to
        /// stop, that the user can't cancel via a save dirty prompt.
        /// </summary>
        /// <value><see langword="true"/> if cancellation of a form closing event should be
        /// disallowed; otherwise <see langword="false"/>.</value>
        bool PreventCloseCancel
        {
            get;
            set;
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
        /// it also means the form may be opened or closed while the Standby call is still occurring.
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
