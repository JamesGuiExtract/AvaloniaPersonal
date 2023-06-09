﻿using Extract.Email.GraphClient;
using Extract.FileActionManager.Database;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Polly;
using Polly.Retry;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileSuppliers
{
    /// <summary>
    /// Interface definition for the <see cref="EmailFileSupplier"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("0734F61A-55FE-49B2-B745-73BB89C67E24")]
    [CLSCompliant(false)]
    public interface IEmailFileSupplier :
        ICategorizedComponent,
        IConfigurableObject,
        IMustBeConfiguredObject,
        ICopyableObject,
        IFileSupplier,
        ILicensedComponent,
        IPersistStream
    {
        /// <summary>
        /// The shared email address that the emails will be read from. Different from the account used to access this shared address.
        /// </summary>
        string SharedEmailAddress { get; set; }

        /// <summary>
        /// The folder to move emails to after they have been downloaded
        /// </summary>
        string QueuedMailFolderName { get; set; }

        /// <summary>
        /// The folder to download emails from
        /// </summary>
        string InputMailFolderName { get; set; }

        /// <summary>
        /// The folder to move messages that fail the download/queue process
        /// </summary>
        string FailedMailFolderName { get; set; }

        /// <summary>
        /// The folder to put downloaded emails into
        /// </summary>
        string DownloadDirectory { get; set; }
    }

    /// <summary>
    /// An <see cref="IFileSupplier"/> that supplies emails using the Graph API provided by microsoft.
    /// </summary>
    [ComVisible(true)]
    [Guid("C6365CA3-B70B-4400-A678-29C29C94B27B")]
    [ProgId("Extract.FileActionManager.FileSuppliers.EmailFileSupplier")]
    [CLSCompliant(false)]
    public sealed class EmailFileSupplier : IEmailFileSupplier, IDisposable
    {
        #region Constants

        // The description of this file supplier
        private const string _COMPONENT_DESCRIPTION = "Files from email";

        // Current file supplier version.
        // Version 2: Remove extra copy of the DownloadDirectory from serialized data
        // Version 3: Remove UserName and Password
        // Version 4: Add FailedMailFolderName
        private const int _CURRENT_VERSION = 4;

        // The license id to validate in licensing calls
        private const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        const int MAX_RETRIES = 10;
        const int DELAY_SECONDS = 3; // Base # of seconds to delay, increases exponentially with each retry
        const string RETRY_ATTEMPT = "Retry-Attempt";
        const string OPERATION_NAME = "Operation-Name";

        #endregion

        #region Fields

        // Indicates that settings have been changed, but not saved.
        private bool _dirty;

        private IFileSupplierTarget _fileTarget;

        // Thread that checks and retrieves new emails
        private Thread _retrieveEmailsFromServerThread;

        // Signal the processing thread to stop
        // True until Start is called
        private bool _stopProcessing = true;

        // Set when the processing thread has stopped
        // Set until Start is called
        private readonly ManualResetEvent _stopProcessingSuccessful = new(true);

        // Signal the processing thread to pause
        private bool _pauseProcessing = false;

        // Set when the processing thread has paused
        private readonly ManualResetEvent _pauseProcessingSuccessful = new(false);

        // Set to restart the processing thread when it is paused
        private readonly ManualResetEvent _unpauseProcessing = new(true);

        // Set when the processing thread is sleeping because no emails were found
        private readonly ManualResetEvent _sleepStarted = new(false);

        // Set to restart the processing thread when it is sleeping because no emails were found
        private readonly ManualResetEvent _stopSleeping = new(false);

        // Set when the processing thread starts
        private readonly ManualResetEvent _processingStartedSuccessful = new(false);

        private readonly ManualResetEvent _processingFailed = new(false);

        // Used to interact with the MS Graph API
        private IEmailManagement _emailManagement;

        // Used to interact with the FAM database
        private IEmailDatabaseManager _emailDatabaseManager;

        // Function used to create an IEmailManagement instance (injectable to facilitate unit testing)
        private Func<EmailManagementConfiguration, IEmailManagement> _emailManagementCreator;

        // Function used to create an IEmailDatabaseManager instance (injectable to facilitate unit testing)
        private Func<EmailManagementConfiguration, IEmailDatabaseManager> _emailDatabaseManagerCreator;

        // Function used to create an IFileSupplierTarget instance (injectable to facilitate unit testing)
        private Func<IFileSupplierTarget, IFileSupplierTarget> _fileSupplierTargetCreator;

        // Whether this instance has been disposed
        private bool disposedValue;

        // Configuration with path-tags-expanded property values
        private EmailManagementConfiguration _emailManagementConfiguration;

        // Set when stop is called
        private CancellationTokenSource _cancelPendingOperations;
        private CancellationToken _cancelPendingOperationsToken;
        private readonly RetryPolicy _retryPolicy;

        private TimeSpan _unitTestingWaitLimit = TimeSpan.FromMinutes(10);

        /// <summary>
        /// For unit testing. If stop or pause has been requested, waits until the thread actually stops/pauses
        /// </summary>
        internal void WaitForSupplyingToStop()
        {
            if (_stopProcessing)
            {
                ExtractException.Assert("ELI53324", "Timeout waiting for stop processing",
                    WaitHandle.WaitAny(new[] { _stopProcessingSuccessful, _processingFailed }, _unitTestingWaitLimit) != WaitHandle.WaitTimeout);
            }
            else if (_pauseProcessing)
            {
                ExtractException.Assert("ELI53325", "Timeout waiting for pause processing",
                    WaitHandle.WaitAny(new[] { _pauseProcessingSuccessful, _processingFailed }, _unitTestingWaitLimit) != WaitHandle.WaitTimeout);
            }
        }

        /// <summary>
        /// For unit testing. Waits until the thread is sleeping because there are no emails to download
        /// </summary>
        internal void WaitForSleep()
        {
            ExtractException.Assert("ELI53326", "Timeout waiting for sleep",
                WaitHandle.WaitAny(new[] { _sleepStarted, _processingFailed }, _unitTestingWaitLimit) != WaitHandle.WaitTimeout);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance
        /// </summary>
        public EmailFileSupplier()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Create a new instance from an <see cref="EmailManagementConfiguration"/> object
        /// </summary>
        /// <param name="emailManagementConfiguration">The configuration to initialize from</param>
        public EmailFileSupplier(EmailManagementConfiguration emailManagementConfiguration)
            : this(emailManagementConfiguration, null, null)
        {
        }

        /// <summary>
        /// Create a new instance using the supplied configuration and dependencies
        /// </summary>
        /// <param name="emailManagementConfiguration">The configuration to initialize from</param>
        /// <param name="emailManagementCreator">Custom function that creates an <see cref="IEmailManagement"/> instance</param>
        public EmailFileSupplier(
            EmailManagementConfiguration emailManagementConfiguration,
            Func<EmailManagementConfiguration, IEmailManagement> emailManagementCreator,
            Func<EmailManagementConfiguration, IEmailDatabaseManager> emailDatabaseManagerCreator = null,
            Func<IFileSupplierTarget, IFileSupplierTarget> fileSupplierTargetCreator = null)
        {
            try
            {
                _emailManagementCreator = emailManagementCreator ?? (config => new EmailManagement(config));
                _emailDatabaseManagerCreator = emailDatabaseManagerCreator ?? (config => new EmailDatabaseManager(config));
                _fileSupplierTargetCreator = fileSupplierTargetCreator ?? (x => x);

                if (emailManagementConfiguration is not null)
                {
                    DownloadDirectory = emailManagementConfiguration.FilePathToDownloadEmails;
                    SharedEmailAddress = emailManagementConfiguration.SharedEmailAddress;
                    QueuedMailFolderName = emailManagementConfiguration.QueuedMailFolderName;
                    InputMailFolderName = emailManagementConfiguration.InputMailFolderName;
                    FailedMailFolderName = emailManagementConfiguration.FailedMailFolderName;
                }

                _retryPolicy = Policy.Handle<ExtractException>()
                    .WaitAndRetry(MAX_RETRIES, CalculateSleepDuration, LogExceptionBeforeRetry);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53448");
            }
        }

        /// <summary>
        /// Create a new instance from an <see cref="EmailFileSupplier"/>
        /// </summary>
        /// <param name="supplier">The <see cref="EmailFileSupplier"/> from which settings should be copied</param>
        public EmailFileSupplier(EmailFileSupplier supplier)
        {
            try
            {
                CopyFrom(supplier);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53202");
            }
        }

        #endregion Constructors

        #region IEmailFileSupplier Members

        /// <inheritdoc/>
        public string SharedEmailAddress { get; set; }

        /// <inheritdoc/>
        public string QueuedMailFolderName { get; set; }

        /// <inheritdoc/>
        public string InputMailFolderName { get; set; }

        /// <inheritdoc/>
        public string FailedMailFolderName { get; set; }

        /// <inheritdoc/>
        public string DownloadDirectory { get; set; }

        #endregion IEmailFileSupplier Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="EmailFileSupplier"/>.
        /// </summary>
        /// <returns><c>true</c> if the configuration was successfully updated or
        /// <c>false</c> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI53201", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (EmailFileSupplier)Clone();

                using (var dialog = new EmailFileSupplierSettingsDialog(cloneOfThis))
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
                throw ExtractException.CreateComVisible("ELI53200",
                    "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the object has been configured and <c>false</c> otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                bool configured = true;

                if (string.IsNullOrEmpty(SharedEmailAddress)
                    || string.IsNullOrEmpty(InputMailFolderName)
                    || string.IsNullOrEmpty(QueuedMailFolderName)
                    || string.IsNullOrEmpty(DownloadDirectory)
                    || string.IsNullOrEmpty(FailedMailFolderName))
                {
                    configured = false;
                }

                return configured;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI53199", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="EmailFileSupplier"/> instance.
        /// </summary>
        public object Clone()
        {
            try
            {
                return new EmailFileSupplier(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53198", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="EmailFileSupplier"/> instance into this one.
        /// </summary>
        public void CopyFrom(object pObject)
        {
            try
            {
                if (pObject is not EmailFileSupplier task)
                {
                    throw new InvalidCastException("Invalid copy-from object. Requires EmailFileSupplier");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53197", "Unable to copy object.", ex);
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
                if (_processingStartedSuccessful.WaitOne(10000))
                {
                    _pauseProcessing = true;
                    _unpauseProcessing.Reset();
                    _stopSleeping.Set();
                    _pauseProcessingSuccessful.WaitOne(10000);
                }
                else
                {
                    throw new ExtractException("ELI53280", "Cannot pause a task that has not started.");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53281");
            }
        }

        /// <summary>
        /// Resumes file supplying after a pause
        /// </summary>
        public void Resume()
        {
            try
            {
                _pauseProcessing = false;
                _pauseProcessingSuccessful.Reset();
                _unpauseProcessing.Set();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53282");
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
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI53195", _COMPONENT_DESCRIPTION);

                if (_cancelPendingOperations is not null)
                {
                    _cancelPendingOperations.Dispose();
                }
                _cancelPendingOperations = new CancellationTokenSource();
                _cancelPendingOperationsToken = _cancelPendingOperations.Token;

                InitializeEmailManagement(pDB, pFAMTM ?? new FAMTagManagerClass());

                _stopProcessing = false;
                _stopProcessingSuccessful.Reset();
                _pauseProcessing = false;
                _pauseProcessingSuccessful.Reset();
                _unpauseProcessing.Set();
                _processingStartedSuccessful.Reset();

                _fileTarget = _fileSupplierTargetCreator(pTarget);

                _retrieveEmailsFromServerThread = new Thread(() => RetrieveEmailsFromServer());
                _retrieveEmailsFromServerThread.Start();
            }
            catch (Exception ex)
            {
                ExtractException ee = new("ELI53196", "Unable to start supplying object", ex);
                pTarget.NotifyFileSupplyingFailed(this, ee.AsStringizedByteStream());
            }
        }

        /// <summary>
        /// Stops file supplying
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_stopProcessing)
                {
                    return;
                }
                _cancelPendingOperations.Cancel();
                _stopProcessing = true;
                _unpauseProcessing.Set();
                _stopSleeping.Set();
                _stopProcessingSuccessful.WaitOne(10000);

                // Dispose of these instances because Start will create new ones
                _emailManagement?.Dispose();
                _emailManagement = null;

                _emailDatabaseManager?.Dispose();
                _emailDatabaseManager = null;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI53194", "Unable to stop supplying object");
            }
        }

        #endregion IFileSupplier Members

        #region IAccessRequired Members

        /// <summary>
        /// Returns bool value indicating if the task requires admin access.
        /// </summary>
        /// <returns><c>true</c> if the task requires admin access
        /// <c>false</c> if task does not require admin access.</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IAccessRequired Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><c>true</c> if the component is licensed; <c>false</c> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            try
            {
                return LicenseUtilities.IsLicensed(_LICENSE_ID);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53193",
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
        /// Initializes an object from the <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new(stream, _CURRENT_VERSION))
                {
                    if (reader.Version <= 2)
                    {
                        // Read UserName and Password
                        reader.ReadString();
                        reader.ReadString();
                    }

                    DownloadDirectory = reader.ReadString();
                    InputMailFolderName = reader.ReadString();

                    if (reader.Version == 1)
                    {
                        // Read extra copy of the download folder
                        reader.ReadString();
                    }

                    QueuedMailFolderName = reader.ReadString();
                    SharedEmailAddress = reader.ReadString();

                    if (reader.Version == 4)
                    {
                        FailedMailFolderName = reader.ReadString();
                    }
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI53192",
                    "Unable to load object from stream.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <c>true</c>, the flag should be cleared. If 
        /// <c>false</c>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new(_CURRENT_VERSION))
                {
                    writer.Write(DownloadDirectory);
                    writer.Write(InputMailFolderName);
                    writer.Write(QueuedMailFolderName);
                    writer.Write(SharedEmailAddress);
                    writer.Write(FailedMailFolderName);

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
                throw ExtractException.CreateComVisible("ELI53191",
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
        /// <see cref="ExtractCategories.EmailFileSupplierGuid"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        private static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileSuppliersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// <see cref="ExtractCategories.EmailFileSupplierGuid"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        private static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileSuppliersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="EmailFileSupplier"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="EmailFileSupplier"/> from which to copy.</param>
        private void CopyFrom(EmailFileSupplier task)
        {
            DownloadDirectory = task.DownloadDirectory;
            InputMailFolderName = task.InputMailFolderName;
            QueuedMailFolderName = task.QueuedMailFolderName;
            SharedEmailAddress = task.SharedEmailAddress;
            FailedMailFolderName = task.FailedMailFolderName;
            _emailManagementCreator = task._emailManagementCreator;
            _emailDatabaseManagerCreator = task._emailDatabaseManagerCreator;
            _fileSupplierTargetCreator = task._fileSupplierTargetCreator;

            _dirty = true;
        }

        /// <summary>
        /// This method is responsible for keeping the thread running until a stop event is received.
        /// </summary>
        private void RetrieveEmailsFromServer()
        {
            try
            {
                _processingStartedSuccessful.Set();

                VerifyEmailFolders().GetAwaiter().GetResult();

                while (!_stopProcessing)
                {
                    if (_pauseProcessing)
                    {
                        _pauseProcessingSuccessful.Set();
                        _unpauseProcessing.WaitOne();
                    }
                    else
                    {
                        bool noMessagesFound = !RetrieveBatchOfNewEmailsFromServer();
                        if (noMessagesFound)
                        {
                            RetryPendingNotifications();
                            RetryPendingMoves();
                            _sleepStarted.Set();

                            _stopSleeping.Reset();
                            WaitHandle.WaitAny(new[] { _stopSleeping, _processingFailed }, 5000);

                            _sleepStarted.Reset();
                        }
                    }
                }

                _stopProcessingSuccessful.Set();
            }
            catch (Exception ex)
            {
                _processingFailed.Set();

                if (_cancelPendingOperationsToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    _cancelPendingOperations.Cancel();
                }
                catch { }

                string serializedExn = ex.AsExtract("ELI53272").AsStringizedByteStream();
                _fileTarget.NotifyFileSupplyingFailed(this, serializedExn);
            }
        }

        /// <summary>
        /// Retrieve and process a batch of emails
        /// </summary>
        /// <returns>true if any messages were retrieved, else false</returns>
        private bool RetrieveBatchOfNewEmailsFromServer()
        {
            var messages = _emailManagement.GetMessagesToProcessAsync().GetAwaiter().GetResult();
            if (messages.Count > 0)
            {
                foreach (var message in messages)
                {
                    // Do not finish processing the batch if a stop or pause has been requested
                    if (_stopProcessing || _pauseProcessing)
                    {
                        break;
                    }

                    _cancelPendingOperationsToken.ThrowIfCancellationRequested();

                    ProcessMessage(message);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        // Add a single message to the database and notify the file supplying target
        private void ProcessMessage(Microsoft.Graph.Message message)
        {
            try
            {
                using TransactionScope scope = _emailDatabaseManager.LockEmailSource();

                // If all that is needed is to move the message then just do that and return
                if (_emailDatabaseManager.IsEmailPendingMoveFromInbox(message.Id))
                {
                    if (TryMoveMessageToQueuedFolder(message.Id))
                    {
                        LogExceptionOnFailure("ELI53454",
                            "Application trace: Failed to clear pending notification for an email source record",
                            message.Id, null, () => _emailDatabaseManager.ClearPendingMoveFromEmailFolder(message.Id));
                    }

                    return;
                }

                // Now that the EmailSource table is locked, confirm that the message is still in the input folder
                if (!_emailManagement.IsMessageInInputFolder(message.Id).GetAwaiter().GetResult())
                {
                    // Probably another file supplier has already queued this file so do nothing
                    return;
                }

                if (!TryAddEmailToDatabase(message, out string filePath))
                {
                    // Either the message has been moved to the failed folder or is still in the input folder and will be attempted again
                    return;
                }

                // Change the email's parent folder so that no other file supplier will process it
                // If this fails then it is likely that subsequent web requests
                // (e.g., moving the message to the failed folder) will also fail so just log an exception
                // and leave PendingMoveFromEmailFolder set so that the message/ will get moved later if needed.
                bool messageWasMovedFromInbox = TryMoveMessageToQueuedFolder(message.Id, filePath);

                // Everything has been accomplished except queueing the file so complete the transaction
                scope.Complete();
                scope.Dispose();

                bool fileWasQueued = false;
                LogExceptionOnFailure("ELI53450", "Application trace: Failed to queue email", message.Id, filePath, () =>
                {
                    // Now that the file is where it needs to be and the transaction has been committed,
                    // the file can be queued for processing (if this is attempted within the transaction then it will deadlock)
                    _fileTarget.NotifyFileAdded(filePath, this);

                    fileWasQueued = true;
                });

                // If the NotifyFileAdded call succeeded then clear the EmailSource field so that it doesn't cause the file to be queued again
                if (fileWasQueued)
                {
                    LogExceptionOnFailure("ELI53451", "Application trace: Failed to clear pending notification for an email source record",
                        message.Id, filePath, () => _emailDatabaseManager.ClearPendingNotifyFromEmailFolder(message.Id));
                }

                if (messageWasMovedFromInbox)
                {
                    LogExceptionOnFailure("ELI53452", "Application trace: Failed to clear pending move for an email source record",
                        message.Id, filePath, () => _emailDatabaseManager.ClearPendingMoveFromEmailFolder(message.Id));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53413");
            }
        }

        // Perform an action and log an exception if it fails
        private static void LogExceptionOnFailure(string eliCode, string exceptionMessage, string messageID, string filePath, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ExtractException uex = new(eliCode, exceptionMessage, ex);
                uex.AddDebugData("Outlook email ID", messageID);
                if (filePath is not null)
                {
                    uex.AddDebugData("File name", filePath);
                }
                uex.Log();
            }
        }

        // Attempt to download and add an email to the FAM database
        private bool TryAddEmailToDatabase(Microsoft.Graph.Message message, out string filePath)
        {
            filePath = null;
            try
            {
                bool messageAlreadyProcessed = _emailDatabaseManager.TryGetExistingEmailFilePath(message, out filePath);
                if (!messageAlreadyProcessed)
                {
                    filePath = _emailDatabaseManager.GetNewFileName(message);
                }

                // Download to a temporary file first to avoid problems with partial downloads or other failures leaving extra
                // files in the downloads folder
                TemporaryFile tempFile = new(false);
                _emailManagement.DownloadMessageToDisk(message, tempFile.FileName).GetAwaiter().GetResult();

                // Workflow will be set by NotifyFileAdded so no need to do that here
                FAMFileInfo fileInfo = new(
                    filePath: filePath,
                    fileSize: new System.IO.FileInfo(tempFile.FileName).Length,
                    pageCount: 0,
                    workflowID: null);

                // Add records to the database if needed
                if (!messageAlreadyProcessed)
                {
                    _emailDatabaseManager.AddEmailToDatabase(message, fileInfo);
                }

                // Now copy the file to the target location
                FileSystemMethods.MoveFile(tempFile.FileName, filePath,
                    overwrite: messageAlreadyProcessed,
                    secureMoveFile: false,
                    doRetries: true);

                return true;
            }
            catch (Exception ex)
            {
                ExtractException uex = new("ELI53251", "Failed to add email to the database", ex);
                uex.AddDebugData("Outlook email ID", message.Id);
                if (filePath is not null)
                {
                    uex.AddDebugData("File name", filePath);
                }
                uex.Log();

                MessageProcessingFailed(message.Id);

                return false;
            }
        }

        // Attempt to move the message to the configured failed folder
        private void MessageProcessingFailed(string messageID, string filePath = null)
        {
            LogExceptionOnFailure("ELI53334", "Application trace: Failed to move email to the failed folder",
                messageID, filePath, () => _emailManagement.MoveMessageToFailedFolder(messageID).GetAwaiter().GetResult());
        }

        // Clean-up the EmailSource table by notifying the file supplying target of
        // files that are in the database but not yet queued
        // This is to take care of corner cases where an error left data in an inconsistent state.
        private void RetryPendingNotifications()
        {
            try
            {
                foreach (string messageID in _emailDatabaseManager.GetEmailsPendingNotifyFromInbox())
                {
                    var context = new Context { { OPERATION_NAME, "ClearPendingNotifyFromEmailFolder" } };

                    _retryPolicy.Execute((_, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        using TransactionScope scope = _emailDatabaseManager.LockEmailSource();

                        string filePath = _emailDatabaseManager.GetExistingEmailFilePath(messageID);
                        _fileTarget.NotifyFileAdded(filePath, this);
                        _emailDatabaseManager.ClearPendingNotifyFromEmailFolder(messageID);

                        scope.Complete();
                    }, context, _cancelPendingOperationsToken);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53426");
            }
        }

        // Clean-up the EmailSource table by clearing any pending moves that have already happened
        // and moving messages that are still in the input folder to the post-download folder
        // This is to take care of corner cases where an error left data in an inconsistent state.
        private void RetryPendingMoves()
        {
            try
            {
                foreach (string messageID in _emailDatabaseManager.GetEmailsPendingMoveFromInbox())
                {
                    var context = new Context { { OPERATION_NAME, "ClearPendingMoveFromEmailFolder" } };

                    _retryPolicy.Execute((_, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        using TransactionScope scope = _emailDatabaseManager.LockEmailSource();

                        bool clearFlag = true;
                        if (_emailManagement.IsMessageInInputFolder(messageID).GetAwaiter().GetResult())
                        {
                            clearFlag = TryMoveMessageToQueuedFolder(messageID);
                        }
                        if (clearFlag)
                        {
                            _emailDatabaseManager.ClearPendingMoveFromEmailFolder(messageID);
                        }

                        scope.Complete();
                    }, context, _cancelPendingOperationsToken);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53441");
            }
        }

        // Attempt to move an email to the post-download folder. Return true if successful
        private bool TryMoveMessageToQueuedFolder(string messageID, string filePath = null)
        {
            bool messageMovedFromInbox = false;
            LogExceptionOnFailure("ELI53423", "Application trace: Failed to move message to the post-download folder",
                messageID, filePath, () =>
                {
                    _emailManagement.MoveMessageToQueuedFolder(messageID).GetAwaiter().GetResult();
                    messageMovedFromInbox = true;
                });
            return messageMovedFromInbox;
        }

        // Create _emailManagementConfiguration, _emailManagement and _emailDatabaseManager with expanded paths
        private void InitializeEmailManagement(FileProcessingDB pDB, IFAMTagManager tagManager)
        {
            _emailManagementConfiguration = new()
            {
                ExternalLoginDescription = Constants.EmailFileSupplierExternalLoginDescription,
                FileProcessingDB = pDB,
                SharedEmailAddress = tagManager.ExpandTagsAndFunctions(SharedEmailAddress, ""),
                InputMailFolderName = tagManager.ExpandTagsAndFunctions(InputMailFolderName, ""),
                QueuedMailFolderName = tagManager.ExpandTagsAndFunctions(QueuedMailFolderName, ""),
                FilePathToDownloadEmails = tagManager.ExpandTagsAndFunctions(DownloadDirectory, ""),
                FailedMailFolderName = tagManager.ExpandTagsAndFunctions(FailedMailFolderName, "")
            };

            _emailManagement = _emailManagementCreator(_emailManagementConfiguration);
            _emailDatabaseManager = _emailDatabaseManagerCreator(_emailManagementConfiguration);
        }

        private async Task VerifyEmailFolders()
        {
            ExtractException.Assert("ELI53391",
                UtilityMethods.FormatInvariant($"Input mail folder, {_emailManagementConfiguration.InputMailFolderName}, does not exist!"),
                await _emailManagement.DoesMailFolderExist(_emailManagementConfiguration.InputMailFolderName).ConfigureAwait(false));

            ExtractException.Assert("ELI53392",
                UtilityMethods.FormatInvariant($"Post-download mail folder, {_emailManagementConfiguration.QueuedMailFolderName}, does not exist!"),
                await _emailManagement.DoesMailFolderExist(_emailManagementConfiguration.QueuedMailFolderName).ConfigureAwait(false));

            ExtractException.Assert("ELI53393",
                UtilityMethods.FormatInvariant($"Failed download mail folder, {_emailManagementConfiguration.FailedMailFolderName}, does not exist!"),
                await _emailManagement.DoesMailFolderExist(_emailManagementConfiguration.FailedMailFolderName).ConfigureAwait(false));
        }

        #endregion Private Members

        #region Retry Policy

        // Called after a failure before the sleep has started
        private void LogExceptionBeforeRetry(Exception exception, TimeSpan sleepDuration, int retryNumber, Context context)
        {
            context[RETRY_ATTEMPT] = retryNumber;

            var uex = new ExtractException("ELI53449",
                UtilityMethods.FormatInvariant(
                    $"Application trace: ({nameof(EmailFileSupplier)}) operation failed. ",
                    $"Retrying in {sleepDuration.TotalSeconds} seconds ({retryNumber}/{MAX_RETRIES})"),
                exception);

            if (context.TryGetValue(OPERATION_NAME, out object value))
            {
                uex.AddDebugData("Request", (string)value);
            }
            uex.AddDebugData("Attempt", retryNumber);

            uex.Log();
        }

        // Calculate the time to wait before retry using exponential back-off strategy
        private static TimeSpan CalculateSleepDuration(int retryNumber)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, retryNumber - 1) * DELAY_SECONDS);
        }

        #endregion Retry Policy

        #region IDisposable Support

        ~EmailFileSupplier()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)

                    try
                    {
                        Stop();
                    }
                    catch { }

                    if (_emailManagement != null)
                    {
                        _emailManagement.Dispose();
                        _emailManagement = null;
                    }
                    if (_emailDatabaseManager != null)
                    {
                        _emailDatabaseManager.Dispose();
                        _emailDatabaseManager = null;
                    }
                    if (_stopProcessingSuccessful != null)
                    {
                        _stopProcessingSuccessful.Dispose();
                    }
                    if (_pauseProcessingSuccessful != null)
                    {
                        _pauseProcessingSuccessful.Dispose();
                    }
                    if (_unpauseProcessing != null)
                    {
                        _unpauseProcessing.Dispose();
                    }
                    if (_sleepStarted != null)
                    {
                        _sleepStarted.Dispose();
                    }
                    if (_stopSleeping != null)
                    {
                        _stopSleeping.Dispose();
                    }
                    if (_processingStartedSuccessful != null)
                    {
                        _processingStartedSuccessful.Dispose();
                    }
                    if (_processingFailed != null)
                    {
                        _processingFailed.Dispose();
                    }
                    if (_cancelPendingOperations != null)
                    {
                        _cancelPendingOperations.Dispose();
                    }
                }

                // free unmanaged resources

                // The thread will keep running as long as the process runs if it isn't stopped        
                _retrieveEmailsFromServerThread?.Abort();

                disposedValue = true;
            }
        }
        #endregion IDisposable Support
    }
}
