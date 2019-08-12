using Extract.FileActionManager.Forms;
using Extract.GoogleCloud;
using Extract.Imaging;
using Extract.Imaging.Utilities;
using Extract.Interop;
using Extract.Licensing;
using System;
using System.Collections.Concurrent;
using System.IO;
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
    /// Interface definition for the <see cref="CloudOCRTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("11BB2F67-A17E-4233-92D4-57B05D82EE32")]
    [CLSCompliant(false)]
    public interface ICloudOCRTask :
        ICategorizedComponent,
        IConfigurableObject,
        IMustBeConfiguredObject,
        ICopyableObject,
        IFileProcessingTask,
        ILicensedComponent,
        IPersistStream
    {
        string ProjectCredentialsFile { get; set; }
        string BucketBaseName { get; set; }
        string ImageBucketName { get; }
        string OutputBucketName { get; }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> that
    /// </summary>
    [ComVisible(true)]
    [Guid("5F25F6DC-449B-405B-A8CC-D9D74CC4BA08")]
    [ProgId("Extract.FileActionManager.FileProcessors.CloudOCRTask")]
    public class CloudOCRTask : ICloudOCRTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: OCR document with GCV";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        static readonly object _GOOGLE_OCR_CREATION_LOCK = new object();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        static ConcurrentDictionary<(string, string), GoogleCloudOCR> _googleCloudOCRInstances = new ConcurrentDictionary<(string, string), GoogleCloudOCR>();
        static ThreadLocal<MiscUtils> _miscUtils = new ThreadLocal<MiscUtils>(() => new MiscUtilsClass());

        GoogleCloudOCR GoogleCloudOCR => _googleCloudOCRInstances[(ProjectCredentialsFile, BucketBaseName)];

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudOCRTask"/> class.
        /// </summary>
        public CloudOCRTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudOCRTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="CloudOCRTask"/> from which settings should
        /// be copied.</param>
        public CloudOCRTask(CloudOCRTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46798");
            }
        }

        #endregion Constructors

        #region ICloudOCRTask Members

        public string ProjectCredentialsFile { get; set; }
        public string BucketBaseName { get; set; }

        public string ImageBucketName => (BucketBaseName ?? "") + "-images";

        public string OutputBucketName => (BucketBaseName ?? "") + "-output";

        #endregion ICloudOCRTask Members

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
        /// Performs configuration needed to create a valid <see cref="CloudOCRTask"/>.
        /// </summary>
        /// <returns><c>true</c> if the configuration was successfully updated or
        /// <c>false</c> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI46799", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (CloudOCRTask)Clone();

                using (var dialog = new CloudOCRTaskSettingsDialog(cloneOfThis))
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
                throw ExtractException.CreateComVisible("ELI46800",
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
                if (string.IsNullOrWhiteSpace(BucketBaseName))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(ProjectCredentialsFile))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46801", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="CloudOCRTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="CloudOCRTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new CloudOCRTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46802", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="CloudOCRTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                if (!(pObject is CloudOCRTask task))
                {
                    throw new InvalidCastException("Invalid copy-from object. Requires CloudOCRTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46803", "Unable to copy object.", ex);
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
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB, IFileRequestHandler pFileRequestHandler)
        {
            try
            {
                ExtractException.Assert("ELI46804", "This object is not configured", IsConfigured());

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI46805", _COMPONENT_DESCRIPTION);

                UnlockLeadtools.UnlockLeadToolsSupport();

                FileActionManagerPathTags pathTags = new FileActionManagerPathTags(pFAMTM);

                string credentialsFile = Path.GetFullPath(pathTags.Expand(ProjectCredentialsFile));

                // This will load from an etf or plain text file
                string encoded = _miscUtils.Value.GetBase64StringFromFile(credentialsFile);
                byte[] bytes = Convert.FromBase64String(encoded);
                string json = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                // Create static cloud OCR task with this json
                if (!_googleCloudOCRInstances.ContainsKey((ProjectCredentialsFile, BucketBaseName)))
                {
                    lock (_GOOGLE_OCR_CREATION_LOCK)
                    {
                        if (!_googleCloudOCRInstances.ContainsKey((ProjectCredentialsFile, BucketBaseName)))
                        {
                            _googleCloudOCRInstances[(ProjectCredentialsFile, BucketBaseName)] = new GoogleCloudOCR(json, ImageBucketName, OutputBucketName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46808",
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
                string inputPath = pFileRecord.Name;

                ExtractException.Assert("ELI47189", "Cannot OCR image page with non-standard view perspective. " +
                "use ImageFormatConverter <strInput> <strOutput> <out_type> /vp to set the view " +
                "perspective to the standard setting.", !ImageMethods.DoesImageHavePageWithNonstandardViewPerspective(inputPath));

                string outputPath = inputPath + ".uss";
                GoogleCloudOCR.ProcessFile(inputPath, outputPath, pFileRecord.FileID, pProgressStatus);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46809", "Failed to OCR document with Google Cloud Vision");
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
                throw ExtractException.CreateComVisible("ELI46810",
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
                    ProjectCredentialsFile = reader.ReadString();
                    BucketBaseName = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46811",
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
                    writer.Write(ProjectCredentialsFile);
                    writer.Write(BucketBaseName);

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
                throw ExtractException.CreateComVisible("ELI46812",
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
        /// Copies the specified <see cref="CloudOCRTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="CloudOCRTask"/> from which to copy.</param>
        void CopyFrom(CloudOCRTask task)
        {
            ProjectCredentialsFile = task.ProjectCredentialsFile;
            BucketBaseName = task.BucketBaseName;

            _dirty = true;
        }

        #endregion Private Members
    }
}
