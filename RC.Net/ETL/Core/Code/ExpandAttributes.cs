using Extract.AttributeFinder;
using Extract.Code.Attributes;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
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
            public static DashboardAttributeField FromString(string s)
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
                    AttributeSetNameID = Int64.Parse(tokens[1]),
                    PathForAttributeInAttributeSet = tokens[2]
                };
            }

            #endregion

        }

        #endregion

        #region Constants

        /// <summary>
        /// Size of the batch to process in each transaction
        /// 
        /// Note: First used 100 but TransactionScope timed out - set to 1 and seems to work fine
        /// </summary>
        const int _BATCH_SIZE = 1;

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
        readonly string _AddDashboardAttribute = @"
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
        public BindingList<DashboardAttributeField> DashboardAttributes = new BindingList<DashboardAttributeField>();

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

                int maxFileTaskSession = MaxReportableFileTaskSessionId();
                int currentLastProcessed = _status.StartingFileTaskSessionId();

                // check if there is anything to do
                if (currentLastProcessed >= maxFileTaskSession)
                {
                    return;
                }

                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    while (currentLastProcessed < maxFileTaskSession)
                    {
                        int lastInBatch = Math.Min(currentLastProcessed + _BATCH_SIZE, maxFileTaskSession);

                        // Records that contain attributes that need to be stored
                        SqlCommand cmd = connection.CreateCommand();
                        cmd.CommandText = @"
                            SELECT ID AttributeSetForFileID
                                    ,FileTaskSessionID
                                    ,AttributeSetNameID
                                    ,VOA
                                FROM [dbo].[AttributeSetForFile] 
                                WHERE FileTaskSessionID > @FirstFileTaskSessionInBatch AND
		                            FileTaskSessionID <= @LastfileTaskSessionInBatch
                                ORDER BY FileTaskSessionID";

                        cmd.Parameters.AddWithValue("@FirstFileTaskSessionInBatch", currentLastProcessed);
                        cmd.Parameters.AddWithValue("@LastfileTaskSessionInBatch", lastInBatch);

                        // Set the timeout so that it waits indefinitely
                        cmd.CommandTimeout = 0;

                        var readerTask = cmd.ExecuteReaderAsync(cancelToken);

                        // Process each batch
                        using (SqlDataReader VOAsToStore = readerTask.Result)
                        using (var scope = new TransactionScope(TransactionScopeOption.Required,
                            new TransactionOptions()
                            {
                                IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                                Timeout = TransactionManager.MaximumTimeout
                            },
                            TransactionScopeAsyncFlowOption.Enabled))
                        {


                            ProcessBatch(VOAsToStore, cancelToken);

                            SaveStatus();
                            scope.Complete();

                        }
                        currentLastProcessed = lastInBatch;
                    }

                    // Since there may be FileTaskSessions that have nothing to do with attributes update all the 
                    // status items to have maxFileTaskSession since all processing is complete at this point
                    _status.LastFileTaskSessionIDProcessed = maxFileTaskSession;
                    _status.LastIDProcessedForDashboardAttribute.Keys
                        .ToList()
                        .ForEach(k => _status.LastIDProcessedForDashboardAttribute[k] = maxFileTaskSession);
                    SaveStatus();
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
        /// Process a batch of VOA's from the database
        /// </summary>
        /// <param name="VOAsToStore"><see cref="SqlDataReader"/> that contains the records to process</param>
        /// <param name="cancelToken"><see cref="CancellationToken"/> that could cancel the operation</param>
        void ProcessBatch(SqlDataReader VOAsToStore, CancellationToken cancelToken)
        {
            // Get the ordinals needed
            int AttributeSetForFileIDColumn = VOAsToStore.GetOrdinal("AttributeSetForFileID");
            int FileTaskSessionIDColumn = VOAsToStore.GetOrdinal("FileTaskSessionID");
            int AttributeSetNameIDColumn = VOAsToStore.GetOrdinal("AttributeSetNameID");
            int VOAColumn = VOAsToStore.GetOrdinal("VOA");

            while (VOAsToStore.Read())
            {
                cancelToken.ThrowIfCancellationRequested();
                Int64 AttributeSetForFileID = VOAsToStore.GetInt64(AttributeSetForFileIDColumn);
                Int32 FileTaskSessionID = VOAsToStore.GetInt32(FileTaskSessionIDColumn);
                Int64 AttributeSetNameID = VOAsToStore.GetInt64(AttributeSetNameIDColumn);

                using (Stream VOAStream = VOAsToStore.GetStream(VOAColumn))
                {
                    // Get the VOAs from the stream
                    IUnknownVector AttributesToStore = AttributeMethods.GetVectorOfAttributesFromSqlBinary(VOAStream);
                    if (_status.LastFileTaskSessionIDProcessed < FileTaskSessionID)
                    {
                        using (var deleteConnection = NewSqlDBConnection())
                        {
                            deleteConnection.Open();
                            using (var deleteCmd = deleteConnection.CreateCommand())
                            {
                                deleteCmd.CommandTimeout = 0;
                                deleteCmd.CommandText = @"
                                                    DELETE FROM Attribute
                                                    WHERE AttributeSetForFileID = @AttributeSetForFileID
                                                ";
                                deleteCmd.Parameters.AddWithValue("@AttributeSetForFileID", AttributeSetForFileID);
                                var deleteTask = deleteCmd.ExecuteNonQueryAsync();
                                deleteTask.Wait(cancelToken);
                            }
                        }

                        // Use separate connection so that the entire VOA can be added in a transaction
                        using (var saveConnection = NewSqlDBConnection())
                        {
                            saveConnection.Open();

                            try
                            {
                                addAttributes(saveConnection, AttributesToStore, AttributeSetForFileID);
                            }
                            catch (Exception ex)
                            {
                                throw ex.AsExtract("ELI45429");
                            }
                        }
                        _status.LastFileTaskSessionIDProcessed = FileTaskSessionID;
                    }

                    // Check for DashboardAttribute records that need to be processed
                    var needToProcess = _status.LastIDProcessedForDashboardAttribute
                        .Select(d => d.Value < FileTaskSessionID);
                    if (needToProcess.Count() > 0)
                    {
                        XPathContext pathContext = new XPathContext(AttributesToStore);

                        // Update the DashboardAttributeFields table
                        foreach (var da in DashboardAttributes)
                        {
                            ProccessDashboardAttributeFields(da, AttributeSetForFileID, pathContext);
                            _status.LastIDProcessedForDashboardAttribute[da.ToString()] = FileTaskSessionID;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Save the DashboardAttribute field
        /// </summary>
        /// <param name="dashboardAttributeField"><see cref="DashboardAttributeField"/> that has the configuration for 
        /// the attribute being saved</param>
        /// <param name="attributeSetForfileID">The ID for the AttributeSetForFile record for this attribute</param>
        /// <param name="pathContext"><see cref="XPathContext"/> for the VOA that is being processed</param>
        void ProccessDashboardAttributeFields(DashboardAttributeField dashboardAttributeField, Int64 attributeSetForfileID, XPathContext pathContext)
        {
            string path = "/root/" + dashboardAttributeField.PathForAttributeInAttributeSet.Replace(@"\", "/");
            var attributesWithPath = pathContext.FindAllOfType<IAttribute>(path);
            var firstAttribute = attributesWithPath.FirstOrDefault();

            string valueToSave = firstAttribute?.Value?.String ?? "UNKNOWN";

            using (var connection = NewSqlDBConnection())
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = _AddDashboardAttribute;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@DashboardNameForAttribute", dashboardAttributeField.DashboardAttributeName);
                cmd.Parameters.AddWithValue("@AttributeSetForFileID", attributeSetForfileID);
                cmd.Parameters.AddWithValue("@DashboardAttributeValue", valueToSave);

                var task = cmd.ExecuteNonQueryAsync();
                task.Wait(_cancelToken);
            }
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
            get => _status ?? new ExpandAttributesStatus
            {
                LastFileTaskSessionIDProcessed = -1
            };

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
                .Where(s => !_status.LastIDProcessedForDashboardAttribute.ContainsKey(s.ToString()));

            var itemsToDelete = _status.LastIDProcessedForDashboardAttribute.Keys
                .Where(k => !DashboardAttributes.Contains(DashboardAttributeField.FromString(k)));

            if (itemsToAdd.Count() > 0 || itemsToDelete.Count() > 0)
            {
                // Add New items
                foreach (var a in itemsToAdd)
                {
                    _status.LastIDProcessedForDashboardAttribute.TryAdd(a.ToString(), -1);
                }

                foreach (var d in itemsToDelete)
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.Required,
                        new TransactionOptions()
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                            Timeout = TransactionManager.MaximumTimeout
                        },
                        TransactionScopeAsyncFlowOption.Enabled))
                    {
                        _status.LastIDProcessedForDashboardAttribute.Remove(d);
                        using (var connection = NewSqlDBConnection())
                        {
                            connection.Open();
                            using (var cmd = connection.CreateCommand())
                            {
                                DashboardAttributeField dashboardAttributeField = DashboardAttributeField.FromString(d);
                                cmd.CommandTimeout = 0;
                                cmd.CommandText = String.Format(CultureInfo.InvariantCulture,
                                    @"DELETE FROM DashboardAttributeFields
                                                WHERE [Name] = '{0}' AND [AttributeSetForFileID = {1}",
                                    dashboardAttributeField.DashboardAttributeName, dashboardAttributeField.AttributeSetNameID);
                                var task = cmd.ExecuteNonQueryAsync();
                                task.Wait(_cancelToken);
                            }
                        }
                        SaveStatus();
                        scope.Complete();
                    }
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
        void addAttributes(SqlConnection connection, IUnknownVector attributes,
            Int64 attributeSetForFileID,
            Int64 parentAttributeID = 0)
        {
            foreach (var attribute in attributes.ToIEnumerable<IAttribute>())
            {
                if (!StoreEmptyAttributes && AttributeIsEmpty(attribute))
                {
                    continue;
                }

                var insertCmd = connection.CreateCommand();

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
                        addAttributes(connection, attribute.SubAttributes, attributeSetForFileID, (Int64)attributeID);
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

            var valueStrings = rasterZones.Select(rz => getRasterZoneValueClause(rz));

            return string.Format(CultureInfo.InvariantCulture,
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
                    {0};", string.Join(",\r\n", valueStrings));
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
        void SaveStatus()
        {
            SaveStatus(_status);
        }
        #endregion

        #region Private Classes

        /// <summary>
        /// Class for the ExpandAttributesStatus stored in the DatabaseService record
        /// </summary>
        [DataContract]
        class ExpandAttributesStatus : DatabaseServiceStatus
        {
            const int _CURRENT_VERSION = 1;

            [DataMember]
            public override int Version { get; protected set; } = _CURRENT_VERSION;

            /// <summary>
            /// The ID of the last FileTaskSession record processed
            /// </summary>
            [DataMember]
            public Int32 LastFileTaskSessionIDProcessed { get; set; }

            /// <summary>
            /// Dictionary contains the last ID processed for each of the defined attributes
            /// </summary>
            [DataMember]
            public Dictionary<string, Int32> LastIDProcessedForDashboardAttribute =
                new Dictionary<string, Int32>();

            /// <summary>
            /// Called after this instance is deserialized.
            /// </summary>
            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {
                if (Version > CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI46106", "Settings were saved with a newer version.");
                    ee.AddDebugData("SavedVersion", Version, false);
                    ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                    throw ee;
                }

                Version = CURRENT_VERSION;
            }

            /// <summary>
            /// Method to return the minimum FileTaskSession that needs to be processed.
            /// </summary>
            /// <returns>The Minimum FileTaskSession that needs to be processed</returns>
            public Int32 StartingFileTaskSessionId()
            {
                return Math.Min(LastFileTaskSessionIDProcessed, LastIDProcessedForDashboardAttribute.Values.Min()) + 1;
            }
        }

        #endregion

    }
}
