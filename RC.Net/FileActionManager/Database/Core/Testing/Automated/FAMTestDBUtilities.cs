using ADODB;
using Extract.Database;
using Extract.Testing.Utilities;
using Extract.Utilities;
using System;
using System.Collections.Generic;
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

            string query = fileSelector.BuildQuery(
                fileProcessingDB, "[FAMFile].[ID]", "ORDER BY [FAMFile].[ID]", false);

            Recordset adoRecordset = fileProcessingDB.GetResultsForQuery(query);

            using var resultsTable = adoRecordset.AsDataTable();

            resultsArray = resultsTable.ToStringArray("\t");

            return resultsArray;
        }

        /// <summary>
        /// Adds the specified test files to the database.
        /// </summary>
        /// <param name="fileProcessingDb">The file processing database.</param>
        /// <param name="testFiles">The <see cref="TestFileManager"/> managing the test files.</param>
        /// <param name="filesToAdd">The files to add including the file's resource name, the action
        /// to which it should be added, the workflow it is being added for, the new action status and
        /// the priority for the file.</param>
        /// <returns>An array of the indices as strings with index 0 as "" so that the indices of the
        /// returned files IDs are 1-based to coincide with like FAM file IDs.</returns>
        public static string[] AddTestFiles<T>(this IFileProcessingDB fileProcessingDB,
            TestFileManager<T> testFiles,
            params (string fileName,
                    string actionName,
                    int workflowID,
                    EActionStatus actionStatus,
                    EFilePriority priority)[]
                filesToAdd)
        {
            var fileIDs = new List<string>();
            fileIDs.Add("");

            fileProcessingDB.RecordFAMSessionStart("Test.fps", filesToAdd.First().actionName, true, false);

            foreach (var fileToAdd in filesToAdd)
            {
                string testFileName = testFiles.GetFile(fileToAdd.fileName);

                var fileRecord = fileProcessingDB.AddFile(
                    testFileName, fileToAdd.actionName, fileToAdd.workflowID, fileToAdd.priority, false, false,
                    fileToAdd.actionStatus, true, out bool alreadyExists, out EActionStatus previousStatus);
                string id = fileRecord.FileID.AsString();
                if (!fileIDs.Contains(id))
                {
                    fileIDs.Add(id);
                }
            }

            fileProcessingDB.RecordFAMSessionStop();

            return fileIDs.ToArray();
        }

        /// <summary>
        /// Extension method that converts an Ado Recordset to a DataTable
        /// </summary>
        /// <param name="records">The recordset to convert</param>
        /// <returns>The dataset containing the records of the given Recordset</returns>
        public static DataTable AsDataTable(this _Recordset records)
        {
            DataTable resultsTable = new DataTable();
            resultsTable.Locale = CultureInfo.CurrentCulture;

            using (OleDbDataAdapter adapter = new System.Data.OleDb.OleDbDataAdapter())
            {
                adapter.Fill(resultsTable, records);
            }

            records.Close();
            return resultsTable;
        }
    }
}
