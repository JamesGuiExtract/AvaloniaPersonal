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
    internal class DocumentDataEncounterRecord : DocumentDataRecord, IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="EncounterDataConfiguration"/> being used.
        /// </summary>
        EncounterDataConfiguration _configuration;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDataRecord"/> class.
        /// </summary>
        /// <param name="famData">The <see cref="FAMData"/> instance that is managing this instance.
        /// </param>
        /// <param name="dataEntryTableRow">The <see cref="DataEntryTableRow"/> representing the
        /// encounter in the LabDE DEP to which this instance pertains.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> that represents this record.</param>
        public DocumentDataEncounterRecord(FAMData famData, DataEntryTableRow dataEntryTableRow, IAttribute attribute)
            : base(famData, dataEntryTableRow, attribute)
        {
            try
            {
                _configuration = famData.DataConfiguration as EncounterDataConfiguration;
                
                ApplyConfiguration(_configuration);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41538");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Gets a <see cref="Color"/> that indicating availability of matching encounters for the
        /// provided <see cref="DocumentDataRecord"/>.
        /// </summary>
        /// <param name="order">The <see cref="DocumentDataRecord"/> for which a status color is needed.
        /// </param>
        /// <returns>A <see cref="Color"/> that indicating availability of matching encounters for the
        /// provided <see cref="DocumentDataRecord"/> or <see langword="null"/> if there is no color that
        /// reflects the current status.</returns>
        public override Color? GetRecordsStatusColor()
        {
            try
            {
                ExtractException.Assert("ELI41539",
                    "Order query columns have not been properly defined.",
                    _configuration.ColorQueryConditions.Count > 0);

                // Select the matching order numbers into a table variable.                
                string declarationsClause =
                    "DECLARE @CSN TABLE ([CSN] NVARCHAR(20)) \r\n" +
                        "INSERT INTO @CSN\r\n" + GetSelectedRecordIDsQuery();

                // If we haven't yet cached the CSNs, set up query components to retrieve the
                // possibly matching encounters from the database.
                string columnsClause = "";
                if (MatchingRecordIDs == null)
                {
                    // Create a column to select the CSNs in a comma delimited format that can
                    // be re-used in subsequent queries.
                    columnsClause =
                        "(SELECT CAST([CSN] AS NVARCHAR(20)) + ''','''  " +
                            "FROM @CSN FOR XML PATH('')) [CSNs], \r\n";
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
                List<string> unmappedRecords = GetQueriesForUnmappedRecordRows(FAMData.LoadedRecords);

                // Aggregate the data returned by a query for potentially matching records (including
                // unmapped records currently in the UI into fields accessible to the ColorQueryConditions.
                string recordDataQuery =
                    "SELECT [CombinedRecords].[CSN], \r\n" +
                    "COUNT([LabDEEncounterFile].[FileID]) AS [FileCount], \r\n" +
                    "MAX([CombinedRecords].[EncounterDateTime]) AS [EncounterDateTime] \r\n" +
                    "FROM ( \r\n" +
                    // TempID is a special column used to prevent these matching unmapped rows from
                    // being grouped together.
                    "SELECT [CSN], [EncounterDateTime], NULL AS [TempID] " +
                    "   FROM [LabDEEncounter] \r\n" +
                    (unmappedRecords.Any()
                        ? "UNION ALL\r\n" + string.Join("\r\nUNION ALL\r\n", unmappedRecords)
                        : "") +
                    ") AS [CombinedRecords]\r\n" +
                    "FULL JOIN [LabDEEncounterFile] ON [LabDEEncounterFile].[EncounterID] = [CombinedRecords].[CSN] \r\n" +
                    "INNER JOIN @CSN ON [CombinedRecords].[CSN] = [@CSN].[CSN] " +
                        "OR [CombinedRecords].[CSN] = '*'\r\n" +
                    (FAMData.AlreadyMappedRecordIDs.Any()
                        ? "WHERE ([CombinedRecords].[CSN] NOT IN (" + string.Join(",", FAMData.AlreadyMappedRecordIDs) + "))\r\n"
                        : "") +
                    // Group by TempID as well so that all matching records from the UI end up as separate rows.
                    "GROUP BY [CombinedRecords].[CSN], [CombinedRecords].[TempID]";

                string colorQuery = declarationsClause + "\r\n\r\n" +
                    "SELECT " + columnsClause + " FROM (\r\n" + recordDataQuery + "\r\n) AS [EncounterData]";

                // Iterate the columns of the resulting row to find the first color for which the
                // configured condition evaluates to true.
                using (DataTable results = FAMData.ExecuteDBQuery(colorQuery))
                {
                    DataRow resultsRow = results.Rows[0];

                    // Cache the CSNs if they have not already been.
                    if (MatchingRecordIDs == null)
                    {
                        MatchingRecordIDs = (resultsRow["CSNs"] == DBNull.Value)
                            ? new HashSet<string>()
                            : new HashSet<string>(
                                // Remove trailing ',' then surround with apostrophes
                                ("'" + resultsRow["CSNs"].ToString().TrimEnd(new[] { ',', '\'' }) + "'")
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
                throw ex.AsExtract("ELI41547");
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
                var dateField = new DocumentDataField(Attribute, _configuration.EncounterDateAttributePath, false);
                var timeField = new DocumentDataField(Attribute, _configuration.EncounterTimeAttributePath, false);

                DateTime encounterDateTime;
                bool useEncounterDateTime = false;
                if (DateTime.TryParse(
                    dateField.Value + " " + timeField.Value,
                    CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault,
                    out encounterDateTime))
                {
                    useEncounterDateTime = true;
                }

                string query = string.Format(CultureInfo.CurrentCulture,
                    "DECLARE @linkExists INT \r\n" +
                    "   SELECT @linkExists = COUNT([EncounterID]) \r\n" +
                    "       FROM [LabDEEncounterFile] WHERE [EncounterID] = {0} AND [FileID] = {1} \r\n" +
                    "IF @linkExists = 1 \r\n" +
                    "BEGIN \r\n" +
                    (useEncounterDateTime
                        ? "    UPDATE [LabDEEncounterFile] SET [DateTime] = '{2}' \r\n"
                        : "    UPDATE [LabDEEncounterFile] SET [DateTime] = NULL \r\n") +
                    "        WHERE [EncounterID] = {0} AND [FileID] = {1} \r\n" +
                    "END \r\n" +
                    "ELSE \r\n" +
                    "BEGIN \r\n" +
                    "    IF {0} IN (SELECT [CSN] FROM [LabDEEncounter]) \r\n" +
                    "    BEGIN \r\n" +
                    (useEncounterDateTime
                        ? "        INSERT INTO [LabDEEncounterFile] ([EncounterID], [FileID], [DateTime]) \r\n" +
                          "            VALUES ({0}, {1}, '{2}') \r\n"
                        : "        INSERT INTO [LabDEEncounterFile] ([EncounterID], [FileID]) \r\n" +
                          "            VALUES ({0}, {1}) \r\n") +
                    "   END \r\n" +
                    "END",
                    "'" + IdField.Value + "'", fileId, "'" + encounterDateTime + "'");

                // The query has no results-- immediately dispose of the DataTable returned.
                FAMData.ExecuteDBQuery(query).Dispose();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41540");
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
        List<string> GetQueriesForUnmappedRecordRows(IEnumerable<DocumentDataRecord> records)
        {
            List<string> unmappedRecords = new List<string>();

            int tempID = 0;

            // If this order is missing data for any active fields, it is not possible to match to
            // other records.
            if (_activeFields.Values.Any(field => string.IsNullOrWhiteSpace(field.Value)))
            {
                return unmappedRecords;
            }

            // Loop through all other order attributes
            foreach (DocumentDataEncounterRecord otherOrder in records
                .Except(new[] { this })
                .OfType<DocumentDataEncounterRecord>())
            {
                // For each matching but unmapped row.
                if (string.IsNullOrWhiteSpace(otherOrder.IdField.Value) &&
                    otherOrder.MatchingRecordIDs.Intersect(MatchingRecordIDs).Any())
                {
                    // Generate an SQL statement that can select data from the UI into query results
                    // that can be combined with actual UI data.
                    StringBuilder unmappedOrder = new StringBuilder();
                    unmappedOrder.Append("SELECT '*' AS [CSN], ");
                    unmappedOrder.Append("GETDATE() AS [EncounterDateTime], ");
                    unmappedOrder.Append("'' AS [Department], ");

                    // TempID is a special column used to prevent these rows from being grouped in
                    // the context of GetOrdersStatusColor's DB query.
                    unmappedOrder.Append(tempID++.ToString(CultureInfo.InvariantCulture));
                    unmappedOrder.AppendLine(" AS [TempID]");

                    unmappedRecords.Add(unmappedOrder.ToString());
                }
            }

            return unmappedRecords;
        }

        #endregion Private Members
    }
}
