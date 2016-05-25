using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;


namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="LearningMachineOutputHandler"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("CE2857BE-FA4B-4132-9316-5378B4A990BD")]
    [CLSCompliant(false)]
    public interface ILearningMachineOutputHandler : IOutputHandler, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {

        /// <summary>
        /// Gets or sets the path to the saved <see cref="LearningMachine"/>
        /// </summary>
        string SavedMachinePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to preserve input attributes
        /// </summary>
        /// <remarks>If usage is <see cref="LearningMachineUsage.Pagination"/> then input attributes
        /// will be preserved as sub-attributes to the appropriate Document attribute.</remarks>
        bool PreserveInputAttributes
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IOutputHandler"/> that uses a <see cref="LearningMachine"/> to create attributes
    /// </summary>
    [ComVisible(true)]
    [Guid("6DC5B103-FB19-4617-90E1-D3C64B18F9B1")]
    [CLSCompliant(false)]
    public class LearningMachineOutputHandler : IdentifiableObject, ILearningMachineOutputHandler
    {
        #region Constants

        /// <summary>
        /// The description of the rule.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Learning machine output handler";

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
        /// <see langword="true"/> if changes have been made to this instance since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        LearningMachine _learningMachine;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LearningMachineOutputHandler"/> class.
        /// </summary>
        public LearningMachineOutputHandler()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39900");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LearningMachineOutputHandler"/> class as a
        /// copy of <see paramref="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="LearningMachineOutputHandler"/> from which
        /// settings should be copied.</param>
        public LearningMachineOutputHandler(LearningMachineOutputHandler source)
        {
            try
            {
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39901");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the path to the saved <see cref="LearningMachine"/>
        /// </summary>
        public string SavedMachinePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to preserve input attributes
        /// </summary>
        /// <remarks>If usage is <see cref="LearningMachineUsage.Pagination"/> then input attributes
        /// will be preserved as sub-attributes to the appropriate Document attribute.</remarks>
        public bool PreserveInputAttributes
        {
            get;
            set;
        }

        #endregion Properties

        #region IOutputHandler Members

        /// <summary>
        /// Processes the output (<see paramref="pAttributes"/>) by running the specified <see cref="LearningMachine"/>
        /// </summary>
        /// <param name="pAttributes">The output to process.</param>
        /// <param name="pDoc">The <see cref="AFDocument"/> the output is from.</param>
        /// <param name="pProgressStatus">A <see cref="ProgressStatus"/> that can be used to update
        /// processing status.</param>
        public void ProcessOutput(IUnknownVector pAttributes, AFDocument pDoc, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI39902", _COMPONENT_DESCRIPTION);

                ExtractException.Assert("ELI39903", "Learning machine output handler is not properly configured.",
                    IsConfigured());

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pAttributes.ReportMemoryUsage();

                if (_learningMachine == null)
                {
                    var pathTags = new AttributeFinderPathTags(pDoc);
                    var fileName = pathTags.Expand(SavedMachinePath);
                    _learningMachine = LearningMachine.Load(fileName);
                }
                IUnknownVector result = _learningMachine.ComputeAnswer(pDoc.Text, pAttributes, PreserveInputAttributes);
                pAttributes.CopyFrom(result);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39904", "Failed to handle output.");
            }
        }

        #endregion IOutputHandler Members

        #region IConfigurableObject Members

        /// <summary>
        /// Displays a form to allow configuration of this <see cref="LearningMachineOutputHandler"/>
        /// instance.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI39905", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                LearningMachineOutputHandler cloneOfThis = (LearningMachineOutputHandler)Clone();

                using (LearningMachineOutputHandlerSettingsDialog dlg
                    = new LearningMachineOutputHandlerSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI39906", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

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
                return SavedMachinePath != null;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39907",
                    "Error checking configuration of Learning machine output handler.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="LearningMachineOutputHandler"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="LearningMachineOutputHandler"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new LearningMachineOutputHandler(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39908",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="LearningMachineOutputHandler"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as LearningMachineOutputHandler;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to LearningMachineOutputHandler");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39909",
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
                    SavedMachinePath = reader.ReadString();
                    PreserveInputAttributes = reader.ReadBoolean();

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39910",
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
                    writer.Write(SavedMachinePath);
                    writer.Write(PreserveInputAttributes);

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
                throw ex.CreateComVisible("ELI39911",
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
        /// "UCLID AF-API Output Handlers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID AF-API Output Handlers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="LearningMachineOutputHandler"/> instance into this one.
        /// </summary><param name="source">The <see cref="LearningMachineOutputHandler"/> from which to copy.
        /// </param>
        void CopyFrom(LearningMachineOutputHandler source)
        {
            SavedMachinePath = source.SavedMachinePath;
            PreserveInputAttributes = source.PreserveInputAttributes;
            _dirty = true;
        }

        #endregion Private Members
    }
}