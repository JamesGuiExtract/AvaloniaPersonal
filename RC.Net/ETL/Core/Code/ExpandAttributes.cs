using Extract.AttributeFinder;
using Extract.Code.Attributes;
using Extract.SqlDatabase;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.ETL
{
    /// <summary>
    /// Database service to expand attributes using voa saved in AttributeSetForFile table
    /// </summary>
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [ExtractCategory("DatabaseService", "Expand attributes")]
    public class ExpandAttributes : DatabaseService, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        #region DashboardAttributeField class definition

        /// <summary>
        /// Class that contains the definition of the fields for the dashboard attribute
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class DashboardAttributeField : INotifyPropertyChanged
        {
            #region DashboardAttributeField Fields

            string _dashboardAttributeName;
            Int64 _attributeSetNameID;
            string _pathForAttributeInAttributeSet;

            #endregion

            #region DashboardAttributeField Properties

            /// <summary>
            /// Name for the Dashboard attribute field
            /// </summary>
            public string DashboardAttributeName
            {
                get { return _dashboardAttributeName; }
                set
                {
                    if (value != _dashboardAttributeName)
                    {
                        _dashboardAttributeName = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            /// <summary>
            /// Attribute set name id for the Set the dashboard attribute is to be extracted from
            /// </summary>
            public Int64 AttributeSetNameID
            {
                get { return _attributeSetNameID; }
                set
                {
                    if (value != _attributeSetNameID)
                    {
                        _attributeSetNameID = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            /// <summary>
            /// Path to extract the attribute from the attribute set
            /// </summary>
            public string PathForAttributeInAttributeSet
            {
                get { return _pathForAttributeInAttributeSet; }
                set
                {
                    if (value != _pathForAttributeInAttributeSet)
                    {
                        _pathForAttributeInAttributeSet = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            #endregion

            #region DashboardAttributeField Events

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            #region DashboardAttributeField Event handlers

            /// <summary>
            /// Called by each of the property Set accessors when property changes
            /// </summary>
            /// <param name="propertyName">Name of the property changed</param>
            protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion

            #region Overrides

            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "{0},{1},{2}", DashboardAttributeName, AttributeSetNameID, PathForAttributeInAttributeSet);
            }

            public override bool Equals(object obj)
            {
                DashboardAttributeField d = obj as DashboardAttributeField;
                if (d is null)
                {
                    return false;
                }

                return d.ToString() == ToString();
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            #endregion

            #region DashboardAttributefield Methods

            /// <summary>
            /// converts a string to a DashboardAttributeField
            /// </summary>
            /// <param name="s">String formatted as DashboardAttributeName,AttributeSetNameID,PathForAttributeInAttributeSet
            /// this is the same format returned by ToString()</param>
            /// <returns>New instance of DashboardAttributeField</returns>
            internal static DashboardAttributeField FromString(string s)
            {
                var tokens = s.Split(new char[] { ' ', ',' });
                if (tokens.Count() < 3)
                {
                    ExtractException ee = new ExtractException("ELI46113", "Unable to convert string to DashboardAttributeField");
                    ee.AddDebugData("string", s, false);
                    throw ee;
                }
                return new DashboardAttributeField()
                {
                    DashboardAttributeName = tokens[0],
                    AttributeSetNameID = Int64.Parse(tokens[1], CultureInfo.InvariantCulture),
                    PathForAttributeInAttributeSet = tokens[2]
                };
            }

            #endregion

        }

        #endregion

        #region Constants

        /// <summary>
        /// Size of the batch to process in each transaction
        /// </summary>
        const int _BATCH_SIZE = 10;

        /// <summary>
        /// String used to create the add attributes sql
        /// Requires setting a parameters on command 
        ///     @TypeName NVARCHAR(255) 
        ///     @AttributeName NVARCHAR(255)
        ///     @AttributeSetForFileID BIGINT
        ///     @Value NVARCHAR(MAX)
        ///     @ParentAttributeID BIGINT
        ///     @GUID UNIQUEIDENTIFIER
        ///     
        /// This also has a {0} specifier and is expected to be used in string.Format.
        /// The {0} is the query for adding raster zones since that is an optional component
        /// </summary>
        readonly string AddAttributeQuery = @"
                DECLARE @AttributeNameID AS BIGINT
                DECLARE @AttributeID AS BIGINT
                DECLARE @EndPos INT
                DECLARE @StartPos INT
                DECLARE @TempType TABLE ( [type] nvarchar(255))

                IF (@TypeName = '')
                BEGIN
                    INSERT INTO @TempType ([type])
                    VALUES (@TypeName)
                END ELSE BEGIN
                    SELECT @StartPos = 1, @EndPos = CHARINDEX('+', @TypeName)
                    WHILE (@StartPos < LEN (@TypeName) + 1)
                    BEGIN
                        IF (@EndPos = 0)
                            SET @EndPos = LEN(@TypeName) + 1
                        INSERT INTO @TempType 
                        SELECT SUBSTRING(@TypeName, @StartPos, @EndPos - @StartPos)

                        SET @StartPos = @EndPos + 1
                        SET @EndPos = CHARINDEX('+', @TypeName, @StartPos)
                    END
                END

                INSERT INTO AttributeType([Type])
                SELECT [Type] FROM @TempType 
                WHERE [Type] NOT IN (SELECT [Type] FROM AttributeType);

                SELECT @AttributeNameID = ID FROM [AttributeName]
                WHERE [Name] = @AttributeName;

                IF (@AttributeNameID IS NULL)
                BEGIN
                    INSERT INTO [AttributeName]([Name])
                    VALUES (@AttributeName)
                
                    SELECT @AttributeNameID = ID FROM [AttributeName]
                    WHERE [Name] = @AttributeName             
                END;

                DECLARE @AttributeIDTable TABLE (AttributeID BIGINT)

                INSERT INTO [dbo].[Attribute]
                    ([AttributeSetForFileID]
                    ,[AttributeNameID]
                    ,[Value]
                    ,[ParentAttributeID]
                    ,[GUID])
                OUTPUT INSERTED.ID INTO @AttributeIDTable
                VALUES
                    ( @AttributeSetForFileID,
                      @AttributeNameID,
                      @Value,
                      @ParentAttributeID,
                      @GUID);
            
                SELECT @AttributeID = AttributeID FROM @AttributeIDTable;

                INSERT INTO [dbo].[AttributeInstanceType]([AttributeID], [AttributeTypeID])
                SELECT @AttributeID, [ID] FROM AttributeType 
                WHERE [Type] IN (SELECT [Type] FROM @TempType);                

                {0}

                SELECT @AttributeID AS AttributeID
            
            ";

        /// <summary>
        /// Query that adds new values to the DashboardAttributeFields table
        /// Requires 3 parameters
        ///     @DashboardNameForAttribute - Name that is used to identify the value in the dashboard
        ///     @AttributeSetForFileID - the AttributeSetForFileID being processed
        ///     @DashboardAttributeValue - Value for the DashbaordAttribute
        /// </summary>
        static readonly string _AddDashboardAttribute = @"
                DELETE FROM DashboardAttributeFields
                WHERE AttributeSetForFileID = @AttributeSetForFileID AND [Name] = @DashboardNameForAttribute

                INSERT INTO DashboardAttributeFields (
                	AttributeSetForFileID
                	,[Name]
                	,[Value]
                )
                VALUES (
                    @AttributeSetForFileID
                	,@DashboardNameForAttribute
                	,@DashboardAttributeValue
                )
            ";

        #endregion

        #region Fields

        /// <summary>
        /// Current version
        /// </summary>
        const int CURRENT_VERSION = 2;

        /// <summary>
        /// Indicates whether the Process method is currently executing.
        /// </summary>
        bool _processing;

        /// <summary>
        /// Status
        /// </summary>
        ExpandAttributesStatus _status;

        /// <summary>
        /// Cancel token - will be set in Process to CancellationToken passed in
        /// </summary>
        CancellationToken _cancelToken = CancellationToken.None;

        #endregion Fields

        #region ExpandAttributes Properties

        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;

        /// <summary>
        /// Indicates if SpatialInfo (RasterZones) should be saved
        /// </summary>
        [DataMember]
        public bool StoreSpatialInfo { get; set; } = true;

        /// <summary>
        /// Indicates if empty attributes should be saved
        /// </summary>
        [DataMember]
        public bool StoreEmptyAttributes { get; set; } = false;

        /// <summary>
        /// List of Dashboard attributes to be saved to DashboardAttributeFields
        /// </summary>
        [DataMember]
        public BindingList<DashboardAttributeField> DashboardAttributes { get; } = new BindingList<DashboardAttributeField>();

        #endregion

        #region DatabaseService implementation

        #region DatabaseService Properties

        /// <summary>
        /// Gets a value indicating whether this instance is processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processing; otherwise, <c>false</c>.
        /// </value>
        public override bool Processing
        {
            get
            {
                return _processing;
            }
        }

        #endregion DatabaseService Properties

        #region DatabaseService Methods


        /// <summary>
        /// Performs the process of expanding the VOA stored in AttributeSetForFile
        /// to the attribute tables if they are not already stored.
        /// </summary>
        /// <param name="cancelToken">Token that can cancel the processing</param>
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                _cancelToken = cancelToken;

                RefreshStatus();
                ExtractException.Assert("ELI46585", "Status cannot be null", _status != null);

                int maxFileTaskSession = MaxReportableFileTaskSessionId(true);
                int currentLastProcessed = _status.CalculateLastFileTaskSessionIDFullyProcessed();

                // check if there is anything to do
                if (currentLastProcessed >= maxFileTaskSession)
                {
                    return;
                }

                while (currentLastProcessed < maxFileTaskSession)
                {
                    int lastInBatch = Math.Min(currentLastProcessed + _BATCH_SIZE, maxFileTaskSession);

                    using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                    connection.Open();

                    // Process each batch
                    using (var scope = new TransactionScope(TransactionScopeOption.Required,
                        new TransactionOptions()
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                            Timeout = TransactionManager.MaximumTimeout,
                        },
                        TransactionScopeAsyncFlowOption.Enabled))
                    {
                        using (var readerTask = GetAttributeSetsToPopulate(connection, currentLastProcessed, lastInBatch, cancelToken))
                        using (SqlDataReader VOAsToStore = readerTask.Result)
                        {
                            ProcessBatch(connection, VOAsToStore, cancelToken);
                        }

                        scope.Complete();
                    }

                    currentLastProcessed = lastInBatch;
                    // Since there may be FileTaskSessions that have nothing to do with attributes update all the 
                    // status items to have maxFileTaskSession since all processing is complete at this point
                    _status.LastFileTaskSessionIDProcessed = Math.Max(lastInBatch, _status.LastFileTaskSessionIDProcessed);
                    _status.LastIDProcessedForDashboardAttribute.Keys
                        .ToList()
                        .ForEach(k => _status.LastIDProcessedForDashboardAttribute[k] =
                                     Math.Max(lastInBatch, _status.LastIDProcessedForDashboardAttribute[k]));
                    SaveStatus(connection);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45425");
            }
            finally
            {
                _processing = false;
            }
        }

        /// <summary>
        /// Gets the attribute sets to populate.
        /// </summary>
        /// <param name="connection">The <see cref="SqlAppRoleConnection"/> to use.</param>
        /// <param name="currentLastProcessed">The last processed file task session.</param>
        /// <param name="lastInBatch">The last file task session in this batch.</param>
        /// <param name="cancelToken">The cancel token that should be used to abort this
        /// query if processing is cancelled.</param>
        static Task<SqlDataReader> GetAttributeSetsToPopulate(SqlAppRoleConnection connection, int currentLastProcessed, int lastInBatch, CancellationToken cancelToken)
        {
            // Records that contain attributes that need to be stored
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT [ASFF].[ID] [AttributeSetForFileID],
	                        CASE WHEN EXISTS (SELECT TOP 1 [ID] FROM [Attribute] WHERE [AttributeSetForFileID] = [ASFF].[ID]) THEN 1 ELSE 0 END
		                        [HasExpandedAttributes],
	                        [FileTaskSessionID], 
	                        [AttributeSetNameID], 
	                        [VOA]
	                    FROM [dbo].[AttributeSetForFile] [ASFF]
                        WHERE [FileTaskSessionID] > @LastFileTaskSessionIDProcessed
		                    AND [FileTaskSessionID] <= @LastFileTaskSessionInBatch";

                cmd.Parameters.AddWithValue("@LastFileTaskSessionIDProcessed", currentLastProcessed);
                cmd.Parameters.AddWithValue("@LastFileTaskSessionInBatch", lastInBatch);

                // Set the timeout so that it waits indefinitely
                cmd.CommandTimeout = 0;

                var readerTask = cmd.ExecuteReaderAsync(cancelToken);

                return readerTask;
            }
        }

        /// <summary>
        /// Process a batch of VOA's from the database
        /// </summary>
        /// <param name="connection">The <see cref="SqlAppRoleConnection"/> to use.</param>
        /// <param name="VOAsToStore"><see cref="SqlDataReader"/> that contains the records to process</param>
        /// <param name="cancelToken"><see cref="CancellationToken"/> that could cancel the operation</param>
        void ProcessBatch(SqlAppRoleConnection connection, SqlDataReader VOAsToStore, CancellationToken cancelToken)
        {
            // Get the ordinals needed
            int attributeSetForFileIDColumn = VOAsToStore.GetOrdinal("AttributeSetForFileID");
            int hasExpandedAttributesColulmn = VOAsToStore.GetOrdinal("HasExpandedAttributes");
            int fileTaskSessionIDColumn = VOAsToStore.GetOrdinal("FileTaskSessionID");
            int attributeSetNameIDColumn = VOAsToStore.GetOrdinal("AttributeSetNameID");
            int voaColumn = VOAsToStore.GetOrdinal("VOA");

            // As we can't use MARS on database connections established via application role
            // authentication, we need to compile as list of attribute sets to be processed
            // rather processing each set as they are read.
            List<(Int64 attributeSetForFileID,
                bool hasExpandedAttributes,
                Int32 fileTaskSessionID,
                Int64 attributeSetNameID,
                List<DashboardAttributeField> dashboardAttributesNeeded,
                byte[] voaData)> attributeSetDataList = new();

            while (VOAsToStore.Read())
            {
                cancelToken.ThrowIfCancellationRequested();
                bool hasExpandedAttributes = (VOAsToStore.GetInt32(hasExpandedAttributesColulmn) == 1);
                Int32 fileTaskSessionID = VOAsToStore.GetInt32(fileTaskSessionIDColumn);
                Int64 attributeSetNameID = VOAsToStore.GetInt64(attributeSetNameIDColumn);

                // Check for DashboardAttribute records that need to be processed
                var dashboardAttributesNeeded = _status.LastIDProcessedForDashboardAttribute
                    .Select(status => (LastFTSID: status.Value, DashboardAttribute: DashboardAttributeField.FromString(status.Key)))
                    .Where(status2 => status2.LastFTSID < fileTaskSessionID && status2.DashboardAttribute.AttributeSetNameID == attributeSetNameID)
                    .Select(status2 => status2.DashboardAttribute);

                if (!hasExpandedAttributes || dashboardAttributesNeeded.Any())
                {
                    using var voaDataStream = VOAsToStore.GetStream(voaColumn);
                    using MemoryStream voaMemoryStream = new();
                    voaDataStream.CopyTo(voaMemoryStream);

                    attributeSetDataList.Add(
                        (attributeSetForFileID: VOAsToStore.GetInt64(attributeSetForFileIDColumn),
                        hasExpandedAttributes: hasExpandedAttributes,
                        fileTaskSessionID: fileTaskSessionID,
                        attributeSetNameID: attributeSetNameID,
                        dashboardAttributesNeeded: dashboardAttributesNeeded.ToList(),
                        voaData: voaMemoryStream.ToArray()));
                }
            }

            VOAsToStore.Close();

            foreach (var attributeSetData in attributeSetDataList)
            {
                // Get the VOAs from the stream
                IUnknownVector AttributesToStore = AttributeMethods.GetVectorOfAttributesFromSqlBinary(attributeSetData.voaData);
                AttributesToStore.ReportMemoryUsage();

                if (!attributeSetData.hasExpandedAttributes 
                    && _status.LastFileTaskSessionIDProcessed < attributeSetData.fileTaskSessionID)
                {
                    try
                    {
                        addAttributes(connection, AttributesToStore, attributeSetData.attributeSetForFileID, cancelToken);
                    }
                    catch (Exception ex)
                    {
                        throw ex.AsExtract("ELI45429");
                    }

                    _status.LastFileTaskSessionIDProcessed = attributeSetData.fileTaskSessionID;
                }

                if (attributeSetData.dashboardAttributesNeeded.Any())
                {
                    XPathContext pathContext = new XPathContext(AttributesToStore);

                    // Update the DashboardAttributeFields table
                    foreach (var da in attributeSetData.dashboardAttributesNeeded)
                    {
                        string valueToSave = GetValueForDashboardAttributeField(da, pathContext);

                        using var cmd = connection.CreateCommand();
                        cmd.CommandText = _AddDashboardAttribute;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@DashboardNameForAttribute", da.DashboardAttributeName);
                        cmd.Parameters.AddWithValue("@AttributeSetForFileID", attributeSetData.attributeSetForFileID);
                        cmd.Parameters.AddWithValue("@DashboardAttributeValue", valueToSave);
                        var task = cmd.ExecuteNonQueryAsync();
                        task.Wait(_cancelToken);
                    }
                }
            }
        }

        /// <summary>
        /// Get the value for a dashboard field
        /// </summary>
        /// <param name="dashboardAttributeField"><see cref="DashboardAttributeField"/> that has the configuration for 
        /// the attribute being saved</param>
        /// <param name="pathContext"><see cref="XPathContext"/> for the VOA that is being processed</param>
        [CLSCompliant(false)]
        public static string GetValueForDashboardAttributeField(DashboardAttributeField dashboardAttributeField, XPathContext pathContext)
        {
            XPathContext.XPathIterator startAt = pathContext.GetIterator("/*");
            startAt.MoveNext();

            string path = dashboardAttributeField.PathForAttributeInAttributeSet;

            object resultOfPath = pathContext.Evaluate(startAt, path);
            if (resultOfPath is List<object> objectList)
            {
                resultOfPath = objectList.FirstOrDefault();
            }

            string value;
            if (resultOfPath is IAttribute attr)
            {
                value = attr.Value.String;
            }
            else if (resultOfPath is null)
            {
                value = "UNKNOWN";
            }
            else
            {
                value = resultOfPath.ToString();
            }

            return value;
        }

        #endregion

        #endregion

        #region IConfigSettings implementation

        /// <summary>
        /// Method returns the state of the configuration
        /// </summary>
        /// <returns>Returns <see langword="true"/> if configuration is valid, otherwise false</returns>
        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(Description);
        }

        /// <summary>
        /// Displays a form to Configures the ExpandAttributes service
        /// </summary>
        /// <returns><see langword="true"/> if configuration was ok'd. if configuration was canceled returns 
        /// <see langword="false"/></returns>
        public bool Configure()
        {
            try
            {
                ExpandAttributesForm form = new ExpandAttributesForm(this);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45646");
                return false;
            }
        }

        #endregion

        #region IHasConfigurableDatabaseServiceStatus

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public DatabaseServiceStatus Status
        {
            get => _status = _status ?? GetLastOrCreateStatus(() => new ExpandAttributesStatus()
            {
                LastFileTaskSessionIDProcessed = -1
            });

            set => _status = value as ExpandAttributesStatus;
        }

        /// <summary>
        /// Refreshes the <see cref="DatabaseServiceStatus"/> by loading from the database, creating a new instance,
        /// or setting it to null (if <see cref="DatabaseServiceID"/>, <see cref="DatabaseServer"/> and
        /// <see cref="DatabaseName"/> are not configured)
        /// </summary>
        public void RefreshStatus()
        {
            try
            {
                if (DatabaseServiceID > 0
                    && !string.IsNullOrEmpty(DatabaseServer)
                    && !string.IsNullOrEmpty(DatabaseName))
                {
                    _status = GetLastOrCreateStatus(() => new ExpandAttributesStatus());

                    UpdateDashboardAttributesStatusItems();
                }
                else
                {
                    _status = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46107");
            }
        }

        #endregion IHasConfigurableDatabaseServiceStatus

        #region Private Methods

        /// <summary>
        /// Checks for DashboardAttributes that have been added or removed and updates the status records and removes
        /// the DashboardAttributes in the database for any that are no longer being added
        /// </summary>
        void UpdateDashboardAttributesStatusItems()
        {
            var itemsToAdd = DashboardAttributes
                .Where(s => !_status.LastIDProcessedForDashboardAttribute.ContainsKey(s.ToString())).ToList();

            var itemsToDelete = _status.LastIDProcessedForDashboardAttribute.Keys
                .Where(k => !DashboardAttributes.Contains(DashboardAttributeField.FromString(k))).ToList();

            if (itemsToAdd.Count() > 0 || itemsToDelete.Count() > 0)
            {
                // Add New items
                foreach (var a in itemsToAdd)
                {
                    _status.LastIDProcessedForDashboardAttribute.TryAdd(a.ToString(), -1);
                }
                foreach (var d in itemsToDelete)
                {
                    using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                    connection.Open();

                    using var scope = new TransactionScope(TransactionScopeOption.Required,
                        new TransactionOptions()
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                            Timeout = TransactionManager.MaximumTimeout
                        },
                        TransactionScopeAsyncFlowOption.Enabled);
                        
                    using var cmd = connection.CreateCommand();

                    DashboardAttributeField dashboardAttributeField = DashboardAttributeField.FromString(d);
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = String.Format(CultureInfo.InvariantCulture,
                        @"DELETE FROM DashboardAttributeFields
                                                WHERE [Name] = '{0}' AND [AttributeSetForFileID] = {1}",
                        dashboardAttributeField.DashboardAttributeName, dashboardAttributeField.AttributeSetNameID);
                    var task = cmd.ExecuteNonQueryAsync();
                    task.Wait(_cancelToken);
                    _status.LastIDProcessedForDashboardAttribute.Remove(d);
                    SaveStatus(connection);
                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// Called after this instance is deserialized.
        /// </summary>
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (Version > CURRENT_VERSION)
            {
                ExtractException ee = new ExtractException("ELI45426", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                throw ee;
            }

            Version = CURRENT_VERSION;
        }

        /// <summary>
        /// Adds the IUnknownVector of attributes to the database attribute related tables
        /// </summary>
        /// <param name="connection">The open connection for the add</param>
        /// <param name="attributes">The attributes to add</param>
        /// <param name="attributeSetForFileID">The ID of the AttributeSetForFileID record that contains the VOA being added</param>
        /// <param name="parentAttributeID">The ID of the parent Attribute record. if 0 it is a top level attribute</param>
        void addAttributes(SqlAppRoleConnection connection, IUnknownVector attributes,
            Int64 attributeSetForFileID, CancellationToken cancelToken,
            Int64 parentAttributeID = 0)
        {
            foreach (var attribute in attributes.ToIEnumerable<IAttribute>())
            {
                cancelToken.ThrowIfCancellationRequested();
                if (!StoreEmptyAttributes && AttributeIsEmpty(attribute))
                {
                    continue;
                }

                using (var insertCmd = connection.CreateCommand())
                {
                    insertCmd.CommandText = string.Format(CultureInfo.InvariantCulture,
                        AddAttributeQuery,
                        (StoreSpatialInfo) ? buildRasterZoneInsert(attribute.Value) : string.Empty);

                    try
                    {
                        var idObject = attribute as IIdentifiableObject;

                        insertCmd.Parameters.Add("@TypeName", SqlDbType.NVarChar, 255).Value = attribute.Type;
                        insertCmd.Parameters.Add("@AttributeName", SqlDbType.NVarChar, 255).Value = attribute.Name;
                        insertCmd.Parameters.Add("@AttributeSetForFileID", SqlDbType.BigInt).Value = attributeSetForFileID;
                        insertCmd.Parameters.Add("@Value", SqlDbType.NVarChar).Value = attribute.Value.String;

                        // if the parentAttributeID is 0 a null value needs to be added to the database
                        if (parentAttributeID == 0)
                        {
                            insertCmd.Parameters.Add("@ParentAttributeID", SqlDbType.BigInt).Value = DBNull.Value;
                        }
                        else
                        {
                            insertCmd.Parameters.Add("@ParentAttributeID", SqlDbType.BigInt).Value = parentAttributeID;
                        }
                        insertCmd.Parameters.Add("@GUID", SqlDbType.UniqueIdentifier).Value = idObject.InstanceGUID;

                        var insertTask = insertCmd.ExecuteScalarAsync(_cancelToken);
                        Int64? attributeID = insertTask.Result as Int64?;
                        if (!(attributeID is null) && !(attribute.SubAttributes is null))
                        {
                            addAttributes(connection, attribute.SubAttributes, attributeSetForFileID, cancelToken, (Int64)attributeID);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtractException ee = new ExtractException("ELI45433", "Unable to add Attribute", ex);
                        ee.AddDebugData("SQL", insertCmd.CommandText, false);
                        throw ee;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the SQL statement to add the RasterZones to the RasterZone table for the given SpatialString
        /// </summary>
        /// <param name="spatialString">SpatialString whose RasterZones are being added</param>
        /// <returns>Insert statement to add RasterZones</returns>
        static string buildRasterZoneInsert(SpatialString spatialString)
        {
            if (!spatialString.HasSpatialInfo())
            {
                return string.Empty;
            }

            var rasterZones = spatialString.GetOriginalImageRasterZones().ToIEnumerable<RasterZone>();
            List<string> inserts = new List<string>();

            foreach (var zoneList in rasterZones.Batch(1000))
            {
                var valueStrings = zoneList.Select(rz => getRasterZoneValueClause(rz));
                inserts.Add(string.Format(CultureInfo.InvariantCulture,
                    @"INSERT INTO [dbo].[RasterZone]
                       ([AttributeID]
                       ,[Top]
                       ,[Left]
                       ,[Bottom]
                       ,[Right]
                       ,[StartX]
                       ,[StartY]
                       ,[EndX]
                       ,[EndY]
                       ,[PageNumber]
                       ,[Height])
                 VALUES
                    {0};", string.Join(",\r\n", valueStrings)));
            }
            return string.Join(";\r\n", inserts);
        }

        /// <summary>
        /// Builds a string to be used in a SQL VALUE clause for the inserting to the RasterZone tables
        /// </summary>
        /// <param name="rasterZone">The RasterZone to use</param>
        /// <returns>String to be used in VALUE clause</returns>
        static string getRasterZoneValueClause(IRasterZone rasterZone)
        {
            ILongRectangle rectangle = rasterZone.GetRectangularBounds(null);
            int top = rectangle.Top;
            int left = rectangle.Left;
            int bottom = rectangle.Bottom;
            int right = rectangle.Right;
            int startX = rasterZone.StartX;
            int startY = rasterZone.StartY;
            int endX = rasterZone.EndX;
            int endY = rasterZone.EndY;
            int pageNumber = rasterZone.PageNumber;
            int height = rasterZone.Height;

            string insert = string.Format(CultureInfo.InvariantCulture,
                "(@AttributeID, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})",
                                               top,
                                               left,
                                               bottom,
                                               right,
                                               startX,
                                               startY,
                                               endX,
                                               endY,
                                               pageNumber,
                                               height);
            return insert;
        }

        /// <summary>
        /// Tests if the given attribute is empty
        /// </summary>
        /// <param name="attribute">Attribute to check for empty</param>
        /// <returns>true if attribute is empty otherwise false</returns>
        bool AttributeIsEmpty(IAttribute attribute)
        {
            if (StoreSpatialInfo && attribute.Value.HasSpatialInfo())
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(attribute.Type))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(attribute.Value.String))
            {
                return false;
            }

            foreach (var a in attribute.SubAttributes.ToIEnumerable<IAttribute>())
            {
                if (!AttributeIsEmpty(a))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Saves the current <see cref="DatabaseServiceStatus"/> to the DB
        /// </summary>
        void SaveStatus(SqlAppRoleConnection connection)
        {
            SaveStatus(connection, _status);
        }
        #endregion

        #region Private Classes

        /// <summary>
        /// Class for the ExpandAttributesStatus stored in the DatabaseService record
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        [DataContract]
        public class ExpandAttributesStatus : DatabaseServiceStatus, IFileTaskSessionServiceStatus
        {
            // Changed Version to 2 because of bug that caused this to use CURRENT_VERSION from the parent class
            const int _CURRENT_VERSION = 2;

            [DataMember]
            public override int Version { get; protected set; } = _CURRENT_VERSION;

            /// <summary>
            /// The ID of the last FileTaskSession record processed
            /// </summary>
            public Int32 LastFileTaskSessionIDProcessed { get; set; }

            /// <summary>
            /// Dictionary contains the last ID processed for each of the defined attributes
            /// </summary>
            [DataMember]
            public Dictionary<string, Int32> LastIDProcessedForDashboardAttribute { get; } =
                new Dictionary<string, Int32>();

            /// <summary>
            /// Called after this instance is deserialized.
            /// </summary>
            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {
                if (Version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI46106", "Settings were saved with a newer version.");
                    ee.AddDebugData("SavedVersion", Version, false);
                    ee.AddDebugData("CurrentVersion", _CURRENT_VERSION, false);
                    throw ee;
                }

                Version = _CURRENT_VERSION;
            }

            /// <summary>
            /// Method to return the last fully processed FileTaskSession ID.
            /// </summary>
            /// <returns>The last FileTaskSessionID that has been completely processed</returns>
            public int CalculateLastFileTaskSessionIDFullyProcessed()
            {
                try
                {
                    return (LastIDProcessedForDashboardAttribute.Count() > 0) ?
                        Math.Min(LastFileTaskSessionIDProcessed, LastIDProcessedForDashboardAttribute.Values.Min()) :
                        LastFileTaskSessionIDProcessed;

                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI46423");
                }
            }
        }

        #endregion

    }
}
