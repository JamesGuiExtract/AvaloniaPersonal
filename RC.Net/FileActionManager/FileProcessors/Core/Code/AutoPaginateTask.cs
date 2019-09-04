using Extract.AttributeFinder;
using Extract.DataEntry;
using Extract.FileActionManager.Conditions;
using Extract.Imaging;
using Extract.Imaging.Utilities;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a file processing task that allows for automatic application of paginated data
    /// for parts or all of a source document based on configured requirements for automatic output
    /// of documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("BE78B1CC-3889-4F57-B264-58A251D46B53")]
    [CLSCompliant(false)]
    public interface IAutoPaginateTask : ICategorizedComponent, IConfigurableObject, IMustBeConfiguredObject,
         ICopyableObject, IFileProcessingTask, ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets the action (if any) into which paginated sources should be moved to pending
        /// in the case this document was able to be fully paginated automatically.
        /// </summary>
        string SourceActionIfFullyPaginated
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the action (if any) into which paginated sources should be moved to pending
        /// in the case this document was not able to be fully paginated automatically.
        /// </summary>
        string SourceActionIfNotFullyPaginated
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
        /// Gets or set the filename assembly defining an IPaginationDocumentDataPanel used to format
        /// and validate the document data. Typically this should be them DEP solution used by users
        /// for verification.
        /// </summary>
        string DocumentDataPanelAssembly
        {
            get;
            set;
        }

        /// <summary>
        /// An condition that, when it evaluates to <c>true</c>, will automatically create output an
        /// output document proposed by the rules. If not specified, all proposed output documents
        /// will be automatically generated regardless of their data.
        /// </summary>
        IPaginationCondition AutoPaginateQualifier
        {
            get;
            set;
        }

        /// <summary>
        /// A FAM tag to be applied to every output document created automatically by this instance.
        /// (optional)
        /// </summary>
        string AutoPaginatedTag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether pages should automatically be oriented to match
        /// the orientation of the text (per OCR).
        /// </summary>
        bool AutoRotatePages
        {
            get;
            set;
        }
    }

    /// <summary>
    /// A file processing task that allows for automatic application of paginated data for parts or
    /// all of a source document based on configured requirements for automatic output of documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("8ECBCC95-7371-459F-8A84-A2AFF7769800")]
    [ProgId("Extract.FileActionManager.AutoPaginateTask")]
    [CLSCompliant(false)]
    public class AutoPaginateTask : IAutoPaginateTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Pagination: Auto-Paginate";

        /// <summary>
        /// Current task version.
        /// Versions:
        /// 1. Initial version
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.PaginationUIObject;

        /// <summary>
        /// The default path for expected pagination attributes file
        /// </summary>
        const string _DEFAULT_EXPECTED_OUTPUT_PATH = "<SourceDocName>.pagination.evoa";

        /// <summary>
        /// A string representation of the GUID of the data entry verification task.
        /// </summary>
        static readonly string _AUTO_PAGINATE_TASK_GUID = typeof(AutoPaginateTask).GUID.ToString("B");

        #endregion Constants

        #region Fields

        /// <summary>
        /// Gets or sets the action (if any) into which paginated sources should be moved to pending
        /// in the case this document was able to be fully paginated automatically.
        /// </summary>
        string _sourceActionIfFullyPaginated;

        /// <summary>
        /// Gets or sets the action (if any) into which paginated sources should be moved to pending
        /// in the case this document was not able to be fully paginated automatically.
        /// </summary>
        string _sourceActionIfNotFullyPaginated;

        /// <summary>
        /// The path tag expression defining the filename that should be given to a pagination
        /// output document.
        /// </summary>
        string _outputPath = "$InsertBeforeExtension(<SourceDocName>,_Auto_Paginated_<SubDocIndex>)";

        /// <summary>
        /// The action into which paginated output documents should be moved to pending.
        /// </summary>
        string _outputAction;

        /// <summary>
        /// Gets or set the filename assembly defining an IPaginationDocumentDataPanel used to format
        /// and validate the document data. Typically this should be them DEP solution used by users
        /// for verification.
        /// </summary>
        string _documentDataPanelAssembly;

        /// <summary>
        /// An condition that, when it evaluates to <c>true</c>, will automatically create output an
        /// output document proposed by the rules. If not specified, all proposed output documents
        /// will be automatically generated regardless of their data.
        /// </summary>
        IPaginationCondition _autoPaginateQualifier = new PaginationDataValidityCondition();

        /// <summary>
        /// A FAM tag to be applied to every output document created automatically by this instance.
        /// (optional)
        /// </summary>
        string _autoPaginatedTag;

        /// <summary>
        /// Indicates whether pages should automatically be oriented to match the orientation of the
        /// text (per OCR).
        /// </summary>
        bool _autoRotatePages = true;

        /// <summary>
        /// The name of the action currently being processed.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the action being processed.
        /// </summary>
        int _actionID;

        /// <summary>
        /// Utility methods to generalte new paginated output files and record them into in the FAM database.
        /// </summary>
        PaginatedOutputCreationUtility _paginatedOutputCreationUtility;

        /// <summary>
        /// A data panel instance used to be able to format and validate data for proposed output documents.
        /// UI is used to be able to maintain support for DEPs with and without NoUILoad support.
        /// </summary>
        IPaginationDocumentDataPanel _depPanel;

        /// <summary>
        /// Used to expand path tag expressions.
        /// </summary>
        ITagUtility _tagUtility;

        /// <summary>
        /// Indicates whether this task object is dirty or not
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Synchronizes calls to ProcessFile to guarantee consistency of AttributeStatusInfo data.
        /// </summary>
        static object _lock = new object();

        /// <summary>
        /// For converting attribute to/from stringized bytestreams
        /// </summary>
        static ThreadLocal<MiscUtils> _miscUtils = new ThreadLocal<MiscUtils>(() => new MiscUtils());

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPaginateTask"/> class.
        /// </summary>
        public AutoPaginateTask()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47016");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPaginateTask"/> class.
        /// </summary>
        public AutoPaginateTask(AutoPaginateTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47017");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the action (if any) into which paginated sources should be moved to pending
        /// in the case this document was able to be fully paginated automatically.
        /// </summary>
        public string SourceActionIfFullyPaginated
        {
            get
            {
                return _sourceActionIfFullyPaginated;
            }

            set
            {
                try
                {
                    if (value != _sourceActionIfFullyPaginated)
                    {
                        _sourceActionIfFullyPaginated = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47018");
                }
            }
        }

        /// <summary>
        /// Gets or sets the action (if any) into which paginated sources should be moved to pending
        /// in the case this document was not able to be fully paginated automatically.
        /// </summary>
        public string SourceActionIfNotFullyPaginated
        {
            get
            {
                return _sourceActionIfNotFullyPaginated;
            }

            set
            {
                try
                {
                    if (value != _sourceActionIfNotFullyPaginated)
                    {
                        _sourceActionIfNotFullyPaginated = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47019");
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
                    throw ex.AsExtract("ELI47020");
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
                    throw ex.AsExtract("ELI47021");
                }
            }
        }

        /// <summary>
        /// Gets or set the filename assembly defining an IPaginationDocumentDataPanel used to format
        /// and validate the document data. Typically this should be them DEP solution used by users
        /// for verification.
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
                    throw ex.AsExtract("ELI47022");
                }
            }
        }

        /// <summary>
        /// An condition that, when it evaluates to <c>true</c>, will automatically create output an
        /// output document proposed by the rules. If not specified, all proposed output documents
        /// will be automatically generated regardless of their data.
        /// </summary>
        public IPaginationCondition AutoPaginateQualifier
        {
            get
            {
                return _autoPaginateQualifier;
            }

            set
            {
                try
                {
                    if (value != _autoPaginateQualifier)
                    {
                        _autoPaginateQualifier = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47023");
                }
            }
        }


        /// <summary>
        /// A FAM tag to be applied to every output document created automatically by this instance.
        /// (optional)
        /// </summary>
        public string AutoPaginatedTag
        {
            get
            {
                return _autoPaginatedTag;
            }

            set
            {
                try
                {
                    if (value != _autoPaginatedTag)
                    {
                        _autoPaginatedTag = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47024");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether pages should automatically be oriented to match
        /// the orientation of the text (per OCR).
        /// </summary>
        public bool AutoRotatePages
        {
            get
            {
                return _autoRotatePages;
            }

            set
            {
                if (value != _autoRotatePages)
                {
                    _autoRotatePages = value;
                    _dirty = true;
                }
            }
        }

        #endregion Properties

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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI47025", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (AutoPaginateTask)Clone();
                FileProcessingDB fileProcessingDB = new FileProcessingDB();
                fileProcessingDB.ConnectLastUsedDBThisProcess();
                using (var dialog = new AutoPaginateTaskSettingsDialog(cloneOfThis, fileProcessingDB))
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
                throw ExtractException.CreateComVisible("ELI47026",
                    "Error configuring" + _COMPONENT_DESCRIPTION + ".", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks whether this object has been configured properly.
        /// </summary>
        /// <returns><see langword="true"/> if the object has been configured properly
        /// and <see langword="false"/> if it has not.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(OutputPath)
                    || string.IsNullOrWhiteSpace(SourceActionIfNotFullyPaginated)
                    || string.IsNullOrWhiteSpace(OutputAction))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI47038",
                    "Failed while checking configuration.", ex);
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="AutoPaginateTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="AutoPaginateTask"/> instance.</returns>
        public object Clone()
        {
            return new AutoPaginateTask(this);
        }

        /// <summary>
        /// Copies the specified <see cref="AutoPaginateTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            CopyFrom((AutoPaginateTask)pObject);
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
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                if (_depPanel != null)
                {
                    _depPanel.PanelControl.Invoke((MethodInvoker)(() =>
                    {
                        // Disposing the owning form of the _depPanel will end the thread.
                        var owningForm = _depPanel.PanelControl.TopLevelControl;
                        owningForm.Dispose();
                        _depPanel = null;
                    }));
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47171", "Error closing auto-pagination task.");
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
            return true;
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI47027", _COMPONENT_DESCRIPTION);

                UnlockLeadtools.UnlockLeadToolsSupport();

                _fileProcessingDB = pDB;
                _actionID = nActionID;

                if (pFAMTM == null)
                {
                    // A FAMTagManager without path tags is better than no tag manager (still can
                    // be used to expand path functions).
                    pFAMTM = new FAMTagManager();
                }

                _tagUtility = (ITagUtility)pFAMTM;

                _paginatedOutputCreationUtility = new PaginatedOutputCreationUtility(OutputPath, pDB, _actionID);

                if (!string.IsNullOrWhiteSpace(DocumentDataPanelAssembly))
                {
                    var expandedAssemblyFileName = _tagUtility.ExpandTagsAndFunctions(
                        DocumentDataPanelAssembly, null, null);

                    _depPanel = CreateDEPContainerThread(expandedAssemblyFileName, _tagUtility, MinStackSize);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI47028",
                    "Error initializing auto-paginate paginate task.", ex);
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI47029", _COMPONENT_DESCRIPTION);

                // https://extract.atlassian.net/browse/ISSUE-15205
                // Assign any alternate component data directory root defined in the database
                // to be used in addition to the default component data directory.
                if (pDB != null)
                {
                    DataEntryMethods.AlternateComponentDataDir =
                        pDB.GetDBInfoSetting("AlternateComponentDataDir", false);
                }

                int fileTaskSessionID = _fileProcessingDB.StartFileTaskSession(
                    _AUTO_PAGINATE_TASK_GUID, pFileRecord.FileID, _actionID);
                DateTime sessionStartTime = DateTime.Now;

                bool fullyPaginated = false;
                if (_depPanel == null)
                {
                    fullyPaginated = ProcessFile(pFileRecord, pDB, pFAMTM, fileTaskSessionID);
                }
                else
                {
                    // It is hard to be sure AttributeStatusInfo and data query data will be thread safe.
                    // The invoke call should serialize anyway, but to be sure, use _lock.
                    lock (_lock)
                    {
                        _depPanel.PanelControl.Invoke((MethodInvoker)(() =>
                        {
                            fullyPaginated = ProcessFile(pFileRecord, pDB, pFAMTM, fileTaskSessionID);
                        }));
                    }
                }

                if (fullyPaginated && !string.IsNullOrWhiteSpace(SourceActionIfFullyPaginated))
                {
                    pDB.SetStatusForFile(
                        pFileRecord.FileID,
                        SourceActionIfFullyPaginated,
                        pFileRecord.WorkflowID,
                        EActionStatus.kActionPending,
                        vbQueueChangeIfProcessing: true,
                        vbAllowQueuedStatusOverride: false,
                        poldStatus: out EActionStatus oldStatus);
                }
                else if (!fullyPaginated && !string.IsNullOrWhiteSpace(SourceActionIfNotFullyPaginated))
                {
                    pDB.SetStatusForFile(
                        pFileRecord.FileID,
                        SourceActionIfNotFullyPaginated,
                        pFileRecord.WorkflowID,
                        EActionStatus.kActionPending,
                        vbQueueChangeIfProcessing: true,
                        vbAllowQueuedStatusOverride: false,
                        poldStatus: out EActionStatus oldStatus);
                }

                var sessionSeconds = (DateTime.Now - sessionStartTime).TotalSeconds;
                _fileProcessingDB.UpdateFileTaskSession(fileTaskSessionID, sessionSeconds, 0, 0);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI47030", "Unable to auto-paginate file", ex);
            }
        }

        bool ProcessFile(FileRecord pFileRecord, FileProcessingDB fileProcessingDB, FAMTagManager pFAMTM, int fileTaskSessionID)
        {
            string fileName = pFileRecord.Name;
            var voaData = new IUnknownVector();
            string dataFilename = fileName + ".voa";
            voaData.LoadFrom(dataFilename, false);
            voaData.UpdateSourceDocNameOfAttributes(fileName);
            voaData.ReportMemoryUsage();

            var attributeArray = voaData
                .ToIEnumerable<IAttribute>()
                .ToArray();
            var rootAttributeNames = new HashSet<string>(
                attributeArray.Select(attribute => attribute.Name),
                StringComparer.OrdinalIgnoreCase);

            bool fullyPaginated = false;

            // If only "Document" attributes exist at the root of the VOA file is there rules-suggested pagination.
            if (rootAttributeNames.Count == 1 &&
                rootAttributeNames.Contains("Document"))
            {
                fullyPaginated = true;

                // Map each document's DocumentData attribute to a DataEntryPaginationDocumentData
                // instance for that data.
                var docDataAttributes = attributeArray
                    .Select(a => a.SubAttributes
                        .ToIEnumerable<IAttribute>()
                        .SingleOrDefault(child => child.Name.Equals(
                            "DocumentData", StringComparison.OrdinalIgnoreCase)))
                        .Where(a => a != null);
                var docDataDictionary = docDataAttributes
                    .ToDictionary(docDataAttribute => docDataAttribute,
                        docDataAttribute => new DataEntryPaginationDocumentData(
                            docDataAttribute.SubAttributes, fileName));

                // If a DEP configuration has been specified, use to format the data and check the validity of it.
                if (_depPanel != null)
                {
                    foreach (var docData in docDataDictionary.Values)
                    {
                        _depPanel.StartUpdateDocumentStatus(docData, statusOnly: false, applyUpdateToUI: false, displayValidationErrors: false);
                    }

                    _depPanel.WaitForDocumentStatusUpdates();
                }

                // Process each candidate pagination output document.
                foreach (var docAttribute in attributeArray)
                {
                    // https://extract.atlassian.net/browse/ISSUE-16592
                    // Do not process any candidate document that was previously generated (either by
                    // auto -pagination or by the create paginated output task)
                    var previousPaginationRequest = AttributeMethods.GetSingleAttributeByName(
                        docAttribute.SubAttributes, "PaginationRequest");
                    if (previousPaginationRequest != null)
                    {
                        continue;
                    }

                    List<PageInfo> sourcePageInfos = GetSourcePageInfos(pFileRecord, docAttribute);
                    var outputData = GetUpdatedDocumentData(pFileRecord, docDataDictionary, docAttribute, sourcePageInfos);

                    // Check to see if the document qualifies to be output automatically.
                    if (AutoPaginateQualifier != null)
                    {
                        string proposedDocumentName = _paginatedOutputCreationUtility.GetPaginatedDocumentFileName(sourcePageInfos, pFAMTM);
                        var documentStatusJson = outputData.PendingDocumentStatus?.ToJson();
                        var serializedAttributes = _miscUtils.Value.GetObjectAsStringizedByteStream(docAttribute.SubAttributes);

                        if (!AutoPaginateQualifier.FileMatchesPaginationCondition(pFileRecord, proposedDocumentName,
                            documentStatusJson, serializedAttributes, fileProcessingDB, _actionID, pFAMTM))
                        {
                            fullyPaginated = false;
                            continue;
                        }
                    }

                    // https://extract.atlassian.net/browse/ISSUE-16644
                    // Don't skip output because all pages are deleted until the AutoPaginateQualifier
                    // has been executed so that such documents can be funneled to verification if the
                    // condition is not met.
                    if (sourcePageInfos.All(page => page.Deleted))
                    {
                        // Write pagination history to record the deleted pages from the source document.
                        _paginatedOutputCreationUtility.WritePaginationHistory(
                            sourcePageInfos, -1, fileTaskSessionID);
                    }
                    else
                    {
                        // The document qualifies to be output; create it.
                        CreatePaginatedOutput(docAttribute, sourcePageInfos, outputData, pFileRecord, fileProcessingDB, pFAMTM, fileTaskSessionID);
                    }
                }
            }

            attributeArray
                .ToIUnknownVector<IAttribute>()
                .SaveAttributes(dataFilename);

            return fullyPaginated;
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
                    SourceActionIfFullyPaginated = reader.ReadString();
                    SourceActionIfNotFullyPaginated = reader.ReadString();
                    OutputPath = reader.ReadString();
                    OutputAction = reader.ReadString();
                    DocumentDataPanelAssembly = reader.ReadString();
                    bool hasQualifier = reader.ReadBoolean();
                    if (hasQualifier)
                    {
                        AutoPaginateQualifier = reader.ReadIPersistStream() as IPaginationCondition;
                        AutoPaginateQualifier.IsPaginationCondition = true;
                    }
                    else
                    {
                        AutoPaginateQualifier = null;
                    }
                    AutoPaginatedTag = reader.ReadString();
                    AutoRotatePages = reader.ReadBoolean();
                }

                // Freshly loaded object is not dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI47031",
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
                    writer.Write(SourceActionIfFullyPaginated);
                    writer.Write(SourceActionIfNotFullyPaginated);
                    writer.Write(OutputPath);
                    writer.Write(OutputAction);
                    writer.Write(DocumentDataPanelAssembly);
                    bool hasQualifier = (AutoPaginateQualifier != null);
                    writer.Write(hasQualifier);
                    if (hasQualifier)
                    {
                        writer.Write((IPersistStream)AutoPaginateQualifier, clearDirty);
                    }
                    writer.Write(AutoPaginatedTag);
                    writer.Write(AutoRotatePages);

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
		        throw ExtractException.CreateComVisible("ELI47032",
                    "Unable to save auto-paginate task.", ex);
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
        /// Copies the specified <see cref="AutoPaginateTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="AutoPaginateTask"/> from which to copy.</param>
        void CopyFrom(AutoPaginateTask task)
        {
            SourceActionIfFullyPaginated = task.SourceActionIfFullyPaginated;
            SourceActionIfNotFullyPaginated = task.SourceActionIfNotFullyPaginated;
            OutputPath = task.OutputPath;
            OutputAction = task.OutputAction;
            DocumentDataPanelAssembly = task.DocumentDataPanelAssembly;
            AutoPaginateQualifier = (task.AutoPaginateQualifier == null)
                ? null
                : (IPaginationCondition)((ICopyableObject)task.AutoPaginateQualifier).Clone();
            AutoPaginatedTag = task.AutoPaginatedTag;
            AutoRotatePages = task.AutoRotatePages;

            _dirty = true;
        }

        /// <summary>
        /// Create a list of <see cref="PageInfo"/>s representing the <paramref name="docAttribute"/>.
        /// </summary>
        static List<PageInfo> GetSourcePageInfos(FileRecord pFileRecord, IAttribute docAttribute)
        {
            IEnumerable<int> getPages(string attributeName)
            {
                var pagesString = docAttribute.SubAttributes
                    .ToIEnumerable<IAttribute>()
                    .Where(attribute => attribute.Name.Equals(
                        attributeName, StringComparison.OrdinalIgnoreCase))
                    .Select(attribute => attribute.Value.String)
                    .SingleOrDefault() ?? "";
                return UtilityMethods.GetPageNumbersFromString(pagesString, -1, false);
            }

            var pages = getPages("Pages");
            var deletedPages = new HashSet<int>(getPages("DeletedPages"));

            // Ensure deleted pages are represented
            pages = pages.Union(deletedPages);

            // Don't sort pages
            // https://extract.atlassian.net/browse/ISSUE-16578
            var sourcePageInfos = pages
                .Select(p => new PageInfo
                {
                    DocumentName = pFileRecord.Name,
                    Page = p,
                    Deleted = deletedPages.Contains(p),
                    Orientation = 0
                })
                .ToList();
            return sourcePageInfos;
        }

        /// <summary>
        /// Gets a <see cref="DataEntryPaginationDocumentData"/> instance for the specified <paramref name="docAttribute"/>
        /// by looking it up from <paramref name="docDataDictionary"/> or creating one if not contained in the dictionary.
        /// </summary>
        /// <param name="pFileRecord"></param>
        /// <param name="docDataDictionary"></param>
        /// <param name="docAttribute"></param>
        /// <param name="sourcePageInfos"></param>
        /// <returns></returns>
        static DataEntryPaginationDocumentData GetUpdatedDocumentData(FileRecord pFileRecord,
            Dictionary<IAttribute, DataEntryPaginationDocumentData> docDataDictionary,
            IAttribute docAttribute, List<PageInfo> sourcePageInfos)
        {
            var docData = sourcePageInfos.All(page => page.Deleted)
                ? null
                : docAttribute.SubAttributes
                    .ToIEnumerable<IAttribute>()
                    .SingleOrDefault(child => child.Name.Equals(
                        "DocumentData", StringComparison.OrdinalIgnoreCase));

            var outputData = (docData == null)
                ? new DataEntryPaginationDocumentData(new AttributeClass(), pFileRecord.Name)
                : docDataDictionary[docData];

            if (outputData.PendingDocumentStatus != null)
            {
                if (outputData.PendingDocumentStatus.Exception != null)
                {
                    throw outputData.PendingDocumentStatus.Exception;
                }

                docData.SubAttributes.Clear();
                var updatedAttributes = (IUnknownVector)_miscUtils.Value.GetObjectFromStringizedByteStream(
                    outputData.PendingDocumentStatus.StringizedData);
                docData.SubAttributes.Append(updatedAttributes);
            }

            return outputData;
        }

        /// <summary>
        /// Reorients the pages in the document per the predominant text orientation provided from OCR. 
        /// </summary>
        static void AutoRotateFilePages(string sourceDocName, List<PageInfo> sourcePageInfos)
        {
            var spatialPageInfos = ImageMethods.GetSpatialPageInfos(sourceDocName);

            if (spatialPageInfos != null)
            {
                sourcePageInfos.ForEach(pageInfo =>
                {
                    var orientation = ImageMethods.GetPageRotation(spatialPageInfos, pageInfo.Page);
                    if (orientation != null)
                    {
                        pageInfo.Orientation = orientation.Value;
                    }
                });
            }
        }

        /// <summary>
        /// Generates a new paginated output document for the specified docAttribute.
        /// </summary>
        void CreatePaginatedOutput(IAttribute docAttribute, List<PageInfo> sourcePageInfos, DataEntryPaginationDocumentData outputData,
            FileRecord pFileRecord, FileProcessingDB fileProcessingDB, FAMTagManager pFAMTM, int fileTaskSessionID)
        {
            // Add the file to the DB and check it out for this process before actually writing
            // it to outputPath to prevent a running file supplier from grabbing it and another
            // process from getting it.
            var newFileInfo = _paginatedOutputCreationUtility.AddFileWithNameConflictResolve(
                sourcePageInfos, pFAMTM, fileTaskSessionID);

            var nonDeletedPageInfos = sourcePageInfos.Where(pageInfo => !pageInfo.Deleted).ToList();

            if (AutoRotatePages)
            {
                AutoRotateFilePages(pFileRecord.Name, nonDeletedPageInfos);
            }

            var nonDeletedImagePages = nonDeletedPageInfos
                .Select(p => p.ImagePage)
                .ToList();

            ImageMethods.StaplePagesAsNewDocument(nonDeletedImagePages, newFileInfo.FileName);

            AttributeMethods.CreateUssAndVoaForPaginatedDocument(
                newFileInfo.FileName, outputData.Attributes, nonDeletedImagePages);

            _paginatedOutputCreationUtility.LinkFilesWithRecordIds(newFileInfo.FileID,
                outputData.PendingDocumentStatus?.Orders, outputData.PendingDocumentStatus?.Encounters);

            _fileProcessingDB.SetStatusForFile(
                newFileInfo.FileID,
                OutputAction,
                pFileRecord.WorkflowID,
                EActionStatus.kActionPending,
                vbQueueChangeIfProcessing: true,
                vbAllowQueuedStatusOverride: false,
                poldStatus: out EActionStatus oldStatus);

            if (!string.IsNullOrWhiteSpace(AutoPaginatedTag))
            {
                fileProcessingDB.TagFile(newFileInfo.FileID, AutoPaginatedTag);
            }

            var paginationRequest = new PaginationRequest(
                PaginationRequestType.Automatic,
                fileTaskSessionID, 
                newFileInfo.FileID,
                sourcePageInfos.Select(p => p.ImagePage));
            docAttribute.SubAttributes.PushBack(paginationRequest.GetAsAttribute());
        }

        /// <summary>
        /// Creates and STA thread to host the <see cref="_depPanel"/> to be used to format and validate all
        /// prposed output data.
        /// </summary>
        IPaginationDocumentDataPanel CreateDEPContainerThread(string documentDataPanelAssembly, ITagUtility tagUtility, uint minStackSize)
        {
            var createdEvent = new ManualResetEvent(false);
            IPaginationDocumentDataPanel panel = null;
            Form form = new Form();
            ExtractException ee = null;

            Thread thread = new Thread(() =>
            {
                try
                {
                    panel = CreateDocumentDataPanel(documentDataPanelAssembly, tagUtility);
                    form = new InvisibleForm();
                    form.Controls.Add(panel.PanelControl);
                }
                catch (Exception ex)
                {
                    ee = ex.AsExtract("ELI47047");
                }
                finally
                {
                    createdEvent.Set();
                }

                Application.Run(form);
            }
            , (int)minStackSize);

            // [DataEntry:292] Some .Net control functionality such as clipboard and 
            // auto-complete depends upon the STA threading model.
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            createdEvent.WaitOne();

            if (ee != null)
            {
                throw ee;
            }

            return panel;
        }

        /// <summary>
        /// Creates the <see cref="IPaginationDocumentDataPanel"/> that should be used to edit data
        /// for documents in the pagination pane.
        /// </summary>
        /// <param name="paginationDocumentDataPanelAssembly"></param>
        /// <returns>The <see cref="IPaginationDocumentDataPanel"/> that should be used to edit data
        /// for documents in the pagination pane.</returns>
        IPaginationDocumentDataPanel CreateDocumentDataPanel(string paginationDocumentDataPanelAssembly, ITagUtility tagUtility)
        {
            if (paginationDocumentDataPanelAssembly.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
            {
                var paginationPanelContainer =
                    new DataEntryPanelContainer(
                        paginationDocumentDataPanelAssembly, new BackgroundDataEntryApp(_fileProcessingDB), tagUtility, null);

                return paginationPanelContainer;
            }
            else
            {
                // May be null if the an IPaginationDocumentDataPanel is not specified to be used in
                // this workflow.
                return UtilityMethods.CreateTypeFromAssembly<IPaginationDocumentDataPanel>(
                    paginationDocumentDataPanelAssembly);
            }
        }

        #endregion Private Members
    }
}