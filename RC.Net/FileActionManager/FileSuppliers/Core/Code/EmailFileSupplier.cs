using Extract.Email.GraphClient;
using Extract.Interop;
using Extract.Licensing;
using Extract.SqlDatabase;
using System;
using System.Runtime.InteropServices;
using System.Threading;
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

        #endregion

        #region Fields

        // Indicates that settings have been changed, but not saved.
        private bool _dirty;

        private IFileSupplierTarget _fileTarget;

        // Thread that checks and retrieves new emails
        private Thread _retrieveEmailsFromServerThread;

        // Signal the processing thread to stop
        private bool stopProcessing = false;

        // Set when the processing thread has stopped
        private readonly ManualResetEvent stopProcessingSuccessful = new(false);

        // Signal the processing thread to pause
        private bool pauseProcessing = false;

        // Set when the processing thread has paused
        private readonly ManualResetEvent pauseProcessingSuccessful = new(false);

        // Set to restart the processing thread when it is paused
        private readonly ManualResetEvent unpauseProcessing = new(true);

        // Set when the processing thread is sleeping because no emails were found
        private readonly ManualResetEvent sleepStarted = new(false);

        // Set to restart the processing thread when it is sleeping because no emails were found
        private readonly ManualResetEvent stopSleeping = new(false);

        // Set when the processing thread starts
        private readonly ManualResetEvent processingStartedSuccessful = new(false);

        // Used to interact with the MS Graph API
        private IEmailManagement _emailManagement;

        // Function used to create an IEmailManagement instance (injectable to facilitate unit testing)
        private readonly Func<EmailManagementConfiguration, IEmailManagement> _emailManagementCreator;

        // Whether this instance has been disposed
        private bool disposedValue;

        // Configuration with path-tags-expanded property values
        private EmailManagementConfiguration _emailManagementConfiguration;

        /// <summary>
        /// For unit testing. If stop or pause has been requested, waits until the thread actually stops/pauses
        /// </summary>
        internal void WaitForSupplyingToStop()
        {
            if (stopProcessing)
            {
                ExtractException.Assert("ELI53324", "Timeout waiting for stop processing", stopProcessingSuccessful.WaitOne(TimeSpan.FromMinutes(1)));
            }
            else if (pauseProcessing)
            {
                ExtractException.Assert("ELI53325", "Timeout waiting for pause processing", pauseProcessingSuccessful.WaitOne(TimeSpan.FromMinutes(1)));
            }
        }

        /// <summary>
        /// For unit testing. Waits until the thread is sleeping because there are no emails to download
        /// </summary>
        internal void WaitForSleep()
        {
            ExtractException.Assert("ELI53326", "Timeout waiting for sleep", sleepStarted.WaitOne(TimeSpan.FromMinutes(1)));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance
        /// </summary>
        public EmailFileSupplier()
            : this(null, null)
        {
        }

        /// <summary>
        /// Create a new instance from an <see cref="EmailManagementConfiguration"/> object
        /// </summary>
        /// <param name="emailManagementConfiguration">The configuration to initialize from</param>
        public EmailFileSupplier(EmailManagementConfiguration emailManagementConfiguration)
            : this(emailManagementConfiguration, null)
        {
        }

        /// <summary>
        /// Create a new instance using the supplied configuration and dependencies
        /// </summary>
        /// <param name="emailManagementConfiguration">The configuration to initialize from</param>
        /// <param name="emailManagementCreator">Custom function that creates an <see cref="IEmailManagement"/> instance</param>
        public EmailFileSupplier(
            EmailManagementConfiguration emailManagementConfiguration,
            Func<EmailManagementConfiguration, IEmailManagement> emailManagementCreator)
        {
            _emailManagementCreator = emailManagementCreator ?? (config => new EmailManagement(config));

            if (emailManagementConfiguration is not null)
            {
                DownloadDirectory = emailManagementConfiguration.FilePathToDownloadEmails;
                SharedEmailAddress = emailManagementConfiguration.SharedEmailAddress;
                QueuedMailFolderName = emailManagementConfiguration.QueuedMailFolderName;
                InputMailFolderName = emailManagementConfiguration.InputMailFolderName;
                FailedMailFolderName = emailManagementConfiguration.FailedMailFolderName;
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
                if (processingStartedSuccessful.WaitOne(10000))
                {
                    this.pauseProcessing = true;
                    this.unpauseProcessing.Reset();
                    this.stopSleeping.Set();
                    this.pauseProcessingSuccessful.WaitOne(10000);
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
                this.pauseProcessing = false;
                this.pauseProcessingSuccessful.Reset();
                this.unpauseProcessing.Set();
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

                InitializeEmailManagement(pDB, pFAMTM ?? new FAMTagManagerClass());

                this.stopProcessing = false;
                this.stopProcessingSuccessful.Reset();
                this.pauseProcessing = false;
                this.pauseProcessingSuccessful.Reset();
                this.unpauseProcessing.Set();
                processingStartedSuccessful.Reset();

                _fileTarget = pTarget;

                this._retrieveEmailsFromServerThread = new Thread(() => RetrieveEmailsFromServer());
                this._retrieveEmailsFromServerThread.Start();
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
                if (stopProcessing)
                {
                    return;
                }
                this.stopProcessing = true;
                this.unpauseProcessing.Set();
                this.stopSleeping.Set();
                this.stopProcessingSuccessful.WaitOne(10000);

                // Dispose of the IEmailManagement instance because Start will create a new instance
                _emailManagement?.Dispose();
                _emailManagement = null;
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

                    this.DownloadDirectory = reader.ReadString();
                    this.InputMailFolderName = reader.ReadString();

                    if (reader.Version == 1)
                    {
                        // Read extra copy of the download folder
                        reader.ReadString();
                    }

                    this.QueuedMailFolderName = reader.ReadString();
                    this.SharedEmailAddress = reader.ReadString();

                    if (reader.Version == 4)
                    {
                        this.FailedMailFolderName = reader.ReadString();
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
                    writer.Write(this.DownloadDirectory);
                    writer.Write(this.InputMailFolderName);
                    writer.Write(this.QueuedMailFolderName);
                    writer.Write(this.SharedEmailAddress);
                    writer.Write(this.FailedMailFolderName);

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
            this.DownloadDirectory = task.DownloadDirectory;
            this.InputMailFolderName = task.InputMailFolderName;
            this.QueuedMailFolderName = task.QueuedMailFolderName;
            this.SharedEmailAddress = task.SharedEmailAddress;
            this.FailedMailFolderName = task.FailedMailFolderName;

            _dirty = true;
        }

        /// <summary>
        /// This method is responsible for keeping the thread running until a stop event is received.
        /// </summary>
        private void RetrieveEmailsFromServer()
        {
            try
            {
                processingStartedSuccessful.Set();
                while (!stopProcessing)
                {
                    if (pauseProcessing)
                    {
                        pauseProcessingSuccessful.Set();
                        unpauseProcessing.WaitOne();
                    }
                    else
                    {
                        bool noMessagesFound = !RetrieveBatchOfNewEmailsFromServer();
                        if (noMessagesFound)
                        {
                            sleepStarted.Set();

                            stopSleeping.Reset();
                            stopSleeping.WaitOne(5000);

                            sleepStarted.Reset();
                        }
                    }
                }

                stopProcessingSuccessful.Set();
            }
            catch (Exception ex)
            {
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
            if (messages != null && messages.Count > 0)
            {
                foreach (var message in messages)
                {
                    ProcessMessage(message);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void ProcessMessage(Microsoft.Graph.Message message)
        {
            try
            {
                bool messageAlreadyProcessed = _emailManagement.TryGetExistingEmailFilePath(message, out string filePath);

                string file = _emailManagement.DownloadMessageToDisk(message, filePath).GetAwaiter().GetResult();
                _emailManagement.MoveMessageToQueuedFolder(message).GetAwaiter().GetResult();
                var fileRecord = _fileTarget.NotifyFileAdded(file, this);

                if (!messageAlreadyProcessed)
                {
                    _emailManagement.WriteEmailToEmailSourceTable(
                        message,
                        fileRecord.FileID,
                        _emailManagementConfiguration.SharedEmailAddress);
                }
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI53251").Log();

                MessageProcessingFailed(message);
            }
        }

        private void MessageProcessingFailed(Microsoft.Graph.Message message)
        {
            try
            {
                _emailManagement.MoveMessageToFailedFolder(message);

            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI53334").Log();
            }
        }

        // Create _emailManagementConfiguration and _emailManagement with expanded paths
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
        }

        #endregion Private Members

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
                    if (stopProcessingSuccessful != null)
                    {
                        stopProcessingSuccessful.Dispose();
                    }
                    if (pauseProcessingSuccessful != null)
                    {
                        pauseProcessingSuccessful.Dispose();
                    }
                    if (unpauseProcessing != null)
                    {
                        unpauseProcessing.Dispose();
                    }
                    if (sleepStarted != null)
                    {
                        sleepStarted.Dispose();
                    }
                    if (stopSleeping != null)
                    {
                        stopSleeping.Dispose();
                    }
                    if (processingStartedSuccessful != null)
                    {
                        processingStartedSuccessful.Dispose();
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
