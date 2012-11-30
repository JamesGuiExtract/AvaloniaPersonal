using EnterpriseDT.Net.Ftp;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Utilities.Ftp
{
    /// <summary>
    /// Logs an FTP event to the FAM database FTPEventHistory table.
    /// </summary>
    [CLSCompliant(false)]
    public class FtpEventRecorder : IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FtpEventRecorder).ToString();

        /// <summary>
        /// The name of the DBInfo setting the defines whether FTP event history should be tracked.
        /// </summary>
        static readonly string _STORE_FTP_EVENT_HISTORY = "StoreFTPEventHistory";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="IFtpEventErrorSource"/> that will provide notification of FTP errors that
        /// have occured.
        /// </summary>
        IFtpEventErrorSource _ftpExceptionSource;

        /// <summary>
        /// The <see cref="SecureFTPConnection"/> which is performing the FTP event to be logged.
        /// </summary>
        SecureFTPConnection _ftpConnection;

        /// <summary>
        /// The <see cref="IFileProcessingDB"/> to which the FTP event should be logged.
        /// </summary>
        IFileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the action in the <see cref="_fileProcessingDB"/> to which this FTP event
        /// relates.
        /// </summary>
        int _actionID = -1;

        /// <summary>
        /// <see langword="true"/> if this FTP event relates to queuing, <see langword="false"/> if
        /// it relates to processing.
        /// </summary>
        bool _queuing;

        /// <summary>
        /// The <see cref="EFTPAction"/> of this FTP event.
        /// </summary>
        EFTPAction _ftpAction;

        /// <summary>
        /// Argument with path of the local file or to the new name to assign to a remote file that
        /// is being renamed.
        /// </summary>
        string _localOrNewArgument;

        /// <summary>
        /// Argument with path of the remote file or to the old name of a remote file that is being
        /// renamed. (<see langword="null"/> if only one argument is being used).
        /// </summary>
        string _remoteOrOldArgument;

        /// <summary>
        /// The <see cref="FTPEventArgs"/> that describes this FTP event.
        /// </summary>
        FTPEventArgs _ftpEvent;

        /// <summary>
        /// The last FTP command sent to the server as part of the recorded operation (outside of
        /// "QUIT").
        /// </summary>
        string _lastFtpCommand;

        /// <summary>
        /// The <see cref="IFileRecord"/> to which this FTP event relates.
        /// </summary>
        IFileRecord _fileRecord;

        /// <summary>
        /// An <see cref="Exception"/> that describes an error that occured during the FTP operation
        /// or <see langword="null"/> if there was no error.
        /// </summary>
        Exception _eventException;

        /// <summary>
        /// The number of retries of the FTP operation that have been performed (whether or not it
        /// eventually succeeded).
        /// </summary>
        int _retryCount = 0;

        /// <summary>
        /// Indicates whether FTP history logging is enabled via the database DBInfo table.
        /// </summary>
        static bool? _ftpEventHistoryEnabled;

        /// <summary>
        /// Protects access to _ftpEventHistoryEnabled.
        /// </summary>
        static object _lock = new object();

        /// <summary>
        /// Indicates whether this instance is actively tracking FTP events.
        /// </summary>
        bool _active;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpEventRecorder"/> class.
        /// <param><b>Note</b></param>
        /// The lifetime of each <see cref="FtpEventRecorder"/> should encapsulate all FTP code that
        /// pertains to one conceptually operation that should be logged. This may include several
        /// distinct operations/events in sequence such as logging in and changing directories
        /// before performing the primary operation The first time an event succeeds following an
        /// error will be considered the start of a retry. Multiple retries of the operation can be
        /// included an instance of this class, but once the operation <see cref="Dispose()"/>
        /// should be called to trigger the event to be logged.
        /// </summary>
        /// <param name="ftpExceptionSource">The <see cref="IFtpEventErrorSource"/> that will
        /// provide notification of FTP errors that have occured.</param>
        /// <param name="ftpConnection">The <see cref="SecureFTPConnection"/> which is performing
        /// the FTP event to be logged.</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> to which the FTP
        /// event should be logged.</param>
        /// <param name="actionID">The ID of the action in the <see paramref="fileProcessingDB"/> to
        /// which this FTP event relates.</param>
        /// <param name="queuing"><see langword="true"/> if this FTP event relates to queuing,
        /// <see langword="false"/> if it relates to processing.</param>
        /// <param name="ftpAction">The <see cref="EFTPAction"/> of this FTP event.</param>
        /// <param name="localOrNewArgument">Argument with path of the local file or to the new
        /// name to assign to a remote file that is being renamed.</param>
        /// <param name="remoteOrOldArgument">Argument with path of the remote file or to the old
        /// name of a remote file that is being renamed. (<see langword="null"/> if only one
        /// argument is being used).</param>
        /// <param name="fileRecord">The <see cref="IFileRecord"/> to which this FTP event relates
        /// or <see langword="null"/> it has not yet been associated with one.</param>
        public FtpEventRecorder(IFtpEventErrorSource ftpExceptionSource,
            SecureFTPConnection ftpConnection, IFileProcessingDB fileProcessingDB, int actionID,
            bool queuing, EFTPAction ftpAction, string localOrNewArgument,
            string remoteOrOldArgument, FileRecord fileRecord = null)
        {
            try
            {
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI33965", _OBJECT_NAME);

                // If needed, retrieve and cache the setting indicating whether logging should occur.
                if (_ftpEventHistoryEnabled == null)
                {
                    lock (_lock)
                    {
                        if (_ftpEventHistoryEnabled == null)
                        {
                            var settings = fileProcessingDB.DBInfoSettings;
                            _ftpEventHistoryEnabled =
                                (settings.GetValue(_STORE_FTP_EVENT_HISTORY) == "1");
                        }
                    }
                }

                // If logging should occur, set the member fields and subscript to receive FTP events
                // from the ftpExceptionSource and ftpConnection.
                if (_ftpEventHistoryEnabled.Value)
                {
                    IgnoreEvent = false;
                    _ftpExceptionSource = ftpExceptionSource;
                    _ftpConnection = ftpConnection;
                    _fileProcessingDB = fileProcessingDB;
                    _actionID = actionID;
                    _queuing = queuing;
                    _ftpAction = ftpAction;
                    _localOrNewArgument = localOrNewArgument;
                    _remoteOrOldArgument = remoteOrOldArgument;
                    _fileRecord = fileRecord;

                    RegisterEvents();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33966");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FtpEventRecorder"/> is actively
        /// tracking FTP events.
        /// </summary>
        /// <value><see langword="true"/> if active; otherwise, <see langword="false"/>.
        /// </value>
        public bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                try
                {
                    if (_active != value)
                    {
                        if (!value)
                        {
                            UnregisterEvents();
                        }
                        else if (_ftpEventHistoryEnabled != null && _ftpEventHistoryEnabled.Value)
                        {
                            RegisterEvents();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34087");
                }
            }
        }

        /// <summary>
        /// Gets or sets the file <see cref="IFileRecord"/> associated with this FTP event.
        /// </summary>
        /// <value>
        /// The file <see cref="IFileRecord"/> associated with this FTP event.
        /// </value>
        public IFileRecord FileRecord
        {
            get
            {
                return _fileRecord;
            }

            set
            {
                _fileRecord = value;
            }
        }

        /// <summary>
        /// Gets or sets the argument with path of the local file or to the new name to assign to a
        /// remote file that is being renamed.
        /// </summary>
        public string LocalOrNewArgument
        {
            get
            {
                return _localOrNewArgument;
            }

            set
            {
                _localOrNewArgument = value;
            }
        }

        /// <summary>
        /// Gets or sets the argument with path of the remote file or to the old name of a remote
        /// file that is being renamed. (<see langword="null"/> if only one argument is being used).
        /// </summary>
        public string RemoteOrOldArgument
        {
            get
            {
                return _remoteOrOldArgument;
            }

            set
            {
                _remoteOrOldArgument = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the FTP operation this instance is tracking should be ignored (not
        /// logged to the database).
        /// </summary>
        /// <value><see langword="true"/> to ignore the event; <see langword="false"/> to log it.
        /// </value>
        public bool IgnoreEvent
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Causes the next <see cref="FtpEventRecorder"/> instance created to recheck the DBInfo
        /// setting indicating whether logging should occur.
        /// </summary>
        public static void RecheckFtpLoggingStatus()
        {
            try
            {
                _ftpEventHistoryEnabled = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33985");
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles when an FTP command is sent.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EnterpriseDT.Net.Ftp.FTPMessageEventArgs"/> instance
        /// containing the event data.</param>
        void HandleFTPCommandSent(object sender, FTPMessageEventArgs e)
        {
            try
            {
                if (!e.Message.EndsWith("QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    _lastFtpCommand = e.Message;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34095");
            }
        }

        /// <summary>
        /// Handles an FTP event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EnterpriseDT.Net.Ftp.FTPEventArgs"/>
        /// instance containing the event data.</param>
        void HandleFtpEvent(object sender, FTPEventArgs e)
        {
            try
            {
                // update _ftpEvent each time as the eventual FTPEventHistory row should pertain to
                // the last event performed (whether successful or not).
                _ftpEvent = e;

                if (_eventException != null)
                {
                    _retryCount++;
                    _eventException = null;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33967");
            }
        }

        /// <summary>
        /// Handles an FTP error reported by the <see cref="_ftpConnection"/>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EnterpriseDT.Net.Ftp.FTPErrorEventArgs"/> instance
        /// containing the event data.</param>
        void HandleError(object sender, FTPErrorEventArgs e)
        {
            try
            {
                _eventException = e.Exception;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33972");
            }
        }

        /// <summary>
        /// Handles an FTP error reported by the <see cref="_ftpExceptionSource"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.ExtractExceptionEventArgs"/> instance containing
        /// the event data.</param>
        void HandleFtpErrorSourceError(object sender, ExtractExceptionEventArgs e)
        {
            try
            {
                _eventException = e.Exception;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33983");
            }
        }

        #endregion Event Handlers

        #region IDisposable Members

        /// <overloads>
        /// Releases resources used by the <see cref="FtpEventRecorder"/>
        /// </overloads>
        /// <summary>
        /// Releases resources used by the <see cref="FtpEventRecorder"/>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the <see cref="FtpEventRecorder"/>.
        /// <para><b>Note</b></para>
        /// This call is what results in the FTP event being logged to the database.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // Stop watching for FTP events (will have registered if _ftpConnection was set).
                    if (_ftpConnection != null)
                    {
                        UnregisterEvents();
                    }

                    RecordFtpEvent();

                    _ftpConnection = null;
                    _fileProcessingDB = null;
                    _ftpEvent = null;
                }
                catch (Exception ex)
                {
                    new ExtractException("ELI33981", "Error recording FTP event history.", ex).Log();
                }    
            }
        }

        /// <summary>
        /// Records the FTP event to the FTPEventHistory database table.
        /// </summary>
        void RecordFtpEvent()
        {
            // If an FTP event was tracked, log it to the FTPEventHistory table.
            if (!IgnoreEvent && _fileProcessingDB != null && _ftpEvent != null)
            {
                // Assign argument1 and argument2 depending on the type of event.
                string argument1 = null;
                string argument2 = null;
                GetEventArguments(ref argument1, ref argument2);

                int fileId = (_fileRecord == null) ? -1 : _fileRecord.FileID;
                int actionId = _actionID;
                string userName = _ftpConnection.UserName;
                string serverAddress = _ftpConnection.ServerAddress;

                // If there is an exception (the operation did not succeed), collect debug data
                // regarding the current event.
                string exceptionString = null;
                if (_eventException != null)
                {
                    Dictionary<string, string> eventData = GetEventData();

                    ExtractException ee = _eventException.AsExtract("ELI33980");
                    foreach (KeyValuePair<string, string> entry in eventData
                        .Where(entry => !string.IsNullOrWhiteSpace(entry.Value)))
                    {
                        ee.AddDebugData(entry.Key, entry.Value, false);
                    }

                    exceptionString = ee.CreateLogString();
                }

                _fileProcessingDB.RecordFTPEvent(fileId, actionId, _queuing, _ftpAction,
                    serverAddress, userName, argument1, argument2, _retryCount, exceptionString);
            }
        }

        /// <summary>
        /// Assigns <see cref="LocalOrNewArgument"/> and <see cref="RemoteOrOldArgument"/> to
        /// <see paramref="argument1"/> and <see paramref="argument2"/> as appropriate for
        /// <see cref="_ftpAction"/>.
        /// </summary>
        /// <param name="argument1">The value that will populate Arg1 in the database.</param>
        /// <param name="argument2">The value that will populate Arg2 in the database.</param>
        void GetEventArguments(ref string argument1, ref string argument2)
        {
            switch (_ftpAction)
            {
                case EFTPAction.kGetDirectoryListing:
                    {
                        argument1 = _remoteOrOldArgument;

                        var listingEvent = _ftpEvent as FTPDirectoryListEventArgs;
                        if (listingEvent != null && listingEvent.FileInfos != null)
                        {
                            argument2 =
                                listingEvent.FileInfos.Length.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    break;

                case EFTPAction.kUploadFileToFtpServer:
                    {
                        argument1 = _localOrNewArgument;
                        argument2 = _remoteOrOldArgument;
                    }
                    break;

                case EFTPAction.kDownloadFileFromFtpServer:
                    {
                        argument1 = _remoteOrOldArgument;
                        argument2 = _localOrNewArgument;
                    }
                    break;

                case EFTPAction.kRenameFileOnFtpServer:
                    {
                        argument1 = _remoteOrOldArgument;
                        argument2 = _localOrNewArgument;
                    }
                    break;

                case EFTPAction.kDeleteFileFromFtpServer:
                    {
                        argument1 = _remoteOrOldArgument;
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets a dictionary of pertinent data for the FTP event that was being tracked.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetEventData()
        {
            Dictionary<string, string> eventData = new Dictionary<string, string>();

            eventData.Add("Command", _lastFtpCommand);

            string eventType = _ftpEvent.GetType().ToString();
            if (eventType.Contains('.'))
            {
                eventType = eventType.Substring(eventType.LastIndexOf('.') + 1);
                eventType = eventType.Replace("Args", "");
                eventData.Add("EventType", eventType);
            }

            if (GetDataFromFTPFileTransferEventArgs(ref eventData))
            { }
            else if (GetDataFromFTPFileRenameEventArgs(ref eventData))
            { }
            else if (GetDataFromFTPDirectoryEventArgs(ref eventData))
            { }
            else if (GetDataFromFTPDirectoryListingEventArgs(ref eventData))
            { }
            else if (GetDataFromFTPConnectionEventArgs(ref eventData))
            { }
            else
            {
                GetDataFromFTPLogInEventArgs(ref eventData);
            }

            return eventData;
        }

        /// <summary>
        /// Retrieves data from <see cref="_ftpEvent"/> as a <see cref="FTPFileTransferEventArgs"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="Dictionary{T, T}"/> to add the data to.
        /// </param>
        /// <returns><see langword="true"/> if <see cref="_ftpEvent"/> were able to be cast to
        /// <see cref="FTPFileTransferEventArgs"/>; <see langword="false"/> otherwise.</returns>
        bool GetDataFromFTPFileTransferEventArgs(ref Dictionary<string, string> eventData)
        {
            var fileTransferEvent = _ftpEvent as FTPFileTransferEventArgs;
            if (fileTransferEvent != null)
            {
                eventData.Add("Local filename", fileTransferEvent.LocalPath);
                eventData.Add("Remote filename", fileTransferEvent.RemotePath);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves data from <see cref="_ftpEvent"/> as a <see cref="FTPFileRenameEventArgs"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="Dictionary{T, T}"/> to add the data to.
        /// </param>
        /// <returns><see langword="true"/> if <see cref="_ftpEvent"/> were able to be cast to
        /// <see cref="FTPFileRenameEventArgs"/>; <see langword="false"/> otherwise.</returns>
        bool GetDataFromFTPFileRenameEventArgs(ref Dictionary<string, string> eventData)
        {
            var fileRenameEvent = _ftpEvent as FTPFileRenameEventArgs;
            if (fileRenameEvent != null)
            {
                eventData.Add("Old filename", fileRenameEvent.OldFilePath);
                eventData.Add("New filename", fileRenameEvent.NewFileName);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves data from <see cref="_ftpEvent"/> as a <see cref="FTPDirectoryEventArgs"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="Dictionary{T, T}"/> to add the data to.
        /// </param>
        /// <returns><see langword="true"/> if <see cref="_ftpEvent"/> were able to be cast to
        /// <see cref="FTPDirectoryEventArgs"/>; <see langword="false"/> otherwise.</returns>
        bool GetDataFromFTPDirectoryEventArgs(ref Dictionary<string, string> eventData)
        {
            var directoryEvent = _ftpEvent as FTPDirectoryEventArgs;
            if (directoryEvent != null)
            {
                eventData.Add("Old directory", directoryEvent.OldDirectoryPath);
                eventData.Add("Remote directory", directoryEvent.NewDirectoryPath);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves data from <see cref="_ftpEvent"/> as a <see cref="FTPDirectoryListEventArgs"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="Dictionary{T, T}"/> to add the data to.
        /// </param>
        /// <returns><see langword="true"/> if <see cref="_ftpEvent"/> were able to be cast to
        /// <see cref="FTPDirectoryListEventArgs"/>; <see langword="false"/> otherwise.</returns>
        bool GetDataFromFTPDirectoryListingEventArgs(ref Dictionary<string, string> eventData)
        {
            var listingEvent = _ftpEvent as FTPDirectoryListEventArgs;

            if (listingEvent != null)
            {
                eventData.Add("Remote directory", listingEvent.DirectoryPath);
                eventData.Add("Contents count", (listingEvent.FileInfos == null)
                    ? ""
                    : listingEvent.FileInfos.Length.ToString(CultureInfo.InvariantCulture));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves data from <see cref="_ftpEvent"/> as a <see cref="FTPConnectionEventArgs"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="Dictionary{T, T}"/> to add the data to.
        /// </param>
        /// <returns><see langword="true"/> if <see cref="_ftpEvent"/> were able to be cast to
        /// <see cref="FTPConnectionEventArgs"/>; <see langword="false"/> otherwise.</returns>
        bool GetDataFromFTPConnectionEventArgs(ref Dictionary<string, string> eventData)
        {
            var connectionEvent = _ftpEvent as FTPConnectionEventArgs;

            if (connectionEvent != null)
            {
                eventData.Add("Server address", connectionEvent.ServerAddress);
                eventData.Add("Server port",
                    connectionEvent.ServerPort.ToString(CultureInfo.InvariantCulture));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves data from <see cref="_ftpEvent"/> as a <see cref="FTPLogInEventArgs"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="Dictionary{T, T}"/> to add the data to.
        /// </param>
        /// <returns><see langword="true"/> if <see cref="_ftpEvent"/> were able to be cast to
        /// <see cref="FTPLogInEventArgs"/>; <see langword="false"/> otherwise.</returns>
        bool GetDataFromFTPLogInEventArgs(ref Dictionary<string, string> eventData)
        {
            var loginEvent = _ftpEvent as FTPLogInEventArgs;

            if (loginEvent != null)
            {
                eventData.Add("Username", loginEvent.UserName);

                return true;
            }

            return false;
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Register to receive FTP events.
        /// </summary>
        void RegisterEvents()
        {
            _active = true;

            _ftpConnection.CommandSent += HandleFTPCommandSent;
            _ftpConnection.Connecting += HandleFtpEvent;
            _ftpConnection.Connected += HandleFtpEvent;
            _ftpConnection.LoggingIn += HandleFtpEvent;
            _ftpConnection.LoggedIn += HandleFtpEvent;
            _ftpConnection.DirectoryListing += HandleFtpEvent;
            _ftpConnection.DirectoryListed += HandleFtpEvent;
            _ftpConnection.Downloading += HandleFtpEvent;
            _ftpConnection.Downloaded += HandleFtpEvent;
            _ftpConnection.Uploading += HandleFtpEvent;
            _ftpConnection.Uploaded += HandleFtpEvent;
            _ftpConnection.RenamingFile += HandleFtpEvent;
            _ftpConnection.RenamedFile += HandleFtpEvent;
            _ftpConnection.Deleting += HandleFtpEvent;
            _ftpConnection.Deleted += HandleFtpEvent;
            _ftpConnection.CreatingDirectory += HandleFtpEvent;
            _ftpConnection.CreatedDirectory += HandleFtpEvent;
            _ftpConnection.DeletingDirectory += HandleFtpEvent;
            _ftpConnection.DeletedDirectory += HandleFtpEvent;
            _ftpConnection.ServerDirectoryChanging += HandleFtpEvent;
            _ftpConnection.ServerDirectoryChanged += HandleFtpEvent;
            _ftpConnection.Error += HandleError;
            _ftpExceptionSource.FtpError += HandleFtpErrorSourceError;
        }

        /// <summary>
        /// Unregister to receive FTP events.
        /// </summary>
        void UnregisterEvents()
        {
            _active = false;

            _ftpConnection.CommandSent -= HandleFTPCommandSent;
            _ftpConnection.Connecting -= HandleFtpEvent;
            _ftpConnection.Connected -= HandleFtpEvent;
            _ftpConnection.LoggingIn -= HandleFtpEvent;
            _ftpConnection.LoggedIn -= HandleFtpEvent;
            _ftpConnection.DirectoryListing -= HandleFtpEvent;
            _ftpConnection.DirectoryListed -= HandleFtpEvent;
            _ftpConnection.Downloading -= HandleFtpEvent;
            _ftpConnection.Downloaded -= HandleFtpEvent;
            _ftpConnection.Uploading -= HandleFtpEvent;
            _ftpConnection.Uploaded -= HandleFtpEvent;
            _ftpConnection.RenamingFile -= HandleFtpEvent;
            _ftpConnection.RenamedFile -= HandleFtpEvent;
            _ftpConnection.Deleting -= HandleFtpEvent;
            _ftpConnection.Deleted -= HandleFtpEvent;
            _ftpConnection.CreatingDirectory -= HandleFtpEvent;
            _ftpConnection.CreatedDirectory -= HandleFtpEvent;
            _ftpConnection.DeletingDirectory -= HandleFtpEvent;
            _ftpConnection.DeletedDirectory -= HandleFtpEvent;
            _ftpConnection.ServerDirectoryChanging -= HandleFtpEvent;
            _ftpConnection.ServerDirectoryChanged -= HandleFtpEvent;
            _ftpConnection.Error -= HandleError;
            _ftpExceptionSource.FtpError -= HandleFtpErrorSourceError;
        }

        #endregion Private Members
    }
}
