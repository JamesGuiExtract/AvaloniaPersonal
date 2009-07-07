using Extract.Interop;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// Class to hold the attribute and name of a specific lab test.
    /// </summary>
    internal class LabTest
    {
        #region Fields

        /// <summary>
        /// The attribute associated with this test
        /// </summary>
        IAttribute _attribute;

        /// <summary>
        /// The name associated with this test
        /// </summary>
        string _name;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LabTest"/> struct.
        /// </summary>
        /// <param name="attribute">The attribute associated with this object. Must
        /// not be <see langword="null"/>.</param>
        public LabTest(IAttribute attribute)
        {
            try
            {
                ExtractException.Assert("ELI26441", "Attribute cannot be null!", attribute != null);

                // Store the attribute
                _attribute = attribute;

                // Set the name based on the attributes spatial string
                SpatialString ss = _attribute.Value;
                _name = ss.String;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26442", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the attribute for this <see cref="LabTest"/>.
        /// </summary>
        /// <returns>The attribute for this <see cref="LabTest"/>.</returns>
        public IAttribute Attribute
        {
            get
            {
                return _attribute;
            }
        }

        /// <summary>
        /// Gets the name for this <see cref="LabTest"/>.
        /// </summary>
        /// <returns>The name for this <see cref="LabTest"/>.</returns>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        #endregion Properties
    }

    /// <summary>
    /// Handles mapping orders from the rules output into Lab orders and their
    /// associated EPIC codes based on a database file.
    /// </summary>
    [Guid("ABC13C14-B6C6-4679-A69B-5083D3B4B60C")]
    [ProgId("Extract.DataEntry.LabDE.LabDEOrderMapper")]
    [ComVisible(true)]
    public class LabDEOrderMapper : IOutputHandler, ICopyableObject, ICategorizedComponent,
        IPersistStream, IConfigurableObject, IMustBeConfiguredObject, IDisposable
    {
        #region Constants

        /// <summary>
        /// The default filename that will appear in the FAM to describe the task the data entry
        /// application is fulfilling
        /// </summary>
        private static readonly string _DEFAULT_OUTPUT_HANDLER_NAME = "LabDE order mapper";

        /// <summary>
        /// The current version for this object.
        /// </summary>
        private static readonly int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the database file to use for order mapping.
        /// </summary>
        private string _databaseFile;

        /// <summary>
        /// Flag to indicate whether this object is dirty or not.
        /// </summary>
        private bool _dirty;

        /// <summary>
        /// A <see cref="DataTable"/> containing the order table from the specified database.
        /// </summary>
        private DataTable _order;

        /// <summary>
        /// A <see cref="DataTable"/> containing the test table from the specified database.
        /// </summary>
        private DataTable _test;

        /// <summary>
        /// A <see cref="DataTable"/> containing the alternate test name table from the specified
        /// database.
        /// </summary>
        private DataTable _alternateTestName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </summary>
        public LabDEOrderMapper()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </summary>
        /// <param name="databaseFile">The name of the database file to attach to.</param>
        public LabDEOrderMapper(string databaseFile)
        {
            try
            {
                _databaseFile = databaseFile;
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
                return _databaseFile;
            }
            set
            {
                _databaseFile = value;
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
            try
            {
                // Expand the tags in the database file name
                AFUtility afUtility = new AFUtility();
                string databaseFile = afUtility.ExpandTagsAndFunctions(_databaseFile, pDoc);

                // Check for the database files existence
                if (!File.Exists(databaseFile))
                {
                    ExtractException ee = new ExtractException("ELI26170",
                        "Database file does not exist!");
                    ee.AddDebugData("Database File Name", databaseFile, false);
                    throw ee;
                }

                IAttribute labInfo = null;
                IAttribute resultDate = null;
                IAttribute resultTime = null;
                List<IAttribute> attributes = GetLabInfo(pAttributes, out labInfo, out resultDate,
                    out resultTime);

                string connectionString = "Data Source=" + databaseFile;
                IUnknownVector newAttributes = new IUnknownVector();
                using (SqlCeConnection dbConnection = new SqlCeConnection(connectionString))
                {
                    // Create an attribute sorter for sorting sub attributes
                    ISortCompare attributeSorter =
                                (ISortCompare) new SpatiallyCompareAttributesClass();

                    foreach (IAttribute attribute in attributes)
                    {
                        if (attribute.Name.Equals("Test", StringComparison.OrdinalIgnoreCase))
                        {
                            List<IAttribute> mappedAttributes =
                                MapOrders(attribute.SubAttributes, labInfo, resultDate, resultTime,
                                    dbConnection);
                            foreach (IAttribute newAttribute in mappedAttributes)
                            {
                                // Sort the sub attributes spatially
                                newAttribute.SubAttributes.Sort(attributeSorter);

                                // Add the attribute to the vector
                                newAttributes.PushBack(newAttribute);
                            }
                        }
                        else
                        {
                            // Not a test attribute, just copy it
                            newAttributes.PushBack(attribute);
                        }
                    }
                }

                // Clear the original attributes and set the attributes to the
                // newly mapped collection
                pAttributes.Clear();
                pAttributes.CopyFrom(newAttributes);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26171", "Unable to handle output.", ex);
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
                LabDEOrderMapper newMapper = new LabDEOrderMapper(_databaseFile);

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

                _databaseFile = mapper.DatabaseFileName;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26176", "Unable to copy order mapper.", ex);
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
                // Display the configuration form
                using (LabDEOrderMapperConfigurationForm configureForm =
                    new LabDEOrderMapperConfigurationForm(_databaseFile))
                {
                    // If the user clicked OK then set the database file
                    if (configureForm.ShowDialog() == DialogResult.OK)
                    {
                        _databaseFile = configureForm.DatabaseFileName;
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

        #region Private Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID Output Handler" COM category.
        /// </summary>
        /// <param name="type">The <see langword="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        private static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.OutputHandlers);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID Output Handler" COM category.
        /// </summary>
        /// <param name="type">The <see langword="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        private static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.OutputHandlers);
        }

        /// <summary>
        /// Searches the attribute collection for the LabInfo attribute and stores that
        /// as an out parameter.  Returns the entire attribute collection as a
        /// <see cref="List{T}"/> of IAttributes.
        /// </summary>
        /// <param name="attributes">The unknown vector of attributes to search.</param>
        /// <param name="labInfo">The lab info attribute (if found).</param>
        /// <param name="resultDate">The result date attribute (if found).</param>
        /// <param name="resultTime">The result time attribute (if found).</param>
        /// <returns>A <see cref="List{T}"/> of IAttributes.</returns>
        static List<IAttribute> GetLabInfo(IUnknownVector attributes, out IAttribute labInfo,
            out IAttribute resultDate, out IAttribute resultTime)
        {
            // Default labInfo to null
            labInfo = null;
            resultDate = null;
            resultTime = null;

            int size = attributes.Size();

            List<IAttribute> list = new List<IAttribute>(size);
            for (int i=0; i < size; i++)
            {
                IAttribute attribute = (IAttribute) attributes.At(i);

                // Check if this attribute is a LabInfo attribute (if we haven't found
                // one yet) and assign it
                if (labInfo == null &&
                    attribute.Name.Equals("LabInfo", StringComparison.OrdinalIgnoreCase))
                {
                    labInfo = attribute;
                }
                // Check if this attribute is a ResultDate attribute (if we haven't found
                // one yet) and assign it
                else if (resultDate == null &&
                    attribute.Name.Equals("ResultDate", StringComparison.OrdinalIgnoreCase))
                {
                    resultDate = attribute;
                }
                // Check if this attribute is a ResultTime attribute (if we haven't found
                // one yet) and assign it
                else if (resultTime == null &&
                    attribute.Name.Equals("ResultTime", StringComparison.OrdinalIgnoreCase))
                {
                    resultDate = attribute;
                }

                list.Add(attribute);
            }

            return list;
        }

        /// <summary>
        /// Performs the mapping from tests to order grouping.
        /// </summary>
        /// <param name="attributes">A vector of attributes to map.</param>
        /// <param name="dbConnection">The database connection to use
        /// for querying.</param>
        /// <param name="labInfo">Temporary parameter for passing in the
        /// LabInfo attribute, in the future this attribute should be a subattribute
        /// of the test.</param>
        /// <param name="resultDate">Temporary parameter for passing in the
        /// ResultDate attribute, in the future this attribute should be a subattribute
        /// of the test.</param>
        /// <param name="resultTime">Temporary parameter for passing in the
        /// ResultTime attribute, in the future this attribute should be a subattribute
        /// of the test.</param>
        List<IAttribute> MapOrders(IUnknownVector attributes, IAttribute labInfo,
            IAttribute resultDate, IAttribute resultTime, SqlCeConnection dbConnection)
        {
            // Get the source doc name from the first attribute
            string sourceDocName = "Unknown";
            if (attributes.Size() > 0)
            {
                sourceDocName = ((IAttribute)attributes.At(0)).Value.SourceDocName;
            }

            // Ensure the data sets are loaded
            LoadDataTables(dbConnection);

            // Get a map of names to attributes from the attribute collection
            Dictionary<string, List<IAttribute>> nameToAttributes =
                GetMapOfNamesToAttributes(attributes);

            IAttribute dateAttribute = null;
            IAttribute timeAttribute = null;
            IAttribute labInfoAttribute = labInfo; // Temporary fix to get lab info
            //IAttribute labInfoAttribute = null;

            // Get the date and time from the attributes
            List<IAttribute> temp;
            if (nameToAttributes.TryGetValue("COLLECTIONDATE", out temp))
            {
                // List should have at least 1 item, pick the first
                ExtractException.Assert("ELI26231", "Attribute list should have at least 1"
                    + " collection date.", temp.Count > 0);

                dateAttribute = temp[0];
            }
            temp = null;
            if (nameToAttributes.TryGetValue("COLLECTIONTIME", out temp))
            {
                // List should have at least 1 item, pick the first
                ExtractException.Assert("ELI26232", "Attribute list should have at least 1"
                    + " collection time.", temp.Count > 0);

                timeAttribute = temp[0];
            }
            temp = null;
            // Temporarily commented out until lab info attributes are moved into the Test attribute
            //if (nameToAttributes.TryGetValue("LABINFO", out temp))
            //{
            //    // List should have at least 1 item, pick the first
            //    ExtractException.Assert("ELI26522", "Attribute list should have at least 1"
            //        + " lab info object", temp.Count > 0);

            //    labInfoAttribute = temp[0];
            //}

            List<IAttribute> mappedList = new List<IAttribute>();
            foreach (KeyValuePair<string, List<IAttribute>> pair in nameToAttributes)
            {
                if (!pair.Key.Equals("COMPONENT", StringComparison.OrdinalIgnoreCase))
                {
                    mappedList.AddRange(pair.Value);
                }
                else
                {
                    // All attributes are unmatched at this point
                    List<LabTest> unmatchedTests = BuildUnmatchedTestList(pair.Value);

                    while (unmatchedTests.Count > 0)
                    {
                        // Create a new IUnknown vector for the matched tests
                        IUnknownVector vecMatched = new IUnknownVector();
                        
                        // Create the new order grouping attribute (default to UnknownOrder)
                        // and add it to the vector
                        AttributeClass orderGrouping = new AttributeClass();
                        orderGrouping.Name = "Order";
                        orderGrouping.Value.CreateNonSpatialString("UnknownOrder", sourceDocName);
                        vecMatched.PushBack(orderGrouping);

                        // Add the date, time, and lab info attribute (if available)
                        if (dateAttribute != null)
                        {
                            vecMatched.PushBack(dateAttribute);
                        }
                        if (timeAttribute != null)
                        {
                            vecMatched.PushBack(timeAttribute);
                        }
                        if (labInfoAttribute != null)
                        {
                            vecMatched.PushBack(labInfoAttribute);
                        }
                        if (resultDate != null)
                        {
                            vecMatched.PushBack(resultDate);
                        }
                        if (resultTime != null)
                        {
                            vecMatched.PushBack(resultTime);
                        }

                        // Get the best match order for the remaining unmatched tests
                        Dictionary<string, string> testNamesToCodes;
                        KeyValuePair<string, List<LabTest>> matchedTests =
                            FindBestOrder(unmatchedTests, out testNamesToCodes);

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
                                // Look for the test code for this match and update the test
                                // name based on the order code and test code
                                string testCode;
                                if (testNamesToCodes.TryGetValue(matches.Name, out testCode))
                                {
                                    SpatialString value = matches.Attribute.Value;
                                    value.Replace(matches.Name, GetTestNameFromOrderAndTestCode(
                                        orderCode, testCode, dbConnection), false, 1, null);
                                }

                                // Add the test to the vector
                                vecMatched.PushBack(matches.Attribute);
                            }

                            // Get the order name and epic code
                            KeyValuePair<string, string> orderNameAndEpicCode =
                                GetOrderNameAndEpicCodeFromOrderCode(orderCode, dbConnection);

                            // Set the order group name
                            orderGrouping.Value.ReplaceAndDowngradeToNonSpatial(
                                orderNameAndEpicCode.Key);

                            // Add the epic code
                            AttributeClass epicCode = new AttributeClass();
                            epicCode.Name = "EpicCode";
                            epicCode.Value.CreateNonSpatialString(orderNameAndEpicCode.Value,
                                sourceDocName);
                            vecMatched.PushBack(epicCode);
                        }
                        else
                        {
                            // Add all of the "UnknownOrder" tests since there is no group for them
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
            }

            return mappedList;
        }

        /// <summary>
        /// Loads the data tables from the sql connection
        /// </summary>
        /// <param name="dbConnection">The database connection to use to load the data sets.</param>
        void LoadDataTables(SqlCeConnection dbConnection)
        {
            if (_order == null)
            {
                using (SqlCeDataAdapter adapter = new SqlCeDataAdapter(
                    "SELECT * FROM [LabOrder]", dbConnection))
                {
                    _order = new DataTable();
                    _order.Locale = CultureInfo.InvariantCulture;
                    adapter.Fill(_order);
                }
            }
            if (_test == null)
            {
                using (SqlCeDataAdapter adapter = new SqlCeDataAdapter(
                    "SELECT * FROM [Test]", dbConnection))
                {
                    _test = new DataTable();
                    _test.Locale = CultureInfo.InvariantCulture;
                    adapter.Fill(_test);
                }
            }
            if (_alternateTestName == null)
            {
                using (SqlCeDataAdapter adapter = new SqlCeDataAdapter(
                    "SELECT * FROM [AlternateTestName]", dbConnection))
                {
                    _alternateTestName = new DataTable();
                    _alternateTestName.Locale = CultureInfo.InvariantCulture;
                    adapter.Fill(_alternateTestName);
                }
            }
        }

        /// <summary>
        /// Builds a list of pairs of test attribtues to test name from the list of unmatched tests.
        /// </summary>
        /// <param name="unmatchedTests">A list of unmatched test attributes.</param>
        /// <returns>A list of pairs of test attributes to test name.</returns>
        static List<LabTest> BuildUnmatchedTestList(
            List<IAttribute> unmatchedTests)
        {
            // Build a list of lab tests
            List<LabTest> returnList = new
                List<LabTest>();
            foreach(IAttribute attribute in unmatchedTests)
            {
                returnList.Add(new LabTest(attribute));
            }

            // Return the list
            return returnList;
        }

        /// <summary>
        /// Find the best order match for the collection of unmatched tests.
        /// </summary>
        /// <param name="unmatchedTests">The list of unmatched tests to try to fit into
        /// a known order.</param>
        /// <param name="testNameToTestCode">A collection of test names (the value
        /// from the <see cref="KeyValuePair{T,K}"/> mapped to its corresponding
        /// test code.</param>
        /// <returns>A pair containing the order code and a list of matched tests.</returns>
        KeyValuePair<string, List<LabTest>> FindBestOrder(
            List<LabTest> unmatchedTests,
            out Dictionary<string, string> testNameToTestCode)
        {
            // Variables to hold the best match seen thus far (will be modified as the
            // best matching algorithm does its work
            int bestMatchCount = 0;
            string bestOrderId = "UnknownOrder";
            List<LabTest> bestMatchedTests = new List<LabTest>();

            // Initialize the test name to code collection so that it will at least not
            // be null when returned (it may be empty if there is no best match).
            testNameToTestCode = new Dictionary<string, string>();

            // Loop through each order and try to find the best match for the collection
            // of unmatched tests
            foreach (DataRow orderRow in _order.Rows)
            {
                Dictionary<string, string> nameToCode = new Dictionary<string,string>();
                List<LabTest> unmatchedCopy = new List<LabTest>(unmatchedTests);
                List<LabTest> matchedTests = new List<LabTest>();

                // Attempt to match all mandatory tests
                string orderCode = (string)orderRow["Code"];
                DataRow[] tests = _test.Select("OrderCode = '" + orderCode + "'"
                    + "AND Mandatory = 1");
                bool allMandatoryMatch = true;
                foreach (DataRow test in tests)
                {
                    string testCode = (string)test["Code"];
                    string testName = (string)test["Name"];

                    // Get the alternate test names for this test
                    DataRow[] alternateName = _alternateTestName.Select("OrderCode = '"
                        + orderCode + "' AND TestCode = '" + testCode + "'");

                    // Check for test match (default to false)
                    bool testMatched = false;

                    // See if this test matches one of the unmatched tests
                    foreach (LabTest labTest in unmatchedCopy)
                    {
                        // If it doesn't match the test, check the alternate tests
                        if (!labTest.Name.Equals(testName, StringComparison.OrdinalIgnoreCase)
                            && !labTest.Name.Equals(testCode, StringComparison.OrdinalIgnoreCase))
                        {
                            bool alternateTestMatch = false;
                            foreach (DataRow alternateTest in alternateName)
                            {
                                // The alternate test name matched so add the test to
                                // the list of matched tests, remove it from the unmatched tests
                                // and update the name to code map
                                if (labTest.Name.Equals((string)alternateTest["Name"],
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!nameToCode.ContainsKey(labTest.Name))
                                    {
                                        nameToCode.Add(labTest.Name, testCode);
                                    }
                                    matchedTests.Add(labTest);
                                    unmatchedCopy.Remove(labTest);
                                    alternateTestMatch = true;
                                    break;
                                }
                            }

                            if (alternateTestMatch)
                            {
                                testMatched = true;
                                break;
                            }
                        }
                        else
                        {
                            // The name was a match, update the name to code map,
                            // add the test to the matched tests and remove it from the
                            // unmatched tests
                            if (!nameToCode.ContainsKey(labTest.Name))
                            {
                                nameToCode.Add(labTest.Name, testCode);
                            }
                            matchedTests.Add(labTest);
                            unmatchedCopy.Remove(labTest);
                            testMatched = true;
                            break;
                        }
                    }

                    // If a test did not match set the all mandatory flag to false and break
                    // from the loop
                    if (!testMatched)
                    {
                        allMandatoryMatch = false;
                        break;
                    }
                }

                // If all the mandatory tests match, try to find matching non-mandatory tests
                // as well
                if (allMandatoryMatch)
                {
                    // Now look for additional matching test
                    DataRow[] nonMandatory = _test.Select("OrderCode = '" + orderCode + "'"
                        + "AND Mandatory = 0");
                    foreach (DataRow test in nonMandatory)
                    {
                        string testCode = (string)test["Code"];
                        string testName = (string)test["Name"];

                        // Get the alternate test names for this test
                        DataRow[] alternateName = _alternateTestName.Select("OrderCode = '"
                            + orderCode + "' AND TestCode = '" + testCode + "'");

                        // See if this test matches one of the unmatched tests
                        foreach (LabTest labTest in unmatchedCopy)
                        {
                            // If it doesn't match the test, check the alternate tests
                            if (!labTest.Name.Equals(testName, StringComparison.OrdinalIgnoreCase)
                                && !labTest.Name.Equals(testCode, StringComparison.OrdinalIgnoreCase))
                            {
                                // Look for a matching alternate test name
                                bool alternateMatch = false;
                                foreach (DataRow alternateTest in alternateName)
                                {
                                    // The alternate test name matched so add the test to
                                    // the list of matched tests, remove it from the unmatched tests
                                    // and update the name to code map
                                    if (labTest.Name.Equals((string)alternateTest["Name"],
                                        StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (!nameToCode.ContainsKey(labTest.Name))
                                        {
                                            nameToCode.Add(labTest.Name, testCode);
                                        }
                                        matchedTests.Add(labTest);
                                        unmatchedCopy.Remove(labTest);
                                        alternateMatch = true;
                                        break;
                                    }
                                }

                                if (alternateMatch)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                // The name was a match, update the name to code map,
                                // add the test to the matched tests and remove it from the
                                // unmatched tests
                                if (!nameToCode.ContainsKey(labTest.Name))
                                {
                                    nameToCode.Add(labTest.Name, testCode);
                                }
                                matchedTests.Add(labTest);
                                unmatchedCopy.Remove(labTest);
                                break;
                            }
                        }
                    }

                    // Check if this is a better match than seen already
                    if (matchedTests.Count > bestMatchCount)
                    {
                        bestMatchCount = matchedTests.Count;
                        bestOrderId = orderCode;
                        bestMatchedTests = matchedTests;
                        testNameToTestCode = nameToCode;
                    }
                }
            }

            // If there was no match then all tests are unmatched, return a copy of the
            // unmatched tests.
            if (bestMatchCount == 0)
            {
                bestMatchedTests = new List<LabTest>(unmatchedTests);
            }

            return new KeyValuePair<string, List<LabTest>>(bestOrderId,
                bestMatchedTests);
        }

        /// <summary>
        /// Builds a map of names to <see cref="List{T}"/> of attributes.
        /// </summary>
        /// <param name="attributes">The vector of attributes to group.</param>
        /// <returns>The map of names to attributes.</returns>
        static Dictionary<string, List<IAttribute>> GetMapOfNamesToAttributes(
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
        /// Gets the order name and epic code from the database based on the specified order code.
        /// </summary>
        /// <param name="orderCode">The order code to search for.</param>
        /// <param name="dbConnection">The database connection to use.</param>
        /// <returns>A pair containing the order name and epic code.</returns>
        static KeyValuePair<string, string> GetOrderNameAndEpicCodeFromOrderCode(string orderCode,
            SqlCeConnection dbConnection)
        {
            string query = "SELECT [Name], [EpicCode] FROM [LabOrder] WHERE [Code] = '"
                + orderCode + "'";

            using (SqlCeDataAdapter dataAdapter = new SqlCeDataAdapter(query, dbConnection))
            {
                using (DataTable dt = new DataTable())
                {
                    dt.Locale = CultureInfo.InvariantCulture;
                    dataAdapter.Fill(dt);

                    // Should only be 1 row, so just return the top row
                    ExtractException.Assert("ELI26233", "Order name not found!",
                        dt.Rows.Count == 1, "Order Code", orderCode ?? "null");

                    return new KeyValuePair<string, string>((string)dt.Rows[0][0],
                        (string)dt.Rows[0][1]);
                }
            }
        }

        /// <summary>
        /// Gets the test name from the database based on the specified order code and test code.
        /// </summary>
        /// <param name="orderCode">The order code to search on.</param>
        /// <param name="testCode">The test code to search on.</param>
        /// <param name="dbConnection">The database connection to use.</param>
        /// <returns>The test name for the specified order code and test code.</returns>
        static string GetTestNameFromOrderAndTestCode(string orderCode, string testCode,
            SqlCeConnection dbConnection)
        {
            string query = "SELECT [Name] FROM [Test] WHERE [OrderCode] = '" + orderCode
                + "' AND [Code] = '" + testCode + "'";

            using (SqlCeDataAdapter dataAdapter = new SqlCeDataAdapter(query, dbConnection))
            {
                using (DataTable dt = new DataTable())
                {
                    dt.Locale = CultureInfo.InvariantCulture;
                    dataAdapter.Fill(dt);

                    // Should only be 1 row, so just return the top row
                    ExtractException.Assert("ELI26234", "Could not find test name!",
                        dt.Rows.Count == 1, "Order Code", orderCode ?? "null",
                        "Test Code", testCode ?? "null");
                    return (string)dt.Rows[0][0];
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
                // Dispose of managed objects
                if (_order != null)
                {
                    _order.Dispose();
                    _order = null;
                }
                if (_test != null)
                {
                    _test.Dispose();
                    _test = null;
                }
                if (_alternateTestName != null)
                {
                    _alternateTestName.Dispose();
                    _alternateTestName = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion
    }
}
