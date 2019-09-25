using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.Redaction.Davidson;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Interface definition for <see cref="ProcessRichTextBatchesTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("50773346-592C-444C-A8AB-17C5DA0699CD")]
    [CLSCompliant(false)]
    public interface IProcessRichTextBatchesTask :
        ICategorizedComponent,
        IConfigurableObject,
        IMustBeConfiguredObject,
        ICopyableObject,
        IFileProcessingTask,
        ILicensedComponent,
        IPersistStream
    {
        /// <summary>
        /// Path (path tags will be expanded) for new, RTF, files
        /// </summary>
        string OutputDirectory { get; set; }

        /// <summary>
        /// Path to redacted file (function with SourceDocName of RTF file path)
        /// </summary>
        string RedactedFile { get; set; }

        /// <summary>
        /// Action to queue source, batch, files to
        /// </summary>
        string SourceAction { get; set; }

        /// <summary>
        /// Action to queue new, RTF, files to
        /// </summary>
        string OutputAction { get; set; }

        /// <summary>
        /// Whether to divide batch files (if <c>true</c>) or update them from their redacted parts (<c>false</c>)
        /// </summary>
        bool DivideBatchIntoRichTextFiles { get; set; }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> that
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    [ComVisible(true)]
    [Guid("79EE2A84-5FA5-4F9A-95C8-72F1E973CC1D")]
    [ProgId("Extract.FileActionManager.FileProcessors.ProcessRichTextBatchesTask")]
    public class ProcessRichTextBatchesTask : IProcessRichTextBatchesTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "RTF: Process batches";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        static readonly string _SPLIT_RICH_TEXT_BATCHES_TASK_GUID = "5F37ABA6-7D18-4AB9-9ABE-79CE0F49C903";
        static readonly string _UPDATE_RICH_TEXT_BATCHES_TASK_GUID = "4FF8821E-D98A-4B45-AD1A-5E7F62621581";

        static readonly Encoding _encoding = Encoding.GetEncoding("windows-1252");

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        CancellationTokenSource _cancelTokenSource;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRichTextBatchesTask"/> class.
        /// </summary>
        public ProcessRichTextBatchesTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRichTextBatchesTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="ProcessRichTextBatchesTask"/> from which settings should
        /// be copied.</param>
        public ProcessRichTextBatchesTask(ProcessRichTextBatchesTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48364");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Directory (path tags will be expanded) for new, RTF, files
        /// </summary>
        public string OutputDirectory { get; set; } = UtilityMethods.FormatInvariant($@"$DirOf($DirOf(<SourceDocName>))\Split\$FileNoExtOf(<SourceDocName>)\{RichTextFormatBatchProcessor.SubBatchNumber}");

        /// <summary>
        /// Action required to be complete for all linked output documents
        /// </summary>
        public string RedactedAction { get; set; }

        /// <summary>
        /// Path to redacted file (function with SourceDocName of RTF file path)
        /// </summary>
        public string RedactedFile { get; set; } = "$InsertBeforeExt(<SourceDocName>,.redacted)";

        /// <summary>
        /// Path for updated batch file (function with SourceDocName of batch file path)
        /// </summary>
        public string UpdatedBatchFile { get; set; } = "$InsertBeforeExt(<SourceDocName>,.redacted)";

        /// <summary>
        /// Action to queue source, batch, files to
        /// </summary>
        public string SourceAction { get; set; }

        /// <summary>
        /// Action to queue new, RTF, files to
        /// </summary>
        public string OutputAction { get; set; }

        /// <summary>
        /// Whether to divide batch files (if <c>true</c>) or update them from their redacted parts (<c>false</c>)
        /// </summary>
        public bool DivideBatchIntoRichTextFiles { get; set; } = true;

        #endregion Properties

        #region IProcessRichTextBatchesTask Members

        #endregion IProcessRichTextBatchesTask Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ProcessRichTextBatchesTask"/>.
        /// </summary>
        /// <returns><c>true</c> if the configuration was successfully updated or
        /// <c>false</c> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI48365", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (ProcessRichTextBatchesTask)Clone();

                FileProcessingDB fileProcessingDB = new FileProcessingDB();
                fileProcessingDB.ConnectLastUsedDBThisProcess();

                using (var dialog = new ProcessRichTextBatchesTaskSettingsDialog(cloneOfThis, fileProcessingDB))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dialog.Settings);
                        return true;
                    }
                }

                fileProcessingDB.CloseAllDBConnections();

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI48366",
                    "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the object has been configured and <c>false</c> otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(OutputDirectory))
                {
                    return false;
                }

                bool updatingBatch = !DivideBatchIntoRichTextFiles;
                if (updatingBatch)
                {
                    if (string.IsNullOrWhiteSpace(RedactedFile)
                        || string.IsNullOrWhiteSpace(UpdatedBatchFile))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI48367", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ProcessRichTextBatchesTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ProcessRichTextBatchesTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new ProcessRichTextBatchesTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI48368", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ProcessRichTextBatchesTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                if (!(pObject is ProcessRichTextBatchesTask task))
                {
                    throw new InvalidCastException("Invalid copy-from object. Requires ProcessRichTextBatchesTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI48369", "Unable to copy object.", ex);
            }
        }

        #endregion ICopyableObject Members

        #region IFileProcessingTask Members

        /// <summary>
        /// Gets the minimum stack size needed for the thread in which this task is to be run.
        /// </summary>
        /// <value>
        /// The minimum stack size needed for the thread in which this task is to be run.
        /// </value>
        [CLSCompliant(false)]
        public uint MinStackSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns a value indicating that the task does not display a UI
        /// </summary>
        public bool DisplaysUI
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            _cancelTokenSource?.Cancel();
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            _cancelTokenSource?.Dispose();
        }

        /// <summary>
        /// Called to notify the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <c>true</c>. If the processor wants to cancel processing,
        ///	it should return <c>false</c>. If the processor does not immediately know
        ///	whether processing should be cancelled right away, it may block until it does know,
        ///	and return at that time.
        /// <para><b>Note</b></para>
        /// This call will be made on a different thread than the other calls, so the Standby call
        /// must be thread-safe. This allows the file processor to block on the Standby call, but
        /// it also means that call to <see cref="ProcessFile"/> or <see cref="Close"/> may come
        /// while the Standby call is still occurring. If this happens, the return value of Standby
        /// will be ignored; however, Standby should promptly return in this case to avoid
        /// needlessly keeping a thread alive.
        /// </summary>
        /// <returns><c>true</c> to standby until the next file is supplied;
        /// <c>false</c> to cancel processing.</returns>
        public bool Standby()
        {
            return true;
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use to expand path tags and
        /// functions.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="pFileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the task to carry out requests for files to be checked out, released or re-ordered
        /// in the queue.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            IFileRequestHandler pFileRequestHandler)
        {
            try
            {
                _cancelTokenSource = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI48370",
                    "Unable to initialize \"" + _COMPONENT_DESCRIPTION + "\" task.");
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="pFileRecord">The file record that contains the info of the file being 
        /// processed.</param>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">A File Action Manager Tag Manager for expanding tags.</param>
        /// <param name="pDB">The File Action Manager database.</param>
        /// <param name="pProgressStatus">Object to provide progress status updates to caller.
        /// </param>
        /// <param name="bCancelRequested"><c>true</c> if cancel was requested; 
        /// <c>false</c> otherwise.</param>
        /// <returns>An <see cref="EFileProcessingResult"/> indicating the result of the
        /// processing.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord,
                                                  int nActionID,
                                                  FAMTagManager pFAMTM,
                                                  FileProcessingDB pDB,
                                                  ProgressStatus pProgressStatus,
                                                  bool bCancelRequested)
        {
            try
            {
                if (bCancelRequested)
                {
                    return EFileProcessingResult.kProcessingCancelled;
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI48371", _COMPONENT_DESCRIPTION);

                if (pFAMTM == null)
                {
                    pFAMTM = new FAMTagManager();
                }

                string taskGUID = DivideBatchIntoRichTextFiles ? _SPLIT_RICH_TEXT_BATCHES_TASK_GUID : _UPDATE_RICH_TEXT_BATCHES_TASK_GUID;
                int fileTaskSessionID = pDB.StartFileTaskSession(taskGUID, pFileRecord.FileID, pFileRecord.ActionID);
                DateTime sessionStartTime = DateTime.Now;

                try
                {
                    if (DivideBatchIntoRichTextFiles)
                    {
                        DivideBatch(pFileRecord, pFAMTM, pDB, pProgressStatus, fileTaskSessionID);
                    }
                    else
                    {
                        UpdateBatch(pFileRecord, pFAMTM, pDB, pProgressStatus);
                    }
                }
                catch (OperationCanceledException)
                {
                    pDB.EndFileTaskSession(fileTaskSessionID, (DateTime.Now - sessionStartTime).TotalSeconds, 0, 0);
                    return EFileProcessingResult.kProcessingCancelled;
                }

                pDB.EndFileTaskSession(fileTaskSessionID, (DateTime.Now - sessionStartTime).TotalSeconds, 0, 0);
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                var uex = ex.AsExtract("ELI48381");
                uex.AddDebugData("Batch file name", pFileRecord.Name);

                throw uex.CreateComVisible("ELI48372", "Error processing rich text batch");
            }
        }

        #endregion IFileProcessingTask Members

        #region IAccessRequired Members

        /// <summary>
        /// Returns bool value indicating if the task requires admin access.
        /// </summary>
        /// <returns><c>true</c> if the task requires admin access
        /// <c>false</c> if task does not require admin access.</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IAccessRequired Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><c>true</c> if the component is licensed; <c>false</c> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            try
            {
                return LicenseUtilities.IsLicensed(_LICENSE_ID);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI48373",
                    "Unable to determine license status.", ex);
            }
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns><see cref="HResult.Ok"/> if changes have been made; 
        /// <see cref="HResult.False"/> if changes have not been made.</returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    DivideBatchIntoRichTextFiles = reader.ReadBoolean();
                    OutputDirectory = reader.ReadString();
                    RedactedAction = reader.ReadString();
                    RedactedFile = reader.ReadString();
                    UpdatedBatchFile = reader.ReadString();
                    OutputAction = reader.ReadString();
                    SourceAction = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI48374",
                    "Unable to load object from stream.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <c>true</c>, the flag should be cleared. If 
        /// <c>false</c>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(DivideBatchIntoRichTextFiles);
                    writer.Write(OutputDirectory);
                    writer.Write(RedactedAction);
                    writer.Write(RedactedFile);
                    writer.Write(UpdatedBatchFile);
                    writer.Write(OutputAction);
                    writer.Write(SourceAction);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI48375",
                    "Unable to save object to stream", ex);
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in 
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStream Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// <see cref="ExtractCategories.FileProcessorsGuid"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// <see cref="ExtractCategories.FileProcessorsGuid"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="ProcessRichTextBatchesTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="ProcessRichTextBatchesTask"/> from which to copy.</param>
        void CopyFrom(ProcessRichTextBatchesTask task)
        {
            DivideBatchIntoRichTextFiles = task.DivideBatchIntoRichTextFiles;
            OutputDirectory = task.OutputDirectory;
            UpdatedBatchFile = task.UpdatedBatchFile;
            RedactedAction = task.RedactedAction;
            RedactedFile = task.RedactedFile;
            OutputAction = task.OutputAction;
            SourceAction = task.SourceAction;

            _dirty = true;
        }


        // Create the file on disk and add it to the DB
        static OutputFileData CreateOutputFile(OutputFileData outputFile, FileProcessingDB fileProcessingDB, EFilePriority priority, int workflowID)
        {
            byte[] outputBytes = _encoding.GetBytes(outputFile.Contents);

            Directory.CreateDirectory(Path.GetDirectoryName(outputFile.FileNameBase));
            string extension = outputFile.FileType == OutputFileType.RichTextFile ? ".rtf" : ".txt";
            string outputFileName = outputFile.FileNameBase + extension;

            ExtractException.Assert("ELI48386", $"Output file already exists!",
                !File.Exists(outputFileName),
                "Output file name", outputFileName);

            bool fileExistsInDB = false;
            try
            {
                outputFile.FileID = fileProcessingDB.AddFileNoQueue(outputFileName, outputBytes.LongLength, 0, priority, workflowID);
            }
            catch (Exception ex)
            {
                ADODB.Recordset recordset = null;
                try
                {
                    // Query to see if the e.OutputFileName can be found in the database.
                    string safeName = outputFileName.Replace("'", "''");
                    string query = UtilityMethods.FormatInvariant(
                        $"SELECT [ID] FROM [FAMFile] WHERE [FileName] = '{safeName}'");

                    recordset = fileProcessingDB.GetResultsForQuery(query);
                    if (recordset.EOF)
                    {
                        // The file was not in the database, the call failed for another reason.
                        throw ex.AsExtract("ELI48382");
                    }
                    fileExistsInDB = true;
                }
                finally
                {
                    recordset.Close();
                }
            }

            ExtractException.Assert("ELI48383", $"Output file already exists in the database!",
                !fileExistsInDB,
                "Output file name", outputFileName);

            File.WriteAllBytes(outputFileName, outputBytes);

            return outputFile;
        }

        // Add a row to the pagination table to associate the output file with it's source
        static OutputFileData WriteToPaginationTable(OutputFileData outputFile, string batchFileName, int fileNumber, FileProcessingDB fileProcessingDB, int fileTaskSessionID)
        {
            var sourcePageInfo = Enumerable.Repeat(
                new StringPairClass
                {
                    StringKey = batchFileName,
                    StringValue = fileNumber.ToString(CultureInfo.InvariantCulture)
                }, 1).ToIUnknownVector();

            fileProcessingDB.AddPaginationHistory(
                outputFile.FileID, sourcePageInfo, null, fileTaskSessionID);

            return outputFile;
        }

        // Set output file to pending
        static OutputFileData QueueOutputFile(OutputFileData outputFile, FileProcessingDB fileProcessingDB, string action, int workflowID)
        {
            fileProcessingDB.SetStatusForFile(
                outputFile.FileID,
                action,
                workflowID,
                EActionStatus.kActionPending,
                vbQueueChangeIfProcessing: true,
                vbAllowQueuedStatusOverride: false,
                poldStatus: out EActionStatus _);

            return outputFile;
        }

        // Read a file and return the lines as a lazy enumerable that will report progress as it's iterated
        IEnumerable<string> GetLinesWithProgressReporting(string fileName, ProgressStatus progressStatus)
        {
            // Keep this as an enumerable in order to save on memory usage
            var lines = File.ReadLines(fileName);

            // Count the lines if reporting progress
            // (this will cause the file to be read twice but files are too big to keep in memory if processing with 8 threads)
            var lineCount = progressStatus == null ? 0 : lines.Count();

            progressStatus?.InitProgressStatus(UtilityMethods.FormatCurrent($"Parsing line 1/{lineCount:N0}"), 1, lineCount / 10 + 1, false);

            return lines.Select((line, index) =>
            {
                var lineNum = index + 1;
                if (lineNum % 10 == 0)
                {
                    _cancelTokenSource.Token.ThrowIfCancellationRequested();
                    progressStatus?.StartNextItemGroup(UtilityMethods.FormatCurrent($"Parsing line {lineNum:N0}/{lineCount:N0}"), 1);
                }
                return line;
            });
        }

        // Iterate a lazy enumeration to resolve all side-effects and report progress
        void IterateAndReportProgress<T>(IEnumerable<T> thunk, int itemCount, string progressMessage, ProgressStatus progressStatus)
        {
            progressStatus?.InitProgressStatus(
                progressMessage + UtilityMethods.FormatCurrent($" 1/{itemCount:N0}"), 1, itemCount / 10 + 1, false);

            foreach (var index in thunk.Select((_, index) => index))
            {
                if ((index + 1) % 10 == 0)
                {
                    _cancelTokenSource.Token.ThrowIfCancellationRequested();
                    progressStatus?.StartNextItemGroup(
                        progressMessage + UtilityMethods.FormatCurrent($" {index + 1:N0}/{itemCount:N0}"), 1);
                }
            }

            progressStatus?.CompleteCurrentItemGroup();
        }

        // Create text files, add them to the database and record them in the pagination table
        void DivideBatch(FileRecord fileRecord,
                         FAMTagManager tagManager,
                         FileProcessingDB fileProcessingDB,
                         ProgressStatus progressStatus,
                         int fileTaskSessionID)
        {
            try
            {
                progressStatus?.InitProgressStatus("Initializing...", 0, 104, true);

                var sourceDocName = fileRecord.Name;
                var pathTags = new FileActionManagerPathTags(tagManager, sourceDocName);

                // Divide file into output files
                // Intermediate results must be evaluated fully in order to report progress accurately
                // but write intermediate results to a temp file to avoid memory issues
                // (the batch text files are very large)
                progressStatus?.StartNextItemGroup("Parsing batch file...", 1);
                var lines = GetLinesWithProgressReporting(fileRecord.Name, progressStatus?.SubProgressStatus);
                using (var tempFile = new TemporaryFile(true))
                using (var temporaryFileStream = new FileStream(tempFile.FileName, FileMode.Create))
                {
                    int outputFileCount = 0;
                    foreach (var outputFile in
                        RichTextFormatBatchProcessor.DivideBatch(lines, OutputDirectory, pathTags)
                        .OfType<OutputFileData>())
                    {
                        outputFileCount++;
                        outputFile.ToProtobuf(temporaryFileStream);
                    }
                    temporaryFileStream.Flush();
                    temporaryFileStream.Position = 0;

                    progressStatus?.StartNextItemGroup("Processing output files...", 100);

                    IEnumerable<OutputFileData> outputFiles =
                        Enumerable.Range(0, outputFileCount)
                        .Select(_ => (OutputFileData)BatchFileItem.FromProtobuf(temporaryFileStream));

                    // Setup enumerable to create and add the output files to the DB
                    var thunk = outputFiles
                        .Select(outputFile => CreateOutputFile(outputFile, fileProcessingDB, fileRecord.Priority, fileRecord.WorkflowID))
                        .Select((outputFile, i) => WriteToPaginationTable(outputFile, sourceDocName, i + 1, fileProcessingDB, fileTaskSessionID));

                    // Optionally queue them to the specified action
                    if (!string.IsNullOrWhiteSpace(OutputAction))
                    {
                        thunk = thunk.Select(outputFile =>
                            QueueOutputFile(outputFile, fileProcessingDB, OutputAction, fileRecord.WorkflowID));
                    }

                    // Iterate the lazy enumeration to resolve all side-effects
                    IterateAndReportProgress(thunk, outputFileCount, "Processing output file", progressStatus?.SubProgressStatus);
                }

                if (!string.IsNullOrWhiteSpace(SourceAction))
                {
                    fileProcessingDB.SetStatusForFile(
                        fileRecord.FileID,
                        SourceAction,
                        fileRecord.WorkflowID,
                        EActionStatus.kActionPending,
                        vbQueueChangeIfProcessing: true,
                        vbAllowQueuedStatusOverride: false,
                        poldStatus: out EActionStatus _);
                }

                progressStatus?.CompleteCurrentItemGroup();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48409");
            }
        }

        // Update batch file with redacted files
        void UpdateBatch(FileRecord fileRecord,
                         FAMTagManager tagManager,
                         FileProcessingDB fileProcessingDB,
                         ProgressStatus progressStatus)
        {
            // If output files are required to be complete for an action make sure they are
            // If not, either skip or fail this file (fail if there is an output file that is failed)
            if (!string.IsNullOrWhiteSpace(RedactedAction))
            {
                var outputFileStatuses = GetStatusesForEveryOutputFile(fileRecord, fileProcessingDB);
                if (!outputFileStatuses.All(c => c == 'C'))
                {
                    var newStatus = outputFileStatuses.Contains('F') ? EActionStatus.kActionFailed : EActionStatus.kActionSkipped;

                    if (newStatus == EActionStatus.kActionFailed)
                    {
                        new ExtractException("ELI48412", UtilityMethods.FormatCurrent($"At least one output file is 'Failed' for action {RedactedAction}"))
                            .Log();
                    }
                    else
                    {
                        new ExtractException("ELI48413", UtilityMethods.FormatCurrent($"At least one output file is not 'Complete' for action {RedactedAction}"))
                            .Log();
                    }

                    string currentActionName = fileProcessingDB.GetActionName(fileRecord.ActionID);
                    fileProcessingDB.SetStatusForFile(
                        fileRecord.FileID,
                        currentActionName,
                        fileRecord.WorkflowID,
                        newStatus,
                        vbQueueChangeIfProcessing: true,
                        vbAllowQueuedStatusOverride: false,
                        poldStatus: out EActionStatus _);

                    return;
                }
            }

            try
            {
                progressStatus?.InitProgressStatus("Initializing...", 0, 2, true);

                var sourceDocName = fileRecord.Name;
                var pathTags = new FileActionManagerPathTags(tagManager, sourceDocName);

                // Divide file into output files
                // Intermediate results must be evaluated fully in order to report progress accurately
                // but write intermediate results to a temp file to avoid memory issues
                // (the batch text files are very large)
                progressStatus?.StartNextItemGroup("Parsing batch file...", 1);
                var lines = GetLinesWithProgressReporting(fileRecord.Name, progressStatus?.SubProgressStatus);
                using (var tempFile = new TemporaryFile(true))
                using (var temporaryFileStream = new FileStream(tempFile.FileName, FileMode.Create))
                {
                    int outputFileCount = 0;
                    int batchItemCount = 0;
                    foreach (var batchItem in
                        RichTextFormatBatchProcessor.DivideBatch(lines, OutputDirectory, pathTags))
                    {
                        batchItemCount++;
                        if (batchItem is OutputFileData)
                        {
                            outputFileCount++;
                        }
                        batchItem.ToProtobuf(temporaryFileStream);
                    }
                    temporaryFileStream.Flush();
                    temporaryFileStream.Position = 0;

                    // Write the batch back with any updated info
                    progressStatus?.StartNextItemGroup("Updating batch file...", 1);

                    IEnumerable<BatchFileItem> batchItems =
                        Enumerable.Range(0, batchItemCount)
                        .Select(_ => BatchFileItem.FromProtobuf(temporaryFileStream));

                    pathTags.AddTag(RichTextFormatBatchProcessor.SubBatchNumber, ":not_a_valid_tag:"); // in case someone tries to use this tag in the wrong place...
                    var redactedBatchName = Path.GetFullPath(pathTags.Expand(UpdatedBatchFile));
                    Directory.CreateDirectory(Path.GetDirectoryName(redactedBatchName));
                    using (var outputStream = new FileStream(redactedBatchName, FileMode.Create))
                    {
                        var thunk = batchItems.Select(batchItem =>
                        {
                            if (batchItem is BetweenFileData pass)
                            {
                                pass.ToDavidsonFormat(outputStream);
                            }
                            else if (batchItem is OutputFileData outputFile)
                            {
                                string ext = outputFile.FileType == OutputFileType.RichTextFile ? ".rtf" : ".txt";
                                pathTags.SourceDocName = outputFile.FileNameBase + ext;
                                string expandedRedactedPath = Path.GetFullPath(pathTags.Expand(RedactedFile));
                                if (File.Exists(expandedRedactedPath))
                                {
                                    outputFile.Contents = File.ReadAllText(expandedRedactedPath, _encoding);
                                }
                                outputFile.ToDavidsonFormat(outputStream);
                            }
                            return batchItem;
                        })
                        .Where(batchItem => batchItem is OutputFileData); // Only count the output files for progress status

                        IterateAndReportProgress(thunk, outputFileCount, "Updating batch with output file", progressStatus?.SubProgressStatus);
                        outputStream.Flush();
                    }
                }

                progressStatus?.CompleteCurrentItemGroup();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48410");
            }
        }

        IEnumerable<char> GetStatusesForEveryOutputFile(FileRecord sourceFileRecord, FileProcessingDB fileProcessingDB)
        {
            string safeRedactAction = RedactedAction.Replace("'", "''");
            string query = null;
            bool isThisDBComplicatedWithWorkflows = sourceFileRecord.WorkflowID > 0;
            if (isThisDBComplicatedWithWorkflows)
            {
                query = UtilityMethods.FormatInvariant($@"
                    SELECT DISTINCT COALESCE(ActionStatus, 'U') FROM
                    WorkflowFile JOIN Pagination ON WorkflowFile.FileID = Pagination.DestFileID
                    LEFT JOIN
                    (
                        SELECT ActionStatus, FileID FROM FileActionStatus
                        LEFT JOIN Action ON Action.ID = FileActionStatus.ActionID
                        WHERE ASCName = '{safeRedactAction}'
                        AND WorkflowID = {sourceFileRecord.WorkflowID}
                    ) AS RedactedStatus ON WorkflowFile.FileID = RedactedStatus.FileID
                     WHERE SourceFileID = {sourceFileRecord.FileID}");
            }
            else
            {
                query = UtilityMethods.FormatInvariant($@"
                    SELECT DISTINCT COALESCE(ActionStatus, 'U') FROM
                    FAMFile JOIN Pagination ON FAMFile.ID = Pagination.DestFileID
                    LEFT JOIN
                    (
                        SELECT ActionStatus, FileID FROM FileActionStatus
                        LEFT JOIN Action ON Action.ID = FileActionStatus.ActionID
                        WHERE ASCName = '{safeRedactAction}'
                    ) AS RedactedStatus ON FAMFile.ID = RedactedStatus.FileID
                     WHERE SourceFileID = {sourceFileRecord.FileID}");
            }

            var recordset = fileProcessingDB.GetResultsForQuery(query);

            try
            {
                if (recordset.EOF || recordset.BOF)
                {
                    return Enumerable.Empty<char>();
                }

                var result = new HashSet<char>();
                while (!recordset.EOF)
                {
                    result.Add(((string)recordset.Fields[0].Value)[0]);
                    recordset.MoveNext();
                }

                return result;
            }
            finally
            {
                recordset.Close();
            }
        }

        #endregion Private Members
    }
}