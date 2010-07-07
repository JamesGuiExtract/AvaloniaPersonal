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
        /// Version 2: Added support for configuring whether mandatory tests are required in
        /// the second pass of the order mapping algorithm.
        /// </summary>
        static readonly int _CURRENT_VERSION = 2;

        /// <summary>
        /// The number of times to retry if failed connecting to the database file.
        /// </summary>
        static readonly int _DB_CONNECTION_RETRIES = 5;

        /// <summary>
        /// The algorithm to find the best possible groupings in GetFinalGrouping is NP-complete.
        /// To prevent a runaway computation from exhausting all memory, the algorithm will be
        /// capped at this many possible grouping combinations to be considering at one time.
        /// </summary>
        static readonly int _COMBINATION_ALGORITHM_SAFETY_CUTOFF = 5000;

        #endregion Constants

        #region Internal Classes

        #region LocalDatabaseCopy

        /// <summary>
        /// A class to manage temporary local copies of remote databases.
        /// <para><b>Note</b></para>
        /// This class is only thread-safe when always accessed within a lock.
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
            /// The last time the original DB was modified.
            /// </summary>
            DateTime _lastDBModificationTime;

            /// <summary>
            /// Keeps track of the DB copy each <see cref="LabDEOrderMapper"/> instance is
            /// referencing.
            /// </summary>
            Dictionary<int, TemporaryFile> _temporaryFileReferences =
                new Dictionary<int, TemporaryFile>();

            /// <summary>
            /// Keeps track of all <see cref="LabDEOrderMapper"/> instances referencing each local
            /// DB copy.
            /// </summary>
            Dictionary<TemporaryFile, List<int>> _orderMapperReferences =
                new Dictionary<TemporaryFile, List<int>>();

            #region Constructors

            /// <summary>
            /// Initializes a new <see cref="LocalDatabaseCopy"/> instance.
            /// </summary>
            /// <param name="orderMapperInstance">The <see cref="LabDEOrderMapper"/> that is
            /// creating/referencing the <see cref="LocalDatabaseCopy"/>.</param>
            /// <param name="originalDatabaseFileName">The name of the source database. This
            /// database will be used directly only if it is not being accessed via a network share.
            /// </param>
            public LocalDatabaseCopy(LabDEOrderMapper orderMapperInstance,
                string originalDatabaseFileName)
            {
                try
                {
                    _originalDatabaseFileName = originalDatabaseFileName;

                    // [DataEntry:399, 688, 986]
                    // Whether or not the file is accessed via a network share, create and use a
                    // local copy. Though multiple connections are allowed to a local file, the
                    // connections cannot see each other's changes.
                    _lastDBModificationTime = File.GetLastWriteTime(_originalDatabaseFileName);

                    _localTemporaryFile = new TemporaryFile();
                    File.Copy(originalDatabaseFileName, _localTemporaryFile.FileName, true);

                    AddReference(orderMapperInstance);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27585", ex);
                }
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            /// The filename of the local database copy to use. This will be the original database
            /// name if the original database is not accessed via a network share (UNC path).
            /// <para><b>Note</b></para>
            /// If the original database file has been updated since the local copy was last created
            /// or updated, a new local copy created from the updated original and the path of the
            /// new local copy will be returned.
            /// </summary>
            /// <param name="orderMapperInstance">The <see cref="LabDEOrderMapper"/> instance for
            /// which the path to the local database copy is needed. The database at the path
            /// specified is guaranteed to exist unmodified until the next call to GetFileName from
            /// the specified instance. (or until this <see cref="LocalDatabaseCopy"/> instance is
            /// disposed)</param>
            /// <returns>The filename of the local database copy to use.</returns>
            public string GetFileName(LabDEOrderMapper orderMapperInstance)
            {
                try
                {
                    DateTime dbModificationTime = File.GetLastWriteTime(_originalDatabaseFileName);

                    // If the original DB has been modified, copy it to a new temporary file.
                    if (dbModificationTime != _lastDBModificationTime)
                    {
                        _localTemporaryFile = new TemporaryFile();

                        _lastDBModificationTime = dbModificationTime;
                        File.Copy(_originalDatabaseFileName, _localTemporaryFile.FileName, true);
                    }
                    
                    // Update the reference for the specified orderMapperInstance so that it
                    // references the new _localTemporaryFile and not the now outdated
                    // temporary file (the outdated one will be deleted if this is the last
                    // instance that was referencing it).
                    UpdateReference(orderMapperInstance);

                    return _localTemporaryFile.FileName;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30120", ex);
                }
            }

            /// <summary>
            /// Notifies the <see cref="LocalDatabaseCopy"/> of a <see cref="LabDEOrderMapper"/>
            /// instance that is referencing it.
            /// </summary>
            /// <param name="orderMapperInstance">The <see cref="LabDEOrderMapper"/> that is
            /// referencing the <see cref="LocalDatabaseCopy"/>.</param>
            public void AddReference(LabDEOrderMapper orderMapperInstance)
            {
                _temporaryFileReferences[orderMapperInstance._instanceID] = _localTemporaryFile;
                
                List<int> fileReferences;
                if (!_orderMapperReferences.TryGetValue(_localTemporaryFile, out fileReferences))
                {
                    fileReferences = new List<int>(new int[] { orderMapperInstance._instanceID });
                    _orderMapperReferences[_localTemporaryFile] = fileReferences;
                }
                else if (!fileReferences.Contains(orderMapperInstance._instanceID))
                {
                    fileReferences.Add(orderMapperInstance._instanceID);
                }
            }

            /// <overloads>Notifies the <see cref="LocalDatabaseCopy"/> of a
            /// <see cref="LabDEOrderMapper"/> instance that is not longer referencing it
            /// </overloads>
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
                return Dereference(orderMapperInstance._instanceID);
            }

            /// <summary>
            /// Notifies the <see cref="LocalDatabaseCopy"/> of a <see cref="LabDEOrderMapper"/>
            /// instance that is not longer referencing it.
            /// </summary>
            /// <param name="orderMapperReferenceID">The ID of the <see cref="LabDEOrderMapper"/>
            /// that is no longer referencing the <see cref="LocalDatabaseCopy"/>.</param>
            /// <returns><see langword="true"/> if there are no more <see cref="LabDEOrderMapper"/>
            /// instances referencing the <see cref="LocalDatabaseCopy"/>; <see langword="false"/>
            /// otherwise.</returns>
            bool Dereference(int orderMapperReferenceID)
            {
                TemporaryFile temporaryFile;
                if (_temporaryFileReferences.TryGetValue(orderMapperReferenceID, out temporaryFile))
                {
                    // If the orderMapperInstance still references an existing temporary file,
                    // remove the reference.
                    _temporaryFileReferences.Remove(orderMapperReferenceID);

                    List<int> orderMapperReferences = _orderMapperReferences[temporaryFile];
                    orderMapperReferences.Remove(orderMapperReferenceID);

                    // If no other references are found for this temporary file, the temporary file
                    // can be disposed of.
                    if (orderMapperReferences.Count == 0)
                    {
                        if (_localTemporaryFile == temporaryFile)
                        {
                            _localTemporaryFile = null;
                        }

                        _orderMapperReferences.Remove(temporaryFile);
                        temporaryFile.Dispose();
                    }
                }
                
                return _temporaryFileReferences.Count == 0;
            }

            /// <summary>
            /// Ensures the specified <see paramref="orderMapperInstance"/> is referencing the
            /// current local DB copy. Removes references to old DB copies if necessary.
            /// </summary>
            /// <param name="orderMapperInstance">The <see cref="LabDEOrderMapper"/> instance for
            /// which local DB reference need to be updated.</param>
            void UpdateReference(LabDEOrderMapper orderMapperInstance)
            {
                TemporaryFile temporaryFile;
                if (!_temporaryFileReferences.TryGetValue(
                    orderMapperInstance._instanceID, out temporaryFile))
                {
                    AddReference(orderMapperInstance);
                }
                else if (temporaryFile != _localTemporaryFile)
                {
                    Dereference(orderMapperInstance);
                    AddReference(orderMapperInstance);
                }
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
                    List<int> orderMapperInstanceIDs = new List<int>(_temporaryFileReferences.Keys);
                    foreach (int orderMapperInstanceID in orderMapperInstanceIDs)
                    {
                        Dereference(orderMapperInstanceID);
                    }

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
            SqlCeConnection _dbConnection;

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
            public Dictionary<string, LabTest> ContainedTests = new Dictionary<string, LabTest>();

            /// <summary>
            /// Initializes a new instance of the <see cref="OrderGroupingPermutation"/>
            /// class.
            /// </summary>
            /// <param name="originalGroup">The original order group for this permutation (before
            /// any merging with other orders has taken place).</param>
            /// <param name="dbConnection">The database connection to use when performing the
            /// order mapping.</param>
            /// <param name="sourceDocName">The sourcedoc name to use in the grouping.</param>
            public OrderGroupingPermutation(OrderGrouping originalGroup,
                SqlCeConnection dbConnection, string sourceDocName)
            {
                // The initial combined group contains only the original group
                CombinedGroup = originalGroup;
                ContainedGroups.Add(originalGroup);

                // Initialize the dictionary of contained tests
                foreach (LabTest test in originalGroup.LabTests)
                {
                    ContainedTests[test.Name.ToUpperInvariant()] = test;
                }

                // Store the other data needed to perform mappings/mergings
                _dbConnection = dbConnection;
                _sourceDocName = sourceDocName;
            }

            /// <summary>
            /// Attempts to merge the specfied <see cref="OrderGrouping"/> with the
            /// currently contained order groupings to form a bigger order.
            /// </summary>
            /// <param name="newGroup">The <see cref="OrderGrouping"/> to
            /// merge with the current grouping.</param>
            /// <param name="labOrders">The collection of lab order codes to
            /// <see cref="LabOrder"/> objects.</param>
            /// <returns>A new merged <see cref="OrderGroupingPermutation"/>
            /// or <see langword="null"/> if no merging can take place.</returns>
            public OrderGroupingPermutation AttemptMerge(OrderGrouping newGroup,
                Dictionary<string, LabOrder> labOrders)
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
                    new Dictionary<string, LabTest>(ContainedTests);
                foreach (LabTest test in newGroup.LabTests)
                {
                    string name = test.Name.ToUpperInvariant();
                    if (combinedTests.ContainsKey(name))
                    {
                        // The test was already contained in the permutation, these two groups
                        // cannot be combined.
                        return null;
                    }

                    combinedTests[name] = test;
                }

                // Groups could potentially be merged, find the best order
                KeyValuePair<string, List<LabTest>> bestOrder =
                    FindBestOrder(new List<LabTest>(combinedTests.Values),
                        labOrders, _dbConnection, false);

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
                    new OrderGroupingPermutation(combinedGroup, _dbConnection, _sourceDocName);
                newGroupingPermutation.ContainedTests = combinedTests;
                newGroupingPermutation.ContainedGroups = new List<OrderGrouping>(ContainedGroups);
                newGroupingPermutation.ContainedGroups.Add(newGroup);

                return newGroupingPermutation;
            }
        }

        #endregion OrderGroupingPermutation

        #endregion Internal Classes

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

        /// <summary>
        /// Whether mandatory tests are required for the second pass of the order mapping algorithm.
        /// </summary>
        bool _requireMandatory;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </summary>
        public LabDEOrderMapper()
            : this(null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </summary>
        /// <param name="databaseFile">The name of the database file to attach to.</param>
        /// <param name="requireMandatory">Whether or not mandatory tests are required
        /// in the second pass of the order mapping algorithm.</param>
        public LabDEOrderMapper(string databaseFile, bool requireMandatory)
        {
            try
            {
                _instanceID = _nextInstanceID++;
                _databaseFile = databaseFile;
                _requireMandatory = requireMandatory;
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
                        MapOrders(testAttributes, dbConnection, _requireMandatory);

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
                LabDEOrderMapper newMapper = new LabDEOrderMapper(_databaseFile, _requireMandatory);

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
                        // Read the require manadatory setting
                        _requireMandatory = reader.ReadBoolean();
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
                    new LabDEOrderMapperConfigurationForm(_databaseFile, _requireMandatory))
                {
                    // If the user clicked OK then set the database file
                    if (configureForm.ShowDialog() == DialogResult.OK)
                    {
                        _databaseFile = configureForm.DatabaseFileName;
                        _requireMandatory = configureForm.RequireMandatoryTests;
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
            string query = "SELECT [Code], [Name], [EpicCode], [TieBreaker] FROM [LabOrder] "
                + "WHERE [EpicCode] IS NOT NULL";
            Dictionary<string, LabOrder> orders = new Dictionary<string, LabOrder>();
            using (SqlCeCommand command = new SqlCeCommand(query, dbConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string code = reader.GetString(0);
                        string name = reader.GetString(1);
                        string epicCode = reader.GetString(2);
                        string tieBreaker = reader.GetString(3);
                        orders.Add(code,
                            new LabOrder(code, name, epicCode, tieBreaker, dbConnection));
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
        /// <param name="requireMandatory">Whether or not mandatory tests are required
        /// in the second pass of the order mapping algorithm.</param>
        static List<IAttribute> MapOrders(List<IAttribute> tests, SqlCeConnection dbConnection,
            bool requireMandatory)
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
                labOrders, sourceDocName, requireMandatory);

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
        /// <param name="requireMandatory">Whether mandatory tests are required when creating
        /// the final groupings.</param>
        /// <returns>A new collection of attributes that represent the second order
        /// grouping attempt.</returns>
        static List<IAttribute> GetFinalGrouping(List<IAttribute> firstPassGrouping,
            SqlCeConnection dbConnection, Dictionary<string, LabOrder> labOrders,
            string sourceDocName, bool requireMandatory)
        {
            List<OrderGrouping> bestGroups = new List<OrderGrouping>();
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

            while (firstPass.Count > 0)
            {
                OrderGroupingPermutation first = new OrderGroupingPermutation(firstPass[0],
                    dbConnection, sourceDocName);
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
                            temp.AttemptMerge(firstPass[j], labOrders);

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
                    GetBestGrouping(possibleGroupings, requireMandatory) ?? first;
                bestGroups.Add(bestGrouping.CombinedGroup);
                foreach (OrderGrouping group in bestGrouping.ContainedGroups)
                {
                    firstPass.Remove(group);
                }
            }

            // Check for any order groupings that are "UnknownOrder"
            // and attempt to map them to an order [DE #833]
            MapSingleUnknownOrders(
                bestGroups, labOrders, dbConnection, sourceDocName, requireMandatory);

            // firstPass should now contain all groupings that could be combined
            // update the test names to their official name
            foreach (OrderGrouping orderGroup in bestGroups)
            {
                orderGroup.UpdateLabTestsToOfficialName(dbConnection);

                // Add the mapped group to the final grouping
                finalGrouping.Add(orderGroup.Attribute);
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
        /// <returns>The "Best" (see note in summary) order grouping from the list of possible
        /// groupings.</returns>
        static OrderGroupingPermutation GetBestGrouping(
            List<OrderGroupingPermutation> possibleGroupings, bool requireMandatory)
        {
            int bestGroupCount = 0;
            string bestTieBreakerString = "";
            OrderGroupingPermutation bestGroup = null;
            foreach (OrderGroupingPermutation group in possibleGroupings)
            {
                if (requireMandatory && group.CombinedGroup.LabOrder != null && 
                    !group.CombinedGroup.ContainsAllMandatoryTests())
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
        /// <param name="requireMandatory">If <see langword="true"/> then will only map to an
        /// order if all mandatory tests are present.</param>
        static void MapSingleUnknownOrders(List<OrderGrouping> orderGroups,
            Dictionary<string, LabOrder> labOrders, SqlCeConnection dbConnection,
            string sourceDocName, bool requireMandatory)
        {
            for (int i = 0; i < orderGroups.Count; i++)
            {
                OrderGrouping group = orderGroups[i];
                if (group.LabOrder == null && group.LabTests.Count == 1)
                {
                    // Try to map this unknown order
                    KeyValuePair<string, List<LabTest>> bestMatch =
                        FindBestOrder(new List<LabTest>(group.LabTests),
                        labOrders, dbConnection, requireMandatory);

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
            string bestTieBreakerString = "";
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

            // No best order match was found, add this test to its own order
            if (bestMatchCount == 0)
            {
                bestMatchedTests.Add(unmatchedTests[0]);
            }

            return new KeyValuePair<string, List<LabTest>>(bestOrderId, bestMatchedTests);
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

            // Create a dictionary to hold the potential codes
            object temp = new object();
            Dictionary<string, object> orderCodes = new Dictionary<string, object>();

            // Query the official name table for potential order codes
            string query = "SELECT DISTINCT [OrderCode] FROM [LabOrderTest] INNER JOIN "
                + "[LabTest] ON [LabOrderTest].[TestCode] = [LabTest].[TestCode] "
                + "WHERE [LabTest].[OfficialName] = '" + testName + "'";
            using (SqlCeCommand command = new SqlCeCommand(query, dbConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string code = reader.GetString(0);
                        if (!orderCodes.ContainsKey(code))
                        {
                            orderCodes.Add(code, temp);
                        }
                    }
                }
            }

            // Query the alternate test table for potential order codes
            query = "SELECT DISTINCT [OrderCode] FROM [LabOrderTest] INNER JOIN "
                + "[AlternateTestName] ON [LabOrderTest].[TestCode] = [AlternateTestName].[TestCode] "
                + "WHERE [AlternateTestName].[Name] = '" + testName + "'";
            using (SqlCeCommand command = new SqlCeCommand(query, dbConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string code = reader.GetString(0);
                        if (!orderCodes.ContainsKey(code))
                        {
                            orderCodes.Add(code, temp);
                        }
                    }
                }
            }

            return new List<string>(orderCodes.Keys);
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
                string connectionString;
                lock (_lock)
                {
                    LocalDatabaseCopy localDatabaseCopy;

                    // If there is not an existing LocalDatabaseCopy instance available for the
                    // specified database, create a new one.                    
                    if (!_localDatabaseCopies.TryGetValue(databaseFile, out localDatabaseCopy))
                    {
                        localDatabaseCopy = new LocalDatabaseCopy(this, databaseFile);
                        _localDatabaseCopies[databaseFile] = localDatabaseCopy;
                    }

                    // Build the connection string
                    connectionString = "Data Source='" + localDatabaseCopy.GetFileName(this) + "';";
                }

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
                lock (_lock)
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
            }

            // Dispose of unmanaged resources
        }

        #endregion
    }
}
