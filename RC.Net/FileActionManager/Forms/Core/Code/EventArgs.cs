using System;
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
        /// Initializes a new <see cref="FileCompleteEventArgs"/> instance.
        /// </summary>
        /// <param name="fileId">The ID of the file that has been completed.</param>
        /// <param name="fileProcessingResult">
        /// <see cref="EFileProcessingResult.kProcessingSuccessful"/> if verification of the
        /// document completed successfully, <see cref="EFileProcessingResult.kProcessingCancelled"/>
        /// if verification of the document was cancelled by the user or
        /// <see cref="EFileProcessingResult.kProcessingSkipped"/> if verification of the current file
        /// was skipped, but the user wishes to continue viewing subsequent documents.</param>
        public FileCompleteEventArgs(int fileId, EFileProcessingResult fileProcessingResult)
            : base()
        {
            FileId = fileId;
            FileProcessingResult = fileProcessingResult;
        }

        /// <summary>
        /// Gets the ID of the file that has been completed.
        /// </summary>
        public int FileId
        {
            get;
            private set;
        }

        /// <summary>
        /// The processing result of the file being shown.
        /// </summary>
        /// <returns></returns>
        public EFileProcessingResult FileProcessingResult
        {
            get;
            private set;
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
    /// Event arguments for a <see cref="IVerificationForm.ExceptionGenerated"/> event.
    /// </summary>
    public class VerificationExceptionGeneratedEventArgs : ExtractExceptionEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationExceptionGeneratedEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The <see cref="ExtractException"/> that occurred.</param>
        /// <param name="canProcessingContinue"><see langword="true"/> if the user should be given
        /// the option to continue verification on the next document; <see langword="false"/> if the
        /// error should prevent the possibility of continuing the verification session.</param>
        public VerificationExceptionGeneratedEventArgs(ExtractException exception,
            bool canProcessingContinue)
            : base(exception)
        {
            try
            {
                CanProcessingContinue = canProcessingContinue;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI37833",
                    "Failed to initialize ExceptionEventArgs!", ex);
            }
        }

        /// <summary>
        /// Gets a value indicating if the user should be given the option to continue verification
        /// on the next document.
        /// </summary>
        /// <value><see langword="true"/> if the user should be given the option to continue
        /// verification on the next document; <see langword="false"/> if the error should prevent
        /// the possibility of continuing the verification session.</value>
        public bool CanProcessingContinue
        {
            get;
            private set;
        }
    }
}
