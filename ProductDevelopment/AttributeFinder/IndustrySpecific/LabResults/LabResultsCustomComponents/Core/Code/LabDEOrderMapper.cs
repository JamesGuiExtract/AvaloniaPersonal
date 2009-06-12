using Extract.Interop;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
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
    /// Handles mapping orders from the rules output into Lab orders and their
    /// associated EPIC codes based on a database file.
    /// </summary>
    [Guid("ABC13C14-B6C6-4679-A69B-5083D3B4B60C")]
    [ProgId("Extract.DataEntry.LabDE.LabDEOrderMapper")]
    [ComVisible(true)]
    public class LabDEOrderMapper : IOutputHandler, ICopyableObject, ICategorizedComponent,
        IPersistStream, IConfigurableObject, IMustBeConfiguredObject
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

                string connectionString = "Data Source=" + databaseFile;
                using (SqlCeConnection dbConnection = new SqlCeConnection(connectionString))
                {
                    IUnknownVector newAttributes = new IUnknownVector();
                    int size = pAttributes.Size();
                    for (int i = 0; i < size; i++)
                    {
                        IAttribute attribute = (IAttribute)pAttributes.At(i);
                        if (attribute.Name.Equals("Test", StringComparison.OrdinalIgnoreCase))
                        {
                            List<IAttribute> mappedAttributes =
                                MapOrders(attribute.SubAttributes, dbConnection);
                            foreach (IAttribute newAttribute in mappedAttributes)
                            {
                                newAttributes.PushBack(newAttribute);
                            }
                        }
                        else
                        {
                            // Not a test attribute, just copy it
                            newAttributes.PushBack(attribute);
                        }
                    }

                    // Clear the original attributes and set the attributes to the
                    // newly mapped collection
                    pAttributes.Clear();
                    pAttributes.CopyFrom(newAttributes);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26171", ex);
                throw new ExtractException("ELI26172", ee.AsStringizedByteStream());
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
                ExtractException ee = ExtractException.AsExtractException("ELI26173", ex);
                throw new ExtractException("ELI26174", ee.AsStringizedByteStream());
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
                ExtractException ee = ExtractException.AsExtractException("ELI26176", ex);
                throw new ExtractException("ELI26177", ee.AsStringizedByteStream());
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
                ExtractException ee = ExtractException.AsExtractException("ELI26178", ex);
                throw new ExtractException("ELI26179", ee.AsStringizedByteStream());
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
                ExtractException ee = ExtractException.AsExtractException("ELI26180", ex);
                throw new ExtractException("ELI26181", ee.AsStringizedByteStream());
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
                ExtractException ee = ExtractException.AsExtractException("ELI26182", ex);
                throw new ExtractException("ELI26183", ee.AsStringizedByteStream());
            }
        }

        /// <summary>
        /// Loads this object from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                ExtractException.Assert("ELI26184", "Stream is null!", stream != null);

                // Get the size of data stream to load
                byte[] dataLengthBuffer = new Byte[4];
                stream.Read(dataLengthBuffer, dataLengthBuffer.Length, IntPtr.Zero);
                int dataLength = BitConverter.ToInt32(dataLengthBuffer, 0);

                // Read the data from the provided stream into a buffer
                byte[] dataBuffer = new byte[dataLength];
                stream.Read(dataBuffer, dataLength, IntPtr.Zero);

                // Read the settings from the buffer; 
                // Create a memory stream and binary formatter to deserialize the settings.
                using (MemoryStream memoryStream = new MemoryStream(dataBuffer))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();

                    // Read the version of the object being loaded.
                    int version = (int)binaryFormatter.Deserialize(memoryStream);
                    ExtractException.Assert("ELI26185", "Unable to load newer data entry task!",
                        version <= _CURRENT_VERSION);

                    // Read the database file name from the stream
                    _databaseFile = (string)binaryFormatter.Deserialize(memoryStream);
                }

                // False since a new object was just loaded
                _dirty = false;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26186", ex);
                throw new ExtractException("ELI26187", ee.AsStringizedByteStream());
            }
        }

        /// <summary>
        /// Saves this object to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to save to.</param>
        /// <param name="clearDirty">If <see langword="true"/> will clear the dirty flag.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                ExtractException.Assert("ELI26188", "Stream is null!", stream != null);

                // Create a memory stream and binary formatter to serialize the settings.
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();

                    // Write the version of the object being saved.
                    binaryFormatter.Serialize(memoryStream, _CURRENT_VERSION);

                    // Save the settings to the memory stream
                    binaryFormatter.Serialize(memoryStream, _databaseFile ?? "");

                    // Write the memory stream to the provided IStream.
                    byte[] dataBuffer = memoryStream.ToArray();
                    byte[] dataLengthBuffer = BitConverter.GetBytes(dataBuffer.Length);
                    stream.Write(dataLengthBuffer, dataLengthBuffer.Length, IntPtr.Zero);
                    stream.Write(dataBuffer, dataBuffer.Length, IntPtr.Zero);

                    if (clearDirty)
                    {
                        _dirty = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26189", ex);
                throw new ExtractException("ELI26190", ee.AsStringizedByteStream());
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
                ExtractException ee = ExtractException.AsExtractException("ELI26191", ex);
                throw new ExtractException("ELI26192", ee.AsStringizedByteStream());
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
                ExtractException ee = ExtractException.AsExtractException("ELI26193", ex);
                throw new ExtractException("ELI26194", ee.AsStringizedByteStream());
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
                ExtractException ee = ExtractException.AsExtractException("ELI26195", ex);
                throw new ExtractException("ELI26196", ee.AsStringizedByteStream());
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID Output Handler" COM category.
        /// </summary>
        /// <param name="type">The <see langref="type"/> being registered.</param>
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
        /// <param name="type">The <see langref="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        private static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.OutputHandlers);
        }

        /// <summary>
        /// Performs the mapping from tests to order grouping.
        /// </summary>
        /// <param name="attributes">A vector of attributes to map.</param>
        /// <param name="dbConnection">The database connection to use
        /// for querying.</param>
        private static List<IAttribute> MapOrders(IUnknownVector attributes,
            SqlCeConnection dbConnection)
        {
            // Get the source doc name from the first attribute
            string sourceDocName = "Unknown";
            if (attributes.Size() > 0)
            {
                sourceDocName = ((IAttribute)attributes.At(0)).Value.SourceDocName;
            }

            // Get a map of names to attributes from the attribute collection
            Dictionary<string, List<IAttribute>> nameToAttributes =
                GetMapOfNamesToAttributes(attributes);

            IAttribute dateAttribute = null;
            IAttribute timeAttribute = null;

            // Get the date and time from the attributes
            List<IAttribute> temp;
            if (nameToAttributes.TryGetValue("DATE", out temp))
            {
                // List should be size 1
                ExtractException.Assert("ELI26231", "Attribute list should only have 1 date",
                    temp.Count == 1, "Count Of Date Attributes", temp.Count);

                dateAttribute = temp[0];
            }
            temp = null;
            if (nameToAttributes.TryGetValue("TIME", out temp))
            {
                // List should be size 1
                ExtractException.Assert("ELI26232", "Attribute list should only have 1 time",
                    temp.Count == 1, "Count Of Time Attributes", temp.Count);

                timeAttribute = temp[0];
            }

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
                    List<IAttribute> unmatchedTests = new List<IAttribute>();
                    unmatchedTests.AddRange(pair.Value);

                    while (unmatchedTests.Count > 0)
                    {
                        Dictionary<IAttribute, Dictionary<string, string>> mapAttributeToOrderMatches =
                            new Dictionary<IAttribute, Dictionary<string, string>>();

                        // Now try to map to an order
                        Dictionary<string, int> countOrderMatches = new Dictionary<string, int>();
                        foreach (IAttribute attribute in unmatchedTests)
                        {
                            string testName = attribute.Value.String.ToUpperInvariant();
                            Dictionary<string, string> orderMatches =
                                GetOrderCodesFromTestName(testName, dbConnection);
                            mapAttributeToOrderMatches.Add(attribute, orderMatches);
                            foreach (string code in orderMatches.Keys)
                            {
                                int count;
                                if (countOrderMatches.TryGetValue(code, out count))
                                {
                                    count++;
                                    countOrderMatches[code] = count;
                                }
                                else
                                {
                                    count = 1;
                                    countOrderMatches.Add(code, count);
                                }
                            }
                        }

                        // Create a new IUnknown vector for the matched tests
                        IUnknownVector vecMatched = new IUnknownVector();
                        
                        // Add the date and time attribute (if avaible)
                        if (dateAttribute != null)
                        {
                            vecMatched.PushBack(dateAttribute);
                        }
                        if (timeAttribute != null)
                        {
                            vecMatched.PushBack(timeAttribute);
                        }

                        // Create the new order grouping attribute (default to UnknownOrder)
                        // and add it to the vector
                        AttributeClass orderGrouping = new AttributeClass();
                        orderGrouping.Name = "Order";
                        orderGrouping.Value.CreateNonSpatialString("UnknownOrder", sourceDocName);
                        vecMatched.PushBack(orderGrouping);

                        if (countOrderMatches.Count > 0)
                        {
                            // Now get the order code for the best match
                            KeyValuePair<string, int> bestMatch = new KeyValuePair<string, int>("", 0);
                            foreach (KeyValuePair<string, int> countPair in countOrderMatches)
                            {
                                if (countPair.Value > bestMatch.Value)
                                {
                                    bestMatch = countPair;
                                }
                            }

                            // Order code is bestMatch.key
                            string orderCode = bestMatch.Key;

                            List<IAttribute> testsToRemove = new List<IAttribute>();
                            foreach (IAttribute attribute in unmatchedTests)
                            {
                                Dictionary<string, string> codes;
                                if (mapAttributeToOrderMatches.TryGetValue(attribute, out codes))
                                {
                                    string testCode;
                                    if (codes.TryGetValue(orderCode, out testCode))
                                    {
                                        // Add the attribute to the list to remove
                                        testsToRemove.Add(attribute);

                                        // Replace the test name
                                        SpatialString value = attribute.Value;
                                        value.Replace(value.String,
                                            GetTestNameFromOrderAndTestCode(orderCode, testCode, dbConnection),
                                            false, 1, null);

                                        vecMatched.PushBack(attribute);
                                    }
                                }
                            }

                            // Remove the matched tests
                            foreach (IAttribute attribute in testsToRemove)
                            {
                                unmatchedTests.Remove(attribute);
                            }

                            // Store the order code
                            AttributeClass orderCodeAttribute = new AttributeClass();
                            orderCodeAttribute.Name = "Order Code";
                            orderCodeAttribute.Value.CreateNonSpatialString(orderCode, sourceDocName);

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
                            // Add all of the unmatched tests since there is no group for them
                            foreach (IAttribute attribute in unmatchedTests)
                            {
                                vecMatched.PushBack(attribute);
                            }

                            // Clear the vector of unmatched tests since they have now been
                            // "matched" with the unknown order
                            unmatchedTests.Clear();
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
        /// Builds a map of names to <see cref="List{T}"/> of attributes.
        /// </summary>
        /// <param name="attributes">The vector of attributes to group.</param>
        /// <returns>The map of names to attributes.</returns>
        private static Dictionary<string, List<IAttribute>> GetMapOfNamesToAttributes(
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
        private static KeyValuePair<string, string> GetOrderNameAndEpicCodeFromOrderCode(string orderCode,
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
        private static string GetTestNameFromOrderAndTestCode(string orderCode, string testCode,
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

        /// <summary>
        /// Gets a collection of order codes and test codes from a specified test name.
        /// The test name specified may be the actual test name or a known alternate
        /// test name.
        /// </summary>
        /// <param name="testName">The name of the test to search for.</param>
        /// <param name="dbConnection">The database connection to use.</param>
        /// <returns>A collection of order codes and test codes for the specified test name.
        /// </returns>
        private static Dictionary<string, string> GetOrderCodesFromTestName(string testName,
            SqlCeConnection dbConnection)
        {
            Dictionary<string, string> orderCodes = new Dictionary<string, string>();

            string query = "SELECT [OrderCode], [Code] FROM [Test] WHERE [Name] = '"
                + testName + "' OR [Code] = '" + testName + "'";

            using (SqlCeDataAdapter dataAdapter = new SqlCeDataAdapter(query, dbConnection))
            {
                using (DataTable dt = new DataTable())
                {
                    dt.Locale = CultureInfo.InvariantCulture;
                    dataAdapter.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        orderCodes.Add((string)row[0], (string)row[1]);
                    }
                }
            }

            // If a collection of order codes was found, just return the collection
            if (orderCodes.Count > 0)
            {
                return orderCodes;
            }

            // No order codes found, check the table of alternate test names
            query = "SELECT [OrderCode], [TestCode] FROM [AlternateTestName] WHERE [Name] = '"
                + testName + "'";

            using (SqlCeDataAdapter dataAdapter = new SqlCeDataAdapter(query, dbConnection))
            {
                using (DataTable dt = new DataTable())
                {
                    dt.Locale = CultureInfo.InvariantCulture;
                    dataAdapter.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        orderCodes.Add((string)row[0], (string)row[1]);
                    }
                }
            }

            return orderCodes;
        }

        #endregion Private Methods
    }
}
