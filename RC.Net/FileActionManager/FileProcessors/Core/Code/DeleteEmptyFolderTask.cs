using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// An <see cref="IFileProcessingTask"/> which deletes a specified directory so long as it is
    /// empty.
    /// </summary>
    [ComVisible(true)]
    [Guid("A697CE90-5D82-4674-98C4-181BF46846B0")]
    [ProgId("Extract.FileActionManager.FileProcessors.DeleteEmptyFolderTask")]
    public class DeleteEmptyFolderTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask,
        ILicensedComponent, IPersistStream
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Delete empty folder";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the folder to be deleted (if empty).
        /// </summary>
        string _folderName;

        /// <summary>
        /// Indicates whether any root folder which end up empty as well should be deleted too.
        /// </summary>
        bool _deleteRecursively;

        /// <summary>
        /// Indicates whether recursive deletion should be stopped at a specified folder.
        /// </summary>
        bool _limitRecursion;

        /// <summary>
        /// If _limitRecursion is set, the folder at which deletion should be stopped.
        /// </summary>
        string _recursionLimit;

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteEmptyFolderTask"/> class.
        /// </summary>
        public DeleteEmptyFolderTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteEmptyFolderTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="DeleteEmptyFolderTask"/> from which settings should
        /// be copied.</param>
        public DeleteEmptyFolderTask(DeleteEmptyFolderTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31855");
            }
        }

        #endregion Constructors

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
        /// Performs configuration needed to create a valid <see cref="DeleteEmptyFolderTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                using (var dialog = new DeleteEmptyFolderTaskSettingsDialog())
                {
                    dialog.FolderName = _folderName;
                    dialog.DeleteRecursively = _deleteRecursively;
                    dialog.LimitRecursion = _limitRecursion;
                    dialog.RecursionLimit = _recursionLimit;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _folderName = dialog.FolderName;
                        _deleteRecursively = dialog.DeleteRecursively;
                        _limitRecursion = dialog.LimitRecursion;
                        _recursionLimit = dialog.RecursionLimit;

                        _dirty = true;
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31856",
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
            return !string.IsNullOrWhiteSpace(_folderName);
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="DeleteEmptyFolderTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="DeleteEmptyFolderTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new DeleteEmptyFolderTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31857", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="DeleteEmptyFolderTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as DeleteEmptyFolderTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to DeleteEmptyFolderTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31858", "Unable to copy object.", ex);
            }
        }

        #endregion ICopyableObject Members

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
            try
            {
                // Validate the license
                validateLicense("ELI31859");
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31860",
                    "Unable to initialize \"Delete empty folder\" task.");
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
                validateLicense("ELI31861");

                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFileRecord.Name, pFAMTM.FPSFileDir);
                string folderName = pathTags.Expand(_folderName);
                folderName = folderName.Replace('/', '\\').TrimEnd('\\');

                string recursionLimit = string.Empty;
                if (_deleteRecursively && _limitRecursion)
                {
                    recursionLimit = pathTags.Expand(_recursionLimit);
                    recursionLimit = recursionLimit.Replace('/', '\\').TrimEnd('\\');
                }

                // If the folder doesn't exist, there is nothing to do.
                if (Directory.Exists(folderName))
                {
                    if (_limitRecursion &&
                        !folderName.StartsWith(recursionLimit, StringComparison.OrdinalIgnoreCase))
                    {
                        ExtractException ee = new ExtractException("ELI31886",
                            "Delete folder task is incorrectly configured. " +
                            "Recursion limit folder is not a parent of the folder to delete.");
                        ee.AddDebugData("Folder", folderName, false);
                        ee.AddDebugData("Recursion Limit", recursionLimit, false);
                        throw ee;
                    }

                    // Delete this folder (if empty), then loop to delete parent folders if
                    // _deleteAncestors is specified.
                    while (!Directory.EnumerateFileSystemEntries(folderName).Any())
                    {
                        // If limiting recursion at a specific folder, check to see if this is
                        // that folder.
                        if (_limitRecursion
                            && folderName.Length == recursionLimit.Length)
                        {
                            // If the parent folder is the recurions limit, stop the recursion.
                            break;
                        }

                        Directory.Delete(folderName);
                        if (!_deleteRecursively)
                        {
                            break;
                        }
                        
                        DirectoryInfo parentInfo = Directory.GetParent(folderName);

                        if (parentInfo == null)
                        {
                            break;
                        }
                        else
                        {
                            folderName = parentInfo.FullName;
                        }
                    }
                }

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31862", "Unable to process the file.");
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
                throw ExtractException.CreateComVisible("ELI31863",
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
                    _folderName = reader.ReadString();
                    _deleteRecursively = reader.ReadBoolean();
                    _limitRecursion = reader.ReadBoolean();
                    _recursionLimit = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31864",
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
                    writer.Write(_folderName);
                    writer.Write(_deleteRecursively);
                    writer.Write(_limitRecursion);
                    writer.Write(_recursionLimit);

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
                throw ExtractException.CreateComVisible("ELI31865",
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

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// <see cref="ExtractGuids.FileProcessors"/> COM category.
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
        /// <see cref="ExtractGuids.FileProcessors"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Copies the specified <see cref="DeleteEmptyFolderTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="DeleteEmptyFolderTask"/> from which to copy.</param>
        void CopyFrom(DeleteEmptyFolderTask task)
        {
            _folderName = task._folderName;
            _deleteRecursively = task._deleteRecursively;
            _limitRecursion = task._limitRecursion;
            _recursionLimit = task._recursionLimit;

            _dirty = true;
        }

        /// <summary>
        /// Throws an <see cref="ExtractException"/> if a the software is not properly licensed to
        /// use this task.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with any thrown licensing exception.
        /// </param>
        static void validateLicense(string eliCode)
        {
            LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                eliCode, _COMPONENT_DESCRIPTION);
        }

        #endregion Private Members
    }
}
