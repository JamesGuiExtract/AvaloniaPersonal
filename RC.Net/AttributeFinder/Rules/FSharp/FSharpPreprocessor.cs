using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.FSharp;
using Microsoft.FSharp.Core;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="FSharpPreprocessor"/> class.
    /// </summary>
    [ComVisible(true)]
    [CLSCompliant(false)]
    [Guid("03F5BFFF-69C8-40A8-9BD5-AB765CB3ED56")]
    public interface IFSharpPreprocessor : IDocumentPreprocessor, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {
        /// <summary>
        /// The path to the script to load functions from
        /// </summary>
        string ScriptPath { get; set; }

        /// <summary>
        /// The unqualified name of the function to run
        /// </summary>
        string FunctionName { get; set; }

        /// <summary>
        /// Whether the generated types can be garbage collected
        /// </summary>
        /// <remarks>
        /// If Collectible is true then some code features will not work but memory usage will be lower
        /// </remarks>
        bool Collectible { get; set; }
    }

    /// <summary>
    /// An <see cref="IPreProcessor"/> that runs an fsharp function
    /// </summary>
    [ComVisible(true)]
    [CLSCompliant(false)]
    [Guid("8D9E6688-4E43-432A-9686-12CF175B1192")]
    public class FSharpPreprocessor : IdentifiableObject, IFSharpPreprocessor
    {
        #region Constants

        /// <summary>
        /// The description of the rule.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "FSharp preprocessor";

        /// <summary>
        /// Current version.
        /// </summary>
        /// <remarks>
        /// Version 2: Add Collectible
        /// </remarks>
        const int _CURRENT_VERSION = 2;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Used to expand path tags and functions
        /// </summary>
        AFUtility _afUtility;

        /// <summary>
        /// <c>true</c> if changes have been made to this instance since it was created;
        /// <c>false</c> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FSharpPreprocessor"/> class.
        /// </summary>
        public FSharpPreprocessor()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46948");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FSharpPreprocessor"/> class as a
        /// copy of <see paramref="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="FSharpPreprocessor"/> from which
        /// settings should be copied.</param>
        public FSharpPreprocessor(FSharpPreprocessor source)
        {
            try
            {
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46949");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The path to the script to load functions from
        /// </summary>
        public string ScriptPath { get; set; }

        /// <summary>
        /// The unqualified name of the function to run
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Whether the generated types can be garbage collected
        /// </summary>
        /// <remarks>
        /// If Collectible is true then some code features will not work but memory usage will be lower
        /// </remarks>
        public bool Collectible { get; set; } = true;

        #endregion Properties

        #region IDocumentPreprocessor members

        public void Process(AFDocument pDocument, ProgressStatus pProgressStatus)
        {
            try
            {
                pDocument.Attribute.ReportMemoryUsage();

                var expandedScriptPath = Path.GetFullPath(AFUtility.ExpandTagsAndFunctions(ScriptPath, pDocument));
                var componentDataDir = AFUtility.ExpandTagsAndFunctions("<ComponentDataDir>", pDocument);

                FSharpFunc<AFDocument, AFDocument> fun = null;
                try
                {
                    fun = FileDerivedResourceCache.GetCachedObject(
                        () => FunctionLoader.LoadFunction<AFDocument>(expandedScriptPath, FunctionName, Collectible, componentDataDir),
                        expandedScriptPath,
                        componentDataDir, // include this dir so that there will be a unique entry in the cache for each FKB in use
                        Collectible.ToString(),
                        FunctionName);

                }
                catch (Exception ex)
                {
                    var uex = new ExtractException("ELI46939", "Unable to get function", ex);
                    uex.AddDebugData("Script path", expandedScriptPath);
                    uex.AddDebugData("Function name", FunctionName);
                    throw uex;
                }

                // Partial clone the input to avoid accidental changes to rsd stack, etc
                var clone = pDocument.PartialClone(false, false);
                var res = fun.Invoke(clone);

                // Copy back the attribute and any tags
                pDocument.Attribute = res.Attribute;
                pDocument.ObjectTags = res.ObjectTags;
                pDocument.StringTags = res.StringTags;

                pDocument.Attribute.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46940", "Failed to preprocess document", ex);
            }
        }

        #endregion IDocumentPreprocessor members

        #region IConfigurableObject Members

        /// <summary>
        /// Displays a form to allow configuration of this <see cref="FSharpPreprocessor"/>
        /// instance.
        /// </summary>
        /// <returns><c>true</c> if the configuration was successfully updated or
        /// <c>false</c> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI46941", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                FSharpPreprocessor cloneOfThis = (FSharpPreprocessor)Clone();

                using (FSharpPreprocessorSettingsDialog dlg
                    = new FSharpPreprocessorSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI46942", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Determines whether this instance is configured.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is configured; otherwise,
        /// <c>false</c>.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                return !(string.IsNullOrWhiteSpace(ScriptPath) || string.IsNullOrWhiteSpace(FunctionName));
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46943",
                    "Error checking configuration of Rule object.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="FSharpPreprocessor"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="FSharpPreprocessor"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new FSharpPreprocessor(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46944",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="FSharpPreprocessor"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as FSharpPreprocessor;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to FSharpPreProcessor");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46945",
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
        /// <returns><c>true</c> if the component is licensed; <see langword="false"/> 
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
                    ScriptPath = reader.ReadString();
                    FunctionName = reader.ReadString();
                    if (reader.Version >= 2)
                    {
                        Collectible = reader.ReadBoolean();
                    }

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46946",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the object should reset
        /// its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <c>true</c>, the flag should be cleared. If
        /// <c>false</c>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(ScriptPath);
                    writer.Write(FunctionName);
                    writer.Write(Collectible);

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
                throw ex.CreateComVisible("ELI46947",
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
        /// "UCLID AF-API Document Preprocessors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.DocumentPreprocessorsGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID AF-API Document Preprocessors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.DocumentPreprocessorsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="FSharpPreprocessor"/> instance into this one.
        /// </summary><param name="source">The <see cref="FSharpPreprocessor"/> from which to copy.
        /// </param>
        void CopyFrom(FSharpPreprocessor source)
        {
            ScriptPath = source.ScriptPath;
            FunctionName = source.FunctionName;
            Collectible = source.Collectible;

            _dirty = true;
        }

        /// <summary>
        /// Gets an <see cref="AFUtility"/> instance.
        /// </summary>
        AFUtility AFUtility
        {
            get
            {
                if (_afUtility == null)
                {
                    _afUtility = new AFUtility();
                }

                return _afUtility;
            }
        }

        #endregion Private Members
    }
}
