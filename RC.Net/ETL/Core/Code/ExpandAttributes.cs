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
    public class ExpandAttributes : DatabaseService, IConfigSettings
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
                    if(value != _attributeSetNameID)
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

        }

        #endregion

        #region Constants

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
        ///     @AttributeSetName - Name of the attribute set to get the data from
        ///     @AttributePath - Path of the attribute - created by using attribute names separated by \
        ///     @DashboardNameForAttribute - Name that is used to identify the value in the dashboard
        /// </summary>
        readonly string AddDashboardAttributes = @"
                ;WITH AttributeWithPath (
                	AttributeSetForFileID
                	,AttributeID
                	,ParentAttributeID
                	,[Name]
                	,[Value]
                	,[Guid]
                	,[Level]
                	)
                AS (
                	-- anchor
                	SELECT Attribute.AttributeSetForFileID
                		,Attribute.id
                		,ParentAttributeID
                		,CAST(AttributeName.Name AS NVARCHAR(MAX))
                		,Attribute.Value
                		,Attribute.GUID
                		,0 [Level]
                	FROM Attribute
                	INNER JOIN AttributeName ON Attribute.AttributeNameID = AttributeName.ID
                	INNER JOIN AttributeSetForFile ON AttributeSetForFile.ID = Attribute.AttributeSetForFileID
                	WHERE ParentAttributeID IS NULL
                		AND AttributeSetForFile.AttributeSetNameID = @AttributeSetNameID
                	
                	UNION ALL
                	
                	SELECT Attribute.AttributeSetForFileID
                		,Attribute.id
                		,Attribute.ParentAttributeID
                		,AttributeWithPath.[Name] + '\' + AttributeName.[Name]
                		,Attribute.Value
                		,Attribute.GUID
                		,[Level] + 1
                	FROM Attribute
                	INNER JOIN AttributeName ON Attribute.AttributeNameID = AttributeName.ID
                	INNER JOIN AttributeWithPath ON Attribute.ParentAttributeID = AttributeWithPath.AttributeID
                	)
                	,ExpandedAttributeSets
                AS (
                	SELECT DISTINCT AttributeSetForFileID
                	FROM Attribute
                	)
                	,DataToInsert
                AS (
                	SELECT DISTINCT ExpandedAttributeSets.AttributeSetForFileID
                		,@DashboardNameForAttribute [Name]
                		,COALESCE(AttributeWithPath.[Value], 'UNKNOWN') [Value]
                		,ROW_NUMBER() OVER (
                			PARTITION BY ExpandedAttributeSets.AttributeSetForFileID ORDER BY ExpandedAttributeSets.AttributeSetForFileID DESC
                			) RowsOfAttribute
                	FROM ExpandedAttributeSets
                	INNER JOIN AttributeSetForFile 
                        ON AttributeSetForFile.ID = ExpandedAttributeSets.AttributeSetForFileID 
                            AND AttributeSetForFile.AttributeSetNameID = @AttributeSetNameID
                	LEFT JOIN AttributeWithPath ON AttributeSetForFile.ID = AttributeWithPath.AttributeSetForFileID
                		AND AttributeWithPath.[Name] = @AttributePath
                	LEFT JOIN DashboardAttributeFields ON AttributeSetForFile.ID = DashboardAttributeFields.AttributeSetForFileID
                		AND DashboardAttributeFields.Name = @DashboardNameForAttribute
                	WHERE DashboardAttributeFields.Name IS NULL
                	)
                INSERT INTO DashboardAttributeFields (
                	AttributeSetForFileID
                	,[Name]
                	,[Value]
                	)
                SELECT AttributeSetForFileID
                	,[Name]
                	,[Value]
                FROM DataToInsert
                WHERE RowsOfAttribute = 1
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

                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    // Records that contain attributes that need to be stored
                    SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                            SELECT AttributeSetForFile.ID AttributeSetForFileID
                                ,AttributeSetForFile.VOA
                            FROM AttributeSetForFile
                            LEFT OUTER JOIN Attribute ON AttributeSetForFile.ID = Attribute.AttributeSetForFileID
                            WHERE (Attribute.AttributeSetForFileID IS NULL)";

                    // Set the timeout so that it waits indefinitely
                    cmd.CommandTimeout = 0;

                    // Get VOA data for each file
                    using (SqlDataReader VOAsToStore = cmd.ExecuteReader())
                    {
                        // Get the ordinals needed
                        int AttributeSetForFileIDColumn = VOAsToStore.GetOrdinal("AttributeSetForFileID");
                        int VOAColumn = VOAsToStore.GetOrdinal("VOA");

                        while (VOAsToStore.Read() && !cancelToken.IsCancellationRequested)
                        {
                            Int64 AttributeSetForFileID = VOAsToStore.GetInt64(AttributeSetForFileIDColumn);

                            try
                            {
                                using (Stream VOAStream = VOAsToStore.GetStream(VOAColumn))
                                {
                                    // Get the VOAs from the stream
                                    IUnknownVector AttributesToStore = AttributeMethods.GetVectorOfAttributesFromSqlBinary(VOAStream);

                                    // Use separate connection so that the entire VOA can be added in a transaction
                                    using (var saveConnection = NewSqlDBConnection())
                                    {
                                        saveConnection.Open();

                                        var transaction = saveConnection.BeginTransaction();

                                        var saveCmd = saveConnection.CreateCommand();
                                        saveCmd.Transaction = transaction;

                                        try
                                        {
                                            addAttributes(saveConnection, transaction, AttributesToStore, AttributeSetForFileID);
                                            transaction.Commit();

                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {
                                                transaction.Rollback();
                                            }
                                            catch (Exception rollbackException)
                                            {
                                                List<ExtractException> exceptionList = new List<ExtractException>();
                                                exceptionList.Add(ex.AsExtract("ELI45427"));
                                                exceptionList.Add(rollbackException.AsExtract("ELI45428"));
                                                throw exceptionList.AsAggregateException();
                                            }
                                            throw ex.AsExtract("ELI45429");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.AsExtract("ELI45424").Log();
                            }
                        }
                    }
                }
                
                // Update the DashboardAttributeFields table
                foreach (var da in DashboardAttributes)
                {
                    using (var connection = NewSqlDBConnection())
                    {
                        connection.Open();
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = AddDashboardAttributes;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@AttributeSetNameID", da.AttributeSetNameID);
                        cmd.Parameters.AddWithValue("@AttributePath", da.PathForAttributeInAttributeSet);
                        cmd.Parameters.AddWithValue("@DashboardNameForAttribute", da.DashboardAttributeName);

                        using (var transaction = connection.BeginTransaction())
                        {
                            cmd.Transaction = transaction;
                            var task = cmd.ExecuteNonQueryAsync(cancelToken);
                            
                            // if there were changes commit the transaction
                            if (task.Result > 0)
                            {
                                transaction.Commit();
                            }
                        }
                    }
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

        #region Private Methods

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
        /// <param name="transaction">The transaction to use for adding attribute data</param>
        /// <param name="attributes">The attributes to add</param>
        /// <param name="attributeSetForFileID">The ID of the AttributeSetForFileID record that contains the VOA being added</param>
        /// <param name="parentAttributeID">The ID of the parent Attribute record. if 0 it is a top level attribute</param>
        void addAttributes(SqlConnection connection, SqlTransaction transaction, IUnknownVector attributes,
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
                insertCmd.Transaction = transaction;

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

                    Int64? attributeID = insertCmd.ExecuteScalar() as Int64?;
                    if (!(attributeID is null) && !(attribute.SubAttributes is null))
                    {
                        addAttributes(connection, transaction, attribute.SubAttributes, attributeSetForFileID, (Int64)attributeID);
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
        #endregion

    }
}
