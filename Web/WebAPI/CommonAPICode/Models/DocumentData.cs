using Extract;
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
using AttributeDBMgr = AttributeDbMgrComponentsLib.AttributeDBMgr;
using ComAttribute = UCLID_AFCORELib.Attribute;
using EActionStatus = UCLID_FILEPROCESSINGLib.EActionStatus;
using static WebAPI.Utils;
using Newtonsoft.Json;

namespace WebAPI.Models
{
    /// <summary>
    /// This class is the data model for the DocumentController.
    /// </summary>
    public sealed class DocumentData: IDisposable
    {
        ApiContext _apiContext;
        AttributeDBMgr _attributeDbMgr;
        FileApi _fileApi;
        ImageUtils _imageUtils;
        ImageConverter _imageConverter;
        ClaimsPrincipal _user;
        SpatialString _cachedUssData;

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
                _fileApi.InUse = false;

                // If a FAMSession was not started, end the session so this instance
                // can be used by others.
                if (_fileApi.FAMSessionId == 0)
                {
                    _fileApi.EndSession();
                }

                _fileApi = null;
            }
        }

        /// <summary>
        /// Opens a session for the specified <see paramref="user"/>.
        /// </summary>
        /// <param name="user">The <see cref="User"/> this instance is specific to.</param>
        /// <param name="remoteIpAddress">The IP address of the web application user.s</param>
        public void OpenSession(User user, string remoteIpAddress)
        {
            try
            {
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
                ExtractException.Assert("ELI45234", "No active user", _user != null);

                try
                {
                    if (FileApi.DocumentSession.IsOpen)
                    {
                        CloseDocument(false);
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
        public WebAppSettings GetSettings()
        {
            try
            {
                var json = FileApi.FileProcessingDB.LoadWebAppSettings(-1, "RedactionVerificationSettings");

                var result = JsonConvert.DeserializeObject<WebAppSettings>(json);
                result.Error = Utils.MakeError(false, "", -1);

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
        public QueueStatus GetQueueStatus()
        {
            try
            {
                int actionId = FileApi.FileProcessingDB.GetActionID(FileApi.Workflow.VerifyAction);
                var stats = FileApi.FileProcessingDB.GetStats(actionId, false);
                var users = FileApi.FileProcessingDB.GetActiveUsers(FileApi.Workflow.VerifyAction);

                var result = new QueueStatus();

                result.PendingDocuments = stats.NumDocumentsPending;
                result.PendingPages = stats.NumPagesPending;
                result.ActiveUsers = users.Size;

                result.Error = Utils.MakeError(false, "", -1);

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
        public DocumentId OpenDocument(int id)
        {
            try
            {
                ExtractException.Assert("ELI45235", "No active user", _user != null);

                // Per GGK request, if a document is already open, return the already open ID
                // without error
                // https://extract.atlassian.net/browse/WEB-55
                if (FileApi.DocumentSession.IsOpen)
                {
                    return new DocumentId()
                    {
                        Id = FileApi.DocumentSession.FileId,
                        Error = MakeError(isError: false, message: "", code: 0)
                    };
                }

                IFileRecord fileRecord = null;
                if (id > 0)
                {
                    AssertRequestFileId("ELI45263", id);

                    fileRecord = FileApi.FileProcessingDB.GetFileToProcess(id, FileApi.Workflow.VerifyAction);
                }
                else
                {
                    var fileRecords = FileApi.FileProcessingDB.GetFilesToProcess(FileApi.Workflow.VerifyAction, 1, false, "");
                    if (fileRecords.Size() == 0)
                    {
                        return new DocumentId()
                        {
                            Id = -1,
                            Error = MakeError(isError: false, message: "", code: 0)
                        };
                    }
                    fileRecord = (IFileRecord)fileRecords.At(0);
                }

                var documentId = new DocumentId()
                {
                    Id = fileRecord.FileID,
                    Error = MakeError(isError: false, message: "", code: 0)
                };

                // The GUID used here is a placeholder JIRA until a TaskClass is created for the web app:
                // https://extract.atlassian.net/browse/ISSUE-15079
                FileApi.DocumentSession =
                (
                    true,
                    FileApi.FileProcessingDB.StartFileTaskSession("AD7F3F3F-20EC-4830-B014-EC118F6D4567", documentId.Id, fileRecord.ActionID),
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
        /// <param name="commit"><c>true</c> to commit the document so that it advances in the
        /// workflow; <c>false</c> to save the document without advancing in the workflow.</param>
        public void CloseDocument(bool commit)
        {
            try
            {
                ExtractException.Assert("ELI45238", "No active user", _user != null);
                AssertDocumentSession("ELI45239");

                _cachedUssData = null;

                double duration = (DateTime.Now - FileApi.DocumentSession.StartTime).TotalSeconds;
                FileApi.FileProcessingDB.UpdateFileTaskSession(FileApi.DocumentSession.Id,
                    duration, 0, 0);

                int fileId = FileApi.DocumentSession.FileId;
                if (commit)
                {
                    FileApi.FileProcessingDB.NotifyFileProcessed(fileId, FileApi.Workflow.VerifyAction, -1, true);
                }
                else
                {
                    FileApi.FileProcessingDB.SetStatusForFile(fileId, FileApi.Workflow.VerifyAction, -1,
                        EActionStatus.kActionPending, false, true, out EActionStatus oldStatus);
                }

                try
                {
                    if (commit && !string.IsNullOrWhiteSpace(FileApi.Workflow.PostVerifyAction))
                    {
                        FileApi.FileProcessingDB.SetFileStatusToPending(fileId,
                            FileApi.Workflow.PostVerifyAction, true);
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
        /// get the document attribute set
        /// </summary>
        /// <param name="fileId">The ID of the file for which to retrieve data.</param>
        /// <param name="includeNonSpatial"><c>true</c> to include non-spatial attributes in the resulting data;
        /// otherwise, <c>false</c>. NOTE: If false, a non-spatial attribute will be excluded even if it has
        /// spatial children.</param>
        /// <param name="verboseSpatialData"><c>false</c> to include only the spatial data needed for
        /// extract software to represent spatial strings; <c>true</c> to include data that may be
        /// useful to 3rd party integrators.</param>
        /// <returns>DocumentAttributeSet instance, including error info iff there is an error</returns>
        /// <remarks>The DocumentData CTOR must be constructed with useAttributeDbMgr = true</remarks>
        public DocumentAttributeSet GetDocumentResultSet(int fileId, 
            bool includeNonSpatial = true, bool verboseSpatialData = true)
        {
            try
            {
                AssertFileInWorkflow(fileId);

                var results = GetAttributeSetForFile(fileId);
                var mapper = new AttributeMapper(results, FileApi.Workflow.Type);
                return mapper.MapAttributesToDocumentAttributeSet(includeNonSpatial, verboseSpatialData);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42124");
            }
        }

        /// <summary>
        /// Updates the document result set.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="updatedData">The updated data.</param>
        public void UpdateDocumentResultSet(int fileId, BareDocumentAttributeSet updatedData)
        {
            try
            {
                AssertFileInWorkflow(fileId);

                var results = GetAttributeSetForFile(fileId);
                var attribute = new AttributeClass() { Name = "Update"};
                attribute.Value.ReplaceAndDowngradeToNonSpatial(updatedData.ToString());
                results.PushBack(attribute);

                string fileName = AttributeDbMgr.FAMDB.GetFileNameFromFileID(fileId);
                var translator = new AttributeTranslator(fileName, updatedData);

                UpdateDocumentData(fileId, translator.ComAttributes);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44889");
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
                AssertFileInWorkflow(fileId);

                var attrSetName = FileApi.Workflow.OutputAttributeSet;

                try
                {
                    Contract.Assert(!String.IsNullOrWhiteSpace(attrSetName),
                                    "the workflow: {0}, has OutputAttributeSet that is empty",
                                    FileApi.Workflow.Name);
                    Contract.Assert(AttributeDbMgr != null, "AttributeDbMgr is null");
                }
                catch (Exception ex)
                {
                    var ee = ex.AsExtract("ELI43651");
                    ee.AddDebugData("MissingResource", ee.Message, false);
                    throw ee;
                }

                const int mostRecentSet = -1;
                var results = AttributeDbMgr.GetAttributeSetForFile(fileId,
                                                                     attributeSetName: attrSetName,
                                                                     relativeIndex: mostRecentSet,
                                                                     closeConnection: true);
                return results;
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
        /// Gets the word zone data.
        /// </summary>
        /// <param name="page">The page.</param>
        public WordZoneData GetWordZoneData(int page)
        {
            try
            {
                AssertDocumentSession("ELI45358");
                
                var ussFileName = GetSourceFileName(FileApi.DocumentSession.FileId) + ".uss";

                ExtractException.Assert("ELI45362", "Word data is not available for document",
                    File.Exists(ussFileName));

                var pageData = UssData.GetSpecifiedPages(page, page);
                var wordZoneData = pageData.MapSpatialStringToWordZoneData();
                return wordZoneData;
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
        public async Task<DocumentSubmitResult> SubmitFile(string fileName, Stream fileStream)
        {
            try
            {
                Contract.Assert(!String.IsNullOrWhiteSpace(fileName), "File name is empty");

                var workflow = FileApi.Workflow;
                var uploads = workflow.DocumentFolder;
                Contract.Assert(!String.IsNullOrWhiteSpace(uploads), "folder path is null or empty");

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var fullPath = GetSafeFilename(uploads, fileName);

                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fs);
                    fs.Close();
                }

                return AddFile(fullPath, DocumentSubmitType.File);
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
            Contract.Assert(!String.IsNullOrWhiteSpace(path) && 
                            !String.IsNullOrWhiteSpace(filename), 
                            "Either path or filename is empty");

            string nameOnly = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            string guid = Guid.NewGuid().ToString();
            string newName = Inv($"{nameOnly}_{ guid}{ extension}");
            string fullfilename = Path.Combine(path, newName);

            return fullfilename;
        }

        /// <summary>
        /// Add file - this encapsulates fileProcessingDB.AddFile
        /// </summary>
        /// <param name="fullPath">path + filename - path is expected to exist at this point</param>
        /// <param name="submitType">File or Text - affects the return value: [File|Text]+Id</param>
        /// <param name="caller">caller of this method - DO NOT SET, specified by compiler</param>
        /// <returns>DocumentSubmitresult instance that contains error info iff an error has occurred</returns>
        public DocumentSubmitResult AddFile(string fullPath, 
                                            DocumentSubmitType submitType, 
                                            [CallerMemberName] string caller = "")
        {
            try
            {
                // Now add the file to the FAM queue
                var fileProcessingDB = FileApi.FileProcessingDB;
                var workflow = FileApi.Workflow;
                Contract.Assert(!String.IsNullOrWhiteSpace(workflow.StartAction), 
                                "The workflow: {0}, must have a Start action",
                                FileApi.WorkflowName);

                // Start a FAM session so that the active action is set. This in turn enables the output result file to be
                // added to the FileMetadataFieldValue, for later retrieval by Get[File|Text]Result.
                // ISSUE-14777 Submitting a file via the API, FileMetaDataFieldValue table does not get populated
                fileProcessingDB.RecordFAMSessionStart(caller, workflow.StartAction, vbQueuing: true, vbProcessing: false);

                var fileRecord =
                    fileProcessingDB.AddFile(fullPath,                                                 // full path to file
                                             workflow.StartAction,                                     // action name
                                             workflow.Id,                                              // workflow ID
                                             EFilePriority.kPriorityNormal,                            // file priority
                                             false,                                                    // force status change
                                             false,                                                    // file modified
                                             UCLID_FILEPROCESSINGLib.EActionStatus.kActionPending,     // action status
                                             false,                                                    // skip page count
                                             out bool bAlreadyExists,                                  // returns whether file already existed
                                             out EActionStatus previousActionStatus);                  // returns the previous action status (if file already existed)

                fileProcessingDB.RecordFAMSessionStop();

                return MakeDocumentSubmitResult(fileId: fileRecord.FileID,
                                                isError: false,
                                                message: "",
                                                code: 0,
                                                submitType: submitType);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43331");
            }
        }

        /// <summary>
        /// Updates the document data for the specified <see paramref="fileId"/> in the FAM DB.
        /// </summary>
        /// <param name="fileId">The FAM file ID for which data should be updated.</param>
        /// <param name="fileData">The file data.</param>
        public void UpdateDocumentData(int fileId, IUnknownVector fileData)
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
        public async Task<DocumentSubmitResult> SubmitText(string submittedText)
        {
            try
            {
                var workflow = FileApi.Workflow; 
                var uploads = workflow.DocumentFolder;
                Contract.Assert(!String.IsNullOrWhiteSpace(uploads), "folder path is null or empty");

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var fullPath = GetSafeFilename(uploads, "SubmittedText.txt");

                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    byte[] text = Encoding.ASCII.GetBytes(submittedText);
                    await fs.WriteAsync(text, 0, submittedText.Length);
                    fs.Close();
                }

                return AddFile(fullPath, DocumentSubmitType.Text);
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
        public ProcessingStatus GetStatus(int fileId)
        {
            try
            {
                AssertFileInWorkflow(fileId);

                EActionStatus status = EActionStatus.kActionFailed;

                try
                {
                    status = FileApi.FileProcessingDB.GetWorkflowStatus(fileId);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI42109");
                }

                var ps = MakeProcessingStatus(ConvertToStatus(status, fileId));
                return ps;
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
                AssertFileInWorkflow(fileId);

                filename = FileApi.FileProcessingDB.GetFileNameFromFileID(fileId);

                try
                {
                    Contract.Assert(!String.IsNullOrWhiteSpace(filename), "Error getting the filename for fileId: {0}", fileId);
                    Contract.Assert(System.IO.File.Exists(filename), "File: {0}, does not exist", filename);
                }
                catch (Exception ex)
                {
                    var ee = ex.AsExtract("ELI43580");
                    ee.AddDebugData("MissingResource", ee.Message, false);
                    ee.AddDebugData("Filename", filename, false);
                    throw ee;
                }

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
        /// <param name="fileId">The file identifier.</param>
        public PagesInfo GetPagesInfo(int fileId)
        {
            AssertFileInWorkflow(fileId);

            AssertRequestFileId("ELI45262", fileId);

            var pagesInfo = new PagesInfo()
            {
                PageInfos = new List<PageInfo>(),
                Error = MakeError(isError: false, message: "", code: 0)
            };

            try
            {
                var fileName = GetSourceFileName(fileId);

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
                AssertFileInWorkflow(fileId);

                AssertRequestFileId("ELI45172", fileId);
                AssertRequestFileExists("ELI45173", fileId);
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
                AssertFileInWorkflow(fileId);

                try
                {
                    getFileTag = FileApi.Workflow.OutputFileMetadataField;
                    Contract.Assert(!String.IsNullOrWhiteSpace(getFileTag), "Workflow does not have a defined OutputFileMetaDataField");

                    filename = FileApi.FileProcessingDB.GetMetadataFieldValue(fileId, getFileTag);
                    Contract.Assert(!String.IsNullOrWhiteSpace(filename), "No result file found for file ID: {0}", fileId);
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
        /// <returns>TextResult instance, may contain error info</returns>
        public async Task<TextResult> GetTextResult(int Id = -1)
        {
            try
            {
                AssertFileInWorkflow(Id);

                var (filename, isError, errMessage) = GetResult(Id);
                Contract.Assert(!isError, "Error returned from GetResult");

                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    byte[] buffer = new byte[fs.Length];
                    int retcode = await fs.ReadAsync(buffer, offset: 0, count: Convert.ToInt32(fs.Length));
                    string text = System.Text.Encoding.ASCII.GetString(buffer);

                    return MakeTextResult(text);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43247");
                ee.AddDebugData("ID", Id, encrypt: false);
                throw ee;
            }
        }

        /// <summary>
        /// GetDocumentType (API) implementation
        /// </summary>
        /// <param name="id">file Id</param>
        /// <returns>document type (string), wrapped in a TextResult</returns>
        public TextResult GetDocumentType(int id)
        {
            try
            {
                AssertFileInWorkflow(id);

                var results = GetAttributeSetForFile(id);
                var docType = GetDocumentType(results);
                return MakeTextResult(docType);
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
                throw new RequestAssertion(eliCode, "No document is currently open");
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
            if (!FileApi.FileProcessingDB.IsFileInWorkflow(fileId, FileApi.Workflow.Id))
            {
                throw new RequestAssertion(eliCode,
                    Utils.Inv($"File Id: {fileId} is not in the workflow: {FileApi.Workflow.Name}"));
            }
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
                var ee = new RequestAssertion(eliCode,
                    Utils.Inv($"Page {page} is not valid for file Id: {fileId}"));
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
        public void AssertRequestFileExists(string eliCode, int fileId)
        {
            string fileName = FileApi.FileProcessingDB.GetFileNameFromFileID(fileId);

            if (!File.Exists(fileName))
            {
                var ee = new RequestAssertion(eliCode, Utils.Inv($"File Id: {fileId} does not exist"));
                ee.AddDebugData("Filename", fileName, true);
                throw ee;
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
        /// Gets the image converter.
        /// </summary>
        /// <value>
        /// The image converter.
        /// </value>
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
        /// Gets the document session file identifier.
        /// </summary>
        /// <value>
        /// The document session file identifier.
        /// </value>
        public int DocumentSessionFileId
        {
            get
            {
                AssertDocumentSession("ELI45270");

                return FileApi.DocumentSession.FileId;
            }
        }

        /// <summary>
        /// Gets the <see cref="SpatialString"/> from the uss file representing the currently open
        /// document's OCR results.
        /// </summary>
        /// <value>
        /// The <see cref="SpatialString"/> representing the currently open document's OCR results.
        /// </value>
        SpatialString UssData
        {
            get
            {
                if (_cachedUssData == null)
                {
                    var ussFileName = GetSourceFileName(FileApi.DocumentSession.FileId) + ".uss";
                    _cachedUssData = new SpatialString();
                    _cachedUssData.LoadFrom(ussFileName, false);
                }

                return _cachedUssData;
            }
        }

        /// <summary>
        /// Utility method to enforce that the specified fileId is contained by the workflow
        /// </summary>
        /// <param name="fileId">file Id to test for inclusion</param>
        /// <remarks>the result of this method is an exception iff fileId is not a member of the workflow</remarks>
        void AssertFileInWorkflow(int fileId)
        {
            bool isInWorkflow = FileApi.FileProcessingDB.IsFileInWorkflow(fileId, FileApi.Workflow.Id);

            try
            {
                Contract.Assert(isInWorkflow,
                                "The specified file Id: {0}, is not in the workflow: {1}",
                                fileId,
                                FileApi.WorkflowName);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43579");
                ee.AddDebugData("MissingResource", ee.Message, false);
                throw ee;
            }
        }

        #endregion Private Members
    }
}
