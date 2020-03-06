using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="DuplicateAndSeparateTrees"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("C2241B09-F2CA-4DEA-B12B-91620343BB5E")]
    [CLSCompliant(false)]
    public interface IDuplicateAndSeparateTrees : IOutputHandler, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {

        /// <summary>
        /// Gets or sets the <see cref="IAttributeSelector"/> used to specify which attribute(s)
        /// are to be duplicated and separated.
        /// </summary>
        /// <value>
        /// The <see cref="IAttributeSelector"/> used to specify which attribute(s) are to be
        /// duplicated and separated.
        /// </value>
        IAttributeSelector AttributeSelector
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the dividing attribute.
        /// </summary>
        /// <value>
        /// The name of the dividing attribute.
        /// </value>
        string DividingAttributeName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to run an output handler on resulting trees.
        /// </summary>
        bool RunOutputHandler
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the output handler to run on resulting trees
        /// </summary>
        IOutputHandler OutputHandler
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IOutputHandler"/> that will duplicate selected attributes along with their
    /// descendant trees within the output by using child attributes of a specific name such that the
    /// final result will contain as many copies of the original tree as there are child attributes
    /// matching the dividing attribute name. Each copy of the tree will have only one of the
    /// original dividing attribute instances, but a copy of all non-dividing attribute instances.
    /// </summary>
    [ComVisible(true)]
    [Guid("DC265266-BFDE-4D45-AF91-CFEA253040E2")]
    [CLSCompliant(false)]
    public class DuplicateAndSeparateTrees : IdentifiableObject, IDuplicateAndSeparateTrees
    {
        #region Constants

        /// <summary>
        /// The description of the rule.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Duplicate and separate attribute trees";

        /// <summary>
        /// Current version.
        /// <para>Version 2: Added IdentifiableObject inheritance</para>
        /// <para>Version 3: Added feature to run an output handler on resulting trees.</para>
        /// </summary>
        const int _CURRENT_VERSION = 3;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Used to locate the parent of each selected attribute (if one exists).
        /// </summary>
        AFUtility _afUtility;

        /// <summary>
        /// The <see cref="IAttributeSelector"/> used to specify which attribute(s) are to be
        /// duplicated and separated.
        /// </summary>
        IAttributeSelector _attributeSelector;

        /// <summary>
        /// The name of the dividing attribute.
        /// </summary>
        string _dividingAttributeName;

        /// <summary>
        /// The <see cref="IOutputHandler"/> to be run on each tree after it is created.
        /// </summary>
        IOutputHandler _outputHandler;

        /// <summary>
        /// Indicates whether to run an output handler on resulting trees.
        /// </summary>
        bool _runOutputHandler;

        /// <summary>
        /// <see langword="true"/> if changes have been made to this instance since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateAndSeparateTrees"/> class.
        /// </summary>
        public DuplicateAndSeparateTrees()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33462");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateAndSeparateTrees"/> class as a
        /// copy of <see paramref="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="DuplicateAndSeparateTrees"/> from which
        /// settings should be copied.</param>
        public DuplicateAndSeparateTrees(DuplicateAndSeparateTrees source)
        {
            try
            {
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33463");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IAttributeSelector"/> used to specify which attribute(s)
        /// are to be duplicated and separated.
        /// </summary>
        /// <value>
        /// The <see cref="IAttributeSelector"/> used to specify which attribute(s) are to be
        /// duplicated and separated.
        /// </value>
        public IAttributeSelector AttributeSelector
        {
            get
            {
                return _attributeSelector;
            }

            set
            {
                try
                {
                    if (_attributeSelector != value)
                    {
                        _attributeSelector = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33501");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the dividing attribute.
        /// </summary>
        /// <value>
        /// The name of the dividing attribute.
        /// </value>
        public string DividingAttributeName
        {
            get
            {
                return _dividingAttributeName;
            }

            set
            {
                try
                {
                    if (_dividingAttributeName != value)
                    {
                        _dividingAttributeName = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33500");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to run an output handler on resulting trees.
        /// </summary>
        public bool RunOutputHandler
        {
            get
            {
                return _runOutputHandler;
            }

            set
            {
                try
                {
                    if (_runOutputHandler != value)
                    {
                        _runOutputHandler = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38446");
                }
            }
        }

        /// <summary>
        /// Gets or sets the output handler to run on resulting trees
        /// </summary>
        public IOutputHandler OutputHandler
        {
            get
            {
                return _outputHandler;
            }

            set
            {
                try
                {
                    if (_outputHandler != value)
                    {
                        _outputHandler = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38437");
                }
            }
        }

        #endregion Properties

        #region IOutputHandler Members

        /// <summary>
        /// Processes the output (<see paramref="pAttributes"/>) by duplicating selected attributes
        /// along with their descendant trees by using child attributes of a specific name such that
        /// the final result will contain as many copies of the original tree as there are child
        /// attributes matching the dividing attribute name. Each copy of the tree will have only
        /// one of the original dividing attribute instances, but a copy of all non-dividing
        /// attribute instances.
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33464", _COMPONENT_DESCRIPTION);

                ExtractException.Assert("ELI33496", "Rule is not properly configured.",
                    IsConfigured());

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pAttributes.ReportMemoryUsage();

                // Obtain all attributes specified as candidates to be duplicated.
                IEnumerable<ComAttribute> selectedAttributes;
                using (RuleObjectProfiler profiler =
                    new RuleObjectProfiler("", "", AttributeSelector, 0))
                {
                    selectedAttributes = AttributeSelector.SelectAttributes(pAttributes, pDoc, pAttributes)
                        .ToIEnumerable<ComAttribute>();
                }

                // Process each of the selected attributes.
                foreach (ComAttribute attribute in selectedAttributes)
                {
                    // So that the garbage collector knows of and properly manages the associated
                    // memory.
                    attribute.ReportMemoryUsage();

                    DuplicateAndSeparateTree(pAttributes, attribute, pDoc);
                }

                // Report memory usage of hierarchy after processing to ensure all COM objects
                // referenced in final result are reported.
                pAttributes.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33465", "Failed to duplicate and separate attribute trees.");
            }
        }

        #endregion IOutputHandler Members

        #region IConfigurableObject Members

        /// <summary>
        /// Displays a form to allow configuration of this <see cref="DuplicateAndSeparateTrees"/>
        /// instance.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33466", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                DuplicateAndSeparateTrees cloneOfThis = (DuplicateAndSeparateTrees)Clone();

                using (DuplicateAndSeparateTreesSettingsDialog dlg
                    = new DuplicateAndSeparateTreesSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI33467", "Error running configuration.");
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
                if (   AttributeSelector == null
                    || RunOutputHandler && OutputHandler == null
                    || !UtilityMethods.IsValidIdentifier(DividingAttributeName)
                    )
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33491",
                    "Error checking configuration of duplicate and separate attribute trees.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="DuplicateAndSeparateTrees"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="DuplicateAndSeparateTrees"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new DuplicateAndSeparateTrees(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33468",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="DuplicateAndSeparateTrees"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as DuplicateAndSeparateTrees;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to DuplicateAndSeparateTrees");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33469",
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
                    AttributeSelector = reader.ReadIPersistStream() as IAttributeSelector;
                    DividingAttributeName = reader.ReadString();

                    if (reader.Version >= 3)
                    {
                        RunOutputHandler = reader.ReadBoolean();
                        if (RunOutputHandler)
                        {
                            OutputHandler = reader.ReadIPersistStream() as IOutputHandler;
                        }
                    }

                    if (reader.Version >= 2)
                    {
                        // Load the GUID for the IIdentifiableObject interface.
                        LoadGuid(stream);
                    }
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33470",
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
                    #pragma warning disable 618
                    writer.Write(ComUtilities.GetIPersistStreamInterface(AttributeSelector), clearDirty);
                    #pragma warning restore 618
                    writer.Write(DividingAttributeName);
                    writer.Write(RunOutputHandler);
                    if (RunOutputHandler)
                    {
                        #pragma warning disable 618
                        writer.Write(ComUtilities.GetIPersistStreamInterface(OutputHandler), clearDirty);
                        #pragma warning restore 618
                    }

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
                throw ex.CreateComVisible("ELI33471",
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
        /// Copies the specified <see cref="DuplicateAndSeparateTrees"/> instance into this one.
        /// </summary><param name="source">The <see cref="DuplicateAndSeparateTrees"/> from which to copy.
        /// </param>
        void CopyFrom(DuplicateAndSeparateTrees source)
        {
            if (source.AttributeSelector == null)
            {
                AttributeSelector = null;
            }
            else
            {
                ICopyableObject copyThis = (ICopyableObject)source.AttributeSelector;
                AttributeSelector = (IAttributeSelector)copyThis.Clone();
            }
            DividingAttributeName = source.DividingAttributeName;
            RunOutputHandler = source.RunOutputHandler;

            if (source.OutputHandler == null)
            {
                OutputHandler = null;
            }
            else
            {
                ICopyableObject copyThis = (ICopyableObject)source.OutputHandler;
                OutputHandler = (IOutputHandler)copyThis.Clone();
            }
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

        /// <summary>
        /// Creates a duplicate of <see paramref="attribute"/> for each child attribute having the
        /// <see cref="DividingAttributeName"/>. Each will contain copies of all non-dividing
        /// attributes, but only one dividing attribute instance.
        /// </summary>
        /// <param name="rootAttributes">All attributes being processed by this output handler.
        /// </param>
        /// <param name="attribute">The <see cref="ComAttribute"/> to duplicate and separate if
        /// appropriate.</param>
        /// <param name="pDoc">The <see cref="AFDocument"/> the output is from.</param>
        void DuplicateAndSeparateTree(IUnknownVector rootAttributes, ComAttribute attribute, AFDocument pDoc)
        {
            // Contains any new hierarchies that are produced.
            var duplicatedTreeRoots = new List<ComAttribute>();
            var duplicatedTrees = new List<IUnknownVector>();

            // Contains all non-dividing attributes encountered.
            List<ComAttribute> nonDividingAttributes = new List<ComAttribute>();

            // Loop through all sub-attributes. Build a collection of root attributes and their trees
            // by copying references only. Deep copies will be made later, after all the trees have been constructed.
            foreach (ComAttribute subAttribute in attribute.SubAttributes
                .ToIEnumerable<ComAttribute>())
            {
                if (subAttribute.Name.Equals(DividingAttributeName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    duplicatedTreeRoots.Add(attribute);
                    var newTree = new IUnknownVector();
                    duplicatedTrees.Add(newTree);
                    newTree.Append(nonDividingAttributes.ToIUnknownVector());
                    newTree.PushBack(subAttribute);
                }
                else
                {
                    // Add this attribute to all duplicate trees created thus far.
                    foreach (IUnknownVector duplicateTree in duplicatedTrees)
                    {
                        duplicateTree.PushBack(subAttribute);
                    }

                    nonDividingAttributes.Add(subAttribute);
                }
            }

            // If more than one dividing attribute was found and/or an output handler will be run
            // then it is necessary to find the place in the original vector where the result vector
            // will be spliced.
            int spliceIndex = 0;
            IUnknownVector outputVector = rootAttributes;
            if (RunOutputHandler || duplicatedTrees.Count > 1)
            {
                ComAttribute parentAttribute =
                    AFUtility.GetAttributeParent(rootAttributes, attribute);
                if (parentAttribute != null)
                {
                    outputVector = parentAttribute.SubAttributes;
                }

                outputVector.FindByReference(attribute, 0, ref spliceIndex);
                ExtractException.Assert("ELI38441", "Internal logic error.", spliceIndex >= 0);

                // The original tree will be replaced with zero or more trees so remove the existing tree
                outputVector.Remove(spliceIndex);
            }

            // Process the resulting trees
            // If no dividing attributes were found just process the input attribute if necessary
            if (duplicatedTreeRoots.Count == 0 && RunOutputHandler)
            {
                var resultVector = new IUnknownVector();
                resultVector.PushBack(attribute);

                OutputHandler.ProcessOutput(resultVector, pDoc, null);

                // Add new tree(s)
                if (resultVector.Size() > 0)
                {
                    outputVector.InsertVector(spliceIndex, resultVector);
                }
            }
            else
            {
                // Process each tree.
                IUnknownVector resultVector;
                for (int i = 0; i < duplicatedTrees.Count; i++)
                {
                    resultVector = new IUnknownVector();

                    // All but the last tree needs to be cloned from the original pieces
                    if (i < duplicatedTrees.Count - 1)
                    {
                        ComAttribute newAttribute = (ComAttribute)((ICopyableObject)duplicatedTreeRoots[i]).Clone();
                        newAttribute.SubAttributes = (IUnknownVector) ((ICopyableObject)duplicatedTrees[i]).Clone();
                        resultVector.PushBack(newAttribute);
                    }
                    // Only rebuild the original tree if there was more than one dividing attribute
                    // or there was an output handler to run on the results.
                    else if (RunOutputHandler || duplicatedTrees.Count > 1)
                    {
                        // The last tree's parts don't need to be cloned because no more copies will
                        // be made from the originals.
                        ComAttribute lastAttribute = duplicatedTreeRoots[i];
                        lastAttribute.SubAttributes = duplicatedTrees[i];
                        resultVector.PushBack(lastAttribute);
                    }

                    // Process the result if required
                    if (RunOutputHandler)
                    {
                        try
                        {
                            OutputHandler.ProcessOutput(resultVector, pDoc, null);
                        }
                        catch (Exception ex)
                        {
                            throw ex.AsExtract("ELI38442");
                        }
                    }

                    // Add new tree(s)
                    if (resultVector.Size() > 0)
                    {
                        outputVector.InsertVector(spliceIndex, resultVector);
                    }
                    spliceIndex += resultVector.Size();
                }
            }
        }

        #endregion Private Members
    }
}