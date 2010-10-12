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
    /// Delegate for a function that takes a single <see langword="string"/> as a parameter.
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
    }
}
