using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for <see cref="MoveCopyAttributes"/> class.
    /// </summary>
    [ComVisible(true)]
    [CLSCompliant(false)]
    [Guid("58D57B5E-B30F-487D-BDC9-FEFE38109E74")]
    public interface IMoveCopyAttributes : IOutputHandler, ICategorizedComponent, IConfigurableObject, ICopyableObject, ILicensedComponent,
        IPersistStream, IIdentifiableObject
    {
        /// <summary>
        /// The XPath query to identify the source of the attributes to be moved or copied
        /// </summary>
        string SourceAttributeTreeXPath { get; set; }

        /// <summary>
        /// The XPath query to identify the Destination for the attributes being moved or copied
        /// </summary>
        string DestinationAttributeTreeXPath { get; set; }

        /// <summary>
        /// If <c>true</c> then Attributes should be copied from the Source to the Destination
        /// if <c>false</c> then Attributes should be moved from the Source to the Destination
        /// </summary>
        bool CopyAttributes { get; set; }
    }

    /// <summary>
    /// <see cref="IOutputHandler"/> class to move or copy attributes using XPath to select the destinations and sources
    /// </summary>
    [ComVisible(true)]
    [CLSCompliant(false)]
    [Guid("416994A8-28D8-4C8D-AAD8-C932935C32AC")]
    public class MoveCopyAttributes : IdentifiableObject, IMoveCopyAttributes
    {
        #region Constructors

        /// <summary>
        /// Initializes MoveCopyAttributes
        /// </summary>
        public MoveCopyAttributes()
        {
            SourceAttributeTreeXPath = string.Empty;
            DestinationAttributeTreeXPath = string.Empty;
            CopyAttributes = false;
        }

        /// <summary>
        /// Initializes MoveCopyAttributes by copying from given instances
        /// </summary>
        /// <param name="copyAttributes">Instance to copy values from</param>
        public MoveCopyAttributes(MoveCopyAttributes copyAttributes)
        {
            try
            {
                CopyFrom(copyAttributes);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46930");
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="MoveCopyAttributes"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion

        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Move/Copy attributes";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        #endregion Constants

        #region IMoveCopyAttributes Properties

        /// <summary>
        /// The XPath query that selects the source attributes
        /// </summary>
        public string SourceAttributeTreeXPath { get; set; }

        /// <summary>
        /// The XPath query that selects the destination attributes
        /// </summary>
        public string DestinationAttributeTreeXPath { get; set; }

        /// <summary>
        /// Flag that indicates if the attributes selected should be copied or moved.
        /// </summary>
        public bool CopyAttributes { get; set; }

        #endregion

        #region IOutputHandler Members

        /// <summary>
        /// Processes the output <see paramref="pAttributes"/>.
        /// </summary>
        /// <param name="pAttributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// on which this task is to be run.</param>
        /// <param name="pDoc">The <see cref="AFDocument"/> the attributes are associated with.
        /// </param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> displaying the progress.
        /// </param>
        public void ProcessOutput(IUnknownVector pAttributes, AFDocument pDoc, ProgressStatus pProgressStatus)
        {
            try
            {
                pAttributes.ReportMemoryUsage();

                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI46998", _COMPONENT_DESCRIPTION);

                ExtractException.Assert("ELI46983", "Source cannot be root.", SourceAttributeTreeXPath != "/");

                var xPathContext = new XPathContext(pAttributes);

                var sources = (xPathContext.Evaluate(SourceAttributeTreeXPath) as List<object>)
                    ?.OfType<IAttribute>()
                    .ToList();

                var destinations = (xPathContext.Evaluate(DestinationAttributeTreeXPath) as List<object>)
                    ?.OfType<IAttribute>().ToList();

                if (destinations.Count == 0)
                {
                    // If the destination is the root copy all of the sources to root
                    if (DestinationAttributeTreeXPath == "/")
                    {
                        CopyOrMoveToRoot(pAttributes, sources);

                    }
                    return;
                }

                // Do not allow Destination to be in a Source tree
                if (destinations.Any(d => sources.Any(s => AttributeMethods.EnumerateDepthFirst(s).Contains(d))))
                {
                    ExtractException ee = new ExtractException("ELI46984", "Destination node cannot be in Source tree.");
                    throw ee;
                }

                foreach (var baseAttribute in pAttributes.ToIEnumerable<IAttribute>())
                {
                    var destinationsUnderBase = destinations
                        .Where(d => AttributeMethods.EnumerateDepthFirst(baseAttribute).Contains(d))
                        .ToList();

                    var sourcesUnderBase = sources
                        .Where(s => AttributeMethods.EnumerateDepthFirst(baseAttribute).Contains(s))
                        .ToList();

                    // find sources that are not under any dest : Move all these sources to dest within the same tree
                    var sourcesNotUnderDestinations = sourcesUnderBase
                        .Where(s => !destinationsUnderBase.Any(d => AttributeMethods.EnumerateDepthFirst(d).Contains(s)))
                        .ToList();

                    // Find sources that are under destination
                    var sourcesUnderDestinations = sourcesUnderBase.Except(sourcesNotUnderDestinations).ToList();

                    // Copy the sources that are under a destination
                    if (sourcesUnderDestinations.Count > 0)
                    {
                        destinationsUnderBase.ForEach(d =>
                        {
                            IUnknownVector sourceUnderDest = null;
                            sourceUnderDest = sourcesUnderDestinations
                                .Where(s => AttributeMethods.EnumerateDepthFirst(d).Any(v => v.Equals(s)))
                                ?.Select(a => (a as ICopyableObject)?.Clone())
                                ?.OfType<IAttribute>()?.ToIUnknownVector();

                            if (sourceUnderDest != null && sourceUnderDest.Size() > 0)
                            {
                                d.SubAttributes.Append(sourceUnderDest);
                            }

                            sourceUnderDest.ReportMemoryUsage();
                        });
                    }

                    // Copy the sources not under a destination to all destinations
                    if (sourcesNotUnderDestinations.Count > 0)
                    {
                        if (destinationsUnderBase.Count == 0)
                        {
                            CopySourcesToDestinations(sourcesNotUnderDestinations, destinations);
                        }
                        else if (sourcesNotUnderDestinations.Count > 0)
                        {
                            CopySourcesToDestinations(sourcesNotUnderDestinations, destinationsUnderBase);
                        }
                    }

                    // If not copying the attributes remove them
                    if (!CopyAttributes)
                    {
                        pAttributes.RemoveAttributes(sourcesUnderDestinations);
                        pAttributes.RemoveAttributes(sourcesNotUnderDestinations);
                    }
                }
            }
            catch (Exception ex)
            {
                var ee = ex.CreateComVisible("ELI46938", "Move/copy attribute task failed.");
                ee.AddDebugData("Source XPath", SourceAttributeTreeXPath);
                ee.AddDebugData("Destination XPath", DestinationAttributeTreeXPath);
                throw ee;
            }
        }

        #endregion

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

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="MoveCopyAttributes"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI46936", _COMPONENT_DESCRIPTION);

                MoveCopyAttributes cloneOfThis = (MoveCopyAttributes)Clone();

                using (var dlg = new MoveCopyAttributesSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI46937", "Error running configuration.");
            }
        }

        #endregion

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="MoveCopyAttributes"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="MoveCopyAttributes"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new MoveCopyAttributes(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46929", "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="MoveCopyAttributes"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as MoveCopyAttributes;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to " + _COMPONENT_DESCRIPTION);
                }

                SourceAttributeTreeXPath = source.SourceAttributeTreeXPath;
                DestinationAttributeTreeXPath = source.DestinationAttributeTreeXPath;
                CopyAttributes = source.CopyAttributes;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46931", "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion

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
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    SourceAttributeTreeXPath = reader.ReadString();

                    DestinationAttributeTreeXPath = reader.ReadString();

                    CopyAttributes = reader.ReadBoolean();

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46927",
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
        public void Save(IStream stream, [MarshalAs(UnmanagedType.Bool)] bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(SourceAttributeTreeXPath);

                    writer.Write(DestinationAttributeTreeXPath);

                    writer.Write(CopyAttributes);

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
                throw ex.CreateComVisible("ELI46928",
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

        #endregion

        #region Private Members

        void CopyOrMoveToRoot(IUnknownVector pAttributes, List<IAttribute> sources)
        {
            IUnknownVector clonedSources = sources
                ?.Select(a => (a as ICopyableObject)?.Clone())
                ?.OfType<IAttribute>().ToIUnknownVector();

            if (!CopyAttributes)
            {
                pAttributes.RemoveAttributes(sources);
            }

            pAttributes.Append(clonedSources);

            clonedSources.ReportMemoryUsage();
        }

        static void CopySourcesToDestinations(List<IAttribute> sources, List<IAttribute> destinations)
        {
            destinations.ForEach(d =>
            {
                IUnknownVector sourcesToCopyMove = null;
                sourcesToCopyMove = sources
                    ?.Select(a => (a as ICopyableObject)?.Clone())
                    ?.OfType<IAttribute>()?.ToIUnknownVector();

                if (sourcesToCopyMove != null && sourcesToCopyMove.Size() > 0)
                {
                    d.SubAttributes.Append(sourcesToCopyMove);
                }
            });
        }

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// appropriate COM categories.
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
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        #endregion
    }
}
