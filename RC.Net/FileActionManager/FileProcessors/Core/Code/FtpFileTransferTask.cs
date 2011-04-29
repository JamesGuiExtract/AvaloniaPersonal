using EnterpriseDT.Net.Ftp;
using EnterpriseDT.Util;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Ftp;
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
    /// Enum for specifing the action to be taken on the remote server
    /// </summary>
    [ComVisible(true)]
    [Guid("BA66282F-6304-4A8C-A792-E2CA7A52DD9A")]
    public enum TransferActionToPerform
    {
        /// <summary>
        /// Indicates file should be uploaded to FTP Server
        /// </summary>
        UploadFileToFtpServer = 0,

        /// <summary>
        /// Indicates file should be downloaded from FTP Server
        /// </summary>
        DownloadFileFromFtpServer = 1,

        /// <summary>
        /// Indicates file should be deleted from FTP Server
        /// </summary>
        DeleteFileFromFtpServer = 2
    }


    /// <summary>
    /// Represents a file processing task that will upload, download or delete files from ftp server
    /// </summary>
    [ComVisible(true)]
    [Guid("A4D719DE-EAD2-47AA-991D-9E60FE0D8D9F")]
    [ProgId("Extract.FileActionManager.FileProcessors.FtpFileTransferTask")]
    public class FtpFileTransferTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask, 
        ILicensedComponent, IPersistStream, IDisposable
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Transfer file via FTP/SFTP";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields
        
        // Action the task is to perform on a file
        TransferActionToPerform _actionToPerform;

        // Contains the string including tags that specifies the remote file name
        string _remoteFileName = "";

        // Contains the string including tags that specifies the local file name
        string _localFileName = SourceDocumentPathTags.SourceDocumentTag;

        // Connection that is used for the settings for the ftp server
        SecureFTPConnection _configuredFtpConnection = new SecureFTPConnection();

        // Indicates that settings have been changed, but not saved.
        bool _dirty;

        // The license id to validate in licensing calls
        static readonly LicenseIdName _licenseId = LicenseIdName.FtpSftpFileTransfer;

        // Connection used when processing files
        SecureFTPConnection _runningConnection;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFileTransferTask"/> class.
        /// </summary>
        public FtpFileTransferTask()
        {
            FtpMethods.InitializeFtpApiLicense(_configuredFtpConnection);
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Specifies the action to perform with the file
        /// </summary>
        public TransferActionToPerform ActionToPerform
        {
            get
            {
                return _actionToPerform;
            }
            set
            {
                if (_actionToPerform != value)
                {
                    _actionToPerform = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Specifies the name of the remote file using tags.
        /// </summary>
        public string RemoteFileName
        {
            get
            {
                return _remoteFileName;
            }
            set
            {
                try
                {
                    if (!_remoteFileName.Equals(value ?? "", StringComparison.Ordinal))
                    {
                        _remoteFileName = value ?? "";
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32417", "Error setting remote file name");
                }
            }
        }

        /// <summary>
        /// Specifies the name of the local file using tags.
        /// </summary>
        public string LocalFileName
        {
            get
            {
                return _localFileName;
            }
            set
            {
                try
                {
                    if (!_localFileName.Equals(value ?? "", StringComparison.Ordinal))
                    {
                        _localFileName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32418", "Error setting local file name");
                }
            }
        }

        /// <summary>
        /// The object that contains all of the settings relevant to make a connection to 
        /// an ftp site
        /// </summary>
        [CLSCompliant(false)]
        public SecureFTPConnection ConfiguredFtpConnection 
        {
            get
            {
                return _configuredFtpConnection;
            }
            set
            {
                try
                {
                    // TODO: Setup code to figure out if the connection object is different than the one
                    // already set 
                    _configuredFtpConnection = value;
                    FtpMethods.InitializeFtpApiLicense(_configuredFtpConnection);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI32489");
                }
                _dirty = true;
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

        #endregion

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="FtpFileTransferTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                using (FtpFileTransferTask cloneOfThis = (FtpFileTransferTask)Clone())
                using (var dialog = new FtpFileTransferSettingsDialog(cloneOfThis))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _dirty = true;
                        CopyFrom(dialog.Settings);

                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32420", "Error running configuration.");
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
            try
            {
                return (ActionToPerform == TransferActionToPerform.DeleteFileFromFtpServer ||
                    !string.IsNullOrWhiteSpace(_localFileName)) &&
                    !string.IsNullOrWhiteSpace(_remoteFileName) &&
                    !string.IsNullOrWhiteSpace(ConfiguredFtpConnection.ServerAddress) &&
                    !string.IsNullOrWhiteSpace(ConfiguredFtpConnection.UserName) &&
                    !string.IsNullOrWhiteSpace(ConfiguredFtpConnection.Password);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32434", "Error checking configuration.");
            }
        }

        #endregion

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="FtpFileTransferTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="FtpFileTransferTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                var task = new FtpFileTransferTask();

                task.CopyFrom(this);

                return task;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32421", "Unable to clone object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="FtpFileTransferTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as FtpFileTransferTask;
                if (task == null)
                {
                    throw new InvalidCastException("Object is not a Transfer file via FTP/SFTP task.");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32422", "Unable to copy object.");
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
            try
            {
                if (_runningConnection != null)
                {
                    _runningConnection.Dispose();
                    _runningConnection = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32443", "Error closing task.");
            }
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
                // Create the running connection and intialize the license
                _runningConnection = (SecureFTPConnection)ConfiguredFtpConnection.Clone();
                FtpMethods.InitializeFtpApiLicense(_runningConnection);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32442", "Unable to Initialize Ftp File Transfer task.");
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
            string localFile = null;
            string remoteFile = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_licenseId, "ELI32423", _COMPONENT_DESCRIPTION);

                // Create a tag manager and expand the tags in the file name
                var tags = new FileActionManagerPathTags(
                    Path.GetFullPath(pFileRecord.Name), pFAMTM.FPSFileDir);

                if (ActionToPerform != TransferActionToPerform.DeleteFileFromFtpServer)
                {
                    localFile = tags.Expand(_localFileName);
                }

                remoteFile = tags.Expand(_remoteFileName);
                remoteFile = remoteFile.Replace('\\', '/');

                _runningConnection.Connect();
                PerformAction(localFile, remoteFile);

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                // Wrap the exception as an extract exception and add debug data
                var ee = ex.CreateComVisible("ELI32425", "Unable to process the file.");
                if (string.IsNullOrWhiteSpace(localFile))
                {
                    ee.AddDebugData("Local File ", localFile, false);
                }
                if (string.IsNullOrWhiteSpace(remoteFile))
                {
                    ee.AddDebugData("Remote file", remoteFile, false);
                }
                ee.AddDebugData("File ID", pFileRecord.FileID, false);
                ee.AddDebugData("Action ID", nActionID, false);

                // Throw the extract exception as a COM visible exception
                throw ee;
            }
            finally
            {
                _runningConnection.Close();
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
                throw ex.CreateComVisible("ELI32427", "Unable to determine license status.");
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
                    _actionToPerform = (TransferActionToPerform)reader.ReadInt32();
                    _remoteFileName = reader.ReadString();
                    _localFileName = reader.ReadString();

                    string hexString = reader.ReadString();
                    using (MemoryStream ftpDataStream = new MemoryStream(hexString.ToByteArray()))
                    {
                        ConfiguredFtpConnection = new SecureFTPConnection();
                        ConfiguredFtpConnection.Load(ftpDataStream);
                    }
                    FtpMethods.InitializeFtpApiLicense(_configuredFtpConnection);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32428",
                    "Unable to load object from stream.");
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
                    writer.Write((int)_actionToPerform);
                    writer.Write(_remoteFileName);
                    writer.Write(_localFileName);

                    // Write the Ftp connection settings to the steam
                    using (MemoryStream ftpDataStream = new MemoryStream())
                    {
                        ConfiguredFtpConnection.Save(ftpDataStream);
                        writer.Write(ftpDataStream.ToArray().ToHexString());
                    }

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
                throw ex.CreateComVisible("ELI32429",
                    "Unable to save object to stream");
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
        /// Releases all resources used by the <see cref="FtpFileTransferTask"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="FtpFileTransferTask"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="FtpFileTransferTask"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_configuredFtpConnection != null)
                {
                    _configuredFtpConnection.Dispose();
                    _configuredFtpConnection = null;
                }
                if (_runningConnection != null)
                {
                    _runningConnection.Dispose();
                    _runningConnection = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable

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
        /// Copies the specified <see cref="FtpFileTransferTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="FtpFileTransferTask"/> from which to copy.</param>
        public void CopyFrom(FtpFileTransferTask task)
        {
            try
            {
                _actionToPerform = task.ActionToPerform;
                _localFileName = task.LocalFileName;
                _remoteFileName = task._remoteFileName;

                ConfiguredFtpConnection = (SecureFTPConnection)task.ConfiguredFtpConnection.Clone();

                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI32419", ex);
            }
        }

        /// <summary>
        /// Performs the configured action on the files
        /// </summary>
        /// <param name="localFile">File name with path of the local file if used</param>
        /// <param name="remoteFile">File name with path of the remote file</param>
        private void PerformAction(string localFile, string remoteFile)
        {
            FtpMethods.SetCurrentFtpWorkingFolder(_runningConnection, remoteFile);
            remoteFile = PathUtil.GetFileName(remoteFile);
            switch (ActionToPerform)
            {
                case TransferActionToPerform.UploadFileToFtpServer:
                    _runningConnection.UploadFile(localFile, remoteFile);
                    break;
                case TransferActionToPerform.DownloadFileFromFtpServer:
                    FtpMethods.GenerateLocalPathCreateIfNotExists(
                        _runningConnection.ServerDirectory, Path.GetDirectoryName(localFile),
                        _runningConnection.ServerDirectory);
                    _runningConnection.DownloadFile(localFile, remoteFile);
                    break;
                case TransferActionToPerform.DeleteFileFromFtpServer:
                    _runningConnection.DeleteFile(remoteFile);
                    break;
            }
        }

        #endregion Methods
    }
}
