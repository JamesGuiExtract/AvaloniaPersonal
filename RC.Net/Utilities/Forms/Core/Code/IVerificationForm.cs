using System;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Event args for a <see cref="IVerificationForm.FileComplete"/> event.
    /// </summary>
    public class FileCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Specifies whether processing should be canceled. (currently processing document should
        /// not be marked complete)
        /// </summary>
        readonly bool _cancelRequested;

        /// <summary>
        /// Initializes a new <see cref="FileCompleteEventArgs"/> instance.
        /// </summary>
        /// <param name="cancelRequested"><see langword="true"/> if processing should be
        /// canceled (currently processing document should not be marked complete);
        /// <see langword="false"/> otherwise.</param>
        public FileCompleteEventArgs(bool cancelRequested)
            : base()
        {
            _cancelRequested = cancelRequested;
        }

        /// <summary>
        /// Gets whether processing should be canceled.
        /// </summary>
        /// <returns><see langword="true"/> if processing should be canceled (currently processing
        /// document should not be marked complete); <see langword="false"/> otherwise.</returns>
        public bool CancelRequested
        {
            get
            {
                return _cancelRequested;
            }
        }
    }

    /// <summary>
    /// Delegate for a function that takes a single <see langword="string"/> as a parameter.
    /// </summary>
    /// <param name="value">The parameter for the delegate method.</param>
    public delegate void StringParameter(string value);

    /// <summary>
    /// Represents a <see cref="Form"/> that verifies files in a multi-threaded environment.
    /// </summary>
    public interface IVerificationForm
    {
        /// <summary>
        /// Occurs when a file has completed verification.
        /// </summary>
        event EventHandler<FileCompleteEventArgs> FileComplete;

        /// <summary>
        /// A thread-safe method that opens a document for verification.
        /// </summary>
        /// <param name="fileName">The filename of the document to open.</param>
        void Open(string fileName);
    }
}
