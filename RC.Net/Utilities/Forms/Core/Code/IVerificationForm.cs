using System;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a <see cref="Form"/> that verifies files in a multi-threaded environment.
    /// </summary>
    public interface IVerificationForm
    {
        /// <summary>
        /// Occurs when a file has completed verification.
        /// </summary>
        event EventHandler<EventArgs> FileVerified;

        /// <summary>
        /// Occurs when an exception is generated.
        /// </summary>
        event EventHandler<ExtractExceptionEventArgs> ExceptionGenerated;

        /// <summary>
        /// A thread-safe method that opens a document for verification.
        /// </summary>
        /// <param name="fileName">The filename of the document to open.</param>
        void Open(string fileName);
    }
}
