using ADODB;
using Extract.Database;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    ///  Utility functions for unit tests.
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// Gets the file IDs selected by the specified <see paarmref="fileSelector"/>.
        /// </summary>
        /// <param name="fileSelector">The <see cref="FAMFileSelector"/> to use.</param>
        /// <param name="fileProcessingDb">The <see cref="FileProcessingDB"/> to use.</param>
        /// <returns>And array of file IDs selected (as strings).</returns>
        public static string[] GetResults(this FAMFileSelector fileSelector, FileProcessingDB fileProcessingDb)
        {
            string[] resultsArray;

            using (DataTable resultsTable = new DataTable())
            {
                resultsTable.Locale = CultureInfo.CurrentCulture;

                string query = fileSelector.BuildQuery(
                    fileProcessingDb, "[FAMFile].[ID]", "ORDER BY [FAMFile].[ID]");

                Recordset adoRecordset = fileProcessingDb.GetResultsForQuery(query);

                using (OleDbDataAdapter adapter = new System.Data.OleDb.OleDbDataAdapter())
                {
                    adapter.Fill(resultsTable, adoRecordset);
                }

                adoRecordset.Close();

                resultsArray = resultsTable.ToStringArray("\t");
            }

            return resultsArray;
        }
    }
}
