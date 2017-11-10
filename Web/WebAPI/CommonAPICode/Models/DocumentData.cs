using Extract;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
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
        AttributeDBMgr _attributeDbMgr;
        FileApi _fileApi;

        /// <summary>
        /// CTOR - this should be used only inside a using statement, so the fileApi in-use flag can be cleared.
        /// </summary>
        /// <Comment>Any use of the CTOR MUST be inside a using statement, to ensure Dispose() is called!</Comment>
        /// <param name="userContext">user's context instance (from the JWT claims)</param>
        /// <param name="useAttributeDbMgr">defaults to false. Set this to true to create the attribute manager
        /// (only needed for calls to GetDocumentResultSet and GetDocumentType)</param>
        public DocumentData(ApiContext userContext, bool useAttributeDbMgr = false)
        {
            try
            {
                // NOTE: By setting _fileApi using the userContext, which comes directly from the JWT Claims, then
                // all references to context values on _fileApi are context values from the JWT.
                _fileApi = FileApiMgr.GetInterface(userContext);

                if (useAttributeDbMgr)
                {
                    _attributeDbMgr = new AttributeDBMgr();
                    Contract.Assert(_attributeDbMgr != null, "Failure to create attributeDbMgr!");

                    _attributeDbMgr.FAMDB = _fileApi.Interface;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42162");
            }
        }

        /// <summary>
        /// Dispose - used to reset the _fileApi in-use flag
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
                _fileApi.InUse = false;
                _fileApi = null;
            }
        }

        /// <summary>
        /// get the document attribute set
        /// </summary>
        /// <returns>DocumentAttributeSet instance, including error info iff there is an error</returns>
        /// <remarks>The DocumentData CTOR must be constructed with useAttributeDbMgr = true</remarks>
        public DocumentAttributeSet GetDocumentResultSet(int fileId)
        {
            try
            {
                AssertFileInWorkflow(fileId);

                var results = GetAttributeSetForFile(fileId);
                var mapper = new AttributeMapper(results, _fileApi.GetWorkflow.Type);
                return mapper.MapAttributesToDocumentAttributeSet();
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

                string fileName = _attributeDbMgr.FAMDB.GetFileNameFromFileID(fileId);
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

                var attrSetName = _fileApi.GetWorkflow.OutputAttributeSet;

                try
                {
                    Contract.Assert(!String.IsNullOrWhiteSpace(attrSetName),
                                    "the workflow: {0}, has OutputAttributeSet that is empty",
                                    _fileApi.GetWorkflow.Name);
                    Contract.Assert(_attributeDbMgr != null, "_attributeDbMgr is null");
                }
                catch (Exception ex)
                {
                    var ee = ex.AsExtract("ELI43651");
                    ee.AddDebugData("MissingResource", ee.Message, false);
                    throw ee;
                }

                const int mostRecentSet = -1;
                var results = _attributeDbMgr.GetAttributeSetForFile(fileId,
                                                                     attributeSetName: attrSetName,
                                                                     relativeIndex: mostRecentSet,
                                                                     closeConnection: true);
                return results;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42106");
                ee.AddDebugData("FileID", fileId, encrypt: false);
                ee.AddDebugData("AttributeSetName", _fileApi.GetWorkflow.OutputAttributeSet, encrypt: false);
                ee.AddDebugData("Workflow", _fileApi.GetWorkflow.Name, encrypt: false);

                throw ee;
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

                var workflow = _fileApi.GetWorkflow;
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
                var fileProcessingDB = _fileApi.Interface;
                var workflow = _fileApi.GetWorkflow;
                Contract.Assert(!String.IsNullOrWhiteSpace(workflow.StartAction), 
                                "The workflow: {0}, must have a Start action", 
                                _fileApi.WorkflowName);

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
        /// <param name="caller"></param>
        public void UpdateDocumentData(int fileId, IUnknownVector fileData, [CallerMemberName] string caller = "")
        {
            try
            {
                var fileProcessingDB = _fileApi.Interface;
                var workflow = _fileApi.GetWorkflow;

                fileProcessingDB.RecordFAMSessionStart(caller, workflow.StartAction, vbQueuing: false, vbProcessing: true);

                try
                {
                    int actionID = fileProcessingDB.GetActionIDForWorkflow(workflow.EndAction, workflow.Id);
                    DateTime start = DateTime.Now;
                    // This is a placeholder JIRA until a TaskClass is created for the web app:
                    // https://extract.atlassian.net/browse/ISSUE-15079
                    int fileTaskSessionId = fileProcessingDB.StartFileTaskSession("AD7F3F3F-20EC-4830-B014-EC118F6D4567", fileId, actionID);

                    _attributeDbMgr.CreateNewAttributeSetForFile(fileTaskSessionId, workflow.OutputAttributeSet, fileData, true, true, true, false);

                    fileProcessingDB.UpdateFileTaskSession(fileTaskSessionId, (DateTime.Now - start).TotalSeconds, 0);
                }
                finally
                {
                    fileProcessingDB.RecordFAMSessionStop();
                }
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
                var workflow = _fileApi.GetWorkflow; 
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
                    status = _fileApi.Interface.GetWorkflowStatus(fileId);
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
        public (string filename, string errorMessage, bool error) GetSourceFileName(int fileId)
        {
            string filename = "";

            try
            {
                AssertFileInWorkflow(fileId);

                filename = _fileApi.Interface.GetFileNameFromFileID(fileId);

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

                return (filename: filename, errorMessage: "", error: false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42110");
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
                    getFileTag = _fileApi.GetWorkflow.OutputFileMetadataField;
                    Contract.Assert(!String.IsNullOrWhiteSpace(getFileTag), "Workflow does not have a defined OutputFileMetaDataField");

                    filename = _fileApi.Interface.GetMetadataFieldValue(fileId, getFileTag);
                    Contract.Assert(!String.IsNullOrWhiteSpace(filename), "No result file found for file ID: {0}", fileId);
                }
                catch (Exception ex)
                {
                    var ee = ex.AsExtract("ELI43580");
                    ee.AddDebugData("MissingResource", ee.Message, false);
                    ee.AddDebugData("Filename", filename, false);
                    throw ee;
                }

                // Start the post action, if any, on this file.
                var postAction = _fileApi.GetWorkflow.PostWorkflowAction;
                if (!String.IsNullOrWhiteSpace(postAction))
                {
                    _fileApi.Interface.SetFileStatusToPending(fileId, postAction, vbAllowQueuedStatusOverride: false);
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
        public async Task<TextResult> GetTextResult(int Id)
        {
            try
            {
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
        /// Utility method to enforce that the specified fileId is contained by the workflow
        /// </summary>
        /// <param name="fileId">file Id to test for inclusion</param>
        /// <remarks>the result of this method is an exception iff fileId is not a member of the workflow</remarks>
        void AssertFileInWorkflow(int fileId)
        {
            bool isInWorkflow =  _fileApi.Interface.IsFileInWorkflow(fileId, _fileApi.GetWorkflow.Id);

            try
            {
                Contract.Assert(isInWorkflow,
                                "The specified file Id: {0}, is not in the workflow: {1}",
                                fileId,
                                _fileApi.WorkflowName);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43579");
                ee.AddDebugData("MissingResource", ee.Message, false);
                throw ee;
            }
        }
    }
}
