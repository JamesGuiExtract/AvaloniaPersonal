using ADODB;
using Extract.Database;
using Extract.Utilities;
using System;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    ///  Utility functions for unit tests.
    /// </summary>
    [CLSCompliant(false)]
    public static class FAMTestDBUtilities
    {
        /// <summary>
        /// Adds a new workflow to the specified <see paramref="fileProcessingDB"/>.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> representing the
        /// database for which the workflow should be added.</param>
        /// <param name="workflowName">The name for the workflow.</param>
        /// <param name="workflowType">The <see cref="EWorkflowType"/> of the workflow.</param>
        /// <param name="actionNames">Action names to be assigned to the workflow.</param>
        /// <returns></returns>
        public static int AddWorkflow(this IFileProcessingDB fileProcessingDB,
            string workflowName, EWorkflowType workflowType, params string[] actionNames)
        {
            int workflowId = fileProcessingDB.AddWorkflow(workflowName, workflowType);

            var workflowActions = actionNames
                .Select(name =>
                {
                    var actionInfo = new VariantVector();
                    actionInfo.PushBack(name);
                    actionInfo.PushBack(true);
                    return actionInfo;
                })
                .ToIUnknownVector();

            fileProcessingDB.SetWorkflowActions(workflowId, workflowActions);

            return workflowId;
        }

        /// <summary>
        /// Gets the file IDs selected by the specified <see paramref="fileSelector"/>.
        /// </summary>
        /// <param name="fileSelector">The <see cref="FAMFileSelector"/> to use.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> to use.</param>
        /// <returns>And array of file IDs selected (as strings).</returns>
        public static string[] GetResults(this IFAMFileSelector fileSelector, FileProcessingDB fileProcessingDB)
        {
            string[] resultsArray;

            using (DataTable resultsTable = new DataTable())
            {
                resultsTable.Locale = CultureInfo.CurrentCulture;

                string query = fileSelector.BuildQuery(
                    fileProcessingDB, "[FAMFile].[ID]", "ORDER BY [FAMFile].[ID]", false);

                Recordset adoRecordset = fileProcessingDB.GetResultsForQuery(query);

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
