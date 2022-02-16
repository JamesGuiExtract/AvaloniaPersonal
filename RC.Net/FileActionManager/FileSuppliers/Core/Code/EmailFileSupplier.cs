using Extract.Email.GraphClient;
using Extract.Encryption;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileSuppliers
{
    /// Interface definition for the <see cref="EmailFileSupplier"/>.
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
        /// The user name to be used to access the email account
        string UserName { get; set; }
        /// The password of the user name to be used to access the email account
        SecureString Password { get; set; }
        /// The shared email address that the emails will be read from. Different from the account used to access this shared address.
        string SharedEmailAddress { get; set; }
        /// The folder to move emails to after they have been downloaded
        string QueuedMailFolderName { get; set; }
        /// The folder to download emails from
        string InputMailFolderName { get; set; }
        // The folder to put downloaded emails into
        string DownloadDirectory { get; set; }
    }

    /// An <see cref="IFileSupplier"/> that supplies emails using the Graph API provided by microsoft.
    [ComVisible(true)]
    [Guid("C6365CA3-B70B-4400-A678-29C29C94B27B")]
    [ProgId("Extract.FileActionManager.FileSuppliers.EmailFileSupplier")]
    [CLSCompliant(false)]
    public sealed class EmailFileSupplier : IEmailFileSupplier, IDisposable
    {
        #region Constants

        /// The description of this file supplier
        const string _COMPONENT_DESCRIPTION = "Files from email";

        /// Current file supplier version.
        const int _CURRENT_VERSION = 1;

        /// The license id to validate in licensing calls
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        #endregion

        #region Fields

        // Indicates that settings have been changed, but not saved.
        bool _dirty;

        private IFileSupplierTarget _fileTarget;

        private System.Timers.Timer _emailSupplierTimer;
        private Dictionary<string, DateTime> emailsBeingProcessed { get; set; } = new Dictionary<string, DateTime>();

        public EmailManagement EmailManagement { get; private set; }

        private bool disposedValue;

        #endregion Fields

        #region Properties
        public EmailManagementConfiguration EmailManagementConfiguration { get; set; } = new EmailManagementConfiguration();

        #endregion

        #region Constructors

        /// Initializes a new instance of the <see cref="EmailFileSupplier"/> class
        public EmailFileSupplier()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailFileSupplier"/> class.
        /// </summary>
        /// <param name="emailManagementConfiguration"></param>
        public EmailFileSupplier(EmailManagementConfiguration emailManagementConfiguration)
        {
            EmailManagementConfiguration = emailManagementConfiguration;
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

        /// Gets the name of the COM object.
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

        /// Creates a copy of the <see cref="EmailFileSupplier"/> instance.
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

        /// Copies the specified <see cref="EmailFileSupplier"/> instance into this one.
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

        /// Pauses file supply
        public void Pause()
        {
            this._emailSupplierTimer.Stop();
        }

        /// Resumes file supplying after a pause
        public void Resume()
        {
            this._emailSupplierTimer.Start();
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

                _fileTarget = pTarget;

                this.EmailManagementConfiguration.FileProcessingDB = pDB;

                this.EmailManagement = new(this.EmailManagementConfiguration);

                // This may be a configuration option later, but for now run every 5 seconds.
                this._emailSupplierTimer = new System.Timers.Timer(5000);
                this._emailSupplierTimer.Elapsed += ManageEmailDownload;
                this._emailSupplierTimer.AutoReset = true;
                this._emailSupplierTimer.Enabled = true;

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
                this._emailSupplierTimer.Stop();
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
        static void RegisterFunction(Type type)
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
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileSuppliersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="EmailFileSupplier"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="EmailFileSupplier"/> from which to copy.</param>
        void CopyFrom(EmailFileSupplier task)
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

        private async void ManageEmailDownload(Object source, ElapsedEventArgs e)
        {
            try
            {
                var messages = (await EmailManagement.GetMessagesToProcessAsync()).ToArray();

                // It is possible two different timers are trying to process the same message, this prevents that.
                messages = messages.Where(message => !this.emailsBeingProcessed.ContainsKey(message.Id)).ToArray();
                messages.ToList().ForEach(message => this.emailsBeingProcessed.Add(message.Id, DateTime.Now));

                var files = await EmailManagement.DownloadMessagesToDisk(messages).ConfigureAwait(false);

                for (int i = 0; i < files.Count; i++)
                {
                    _fileTarget.NotifyFileAdded(files[i], this);

                    await EmailManagement.MoveMessageToQueuedFolder(messages[i]);
                }

                ClearOldMessages();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI53208").Log();
            }
        }

        private void ClearOldMessages()
        {
            foreach (var message in this.emailsBeingProcessed)
            {
                // Five minutes is arbitrary, but there needs to be some delay. The inbox does not update instantly
                // and anything under 30 seconds was inconsistant (sometimes it worked, sometimes it did not).
                if (message.Value < DateTime.Now.AddMinutes(-5))
                {
                    this.emailsBeingProcessed.Remove(message.Key);
                }
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
                }
                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                if (this._emailSupplierTimer != null)
                {
                    this._emailSupplierTimer.Dispose();
                    this._emailSupplierTimer = null;
                }
                // The thread will keep running as long as the process runs if it isn't stopped        
                disposedValue = true;
            }
        }

        #endregion Private Members
    }
}
