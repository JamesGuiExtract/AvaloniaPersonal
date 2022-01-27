using Extract.Utilities;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    [CLSCompliant(false)]
    public class MimeFileSplitter
    {
        private readonly IFAMTagManager _tagManager;
        private readonly string _outputDirPathTagFunction;
        private readonly IFileProcessingDB _fileProcessingDB;
        private readonly string _taskClassGuid;

        public string SourceAction { get; set; }

        public string OutputAction { get; set; }

        /// <summary>
        /// Create a MimeFileSplitter instance
        /// </summary>
        /// <param name="taskClassGuid">String form of the GUID to be used for FileTaskSession records</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> to add the files to</param>
        /// <param name="outputDirPathTagFunction">Path to the output file directory, can include path tags and functions (e.g., based on SourceDocumentName)</param>
        /// <param name="tagManager">Used to expand tags and functions in the outputDirPathTagFunction</param>
        public MimeFileSplitter(
            string taskClassGuid,
            IFileProcessingDB fileProcessingDB,
            string outputDirPathTagFunction,
            IFAMTagManager tagManager)
        {
            _tagManager = tagManager;
            _outputDirPathTagFunction = outputDirPathTagFunction;
            _fileProcessingDB = fileProcessingDB;
            _taskClassGuid = taskClassGuid;
        }

        /// Parse the specified source MIME (.eml) file and create files for the message body and each attachment
        /// The source file must exist in the configured IFileProcessingDB so that the pagination table can be populated.
        public void SplitFile(FileRecord fileRecord)
        {
            _ = fileRecord ?? throw new ArgumentNullException(nameof(fileRecord));

            int fileTaskSessionID;
            try
            {
                fileTaskSessionID = _fileProcessingDB.StartFileTaskSession(_taskClassGuid, fileRecord.FileID, fileRecord.ActionID);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI53155", "Unable to start file task session", ex);
            }

            try
            {
                string sourceDocName = fileRecord.Name;
                using FileStream fileStream = new(sourceDocName, FileMode.Open, FileAccess.Read);
                using MimeMessage message = MimeMessage.Load(fileStream);

                foreach (SourceToOutput sourceToOutputInfo in CreateOutputFiles(message, fileRecord))
                {
                    WriteToPaginationTable(sourceToOutputInfo.DestinationFileID, sourceDocName, sourceToOutputInfo.ChildDocumentNumber, fileTaskSessionID);
                    QueueOutputFile(sourceToOutputInfo.DestinationFileID, fileRecord.WorkflowID);
                }

                QueueSourceFile(fileRecord.FileID, fileRecord.WorkflowID);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53156");
            }
            finally
            {
                try
                {
                    _fileProcessingDB.EndFileTaskSession(fileTaskSessionID, 0, 0, false);
                }
                catch { }
            }
        }

        // Add a row to the pagination table to associate the output file with it's source
        void WriteToPaginationTable(int outputFileID, string sourceFileName, int fileNumber, int fileTaskSessionID)
        {
            var sourcePageInfo = Enumerable.Repeat(
                new StringPairClass
                {
                    StringKey = sourceFileName,
                    StringValue = fileNumber.ToString(CultureInfo.InvariantCulture)
                }, 1).ToIUnknownVector();

            _fileProcessingDB.AddPaginationHistory(outputFileID, sourcePageInfo, null, fileTaskSessionID);
        }

        // Set output file to pending if the OutputAction is specified
        void QueueOutputFile(int fileID, int workflowID)
        {
            if (!string.IsNullOrWhiteSpace(OutputAction))
            {
                _fileProcessingDB.SetStatusForFile(
                    fileID,
                    OutputAction,
                    workflowID,
                    EActionStatus.kActionPending,
                    vbQueueChangeIfProcessing: false,
                    vbAllowQueuedStatusOverride: false,
                    poldStatus: out EActionStatus _);
            }
        }

        // Set output file to pending if the OutputAction is specified
        void QueueSourceFile(int fileID, int workflowID)
        {
            if (!string.IsNullOrWhiteSpace(SourceAction))
            {
                _fileProcessingDB.SetStatusForFile(
                    fileID,
                    SourceAction,
                    workflowID,
                    EActionStatus.kActionPending,
                    vbQueueChangeIfProcessing: true,
                    vbAllowQueuedStatusOverride: false,
                    poldStatus: out EActionStatus _);
            }
        }

        class SourceToOutput
        {
            public int ChildDocumentNumber { get; set; }
            public int DestinationFileID { get; set; }
        }

        // Create files from the main message body and any attachments
        IEnumerable<SourceToOutput> CreateOutputFiles(MimeMessage message, FileRecord fileRecord)
        {
            string sourceDocName = fileRecord.Name;
            string targetBaseName = Path.GetFileNameWithoutExtension(sourceDocName);

            DirectoryInfo outputDir =
                Directory.CreateDirectory(_tagManager.ExpandTagsAndFunctions(_outputDirPathTagFunction, sourceDocName));

            string targetBasePath = Path.Combine(outputDir.FullName, targetBaseName);

            // Create file for the body
            int fileID = CreateOutputFile(
                message,
                UtilityMethods.FormatInvariant($"{targetBasePath}_body"),
                fileRecord);
            yield return new SourceToOutput { ChildDocumentNumber = 1, DestinationFileID = fileID };

            // Create file for each attachment
            int attachmentNumber = 0;
            foreach (MimeEntity attachment in message.Attachments)
            {
                attachmentNumber++;
                int childDocumentNumber = attachmentNumber + 1;
                string outputPath = UtilityMethods.FormatInvariant($"{targetBasePath}_attachment_{attachmentNumber:D3}");
                if (attachment is MimePart fileAttachment)
                {
                    fileID = CreateOutputFile(fileAttachment, outputPath, fileRecord);
                    yield return new SourceToOutput { ChildDocumentNumber = childDocumentNumber, DestinationFileID = fileID };
                }
                else if (attachment is MessagePart messageAttachment)
                {
                    fileID = CreateOutputFile(messageAttachment.Message, outputPath, fileRecord);
                    yield return new SourceToOutput { ChildDocumentNumber = childDocumentNumber, DestinationFileID = fileID };
                }
            }
        }

        // Create a file for the message body, on disk/in the database. Returns the associated FAMFile.ID
        int CreateOutputFile(MimeMessage message, string targetBasePath, FileRecord sourceFile)
        {
            using var data = GetData(message, out bool isHtml);
            string filename = "text" + (isHtml ? ".html" : ".txt");
            return CreateOutputFile(data, targetBasePath, filename, sourceFile);
        }

        // Create a file for an attachment, on disk/in the database. Returns the associated FAMFile.ID
        int CreateOutputFile(MimePart attachment, string targetBasePath, FileRecord sourceFile)
        {
            using var data = GetData(attachment);
            string filename = attachment.FileName ?? "untitled";
            return CreateOutputFile(data, targetBasePath, filename, sourceFile);
        }

        // Create a unique name for a file using the format <targetBasePath>_<targetFileName> if possible
        // or <targetBasePath>_copy_001_<targetFileName>, etc, if a file of the first format already exists on disk or in the database
        int CreateOutputFile(MemoryStream data, string targetBasePath, string targetFileName, FileRecord sourceFile)
        {
            // Write the data to a temporary file so that we can get the number of pages, if applicable
            int numberOfPages = 0;
            string ext = Path.GetExtension(targetFileName);
            using TemporaryFile tempFile = new(ext, false);
            string tempOutputFilePath = tempFile.FileName;
            using (FileStream stream = File.Create(tempOutputFilePath))
            {
                data.Position = 0;
                data.CopyTo(stream);
            }

            // Don't try to get pages from known text file types
            if (!string.IsNullOrEmpty(ext)
                && !ext.Equals(".txt", StringComparison.OrdinalIgnoreCase)
                && !ext.Equals(".rtf", StringComparison.OrdinalIgnoreCase)
                && !ext.Equals(".html", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    numberOfPages = UtilityMethods.GetNumberOfPagesInImage(tempOutputFilePath);
                }
                catch (Exception)
                {
                    // Getting the number of pages from a non-image file will fail but it's not that important
                }
            }

            // Keep trying to find an original name
            for (int copy = 0; ; copy++)
            {
                // Build the filename
                string outputFilePath = targetBasePath;

                if (copy > 0)
                {
                    outputFilePath = UtilityMethods.FormatInvariant($"{outputFilePath}_copy_{copy:D3}");
                }

                outputFilePath += "_" + targetFileName;

                var outputFileRecord = new FileRecordClass
                {
                    Name = outputFilePath,
                    Pages = numberOfPages,
                    Priority = sourceFile.Priority,
                    WorkflowID = sourceFile.WorkflowID
                };

                // Check the file system and try adding the file to the database
                if (File.Exists(outputFilePath)
                    || !TryAddFileToDatabase(outputFileRecord, data.Length, out int fileID))
                {
                    // If the file already exists, add a _copy_ number added to the file name
                    continue;
                }

                // Now that the file has been added to the database, copy it to the final destination
                File.Copy(tempOutputFilePath, outputFilePath, true);

                return fileID;
            }
        }

        // Try to add a file to the database, return true if successful, false if the file already exists in the database
        // Throws an exception if there is a different error adding the file (add failed but the file does not appear to be in the database already)
        bool TryAddFileToDatabase(IFileRecord fileRecord, long fileSize, out int fileID)
        {
            try
            {
                fileID = _fileProcessingDB.AddFileNoQueue(fileRecord.Name, fileSize, fileRecord.Pages, fileRecord.Priority, fileRecord.WorkflowID);
                return true;
            }
            catch (Exception ex)
            {
                ADODB.Recordset recordset = null;
                try
                {
                    // Query to see if the e.OutputFileName can be found in the database.
                    string safeName = fileRecord.Name.Replace("'", "''");
                    string query = UtilityMethods.FormatInvariant(
                        $"SELECT [ID] FROM [FAMFile] WHERE [FileName] = '{safeName}'");

                    recordset = _fileProcessingDB.GetResultsForQuery(query);
                    if (recordset.EOF)
                    {
                        // The file was not in the database, the call failed for another reason.
                        throw ex.AsExtract("ELI53157");
                    }

                    fileID = -1;
                    return false;
                }
                finally
                {
                    recordset.Close();
                }
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
    }
}
