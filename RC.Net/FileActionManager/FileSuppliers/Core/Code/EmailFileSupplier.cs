using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities.EmailGraphApi;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
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
        public EmailManagementConfiguration EmailManagementConfiguration { get; set; }
    }

    /// An <see cref="IFileSupplier"/> that supplies emails using the Graph API provided by microsoft.
    [ComVisible(true)]
    [Guid("C6365CA3-B70B-4400-A678-29C29C94B27B")]
    [ProgId("Extract.FileActionManager.FileSuppliers.EmailFileSupplier")]
    [CLSCompliant(false)]
    public class EmailFileSupplier : IEmailFileSupplier
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

        private bool _pauseProcessing = false;
        private bool _stopProcessing = false;
        private IFileSupplierTarget _fileTarget;
        private FileProcessingDB _fileProcessingDB;
        private int _ActionID;
        private Thread _emailSupplierThread;

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

        private void SetupConfigurationNOUI()
        {
            SecureString secureString = new();
            foreach (char c in "an.Ass5.hogs.a.mimic".ToCharArray())
            {
                secureString.AppendChar(c);
            }

            this.EmailManagementConfiguration.UserName = "email_test@extractsystems.com";
            this.EmailManagementConfiguration.Password = secureString;
            this._fileProcessingDB.SetDBInfoSetting("AzureClientID", "6311c46a-18a8-4f8c-9702-e0d9b02eb7d2", true, false);
            this._fileProcessingDB.SetDBInfoSetting("AzureTenantID", "bd07e2c0-7f9a-478c-a4f2-0d3865717565", true, false);
            this._fileProcessingDB.SetDBInfoSetting("AzureInstance", "https://login.microsoftonline.com", true, false);
            this.EmailManagementConfiguration.SharedEmailAddress = "emailsuppliertest@extractsystems.com";
            this.EmailManagementConfiguration.FilepathToDownloadEmails = "C:\\ProgramData\\Extract Systems\\Emails";
            this.EmailManagementConfiguration.Authority = "extractsystems.com";
            this.EmailManagementConfiguration.EmailBatchSize = 1;
            this.EmailManagementConfiguration.InputMailFolderName = "Inbox";
            this.EmailManagementConfiguration.QueuedMailFolderName = "Queued";
        }

        #endregion Constructors

        #region IEmailFileSupplier Members

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
                // TODO: Uncomment the below when configuration can be done via a UI.
                //if(string.IsNullOrEmpty(EmailManagementConfiguration.UserName)
                //    || string.IsNullOrEmpty(EmailManagementConfiguration.Password.AsString())
                //    || string.IsNullOrEmpty(EmailManagementConfiguration.SharedEmailAddress)
                //    || string.IsNullOrEmpty(EmailManagementConfiguration.InputMailFolderName)
                //    || string.IsNullOrEmpty(EmailManagementConfiguration.QueuedMailFolderName)
                //    || string.IsNullOrEmpty(EmailManagementConfiguration.Authority))
                //{
                //    configured = false;
                //}

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
                if (!(pObject is EmailFileSupplier task))
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
            _pauseProcessing = true;
        }

        /// Resumes file supplying after a pause
        public void Resume()
        {
            _pauseProcessing = false;
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

                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFAMTM);

                _fileTarget = pTarget;
                _fileProcessingDB = pDB;
                _ActionID = nActionID;

                // TODO: Remove this line with proper UI configuraiton.
                SetupConfigurationNOUI();

                this.EmailManagementConfiguration.FileProcessingDB = pDB;

                this.StartHelper();
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI53196", "Unable to start supplying object", ex);
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
                this._stopProcessing = true;
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
                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    };

                    this.EmailManagementConfiguration = JsonConvert.DeserializeObject<EmailManagementConfiguration>(reader.ReadString(), settings);

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
                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    };
                    writer.Write(JsonConvert.SerializeObject(this.EmailManagementConfiguration, settings));

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
            this.EmailManagementConfiguration = task.EmailManagementConfiguration;
            _dirty = true;
        }

        private void StartHelper()
        {
            _emailSupplierThread = new Thread(ManageEmailDownload);
            _emailSupplierThread.Start();
        }

        private void ManageEmailDownload()
        {
            
            EmailManagement emailManagement = new EmailManagement(this.EmailManagementConfiguration);

            while (!_pauseProcessing)
            {
                if (_stopProcessing)
                {
                    break;
                }

                try
                {
                    var messages = emailManagement.GetMessagesToProcessBatches().Result;
                    var files = emailManagement.DownloadMessagesToDisk(messages).Result;

                    for (int i = 0; i < files.Length; i++)
                    {
                        _fileTarget.NotifyFileAdded(files[i], this);

                        emailManagement.MoveMessageToQueuedFolder(messages[i]).Wait();
                    }
                }
                catch (Exception ex)
                {
                    ex.AsExtract("ELI53208").Log();
                }
            }
        }

        #endregion Private Members
    }
}
