using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Gets the name of the ID field for the record type.
        /// <para><b>Note</b></para>
        /// Must correspond with one of the columns returned by <see cref="GetRecordInfoQuery"/>.
        /// </summary>
        /// <value>
        /// The name of the ID field for the record type.
        /// </value>
        string IdFieldName
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="DocumentDataRecord"/>.
        /// </summary>
        /// <param name="famData">The <see cref="FAMData"/> instance that is managing this instance.
        /// </param>
        /// <param name="dataEntryTableRow">The <see cref="DataEntryTableRow"/> representing the
        /// order in the LabDE DEP to which this instance pertains.</param>
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
        /// documents that have been filed against the order. Multiple rows may be return per record
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
