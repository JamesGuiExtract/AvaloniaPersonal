using System.Collections.Generic;
using System.Collections.Specialized;
using UCLID_AFCORELib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Represents a configuration to be able to retrieve data from a FAM database for a particular
    /// record type.
    /// </summary>
    internal interface IFAMDataConfiguration
    {
        /// <summary>
        /// Gets the root clause of an SQL query that selects and joins any relevant tables to be
        /// used to query potentially matching records.
        /// </summary>
        /// <value>
        /// The root clause of an SQL query that is to be used to query potentially matching records.
        /// </value>
        string BaseSelectQuery
        {
            get;
        }

        /// <summary>
        /// Gets the name of the ID field for the record type.
        /// <para><b>Note</b></para>
        /// Must correspond with one of the columns returned by <see cref="GetRecordInfoQuery"/>.
        /// </summary>
        /// <value>
        /// The name of the ID field for the record type.
        /// </value>
        string IdFieldDisplayName
        {
            get;
        }

        /// <summary>
        /// Gets the name of the column representing the record number field in the database.
        /// </summary>
        /// <value>
        /// The name of the column representing the record number field in the database.
        /// </value>
        string IdFieldDatabaseName
        {
            get;
        }

        /// <summary>
        /// Gets the attribute path, relative to the main record attribute, of the attribute that
        /// represents the record ID.
        /// </summary>
        /// <value>The attribute path, relative to the main record attribute, of the attribute that
        /// represents the record ID.he name of the identifier field database.
        /// </value>
        string IdFieldAttributePath
        {
            get;
        }

        /// <summary>
        /// Gets or sets a list of SQL clauses (to be used in a WHERE clause against
        /// <see cref="BaseSelectQuery"/>) that must all be true for a potential record to be matching.
        /// </summary>
        /// <value>
        /// A list of SQL clauses (to be used in a WHERE clause against <see cref="BaseSelectQuery"/>)
        /// that must all be true for a potential record to be matching.
        /// </value>
        List<string> RecordMatchCriteria
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the definitions of the columns for the record selection grid. The keys are
        /// the column names and the values are  an SQL query part that selects the appropriate data
        /// from the FAM DB table. 
        /// <para><b>Note</b></para>
        /// The <see cref="IdFieldDatabaseName"/> field must be selected as the first column.
        /// </summary>
        /// <value>
        /// The definitions of the columns for the record selection grid.
        /// </value>
        OrderedDictionary RecordQueryColumns
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
        /// </summary>
        /// <value>
        /// The different possible status colors for the buttons and their SQL query conditions.
        /// </value>
        OrderedDictionary ColorQueryConditions
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new <see cref="DocumentDataRecord"/> to be used in this configuration.
        /// </summary>
        /// <param name="famData">The <see cref="FAMData"/> instance that is managing this instance.
        /// </param>
        /// <param name="dataEntryTableRow">The <see cref="DataEntryTableRow"/> representing the
        /// record in the LabDE DEP to which this instance pertains.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> that represents this record.</param>
        /// <returns>
        /// A new <see cref="DocumentDataRecord"/>.
        /// </returns>
        DocumentDataRecord CreateDocumentDataRecord(
            FAMData famData, DataEntryTableRow dataEntryTableRow, IAttribute attribute);

        /// <summary>
        /// Generates an SQL query to retrieve data from a FAM DB related to the
        /// <see paramref="selectedRecordNumbers"/>.
        /// <para><b>Note</b></para>
        /// The first column returned by the query must be the record ID. Also, in addition to the
        /// relevant record data, the query must return a "FileID" column that is used to report any
        /// documents that have been filed against the record. Multiple rows may be return per record
        /// for records for which multiple files have been submitted.
        /// </summary>
        /// <param name="recordIDs">The record IDs for which info is needed.</param>
        /// <param name="queryForRecordIDs"><c>true</c> if <see paramref="recordIDs"/> represents an SQL
        /// query that will return the record IDs, <c>false</c> if <see paramref="recordIDs"/>
        /// is a literal comma delimited list of record IDs.</param>
        /// <returns>An SQL query to retrieve data from a FAM DB related to the
        /// <see paramref="selectedRecordNumbers"/>.</returns>
        string GetRecordInfoQuery(string recordIDs, bool queryForRecordIDs);
    }
}
