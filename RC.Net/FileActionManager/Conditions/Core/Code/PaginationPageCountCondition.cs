using Extract.AttributeFinder;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// Specifies which pages of a proposed document can be deleted if the condition is to evaluate to true.
    /// </summary>
    [ComVisible(true)]
    [Guid("B8112AA7-E6FC-4B2C-85C1-1AE870E88DA2")]
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum DeletedPageAllowance
    {
        /// <summary>
        /// Any number of deleted pages are allowed.
        /// </summary>
        NotRestricted = 0,

        /// <summary>
        /// Condition false if any page other than the first is deleted.
        /// </summary>
        OnlyFirst = 1,

        /// <summary>
        /// Condition false if any page other than the last is deleted.
        /// </summary>
        OnlyLast = 2,

        /// <summary>
        /// Condition false if any page other than the first or last is deleted.
        /// </summary>
        OnlyFirstOrLast = 3
    }

    /// <summary>
    /// Interface definition for the pagination version of page count condition.
    /// </summary>
    [ComVisible(true)]
    [Guid("1A5ADA12-E2B6-473B-B1B8-CF456C4FBA47")]
    [CLSCompliant(false)]
    public interface IPaginationPageCountCondition : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IPaginationCondition, ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Specifies the <see cref="PageCountComparisonOperator"/> to use when comparing a
        /// document's page count to the specified <see cref="PageCount"/>.
        /// </summary>
        PageCountComparisonOperator OutputPageCountComparisonOperator { get; set; }

        /// <summary>
        /// Specifies the <see langword="int"/> that should be compared to a document's page count.
        /// </summary>
        /// <value>
        /// The <see langword="int"/> that should be compared to a document's page count.
        /// </value>
        int OutputPageCount { get; set; }

        /// <summary>
        /// Specifies the comparison operator to use when testing if the number of deleted pages is valid.
        /// </summary>
        PageCountComparisonOperator DeletedPageCountComparisonOperator { get; set; }

        /// <summary>
        /// Specifies the <see langword="int"/> that should be compared to a document's deleted page count.
        /// </summary>
        int DeletedPageCount { get; set; }

        /// <summary>
        /// Specifies which pages of a proposed document can be deleted if the condition is to evaluate to true.
        /// </summary>
        DeletedPageAllowance DeletedPageAllowance { get; set; }
    }

    /// <summary>
    /// A <see cref="IFAMCondition"/> based on the page count of a file.
    /// </summary>
    [ComVisible(true)]
    [Guid("0B21EAA1-3181-4CFC-AA34-8F69ECDDB965")]
    [ProgId("Extract.FileActionManager.Conditions.PaginationPageCountCondition")]
    public class PaginationPageCountCondition : IPaginationPageCountCondition
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Page count condition (pagination)";

        /// <summary>
        /// Current task version.
        /// </summary>
        internal const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Specifies the <see cref="PageCountComparisonOperator"/> to use when comparing a
        /// document's page count to the specified <see cref="PageCount"/>.
        /// </summary>
        PageCountComparisonOperator _outputPageCountConditionOperator = PageCountComparisonOperator.NotDefined;

        /// <summary>
        /// Specifies the <see langword="int"/> that should be compared to a document's page count.
        /// </summary>
        int _outputPageCount = 1;

        /// <summary>
        /// Specifies the <see cref="PageCountComparisonOperator"/> to use when comparing a
        /// document's page count to the specified <see cref="PageCount"/>.
        /// </summary>
        PageCountComparisonOperator _deletedPageCountConditionOperator = PageCountComparisonOperator.NotDefined;

        /// <summary>
        /// Specifies the <see langword="int"/> that should be compared to a document's page count.
        /// </summary>
        int _deletedPageCount = 1;

        /// <summary>
        /// Specifies which pages of a proposed document can be deleted if the condition is to evaluate to true.
        /// </summary>
        DeletedPageAllowance _deletedPageAllowance;

        /// <summary>
        /// Indicates if there have been changes to the instance since it was created/loaded.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationPageCountCondition"/> class.
        /// </summary>
        public PaginationPageCountCondition()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47099");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationPageCountCondition"/> class as a copy
        /// of the specified <see paramref="task"/>.
        /// </summary>
        /// <param name="task">The <see cref="PaginationPageCountCondition"/> from which settings should be
        /// copied.</param>
        public PaginationPageCountCondition(PaginationPageCountCondition task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47100");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Specifies the <see cref="PageCountComparisonOperator"/> to use when comparing a
        /// document's page count to the specified <see cref="PageCount"/>.
        /// </summary>
        /// <value>
        /// The <see cref="PageCountComparisonOperator"/> to use when comparing a document's page
        /// count to the specified <see cref="PageCount"/>.
        /// </value>
        public PageCountComparisonOperator OutputPageCountComparisonOperator
        {
            get
            {
                return _outputPageCountConditionOperator;
            }

            set
            {
                try
                {
                    if (value != _outputPageCountConditionOperator)
                    {
                        _outputPageCountConditionOperator = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47101");
                }
            }
        }

        /// <summary>
        /// Specifies the <see langword="int"/> that should be compared to a document's page count.
        /// </summary>
        /// <value>
        /// The <see langword="int"/> that should be compared to a document's page count.
        /// </value>
        public int OutputPageCount
        {
            get
            {
                return _outputPageCount;
            }

            set
            {
                try
                {
                    if (value != _outputPageCount)
                    {
                        _outputPageCount = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47102");
                }
            }
        }

        /// <summary>
        /// Specifies the <see cref="PageCountComparisonOperator"/> to use when comparing a
        /// document's page count to the specified <see cref="PageCount"/>.
        /// </summary>
        /// <value>
        /// The <see cref="PageCountComparisonOperator"/> to use when comparing a document's page
        /// count to the specified <see cref="PageCount"/>.
        /// </value>
        public PageCountComparisonOperator DeletedPageCountComparisonOperator
        {
            get
            {
                return _deletedPageCountConditionOperator;
            }

            set
            {
                try
                {
                    if (value != _deletedPageCountConditionOperator)
                    {
                        _deletedPageCountConditionOperator = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47117");
                }
            }
        }

        /// <summary>
        /// Specifies the <see langword="int"/> that should be compared to a document's page count.
        /// </summary>
        /// <value>
        /// The <see langword="int"/> that should be compared to a document's page count.
        /// </value>
        public int DeletedPageCount
        {
            get
            {
                return _deletedPageCount;
            }

            set
            {
                try
                {
                    if (value != _deletedPageCount)
                    {
                        _deletedPageCount = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47118");
                }
            }
        }

        /// <summary>
        /// Specifies which pages of a proposed document can be deleted if the condition is to evaluate to true.
        /// </summary>
        public DeletedPageAllowance DeletedPageAllowance
        {
            get
            {
                return _deletedPageAllowance;
            }

            set
            {
                try
                {
                    if (value != _deletedPageAllowance)
                    {
                        _deletedPageAllowance = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47119");
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
                if (OutputPageCountComparisonOperator == Conditions.PageCountComparisonOperator.NotDefined &&
                    DeletedPageCountComparisonOperator == Conditions.PageCountComparisonOperator.NotDefined &&
                    DeletedPageAllowance == DeletedPageAllowance.NotRestricted)
                {
                    throw new ExtractException("ELI47104", _COMPONENT_DESCRIPTION + 
                        " has not been configured.");
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47105", ex.Message);
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
                int sourcePageCount = pSourceFileRecord.Pages;

                var documentAttributes = (IUnknownVector)new MiscUtils().GetObjectFromStringizedByteStream(bstrSerializedDocumentAttributes);
                documentAttributes.ReportMemoryUsage();

                var deletedPages = new HashSet<int>(
                    UtilityMethods.GetPageNumbersFromString(
                        documentAttributes
                        .ToIEnumerable<IAttribute>()
                        .Where(attribute => attribute.Name.Equals(
                            "DeletedPages", StringComparison.OrdinalIgnoreCase))
                        .Select(attribute => attribute.Value.String)
                        .SingleOrDefault() ?? "", sourcePageCount, true));

                var outputPages =
                    UtilityMethods.GetPageNumbersFromString(
                        documentAttributes
                        .ToIEnumerable<IAttribute>()
                        .Where(attribute => attribute.Name.Equals(
                            "Pages", StringComparison.OrdinalIgnoreCase))
                        .Select(attribute => attribute.Value.String)
                        .SingleOrDefault() ?? "", sourcePageCount, true)
                    .Except(deletedPages)
                    .ToList();

                return IsConditionMet(OutputPageCountComparisonOperator, outputPages.Count(), OutputPageCount)
                    && IsConditionMet(DeletedPageCountComparisonOperator, deletedPages.Count(), DeletedPageCount)
                    && IsConditionMet(DeletedPageAllowance, deletedPages, outputPages);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47106", "Error occured in '" + _COMPONENT_DESCRIPTION + "'");
            }
        }

        #endregion IPaginationCondition Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="PaginationPageCountCondition"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI47107",
                    _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                PaginationPageCountCondition cloneOfThis = (PaginationPageCountCondition)Clone();

                using (PaginationPageCountConditionSettingsDialog dlg
                    = new PaginationPageCountConditionSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI47108", "Error running configuration.");
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
                throw ex.CreateComVisible("ELI47109",
                    "Failed to check '" + _COMPONENT_DESCRIPTION + "' configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="PaginationPageCountCondition"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="PaginationPageCountCondition"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new PaginationPageCountCondition(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47110",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="PaginationPageCountCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as PaginationPageCountCondition;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to PaginationPageCountCondition");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47111",
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
                    OutputPageCountComparisonOperator = (PageCountComparisonOperator)reader.ReadInt32();
                    OutputPageCount = reader.ReadInt32();
                    DeletedPageCountComparisonOperator = (PageCountComparisonOperator)reader.ReadInt32();
                    DeletedPageCount = reader.ReadInt32();
                    DeletedPageAllowance = (DeletedPageAllowance)reader.ReadInt32();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47112",
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
                    writer.Write((int)OutputPageCountComparisonOperator);
                    writer.Write(OutputPageCount);
                    writer.Write((int)DeletedPageCountComparisonOperator);
                    writer.Write(DeletedPageCount);
                    writer.Write((int)DeletedPageAllowance);

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
                throw ex.CreateComVisible("ELI47113",
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
        /// Copies the specified <see cref="PaginationPageCountCondition"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="PaginationPageCountCondition"/> from which to copy.
        /// </param>
        void CopyFrom(PaginationPageCountCondition source)
        {
            OutputPageCountComparisonOperator = source.OutputPageCountComparisonOperator;
            OutputPageCount = source.OutputPageCount;
            DeletedPageCountComparisonOperator = source.DeletedPageCountComparisonOperator;
            DeletedPageCount = source.DeletedPageCount;
            DeletedPageAllowance = source.DeletedPageAllowance;

            _dirty = true;
        }

        /// <summary>
        /// Detemines whether the condition is true based on the specified page count comparison.
        /// </summary>
        static bool IsConditionMet(PageCountComparisonOperator pageCountComparisonOperator, int actualPageCount, int referencePageCount)
        {
            switch (pageCountComparisonOperator)
            {
                case PageCountComparisonOperator.Equal:
                    return actualPageCount == referencePageCount;

                case PageCountComparisonOperator.NotEqual:
                    return actualPageCount != referencePageCount;

                case PageCountComparisonOperator.LessThan:
                    return actualPageCount < referencePageCount;

                case PageCountComparisonOperator.LessThanOrEqual:
                    return actualPageCount <= referencePageCount;

                case PageCountComparisonOperator.GreaterThan:
                    return actualPageCount > referencePageCount;

                case PageCountComparisonOperator.GreaterThanOrEqual:
                    return actualPageCount >= referencePageCount;

                case PageCountComparisonOperator.NotDefined:
                    return true;

                default:
                    ExtractException.ThrowLogicException("ELI47114");
                    return false;
            }
        }

        /// <summary>
        /// Specifies whether the condition is true based on the specified <paramref name="deletedPageAllowance"/>.
        /// </summary>
        static bool IsConditionMet(DeletedPageAllowance deletedPageAllowance, IEnumerable<int> deletedPages, IEnumerable<int> outputPages)
        {
            var deletedPageCount = deletedPages.Count();

            // If no pages are proposed for deletion, any configuration of DeletedPageAllowance will return true;
            if (deletedPageCount == 0)
            {
                return true;
            }

            switch (deletedPageAllowance)
            {
                case DeletedPageAllowance.NotRestricted:
                    return true;

                case DeletedPageAllowance.OnlyFirst:
                    return deletedPageCount == 1
                        && deletedPages.Single() < outputPages.Min();

                case DeletedPageAllowance.OnlyLast:
                    return deletedPageCount == 1
                        && deletedPages.Single() > outputPages.Max();

                case DeletedPageAllowance.OnlyFirstOrLast:
                    {
                        if (deletedPageCount == 1)
                        {
                            return deletedPages.Single() < outputPages.Min()
                                || deletedPages.Single() > outputPages.Max();
                        }
                        else if (deletedPageCount == 2)
                        {
                            return deletedPages.Min() < outputPages.Min()
                                && deletedPages.Max() > outputPages.Max();
                        }
                        else
                        {
                            return false;
                        }
                    }

                default:
                    ExtractException.ThrowLogicException("ELI47174");
                    return false;
            }
        }

        #endregion Private Members
    }
}
