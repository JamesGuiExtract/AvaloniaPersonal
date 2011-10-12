﻿using EnterpriseDT.Net.Ftp;
using Extract.Licensing;
using System;
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
        /// The <see cref="FTPEventArgs"/> that describes this FTP event.
        /// </summary>
        FTPEventArgs _ftpEvent;

        /// <summary>
        /// The <see cref="IFileRecord"/> to which this FTP event relates.
        /// </summary>
        IFileRecord _fileRecord;
        
        /// <summary>
        /// Indicates whether the FTP event has ended.
        /// </summary>
        bool _eventEnded;

        /// <summary>
        /// An <see cref="Exception"/> that describes an error that occured during the FTP operation
        /// or <see langword="null"/> if there was no error.
        /// </summary>
        Exception _eventException;

        /// <summary>
        /// The number of times the FTP operation was retried (whether or not it eventually succeeded).
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

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpEventRecorder"/> class.
        /// <param><b>Note</b></param>
        /// The lifetime of each <see cref="FtpEventRecorder"/> should encapsulate a single FTP
        /// operation that should be logged upon its eventual success or failure. Multiple retries
        /// of the operation can be included an instance of this class, but once the operation
        /// succeeds or the retry attempts have been exhausted, it is expected that
        /// <see cref="Dispose()"/> be called or the behavior of this class will be incorrect/undefined.
        /// The <see cref="Dispose()"/> call is what triggers the event to be logged.
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
        public FtpEventRecorder(IFtpEventErrorSource ftpExceptionSource,
            SecureFTPConnection ftpConnection, IFileProcessingDB fileProcessingDB, int actionID,
            bool queuing, EFTPAction ftpAction)
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
                    _ftpExceptionSource = ftpExceptionSource;
                    _ftpConnection = ftpConnection;
                    _fileProcessingDB = fileProcessingDB;
                    _actionID = actionID;
                    _queuing = queuing;
                    _ftpAction = ftpAction;

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
        /// Handles the start of an FTP event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EnterpriseDT.Net.Ftp.FTPEventArgs"/>
        /// instance containing the event data.</param>
        void HandleEventBegin(object sender, FTPEventArgs e)
        {
            try
            {
                if (_eventEnded && _eventException == null)
                {
                    new ExtractException("ELI33982", "Unexpected FTP event.").Log();
                    UnregisterEvents();
                }
                else
                {
                    // If this event has previously ended, this is a retry.
                    if (_eventEnded)
                    {
                        _retryCount++;
                    }

                    _eventException = null;
                    _eventEnded = false;
                    _ftpEvent = e;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33967");
            }
        }

        /// <summary>
        /// Handles the end of an FTP event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EnterpriseDT.Net.Ftp.FTPEventArgs"/>
        /// instance containing the event data.</param>
        void HandleEventEnd(object sender, FTPEventArgs e)
        {
            try
            {
                _ftpEvent = e;
                _eventEnded = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33971");
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
        void HandleFtpExceptionSourceError(object sender, ExtractExceptionEventArgs e)
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

                    // If these objects were set, an FTP event was tracked.
                    if (_ftpConnection != null && _fileProcessingDB != null && _ftpEvent != null)
                    {
                        // Assign argument1 and argument2 using _ftpEvent and the _ftpAction.
                        string argument1 = null;
                        string argument2 = null;

                        var fileTransferEvent = _ftpEvent as FTPFileTransferEventArgs;
                        if (fileTransferEvent != null)
                        {
                            if (_ftpAction == EFTPAction.kDownloadFileFromFtpServer)
                            {
                                argument1 = fileTransferEvent.RemotePath;
                                argument2 = fileTransferEvent.LocalPath;
                            }
                            else if ((_ftpAction == EFTPAction.kUploadFileToFtpServer))
                            {
                                argument1 = fileTransferEvent.LocalPath;
                                argument2 = fileTransferEvent.RemotePath;
                            }
                            else
                            {
                                argument1 = fileTransferEvent.RemotePath;
                            }
                        }
                        else
                        {
                            var fileRenameEvent = _ftpEvent as FTPFileRenameEventArgs;
                            if (fileRenameEvent != null)
                            {
                                argument1 = fileRenameEvent.OldFilePath;
                                argument2 = fileRenameEvent.NewFilePath;
                            }
                            else
                            {
                                ExtractException.ThrowLogicException("ELI33987");
                            }
                        }

                        // If the event never ended, this is an error (even if _eventException was
                        // not otherwise set).
                        if (!_eventEnded && _eventException == null)
                        {
                            _eventException =
                                new ExtractException("ELI33973", "Operation did not complete.");
                        }

                        int fileId = (_fileRecord == null) ? -1 : _fileRecord.FileID;
                        int actionId = _actionID;
                        string userName = _ftpConnection.UserName;
                        string serverAddress = _ftpConnection.ServerAddress;
                        string exception = (_eventException == null)
                            ? null
                            : _eventException.AsExtract("ELI33980").CreateLogString();

                        _fileProcessingDB.RecordFTPEvent(fileId, actionId, _queuing, _ftpAction,
                            serverAddress, userName, argument1, argument2, _retryCount, exception);
                    }
                }
                catch (Exception ex)
                {
                    new ExtractException("ELI33981", "Error logging FTP event history.", ex).Log();
                }
                finally
                {
                    // Whether or not a successful entry was logged, 
                    _ftpConnection = null;
                    _fileProcessingDB = null;
                    _ftpEvent = null;
                }
            }
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Register to receive FTP events.
        /// </summary>
        void RegisterEvents()
        {
            _ftpConnection.Downloading += HandleEventBegin;
            _ftpConnection.Uploading += HandleEventBegin;
            _ftpConnection.RenamingFile += HandleEventBegin;
            _ftpConnection.Deleting += HandleEventBegin;
            _ftpConnection.Downloaded += HandleEventEnd;
            _ftpConnection.Uploaded += HandleEventEnd;
            _ftpConnection.RenamedFile += HandleEventEnd;
            _ftpConnection.Deleted += HandleEventEnd;
            _ftpConnection.Error += HandleError;
            _ftpExceptionSource.FtpError += HandleFtpExceptionSourceError;
        }

        /// <summary>
        /// Unregister to receive FTP events.
        /// </summary>
        void UnregisterEvents()
        {
            _ftpConnection.Downloading -= HandleEventBegin;
            _ftpConnection.Uploading -= HandleEventBegin;
            _ftpConnection.RenamingFile -= HandleEventBegin;
            _ftpConnection.Deleting -= HandleEventBegin;
            _ftpConnection.Downloaded -= HandleEventEnd;
            _ftpConnection.Uploaded -= HandleEventEnd;
            _ftpConnection.RenamedFile -= HandleEventEnd;
            _ftpConnection.Deleted -= HandleEventEnd;
            _ftpConnection.Error -= HandleError;
            _ftpExceptionSource.FtpError -= HandleFtpExceptionSourceError;
        }

        #endregion Private Members
    }
}
