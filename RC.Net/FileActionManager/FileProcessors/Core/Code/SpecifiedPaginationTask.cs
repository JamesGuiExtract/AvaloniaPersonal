using DynamicData;
using Extract.AttributeFinder;
using Extract.FileActionManager.FileProcessors.Views;
using Extract.FileActionManager.FileProcessors.Models;
using Extract.Imaging;
using Extract.Imaging.Utilities;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

using static System.FormattableString;
using SettingsModel = Extract.FileActionManager.FileProcessors.Models.SpecifiedPaginationTaskSettingsModelV1;

namespace Extract.FileActionManager.FileProcessors
{
    [ComVisible(true)]
    [Guid(Constants.TaskClassSpecifiedPagination)]
    [ProgId("Extract.FileActionManager.SpecifiedPaginationTask")]
    [CLSCompliant(false)]
    public class SpecifiedPaginationTask : ICategorizedComponent, IConfigurableObject, IMustBeConfiguredObject,
         ICopyableObject, IFileProcessingTask, ILicensedComponent, IPersistStream, IDomainObject
    {
        #region Constants

        const string _COMPONENT_DESCRIPTION = "Pagination: Specified Pagination";

        /// <summary>
        /// NOTE: This version is unlikely to change. Instead, versioning will be handled by
        /// the class aliased by SettingsModel above.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        static readonly string _SPECIFIED_PAGINATION_TASK_GUID = typeof(SpecifiedPaginationTask).GUID.ToString("B");

        #endregion Constants

        #region Fields

        bool _dirty;

        readonly DataTransferObjectSerializer _serializer = new(typeof(SpecifiedPaginationTask).Assembly);

        static ThreadLocal<MiscUtils> _miscUtils = new ThreadLocal<MiscUtils>(() => new MiscUtils());

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecifiedPaginationTask"/> class.
        /// </summary>
        public SpecifiedPaginationTask()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53880");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecifiedPaginationTask"/> class.
        /// </summary>
        public SpecifiedPaginationTask(SpecifiedPaginationTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53881");
            }
        }

        #endregion Constructors

        #region Properties

        public IList<PageSourceV1> PageSources { get; } = new Collection<PageSourceV1>();

        public string OutputPath { get; set; }

        public string OutputAction { get; set; }

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
        /// Performs configuration needed to create a valid <see cref="SpecifiedPaginationTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI53882", _COMPONENT_DESCRIPTION);

