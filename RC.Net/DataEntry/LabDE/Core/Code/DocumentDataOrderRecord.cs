using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
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
    public class DocumentDataOrderRecord : DocumentDataRecord
    {
        #region Fields

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
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "fam")]
        public DocumentDataOrderRecord(FAMData famData, DataEntryTableRow dataEntryTableRow, IAttribute attribute)
            : base(famData, dataEntryTableRow, attribute)
        {
            try
            {
                _configuration = famData.DataConfiguration as OrderDataConfiguration;

                ApplyConfiguration(_configuration);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41549");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Gets a <see cref="Color"/> that indicating availability of matching orders for the
        /// provided <see cref="DocumentDataRecord"/>.
        /// </summary>
        /// <param name="order">The <see cref="DocumentDataRecord"/> for which a status color is needed.
        /// </param>
        /// <returns>A <see cref="Color"/> that indicating availability of matching orders for the
        /// provided <see cref="DocumentDataRecord"/> or <see langword="null"/> if there is no color that
        /// reflects the current status.</returns>
        public override Color? GetRecordsStatusColor()
        {
            try
            {
                ExtractException.Assert("ELI38152",
                        "Order query columns have not been properly defined.",
                        _configuration.ColorQueryConditions.Count > 0);

                // Select the matching order numbers into a table variable.                
                string declarationsClause =
                    "DECLARE @OrderNumbers TABLE ([OrderNumber] NVARCHAR(MAX)) \r\n" +
                        "INSERT INTO @OrderNumbers\r\n" + GetSelectedRecordIdsQuery();

                // If we haven't yet cached the order numbers, set up query components to retrieve the
                // possibly matching orders from the database.
                string columnsClause = "";
                if (MatchingRecordIds == null)
                {
                    // Create a column to select the order numbers in a comma delimited format that can
                    // be re-used in subsequent queries.
                    columnsClause =
                        "(SELECT CAST([OrderNumber] AS NVARCHAR(MAX)) + ''','''  " +
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
                var unmappedOrders = new List<string>(GetQueriesForUnmappedRecordRows(FAMData.LoadedRecords));

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
                    "SELECT [LabDEOrder].[OrderNumber], [ReceivedDateTime], [OrderStatus], [ReferenceDateTime], [ORMMessage], NULL AS [TempID] " +
                    "   FROM [LabDEOrder] \r\n" +
                    "   INNER JOIN @OrderNumbers ON [LabDEOrder].[OrderNumber] = [@OrderNumbers].[OrderNumber]\r\n" +
                    (unmappedOrders.Any()
                        ? "UNION ALL\r\n" + string.Join("\r\nUNION ALL\r\n", unmappedOrders)
                        : "") +
                    ") AS [CombinedOrders]\r\n" +
                    "LEFT JOIN [LabDEOrderFile] ON [LabDEOrderFile].[OrderNumber] = [CombinedOrders].[OrderNumber] \r\n" +
                    (FAMData.AlreadyMappedRecordIds.Any()
                        ? "WHERE ([CombinedOrders].[OrderNumber] NOT IN (" + string.Join(",", FAMData.AlreadyMappedRecordIds) + "))\r\n"
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
                    if (MatchingRecordIds == null)
                    {
                        MatchingRecordIds = (resultsRow["OrderNumbers"] == DBNull.Value)
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
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41548");
            }
        }

        /// <summary>
        /// Links this record with the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file ID to link to the record.</param>
        public override void LinkFileWithRecord(int fileId)
        { 
            try
            {
                var collectionDateField = new DocumentDataField(Attribute, _configuration.CollectionDateAttributePath, false);
                var collectionTimeField = new DocumentDataField(Attribute, _configuration.CollectionTimeAttributePath, false);

                DateTime collectionDateTime;
                bool useCollectionDateTime = false;
                if (DateTime.TryParse(
                    collectionDateField.Value + " " + collectionTimeField.Value,
                    CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault,
                    out collectionDateTime))
                {
                    useCollectionDateTime = true;
                }

                FAMData.LinkFileWithOrder(fileId, IdField.Value, useCollectionDateTime ? (DateTime?)collectionDateTime : null);
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
        /// <param name="records">The records for which matching database records are needed.</param>
        /// <returns>SQL queries that will select the data for the order rows into a result set that
        /// can be joined with data for potentially matching orders in the LabDEOrder database table.
        /// </returns>
        protected virtual ReadOnlyCollection<string> GetQueriesForUnmappedRecordRows(IEnumerable<DocumentDataRecord> records)
        {
            List<string> unmappedRecords = new List<string>();

            int tempID = 0;

            // If this order is missing data for any active fields, it is not possible to match to
            // other records.
            if (ActiveFields.Values.Any(field => string.IsNullOrWhiteSpace(field.Value)))
            {
                return unmappedRecords.AsReadOnly();
            }

            // Loop through all other order attributes
            foreach (DocumentDataOrderRecord otherOrder in records
                .Except(new[] { this })
                .OfType<DocumentDataOrderRecord>())
            {
                // For each matching but unmapped row.
                if (string.IsNullOrWhiteSpace(otherOrder.IdField.Value) &&
                    otherOrder.ActiveFields.Values.Select(field => field.Value)
                    .SequenceEqual(
                        ActiveFields.Values.Select(field => field.Value)))
                {
                    // Generate an SQL statement that can select data from the UI into query results
                    // that can be combined with actual UI data.
                    StringBuilder unmappedOrder = new StringBuilder();
                    unmappedOrder.Append("SELECT '*' AS [OrderNumber], ");
                    unmappedOrder.Append("GETDATE() AS [ReceivedDateTime], ");
                    unmappedOrder.Append("'*' AS [OrderStatus], ");
                    unmappedOrder.Append("GETDATE() AS [ReferenceDateTime], ");
                    unmappedOrder.Append("'' AS [ORMMessage], ");

                    // TempID is a special column used to prevent these rows from being grouped in
                    // the context of GetOrdersStatusColor's DB query.
                    unmappedOrder.Append(tempID++.ToString(CultureInfo.InvariantCulture));
                    unmappedOrder.AppendLine(" AS [TempID]");

                    unmappedRecords.Add(unmappedOrder.ToString());
                }
            }

            return unmappedRecords.AsReadOnly();
        }

        #endregion Private Members
    }
}
