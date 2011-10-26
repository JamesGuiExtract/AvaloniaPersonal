using EnterpriseDT.Net.Ftp;
using EnterpriseDT.Util;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Ftp;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Interface for the <see cref="FtpTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("58E04DBF-7087-4EBD-BC14-6499700C7CF9")]
    [CLSCompliant(false)]
    public interface IFtpTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask,
        ILicensedComponent, IPersistStream, IFtpEventErrorSource, IDisposable
    {
        /// <summary>
        /// Specifies the action to perform with the file
        /// </summary>
        EFTPAction ActionToPerform
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies the name of the remote file using tags.
        /// </summary>
        string RemoteOrOldFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies the name of the local file using tags.
        /// </summary>
        string LocalOrNewFileName
        {
            get;
            set;
        }

        /// <summary>
        /// The object that contains all of the settings relevant to make a connection to 
        /// an ftp site
        /// </summary>
        [CLSCompliant(false)]
        SecureFTPConnection ConfiguredFtpConnection
        {
            get;
            set;
        }

        /// <summary>
        /// Number of times to retry calls to the ftp server
        /// </summary>
        int NumberOfTimesToRetry
        {
            get;
            set;
        }

        /// <summary>
        /// Time to wait between retries for calls to Ftp server
        /// </summary>
        int TimeToWaitBetweenRetries
        {
            get;
            set;
        }

        /// <summary>
        /// If a file delete is being performed, indicates whether the folder it was deleted from
        /// should be deleted if it is now empty.
        /// </summary>
        bool DeleteEmptyFolder
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a file processing task that will upload, download or delete files from ftp server
    /// </summary>
    [ComVisible(true)]
    [Guid("A4D719DE-EAD2-47AA-991D-9E60FE0D8D9F")]
    [ProgId("Extract.FileActionManager.FileProcessors.FtpTask")]
    public class FtpTask : IFtpTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Transfer, rename or delete via FTP/SFTP";

        /// <summary>
        /// Current task version.
        /// <para><b>Version 3</b></para>
        /// Added <see cref="DeleteEmptyFolder"/>.
        /// </summary>
        const int _CURRENT_VERSION = 3;

        /// <summary>
        /// Default wait time between retries
        /// </summary>
        const int _DEFAULT_WAIT_TIME_IN_MILLISECONDS_BETWEEN_RETRIES = 1000;

        /// <summary>
        /// Default number of retries before failure
        /// </summary>
        const int _DEFAULT_NUMBER_OF_RETRIES_BEFORE_FAILURE = 10;

        #endregion Constants

        #region Fields
        
        // Action the task is to perform on a file
        EFTPAction _actionToPerform;

        // Contains the string including tags that specifies the remote file name
        string _remoteOrOldFileName = "";

        // Contains the string including tags that specifies the local file name or the new name
        // to assign to a remote file that is being renamed.
        string _localOrNewFileName = SourceDocumentPathTags.SourceDocumentTag;

        // Connection that is used for the settings for the ftp server
        SecureFTPConnection _configuredFtpConnection = new SecureFTPConnection();

        // Number of times to retry
        int _numberOfTimesToRetry = _DEFAULT_NUMBER_OF_RETRIES_BEFORE_FAILURE;

        // Time to wait between retries
        int _timeToWaitBetweenRetries = _DEFAULT_WAIT_TIME_IN_MILLISECONDS_BETWEEN_RETRIES;

        // If a file delete is being performed, indicates whether the folder it was deleted from
        // should be deleted if it is now empty.
        bool _deleteEmptyFolder;

        // Indicates that settings have been changed, but not saved.
        bool _dirty;

        // The license id to validate in licensing calls
        static readonly LicenseIdName _licenseId = LicenseIdName.FtpSftpFileTransfer;

        // Connection used when processing files
        SecureFTPConnection _runningConnection;

        // Event to signal if that task has been canceled
        // This will only be checked if a file is being retried.
        AutoResetEvent _cancelTask = new AutoResetEvent(false);

        // Flag set when task is initialized to indicate if loading the download info file is required
        bool _loadDownloadInfoFile;

        /// <summary>
        /// Allows for FTP operations to be automatically retried as configured.
        /// </summary>
        Retry<Exception> _retry;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpTask"/> class.
        /// </summary>
        public FtpTask()
        {
            FtpMethods.InitializeFtpApiLicense(_configuredFtpConnection);
        }

        #endregion Constructor

        #region Events

        /// <summary>
        /// Raised when an error occurs during an FTP operation.
        /// </summary>
        public event EventHandler<ExtractExceptionEventArgs> FtpError;

        #endregion Events

        #region Properties

        /// <summary>
        /// Specifies the action to perform with the file
        /// </summary>
        [CLSCompliant(false)]
        public EFTPAction ActionToPerform
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
        public string RemoteOrOldFileName
        {
            get
            {
                return _remoteOrOldFileName;
            }
            set
            {
                try
                {
                    if (!_remoteOrOldFileName.Equals(value ?? "", StringComparison.Ordinal))
                    {
                        _remoteOrOldFileName = value ?? "";
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
        /// Specifies the name of the local file or the new name to assign to a remote file that is
        /// being renamed using tags.
        /// </summary>
        public string LocalOrNewFileName
        {
            get
            {
                return _localOrNewFileName;
            }
            set
            {
                try
                {
                    if (!_localOrNewFileName.Equals(value ?? "", StringComparison.Ordinal))
                    {
                        _localOrNewFileName = value;
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

        /// <summary>
        /// Number of times to retry calls to the ftp server
        /// </summary>
        public int NumberOfTimesToRetry
        {
            get
            {
                return _numberOfTimesToRetry;
            }
            set
            {
                if (_numberOfTimesToRetry != value)
                {
                    _numberOfTimesToRetry = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Time to wait between retries for calls to Ftp server
        /// </summary>
        public int TimeToWaitBetweenRetries
        {
            get
            {
                return _timeToWaitBetweenRetries;
            }
            set
            {
                if (_timeToWaitBetweenRetries != value)
                {
                    _timeToWaitBetweenRetries = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// If a file delete is being performed, indicates whether the folder it was deleted from
        /// should be deleted if it is now empty.
        /// </summary>
        public bool DeleteEmptyFolder
        {
            get
            {
                return _deleteEmptyFolder;
            }

            set
            {
                if (_deleteEmptyFolder != value)
                {
                    _deleteEmptyFolder = value;
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

        #endregion

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="FtpTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                using (FtpTask cloneOfThis = (FtpTask)Clone())
                using (var dialog = new FtpTaskSettingsDialog(cloneOfThis))
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
                return (ActionToPerform == EFTPAction.kDeleteFileFromFtpServer ||
                    !string.IsNullOrWhiteSpace(_localOrNewFileName)) &&
                    !string.IsNullOrWhiteSpace(_remoteOrOldFileName) &&
                    _remoteOrOldFileName[0] != '.' &&
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
        /// Creates a copy of the <see cref="FtpTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="FtpTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                var task = new FtpTask();

                task.CopyFrom(this);

                return task;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32421", "Unable to clone object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="FtpTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as FtpTask;
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
            try
            {
                _cancelTask.Set();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32649", "Could not cancel.");
            }
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
        /// while the Standby call is still ocurring. If this happens, the return value of Standby
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
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Create the running connection and intialize the license
                _runningConnection = (SecureFTPConnection)ConfiguredFtpConnection.Clone();
                FtpMethods.InitializeFtpApiLicense(_runningConnection);

                // Check if the RemoteSourceDocName tag is used in the input or output file name
                _loadDownloadInfoFile = _localOrNewFileName.Contains(FileActionManagerPathTags.RemoteSourceDocumentTag) ||
                    _remoteOrOldFileName.Contains(FileActionManagerPathTags.RemoteSourceDocumentTag);

                // Create retry object
                _retry = new Retry<Exception>(NumberOfTimesToRetry, TimeToWaitBetweenRetries, _cancelTask);

                // Notify the FtpEventRecorder to recheck the DBInfo setting for whether to log FTP events
                FtpEventRecorder.RecheckFtpLoggingStatus();
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
            string localOrNewFile = null;
            string remoteOrOldFile = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_licenseId, "ELI32423", _COMPONENT_DESCRIPTION);

                IPathTags tags;

                if (_loadDownloadInfoFile)
                {
                    FtpDownloadedFileInfo downloadFileInfo = new FtpDownloadedFileInfo(Path.GetFullPath(pFileRecord.Name) + ".info");

                    // Load the .info file 
                    downloadFileInfo.Load();

                    // Create a tag manager and expand the tags in the file name
                    tags = new FileActionManagerPathTags(
                       Path.GetFullPath(pFileRecord.Name), pFAMTM.FPSFileDir, downloadFileInfo.RemoteSourceDocName);
                }
                else
                {
                    tags = new FileActionManagerPathTags(
                       Path.GetFullPath(pFileRecord.Name), pFAMTM.FPSFileDir);
                }

                if (ActionToPerform != EFTPAction.kDeleteFileFromFtpServer)
                {
                    localOrNewFile = tags.Expand(_localOrNewFileName);
                }

                remoteOrOldFile = tags.Expand(_remoteOrOldFileName);
                remoteOrOldFile = remoteOrOldFile.Replace('\\', '/');

                // There are retry settings for the file supplier that will allow quicker exiting if
                // the stop button is pressed on the FAM so change the connection to not retry.
                _runningConnection.RetryCount = 0;

                // Call the PerformAction retry within a FtpEventRecorder block so that an FTP event
                // history row will be added upon success or after all retries have been exhausted.
                using (FtpEventRecorder recorder = new FtpEventRecorder(this,
                    _runningConnection, pDB, nActionID, false, ActionToPerform))
                {
                    // Assign the FileRecord for the recorder so that it knows which file the FTP
                    // event relates to.
                    recorder.FileRecord = pFileRecord;

                    _retry.DoRetry(() => PerformAction(localOrNewFile, remoteOrOldFile));
                }

                if (DeleteEmptyFolder && ActionToPerform == EFTPAction.kDeleteFileFromFtpServer)
                {
                    string remoteFolder =
                        remoteOrOldFile.Substring(0, remoteOrOldFile.LastIndexOf('/') + 1);

                    try
                    {
                        bool isFolderEmpty;
                        
                        using (FtpEventRecorder recorder = new FtpEventRecorder(this, _runningConnection,
                            pDB, nActionID, false, EFTPAction.kGetDirectoryListing))
                        {
                            recorder.FileRecord = pFileRecord;

                            isFolderEmpty = _retry.DoRetry(() => IsFolderEmpty(remoteFolder));
                        }

                        if (isFolderEmpty)
                        {
                            using (FtpEventRecorder recorder = new FtpEventRecorder(this,
                                _runningConnection, pDB, nActionID, false, EFTPAction.kDeleteFileFromFtpServer))
                            {
                                recorder.FileRecord = pFileRecord;

                                _retry.DoRetry(() => DeleteRemoteFolder(remoteFolder));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtractException ee = new ExtractException("ELI34010",
                            "Failed to delete empty remote directory", ex);
                        ee.AddDebugData("Remote directory", remoteFolder, false);
                        
                        // The deletion of a directory when empty is not a critical task and may
                        // fail because there are directory contents we can't see. Just log.
                        ee.Log();
                    }
                }

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                // Wrap the exception as an extract exception and add debug data
                var ee = ex.CreateComVisible("ELI32425", "Unable to process the file.");
                if (string.IsNullOrWhiteSpace(localOrNewFile))
                {
                    ee.AddDebugData("Local File ", localOrNewFile, false);
                }
                if (string.IsNullOrWhiteSpace(remoteOrOldFile))
                {
                    ee.AddDebugData("Remote file", remoteOrOldFile, false);
                }
                ee.AddDebugData("File ID", pFileRecord.FileID, false);
                ee.AddDebugData("Action ID", nActionID, false);

                // Throw the extract exception as a COM visible exception
                throw ee;
            }
            finally
            {
                if (_runningConnection.IsConnected)
                {
                    _runningConnection.Close();
                }
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
                    InitializeDefaults();

                    _actionToPerform = (EFTPAction)reader.ReadInt32();
                    _remoteOrOldFileName = reader.ReadString();
                    _localOrNewFileName = reader.ReadString();

                    if (reader.Version >= 2)
                    {
                        _numberOfTimesToRetry = reader.ReadInt32();
                        _timeToWaitBetweenRetries = reader.ReadInt32();
                    }

                    if (reader.Version >= 3)
                    {
                        _deleteEmptyFolder = reader.ReadBoolean();
                    }

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
                    writer.Write(_remoteOrOldFileName);
                    writer.Write(_localOrNewFileName);
                    writer.Write(_numberOfTimesToRetry);
                    writer.Write(_timeToWaitBetweenRetries);
                    writer.Write(_deleteEmptyFolder);

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
        /// Releases all resources used by the <see cref="FtpTask"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="FtpTask"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="FtpTask"/>.
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
                if (_cancelTask != null)
                {
                    _cancelTask.Dispose();
                    _cancelTask = null;
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
        /// Copies the specified <see cref="FtpTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="FtpTask"/> from which to copy.</param>
        public void CopyFrom(FtpTask task)
        {
            try
            {
                _actionToPerform = task.ActionToPerform;
                _localOrNewFileName = task.LocalOrNewFileName;
                _remoteOrOldFileName = task._remoteOrOldFileName;
                _timeToWaitBetweenRetries = task._timeToWaitBetweenRetries;
                _numberOfTimesToRetry = task._numberOfTimesToRetry;
                _deleteEmptyFolder = task._deleteEmptyFolder;

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
        /// <param name="localOrNewFile">File name with path of the local file if used or or the new
        /// name to assign to a remote file that is being renamed.</param>
        /// <param name="remoteOrOldFile">The remote or old file.</param>
        private void PerformAction(string localOrNewFile, string remoteOrOldFile)
        {
            try
            {
                if (!_runningConnection.IsConnected)
                {
                    _runningConnection.Connect();
                }
                
                FtpMethods.SetCurrentFtpWorkingFolder(_runningConnection, remoteOrOldFile);
                remoteOrOldFile = PathUtil.GetFileName(remoteOrOldFile);
                if (ActionToPerform == EFTPAction.kRenameFileOnFtpServer)
                {
                    localOrNewFile = PathUtil.GetFileName(localOrNewFile);
                }

                switch (ActionToPerform)
                {
                    case EFTPAction.kUploadFileToFtpServer:
                        _runningConnection.UploadFile(localOrNewFile, remoteOrOldFile);
                        break;
                    case EFTPAction.kDownloadFileFromFtpServer:
                        FtpMethods.GenerateLocalPathCreateIfNotExists(
                            _runningConnection.ServerDirectory, Path.GetDirectoryName(localOrNewFile),
                            _runningConnection.ServerDirectory);
                        _runningConnection.DownloadFile(localOrNewFile, remoteOrOldFile);
                        break;
                    case EFTPAction.kRenameFileOnFtpServer:
                        _runningConnection.RenameFile(remoteOrOldFile, localOrNewFile);
                        break;
                    case EFTPAction.kDeleteFileFromFtpServer:
                        _runningConnection.DeleteFile(remoteOrOldFile);
                        break;
                    
                    default:
                        throw new ExtractException("ELI34054", "Unexpected FTP operation.");
                }
            }
            catch (Exception ex)
            {
                // Raise an FTP error (as part of the IFtpEventExceptionSource implementation)
                OnFtpError(ex.AsExtract("ELI33979"));

                ExtractException ee = ex.AsExtract("ELI32648");
                switch (ActionToPerform)
                {
                    case EFTPAction.kUploadFileToFtpServer:
                        ee.AddDebugData("Action", "UploadFielToFtpServer", false);
                        ee.AddDebugData("Local file", localOrNewFile, false);
                        break;
                    case EFTPAction.kDownloadFileFromFtpServer:
                        ee.AddDebugData("Action", "DownloadFileFromFtpServer", false);
                        ee.AddDebugData("Local file", localOrNewFile, false);
                        break;
                    case EFTPAction.kRenameFileOnFtpServer:
                        ee.AddDebugData("Action", "RenameFileOnFtpServer", false);
                        ee.AddDebugData("New filename", localOrNewFile, false);
                        break;
                    case EFTPAction.kDeleteFileFromFtpServer:
                        ee.AddDebugData("Action", "DeleteFileFromFtpServer", false);
                        break;
                }
                ee.AddDebugData("RemoteFile", remoteOrOldFile, false);
                throw ee;
            }
        }

        /// <summary>
        /// Determines whether <see paramref="remoteFolder"/> is empty.
        /// </summary>
        /// <param name="remoteFolder">The remote folder to check.</param>
        /// <returns><see langword="true"/> if <see paramref="remoteFolder"/> is empty; otherwise,
        /// <see langword="false"/>.</returns>
        bool IsFolderEmpty(string remoteFolder)
        {
            bool showHiddenFilesValue = false;

            try
            {
                showHiddenFilesValue = _runningConnection.ShowHiddenFiles;
                _runningConnection.ShowHiddenFiles = true;

                FTPFile[] directoryContents = _retry.DoRetry(() =>
                    _runningConnection.GetFileInfos(remoteFolder, false));

                return (directoryContents.Length == 0);
            }
            catch (Exception ex)
            {
                // Raise an FTP error (as part of the IFtpEventExceptionSource implementation)
                OnFtpError(ex.AsExtract("ELI34005"));

                ExtractException ee = new ExtractException("ELI34000",
                    "Failed to get directory listing on remote server", ex);
                ee.AddDebugData("Remote directory", remoteFolder, false);
                throw ee;
            }
            finally
            {
                if (!showHiddenFilesValue && _runningConnection.IsConnected)
                {
                    _runningConnection.ShowHiddenFiles = false;
                }
            }
        }

        /// <summary>
        /// Deletes the <see paramref="remoteFolder"/>.
        /// </summary>
        /// <param name="remoteFolder">The folder to delete.</param>
        void DeleteRemoteFolder(string remoteFolder)
        {
            try
            {
                _runningConnection.DeleteDirectory(remoteFolder);
            }
            catch (Exception ex)
            {
                // Raise an FTP error (as part of the IFtpEventExceptionSource implementation)
                OnFtpError(ex.AsExtract("ELI34006"));

                ExtractException ee = new ExtractException("ELI34007",
                    "Failed to delete directory on remote server", ex);
                ee.AddDebugData("Remote directory", remoteFolder, false);
                throw ee;
            }
        }

        void InitializeDefaults()
        {
            _numberOfTimesToRetry = _DEFAULT_NUMBER_OF_RETRIES_BEFORE_FAILURE;
            _timeToWaitBetweenRetries = _DEFAULT_WAIT_TIME_IN_MILLISECONDS_BETWEEN_RETRIES;
        }

        /// <summary>
        /// Raises the <see cref="FtpError"/> event.
        /// </summary>
        /// <param name="ftpException">An <see cref="ExtractException"/> containing information
        /// about the error.</param>
        void OnFtpError(ExtractException ftpException)
        {
            if (FtpError != null)
            {
                FtpError(this, new ExtractExceptionEventArgs(ftpException));
            }
        }

        #endregion Methods
    }
}
