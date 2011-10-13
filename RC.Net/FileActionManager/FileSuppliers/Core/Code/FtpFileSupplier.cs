using EnterpriseDT.Net.Ftp;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Ftp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileSuppliers
{
    /// <summary>
    /// Enum for whether the remote location should be polled for files and, if so, whether it
    /// should be polled continuously or at set times.
    /// </summary>
    [ComVisible(true)]
    [Guid("68EF6C79-CE78-44A6-A40A-86BA307F93BA")]
    public enum PollingMethod
    {
        /// <summary>
        /// Don't poll the server. Whatever files are available for download when the FAM is started
        /// are the only ones that will be downloaded.
        /// </summary>
        NoPolling = 0,

        /// <summary>
        /// Poll the server a regular intervals indefinitely.
        /// </summary>
        Continuously = 1,

        /// <summary>
        /// Poll the server at specific times of the day.
        /// </summary>
        SetTimes = 2,
    }

    /// <summary>
    /// Interface for the <see cref="FtpFileSupplier"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("98BD0513-3C87-43FD-BF53-D123FAD283E2")]
    [CLSCompliant(false)]
    public interface IFtpFileSupplier : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileSupplier, ILicensedComponent,
        IPersistStream, IFtpEventErrorSource, IDisposable
    {
        /// <summary>
        /// Folder on FTP site to download file from
        /// </summary>
        string RemoteDownloadFolder
        {
            get;
            set;
        }

        /// <summary>
        /// Extensions of files to download
        /// </summary>
        string FileExtensionsToDownload
        {
            get;
            set;
        }

        /// <summary>
        /// Flag indicating that all subfolders of the download folder should be searched
        /// for files to download
        /// </summary>
        bool RecursivelyDownload
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the remote location should be polled for files and, if so, whether it
        /// should be polled continuously or at set times.
        /// </summary>
        PollingMethod PollingMethod
        {
            get;
            set;
        }

        /// <summary>
        /// The interval in minutes between checks of the remote location for files. Only used if
        /// <see cref="PollingMethod"/> is PollingMethod.Continuously.
        /// </summary>
        Int32 PollingIntervalInMinutes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> values indicating the times of day that the
        /// server should be polled. Only used if <see cref="PollingMethod"/> is
        /// PollingMethod.SetTimes.
        /// </summary>
        // In order to export to COM, this cannot be a generic collection, such as IEnumerable.
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        DateTime[] PollingTimes
        {
            get;
            set;
        }

        /// <summary>
        /// Local folder that files are copied to when they are downloaded from
        /// the remote server
        /// </summary>
        string LocalWorkingFolder
        {
            get;
            set;
        }

        /// <summary>
        /// The object that contains all of the settings relevant to make a connection to 
        /// an ftp site
        /// </summary>
        SecureFTPConnection ConfiguredFtpConnection
        {
            get;
            set;
        }

        /// <summary>
        /// Number of connections used to download files
        /// </summary>
        int NumberOfConnections
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
    }

    /// <summary>
    /// A File supplier that will get files from a SFTP/FTP site
    /// </summary>
    [ComVisible(true)]
    [Guid("2D201AC7-8EE8-47D0-96B3-708F4E34435C")]
    [ProgId("Extract.FileActionManager.FileSuppliers.FTPFileSupplier")]
    public class FtpFileSupplier : IFtpFileSupplier
    {
        #region Constants

        /// <summary>
        /// The description of this file supplier
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Files from FTP site";

        /// <summary>
        /// Current file supplier version.
        /// <para>Version 4</para>
        /// Boolean for whether to poll became PollingMethod; also added PollingTimes
        /// <para>Version 5</para>
        /// Removed the after-download actions per [DotNetRCAndUtils:739]
        /// </summary>
        const int _CURRENT_VERSION = 5;

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

        /// <summary>
        /// Default wait time between retries
        /// </summary>
        const int _DEFAULT_WAIT_TIME_IN_MILLISECONDS_BETWEEN_RETRIES = 1000; 

        /// <summary>
        /// Default number of retries before failure
        /// </summary>
        const int _DEFAULT_NUMBER_OF_RETRIES_BEFORE_FAILURE = 10;

        /// <summary>
        /// Default number of retries before failure
        /// </summary>
        const int _CONSECUTIVE_FAILURES_BEFORE_RE_POLLING = 10;

        #endregion

        #region Fields

        // Whether the object is dirty or not.
        bool _dirty;

        // Target for the files that are being supplier
        IFileSupplierTarget _fileTarget;

        // Field for the RemoteDownloadFolder
        string _remoteDownloadFolder;

        // Field for the FileExtensionsToDownload property
        string _fileExtensionsToDownload;

        // This is a regular expression that is set by the 
        // FileExtensionsToDownload set operator and used to find the
        // the files to download
        string _fileExtensionsToDownloadRegEx;

        // Field for the RecursivelyDowload property
        bool _recursivelyDownload;

        // Indicates whether the remote location should be polled for files and, if so, whether it
        // should be polled continuously or at set times.
        PollingMethod _pollingMethod;

        // Field for PollingIntervalInMinutes property
        Int32 _pollingIntervalInMinutes;
        
        // Field for PollingTimes property
        DateTime[] _pollingTimes = new DateTime[0];

        // Field for LocalWorkingFolder property
        string _localWorkingfolder;

        // Field for NumberOfConnections property
        int _numberOfConnections;

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

        // Number of times to retry
        int _numberOfTimesToRetry = _DEFAULT_NUMBER_OF_RETRIES_BEFORE_FAILURE;

        // Time to wait between retries
        int _timeToWaitBetweenRetries = _DEFAULT_WAIT_TIME_IN_MILLISECONDS_BETWEEN_RETRIES;

        /// <summary>
        /// The <see cref="FileProcessingDB"/> in use.
        /// </summary>
        IFileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the action for which files are being queued.
        /// </summary>
        int _actionID = -1;

        /// <summary>
        /// Keeps track of how many consecutive files fail to download; DownloadFiles will be
        /// aborted if this number becomes > <see cref="NumberOfTimesToRetry"/>
        /// </summary>
        int _consecutiveDownloadFailures = 0;

        /// <summary>
        /// Allows for FTP operations to be automatically retried as configured.
        /// </summary>
        Retry<Exception> _retry;

        /// <summary>
        /// Protects access to creating/disposal of the _pollingTimer.
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Events
        
        /// <summary>
        /// Raised when an error occurs during an FTP operation.
        /// </summary>
        public event EventHandler<ExtractExceptionEventArgs> FtpError;

        #endregion Events

        #region Properties

        /// <summary>
        /// Folder on FTP site to download file from
        /// </summary>
        public string RemoteDownloadFolder
        {
            get
            {
                return _remoteDownloadFolder;
            }
            set
            {
                try
                {
                    if (_remoteDownloadFolder != value)
                    {
                        _remoteDownloadFolder = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33073");
                }
            }
        }

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
        public bool RecursivelyDownload 
        { 
            get
            {
                return _recursivelyDownload;
            }
            set
            {
                if (_recursivelyDownload != value)
                {
                    _recursivelyDownload = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Indicates whether the remote location should be polled for files and, if so, whether it
        /// should be polled continuously or at set times.
        /// </summary>
        public PollingMethod PollingMethod
        { 
            get
            {
                return _pollingMethod;
            }
            set
            {
                if (_pollingMethod != value)
                {
                    _pollingMethod = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// The interval in minutes between checks of the remote location for files. Only used if
        /// <see cref="PollingMethod"/> is PollingMethod.Continuously.
        /// </summary>
        public Int32 PollingIntervalInMinutes 
        { 
            get
            {
                return _pollingIntervalInMinutes;
            }
            set
            {
                if (_pollingIntervalInMinutes != value)
                {
                    _pollingIntervalInMinutes = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> values indicating the times of day that the
        /// server should be polled. Only used if <see cref="PollingMethod"/> is
        /// PollingMethod.SetTimes.
        /// </summary>
        public DateTime[] PollingTimes
        {
            get
            {
                return _pollingTimes;
            }

            set
            {
                try
                {
                    if (value != _pollingTimes)
                    {
                        // To make the displayed list of times as readable as possible, sort the times,
                        // remove duplicates, and make sure they all fall on Jan 1, 0001.
                        _pollingTimes = value
                            .Select(time => new DateTime(1, 1, 1, time.Hour, time.Minute, time.Second))
                            .Distinct()
                            .OrderBy(time => time)
                            .ToArray();

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33996");
                }
            }
        }

        /// <summary>
        /// Local folder that files are copied to when they are downloaded from
        /// the remote server
        /// </summary>
        public string LocalWorkingFolder 
        { 
            get
            {
                return _localWorkingfolder;
            }
            set
            {
                try
                {
                    if (_localWorkingfolder != value)
                    {
                        _localWorkingfolder = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33075");
                }
            }
        }

        /// <summary>
        /// The object that contains all of the settings relevant to make a connection to 
        /// an ftp site
        /// </summary>
        [CLSCompliant(false)]
        public SecureFTPConnection ConfiguredFtpConnection { get; set; }

        /// <summary>
        /// Number of connections used to download files
        /// </summary>
        public int NumberOfConnections 
        { 
            get
            {
                return _numberOfConnections;
            }
            set
            {
                if (_numberOfConnections != value)
                {
                    _numberOfConnections = value;
                    _dirty = true;
                }
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
        
        #endregion Properties
          
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFileSupplier"/>.
        /// </summary>
        public FtpFileSupplier()
        {
            ConfiguredFtpConnection = new SecureFTPConnection();

            FtpMethods.InitializeFtpApiLicense(ConfiguredFtpConnection);

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

        #endregion Constructors
        
        #region Methods

        /// <summary>
        /// Converts the specified text into an array of <see cref="DateTime"/> values. Used for
        /// settings the <see cref="PollingTimes"/> property.
        /// </summary>
        /// <param name="text">The text to be parsed into <see cref="DateTime"/> values.</param>
        /// <returns>The array of <see cref="DateTime"/> values.</returns>
        internal static DateTime[] ConvertTextToTimes(string text)
        {
            try
            {
                string[] entries = text.Split(new char[] { ';', ',' },
                    StringSplitOptions.RemoveEmptyEntries);

                DateTime[] times = new DateTime[entries.Length];

                for (int i = 0; i < entries.Length; i++)
                {
                    DateTime time;
                    if (!DateTime.TryParse(entries[i], CultureInfo.CurrentCulture,
                        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out time))
                    {
                        throw new ExtractException("ELI33992",
                            "Failed to parse time value: \"" + entries[i] + "\"");
                    }

                    times[i] = time;
                }

                return times;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33993");
            }
        }

        /// <summary>
        /// Gets the <see cref="PollingTimes"/> property as a string list.
        /// </summary>
        /// <returns></returns>
        internal string GetPollingTimesAsText()
        {
            try 
	        {	        
		        string text = String.Join(", ", PollingTimes
                    .Select(time => time.ToString("t", CultureInfo.CurrentCulture)));

                return text;
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI33994");
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

        #endregion ICategorizedComponent Members

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

        #endregion IConfigurableObject Members

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
                    RemoteDownloadFolder[0] != '.' &&
                    !string.IsNullOrWhiteSpace(LocalWorkingFolder) && 
                    !string.IsNullOrWhiteSpace(FileExtensionsToDownload) &&
                    (PollingMethod != PollingMethod.Continuously || PollingIntervalInMinutes > 0) &&
                    (PollingMethod != PollingMethod.SetTimes || PollingTimes.Length > 0) &&
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

        #endregion IMustBeConfigured Members

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

        #endregion ICopyableObject Members

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
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="nActionID">The ID of the action for which files are being queued.</param>
        [CLSCompliant(false)]
        public void Start(IFileSupplierTarget pTarget, FAMTagManager pFAMTM, FileProcessingDB pDB,
            int nActionID)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_licenseId,
                   "ELI32216", _COMPONENT_DESCRIPTION);

                _fileProcessingDB = pDB;
                _actionID = nActionID;

                // Create retry object
                _retry = new Retry<Exception>(NumberOfTimesToRetry, TimeToWaitBetweenRetries, _stopSupplying);

                // Notify the FtpEventRecorder to recheck the DBInfo setting for whether to log FTP events
                FtpEventRecorder.RecheckFtpLoggingStatus();

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

                _fileProcessingDB = null;
                _actionID = -1;

                // Clear out any remaining files in the files to download queue
                _filesToDownload = new ConcurrentQueue<FTPFile>();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32001", "Unable to stop supplying object.");
            }
        }

        #endregion IFileSupplier Members

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
                    if (reader.Version < 4)
                    {
                        PollingMethod = reader.ReadBoolean()
                            ? PollingMethod.Continuously
                            : PollingMethod.NoPolling;
                    }
                    else
                    {
                        PollingMethod = (PollingMethod)reader.ReadInt32();
                    }
                    PollingIntervalInMinutes = reader.ReadInt32();
                    if (reader.Version < 5)
                    {
                        // Ignore removed after-download action settings.
                        reader.ReadInt32();
                        reader.ReadString();
                    }
                    LocalWorkingFolder = reader.ReadString();

                    if (reader.Version >= 2)
                    {
                        NumberOfConnections = reader.ReadInt32();
                    }

                    if (reader.Version >= 3)
                    {
                        NumberOfTimesToRetry = reader.ReadInt32();
                        TimeToWaitBetweenRetries = reader.ReadInt32();
                    }

                    if (reader.Version >= 4)
                    {
                        PollingTimes = reader.ReadStructArray<DateTime>();
                    }

                    string hexString = reader.ReadString();
                    using (MemoryStream ftpDataStream = new MemoryStream(hexString.ToByteArray()))
                    {
                        ConfiguredFtpConnection = new SecureFTPConnection();
                        ConfiguredFtpConnection.Load(ftpDataStream);
                    }
                    FtpMethods.InitializeFtpApiLicense(ConfiguredFtpConnection);
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
                    writer.Write((int)PollingMethod);
                    writer.Write(PollingIntervalInMinutes);
                    writer.Write(LocalWorkingFolder);
                    writer.Write(NumberOfConnections);
                    writer.Write(NumberOfTimesToRetry);
                    writer.Write(TimeToWaitBetweenRetries);
                    writer.Write(PollingTimes);

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

        #endregion IPersistStream Members

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

        #region Event Handlers

        /// <summary>
        /// Handles the PollingTimer timeout event, it will cause the ManageFileDownload thread
        /// to connect to the ftp server and get a listing of files to download.  If the 
        /// ManageFileDownload thread is already connected to the ftp server this will cause it
        /// to connect to the server again when ever it finishes.
        /// </summary>
        /// <param name="o">Object the triggered the event</param>
        void HandlePollingTimerTimeout(object o)
        {
            try
            {
                _pollFtpServerForFiles.Set();

                // If using set times, rather than use the same interval, every time the timer fires we
                // need to re-calculate the time interval until the next set time, and reset
                // _pollingTimer accordingly.
                if (PollingMethod == FileSuppliers.PollingMethod.SetTimes)
                {
                    lock (_lock)
                    {
                        System.Threading.Timer oldPollingTimer = _pollingTimer;

                        long timeoutValue = GetMillisecondsUntilNextSetTime();

                        _pollingTimer = new System.Threading.Timer(HandlePollingTimerTimeout, null,
                            timeoutValue, timeoutValue);

                        oldPollingTimer.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33997");
            }
        }

        #endregion Event Handlers

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
                // If polling at set times, don't poll right away; wait until the next set time.
                if (PollingMethod == FileSuppliers.PollingMethod.SetTimes && ExitManageFileDownload())
                {
                    return;
                }

                do
                {
                    _pollFtpServerForFiles.Reset();
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
            // Reset _consecutiveDownloadFailures before launching new DownloadFiles threads.
            _consecutiveDownloadFailures = 0;

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
                if (PollingMethod == PollingMethod.NoPolling)
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
                    FtpMethods.InitializeFtpApiLicense(runningConnection);

                    FTPFile currentFtpFile;

                    // There are retry settings for the file supplier that will allow quicker exiting if
                    // the stop button is pressed on the FAM so change the connection to not retry.
                    runningConnection.RetryCount = 0;

                    // Download the files
                    while (GetNextFileToDownload(out currentFtpFile))
                    {
                        try
                        {
                            // Call the DownloadFileFromFtpServer retry within a FtpEventRecorder
                            // block so that an FTP event history row will be added upon success or
                            // after all retries have been exhausted.
                            using (FtpEventRecorder recorder = new FtpEventRecorder(this,
                                runningConnection, _fileProcessingDB, _actionID, true,
                                EFTPAction.kDownloadFileFromFtpServer))
                            {
                                // Assign the FileRecord for the recorder so that it knows which
                                // file the FTP event relates to.
                                recorder.FileRecord = _retry.DoRetry(() =>
                                    DownloadFileFromFtpServer(runningConnection, currentFtpFile));

                                // If we got here, the file successfullly download; reset
                                // _consecutiveDownloadFailures.
                                Interlocked.Exchange(ref _consecutiveDownloadFailures, 0);
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractLog("ELI33977");

                            // If at least _CONSECUTIVE_FAILURES_BEFORE_RE_POLLING files have failed
                            // since the last one was succesfully downloaded and the polling timer 
                            // has fired since the last poll, re-poll before trying anymore downloads
                            // in case the files in our list are no longer available on the server.
                            if (Interlocked.Increment(ref _consecutiveDownloadFailures)
                                    > _CONSECUTIVE_FAILURES_BEFORE_RE_POLLING &&
                                _pollingTimer != null && _pollFtpServerForFiles.WaitOne(0))
                            {
                                // Clear out the existing queue so that a new list is generated from
                                // scratch on the next poll.
                                FTPFile temp;
                                while (_filesToDownload.TryDequeue(out temp));

                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI32371");
            }
        }

        #endregion Thread Functions

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID File Suppliers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileSuppliersGuid);
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
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileSuppliersGuid);
        }

        /// <summary>
        /// Copies settings from the given file suppler
        /// </summary>
        /// <param name="fileSupplier">The FtpFileSupplier to copy setttings from </param>
        void CopyFrom(FtpFileSupplier fileSupplier)
        {
            try
            {
                RemoteDownloadFolder = fileSupplier.RemoteDownloadFolder;
                FileExtensionsToDownload = fileSupplier.FileExtensionsToDownload;
                RecursivelyDownload = fileSupplier.RecursivelyDownload;
                PollingMethod = fileSupplier.PollingMethod;
                PollingIntervalInMinutes = fileSupplier.PollingIntervalInMinutes;
                PollingTimes = fileSupplier.PollingTimes;
                LocalWorkingFolder = fileSupplier.LocalWorkingFolder;
                ConfiguredFtpConnection = (SecureFTPConnection)fileSupplier.ConfiguredFtpConnection.Clone();
                NumberOfConnections = fileSupplier.NumberOfConnections;
                NumberOfTimesToRetry = fileSupplier.NumberOfTimesToRetry;
                TimeToWaitBetweenRetries = fileSupplier.TimeToWaitBetweenRetries;

                FtpMethods.InitializeFtpApiLicense(ConfiguredFtpConnection);
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
        /// <param name="dirContents">Contents of a directory on the FTP server</param>
        void DetermineFilesToDownload(FTPFile[] dirContents)
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
                else if (RecursivelyDownload && file.Dir)
                {
                    DetermineFilesToDownload(file.Children);
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
                    FtpMethods.InitializeFtpApiLicense(runningConnection);

                    // There are retry settings for the file supplier that will allow quicker exiting if
                    // the stop button is pressed on the FAM so change the connection to not retry.

                    FTPFile[] directoryContents = _retry.DoRetry(() =>
                        GetFileListFromFtpServer(runningConnection));

                    // Fill the filesToDownload list
                    DetermineFilesToDownload(directoryContents);
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
        /// Gets a list of files and directories from the ftp server using the runningConnection
        /// </summary>
        /// <param name="runningConnection">Connection to the ftp server.</param>
        /// <returns>Array of FTPFile objects representing the files and directories on the ftp server</returns>
        private FTPFile[] GetFileListFromFtpServer(SecureFTPConnection runningConnection)
        {
            if (!runningConnection.IsConnected)
            {
                // Connect to the ftp server
                runningConnection.Connect();
            }

            // Get all the files and directories in the working folder and subfolders if required
            FTPFile[] directoryContents = runningConnection.GetFileInfos(RemoteDownloadFolder, RecursivelyDownload);
            return directoryContents;
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
        /// Intializes properties and fields to initial default values
        /// </summary>
        void InitializeDefaults()
        {
            // Set Polling IntervalInMinutes to the default
            PollingIntervalInMinutes = 1;
            PollingTimes = new DateTime[0];
            NumberOfConnections = 1;
            NumberOfTimesToRetry = _DEFAULT_NUMBER_OF_RETRIES_BEFORE_FAILURE;
            TimeToWaitBetweenRetries = _DEFAULT_WAIT_TIME_IN_MILLISECONDS_BETWEEN_RETRIES;
        }

        /// <summary>
        /// Stops polling if polling is active
        /// </summary>
        void StopPolling()
        {
            lock (_lock)
            {
                if (_pollingTimer != null)
                {
                    _pollingTimer.Dispose();
                    _pollingTimer = null;
                }
            }
        }

        /// <summary>
        /// Starts a polling timer if polling is enabled
        /// </summary>
        void StartPolling()
        {
            if (PollingMethod != FileSuppliers.PollingMethod.NoPolling)
            {
                Int64 timeoutValue;

                if (PollingMethod == PollingMethod.Continuously)
                {
                    timeoutValue = (Int64)PollingIntervalInMinutes * 60 * 1000;
                }
                else // (PollingMethod == PollingMethod.SetTimes)
                {
                    timeoutValue = GetMillisecondsUntilNextSetTime();
                }

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

        /// <summary>
        /// Download a file from the ftp server
        /// </summary>
        /// <param name="runningConnection">Connection to the ftp server</param>
        /// <param name="currentFtpFile">FTPFile object that has info for downloading the file</param>
        /// <returns>The <see cref="IFileRecord"/> associated with the dowloaded file if the
        /// download and supplying succeeded; otherwise <see langword="null"/>.</returns>
        private IFileRecord DownloadFileFromFtpServer(SecureFTPConnection runningConnection,
            FTPFile currentFtpFile)
        {
            string localFile = "";

            try
            {
                // The FileRecord will be set inside of the runningConnection.Downloaded delegate
                // if the download and supplying succeeds.
                IFileRecord fileRecord = null;

                // If not already connected connect to the FTP Server
                if (!runningConnection.IsConnected)
                {
                    runningConnection.Connect();
                }

                FtpMethods.SetCurrentFtpWorkingFolder(runningConnection, currentFtpFile.Path);
                string localFilePath = FtpMethods.GenerateLocalPathCreateIfNotExists(
                    runningConnection.ServerDirectory,
                    _expandedLocalWorkingFolder,
                    RemoteDownloadFolder);

                // Determine the full name of the local file
                localFile = Path.Combine(localFilePath, currentFtpFile.Name);

                string infoFileName = localFile + ".info";

                if (File.Exists(localFile) && File.Exists(infoFileName))
                {
                    FtpDownloadedFileInfo currentDownloadedFileInfo = new FtpDownloadedFileInfo(infoFileName);
                    currentDownloadedFileInfo.Load();

                    if (currentDownloadedFileInfo.RemoteFileSize == currentFtpFile.Size &&
                        currentDownloadedFileInfo.RemoteLastModifiedTime == currentFtpFile.LastModified)
                    {
                        // The file does not need to be downloaded but it should be added to the
                        // database again if required
                        fileRecord = _fileTarget.NotifyFileAdded(localFile, this);

                        return fileRecord;
                    }
                }

                // Define the runningConnection.Downloaded delegate in this scope so that it can set
                // the fileRecord variable.
                FTPFileTransferEventHandler handleDownloaded = ((sender, e) =>
                {
                    try
                    {
                        if (e.Succeeded)
                        {
                            // Verify that the files is exists localy
                            if (File.Exists(e.LocalPath) && e.LocalFileSize == e.RemoteFileSize)
                            {
                                // Add the local file that was just downloaded to the database
                                fileRecord = _fileTarget.NotifyFileAdded(e.LocalPath, this);
                            }
                            else
                            {
                                ExtractException ee = new ExtractException("ELI32311", "File was not downloaded.");
                                ee.AddDebugData("LocalFile", e.LocalPath, false);
                                ee.AddDebugData("RemoteFile", e.RemoteFile, false);
                                ee.AddDebugData("LocalFileSize", e.LocalFileSize, false);
                                ee.AddDebugData("RemoteFileSize", e.RemoteFileSize, false);
                                throw ee;
                            }
                        }
                        else
                        {
                            ExtractException ee;

                            // The download may have failed due to the file existing in the local folder
                            // and so the file may not have been changed. Don't want to delete it 
                            // since that could delete a file that existed before the download attempt.
                            if (File.Exists(e.LocalPath))
                            {
                                ee = new ExtractException("ELI32323",
                                    "File may have been partially downloaded", e.Exception);
                            }
                            else
                            {
                                ee = new ExtractException("ELI33989", "Download failed", e.Exception);
                            }

                            ee.AddDebugData("Local File", e.LocalPath, false);
                            ee.AddDebugData("Remote File", e.RemoteFile, false);
                            throw ee;
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtractException ee = ex.AsExtract("ELI32142");
                        ee.AddDebugData("Remote File", e.RemoteFile, false);

                        // In most cases we don't want throw from an event handler, but in this case
                        // we know this event is part of the runningConnection.DownloadFile() call.
                        // We want that call to throw this exception if the download did not
                        // properly complete.
                        throw ee;
                    }
                });

                try
                {
                    runningConnection.Downloaded += handleDownloaded;

                    runningConnection.DownloadFile(localFile, currentFtpFile.Name);
                }
                catch (Exception ex)
                {
                    // Raise an FTP error (as part of the IFtpEventExceptionSource implementation)
                    OnFtpError(ex.AsExtract("ELI33978"));
                    throw;
                }
                finally
                {
                    runningConnection.Downloaded -= handleDownloaded;
                }

                // Make sure the local file exists ( the dowload may have failed. in some way)
                if (File.Exists(localFile))
                {
                    FtpDownloadedFileInfo ftpInfoFile = new FtpDownloadedFileInfo(infoFileName,
                        currentFtpFile);
                    ftpInfoFile.Save();
                }

                return fileRecord;
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
                throw ee;
            }
            finally
            {
                CloseConnectionIfPausedAndWaitForResume(runningConnection);
            }
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

        /// <summary>
        /// Gets the number of milliseconds until the next time in PollingTimes.
        /// </summary>
        /// <returns>The number of milliseconds until the next time in PollingTimes</returns>
        long GetMillisecondsUntilNextSetTime()
        {
            long timeoutValue = (long)PollingTimes
                .Select(time => time.TimeOfDay - DateTime.Now.TimeOfDay)
                .Select(timeSpan => (timeSpan.Ticks < 0)
                    ? timeSpan + new TimeSpan(1, 0, 0, 0) // If we have passed the time; use tomorrow.
                    : timeSpan)
                .OrderBy(timeSpan => timeSpan)
                .First()
                .TotalMilliseconds;

            return timeoutValue;
        }

        #endregion Private Members
    }
}
