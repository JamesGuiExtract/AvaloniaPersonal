using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="ValueConditionSelector"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("4F69DC0A-07C2-427A-AD19-2BC880838079")]
    [CLSCompliant(false)]
    public interface IValueConditionSelector : IAttributeSelector, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {
        /// <summary>
        /// Gets or sets the <see cref="IAFCondition"/> to be used to determine whether an attribute
        /// should be selected.
        /// </summary>
        IAFCondition Condition
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IAttributeSelector"/> that selects attributes based upon whether a
    /// <see cref="IAFCondition"/> run against the value of the attributes is met.
    /// </summary>
    [ComVisible(true)]
    [Guid("005AC4A8-10FD-4872-A635-ED976205F479")]
    [CLSCompliant(false)]
    public class ValueConditionSelector : IdentifiableObject, IValueConditionSelector
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Value condition selector";

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
        /// The <see cref="IAFCondition"/> to be used to determine whether an attribute should be
        /// selected.
        /// </summary>
        IAFCondition _condition;

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="ValueConditionSelector"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueConditionSelector"/> class.
        /// </summary>
        public ValueConditionSelector()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33725");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueConditionSelector"/> class as a
        /// copy of the specified <see paramref="valueConditionSelector"/>.
        /// </summary>
        /// <param name="valueConditionSelector">The <see cref="ValueConditionSelector"/>
        /// from which settings should be copied.</param>
        public ValueConditionSelector(ValueConditionSelector valueConditionSelector)
        {
            try
            {
                CopyFrom(valueConditionSelector);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33726");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IAFCondition"/> to be used to determine whether an attribute
        /// should be selected.
        /// </summary>
        public IAFCondition Condition
        {
            get
            {
                return _condition;
            }

            set
            {
                if (value != _condition)
                {
                    _condition = value;
                    _dirty = true;
                }
            }
        }

        #endregion Properties

        #region IAttributeSelector Members

        /// <summary>
        /// Selects <see cref="ComAttribute"/>s from <see paramref="pAttrIn"/> based upon whether
        /// the <see cref="Condition"/> run against the value of each attribute is met.
        /// </summary>
        /// <param name="pAttrIn">The domain of attributes from which to select.</param>
        /// <param name="pAFDoc">The <see cref="AFDocument"/> context for this execution.</param>
        /// <param name="pAttrContext">The parent object's input attributes (or, if the parent object
        /// is an attribute selector, its context attributes).</param>
        /// <returns>The attributes whose values satisfy the <see cref="Condition"/>.</returns>
        public IUnknownVector SelectAttributes(IUnknownVector pAttrIn, AFDocument pAFDoc, IUnknownVector pAttrContext)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33727", _COMPONENT_DESCRIPTION);

                IEnumerable<ComAttribute> selectedAttributes =
                    GetSelectedAttributes(pAttrIn, pAFDoc);

                IUnknownVector selectedAttributeVector = selectedAttributes.ToIUnknownVector();

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pAttrIn.ReportMemoryUsage();
                pAttrContext.ReportMemoryUsage();
                selectedAttributeVector.ReportMemoryUsage();

                return selectedAttributeVector;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33728",
                    "Failed to select attributes based upon value condition.");
            }
        }

        /// <summary>
        /// Selects <see cref="ComAttribute"/>s from <see paramref="pAttrIn"/> based upon whether
        /// the <see cref="Condition"/> run against the value of each attribute is met.
        /// </summary>
        /// <param name="sourceAttributeVector">The domain of attributes from which to select.</param>
        /// <param name="afDoc">The <see cref="AFDocument"/> context for this execution.</param>
        /// <returns>The attributes whose values satisfy the <see cref="Condition"/>.</returns>
        IEnumerable<ComAttribute> GetSelectedAttributes(IUnknownVector sourceAttributeVector,
            AFDocument afDoc)
        {
            foreach (ComAttribute attribute in sourceAttributeVector.ToIEnumerable<ComAttribute>())
            {
                // So that the garbage collector knows of and properly manages the associated
                // memory.
                attribute.ReportMemoryUsage();

                // Create a new AFDocument for each attribute whose text is the value of the
                // attribute being tested. (No need to clone the document text since it is being
                // replaced.)
                AFDocument afDocument = afDoc.PartialClone(false, false);
                afDocument.Text = attribute.Value;

                using (RuleObjectProfiler profiler = new RuleObjectProfiler("", "", Condition, 0))
                {
                    // Select all attributes that meet the condition.
                    if (Condition.ProcessCondition(afDocument))
                    {
                        yield return attribute;
                    }
                }
            }
        }

        #endregion IAttributeSelector Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ValueConditionSelector"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33729", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                ValueConditionSelector cloneOfThis = (ValueConditionSelector)Clone();

                using (ValueConditionSelectorSettingsDialog dlg
                    = new ValueConditionSelectorSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI33730", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ValueConditionSelector"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ValueConditionSelector"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new ValueConditionSelector(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33731",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ValueConditionSelector"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as ValueConditionSelector;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to ValueConditionSelector");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33732",
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
                    Condition = (IAFCondition)reader.ReadIPersistStream();

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33733",
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
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write((IPersistStream)Condition, clearDirty);

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
                throw ex.CreateComVisible("ELI33734",
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
                if (Condition == null)
                {
                    return false;
                }

                IMustBeConfiguredObject mustBeConfiguredObject = Condition as IMustBeConfiguredObject;
                if (mustBeConfiguredObject != null)
                {
                    return mustBeConfiguredObject.IsConfigured();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33747",
                    "Error checking configuration of value condition selector.");
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
        /// Copies the specified <see cref="ValueConditionSelector"/> instance into this one.
        /// </summary><param name="source">The <see cref="ValueConditionSelector"/> from which
        /// to copy.</param>
        void CopyFrom(ValueConditionSelector source)
        {
            if (source.Condition == null)
            {
                Condition = null;
            }
            else
            {
                ICopyableObject copyThis = (ICopyableObject)source.Condition;
                Condition = (IAFCondition)copyThis.Clone();
            }

            _dirty = true;
        }

        #endregion Private Members
    }
}
