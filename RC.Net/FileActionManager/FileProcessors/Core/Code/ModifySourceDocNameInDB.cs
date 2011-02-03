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
    /// Represents a file processing task that will change the name of the source document in the 
    /// FAM database.
    /// </summary>
    [ComVisible(true)]
    [Guid("488C4C2A-BEE4-4004-83E9-07A318DB0981")]
    [ProgId("Extract.FileActionManager.FileProcessors.ModifySourceDocNameInDB")]
    public class ModifySourceDocNameInDB : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask, 
        ILicensedComponent, IPersistStream
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Modify Source Document Name in Database";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name with tags to rename the file
        /// </summary>
        string _renameFileTo;

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        static readonly LicenseIdName _licenseId = LicenseIdName.FileActionManagerObjects;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifySourceDocNameInDB"/> class.
        /// </summary>
        public ModifySourceDocNameInDB()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifySourceDocNameInDB"/> class.
        /// </summary>
        /// <param name="renameFileTo">String with tags representing new name.</param>
        [CLSCompliant(false)]
        public ModifySourceDocNameInDB(string renameFileTo)
            : this()
        {
            _renameFileTo = renameFileTo;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string RenameFileTo
        {
            get
            {
                return _renameFileTo;
            }
            set
            {
                if (!_renameFileTo.Equals(value, StringComparison.Ordinal))
                {
                    _renameFileTo = value;
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
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileProcessors);
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
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Copies the specified <see cref="ModifySourceDocNameInDB"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="ModifySourceDocNameInDB"/> from which to copy.</param>
        public void CopyFrom(ModifySourceDocNameInDB task)
        {
            try
            {
                _renameFileTo = task.RenameFileTo;
                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31507", ex);
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

        #endregion

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ModifySourceDocNameInDB"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                using (var dialog = new ModifySourceDocNameInDBSettingsDialog(_renameFileTo))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _renameFileTo = dialog.RenameFileTo;
                      _dirty = true;
                      return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31508",
                    "Error running configuration.", ex);
            }
        }

        #endregion

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(_renameFileTo);
        }

        #endregion

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ModifySourceDocNameInDB"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ModifySourceDocNameInDB"/> instance.</returns>
        public object Clone()
        {
            try
            {
                var task =
                    new ModifySourceDocNameInDB(_renameFileTo);

                return task;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31509", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ModifySourceDocNameInDB"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as ModifySourceDocNameInDB;
                if (task == null)
                {
                    throw new InvalidCastException("Object is not a modify source document in database task.");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31510", "Unable to copy object.", ex);
            }
        }

        #endregion

        #region IFileProcessingTask Members

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
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            // Nothing to do
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
            string newFileNameInDB = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_licenseId, "ELI31511", _COMPONENT_DESCRIPTION);
                
                // Name of the file being processed that will have it's name changed in the database
                string fileName = pFileRecord.Name;

                // Create a tag manager and expand the tags in the file name
                var tags = new FileActionManagerPathTags(
                    Path.GetFullPath(fileName), pFAMTM.FPSFileDir);

                // Get the new name for the file in the database
                newFileNameInDB = tags.Expand(_renameFileTo);

                // Change the file name in the database
                pDB.RenameFile(pFileRecord, newFileNameInDB);

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI31513", "Unable to change the SourceDocName.", ex);
                if (newFileNameInDB != null)
                {
                    ee.AddDebugData("New name for file in database", newFileNameInDB, false);
                }
                ee.AddDebugData("File Being Processed", newFileNameInDB, false);
                ee.AddDebugData("File ID", pFileRecord.FileID, false);
                ee.AddDebugData("Action ID", nActionID, false);

                // Throw the extract exception as a COM visible exception
                throw ExtractException.CreateComVisible("ELI31514",
                    "Unable to process the file.", ee);
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
            // This task requires admin access
            return true;
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
                return LicenseUtilities.IsLicensed(_licenseId);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31515",
                    "Unable to determine license status.", ex);
            }
        }

        #endregion

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
                    // Read the file name from the stream
                    _renameFileTo = reader.ReadString();

                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31516",
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
                    // Serialize the settings
                    writer.Write(_renameFileTo);

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
                throw ExtractException.CreateComVisible("ELI31517",
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
    }
}
