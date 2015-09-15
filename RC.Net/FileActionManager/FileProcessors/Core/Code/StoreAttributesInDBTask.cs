using Extract.AttributeFinder;
using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AttributeDbMgrComponentsLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Interface definition for the <see cref="StoreAttributesInDBTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("31FE0077-EF9C-4F69-8BBD-D7344A3855B7")]
    [CLSCompliant(false)]
    public interface IStoreAttributesInDBTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask,
        ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets the name of the VOA file to store in the DB
        /// </summary>
        /// <value>
        /// The name of the VOA file to store in the DB
        /// </value>
        string VOAFileName { get; set; }

        /// <summary>
        /// Gets or sets the name to use for the dataset.
        /// </summary>
        /// <value>
        /// The name to use for the dataset.
        /// </value>
        string AttributeSetName { get; set; }

        /// <summary>
        /// Gets or sets whether raster zone data should be stored with the attributes.
        /// </summary>
        /// <value><see langword="true"/> if raster zone data should be stored; otherwise,
        /// <see langword="false"/>.</value>
        bool StoreRasterZones { get; set; }
    }

    /// <summary>
    /// A <see cref="IFileProcessingTask"/> that allows validation of XML file syntax and schema.
    /// </summary>
    [ComVisible(true)]
    [Guid("B25D64C0-6FF6-4E0B-83D4-0D5DFEB68006")]
    [ProgId("Extract.FileActionManager.FileProcessors.StoreAttributesInDbTask")]
    public class StoreAttributesInDBTask: IStoreAttributesInDBTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Store attributes in DB";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the VOA file to store in the DB.
        /// </summary>
        string _voaFileName = "<SourceDocName>.voa";

        /// <summary>
        /// The name to use for the dataset.
        /// </summary>
        string _attributeSetName;

        /// <summary>
        /// Indicates whether raster zone data should be stored with the attributes.
        /// </summary>
        bool _storeRasterZones = true;

        /// <summary>
        /// Provides the implementation to be able to store the attributes in the database.
        /// </summary>
        AttributeDBMgr _attributeDBManager;

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        #endregion Fields

         #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAttributesInDBTask"/> class.
        /// </summary>
        public StoreAttributesInDBTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAttributesInDBTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="StoreAttributesInDBTask"/> from which settings should
        /// be copied.</param>
        public StoreAttributesInDBTask(StoreAttributesInDBTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38643");
            }
        }

        #endregion Constructors

        #region IStoreAttributesInDBTask Members

        /// <summary>
        /// Gets or sets the name of the VOA file to store in the DB.
        /// </summary>
        /// <value>
        /// The name of the VOA file to store in the DB.
        /// </value>
        public string VOAFileName
        {
            get
            {
                return _voaFileName;
            }

            set
            {
                if (!string.Equals(_voaFileName, value, StringComparison.Ordinal))
                {
                    _voaFileName = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name to use for the dataset.
        /// </summary>
        /// <value>
        /// The name to use for the dataset.
        /// </value>
        public string AttributeSetName
        {
            get
            {
                return _attributeSetName;
            }

            set
            {
                if (!string.Equals(_attributeSetName, value, StringComparison.Ordinal))
                {
                    _attributeSetName = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether raster zone data should be stored with the attributes.
        /// </summary>
        /// <value><see langword="true"/> if raster zone data should be stored; otherwise,
        /// <see langword="false"/>.</value>
        public bool StoreRasterZones
        {
            get
            {
                return _storeRasterZones;
            }

            set
            {
                if (value != _storeRasterZones)
                {
                    _storeRasterZones = value;
                    _dirty = true;
                }
            }
        }

        #endregion IStoreAttributesInDBTask Members

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
        /// Performs configuration needed to create a valid <see cref="StoreAttributesInDBTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI38644", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (StoreAttributesInDBTask)Clone();

                using (var dialog = new StoreAttributesInDBTaskSettingsDialog(cloneOfThis))
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
                throw ExtractException.CreateComVisible("ELI38645",
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
                return (!string.IsNullOrWhiteSpace(VOAFileName) &&
                        !string.IsNullOrWhiteSpace(AttributeSetName));
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38646", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="StoreAttributesInDBTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="StoreAttributesInDBTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new StoreAttributesInDBTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI38647", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="StoreAttributesInDBTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as StoreAttributesInDBTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to StoreAttributesInDbTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI38648", "Unable to copy object.", ex);
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
                _attributeDBManager = new AttributeDBMgr();
                _attributeDBManager.FAMDB = pDB;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38649", 
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
                    "ELI38650", _COMPONENT_DESCRIPTION);

                FileActionManagerPathTags pathTags = new FileActionManagerPathTags(
                    pFAMTM, pFileRecord.Name);

                string voaFileName = pathTags.Expand(VOAFileName);
                ExtractException.Assert("ELI38651", "VOA file not found.", File.Exists(voaFileName),
                    "VOA filename", voaFileName);

                IUnknownVector voaData = new IUnknownVector();
                voaData.LoadFrom(voaFileName, false);

                voaData.ReportMemoryUsage();

                _attributeDBManager.CreateNewAttributeSetForFile(
                    pFileRecord.FileID, 0/*_attributeSetName*/, voaData /*, StoreRasterZones*/);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38652", "Unable to process the file.");
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
                return LicenseUtilities.IsLicensed(_LICENSE_ID);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI38653",
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
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    _voaFileName = reader.ReadString();
                    _attributeSetName = reader.ReadString();
                    _storeRasterZones = reader.ReadBoolean();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI38654",
                    "Unable to load object from stream.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> into which the object should be saved.
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
                    writer.Write(_voaFileName);
                    writer.Write(_attributeSetName);
                    writer.Write(_storeRasterZones);

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
                throw ExtractException.CreateComVisible("ELI38655",
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
        /// Copies the specified <see cref="StoreAttributesInDBTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="StoreAttributesInDBTask"/> from which to copy.</param>
        void CopyFrom(StoreAttributesInDBTask task)
        {
            _voaFileName = task.VOAFileName;
            _attributeSetName = task.AttributeSetName;
            _storeRasterZones = task.StoreRasterZones;

            _dirty = true;
        }

        #endregion Private Members
    }
}
