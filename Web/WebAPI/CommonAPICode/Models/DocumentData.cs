using Extract;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_IMAGEUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using static WebAPI.Utils;
using AttributeDBMgr = AttributeDbMgrComponentsLib.AttributeDBMgr;
using ComAttribute = UCLID_AFCORELib.Attribute;
using EActionStatus = UCLID_FILEPROCESSINGLib.EActionStatus;
using System.Text.RegularExpressions;
using System.Linq;

namespace WebAPI.Models
{
    /// <summary>
    /// This class is the data model for the DocumentController.
    /// </summary>
    public sealed class DocumentData: IDisposable
    {
        const string _WEB_VERIFY_TASK_GUID = "FD7867BD-815B-47B5-BAF4-243B8C44AABB";

        ApiContext _apiContext;
        AttributeDBMgr _attributeDbMgr;
        FileApi _fileApi;
        ClaimsPrincipal _user;
        bool _endSessionOnDispose;

        // To be used for FAM DB operations that occur outside the context of a given DocumentData instance.
        static FileProcessingDB _utilityFileProcessingDB;
        static object _lock = new object();

        static ThreadLocal<MiscUtils> _miscUtils = new ThreadLocal<MiscUtils>(() => new MiscUtils());
        static ThreadLocal<ImageUtils> _imageUtils = new ThreadLocal<ImageUtils>(() => new ImageUtils());
        static ThreadLocal<ImageConverter> _imageConverter = new ThreadLocal<ImageConverter>(() => new ImageConverter());

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentData"/> class.
        /// </summary>
        /// <para><b>Note</b></para>
        /// This should be used only inside a using statement, so the fileApi in-use flag can be cleared.
        /// <param name="apiContext">The API context.</param>
        public DocumentData(ApiContext apiContext)
        {
            try
            {
                _apiContext = apiContext;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42162");
            }
        }

        /// <summary>
        /// Initializes a <see cref="DocumentData"/> instance.
        /// <para><b>Note</b></para>
        /// This should be used only inside a using statement, so the fileApi in-use flag can be cleared.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> this instance should be specific to.</param>
        /// <param name="requireSession"><c>true</c> if an active FAM session is required; otherwise, <c>false</c>.</param>
        public DocumentData(ClaimsPrincipal user, bool requireSession)
        {
            try
            {
                _apiContext = ClaimsToContext(user);
                _user = requireSession ? user : null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45276");
            }
        }

        /// <summary>
        /// Dispose - used to reset the FileApi in-use flag
        /// </summary>
        public void Dispose()
        {
            if (_attributeDbMgr != null)
            {
                _attributeDbMgr = null;
            }

            // Allow the fileApi object to be reused by clearing the InUse flag.
            if (_fileApi != null)
            {
                if (_endSessionOnDispose)
                {
                    try
                    {
                        _fileApi.EndSession();
                    }
                    catch { }
                }

                _fileApi.InUse = false;
                _fileApi = null;
            }
        }

        /// <summary>
        /// Opens a session for the specified ClaimsPrincipal.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> this instance is specific to.</param>
        /// <param name="remoteIpAddress">The IP address of the web application user.s</param>
        /// <param name="endSessionOnDispose">Whether to call EndSession() when this instance is disposed</param>
        public void OpenSession(ClaimsPrincipal claimsPrincipal, string remoteIpAddress, bool endSessionOnDispose)
        {
            var user = new User()
            {
                Username = claimsPrincipal.GetUsername(),
                WorkflowName = claimsPrincipal.GetClaim(Utils._WORKFLOW_NAME)
            };

            OpenSession(user, remoteIpAddress, endSessionOnDispose);
        }

        /// <summary>
        /// Opens a session for the specified user.
        /// </summary>
        /// <param name="user">The <see cref="User"/> this instance is specific to.</param>
        /// <param name="remoteIpAddress">The IP address of the web application user.s</param>
        /// <param name="endSessionOnDispose">Whether to call EndSession() when this instance is disposed</param>
        public void OpenSession(User user, string remoteIpAddress, bool endSessionOnDispose)
        {
            try
            {
                _endSessionOnDispose = endSessionOnDispose;
                FileApi.FileProcessingDB.RecordWebSessionStart(
                    "WebRedactionVerification", _apiContext.SessionId,
                    remoteIpAddress, user.Username);
                FileApi.FileProcessingDB.RegisterActiveFAM();

                // Once a FAM session has been established, tie the session to this context
                FileApi.AssignSession(_apiContext);

                _apiContext.FAMSessionId = FileApi.FAMSessionId;
            }
            catch (Exception ex)
            {
                FileApi.AbortSession();
                _fileApi = null;

                throw ex.AsExtract("ELI45225");
            }
        }

        /// <summary>
        /// Closes the web application session.
        /// </summary>
        public void CloseSession()
        {
            try
            {
                HTTPError.AssertRequest("ELI45234", _user != null, "No active user");

                // https://extract.atlassian.net/browse/ISSUE-16239
                // From the FAM's perspective UnregisterActiveFAM will close any current open document.
                // Don't manually close the document here; it will interfere with properly reverting
                // files to either the skipped or pending state.

                try
                {
                    FileApi.EndSession();
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI46270");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45229");
            }
        }

