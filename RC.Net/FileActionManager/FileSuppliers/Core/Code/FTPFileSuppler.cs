using EnterpriseDT.Net.Ftp;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileSuppliers
{
    /// <summary>
    /// Enum for specifing the action to be taken on the remote
    /// server after the file has been downloaded
    /// </summary>
    [ComVisible(true)]
    [Guid("4A889E08-F8C0-4319-BEF9-3FA1DD10E71E")]
    public enum AfterDownloadRemoteFileActon
    {
         /// <summary>
        /// Change the file extension of the file on the server
        /// </summary>
        ChangeRemoteFileExtension = 0,

       /// <summary>
        /// Delete the remote file from the server
        /// </summary>
        DeleteRemoteFile = 1,

         /// <summary>
        /// Do nothing to the remote file on the server
        /// </summary>
        DoNothingToRemoteFile = 2
   }

    /// <summary>
    /// A File supplier that will get files from a SFTP/FTP site
    /// </summary>
    [ComVisible(true)]
    [Guid("2D201AC7-8EE8-47D0-96B3-708F4E34435C")]
    [ProgId("Extract.FileActionManager.FileSuppliers.FTPFileSupplier")]
    public class FtpFileSupplier : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileSupplier, ILicensedComponent,
        IPersistStream, IDisposable
    {

        #region Constants

        /// <summary>
        /// The description of this file supplier
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Files from FTP site";

        /// <summary>
        /// Current file supplier version.
        /// </summary>
        const int _CURRENT_VERSION = 2;

        /// <summary>
        /// License owner for the edtftpnetpro library
        /// </summary>
        static readonly string _FTP_API_LICENSE_OWNER = "ExtractSystems";

        /// <summary>
        /// License key for the edtftpnetpro library
        /// </summary>
        static readonly string _FTP_API_LICENSE_KEY = "064-7556-4340-7862";

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        static readonly LicenseIdName _licenseId = LicenseIdName.FtpSftpFileTransfer;

        /// <summary>
        /// Number of milliseconds to wait between checking the <see cref="_filesToDownload"/> 
        /// for more files.
        /// This is used to wait on the <see cref="_stopSupplying"/> and <see cref="_pauseEvent"/> 
        /// when there are no more files in the <see cref="_filesToDownload"/>
        /// </summary>
        const int _WAIT_TIME_FOR_MORE_FILES_TO_BE_ADDED = 200;

        #endregion

        #region Fields

        // Whether the object is dirty or not.
        bool _dirty;

        // Target for the files that are being supplier
        IFileSupplierTarget _fileTarget;

        // Field for the FileExtensionsToDownload property
        string _fileExtensionsToDownload;

        // This is a regular expression that is set by the 
        // FileExtensionsToDownload set operator and used to file the
        // the files to download
        string _fileExtensionsToDownloadRegEx;

        // Event that signals that supplying has started
        // This is needed because the Start can be called
        // on one thread and Stop can be called on another thread
        // so need to make sure processing has actually started
        // before stopping it.
        AutoResetEvent _supplyingStarted = new AutoResetEvent(false);

        // Event used for controlling a pause.  If the event is NOT signaled supplying will
        // pause until it is signaled.  Its initial state should be set to signaled.
        ManualResetEvent _pauseEvent = new ManualResetEvent(true);

        // Event to indicate supplying should stop
        ManualResetEvent _stopSupplying = new ManualResetEvent(false);

        // Event to indicate the FTP Server should be checked for more files
        ManualResetEvent _pollFtpServerForFiles = new ManualResetEvent(false);
       
        // Event to indicate that all files have been added to the queue for the current
        // poll of the ftp server.
        ManualResetEvent _doneAddingFilesToQueue = new ManualResetEvent(false);

        // Thread that has been created to manage the download of files
        // from the ftp server
        Thread _ftpDownloadManagerThread;

        // LocalWorkingFolder with tags expanded
        string _expandedLocalWorkingFolder;

        // Queue of files to be downloaded.
        ConcurrentQueue<FTPFile> _filesToDownload = new ConcurrentQueue<FTPFile>();

         // Timer used to poll the ftp site if polling is enabled.
        System.Threading.Timer _pollingTimer;

        #endregion

        #region Properties

        /// <summary>
        /// Folder on FTP site to download file from
        /// </summary>
        public string RemoteDownloadFolder { get; set; }

        /// <summary>
        /// Extensions of files to download
        /// </summary>
        public string FileExtensionsToDownload 
        { 
            get
            {
                return _fileExtensionsToDownload;
            }
            set
            {
                try
                {
                    _fileExtensionsToDownload = value;
                    _fileExtensionsToDownloadRegEx = "^(" + FileExtensionsToDownload + ")$";
                    _fileExtensionsToDownloadRegEx = _fileExtensionsToDownloadRegEx
                        .Replace(".", "\\.")
                        .Replace(';', '|')
                        .Replace("*", ".*")
                        .Replace("?", ".");
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32181", "Unable to update FileExentionsToDownload");
                }
            }
        }

        /// <summary>
        /// Flag indicating that all subfolders of the download folder should be searched
        /// for files to download
        /// </summary>
        public bool RecursivelyDownload { get; set; }

        /// <summary>
        /// Flag indicating that the remote location should be polled for files
        /// every <see cref="PollingIntervalInMinutes"/>
        /// </summary>
        public bool PollRemoteLocation { get; set; }

        /// <summary>
        /// The interval in minutes between checks of the remote location for
        /// files only used if <see cref="PollRemoteLocation"/> is <see langword="true"/>
        /// </summary>
        public Int32 PollingIntervalInMinutes { get; set; }

        /// <summary>
        /// Action to be taken after the file has been downloaded from the server
        /// </summary>
        public AfterDownloadRemoteFileActon AfterDownloadAction { get; set; }

        /// <summary>
        /// The extension to change the remote file's extension to on the remote
        /// server.  Only used if <see cref="AfterDownloadAction"/> is set to 
        /// ChangeRemoteFileExtension
        /// </summary>
        public string NewExtensionForRemoteFile { get; set; }

        /// <summary>
        /// Local folder that files are copied to when they are downloaded from
        /// the remote server
        /// </summary>
        public string LocalWorkingFolder { get; set; }

        /// <summary>
        /// The object that contains all of the settings relevant to make a connection to 
        /// an ftp site
        /// </summary>
        [CLSCompliant(false)]
        public SecureFTPConnection ConfiguredFtpConnection { get; set; }

        /// <summary>
        /// Number of connections used to download files
        /// </summary>
        public int NumberOfConnections { get; set; }
        
        #endregion
          
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFileSupplier"/>.
        /// </summary>
        public FtpFileSupplier()
        {
            ConfiguredFtpConnection = new SecureFTPConnection();
            
            InitializeFtpApiLicense(ConfiguredFtpConnection);

            InitializeDefaults();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFileSupplier"/> using the given setting
        /// </summary>
        /// <param name="ftpFileSupplier">The <see cref="FtpFileSupplier"/> to initialize this
        /// instance of FtpFileSupplier with</param>
        public FtpFileSupplier(FtpFileSupplier ftpFileSupplier)
        {
            if (ftpFileSupplier != null)
            {
                CopyFrom(ftpFileSupplier);
            }
        }

        #endregion

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
        /// Performs configuration needed to create a valid <see cref="FtpFileSupplier"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_licenseId,
                    "ELI31989", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                using (FtpFileSupplier cloneOfThis = (FtpFileSupplier) Clone())
                using (FtpFileSupplierSettingsDialog dlg = new FtpFileSupplierSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31990",
                    "Error running configuration.");
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
                // This class is configured if the settings are valid
                return 
                    !string.IsNullOrWhiteSpace(RemoteDownloadFolder) && 
                    !string.IsNullOrWhiteSpace(LocalWorkingFolder) && 
                    !string.IsNullOrWhiteSpace(FileExtensionsToDownload) && 
                    (!PollRemoteLocation || PollingIntervalInMinutes > 0) &&
                    (AfterDownloadAction != AfterDownloadRemoteFileActon.ChangeRemoteFileExtension || 
                        !string.IsNullOrWhiteSpace(NewExtensionForRemoteFile)) &&
                    !string.IsNullOrWhiteSpace(ConfiguredFtpConnection.ServerAddress) && 
                    !string.IsNullOrWhiteSpace(ConfiguredFtpConnection.UserName) &&
                    !string.IsNullOrWhiteSpace(ConfiguredFtpConnection.Password);
                
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31991",
                    "Failed checking configuration.");
            }
        }

        #endregion

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="FtpFileSupplier"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="FtpFileSupplier"/> instance.</returns>
        public object Clone()
        {
            try
            {
                FtpFileSupplier supplier = new FtpFileSupplier(this);
                return supplier;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31992", "Unable to clone object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="FtpFileSupplier"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                FtpFileSupplier supplier = (FtpFileSupplier)pObject;
                CopyFrom(supplier);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31993", "Unable to copy object.");
            }
        }

        #endregion

        #region IFileSupplier Members

        /// <summary>
        /// Pauses file supply
        /// </summary>
        public void Pause()
        {
            try
            {
                // Stop the polling threads
                StopPolling();

                // Pause the suppling
                _pauseEvent.Reset();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31998", "Unable to pause supplying object.");
            }
        }

        /// <summary>
        /// Resumes file supplying after a pause
        /// </summary>
        public void Resume()
        {
            try
            {
                // Resume supplying 
                _pauseEvent.Set();

                StartPolling();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31999", "Unable to resume supplying object.");
            }
        }

        /// <summary>
        /// Starts file supplying
        /// </summary>
        /// <param name="pTarget">The IFileSupplerTarget that receives the files</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        [CLSCompliant(false)]
        public void Start(IFileSupplierTarget pTarget, FAMTagManager pFAMTM)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_licenseId,
                   "ELI32216", _COMPONENT_DESCRIPTION);

                InitializeEventsForStart();

                ExpandLocalWorkingFolder(pFAMTM.FPSFileDir);

                // Set the file target
                _fileTarget = pTarget;

                StartFileDownloadManagementThread();
                
                StartPolling();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32000", "Unable to start supplying object.");
            }
        }

        /// <summary>
        /// Stops file supplying
        /// </summary>
        public void Stop()
        {
            try
            {
                StopPolling();

                StopThreads();

                // Wait for the DownloadManagerThread to stop
                _ftpDownloadManagerThread.Join();
                _ftpDownloadManagerThread = null;

                // Clear out any remaining files in the files to download queue
                _filesToDownload = new ConcurrentQueue<FTPFile>();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32001", "Unable to stop supplying object.");
            }
        }

        #endregion

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
            try
            {
                return LicenseUtilities.IsLicensed(_licenseId);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31994",
                    "Unable to determine license status.");
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
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.
        /// </param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    InitializeDefaults();

                    RemoteDownloadFolder = reader.ReadString();
                    FileExtensionsToDownload = reader.ReadString();
                    RecursivelyDownload = reader.ReadBoolean();
                    PollRemoteLocation = reader.ReadBoolean();
                    PollingIntervalInMinutes = reader.ReadInt32();
                    AfterDownloadAction = (AfterDownloadRemoteFileActon)reader.ReadInt32();
                    NewExtensionForRemoteFile = reader.ReadString();
                    LocalWorkingFolder = reader.ReadString();

                    if (reader.Version == 2)
                    {
                        NumberOfConnections = reader.ReadInt32();
                    }
                    else
                    {
                        NumberOfConnections = 1;
                    }

                    string hexString = reader.ReadString();
                    using (MemoryStream ftpDataStream = new MemoryStream(hexString.ToByteArray()))
                    {
                        ConfiguredFtpConnection = new SecureFTPConnection();
                        ConfiguredFtpConnection.Load(ftpDataStream);
                    }
                    InitializeFtpApiLicense(ConfiguredFtpConnection);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31995",
                    "Unable to load FTP file supplier.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.
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
                    writer.Write(RemoteDownloadFolder);
                    writer.Write(FileExtensionsToDownload);
                    writer.Write(RecursivelyDownload);
                    writer.Write(PollRemoteLocation);
                    writer.Write(PollingIntervalInMinutes);
                    writer.Write((int)AfterDownloadAction);
                    writer.Write(NewExtensionForRemoteFile);
                    writer.Write(LocalWorkingFolder);
                    writer.Write(NumberOfConnections);

                    // Write the Ftp connection settings to the steam
                    using (MemoryStream ftpDataStream = new MemoryStream())
                    {
                        ConfiguredFtpConnection.Save(ftpDataStream);
                        writer.Write(ftpDataStream.ToArray().ToHexString());
                    }
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31996",
                    "Unable to save FTP file supplier.");
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
        /// Releases all resources used by the <see cref="FtpFileSupplier"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="FtpFileSupplier"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="FtpFileSupplier"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ConfiguredFtpConnection != null)
                {
                    ConfiguredFtpConnection.Dispose();
                    ConfiguredFtpConnection = null;
                }
                if (_supplyingStarted != null)
                {
                    _supplyingStarted.Dispose();
                    _supplyingStarted = null;
                }
                if (_pauseEvent != null)
                {
                    _pauseEvent.Dispose();
                    _pauseEvent = null;
                }
                if (_stopSupplying != null)
                {
                    _stopSupplying.Dispose();
                    _stopSupplying = null;
                }
                if(_pollFtpServerForFiles != null)
                {
                    _pollFtpServerForFiles.Dispose();
                    _pollFtpServerForFiles = null;
                }
                if (_pollingTimer != null)
                {
                    _pollingTimer.Dispose();
                    _pollingTimer = null;
                }
                if (_doneAddingFilesToQueue != null)
                {
                    _doneAddingFilesToQueue.Dispose();
                    _doneAddingFilesToQueue = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable

        #region EventHandlers

        /// <summary>
        /// Handles the FileDownloaded event when downloading files from the ftpserver
        /// If the file was successfully downloaded this will perform the 
        /// after download action othewise it will delete any file in the local folder 
        /// since this file may be corrupt.
        /// </summary>
        /// <param name="sender">The SecureFTPConnection that is downloading files</param>
        /// <param name="e">The FTPFileTransferEventArgs object that contains
        /// information about the file downloaded</param>
        void HandleFileDownloaded(object sender, FTPFileTransferEventArgs e)
        {
            try
            {
                SecureFTPConnection runningConnection = (SecureFTPConnection)sender;
                if (e.Succeeded)
                {
                    // Verify that the files is exists localy
                    if (File.Exists(e.LocalPath))
                    {
                        // Add the local file that was just downloaded to the database
                        _fileTarget.NotifyFileAdded(e.LocalPath, this);
                    }
                    else
                    {
                        ExtractException ee = new ExtractException("ELI32311", "File was not downloaded.");
                        ee.AddDebugData("LocalFile", e.LocalPath, false);
                        ee.AddDebugData("RemoteFile", e.RemoteFile, false);
                        throw ee;
                    }
                    
                    // Perform the after download action
                    switch (AfterDownloadAction)
                    {
                        case AfterDownloadRemoteFileActon.ChangeRemoteFileExtension:
                            runningConnection.RenameFile(e.RemoteFile, e.RemoteFile + NewExtensionForRemoteFile);
                            break;
                        case AfterDownloadRemoteFileActon.DeleteRemoteFile:
                            runningConnection.DeleteFile(e.RemoteFile);
                            break;
                    }
                }
                else
                {
                    // If the file was partially copied need to delete the file
                    if (File.Exists(e.LocalPath))
                    {
                        ExtractException ee = new ExtractException("ELI32323",
                            "File may have been partially downloaded");
                        ee.AddDebugData("Local File", e.LocalPath, false);
                        ee.AddDebugData("Remote File", e.RemoteFile, false);
                        ee.Log();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI32142");
                ee.AddDebugData("Remote File", e.RemoteFile, false);
                ee.Log();
            }
        }

        /// <summary>
        /// Handles the PollingTimer timeout event, it will cause the ManageFileDownload thread
        /// to connect to the ftp server and get a listing of files to download.  If the 
        /// ManageFileDownload thread is already connected to the ftp server this will cause it
        /// to connect to the server again when ever it finishes.
        /// </summary>
        /// <param name="o">Object the triggered the event</param>
        void HandlePollingTimerTimeout(object o)
        {
            _pollFtpServerForFiles.Set();
        }

        #endregion

        #region Thread Functions
        
        /// <summary>
        /// Gets a list of files to download from the ftp server and filters
        /// them to generate a list of files to download and then manages the 
        /// connections used to download the files from the ftp server
        /// </summary>
        void ManageFileDownload()
        {
            try
            {
                do
                {
                    _doneAddingFilesToQueue.Reset();
                    
                    // Start the file download threads
                    List<Task> downloadThreads = StartFileDownloadThreads();

                    GetFilesToDownload();

                    WaitForDownloadThreadsToFinish(downloadThreads);
                }
                while (!ExitManageFileDownload());
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI32197");
            }
            finally
            {
                _fileTarget.NotifyFileSupplyingDone(this);
            }
        }

        /// <summary>
        /// Starts <see cref="NumberOfConnections"/> file download threads.
        /// </summary>
        /// <returns>List containing the Delegate and the IAsynResult for each thread started for
        /// downloading files</returns>
        List<Task> StartFileDownloadThreads()
        {
            // Set up the download connections
            List<Task> downloadThreads =
                new List<Task>(NumberOfConnections);

            // Start the download threads
            for (int i = 0; i < NumberOfConnections; i++)
            {
                downloadThreads.Add(Task.Factory.StartNew(DownloadFiles));
            }
            return downloadThreads;
        }

        /// <summary>
        /// Determines if the ftp server should be checked again for files
        /// </summary>
        /// <returns><see lang="true"/> if Polling is enabled and it is time to check for files
        /// <see lang="false"/> if polling is not enabled or supplying should stop</returns>
        bool ExitManageFileDownload()
        {
            try
            {
                // If not polling, should exit if done adding files
                if (!PollRemoteLocation)
                {
                    return true;
                }

                // Wait for either stop suppling event or for a poll FTP server event
                WaitHandle[] handlesToWaitFor = new WaitHandle[]
                {
                    _stopSupplying,
                    _pollFtpServerForFiles
                };
                
                // Return false if result is index of pollFtpServerForFiles
                return WaitHandle.WaitAny(handlesToWaitFor) != 1;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32288");
            }
        }

        /// <summary>
        /// Downloads files from the <see cref="_filesToDownload"/> queue until it is empty or
        /// <see cref="_stopSupplying"/> is signaled.
        /// </summary>
        void DownloadFiles()
        {
            try
            {
                using (SecureFTPConnection runningConnection = (SecureFTPConnection)ConfiguredFtpConnection.Clone())
                {
                    InitializeFtpApiLicense(runningConnection);

                    // Add event handler for when files are down downloading.
                    runningConnection.Downloaded += new FTPFileTransferEventHandler(HandleFileDownloaded);

                    FTPFile currentFtpFile;
                    string localFile = "";

                    // Download the files
                    while (GetNextFileToDownload(out currentFtpFile))
                    {
                        try
                        {
                            // If not already connected connect to the FTP Server
                            if (!runningConnection.IsConnected)
                            {
                                runningConnection.Connect();
                            }

                            SetCurrentFtpWorkingFolder(runningConnection, currentFtpFile);
                            string localFilePath = GenerateLocalPathCreateIfNotExists(runningConnection.ServerDirectory);

                            // Determine the full name of the local file
                            localFile = Path.Combine(localFilePath, currentFtpFile.Name);
                            runningConnection.DownloadFile(localFile, currentFtpFile.Name);
                        }
                        catch (Exception ex)
                        {
                            ExtractException ee = new ExtractException("ELI32303", "Unable to download file.", ex);

                            if (currentFtpFile != null)
                            {
                                ee.AddDebugData("Source on FTP Server", currentFtpFile.Name, false);
                            }
                            ee.AddDebugData("Local File", localFile, false);
                            ee.AddDebugData("Current FTP working folder", runningConnection.ServerDirectory, false);
                            ee.Log();
                        }
                        finally
                        {
                            CloseConnectionIfPausedAndWaitForResume(runningConnection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI32371");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID File Suppliers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileSuppliers);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID File Suppliers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileSuppliers);
        }

        /// <summary>
        /// Copies settings from the given file suppler
        /// </summary>
        /// <param name="fileSupplier">The FtpFileSupplier to copy setttings from </param>
        public void CopyFrom(FtpFileSupplier fileSupplier)
        {
            try
            {
                RemoteDownloadFolder = fileSupplier.RemoteDownloadFolder;
                FileExtensionsToDownload = fileSupplier.FileExtensionsToDownload;
                RecursivelyDownload = fileSupplier.RecursivelyDownload;
                PollRemoteLocation = fileSupplier.PollRemoteLocation;
                PollingIntervalInMinutes = fileSupplier.PollingIntervalInMinutes;
                AfterDownloadAction = fileSupplier.AfterDownloadAction;
                NewExtensionForRemoteFile = fileSupplier.NewExtensionForRemoteFile;
                LocalWorkingFolder = fileSupplier.LocalWorkingFolder;
                ConfiguredFtpConnection = (SecureFTPConnection)fileSupplier.ConfiguredFtpConnection.Clone();
                NumberOfConnections = fileSupplier.NumberOfConnections;

                InitializeFtpApiLicense(ConfiguredFtpConnection);
                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32021");
            }
        }

        /// <summary>
        /// Determines which files in the dirContents should be downloaded and puts 
        /// them in <see cref="_filesToDownload"/> queue
        /// </summary>
        /// <param name="runningConnection">Connection to the FTP server</param>
        /// <param name="dirContents">Contents of a directory on the FTP server</param>
        void DetermineFilesToDownload(SecureFTPConnection runningConnection, FTPFile[] dirContents)
        {
            // Filter the directory contents for files and sub directories
            foreach (FTPFile file in dirContents)
            {
                if (_stopSupplying.WaitOne(0))
                {
                    return;
                }

                if (FilesToDownloadFilter(file))
                {
                    _filesToDownload.Enqueue(file);
                }
                else if (RecursivelyDownload && file.Dir  )
                {
                    DetermineFilesToDownload(runningConnection, file.Children);
                }
            }
        }

        /// <summary>
        /// Method used to determine if a file should be downloaded
        /// </summary>
        /// <param name="file">FTPFile record of file to check if it should be downloaded</param>
        /// <returns><see langword="true"/> if file should be downloaded
        /// <see langword="false"/>if the file should not be downloaded</returns>
        bool FilesToDownloadFilter(FTPFile file)
        {
            // if it is a directory return false
            if (file.Dir)
            {
                return false;
            }

            return Regex.IsMatch(file.Name, _fileExtensionsToDownloadRegEx,
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }

        /// <summary>
        /// Waits for all of the download threads to exit.
        /// </summary>
        /// <param name="downloadThreads">List of download threads</param>
        static void WaitForDownloadThreadsToFinish(List<Task> downloadThreads)
        {
            // Wait for download threads to finish
            foreach (var t in downloadThreads)
            {
                t.Wait();
            }

            // Clear the list
            downloadThreads.Clear();
        }

        /// <summary>
        /// Get the files to download from the ftp server and put them the filesToDownload queue
        /// </summary>
        void GetFilesToDownload()
        {
            try
            {
                using (SecureFTPConnection runningConnection = (SecureFTPConnection)ConfiguredFtpConnection.Clone())
                {
                    InitializeFtpApiLicense(runningConnection);

                    // Connect to the ftp server
                    runningConnection.Connect();

                    // Get all the files and directories in the working folder and subfolders if required
                    FTPFile[] directoryContents = runningConnection.GetFileInfos(RemoteDownloadFolder, RecursivelyDownload);

                    // Fill the filesToDownload list
                    DetermineFilesToDownload(runningConnection, directoryContents);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32324");
            }
            finally
            {
                _doneAddingFilesToQueue.Set();
            }
        }

        /// <summary>
        /// Returns the local path for the file being downloaded 
        /// </summary>
        /// <param name="currentRemoteWorkingFolder">Current working folder on the ftp server</param>
        /// <returns>The local path rooted to the _expandedLocalWorkingFolder </returns>
        string GenerateLocalPathCreateIfNotExists(string currentRemoteWorkingFolder)
        {
            // Generate the path so that the directory structure rooted to 
            // the expanded local working folder will be the same as the 
            // current ftp working folder rooted to the RemoteDownloadFolder.
            string pathForFile = Path.Combine(_expandedLocalWorkingFolder,
                currentRemoteWorkingFolder.Remove(0, RemoteDownloadFolder.Length));

            // Convert / to \ since the ftp server path char may be different that windows
            pathForFile = pathForFile.Replace('/', '\\');

            // Make sure the local folder exists
            if (!Directory.Exists(pathForFile))
            {
                Directory.CreateDirectory(pathForFile);
            }
            return pathForFile;
        }

        /// <summary>
        /// Gets the next file to download from the <see cref="_filesToDownload"/> Queue
        /// </summary>
        /// <param name="nextFile">Next file to download from the queue</param>
        /// <returns><see lang="true"/> if nextFile contains the next file
        /// and <see lang="false"/> if there are no more files to download or if supplying should 
        /// stop</returns>
        bool GetNextFileToDownload(out FTPFile nextFile)
        {
            // Loop until a file is removed from the queue or supplying is done
            while (!_stopSupplying.WaitOne(0))
            {
                if (_filesToDownload.TryDequeue(out nextFile))
                {
                    return true;
                }

                // If done adding files to the queue need to exit this loop
                if (_doneAddingFilesToQueue.WaitOne(0))
                {
                    if (_filesToDownload.IsEmpty)
                    {
                        break;
                    }
                    
                    continue;
                }

                // Wait on the stop supplying event for a set time before checking the queue
                // for files.
                _stopSupplying.WaitOne(_WAIT_TIME_FOR_MORE_FILES_TO_BE_ADDED);
            }

            nextFile = null;
            return false;
        }

        /// <summary>
        /// Sets the current working folder on the FTP server if it is not the same as the currentFile
        /// </summary>
        /// <param name="runningConnection">Connection to the FTP server</param>
        /// <param name="currentFile">Current file that will be downloaded next</param>
        static void SetCurrentFtpWorkingFolder(SecureFTPConnection runningConnection, FTPFile currentFile)
        {
            // Determine the current working folder on the ftp server 
            string currentFileDir = currentFile
                .Path
                .Remove(currentFile.Path.Length - currentFile.Name.Length);

            // Only change the working directory if it needs to be changed.
            if (currentFileDir != runningConnection.ServerDirectory)
            {
                runningConnection.ChangeWorkingDirectory(currentFileDir);
            }
            return;
        }
        
        /// <summary>
        /// Sets up and starts the download manager thread.
        /// </summary>
        void StartFileDownloadManagementThread()
        {
            // Start the supplying thread
            _ftpDownloadManagerThread = new Thread(ManageFileDownload);
            _ftpDownloadManagerThread.Start();

            _supplyingStarted.Set();
        }

        /// <summary>
        /// Sets the LicenseOwner and LicenseKey properties for the ftpConnection
        /// </summary>
        /// <param name="ftpConnection">Connection to be initialized</param>
        static void InitializeFtpApiLicense(SecureFTPConnection ftpConnection)
        {
            ftpConnection.LicenseOwner = _FTP_API_LICENSE_OWNER;
            ftpConnection.LicenseKey = _FTP_API_LICENSE_KEY;
        }

        /// <summary>
        /// Intializes properties and fields to initial default values
        /// </summary>
        void InitializeDefaults()
        {
            AfterDownloadAction = AfterDownloadRemoteFileActon.DeleteRemoteFile;

            // Set Polling IntervalInMinutes to the default
            PollingIntervalInMinutes = 1;
            NumberOfConnections = 1;
        }

        /// <summary>
        /// Stops polling if polling is active
        /// </summary>
        void StopPolling()
        {
            if (PollRemoteLocation)
            {
                _pollingTimer.Dispose();
                _pollingTimer = null;
            }
        }

        /// <summary>
        /// Starts a polling timer if polling is enabled
        /// </summary>
        void StartPolling()
        {
            if (PollRemoteLocation)
            {
                Int64 timeoutValue = (Int64)PollingIntervalInMinutes * 60 * 1000;
                _pollingTimer = new System.Threading.Timer(HandlePollingTimerTimeout, null,
                    timeoutValue, timeoutValue);
            }
        }

        /// <summary>
        /// Sets _expandedLocalWorkingFolder by expanding the tags in LocalWorking folder
        /// </summary>
        /// <param name="FPSFileDir">The FPS file directory needed to expand the LocalWorking Folder</param>
        void ExpandLocalWorkingFolder(string FPSFileDir)
        {
            FileActionManagerSupplierPathTags pathTags =
                new FileActionManagerSupplierPathTags(FPSFileDir);

            _expandedLocalWorkingFolder = pathTags.Expand(LocalWorkingFolder);
        }

        /// <summary>
        /// Resets or Sets events and flags to the starting state
        /// </summary>
        void InitializeEventsForStart()
        {
            _stopSupplying.Reset();
            _pollFtpServerForFiles.Reset();
            _pauseEvent.Set();
            _doneAddingFilesToQueue.Reset();
        }

        /// <summary>
        /// Sets events that will cause the ManageFileDownload and File downloading threads to exit
        /// </summary>
        void StopThreads()
        {
            _supplyingStarted.WaitOne();
            _stopSupplying.Set();
        }

        /// <summary>
        /// Checks if supplying should be paused and if it is closes the runningConnection and
        /// waits for supplying to resume
        /// </summary>
        /// <param name="runningConnection">FTP connection to close if supplying is paused</param>
        void CloseConnectionIfPausedAndWaitForResume(SecureFTPConnection runningConnection)
        {
            if (!_pauseEvent.WaitOne(0))
            {
                // Don't leave the connection open while pausing
                if (runningConnection.IsConnected)
                {
                    runningConnection.Close();
                }
                _pauseEvent.WaitOne();
            }
        }

        #endregion
    }
}
