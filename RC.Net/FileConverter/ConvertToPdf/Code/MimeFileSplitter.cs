using Extract.Utilities;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileConverter.ConvertToPdf
{
    public class SourceToOutput
    {
        public int ChildDocumentNumber { get; set; }
        public int DestinationFileID { get; set; }
    }

    [CLSCompliant(false)]
    public interface IMimeFileSplitterDatabaseClient
    {
        DisposableFileTaskSession CreateFileTaskSession(EmailFileRecord sourceFile);

        void ProcessOutputFiles(int fileTaskSessionID, EmailFileRecord sourceFile, IEnumerable<SourceToOutput> outputFiles);

        bool TryAddFileToDatabase(EmailPartFileRecord fileRecord, out int fileID);

        string GetOutputFilePath(EmailPartFileRecord outputFileRecord, string outputDir, int copyNumber);
    }

    [CLSCompliant(false)]
    public class MimeFileSplitter
    {
        private readonly IMimeFileSplitterDatabaseClient _databaseClient;
        private readonly IFAMTagManager _tagManager;
        private readonly string _outputDir;

        /// <summary>
        /// Create a MimeFileSplitter instance with no path tag support
        /// </summary>
        /// <param name="databaseClient">The <see cref="IMimeFileSplitterDatabaseClient"/> implementation used to communicate with the database</param>
        /// <param name="outputDirectory">Path to the output file directory</param>
        public MimeFileSplitter(IMimeFileSplitterDatabaseClient databaseClient, string outputDirectory)
        {
            _databaseClient = databaseClient ?? throw new ArgumentNullException(nameof(databaseClient));
            _outputDir = outputDirectory;
        }

        /// <summary>
        /// Create a MimeFileSplitter instance
        /// </summary>
        /// <param name="databaseClient">The <see cref="IMimeFileSplitterDatabaseClient"/> implementation used to communicate with the database</param>
        /// <param name="outputDirPathTagFunction">Path to the output file directory, can include path tags and functions
        /// (e.g., based on SourceDocumentName)</param>
        /// <param name="tagManager">Used to expand tags and functions in the outputDirPathTagFunction</param>
        public MimeFileSplitter(
            IMimeFileSplitterDatabaseClient databaseClient,
            string outputDirPathTagFunction,
            IFAMTagManager tagManager)
        {
            _databaseClient = databaseClient ?? throw new ArgumentNullException(nameof(databaseClient));
            _tagManager = tagManager ?? throw new ArgumentNullException(nameof(databaseClient));
            _outputDir = outputDirPathTagFunction ?? throw new ArgumentNullException(nameof(databaseClient));
        }

        /// <summary>
        /// Parse the specified source MIME (.eml) file and create files for the message body and each attachment
        /// The source file must exist in the configured IFileProcessingDB so that the pagination table can be populated.
        /// <summary>
        public void SplitFile(EmailFileRecord sourceFileRecord)
        {
            _ = sourceFileRecord ?? throw new ArgumentNullException(nameof(sourceFileRecord));

            using var fileTaskSession = _databaseClient.CreateFileTaskSession(sourceFileRecord);

            try
            {
                string sourceDocName = sourceFileRecord.FilePath;
                using FileStream fileStream = new(sourceDocName, FileMode.Open, FileAccess.Read);
                using MimeMessage message = MimeMessage.Load(fileStream);

                IEnumerable<SourceToOutput> outputFiles = CreateOutputFiles(sourceFileRecord, message);

                _databaseClient.ProcessOutputFiles(fileTaskSession.SessionID, sourceFileRecord, outputFiles);

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53156");
            }
        }

        // Create files from the main message body and any attachments
        IEnumerable<SourceToOutput> CreateOutputFiles(EmailFileRecord sourceFileRecord, MimeMessage message)
        {
            string outputDir = Directory.CreateDirectory(GetOutputDir(sourceFileRecord.FilePath)).FullName;

            // Create file for the body
            int fileID = CreateOutputFile(message, null, sourceFileRecord, outputDir);
            yield return new SourceToOutput { ChildDocumentNumber = 1, DestinationFileID = fileID };

            // Create file for each attachment
            int attachmentNumber = 0;
            foreach (MimeEntity attachment in message.Attachments)
            {
                attachmentNumber++;
                int childDocumentNumber = attachmentNumber + 1;
                if (attachment is MimePart fileAttachment)
                {
                    fileID = CreateOutputFile(fileAttachment, attachmentNumber, sourceFileRecord, outputDir);
                    yield return new SourceToOutput { ChildDocumentNumber = childDocumentNumber, DestinationFileID = fileID };
                }
                else if (attachment is MessagePart messageAttachment)
                {
                    fileID = CreateOutputFile(messageAttachment.Message, attachmentNumber, sourceFileRecord, outputDir);
                    yield return new SourceToOutput { ChildDocumentNumber = childDocumentNumber, DestinationFileID = fileID };
                }
            }
        }

        // Create a file for the message body, on disk/in the database. Returns the associated FAMFile.ID
        int CreateOutputFile(MimeMessage message, int? maybeAttachmentNumber, EmailFileRecord sourceFileRecord, string outputDir)
        {
            using MemoryStream data = GetData(message, out bool isHtml);
            string fileName = "text" + (isHtml ? ".html" : ".txt");

            EmailPartFileRecord record;
            if (maybeAttachmentNumber is int attachmentNumber)
            {
                record = new EmailPartFileRecord(fileName, attachmentNumber, data.Length, sourceFileRecord);
            }
            else
            {
                record = new EmailPartFileRecord(fileName, data.Length, sourceFileRecord);
            }
            return CreateOutputFile(data, record, outputDir);
        }

        // Create a file for an attachment, on disk/in the database. Returns the associated FAMFile.ID
        int CreateOutputFile(MimePart attachment, int attachmentNumber, EmailFileRecord sourceFileRecord, string outputDir)
        {
            using MemoryStream data = GetData(attachment);
            string fileName = attachment.FileName ?? "untitled";

            EmailPartFileRecord record = new(fileName, attachmentNumber, data.Length, sourceFileRecord);
            return CreateOutputFile(data, record, outputDir);
        }

        // Create a unique name for a file using the format <targetBasePath>_<targetFileName> if possible
        // or <targetBasePath>_copy_001_<targetFileName>, etc, if a file of the first format already exists on disk or in the database
        int CreateOutputFile(MemoryStream data, EmailPartFileRecord outputFileRecord, string outputDir)
        {
            // Write the data to a temporary file so that we can get the number of pages, if applicable
            string ext = Path.GetExtension(outputFileRecord.OriginalName);
            using TemporaryFile tempFile = new(ext, false);
            string tempOutputFilePath = tempFile.FileName;
            using (FileStream stream = File.Create(tempOutputFilePath))
            {
                data.Position = 0;
                data.CopyTo(stream);
            }

            // Keep trying to find an original name
            for (int copy = 0; ; copy++)
            {
                outputFileRecord.FilePath = _databaseClient.GetOutputFilePath(outputFileRecord, outputDir, copy);

                // Check the file system and try adding the file to the database
                if (File.Exists(outputFileRecord.FilePath)
                    || !_databaseClient.TryAddFileToDatabase(outputFileRecord, out int fileID))
                {
                    // If the file already exists, add a _copy_ number to the file name
                    continue;
                }

                // Now that the file has been added to the database, copy it to the final destination
                File.Copy(tempOutputFilePath, outputFileRecord.FilePath, true);

                return fileID;
            }
        }

        // Get message text as a stream
        static MemoryStream GetData(MimeMessage message, out bool isHtml)
        {
            isHtml = false;
            MemoryStream result = new();
            using var writer = new StreamWriter(result, Encoding.Default, 1024, leaveOpen: true);

            if (!string.IsNullOrEmpty(message.HtmlBody))
            {
                writer.Write(message.HtmlBody);
                isHtml = true;
            }
            else if (!string.IsNullOrEmpty(message.TextBody))
            {
                writer.Write(message.TextBody);
            }
            return result;
        }

        // Get attachment data as a stream
        static MemoryStream GetData(MimePart fileAttachment)
        {
            MemoryStream result = new();
            fileAttachment.Content.DecodeTo(result);
            return result;
        }

        // Expand the output dir path tag function if using a tag manager, else just return the output dir
        string GetOutputDir(string sourceDocName)
        {
            if (_tagManager == null)
            {
                return _outputDir;
            }

            return _tagManager.ExpandTagsAndFunctions(_outputDir, sourceDocName);
        }
    }
}
