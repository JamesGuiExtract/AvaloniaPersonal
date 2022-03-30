using Extract.Utilities;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// <summary>
        /// Add a new FileTaskSession record to the database and return an IDisposable wrapper that will close it
        /// </summary>
        /// <param name="sourceFile">Record with the FileID and Action for the FileTaskSession row</param>
        DisposableFileTaskSession CreateFileTaskSession(EmailFileRecord sourceFile);

        /// <summary>
        /// Perform final work for the new files, add pagination history, set action status, etc
        /// </summary>
        /// <param name="fileTaskSessionID">The FileTaskSession.ID to be used for pagination history</param>
        /// <param name="sourceFile">The source record that the other files derive from</param>
        /// <param name="outputFiles">Information about each of the new files</param>
        void ProcessOutputFiles(int fileTaskSessionID, EmailFileRecord sourceFile, IEnumerable<SourceToOutput> outputFiles);

        /// <summary>
        /// Attempt to add a record for this file to the database
        /// </summary>
        /// <param name="fileRecord">Record with information about the file including the proposed file path</param>
        /// <param name="temporaryFilePath">Path to the temporary location of the file (can be used to obtain a page count)</param>
        /// <param name="fileID">The FAMFile.ID if the file was successfully added</param>
        bool TryAddFileToDatabase(EmailPartFileRecord fileRecord, string temporaryFilePath, out int fileID);

        /// <summary>
        /// Build a proposed path for a new file
        /// </summary>
        /// <param name="outputFileRecord">Record with information about the file</param>
        /// <param name="outputDir">The directory for the file</param>
        /// <param name="copyNumber">If > 0 then this will be used in the name of the returned path</param>
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
            bool addHeader = !maybeAttachmentNumber.HasValue;
            using MemoryStream data = GetData(message, addHeader, out bool isHtml);
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
                    || !_databaseClient.TryAddFileToDatabase(outputFileRecord, tempOutputFilePath, out int fileID))
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
        static MemoryStream GetData(MimeMessage message, bool addHeader, out bool isHtml)
        {
            isHtml = false;
            MemoryStream result = new();
            using var writer = new StreamWriter(result, Encoding.Default, 1024, leaveOpen: true);

            if (addHeader)
            {
                string bodyWithHeader = GetBodyWithHeader(message);
                writer.Write(bodyWithHeader);
                isHtml = true;
            }
            else if (!string.IsNullOrEmpty(message.HtmlBody))
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

        // Get the html or text body as html with added header
        static string GetBodyWithHeader(MimeMessage message)
        {
            string body;
            TextConverter converter;

            if (!string.IsNullOrEmpty(message.HtmlBody))
            {
                body = message.HtmlBody;
                converter = new HtmlToHtml
                {
                    HeaderFormat = HeaderFooterFormat.Html
                };
            }
            else if (!string.IsNullOrEmpty(message.TextBody))
            {
                body = message.TextBody;
                converter = new TextToHtml
                {
                    HeaderFormat = HeaderFooterFormat.Html
                };
            }
            else
            {
                return "";
            }

            using StringWriter stringWriter = new();
            using HtmlWriter htmlWriter = new(stringWriter);
            htmlWriter.WriteStartTag(HtmlTagId.Div);
            htmlWriter.WriteStartTag(HtmlTagId.P);
            WriteField(htmlWriter, "From", message.From.ToString());
            WriteField(htmlWriter, "Sent", message.Date.ToString("f", CultureInfo.CurrentCulture));
            WriteField(htmlWriter, "To", message.To.ToString());
            WriteField(htmlWriter, "Subject", message.Subject, false);
            htmlWriter.WriteEndTag(HtmlTagId.Div);
            htmlWriter.WriteText(Environment.NewLine);
            htmlWriter.Flush();
            converter.Header = stringWriter.ToString();

            return converter.Convert(body);
        }

        static void WriteField(HtmlWriter htmlWriter, string key, string value, bool writeBreak = true)
        {
            htmlWriter.WriteText(Environment.NewLine);
            htmlWriter.WriteStartTag(HtmlTagId.B);
            htmlWriter.WriteText(key + ": ");
            htmlWriter.WriteEndTag(HtmlTagId.B);
            htmlWriter.WriteText(value);

            if (writeBreak)
            {
                htmlWriter.WriteStartTag(HtmlTagId.Br);
            }
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
