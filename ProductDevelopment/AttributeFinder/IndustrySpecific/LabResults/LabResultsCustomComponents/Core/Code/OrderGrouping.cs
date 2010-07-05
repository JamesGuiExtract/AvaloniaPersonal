using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

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
        readonly IAttribute _attribute;

        /// <summary>
        /// The collection date for this order group.
        /// </summary>
        readonly DateTime _collectionDate = DateTime.MinValue;

        /// <summary>
        /// The collection time for this order group.
        /// </summary>
        readonly DateTime _collectionTime = DateTime.MinValue;

        /// <summary>
        /// The collection of lab tests in this order group.
        /// </summary>
        readonly List<LabTest> _labTests = new List<LabTest>();

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
        /// <param name="grouping">The <see cref="OrderGrouping"/> to initialize
        /// the group from.</param>
        public OrderGrouping(OrderGrouping grouping)
        {
            _attribute = grouping._attribute;
            _collectionDate = grouping._collectionDate;
            _collectionTime = grouping._collectionTime;
            _labTests.AddRange(grouping._labTests);
            _nameToAttributes = new Dictionary<string,List<IAttribute>>();
            foreach (KeyValuePair<string, List<IAttribute>> pair in grouping._nameToAttributes)
            {
                _nameToAttributes.Add(pair.Key, new List<IAttribute>(pair.Value));
            }
            _labOrder = grouping._labOrder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderGrouping"/> class.
        /// </summary>
        /// <param name="attribute">The attribute to initialize the group from.</param>
        public OrderGrouping(IAttribute attribute)
            : this(attribute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderGrouping"/> class.
        /// </summary>
        /// <param name="attribute">The attribute to initialize the group from.</param>
        /// <param name="labOrders">A map of lab order codes to
        /// <see cref="LabOrder"/>s.  If not <see langword="null"/> then
        /// the EpicCode attribute will be used to search the collection and
        /// set the <see cref="LabOrder"/> value.</param>
        // The call to DateTime.TryParse has been analyzed and the result can be
        // safely ignored since we are explicitly handling the setting of the out
        // parameter to the default value of DateTime.MinValue
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            MessageId="System.DateTime.TryParse(System.String,System.DateTime@)")]
        public OrderGrouping(IAttribute attribute, Dictionary<string, LabOrder> labOrders)
        {
            try
            {
                _attribute = attribute;

                // Get the sub attributes
                IUnknownVector attributes = attribute.SubAttributes;

                // Get a map of names to attributes from the attribute collection
                _nameToAttributes = LabDEOrderMapper.GetMapOfNamesToAttributes(attributes);

                // Get the date and time from the attributes
                List<IAttribute> temp;
                if (_nameToAttributes.TryGetValue("COLLECTIONDATE", out temp))
                {
                    // List should have at least 1 item, pick the first
                    ExtractException.Assert("ELI29076", "Attribute list should have at least 1"
                        + " collection date.", temp.Count > 0);

                    IAttribute dateAttribute = temp[0];
                    string date = dateAttribute.Value.String;
                    DateTime.TryParse(date, out _collectionDate);
                }
                temp = null;
                if (_nameToAttributes.TryGetValue("COLLECTIONTIME", out temp))
                {
                    // List should have at least 1 item, pick the first
                    ExtractException.Assert("ELI29077", "Attribute list should have at least 1"
                        + " collection time.", temp.Count > 0);

                    IAttribute timeAttribute = temp[0];
                    string time = timeAttribute.Value.String;
                    DateTime.TryParse(time, out _collectionTime);
                }

                // Get the list of lab tests
                if (_nameToAttributes.TryGetValue("COMPONENT", out temp))
                {
                    _labTests.AddRange(LabDEOrderMapper.BuildTestList(temp));
                }

                // Get the epic code and set the _labOrder value
                temp = null;
                if (labOrders != null &&
                    _nameToAttributes.TryGetValue("EPICCODE", out temp))
                {
                    string epicCode = temp[0].Value.String;

                    // Find the lab order for this code
                    foreach (LabOrder order in labOrders.Values)
                    {
                        if (order.EpicCode.Equals(epicCode))
                        {
                            _labOrder = order;
                            break;
                        }
                    }

                    temp = null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29078", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Inserts the name attribute map from one <see cref="OrderGrouping"/> into this
        /// group.
        /// </summary>
        /// <param name="group">The group to get the map from.</param>
        internal void InsertNameAttributeMap(OrderGrouping group)
        {
            foreach (KeyValuePair<string, List<IAttribute>> pair in group.NameToAttributes)
            {
                string key = pair.Key;

                List<IAttribute> attributes;
                if (!_nameToAttributes.TryGetValue(key, out attributes))
                {
                    _nameToAttributes.Add(key, pair.Value);
                }
                else if (key.Equals("COLLECTIONDATE", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("COLLECTIONTIME", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("NAME", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("EPICCODE", StringComparison.OrdinalIgnoreCase))
                {
                    // Do nothing with these attributes, they will be obtained from this
                    // object or modified when this object is added to a new grouping
                }
                else
                {
                    attributes.AddRange(pair.Value);
                }
            }

            // Update the labtests collection
            List<IAttribute> temp;
            if (_nameToAttributes.TryGetValue("COMPONENT", out temp))
            {
                _labTests.Clear();
                _labTests.AddRange(LabDEOrderMapper.BuildTestList(temp));
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
        /// collection)
        /// </returns>
        internal IUnknownVector GetAllAttributesAsIUnknownVector()
        {
            IUnknownVector subAttributes = new IUnknownVector();
            foreach(List<IAttribute> list in _nameToAttributes.Values)
            {
                foreach(IAttribute attribute in list)
                {
                    subAttributes.PushBack(attribute);
                }
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
        /// Updates the lab tests in the attribute collection to their official name.
        /// <para><b>Note:</b></para>
        /// This should only be called as the last step before adding this list to the final
        /// order grouping collection.
        /// </summary>
        internal void UpdateLabTestsToOfficialName(SqlCeConnection dbConnection)
        {
            // Only perform mapping if the LabOrder has been set (this group may be
            // an unknown order)
            if (_labOrder != null)
            {
                // The get matching tests call should update the test code for each lab test
                List<LabTest> temp = _labOrder.GetMatchingTests(_labTests);

                // Sanity check, the lists should be the same size
                if (temp.Count != _labTests.Count)
                {
                    ExtractException.ThrowLogicException("ELI29080");
                }

                // Create a dictionary mapping the LabTest to its name
                Dictionary<string, LabTest> labTests = new Dictionary<string, LabTest>(temp.Count);
                foreach (LabTest test in temp)
                {
                    labTests.Add(test.Name.ToUpperInvariant(), test);
                }

                // Now iterate the collection of component attributes and update their value
                List<IAttribute> tests;
                if (!_nameToAttributes.TryGetValue("COMPONENT", out tests))
                {
                    // At this point there should always be at least 1 test
                    ExtractException.ThrowLogicException("ELI29081");
                }

                foreach (IAttribute attribute in tests)
                {
                    // Get the name
                    string name = attribute.Value.String.ToUpperInvariant();

                    // Get the lab test for this name
                    LabTest labTest;
                    if (!labTests.TryGetValue(name, out labTest))
                    {
                        ExtractException.ThrowLogicException("ELI29082");
                    }

                    // Create the official name subattribute
                    IAttribute officialName = new AttributeClass();
                    officialName.Name = "OfficialName";
                    officialName.Value.CreateNonSpatialString(
                        LabDEOrderMapper.GetTestNameFromTestCode(labTest.TestCode, dbConnection),
                        attribute.Value.SourceDocName);

                    // Add the official name subattribute
                    attribute.SubAttributes.PushBack(officialName);
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
        /// Gets/sets the <see cref="LabOrder"/> that this group is associated with.
        /// </summary>
        public LabOrder LabOrder
        {
            get
            {
                return _labOrder;
            }
            set
            {
                _labOrder = value;
            }
        }

        #endregion Properties
    }
}
