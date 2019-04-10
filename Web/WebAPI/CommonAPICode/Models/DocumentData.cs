using Extract;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_IMAGEUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using static WebAPI.Utils;
using AttributeDBMgr = AttributeDbMgrComponentsLib.AttributeDBMgr;
using ComAttribute = UCLID_AFCORELib.Attribute;
using EActionStatus = UCLID_FILEPROCESSINGLib.EActionStatus;

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
        ImageUtils _imageUtils;
        ImageConverter _imageConverter;
        ClaimsPrincipal _user;
        bool _endSessionOnDispose;

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
            if (_imageUtils != null)
            {
                _imageUtils = null;
            }

            if (_imageConverter != null)
            {
                _imageConverter = null;
            }

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

                try
                {
                    if (FileApi.DocumentSession.IsOpen)
                    {
                        CloseDocument(EActionStatus.kActionPending);
                    }
                }
                finally
                {
                    try
                    {
                        FileApi.EndSession();
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI46270");
                    }
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

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45272");
            }
        }

        /// <summary>
        /// Gets the number of document, pages and active users in the current verification queue.
        /// </summary>
        public QueueStatusResult GetQueueStatus()
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

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45525");
            }
        }

        /// <summary>
        /// Checkouts the document.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public DocumentIdResult OpenDocument(int id)
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

                    fileRecord = FileApi.FileProcessingDB.GetFileToProcess(id, FileApi.Workflow.EditAction);

                    HTTPError.Assert("ELI46297", StatusCodes.Status423Locked, fileRecord != null,
                        "Another application is editing the document", ("FileId", id, true));
                }
                else
                {
                    var fileRecords = FileApi.FileProcessingDB.GetFilesToProcess(FileApi.Workflow.EditAction, 1, false, "");
                    if (fileRecords.Size() == 0)
                    {
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

                FileApi.FileProcessingDB.UpdateFileTaskSession(FileApi.DocumentSession.Id,
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
                        FileApi.FileProcessingDB.SetFileStatusToPending(fileId,
                            FileApi.Workflow.PostEditAction, true);
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
                    FileApi.FileProcessingDB.SetFileStatusToPending(fileId,
                        FileApi.Workflow.PostWorkflowAction, true);
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
        /// <returns>corresponding DocumentAttributeSet</returns>
        /// <returns>DocumentAttributeSet instance, including error info iff there is an error</returns>
        /// <remarks>The DocumentData CTOR must be constructed with useAttributeDbMgr = true</remarks>
        public DocumentDataResult GetDocumentData(int fileId, 
            bool includeNonSpatial, bool verboseSpatialData, bool splitMultiPageAttributes)
        {
            try
            {
                AssertRequestFileId("ELI46348", fileId);

                var results = GetAttributeSetForFile(fileId);
                var mapper = new AttributeMapper(results, FileApi.Workflow.Type);
                return mapper.MapAttributesToDocumentAttributeSet(
                    includeNonSpatial, verboseSpatialData, splitMultiPageAttributes);
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
                var translator = new AttributeTranslator(fileName, inputData);

                UpdateDocumentData(fileId, translator.ComAttributes);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44889");
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
        /// <remarks>The DocumentData CTOR must be constructed with useAttributeDbMgr = true</remarks>
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
            try
            {
                SpatialString ussData = GetUssData(fileId);

                HTTPError.Assert("ELI45362", StatusCodes.Status404NotFound,
                    ussData != null,
                    "Word data is not available for document");

                HTTPError.Assert("ELI46431", StatusCodes.Status404NotFound,
                    ussData.HasSpatialInfo(), "Spatial data not available",
                    ("Page", page, true), ("Test", "value", false));

                HTTPError.AssertRequest("ELI46420",
                        page > 0, "Invalid page number",
                        ("Page", page, true), ("Test", "value", false));

                HTTPError.Assert("ELI46421", StatusCodes.Status404NotFound,
                    page <= ussData.GetLastPageNumber(), "Page not found",
                    ("Page", page, true), ("Test", "value", false));

                var pageData = ussData.GetSpecifiedPages(page, page);
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

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var fullPath = GetSafeFilename(uploads, fileName);

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
            string fullfilename = Path.Combine(path, newName);

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
                    FileApi.DocumentSession.Id, FileApi.Workflow.OutputAttributeSet, fileData, true, true, true, false);
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

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var fullPath = GetSafeFilename(uploads, "SubmittedText.txt");

                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    byte[] text = Encoding.ASCII.GetBytes(submittedText);
                    fs.Write(text, 0, submittedText.Length);
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

                IIUnknownVector spatialPageInfos = ImageUtils.GetSpatialPageInfos(fileName);
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
        /// </summary>
        /// <param name="pageNum">The page number.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        public byte[] GetPageImage(int fileId, int pageNum)
        {
            try
            {
                AssertRequestFileId("ELI46355", fileId);

                AssertRequestFileId("ELI45172", fileId);
                AssertFileExists("ELI45173", fileId);
                AssertRequestFilePage("ELI45174", fileId, pageNum);

                var fileName = GetSourceFileName(fileId);
                byte[] imageData = (byte[])ImageConverter.GetPDFImage(fileName, pageNum);

                return imageData;
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
                var documentText = GetUssData(fileName);

                HTTPError.Assert("ELI46405", StatusCodes.Status404NotFound,
                    documentText != null, "Document text not found");

                if (page == -1)
                {
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
                        ("Page", page, true),("Test", "value", false));

                    HTTPError.Assert("ELI46419", StatusCodes.Status404NotFound,
                        page <= documentText.GetLastPageNumber(), "Page not found",
                        ("Page", page, true), ("Test", "value", false));

                    var spatialPage = documentText.GetSpecifiedPages(page, page);
                    pages.Add(new PageText(page, spatialPage.String));
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

        public EWorkflowType WorkflowType => FileApi.Workflow.Type;

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
        /// 
        /// </summary>
        ImageUtils ImageUtils
        {
            get
            {
                try
                {
                    if (_imageUtils == null)
                    {
                        _imageUtils = new ImageUtils();
                    }

                    return _imageUtils;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45023");
                }
            }
        }

        /// <summary>
        /// The image converter
        /// </summary>
        ImageConverter ImageConverter
        {
            get
            {
                try
                {
                    if (_imageConverter == null)
                    {
                        _imageConverter = new ImageConverter();
                    }

                    return _imageConverter;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI0");
                }
            }
        }

        /// <summary>
        /// Determine the type of a document
        /// </summary>
        /// <param name="attributes">UnknwonVector containing atribute</param>
        /// <returns>returns the value of the DocumentType attribute, or "Unknown"</returns>
        /// <remarks>The DocumentData CTOR must be constructed with useAttributeDbMgr = true</remarks>
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
        /// Gets the USS file data as a <see cref="SpatialString"/> for the specified file.
        /// </summary>
        /// <param name="fileId">The ID for which the data is needed.</param>
        /// <returns>A <see cref="SpatialString"/> representing the OCR data.</returns>
        SpatialString GetUssData(int fileId)
        {
            return GetUssData(GetSourceFileName(fileId));
        }

        /// <summary>
        /// Gets the USS file data as a <see cref="SpatialString"/> for the specified file.
        /// </summary>
        /// <param name="fileName">The source document name of the file for which the data is needed.
        /// </param>
        /// <returns>A <see cref="SpatialString"/> representing the OCR data.</returns>
        SpatialString GetUssData(string fileName)
        {
            var ussFileName = fileName + ".uss";
            if (File.Exists(ussFileName))
            {
                var ussData = new SpatialString();
                ussData.LoadFrom(ussFileName, false);
                return ussData;
            }

            return null;
        }

        #endregion Private Members
    }
}
