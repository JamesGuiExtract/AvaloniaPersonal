using Extract.FileConverter;
using Extract.FileConverter.ConvertToPdf;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// The mode that this task will run
    /// </summary>
    [ComVisible(true)]
    [Guid("E2F62C6C-F45E-4C89-BFE5-6F8DA0C4A9FE")]
    public enum ConvertEmailProcessingMode
    {
        /// <summary>
        /// Convert a MIME file into a PDF file without using intermediate files/actions
        /// </summary>
        Combo,

        /// <summary>
        /// Split an email into its pieces and populate the pagination history table
        /// </summary>
        Split
    }

    /// <summary>
    /// Interface definition for <see cref="ConvertEmailToPdfTask"/>
    /// </summary>
    [ComVisible(true)]
    [Guid("635CD529-FE8F-4B64-A7C8-A30B314EF612")]
    [CLSCompliant(false)]
    public interface IConvertEmailToPdfTask :
        ICategorizedComponent,
        IConfigurableObject,
        IMustBeConfiguredObject,
        ICopyableObject,
        IFileProcessingTask,
        ILicensedComponent,
        IPersistStream
    {
        /// <summary>
        /// Path to the folder for new files. Supports path tags
        /// </summary>
        string OutputDirectory { get; set; }

        /// <summary>
        /// Action to queue source files to after they have been processed
        /// </summary>
        string SourceAction { get; set; }

        /// <summary>
        /// Action to queue new files to
        /// </summary>
        string OutputAction { get; set; }

        /// <summary>
        /// The processing mode this instance is configured to use
        /// </summary>
        ConvertEmailProcessingMode ProcessingMode { get; set; }

        /// <summary>
        /// Path to the output file. Supports path tags
        /// </summary>
        string OutputFilePath { get; set; }

        /// <summary>
        /// Whether to change the file record in the database to point to the output file
        /// </summary>
        bool ModifySourceDocName { get; set; }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> that creates multiple files from an email file
    /// </summary>
    [ComVisible(true)]
    [Guid("66DE247E-997A-4261-B18E-B4F3C8584D9F")]
    [ProgId("Extract.FileActionManager.FileProcessors.ConvertEmailToPdfTask")]
    public class ConvertEmailToPdfTask : IConvertEmailToPdfTask
    {
        #region Constants

        // The description of this task
        const string _COMPONENT_DESCRIPTION = "Core: Convert email to PDF";

        // Current task version.
        // Version 2:
        // - Add ProcessingMode
        // - Add ModifySourceDocName
        // - Add OutputFilePath
        const int _CURRENT_VERSION = 2;

        // The license id to validate in licensing calls
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        #endregion Constants

        #region Fields

        // Indicates that settings have been changed, but not saved.
        bool _dirty;

        // Used for Combo mode processing
        MimeKitEmailToPdfConverter _emailToPdfConverter;

        // Used for Split mode processing
        MimeFileSplitter _mimeFileSplitter;

        #endregion Fields

        #region Constructors

        /// Initializes a new instance of the <see cref="ConvertEmailToPdfTask"/> class.
        public ConvertEmailToPdfTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertEmailToPdfTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="ConvertEmailToPdfTask"/> from which settings should
        /// be copied.</param>
        public ConvertEmailToPdfTask(ConvertEmailToPdfTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53137");
            }
        }

        #endregion Constructors

        #region IConvertEmailToPdfTask Members

        /// <inheritdoc/>
        public string OutputDirectory { get; set; } = "$DirOf(<SourceDocName>)";

        /// <inheritdoc/>
        public string SourceAction { get; set; }

        /// <inheritdoc/>
        public string OutputAction { get; set; }

        /// <inheritdoc/>
        public ConvertEmailProcessingMode ProcessingMode { get; set; }

        /// <inheritdoc/>
        public string OutputFilePath { get; set; } = "<SourceDocName>.pdf";

        /// <inheritdoc/>
        public bool ModifySourceDocName { get; set; } = true;

        #endregion IConvertEmailToPdfTask Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ConvertEmailToPdfTask"/>.
        /// </summary>
        /// <returns><c>true</c> if the configuration was successfully updated or
        /// <c>false</c> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI53138", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (ConvertEmailToPdfTask)Clone();

                FileProcessingDBClass fileProcessingDB = new();
                fileProcessingDB.ConnectLastUsedDBThisProcess();

                using ConvertEmailToPdfTaskSettingsDialog dialog = new(cloneOfThis, fileProcessingDB);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    CopyFrom(dialog.Settings);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53139",
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
                if (ProcessingMode == ConvertEmailProcessingMode.Split)
                {
                    if (string.IsNullOrWhiteSpace(OutputDirectory))
                    {
                        return false;
                    }
                }
                else if (ProcessingMode == ConvertEmailProcessingMode.Combo)
                {
                    if (string.IsNullOrWhiteSpace(OutputFilePath))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI53140", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ConvertEmailToPdfTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ConvertEmailToPdfTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new ConvertEmailToPdfTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53141", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ConvertEmailToPdfTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                if (pObject is not ConvertEmailToPdfTask task)
                {
                    throw new InvalidCastException("Invalid copy-from object. Requires " + nameof(ConvertEmailToPdfTask));
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53142", "Unable to copy object.", ex);
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
            // Do nothing, this task is not cancellable
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            // Nothing to do
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
                if (ProcessingMode == ConvertEmailProcessingMode.Combo)
                {
                    _emailToPdfConverter = MimeKitEmailToPdfConverter.CreateDefault();
                }
                else if (ProcessingMode == ConvertEmailProcessingMode.Split)
                {
                    DatabaseClientForMimeFileSplitter databaseClient = new(pDB, OutputAction);
                    _mimeFileSplitter = new MimeFileSplitter(databaseClient, OutputDirectory, pFAMTM ?? new FAMTagManagerClass());
                }
                else
                {
                    throw new NotImplementedException(UtilityMethods.FormatInvariant(
                        $"Unknown {nameof(ConvertEmailProcessingMode)}: {ProcessingMode}"));
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI53143",
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
        public EFileProcessingResult ProcessFile(
            FileRecord pFileRecord,
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
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI53144", _COMPONENT_DESCRIPTION);

                if (ProcessingMode == ConvertEmailProcessingMode.Split)
                {
                    _mimeFileSplitter.SplitFile(new(pFileRecord));
                }
                else if (ProcessingMode == ConvertEmailProcessingMode.Combo)
                {
                    ConvertEmailToPDF(pFileRecord, pFAMTM, pDB);
                }
                else
                {
                    throw new NotImplementedException(UtilityMethods.FormatInvariant(
                        $"Unknown {nameof(ConvertEmailProcessingMode)}: {ProcessingMode}"));
                }

                // Set the action status for the source file, if configured
                QueueFile(pDB, pFileRecord, SourceAction);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI53145", "Failed to convert email");
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
                throw ExtractException.CreateComVisible("ELI53146",
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
                using IStreamReader reader = new(stream, _CURRENT_VERSION);

                OutputDirectory = reader.ReadString();
                SourceAction = reader.ReadString();
                OutputAction = reader.ReadString();

                if (reader.Version < 2)
                {
                    ProcessingMode = ConvertEmailProcessingMode.Split;
                }
                else
                {
                    ProcessingMode = (ConvertEmailProcessingMode)reader.ReadInt32();
                    ExtractException.Assert("ELI53237", "Unknown enum value",
                        Enum.IsDefined(typeof(ConvertEmailProcessingMode), ProcessingMode));

                    OutputFilePath = reader.ReadString();
                    ModifySourceDocName = reader.ReadBoolean();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53147",
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
                using IStreamWriter writer = new(_CURRENT_VERSION);

                writer.Write(OutputDirectory);
                writer.Write(SourceAction);
                writer.Write(OutputAction);
                writer.Write((int)ProcessingMode);
                writer.Write(OutputFilePath);
                writer.Write(ModifySourceDocName);

                // Write to the provided IStream.
                writer.WriteTo(stream);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53148",
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
        /// Copies the specified <see cref="IConvertEmailToPdfTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="IConvertEmailToPdfTask"/> from which to copy.</param>
        void CopyFrom(IConvertEmailToPdfTask task)
        {
            OutputDirectory = task.OutputDirectory;
            SourceAction = task.SourceAction;
            OutputAction = task.OutputAction;
            ProcessingMode = task.ProcessingMode;
            OutputFilePath = task.OutputFilePath;
            ModifySourceDocName = task.ModifySourceDocName;

            _dirty = true;
        }

        // Split a MIME file and recombine the parts into a PDF
        void ConvertEmailToPDF(FileRecord fileRecord, FAMTagManager tagManager, FileProcessingDB pDB)
        {
            tagManager = tagManager ?? new FAMTagManagerClass();

            using TemporaryFile tempOutputFile = new(".pdf", true);
            EmailFile emailFile = new(fileRecord.Name);
            PdfFile pdfFile = new(tempOutputFile.FileName);

            if (!_emailToPdfConverter.ConvertEmail(emailFile, pdfFile, out int pageCount))
            {
                throw new ExtractException("ELI53236", "Could not convert email to PDF file");
            }

            string outputFilePath = tagManager.ExpandTagsAndFunctions(OutputFilePath, fileRecord.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            // If modifying the source doc then rename the file in the database
            if (ModifySourceDocName)
            {
                pDB.RenameFile(fileRecord, outputFilePath);

                fileRecord.FileSize = new FileInfo(pdfFile.FilePath).Length;
                fileRecord.Pages = pageCount;
                pDB.SetFileInformationForFile(fileRecord.FileID, fileRecord.FileSize, pageCount);
            }
            // Else, if an output action is configured, add the new file to the database and set to pending
            else if (!string.IsNullOrWhiteSpace(OutputAction))
            {
                long outputFileSize = new FileInfo(pdfFile.FilePath).Length;

                int fileID = pDB.AddFileNoQueue(
                    outputFilePath,
                    outputFileSize,
                    pageCount,
                    fileRecord.Priority,
                    fileRecord.WorkflowID);

                FileRecordClass outputFileRecord = new()
                {
                    FileID = fileID
                };

                // Set the action status
                QueueFile(pDB, outputFileRecord, OutputAction);
            }

            // Copy the output file to the final location
            new Retry<IOException>(100, 600).DoRetry(() =>
                File.Copy(tempOutputFile.FileName, outputFilePath, true));
        }

        // If actionName is non-empty then set the action status for the file to pending
        static void QueueFile(IFileProcessingDB fileProcessingDB, FileRecord fileRecord, string actionName)
        {
            if (!string.IsNullOrWhiteSpace(actionName))
            {
                fileProcessingDB.SetStatusForFile(
                    fileRecord.FileID,
                    actionName,
                    fileRecord.WorkflowID,
                    EActionStatus.kActionPending,
                    vbQueueChangeIfProcessing: true,
                    vbAllowQueuedStatusOverride: false,
                    poldStatus: out EActionStatus _);
            }
        }
        #endregion Private Members
    }
}