        /// <summary>
        /// Gets the settings for the web application
        /// </summary>
        public WebAppSettingsResult GetSettings()
        {
            try
            {
                var json = FileApi.FileProcessingDB.LoadWebAppSettings(-1, "RedactionVerificationSettings");

                var result = JsonConvert.DeserializeObject<WebAppSettingsResult>(json);

                if(!string.IsNullOrEmpty(result.DocumentTypes))
                {
                    result.ParsedDocumentTypes = File.ReadAllLines(result.DocumentTypes);
                }

                return result;
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI45272");
                ee.AddDebugData("Current User", System.Environment.UserDomainName);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the number of document, pages and active users in the current verification queue.
        /// </summary>
        public QueueStatusResult GetQueueStatus(string userName)
        {
            try
            {
                int actionId = FileApi.FileProcessingDB.GetActionID(FileApi.Workflow.EditAction);
                var stats = FileApi.FileProcessingDB.GetStats(actionId, false, true);
                var users = FileApi.FileProcessingDB.GetActiveUsers(FileApi.Workflow.EditAction);

                var result = new QueueStatusResult();

                result.PendingDocuments = stats.NumDocumentsPending;
                result.PendingPages = stats.NumPagesPending;
                result.ActiveUsers = users.Size;

                result.skippedDocumentsForCurrentUser = FileApi.FileProcessingDB.GetNumberSkippedForUser(userName, actionId, false);

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45525");
            }
        }

        /// <summary>
        /// Allows a user to change their password.
        /// </summary>
        public void ChangePassword(string userName, string oldPassword, string newPassword)
        {
            try
            {
                FileApi.FileProcessingDB.ChangePassword(userName, oldPassword, newPassword);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets a page of queued/skipped files for the workflow's Edit action
        /// </summary>
        /// <param name="userName">The currently logged in user's name</param>
        /// <param name="skippedFiles">Whether to return files skipped for this user rather than pending files</param>
        /// <param name="filter">Search string to filter the results</param>
        /// <param name="fromBeginning">Sort file IDs in ascending order before selecting the subset</param>
        /// <param name="pageIndex">Skip pageIndex * pageSize records from the beginning/end</param>
        /// <param name="pageSize">The maximum records to return</param>
        public QueuedFilesResult GetQueuedFiles(string userName, bool skippedFiles, string filter, bool fromBeginning, int pageIndex, int pageSize)
        {
            try
            {
                int wfID = FileApi.Workflow.Id;
                string action = FileApi.Workflow.EditAction;
                string joinSkipped = skippedFiles
                    ? Inv($"JOIN SkippedFile ON SkippedFile.FileID = FAMFile.ID")
                    : "";
                string skippedOrPendingClause = skippedFiles
                    ? Inv($"FileActionStatus.ActionStatus = 'S' AND SkippedFile.UserName = '{userName.Replace("'", "''")}'")
                    : "FileActionStatus.ActionStatus = 'P'";
                string offset = pageIndex >= 0 && pageSize > 0
                    ? Inv($"OFFSET {pageIndex * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY")
                    : "";
                string filterClause = "";
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var searchPattern = Inv($"%{filter.Replace("'", "''")}%");
                    filterClause = Inv($@"WHERE
                        OriginalFileName LIKE '{searchPattern}'
                        OR SubmittedByUser LIKE '{searchPattern}'
                        OR DocumentType LIKE '{searchPattern}'
                        OR DateSubmitted LIKE '{searchPattern}'
                        OR Comment LIKE '{searchPattern}'");
                }

                string sortDirection = fromBeginning ? "ASC" : "DESC";
                string query = Inv($@"
                   SELECT
                     ID,
                     Pages,
                     DateSubmitted,
                     Comment,
                     COALESCE(OriginalFileName, '') AS OriginalFileName,
                     COALESCE(SubmittedByUser, '') AS SubmittedByUser,
                     COALESCE(DocumentType, '') AS DocumentType
                   FROM
                   (SELECT
                     FAMFile.ID,
                     FAMFile.Pages,
                     CONVERT(VARCHAR(10), WorkflowFile.AddedDateTime, 23) As DateSubmitted,
                     COALESCE(CONVERT(VARCHAR(MAX), Comment), '') AS Comment,
                     MetadataField.Name AS MetadataName,
                     FileMetadataFieldValue.Value AS MetadataValue
                    FROM FAMFile
                      JOIN WorkflowFile ON WorkflowFile.FileID = FAMFile.ID
                      JOIN Action ON Action.WorkflowID = WorkflowFile.WorkflowID
                      JOIN FileActionStatus ON FileActionStatus.FileID = FAMFile.ID AND FileActionStatus.ActionID = Action.ID
                      {joinSkipped}
                      LEFT JOIN FileActionComment ON FileActionComment.FileID = FAMFile.ID
                      LEFT JOIN FileMetadataFieldValue ON FileMetadataFieldValue.FileID = FAMFile.ID
                      LEFT JOIN MetadataField ON MetadataField.ID = FileMetadataFieldValue.MetaDataFieldID
                      WHERE ASCName = '{action.Replace("'", "''")}'
                      AND WorkflowFile.WorkflowID = {wfID}
				      AND WorkflowFile.Deleted = 0
                      AND {skippedOrPendingClause}
                    ) AS SourceTable PIVOT(MIN(MetadataValue) FOR MetadataName IN ([OriginalFileName], [SubmittedByUser], [DocumentType])) AS PivotTable
                    {filterClause}
                    ORDER BY ID {sortDirection}
                    {offset}
                    ");

                var rs = FileApi.FileProcessingDB.GetResultsForQuery(query);
                var results = new List<QueuedFileDetails>();
                if (!rs.BOF && !rs.EOF)
                {
                    rs.MoveFirst();
                    while (!rs.EOF)
                    {
                        var record = new QueuedFileDetails();
                        record.FileID = (int)rs.Fields["ID"].Value;
                        record.NumberOfPages = (int)rs.Fields["Pages"].Value;
                        record.DateSubmitted = (string)rs.Fields["DateSubmitted"].Value;
                        record.OriginalFileName = (string)rs.Fields["OriginalFileName"].Value;
                        record.SubmittedByUser = (string)rs.Fields["SubmittedByUser"].Value;
                        record.DocumentType = (string)rs.Fields["DocumentType"].Value;
                        record.Comment = (string)rs.Fields["Comment"].Value;

                        results.Add(record);
                        rs.MoveNext();
                    }
                }
                rs.Close();

                return new QueuedFilesResult { QueuedFiles = results };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47048");
            }
        }


        /// <summary>
        /// Checkouts the document.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="processSkipped">If <paramref name="id"/> is -1, if this is <c>true</c> then the document to open
        /// will be the next one in the skipped queue for the user, if <c>false</c> the next document in the pending queue will be opend</param>
        /// <param name="dataUpdateOnly"><c>true</c> if the session is being opened only for updating data, in which case the session
        /// will be allowed for completed/failed files; otherwise, <c>false</c>.</param>
        /// <param name="userName"> The username of the user who is making the call. Only required for the recursive version </param>
        /// <param name="retries"> the number of times to retry. </param>
        /// <returns></returns>
        public DocumentIdResult OpenDocument(int id, bool processSkipped = false, bool dataUpdateOnly = false, string userName = "", int retries = 10)
        {
            try
            {
                ExtractException.Assert("ELI45235", "No active user", _user != null);

                // Per GGK request, if a document is already open, return the already open ID
                // without error
                // https://extract.atlassian.net/browse/WEB-55
                // Modified to only return open document if there is no file ID specified so that you can open a specific file with, e.g., http://localhost:4205/?docid=11
                if (FileApi.DocumentSession.IsOpen && (id < 0 || id == FileApi.DocumentSession.FileId))
                {
                    return new DocumentIdResult()
                    {
                        Id = FileApi.DocumentSession.FileId
                    };
                }

                IFileRecord fileRecord = null;
                if (id > 0)
                {
                    AssertRequestFileId("ELI45263", id);

                    fileRecord = FileApi.FileProcessingDB.GetFileToProcess(id, FileApi.Workflow.EditAction,
                        processSkipped ? "S":"P");

                    // https://extract.atlassian.net/browse/ISSUE-16748
                    // If the session is being opened only for a put/patch data call, allow the session for completed and failed files.
                    if (dataUpdateOnly && fileRecord == null)
                    {
                        fileRecord = FileApi.FileProcessingDB.GetFileToProcess(id, FileApi.Workflow.EditAction, "C");
                    }
                    if (dataUpdateOnly && fileRecord == null)
                    {
                        fileRecord = FileApi.FileProcessingDB.GetFileToProcess(id, FileApi.Workflow.EditAction, "F");
                    }

                    HTTPError.Assert("ELI46297", StatusCodes.Status423Locked, fileRecord != null,
                        "Document is not queued or is locked by another process.", ("FileId", id, true));
                }
                else
                {
                    var fileRecords = FileApi.FileProcessingDB.GetFilesToProcess(FileApi.Workflow.EditAction, 1, processSkipped, _user.GetUsername());
                    if (fileRecords.Size() == 0)
                    {
                        if (retries > 0 )
                        {
                            var queueStatus = GetQueueStatus(userName);
                            if ((processSkipped ? queueStatus.skippedDocumentsForCurrentUser : queueStatus.PendingDocuments) > 0)
                            {
                                ExtractException retryException = new ExtractException("ELI47262", "Application Trace: Retry open document.");
                                retryException.AddDebugData("Remaining Retries", retries);
                                retryException.Log();

                                return OpenDocument(id, processSkipped, false, userName, --retries);
                            }
                        }

                        return new DocumentIdResult()
                        {
                            Id = -1
                        };
                    }
                    fileRecord = (IFileRecord)fileRecords.At(0);
                }

                var documentId = new DocumentIdResult()
                {
                    Id = fileRecord.FileID
                };

                FileApi.DocumentSession =
                (
                    true,
                    FileApi.FileProcessingDB.StartFileTaskSession(_WEB_VERIFY_TASK_GUID, documentId.Id, fileRecord.ActionID),
                    documentId.Id,
                    DateTime.Now
                );

                return documentId;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45236");
            }
        }

        /// <summary>
        /// Releases the document.
        /// </summary>
        /// <param name="setStatusTo"><see cref="EActionStatus.kActionCompleted"/> to commit the document so that it advances in the
        /// workflow; other values to save the document but set the file's status in the EditAction to a non-completed value.</param>
        /// <param name="duration">Optional duration, in ms, to use for updating the file task session record</param>
        /// <param name="exception">Optional exception for logging if <see paramref="setStatusTo"/> is <see cref="EActionStatus.kActionFailed"/></param>
        public void CloseDocument(EActionStatus setStatusTo, int duration = -1, Exception exception = null)
        {
            try
            {
                ExtractException.Assert("ELI45238", "No active user", _user != null);
                ExtractException.Assert("ELI46669", "No open document", FileApi.DocumentSession.IsOpen);

                double durationInSeconds = duration > 0
                    ? duration / 1000.0
                    : (DateTime.Now - FileApi.DocumentSession.StartTime).TotalSeconds;

                FileApi.FileProcessingDB.EndFileTaskSession(FileApi.DocumentSession.Id,
                    durationInSeconds, 0, 0);

                int fileId = FileApi.DocumentSession.FileId;
                if (setStatusTo == EActionStatus.kActionCompleted)
                {
                    FileApi.FileProcessingDB.NotifyFileProcessed(fileId, FileApi.Workflow.EditAction, -1, true);
                }
                else if (setStatusTo == EActionStatus.kActionSkipped)
                {
                    FileApi.FileProcessingDB.NotifyFileSkipped(fileId, FileApi.Workflow.EditAction, -1, true);
                }
                else if (setStatusTo == EActionStatus.kActionFailed)
                {
                    string exceptionString = null;
                    if (exception != null)
                    {
                        try
                        {
                            exceptionString = exception.AsExtract("ELI46612").AsStringizedByteStream();
                        }
                        catch { }
                    }
                    FileApi.FileProcessingDB.NotifyFileFailed(fileId, FileApi.Workflow.EditAction, -1, exceptionString, true);
                }
                else
                {
                    FileApi.FileProcessingDB.SetStatusForFile(fileId, FileApi.Workflow.EditAction, -1,
                        setStatusTo, false, true, out EActionStatus oldStatus);
                }

                try
                {
                    if (setStatusTo == EActionStatus.kActionCompleted && !string.IsNullOrWhiteSpace(FileApi.Workflow.PostEditAction))
                    {
                        // Note: SetFileStatusToPending will immediately change the status even if the file is
                        // processing, which can result the file being stuck since it won't be removed from
                        // the LockedFile table when processing completes. SetStatusForFile allows
                        // vbQueueChangeIfProcessing to be specified.
                        FileApi.FileProcessingDB.SetStatusForFile(fileId, FileApi.Workflow.PostEditAction,
                            FileApi.Workflow.Id, EActionStatus.kActionPending,
                            vbQueueChangeIfProcessing: true, vbAllowQueuedStatusOverride: false,
                            out var eActionStatus);
                    }
                }
                finally
                {
                    FileApi.DocumentSession = (false, 0, 0, new DateTime());
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45240");
            }
        }

        /// <summary>
        /// Marks the document as deleted in the workflow. Does not necessarily mean the document is
        /// physically deleted, though depending on how the workflow is configured, it could be.
        /// </summary>
        public void DeleteDocument(int fileId)
        {
            try
            {
                AssertRequestFileId("ELI46346", fileId);

                FileApi.FileProcessingDB.MarkFileDeleted(fileId, FileApi.Workflow.Id);

                if (!string.IsNullOrWhiteSpace(FileApi.Workflow.PostWorkflowAction))
                {
                    // Note: SetFileStatusToPending will immediately change the status even if the file is
                    // processing, which can result the file being stuck since it won't be removed from
                    // the LockedFile table when processing completes. SetStatusForFile allows
                    // vbQueueChangeIfProcessing to be specified.
                    FileApi.FileProcessingDB.SetStatusForFile(fileId, FileApi.Workflow.PostWorkflowAction,
                            FileApi.Workflow.Id, EActionStatus.kActionPending,
                            vbQueueChangeIfProcessing: true, vbAllowQueuedStatusOverride: false,
                            out var eActionStatus);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46292");
            }
        }

        /// <summary>
        /// Gets the document attribute set
        /// </summary>
        /// <param name="fileId">The ID of the file for which to retrieve data.</param>
        /// <param name="includeNonSpatial"><c>true</c> to include non-spatial attributes in the resulting data;
        /// otherwise, <c>false</c>. NOTE: If false, a non-spatial attribute will be excluded even if it has
        /// spatial children.</param>
        /// <param name="verboseSpatialData"><c>false</c> to include only the spatial data needed for
        /// extract software to represent spatial strings; <c>true</c> to include data that may be
        /// useful to 3rd party integrators.</param>
        /// <param name="splitMultiPageAttributes"><c>true</c> to split multi-page attributes into a separate
        /// attribute for every page; <c>false</c> to map multi-page attributes as they are.</param>
        /// <param name="cacheData">Specifies if the attribute data for this file should be cached
        /// as a side effect of the call. Caching is required if data is to be edited via <see cref="EditPageData"/>
        /// and <see cref="CommitCachedDocumentData"/>. In order to be cached,
        /// <see paramref="splitMultiPageAttributes"/> must be <c>true</c>.</param>
        /// <returns>DocumentAttributeSet instance, including error info iff there is an error</returns>
        public DocumentDataResult GetDocumentData(int fileId, 
            bool includeNonSpatial, bool verboseSpatialData, bool splitMultiPageAttributes,
            bool cacheData)
        {
            try
            {
                AssertRequestFileId("ELI46348", fileId);

                var results = GetAttributeSetForFile(fileId);
                var mapper = new AttributeMapper(results, FileApi.Workflow.Type);

                var documentData = mapper.MapAttributesToDocumentAttributeSet(
                    includeNonSpatial, verboseSpatialData, splitMultiPageAttributes);

                if (cacheData)
                {
                    ExtractException.Assert("ELI49513", "Attribute data caching supported by page only.",
                        splitMultiPageAttributes);

                    var pagesOfAttributes = documentData.Attributes
                        .Where(attribute => attribute.HasPositionInfo == true)
                        .GroupBy(attribute => attribute.SpatialPosition.Pages.Single())
                        .ToDictionary(group => group.Key, group => group.ToList());

                    var mapOfAttributes = new StrToStrMap();
                    var fileName = GetSourceFileName(fileId);
                    var pageCount = _imageUtils.Value.GetPageCount(fileName);
                    for (int page = 1; page <= pageCount; page++)
                    {
                        string attributeJSON = "";
                        if (pagesOfAttributes.TryGetValue(page, out var attributes))
                        {
                            attributeJSON = JsonConvert.SerializeObject(attributes);
                        }

                        mapOfAttributes.Set(page.ToString(), attributeJSON);
                    }

                    // bOverwriteModifiedData == false because we don't want to overwrite uncommitted changes with data
                    // from the last stored attribute set. This ensures when this is called via a browser refresh that
                    // changes aren't lost.
                    // https://extract.atlassian.net/browse/ISSUE-16827
                    FileApi.FileProcessingDB.CacheAttributeData(FileApi.DocumentSession.Id, 
                        mapOfAttributes, bOverwriteModifiedData: false);
                }

                return documentData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42124");
            }
        }

        /// <summary>
        /// Replaces the document attribute set.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="inputData">The updated data.</param>
        public void PutDocumentResultSet(int fileId, DocumentDataInput inputData)
        {
            try
            {
                AssertRequestFileId("ELI46349", fileId);

                string fileName = AttributeDbMgr.FAMDB.GetFileNameFromFileID(fileId);
                var translator = new AttributeTranslator(fileName, inputData.Attributes);

                UpdateDocumentData(fileId, translator.ComAttributes);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44889");
            }
        }

        /// <summary>
        /// Edits a given page of a document by replacing the existing attributes with the specified
        /// <see paramref="inputData"/>.
        /// <para><b>Note</b></para>
        /// Edits made via this call will only become part of a new attribute set after calling
        /// <see cref="CommitCachedDocumentData"/>.
        /// </summary>
        /// <param name="fileId">The ID of the file for which to edit data.</param>
        /// <param name="page">The page number for which data is to be edited.</param>
        /// <param name="inputData">The new <see cref="DocumentAttribute"/>s to apply to the page.</param>
        public void EditPageData(int fileId, int page, List<DocumentAttribute> inputData)
        {
            try
            {
                AssertDocumentSession("ELI49465");
                AssertRequestFileId("ELI46349", fileId);

                FileApi.FileProcessingDB.CacheFileTaskSessionData(FileApi.DocumentSession.Id, page,
                    null, null, null, JsonConvert.SerializeObject(inputData), null, vbCrucialUpdate: true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49512");
            }
        }

        /// <summary>
        /// Commits all document data edits made via <see cref="EditPageData"/> to a new attribute
        /// set for the file.
        /// </summary>
        /// <param name="fileId">The ID of the file for which edited data should be committed.</param>
        public void CommitCachedDocumentData(int fileId)
        {
            try
            {
                AssertRequestFileId("ELI49553", fileId);
                AssertDocumentSession("ELI49554");

                AttributeDbMgr.FAMDB.GetCachedFileTaskSessionData(FileApi.DocumentSession.Id, -1,
                    ECacheDataType.kAttributes, true,
                    out _, out _, out _, out string cachedData, out _);

                string fileName = AttributeDbMgr.FAMDB.GetFileNameFromFileID(fileId);
                var documentData = JsonConvert.DeserializeObject<List<DocumentAttribute>>(cachedData);
                var translator = new AttributeTranslator(fileName, documentData);

                UpdateDocumentData(fileId, translator.ComAttributes);
                FileApi.FileProcessingDB.MarkAttributeDataUnmodified(FileApi.DocumentSession.Id);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49555");
            }
        }

        /// <summary>
        /// Gets all pages of uncommitted data edits from document sessions other than this one
        /// so long as a new attribute set has not been stored for the document more recently than
        /// the edits were made.
        /// </summary>
        /// <param name="fileId">The ID of the file for which uncommitted edits should be retrieved.</param>
        /// <returns>A <see cref="UncommittedDocumentDataResult"/> representing the uncommitted edits.</returns>
        public UncommittedDocumentDataResult GetUncommittedDocumentData(int fileId)
        {
            try
            {
                AssertRequestFileId("ELI49521", fileId);
                AssertDocumentSession("ELI49542");

                var result = new UncommittedDocumentDataResult();
                var mostRecentDateTime = new DateTime(0);

                int actionId = FileApi.FileProcessingDB.GetActionID(FileApi.Workflow.EditAction);
                var attrSetName = FileApi.Workflow.OutputAttributeSet;
                var uncommittedDocumentData = FileApi.FileProcessingDB.GetUncommittedAttributeData(
                    fileId, actionId, attrSetName);
                int nCount = uncommittedDocumentData.Size();

                // Each item in uncommittedDocumentData is a variant vector with the following fields:
                // 0 FullUserName: (user that applied edit)
                // 1 AttributeDataModifiedTime: (time of the edit)
                // 2 Page: page number that was edited
                // 3 AttributeData: JSON representation of the attributes.
                var pageDictionary = new Dictionary<int, List<DocumentAttribute>>();
                for (int i = 0; i < nCount; i++)
                {
                    var pageData = (IVariantVector)uncommittedDocumentData.At(i);

                    if (i == 0)
                    {
                        result.UserName = (string)pageData[0];
                    }
                    var pageModifiedTime = (DateTime)pageData[1];
                    mostRecentDateTime = new DateTime(Math.Max(mostRecentDateTime.Ticks, pageModifiedTime.Ticks));
                    int pageNum = (int)pageData[2];
                    var pageAttributes = JsonConvert.DeserializeObject<List<DocumentAttribute>>((string)pageData[3]);
                    pageDictionary[pageNum] = pageAttributes;
                }

                // Use the most recently edited page to represent the 
                result.ModifiedDateTime = mostRecentDateTime.ToString("dddd, MMMM dd yyyy HH:mm tt");

                result.UncommittedPagesOfAttributes = pageDictionary
                    .Select(page => new PageOfAttributes() { PageNumber = page.Key, Attributes = page.Value })
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49529");
            }
        }

        /// <summary>
        /// Deletes all cache data rows that are not associated with this document session.
        /// </summary>
        /// <param name="fileId">The ID of the file for which cache data is to be deleted.</param>
        public void DiscardOldCacheData(int fileId)
        {
            try
            {
                AssertRequestFileId("ELI49556", fileId);
                AssertDocumentSession("ELI49557");

                int actionId = FileApi.FileProcessingDB.GetActionID(FileApi.Workflow.EditAction);
                FileApi.FileProcessingDB.DiscardOldCacheData(fileId, actionId, FileApi.DocumentSession.Id);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49558");
            }
        }

        /// <summary>
        /// Patches attributes in the existing document attribute set.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="patchData">The updated data.</param>
        public void PatchDocumentData(int fileId, DocumentDataPatch patchData)
        {
            try
            {
                AssertRequestFileId("ELI46350", fileId);

                var existingData = GetAttributeSetForFile(fileId);
                string fileName = AttributeDbMgr.FAMDB.GetFileNameFromFileID(fileId);
                var translator = new AttributeTranslator(fileName, existingData, patchData);

                UpdateDocumentData(fileId, translator.ComAttributes);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44889");
            }
        }

        /// <summary>
        /// Returns the metadata field value from the open document.
        /// </summary>
        /// <param name="metaDataField">The field to obtain the value from</param>
        /// <returns></returns>
        public MetadataFieldResult GetMetadataField(string metaDataField)
        {
            try
            {
                string metadataValue = FileApi.FileProcessingDB.GetMetadataFieldValue(this.DocumentSessionFileId, metaDataField);
                return new MetadataFieldResult { Value = metadataValue };
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI47181");
            }
        }

        /// <summary>
        /// Sets the metadatafield value in the database.
        /// </summary>
        /// <param name="metadataField">The metadata field to assign</param>
        /// <param name="metadataFieldValue">The metadatafield value</param>
        public void SetMetadataField(string metadataField, string metadataFieldValue)
        {
            try
            {

                try
                {
                    FileApi.FileProcessingDB.SetMetadataFieldValue(this.DocumentSessionFileId, metadataField, metadataFieldValue);
                }
                catch
                {
                    HTTPError.Assert("ELI47203", StatusCodes.Status404NotFound,
                        GetMetadataField(metadataField).Value != null,
                        Inv($"The metadata field: {metadataField} is not present in the database"));

                    throw;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47201");
            }
        }

        /// <summary>
        /// Sets the FileActionComment of the EditAction for the open file
        /// </summary>
        /// <param name="comment">The comment text to apply</param>
        public void SetComment(string comment)
        {
            try
            {
                ExtractException.Assert("ELI46694", "No open document", FileApi.DocumentSession.IsOpen);

                int fileId = FileApi.DocumentSession.FileId;
                int actionId = FileApi.FileProcessingDB.GetActionID(FileApi.Workflow.EditAction);
                FileApi.FileProcessingDB.SetFileActionComment(fileId, actionId, comment);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46695");
            }
        }

        /// <summary>
        /// Gets the FileActionComment of the EditAction for the open file
        /// </summary>
        public CommentData GetComment()
        {
            try
            {
                ExtractException.Assert("ELI46716", "No open document", FileApi.DocumentSession.IsOpen);

                int fileId = FileApi.DocumentSession.FileId;
                int actionId = FileApi.FileProcessingDB.GetActionID(FileApi.Workflow.EditAction);
                string comment = FileApi.FileProcessingDB.GetFileActionComment(fileId, actionId);
                return new CommentData { Comment = comment };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46717");
            }
        }

        /// <summary>
        /// get the specified file attribute set
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <returns>IUnknownVector (attribute)</returns>
        IUnknownVector GetAttributeSetForFile(int fileId)
        {
            try
            {
                AssertRequestFileId("ELI46351", fileId);

                var attrSetName = FileApi.Workflow.OutputAttributeSet;

                try
                {
                    HTTPError.Assert("ELI46333", StatusCodes.Status404NotFound,
                        !String.IsNullOrWhiteSpace(attrSetName),
                        "Document data not found for file",
                        ("FileId", fileId, true), ("Workflow", FileApi.Workflow.Name, true));
                    HTTPError.Assert("ELI46334", AttributeDbMgr != null, "AttributeDbMgr is null");
                }
                catch (Exception ex)
                {
                    var ee = ex.AsExtract("ELI43651");
                    ee.AddDebugData("MissingResource", ee.Message, false);
                    throw ee;
                }

                const int mostRecentSet = -1;
                try
                {
                    var results = AttributeDbMgr.GetAttributeSetForFile(fileId,
                                                                     attributeSetName: attrSetName,
                                                                     relativeIndex: mostRecentSet,
                                                                     closeConnection: true);
                    return results;
                }
                catch (Exception ex)
                {
                    var ee = ex.AsExtract("ELI46415");
                    if (ee.EliCode == "ELI38763")
                    {
                        throw new HTTPError("ELI46416", StatusCodes.Status404NotFound,
                            "Document data not found", ee);
                    }

                    throw ex;
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42106");
                ee.AddDebugData("FileID", fileId, encrypt: false);
                ee.AddDebugData("AttributeSetName", FileApi.Workflow.OutputAttributeSet, encrypt: false);
                ee.AddDebugData("Workflow", FileApi.Workflow.Name, encrypt: false);

                throw ee;
            }
        }

        /// <summary>
        /// The word zone data, grouped by line.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="page">The page.</param>
        public WordZoneDataResult GetWordZoneData(int fileId, int page)
        {
            SpatialString pageData = null;

            try
            {
                HTTPError.AssertRequest("ELI46420",
                    page > 0, "Invalid page number",
                    ("Page", page, true));

                string wordZoneDataJson = null;
                try
                {
                    // First, check to see if word zone data is cached.
                    FileApi.FileProcessingDB.GetCachedFileTaskSessionData(FileApi.DocumentSession.Id, page,
                        ECacheDataType.kWordZone, false,
                        out _, out _, out wordZoneDataJson, out _, out _);
                }
                catch (Exception ex)
                {
                    // A failure retrieving cached data shouldn't be cause to report back to caller with a
                    // failure; log the error, then instead retrieve the data from the source files.
                    var ee = new ExtractException("ELI49459", "Failed to retrieve cached data", ex);
                    ee.Log();
                }

                if (!string.IsNullOrWhiteSpace(wordZoneDataJson))
                {
                    // Surprisingly, returning the JSON directly appeared to be slower than serializing here
                    // (which will be de-serialized for transport).
                    return JsonConvert.DeserializeObject<WordZoneDataResult>(wordZoneDataJson);
                }

                pageData = GetUssData(fileId, page);

                HTTPError.Assert("ELI45362", StatusCodes.Status404NotFound,
                    pageData != null,
                    "Word data is not available for document");

                HTTPError.Assert("ELI46421", StatusCodes.Status404NotFound,
                    !String.IsNullOrWhiteSpace(pageData.String), "Page not found",
                    ("Page", page, true), ("Test", "value", false));

                var wordZoneData = pageData.MapSpatialStringToWordZoneData();

                return new WordZoneDataResult
                {
                    Zones = wordZoneData
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42124");
            }
            finally
            {
                if (pageData != null)
                {
                    Marshal.FinalReleaseComObject(pageData);
                }
            }
        }

        /// <summary>
        /// SubmitFile implementation for unit testing
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="fileStream">filestream object</param>
        /// <returns>DocumentSubmitResult instance that contains error info iff an error occurs</returns>
        public DocumentIdResult SubmitFile(string fileName, Stream fileStream)
        {
            try
            {
                HTTPError.AssertRequest("ELI46335", !String.IsNullOrWhiteSpace(fileName), "File name is empty");

                var workflow = FileApi.Workflow;
                var uploads = workflow.DocumentFolder;
                HTTPError.Assert("ELI46336", !String.IsNullOrWhiteSpace(uploads), "Target location not configured.");

                var fullPath = GetSafeFilename(uploads, fileName);

                string directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    fileStream.CopyTo(fs);
                    fs.Close();
                }

                return AddFile(fullPath);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43242");
            }
        }

        /// <summary>
        /// get a "safe" filename - appends a GUID so there is never a file name collision issue
        /// </summary>
        /// <param name="path">The path of the file</param>
        /// <param name="filename">The (base) name of the file, including extension</param>
        /// <returns>a filename that can be used safely</returns>
        static string GetSafeFilename(string path, string filename)
        {
            HTTPError.Assert("ELI46337", !String.IsNullOrWhiteSpace(path) && 
                            !String.IsNullOrWhiteSpace(filename), 
                            "Either path or filename is empty");

            string filenameOnly = Path.GetFileName(filename);
            string guid = Guid.NewGuid().ToString();
            string newName = Inv($"{guid}_{filenameOnly}");

            // https://extract.atlassian.net/browse/ISSUE-15733
            // In order to prevent performance issues from having too many files in the same folder, 
            // create a folder hierarchy 2 levels deep with 256 * 4096 = 1M potential buckets where it is
            // very unlikely to have more than 1K items in any one folder.
            string fullfilename = Path.Combine(path, guid.Substring(0,2), guid.Substring(2,3), newName);

            return fullfilename;
        }

        /// <summary>
        /// Add file - this encapsulates fileProcessingDB.AddFile
        /// </summary>
        /// <param name="fullPath">path + filename - path is expected to exist at this point</param>
        /// <param name="caller">caller of this method - DO NOT SET, specified by compiler</param>
        /// <returns>DocumentSubmitresult instance that contains error info iff an error has occurred</returns>
        public DocumentIdResult AddFile(string fullPath,
                                            [CallerMemberName] string caller = "")
        {
            try
            {
                HTTPError.Assert("ELI46619", "Session closed", FileApi.FAMSessionId != 0);

                // Now add the file to the FAM queue
                var fileProcessingDB = FileApi.FileProcessingDB;
                var workflow = FileApi.Workflow;
                HTTPError.Assert("ELI46338", !String.IsNullOrWhiteSpace(workflow.StartAction),
                    "Workfow must have a start action", ("Workflow", FileApi.WorkflowName, true));

                var fileRecord =
                    FileApi.FileProcessingDB.AddFile(
                        fullPath,                                                 // full path to file
                        workflow.StartAction,                                     // action name
                        workflow.Id,                                              // workflow ID
                        EFilePriority.kPriorityNormal,                            // file priority
                        false,                                                    // force status change
                        false,                                                    // file modified
                        UCLID_FILEPROCESSINGLib.EActionStatus.kActionPending,     // action status
                        false,                                                    // skip page count
                        out bool bAlreadyExists,                                  // returns whether file already existed
                        out EActionStatus previousActionStatus);                  // returns the previous action status (if file already existed)

                //try
                //{
                //    FileApi.FileProcessingDB.SetMetadataFieldValue(fileRecord.FileID, "UploadedFromPath", fullPath);
                //}
                //catch { }

                //try
                //{
                //    FileApi.FileProcessingDB.SetMetadataFieldValue(fileRecord.FileID, "Uploader", _user.GetUsername());
                //}
                //catch { }

                DocumentIdResult result = new DocumentIdResult()
                {
                    Id = fileRecord.FileID
                };

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43331");
            }
        }

        /// <summary>
        /// Updates the document data for the specified fileId in the FAM DB.
        /// </summary>
        /// <param name="fileId">The FAM file ID for which data should be updated.</param>
        /// <param name="fileData">The file data.</param>
        void UpdateDocumentData(int fileId, IUnknownVector fileData)
        {
            try
            {
                AssertDocumentSession("ELI45297");

                AttributeDbMgr.CreateNewAttributeSetForFile(
                    FileApi.DocumentSession.Id, FileApi.Workflow.OutputAttributeSet, fileData,
                    vbStoreDiscreteFields: false,
                    vbStoreRasterZone: false,
                    vbStoreEmptyAttributes: true,
                    closeConnection: false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45084");
            }
        }

        /// <summary>
        /// implementation for SubmitText
        /// </summary>
        /// <param name="submittedText">text to submit</param>
        /// <returns>DocumentSubmitResult instance that contains error info iff an error occurs</returns>
        public DocumentIdResult SubmitText(string submittedText)
        {
            try
            {
                HTTPError.AssertRequest("ELI46340", !String.IsNullOrEmpty(submittedText),
                    "Text not provided");
                var workflow = FileApi.Workflow; 
                var uploads = workflow.DocumentFolder;
                HTTPError.Assert("ELI46339", !String.IsNullOrWhiteSpace(uploads),
                    "Target location not configured");

                var fullPath = GetSafeFilename(uploads, "SubmittedText.txt");

                string directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var fs = new FileStream(fullPath, FileMode.Create))
                using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
                {
                    sw.Write(submittedText);
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                }

                return AddFile(fullPath);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43243");
            }
        }

        /// <summary>
        /// implementation of GetStatus
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <returns>List of ProcessingStatus, can contain error info</returns>
        public ProcessingStatusResult GetStatus(int fileId)
        {
            try
            {
                AssertRequestFileId("ELI46352", fileId);

                EActionStatus status = EActionStatus.kActionFailed;

                try
                {
                    status = FileApi.FileProcessingDB.GetWorkflowStatus(fileId);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI42109");
                }

                return new ProcessingStatusResult()
                {
                    DocumentStatus = ConvertToStatus(status, fileId),
                    StatusText = Enum.GetName(typeof(DocumentProcessingStatus),
                        ConvertToStatus(status, fileId)),
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43244");
            }
        }


        static DocumentProcessingStatus ConvertToStatus(EActionStatus actionStatus, int fileId)
        {
            switch (actionStatus)
            {
                case EActionStatus.kActionCompleted:
                    return DocumentProcessingStatus.Done;

                case EActionStatus.kActionFailed:
                    return DocumentProcessingStatus.Failed;

                case EActionStatus.kActionProcessing:
                    return DocumentProcessingStatus.Processing;

                case EActionStatus.kActionUnattempted:
                    return DocumentProcessingStatus.Incomplete;

                default:
                    return DocumentProcessingStatus.NotApplicable;
            }
        }

        /// <summary>
        /// GetSourceFileName 
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <returns>the full path + filename of the original source file</returns>
        public string GetSourceFileName(int fileId)
        {
            string filename = "";

            try
            {
                AssertRequestFileId("ELI46353", fileId);

                filename = FileApi.FileProcessingDB.GetFileNameFromFileID(fileId);

                HTTPError.Assert("ELI46342", StatusCodes.Status404NotFound,
                    File.Exists(filename), "File does not exist",
                    ("fileId", fileId, true),
                    ("Filename", filename, false));

                return filename;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42110");
            }
        }

        /// <summary>
        /// Gets the pages information.
        /// </summary>
        /// <param name="fileId">The file ID</param>
        public PagesInfoResult GetPagesInfo(int fileId)
        {
            AssertRequestFileId("ELI46354", fileId);

            var pagesInfo = new PagesInfoResult()
            {
                PageInfos = new List<PageInfo>()
            };

            try
            {
                var fileName = GetSourceFileName(fileId);

                HTTPError.Assert("ELI46433", StatusCodes.Status404NotFound,
                    Path.GetExtension(fileName) != ".txt",
                    "Page info does not exist for text documents");

                IIUnknownVector spatialPageInfos = _imageUtils.Value.GetSpatialPageInfos(fileName);
                int count = spatialPageInfos.Size();
                for (int i = 0; i < count; i++)
                {
                    pagesInfo.PageInfos.Add(new PageInfo(i + 1, (SpatialPageInfo)spatialPageInfos.At(i)));
                }

                pagesInfo.PageCount = pagesInfo.PageInfos.Count;

                return pagesInfo;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45016");
            }
        }

        /// <summary>
        /// Gets the page image.
        /// <para><b>Note</b></para>
        /// This call has the side-effect of triggering the caching of data for the subsequent document page.
        /// </summary>
        /// <param name="pageNum">The page number.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="cacheData"><c>true</c> to cache the image and uss data for the next page as
        /// a side effect of this call.</param>
        /// <returns>An array of bytes representing a PDF image of the page.</returns>
        public byte[] GetPageImage(int fileId, int pageNum, bool cacheData)
        {
            try
            {
                AssertRequestFileId("ELI46355", fileId);

                AssertRequestFileId("ELI45172", fileId);
                AssertFileExists("ELI45173", fileId);
                AssertRequestFilePage("ELI45174", fileId, pageNum);

                var fileName = GetSourceFileName(fileId);

                // First, check the cache.
                Array cachedImageData = new byte[0];
                try
                {
                    FileApi.FileProcessingDB.GetCachedFileTaskSessionData(FileApi.DocumentSession.Id, pageNum,
                        ECacheDataType.kImage, false,
                        out cachedImageData, out _, out _, out _, out _);
                }
                catch (Exception ex)
                {
                    // A failure retrieving cached data shouldn't be cause to report back to caller with a
                    // failure; log the error, then instead retrieve the data from the source files.
                    var ee = new ExtractException("ELI49460", "Failed to retrieve cached data", ex);
                    ee.Log();
                }

                // Set sessionId variables for closure of the Task.Run lamda
                int sessionId = FileApi.DocumentSession.Id;

                if (cacheData)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            var cachedPages = UtilityFileProcessingDB.GetCachedPageNumbers(
                                sessionId, ECacheDataType.kImage);

                            // Trigger data cache for the first page and also for the subsequent page
                            // (if not already cached)
                            int[] pagesToCache = (pageNum == 1)
                                ? new[] { 2, 1 }   // Prioritize caching 2nd page over the first as the second would be need first.
                                : new[] { pageNum + 1 };
                            int pageCount = _imageUtils.Value.GetPageCount(fileName);
                            pagesToCache = pagesToCache
                                .Where(p => p <= pageCount)
                                .Except((int[])cachedPages)
                                .ToArray();

                            foreach (int page in pagesToCache)
                            {
                                CachePageData(sessionId, fileId, page);
                            }
                        }
                        catch (Exception cacheException)
                        {
                            // Since the ability to access cached data is not critical, log the
                            // exception but otherwise ignore.
                            // If needed, exceptions will be stored in the cache table where the
                            // data would have been.
                            var eeAppTrace = new ExtractException("ELI49468",
                                "Application Trace: Cache operation did not succeed.", cacheException);
                            eeAppTrace.AddDebugData("Session ID", sessionId, false);
                            eeAppTrace.AddDebugData("File ID", fileId, false);
                            eeAppTrace.AddDebugData("Page", pageNum, false);
                            eeAppTrace.AddDebugData("Note",
                                "If logged during unit test execution, this is not likely an indication of a problem.",
                                true);
                            eeAppTrace.Log();
                        }
                    });
                }

                if (cachedImageData != null && cachedImageData.Length > 0)
                {
                    return (byte[])cachedImageData;
                }
                else
                {
                    byte[] imageData = (byte[])_imageConverter.Value.GetPDFImage(fileName, pageNum, true);

                    return imageData;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45264");
            }
        }

        /// <summary>
        /// GetResult - used by several API calls
        /// </summary>
        /// <param name="fileId">the database file id to use</param>
        /// <returns>returns a tuple of filename, error flag, error message</returns>
        public (string filename, bool error, string errorMessage) GetResult(int fileId)
        {
            string filename = "";
            string getFileTag = "";

            try
            {
                AssertRequestFileId("ELI46356", fileId);

                try
                {
                    getFileTag = FileApi.Workflow.OutputFileMetadataField;
                    HTTPError.Assert("ELI46343", !String.IsNullOrWhiteSpace(getFileTag),
                        "Workflow not configured to provide output data");

                    filename = FileApi.FileProcessingDB.GetMetadataFieldValue(fileId, getFileTag);
                    HTTPError.Assert("ELI46344", StatusCodes.Status404NotFound,
                        !String.IsNullOrWhiteSpace(filename), 
                        "No result found for the specified file", ("FileId", fileId, true));
                }
                catch (Exception ex)
                {
                    var ee = ex.AsExtract("ELI43580");
                    ee.AddDebugData("MissingResource", ee.Message, false);
                    ee.AddDebugData("Filename", filename, false);
                    throw ee;
                }

                return (filename, error: false, errorMessage: "");
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42119");
                ee.AddDebugData("FileID", fileId, encrypt: false);
                ee.AddDebugData("OutputFileMetadataField", getFileTag, encrypt: false);
                throw ee;
            }
        }

        /// <summary>
        /// GetTextResult implementation
        /// </summary>
        /// <param name="Id">file id</param>
        /// <param name="page"></param>
        /// <returns>TextResult instance, may contain error info</returns>
        public PageTextResult GetText(int Id, int page = -1)
        {
            try
            {
                AssertRequestFileId("ELI46357", Id);

                string fileName = GetSourceFileName(Id);

                var pages = GetText(fileName, page);

                return new PageTextResult
                {
                    Pages = pages
                };
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43247");
                ee.AddDebugData("ID", Id, encrypt: false);
                throw ee;
            }
        }

        List<PageText> GetText(string fileName, int page)
        {
            var pages = new List<PageText>();

            if (Path.GetExtension(fileName) == ".txt")
            {
                AssertFileExists("ELI46390", fileName);
                string text = File.ReadAllText(fileName);
                pages.Add(new PageText(0, text));
            }
            else
            {
                if (page == -1)
                {
                    var documentText = GetUssData(fileName);

                    HTTPError.Assert("ELI46405", StatusCodes.Status404NotFound,
                        documentText != null, "Document text not found");

                    var spatialPages = documentText.GetPages(vbIncludeBlankPages: true, strTextForBlankPage: "");
                    var pageCount = spatialPages.Size();
                    for (int i = 0; i < pageCount; i++)
                    {
                        var spatialPage = (SpatialString)spatialPages.At(i);
                        pages.Add(new PageText(i + 1, spatialPage.String));
                    }
                }
                else
                {
                    HTTPError.AssertRequest("ELI46418",
                        page > 0, "Invalid page number",
                        ("Page", page, true), ("Test", "value", false));

                    var pageText = GetUssData(fileName, page);

                    HTTPError.Assert("ELI46776", StatusCodes.Status404NotFound,
                        pageText != null, "Document text not found");

                    HTTPError.Assert("ELI46419", StatusCodes.Status404NotFound,
                        !String.IsNullOrWhiteSpace(pageText.String), "Page not found",
                        ("Page", page, true), ("Test", "value", false));

                    pages.Add(new PageText(page, pageText.String));
                }
            }

            return pages;
        }

        /// <summary>
        /// GetTextResult implementation
        /// </summary>
        /// <param name="Id">file id</param>
        /// <returns>TextResult instance, may contain error info</returns>
        public PageTextResult GetTextResult(int Id)
        {
            try
            {
                AssertRequestFileId("ELI46358", Id);

                var (filename, isError, errMessage) = GetResult(Id);

                AssertFileExists("ELI46407", filename);

                var pages = GetText(filename, -1);

                return new PageTextResult
                {
                    Pages = pages
                };
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI46307");
                ee.AddDebugData("ID", Id, encrypt: false);
                throw ee;
            }
        }

        /// <summary>
        /// GetDocumentType (API) implementation
        /// </summary>
        /// <param name="id">file Id</param>
        /// <returns>document type (string), wrapped in a TextResult</returns>
        public TextData GetDocumentType(int id)
        {
            try
            {
                var results = GetAttributeSetForFile(id);
                var docType = GetDocumentType(results);

                return new TextData
                {
                    Text = docType
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43329");
            }
        }

        /// <summary>
        /// Asserts the document session.
        /// </summary>
        /// <param name="eliCode">The eli code.</param>
        public void AssertDocumentSession(string eliCode)
        {
            if (!FileApi.DocumentSession.IsOpen)
            {
                throw new HTTPError(eliCode, "No document is currently open");
            }
        }

        /// <summary>
        /// Gets the request file identifier error.
        /// </summary>
        /// <param name="eliCode">The eli code.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        public void AssertRequestFileId(string eliCode, int fileId)
        {
            HTTPError.Assert(eliCode, StatusCodes.Status404NotFound,
                FileApi.FileProcessingDB.IsFileInWorkflow(fileId, FileApi.Workflow.Id),
                "File not in the workflow",
                ("Workflow", FileApi.Workflow.Name, true),
                ("FileId", fileId, true));
        }

        /// <summary>
        /// Gets the request file page error.
        /// </summary>
        /// <param name="eliCode">The eli code.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public void AssertRequestFilePage(string eliCode, int fileId, int page)
        {
            string fileName = FileApi.FileProcessingDB.GetFileNameFromFileID(fileId);

            var fileRecord = FileApi.FileProcessingDB.GetFileRecord(fileName, FileApi.Workflow.StartAction);

            if (page <= 0 || page > fileRecord.Pages)
            {
                var ee = new HTTPError(eliCode, StatusCodes.Status404NotFound,
                    "Page is not valid for specified file");
                ee.AddDebugData("FileId", fileId, false);
                ee.AddDebugData("Page", page, false);
                ee.AddDebugData("Filename", fileName, true);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the request file exists error.
        /// </summary>
        /// <param name="eliCode">The eli code.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        public void AssertFileExists(string eliCode, int fileId)
        {
            string fileName = FileApi.FileProcessingDB.GetFileNameFromFileID(fileId);

            try
            {
                AssertFileExists(eliCode, fileName);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI46403");
                ee.AddDebugData("FileID", fileId, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eliCode"></param>
        /// <param name="fileName"></param>
        public void AssertFileExists(string eliCode, string fileName)
        {
            if (!File.Exists(fileName))
            {
                var ee = new HTTPError(eliCode, StatusCodes.Status404NotFound,
                    "The requested file does not exist");
                ee.AddDebugData("FileName", fileName, true);
                throw ee;
            }
        }

        /// <summary>
        /// The document session file identifier
        /// </summary>
        public int DocumentSessionFileId
        {
            get
            {
                AssertDocumentSession("ELI45270");

                return FileApi.DocumentSession.FileId;
            }
        }

        /// <summary>
        /// The document session file identifier
        /// </summary>
        public int DocumentSessionId
        {
            get
            {
                AssertDocumentSession("ELI49439");

                return FileApi.DocumentSession.Id;
            }
        }

        /// <summary>
        /// Gets the workflow type
        /// </summary>
        public EWorkflowType WorkflowType => FileApi.Workflow.Type;

        /// <summary>
        /// Gets the results of a search as <see cref="DocumentAttribute"/>s
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        /// <param name="searchParameters">The query and options for the search</param>
        public DocumentDataResult GetSearchResults(int docID, SearchParameters searchParameters)
        {
            try
            {
                AssertRequestFileId("ELI48347", docID);

                HTTPError.Assert("ELI48348", StatusCodes.Status400BadRequest,
                    !string.IsNullOrWhiteSpace(searchParameters.Query),
                    "Cannot search for empty string");

                HTTPError.Assert("ELI48353", StatusCodes.Status400BadRequest,
                    string.IsNullOrEmpty(searchParameters.ResultType) || IsValidIdentifier(searchParameters.ResultType),
                    @"Invalid result type. Type must fully match this pattern: ([_a-zA-Z]\w*)?");

                int magicPage = -1;
                if (searchParameters.PageNumber != null)
                {
                    magicPage = searchParameters.PageNumber.Value;
                    AssertRequestFilePage("ELI48349", docID, magicPage);
                }

                var resultType = searchParameters.ResultType ?? "";
                IEnumerable<ISpatialString> pages = null;

                if (magicPage == -1)
                {
                    var pageVector = GetSpatialStringPages(GetSourceFileName(docID));
                    pages = pageVector?.ToIEnumerable<ISpatialString>();
                }
                else
                {
                    var uss = GetUssData(GetSourceFileName(docID), magicPage);
                    pages = uss == null ? null : Enumerable.Repeat(uss, 1);
                }

                HTTPError.Assert("ELI48438", StatusCodes.Status404NotFound,
                    pages != null,
                    "No USS file found for the file: " + GetSourceFileName(docID));

                var query = searchParameters.Query;
                if (searchParameters.QueryType == QueryType.Literal)
                {
                    query = Regex.Escape(query);
                }

                var parserType = Type.GetTypeFromProgID("ESRegExParser.DotNetRegExParser.1");
                var parser = (IRegularExprParser)Activator.CreateInstance(parserType);
                parser.IgnoreCase = !searchParameters.CaseSensitive;
                parser.Pattern = query;

                List<DocumentAttribute> results = new List<DocumentAttribute>();

                foreach (var page in pages)
                {
                    IEnumerable<IToken> tokens = null;
                    try
                    {
                        tokens =
                            parser
                            .Find(page.String, false, false, bDoNotStopAtEmptyMatch: true) // Continue after empty match so that end users aren't surprised by this, non-standard behavior
                            .ToIEnumerable<IObjectPair>()
                            .Select(pair => pair.Object1) // This is the 'top-level match' (i.e., not the named group matches)
                            .Cast<IToken>();
                    }
                    catch (Exception ex) when (searchParameters.QueryType == QueryType.Regex)
                    {
                        var httpError = new HTTPError("ELI48350", StatusCodes.Status400BadRequest, "Error executing search. Possibly due to a bad regex", ex);
                        httpError.AddDebugData("Query", searchParameters.Query);
                        throw httpError;
                    }

                    var mapper = new AttributeMapper(null, FileApi.Workflow.Type);
                    results.AddRange(
                        tokens
                        .Where(token => token.StartPosition <= token.EndPosition) // Exclude empty matches
                        .Select(token => mapper.MapTokenToAttribute(token, "Manual", resultType, page, false))
                        .Where(attr => attr.HasPositionInfo ?? false)
                    );
                }

                return new DocumentDataResult { Attributes = results };
            }
            catch (HTTPError)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48303");
            }
        }

        /// <summary>
        /// Request page image, uss and word zone data to be cached for the specified page.
        /// </summary>
        /// <param name="documentSessionId">The document session for which the data is to be cached.
        /// If the document session is not open by the time of data storage, the call will be ignored
        /// (have no effect).</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="pageNum">The page number for which data should be cached.</param>
        public static async Task CachePageDataAsync(int documentSessionId, int fileId, int pageNum)
        {
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        // Check to be sure cache data doesn't already exist before re-caching.
                        Array cachedPages = UtilityFileProcessingDB.GetCachedPageNumbers(
                            documentSessionId, ECacheDataType.kImage);

                        if (!Array.Exists((int[])cachedPages, x => x == pageNum))
                        {
                            CachePageData(documentSessionId, fileId, pageNum);
                        }
                    }
                    catch (Exception ex)
                    {
                        // We don't know if caller will await result of the operation; log before
                        // throwing to guarantee we have record of the error.
                        var ee = new ExtractException("ELI49461", "Failed to cache data", ex);
                        ee.Log();
                        throw ee;
                    }

                    return;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48423");
            }
        }

        #region Private Members

        FileApi FileApi
        {
            get
            {
                try
                {
                    if (_fileApi == null)
                    {
                        // NOTE: By setting _fileApi using the userContext, which comes directly from the JWT Claims, then
                        // all references to context values on _fileApi are context values from the JWT.
                        _fileApi = FileApiMgr.GetInterface(_apiContext, _user);
                    }

                    return _fileApi;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45024");
                }
            }
        }

        /// <summary>
        /// A FileProcessingDB to be used for FAM DB operations that occur outside the context
        /// of a given DocumentData instance.
        /// </summary>
        static FileProcessingDB UtilityFileProcessingDB
        {
            get
            {
                var context = CurrentApiContext;

                // While in a production scenario the database shouldn't ever change in a given
                // process, for unit tests it will change.
                if (_utilityFileProcessingDB == null ||
                    _utilityFileProcessingDB.DatabaseServer != context.DatabaseServerName ||
                    _utilityFileProcessingDB.DatabaseName != context.DatabaseName)
                {
                    lock (_lock)
                    {
                        if (_utilityFileProcessingDB == null ||
                            _utilityFileProcessingDB.DatabaseServer != context.DatabaseServerName ||
                            _utilityFileProcessingDB.DatabaseName != context.DatabaseName)
                        {
                            _utilityFileProcessingDB = new FileProcessingDB();
                            _utilityFileProcessingDB.DatabaseServer = context.DatabaseServerName;
                            _utilityFileProcessingDB.DatabaseName = context.DatabaseName;
                            _utilityFileProcessingDB.NumberOfConnectionRetries = context.NumberOfConnectionRetries;
                            _utilityFileProcessingDB.ConnectionRetryTimeout = context.ConnectionRetryTimeout;
                        }
                    }
                }

                return _utilityFileProcessingDB;
            }
        }

        AttributeDBMgr AttributeDbMgr
        {
            get
            {
                try
                {
                    if (_attributeDbMgr == null)
                    {
                        _attributeDbMgr = new AttributeDBMgr();

                        _attributeDbMgr.FAMDB = FileApi.FileProcessingDB;
                    }

                    return _attributeDbMgr;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45023");
                }
            }
        }

        /// <summary>
        /// Determine the type of a document
        /// </summary>
        /// <param name="attributes">UnknwonVector containing atribute</param>
        /// <returns>returns the value of the DocumentType attribute, or "Unknown"</returns>
        static string GetDocumentType(IIUnknownVector attributes)
        {
            try
            {
                int count = attributes.Size();
                for (int i = 0; i < count; i++)
                {
                    ComAttribute attribute = (ComAttribute)attributes.At(i);
                    if (attribute.Name.Equals("DocumentType", StringComparison.OrdinalIgnoreCase))
                    {
                        return attribute.Value.String;
                    }
                }

                return "Unknown";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42121");
            }
        }

        /// <summary>
        /// Gets the USS file data as a <see cref="SpatialString"/> for the specified file.
        /// </summary>
        /// <param name="fileId">The ID for which the data is needed.</param>
        /// <param name="page">The page number to retrieve. -1 to retrieve the entire string.</param>
        /// <returns>A <see cref="SpatialString"/> representing the OCR data.</returns>
        SpatialString GetUssData(int fileId, int page = -1)
        {
            return GetUssData(GetSourceFileName(fileId), page);
        }

        /// <summary>
        /// Gets the USS file data as a <see cref="SpatialString"/> for the specified file.
        /// </summary>
        /// <param name="fileName">The source document name of the file for which the data is needed.
        /// </param>
        /// <param name="page">The page number to retrieve. -1 to retrieve the entire string.</param>
        /// <returns>A <see cref="SpatialString"/> representing the OCR data.</returns>
        static SpatialString GetUssData(string fileName, int page = -1)
        {
            var ussFileName = fileName + ".uss";
            if (File.Exists(ussFileName))
            {
                var ussData = new SpatialString();
                if (page > 0)
                {
                    ussData.LoadPageFromFile(ussFileName, page);
                }
                else
                {
                    ussData.LoadFrom(ussFileName, false);
                }

                Utils.ReportMemoryUsage(ussData);

                return ussData;
            }

            return null;
        }

        /// <summary>
        /// Gets the USS file data as a collection of pages for the specified file.
        /// </summary>
        /// <param name="fileName">The source document name of the file for which the data is needed.
        /// </param>
        /// <returns>A <see cref="IIUnknownVector"/> of <see cref="SpatialString"/>s representing the pages.</returns>
        IIUnknownVector GetSpatialStringPages(string fileName)
        {
            var ussFileName = fileName + ".uss";
            if (File.Exists(ussFileName))
            {
                var loader = new SpatialString();
                var pages = loader.LoadPagesFromFile(ussFileName);

                ReportMemoryUsage(pages);

                return pages;
            }

            return null;
        }

        /// <summary>
        /// Caches page image, uss and word zone data to the database for faster response when loading the
        /// specified page.
        /// </summary>
        /// <param name="documentSessionId">The document session ID (FileTaskSessionID) that serves as context
        /// for this cache; the cached data will be deleted once this session ends.</param>
        /// <param name="docID">The currently open document ID</param>
        /// <param name="page">The page for which to cache data.</param>
        static void CachePageData(int documentSessionId, int docID, int page)
        {
            try
            {
                // Don't bother trying to gather data for caching for a session that has already closed.
                ADODB.Recordset adoRecordset = UtilityFileProcessingDB.GetResultsForQuery(
                    Inv($"SELECT TOP 1 [ID] FROM [FileTaskSession] WHERE [ID] = {documentSessionId} AND [DateTimeStamp] IS NULL"));
                try
                {
                    if (adoRecordset.EOF)
                    {
                        return;
                    }
                }
                finally
                {
                    adoRecordset.Close();
                }

                string fileName = UtilityFileProcessingDB.GetFileNameFromFileID(docID);
                int pageCount = _imageUtils.Value.GetPageCount(fileName);

                HTTPError.Assert("ELI48418", StatusCodes.Status404NotFound, page > 0 && page <= pageCount,
                    "Page not found");

                var image = (byte[])_imageConverter.Value.GetPDFImage(fileName, page, true);
                string stringizedPageUss = null;
                string wordZoneJson = null;
                var pageUSS = GetUssData(fileName, page);
                if (pageUSS != null)
                {
                    stringizedPageUss = _miscUtils.Value.GetObjectAsStringizedByteStream(pageUSS);
                    var wordZoneData = pageUSS.MapSpatialStringToWordZoneData();
                    var wordZoneDataResult = new WordZoneDataResult
                    {
                        Zones = wordZoneData
                    };
                    wordZoneJson = JsonConvert.SerializeObject(wordZoneDataResult, Formatting.None);
                }

                UtilityFileProcessingDB.CacheFileTaskSessionData(documentSessionId, page,
                    image, stringizedPageUss, wordZoneJson, null, null, vbCrucialUpdate: false);
            }
            catch (Exception ex)
            {
                var stringizedException = ex.AsExtract("ELI48415").AsStringizedByteStream();

                try
                {
                    UtilityFileProcessingDB.CacheFileTaskSessionData(documentSessionId, page,
                        null, null, null, null, stringizedException, vbCrucialUpdate: false);
                }
                catch (Exception ex2)
                {
                    ex2.ExtractLog("ELI48426");
                }

                throw ex;
            }
        }

        #endregion Private Members
    }
}
