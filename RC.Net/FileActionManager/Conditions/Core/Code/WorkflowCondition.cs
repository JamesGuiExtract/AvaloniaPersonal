using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// A <see cref="IFAMCondition"/> based on a file's workflow. Used to add special conditions to
    /// FPS files processing all workflows.
    /// </summary>
    [ComVisible(true)]
    [Guid("CAD27F57-45E7-48BB-987F-9B8395D20D52")]
    [CLSCompliant(false)]
    public interface IWorkflowCondition : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFAMCondition, IPaginationCondition,
        ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="SelectedWorkflows"/> should be
        /// inclusive as those that satisfy the condition.
        /// </summary>
        /// <value><c>true</c> if the selected workflows are the ones that should satisfy the
        /// condition; <c>false</c> if <see cref="SelectedWorkflows"/> are those that should cause
        /// the condition to not be met.
        /// </value>
        bool Inclusive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an <see cref="IUnknownVector"/> of workflow names to test against a file's
        /// workflow.
        /// </summary>
        /// <value>
        /// An <see cref="IUnknownVector"/> of workflow names to test against a file's workflow.
        /// </value>
        IVariantVector SelectedWorkflows
        {
            get;
            set;
        }
    }

    /// <summary>
    /// A <see cref="IFAMCondition"/> based on a file's workflow. Used to add special conditions to
    /// FPS files processing all workflows.
    /// </summary>
    [ComVisible(true)]
    [Guid("3F6D3012-076F-4835-A0E6-3BF7C0336BD9")]
    [ProgId("Extract.FileActionManager.Conditions.WorkflowCondition")]
    public class WorkflowCondition : IWorkflowCondition
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Workflow condition";

        /// <summary>
        /// Current task version.
        /// </summary>
        internal const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates whether the <see cref="SelectedWorkflows"/> should be inclusive as those that
        /// satisfy the condition.
        /// </summary>
        bool _inclusive = true;

        /// <summary>
        /// The workflow names to test against a file's workflow.
        /// </summary>
        IVariantVector _selectedWorkflows = new VariantVector();

        /// <summary>
        /// A cached map of workflow IDs to workflow names to prevent having to repeatedly look up
        /// workflows at runtime.
        /// </summary>
        Dictionary<int, string> _cachedWorkflowNames;

        /// <summary>
        /// To avoid expensive settings validation check, don't repeat validation if it has already
        /// been done.
        /// </summary>
        bool _settingsValidated;

        /// <summary>
        /// <see langword="true"/> if changes have been made to
        /// <see cref="WorkflowCondition"/> since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowCondition"/> class.
        /// </summary>
        public WorkflowCondition()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43453");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowCondition"/> class as a copy
        /// of the specified <see paramref="task"/>.
        /// </summary>
        /// <param name="task">The <see cref="WorkflowCondition"/> from which settings should be
        /// copied.</param>
        public WorkflowCondition(WorkflowCondition task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43454");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="SelectedWorkflows"/> should be
        /// inclusive as those that satisfy the condition.
        /// </summary>
        /// <value><c>true</c> if the selected workflows are the ones that should satisfy the
        /// condition; <c>false</c> if <see cref="SelectedWorkflows"/> are those that should cause
        /// the condition to not be met.
        /// </value>
        public bool Inclusive
        {
            get
            {
                return _inclusive;
            }

            set
            {
                try
                {
                    if (value != _inclusive)
                    {
                        _inclusive = value;
                        _dirty = true;
                        _settingsValidated = false;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI43455");
                }
            }
        }

        /// <summary>
        /// Gets or sets an <see cref="IUnknownVector"/> of workflow names to test against a file's
        /// workflow.
        /// </summary>
        /// <value>
        /// An <see cref="IUnknownVector"/> of workflow names to test against a file's workflow.
        /// </value>
        public IVariantVector SelectedWorkflows
        {
            get
            {
                return _selectedWorkflows;
            }

            set
            {
                try
                {

                    _selectedWorkflows = (IVariantVector)value.Clone();
                    _dirty = true;
                    _settingsValidated = false;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI43456");
                }
            }
        }

        /// <summary>
        /// Used to allow PaginationTask to inform IPaginationCondition implementers when they are
        /// being used in the context of the IPaginationCondition interface.
        /// NOTE: While it is not necessary for implementers to persist this setting, this setting
        /// does need to be copied in the context of the ICopyableObject interface (CopyFrom)
        /// </summary>
        public bool IsPaginationCondition { get; set; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Validates the instance's current settings.
        /// </summary>
        public void ValidateSettings()
        {
            try
            {
                // fileProcessingDb.ConnectLastUsedDBThisProcess() is fairly expensive-- db schema checks etc.
                // Avoid running this for every file while processing; only re-validate if setting have changed.
                if (!_settingsValidated)
                {
                    var fileProcessingDb = new FileProcessingDB();
                    fileProcessingDb.ConnectLastUsedDBThisProcess();

                    if (!fileProcessingDb.UsingWorkflows)
                    {
                        throw new ExtractException("ELI43475", "The workflow condition requires workflows in the database.");
                    }

                    if (SelectedWorkflows.Size == 0)
                    {
                        throw new ExtractException("ELI43457", _COMPONENT_DESCRIPTION +
                            " must have at least one workflow specified.");
                    }

                    _settingsValidated = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI43458", ex.Message);
            }
        }

        #endregion Public Methods

        #region IFAMCondition Members

        /// <summary>
        /// Compares the workflow the document represented by <see paramref="pFileRecord"/> belongs
        /// to determine if the condition is met.
        /// </summary>
        /// <param name="pFileRecord">A <see cref="FileRecord"/> specifying the file to be tested.
        /// </param>
        /// <param name="pFPDB">The <see cref="FileProcessingDB"/> currently in use.</param>
        /// <param name="lActionID">The ID of the database action in use.</param>
        /// <param name="pFAMTagManager">A <see cref="FAMTagManager"/> to be used to evaluate any
        /// FAM tags used by the condition.</param>
        /// <returns><see langword="true"/> if the condition was met, <see langword="false"/> if it
        /// was not.</returns>
        public bool FileMatchesFAMCondition(FileRecord pFileRecord, FileProcessingDB pFPDB,
            int lActionID, FAMTagManager pFAMTagManager)
        {
            try
            {
                return FileMatchesCondition(pFileRecord, pFPDB);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI43460",
                    "Error occurred in '" + _COMPONENT_DESCRIPTION + "'", ex);
            }
        }

        /// <summary>
        /// Returns bool value indicating if the condition requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the condition requires admin access
        /// <see langword="false"/> if condition does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IFAMCondition Members

        #region IPaginationCondition Members

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
                return FileMatchesCondition(pSourceFileRecord, pFPDB);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI47083",
                    "Error occured in '" + _COMPONENT_DESCRIPTION + "'", ex);
            }
        }

        #endregion IPaginationCondition Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="WorkflowCondition"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI43464",
                    _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                WorkflowCondition cloneOfThis = (WorkflowCondition)Clone();

                using (WorkflowConditionSettingsDialog dlg
                    = new WorkflowConditionSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI43463", "Error running configuration.");
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
                throw ex.CreateComVisible("ELI43462",
                    "Failed to check '" + _COMPONENT_DESCRIPTION + "' configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="WorkflowCondition"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="WorkflowCondition"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new WorkflowCondition(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI43461",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="WorkflowCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as WorkflowCondition;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to WorkflowCondition");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI43465",
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
                    Inclusive = reader.ReadBoolean();
                    SelectedWorkflows = reader.ReadStringArray().ToVariantVector();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
                _settingsValidated = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI43466",
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
                    writer.Write(Inclusive);
                    writer.Write(SelectedWorkflows.ToIEnumerable<string>().ToArray());

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
                throw ex.CreateComVisible("ELI43467",
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
        /// Gets the name of the workflow with the specified <see paramref="workflowId"/>.
        /// </summary>
        /// <param name="fileProcessingDb">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="workflowID">The ID of the workflow for which the name is needed.</param>
        /// <returns>The name of the workflow.</returns>
        string GetWorkflowName(FileProcessingDB fileProcessingDb, int workflowId)
        {
            // Cache the workflow names if we haven't already.
            if (_cachedWorkflowNames == null)
            {
                _cachedWorkflowNames = fileProcessingDb.GetWorkflows()
                    .ComToDictionary()
                    .ToDictionary(
                        entry => int.Parse(entry.Value, CultureInfo.InvariantCulture),
                        entry => entry.Key);
            }

            return _cachedWorkflowNames[workflowId];
        }

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileActionManagerConditionsGuid);
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
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileActionManagerConditionsGuid);
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.PaginationConditionsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="WorkflowCondition"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="WorkflowCondition"/> from which to copy.
        /// </param>
        void CopyFrom(WorkflowCondition source)
        {
            Inclusive = source.Inclusive;
            SelectedWorkflows = (IVariantVector)source.SelectedWorkflows;

            _dirty = true;
            _settingsValidated = false;
        }

        /// <summary>
        /// Indicates whether the specified <paramref name="pFileRecord"/> satisifies the condition.
        /// </summary>
        /// <param name="pFileRecord">The <see cref="FileRecord"/> representing the file to be tested.</param>
        /// <param name="pFPDB">The database being used.</param>
        /// <returns><c>true</c> if the condition is met; <c>false</c> if the condition is not met.</returns>
        bool FileMatchesCondition(FileRecord pFileRecord, FileProcessingDB pFPDB)
        {
            // Validate the license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI43459",
                _COMPONENT_DESCRIPTION);

            ValidateSettings();

            ExtractException.Assert("ELI43473", "Workflow condition: workflows expected",
                pFileRecord.WorkflowID > 0);

            string workflowName = GetWorkflowName(pFPDB, pFileRecord.WorkflowID);

            return SelectedWorkflows.Contains(workflowName) == Inclusive;
        }

        #endregion Private Members
    }
}
