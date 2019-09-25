using Extract.AttributeFinder;
using Extract.FileActionManager.Forms;
using Extract.Imaging;
using Extract.Imaging.Utilities;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// An <see cref="IFileProcessingTask"/> which splits a multi-page source document into separate
    /// documents porting uss and voa data to the output documents in the process.
    /// </summary>
    [ComVisible(true)]
    [Guid("3D2B6D31-DB61-4D54-8A12-1355FC8E3EEE")]
    [CLSCompliant(false)]
    public interface ISplitMultipageDocumentTask : ICategorizedComponent, ICopyableObject,
        IConfigurableObject, IMustBeConfiguredObject, IFileProcessingTask, ILicensedComponent,
        IPersistStream
    {
        /// <summary>
        /// Gets or sets the path tag expression that defines the output filenames for split
        /// documents.
        /// </summary>
        string OutputPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path tag expression that defines the voa filename for both the source
        /// and output documents.
        /// </summary>
        string VOAPath
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> which splits a multi-page source document into separate
    /// documents porting uss and voa data to the output documents in the process.
    /// </summary>
    [ComVisible(true)]
    [Guid("EF1279E8-4EC2-4CBF-9DE5-E107D97916C0")]
    [ProgId("Extract.FileActionManager.FileProcessors.SplitMultipageDocumentTask")]
    public class SplitMultipageDocumentTask : ISplitMultipageDocumentTask, IDisposable
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Split multi-page document";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// A string representation of the GUID of the split multi-page document task.
        /// </summary>
        static readonly string _SPLIT_MULTI_PAGE_DOCUMENT_TASK_GUID =
            typeof(SplitMultipageDocumentTask).GUID.ToString("B");

        /// <summary>
        /// The page number tag
        /// </summary>
        internal const string PageNumberTag = "<PageNumber>";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="ImageCodecs"/> instance to use to read document images from disk.
        /// </summary>
        ImageCodecs _codecs;

        /// <summary>
        /// The path tag function the describes where output files should go.
        /// </summary>
        string _outputPath =
            @"$DirOf(<SourceDocName>)\Split_$FileOf(<SourceDocName>)\$InsertBeforeExt($FileOf(<SourceDocName>),."
                + PageNumberTag + ")";
                
        /// <summary>
        /// The path of the voa file relative to both the source and output documents.
        /// </summary>
        string _voaPath = "<SourceDocName>.voa";

        /// <summary>
        /// Indicates whether this task object is dirty or not
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitMultipageDocumentTask"/> class.
        /// </summary>
        public SplitMultipageDocumentTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitMultipageDocumentTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="SplitMultipageDocumentTask"/> from which settings should
        /// be copied.</param>
        public SplitMultipageDocumentTask(SplitMultipageDocumentTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44821");
            }
        }

        #endregion Constructors

        #region ISplitMultipageDocumentTask

        /// <summary>
        /// Gets or sets the path tag expression that defines the output filenames for split
        /// documents.
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
                    throw ex.AsExtract("ELI44844");
                }
            }
        }

        /// <summary>
        /// Gets or sets the path tag expression that defines the voa filename for both the source
        /// and output documents.
        /// </summary>
        public string VOAPath
        {
            get
            {
                return _voaPath;
            }
            set
            {
                try
                {
                    if (value != _voaPath)
                    {
                        _voaPath = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI44850");
                }
            }
        }

        #endregion ISplitMultipageDocumentTask

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
        /// Performs configuration needed to create a valid <see cref="SendEmailTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI44856", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (SplitMultipageDocumentTask)Clone();

                using (var dialog = new SplitMultipageDocumentTaskSettingsDialog(cloneOfThis))
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
                throw ExtractException.CreateComVisible("ELI44857",
                    "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                return !string.IsNullOrWhiteSpace(OutputPath);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44858", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="SplitMultipageDocumentTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="SplitMultipageDocumentTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new SplitMultipageDocumentTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI44826", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="SplitMultipageDocumentTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as SplitMultipageDocumentTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to SplitMultipageDocumentTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI44827", "Unable to copy object.", ex);
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
            if (_codecs != null)
            {
                _codecs.Dispose();
                _codecs = null;
            }
        }

        /// <summary>
        /// Called to notify the file processor that the pending document queue is empty, but
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
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI44828", _COMPONENT_DESCRIPTION);
				
				// Unlock pdf support
                UnlockLeadtools.UnlockPdfSupport(false);

                _codecs = new ImageCodecs();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44829", "Unable to initialize \"Create file\" task.");
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
        /// <returns>An <see cref="EFileProcessingResult"/> indicating the result of the
        /// processing.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord,
            int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI44830", _COMPONENT_DESCRIPTION);

                var startTime = DateTime.Now;
                int fileTaskSessionID = pDB.StartFileTaskSession(
                    _SPLIT_MULTI_PAGE_DOCUMENT_TASK_GUID, pFileRecord.FileID, nActionID);

                var sourceDocName = pFileRecord.Name;

                int pageCount;
                using (var imageReader = _codecs.CreateReader(pFileRecord.Name))
                {
                    pageCount = imageReader.PageCount;
                }

                if (pageCount > 1)
                {
                    SplitDocument(pFileRecord, pDB, pFAMTM, fileTaskSessionID, pageCount);
                }
                else
                {
                    // Even if document wasn't paginated, record to the database the fact that this
                    // document was processed, yet didn't produce any new output documents.
                    AddPaginationHistory(pDB, fileTaskSessionID, sourceDocName, pFileRecord.FileID,
                        Enumerable.Range(1, pageCount));
                }

                double duration = (DateTime.Now - startTime).TotalSeconds;
                pDB.EndFileTaskSession(fileTaskSessionID, duration, 0, 0);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44831", "Unable to process the file.");
            }
        }

        #endregion IFileProcessingTask Members

        #region IAccessRequired Members

        /// <summary>
        /// Returns bool value indicating if the task requires admin access.
        /// </summary>
        /// <returns><see langword="true"/> if the task requires admin access
        /// <see langword="false"/> if task does not require admin access.</returns>
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
            try
            {
                return LicenseUtilities.IsLicensed(LicenseIdName.FileActionManagerObjects);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI44832",
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
                    OutputPath = reader.ReadString();
                    VOAPath = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI44833",
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
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(OutputPath);
                    writer.Write(VOAPath);

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
                throw ExtractException.CreateComVisible("ELI44834",
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

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="SplitMultipageDocumentTask"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="SplitMultipageDocumentTask"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="SplitMultipageDocumentTask"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Splits the multi-page document represented by <see paramref="pFileRecord"/> into separate
        /// documents porting uss and voa data to the output documents in the process.
        /// </summary>
        /// <param name="pFileRecord">The <see cref="FileRecord"/> representing the document.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> to which the split should be recorded.
        /// </param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use to expand path tags.</param>
        /// <param name="fileTaskSessionID">The ID of the active file task session.</param>
        /// <param name="pageCount">The number of pages in the document</param>
        void SplitDocument(FileRecord pFileRecord, FileProcessingDB pDB, FAMTagManager pFAMTM, int fileTaskSessionID, int pageCount)
        {
            ExtractException.Assert("ELI44843", "Internal logic error", pageCount > 1);

            string sourceDocName = pFileRecord.Name;
            var pathTags = new FileActionManagerPathTags(pFAMTM, sourceDocName);

            // Load the voa if it exists
            string voaFileName = pathTags.Expand(VOAPath);
            IUnknownVector attributes = null;
            if (File.Exists(voaFileName))
            {
                attributes = new IUnknownVector();
                attributes.LoadFrom(voaFileName, false);

				// Update source doc name on all attributes
                AttributeMethods.UpdateSourceDocNameOfAttributes(attributes, sourceDocName);
                attributes.ReportMemoryUsage();
            }

            // Output each page as a separate doc
            for (int pageNum = 1; pageNum <= pageCount; pageNum++)
            {
                string outputFileName = GetOutputFileName(pathTags, pageNum);
                var imagePage = new ImagePage(sourceDocName, pageNum, 0);

                using (var tempFile = new TemporaryFile(true))
                {
                    ImageMethods.StaplePagesAsNewDocument(new[] { imagePage }, tempFile.FileName);

                    int fileId = -1;
                    if (pDB.IsFileNameInWorkflow(outputFileName, pFileRecord.WorkflowID))
                    {
                        fileId = pDB.GetFileID(outputFileName);
                    }
                    else
                    {
                        long fileSize = new FileInfo(tempFile.FileName).Length;

                        fileId = pDB.AddFileNoQueue(
                            outputFileName, fileSize, 1, EFilePriority.kPriorityNormal,
                            pFileRecord.WorkflowID);
                    }

                    // Record pagination to DB.
                    AddPaginationHistory(pDB, fileTaskSessionID, sourceDocName, fileId, new[] { pageNum } );

                    File.Copy(tempFile.FileName, outputFileName, true);
                }

                AttributeMethods.CreateUssAndVoaForPaginatedDocument(outputFileName, attributes, new[] { imagePage });
            }
        }

        /// <summary>
        /// Gets the name of the output file for the specified <see cref="pageNum"/>.
        /// </summary>
        /// <param name="pathTags">The <see cref="FileActionManagerPathTags"/> to use to expand path
        /// tag expressions.</param>
        /// <param name="pageNum">The page number from the source file the output file represents.
        /// </param>
        /// <returns>The name of the output file for the specified page.</returns>
        string GetOutputFileName(FileActionManagerPathTags pathTags, int pageNum)
        {
            pathTags.AddTag(PageNumberTag, pageNum.ToString("D4", CultureInfo.InvariantCulture));
            string outputFileName = pathTags.Expand(OutputPath);

            string directory = Path.GetDirectoryName(outputFileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return outputFileName;
        }

        /// <summary>
        /// Gets the name for an output voa file relative to the specified
        /// <see paramref="outputFileName"/> for the associated document.
        /// </summary>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> used to expand <see cref="VOAPath"/>.
        /// </param>
        /// <param name="outputFileName">The filename of the associated output document.</param>
        /// <returns></returns>
        string GetVOAFileName(FAMTagManager pFAMTM, string outputFileName)
        {
            var pathTags = new FileActionManagerPathTags(pFAMTM, outputFileName);
            string voaFileName = pathTags.Expand(VOAPath);

            string directory = Path.GetDirectoryName(outputFileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return voaFileName;
        }

        /// <summary>
        /// Records to the Pagination table of <see paramref="pDB"/> the association
        /// <see paramref="outputDocName"/> to <see paramref="sourceDocName"/>
        /// </summary>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> to which the association is recorded.
        /// </param>
        /// <param name="fileTaskSessionID">The file task session identifier.</param>
        /// <param name="sourceDocName">Name of the source document.</param>
        /// <param name="outputDocId">Name of the output document.</param>
        /// <param name="pages">The pages numbers of the source document added to the output document.
        /// </param>
        static void AddPaginationHistory(FileProcessingDB pDB, int fileTaskSessionID,
            string sourceDocName, int outputDocId, IEnumerable<int> pages)
        {
            var sourcePageInfo = pages.Select(pageNum =>
                    new StringPairClass()
                    {
                        StringKey = sourceDocName,
                        StringValue = pageNum.ToString(CultureInfo.InvariantCulture)
                    }
                ).ToIUnknownVector();
            pDB.AddPaginationHistory(outputDocId, sourcePageInfo, null, fileTaskSessionID);
        }

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
        /// Copies the specified <see cref="SplitMultipageDocumentTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="SplitMultipageDocumentTask"/> from which to copy.</param>
        void CopyFrom(SplitMultipageDocumentTask task)
        {
            OutputPath = task.OutputPath;
            VOAPath = task.VOAPath;

            _dirty = true;
        }

        #endregion Private Members
    }
}