                FileProcessingDB fileProcessingDB = new FileProcessingDB();
                fileProcessingDB.ConnectLastUsedDBThisProcess();
                using (var dialog = new SpecifiedPaginationTaskSettingsForm(GetSettings(), fileProcessingDB))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dialog.GetSettings());
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53883",
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
                return PageSources.Any()
                    && !string.IsNullOrWhiteSpace(OutputPath)
                    && !string.IsNullOrWhiteSpace(OutputAction);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53884",
                    "Failed to validate specified pagination configuration.", ex);
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="SpecifiedPaginationTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="SpecifiedPaginationTask"/> instance.</returns>
        public object Clone()
        {
            return new SpecifiedPaginationTask(this);
        }

        /// <summary>
        /// Copies the specified <see cref="SpecifiedPaginationTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                if (pObject is SpecifiedPaginationTask task)
                {
                    CopyFrom(task);
                }
                else if (pObject is SpecifiedPaginationTaskSettingsModelV1 v1)
                {
                    CopyFrom(v1);
                }
                else
                {
                    throw new ArgumentException(
                        Invariant($"Unknown specific pagination task source: '{nameof(pObject)}'"));
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI53940", Invariant($"Error copying {nameof(SpecifiedPaginationTask)}"));
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
        /// Returns a value indicating that the task displays a UI
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
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI53885", _COMPONENT_DESCRIPTION);

                UnlockLeadtools.UnlockLeadToolsSupport();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53886",
                    "Error initializing specified pagination task.", ex);
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI53887", _COMPONENT_DESCRIPTION);

                int fileTaskSessionID = pDB.StartFileTaskSession(
                    _SPECIFIED_PAGINATION_TASK_GUID, pFileRecord.FileID, pFileRecord.ActionID);

                string sourceDocName = pFileRecord.Name;
                (List<ImagePage> imagePages, List<PageInfo> pageInfos) = 
                    GetPageLists(pFAMTM, sourceDocName);

                string outputFileName = GetOutputFileName(pFileRecord, pDB, pFAMTM);

                PaginatedOutputCreationUtility paginatedOutputCreationUtility = new(
                    outputFileName, pDB, pFileRecord.ActionID, pFileRecord.WorkflowID);

                int fileId = -1;
                using (var tempFile = new TemporaryFile(Path.GetExtension(outputFileName), true))
                {
                    ImageMethods.StaplePagesAsNewDocument(imagePages, tempFile.FileName);
                    long fileSize = new FileInfo(tempFile.FileName).Length;

                    fileId = pDB.AddFileNoQueue(
                        outputFileName, fileSize, imagePages.Count, EFilePriority.kPriorityNormal,
                        pFileRecord.WorkflowID);

                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFileName));
                    File.Copy(tempFile.FileName, outputFileName, true);
                }

                var voaData = new IUnknownVector();
                string dataFilename = sourceDocName + ".voa";
                voaData.LoadFrom(dataFilename, false);
                AttributeMethods.CreateUssAndVoaForPaginatedDocument(outputFileName, voaData, imagePages);

                paginatedOutputCreationUtility.WritePaginationHistory(pageInfos, fileId, fileTaskSessionID);

                string outputAction = pFAMTM.ExpandTagsAndFunctions(OutputAction, sourceDocName);
                pDB.SetFileStatusToPending(fileId, outputAction, true);

                pDB.EndFileTaskSession(fileTaskSessionID, 0, 0, false);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53888",
                    "Specified pagination processing failed.", ex);
            }
        }

        (List<ImagePage> imagePages, List<PageInfo> pageInfos)
            GetPageLists(FAMTagManager pFAMTM, string sourceDocName)
        {
            List<ImagePage> imagePages = new();
            foreach (var pageSource in PageSources)
            {
                var sourceFileName = pFAMTM.ExpandTagsAndFunctions(pageSource.Document, sourceDocName);
                int pageCount = UtilityMethods.GetNumberOfPagesInImage(sourceFileName);

                imagePages.AddRange(
                    _miscUtils.Value.GetNumbersFromRange((uint)pageCount, pageSource.Pages)
                        .ToIEnumerable<uint>()
                        .Select(page => new ImagePage(sourceFileName, (int)page, 0)));
            }

            List<PageInfo> pageInfos = imagePages.Select(
                page => new PageInfo()
                {
                    DocumentName = page.DocumentName,
                    Page = page.PageNumber,
                })
                .ToList();

            return (imagePages, pageInfos);
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
                    string json = reader.ReadString();
                    CopyFrom(_serializer.Deserialize(json).CreateDomainObject());
                };

                // Freshly loaded object is not dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53889",
                    "Unable to load specified pagination task.", ex);
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
                    // Write task settings in json format
                    string json = _serializer.Serialize(CreateDataTransferObject());
                    writer.Write(json);

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
		        throw ExtractException.CreateComVisible("ELI53890",
                    "Unable to save specified pagination task.", ex);
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

        string GetOutputFileName(FileRecord pFileRecord, FileProcessingDB pDB, FAMTagManager pFAMTM)
        {
            string outputFileName = pFAMTM.ExpandTagsAndFunctions(OutputPath, pFileRecord.Name);
            var pathTags = new SourceDocumentPathTags(outputFileName);

            int copyCount = 1;
            while (File.Exists(outputFileName)
                || pDB.IsFileNameInWorkflow(outputFileName, pFileRecord.WorkflowID))
            {
                ExtractException.Assert("ELI53949", "Failed to find unique filename",
                    copyCount < 100, "Filename", outputFileName);

                outputFileName = pathTags.Expand(
                    Invariant($"$InsertBeforeExt(<SourceDocName>,_{copyCount++:D2})"));
            }

            return outputFileName;
        }

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
        /// Copies the specified <see cref="SpecifiedPaginationTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="SpecifiedPaginationTask"/> from which to copy.</param>
        void CopyFrom(SpecifiedPaginationTask task)
        {
            CopyFrom(task.GetSettings());
        }

        SettingsModel GetSettings()
        {
            return CreateDataTransferObject().DataTransferObject as SettingsModel;
        }

        void CopyFrom(SpecifiedPaginationTaskSettingsModelV1 settingsModel)
        {
            PageSources.Clear();
            PageSources.AddRange(settingsModel.PageSources);
            OutputPath = settingsModel.OutputPath;
            OutputAction = settingsModel.OutputAction;
        }

        #endregion Private Members
        
        #region IDomainObject

        public DataTransferObjectWithType CreateDataTransferObject()
        {
            try
            {
                var dto = new SettingsModel()
                {
                    PageSources = new List<PageSourceV1>(PageSources).AsReadOnly(),
                    OutputPath = OutputPath,
                    OutputAction = OutputAction
                };
                return new DataTransferObjectWithType(dto);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53937");
            }
        }

        #endregion IDomainObject
    }
}