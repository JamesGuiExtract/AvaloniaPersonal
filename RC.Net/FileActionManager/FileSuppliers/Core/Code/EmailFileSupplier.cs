using Extract.Email.GraphClient;
using Extract.Encryption;
using Extract.Interop;
using Extract.Licensing;
using Extract.SqlDatabase;
using Extract.Utilities;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
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
        /// The user name to be used to access the email account
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// The password of the user name to be used to access the email account
        /// </summary>
        SecureString Password { get; set; }

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
        private const int _CURRENT_VERSION = 1;

        // The license id to validate in licensing calls
        private const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        #endregion

        #region Fields

        // Indicates that settings have been changed, but not saved.
        private bool _dirty;

        private IFileSupplierTarget _fileTarget;

        private bool stopProcessing = false;
        private readonly ManualResetEvent stopProcessingSuccessful = new(false);
        private bool pauseProcessing = false;
        private readonly ManualResetEvent pauseProcessingSuccessful = new(false);
        private readonly ManualResetEvent sleepResetter = new(false);
        private readonly ManualResetEvent processingStartedSuccessful = new(false);

        public EmailManagement EmailManagement { get; private set; }

        private bool disposedValue;

        #endregion Fields

        #region Properties
        public EmailManagementConfiguration EmailManagementConfiguration { get; set; } = new EmailManagementConfiguration();

        private ExtractRoleConnection extractRoleConnection;
        private Thread _processNewFiles;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailFileSupplier"/> class
        /// </summary>
        public EmailFileSupplier()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailFileSupplier"/> class.
        /// </summary>
        /// <param name="emailManagementConfiguration"></param>
        public EmailFileSupplier(EmailManagementConfiguration emailManagementConfiguration)
        {
            EmailManagementConfiguration.FilepathToDownloadEmails = emailManagementConfiguration.FilepathToDownloadEmails;
            EmailManagementConfiguration.UserName = emailManagementConfiguration.UserName;
            EmailManagementConfiguration.Password = new NetworkCredential("", emailManagementConfiguration.Password.Unsecure()).SecurePassword;
            EmailManagementConfiguration.SharedEmailAddress = emailManagementConfiguration.SharedEmailAddress;
            EmailManagementConfiguration.QueuedMailFolderName = emailManagementConfiguration.QueuedMailFolderName;
            EmailManagementConfiguration.InputMailFolderName = emailManagementConfiguration.InputMailFolderName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailFileSupplier"/> class
        /// </summary>
        /// <param name="supplier">The <see cref="EmailFileSupplier"/> from which settings should
        /// be copied.</param>
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
        public string UserName
        {
            get => EmailManagementConfiguration.UserName;
            set => EmailManagementConfiguration.UserName = value;
        }

        /// <inheritdoc/>
        public SecureString Password
        {
            get => EmailManagementConfiguration.Password;
            set => EmailManagementConfiguration.Password = value;
        }

        /// <inheritdoc/>
        public string SharedEmailAddress
        {
            get => EmailManagementConfiguration.SharedEmailAddress;
            set => EmailManagementConfiguration.SharedEmailAddress = value;
        }

        /// <inheritdoc/>
        public string QueuedMailFolderName
        {
            get => EmailManagementConfiguration.QueuedMailFolderName;
            set => EmailManagementConfiguration.QueuedMailFolderName = value;
        }

        /// <inheritdoc/>
        public string InputMailFolderName
        {
            get => EmailManagementConfiguration.InputMailFolderName;
            set => EmailManagementConfiguration.InputMailFolderName = value;
        }

        /// <inheritdoc/>
        public string DownloadDirectory
        {
            get => EmailManagementConfiguration.FilepathToDownloadEmails;
            set => EmailManagementConfiguration.FilepathToDownloadEmails = value;
        }

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

                if (string.IsNullOrEmpty(EmailManagementConfiguration.UserName)
                    || string.IsNullOrEmpty(EmailManagementConfiguration.Password.AsString())
                    || string.IsNullOrEmpty(EmailManagementConfiguration.SharedEmailAddress)
                    || string.IsNullOrEmpty(EmailManagementConfiguration.InputMailFolderName)
                    || string.IsNullOrEmpty(EmailManagementConfiguration.QueuedMailFolderName)
                    || string.IsNullOrEmpty(EmailManagementConfiguration.FilepathToDownloadEmails))
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
                    sleepResetter.Set();
                    this.pauseProcessing = true;
                    this.pauseProcessingSuccessful.WaitOne(10000);
                }
                else
                {
                    throw new ExtractException("ELI53280", "Cannot pause a task that has not started.");
                }
            }
            catch(Exception ex)
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
                sleepResetter.Reset();
                this.pauseProcessing = false;
                this.pauseProcessingSuccessful.Reset();
            }
            catch(Exception ex)
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

                this.stopProcessing = false;
                this.stopProcessingSuccessful.Reset();
                this.pauseProcessing = false;
                this.pauseProcessingSuccessful.Reset();
                sleepResetter.Reset();
                processingStartedSuccessful.Reset();

                _fileTarget = pTarget;

                this.EmailManagementConfiguration.FileProcessingDB = pDB;
                this.EmailManagement = new(this.EmailManagementConfiguration);

                extractRoleConnection = new ExtractRoleConnection(pDB.DatabaseServer, pDB.DatabaseName);
                extractRoleConnection.Open();

                this._processNewFiles = new Thread(() => ProcessFilesHelper());
                this._processNewFiles.Start();
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
                sleepResetter.Set();
                this.stopProcessing = true;
                this.stopProcessingSuccessful.WaitOne(10000);
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
                MapLabel mapLabel = new MapLabel();

                using (IStreamReader reader = new(stream, _CURRENT_VERSION))
                {
                    this.EmailManagementConfiguration.UserName = reader.ReadString();
                    this.EmailManagementConfiguration.Password = new NetworkCredential("", ExtractEncryption.DecryptString(reader.ReadString(), mapLabel)).SecurePassword;
                    this.EmailManagementConfiguration.FilepathToDownloadEmails = reader.ReadString();
                    this.EmailManagementConfiguration.InputMailFolderName = reader.ReadString();
                    this.EmailManagementConfiguration.FilepathToDownloadEmails = reader.ReadString();
                    this.EmailManagementConfiguration.QueuedMailFolderName = reader.ReadString();
                    this.EmailManagementConfiguration.SharedEmailAddress = reader.ReadString();
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
                    writer.Write(this.EmailManagementConfiguration.UserName);
                    writer.Write(ExtractEncryption.EncryptString(this.EmailManagementConfiguration.Password.Unsecure(), new MapLabel()));
                    writer.Write(this.EmailManagementConfiguration.FilepathToDownloadEmails);
                    writer.Write(this.EmailManagementConfiguration.InputMailFolderName);
                    writer.Write(this.EmailManagementConfiguration.FilepathToDownloadEmails);
                    writer.Write(this.EmailManagementConfiguration.QueuedMailFolderName);
                    writer.Write(this.EmailManagementConfiguration.SharedEmailAddress);

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
            this.EmailManagementConfiguration.UserName = task.EmailManagementConfiguration.UserName;
            this.EmailManagementConfiguration.Password = new NetworkCredential("", task.EmailManagementConfiguration.Password.Unsecure()).SecurePassword;
            this.EmailManagementConfiguration.FilepathToDownloadEmails = task.EmailManagementConfiguration.FilepathToDownloadEmails;
            this.EmailManagementConfiguration.InputMailFolderName = task.EmailManagementConfiguration.InputMailFolderName;
            this.EmailManagementConfiguration.FilepathToDownloadEmails = task.EmailManagementConfiguration.FilepathToDownloadEmails;
            this.EmailManagementConfiguration.QueuedMailFolderName = task.EmailManagementConfiguration.QueuedMailFolderName;
            this.EmailManagementConfiguration.SharedEmailAddress = task.EmailManagementConfiguration.SharedEmailAddress;
            
            _dirty = true;
        }

        /// <summary>
        /// This method is responsible for keeping the thread running until a stop event is recieved.
        /// Upon stopping, the file target is notified of it finishing.
        /// </summary>
        private void ProcessFilesHelper()
        {
            try
            {
                processingStartedSuccessful.Set();
                while (!this.stopProcessing)
                {
                    ProcessFiles();
                }

                stopProcessingSuccessful.Set();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI53272").Log();
            }

            _fileTarget.NotifyFileSupplyingDone(this);
        }

        /// <summary>
        /// This method is responsible for processing new files as long as the thread is not paused or stopped.
        /// </summary>
        private void ProcessFiles()
        {
            if (!this.pauseProcessing)
            {
                var messages = EmailManagement.GetMessagesToProcessAsync().Result;
                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        ProcessMessage(message);
                    }
                }
                else
                {
                    this.sleepResetter.WaitOne(5000);
                }
            }
            else
            {
                pauseProcessingSuccessful.Set();
            }
        }

        private void ProcessMessage(Microsoft.Graph.Message message)
        {
            try
            {
                var messageAlreadyProcessed = EmailFileSupplierDataAccess.DoesEmailExistInEmailSourceTable(extractRoleConnection, message);

                string file = EmailManagement.DownloadMessageToDisk(message, messageAlreadyProcessed).Result;
                EmailManagement.MoveMessageToQueuedFolder(message).Wait();
                var fileRecord = _fileTarget.NotifyFileAdded(file, this);
                if (!messageAlreadyProcessed)
                {
                    EmailFileSupplierDataAccess.WriteEmailToEmailSourceTable(this.EmailManagementConfiguration.FileProcessingDB
                                    , message
                                    , fileRecord
                                    , extractRoleConnection
                                    , this.EmailManagementConfiguration.SharedEmailAddress);
                }
            }
            catch (Exception ex)
            {
                // TODO: Add in logic to move to failed folder? https://extract.atlassian.net/browse/ISSUE-18044
                _fileTarget.NotifyFileSupplyingFailed(this, $"Failed to supply message with subject: {message.Subject}");
                ex.AsExtract("ELI53251").Log();
            }
        }

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
                    if (this.extractRoleConnection != null)
                    {
                        this.extractRoleConnection.Dispose();
                        this.extractRoleConnection = null;
                    }
                    if (this.EmailManagement != null)
                    {
                        this.EmailManagement.Dispose();
                        this.EmailManagement = null;
                    }
                    if (stopProcessingSuccessful != null)
                    {
                        stopProcessingSuccessful.Dispose();
                    }
                    if (pauseProcessingSuccessful != null)
                    {
                        pauseProcessingSuccessful.Dispose();
                    }
                    if (sleepResetter != null)
                    {
                        sleepResetter.Dispose();
                    }
                    if (pauseProcessingSuccessful != null)
                    {
                        pauseProcessingSuccessful.Dispose();
                    }
                    if (processingStartedSuccessful != null)
                    {
                        processingStartedSuccessful.Dispose();
                    }
                }

                // free unmanaged resources

                // The thread will keep running as long as the process runs if it isn't stopped        
                this._processNewFiles?.Abort();

                disposedValue = true;
            }
        }

        #endregion Private Members
    }
}
