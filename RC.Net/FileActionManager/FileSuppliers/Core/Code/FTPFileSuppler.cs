using EnterpriseDT.Net.Ftp;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        static readonly LicenseIdName _licenseId = LicenseIdName.FileActionManagerObjects;

        #endregion

        #region Fields

        /// <summary>
        /// Whether the object is dirty or not.
        /// </summary>
        bool _dirty;

        // Target for the files that are being supplied
        // this will be set in the Start method
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
        EventWaitHandle _supplyingStarted;

        // flag to indicate that supplying should be stopped
        bool _stopSupplying;

        // Thread that has been created to manage the download of files
        // from the ftp server
        Thread _ftpDownloadManagerThread;

        // LocalWorkingFolder with tags expanded
        string _expandedLocalWorkingFolder;

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

        #endregion
  
        
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFileSupplier"/>.
        /// </summary>
        public FtpFileSupplier()
        {
            ConfiguredFtpConnection = new SecureFTPConnection();
            ConfiguredFtpConnection.LicenseOwner = "trialuser";
            ConfiguredFtpConnection.LicenseKey = "701-9435-3077-362";
            AfterDownloadAction = AfterDownloadRemoteFileActon.DeleteRemoteFile;
            _supplyingStarted = new EventWaitHandle(false, EventResetMode.AutoReset);
            _stopSupplying = false;
            
            // Set Polling IntervalInMinutes to the default
            PollingIntervalInMinutes = 1;
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
            _supplyingStarted = new EventWaitHandle(false, EventResetMode.AutoReset);
            _stopSupplying = false;
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

                FileActionManagerSupplierPathTags pathTags = 
                    new FileActionManagerSupplierPathTags(pFAMTM.FPSFileDir);

                _expandedLocalWorkingFolder = pathTags.Expand(LocalWorkingFolder);

                // Set the file target
                _fileTarget = pTarget;

                // Set the stop supplying flag to false
                _stopSupplying = false;
                
                // Start the supplying thread
                _ftpDownloadManagerThread = new Thread(ManageFileDownload);
                _ftpDownloadManagerThread.Start();

                _supplyingStarted.Set();
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
                _supplyingStarted.WaitOne();
                _stopSupplying = true;

                // Will need to put cancel logic in here
                // Wait for the thread to stop
                _ftpDownloadManagerThread.Join();
                _ftpDownloadManagerThread = null;
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
                    RemoteDownloadFolder = reader.ReadString();
                    FileExtensionsToDownload = reader.ReadString();
                    RecursivelyDownload = reader.ReadBoolean();
                    PollRemoteLocation = reader.ReadBoolean();
                    PollingIntervalInMinutes = reader.ReadInt32();
                    AfterDownloadAction = (AfterDownloadRemoteFileActon)reader.ReadInt32();
                    NewExtensionForRemoteFile = reader.ReadString();
                    LocalWorkingFolder = reader.ReadString();

                    string hexString = reader.ReadString();
                    using (MemoryStream ftpDataStream = new MemoryStream(hexString.ToByteArray()))
                    {
                        ConfiguredFtpConnection = new SecureFTPConnection();
                        ConfiguredFtpConnection.Load(ftpDataStream);
                    }
                    ConfiguredFtpConnection.LicenseOwner = "trialuser";
                    ConfiguredFtpConnection.LicenseKey = "701-9435-3077-362";
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
                    _supplyingStarted.Dispose();
                    _supplyingStarted = null;
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
                    // Add the local file that was just downloaded to the database
                    _fileTarget.NotifyFileAdded(e.LocalPath, this);
                    
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
                        File.Delete(e.LocalPath);
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
                using (SecureFTPConnection runningConnection = (SecureFTPConnection)ConfiguredFtpConnection.Clone())
                {
                    // Add event handler for when files are down downloading.
                    runningConnection.Downloaded += new FTPFileTransferEventHandler(HandleFileDownloaded);

                    // Get the list of files from the ftp server
                    runningConnection.Connect();
                    
                    // Get all the files and directories in the working folder and subfolders if required
                    FTPFile[] directoryContents = runningConnection.GetFileInfos(RemoteDownloadFolder, RecursivelyDownload);

                    // Create list to contain all of the files to be downloaded
                    List<FTPFile> filesToDownload = new List<FTPFile>();
                    
                    // Fill the filesToDownload list
                    DetermineFilesToDownload(runningConnection, directoryContents, filesToDownload, RecursivelyDownload);
                    
                    // Download the files
                    foreach (FTPFile f in filesToDownload)
                    {
                        // Check if suppling should be stopped
                        if (_stopSupplying)
                        {
                            return;
                        }

                        // Determine the current working folder on the ftp server
                        string currentWorkingDir = f.Path.Remove(f.Path.Length - f.Name.Length);

                        // Only change the working directory if it needs to be changed.
                        if (currentWorkingDir != runningConnection.ServerDirectory)
                        {
                            runningConnection.ChangeWorkingDirectory(currentWorkingDir);
                        }

                        // make sure the path exists on the local machine
                        string pathForFile = _expandedLocalWorkingFolder + "\\" + 
                            currentWorkingDir.Remove(0, RemoteDownloadFolder.Length);
                        pathForFile = pathForFile.Replace('/', '\\');
                        pathForFile = pathForFile.Replace("\\\\", "\\"); 
                        if (!Directory.Exists(pathForFile))
                        {
                            Directory.CreateDirectory(pathForFile);
                        }

                        // Determine the full name of the local file
                        string localFile = pathForFile + "\\" + f.Name;
                        localFile = localFile.Replace("\\\\", "\\");
                        runningConnection.DownloadFile(localFile, f.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI32197");
            }
            finally
            {
                // Suppling is finished
                _fileTarget.NotifyFileSupplyingDone(this);
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

                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32021");
            }
        }

        /// <summary>
        /// Determines which files in the dirContents should be downloaded and puts 
        /// them in the files list.
        /// </summary>
        /// <param name="runningConnection">Connection to the FTPp server</param>
        /// <param name="dirContents">Contents of a directory on the FTP server</param>
        /// <param name="files">List of files to download</param>
        /// <param name="recurseDir">if <see lang="true"/> directories will be recursed</param>
        void DetermineFilesToDownload(SecureFTPConnection runningConnection, FTPFile[] dirContents, 
            List<FTPFile> files, bool recurseDir)
        {
            // Filter the directory contents for files and sub directories
            foreach (FTPFile f in dirContents)
            {
                // if the FTPFile is a file add it to the files list
                if (FilesToDownloadFilter(f))
                {
                    files.Add(f);
                }
                // if the recursing directories and FTPFile is a directory
                else if (recurseDir && f.Dir  )
                {
                    DetermineFilesToDownload(runningConnection, f.Children, files, recurseDir);
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

        #endregion
    }
}
