using Extract.DataEntry;
using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a file processing task that allows pagination of documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("865834B8-A9F7-491C-B8A3-853AE889590E")]
    [CLSCompliant(false)]
    public interface IPaginationTask : ICategorizedComponent, IConfigurableObject, ICopyableObject,
        IFileProcessingTask, ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets the action (if any) into which paginated sources should be moved to pending.
        /// </summary>
        string SourceAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path tag expression defining the filename that should be given to a
        /// pagination output document.
        /// </summary>
        string OutputPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set the action into which paginated output documents should be moved to pending.
        /// </summary>
        string OutputAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set the filename assembly defining an IPaginationDocumentDataPanel to display
        /// document data.
        /// </summary>
        string DocumentDataPanelAssembly
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets whether to output expected pagination attributes.
        /// </summary>
        bool OutputExpectedPaginationAttributesFiles
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the file path to output expected pagination attributes to
        /// </summary>
        string ExpectedPaginationAttributesOutputPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the task should run in single source document mode.
        /// </summary>
        /// <value><c>true</c> if only every one source file should be loaded at once and applied at
        /// once; otherwise, <c>false</c>.
        /// </value>
        bool SingleSourceDocumentMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether pages should automatically be oriented to match
        /// the orientation of the text (per OCR).
        /// </summary>
        /// <value>
        ///   <c>true</c> if pages should automatically be oriented to match the orientation of the
        ///   text; otherwise, <c>false</c>.
        /// </value>
        bool AutoRotateImages
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether document pages for newly loaded documents should
        /// be collapsed by default.
        /// </summary>
        bool DefaultToCollapsed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the select all check box should be visible.
        /// </summary>
        bool SelectAllCheckBoxVisible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the load next document button should be visible.
        /// </summary>
        bool LoadNextDocumentVisible
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a file processing task that allows pagination of documents.
    /// </summary>
    [ComVisible(true)]
    [Guid(Constants.TaskClassPaginationVerification)]
    [ProgId("Extract.FileActionManager.PaginationTask")]
    [CLSCompliant(false)]
    public class PaginationTask : IPaginationTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Pagination: Verify";

        /// <summary>
        /// Current task version.
        /// Versions:
        /// 1. Initial version
        /// 2. Added OutputExpectedPaginationAttributesFiles and ExpectedPaginationAttributesOutputPath
        /// 3. Added SingleSourceDocumentMode
        /// 4. Added AutoRotateImages
        /// 5. Added DefaultToCollapsed
        /// 6. Added SelectAllCheckBoxVisible, LoadNextDocumentButtonVisible
        /// </summary>
        const int _CURRENT_VERSION = 6;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.PaginationUIObject;

        /// <summary>
        /// The default path for expected pagination attributes file
        /// </summary>
        const string _DEFAULT_EXPECTED_OUTPUT_PATH = "<SourceDocName>.pagination.evoa";
        
        #endregion Constants

        #region Fields

        /// <summary>
        /// The form to display for paginating files.
        /// </summary>
        static VerificationForm<PaginationTaskForm> _form;

        /// <summary>
        /// The action (if any) into which paginated sources should be moved to pending.
        /// </summary>
        string _sourceAction;

        /// <summary>
        /// The path tag expression defining the filename that should be given to a pagination
        /// output document.
        /// </summary>
        string _outputPath = "$InsertBeforeExtension(<SourceDocName>,_User_Paginated_<SubDocIndex>)";

        /// <summary>
        /// The action into which paginated output documents should be moved to pending.
        /// </summary>
        string _outputAction;

        /// <summary>
        /// The filename assembly defining an <see cref="IPaginationDocumentDataPanel"/> to display
        /// document data.
        /// </summary>
        string _documentDataPanelAssembly;

        /// <summary>
        /// The name of the action currently being processed.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the action being processed.
        /// </summary>
        int _actionID;

        /// <summary>
        /// The <see cref="FAMTagManager"/> to use to expand path tags and functions.
        /// </summary>
        FAMTagManager _tagManager;

        /// <summary>
        /// The <see cref="IFileRequestHandler"/> that can be used by the task to carry out requests
        /// for files to be checked out, released or re-ordered in the queue.
        /// </summary>
        IFileRequestHandler _fileRequestHandler;

        /// <summary>
        /// Object used to mutex around the verification form creation.
        /// </summary>
        static readonly object _lock = new object();

        /// <summary>
        /// Indicates whether this task object is dirty or not
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Gets/sets the file path to output expected pagination attributes to
        /// </summary>
        private bool _outputExpectedPaginationAttributesFiles;

        /// <summary>
        /// Gets/sets the file path to output expected pagination attributes to
        /// </summary>
        private string _expectedPaginationAttributesOutputPath = _DEFAULT_EXPECTED_OUTPUT_PATH;

        /// <summary>
        /// The single source document mode
        /// </summary>
        bool _singleSourceDocumentMode;

        /// <summary>
        /// Indicates whether pages should automatically be oriented to match the orientation of the
        /// text (per OCR).
        /// </summary>
        bool _autoRotateImages = true;

        /// <summary>
        /// Indicates whether document pages for newly loaded documents should be collapsed by default.
        /// </summary>
        bool _defaultToCollapsed = false;

        /// <summary>
        /// Indicates whether the select all check box should be visible.
        /// </summary>
        bool _selectAllCheckBoxVisible = true;

        /// <summary>
        /// Indicates whether the load next document button should be visible.
        /// </summary>
        bool _loadNextDocumentButtonVisible = true;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationTask"/> class.
        /// </summary>
        public PaginationTask()
        {
            try
            {
                // Lock around form creation
                lock (_lock)
                {
                    if (_form == null)
                    {
                        _form = new VerificationForm<PaginationTaskForm>();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40135");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationTask"/> class.
        /// </summary>
        public PaginationTask(PaginationTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40136");
            }
        }
        
        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the action (if any) into which paginated sources should be moved to pending.
        /// </summary>
        public string SourceAction
        {
            get
            {
                return _sourceAction;
            }

            set
            {
                try
                {
                    if (value != _sourceAction)
                    {
                        _sourceAction = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40118");
                }
            }
        }

        /// <summary>
        /// Gets or sets the path tag expression defining the filename that should be given to a
        /// pagination output document.
        /// </summary>
        public string OutputPath
        {
            get
            {
                return _outputPath;
            }

            set
            {
                try
                {
                    if (value != _outputPath)
                    {
                        _outputPath = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40119");
                }
            }
        }

        /// <summary>
        /// Gets or sets the action into which paginated output documents should be moved to pending.
        /// </summary>
        public string OutputAction
        {
            get
            {
                return _outputAction;
            }

            set
            {
                try
                {
                    if (value != _outputAction)
                    {
                        _outputAction = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40120");
                }
            }
        }

        /// <summary>
        /// Gets or sets the filename assembly defining an <see cref="IPaginationDocumentDataPanel"/>
        /// to display document data.
        /// </summary>
        public string DocumentDataPanelAssembly
        {
            get
            {
                return _documentDataPanelAssembly;
            }

            set
            {
                try
                {
                    if (value != _documentDataPanelAssembly)
                    {
                        _documentDataPanelAssembly = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40121");
                }
            }
        }

        /// <summary>
        /// Gets/sets whether to output expected pagination attributes to a file
        /// </summary>
        public bool OutputExpectedPaginationAttributesFiles
        {
            get
            {
                return _outputExpectedPaginationAttributesFiles;
            }
            set
            {
                if (value != _outputExpectedPaginationAttributesFiles)
                {
                    _outputExpectedPaginationAttributesFiles = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets/sets the file path to output expected pagination attributes to
        /// </summary>
        public string ExpectedPaginationAttributesOutputPath
        {
            get
            {
                return _expectedPaginationAttributesOutputPath;
            }
            set
            {
                if (value != _expectedPaginationAttributesOutputPath)
                {
                    _expectedPaginationAttributesOutputPath = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets/sets 
        /// </summary>
        public bool SingleSourceDocumentMode
        {
            get
            {
                return _singleSourceDocumentMode;
            }
            set
            {
                if (value != _singleSourceDocumentMode)
                {
                    _singleSourceDocumentMode = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether pages should automatically be oriented to match
        /// the orientation of the text (per OCR).
        /// </summary>
        /// <value>
        ///   <c>true</c> if pages should automatically be oriented to match the orientation of the
        ///   text; otherwise, <c>false</c>.
        /// </value>
        public bool AutoRotateImages
        {
            get
            {
                return _autoRotateImages;
            }

            set
            {
                if (value != _autoRotateImages)
                {
                    _autoRotateImages = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether document pages for newly loaded documents should
        /// be collapsed by default.
        /// </summary>
        public bool DefaultToCollapsed
        {
            get
            {
                return _defaultToCollapsed;
            }

            set
            {
                if (value != _defaultToCollapsed)
                {
                    _defaultToCollapsed = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the select all check box should be visible.
        /// </summary>
        public bool SelectAllCheckBoxVisible
        {
            get
            {
                return _selectAllCheckBoxVisible;
            }

            set
            {
                if (value != _selectAllCheckBoxVisible)
                {
                    _selectAllCheckBoxVisible = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the load next document button should be visible.
        /// </summary>
        public bool LoadNextDocumentVisible
        {
            get
            {
                return _loadNextDocumentButtonVisible;
            }

            set
            {
                if (value != _loadNextDocumentButtonVisible)
                {
                    _loadNextDocumentButtonVisible = value;
                    _dirty = true;
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID File Processors" COM category.
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
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="PaginationTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="PaginationTask"/> from which to copy.</param>
        void CopyFrom(PaginationTask task)
        {
            SourceAction = task.SourceAction;
            OutputPath = task.OutputPath;
            OutputAction = task.OutputAction;
            DocumentDataPanelAssembly = task.DocumentDataPanelAssembly;
            OutputExpectedPaginationAttributesFiles = task.OutputExpectedPaginationAttributesFiles;
            ExpectedPaginationAttributesOutputPath = task.ExpectedPaginationAttributesOutputPath;
            SingleSourceDocumentMode = task.SingleSourceDocumentMode;
            AutoRotateImages = task.AutoRotateImages;
            DefaultToCollapsed = task.DefaultToCollapsed;
            SelectAllCheckBoxVisible = task.SelectAllCheckBoxVisible;
            LoadNextDocumentVisible = task.LoadNextDocumentVisible;

            _dirty = true;
        }

        /// <summary>
        /// Creates a <see cref="PaginationTaskForm"/> with the current settings.
        /// </summary>
        /// <returns>A <see cref="PaginationTaskForm"/> with the current settings.</returns>
        IVerificationForm CreatePaginationTaskForm()
        {
            try
            {
                var form = new PaginationTaskForm(this, DocumentDataPanelAssembly,
                    _fileProcessingDB, _actionID, _tagManager, _fileRequestHandler);

                return form;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40137");
            }
        }

        #endregion Methods

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
        /// Performs configuration needed to create a valid <see cref="ExtractImageAreaTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI40122", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (PaginationTask)Clone();
                FileProcessingDB fileProcessingDB = new FileProcessingDB();
                fileProcessingDB.ConnectLastUsedDBThisProcess();
                using (var dialog = new PaginationTaskSettingsDialog(cloneOfThis, fileProcessingDB))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dialog.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40123",
                    "Error configuring" + _COMPONENT_DESCRIPTION + ".", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="PaginationTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="PaginationTask"/> instance.</returns>
        public object Clone()
        {
            return new PaginationTask(this);
        }

        /// <summary>
        /// Copies the specified <see cref="PaginationTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            CopyFrom((PaginationTask)pObject);
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
        /// Returns a value indicating that the task displays a UI
        /// </summary>
        public bool DisplaysUI
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI40124", _COMPONENT_DESCRIPTION);

                _form.Cancel();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40125", 
                    "Error canceling paginate files task.", ex);
            }
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI40126", _COMPONENT_DESCRIPTION);

                _form.CloseForm();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40127",
                    "Error closing paginate files task.", ex);
            }
        }

        /// <summary>
        /// Called notify to the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <see langword="true"/>. If the processor wants to cancel processing,
        ///	it should return <see langword="false"/>. If the processor does not immediately know
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
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        public bool Standby()
        {
            try
            {
                return _form.Standby();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI40128", "Error stopping paginate files task.");
            }
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
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
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI40129", _COMPONENT_DESCRIPTION);

                ExtractException.Assert("ELI43427",
                    "Pagination is not supported for <All workflows>", !pDB.RunningAllWorkflows);

                _fileProcessingDB = pDB;
                _actionID = nActionID;
                _tagManager = pFAMTM;
                _fileRequestHandler = pFileRequestHandler;

                _form.ShowForm(CreatePaginationTaskForm, 0x400000);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40130",
                    "Error initializing paginate files task.", ex);
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
        /// <param name="bCancelRequested"><see langword="true"/> if cancel was requested; 
        /// <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if processing should continue; <see langword="false"/> 
        /// if all file processing should be cancelled.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord, int nActionID,
            FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI40131", _COMPONENT_DESCRIPTION);

                if (bCancelRequested)
                {
                    return EFileProcessingResult.kProcessingCancelled;
                }

                // https://extract.atlassian.net/browse/ISSUE-15205
                // Assign any alternate component data directory root defined in the database
                // to be used in addition to the default component data directory.
                if (pDB != null)
                {
                    DataEntryMethods.AlternateComponentDataDir =
                        pDB.GetDBInfoSetting("AlternateComponentDataDir", false);
                }

                EFileProcessingResult result = _form.ShowDocument(pFileRecord.Name, pFileRecord.FileID,
                    nActionID, pFAMTM, pDB);

                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40132", "Unable to paginate files.", ex);
            }
        }

        #endregion IFileProcessingTask Members

		#region IAccessRequired Members

		/// <summary>
		/// Returns bool value indicating if the task requires admin access
		/// </summary>
		/// <returns><see langword="true"/> if the task requires admin access
		/// <see langword="false"/> if task does not require admin access</returns>
		public bool RequiresAdminAccess()
		{
			return false;
		}

		#endregion IAccessRequired Members

		#region ILicensedComponent Members

		/// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(_LICENSE_ID);
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
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    SourceAction = reader.ReadString();
                    OutputPath = reader.ReadString();
                    OutputAction = reader.ReadString();
                    DocumentDataPanelAssembly = reader.ReadString();

                    if (reader.Version >= 2)
                    {
                        OutputExpectedPaginationAttributesFiles = reader.ReadBoolean();
                        ExpectedPaginationAttributesOutputPath = reader.ReadString();
                    }

                    if (reader.Version >= 3)
                    {
                        SingleSourceDocumentMode = reader.ReadBoolean();
                    }

                    if (reader.Version >= 4)
                    {
                        AutoRotateImages = reader.ReadBoolean();
                    }

                    if (reader.Version >= 5)
                    {
                        DefaultToCollapsed = reader.ReadBoolean();
                    }

                    if (reader.Version >= 6)
                    {
                        SelectAllCheckBoxVisible = reader.ReadBoolean();
                        LoadNextDocumentVisible = reader.ReadBoolean();
                    }
                }

                // Freshly loaded object is not dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40133",
                    "Unable to load pagination files task.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            try 
	        {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Save the settings
                    writer.Write(SourceAction);
                    writer.Write(OutputPath);
                    writer.Write(OutputAction);
                    writer.Write(DocumentDataPanelAssembly);
                    writer.Write(OutputExpectedPaginationAttributesFiles);
                    writer.Write(ExpectedPaginationAttributesOutputPath);
                    writer.Write(SingleSourceDocumentMode);
                    writer.Write(AutoRotateImages);
                    writer.Write(DefaultToCollapsed);
                    writer.Write(SelectAllCheckBoxVisible);
                    writer.Write(LoadNextDocumentVisible);

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
		        throw ExtractException.CreateComVisible("ELI40134",
                    "Unable to save paginate files task.", ex);
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

        #endregion Private Members
    }
}