using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// Class that contains all the data for an order group attribute
    /// </summary>
    internal class OrderGrouping
    {
        #region Fields

        /// <summary>
        /// The attribute that this order group is associated with (this is the top level attribute).
        /// </summary>
        IAttribute _attribute;

        /// <summary>
        /// The collection date for this order group.
        /// </summary>
        readonly DateTime _collectionDate = DateTime.MinValue;

        /// <summary>
        /// The collection time for this order group.
        /// </summary>
        readonly DateTime _collectionTime = DateTime.MinValue;

        /// <summary>
        /// The result date for this order group.
        /// </summary>
        readonly DateTime _resultDate = DateTime.MinValue;

        /// <summary>
        /// The result time for this order group.
        /// </summary>
        readonly DateTime _resultTime = DateTime.MinValue;

        /// <summary>
        /// The collection of lab tests in this order group.
        /// </summary>
        List<LabTest> _labTests = new List<LabTest>();

        /// <summary>
        /// Set of all known outstanding order codes. Only orders in this set will be considered for
        /// use. If null then all orders will be considered.
        /// </summary>
        readonly HashSet<string> _outstandingOrderCodes;

        /// <summary>
        /// A dictionary grouping attributes to their names
        /// </summary>
        readonly Dictionary<string, List<IAttribute>> _nameToAttributes;

        /// <summary>
        /// The lab order that this group is associated with.
        /// </summary>
        LabOrder _labOrder;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderGrouping"/> class.
        /// </summary>
        /// <param name="labOrder">The <see cref="LabOrder"/> of this instance</param>
        /// <param name="tests">The collection of <see cref="LabTests"/> contained in this instance</param>
        /// <param name="sourceGroups">The collection of <see cref="OrderGrouping"/>s that are a part of this instance</param>
        public OrderGrouping(LabOrder labOrder, List<LabTest> tests, params OrderGrouping[] sourceGroups)
        {
            _labOrder = labOrder;
            _labTests = tests;
            _nameToAttributes = new Dictionary<string, List<IAttribute>>();
            string sourceDocName = null;
            foreach (var grouping in sourceGroups)
            {
                // Set collection date if not already set
                if (_collectionDate == DateTime.MinValue)
                {
                    _collectionDate = grouping._collectionDate;
                }

                // Set collection time if not already set
                if (_collectionTime == DateTime.MinValue)
                {
                    _collectionTime = grouping._collectionTime;
                }

                // Set result date if not already set
                if (_resultDate == DateTime.MinValue)
                {
                    _resultDate = grouping._resultDate;
                }

                // Set result time if not already set
                if (_resultTime == DateTime.MinValue)
                {
                    _resultTime = grouping._resultTime;
                }

                // Add other attributes
                InsertNameToAttributeMap(grouping.NameToAttributes);

                // Get the source doc to use to create new attributes
                if (sourceDocName == null)
                {
                    sourceDocName = grouping._attribute.Value.SourceDocName;
                }
            }

            // Update the order name of the group mapping
            var orderName = new SpatialStringClass();
            orderName.CreateNonSpatialString(_labOrder == null ? "UnknownOrder"
                : _labOrder.OrderName, sourceDocName);
            NameToAttributes["NAME"] = new List<IAttribute>(1)
                { new AttributeClass { Name = "Name", Value = orderName } };

            // Update order code
            if (_labOrder != null)
            {
                // Update the order code of the group mapping
                var orderCode = new SpatialStringClass();
                orderCode.CreateNonSpatialString(_labOrder.OrderCode, sourceDocName);
                NameToAttributes["ORDERCODE"] = new List<IAttribute>(1)
                    { new AttributeClass { Name = "OrderCode", Value = orderCode } };
            }

            // Create the root attribute
            _attribute = new AttributeClass();
            _attribute.Name = "Test";
            _attribute.Value.CreateNonSpatialString("N/A", sourceDocName);

            // Update the subattributes
            _attribute.SubAttributes = GetAllAttributesAsIUnknownVector();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderGrouping"/> class.
        /// </summary>
        /// <param name="labOrder">The <see cref="LabOrder"/> of this instance</param>
        /// <param name="tests">The collection of <see cref="LabTests"/> contained in this instance</param>
        /// <param name="limitToOutstandingOrders">Whether to limit possible orders to outstanding order codes</param>
        /// <param name="sourceDocName">The source image of the data</param>
        /// <param name="nonTestAttributes">The subattributes of the Test hierarchy that are not tests
        /// (not Component attributes)</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
                    MessageId="System.DateTime.TryParse(System.String,System.DateTime@)")]
        public OrderGrouping(LabOrder labOrder, List<LabTest> tests, bool limitToOutstandingOrders,
            string sourceDocName, List<IAttribute> nonTestAttributes)
        {
            try
            {
                _labOrder = labOrder;
                _labTests = tests;
                _nameToAttributes = LabDEOrderMapper.GetMapOfNamesToAttributes(nonTestAttributes);
                
                // Get the date and time from the attributes
                List<IAttribute> tempList;
                if (_nameToAttributes.TryGetValue("COLLECTIONDATE", out tempList))
                {
                    // List should have at least 1 item, pick the first
                    ExtractException.Assert("ELI38963", "Attribute list should have at least 1"
                        + " collection date.", tempList.Count > 0);

                    IAttribute dateAttribute = tempList[0];
                    string date = dateAttribute.Value.String;
                    DateTime.TryParse(date, out _collectionDate);
                }
                tempList = null;
                if (_nameToAttributes.TryGetValue("COLLECTIONTIME", out tempList))
                {
                    // List should have at least 1 item, pick the first
                    ExtractException.Assert("ELI38964", "Attribute list should have at least 1"
                        + " collection time.", tempList.Count > 0);

                    IAttribute timeAttribute = tempList[0];
                    string time = timeAttribute.Value.String;
                    DateTime.TryParse(time, out _collectionTime);
                }

                tempList = null;
                if (_nameToAttributes.TryGetValue("RESULTDATE", out tempList))
                {
                    // List should have at least 1 item, pick the first
                    ExtractException.Assert("ELI38965", "Attribute list should have at least 1"
                        + " result date.", tempList.Count > 0);

                    IAttribute dateAttribute = tempList[0];
                    string date = dateAttribute.Value.String;
                    DateTime.TryParse(date, out _resultDate);
                }
                tempList = null;
                if (_nameToAttributes.TryGetValue("RESULTTIME", out tempList))
                {
                    // List should have at least 1 item, pick the first
                    ExtractException.Assert("ELI38966", "Attribute list should have at least 1"
                        + " result time.", tempList.Count > 0);

                    IAttribute timeAttribute = tempList[0];
                    string time = timeAttribute.Value.String;
                    DateTime.TryParse(time, out _resultTime);
                }

                // Get set of outstanding orders
                if (limitToOutstandingOrders)
                {
                    tempList = null;
                    if (_nameToAttributes.TryGetValue("OUTSTANDINGORDERCODE", out tempList))
                    {
                        _outstandingOrderCodes = LabDEOrderMapper.BuildOutstandingOrdersSet(tempList);
                    }
                    else
                    {
                        _outstandingOrderCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                }

                // Update the order name of the group mapping
                var orderName = new SpatialStringClass();
                orderName.CreateNonSpatialString(_labOrder == null ? "UnknownOrder"
                    : _labOrder.OrderName, sourceDocName);
                NameToAttributes["NAME"] = new List<IAttribute>(1)
                    { new AttributeClass { Name = "Name", Value = orderName } };

                if (_labOrder != null)
                {
                    // Update the order code of the group mapping
                    var orderCode = new SpatialStringClass();
                    orderCode.CreateNonSpatialString(_labOrder.OrderCode, sourceDocName);
                    NameToAttributes["ORDERCODE"] = new List<IAttribute>(1)
                        { new AttributeClass { Name = "OrderCode", Value = orderCode } };
                }

                // Create the root attribute
                _attribute = new AttributeClass();
                _attribute.Name = "Test";
                _attribute.Value.CreateNonSpatialString("N/A", sourceDocName);

                // Update the subattributes
                _attribute.SubAttributes = GetAllAttributesAsIUnknownVector();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI38967", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Inserts the values from a name to attribute map into this group.
        /// </summary>
        /// <param name="nameToAttributes">The map to add.</param>
        internal void InsertNameToAttributeMap(Dictionary<string, List<IAttribute>> nameToAttributes)
        {
            foreach (KeyValuePair<string, List<IAttribute>> pair in nameToAttributes)
            {
                string key = pair.Key;

                List<IAttribute> attributes;
                if (!_nameToAttributes.TryGetValue(key, out attributes))
                {
                    _nameToAttributes.Add(key, pair.Value);
                }
                else if (key.Equals("COLLECTIONDATE", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("COLLECTIONTIME", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("RESULTDATE", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("RESULTTIME", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("NAME", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("ORDERCODE", StringComparison.OrdinalIgnoreCase))
                {
                    // Do nothing with these attributes, they will be obtained from this
                    // object or modified when this object is added to a new grouping
                }
                else
                {
                    attributes.AddRange(pair.Value);
                }
            }
        }

        /// <summary>
        /// Returns an IUnknownVector containing all of the attributes of this group 
        /// (all of the attributes in the <see cref="OrderGrouping.NameToAttributes"/>
        /// collection)
        /// </summary>
        /// <returns>
        /// An IUnknownVector containing all of the attributes of this group 
        /// (all of the attributes in the <see cref="OrderGrouping.NameToAttributes"/>
        /// collection plus all lab tests)
        /// </returns>
        internal IUnknownVector GetAllAttributesAsIUnknownVector()
        {
            IUnknownVector subAttributes = new IUnknownVector();
            foreach(var attribute in _nameToAttributes.Keys
                .Where(key => !key.Equals("COMPONENT", StringComparison.OrdinalIgnoreCase))
                .SelectMany(key => _nameToAttributes[key]))
            {
                subAttributes.PushBack(attribute);
            }
            foreach(var test in _labTests)
            {
                subAttributes.PushBack(test.Attribute);
            }

            return subAttributes;
        }

        /// <summary>
        /// Compares two <see cref="OrderGrouping"/> objects to see if their collection dates
        /// are equal.
        /// </summary>
        /// <param name="group1">The first group to compare.</param>
        /// <param name="group2">The second group to compare.</param>
        /// <returns><see langword="true"/> if the collection dates are the same and
        /// <see langword="false"/> if they are not.</returns>
        internal static bool CollectionDatesEqual(OrderGrouping group1, OrderGrouping group2)
        {
            bool equal = (group1.CollectionDate == group1.CollectionTime)
                    && (group2.CollectionDate == group2.CollectionTime);

            if (!equal)
            {
                // Check for valid collection dates
                if (group1.CollectionDate != DateTime.MinValue
                    && group2.CollectionDate != DateTime.MinValue)
                {
                    // Check if the dates are equal
                    TimeSpan difference = group1.CollectionDate.Subtract(group2.CollectionDate);
                    equal = difference.Days == 0;
                }
                else
                {
                    // One date does not exist, set equal to true
                    equal = true;
                }

                // If there are valid times, compare those
                if (equal && group1.CollectionTime != DateTime.MinValue
                    && group2.CollectionTime != DateTime.MinValue)
                {
                    TimeSpan difference = group1.CollectionTime.Subtract(group2.CollectionTime);
                    equal = difference.Minutes == 0;
                }
            }


            // Return the compared value
            return equal;
        }

        /// <summary>
        /// Compares two <see cref="OrderGrouping"/> objects to see if their result dates
        /// are equal.
        /// </summary>
        /// <param name="group1">The first group to compare.</param>
        /// <param name="group2">The second group to compare.</param>
        /// <returns><see langword="true"/> if the result dates are the same and
        /// <see langword="false"/> if they are not.</returns>
        internal static bool ResultDatesEqual(OrderGrouping group1, OrderGrouping group2)
        {
            bool equal = (group1.ResultDate == group1.ResultTime)
                    && (group2.ResultDate == group2.ResultTime);

            if (!equal)
            {
                // Check for valid result dates
                if (group1.ResultDate != DateTime.MinValue
                    && group2.ResultDate != DateTime.MinValue)
                {
                    // Check if the dates are equal
                    TimeSpan difference = group1.ResultDate.Subtract(group2.ResultDate);
                    equal = difference.Days == 0;
                }
                else
                {
                    // One date does not exist, set equal to true
                    equal = true;
                }

                // If there are valid times, compare those
                if (equal && group1.ResultTime != DateTime.MinValue
                    && group2.ResultTime != DateTime.MinValue)
                {
                    TimeSpan difference = group1.ResultTime.Subtract(group2.ResultTime);
                    equal = difference.Minutes == 0;
                }
            }


            // Return the compared value
            return equal;
        }

        /// <summary>
        /// Updates the lab tests in the attribute collection to their official name.
        /// <para><b>Note:</b></para>
        /// This should only be called as the last step before adding this list to the final
        /// order grouping collection.
        /// </summary>
        internal void AddOfficialNameAndTestCode(OrderMappingDBCache dbCache,
            bool addESNamesAttribute,
            bool addESTestCodesAttribute,
            bool setFuzzyType)
        {
            foreach (LabTest test in _labTests)
            {
                IAttribute attribute = test.Attribute;

                // Only perform mapping if the LabOrder has been set (this group may be
                // an unknown order)
                if (_labOrder != null)
                {
                    // Create the official name subattribute
                    IAttribute officialName = new AttributeClass();
                    officialName.Name = "OfficialName";
                    officialName.Value.CreateNonSpatialString(
                        LabDEOrderMapper.GetTestNameFromTestCode(test.TestCode, dbCache),
                        attribute.Value.SourceDocName);

                    // Add the official name subattribute
                    attribute.SubAttributes.PushBack(officialName);

                    // Create the test code subattribute
                    IAttribute testCode = new AttributeClass();
                    testCode.Name = "TestCode";
                    testCode.Value.CreateNonSpatialString(test.TestCode,
                        attribute.Value.SourceDocName);

                    // Add the test code subattribute
                    attribute.SubAttributes.PushBack(testCode);

                    // Create the ESNames subattribute
                    if (addESNamesAttribute)
                    {
                        HashSet<string> esNames;
                        if (dbCache.TryGetESNames(test.TestCode, out esNames))
                        {
                            IAttribute esNamesAttribute = new AttributeClass();
                            esNamesAttribute.Name = "ESNames";
                            esNamesAttribute.Value.CreateNonSpatialString
                                (String.Join(";", esNames.OrderBy(x => x)), attribute.Value.SourceDocName);

                            // Add the ESName subattribute
                            attribute.SubAttributes.PushBack(esNamesAttribute);
                        }
                    }

                }

                if (addESTestCodesAttribute)
                {
                    var esTestCodes = dbCache.GetESTestCodesForName(test.Name);
                    if (esTestCodes.Any())
                    {
                        IAttribute esCodesAttribute = new AttributeClass();
                        esCodesAttribute.Name = "ESTestCodes";
                        esCodesAttribute.Value.CreateNonSpatialString
                            (String.Join(";", esTestCodes.OrderBy(x => x)), attribute.Value.SourceDocName);

                        // Add the ESTestCodes subattribute
                        attribute.SubAttributes.PushBack(esCodesAttribute);
                    }
                }

                if (setFuzzyType && test.FuzzyMatch)
                {
                    attribute.Type = "Fuzzy";
                }
            }
        }

        /// <summary>
        /// Checks whether this order grouping contains all mandatory tests.
        /// <para><b>Note:</b></para>
        /// Do not call this method until this group has been mapped to an order (i.e.
        /// <see cref="OrderGrouping.LabOrder"/> does not equal <see langword="null"/>).
        /// </summary>
        /// <returns>Whether all mandatory tests are present or not.</returns>
        internal bool ContainsAllMandatoryTests()
        {
            try
            {
                ExtractException.Assert("ELI30093",
                    "Cannot check for mandatory tests when order has not been mapped.",
                    _labOrder != null);

                return _labOrder.ContainsAllMandatoryTests(_labTests);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30094", ex);
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the top level attribute that this group is associated with.
        /// </summary>
        public IAttribute Attribute
        {
            get
            {
                return _attribute;
            }
        }

        /// <summary>
        /// Gets the collection date that this group is associated with.
        /// </summary>
        public DateTime CollectionDate
        {
            get
            {
                return _collectionDate;
            }
        }

        /// <summary>
        /// Gets the collection time that this group is associated with.
        /// </summary>
        public DateTime CollectionTime
        {
            get
            {
                return _collectionTime;
            }
        }

        /// <summary>
        /// Gets the result date that this group is associated with.
        /// </summary>
        public DateTime ResultDate
        {
            get
            {
                return _resultDate;
            }
        }

        /// <summary>
        /// Gets the result time that this group is associated with.
        /// </summary>
        public DateTime ResultTime
        {
            get
            {
                return _resultTime;
            }
        }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="LabTest"/> that
        /// this group is associated with.
        /// </summary>
        public ReadOnlyCollection<LabTest> LabTests
        {
            get
            {
                return _labTests.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the map of attribute names to attributes that this group is associated with.
        /// </summary>
        public Dictionary<string, List<IAttribute>> NameToAttributes
        {
            get
            {
                return _nameToAttributes;
            }
        }

        /// <summary>
        /// Gets the <see cref="LabOrder"/> that this group is associated with.
        /// </summary>
        public LabOrder LabOrder
        {
            get
            {
                return _labOrder;
            }
        }

        /// <summary>
        /// Gets the set of all known outstanding order codes. Only orders in this set will be
        /// considered for use. If <see langword="null"/> then all orders will be considered.
        /// </summary>
        public HashSet<string> OutstandingOrderCodes
        {
            get
            {
                return _outstandingOrderCodes;
            }
        }

        #endregion Properties
    }
}
