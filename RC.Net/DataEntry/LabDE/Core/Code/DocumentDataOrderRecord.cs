using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using UCLID_AFCORELib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Represents a field in the voa data found by rules and/or displayed for verification.
    /// </summary>
    internal class DocumentDataOrderRecord : DocumentDataRecord, IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="IAttribute"/> currently representing the MRN associated with the LabDE
        /// order.
        /// </summary>
        DocumentDataField _patientMRNField;

        /// <summary>
        /// The <see cref="IAttribute"/> currently representing the order code associated with the
        /// LabDE order.
        /// </summary>
        DocumentDataField _orderCodeField;

        /// <summary>
        /// The <see cref="IAttribute"/> currently representing the order number associated with the
        /// LabDE order.
        /// </summary>
        DocumentDataField _orderNumberField;

        /// <summary>
        /// The collection date field
        /// </summary>
        DocumentDataField _collectionDateField;

        /// <summary>
        /// The collection time field
        /// </summary>
        DocumentDataField _collectionTimeField;

        /// <summary>
        /// The configuration
        /// </summary>
        OrderDataConfiguration _configuration;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDataRecord"/> class.
        /// </summary>
        /// <param name="famData">The <see cref="FAMData"/> instance that is managing this instance.
        /// </param>
        /// <param name="dataEntryTableRow">The <see cref="DataEntryTableRow"/> representing the
        /// order in the LabDE DEP to which this instance pertains.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> that represents this record.</param>
        public DocumentDataOrderRecord(FAMData famData, DataEntryTableRow dataEntryTableRow, IAttribute attribute)
            : base(famData, dataEntryTableRow, attribute)
        {
            try
            {
                _configuration = famData.DataConfiguration as OrderDataConfiguration;
                _patientMRNField = new DocumentDataField(attribute, _configuration.PatientMRNAttributePath, true);
                Fields.Add(_patientMRNField);
                _orderCodeField = new DocumentDataField(attribute, _configuration.OrderCodeAttributePath, true);
                Fields.Add(_orderCodeField);
                _orderNumberField = new DocumentDataField(attribute, _configuration.OrderNumberAttributePath, true);
                Fields.Add(_orderNumberField);
                _collectionDateField = new DocumentDataField(attribute, _configuration.CollectionDateAttributePath, false);
                Fields.Add(_collectionDateField);
                _collectionTimeField = new DocumentDataField(attribute, _configuration.CollectionTimeAttributePath, false);
                Fields.Add(_collectionTimeField);

                _patientMRNField.AttributeUpdated += Handle_AttributeUpdated;
                _orderCodeField.AttributeUpdated += Handle_AttributeUpdated;
                _orderNumberField.AttributeUpdated += Handle_AttributeUpdated;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38157");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the patient MRN field.
        /// </summary>
        /// <value>
        /// The patient MRN field.
        /// </value>
        public DocumentDataField PatientMRNField
        {
            get
            {
                return _patientMRNField;
            }
        }

        /// <summary>
        /// Gets the order code field.
        /// </summary>
        /// <value>
        /// The order code field.
        /// </value>
        public DocumentDataField OrderCodeField
        {
            get
            {
                return _orderCodeField;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Gets the <see cref="IFAMDataConfiguration"/> for this record.
        /// </summary>
        /// <value>
        /// The <see cref="IFAMDataConfiguration"/> for this record.
        /// </value>
        public override IFAMDataConfiguration FAMDataConfiguration
        {
            get
            {
                return _configuration;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DocumentDataField"/> representing ID for this record type.
        /// </summary>
        /// <value>
        /// The <see cref="DocumentDataField"/> representing ID for this record type.
        /// </value>
        public override DocumentDataField IdField
        {
            get
            {
                return _orderNumberField;
            }
        }

        /// <summary>
        /// Gets an SQL query that retrieves possibly matching order numbers for this instance.
        /// </summary>
        /// <returns>An SQL query that retrieves possibly matching order numbers for this instance.
        /// </returns>
        public override string GetSelectedRecordIDsQuery()
        {
            string selectedOrderNumbersQuery = null;

            if (MatchingRecordIDs != null)
            {
                // If the matching order IDs have been cached, select them directly rather than
                //querying for them.
                if (MatchingRecordIDs.Count > 0)
                {
                    selectedOrderNumbersQuery = string.Join("\r\nUNION\r\n",
                        MatchingRecordIDs.Select(orderNum => "SELECT " + orderNum));
                }
                else
                {
                    // https://extract.atlassian.net/browse/ISSUE-13003
                    // In the case that we've cached MatchingOrderIDs, but there are none, use a
                    // query of "SELECT NULL"-- without anything at all an SQL syntax error will
                    // result when selectedOrderNumbersQuery is plugged into a larger query.
                    selectedOrderNumbersQuery = "SELECT NULL";
                }
            }
            else if (!string.IsNullOrWhiteSpace(PatientMRNField.Value) &&
                     !string.IsNullOrWhiteSpace(OrderCodeField.Value))
            {
                List<string> queryParts = new List<string>();
                queryParts.Add("SELECT [OrderNumber] FROM [LabDEOrder] " +
                    "INNER JOIN [LabDEPatient] ON [PatientMRN] = [MRN] " +
                    "WHERE [CurrentMRN] = '" + PatientMRNField.Value.Replace("'", "''") + "'");
                queryParts.Add("SELECT [OrderNumber] FROM [LabDEOrder] " +
                    "WHERE [OrderCode] = '" + OrderCodeField.Value.Replace("'", "''") + "'");

                if (!_configuration.ShowUnavailableOrders)
                {
                    queryParts.Add("SELECT [LabDEOrder].[OrderNumber] FROM [LabDEOrder] " +
                        "WHERE [OrderStatus] = 'A'");
                }
                if (!string.IsNullOrWhiteSpace(_configuration.CustomOrderMatchCriteriaQuery))
                {
                    queryParts.Add(_configuration.CustomOrderMatchCriteriaQuery);
                }

                selectedOrderNumbersQuery = string.Join("\r\nINTERSECT\r\n", queryParts);
            }
            else
            {
                // If missing MRN or order code, prevent any orders from being looked up.
                selectedOrderNumbersQuery =
                    "SELECT [LabDEOrder].[OrderNumber] FROM [LabDEOrder] WHERE 1 = 0";
            }

            return selectedOrderNumbersQuery;
        }

        /// <summary>
        /// Gets a <see cref="Color"/> that indicating availability of matching orders for the
        /// provided <see cref="DocumentDataRecord"/>.
        /// </summary>
        /// <param name="order">The <see cref="DocumentDataRecord"/> for which a status color is needed.
        /// </param>
        /// <returns>A <see cref="Color"/> that indicating availability of matching orders for the
        /// provided <see cref="DocumentDataRecord"/> or <see langword="null"/> if there is no color that
        /// reflects the current status.</returns>
        public override Color? GetOrdersStatusColor()
        {
            ExtractException.Assert("ELI38152",
                "Order query columns have not been properly defined.",
                _configuration.ColorQueryConditions.Count > 0);

            // Select the matching order numbers into a table variable.                
            string declarationsClause =
                "DECLARE @OrderNumbers TABLE ([OrderNumber] NVARCHAR(20)) \r\n" +
                    "INSERT INTO @OrderNumbers\r\n" + GetSelectedRecordIDsQuery();

            // If we haven't yet cached the order numbers, set up query components to retrieve the
            // possibly matching orders from the database.
            string columnsClause = "";
            if (MatchingRecordIDs == null)
            {
                // Create a column to select the order numbers in a comma delimited format that can
                // be re-used in subsequent queries.
                columnsClause =
                    "(SELECT CAST([OrderNumber] AS NVARCHAR(20)) + ''','''  " +
                        "FROM @OrderNumbers FOR XML PATH('')) [OrderNumbers], \r\n";
            }

            // Convert ColorQueryConditions into a clause that will return 1 when the expression
            // evaluates as true.
            columnsClause +=
                string.Join(", \r\n", _configuration.ColorQueryConditions
                    .OfType<DictionaryEntry>()
                    .Select(column =>
                        "CASE WHEN (" + column.Value + ") THEN 1 ELSE 0 END AS [" + column.Key + "]"));

            // Queries to select data all other order rows that would match to the same LabDEOrder
            // table rows.
            List<string> unmappedOrders = GetMatchingUnmappedOrderRows(FAMData.LoadedRecords);

            // Aggregate the data returned by a query for potentially matching orders (including
            // unmapped orders currently in the UI into fields accessible to the ColorQueryConditions.
            string orderDataQuery =
                "SELECT [CombinedOrders].[OrderNumber], \r\n" +
                "MAX([CombinedOrders].[OrderStatus]) AS [OrderStatus], \r\n" +
                "COUNT([LabDEOrderFile].[FileID]) AS [FileCount], \r\n" +
                "MAX([CombinedOrders].[ReceivedDateTime]) AS [ReceivedDateTime], \r\n" +
                "MAX([CombinedOrders].[ReferenceDateTime]) AS [ReferenceDateTime] \r\n" +
                "FROM ( \r\n" +
                // TempID is a special column used to prevent these matching unmapped rows from
                // being grouped together.
                "SELECT [OrderNumber], [OrderCode], [PatientMRN], [ReceivedDateTime], [OrderStatus], " +
                "[ReferenceDateTime], [ORMMessage], NULL AS [TempID] FROM [LabDEOrder] \r\n" +
                (unmappedOrders.Any()
                    ? "UNION ALL\r\n" + string.Join("\r\nUNION ALL\r\n", unmappedOrders)
                    : "") +
                ") AS [CombinedOrders]\r\n" +
                "FULL JOIN [LabDEOrderFile] ON [LabDEOrderFile].[OrderNumber] = [CombinedOrders].[OrderNumber] \r\n" +
                "INNER JOIN @OrderNumbers ON [CombinedOrders].[OrderNumber] = [@OrderNumbers].[OrderNumber] " +
                    "OR [CombinedOrders].[OrderNumber] = '*'\r\n" +
                (FAMData.AlreadyMappedRecordIDs.Any()
                    ? "WHERE ([CombinedOrders].[OrderNumber] NOT IN (" + string.Join(",", FAMData.AlreadyMappedRecordIDs) + "))\r\n"
                    : "") +
                // Group by TempID as well so that all matching orders from the UI end up as separate rows.
                "GROUP BY [CombinedOrders].[OrderNumber], [CombinedOrders].[TempID]";

            string colorQuery = declarationsClause + "\r\n\r\n" +
                "SELECT " + columnsClause + " FROM (\r\n" + orderDataQuery + "\r\n) AS [OrderData]";

            // Iterate the columns of the resulting row to find the first color for which the
            // configured condition evaluates to true.
            using (DataTable results = FAMData.ExecuteDBQuery(colorQuery))
            {
                DataRow resultsRow = results.Rows[0];

                // Cache the order numbers if they have not already been.
                if (MatchingRecordIDs == null)
                {
                    MatchingRecordIDs = (resultsRow["OrderNumbers"] == DBNull.Value)
                        ? new HashSet<string>()
                        : new HashSet<string>(
                            // Remove trailing ',' then surround with apostrophes
                            ("'" + resultsRow["OrderNumbers"].ToString().TrimEnd(new[] { ',', '\'' }) + "'")
                            .Split(','));
                }

                foreach (string color in _configuration.ColorQueryConditions.Keys)
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

        /// <summary>
        /// Links this record with the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file ID to link to the <see paramref="orderNumber"/>.</param>
        public override void LinkFileWithRecord(int fileId)
        {
            try
            {
                DateTime collectionDateTime = DateTime.Parse(
                    _collectionDateField.Value + " " + _collectionTimeField.Value,
                    CultureInfo.CurrentCulture);

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
                    "    IF {0} IN (SELECT [OrderNumber] FROM [LabDEOrder]) \r\n" +
                    "    BEGIN \r\n" +
                    "        INSERT INTO [LabDEOrderFile] ([OrderNumber], [FileID], [CollectionDate]) \r\n" +
                    "            VALUES ({0}, {1}, {2}) \r\n" +
                    "   END \r\n" +
                    "END",
                    "'" + IdField.Value + "'", fileId, "'" + collectionDateTime + "'");

                // The query has no results-- immediately dispose of the DataTable returned.
                FAMData.ExecuteDBQuery(query).Dispose();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38184");
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Gets all data other order rows that match the criteria used for identifying potential
        /// matching orders in the database that have not already been mapped to an order number.
        /// </summary>
        /// <param name="orders"></param>
        /// <returns>SQL queries that will select the data for the order rows into a result set that
        /// can be joined with data for potentially matching orders in the LabDEOrder database table.
        /// </returns>
        List<string> GetMatchingUnmappedOrderRows(IEnumerable<DocumentDataRecord> orders)
        {
            List<string> unmappedOrders = new List<string>();

            int tempID = 0;

            // If this order is missing an MRN or OrderCode, it is not possible to match to other
            // orders.
            if (string.IsNullOrWhiteSpace(OrderCodeField.Value) ||
                string.IsNullOrWhiteSpace(PatientMRNField.Value))
            {
                return unmappedOrders;
            }

            // Loop through all other order attributes
            foreach (DocumentDataOrderRecord otherOrder in orders
                .Except(new[] { this })
                .OfType<DocumentDataOrderRecord>())
            {
                // For each matching but unmapped row.
                if (otherOrder.PatientMRNField.Value.Equals(PatientMRNField.Value, StringComparison.OrdinalIgnoreCase) &&
                    otherOrder.OrderCodeField.Value.Equals(OrderCodeField.Value, StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrWhiteSpace(otherOrder.IdField.Value))
                {
                    // Generate an SQL statement that can select data from the UI into query results
                    // that can be combined with actual UI data.
                    StringBuilder unmappedOrder = new StringBuilder();
                    unmappedOrder.Append("SELECT '*' AS [OrderNumber], ");

                    unmappedOrder.Append("'");
                    unmappedOrder.Append(OrderCodeField.Value);
                    unmappedOrder.Append("'");
                    unmappedOrder.Append(" AS [OrderCode], ");

                    unmappedOrder.Append("'");
                    unmappedOrder.Append(PatientMRNField.Value);
                    unmappedOrder.Append("'");
                    unmappedOrder.Append(" AS [PatientMRN], ");

                    unmappedOrder.Append("GETDATE() AS [ReceivedDateTime], ");

                    unmappedOrder.Append("'*' AS [OrderStatus], ");

                    unmappedOrder.Append("GETDATE() AS [ReferenceDateTime], ");

                    unmappedOrder.Append("'' AS [ORMMessage], ");

                    // TempID is a special column used to prevent these rows from being grouped in
                    // the context of GetOrdersStatusColor's DB query.
                    unmappedOrder.Append(tempID++.ToString(CultureInfo.InvariantCulture));
                    unmappedOrder.AppendLine(" AS [TempID]");

                    unmappedOrders.Add(unmappedOrder.ToString());
                }
            }

            return unmappedOrders;
        }

        #endregion Private Members
    }
}
