using Extract.AttributeFinder;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.Redaction
{
    /// <summary>
    /// An IFAMCondition whose result depends on whether the spatial attributes in the two specified
    /// data files match to the degree specified and optionally output the result of merging the
    /// two files.
    /// </summary>
    [ComVisible(true)]
    [Guid("1EF63B0F-EDFE-40AE-A48B-98D4E4092AEA")]
    [ProgId("Extract.Redaction.VOAFileCompareCondition")]
    public class VOAFileCompareCondition : IFAMCondition, IConfigurableObject,
        IMustBeConfiguredObject, IAccessRequired, ICategorizedComponent, ICopyableObject,
        ILicensedComponent, IPersistStream
    {
        #region Constants

        /// <summary>
        /// The COM object name.
        /// </summary>
        internal const string _COMPONENT_DESCRIPTION = "Compare ID Shield data files condition";

        /// <summary>
        /// Current task version.
        /// </summary>
        internal const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to
        /// <see cref="VOAFileCompareCondition"/> since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The ID Shield ini settings.
        /// </summary>
        InitializationSettings _idShieldSettings = new InitializationSettings();

        /// <summary>
        /// The settings for this object.
        /// </summary>
        VOAFileCompareConditionSettings _settings;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileCompareCondition"/> class.
        /// </summary>
        public VOAFileCompareCondition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileCompareCondition"/> class.
        /// </summary>
        /// <param name="task">The <see cref="VOAFileCompareCondition"/> from which
        /// settings should be copied.</param>
        public VOAFileCompareCondition(VOAFileCompareCondition task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31718");
            }
        }

        #endregion Constructors

        #region IFAMCondition Members

        /// <summary>
        /// Compares the specified data files for the specified <see paramref="pFileRecord"/>.
        /// </summary>
        /// <param name="pFileRecord">The <see cref="FileRecord"/> specifying the database file for
        /// which the data files should be compared.</param>
        /// <param name="pFPDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="lActionID">The ID of the action from <see cref="FileProcessingDB"/> that is
        /// being used.</param>
        /// <param name="pFAMTagManager">The <see cref="FAMTagManager"/> to be used to resolve the
        /// filenames of the data files to be compared.</param>
        /// <returns>The result of whether spatial attributes in the files match and the
        /// <see cref="VOAFileCompareConditionSettings.ConditionMetIfMatching"/> setting.</returns>
        [CLSCompliant(false)]
        public bool FileMatchesFAMCondition(FileRecord pFileRecord, FileProcessingDB pFPDB,
            int lActionID, FAMTagManager pFAMTagManager)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI31775",
                    _COMPONENT_DESCRIPTION);

                SpatialAttributeMergeUtils attributeMerger = VOAFileMergeTask.InitializeAttributeMerger(
                    _idShieldSettings, _settings.OverlapThreshold);

                bool conditionResult = VOAFileMergeTask.CompareMergeFiles(
                    _settings, _idShieldSettings, attributeMerger, pFileRecord.Name, pFAMTagManager);
                
                return conditionResult;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31776", "ID Shield data file compare failed.");
            }
        }

        #endregion IFAMCondition Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid
        /// <see cref="VOAFileCompareCondition"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI31727",
                    _COMPONENT_DESCRIPTION);

                // Allow the user to set the verification settings
                using (var dialog = new VOAFileCompareConditionSettingsDialog(_settings))
                {
                    bool result = dialog.ShowDialog() == DialogResult.OK;

                    // Store the result
                    if (result)
                    {
                        _settings = dialog.VOAFileCompareConditionSettings;
                        _dirty = true;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31728", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                return (_settings != null &&
                        !string.IsNullOrWhiteSpace(_settings.DataFile1) &&
                        !string.IsNullOrWhiteSpace(_settings.DataFile2) &&
                        (!_settings.CreateOutput || !string.IsNullOrWhiteSpace(_settings.OutputFile)));
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32132",
                    "Failed to check " + _COMPONENT_DESCRIPTION + " configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

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

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="VOAFileCompareCondition"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="VOAFileCompareCondition"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new VOAFileCompareCondition(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31729",
                    "Failed to clone " + _COMPONENT_DESCRIPTION + " object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="VOAFileCompareCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                CopyFrom((VOAFileCompareCondition)pObject);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31730",
                    "Failed to copy " + _COMPONENT_DESCRIPTION + " object.");
            }
        }

        #endregion ICopyableObject Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.IDShieldCoreObjects);
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
                    _settings = VOAFileCompareConditionSettings.ReadFrom(reader);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31731", "Unable to load verification task.");
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
                throw ex.CreateComVisible("ELI31732",
                    "Unable to save replaced indexed text settings.");
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
        /// <see cref="ExtractGuids.FileActionManagerConditions"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileActionManagerConditions);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// <see cref="ExtractGuids.FileActionManagerConditions"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileActionManagerConditions);
        }

        /// <summary>
        /// Copies the specified <see cref="VOAFileCompareCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="task">The <see cref="VOAFileCompareCondition"/> from which to
        /// copy.</param>
        public void CopyFrom(VOAFileCompareCondition task)
        {
            try
            {
                if (task._settings == null)
                {
                    _settings = null;
                }
                else
                {
                    _settings = new VOAFileCompareConditionSettings(task._settings);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32136");
            }
        }

        #endregion Private Members
    }
}
