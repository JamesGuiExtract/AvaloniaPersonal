using Extract.AttributeFinder;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.ETL
{
    /// <summary>
    /// Database service to expand attributes using voa saved in AttributeSetForFile table
    /// </summary>
    public class ExpandAttributes : DatabaseService
    {
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


        #endregion
        
        #region Fields

        /// <summary>
        /// Current version
        /// </summary>
        const int CURRENT_VERSION = 1;

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
        public override void Process()
        {
            try
            {
                _processing = true;

                using (var connection = getNewSqlDbConnection())
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

                        while (VOAsToStore.Read())
                        {
                            Int64 AttributeSetForFileID = VOAsToStore.GetInt64(AttributeSetForFileIDColumn);

                            try
                            {
                                using (Stream VOAStream = VOAsToStore.GetStream(VOAColumn))
                                {
                                    // Get the VOAs from the stream
                                    IUnknownVector AttributesToStore = AttributeMethods.GetVectorOfAttributesFromSqlBinary(VOAStream);

                                    // Use separate connection so that the entire VOA can be added in a transaction
                                    using (var saveConnection = getNewSqlDbConnection())
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
                    (StoreSpatialInfo) ? buildRasterZoneInsert(attribute.Value): "");

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
                catch(Exception ex)
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
        string buildRasterZoneInsert(SpatialString spatialString)
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
        string getRasterZoneValueClause(IRasterZone rasterZone)
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

            string insert =string.Format(CultureInfo.InvariantCulture,
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
