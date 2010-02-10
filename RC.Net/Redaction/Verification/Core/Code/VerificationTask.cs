using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a file processing task that performs verification of redactions.
    /// </summary>
    [ComVisible(true)]
    [Guid("AD7F3F3F-20EC-4830-B014-EC118F6D4567")]
    [ProgId("Extract.Redaction.Verification.VerificationTask")]
    public class VerificationTask : ICategorizedComponent, IConfigurableObject, ICopyableObject,
        IFileProcessingTask, ILicensedComponent, IPersistStream
    {
        #region Constants

        const string _COMPONENT_DESCRIPTION = "Redaction: Verify sensitive data";

        /// <summary>
        /// Current task version.
        /// <para>Version 3</para>
        /// Added enable input event tracking setting.
        /// </summary>
        const int _CURRENT_VERSION = 5;
        
        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="VerificationTask"/> 
        /// since it was created; <see langword="false"/> if no changes have been made since it
        /// was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Settings for verification.
        /// </summary>
        VerificationSettings _settings;

        /// <summary>
        /// The form to display for verifying documents.
        /// </summary>
        static VerificationForm<VerificationTaskForm> _form;

        /// <summary>
        /// Object used to mutex around the verification form creation.
        /// </summary>
        static readonly object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationTask"/> class.
        /// </summary>
        public VerificationTask()
        {
            _settings = new VerificationSettings();

            // Lock around form creation
            lock (_lock)
            {
                if (_form == null)
                {
                    _form = new VerificationForm<VerificationTaskForm>();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationTask"/> class.
        /// </summary>
        public VerificationTask(VerificationTask task)
        {
            CopyFrom(task);
        }
        
        #endregion Constructors
        
        #region Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Copies the specified <see cref="VerificationTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="VerificationTask"/> from which to copy.</param>
        public void CopyFrom(VerificationTask task)
        {
            _settings = task._settings;
        }

        /// <summary>
        /// Creates a <see cref="VerificationTaskForm"/> with the current settings.
        /// </summary>
        /// <returns>A <see cref="VerificationTaskForm"/> with the current settings.</returns>
        IVerificationForm CreateVerificationTaskForm()
        {
            return new VerificationTaskForm(_settings);
        }
        
        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets/sets the <see cref="VerificationSettings"/>.
        /// </summary>
        public VerificationSettings Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
            }
        }

        #endregion Properties

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
        /// Performs configuration needed to create a valid <see cref="VerificationTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldVerificationObject, "ELI26890",
					_COMPONENT_DESCRIPTION);

                // Allow the user to set the verification settings
                using (VerificationSettingsDialog dialog = new VerificationSettingsDialog(_settings))
                {
                    bool result = dialog.ShowDialog() == DialogResult.OK;

                    // Store the result
                    if (result)
                    {
                        _settings = dialog.VerificationSettings;
                        _dirty = true;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26511",
                    "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="VerificationTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="VerificationTask"/> instance.</returns>
        public object Clone()
        {
            return new VerificationTask(this);
        }

        /// <summary>
        /// Copies the specified <see cref="VerificationTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            CopyFrom((VerificationTask)pObject);
        }

        #endregion ICopyableObject Members
        
        #region IFileProcessingTask Members

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldVerificationObject, "ELI26893",
					_COMPONENT_DESCRIPTION);

                _form.Cancel();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26598", 
                    "Error canceling verification.", ex);
            }
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldVerificationObject, "ELI26901",
					_COMPONENT_DESCRIPTION);

                _form.CloseForm();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26599",
                    "Error closing verification.", ex);
            }
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldVerificationObject, "ELI26891",
					_COMPONENT_DESCRIPTION);

                _form.ShowForm(CreateVerificationTaskForm);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26600",
                    "Error initializing verification.", ex);
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="bstrFileFullName">The file to process.</param>
        /// <param name="nFileID">The ID of the file being processed.</param>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">A File Action Manager Tag Manager for expanding tags.</param>
        /// <param name="pDB">The File Action Manager database.</param>
        /// <param name="pProgressStatus">Object to provide progress status updates to caller.
        /// </param>
        /// <param name="bCancelRequested"><see langword="true"/> if cancel was requested; 
        /// <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if processing should continue; <see langword="false"/> 
        /// if all file processing should be cancelled.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(string bstrFileFullName, int nFileID, int nActionID,
            FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldVerificationObject, "ELI26894",
					_COMPONENT_DESCRIPTION);

                if (bCancelRequested)
                {
                    return EFileProcessingResult.kProcessingCancelled;
                }

                EFileProcessingResult result = _form.ShowDocument(bstrFileFullName, nFileID,
                    nActionID, pFAMTM, pDB);

                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26601",
                    "Unable to verify document.", ex);
            }
        }

        #endregion IFileProcessingTask Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.IDShieldVerificationObject);
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
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the settings
                    _settings = VerificationSettings.ReadFrom(reader);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26465", 
                    "Unable to load verification task.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            try 
	        {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Serialize the settings
                    _settings.WriteTo(writer);

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
		        throw ExtractException.CreateComVisible("ELI26473", 
			        "Unable to save verification task.", ex);
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
    }
}
