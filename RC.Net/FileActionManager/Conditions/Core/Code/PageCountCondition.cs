using Extract.Imaging;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
    /// Represents the operators available to compare page count.
    /// </summary>
    [ComVisible(true)]
    [Guid("4321909A-E972-4085-ADDB-6A0E0DCD8080")]
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum PageCountComparisonOperator
    {
        /// <summary>
        /// Tests whether the page count is equal to the specified value.
        /// </summary>
        Equal = 0,

        /// <summary>
        /// Tests whether the page count is not equal to the specified value.
        /// </summary>
        NotEqual = 1,

        /// <summary>
        /// Tests whether the page count is less than the specified value.
        /// </summary>
        LessThan = 2,

        /// <summary>
        /// Tests whether the page count is less than or equal to the specified value.
        /// </summary>
        LessThanOrEqual = 3,

        /// <summary>
        /// Tests whether the page count is greater than to the specified value.
        /// </summary>
        GreaterThan = 4,

        /// <summary>
        /// Tests whether the page count is greater than or equal to the specified value.
        /// </summary>
        GreaterThanOrEqual = 5,

        /// <summary>
        /// The operator to use has not been defined.
        /// </summary>
        NotDefined = 6,
    }

    /// <summary>
    /// Interface definition for the page count condition.
    /// </summary>
    [ComVisible(true)]
    [Guid("9C3781D9-01A3-4406-808C-2416E0C81EF5")]
    [CLSCompliant(false)]
    public interface IPageCountCondition : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFAMCondition, ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Specifies the <see cref="PageCountComparisonOperator"/> to use when comparing a
        /// document's page count to the specified <see cref="PageCount"/>.
        /// </summary>
        /// <value>
        /// The <see cref="PageCountComparisonOperator"/> to use when comparing a document's page
        /// count to the specified <see cref="PageCount"/>.
        /// </value>
        PageCountComparisonOperator PageCountComparisonOperator { get; set; }

        /// <summary>
        /// Specifies the <see langword="int"/> that should be compared to a document's page count.
        /// </summary>
        /// <value>
        /// The <see langword="int"/> that should be compared to a document's page count.
        /// </value>
        int PageCount { get; set; }

        /// <summary>
        /// Gets or sets whether the page count from the database should be used, if available.
        /// </summary>
        /// <value> <see langword="true"/> if the page count from the database should be used;
        /// <see langword="false"/> if the condition should always check the file itself.
        /// </value>
        bool UseDBPageCount { get; set; }
    }

    /// <summary>
    /// A <see cref="IFAMCondition"/> based on the page count of a file.
    /// </summary>
    [ComVisible(true)]
    [Guid("5AE230C8-2375-44CD-8F20-EE0D089BF07B")]
    [ProgId("Extract.FileActionManager.Conditions.PageCountCondition")]
    public class PageCountCondition : IPageCountCondition, IDisposable
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Page count condition";

        /// <summary>
        /// Current task version.
        /// Version 2: Added UseDBPageCount
        /// </summary>
        internal const int _CURRENT_VERSION = 2;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Specifies the <see cref="PageCountComparisonOperator"/> to use when comparing a
        /// document's page count to the specified <see cref="PageCount"/>.
        /// </summary>
        PageCountComparisonOperator _pageCountConditionOperator = PageCountComparisonOperator.NotDefined;

        /// <summary>
        /// Specifies the <see langword="int"/> that should be compared to a document's page count.
        /// </summary>
        int _pageCount = 1;

        /// <summary>
        /// Indicates whether the page count from the database should be used, if available.
        /// </summary>
        bool _useDBPageCount = true;

        /// <summary>
        /// <see langword="true"/> if changes have been made to
        /// <see cref="PageCountCondition"/> since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// An <see cref="ImageCodecs"/> instance used to check page count if the associated
        /// <see cref="FileRecord"/> does not contain the page count.
        /// </summary>
        ImageCodecs _codecs;

        /// <summary>
        /// Used to check  for image file extensions.
        /// </summary>
        MiscUtils _miscUtils;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageCountCondition"/> class.
        /// </summary>
        public PageCountCondition()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36681");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageCountCondition"/> class as a copy
        /// of the specified <see paramref="task"/>.
        /// </summary>
        /// <param name="task">The <see cref="PageCountCondition"/> from which settings should be
        /// copied.</param>
        public PageCountCondition(PageCountCondition task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36682");
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
        public PageCountComparisonOperator PageCountComparisonOperator
        {
            get
            {
                return _pageCountConditionOperator;
            }

            set
            {
                try
                {
                    if (value != _pageCountConditionOperator)
                    {
                        _pageCountConditionOperator = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36702");
                }
            }
        }

        /// <summary>
        /// Specifies the <see langword="int"/> that should be compared to a document's page count.
        /// </summary>
        /// <value>
        /// The <see langword="int"/> that should be compared to a document's page count.
        /// </value>
        public int PageCount
        {
            get
            {
                return _pageCount;
            }

            set
            {
                try
                {
                    if (value != _pageCount)
                    {
                        _pageCount = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36703");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the page count from the database should be used, if available.
        /// </summary>
        /// <value> <see langword="true"/> if the page count from the database should be used;
        /// <see langword="false"/> if the condition should always check the file itself.
        /// </value>
        public bool UseDBPageCount
        {
            get
            {
                return _useDBPageCount;
            }

            set
            {
                try
                {
                    if (value != _useDBPageCount)
                    {
                        _useDBPageCount = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36707");
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
                if (PageCountComparisonOperator == Conditions.PageCountComparisonOperator.NotDefined)
                {
                    throw new ExtractException("ELI36705", _COMPONENT_DESCRIPTION + 
                        " has not been configured.");
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36683", ex.Message);
            }
        }

        #endregion Public Methods

        #region IFAMCondition Members

        /// <summary>
        /// Compares the page count of the document represented by <see paramref="pFileRecord"/> to
        /// the current configuration to determine if the condition is met.
        /// </summary>
        /// <param name="pFileRecord">A <see cref="FileRecord"/> specifing the file to be tested.
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
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI36684",
                    _COMPONENT_DESCRIPTION);

                ValidateSettings();
                int documentPageCount = SourceFilePageCount(pFileRecord);

                return IsConditionMet(documentPageCount);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI36685",
                    "Error occured in '" + _COMPONENT_DESCRIPTION + "'", ex);
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

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="PageCountCondition"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI36686",
                    _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                PageCountCondition cloneOfThis = (PageCountCondition)Clone();

                using (PageCountConditionSettingsDialog dlg
                    = new PageCountConditionSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI36687", "Error running configuration.");
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
                throw ex.CreateComVisible("ELI36688",
                    "Failed to check '" + _COMPONENT_DESCRIPTION + "' configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="PageCountCondition"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="PageCountCondition"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new PageCountCondition(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36689",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="PageCountCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as PageCountCondition;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to PageCountCondition");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36690",
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
                    PageCountComparisonOperator = (PageCountComparisonOperator)reader.ReadInt32();
                    PageCount = reader.ReadInt32();
                    UseDBPageCount = (reader.Version >= 2) ? reader.ReadBoolean() : true;
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36691",
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
                    writer.Write((int)PageCountComparisonOperator);
                    writer.Write(PageCount);
                    writer.Write(UseDBPageCount);

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
                throw ex.CreateComVisible("ELI36692",
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

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="PageCountCondition"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="PageCountCondition"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="PageCountCondition"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable Members

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
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileActionManagerConditionsGuid);
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
        }

        /// <summary>
        /// Copies the specified <see cref="PageCountCondition"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="PageCountCondition"/> from which to copy.
        /// </param>
        void CopyFrom(PageCountCondition source)
        {
            PageCountComparisonOperator = source.PageCountComparisonOperator;
            PageCount = source.PageCount;
            UseDBPageCount = source.UseDBPageCount;

            _dirty = true;
        }

        bool IsConditionMet(int documentPageCount)
        {
            bool conditionMet = false;

            switch (PageCountComparisonOperator)
            {
                case PageCountComparisonOperator.Equal:
                    conditionMet = documentPageCount == PageCount;
                    break;

                case PageCountComparisonOperator.NotEqual:
                    conditionMet = documentPageCount != PageCount;
                    break;

                case PageCountComparisonOperator.LessThan:
                    conditionMet = documentPageCount < PageCount;
                    break;

                case PageCountComparisonOperator.LessThanOrEqual:
                    conditionMet = documentPageCount <= PageCount;
                    break;

                case PageCountComparisonOperator.GreaterThan:
                    conditionMet = documentPageCount > PageCount;
                    break;

                case PageCountComparisonOperator.GreaterThanOrEqual:
                    conditionMet = documentPageCount >= PageCount;
                    break;

                default:
                    ExtractException.ThrowLogicException("ELI36704");
                    break;
            }

            return conditionMet;
        }

        int SourceFilePageCount(FileRecord pFileRecord)
        {
            int documentPageCount = UseDBPageCount ? pFileRecord.Pages : 0;

            // https://extract.atlassian.net/browse/ISSUE-12030
            // When the condition is being used during queuing or the page count check was
            // skipped during queuing we will need to manually check for the number of pages.
            if (documentPageCount == 0)
            {
                string sourceDocName = pFileRecord.Name;

                if (!File.Exists(sourceDocName))
                {
                    var ee = new ExtractException("ELI36706",
                        "Cannot check page count of non-existent document.");
                    ee.AddDebugData("SourceDocName", sourceDocName, false);
                    throw ee;
                }

                if (_codecs == null)
                {
                    _codecs = new ImageCodecs();
                }

                try
                {
                    using (var imageReader = _codecs.CreateReader(pFileRecord.Name))
                    {
                        documentPageCount = imageReader.PageCount;
                    }
                }
                catch (Exception ex)
                {
                    if (_miscUtils == null)
                    {
                        _miscUtils = new MiscUtils();
                    }

                    // https://extract.atlassian.net/browse/ISSUE-12044
                    // If the document has an image or numeric extension, throw an exception
                    // because we should be able to get a page count. But if the document does
                    // not appear to be an image, ignore this error and let the page count be
                    // zero.
                    if (_miscUtils.HasImageFileExtension(sourceDocName) ||
                        _miscUtils.HasNumericFileExtension(sourceDocName))
                    {
                        var ee = new ExtractException("ELI36712",
                            "Unable to get page count of document.", ex);
                        ee.AddDebugData("SourceDocName", sourceDocName, false);
                        throw ee;
                    }
                }
            }

            return documentPageCount;
        }

        #endregion Private Members
    }
}
