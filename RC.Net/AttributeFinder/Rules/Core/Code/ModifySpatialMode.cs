using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Specifies the conditions available for <see cref="IModifySpatialMode"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("F688CD5A-7DFD-4FFC-B3AB-FDC1D389939A")]
    public enum ModifySpatialModeRasterZoneCountCondition
    {
        /// <summary>
        /// Condition is met when the source value has a single raster zone.
        /// </summary>
        Single = 0,

        /// <summary>
        /// Condition is met when the source value has more than one raster zone.
        /// </summary>
        Multiple = 1
    }

    /// <summary>
    /// Specifies the spatial mode modifications available for <see cref="IModifySpatialMode"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("7038D730-D511-4D89-B72D-DEE85C6215F0")]
    public enum ModifySpatialModeAction
    {
        /// <summary>
        /// Spatial attribute should be downgraded to hybrid.
        /// </summary>
        DowngradeToHybrid = 0,

        /// <summary>
        /// Hybrid attribute should be converted to pseudo-spatial.
        /// </summary>
        ConvertToPseudoSpatial = 1,

        /// <summary>
        /// Spatial info should be removed.
        /// </summary>
        Remove = 2
    }

    /// <summary>
    /// An interface for the <see cref="ModifySpatialMode"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("137677A9-3771-4489-81B8-8C6C2F48CED8")]
    [CLSCompliant(false)]
    public interface IModifySpatialMode : IAttributeModifyingRule, IOutputHandler,
        IDocumentPreprocessor, ICategorizedComponent, IConfigurableObject, ICopyableObject,
        ILicensedComponent, IPersistStream, IIdentifiableRuleObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether performing the modification of spatial info
        /// should be conditional on the number of raster zones in the attribute.
        /// </summary>
        /// <value><see langword="true"/> if performing the modification of spatial info should
        /// be conditional on the number of raster zones in the attribute; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool UseCondition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="ModifySpatialModeRasterZoneCountCondition"/> to use.
        /// </summary>
        /// <value>
        /// The <see cref="ModifySpatialModeRasterZoneCountCondition"/> to use.
        /// </value>
        ModifySpatialModeRasterZoneCountCondition ZoneCountCondition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="ModifySpatialModeAction"/> to run on the attribute value.
        /// </summary>
        /// <value>
        /// The <see cref="ModifySpatialModeAction"/> to run on the attribute value.
        /// </value>
        ModifySpatialModeAction ModifySpatialModeAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the modification should be applied to
        /// sub-attributes recursively.
        /// </summary>
        /// <value><see langword="true"/> if the modification should be applied to sub-attributes
        /// recursively; otherwise, <see langword="false"/>.
        /// </value>
        bool ModifyRecursively
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IAttributeModifyingRule"/> that modifies the mode of the spatial info of the
    /// <see cref="IAttribute"/> value.
    /// </summary>
    [ComVisible(true)]
    [Guid("095B4B5C-0C07-4E43-BA2E-D13885860FEF")]
    [CLSCompliant(false)]
    public class ModifySpatialMode : IdentifiableRuleObject, IModifySpatialMode
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Modify spatial mode";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// A value indicating whether performing the modification of spatial info should be
        /// conditional on the number of raster zones in the attribute.
        /// </summary>
        bool _useCondition;

        /// <summary>
        /// The <see cref="ModifySpatialModeRasterZoneCountCondition"/> to use.
        /// </summary>
        ModifySpatialModeRasterZoneCountCondition _zoneCountCondition;

        /// <summary>
        /// The <see cref="ModifySpatialModeAction"/> to run on the attribute value.
        /// </summary>
        ModifySpatialModeAction _modifySpatialInfoAction;

        /// <summary>
        /// Indicates whether the modification should be applied to sub-attributes recursively.
        /// </summary>
        bool _modifyRecursively;

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="ModifySpatialMode"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="ModifySpatialMode"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifySpatialMode"/> class.
        /// </summary>
        public ModifySpatialMode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifySpatialMode"/> class.
        /// </summary>
        /// <param name="modifySpatialMode">The <see cref="ModifySpatialMode"/> from which settings
        /// should be copied.</param>
        public ModifySpatialMode(ModifySpatialMode modifySpatialMode)
        {
            try
            {
                CopyFrom(modifySpatialMode);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34682");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether performing the modification of spatial info
        /// should be conditional on the number of raster zones in the attribute.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if performing the modification of spatial info should
        /// be conditional on the number of raster zones in the attribute; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool UseCondition
        {
            get
            {
                return _useCondition;
            }

            set
            {
                if (value != _useCondition)
                {
                    _useCondition = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ModifySpatialModeRasterZoneCountCondition"/> to use.
        /// </summary>
        /// <value>
        /// The <see cref="ModifySpatialModeRasterZoneCountCondition"/> to use.
        /// </value>
        public ModifySpatialModeRasterZoneCountCondition ZoneCountCondition
        {
            get
            {
                return _zoneCountCondition;
            }

            set
            {
                if (value != _zoneCountCondition)
                {
                    _zoneCountCondition = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ModifySpatialModeAction"/> to run on the attribute value.
        /// </summary>
        /// <value>
        /// The <see cref="ModifySpatialModeAction"/> to run on the attribute value.
        /// </value>
        public ModifySpatialModeAction ModifySpatialModeAction
        {
            get
            {
                return _modifySpatialInfoAction;
            }
            set
            {
                if (value != _modifySpatialInfoAction)
                {
                    _modifySpatialInfoAction = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the modification should be applied to
        /// sub-attributes recursively.
        /// </summary>
        /// <value><see langword="true"/> if the modification should be applied to sub-attributes
        /// recursively; otherwise, <see langword="false"/>.
        /// </value>
        public bool ModifyRecursively
        {
            get
            {
                return _modifyRecursively;
            }

            set
            {
                if (value != _modifyRecursively)
                {
                    _modifyRecursively = value;
                    _dirty = true;
                }
            }
        }

        #endregion Properties

        #region IAttributeModifyingRule

        /// <summary>
        /// Modifies the attribute value.
        /// </summary>
        /// <param name="pAttributeToBeModified">The attribute to be modified.</param>
        /// <param name="pOriginInput">The original <see cref="AFDocument"/>.</param>
        /// <param name="pProgressStatus">A <see cref="ProgressStatus"/> instance that can be used
        /// to indicate progress.</param>
        public void ModifyValue(UCLID_AFCORELib.Attribute pAttributeToBeModified,
            AFDocument pOriginInput, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34683", _COMPONENT_DESCRIPTION);

                // Do not modify recursively since the AF framework already executes all value
                // modifiers recursively.
                ModifyAttributeSpatialMode(pAttributeToBeModified, false);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34684", "Failed to modify spatial mode.");
            }
        }

        #endregion IAttributeModifyingRule

        #region IOutputHandler

        /// <summary>
        /// Processes the output <see paramref="pAttributes"/>.
        /// </summary>
        /// <param name="pAttributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// to modify.</param>
        /// <param name="pDoc">The <see cref="AFDocument"/> the attributes are associated with.
        /// </param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> displaying the progress.
        /// </param>
        public void ProcessOutput(IUnknownVector pAttributes, AFDocument pDoc, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34685", _COMPONENT_DESCRIPTION);

                foreach (IAttribute attribute in pAttributes.ToIEnumerable<IAttribute>())
                {
                    ModifyAttributeSpatialMode(attribute, ModifyRecursively);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34686", "Failed to modify spatial mode of output.");
            }
        }

        #endregion IOutputHandler

        #region IDocumentProcessor

        /// <summary>
        /// Processes the specified <see paramref="pDocument"/>
        /// </summary>
        /// <param name="pDocument">The <see cref="AFDocument"/> to process.</param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> displaying the progress.
        /// </param>
        public void Process(AFDocument pDocument, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34703", _COMPONENT_DESCRIPTION);

                ModifySpatialStringSpatialMode(pDocument.Text);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34702", "Failed to modify spatial mode of output.");
            }
        }

        #endregion IDocumentProcessor

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ModifySpatialMode"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34687", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                ModifySpatialMode cloneOfThis = (ModifySpatialMode)Clone();

                using (ModifySpatialModeSettingsDialog dlg
                    = new ModifySpatialModeSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI34688", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ModifySpatialMode"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ModifySpatialMode"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new ModifySpatialMode(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34689",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ModifySpatialMode"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as ModifySpatialMode;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to ModifySpatialMode");
                }

                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34690",
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
                    UseCondition = reader.ReadBoolean();
                    ZoneCountCondition = (ModifySpatialModeRasterZoneCountCondition)reader.ReadInt32();
                    ModifySpatialModeAction = (ModifySpatialModeAction)reader.ReadInt32();
                    ModifyRecursively = reader.ReadBoolean();

                    // Load the GUID for the IIdentifiableRuleObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34691",
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
                    writer.Write(UseCondition);
                    writer.Write((int)ZoneCountCondition);
                    writer.Write((int)ModifySpatialModeAction);
                    writer.Write(ModifyRecursively);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                // Save the GUID for the IIdentifiableRuleObject interface.
                SaveGuid(stream);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34692",
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
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.ValueModifiersGuid);
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.DocumentPreprocessorsGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.ValueModifiersGuid);
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.DocumentPreprocessorsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="ModifySpatialMode"/> instance into this one.
        /// </summary><param name="source">The <see cref="ModifySpatialMode"/> from which to copy.
        /// </param>
        void CopyFrom(ModifySpatialMode source)
        {
            UseCondition = source.UseCondition;
            ZoneCountCondition = source.ZoneCountCondition;
            ModifySpatialModeAction = source.ModifySpatialModeAction;
            ModifyRecursively = source.ModifyRecursively;

            _dirty = true;
        }

        /// <summary>
        /// Modifies the spatial mode for the specified <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to modify.</param>
        /// <param name="modifyRecursively">Indicates whether the modification should be applied to
        /// sub-attributes recursively.</param>
        void ModifyAttributeSpatialMode(IAttribute attribute, bool modifyRecursively)
        {
            try
            {
                if (modifyRecursively)
                {
                    foreach (IAttribute subAttribute in
                        attribute.SubAttributes.ToIEnumerable<IAttribute>())
                    {
                        ModifyAttributeSpatialMode(subAttribute, modifyRecursively);
                    }
                }

                ModifySpatialStringSpatialMode(attribute.Value);
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI34694");
                try
                {
                    ee.AddDebugData("Attribute name", attribute.Name, true);
                }
                catch { }

                throw ee;
            }
        }

        /// <summary>
        /// Modifies the spatial mode for the specified <see cref="SpatialString"/>.
        /// </summary>
        /// <param name="spatialString">The <see cref="SpatialString"/> to modify.</param>
        void ModifySpatialStringSpatialMode(SpatialString spatialString)
        {
            // No modification is possible on an attribute without spatial info.
            if (!spatialString.HasSpatialInfo())
            {
                return;
            }

            // Do not preform modification if the attribute value does not meet the specified
            // condition.
            if (UseCondition)
            {
                int zoneCount = spatialString.GetOCRImageRasterZones().Size();

                if ((zoneCount == 1) !=
                    (ZoneCountCondition == ModifySpatialModeRasterZoneCountCondition.Single))
                {
                    return;
                }
            }

            // Perform the specified spatial mode modification.
            switch (ModifySpatialModeAction)
            {
                case ModifySpatialModeAction.DowngradeToHybrid:
                    {
                        // Downgrade to hybrid only necessary if current mode is spatial.
                        if (spatialString.GetMode() == ESpatialStringMode.kSpatialMode)
                        {
                            spatialString.DowngradeToHybridMode();
                        }
                    }
                    break;

                case ModifySpatialModeAction.ConvertToPseudoSpatial:
                    {
                        // Conversion to pseudo-spatial only makes sense if the value is hybrid.
                        if (spatialString.GetMode() == ESpatialStringMode.kHybridMode)
                        {
                            IUnknownVector rasterZones = spatialString.GetOCRImageRasterZones();
                            ExtractException.Assert("ELI34693",
                                "Cannot use multiple raster zones to create pseudo-spatial string.",
                                rasterZones.Size() == 1);

                            spatialString.CreatePseudoSpatialString((RasterZone)rasterZones.At(0),
                                spatialString.String, spatialString.SourceDocName, spatialString.SpatialPageInfos);
                        }
                    }
                    break;

                case ModifySpatialModeAction.Remove:
                    {
                        // We've ensured above that value has spatial info.
                        spatialString.DowngradeToNonSpatialMode();
                    }
                    break;
            }

            return;
        }

        #endregion Private Members
    }
}
