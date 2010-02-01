using Extract.Interop;
using Extract.Licensing;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a file processing task that extends redactions to cover surrounding context.
    /// </summary>
    [ComVisible(true)]
    [Guid("53E116BE-BB3F-49A6-B24A-C50EE69F50BC")]
    [ProgId("Extract.Redaction.SurroundContextTask")]
    public class SurroundContextTask : ICategorizedComponent, IConfigurableObject, ICopyableObject,
                     IFileProcessingTask, ILicensedComponent, IPersistStream
    {
        #region Constants

        const string _COMPONENT_DESCRIPTION = "Redaction: Extend redactions to surround context";

        const int _CURRENT_VERSION = 1;
        
        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="SurroundContextTask"/> 
        /// since it was created; <see langword="false"/> if no changes have been made since it
        /// was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Settings to extend redactions to cover surrounding context.
        /// </summary>
        SurroundContextSettings _settings;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurroundContextTask"/> class.
        /// </summary>
        public SurroundContextTask()
        {
            _settings = new SurroundContextSettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurroundContextTask"/> class.
        /// </summary>
        public SurroundContextTask(SurroundContextTask task)
        {
            CopyFrom(task);
        }
        
        #endregion Constructors
        
        #region Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// <see cref="ExtractGuids.FileProcessors"/> COM category.
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
        /// <see cref="ExtractGuids.FileProcessors"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Copies the specified <see cref="SurroundContextTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="SurroundContextTask"/> from which to copy.</param>
        public void CopyFrom(SurroundContextTask task)
        {
            _settings = task._settings;
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
        /// Performs configuration needed to create a valid <see cref="SurroundContextTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI29500",
					_COMPONENT_DESCRIPTION);

                // Allow the user to configure the settings
                using (SurroundContextSettingsDialog dialog = new SurroundContextSettingsDialog(_settings))
                {
                    bool result = dialog.ShowDialog() == DialogResult.OK;

                    // Store the result
                    if (result)
                    {
                        _settings = dialog.SurroundContextSettings;
                        _dirty = true;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29501",
                    "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="SurroundContextTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="SurroundContextTask"/> instance.</returns>
        public object Clone()
        {
            return new SurroundContextTask(this);
        }

        /// <summary>
        /// Copies the specified <see cref="SurroundContextTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                CopyFrom((SurroundContextTask)pObject);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29525",
                    "Unable to copy task.", ex);
            }
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI29502",
                    _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29503", 
                    "Unable to cancel 'Extend redactions to surround context' task.", ex);
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI29504",
                    _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29505",
                    "Unable to close 'Extend redactions to surround context' task.", ex);
            }
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI29506",
                    _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29507",
                    "Unable to initialize 'Extend redactions to surround context' task.", ex);
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI29508",
                    _COMPONENT_DESCRIPTION);

                // TODO: Implement
                
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29509",
                    "Unable to extend redactions to cover surrounding context.", ex);
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
            try
            {
                return LicenseUtilities.IsLicensed(LicenseIdName.IDShieldCoreObjects);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29524",
                    "Error checking licensing state.", ex);
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
                    _settings = SurroundContextSettings.ReadFrom(reader);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29510",
                    "Unable to load 'Extend redactions to surround context' task.", ex);
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
                throw ExtractException.CreateComVisible("ELI29511",
                    "Unable to save 'Extend redactions to surround context' task.", ex);
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
