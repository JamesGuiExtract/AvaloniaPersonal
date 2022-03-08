using Extract.FileConverter;
using Extract.FileConverter.ConvertToPdf;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Dependency for <see cref="MimeFileSplitter"/> that handles interactions with an <see cref="IFileProcessingDB"/>
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(false)]
    public class DatabaseClientForMimeFileSplitter : IMimeFileSplitterDatabaseClient
    {
        private readonly IFileProcessingDB _fileProcessingDB;
        private readonly string _outputAction;

        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> to use</param>
        /// <param name="sourceAction">Optional action to queue source documents to</param>
        /// <param name="outputAction">Optional action to queue output documents to</param>
        public DatabaseClientForMimeFileSplitter(IFileProcessingDB fileProcessingDB, string outputAction)
        {
            _fileProcessingDB = fileProcessingDB ?? throw new ArgumentNullException(nameof(fileProcessingDB));
            _outputAction = outputAction;
        }

        /// <summary>
        /// Create a new file task session for the file that this instance is processing
        /// </summary>
        public DisposableFileTaskSession CreateFileTaskSession(EmailFileRecord sourceFile)
        {
            try
            {
                int sessionID = _fileProcessingDB.StartFileTaskSession(
                    Constants.TaskClassSplitMimeFile,
                    sourceFile.FileID,
                    sourceFile.ActionID);

                return new(_fileProcessingDB, sessionID);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53240");
            }
        }

        /// <summary>
        /// Record pagination history and queue output/source documents to appropriate actions
        /// </summary>
        /// <param name="fileTaskSessionID">The file task session ID to use for writing the pagination history</param>
        /// <param name="outputFiles">The output document info</param>
        public void ProcessOutputFiles(int fileTaskSessionID, EmailFileRecord sourceFile, IEnumerable<SourceToOutput> outputFiles)
        {
            try
            {
                foreach (SourceToOutput sourceToOutputInfo in outputFiles)
                {
                    WriteToPaginationTable(
                        sourceToOutputInfo.DestinationFileID,
                        sourceFile.FilePath,
                        sourceToOutputInfo.ChildDocumentNumber,
                        fileTaskSessionID);
                    QueueOutputFile(sourceToOutputInfo.DestinationFileID, sourceFile.WorkflowID);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53232");
            }
        }

        /// <summary>
        /// Attempt to add a new file record to the database. Returns true if successful and false if the file
        /// is already in the database. Throws an exception if the attempt fails for another reason.
        /// </summary>
        /// <param name="fileRecord">Record with information about the file including the proposed file path</param>
        /// <param name="temporaryFilePath">Path to the temporary location of the file (can be used to obtain a page count)</param>
        /// <param name="fileID">The FAMFile.ID if the file was successfully added</param>
        public bool TryAddFileToDatabase(EmailPartFileRecord fileRecord, string temporaryFilePath, out int fileID)
        {
            try
            {
                if (fileRecord.Pages <= 0)
                {
                    fileRecord.Pages = GetPageCount(temporaryFilePath);
                }

                fileID = _fileProcessingDB.AddFileNoQueue(
                    fileRecord.FilePath,
                    fileRecord.FileSize,
                    fileRecord.Pages,
                    fileRecord.SourceEmailFileRecord.Priority,
                    fileRecord.SourceEmailFileRecord.WorkflowID);
                return true;
            }
            catch (Exception ex)
            {
                ADODB.Recordset recordset = null;
                try
                {
                    // Query to see if the e.OutputFileName can be found in the database.
                    string safeName = fileRecord.FilePath.Replace("'", "''");
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

        /// <summary>
        /// Build a path for an email part using the output dir, the source email name and other information
        /// </summary>
        /// <param name="outputFileRecord">Information about the email part, including the source email info</param>
        /// <param name="outputDir">The directory to use for the path</param>
        /// <param name="copyNumber">If greater than 0 this will be used as part of the path</param>
        public string GetOutputFilePath(EmailPartFileRecord outputFileRecord, string outputDir, int copyNumber = 0)
        {
            try
            {
                string targetBaseName = Path.GetFileNameWithoutExtension(outputFileRecord.SourceEmailFileRecord.FilePath);
                string targetBasePath = Path.Combine(outputDir, targetBaseName);

                StringBuilder outputPathBuilder = new(targetBasePath);
                if (outputFileRecord.IsAttachment)
                {
                    outputPathBuilder.Append("_attachment_")
                        .AppendFormat(CultureInfo.InvariantCulture, "{0:D3}", outputFileRecord.AttachmentNumber);
                }
                else
                {
                    outputPathBuilder.Append("_body");
                }

                if (copyNumber > 0)
                {
                    outputPathBuilder.Append("_copy_").AppendFormat(CultureInfo.InvariantCulture, "{0:D3}", copyNumber);
                }

                outputPathBuilder.Append('_');
                outputPathBuilder.Append(outputFileRecord.OriginalName);

                return outputPathBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53238");
            }
        }

        private void WriteToPaginationTable(int outputFileID, string sourceFileName, int fileNumber, int fileTaskSessionID)
        {
            var sourcePageInfo = Enumerable.Repeat(
                new StringPairClass
                {
                    StringKey = sourceFileName,
                    StringValue = fileNumber.ToString(CultureInfo.InvariantCulture)
                }, 1).ToIUnknownVector();

            _fileProcessingDB.AddPaginationHistory(outputFileID, sourcePageInfo, null, fileTaskSessionID);
        }

        private void QueueOutputFile(int fileID, int workflowID)
        {
            if (!string.IsNullOrWhiteSpace(_outputAction))
            {
                _fileProcessingDB.SetStatusForFile(
                    fileID,
                    _outputAction,
                    workflowID,
                    EActionStatus.kActionPending,
                    vbQueueChangeIfProcessing: false,
                    vbAllowQueuedStatusOverride: false,
                    poldStatus: out EActionStatus _);
            }
        }

        private static int GetPageCount(string filePath)
        {
            var filePathHolder = FilePathHolder.Create(filePath);

            // Don't try to get pages unless an Image, Pdf or Unknown type
            if (filePathHolder.FileType == FileType.Image
                || filePathHolder.FileType == FileType.Pdf
                || filePathHolder.FileType == FileType.Unknown)
            {
                try
                {
                    return UtilityMethods.GetNumberOfPagesInImage(filePath);
                }
                catch
                {
                    // Getting the number of pages can fail but it's not that important
                }
            }

            return 0;
        }

    }
}