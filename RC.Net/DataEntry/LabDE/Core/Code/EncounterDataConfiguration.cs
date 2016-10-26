using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UCLID_AFCORELib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// A configuration to be used to represent encounter record types.
    /// </summary>
    /// <seealso cref="Extract.DataEntry.LabDE.IFAMDataConfiguration" />
    internal class EncounterDataConfiguration : IFAMDataConfiguration
    {
        #region Constants

        /// <summary>
        /// The root clause of an SQL query that is to be used to query potentially matching records.
        /// </summary>
        const string _BASE_SELECT_QUERY =
            "SELECT [CSN] FROM [LabDEEncounter]\r\n" +
            "   INNER JOIN[LabDEPatient] ON[PatientMRN] = [MRN]";

        /// <summary>
        /// The display name of the ID field name for encounters
        /// </summary>
        const string _ID_FIELD_DISPLAY_NAME = "CSN";

        /// <summary>
        /// The name of the column representing the encounter ID field in the database.
        /// </summary>
        const string ID_FIELD_DATABASE_NAME = "CSN";

        /// <summary>
        /// The attribute path, relative to the main record attribute, of the attribute that
        /// represents the record ID.
        /// </summary>
        const string _DEFAULT_ID_FIELD_ATTRIBUTE_PATH = "CSN";

        /// <summary>
        /// The default attribute path for the attribute containing the encounter date.
        /// </summary>
        internal const string _DEFAULT_DATE_ATTRIBUTE_PATH = "Date";

        /// <summary>
        /// The default attribute path for the attribute containing the encounter date.
        /// </summary>
        internal const string _DEFAULT_TIME_ATTRIBUTE_PATH = "Time";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterDataConfiguration"/> class.
        /// </summary>
        public EncounterDataConfiguration()
        {
            try
            {
                // Initialize defaults.
                IdFieldAttributePath = _DEFAULT_ID_FIELD_ATTRIBUTE_PATH;
                EncounterDateAttributePath = _DEFAULT_DATE_ATTRIBUTE_PATH;
                EncounterTimeAttributePath = _DEFAULT_TIME_ATTRIBUTE_PATH;

                RecordMatchCriteria = new List<string>();
                RecordMatchCriteria.Add("{/PatientInfo/MR_Number} = [PatientMRN]");
                RecordMatchCriteria.Add("{Department} = [Department]");

                RecordQueryColumns = new OrderedDictionary();
                RecordQueryColumns.Add(IdFieldDatabaseName, "[LabDEEncounter].[CSN]");
                RecordQueryColumns.Add("Patient", "[LabDEPatient].[LastName] + ', ' + [LabDEPatient].[FirstName]");
                RecordQueryColumns.Add("Date / Time", "[LabDEEncounter].[EncounterDateTime]");
                RecordQueryColumns.Add("Department", "[LabDEEncounter].[Department]");
                RecordQueryColumns.Add("Type", "[LabDEEncounter].[EncounterType]");
                RecordQueryColumns.Add("Provider", "[LabDEEncounter].[EncounterProvider]");

                ColorQueryConditions = new OrderedDictionary();
                ColorQueryConditions.Add("Red", "COUNT(*) = 0");
                ColorQueryConditions.Add("Yellow",
                    "COUNT(CASE WHEN ([CSN] = '*') THEN 1 END) >= COUNT(CASE WHEN ([FileCount] = 0) THEN 1 END)");
                ColorQueryConditions.Add("Lime", "COUNT(CASE WHEN ([FileCount] = 0) THEN 1 END) > 0");
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI41534");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the root clause of an SQL query that selects and joins any relevant tables
        /// to be used to query potentially matching records.
        /// </summary>
        /// <value>
        /// The root clause of an SQL query that is to be used to query potentially matching records.
        /// </value>
        public string BaseSelectQuery
        {
            get
            {
                return _BASE_SELECT_QUERY;
            }
        }

        /// <summary>
        /// Gets the name of the ID field for the record type.
        /// <para><b>Note</b></para>
        /// Must correspond with one of the columns returned by <see cref="GetRecordInfoQuery"/>.
        /// </summary>
        /// <value>
        /// The name of the ID field for the record type.
        /// </value>
        public string IdFieldDisplayName
        {
            get
            {
                return _ID_FIELD_DISPLAY_NAME;
            }
        }

        /// <summary>
        /// Gets the name of the column representing the record number field in the database.
        /// </summary>
        /// <value>
        /// The name of the column representing the record number field in the database.
        /// </value>
        public string IdFieldDatabaseName
        {
            get
            {
                return ID_FIELD_DATABASE_NAME;
            }
        }

        /// <summary>
        /// Gets the attribute path, relative to the main record attribute, of the attribute that
        /// represents the record ID.
        /// </summary>
        /// <value>The attribute path, relative to the main record attribute, of the attribute that
        /// represents the rec
        public string IdFieldAttributePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the default attribute path for the attribute containing the encounter date.
        /// </summary>
        /// <value>
        /// The default attribute path for the attribute containing the encounter date.
        /// </value>
        public string EncounterDateAttributePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the default attribute path for the attribute containing the encounter time.
        /// </summary>
        /// <value>
        /// The default attribute path for the attribute containing the encounter time.
        /// </value>
        public string EncounterTimeAttributePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of SQL clauses (to be used in a WHERE clause against
        /// <see cref="BaseSelectQuery"/>) that must all be true for a potential record to be matching.
        /// </summary>
        /// <value>
        /// A list of SQL clauses (to be used in a WHERE clause against <see cref="BaseSelectQuery"/>)
        /// that must all be true for a potential record to be matching.
        public List<string> RecordMatchCriteria
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the definitions of the columns for the record selection grid. The keys are
        /// the column names and the values are  an SQL query part that selects the appropriate data
        /// from the FAM DB table. 
        /// <para><b>Note</b></para>
        /// The IdFieldDatabaseName must be selected as the first column.
        /// </summary>
        /// <value>
        /// The definitions of the columns for the selection grid.
        /// </value>
        public OrderedDictionary RecordQueryColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the different possible status colors for the buttons and their SQL query
        /// conditions. The keys are the color name (from <see cref="System.Drawing.Color"/>) while
        /// the values are SQL query expression that evaluates to true if the color should be used
        /// based to indicate available record status. If multiple expressions evaluate to true, the
        /// first of the matching rows will be used.
        /// <para><b>Note</b></para>
        /// The fields available to query against for each encounter are:
        /// CSN:                The CSN (ID) of the encounter
        /// FileCount:          The number of files that have been filed against this encounter
        /// EncounterDateTime:  The date/time of the encounter
        /// </summary>
        /// <value>
        /// The different possible status colors for the buttons and their SQL query conditions.
        /// </value>
        public OrderedDictionary ColorQueryConditions
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a new <see cref="DocumentDataRecord"/> to be used in this configuration..
        /// </summary>
        /// <param name="famData">The <see cref="FAMData"/> instance that is managing this instance.
        /// </param>
        /// <param name="dataEntryTableRow">The <see cref="DataEntryTableRow"/> representing the
        /// encounter in the DEP to which this instance pertains.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> that represents this record.</param>
        /// <returns>
        /// A new <see cref="DocumentDataRecord"/>.
        /// </returns>
        public DocumentDataRecord CreateDocumentDataRecord(
            FAMData famData, DataEntryTableRow dataEntryTableRow, IAttribute attribute)
        {
            try
            {
                return new DocumentDataEncounterRecord(famData, dataEntryTableRow, attribute);
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI41535");
            }
        }

        /// <summary>
        /// Generates an SQL query to retrieve data from a FAM DB related to the
        /// <see paramref="recordIDs"/>.
        /// <para><b>Note</b></para>
        /// The first column returned by the query must be the record ID. Also, in addition to the
        /// relevant record data, the query must return a "FileID" column that is used to report any
        /// documents that have been filed against the encounter. Multiple rows may be return per
        /// record for records for which multiple files have been submitted.
        /// </summary>
        /// <param name="recordIDs">The record IDs for which info is needed.</param>
        /// <param name="selectViaQuery"><c>true</c> if <see paramref="recordIDs"/> represents an SQL
        /// query that will return the record IDs, <c>false</c> if <see paramref="recordIDs"/>
        /// is a literal comma delimited list of record IDs.</param>
        /// <returns>An SQL query to retrieve data from a FAM DB related to the
        /// <see paramref="selectedRecordNumbers"/>.</returns>
        public string GetRecordInfoQuery(string recordIDs, bool selectViaQuery)
        {
            try
            {
                ExtractException.Assert("ELI41536",
                        "Encounter query columns have not been properly defined.",
                        RecordQueryColumns.Count > 0 &&
                        RecordQueryColumns[0].ToString().IndexOf(
                            IdFieldDatabaseName, StringComparison.OrdinalIgnoreCase) >= 0);

                // If recordIDs is a query, select the results into a table that can be joined with
                // LabDEOrderFile.
                string declarationsClause = "";
                if (selectViaQuery)
                {
                    declarationsClause = "DECLARE @CSNs TABLE ([CSN] NVARCHAR(20)) \r\n" +
                        "INSERT INTO @CSNs\r\n" + recordIDs;
                }

                string columnsClause = string.Join(", \r\n",
                    RecordQueryColumns
                        .OfType<DictionaryEntry>()
                        .Select(column => column.Value + " AS [" + column.Key + "]"));

                // Add a query against [LabDEEncounterFile].[FileID] behind the scenes here to be able to collect
                // and return correspondingFileIds.
                string query = declarationsClause + "\r\n\r\n" +
                    "SELECT " + columnsClause + ",\r\n [LabDEEncounterFile].[FileID]\r\n " +
                    "FROM [LabDEEncounter] \r\n" +
                    "INNER JOIN [LabDEPatient] p ON [LabDEEncounter].[PatientMRN] = p.[MRN] \r\n" +
                    "INNER JOIN [LabDEPatient] ON p.[CurrentMRN] = [LabDEPatient].[MRN] \r\n" +
                    "FULL JOIN [LabDEEncounterFile] ON [LabDEEncounterFile].[EncounterID] = [LabDEEncounter].[CSN] \r\n" +
                    (selectViaQuery
                        ? "INNER JOIN @CSNs ON [LabDEEncounter].[CSN] = [@CSNs].[CSN]"
                        : "WHERE [LabDEEncounter].[CSN] IN (" + recordIDs + ")");

                return query;
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI41537");
            }
        }

        #endregion Methods
    }
}
