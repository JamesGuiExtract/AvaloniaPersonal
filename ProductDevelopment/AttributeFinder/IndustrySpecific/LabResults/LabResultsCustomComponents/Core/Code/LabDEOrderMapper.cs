using Extract.AttributeFinder;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlServerCe;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// Handles mapping orders from the rules output into Lab orders and their
    /// associated EPIC codes based on a database file.
    /// </summary>
    [Guid("ABC13C14-B6C6-4679-A69B-5083D3B4B60C")]
    [ProgId("Extract.DataEntry.LabDE.LabDEOrderMapper")]
    [ComVisible(true)]
    public class LabDEOrderMapper : IOutputHandler, ICopyableObject, ICategorizedComponent,
        IPersistStream, IConfigurableObject, IMustBeConfiguredObject, ILicensedComponent, IDisposable
    {
        #region Constants

        /// <summary>
        /// The default filename that will appear in the FAM to describe the task the data entry
        /// application is fulfilling
        /// </summary>
        static readonly string _DEFAULT_OUTPUT_HANDLER_NAME = "LabDE order mapper";

        /// <summary>
        /// The current version for this object.
        /// Version 2: Added support for configuring whether mandatory tests are required in
        /// the second pass of the order mapping algorithm.
        /// Version 3: Added support for filled requirement of orders.
        /// Added support for configuring whether provided OutstandingOrderCode
        /// attribute values are preferred when mapping.
        /// Added support for eliminating duplicate subattributes after mapping.
        /// </summary>
        static readonly int _CURRENT_VERSION = 3;

        /// <summary>
        /// A couple algorithms used in the order mapper to find the best combinations of objects
        /// are NP-complete. To prevent a runaway computation from exhausting all memory, these
        /// algorithms will be capped at this many possible grouping combinations to be considering
        /// at one time.
        /// </summary>
        internal static readonly int _COMBINATION_ALGORITHM_SAFETY_CUTOFF = 5000;

        #endregion Constants

        #region OrderGroupingPermutation

        /// <summary>
        /// Represents a possible order grouping permutation, containing all tests that are
        /// part of the grouping as well as a reference to which orders combined to produce
        /// this permutation.
        /// </summary>
        class OrderGroupingPermutation
        {
            /// <summary>
            /// The database connection to use for mapping orders.
            /// </summary>
            OrderMappingDBCache _dbCache;

            /// <summary>
            /// The sourcedoc name to use in the grouping.
            /// </summary>
            string _sourceDocName;

            /// <summary>
            /// The combined order grouping that this permutation contains.
            /// </summary>
            public OrderGrouping CombinedGroup;

            /// <summary>
            /// The list of <see cref="OrderGrouping"/> objects that are contained by this
            /// order grouping permutation.
            /// </summary>
            public List<OrderGrouping> ContainedGroups = new List<OrderGrouping>();

            /// <summary>
            /// The collection of <see cref="LabTest"/> objects contained by this order
            /// mapping.
            /// </summary>
            public Dictionary<string, LabTest> ContainedTests =
                new Dictionary<string, LabTest>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// The set of all known outstanding order codes. Only orders in this set will be
            /// considered for use. If null then all orders will be considered.
            /// </summary>
            public HashSet<string> OutstandingOrderCodes;

            /// <summary>
            /// Initializes a new instance of the <see cref="OrderGroupingPermutation"/>
            /// class.
            /// </summary>
            /// <param name="originalGroup">The original order group for this permutation (before
            /// any merging with other orders has taken place).</param>
            /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use when performing the
            /// order mapping.</param>
            /// <param name="sourceDocName">The sourcedoc name to use in the grouping.</param>
            public OrderGroupingPermutation(OrderGrouping originalGroup,
                OrderMappingDBCache dbCache, string sourceDocName)
            {
                // The initial combined group contains only the original group
                CombinedGroup = originalGroup;
                ContainedGroups.Add(originalGroup);
                OutstandingOrderCodes = originalGroup.OutstandingOrderCodes;

                // Initialize the dictionary of contained tests
                foreach (LabTest test in originalGroup.LabTests)
                {
                    ContainedTests[test.Name] = test;
                }

                // Store the other data needed to perform mapping/merging
                _dbCache = dbCache;
                _sourceDocName = sourceDocName;
            }

            /// <summary>
            /// Attempts to merge the specified <see cref="OrderGrouping"/> with the
            /// currently contained order groupings to form a bigger order.
            /// </summary>
            /// <param name="newGroup">The <see cref="OrderGrouping"/> to
            /// merge with the current grouping.</param>
            /// <param name="labOrders">The collection of lab order codes to
            /// <see cref="LabOrder"/> objects.</param>
            /// <returns>A new merged <see cref="OrderGroupingPermutation"/>
            /// or <see langword="null"/> if no merging can take place.</returns>
            /// <param name="limitToOutstandingOrders">Whether to limit orders to be considered to
            /// the set of outstanding order codes as specified in either of the
            /// <see cref="OrderGrouping"/> objects.</param>
            public OrderGroupingPermutation AttemptMerge(OrderGrouping newGroup,
                Dictionary<string, LabOrder> labOrders, bool limitToOutstandingOrders)
            {
                if (!OrderGrouping.CollectionDatesEqual(CombinedGroup, newGroup))
                {
                    // Collection dates don't match, cannot group these orders
                    return null;
                }

                // Check if this group is already contained by the current group
                if (ContainedGroups.Contains(newGroup))
                {
                    // This group is already contained, just return null
                    return null;
                }

                // Build a new set of tests by adding newGroup's tests.
                Dictionary<string, LabTest> combinedTests =
                    new Dictionary<string, LabTest>(ContainedTests, StringComparer.OrdinalIgnoreCase);
                foreach (LabTest test in newGroup.LabTests)
                {
                    string name = test.Name;
                    if (combinedTests.ContainsKey(name))
                    {
                        // The test was already contained in the permutation, these two groups
                        // cannot be combined.
                        return null;
                    }

                    combinedTests[name] = test;
                }

                // Combine outstanding orders from both groups.
                if (limitToOutstandingOrders)
                {
                    OutstandingOrderCodes.UnionWith(newGroup.OutstandingOrderCodes);
                }
                
                // Groups could potentially be merged, find the best order
                KeyValuePair<string, List<LabTest>> bestOrder =
                    FindBestOrder(new List<LabTest>(combinedTests.Values),
                        labOrders, _dbCache, false, false, OutstandingOrderCodes);

                // In order to group into a new combined order, the resulting order must contain
                // all of the tests from the list.
                if (combinedTests.Count != bestOrder.Value.Count)
                {
                    return null;
                }

                // Clone the current CombinedGroup
                OrderGrouping combinedGroup = new OrderGrouping(CombinedGroup);

                // Add the attribute map from the newGroup into the combined group
                combinedGroup.InsertNameAttributeMap(newGroup);

                // Create a new combined group using the current combined group
                combinedGroup =
                    CreateNewOrderGrouping(bestOrder.Key, labOrders, _sourceDocName, combinedGroup);

                // Create the new grouping permutation
                OrderGroupingPermutation newGroupingPermutation =
                    new OrderGroupingPermutation(combinedGroup, _dbCache, _sourceDocName);
                newGroupingPermutation.ContainedTests = combinedTests;
                newGroupingPermutation.ContainedGroups = new List<OrderGrouping>(ContainedGroups);
                newGroupingPermutation.ContainedGroups.Add(newGroup);

                return newGroupingPermutation;
            }
        }

        #endregion OrderGroupingPermutation

        #region Fields

        /// <summary>
        /// For each order mapper instance, keeps track of the local database copy to use.
        /// </summary>
        TemporaryFileCopyManager _localDatabaseCopyManager;

        /// <summary>
        /// The name of the database file to use for order mapping.
        /// </summary>
        string _databaseFile;

        /// <summary>
        /// Flag to indicate whether this object is dirty or not.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Whether mandatory tests are required for the second pass of the order mapping algorithm.
        /// </summary>
        bool _requireMandatory;

        /// <summary>
        /// Whether to require that orders meet their filled requirement
        /// </summary>
        bool _useFilledRequirement = true;

        /// <summary>
        /// Whether to prefer orders that match known, outstanding order code attributes.
        /// </summary>
        bool _useOutstandingOrders = false;

        /// <summary>
        /// Whether to remove any duplicate Test subattributes after the mapping is finished.
        /// </summary>
        bool _eliminateDuplicateTestSubAttributes = false;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </summary>
        public LabDEOrderMapper()
            : this(null, false, true, false, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </summary>
        /// <param name="databaseFile">The name of the database file to attach to.</param>
        /// <param name="requireMandatory">Whether or not mandatory tests are required
        /// in the second pass of the order mapping algorithm.</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled requirement.</param>
        /// <param name="useOutstandingOrders">Whether to prefer orders that match known,
        /// outstanding order code attributes.</param>
        /// <param name="eliminateDuplicateTestSubAttributes">Whether to remove any duplicate Test
        /// subattributes after the mapping is finished. (Prevents lots of extra ResultDate attributes, e.g.)</param>
        public LabDEOrderMapper(
            string databaseFile,
            bool requireMandatory,
            bool useFilledRequirement,
            bool useOutstandingOrders,
            bool eliminateDuplicateTestSubAttributes)
        {
            try
            {
                _databaseFile = databaseFile;
                _localDatabaseCopyManager = new TemporaryFileCopyManager();
                _requireMandatory = requireMandatory;
                _useFilledRequirement = useFilledRequirement;
                _useOutstandingOrders = useOutstandingOrders;
                _eliminateDuplicateTestSubAttributes = eliminateDuplicateTestSubAttributes;
                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26169", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the database file name.
        /// </summary>
        /// <returns>The database file name.</returns>
        public string DatabaseFileName
        {
            get
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI26887", _DEFAULT_OUTPUT_HANDLER_NAME);

                return _databaseFile;
            }
            set
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI26895", _DEFAULT_OUTPUT_HANDLER_NAME);

                _databaseFile = value;
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets/sets whether mandatory tests are required during the second pass of the order
        /// mapping algorithm.
        /// </summary>
        public bool RequireMandatoryTests
        {
            get
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI30090", _DEFAULT_OUTPUT_HANDLER_NAME);

                return _requireMandatory;
            }
            set
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI30091", _DEFAULT_OUTPUT_HANDLER_NAME);

                _requireMandatory = value;
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets whether to require that orders meet their filled requirement
        /// </summary>
        public bool UseFilledRequirement
        {
            get
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI37880", _DEFAULT_OUTPUT_HANDLER_NAME);

                return _useFilledRequirement;
            }
            set
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI37881", _DEFAULT_OUTPUT_HANDLER_NAME);

                _useFilledRequirement = value;
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets whether to prefer orders with codes matching known, outstanding order codes
        /// </summary>
        public bool UseOutstandingOrders
        {
            get
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI37882", _DEFAULT_OUTPUT_HANDLER_NAME);

                return _useOutstandingOrders;
            }
            set
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI37883", _DEFAULT_OUTPUT_HANDLER_NAME);

                _useOutstandingOrders = value;
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets whether to remove any duplicate Test subattributes after the mapping is finished.
        /// </summary>
        public bool EliminateDuplicateTestSubAttributes
        {
            get
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI37884", _DEFAULT_OUTPUT_HANDLER_NAME);

                return _eliminateDuplicateTestSubAttributes;
            }
            set
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI37885", _DEFAULT_OUTPUT_HANDLER_NAME);

                _eliminateDuplicateTestSubAttributes = value;
                _dirty = true;
            }
        }

        #endregion Properties

        #region IOutputHandler Members

        /// <summary>
        /// Processes the attributes for output.
        /// </summary>
        /// <param name="pAttributes">The collection of attributes to process.</param>
        /// <param name="pDoc">The document object.</param>
        /// <param name="pProgressStatus">The progress status to update.</param>
        public void ProcessOutput(IUnknownVector pAttributes, AFDocument pDoc,
            ProgressStatus pProgressStatus)
        {
            OrderMappingDBCache dbCache = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI26889", _DEFAULT_OUTPUT_HANDLER_NAME);

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pAttributes.ReportMemoryUsage();

                // Create the database cache object
                dbCache = new OrderMappingDBCache(pDoc, _databaseFile);

                // Build a new vector of attributes that have been mapped to orders
                IUnknownVector newAttributes = new IUnknownVector();

                // Build the list of test attributes to map (all other attributes
                // should just be added to the new attributes collection
                int size = pAttributes.Size();
                List<IAttribute> testAttributes = new List<IAttribute>();
                for (int i = 0; i < size; i++)
                {
                    IAttribute attribute = (IAttribute)pAttributes.At(i);
                    if (attribute.Name.Equals("Test", StringComparison.OrdinalIgnoreCase))
                    {
                        testAttributes.Add(attribute);
                    }
                    else
                    {
                        // Not a test attribute, just copy it
                        newAttributes.PushBack(attribute);
                    }
                }

                // Only need to perform mapping if there are test attributes
                if (testAttributes.Count > 0)
                {
                    // Perform order mapping on the list of test attributes
                    IEnumerable<IAttribute> mappedAttributes = MapOrders(testAttributes, dbCache,
                        _requireMandatory, _useFilledRequirement, _useOutstandingOrders);

                    // Create an attribute sorter for sorting sub attributes
                    ISortCompare attributeSorter =
                                (ISortCompare)new SpatiallyCompareAttributesClass();

                    // Add each order to the output collection (sorting the subattributes)
                    List<IAttribute> temp = new List<IAttribute>();
                    foreach (IAttribute newAttribute in mappedAttributes)
                    {
                        // Remove duplicate subattributes if specified
                        if (_eliminateDuplicateTestSubAttributes)
                        {
                            EliminateDuplicates(newAttribute.SubAttributes);
                        }

                        // Sort the sub attributes spatially
                        newAttribute.SubAttributes.Sort(attributeSorter);

                        // Add the attribute to the temporary list
                        temp.Add(newAttribute);
                    }
                    
                    // Sort the orders spatially
                    temp.Sort((x, y) => CompareTestAttributes(attributeSorter, x, y));
                    
                    foreach (IAttribute newAttribute in temp)
                    {
                        // Add the attribute to the vector
                        newAttributes.PushBack(newAttribute);
                    }
                }

                // Finished with database so close connection
                dbCache.Dispose();
                dbCache = null;

                // Clear the original attributes and set the attributes to the
                // newly mapped collection
                pAttributes.Clear();
                pAttributes.CopyFrom(newAttributes);

                // Report memory usage of hierarchy after processing to ensure all COM objects
                // referenced in final result are reported.
                pAttributes.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26171", "Unable to handle output.", ex);
            }
            finally
            {
                if (dbCache != null)
                {
                    dbCache.Dispose();
                    dbCache = null;
                }
            }
        }

        #endregion

        #region ICopyableObject Members

        /// <summary>
        /// Returns a copy of this object.
        /// </summary>
        /// <returns>A copy of this object.</returns>
        public object Clone()
        {
            try
            {
                LabDEOrderMapper newMapper = new LabDEOrderMapper(_databaseFile, _requireMandatory,
                    _useFilledRequirement, _useOutstandingOrders, _eliminateDuplicateTestSubAttributes);

                return newMapper;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26173", "Clone failed.", ex);
            }
        }

        /// <summary>
        /// Sets this object from the specified object.
        /// </summary>
        /// <param name="pObject">The object to copy from.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                LabDEOrderMapper mapper = pObject as LabDEOrderMapper;
                if (mapper == null)
                {
                    ExtractException ee = new ExtractException("ELI26175", "Cannot copy from object!");
                    ee.AddDebugData("Object Type",
                        pObject != null ? pObject.GetType().ToString() : "null", false);
                    throw ee;
                }

                CopyFrom(mapper);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26176", "Unable to copy order mapper.", ex);
            }
        }

        /// <summary>
        /// Sets this <see cref="LabDEOrderMapper"/> from the specified
        /// <see cref="LabDEOrderMapper"/>.
        /// </summary>
        /// <param name="orderMapper">The <see cref="LabDEOrderMapper"/> to copy from.</param>
        [ComVisible(false)]
        public void CopyFrom(LabDEOrderMapper orderMapper)
        {
            try
            {
                _databaseFile = orderMapper.DatabaseFileName;
                _requireMandatory = orderMapper.RequireMandatoryTests;
                _useFilledRequirement = orderMapper.UseFilledRequirement;
                _useOutstandingOrders = orderMapper.UseOutstandingOrders;
                _eliminateDuplicateTestSubAttributes = orderMapper.EliminateDuplicateTestSubAttributes;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30092", ex);
            }
        }

        #endregion

        #region ICategorizedComponent Members

        /// <summary>
        /// Returns the name of this COM object.
        /// </summary>
        /// <returns>The name of this COM object.</returns>
        public string GetComponentDescription()
        {
            try
            {
                // Return the component description
                return _DEFAULT_OUTPUT_HANDLER_NAME;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26178", "Unable to get component description.", ex);
            }
        }

        #endregion

        #region IPersistStream Members

        /// <summary>
        /// Returns the class ID for this object.
        /// </summary>
        /// <param name="classID"></param>
        public void GetClassID(out Guid classID)
        {
            try
            {
                classID = this.GetType().GUID;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26180", "Unable to get class ID.", ex);
            }
        }

        /// <summary>
        /// Returns whether this object is dirty or not.
        /// </summary>
        /// <returns>Whether this object is dirty or not.</returns>
        public int IsDirty()
        {
            try
            {
                return HResult.FromBoolean(_dirty);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26182", "Unable to get dirty flag.", ex);
            }
        }

        /// <summary>
        /// Loads this object from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the database file name from the stream
                    _databaseFile = reader.ReadString();

                    if (reader.Version >= 2)
                    {
                        // Read the require mandatory setting
                        _requireMandatory = reader.ReadBoolean();
                    }

                    if (reader.Version >= 3)
                    {
                        // Read the filled requirement setting
                        _useFilledRequirement = reader.ReadBoolean();

                        // Read the use outstanding orders setting
                        _useOutstandingOrders = reader.ReadBoolean();

                        // Read the eliminate duplicate Test subattributes setting
                        _eliminateDuplicateTestSubAttributes = reader.ReadBoolean();
                    }
                }

                // False since a new object was just loaded
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26186", "Unable to load order mapper.", ex);
            }
        }

        /// <summary>
        /// Saves this object to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to save to.</param>
        /// <param name="clearDirty">If <see langword="true"/> will clear the dirty flag.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Serialize the settings
                    writer.Write(_databaseFile);
                    writer.Write(_requireMandatory);
                    writer.Write(_useFilledRequirement);
                    writer.Write(_useOutstandingOrders);
                    writer.Write(_eliminateDuplicateTestSubAttributes);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);

                    if (clearDirty)
                    {
                        _dirty = false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26189", "Unable to save order mapper.", ex);
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// <para>NOTE: Not implemented.</para>
        /// </summary>
        /// <param name="size">Will always be <see cref="HResult.NotImplemented"/> to indicate this 
        /// method is not implemented.</param>
        public void GetSizeMax(out long size)
        {
            try
            {
                size = HResult.NotImplemented;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26191", "Unable to get max size.", ex);
            }
        }

        #endregion

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to run the class as an <see cref="IOutputHandler"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was not successful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI26902", _DEFAULT_OUTPUT_HANDLER_NAME);

                // Display the configuration form
                using (LabDEOrderMapperConfigurationForm configureForm =
                    new LabDEOrderMapperConfigurationForm(_databaseFile, _requireMandatory,
                        _useFilledRequirement, _useOutstandingOrders, _eliminateDuplicateTestSubAttributes))
                {
                    // If the user clicked OK then set fields
                    if (configureForm.ShowDialog() == DialogResult.OK)
                    {
                        _databaseFile = configureForm.DatabaseFileName;
                        _requireMandatory = configureForm.RequireMandatoryTests;
                        _useFilledRequirement = configureForm.UseFilledRequirement;
                        _useOutstandingOrders = configureForm.UseOutstandingOrders;
                        _eliminateDuplicateTestSubAttributes = configureForm.EliminateDuplicateTestSubAttributes;

                        _dirty = true;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26193", "Unable to configure order mapper.", ex);
            }
        }

        #endregion

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the current object has been configured.
        /// Object is configured if the database file name has been set.
        /// </summary>
        /// <returns><see langword="true"/> if the database file name is not
        /// <see langword="null"/> or empty string and returns <see langword="false"/>
        /// otherwise.</returns>
        public bool IsConfigured()
        {
            try
            {
                return !string.IsNullOrEmpty(_databaseFile);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26195", "Unable to determine configuration status.", ex);
            }
        }

        #endregion

        #region ILicensedComponent Members

        /// <summary>
        /// Returns <see langword="true"/> if this component is licensed and
        /// <see langword="false"/> if it is not licensed.
        /// </summary>
        /// <returns>Whether this component is licensed or not.</returns>
        public bool IsLicensed()
        {
            try
            {
                return LicenseUtilities.IsLicensed(LicenseIdName.LabDECoreObjects);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26903",
                    "Unable to determine license status.", ex);
            }
        }

        #endregion ILicensedComponent Members

        #region Private Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID Output Handler" COM category.
        /// </summary>
        /// <param name="type">The <see langword="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID Output Handler" COM category.
        /// </summary>
        /// <param name="type">The <see langword="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Builds a collection of order codes mapped to <see cref="LabOrder"/>s.
        /// </summary>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use to retrieve the data.</param>
        /// <param name="useFilledRequirement">Whether to retrieve the filled requirement for orders.</param>
        /// <returns>A collection of order codes mapped to <see cref="LabOrder"/>s.</returns>
        static Dictionary<string, LabOrder> FillLabOrderCollection(OrderMappingDBCache dbCache,
            bool useFilledRequirement)
        {
            bool hasFilledRequirement = false;
            if (useFilledRequirement)
            {
                string infoQuery = "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS "
                    + "WHERE TABLE_NAME = 'LabOrder' AND COLUMN_NAME = 'FilledRequirement'";

                using (SqlCeCommand command = new SqlCeCommand(infoQuery, dbCache.DBConnection))
                {
                    hasFilledRequirement = command.ExecuteScalar() != null;
                }
            }

            Dictionary<string, LabOrder> orders =
                new Dictionary<string, LabOrder>(StringComparer.OrdinalIgnoreCase);
            
            string query = hasFilledRequirement
                ? "SELECT [Code], [Name], [EpicCode], [TieBreaker], "
                    + "[FilledRequirement] FROM [LabOrder] WHERE [EpicCode] IS NOT NULL"
                
                : "SELECT [Code], [Name], [EpicCode], [TieBreaker] "
                    + "FROM [LabOrder] WHERE [EpicCode] IS NOT NULL";

            using (SqlCeCommand command = new SqlCeCommand(query, dbCache.DBConnection))
            using (SqlCeDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string code = reader.GetString(0);
                    string name = reader.GetString(1);
                    string epicCode = reader.GetString(2);
                    string tieBreaker = reader.GetString(3);
                    int filledRequirement = hasFilledRequirement
                        ? (reader.IsDBNull(4) ? 0 : reader.GetInt32(4)) : 0;
                    orders.Add(code,
                        new LabOrder(code, name, epicCode, tieBreaker, dbCache, filledRequirement));
                }
            }
            return orders;
        }

        /// <summary>
        /// Performs the mapping from tests to order grouping.
        /// </summary>
        /// <param name="tests">A list of tests to group.</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use
        /// for querying.</param>
        /// <param name="requireMandatory">Whether or not mandatory tests are required
        /// in the second pass of the order mapping algorithm.</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled requirement.</param>
        /// <param name="useOutstandingOrders">Whether to prefer orders that match known,
        /// outstanding order code attributes.</param>
        static IEnumerable<IAttribute> MapOrders(List<IAttribute> tests, OrderMappingDBCache dbCache,
            bool requireMandatory, bool useFilledRequirement, bool useOutstandingOrders)
        {
            // If there are no tests then just return the input
            if (tests.Count == 0)
            {
                return tests;
            }

            // Get the source doc name from the first attribute
            string sourceDocName = "Unknown";
            sourceDocName = tests[0].Value.SourceDocName;

            // Build the list of all lab orders with their associated test collections.
            Dictionary<string, LabOrder> labOrders = FillLabOrderCollection(dbCache, useFilledRequirement);

            // Perform the first pass
            var firstPassResult = FirstPassGrouping(
                tests, dbCache, labOrders, sourceDocName, useFilledRequirement, useOutstandingOrders);

            // Perform the second pass
            var secondPassResult = GetFinalGrouping(firstPassResult, dbCache, labOrders,
                sourceDocName, requireMandatory, useFilledRequirement, useOutstandingOrders);

            // If first attempt was trying to limit to outstanding orders,
            // try unknown orders again without limiting
            if (useOutstandingOrders)
            {
                var known = secondPassResult.Item1;
                List<IAttribute> unknown;

                // Only use result of first attempt if at least one known order was mapped.
                if (known.Count > 0)
                {
                    unknown = secondPassResult.Item2;
                }
                else
                {
                    unknown = tests;
                }

                firstPassResult = FirstPassGrouping(
                        unknown, dbCache, labOrders, sourceDocName, useFilledRequirement, false);

                secondPassResult = GetFinalGrouping(firstPassResult, dbCache, labOrders,
                        sourceDocName, requireMandatory, useFilledRequirement, false);

                // Return the final groupings
                return known.Concat(secondPassResult.Item1.Concat(secondPassResult.Item2));
            }

            // Return the final groupings
            return secondPassResult.Item1.Concat(secondPassResult.Item2);
        }

        /// <summary>
        /// Performs the first pass grouping for each group of TEST attributes.
        /// </summary>
        /// <param name="tests">The list of TEST attributes to group.</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use for the grouping.</param>
        /// <param name="labOrders">A collection mapping order codes to
        /// <see cref="LabOrder"/>s.</param>
        /// <param name="sourceDocName">The sourcedoc name to use in the grouping.</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled requirement.</param>
        /// <param name="limitToOutstandingOrders">Whether to limit orders to be considered based
        /// on known, outstanding order codes.</param>
        /// <returns>
        /// A tuple representing the first order mapping pass as a list of mapped groups and a list
        /// of unmapped groups.
        /// </returns>
        static List<IAttribute> FirstPassGrouping(
            IEnumerable<IAttribute> tests,
            OrderMappingDBCache dbCache,
            Dictionary<string, LabOrder> labOrders,
            string sourceDocName,
            bool useFilledRequirement,
            bool limitToOutstandingOrders)
        {
            var firstPassMapping = new List<IAttribute>();

            foreach (IAttribute attribute in tests)
            {
                IUnknownVector attributes = attribute.SubAttributes;

                // Get a map of names to attributes from the attribute collection
                Dictionary<string, List<IAttribute>> nameToAttributes =
                    GetMapOfNamesToAttributes(attributes);

                // Get outstanding order codes.
                HashSet<string> outstandingOrderCodes = null;
                if (limitToOutstandingOrders)
                {
                    List<IAttribute> outstanding = null;
                    if (nameToAttributes.TryGetValue("OUTSTANDINGORDERCODE", out outstanding))
                    {
                        outstandingOrderCodes = BuildOutstandingOrdersSet(outstanding);
                    }
                    else
                    {
                        outstandingOrderCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                }

                // Build lists of all attributes that are in/not in the components category
                List<IAttribute> components = null;
                List<IAttribute> nonComponents = new List<IAttribute>();
                foreach (KeyValuePair<string, List<IAttribute>> pair in nameToAttributes)
                {
                    if (pair.Key.Equals("COMPONENT"))
                    {
                        components = pair.Value;
                    }
                    else if (pair.Key.Equals("NAME"))
                    {
                        // Do nothing as a new name will created later
                    }
                    else
                    {
                        nonComponents.AddRange(pair.Value);
                    }
                }

                List<IAttribute> mappedList = new List<IAttribute>();
                if (components != null && components.Count > 0)
                {
                    // All attributes are unmatched at this point
                    List<LabTest> unmatchedTests = BuildTestList(components);
                    while (unmatchedTests.Count > 0)
                    {
                        // Create a new IUnknown vectors for the matched tests
                        IUnknownVector vecMatched = new IUnknownVector();

                        // Add all the non-component attributes to the sub attributes
                        foreach (IAttribute attr in nonComponents)
                        {
                            vecMatched.PushBack(attr);
                        }

                        // Create the new order grouping attribute (default to UnknownOrder)
                        // and add it to the vector
                        AttributeClass orderGrouping = new AttributeClass();
                        orderGrouping.Name = "Name";
                        orderGrouping.Value.CreateNonSpatialString("UnknownOrder", sourceDocName);
                        vecMatched.PushBack(orderGrouping);


                        // Get the best match order for the remaining unmatched tests (require
                        // mandatory tests)
                        KeyValuePair<string, List<LabTest>> matchedTests = FindBestOrder(unmatchedTests,
                            labOrders, dbCache, true, useFilledRequirement, outstandingOrderCodes);

                        // Update the unmatched test list by removing the now matched tests
                        foreach (LabTest matches in matchedTests.Value)
                        {
                            unmatchedTests.Remove(matches);
                        }

                        string orderCode = matchedTests.Key;
                        if (!orderCode.Equals("UnknownOrder", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (LabTest matches in matchedTests.Value)
                            {
                                // Add the test to the vector
                                vecMatched.PushBack(matches.Attribute);
                            }

                            LabOrder labOrder;
                            if (!labOrders.TryGetValue(orderCode, out labOrder))
                            {
                                ExtractException ee = new ExtractException("ELI29072",
                                    "Order code could not be found in collection.");
                                ee.AddDebugData("OrderCode", orderCode, false);
                                throw ee;
                            }


                            // Set the order group name
                            orderGrouping.Value.ReplaceAndDowngradeToNonSpatial(
                                labOrder.OrderName);

                            // Add the epic code
                            AttributeClass epicCode = new AttributeClass();
                            epicCode.Name = "EpicCode";
                            epicCode.Value.CreateNonSpatialString(labOrder.EpicCode,
                                sourceDocName);
                            vecMatched.PushBack(epicCode);
                        }
                        else
                        {
                            // Add all of the "UnknownOrder" tests since there is no group for them
                            // NOTE: With the current algorithm, this should only be 1 test
                            foreach (LabTest matches in matchedTests.Value)
                            {
                                vecMatched.PushBack(matches.Attribute);
                            }
                        }

                        AttributeClass newAttribute = new AttributeClass();
                        newAttribute.Name = "Test";
                        newAttribute.Value.CreateNonSpatialString("N/A", sourceDocName);
                        newAttribute.SubAttributes = vecMatched;

                        // Add the attribute to the return list
                        mappedList.Add(newAttribute);
                    }
                }

                firstPassMapping.AddRange(mappedList);
            }

            return firstPassMapping;
        }

        /// <summary>
        /// Operates on the <paramref name="firstPassGrouping"/> collection (which should be the
        /// result of calling <see cref="FirstPassGrouping"/>, and tries to combine smaller
        /// order groups into larger order groups.  Returns a new collection of attributes
        /// that is the post-final grouping result.
        /// </summary>
        /// <param name="firstPassGrouping">A collection of attributes that have been grouped
        /// into orders.</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use for querying data.</param>
        /// <param name="labOrders">The collection of lab order codes to <see cref="LabOrder"/>
        /// objects.</param>
        /// <param name="sourceDocName">The source document name to be used when creating new
        /// spatial strings.</param>
        /// <param name="requireMandatory">Whether mandatory tests are required when creating
        /// the final groupings.</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled requirement.</param>
        /// <param name="limitToOutstandingOrders">Whether to limit orders to be considered based
        /// on known, outstanding order codes.</param>
        /// <returns>
        /// A tuple representing the second order mapping pass as a list of mapped groups and a list
        /// of unmapped groups.
        /// </returns>
        static Tuple<List<IAttribute>, List<IAttribute>> GetFinalGrouping(
            List<IAttribute> firstPassGrouping,
            OrderMappingDBCache dbCache,
            Dictionary<string, LabOrder> labOrders,
            string sourceDocName,
            bool requireMandatory,
            bool useFilledRequirement,
            bool limitToOutstandingOrders)
        {
            var bestGroups = new List<OrderGrouping>();
            var finalGrouping = Tuple.Create(new List<IAttribute>(), new List<IAttribute>());
            var firstPass = new List<OrderGrouping>();

            foreach (IAttribute attribute in firstPassGrouping)
            {
                firstPass.Add(new OrderGrouping(attribute, labOrders, limitToOutstandingOrders));
            }

            while (firstPass.Count > 0)
            {
                OrderGroupingPermutation first = new OrderGroupingPermutation(firstPass[0],
                    dbCache, sourceDocName);
                List<OrderGroupingPermutation> possibleGroupings = new List<OrderGroupingPermutation>();
                possibleGroupings.Add(first);

                for (int i = 0; i < possibleGroupings.Count; i++)
                {
                    OrderGroupingPermutation temp = possibleGroupings[i];
                    OrderGrouping lastOrder = temp.ContainedGroups[temp.ContainedGroups.Count - 1];

                    // Now compare each item in the first pass grouping to see if it can be grouped
                    // with another item in the grouping
                    for (int j = firstPass.IndexOf(lastOrder) + 1; j < firstPass.Count; j++)
                    {
                        OrderGroupingPermutation newPossibility =
                            temp.AttemptMerge(firstPass[j], labOrders, limitToOutstandingOrders);

                        if (newPossibility != null)
                        {
                            // [DataEntry:965]
                            // If too many possible combinations have been found, begin allowing
                            // further combinations only with the next test that can be grouped.
                            // This will prevent the total number of stored combinations from being
                            // greater than _COMBINATION_ALGORITHM_SAFETY_CUTOFF and prevent the
                            // possibility of out-of-memory issues.
                            if (possibleGroupings.Count == _COMBINATION_ALGORITHM_SAFETY_CUTOFF)
                            {
                                possibleGroupings[i] = newPossibility;
                                temp = newPossibility;
                            }
                            else
                            {
                                possibleGroupings.Add(newPossibility);
                            }
                        }
                    }
                }

                // Get the best grouping and remove all matched groupings from the first
                // pass grouping collection
                OrderGroupingPermutation bestGrouping =
                    GetBestGrouping(possibleGroupings, requireMandatory, dbCache) ?? first;
                bestGroups.Add(bestGrouping.CombinedGroup);
                foreach (OrderGrouping group in bestGrouping.ContainedGroups)
                {
                    firstPass.Remove(group);
                }
            }

            // Check for any order groupings that are "UnknownOrder"
            // and attempt to map them to an order [DE #833]
            MapSingleUnknownOrders(bestGroups, labOrders, dbCache, sourceDocName, requireMandatory,
                useFilledRequirement);

            // bestGroups should now contain all groupings that could be combined
            foreach (OrderGrouping orderGroup in bestGroups)
            {
                // Add the mapped group to the final grouping
                if (orderGroup.LabOrder != null)
                {
                    finalGrouping.Item1.Add(orderGroup.Attribute);

                    // Add subattributes for the official name and test code
                    orderGroup.AddOfficialNameAndTestCode(dbCache);
                }
                else
                {
                    finalGrouping.Item2.Add(orderGroup.Attribute);
                }
            }

            return finalGrouping;
        }

        /// <summary>
        /// Gets the best order grouping from a collection of potential order groupings.
        /// <para>Note:</para>
        /// Best is defined as the order grouping with the most tests.
        /// </summary>
        /// <param name="possibleGroupings">The collection of possible test groupings to compare.
        /// </param>
        /// <param name="requireMandatory">Whether mandatory tests are required when creating
        /// the final groupings.</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use for mapping.</param>
        /// <returns>The "Best" (see note in summary) order grouping from the list of possible
        /// groupings.</returns>
        static OrderGroupingPermutation GetBestGrouping(
            List<OrderGroupingPermutation> possibleGroupings, bool requireMandatory,
            OrderMappingDBCache dbCache)
        {
            int bestGroupCount = 0;
            string bestTieBreakerString = "";
            OrderGroupingPermutation bestGroup = null;
            foreach (OrderGroupingPermutation group in possibleGroupings)
            {
                if (requireMandatory && group.CombinedGroup.LabOrder != null && 
                    !group.CombinedGroup.ContainsAllMandatoryTests(dbCache))
                {
                    continue;
                }

                if (group.CombinedGroup.LabOrder != null &&
                    group.CombinedGroup.LabTests.Count < group.CombinedGroup.LabOrder.FilledRequirement)
                {
                    continue;
                }

                int groupCount = group.ContainedGroups.Count;
                string tieBreakerString = (group.CombinedGroup.LabOrder == null) ? "" :
                    group.CombinedGroup.LabOrder.TieBreakerString;

                // Best match if:
                // 1. Has more contained groups than other grouping OR
                if (bestGroup == null || groupCount > bestGroupCount)
                {
                    bestGroup = group;
                    bestGroupCount = groupCount;
                    bestTieBreakerString = tieBreakerString;
                }
                // 2. Has same number of contained groups AND a lesser TieBreakerString.
                else if (groupCount == bestGroupCount && 
                         string.Compare(
                            tieBreakerString, bestTieBreakerString, StringComparison.Ordinal) < 0)
                {
                    bestGroup = group;
                    bestGroupCount = groupCount;
                    bestTieBreakerString = tieBreakerString;
                }
            }

            return bestGroup;
        }

        /// <summary>
        /// Creates a new <see cref="OrderGrouping"/> from the sub attributes contained in the
        /// <paramref name="sourceGroup"/> <see cref="OrderGrouping"/>.
        /// </summary>
        /// <param name="orderCode">The order code to create the group for.
        /// <para><b>Note:</b></para>
        /// This must be a valid order code, do not call this method with "UnknownOrder".</param>
        /// <param name="labOrders">The map of order codes to <see cref="LabOrder"/>s.</param>
        /// <param name="sourceDocName">The source doc name for the new attributes.</param>
        /// <param name="sourceGroup">The <see cref="OrderGrouping"/> that contains the
        /// sub attributes that should be added to the new <see cref="OrderGrouping"/>.</param>
        /// <returns>A new <see cref="OrderGrouping"/> based on <paramref name="sourceGroup"/>
        /// and the new <paramref name="orderCode"/>.</returns>
        static OrderGrouping CreateNewOrderGrouping(string orderCode,
            Dictionary<string, LabOrder> labOrders, string sourceDocName,
            OrderGrouping sourceGroup)
        {
            LabOrder labOrder;
            if (!labOrders.TryGetValue(orderCode, out labOrder))
            {
                ExtractException ee = new ExtractException("ELI29074",
                    "Order code could not be found in collection.");
                ee.AddDebugData("OrderCode", orderCode, false);
                throw ee;
            }

            // Update the order name of the group1 mapping
            List<IAttribute> tempList = new List<IAttribute>();
            IAttribute temp = new AttributeClass();
            temp.Name = "Name";
            temp.Value.CreateNonSpatialString(labOrder.OrderName, sourceDocName);
            tempList.Add(temp);
            sourceGroup.NameToAttributes["NAME"] = tempList;

            // Update the epic code of the group mapping
            tempList = new List<IAttribute>();
            temp = new AttributeClass();
            temp.Name = "EpicCode";
            temp.Value.CreateNonSpatialString(labOrder.EpicCode, sourceDocName);
            tempList.Add(temp);
            sourceGroup.NameToAttributes["EPICCODE"] = tempList;

            // Build a new order for this group
            IAttribute attribute = new AttributeClass();
            attribute.Name = "Test";
            attribute.Value.CreateNonSpatialString("N/A", sourceDocName);
            attribute.SubAttributes = sourceGroup.GetAllAttributesAsIUnknownVector();

            // Create a new group with this object
            OrderGrouping newGroup = new OrderGrouping(attribute, null,
                sourceGroup.NameToAttributes, false, sourceGroup.OutstandingOrderCodes);
            newGroup.LabOrder = labOrder;
            return newGroup;
        }

        /// <summary>
        /// Attempts to map any single unknown orders into an actual order grouping.
        /// If a mapping is possible, the unknown order <see cref="OrderGrouping"/>
        /// will be replaced in <paramref name="orderGroups"/> with the a new
        /// <see cref="OrderGrouping"/> that has been mapped to a specific order.
        /// </summary>
        /// <param name="orderGroups">The collection of <see cref="OrderGrouping"/>s
        /// to check.</param>
        /// <param name="labOrders">The map of order codes to <see cref="LabOrder"/>s.</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use for querying data.</param>
        /// <param name="sourceDocName">The source doc name for the new attributes.</param>
        /// <param name="requireMandatory">If <see langword="true"/> then will only map to an
        /// order if all mandatory tests are present.</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled requirement.</param>
        static void MapSingleUnknownOrders(List<OrderGrouping> orderGroups,
            Dictionary<string, LabOrder> labOrders, OrderMappingDBCache dbCache,
            string sourceDocName, bool requireMandatory, bool useFilledRequirement)
        {
            for (int i = 0; i < orderGroups.Count; i++)
            {
                OrderGrouping group = orderGroups[i];
                if (group.LabOrder == null && group.LabTests.Count == 1)
                {
                    // Try to map this unknown order
                    KeyValuePair<string, List<LabTest>> bestMatch = FindBestOrder(
                        new List<LabTest>(group.LabTests), labOrders, dbCache, requireMandatory,
                        useFilledRequirement, group.OutstandingOrderCodes);

                    // Check for a new mapping
                    if (!bestMatch.Key.Equals("UnknownOrder", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create the new order grouping
                        OrderGrouping newGroup = CreateNewOrderGrouping(bestMatch.Key,
                            labOrders, sourceDocName, group);

                        // Update the list entry with the new order grouping
                        orderGroups[i] = newGroup;
                    }
                }
            }
        }

        /// <summary>
        /// Builds a list of pairs of test attributes to test name from the list of unmatched tests.
        /// </summary>
        /// <param name="tests">A list of test attributes.</param>
        /// <returns>A list of <see cref="LabTest"/>.</returns>
        internal static List<LabTest> BuildTestList(
            List<IAttribute> tests)
        {
            // Build a list of lab tests
            List<LabTest> returnList = new
                List<LabTest>(tests.Count);
            foreach (IAttribute attribute in tests)
            {
                returnList.Add(new LabTest(attribute));
            }

            // Return the list
            return returnList;
        }

        /// <summary>
        /// Builds the set of outstanding order codes from the list of OutstandingOrderCode attributes
        /// </summary>
        /// <param name="ordercodes">A list of attributes whose values are outstanding order codes.</param>
        /// <returns>A set of the outstanding order codes</returns>
        internal static HashSet<string> BuildOutstandingOrdersSet(List<IAttribute> ordercodes)
        {
            // Build a list of outstanding orders
            var outstandingOrderCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (IAttribute attribute in ordercodes)
            {
                outstandingOrderCodes.Add(attribute.Value.String);
            }

            return outstandingOrderCodes;
        }

        /// <summary>
        /// Find the best order match for the collection of unmatched tests.
        /// </summary>
        /// <param name="unmatchedTests">The list of unmatched tests to try to fit into
        /// a known order.</param>
        /// <param name="labOrders">A collection mapping order codes to
        /// <see cref="LabOrder"/>s</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use for data queries.</param>
        /// <param name="requireMandatory">If <see langword="true"/> then will only map to an
        /// order if all mandatory tests are present.  If <see langword="false"/> will map to
        /// the order with the most tests, even if all mandatory are not present.</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled requirement.</param>
        /// <param name="outstandingOrderCodes">Set of order codes to limit the orders to be considered.</param>
        /// <returns>A pair containing the order code and a list of matched tests.</returns>
        static KeyValuePair<string, List<LabTest>> FindBestOrder(List<LabTest> unmatchedTests,
            Dictionary<string, LabOrder> labOrders, OrderMappingDBCache dbCache,
            bool requireMandatory, bool useFilledRequirement, HashSet<string> outstandingOrderCodes)
        {
            // Variables to hold the best match seen thus far (will be modified as the
            // best matching algorithm does its work
            int bestMatchCount = 0;
            string bestTieBreakerString = "";
            string bestOrderId = "UnknownOrder";
            List<LabTest> bestMatchedTests = new List<LabTest>();

            // Check to see if the first test is part of any valid order
            ReadOnlyCollection<string> potentialOrderCodes =
                dbCache.GetPotentialOrderCodes(unmatchedTests[0].Name);

            // Loop through the potential orders attempting to match the order
            foreach (string orderCode in potentialOrderCodes)
            {
                // If outstandingOrderCodes is not null then limit to this set
                if (outstandingOrderCodes != null && !outstandingOrderCodes.Contains(orderCode))
                {
                    continue;
                }

                // Get the lab order from the order code
                LabOrder labOrder;
                if (!labOrders.TryGetValue(orderCode, out labOrder))
                {
                    ExtractException ee = new ExtractException("ELI29075",
                        "Order code was not found in the collection.");
                    ee.AddDebugData("Order Code", orderCode, false);
                    throw ee;
                }

                // Check if either mandatory not required or all the mandatory tests are present
                if (!requireMandatory || labOrder.ContainsAllMandatoryTests(unmatchedTests, dbCache))
                {
                    List<LabTest> matchedTests = labOrder.GetMatchingTests(unmatchedTests, dbCache);

                    if (!useFilledRequirement || matchedTests.Count >= labOrder.FilledRequirement)
                    {
                        // Best match if more tests matched OR
                        if (matchedTests.Count > bestMatchCount)
                        {
                            bestMatchCount = matchedTests.Count;
                            bestMatchedTests = matchedTests;
                            bestOrderId = labOrder.OrderCode;
                            bestTieBreakerString = labOrder.TieBreakerString;
                        }
                        // ...equal test count AND a lesser TieBreakerString.
                        else if (matchedTests.Count == bestMatchCount &&
                                 string.Compare(labOrder.TieBreakerString, bestTieBreakerString,
                                                StringComparison.Ordinal) < 0)
                        {
                            bestMatchCount = matchedTests.Count;
                            bestMatchedTests = matchedTests;
                            bestOrderId = labOrder.OrderCode;
                            bestTieBreakerString = labOrder.TieBreakerString;
                        }
                    }
                }
            }

            // No best order match was found, add this test to its own order
            if (bestMatchCount == 0)
            {
                bestMatchedTests.Add(unmatchedTests[0]);
            }
            else
            {
                // bestMatchedTests contains copies of the original source tests. Now that this set
                // is finalized, find the corresponding tests from the source parameter and use them
                // after assign the mapped test codes.
                // This is so the caller can compare the resulting tests to the source tests by
                // reference.
                for (int i = 0; i < bestMatchedTests.Count; i++)
                {
                    LabTest bestMatchedTest = bestMatchedTests[i];

                    LabTest sourceTest = unmatchedTests
                        .Where(test => test.Attribute == bestMatchedTest.Attribute)
                        .First();

                    sourceTest.TestCode = bestMatchedTest.TestCode;

                    bestMatchedTests[i] = sourceTest;
                }
            }

            return new KeyValuePair<string, List<LabTest>>(bestOrderId, bestMatchedTests);
        }

        /// <summary>
        /// Builds a map of names to <see cref="List{T}"/> of attributes.
        /// </summary>
        /// <param name="attributes">The vector of attributes to group.</param>
        /// <returns>The map of names to attributes.</returns>
        internal static Dictionary<string, List<IAttribute>> GetMapOfNamesToAttributes(
            IUnknownVector attributes)
        {
            // Get an AFUtility object and use it to produce a StrToObject map for
            // names to attributes
            AFUtility afUtility = new AFUtility();
            StrToObjectMap nameMap = afUtility.GetNameToAttributesMap(attributes);

            // Create a dictionary to hold the values
            Dictionary<string, List<IAttribute>> nameToAttributes =
                new Dictionary<string, List<IAttribute>>();

            // Loop through the StrToObject map and copy it into the dictionary.
            int size = nameMap.Size;
            for (int i = 0; i < size; i++)
            {
                string name;
                object vecAttributes;
                nameMap.GetKeyValue(i, out name, out vecAttributes);

                IUnknownVector vectorOfAttributes = vecAttributes as IUnknownVector;
                int vectorSize = vectorOfAttributes.Size();
                List<IAttribute> listAttributes = new List<IAttribute>(vectorSize);
                for (int j = 0; j < vectorSize; j++)
                {
                    listAttributes.Add((IAttribute)vectorOfAttributes.At(j));
                }

                nameToAttributes.Add(name.ToUpperInvariant(), listAttributes);
            }

            return nameToAttributes;
        }

        /// <summary>
        /// Gets the test name from the database based on the specified order code and test code.
        /// </summary>
        /// <param name="testCode">The test code to search on.</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use.</param>
        /// <returns>The test name for the specified order code and test code.</returns>
        internal static string GetTestNameFromTestCode(string testCode, OrderMappingDBCache dbCache)
        {
            // Escape the single quote
            string testCodeEscaped = testCode.Replace("'", "''");

            string query = "SELECT [OfficialName] FROM [LabTest] WHERE [TestCode] = '"
                + testCodeEscaped + "'";

            using (SqlCeCommand command = new SqlCeCommand(query, dbCache.DBConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetString(0);
                    }
                    else
                    {
                        ExtractException ee = new ExtractException("ELI26234",
                            "Could not find test name!");
                        ee.AddDebugData("TestCode", testCode, false);
                        throw ee;
                    }
                }
            }
        }

        /// <summary>
        /// Compares two Test hierarchies using the first Component sub-attribute of each hierarchy
        /// or else the top-level attribute there are no Components.
        /// </summary>
        /// <param name="sorter">The <see cref="ISortCompare"/> to perform the comparison.</param>
        /// <param name="x">The attribute representing the first Test hierarchy.</param>
        /// <param name="y">The attribute representing the second Test hierarchy.</param>
        /// <returns>1 if the second argument is greater than the first else -1</returns>
        static int CompareTestAttributes(ISortCompare sorter, IAttribute x, IAttribute y)
        {
            IAttribute componentX = x.SubAttributes.ToIEnumerable<IAttribute>()
                .FirstOrDefault(attribute =>
                    attribute.Name.Equals("Component", StringComparison.OrdinalIgnoreCase));

            IAttribute componentY = y.SubAttributes.ToIEnumerable<IAttribute>()
                .FirstOrDefault(attribute =>
                    attribute.Name.Equals("Component", StringComparison.OrdinalIgnoreCase));

            if (componentX == null && componentY == null)
            {
                return sorter.LessThan(x, y) ? -1 : 1;
            }
            else if (componentX == null)
            {
                return -1;
            }
            else if (componentY == null)
            {
                return 1;
            }
            else
            {
                return sorter.LessThan(componentX, componentY) ? -1 : 1;
            }
        }

        /// <summary>
        /// Removes duplicate attributes from a vector.
        /// </summary>
        /// <param name="attributes">The vector of attributes.</param>
        internal static void EliminateDuplicates(IUnknownVector attributes)
        {
            for (int n = 0; n < attributes.Size(); n++)
            {
                // Retrieve this attribute
                ComAttribute attribute = (ComAttribute)attributes.At(n);

                // Check remaining attributes in the vector
                // If any duplicates are found, discard them
                for (int i = n + 1; i < attributes.Size(); i++)
                {
                    // Retrieve the next attribute
                    ComAttribute nextAttribute = (ComAttribute)attributes.At(i);

                    // Check for duplicate
                    if (nextAttribute.IsNonSpatialMatch(attribute))
                    {
                        // Discard this attribute and decrement index
                        attributes.Remove(i--);
                    }
                }
            }
        }

        #endregion Private Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="LabDEOrderMapper"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="LabDEOrderMapper"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="LabDEOrderMapper"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _localDatabaseCopyManager.Dispose();
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
