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
    /// An interface for the <see cref="LabDEOrderMapper"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("BAAB1CA3-A57D-4EEB-82B4-ECC17F41B7E5")]
    [CLSCompliant(false)]
    public interface ILabDEOrderMapper : IOutputHandler, ICategorizedComponent, IConfigurableObject,
        ICopyableObject, ILicensedComponent, IPersistStream, IMustBeConfiguredObject, IIdentifiableObject
    {
        /// <summary>
        /// Gets the database file name.
        /// </summary>
        /// <returns>The database file name.</returns>
        string DatabaseFileName { get; set; }

        /// <summary>
        /// Gets whether to remove any duplicate Test subattributes after the mapping is finished.
        /// </summary>
        bool EliminateDuplicateTestSubAttributes { get; set; }

        /// <summary>
        /// Gets/sets whether mandatory tests are required during the second pass of the order
        /// mapping algorithm.
        /// </summary>
        bool RequireMandatoryTests { get; set; }

        /// <summary>
        /// Gets whether filled/mandatory requirements can be disregarded in order to increase the
        /// number of mapped result components
        /// </summary>
        bool RequirementsAreOptional { get; set; }

        /// <summary>
        /// Gets whether to require that orders meet their filled requirement
        /// </summary>
        bool UseFilledRequirement { get; set; }

        /// <summary>
        /// Gets whether to prefer orders with codes matching known, outstanding order codes
        /// </summary>
        bool UseOutstandingOrders { get; set; }

        /// <summary>
        /// Whether to skip second pass of the order mapping algorithm
        /// </summary>
        bool SkipSecondPass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether add an ESNames attribute to mapped components
        /// </summary>
        bool AddESNamesAttribute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add an ESTestCodes attribute to components
        /// to show all mappings that were available
        /// </summary>
        bool AddESTestCodesAttribute { get; set; }

        /// <summary>
        /// Gets or sets whether set type of components to Fuzzy if they were mapped using a fuzzy
        /// regex pattern
        /// </summary>
        bool SetFuzzyType { get; set; }
    }

    /// <summary>
    /// Handles mapping orders from the rules output into Lab orders and their
    /// associated order codes based on a database file.
    /// </summary>
    [Guid("ABC13C14-B6C6-4679-A69B-5083D3B4B60C")]
    [ProgId("Extract.DataEntry.LabDE.LabDEOrderMapper")]
    [ComVisible(true)]
    public class LabDEOrderMapper : IdentifiableObject, ILabDEOrderMapper, IDisposable
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
        /// Version 4: Added requirements are optional setting.
        /// Changed default values of RequireMandatoryTests and EliminateDuplicateTestSubattributes to true
        /// Version 5: Added SkipSecondPass, AddESNamesAttribute, AddESTestCodesAttribute and SetFuzzyType options
        /// </summary>
        static readonly int _CURRENT_VERSION = 5;

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
            public List<LabTest> ContainedTests = new List<LabTest>();

            /// <summary>
            /// Initializes a new instance of the <see cref="OrderGroupingPermutation"/>
            /// class.
            /// </summary>
            /// <param name="originalGroup">The original order group for this permutation (before
            /// any merging with other orders has taken place).</param>
            public OrderGroupingPermutation(OrderGrouping originalGroup)
            {
                // The initial combined group contains only the original group
                CombinedGroup = originalGroup;
                ContainedGroups.Add(originalGroup);
                ContainedTests = new List<LabTest>(originalGroup.LabTests);
            }

            /// <summary>
            /// Attempts to merge the specified <see cref="OrderGrouping"/> with the
            /// currently contained order groupings.
            /// </summary>
            /// <param name="newGroup">The <see cref="OrderGrouping"/> to
            /// merge with the current grouping.</param>
            /// <param name="labOrder">The <see cref="LabOrder"/> to be mapped to</param>
            /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> used for mapping</param>
            /// <param name="compatibleDates">Whether the collection and result dates/times
            /// of this instance were compatible with <paramref name="newGroup"/></param>
            /// <returns>A new merged <see cref="OrderGroupingPermutation"/>
            /// or <see langword="null"/> if no merging can take place.</returns>
            public OrderGroupingPermutation AttemptMerge(OrderGrouping newGroup,
                LabOrder labOrder, OrderMappingDBCache dbCache, out bool compatibleDates)
            {
                if (   !OrderGrouping.CollectionDatesEqual(CombinedGroup, newGroup)
                    || !OrderGrouping.ResultDatesEqual(CombinedGroup, newGroup))
                {
                    // Collection dates don't match, cannot group these orders
                    compatibleDates = false;
                    return null;
                }
                compatibleDates = true;

                // Check if this group is already contained by the current group
                if (ContainedGroups.Contains(newGroup))
                {
                    // This group is already contained, just return null
                    return null;
                }

                // Map all the tests to this order. Don't require mandatory yet
                List<LabTest> combinedTests = ContainedTests.Concat(newGroup.LabTests).ToList();
                List<LabTest> matchedTests = labOrder.GetMatchingTests(combinedTests, dbCache,
                    true, false);

                // In order to group into a new combined order, the resulting order must contain
                // all of the tests from the list.
                if (combinedTests.Count != matchedTests.Count)
                {
                    return null;
                }

                var combinedGroup = new OrderGrouping(labOrder, matchedTests, CombinedGroup, newGroup);

                // Create the new grouping permutation
                var newGroupingPermutation = new OrderGroupingPermutation(combinedGroup);
                newGroupingPermutation.ContainedTests = matchedTests;
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
        bool _useFilledRequirement;

        /// <summary>
        /// Whether to prefer orders that match known, outstanding order code attributes.
        /// </summary>
        bool _useOutstandingOrders;

        /// <summary>
        /// Whether filled/mandatory requirements can be disregarded in order to increase the number of mapped result components.
        /// </summary>
        bool _requirementsAreOptional;

        /// <summary>
        /// Whether to remove any duplicate Test subattributes after the mapping is finished.
        /// </summary>
        bool _eliminateDuplicateTestSubAttributes = true;

        /// <summary>
        /// Whether to skip second pass of the order mapping algorithm
        /// </summary>
        bool _skipSecondPass = false;

        /// <summary>
        /// Gets or sets whether add an ESNames attribute to mapped components
        /// </summary>
        private bool _addESNamesAttribute;

        /// <summary>
        /// Gets or sets whether to add an ESTestCodes attribute to components
        /// to show all mappings that were available
        /// </summary>
        private bool _addESTestCodesAttribute;

        /// <summary>
        /// Gets or sets whether set type of components to Fuzzy if they were mapped using a fuzzy
        /// regex pattern
        /// </summary>
        private bool _setFuzzyType;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </summary>
        public LabDEOrderMapper()
            : this(databaseFile: null, requireMandatory: true,
                useFilledRequirement: true, useOutstandingOrders: false,
                requirementsAreOptional: true, eliminateDuplicateTestSubAttributes: true,
                skipSecondPass: false, addESNamesAttribute: true, addESTestCodesAttribute: false,
                setFuzzyType: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper" /> class.
        /// </summary>
        /// <param name="databaseFile">The name of the database file to attach to.</param>
        /// <param name="requireMandatory">Whether or not mandatory tests are required
        /// in the second pass of the order mapping algorithm.</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled requirement.</param>
        /// <param name="useOutstandingOrders">Whether to prefer orders that match known,
        /// outstanding order code attributes.</param>
        /// <param name="requirementsAreOptional">Whether filled/mandatory requirements can be disregarded
        /// in order to increase the number of mapped result components</param>
        /// <param name="eliminateDuplicateTestSubAttributes">Whether to remove any duplicate Test
        /// subattributes after the mapping is finished. (Prevents lots of extra ResultDate attributes, e.g.)</param>
        /// <param name="skipSecondPass">Whether to skip the second pass of the order mapping algorithm</param>
        /// <param name="addESNamesAttribute">Whether add an ESNames attribute to mapped components</param>
        /// <param name="addESTestCodesAttribute">Whether to add an ESTestCodes attribute to components to show
        /// all mappings that were available</param>
        /// <param name="setFuzzyType">Whether set type of components to Fuzzy if they were mapped using a fuzzy
        /// regex pattern</param>
        public LabDEOrderMapper(
            string databaseFile,
            bool requireMandatory,
            bool useFilledRequirement,
            bool useOutstandingOrders,
            bool requirementsAreOptional,
            bool eliminateDuplicateTestSubAttributes,
            bool skipSecondPass,
            bool addESNamesAttribute,
            bool addESTestCodesAttribute,
            bool setFuzzyType)
        {
            try
            {
                _databaseFile = databaseFile;
                _localDatabaseCopyManager = new TemporaryFileCopyManager();
                _requireMandatory = requireMandatory;
                _useFilledRequirement = useFilledRequirement;
                _useOutstandingOrders = useOutstandingOrders;
                _requirementsAreOptional = requirementsAreOptional;
                _eliminateDuplicateTestSubAttributes = eliminateDuplicateTestSubAttributes;
                SkipSecondPass = skipSecondPass;
                AddESNamesAttribute = addESNamesAttribute;
                AddESTestCodesAttribute = addESTestCodesAttribute;
                SetFuzzyType = setFuzzyType;
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
        /// Gets whether filled/mandatory requirements can be disregarded in order to increase the
        /// number of mapped result components
        /// </summary>
        public bool RequirementsAreOptional
        {
            get
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI39141", _DEFAULT_OUTPUT_HANDLER_NAME);

                return _requirementsAreOptional;
            }
            set
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI39142", _DEFAULT_OUTPUT_HANDLER_NAME);

                _requirementsAreOptional = value;
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

        /// <summary>
        /// Whether to skip second pass of the order mapping algorithm
        /// </summary>
        public bool SkipSecondPass
        {
            get
            {
                return _skipSecondPass;
            }
            set
            {
                if (value != _skipSecondPass)
                {
                    _skipSecondPass = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether add an ESNames attribute to mapped components
        /// </summary>
        public bool AddESNamesAttribute
        {
            get
            {
                return _addESNamesAttribute;
            }
            set
            {
                if (value != _addESNamesAttribute)
                {
                    _addESNamesAttribute = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to add an ESTestCodes attribute to components to show all mappings
        /// that were available
        /// </summary>
        public bool AddESTestCodesAttribute
        {
            get
            {
                return _addESTestCodesAttribute;
            }
            set
            {
                if (value != _addESTestCodesAttribute)
                {
                    _addESTestCodesAttribute = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether set type of components to Fuzzy if they were mapped using a fuzzy
        /// regex pattern
        /// </summary>
        public bool SetFuzzyType
        {
            get
            {
                return _setFuzzyType;
            }
            set
            {
                if (value != _setFuzzyType)
                {
                    _setFuzzyType = value;
                    _dirty = true;
                }
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
                    bool requireMandatory = _requireMandatory ? dbCache.AreMandatoryRequirementsDefined() : false;
                    bool useFilledRequirement = _useFilledRequirement ? dbCache.AreFilledRequirementsDefined() : false;

                    // Perform order mapping on the list of test attributes
                    IEnumerable<IAttribute> mappedAttributes = MapOrders(testAttributes, dbCache,
                        requireMandatory, useFilledRequirement);

                    // Create an attribute sorter for sorting sub attributes
                    ISortCompare attributeSorter =
                                (ISortCompare)new SpatiallyCompareAttributesClass();

                    // Sort the subattributes spatially
                    foreach (IAttribute newAttribute in mappedAttributes)
                    {
                        // Remove duplicate subattributes if specified
                        if (_eliminateDuplicateTestSubAttributes)
                        {
                            EliminateDuplicates(newAttribute.SubAttributes);
                        }
                        // Sort the sub attributes spatially
                        newAttribute.SubAttributes.Sort(attributeSorter);
                    }
                    
                    // Sort the orders spatially, using the first component subattribute
                    // and add to the output collection
                    foreach (IAttribute newAttribute in mappedAttributes
                        .OrderBy(a => a, new CompareTestAttributes(attributeSorter)))
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
                    _useFilledRequirement, _useOutstandingOrders, _requirementsAreOptional,
                    _eliminateDuplicateTestSubAttributes, _skipSecondPass, _addESNamesAttribute,
                    _addESTestCodesAttribute, _setFuzzyType
                    );

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
                _requirementsAreOptional = orderMapper._requirementsAreOptional;
                _eliminateDuplicateTestSubAttributes = orderMapper.EliminateDuplicateTestSubAttributes;
                SkipSecondPass = orderMapper.SkipSecondPass;
                AddESNamesAttribute = orderMapper.AddESNamesAttribute;
                AddESTestCodesAttribute = orderMapper.AddESTestCodesAttribute;
                SetFuzzyType = orderMapper.SetFuzzyType;
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
                    else
                    {
                        // Use old defaults for the options that have new defaults in version 4
                        _requireMandatory = false;
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
                    else
                    {
                        // Use old defaults for the options that have new defaults in version 4
                        _eliminateDuplicateTestSubAttributes = false;
                    }

                    if (reader.Version >= 4)
                    {
                        // Read the requirements-are-optional setting
                        _requirementsAreOptional = reader.ReadBoolean();
                    }
                    else
                    {
                        _requirementsAreOptional = false;
                    }

                    if (reader.Version >= 5)
                    {
                        SkipSecondPass = reader.ReadBoolean();
                        AddESNamesAttribute = reader.ReadBoolean();
                        AddESTestCodesAttribute = reader.ReadBoolean();
                        SetFuzzyType = reader.ReadBoolean();
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
                    writer.Write(_requirementsAreOptional);
                    writer.Write(SkipSecondPass);
                    writer.Write(AddESNamesAttribute);
                    writer.Write(AddESTestCodesAttribute);
                    writer.Write(SetFuzzyType);

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
                        _useFilledRequirement, _useOutstandingOrders,
                        _requirementsAreOptional, _eliminateDuplicateTestSubAttributes,
                        _skipSecondPass, _addESNamesAttribute, _addESTestCodesAttribute,
                        _setFuzzyType))
                {
                    // If the user clicked OK then set fields
                    if (configureForm.ShowDialog() == DialogResult.OK)
                    {
                        _databaseFile = configureForm.DatabaseFileName;
                        _requireMandatory = configureForm.RequireMandatoryTests;
                        _useFilledRequirement = configureForm.UseFilledRequirement;
                        _useOutstandingOrders = configureForm.UseOutstandingOrders;
                        _requirementsAreOptional = configureForm.RequirementsAreOptional;
                        _eliminateDuplicateTestSubAttributes = configureForm.EliminateDuplicateTestSubAttributes;
                        SkipSecondPass = configureForm.SkipSecondPass;
                        AddESNamesAttribute = configureForm.AddESNamesAttribute;
                        AddESTestCodesAttribute = configureForm.AddESTestCodesAttribute;
                        SetFuzzyType = configureForm.SetFuzzyType;

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
        /// <returns>A collection of order codes mapped to <see cref="LabOrder"/>s.</returns>
        static Dictionary<string, LabOrder> FillLabOrderCollection(OrderMappingDBCache dbCache)
        {
            Dictionary<string, LabOrder> orders =
                new Dictionary<string, LabOrder>(StringComparer.OrdinalIgnoreCase);

            string query = "SELECT [Code], [Name], [TieBreaker], [FilledRequirement]"
                    + " FROM [LabOrder] WHERE [Code] IS NOT NULL";

            using (SqlCeCommand command = new SqlCeCommand(query, dbCache.DBConnection))
            using (SqlCeDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string code = reader.GetString(0);
                    string name = reader.GetString(1);
                    string tieBreaker = reader[2] as string;
                    int filledRequirement = reader[3] as int? ?? 0;
                    orders.Add(code,
                        new LabOrder(code, name, tieBreaker, dbCache, filledRequirement));
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
        IEnumerable<IAttribute> MapOrders(List<IAttribute> tests, OrderMappingDBCache dbCache,
            bool requireMandatory, bool useFilledRequirement)
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
            Dictionary<string, LabOrder> labOrders = FillLabOrderCollection(dbCache);

            // Perform the first pass
            var firstPassResult = FirstPassGrouping(
                tests, dbCache, labOrders, sourceDocName, useFilledRequirement, _useOutstandingOrders, !_skipSecondPass);

            if (_skipSecondPass)
            {
                return firstPassResult.Select(g =>
                    {
                        g.AddOfficialNameAndTestCode(dbCache, AddESNamesAttribute, AddESTestCodesAttribute, SetFuzzyType);
                        return g.Attribute;
                    }).ToList();
            }

            // Limit to outstanding orders if useOutstandingOrders and there actually are any outstanding orders
            bool limitToOutstandingOrders = _useOutstandingOrders ?
                firstPassResult.Any(g => g.OutstandingOrderCodes.Any()) : false;

            // Perform the second pass
            var secondPassResult = GetFinalGrouping(firstPassResult, dbCache, labOrders,
               requireMandatory, useFilledRequirement, _requirementsAreOptional, limitToOutstandingOrders);

            // If first attempt was trying to limit to outstanding orders,
            // try unknown orders again without limiting
            if (limitToOutstandingOrders)
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

                if (unknown.Count > 0)
                {
                    firstPassResult = FirstPassGrouping(
                            unknown, dbCache, labOrders, sourceDocName, useFilledRequirement, false);

                    secondPassResult = GetFinalGrouping(firstPassResult, dbCache, labOrders,
                            requireMandatory, useFilledRequirement, _requirementsAreOptional, false);

                    // Return the final groupings
                    return known.Concat(secondPassResult.Item1.Concat(secondPassResult.Item2));
                }
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
        /// <param name="separateUnknownTests">Whether to separate unknown tests into one Unknown
        /// Order per test or to leave in groups (zero or one per Test hierarchy).</param>
        /// on known, outstanding order codes.</param>
        /// <returns>
        /// A tuple representing the first order mapping pass as a list of mapped groups and a list
        /// of unmapped groups.
        /// </returns>
        static List<OrderGrouping> FirstPassGrouping(
            IEnumerable<IAttribute> tests,
            OrderMappingDBCache dbCache,
            Dictionary<string, LabOrder> labOrders,
            string sourceDocName,
            bool useFilledRequirement,
            bool limitToOutstandingOrders,
            bool separateUnknownTests = true)
        {
            var firstPassMapping = new List<OrderGrouping>();

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

                if (components != null && components.Count > 0)
                {
                    List<LabTest> unknownOrderTests = null;

                    // All attributes are unmatched at this point
                    List<LabTest> unmatchedTests = BuildTestList(components);
                    while (unmatchedTests.Count > 0)
                    {
                        // Get the best match order for the remaining unmatched tests (require
                        // mandatory tests)
                        KeyValuePair<string, List<LabTest>> matchedOrder = FindBestOrder(unmatchedTests,
                            labOrders, dbCache, true, useFilledRequirement, outstandingOrderCodes, false, false);

                        LabOrder labOrder;
                        if (!labOrders.TryGetValue(matchedOrder.Key, out labOrder))
                        {
                            if (unknownOrderTests == null)
                            {
                                unknownOrderTests = matchedOrder.Value;
                            }
                            else
                            {
                                unknownOrderTests.AddRange(matchedOrder.Value);
                            }
                        }
                        else
                        {
                            var orderGroup = new OrderGrouping(labOrder, matchedOrder.Value,
                                limitToOutstandingOrders, sourceDocName, nonComponents);

                            firstPassMapping.Add(orderGroup);
                        }

                        // Update the unmatched test list by removing the now matched tests
                        var matchedAttributes = new HashSet<IAttribute>(matchedOrder.Value.Select(test => test.Attribute));
                        unmatchedTests = unmatchedTests.Where(test => !matchedAttributes.Contains(test.Attribute)).ToList();
                    }
                    if (unknownOrderTests != null)
                    {
                        if (separateUnknownTests)
                        {
                            foreach (var test in unknownOrderTests)
                            {
                                var testList = new List<LabTest>(1) {test};
                                var orderGroup = new OrderGrouping(null, testList,
                                    limitToOutstandingOrders, sourceDocName, nonComponents);
                                firstPassMapping.Add(orderGroup);
                            }
                        }
                        else
                        {
                            var orderGroup = new OrderGrouping(null, unknownOrderTests,
                                limitToOutstandingOrders, sourceDocName, nonComponents);
                            firstPassMapping.Add(orderGroup);
                        }
                    }
                }
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
        /// <param name="requireMandatory">Whether mandatory tests are required when creating
        /// the final groupings.</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled requirement.</param>
        /// <param name="requirementsAreOptional">Whether filled/mandatory requirements can be disregarded
        /// in order to increase the number of mapped tests.</param>
        /// <param name="limitToOutstandingOrders">Whether to limit orders to be considered based
        /// on known, outstanding order codes.</param>
        /// <returns>
        /// A tuple representing the second order mapping pass as a list of mapped groups and a list
        /// of unmapped groups.
        /// </returns>
        Tuple<List<IAttribute>, List<IAttribute>> GetFinalGrouping(
            IEnumerable<OrderGrouping> firstPassGrouping,
            OrderMappingDBCache dbCache,
            Dictionary<string, LabOrder> labOrders,
            bool requireMandatory,
            bool useFilledRequirement,
            bool requirementsAreOptional,
            bool limitToOutstandingOrders)
        {
            // Combine all outstanding order codes
            HashSet<string> outstandingOrderCodes = null;
            if (limitToOutstandingOrders)
            {
                outstandingOrderCodes = new HashSet<string>();
                foreach(var group in firstPassGrouping)
                {
                    outstandingOrderCodes.UnionWith(group.OutstandingOrderCodes);
                }
            }

            // Try once allowing only four consecutive groups to be skipped when matching an order
            IEnumerable<OrderGrouping> bestGroups = MergeGroups(firstPassGrouping, labOrders,
                dbCache, requireMandatory, useFilledRequirement, outstandingOrderCodes, false, 4);

            // Try again, allowing more groups to be skipped this time
            bestGroups = MergeGroups(bestGroups, labOrders,
                dbCache, requireMandatory, useFilledRequirement, outstandingOrderCodes, false, 50);

            // Check for any order groupings that are "UnknownOrder"
            // and attempt to map them to an order [DE #833]
            bestGroups = MapSingleUnknownOrders(bestGroups, labOrders, dbCache, requireMandatory,
                useFilledRequirement);

            if (requirementsAreOptional)
            {
                // If mandatory requirements have been enforced thus far, attempt to combine unknown
                // order groups with other groups, ignoring mandatory requirements
                if (requireMandatory)
                {
                    bestGroups = MergeGroups(bestGroups, labOrders, dbCache, false,
                        useFilledRequirement, outstandingOrderCodes, true, 4);
                }

                // If filled requirements have been enforced thus far, attempt to combine unknown
                // order groups with other groups, ignoring filled requirements
                if (useFilledRequirement)
                {
                    bestGroups = MergeGroups(bestGroups, labOrders, dbCache, requireMandatory,
                        false, outstandingOrderCodes, true, 4);
                }

                // If mandatory AND filled requirements have been enforced thus far, attempt to combine unknown
                // order groups with other groups, ignoring both requirements
                if (requireMandatory && useFilledRequirement)
                {
                    bestGroups = MergeGroups(bestGroups, labOrders, dbCache, false,
                        false, outstandingOrderCodes, true, 4);
                }
            }

            // BestGroups should now contain all groupings that could be combined
            var finalGrouping = Tuple.Create(new List<IAttribute>(), new List<IAttribute>());
            foreach (OrderGrouping orderGroup in bestGroups)
            {
                // Add subattributes for the official name and test code
                orderGroup.AddOfficialNameAndTestCode(dbCache, AddESNamesAttribute, AddESTestCodesAttribute, SetFuzzyType);

                // Add the mapped group to the final grouping
                if (orderGroup.LabOrder != null)
                {
                    finalGrouping.Item1.Add(orderGroup.Attribute);
                }
                else
                {
                    finalGrouping.Item2.Add(orderGroup.Attribute);
                }
            }

            return finalGrouping;
        }

        /// <summary>
        /// Attempts to merge <see cref="OrderGrouping"/>s together to create larger groups
        /// </summary>
        /// <param name="orderGroups">The <see cref="OrderGrouping"/>s to merge</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use for querying data.</param>
        /// <param name="labOrders">The collection of lab order codes to <see cref="LabOrder"/>
        /// objects.</param>
        /// <param name="requireMandatory">Whether mandatory tests are required</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled
        /// requirement</param>
        /// <param name="outstandingOrderCodes">Set of order codes to limit the orders to be considered.</param>
        /// <param name="mergeUnknownOrders">Whether only mergers containing at least one unknown
        /// order will be performed</param>
        /// <param name="consecutiveGroupsAllowedToBeSkipped">The number of <see cref="OrderGrouping"/>s that
        /// can be skipped in a row before no further attempts for a particular order will be tried.</param>
        /// <returns>List of merged <see cref="OrderGrouping"/>s</returns>
        private static IEnumerable<OrderGrouping> MergeGroups(
            IEnumerable<OrderGrouping> orderGroups,
            Dictionary<string, LabOrder> labOrders,
            OrderMappingDBCache dbCache,
            bool requireMandatory,
            bool useFilledRequirement,
            HashSet<string> outstandingOrderCodes,
            bool mergeUnknownOrders,
            int consecutiveGroupsAllowedToBeSkipped)
        {

            // If merging unknown orders but there are no unknown orders, just return the input
            if (mergeUnknownOrders && !orderGroups.Where(g => g.LabOrder == null).Any())
            {
                return orderGroups;
            }

            var preMergeGroups = new LinkedList<OrderGrouping>(orderGroups);
            var postMergeGroups = new LinkedList<OrderGrouping>();
            while (preMergeGroups.Count > 0)
            {
                // Get the best groups and remove all matched groups from the pending collection
                OrderGroupingPermutation bestGrouping = GetBestGrouping(preMergeGroups, labOrders,
                    dbCache, requireMandatory, useFilledRequirement, outstandingOrderCodes,
                    consecutiveGroupsAllowedToBeSkipped);

                // If no merge could take place or an unknown order is required to be, but wasn't,
                // merged, then just move the first group from pre to post-merged collection.
                if (bestGrouping == null || mergeUnknownOrders
                    && !bestGrouping.ContainedGroups.Where(g => g.LabOrder == null).Any())
                {
                    postMergeGroups.AddLast(preMergeGroups.First.Value);
                    preMergeGroups.RemoveFirst();
                }
                else
                {
                    postMergeGroups.AddLast(bestGrouping.CombinedGroup);
                    foreach (OrderGrouping group in bestGrouping.ContainedGroups)
                    {
                        preMergeGroups.Remove(group);
                    }
                }
            }

            return postMergeGroups;
        }

        /// <summary>
        /// Find the best order match for the collection of <see cref="OrderGrouping"/>s
        /// </summary>
        /// <param name="unmatchedGroups">The list of <see cref="OrderGrouping"/>s to try to fit
        /// into the best order possible</param>
        /// <param name="labOrders">A collection mapping order codes to <see cref="LabOrder"/>s</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use for mapping</param>
        /// <param name="requireMandatory">If <see langword="true"/> then will only map to an
        /// order if all mandatory tests are present.  If <see langword="false"/> will map to
        /// the order with the most tests, even if all mandatory are not present.</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled requirement.</param>
        /// <param name="outstandingOrderCodes">Set of order codes to limit the orders to be considered.</param>
        /// <param name="consecutiveGroupsAllowedToBeSkipped">The number of <see cref="OrderGrouping"/>s that
        /// can be skipped in a row before no further attempts for a particular order will be tried.</param>
        /// <returns>An <see cref="OrderGroupingPermutation"/> describing the merged groups</returns>
        static OrderGroupingPermutation GetBestGrouping(IEnumerable<OrderGrouping> unmatchedGroups,
            Dictionary<string, LabOrder> labOrders, OrderMappingDBCache dbCache,
            bool requireMandatory, bool useFilledRequirement, HashSet<string> outstandingOrderCodes,
            int consecutiveGroupsAllowedToBeSkipped)
        {
            // Variables to hold the best match seen thus far (will be modified as the
            // best matching algorithm does its work
            int bestGroupCount = 0;
            int bestMatchScoreSum = 0;
            string bestTieBreakerString = "";
            OrderGroupingPermutation bestGroup = null;

            // Get possible orders for the first group
            var potentialOrderCodes = GetPotentialOrderCodes(unmatchedGroups.First().LabTests, dbCache)
                .ToList();

            // Only the first group will be used to generate matchScoreSums to avoid bias towards
            // permutations that include more tests with defined MatchScoringQueries
            var attributesToScore = new HashSet<IAttribute>(
                unmatchedGroups.First().LabTests.Select(labTest => labTest.Attribute));

            // Loop through the potential orders attempting to match the order
            foreach (string orderCode in potentialOrderCodes)
            {
                // If outstandingOrderCodes is not null or empty then limit to this set
                if (outstandingOrderCodes != null && outstandingOrderCodes.Count > 0
                    && !outstandingOrderCodes.Contains(orderCode))
                {
                    continue;
                }

                // Get the lab order from the order code
                LabOrder labOrder;
                if (!labOrders.TryGetValue(orderCode, out labOrder))
                {
                    ExtractException ee = new ExtractException("ELI40319",
                        "Order code was not found in the collection.");
                    ee.AddDebugData("Order Code", orderCode, false);
                    throw ee;
                }

                var currentPermutation = new OrderGroupingPermutation(unmatchedGroups.First());
                int skipped = 0;
                foreach (var nextGroup in unmatchedGroups.Skip(1))
                {
                    bool compatibleDates;
                    var permutation = currentPermutation.AttemptMerge
                        (nextGroup, labOrder, dbCache, out compatibleDates);

                    // Skip group if no merge could happen
                    if (permutation == null)
                    {
                        // Count as skipped only if the dates were compatible
                        if (compatibleDates)
                        {
                            skipped++;
                            if (skipped > consecutiveGroupsAllowedToBeSkipped)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        currentPermutation = permutation;
                        skipped = 0;
                    }
                }

                // The group so far
                var group = currentPermutation.CombinedGroup;

                // If no merging has happened, attempt to map the group to the order being considered
                if (group.LabOrder != null && group.LabOrder.OrderCode != labOrder.OrderCode)
                {
                    List<LabTest> matchedTests = labOrder.GetMatchingTests(group.LabTests, dbCache,
                        true, requireMandatory);

                    // In order to accept this mapping the order must contain all of the tests from
                    // the original group.
                    if (matchedTests == null || matchedTests.Count != group.LabTests.Count)
                    {
                        continue;
                    }

                    // Make a new group with this order
                    var newGroup = new OrderGrouping(labOrder, matchedTests, group);

                    // Create a new grouping permutation
                    var newPermutation = new OrderGroupingPermutation(newGroup);
                    newPermutation.ContainedGroups = new List<OrderGrouping>(currentPermutation.ContainedGroups);
                    currentPermutation = newPermutation;
                    group = currentPermutation.CombinedGroup;
                }

                // Check mandatory requirement
                if (requireMandatory && group.LabOrder != null && !group.ContainsAllMandatoryTests())
                {
                    continue;
                }

                // Check filled requirement
                if (useFilledRequirement && group.LabOrder != null &&
                    group.LabTests.Count < group.LabOrder.FilledRequirement)
                {
                    continue;
                }

                int groupCount = currentPermutation.ContainedGroups.Count;
                string tieBreakerString = "";
                bool isBetterMatch = false;
                int matchScoreSum = 0;

                if (group.LabOrder != null)
                {
                    tieBreakerString = group.LabOrder.TieBreakerString;
                    if (potentialOrderCodes.Count > 1)
                    {
                        matchScoreSum = currentPermutation
                            .ContainedTests
                            .Where(labTest => attributesToScore.Contains(labTest.Attribute))
                            .Sum(labTest => dbCache.GetMappingScore(labTest.TestCode, labTest.Attribute));
                    }
                }

                // Best match if:
                // 1. Has more contained groups than other grouping OR
                if (bestGroup == null || groupCount > bestGroupCount)
                {
                    isBetterMatch = true;
                }
                // 2. Has same number of contained groups AND better match score sum or a lesser TieBreakerString.
                else if (groupCount == bestGroupCount)
                {
                    if (matchScoreSum > bestMatchScoreSum)
                    {
                        isBetterMatch = true;
                    }
                    else if (matchScoreSum == bestMatchScoreSum &&
                        string.Compare(
                            tieBreakerString, bestTieBreakerString, StringComparison.Ordinal) < 0)
                    {
                        isBetterMatch = true;
                    }
                }

                if (isBetterMatch)
                {
                    bestGroup = currentPermutation;
                    bestGroupCount = groupCount;
                    bestTieBreakerString = tieBreakerString;
                    bestMatchScoreSum = matchScoreSum;
                }
            }

            return bestGroup;
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
        /// <param name="requireMandatory">If <see langword="true"/> then will only map to an
        /// order if all mandatory tests are present.</param>
        /// <param name="useFilledRequirement">Whether to require that orders meet their filled requirement.</param>
        static IEnumerable<OrderGrouping> MapSingleUnknownOrders(IEnumerable<OrderGrouping> orderGroups,
            Dictionary<string, LabOrder> labOrders, OrderMappingDBCache dbCache,
            bool requireMandatory, bool useFilledRequirement)
        {
            return orderGroups.Select(group =>
            {
                var newGroup = group;
                if (group.LabOrder == null && group.LabTests.Count == 1)
                {
                    // Try to map this unknown order
                    KeyValuePair<string, List<LabTest>> bestMatch = FindBestOrder(
                        new List<LabTest>(group.LabTests), labOrders, dbCache, requireMandatory,
                        useFilledRequirement, group.OutstandingOrderCodes, true, false);

                    // Check for a new mapping
                    if (!bestMatch.Key.Equals("UnknownOrder", StringComparison.OrdinalIgnoreCase))
                    {
                        newGroup = new OrderGrouping(labOrders[bestMatch.Key], bestMatch.Value, group);
                    }
                }

                return newGroup;
            });
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
        /// <param name="finalPass">Whether this is the second, merge-groups, pass of the algorithm</param>
        /// <param name="mustUseAllTests">Whether all tests must be mapped to a single order</param>
        /// <returns>A pair containing the order code and a list of matched tests.</returns>
        static KeyValuePair<string, List<LabTest>> FindBestOrder(List<LabTest> unmatchedTests,
            Dictionary<string, LabOrder> labOrders, OrderMappingDBCache dbCache,
            bool requireMandatory, bool useFilledRequirement, HashSet<string> outstandingOrderCodes,
            bool finalPass, bool mustUseAllTests)
        {
            // Variables to hold the best match seen thus far (will be modified as the
            // best matching algorithm does its work
            int bestMatchCount = 0;
            string bestTieBreakerString = "";
            string bestOrderId = "UnknownOrder";
            int bestMatchScoreSum = 0;
            List<LabTest> bestMatchedTests = new List<LabTest>();

            // Check to see if the first test is part of any valid order
            var potentialOrderCodes = dbCache.GetPotentialOrderCodes(unmatchedTests[0].Name)
                .ToList();

            // Loop through the potential orders attempting to match the order
            foreach (string orderCode in potentialOrderCodes)
            {
                // If outstandingOrderCodes is not null then limit to this set
                if (outstandingOrderCodes != null && outstandingOrderCodes.Count > 0
                    && !outstandingOrderCodes.Contains(orderCode))
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

                // Check if all tests could possibly fit in the order
                if (mustUseAllTests && labOrder.MaxSize < unmatchedTests.Count)
                {
                    continue;
                }

                List<LabTest> matchedTests = labOrder.GetMatchingTests(unmatchedTests, dbCache,
                    finalPass, requireMandatory);

                // If using this order is possible
                if (matchedTests != null && (!useFilledRequirement || matchedTests.Count >= labOrder.FilledRequirement))
                {
                    bool isBetterOrder = false;
                    int matchScoreSum = 0;
                    if (potentialOrderCodes.Count > 1)
                    {
                        matchScoreSum = matchedTests
                            .Sum(labTest => dbCache.GetMappingScore(labTest.TestCode, labTest.Attribute));
                    }


                    // Better match if more tests matched
                    if (matchedTests.Count > bestMatchCount)
                    {
                        isBetterOrder = true;
                    }
                    // Or equal test count and better match score sum or a lesser TieBreakerString.
                    else if (matchedTests.Count == bestMatchCount)
                    {
                        if (matchScoreSum > bestMatchScoreSum)
                        {
                            isBetterOrder = true;
                        }
                        else if (matchScoreSum == bestMatchScoreSum &&
                            string.Compare(labOrder.TieBreakerString, bestTieBreakerString,
                                            StringComparison.Ordinal) < 0)
                        {
                            isBetterOrder = true;
                        }
                    }

                    if (isBetterOrder)
                    {
                        bestMatchCount = matchedTests.Count;
                        bestMatchedTests = matchedTests;
                        bestOrderId = labOrder.OrderCode;
                        bestTieBreakerString = labOrder.TieBreakerString;
                        bestMatchScoreSum = matchScoreSum;
                    }
                }
            }

            // No best order match was found, add this test to its own order
            if (bestMatchCount == 0)
            {
                bestMatchedTests.Add(unmatchedTests[0]);
            }

            return new KeyValuePair<string, List<LabTest>>(bestOrderId, bestMatchedTests);
        }

        /// <summary>
        /// Gets the intersection of potential order codes for a collection of <see cref="LabTest"/>s
        /// </summary>
        /// <param name="labTests">The collection of <see cref="LabTest"/>s</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to query</param>
        /// <returns>The collection of potential order codes that all of the tests could be a part of</returns>
        static IEnumerable<string> GetPotentialOrderCodes(IEnumerable<LabTest> labTests,
            OrderMappingDBCache dbCache)
        {
            if (labTests.Count() == 0)
            {
                return Enumerable.Empty<string>();
            }
            var potentialOrderCodes = new HashSet<string>
                (dbCache.GetPotentialOrderCodes(labTests.First().Name));
            foreach(var test in labTests.Skip(1))
            {
                potentialOrderCodes.IntersectWith(dbCache.GetPotentialOrderCodes(test.Name));
            }

            return potentialOrderCodes;
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
                List<IAttribute> listAttributes = nameToAttributes.GetOrAdd(name.ToUpperInvariant(),
                    _ => new List<IAttribute>(vectorSize));
                for (int j = 0; j < vectorSize; j++)
                {
                    listAttributes.Add((IAttribute)vectorOfAttributes.At(j));
                }
            }

            return nameToAttributes;
        }

        /// <summary>
        /// Builds a map of names to <see cref="List{T}"/> of attributes.
        /// </summary>
        /// <param name="attributes">The list of attributes to group.</param>
        /// <returns>The map of names to attributes.</returns>
        internal static Dictionary<string, List<IAttribute>> GetMapOfNamesToAttributes(
            List<IAttribute> attributes)
        {
            // Create a dictionary to hold the values
            Dictionary<string, List<IAttribute>> nameToAttributes =
                new Dictionary<string, List<IAttribute>>();

            foreach (var attribute in attributes)
            {
                nameToAttributes.GetOrAdd(attribute.Name.ToUpperInvariant(), name =>
                    new List<IAttribute>()).Add(attribute);
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
        /// Helper class used to compare Test hierarchies
        /// </summary>
        private class CompareTestAttributes : IComparer<IAttribute>
        {
            private ISortCompare _sorter;
            
            /// <summary>
            /// Create a new instance of CompareTestAttributes
            /// </summary>
            /// <param name="sorter">The <see cref="ISortCompare"/> used to compare
            /// <see cref="IAttribute"/>s</param>
            public CompareTestAttributes(ISortCompare sorter)
            {
                _sorter = sorter;
            }

            /// <summary>
            /// Compares two Test hierarchies spatially using the first Component sub-attribute of
            /// each hierarchy or else the top-level attribute there are no Components.
            /// </summary>
            /// <param name="x">The attribute representing the first Test hierarchy.</param>
            /// <param name="y">The attribute representing the second Test hierarchy.</param>
            /// <returns>-1 if x &lt; y, 1 if x &gt; y, and 0 if x == y</returns>
            int IComparer<IAttribute>.Compare(IAttribute x, IAttribute y)
            {
                IAttribute componentX = x.SubAttributes.ToIEnumerable<IAttribute>()
                    .FirstOrDefault(attribute =>
                        attribute.Name.Equals("Component", StringComparison.OrdinalIgnoreCase));

                IAttribute componentY = y.SubAttributes.ToIEnumerable<IAttribute>()
                    .FirstOrDefault(attribute =>
                        attribute.Name.Equals("Component", StringComparison.OrdinalIgnoreCase));

                if (componentX == null && componentY == null)
                {
                    if (_sorter.LessThan(x, y))
                    {
                        return -1;
                    }
                    else if (_sorter.LessThan(y, x))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
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
                    if (_sorter.LessThan(componentX, componentY))
                    {
                        return -1;
                    }
                    else if (_sorter.LessThan(componentY, componentX))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
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