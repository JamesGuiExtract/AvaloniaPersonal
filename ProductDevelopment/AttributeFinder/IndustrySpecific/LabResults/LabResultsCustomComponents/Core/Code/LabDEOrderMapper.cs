using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using UCLID_COMLMLib;
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
        /// </summary>
        static readonly int _CURRENT_VERSION = 1;

        /// <summary>
        /// The number of times to retry if failed connecting to the database file.
        /// </summary>
        static readonly int _DB_CONNECTION_RETRIES = 5;

        #endregion Constants

        #region LocalDatabaseCopy

        /// <summary>
        /// A class to manage temporary local copies of remote databases.
        /// </summary>
        class LocalDatabaseCopy : IDisposable
        {
            /// <summary>
            /// Used to store a local copy of the database if necessary.
            /// </summary>
            TemporaryFile _localTemporaryFile;

            /// <summary>
            /// The filename of the original database.
            /// </summary>
            string _originalDatabaseFileName;

            /// <summary>
            /// Keeps track of all <see cref="LabDEOrderMapper"/> instances referencing this copy.
            /// </summary>
            Dictionary<int, bool> _orderMapperReferences = new Dictionary<int, bool>();

            #region Constructors

            /// <summary>
            /// Initializes a new <see cref="LocalDatabaseCopy"/> instance.
            /// </summary>
            /// <param name="originalDatabaseFileName"></param>
            public LocalDatabaseCopy(string originalDatabaseFileName)
            {
                try
                {
                    _originalDatabaseFileName = originalDatabaseFileName;

                    // Use ConvertToNetworkPath to tell if the DB is being accesseed via a
                    // network share.
                    FileSystemMethods.ConvertToNetworkPath(ref _originalDatabaseFileName, false);

                    // [DataEntry:688]
                    // Whether or not the file is local, if it is being accessed via a network share
                    // a local copy must be used since SQL Compact does not support multiple
                    // connections via a network share.
                    if (_originalDatabaseFileName.StartsWith(@"\\", StringComparison.Ordinal))
                    {
                        _localTemporaryFile = new TemporaryFile();
                        File.Copy(originalDatabaseFileName, _localTemporaryFile.FileName, true);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27585", ex);
                }
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// The filename of the local database copy to use. This will be the original database
            /// name if the original database is not accessed via a network share (UNC path).
            /// </summary>
            /// <returns>The filename of the local database copy to use.</returns>
            public string FileName
            {
                get
                {
                    return (_localTemporaryFile == null) ? _originalDatabaseFileName :
                        _localTemporaryFile.FileName;
                }
            }

            #endregion Properties

            #region Methods

            /// <summary>
            /// Notifies the <see cref="LocalDatabaseCopy"/> of a <see cref="LabDEOrderMapper"/>
            /// instance that is referencing it.
            /// </summary>
            /// <param name="orderMapperInstance">The <see cref="LabDEOrderMapper"/> that is
            /// referencing the <see cref="LocalDatabaseCopy"/>.</param>
            public void AddReference(LabDEOrderMapper orderMapperInstance)
            {
                _orderMapperReferences[orderMapperInstance._instanceID] = true;
            }

            /// <summary>
            /// Notifies the <see cref="LocalDatabaseCopy"/> of a <see cref="LabDEOrderMapper"/>
            /// instance that is not longer referencing it.
            /// </summary>
            /// <param name="orderMapperInstance">The <see cref="LabDEOrderMapper"/> that is no
            /// longer referencing the <see cref="LocalDatabaseCopy"/>.</param>
            /// <returns><see langword="true"/> if there are no more <see cref="LabDEOrderMapper"/>
            /// instances referencing the <see cref="LocalDatabaseCopy"/>; <see langword="false"/>
            /// otherwise.</returns>
            public bool Dereference(LabDEOrderMapper orderMapperInstance)
            {
                _orderMapperReferences.Remove(orderMapperInstance._instanceID);
                return _orderMapperReferences.Count == 0;
            }

            #endregion Methods

            #region IDisposable Members

            /// <summary>
            /// Releases all resources used by the <see cref="LocalDatabaseCopy"/>.
            /// </summary>
            /// 
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <overloads>Releases resources used by the <see cref="LocalDatabaseCopy"/>.
            /// </overloads>
            /// <summary>
            /// Releases all unmanaged resources used by the <see cref="LocalDatabaseCopy"/>.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Dispose of managed objects
                    if (_localTemporaryFile != null)
                    {
                        _localTemporaryFile.Dispose();
                        _localTemporaryFile = null;
                    }
                }

                // Dispose of unmanaged resources
            }

            #endregion IDisposable Members
        }

        #endregion LocalDatabaseCopy

        #region Fields

        /// <summary>
        /// Object for mutexing local database copy creation
        /// </summary>
        static object _lock = new object();

        /// <summary>
        /// Keeps track of the local copy of each database to use. This will be the original
        /// database if it resides on the same machine or it will be a local copy if the original
        /// resides on a remote machine.
        /// </summary>
        static Dictionary<string, LocalDatabaseCopy> _localDatabaseCopies =
            new Dictionary<string, LocalDatabaseCopy>();

        /// <summary>
        /// The next OrderMapper instance ID
        /// </summary>
        static int _nextInstanceID;

        /// <summary>
        /// The ID of this OrderMapper instance
        /// </summary>
        int _instanceID;

        /// <summary>
        /// The name of the database file to use for order mapping.
        /// </summary>
        string _databaseFile;

        /// <summary>
        /// Flag to indicate whether this object is dirty or not.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </summary>
        public LabDEOrderMapper()
            : this(null)
        {
            _instanceID = _nextInstanceID++;
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
            SqlCeConnection dbConnection = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI26889", _DEFAULT_OUTPUT_HANDLER_NAME);

                // Get a database connection for processing (creating a local copy of the database
                // first, if necessary).
                dbConnection = GetDatabaseConnection(pDoc);

                // Open the database connection
                dbConnection.Open();

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
                    List<IAttribute> mappedAttributes =
                        MapOrders(testAttributes, dbConnection);

                    // Create an attribute sorter for sorting sub attributes
                    ISortCompare attributeSorter =
                                (ISortCompare)new SpatiallyCompareAttributesClass();

                    // Add each order to the output collection (sorting subattributes)
                    foreach (IAttribute newAttribute in mappedAttributes)
                    {
                        // Sort the sub attributes spatially
                        newAttribute.SubAttributes.Sort(attributeSorter);

                        // Add the attribute to the vector
                        newAttributes.PushBack(newAttribute);
                    }
                }

                // Finished with database so close connection
                dbConnection.Dispose();
                dbConnection = null;

                // Clear the original attributes and set the attributes to the
                // newly mapped collection
                pAttributes.Clear();
                pAttributes.CopyFrom(newAttributes);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI26171", "Unable to handle output.", ex);
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Dispose();
                    dbConnection = null;
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
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI26902", _DEFAULT_OUTPUT_HANDLER_NAME);

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
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.OutputHandlers);
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
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.OutputHandlers);
        }

        /// <summary>
        /// Builds a collection of order codes mapped to <see cref="LabOrder"/>s.
        /// </summary>
        /// <param name="dbConnection">The database connection to use to retrieve the data.</param>
        /// <returns>A collection of order codes mapped to <see cref="LabOrder"/>s.</returns>
        static Dictionary<string, LabOrder> FillLabOrderCollection(SqlCeConnection dbConnection)
        {
            string query = "SELECT [Code], [Name], [EpicCode] FROM [LabOrder] "
                + "WHERE [EpicCode] IS NOT NULL";
            Dictionary<string, LabOrder> orders = new Dictionary<string,LabOrder>();
            using (SqlCeCommand command = new SqlCeCommand(query, dbConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string code = reader.GetString(0);
                        string name = reader.GetString(1);
                        string epicCode = reader.GetString(2);
                        orders.Add(code, new LabOrder(code, name, epicCode, dbConnection));
                    }
                }
            }

            return orders;
        }

        /// <summary>
        /// Performs the mapping from tests to order grouping.
        /// </summary>
        /// <param name="tests">A list of tests to group.</param>
        /// <param name="dbConnection">The database connection to use
        /// for querying.</param>
        static List<IAttribute> MapOrders(List<IAttribute> tests, SqlCeConnection dbConnection)
        {
            // If there are no tests, just return an empty list
            if (tests.Count == 0)
            {
                return new List<IAttribute>();
            }

            // Get the source doc name from the first attribute
            string sourceDocName = "Unknown";
            sourceDocName = tests[0].Value.SourceDocName;

            // Build the list of all lab orders with their associated test collections.
            Dictionary<string, LabOrder> labOrders = FillLabOrderCollection(dbConnection);

            // Perform the first pass grouping of the tests
            List<IAttribute> firstPass = FirstPassGrouping(tests, dbConnection, labOrders,
                sourceDocName);

            // Perform the final grouping of the tests
            List<IAttribute> finalGroupings = GetFinalGrouping(firstPass, dbConnection,
                labOrders, sourceDocName);

            // Return the final groupings
            return finalGroupings;
        }

        /// <summary>
        /// Performs the first pass grouping for each group of TEST attributes.
        /// </summary>
        /// <param name="tests">The list of TEST attributes to group.</param>
        /// <param name="dbConnection">The database connection to use for the grouping.</param>
        /// <param name="labOrders">A collection mapping order codes to
        /// <see cref="LabOrder"/>s.</param>
        /// <param name="sourceDocName">The sourcedoc name to use in the grouping.</param>
        static List<IAttribute> FirstPassGrouping(List<IAttribute> tests, SqlCeConnection dbConnection,
            Dictionary<string, LabOrder> labOrders, string sourceDocName)
        {
            List<IAttribute> firstPassMapping = new List<IAttribute>();

            foreach (IAttribute attribute in tests)
            {
                IUnknownVector attributes = attribute.SubAttributes;

                // Get a map of names to attributes from the attribute collection
                Dictionary<string, List<IAttribute>> nameToAttributes =
                    GetMapOfNamesToAttributes(attributes);

                // Build a list of all attributes that are not in the components category
                List<IAttribute> components = null;
                List<IAttribute> nonComponents = new List<IAttribute>();
                foreach (KeyValuePair<string, List<IAttribute>> pair in nameToAttributes)
                {
                    if (pair.Key.Equals("COMPONENT", StringComparison.OrdinalIgnoreCase))
                    {
                        components = pair.Value;
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
                        // Create a new IUnknown vector for the matched tests
                        IUnknownVector vecMatched = new IUnknownVector();

                        // Add all the noncomponent attributes to the sub attributes
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
                        KeyValuePair<string, List<LabTest>> matchedTests =
                            FindBestOrder(unmatchedTests, labOrders, dbConnection, true);

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
        /// <param name="dbConnection">The database connection to use for querying data.</param>
        /// <param name="labOrders">The collection of lab order codes to <see cref="LabOrder"/>
        /// objects.</param>
        /// <param name="sourceDocName">The source document name to be used when creating new
        /// spatial strings.</param>
        /// <returns>A new collection of attributes that represent the second order
        /// grouping attempt.</returns>
        static List<IAttribute> GetFinalGrouping(List<IAttribute> firstPassGrouping,
            SqlCeConnection dbConnection, Dictionary<string, LabOrder> labOrders,
            string sourceDocName)
        {
            List<IAttribute> finalGrouping = new List<IAttribute>();

            List<OrderGrouping> firstPass = new List<OrderGrouping>(firstPassGrouping.Count);
            foreach (IAttribute attribute in firstPassGrouping)
            {
                if (attribute.Name.Equals("Test", StringComparison.OrdinalIgnoreCase))
                {
                    firstPass.Add(new OrderGrouping(attribute, labOrders));
                }
                else
                {
                    finalGrouping.Add(attribute);
                }
            }

            // Now compare each item in the first pass grouping to see if it can be grouped
            // with another item in the grouping
            for (int i=0; i < firstPass.Count; i++)
            {
                OrderGrouping group1 = firstPass[i];
                List<LabTest> group1List = new List<LabTest>(group1.LabTests);
                for (int j=i+1; j < firstPass.Count; j++)
                {
                    OrderGrouping group2 = firstPass[j];

                    // Check collection dates
                    if (!OrderGrouping.CollectionDatesEqual(group1, group2))
                    {
                        // Collection dates don't match, cannot group these orders
                        continue;
                    }

                    List<LabTest> group2List = new List<LabTest>(group2.LabTests);
                    group2List.AddRange(group1List);

                    // Build a new list of tests by combining the two lists
                    bool unique = true;
                    Dictionary<string, LabTest> labTests = new Dictionary<string, LabTest>();
                    foreach (LabTest test in group2List)
                    {
                        string name = test.Name.ToUpperInvariant();
                        if (labTests.ContainsKey(name))
                        {
                            // The test was already contained in the list, these two groups
                            // cannot be combined, just break from the inner loop
                            unique = false;
                            break;
                        }

                        labTests.Add(name, test);
                    }

                    // There were duplicate tests, these orders cannot be combined
                    if (!unique)
                    {
                        continue;
                    }

                    // Groups could potentially be merged, find the best order (do not
                    // require mandatory tests in the second pass grouping)
                    KeyValuePair<string, List<LabTest>> bestOrder =
                        FindBestOrder(group2List, labOrders, dbConnection, false);

                    // In order to group into a new combined order, the resulting order
                    // must contain all of the tests from the list
                    if (group2List.Count == bestOrder.Value.Count)
                    {
                        // Update the attributes of group1 with the attributes in group2
                        group1.InsertNameAttributeMap(group2);

                        OrderGrouping groupCombined =
                            CreateNewOrderGrouping(bestOrder.Key, labOrders, sourceDocName, group1);

                        // Remove the combined groups from the list
                        firstPass.RemoveAt(j);
                        firstPass.RemoveAt(i);

                        // Add the combined group to the list at the location of the
                        // first group [DataEntry #817]
                        firstPass.Insert(i, groupCombined);

                        // Set the index back to -1 so it will be incremented to 0
                        i = -1;

                        // Break from the inner loop so that we can begin the grouping
                        // over again with the new firstPass list
                        break;
                    }
                }
            }

            // Check for any order groupings that are "UnknownOrder"
            // and attempt to map them to an order [DE #833]
            MapSingleUnknownOrders(firstPass, labOrders, dbConnection, sourceDocName);

            // firstPass should now contain all groupings that could be combined
            // update the test names to their official name
            foreach (OrderGrouping orderGroup in firstPass)
            {
                orderGroup.UpdateLabTestsToOfficialName(dbConnection);

                // Add the mapped group to the final grouping
                finalGrouping.Add(orderGroup.Attribute);
            }

            return finalGrouping;
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
            OrderGrouping newGroup = new OrderGrouping(attribute);
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
        /// <param name="dbConnection">The database connection to use for querying data.</param>
        /// <param name="sourceDocName">The source doc name for the new attributes.</param>
        static void MapSingleUnknownOrders(List<OrderGrouping> orderGroups,
            Dictionary<string, LabOrder> labOrders, SqlCeConnection dbConnection,
            string sourceDocName)
        {
            for (int i = 0; i < orderGroups.Count; i++)
            {
                OrderGrouping group = orderGroups[i];
                if (group.LabOrder == null && group.LabTests.Count == 1)
                {
                    // Try to map this unknown order
                    KeyValuePair<string, List<LabTest>> bestMatch =
                        FindBestOrder(new List<LabTest>(group.LabTests),
                        labOrders, dbConnection, false);

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
            foreach(IAttribute attribute in tests)
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
        /// <param name="labOrders">A collection mapping order codes to
        /// <see cref="LabOrder"/>s</param>
        /// <param name="dbConnection">The database connection to use for data queries.</param>
        /// <param name="requireMandatory">If <see langword="true"/> then will only map to an
        /// order if all mandatory tests are present.  If <see langword="false"/> will map to
        /// the order with the most tests, even if all mandatory are not present.</param>
        /// <returns>A pair containing the order code and a list of matched tests.</returns>
        static KeyValuePair<string, List<LabTest>> FindBestOrder(List<LabTest> unmatchedTests,
            Dictionary<string, LabOrder> labOrders, SqlCeConnection dbConnection,
            bool requireMandatory)
        {
            // Variables to hold the best match seen thus far (will be modified as the
            // best matching algorithm does its work
            int bestMatchCount = 0;
            int bestMandatoryCount = 0;
            string bestOrderId = "UnknownOrder";
            List<LabTest> bestMatchedTests = new List<LabTest>();

            // Check to see if the first test is part of any valid order
            List<string> potentialOrderCodes =
                GetPotentialOrderCodes(unmatchedTests[0].Name, dbConnection);

            // Loop through the potential orders attempting to match the order
            foreach (string orderCode in potentialOrderCodes)
            {
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
                if (!requireMandatory || labOrder.ContainsAllMandatoryTests(unmatchedTests))
                {
                    List<LabTest> matchedTests = labOrder.GetMatchingTests(unmatchedTests);

                    // Best match if more tests matched OR equal test count and the mandatory test
                    // count is smaller
                    if (matchedTests.Count > bestMatchCount
                        || (matchedTests.Count == bestMatchCount && labOrder.MandatoryCount < bestMandatoryCount))
                    {
                        bestMatchCount = matchedTests.Count;
                        bestMatchedTests = matchedTests;
                        bestOrderId = labOrder.OrderCode;
                        bestMandatoryCount = labOrder.MandatoryCount;
                    }
                }
            }

            // No best order match was found, add this test to its own order
            if (bestMatchCount == 0)
            {
                bestMatchedTests.Add(unmatchedTests[0]);
            }

            return new KeyValuePair<string,List<LabTest>>(bestOrderId, bestMatchedTests);
        }

        /// <summary>
        /// Gets the list of potential order codes for the specified test.
        /// </summary>
        /// <param name="testName">The test to get the order codes for.</param>
        /// <param name="dbConnection">The database connection to use.</param>
        /// <returns>A list of potential order codes (or an empty list if no potential orders).
        /// </returns>
        static List<string> GetPotentialOrderCodes(string testName, SqlCeConnection dbConnection)
        {
            // Escape the single quote
            testName = testName.Replace("'", "''");

            List<string> orderCodes = new List<string>();
            string query = "SELECT DISTINCT [OrderCode] FROM [LabOrderTest] INNER JOIN "
                + "[LabTest] ON [LabOrderTest].[TestCode] = [LabTest].[TestCode] "
                + "INNER JOIN [AlternateTestName] ON [AlternateTestName].[TestCode] = "
                + "[LabOrderTest].[TestCode] WHERE [AlternateTestName].[Name] = '"
                + testName + "' OR [LabTest].[OfficialName] = '" + testName + "'";

            using (SqlCeCommand command = new SqlCeCommand(query, dbConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        orderCodes.Add(reader.GetString(0));
                    }
                }
            }

            return orderCodes;
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
        /// <param name="dbConnection">The database connection to use.</param>
        /// <returns>The test name for the specified order code and test code.</returns>
        internal static string GetTestNameFromTestCode(string testCode, SqlCeConnection dbConnection)
        {
            // Escape the single quote
            string testCodeEscaped = testCode.Replace("'", "''");

            string query = "SELECT [OfficialName] FROM [LabTest] WHERE [TestCode] = '"
                + testCodeEscaped + "'";

            using (SqlCeCommand command = new SqlCeCommand(query, dbConnection))
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
        /// Gets the database connection to use for processing, creating a local copy if necessary.
        /// </summary>
        /// <param name="pDoc">The document object.</param>
        /// <returns>The <see cref="SqlCeConnection"/> to use for processing.</returns>
        SqlCeConnection GetDatabaseConnection(AFDocument pDoc)
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

                // Lock to ensure multiple copies of the same database aren't created.
                LocalDatabaseCopy localDatabaseCopy = null;
                lock (_lock)
                {
                    // If there is not an existing LocalDatabaseCopy instance available for the
                    // specified database, create a new one.                    
                    if (!_localDatabaseCopies.TryGetValue(databaseFile, out localDatabaseCopy) ||
                        !File.Exists(localDatabaseCopy.FileName))
                    {
                        localDatabaseCopy = new LocalDatabaseCopy(databaseFile);
                        _localDatabaseCopies[databaseFile] = localDatabaseCopy;
                    }
                    else
                    {
                        // Otherwise, reference the existing LocalDatabaseCopy instance.
                        localDatabaseCopy.AddReference(this);
                    }
                }

                // Build the connection string
                string connectionString = "Data Source='" + localDatabaseCopy.FileName + "';";

                // Try to open the database connection, if there is a sqlce exception,
                // just increment retry count, sleep, and try again
                int retryCount = 0;
                Exception tempEx = null;
                SqlCeConnection dbConnection = null;
                while (dbConnection == null && retryCount < _DB_CONNECTION_RETRIES)
                {
                    try
                    {
                        dbConnection = new SqlCeConnection(connectionString);
                    }
                    catch (SqlCeException ex)
                    {
                        tempEx = ex;
                        retryCount++;
                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        throw ExtractException.AsExtractException("ELI26651", ex);
                    }
                }

                // If all the retries failed and the connection is still null, throw an exception
                if (retryCount >= _DB_CONNECTION_RETRIES && dbConnection == null)
                {
                    ExtractException ee = new ExtractException("ELI26652",
                        "Unable to open database connection!", tempEx);
                    ee.AddDebugData("Retries", retryCount, false);
                    throw ee;
                }

                return dbConnection;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27743", 
                    "Failed to obtain a database connection!", ex);
                ee.AddDebugData("Database", _databaseFile, false);
                throw ee;
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
                // Remove any existing reference to a local database copy. Dispose of the
                // local database copy if this was the last instance referencing it.
                LocalDatabaseCopy localDatabaseCopy;
                if (_localDatabaseCopies.TryGetValue(_databaseFile, out localDatabaseCopy))
                {
                    if (localDatabaseCopy.Dereference(this))
                    {
                        _localDatabaseCopies.Remove(_databaseFile);
                        localDatabaseCopy.Dispose();
                    }
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion
    }
}
