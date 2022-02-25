using System;
using System.Runtime.InteropServices;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// Data for part of an email that is being turned into a file
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(false)]
    public class EmailPartFileRecord
    {
        /// <summary>
        /// Create an instance to represent an email body
        /// </summary>
        /// <param name="placeholderName">The placeholder name for the body (e.g., text.html)</param>
        /// <param name="fileSize">The length in bytes of the data</param>
        /// <param name="sourceEmailFileRecord">The <see cref="EmailFileRecord"/> of the source email</param>
        public EmailPartFileRecord(string placeholderName, long fileSize, EmailFileRecord sourceEmailFileRecord)
        {
            OriginalName = placeholderName;
            FileSize = fileSize;
            SourceEmailFileRecord = sourceEmailFileRecord;
            IsAttachment = false;
        }

        /// <summary>
        /// Create an instance to represent an email attachment
        /// </summary>
        /// <param name="originalName">The name of the attachment</param>
        /// <param name="attachmentNumber">The sequence number of the attachment</param>
        /// <param name="fileSize">The length in bytes of the data</param>
        /// <param name="sourceEmailFileRecord">The <see cref="EmailFileRecord"/> of the source email</param>
        public EmailPartFileRecord(string originalName, int attachmentNumber, long fileSize, EmailFileRecord sourceEmailFileRecord)
        {
            OriginalName = originalName;
            FileSize = fileSize;
            SourceEmailFileRecord = sourceEmailFileRecord;
            IsAttachment = true;
            AttachmentNumber = attachmentNumber;
        }

        /// <summary>
        /// The name of the attachment or a placeholder name for the body (e.g., text.html)
        /// </summary>
        public string OriginalName { get; }

        /// <summary>
        /// The length, in bytes, of the data
        /// </summary>
        public long FileSize { get; }

        /// <summary>
        /// The <see cref="EmailFileRecord"/> of the source email
        /// </summary>
        public EmailFileRecord SourceEmailFileRecord { get; }

        /// <summary>
        /// The path this part was/will be written to
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The number of pages, if applicable
        /// </summary>
        public int Pages { get; set; }

        /// <summary>
        /// Whether this part is an email attachment
        /// </summary>
        public bool IsAttachment { get; set; }

        /// <summary>
        /// If this part is an email attachment then the sequence number of the attachment
        /// </summary>
        public int AttachmentNumber { get; set; }
    }
}
