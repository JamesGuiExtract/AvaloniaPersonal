using Extract.Interop;
using Extract.Licensing;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// Interface definition for the pagination data validity condition.
    /// </summary>
    [ComVisible(true)]
    [Guid("56DC67B8-134D-41E8-992E-0E4E86FFF8F0")]
    [CLSCompliant(false)]
    public interface IPaginationDataValidityCondition : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IPaginationCondition, ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Condition can only evaluate as true if there are no errors in the data.
        /// </summary>
        bool IfNoErrors { get; set; }

        /// <summary>
        /// Condition can only evaluate as true if there are no warnings in the data.
        /// </summary>
        bool IfNoWarnings { get; set; }
    }

    /// <summary>
    /// A <see cref="IFAMCondition"/> based on the page count of a file.
    /// </summary>
    [ComVisible(true)]
    [Guid("4D02F01A-E7A8-47FE-95B0-1B9FDE5AF7E8")]
    [ProgId("Extract.FileActionManager.Conditions.PaginationDataValidityCondition")]
    public class PaginationDataValidityCondition : IPaginationDataValidityCondition
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Data validity condition (pagination)";

        /// <summary>
        /// Current task version.
        /// </summary>
        internal const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// If true, condition can only evaluate as true if there are no errors in the data.
        /// </summary>
        bool _ifNoErrors = true;

        /// <summary>
        /// If true, condition can only evaluate as true if there are no warnings in the data.
        /// </summary>
        bool _ifNoWarnings = true;

        /// <summary>
        /// Indicates if changes have been made to this instance since creation/loading.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationDataValidityCondition"/> class.
        /// </summary>
        public PaginationDataValidityCondition()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47131");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationDataValidityCondition"/> class as a copy
        /// of the specified <see paramref="task"/>.
        /// </summary>
        /// <param name="task">The <see cref="PaginationDataValidityCondition"/> from which settings should be
        /// copied.</param>
        public PaginationDataValidityCondition(PaginationDataValidityCondition task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47132");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// If true, condition can only evaluate as true if there are no errors in the data.
        /// </summary>
        public bool IfNoErrors
        {
            get
            {
                return _ifNoErrors;
            }

            set
            {
                try
                {
                    if (value != _ifNoErrors)
                    {
                        _ifNoErrors = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47133");
                }
            }
        }

        /// <summary>
        /// If true, condition can only evaluate as true if there are no warning in the data.
        /// </summary>
        public bool IfNoWarnings
        {
            get
            {
                return _ifNoWarnings;
            }

            set
            {
                try
                {
                    if (value != _ifNoWarnings)
                    {
                        _ifNoWarnings = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47134");
                }
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Validates the instance's current settings.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if the instance's settings are not valid.</throws>
        public void ValidateSettings()
        {
            try
            {
                // At least one of the restrictions must be selected.
                if (!IfNoErrors && !IfNoWarnings)
                {
                    throw new ExtractException("ELI47135", _COMPONENT_DESCRIPTION + 
                        " has not been configured.");
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47136", ex.Message);
            }
        }

        #endregion Public Methods

        #region IPaginationCondition Members

        /// <summary>
        /// Returns bool value indicating if the condition requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the condition requires admin access
        /// <see langword="false"/> if condition does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        /// <summary>
        /// Used to allow PaginationTask to inform IPaginationCondition implementers when they are
        /// being used in the context of the IPaginationCondition interface.
        /// NOTE: While it is not necessary for implementers to persist this setting, this setting
        /// does need to be copied in the context of the ICopyableObject interface (CopyFrom)
        /// </summary>
        public bool IsPaginationCondition { get; set; }

        /// <summary>
        /// Tests proposed pagination output <see paramref="pFileRecord"/> to determine if it is
        /// qualified to be automatically generated.
        /// </summary>
        /// <param name="pSourceFileRecord">A <see cref="FileRecord"/> specifing the source document
        /// for the proposed output document tested here.
        /// </param>
        /// <param name="bstrProposedFileName">The filename planned to be assigned to the document.</param>
        /// <param name="bstrDocumentStatus">A json representation of the pagination DocumentStatus
        /// for the proposed output document.</param>
        /// <param name="bstrSerializedDocumentAttributes">A searialized copy of all attributes that fall
        /// under a root-level "Document" attribute including Pages, DeletedPages and DocumentData (the
        /// content of which would become the output document's voa)</param>
        /// <param name="pFPDB">The <see cref="FileProcessingDB"/> currently in use.</param>
        /// <param name="lActionID">The ID of the database action in use.</param>
        /// <param name="pFAMTagManager">A <see cref="FAMTagManager"/> to be used to evaluate any
        /// FAM tags used by the condition.</param>
        /// <returns><see langword="true"/> if the condition was met, <see langword="false"/> if it
        /// was not.</returns>
        public bool FileMatchesPaginationCondition(FileRecord pSourceFileRecord, string bstrProposedFileName,
            string bstrDocumentStatus, string bstrSerializedDocumentAttributes,
            FileProcessingDB pFPDB, int lActionID, FAMTagManager pFAMTagManager)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bstrDocumentStatus))
                {
                    return true;
                }
                else
                {
                    var documentStatus = DocumentStatus.FromJson(bstrDocumentStatus);
                    if (IfNoErrors && documentStatus.DataError)
                    {
                        return false;
                    }

                    if (IfNoWarnings && documentStatus.DataWarning)
                    {
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47137", "Error occured in '" + _COMPONENT_DESCRIPTION + "'");
            }
        }

        #endregion IPaginationCondition Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="PaginationDataValidityCondition"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI47138",
                    _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                PaginationDataValidityCondition cloneOfThis = (PaginationDataValidityCondition)Clone();

                using (PaginationDataValidityConditionSettingsDialog dlg
                    = new PaginationDataValidityConditionSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47139", "Error running configuration.");
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
                try
                {
                    // Return true if ValidateSettings does not throw an exception.
                    ValidateSettings();

                    return true;
                }
                catch
                {
                    // Otherwise return false and eat the exception.
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47140",
                    "Failed to check '" + _COMPONENT_DESCRIPTION + "' configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="PaginationDataValidityCondition"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="PaginationDataValidityCondition"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new PaginationDataValidityCondition(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47141",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="PaginationDataValidityCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as PaginationDataValidityCondition;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to PaginationDataValidityCondition");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47142",
                    "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion ICopyableObject Members

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

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.ExtractCoreObjects);
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
        /// <returns>
        ///   <see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    IfNoErrors = reader.ReadBoolean();
                    IfNoWarnings = reader.ReadBoolean();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47143",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the object should reset
        /// its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                ValidateSettings();

                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(IfNoErrors);
                    writer.Write(IfNoWarnings);

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
                throw ex.CreateComVisible("ELI47144",
                    "Failed to save '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            throw new NotImplementedException();
        }

        #endregion IPersistStream Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.PaginationConditionsGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.PaginationConditionsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="PaginationDataValidityCondition"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="PaginationDataValidityCondition"/> from which to copy.
        /// </param>
        void CopyFrom(PaginationDataValidityCondition source)
        {
            IfNoErrors = source.IfNoErrors;
            IfNoWarnings = source.IfNoWarnings;

            _dirty = true;
        }

        #endregion Private Members
    }
}
