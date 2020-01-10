using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="InputFinder"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("C65C9DFD-E43A-42A4-9A6F-5FEA604C2ED6")]
    [CLSCompliant(false)]
    public interface IInputFinder : IAttributeFindingRule, ICategorizedComponent,
        ICopyableObject, ILicensedComponent, IPersistStream, IIdentifiableObject
    {
    }

    /// <summary>
    /// An <see cref="IAttributeFindingRule"/> that adds the <see cref="T:AFDocument.Attribute"/>
    /// (and children) as a literal output attribute.
    /// </summary>
    [ComVisible(true)]
    [Guid("26F84085-12B8-48EC-A654-5A47508A3DF9")]
    [CLSCompliant(false)]
    public class InputFinder : IdentifiableObject, IInputFinder
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Input finder";

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
        /// <see langword="true"/> if changes have been made to <see cref="InputFinder"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InputFinder"/> class.
        /// </summary>
        public InputFinder()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40337");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputFinder"/> class as a copy of
        /// the specified <see paramref="inputFinder"/>.
        /// </summary>
        /// <param name="inputFinder">The <see cref="InputFinder"/> from which
        /// settings should be copied.</param>
        public InputFinder(InputFinder inputFinder)
        {
            try
            {
                CopyFrom(inputFinder);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40338");
            }
        }

        #endregion Constructors

        #region IAttributeFindingRule

        /// <summary>
        /// Parses the <see paramref="pDocument"/> and returns a vector of found
        /// <see cref="ComAttribute"/> objects.
        /// </summary>
        /// <param name="pDocument">The <see cref="AFDocument"/> to parse.</param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> to indicate processing
        /// progress.</param>
        /// <returns>An <see cref="IUnknownVector"/> of found <see cref="ComAttribute"/>s.</returns>
        public IUnknownVector ParseText(AFDocument pDocument, ProgressStatus pProgressStatus)
        {
            try
            {
                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pDocument.Attribute.ReportMemoryUsage();
                // Create an AFDocument copy that clones only the attribute hierarchy and not the
                // document text. The AttributeRule class will have cloned the document text
                // already, but not the attribute hierarchy. We don't want the downstream rules to
                // be able to inadvertently modify the AFDocument attribute hierarchy.
                AFDocument afDocCopy = pDocument.PartialClone(true, false);
                IAttribute attribute = afDocCopy.Attribute;

                // Update source document of subattributes
                if (!String.IsNullOrEmpty(pDocument.Text.SourceDocName))
                {
                    foreach (var a in attribute.EnumerateDepthFirst())
                    {
                        a.Value.SourceDocName = pDocument.Text.SourceDocName;
                    }
                }
                IUnknownVector result = new IUnknownVector();
                result.PushBack(attribute);

                // So that the garbage collector knows of and properly manages the associated
                // memory (including the new attribute copy).
                result.ReportMemoryUsage();

                return result;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34805", "Failed to find attributes from input.");
            }
        }

        #endregion IAttributeFindingRule

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="InputFinder"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="InputFinder"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new InputFinder(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI40339",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="InputFinder"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as InputFinder;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to InputFinder");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI40340",
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
                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI40341",
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
                throw ex.CreateComVisible("ELI40342",
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
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.ValueFindersGuid);
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
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.ValueFindersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="InputFinder"/> instance into this one.
        /// </summary><param name="source">The <see cref="InputFinder"/> from which to copy.
        /// </param>
        // Even though this currently does nothing, this method is here to keep the ICopyableObject
        // pattern consistent. Block FXCop warnings related to the fact this currently does nothing.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "source")]
        void CopyFrom(InputFinder source)
        {
            // Nothing to do.
        }

        #endregion Private Members
    }
}
