using Extract.Database;
using Extract.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Provides access to the LabDE related data in a FAM database.
    /// </summary>
    internal class FAMData : IDisposable
    {
        #region Constants

        /// <summary>
        /// An SQL column specification for the order name.
        /// </summary>
        const string _ORDER_NAME_XPATH =
            "[ORMMessage].value('(/ORM_O01/ORM_O01.ORDER/ORM_O01.ORDER_DETAIL/ORM_O01.OBRRQDRQ1RXOODSODT_SUPPGRP/OBR/OBR.4/CE.2)[1]','NVARCHAR(MAX)')";

        /// <summary>
        /// An SQL column specification for the ordering provider first name using an x-path query
        /// for the XML version of an ORM-O01 HL7 message.
        /// </summary>
        const string _ORDER_PROVIDER_FIRST_NAME_XPATH =
            "[ORMMessage].value('(/ORM_O01/ORM_O01.ORDER/ORM_O01.ORDER_DETAIL/ORM_O01.OBRRQDRQ1RXOODSODT_SUPPGRP/OBR/OBR.16/XCN.2/FN.1)[1]','NVARCHAR(MAX)')";

        /// <summary>
        /// An SQL column specification for the ordering provider last name using an x-path query
        /// for the XML version of an ORM-O01 HL7 message.
        /// </summary>
        const string _ORDER_PROVIDER_LAST_NAME_XPATH =
            "[ORMMessage].value('(/ORM_O01/ORM_O01.ORDER/ORM_O01.ORDER_DETAIL/ORM_O01.OBRRQDRQ1RXOODSODT_SUPPGRP/OBR/OBR.16/XCN.3)[1]','NVARCHAR(MAX)')";

        /// <summary>
        /// The default attribute path for the attribute containing the patient MRN.
        /// </summary>
        internal const string _DEFAULT_PATIENT_MRN_ATTRIBUTE = "/PatientInfo/MR_Number";

        /// <summary>
        /// The default attribute path for the attribute containing the order code.
        /// </summary>
        internal const string _DEFAULT_ORDER_CODE_ATTRIBUTE = "OrderCode";

        /// <summary>
        /// The default attribute path for the attribute containing the collection date.
        /// </summary>
        internal const string _DEFAULT_COLLECTION_DATE_ATTRIBUTE = "CollectionDate";

        /// <summary>
        /// The default attribute path for the attribute containing the collection time.
        /// </summary>
        internal const string _DEFAULT_COLLECTION_TIME_ATTRIBUTE = "CollectionTime";

        #endregion Constants

        #region OrderInfo Class

        /// <summary>
        /// Information regarding an order retrieve from the FAM DB.
        /// </summary>
        class OrderInfo
        {
            /// <summary>
            /// A description of the order compiled using <see cref="FAMData.OrderQueryColumns"/>.
            /// </summary>
            public string Description
            {
                get;
                set;
            }

            /// <summary>
            /// The FAM file IDs for files that have already been filed against this order.
            /// </summary>
            public List<int> CorrespondingFileIDs
            {
                get;
                set;
            }
        }

        #endregion OrderInfo Class

        #region Fields

        /// <summary>
        /// An <see cref="OleDbConnection"/> to use for querying against the specified
        /// <see cref="FileProcessingDB"/>.
        /// </summary>
        OleDbConnection _oleDbConnection;

        /// <summary>
        /// Maps each <see cref="DataEntryTableRow"/> in a LabDE order table to an
        /// <see cref="FAMOrderRow"/> instance to manage access to related FAM DB data.
        /// </summary>
        Dictionary<DataEntryTableRow, FAMOrderRow> _rowData =
            new Dictionary<DataEntryTableRow, FAMOrderRow>();

        /// <summary>
        /// Cached info for the most recently accessed orders from the FAM DB.
        /// </summary>
        DataCache<string, OrderInfo> _orderInfoCache = new DataCache<string, OrderInfo>(100, null);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMData"/> class.  
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> containing the LabDE
        /// data.</param>
        public FAMData(FileProcessingDB fileProcessingDB)
        {
            try
            {
                FileProcessingDB = fileProcessingDB;

                // Initialize defaults.
                PatientMRNAttribute = _DEFAULT_PATIENT_MRN_ATTRIBUTE;
                OrderCodeAttribute = _DEFAULT_ORDER_CODE_ATTRIBUTE;
                CollectionDateAttribute = _DEFAULT_COLLECTION_DATE_ATTRIBUTE;
                CollectionTimeAttribute = _DEFAULT_COLLECTION_TIME_ATTRIBUTE;

                OrderQueryColumns = new OrderedDictionary();
                OrderQueryColumns.Add("Order #", "[LabDEOrder].[OrderNumber]");
                OrderQueryColumns.Add("Order Name", _ORDER_NAME_XPATH);
                OrderQueryColumns.Add("Patient", "[LabDEPatient].LastName + ', ' + [LabDEPatient].FirstName");
                OrderQueryColumns.Add("Ordered by",
                    "(" + _ORDER_PROVIDER_LAST_NAME_XPATH + " + ', ' + " + _ORDER_PROVIDER_FIRST_NAME_XPATH + ")");
                OrderQueryColumns.Add("Requested", "[LabDEOrder].[ReferenceDateTime]");
                OrderQueryColumns.Add("Collection date/time", "[LabDEOrderFile].[CollectionDate]");

                ColorQueryConditions = new OrderedDictionary();
                ColorQueryConditions.Add("Lime", "COUNT([OrderNumber]) = 1 + COUNT(CASE WHEN ([OrderStatus] <> 'A' OR [FileCount] > 0) THEN 1 END)");
                ColorQueryConditions.Add("Yellow", "COUNT([OrderNumber]) > 1 + COUNT(CASE WHEN ([OrderStatus] <> 'A' OR [FileCount] > 0) THEN 1 END)");
                ColorQueryConditions.Add("Cyan", "COUNT([OrderNumber]) > 0");
                ColorQueryConditions.Add("Red", "COUNT([OrderNumber]) = 0");

                AttributeStatusInfo.DataReset += HandleAttributeStatusInfo_DataReset;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38150");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised to indicate data associated with one of the table rows being managed has changed.
        /// </summary>
        public event EventHandler<RowDataUpdatedArgs> RowDataUpdated;

        /// <summary>
        /// Raised to indicate the value of an OrderNumber attribute has changed.
        /// </summary>
        public event EventHandler<AttributeValueModifiedEventArgs> OrderAttributeValueModified;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="FileProcessingDB"/> containing the LabDE data.
        /// </summary>
        /// <value>
        /// The <see cref="FileProcessingDB"/> containing the LabDE data.
        /// </value>
        public FileProcessingDB FileProcessingDB
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the order number. The path
        /// should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the order number.
        /// </value>
        public string OrderNumberAttribute
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the patient MRN. The path
        /// should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the patient MRN.
        /// </value>
        public string PatientMRNAttribute
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the order code. The path
        /// should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the order code.
        /// </value>
        public string OrderCodeAttribute
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the collection date. The
        /// path should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the collection date.
        /// </value>
        public string CollectionDateAttribute
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the collection time. The
        /// path should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the collection time.
        /// </value>
        public string CollectionTimeAttribute
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether unavailable orders should be displayed in the
        /// picker UI.
        /// </summary>
        /// <value><see langword="true"/> if unavailable orders should be displayed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool ShowUnavailableOrders
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an SQL query that selects order numbers based on additional custom criteria
        /// above having a matching MRN and order code.
        /// </summary>
        /// <value>
        /// An SQL query that selects order numbers based on additional custom criteria
        /// </value>
        public string CustomOrderMatchCriteriaQuery
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the definitions of the columns for the order selection grid. The keys are
        /// the column names and the values are  an SQL query part that selects the appropriate data
        /// from the [LabDEOrder] table. 
        /// <para><b>Note</b></para>
        /// The OrderNumber field must be selected as the first column.
        /// </summary>
        /// <value>
        /// The definitions of the columns for the order selection grid.
        /// </value>
        public OrderedDictionary OrderQueryColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the different possible status colors for the buttons and their SQL query
        /// conditions. The keys are the color name (from <see cref="System.Drawing.Color"/>) while
        /// the values are SQL query expression that evaluates to true if the color should be used
        /// based to indicate available order status. If multiple expressions evaluate to true, the
        /// first of the matching rows will be used.
        /// <para><b>Note</b></para>
        /// The fields available to query against for each order are:
        /// OrderNumber:        The order number.
        /// Available:          1 if the order's status in the DB is Available, 0 if it is cancelled.
        /// FileCount:          The number of files that have been filed against this order.
        /// ReceivedDateTime:   The time the ORM-O01 HL7 message defining the order was received.
        /// ReferenceDateTime:  A configurable date/time extracted from the message (requested
        ///                     date/time is typical).
        /// </summary>
        /// <value>
        /// The different possible status colors for the buttons and their SQL query conditions.
        /// </value>
        public OrderedDictionary ColorQueryConditions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a <see cref="DataTable"/> listing the possibly matching orders for
        /// the provided <see cref="FAMOrderRow"/> with the columns being those defined in
        /// <see cref="OrderQueryColumns"/>.
        /// </summary>
        /// <param name="order">The <see cref="FAMOrderRow"/> for which matching orders are to be
        /// retrieved.</param>
        /// <returns>A <see cref="DataTable"/> listing the possibly matching orders.</returns>
        public DataTable GetMatchingOrders(FAMOrderRow order)
        {
            try 
	        {
                // Get the cached list of order numbers associated with the current FAMOrderRow or
                // generate a query to retrieve them.
                string selectedOrderNumbers =
                    order.MatchingOrderIDs ?? GetSelectedOrderNumbersQuery(order);

                // LoadOrderInfo has the side-effect of caching data from selectedOrderNumbers into
                // _orderInfoCache
                DataTable matchingOrderTable = LoadOrderInfo(selectedOrderNumbers);

                // Cache the list of orders if they have not yet been.
                if (order.MatchingOrderIDs == null)
                {
                    order.MatchingOrderIDs = "'" +
                        string.Join("','", matchingOrderTable.Rows
                            .OfType<DataRow>()
                            .Select(row => row.ItemArray[0].ToString())) + "'";
                }

                return matchingOrderTable;
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI38176");
	        }
        }

        /// <summary>
        /// Gets a <see cref="DataTable"/> with the orders specified by the order numbers in
        /// <see paramref="selectedOrderNumbers"/> with the columns being those defined in
        /// <see cref="OrderQueryColumns"/>. Info for the specified orders is also cached for
        /// subsequent calls to <see cref="GetOrderDescription"/> or
        /// <see cref="GetCorrespondingFileIDs"/>.
        /// </summary>
        /// <param name="selectedOrderNumbers">A comma delimited list of order numbers to be
        /// returned or an SQL query that selects the order numbers to be returned.</param>
        /// <returns>A <see cref="DataTable"/> listing specified orders.</returns>
        public DataTable LoadOrderInfo(string selectedOrderNumbers)
        {
            ExtractException.Assert("ELI38151",
                "Order query columns have not been properly defined.",
                OrderQueryColumns.Count > 0 &&
                OrderQueryColumns[0].ToString().ToUpperInvariant().Contains("ORDERNUMBER"));

            string columnsClause = string.Join(", \r\n",
                OrderQueryColumns
                    .OfType<DictionaryEntry>()
                    .Select(column => column.Value + " AS [" + column.Key + "]"));

            // Add a query against [LabDEOrderFile].[FileID] behind the scenes here to be able to collect
            // and return correspondingFileIds.
            string query = "SELECT " + columnsClause + "\r\n, [LabDEOrderFile].[FileID]\r\n " +
                "FROM [LabDEOrder] \r\n" +
                "INNER JOIN [LabDEPatient] ON [LabDEOrder].[PatientMRN] = [LabDEPatient].[MRN] \r\n" +
                "FULL JOIN [LabDEOrderFile] ON [LabDEOrderFile].[OrderNumber] = [LabDEOrder].[OrderNumber] \r\n" +
                "WHERE [LabDEOrder].[OrderNumber] IN (" + selectedOrderNumbers + ")";

            string lastOrderNumber = null;
            using (DataTable results = DBMethods.ExecuteDBQuery(OleDbConnection, query))
            {
                // The results will contain both an extra column (FileID) not expected by the
                // caller, as well as potentially multiple rows per order for orders for which
                // multiple files have been submitted. Generate the eventual return value as a
                // copy of results to initialize the columns, but we will then manually copy only
                // the rows expected by the caller.
                DataTable matchingOrders = results.Copy();
                matchingOrders.Rows.Clear();

                // Remove the FileID column that was not specified by OrderQueryColumns, but rather
                // used behind the scenes to compile correspondingFileIds.
                matchingOrders.Columns.Remove("FileID");

                OrderInfo orderInfo = null;

                // Loop to copy a single row per distinct order into matchingOrders.
                foreach (DataRow row in results.Rows)
                {
                    string orderNumber = row.ItemArray[0].ToString();
                    if (orderNumber != lastOrderNumber)
                    {
                        matchingOrders.ImportRow(row);
                        lastOrderNumber = orderNumber;

                        // Cache the order info for GetOrderDescription or GetCorrespondingFileIds.
                        orderInfo = new OrderInfo();
                        orderInfo.Description = string.Join("\r\n", matchingOrders.Columns
                            .OfType<DataColumn>()
                            .Select(column => column.ColumnName + ": " +
                                row.ItemArray[column.Ordinal].ToString()));
                        orderInfo.CorrespondingFileIDs = new List<int>();
                        
                        _orderInfoCache.CacheData(orderNumber, orderInfo);
                     }

                    // Collect all associated file IDs for the current order.
                    object fileIdValue = row.ItemArray.Last();
                    if (!(fileIdValue is System.DBNull))
                    {
                        orderInfo.CorrespondingFileIDs.Add((int)fileIdValue);
                    }
                }

                return matchingOrders;
            }
        }

        /// <summary>
        /// Gets a <see cref="Color"/> that indicating availability of matching orders for the
        /// provided <see cref="FAMOrderRow"/>.
        /// </summary>
        /// <param name="order">The <see cref="FAMOrderRow"/> for which a status color is needed.
        /// </param>
        /// <returns>A <see cref="Color"/> that indicating availability of matching orders for the
        /// provided <see cref="FAMOrderRow"/> or <see langword="null"/> if there is no color that
        /// reflects the current status.</returns>
        public Color? GetOrdersStatusColor(FAMOrderRow order)
        {
            ExtractException.Assert("ELI38152",
                "Order query columns have not been properly defined.",
                ColorQueryConditions.Count > 0);   
                
            // If we haven't yet cached the order numbers, set up query components to retrieve the
            // possibly matching orders from the database.
            string declarationsClause = "";
            string columnsClause = "";
            if (order.MatchingOrderIDs == null)
            {
                // Select the matching order numbers into a table variable.                
                declarationsClause =
                    "DECLARE @OrderNumbers TABLE ([OrderNumber] NVARCHAR(20)) \r\n" +
                        "INSERT INTO @OrderNumbers " + GetSelectedOrderNumbersQuery(order);

                // Create a column to select the order numbers in a comma delimited format that can
                // be re-used in subsequent queries.
                columnsClause =
                    "(SELECT CAST([OrderNumber] AS NVARCHAR(20)) + ''','''  " +
                        "FROM @OrderNumbers FOR XML PATH('')) [OrderNumbers], \r\n";
            }

            // Convert ColorQueryConditions into a clause that will return 1 when the expression
            // evaluates as true.
            columnsClause +=
                string.Join(", \r\n", ColorQueryConditions
                    .OfType<DictionaryEntry>()
                    .Select(column =>
                        "CASE WHEN (" + column.Value + ") THEN 1 ELSE 0 END AS [" + column.Key + "]"));

            // Aggregate the data returned by a query for potentially matching orders into fields
            // accessible to the ColorQueryConditions.
            string orderDataQuery = 
                "SELECT [LabDEOrder].[OrderNumber], \r\n" +
                "MAX([LabDEOrder].[OrderStatus]) AS [OrderStatus], \r\n" +
                "COUNT([LabDEOrderFile].[FileID]) AS [FileCount], \r\n" +
                "MAX([LabDEOrder].[ReceivedDateTime]) AS [ReceivedDateTime], \r\n" +
                "MAX([LabDEOrder].[ReferenceDateTime]) AS [ReferenceDateTime] \r\n" +
                "FROM [LabDEOrder] \r\n" +
                "FULL JOIN [LabDEOrderFile] ON [LabDEOrderFile].[OrderNumber] = [LabDEOrder].[OrderNumber] \r\n" +
                "WHERE [LabDEOrder].[OrderNumber] IN (\r\n" +
                    ((order.MatchingOrderIDs != null) 
                        ? order.MatchingOrderIDs
                        : "SELECT [OrderNumber] FROM @OrderNumbers") + "\r\n)" +
                "GROUP BY [LabDEOrder].[OrderNumber]";

            string colorQuery = declarationsClause + "\r\n" +
                "SELECT " + columnsClause + " FROM (\r\n" + orderDataQuery + "\r\n) AS [OrderData]";

            // Iterate the columns of the resulting row to find the first color for which the
            // configured condition evaluates to true.
            using (DataTable results = DBMethods.ExecuteDBQuery(OleDbConnection, colorQuery))
            {
                DataRow resultsRow = results.Rows[0];

                // Cache the order numbers if they have not already been.
                if (order.MatchingOrderIDs == null)
                {
                    order.MatchingOrderIDs = (resultsRow["OrderNumbers"] == DBNull.Value)
                        ? ""
                        // Remove trailing ',' then surround with apostrophes
                        : "'" + resultsRow["OrderNumbers"].ToString().TrimEnd(new[] { ',', '\'' }) + "'";
                }

                foreach (string color in ColorQueryConditions.Keys)
                {
                    if (resultsRow.Field<int>(color) == 1)
                    {
                        return (Color)typeof(Color).GetProperty(color).GetValue(null, null);
                    }
                }
            }

            // If the condition was not true for any color, return null.
            return null;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets a <see cref="FAMOrderRow"/> instance that provides access to FAM database data for
        /// the specified <see paramref="dataEntryRow"/>.
        /// </summary>
        /// <param name="dataEntryRow">The <see cref="DataEntryTableRow"/> for which data is needed.
        /// </param>
        /// <returns>a <see cref="FAMOrderRow"/> instance that provides access to FAM database data.
        /// </returns>
        public FAMOrderRow GetRowData(DataEntryTableRow dataEntryRow)
        {
            try
            {
                if (dataEntryRow == null || dataEntryRow.Attribute == null)
                {
                    return null;
                }

                // Create a new FAMOrderRow instance if one does not already exist for this row.
                FAMOrderRow rowData = null;
                if (!_rowData.TryGetValue(dataEntryRow, out rowData))
                {
                    rowData = new FAMOrderRow(this, dataEntryRow);
                    _rowData[dataEntryRow] = rowData;

                    rowData.DataUpdated += HandleRowData_DataUpdated;

                    // Register to track changes to the order number attribute's value for the purpose
                    // of raising OrderAttributeValueModified when appropriate.
                    TrackOrderAttribute(dataEntryRow);
                }

                // Provide rowData instance with access to the attributes that currently represent
                // the MRN and order code for the selected row.
                rowData.PatientMRNAttribute =
                    GetAttribute(dataEntryRow.Attribute, PatientMRNAttribute);
                rowData.OrderCodeAttribute =
                    GetAttribute(dataEntryRow.Attribute, OrderCodeAttribute);
                
                return rowData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38153");
            }
        }

        /// <summary>
        /// Gets a text description for the specified <see paramref="orderNumber"/>. The description
        /// is built using <see cref="OrderQueryColumns"/>.
        /// </summary>
        /// <param name="orderNumber">The order for which a description is needed.</param>
        /// <returns>A text description of the order.</returns>
        public string GetOrderDescription(string orderNumber)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(orderNumber))
                {
                    OrderInfo orderInfo = GetOrderInfo(orderNumber);
                    if (orderInfo != null)
                    {
                        return orderInfo.Description;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38166");
            }
        }

        /// <summary>
        /// Gets the file IDs for files that have already been filed against
        /// <see paramref="orderNumber"/> in LabDE.
        /// </summary>
        /// <param name="orderNumber">The order number.</param>
        /// <returns>The file IDs for files that have already been filed against the order number.
        /// </returns>
        public IEnumerable<int> GetCorrespondingFileIDs(string orderNumber)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(orderNumber))
                {
                    OrderInfo orderInfo = GetOrderInfo(orderNumber);
                    if (orderInfo != null)
                    {
                        return orderInfo.CorrespondingFileIDs;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38187");
            }
        }

        /// <summary>
        /// Deletes any <see cref="FAMOrderRow"/> being managed for the specified
        /// <see paramref="dataEntryRow"/>.
        /// </summary>
        /// <param name="dataEntryRow">The <see cref="DataEntryTableRow"/> for which data should be
        /// deleted.</param>
        public void DeleteRow(DataEntryTableRow dataEntryRow)
        {
            try
            {
                if (dataEntryRow == null)
                {
                    return;
                }

                FAMOrderRow rowData = null;
                if (_rowData.TryGetValue(dataEntryRow, out rowData))
                {
                    rowData.DataUpdated -= HandleRowData_DataUpdated;
                    rowData.Dispose();
                    _rowData.Remove(dataEntryRow);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38154");
            }
        }

        /// <summary>
        /// Gets the descriptions of all orders currently in <see cref="_rowData"/> that have
        /// previously been submitted via LabDE.
        /// </summary>
        /// <returns>The descriptions of all orders on the active document that have previously been
        /// submitted.</returns>
        public IEnumerable<string> GetPreviouslySubmittedOrders()
        {
            foreach (KeyValuePair<DataEntryTableRow, FAMOrderRow> rowData in _rowData)
            {
                string orderNumber =
                    GetAttribute(rowData.Key.Attribute, OrderNumberAttribute).Value.String;

                OrderInfo orderInfo = GetOrderInfo(orderNumber);
                if (orderInfo != null && orderInfo.CorrespondingFileIDs.Any()) 
                {
                    yield return orderInfo.Description;
                }
            }
        }

        /// <summary>
        /// Links the orders currently in <see cref="_rowData"/> with <see paramref="fileId"/>
        /// FAM DB (via the LabDEOrderFile table). If the link already exists, the collection date
        /// will be modified if necessary to match the currently specified collection date.
        /// </summary>
        /// <param name="fileId">The fileId the orders should be linked to.</param>
        public void LinkFileWithCurrentOrders(int fileId)
        {
            try
            {
                foreach (KeyValuePair<DataEntryTableRow, FAMOrderRow> rowData in _rowData)
                {
                    string orderNumber =
                        GetAttribute(rowData.Key.Attribute, OrderNumberAttribute).Value.String;
                    IAttribute collectionDateAttribute =
                        GetAttribute(rowData.Key.Attribute, CollectionDateAttribute);
                    IAttribute collectionTimeAttribute =
                        GetAttribute(rowData.Key.Attribute, CollectionTimeAttribute);
                    DateTime collectionDateTime = DateTime.Parse(
                        collectionDateAttribute.Value.String + " " + collectionTimeAttribute.Value.String,
                        CultureInfo.CurrentCulture);

                    LinkFileWithOrder(orderNumber, fileId, collectionDateTime);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38183");
            }
        }

        /// <summary>
        /// Links the specified <see paramref="orderNumber"/> with <see paramref="fileId"/>.
        /// If the link already exists, the collection date time of the order will be modified if
        /// necessary to match <see paramref="collectionDateTime"/>.
        /// </summary>
        /// <param name="orderNumber">The order number to link to the <see paramref="fileId"/>
        /// </param>
        /// <param name="fileId">The file ID to link to the <see paramref="orderNumber"/>.</param>
        /// <param name="collectionDateTime">The collection date time to assign to the link.</param>
        public void LinkFileWithOrder(string orderNumber, int fileId, DateTime collectionDateTime)
        {
            try
            {
                string query = string.Format(CultureInfo.CurrentCulture,
                    "DECLARE @linkExists INT \r\n" +
                    "   SELECT @linkExists = COUNT([OrderNumber]) \r\n" +
                    "       FROM [LabDEOrderFile] WHERE [OrderNumber] = {0} AND [FileID] = {1} \r\n" +
                    "IF @linkExists = 1 \r\n" +
                    "BEGIN \r\n" +
                    "    UPDATE [LabDEOrderFile] SET [CollectionDate] = {2} \r\n" +
                    "        WHERE [OrderNumber] = {0} AND [FileID] = {1} \r\n" +
                    "END \r\n" +
                    "ELSE \r\n" +
                    "BEGIN \r\n" +
                    "    INSERT INTO [LabDEOrderFile] ([OrderNumber], [FileID], [CollectionDate]) \r\n" +
                    "        VALUES ({0}, {1}, {2}) \r\n" +
                    "END",
                    "'" + orderNumber + "'", fileId, "'" + collectionDateTime + "'");

                // The query has no results-- immediately dispose of the DataTable returned.
                DBMethods.ExecuteDBQuery(OleDbConnection, query).Dispose();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38184");
            }
        }

        /// <summary>
        /// Clears all data currently cached to force it to be re-retrieved from the FAM DB next
        /// time it is needed.
        /// </summary>
        public void ClearCachedData()
        {
            try
            {
                _orderInfoCache.Clear();
                foreach (FAMOrderRow row in _rowData.Values)
                {
                    row.ClearCachedData();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38193");
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>   
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_rowData != null)
                {
                    CollectionMethods.ClearAndDispose(_rowData);
                    _rowData = null;
                }

                if (_oleDbConnection != null)
                {
                    _oleDbConnection.Dispose();
                    _oleDbConnection = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="FAMOrderRow.DataUpdated"/> event of one of the
        /// <see cref="FAMOrderRow"/>s from <see cref="_rowData"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRowData_DataUpdated(object sender, EventArgs e)
        {
            try
            {
                OnRowDataUpdated((FAMOrderRow)sender);
            }
            catch (Exception ex)
            {
                // Throw here since we know this event is triggered by our own UI event handler.
                throw ex.AsExtract("ELI38155");
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.AttributeValueModified"/> event for an order
        /// number attribute.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.DataEntry.AttributeValueModifiedEventArgs"/>
        /// instance containing the event data.</param>
        public void Handle_AttributeValueModified(object sender, AttributeValueModifiedEventArgs e)
        {
            try
            {
                if (!e.IncrementalUpdate)
                {
                    OnOrderAttributeValueModified(e);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38167");
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.AttributeValueModified"/> event for an order
        /// number attribute.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.DataEntry.AttributeValueModifiedEventArgs"/>
        /// instance containing the event data.</param>
        public void Handle_AttributeDeleted(object sender, AttributeDeletedEventArgs e)
        {
            try
            {
                var statusInfo = AttributeStatusInfo.GetStatusInfo(e.DeletedAttribute);
                statusInfo.AttributeValueModified -= Handle_AttributeValueModified;
                statusInfo.AttributeDeleted -= Handle_AttributeDeleted;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38168");
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.DataReset"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAttributeStatusInfo_DataReset(object sender, EventArgs e)
        {
            try
            {
                // If AttributeStatusInfo data is being reset, we can no longer use any of the
                // attributes associated with the rows. Dispose of them.
                if (_rowData != null)
                {
                    CollectionMethods.ClearAndDispose(_rowData);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38202");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets a lazily instantiated <see cref="OleDbConnection"/> instance against the currently
        /// specified <see cref="FileProcessingDB"/>.
        /// </summary>
        /// <value>
        /// A <see cref="OleDbConnection"/> against the currently specified
        /// <see cref="FileProcessingDB"/>.
        /// </value>
        OleDbConnection OleDbConnection
        {
            get
            {
                if (_oleDbConnection == null)
                {
                    ExtractException.Assert("ELI38156", "Missing database connection.",
                        FileProcessingDB != null);

                    _oleDbConnection = new OleDbConnection(FileProcessingDB.ConnectionString);
                    _oleDbConnection.Open();
                }

                return _oleDbConnection;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="OrderInfo"/> for the specified <see paramref="orderNumber"/>.
        /// </summary>
        /// <param name="orderNumber">The order number for which info should be retrieved.</param>
        /// <returns>The <see cref="OrderInfo"/> for the specified <see paramref="orderNumber"/>.
        /// </returns>
        OrderInfo GetOrderInfo(string orderNumber)
        {
            OrderInfo orderInfo = null;

            // First attempt to get the order info from the cache.
            if (!string.IsNullOrWhiteSpace(orderNumber) &&
                !_orderInfoCache.TryGetData(orderNumber, out orderInfo))
            {
                // If there's not cached info on-hand for this order, load it from the DB now.
                // The resulting DataTable is not needed; immediately dispose of it.
                LoadOrderInfo(orderNumber).Dispose();

                // The order info should now be cached if the order exists.
                _orderInfoCache.TryGetData(orderNumber, out orderInfo);
            }

            return orderInfo;
        }

        /// <summary>
        /// Gets an SQL query that retrieves possibly matching order numbers for the specified
        /// <see paramref="order"/>.
        /// </summary>
        /// <returns>An SQL query that retrieves possibly matching order numbers for the specified.
        /// </returns>
        string GetSelectedOrderNumbersQuery(FAMOrderRow order)
        {
            List<string> queryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(order.PatientMRN) && !string.IsNullOrWhiteSpace(order.OrderCode))
            {
                queryParts.Add("SELECT [LabDEOrder].[OrderNumber] FROM [LabDEOrder] " +
                    "WHERE [PatientMRN] = '" + order.PatientMRN.Replace("'", "''") + "'");
                queryParts.Add("SELECT [LabDEOrder].[OrderNumber] FROM [LabDEOrder] " +
                    "WHERE [OrderCode] = '" + order.OrderCode.Replace("'", "''") + "'");

                if (!ShowUnavailableOrders)
                {
                    queryParts.Add("SELECT [LabDEOrder].[OrderNumber] FROM [LabDEOrder] " +
                        "WHERE [OrderStatus] = 'A'");
                }
                if (!string.IsNullOrWhiteSpace(CustomOrderMatchCriteriaQuery))
                {
                    queryParts.Add(CustomOrderMatchCriteriaQuery);
                }
            }
            else
            {
                // If missing MRN or order code, prevent any orders from being looked up.
                queryParts.Add("SELECT [LabDEOrder].[OrderNumber] FROM [LabDEOrder] WHERE 1 = 0");
            }
            

            string selectedOrderNumbers = string.Join("\r\nINTERSECT\r\n", queryParts);
            return selectedOrderNumbers;
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/> indicated by the specified
        /// <see paramref="attributeQuery"/>.
        /// </summary>
        /// <param name="orderAttribute">The <see cref="IAttribute"/> that represents the order for
        /// which <see paramref="attributeQuery"/> is relevant.</param>
        /// <param name="attributeQuery">The attribute query.</param>
        /// <returns>The <see cref="IAttribute"/> indicated by the specified
        /// <see paramref="attributeQuery"/>.</returns>
        static IAttribute GetAttribute(IAttribute orderAttribute, string attributeQuery)
        {
            // If attributeQuery is root-relative, set orderAttribute so that the query is not
            // evaluated relative to it.
            if (attributeQuery.StartsWith("/", StringComparison.Ordinal))
            {
                orderAttribute = null;
            }

            return AttributeStatusInfo.ResolveAttributeQuery(orderAttribute, attributeQuery)
                .SingleOrDefault();
        }

        /// <summary>
        /// Registers to track changes to the order number attribute's value for the purpose
        /// of raising OrderAttributeValueModified when appropriate.
        /// </summary>
        /// <param name="dataEntryRow">The <see cref="DataEntryTableRow"/> for which attribute value
        /// changes should be tracked.</param>
        void TrackOrderAttribute(DataEntryTableRow dataEntryRow)
        {
            IAttribute orderNumberAttribute =
                GetAttribute(dataEntryRow.Attribute, OrderNumberAttribute);
            if (orderNumberAttribute != null)
            {
                var statusInfo = AttributeStatusInfo.GetStatusInfo(orderNumberAttribute);
                statusInfo.AttributeValueModified += Handle_AttributeValueModified;
                statusInfo.AttributeDeleted += Handle_AttributeDeleted;

                // When initially registering to track an attribute, raise
                // OrderAttributeValueModified right away. (In this case, the order picker column
                // will not otherwise know of the order number's initial value).
                var eventArgs = new AttributeValueModifiedEventArgs(
                    orderNumberAttribute, false, false, false);
                OnOrderAttributeValueModified(eventArgs);
            }
        }

        /// <summary>
        /// Raises the <see cref="RowDataUpdated"/> event.
        /// </summary>
        /// <param name="orderRow"></param>
        void OnRowDataUpdated(FAMOrderRow orderRow)
        {
            if (RowDataUpdated != null)
            {
                RowDataUpdated(this, new RowDataUpdatedArgs(orderRow));
            }
        }

        /// <summary>
        /// Raises the <see cref="OrderAttributeValueModified"/> event.
        /// </summary>
        /// <param name="eventArgs"></param>
        void OnOrderAttributeValueModified(AttributeValueModifiedEventArgs eventArgs)
        {
            if (OrderAttributeValueModified != null)
            {
                OrderAttributeValueModified(this, eventArgs);
            }
        }

        #endregion Private Members
    }

    /// <summary>
    /// An <see cref="EventArgs"/> that represents data for the <see cref="FAMData.RowDataUpdated"/>
    /// event.
    /// </summary>
    internal class RowDataUpdatedArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RowDataUpdatedArgs"/> class.
        /// </summary>
        /// <param name="rowData">The <see cref="FAMOrderRow"/> for which data was updated.</param>
        public RowDataUpdatedArgs(FAMOrderRow rowData)
            : base()
        {
            FAMOrderRow = rowData;
        }

        /// <summary>
        /// The <see cref="FAMOrderRow"/> for which data was updated.
        /// </summary>
        public FAMOrderRow FAMOrderRow
        {
            get;
            private set;
        }
    }
}
