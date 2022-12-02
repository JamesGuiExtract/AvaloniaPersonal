
using System.Collections.Generic;

namespace WebAPI
{
    /// <summary>
    /// Information about a document
    /// </summary>
    public class QueuedFileDetails
    {
        /// <summary>
        /// The document's ID
        /// </summary>
        public int FileID;

        /// <summary>
        /// The number of pages in the document
        /// </summary>
        public int NumberOfPages;

        /// <summary>
        /// The date (YYYY-MM-DD) the file was submitted
        /// </summary>
        public string DateSubmitted;

        /// <summary>
        /// The path of the file that was submitted
        /// </summary>
        public string OriginalFileName;

        /// <summary>
        /// The user who submitted the file
        /// </summary>
        public string SubmittedByUser;

        /// <summary>
        /// The document type
        /// </summary>
        public string DocumentType;

        /// <summary>
        /// The user comment applied to this document
        /// </summary>
        public string Comment;
    }

    /// <summary>
    /// A result representing a list of documents pending (or skipped) in a queue
    /// </summary>
    public class QueuedFilesResult
    {
        /// <summary>
        /// The list of documents pending (or skipped) in a queue
        /// </summary>
        public IEnumerable<QueuedFileDetails> QueuedFiles;
    }
}