﻿using Extract.DataEntry.LabDE;
using Extract.FileActionManager.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Utility class to take care of tasks related to generating new paginated output documents.
    /// </summary>
    public class PaginatedOutputCreationUtility
    {
        /// <summary>
        /// The path tag expression used to generate filenames for output documents.
        /// </summary>
        string _outputPathTagExpression;

        /// <summary>
        /// The interface providing access into the FAM DB the output document is being added to.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the action in the database into which the file is being added.
        /// </summary>
        int _actionID;

        /// <summary>
        /// The ID for the workflow into which the file should be added.
        /// </summary>
        int _workflowID;

        /// <summary>
        /// Initializes a new PaginatedOutputCreationUtility instance.
        /// </summary>
        /// <param name="outputPathTagExpression">The path tag expression used to generate filenames for output documents.</param>
        /// <param name="fileProcessingDB">The interface providing access into the FAM DB the output document is being added to.</param>
        /// <param name="actionID">The ID of the action in the database into which the file is being added.</param>
        /// <param name="workflowID">The ID for the workflow into which the file should be added.</param>
        public PaginatedOutputCreationUtility(string outputPathTagExpression, FileProcessingDB fileProcessingDB, int actionID, int workflowID)
        {
            try
            {
                _outputPathTagExpression = outputPathTagExpression;
                _fileProcessingDB = fileProcessingDB;
                _actionID = actionID;
                _workflowID = workflowID;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47175");
            }
        }

        /// <summary>
        /// Generates a filename for a new output document.
        /// </summary>
        /// <param name="pageInfos">The PageInfos for the pages to be in the document.</param>
        /// <param name="tagManager">The tag manager that should be used to expand the path tag expression.</param>
        /// <param name="subDocIndex">The one-based index of this sub document. If less than 1, the DB will be queried for the subDocIndex</param>
        public string GetPaginatedDocumentFileName(IEnumerable<PageInfo> pageInfos, FAMTagManager tagManager, int subDocIndex = -1)
        {
            try
            {
                var sourcePageInfo = pageInfos.Where(info => !info.Deleted).ToList();
				string sourceDocName = sourcePageInfo.FirstOrDefault()?.DocumentName;
                if (sourceDocName == null)
                {
                    return "";
                }
                var pathTags = new FileActionManagerPathTags(tagManager, sourceDocName);
                if (_outputPathTagExpression.Contains(PaginationSettings.SubDocIndexTag))
                {
                    if (subDocIndex <= 0)
                    {
                        subDocIndex = GetFirstSubDocIndex(pageInfos);
                    }

                    pathTags.AddTag(PaginationSettings.SubDocIndexTag,
                        subDocIndex.ToString(CultureInfo.InvariantCulture));
                }
                if (_outputPathTagExpression.Contains(PaginationSettings.FirstPageTag))
                {
                    int firstPageNum = sourcePageInfo
                            .Where(page => page.DocumentName == sourceDocName)
                            .Min(page => page.Page);

                    pathTags.AddTag(PaginationSettings.FirstPageTag,
                        firstPageNum.ToString(CultureInfo.InvariantCulture));
                }
                if (_outputPathTagExpression.Contains(PaginationSettings.LastPageTag))
                {
                    int lastPageNum = sourcePageInfo
                        .Where(page => page.DocumentName == sourceDocName)
                        .Max(page => page.Page);

                    pathTags.AddTag(PaginationSettings.LastPageTag,
                        lastPageNum.ToString(CultureInfo.InvariantCulture));
                }

                return Path.GetFullPath(pathTags.Expand(_outputPathTagExpression));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47051");
            }
        }

        /// <summary>
        /// Calculates first sub-document index (one-based) if the output path tag expression contains SubDocIndex tag></SubDocIndex>
        /// </summary>
        /// <param name="pageInfos">The PageInfos for the pages to be in the document.</param>
        /// <returns>The count + 1 of previously output documents for this source document or 0 if the output tag expression doesn't contain a SubDocIndex tag</returns>
        public int GetFirstSubDocIndex(IEnumerable<PageInfo> pageInfos)
        {
            try
            {
                int subDocIndex = 0;

                if (_outputPathTagExpression.Contains(PaginationSettings.SubDocIndexTag))
                {
                    var sourcePageInfo = pageInfos.Where(info => !info.Deleted).ToList();
                    string sourceDocName = sourcePageInfo.FirstOrDefault()?.DocumentName;
                    if (sourceDocName != null)
                    {
                        string query = string.Format(CultureInfo.InvariantCulture,
                            "SELECT COUNT(DISTINCT([DestFileID])) + 1 AS [SubDocIndex] " +
                            "   FROM [Pagination] " +
                            "   INNER JOIN [FAMFile] ON [Pagination].[SourceFileID] = [FAMFile].[ID] " +
                            "   WHERE [FileName] = '{0}'",
                            sourceDocName.Replace("'", "''"));

                        var recordset = _fileProcessingDB.GetResultsForQuery(query);
                        subDocIndex = (int)recordset.Fields["SubDocIndex"].Value;
                        recordset.Close();
                    }
                }
                return subDocIndex;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47288");
            }
        }

        /// <summary>
        /// Gets the priority to assign a paginated output file.
        /// </summary>
        /// <param name="pageInfos">The PageInfos for the pages to be in the document.</param>
        /// <param name="minimumPriority">The <see cref="EFilePriority"/> that should be assigned for the
        /// file except in the case that the source document(s) have a higher priority than specified here.</param>
        EFilePriority GetPriorityForFile(IEnumerable<PageInfo> pageInfos, EFilePriority minimumPriority)
        {
            try
            {
                var sourcePageInfo = pageInfos.Where(info => !info.Deleted).ToList();
                var sourceDocNames = string.Join(", ",
                    sourcePageInfo
                        .Select(page => "'" + page.DocumentName.Replace("'", "''") + "'")
                        .Distinct());

                string query = string.Format(CultureInfo.InvariantCulture,
                    "SELECT MAX([FAMFile].[Priority]) AS [MaxPriority] FROM [FileActionStatus]" +
                    "   INNER JOIN [FAMFile] ON [FileID] = [FAMFile].[ID]" +
                    "   WHERE [ActionID] = {0}" +
                    "   AND [FileName] IN ({1})", _actionID, sourceDocNames);

                var recordset = _fileProcessingDB.GetResultsForQuery(query);
                var priority = (EFilePriority)
                    Math.Max(
                        (int)minimumPriority,
                        (int)(EFilePriority)recordset.Fields["MaxPriority"].Value);
                recordset.Close();

                return priority;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47052");
            }
        }

        /// <summary>
        /// Resolves name conflicts and then adds the filename associated with argument <see paramref="e"/>
        /// to the FAM DB with the FileProcessingDB AddFileNoQueue method.</summary>
        /// <remarks>If the filename already exists on the file system or if the DB add fails because
        /// the file already exists in the DB, it will add 6 random chars before the extension and
        /// try to add that filename.</remarks>
        /// <param name="pageInfos">The PageInfos for the pages to be in the document.</param>
        /// <param name="tagManager">The tag manager that should be used to expand the path tag expression.</param>
        /// <param name="fileTaskSessionId">The file task session ID for the current pagination operation.</param>
        /// <param name="minimumPriority">The <see cref="EFilePriority"/> that should be assigned for the
        /// file except in the case that the source document(s) have a higher priority than specified here.</param>
        /// <returns>The ID of the newly added filename in the FAMFile table.</returns>
        public (int FileID, string FileName) AddFileWithNameConflictResolve(
            IEnumerable<PageInfo> pageInfos, FAMTagManager tagManager, int fileTaskSessionId, 
            EFilePriority minimumPriority)
        {
            try
            {
                var sourcePageInfo = pageInfos.Where(info => !info.Deleted).ToList();
                int pageCount = sourcePageInfo.Count;

                string outputFileName = GetPaginatedDocumentFileName(pageInfos, tagManager);
                var priority = GetPriorityForFile(pageInfos, minimumPriority);

                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(outputFileName));

                // First resolve conflict with file system
                if (File.Exists(outputFileName))
                {
                    var pathTags = new SourceDocumentPathTags(outputFileName);
                    outputFileName = pathTags.Expand(
                        "$InsertBeforeExt(<SourceDocName>,_$RandomAlphaNumeric(6))");
                }

                int fileID = -1;
                try
                {
                    fileID = _fileProcessingDB.AddFileNoQueue(
                        outputFileName, 0, pageCount, priority, _workflowID);
                }
                catch (Exception ex)
                {
                    // Query to see if the e.OutputFileName can be found in the database.
                    string query = string.Format(CultureInfo.InvariantCulture,
                        "SELECT [ID] FROM [FAMFile] WHERE [FileName] = '{0}'",
                        outputFileName.Replace("'", "''"));

                    var recordset = _fileProcessingDB.GetResultsForQuery(query);
                    bool fileExistsInDB = !recordset.EOF;
                    recordset.Close();
                    if (fileExistsInDB)
                    {
                        var pathTags = new SourceDocumentPathTags(outputFileName);
                        outputFileName = pathTags.Expand(
                            "$InsertBeforeExt(<SourceDocName>,_$RandomAlphaNumeric(6))");

                        fileID = _fileProcessingDB.AddFileNoQueue(
                            outputFileName, 0, pageCount, priority, _workflowID);
                    }
                    else
                    {
                        // The file was not in the database, the call failed for another reason.
                        throw ex.AsExtract("ELI47053");
                    }
                }

                if (fileTaskSessionId > 0)
                {
                    WritePaginationHistory(pageInfos, fileID, fileTaskSessionId);
                }

                return (fileID, outputFileName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47215");
            }
        }

        /// <summary>
        /// Records to the database the pagination that has occurred (which pages have been used in
        /// which output documents, which pages have been deleted).
        /// </summary>
        /// <param name="pageInfos">The PageInfos for the pages for which history should be recorded.</param>
        /// <param name="outputDocument">The ID of the output document that has been produced.</param>
        /// <param name="fileTaskSessionId">The file task session ID for the current pagination operation.</param>
        public void WritePaginationHistory(IEnumerable<PageInfo> pageInfos, int outputFileID, int fileTaskSessionId)
        {
            try
            {
                var firstSourceFileName = pageInfos
                    .OrderBy(sourcePage => !sourcePage.Deleted) // Prefer (but not require) a non-deleted source document
                    .FirstOrDefault()
                    ?.DocumentName;

                var sourcePageInfo = (firstSourceFileName == null)
                    ? null
                    : pageInfos
                        .Where(sourcePage => !sourcePage.Deleted)
                        .Select(sourcePage => new StringPairClass
                        {
                            StringKey = sourcePage.DocumentName,
                            StringValue = sourcePage.Page.ToString(CultureInfo.InvariantCulture)
                        })
                        .ToIUnknownVector();

                var deletedSourcePageInfo = pageInfos
                    .Where(sourcePage => sourcePage.Deleted)
                    .Select(sourcePage => new StringPairClass
                    {
                        StringKey = sourcePage.DocumentName,
                        StringValue = sourcePage.Page.ToString(CultureInfo.InvariantCulture)
                    })
                    .ToIUnknownVector();

                _fileProcessingDB.AddPaginationHistory(
                    outputFileID, sourcePageInfo, deletedSourcePageInfo, fileTaskSessionId);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47216");
            }
        }

        /// <summary>
        /// Links <see paramref="fileId"/> to the specified <paramref name="orders"/>
        /// and <paramref name="encounters"/> in the FAM DB.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void LinkFilesWithRecordIds(int fileId,
            IEnumerable<(string OrderNumber, DateTime? OrderDate)> orders,
            IEnumerable<(string EncounterNumber, DateTime? EncounterDate)> encounters)
        {
            try
            {
                if (orders?.Any() != true && encounters?.Any() != true)
                {
                    return;
                }

                ExtractException.Assert("ELI47250", "Failed to link order ID to document.", fileId > 0);
                ExtractException.Assert("ELI47251", "Failed to link encounter ID to document.", fileId > 0);

                using (var famData = new FAMData(_fileProcessingDB))
                {
                    orders?.ToList().ForEach(order =>
                        famData.LinkFileWithOrder(fileId, order.OrderNumber, order.OrderDate));

                    encounters?.ToList().ForEach(encounter =>
                        famData.LinkFileWithEncounter(fileId, encounter.EncounterNumber, encounter.EncounterDate));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47252");
            }
        }
    }
}
