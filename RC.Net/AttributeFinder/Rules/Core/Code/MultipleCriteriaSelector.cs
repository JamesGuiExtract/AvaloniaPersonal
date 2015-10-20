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

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="MultipleCriteriaSelector"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("5D0A160B-9858-44C8-A4B1-E2B24B5EDC6B")]
    [CLSCompliant(false)]
    public interface IMultipleCriteriaSelector : IAttributeSelector, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {
        /// <summary>
        /// An <see cref="IUnknownVector"/> of <see cref="IObjectWithDescription"/>s each containing
        /// an <see cref="IAttributeSelector"/> to be used to select attributes.
        /// </summary>
        IUnknownVector Selectors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets which selectors should be negated. It is required that this array contain
        /// the same number of items in <see cref="Selectors"/>, and it assumed it is in
        /// corresponding order.
        /// </summary>
        /// <value>A <see langword="bool[]"/> where each entry is <see langword="true"/> to select
        /// only the items not selected by the <see cref="IAttributeSelector"/> at the corresponding
        /// index in <see cref="Selectors"/> or <see langword="false"/> to select the same items.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        bool[] NegatedSelectors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the selectors should be combined exclusively
        /// (and'd) or inclusively (or'd).
        /// </summary>
        /// <value><see langword="true"/>To include only attributes selected by all the
        /// <see cref="Selectors"/>; <see langword="false"/> to include attributes selected by any
        /// of the selectors.
        /// </value>
        bool SelectExclusively
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IAttributeSelector"/> that selects attributes based upon one or more selectors
    /// or their negated (inverse) selections.
    /// </summary>
    [ComVisible(true)]
    [Guid("C752F09E-5D24-4E65-8250-4811F60D694E")]
    [CLSCompliant(false)]
    public class MultipleCriteriaSelector : IdentifiableObject, IMultipleCriteriaSelector
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Multiple criteria attribute selector";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.RuleWritingCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// An <see cref="IUnknownVector"/> of <see cref="IObjectWithDescription"/>s each containing
        /// an <see cref="IAttributeSelector"/> to be used to select attributes.
        /// </summary>
        IUnknownVector _selectors = new IUnknownVector();

        /// <summary>
        /// Indicates whether the selectors should be combined exclusively (and'd) or inclusively
        /// (or'd).
        /// </summary>
        bool _selectExclusively = true;

        /// <summary>
        /// Indicates which selectors should be negated.
        /// </summary>
        bool[] _negatedSelectors = new bool[0];

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="MultipleCriteriaSelector"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleCriteriaSelector"/> class.
        /// </summary>
        public MultipleCriteriaSelector()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33859");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleCriteriaSelector"/> class as a
        /// copy of the specified <see paramref="multipleCriteriaSelector"/>.
        /// </summary>
        /// <param name="multipleCriteriaSelector">The <see cref="MultipleCriteriaSelector"/>
        /// from which settings should be copied.</param>
        public MultipleCriteriaSelector(MultipleCriteriaSelector multipleCriteriaSelector)
        {
            try
            {
                CopyFrom(multipleCriteriaSelector);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33860");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// An <see cref="IUnknownVector"/> of <see cref="IObjectWithDescription"/>s each containing
        /// an <see cref="IAttributeSelector"/> to be used to select attributes.
        /// </summary>
        public IUnknownVector Selectors
        {
            get
            {
                return _selectors;
            }

            set
            {
                _selectors = value;
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets which selectors should be negated. It is required that this array contain
        /// the same number of items in <see cref="Selectors"/>, and it assumed it is in
        /// corresponding order.
        /// </summary>
        /// <value>A <see langword="bool[]"/> where each entry is <see langword="true"/> to select
        /// only the items not selected by the <see cref="IAttributeSelector"/> at the corresponding
        /// index in <see cref="Selectors"/> or <see langword="false"/> to select the same items.
        /// </value>
        public bool[] NegatedSelectors
        {
            get
            {
                return _negatedSelectors;
            }

            set
            {
                try
                {
                    _negatedSelectors = new bool[value.Length];
                    value.CopyTo(_negatedSelectors, 0);
                    _dirty = true;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33881");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the selectors should be combined exclusively
        /// (and'd) or inclusively (or'd).
        /// </summary>
        /// <value><see langword="true"/>To include only attributes selected by all the
        /// <see cref="Selectors"/>; <see langword="false"/> to include attributes selected by any
        /// of the selectors.
        /// </value>
        public bool SelectExclusively
        {
            get
            {
                return _selectExclusively;
            }

            set
            {
                if (value != _selectExclusively)
                {
                    _selectExclusively = value;
                    _dirty = true;
                }
            }
        }

        #endregion Properties

        #region IAttributeSelector Members

        /// <summary>
        /// Selects <see cref="ComAttribute"/>s from <see paramref="pAttrIn"/> 
        /// </summary>
        /// <param name="pAttrIn">The domain of attributes from which to select.</param>
        /// <param name="pAFDoc">The <see cref="AFDocument"/> context for this execution.</param>
        /// <param name="pAttrContext">The parent object's input attributes (or, if the parent object
        /// is an attribute selector, its context attributes).</param>
        /// <returns></returns>
        public IUnknownVector SelectAttributes(IUnknownVector pAttrIn, AFDocument pAFDoc, IUnknownVector pAttrContext)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33861", _COMPONENT_DESCRIPTION);

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pAttrIn.ReportMemoryUsage();
                pAttrContext.ReportMemoryUsage();

                IEnumerable<ComAttribute> selectedAttributes = GetSelectedAttributes(pAttrIn, pAFDoc, pAttrContext);

                IUnknownVector selectedAttributeVector = selectedAttributes.ToIUnknownVector();

                // Report memory usage of hierarchy after processing to ensure all COM objects
                // referenced in final result are reported.
                selectedAttributeVector.ReportMemoryUsage();

                return selectedAttributeVector;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33862",
                    "Failed to select attributes based upon multiple criteria.");
            }
        }

        /// <summary>
        /// Selects <see cref="ComAttribute"/>s from <see paramref="pAttrIn"/> based upon one or
        /// more selectors or their negated (inverse) selections.
        /// </summary>
        /// <param name="sourceAttributeVector">The domain of attributes from which to select.</param>
        /// <param name="afDocument">The <see cref="AFDocument"/> context for this execution.</param>
        /// <param name="contextAttributeVector">The parent object's input attributes (or, if the
        /// parent object is an attribute selector, its context attributes).</param>
        /// <returns></returns>
        IEnumerable<ComAttribute> GetSelectedAttributes(IUnknownVector sourceAttributeVector,
            AFDocument afDocument, IUnknownVector contextAttributeVector)
        {
            ExtractException.Assert("ELI33887",
                    "Corrupt " + _COMPONENT_DESCRIPTION + " configuration.",
                    NegatedSelectors.Length == Selectors.Size());

            // If there are no source attributes, there is nothing to do.
            if (sourceAttributeVector.Size() == 0)
            {
                return new List<ComAttribute>();
            }

            HashSet<ComAttribute> candidateAttributes =
                new HashSet<ComAttribute>(sourceAttributeVector.ToIEnumerable<ComAttribute>());

            // If selecting exclusively, the remaining candidateAttributes will be the result;
            // if selecting inclusively, build the result by moving selected candidates into a new set.
            HashSet<ComAttribute> resultingAttributes = new HashSet<ComAttribute>();

            // Iterate through each selector.
            for (int i = 0; i < NegatedSelectors.Length; i++)
            {
                ObjectWithDescription owd = (ObjectWithDescription)Selectors.At(i);
                if (!owd.Enabled)
                {
                    continue;
                }
                IAttributeSelector selector = (IAttributeSelector)owd.Object;
                HashSet<ComAttribute> selectedAttributes = null;
                using (RuleObjectProfiler profiler = new RuleObjectProfiler
                    (owd.Description, "", selector, 0))
                {
                    // Get the attributes selected by this selector.
                    selectedAttributes = new HashSet<ComAttribute>(
                        selector
                        .SelectAttributes(candidateAttributes.ToIUnknownVector(),
                                          afDocument, contextAttributeVector)
                        .ToIEnumerable<ComAttribute>());
                }

                // If negating the selection, select only those attributes in sourceAttributes
                // that are not in selectedAttributes.
                if (NegatedSelectors[i])
                {
                    selectedAttributes = new HashSet<ComAttribute>(candidateAttributes
                        .Where(attribute => !selectedAttributes.Contains(attribute)));
                }

                // If selecting exclusively, the candidates for the next selector are
                // the selected attributes.
                if (SelectExclusively)
                {
                    candidateAttributes = resultingAttributes = selectedAttributes;
                    
                    // If there are no more candidates then no need to run remaining selectors.
                    if (candidateAttributes.Count == 0)
                    {
                        break;
                    }
                }
                // Else, if selecting inclusively and some attributes were selected by this
                // selector, add the selected attributes to those already selected by other
                // selectors and remove them from the candidate set.
                else if (selectedAttributes.Count > 0)
                {
                    resultingAttributes.UnionWith(selectedAttributes);
                    candidateAttributes.ExceptWith(selectedAttributes);

                    // If no more candidates no need to run remaining selectors.
                    if (candidateAttributes.Count == 0)
                    {
                        break;
                    }
                }
            }

            return resultingAttributes;
        }

        #endregion IAttributeSelector Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="MultipleCriteriaSelector"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33863", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                MultipleCriteriaSelector cloneOfThis = (MultipleCriteriaSelector)Clone();

                using (MultipleCriteriaSelectorSettingsDialog dlg
                    = new MultipleCriteriaSelectorSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI33864", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="MultipleCriteriaSelector"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="MultipleCriteriaSelector"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new MultipleCriteriaSelector(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33865",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="MultipleCriteriaSelector"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as MultipleCriteriaSelector;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to MultipleCriteriaSelector");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33866",
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
            return LicenseUtilities.IsLicensed(_LICENSE_ID);
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
                    Selectors = (IUnknownVector)reader.ReadIPersistStream();
                    NegatedSelectors = reader.ReadBooleanArray();
                    SelectExclusively = reader.ReadBoolean();

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33867",
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
                ExtractException.Assert("ELI33883",
                    "Corrupt " + _COMPONENT_DESCRIPTION + " configuration.",
                    NegatedSelectors.Length == Selectors.Size());

                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write((IPersistStream)Selectors, clearDirty);
                    writer.Write(_negatedSelectors);
                    writer.Write(SelectExclusively);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                // Save the GUID for the IIdentifiableObject interface.
                SaveGuid(stream);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33868",
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

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Determines whether this instance is configured.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this instance is configured; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                if (NegatedSelectors.Length == 0)
                {
                    return false;
                }

                foreach (IObjectWithDescription owd in
                    Selectors.ToIEnumerable<IObjectWithDescription>())
                {
                    IMustBeConfiguredObject mustBeConfiguredObject =
                        owd.Object as IMustBeConfiguredObject;
                    if (mustBeConfiguredObject != null &&
                        !mustBeConfiguredObject.IsConfigured())
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33869",
                    "Error checking configuration of value multiple criteria selector.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID AF-API Selectors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.AttributeSelectorsGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID AF-API Selectors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.AttributeSelectorsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="MultipleCriteriaSelector"/> instance into this one.
        /// </summary><param name="source">The <see cref="MultipleCriteriaSelector"/> from which
        /// to copy.</param>
        void CopyFrom(MultipleCriteriaSelector source)
        {
            if (source.Selectors == null)
            {
                Selectors = null;
            }
            else
            {
                ICopyableObject copyThis = (ICopyableObject)source.Selectors;
                Selectors = (IUnknownVector)copyThis.Clone();
            }

            NegatedSelectors = new bool[source.NegatedSelectors.Length];
            source.NegatedSelectors.CopyTo(NegatedSelectors, 0);
            SelectExclusively = source.SelectExclusively;

            _dirty = true;
        }

        #endregion Private Members
    }
}
