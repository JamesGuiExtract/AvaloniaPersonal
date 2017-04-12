﻿using Extract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using static DocumentAPI.Utils;
using AttributeDBMgr = AttributeDbMgrComponentsLib.AttributeDBMgr;
using ComAttribute = UCLID_AFCORELib.Attribute;
using EActionStatus = UCLID_FILEPROCESSINGLib.EActionStatus;

namespace DocumentAPI.Models
{
    /// <summary>
    /// This class is the data model for the DocumentController.
    /// </summary>
    public sealed class DocumentData: IDisposable
    {
        ApiContext _apiContext;
        AttributeDBMgr _attributeDbMgr;
        FileApi _fileApi;

        /// <summary>
        /// CTOR - this should be used only inside a using statement, so the fileApi in-use flag can be cleared.
        /// </summary>
        /// <Comment>Any use of the CTOR MUST be inside a using statement, to ensure Dispose() is called!</Comment>
        /// <param name="apiContext">context instance</param>
        public DocumentData(ApiContext apiContext)
        {
            try
            {
                Contract.Assert(apiContext != null, "attempted to set a null API context");
                _apiContext = apiContext;
                _fileApi = FileApiMgr.GetInterface(apiContext);
                _attributeDbMgr = new AttributeDBMgr();
                Contract.Assert(_attributeDbMgr != null, "Failure to create attributeDbMgr!");

                _attributeDbMgr.FAMDB = _fileApi.Interface;

            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42162");
                Log.WriteLine(ee);

                throw ee;
            }
        }

        /// <summary>
        /// Dispose - used to reset the _fileApi in-use flag
        /// </summary>
        public void Dispose()
        {
            _apiContext = null;
            _attributeDbMgr = null;
            
            // Allow the fileApi object to be reused by clearing the InUse flag.
            _fileApi.InUse = false;
            _fileApi = null;
        }

        /// <summary>
        /// get the document attribute set
        /// </summary>
        /// <returns>DocumentAttributeSet instance, including error info iff there is an error</returns>
        public DocumentAttributeSet GetDocumentResultSet(string id)
        {
            try
            {
                var results = GetAttributeSetForFile(id);
                var mapper = new AttributeMapper(results, _fileApi.GetWorkflow.Type);
                return mapper.MapAttributesToDocumentAttributeSet();
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42124");
                Log.WriteLine(ee);
                throw ee;
            }
        }

        /// <summary>
        /// get the specified file attribute set
        /// </summary>
        /// <param name="id">file id</param>
        /// <returns>IUnknownVector (attribute)</returns>
        public IUnknownVector GetAttributeSetForFile(string id)
        {
            try
            {
                var fileId = ConvertIdToFileId(id);
                const int mostRecentSet = -1;

                var attrSetName = _fileApi.GetWorkflow.OutputAttributeSet;

                var results = _attributeDbMgr.GetAttributeSetForFile(fileID: fileId,
                                                                     attributeSetName: attrSetName,
                                                                     relativeIndex: mostRecentSet,
                                                                     closeConnection: true);
                return results;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42106");
                ee.AddDebugData("FileID", id, encrypt: false);
                ee.AddDebugData("AttributeSetName", _fileApi.GetWorkflow.OutputAttributeSet, encrypt: false);
                ee.AddDebugData("Workflow", _fileApi.GetWorkflow.Name, encrypt: false);
                Log.WriteLine(ee);

                return null;
            }
        }

        /// <summary>
        /// Convert from a string Id to a file Id, removing optional File or Text preamble as necessary.
        /// </summary>
        /// <param name="id">file id</param>
        /// <returns>file Id (integer)</returns>
        static public int ConvertIdToFileId(string id)
        {
            try
            {
                Contract.Assert(!String.IsNullOrEmpty(id), "id is empty");

                int startPosition;
                string value;
                string preamble = "";
                if (id.Contains("File"))
                {
                    preamble = "File";
                }
                else if (id.Contains("Text"))
                {
                    preamble = "Text";
                }

                if (!String.IsNullOrEmpty(preamble))
                {
                    startPosition = id.IndexOf(preamble, startIndex: 0, comparisonType: StringComparison.OrdinalIgnoreCase);
                    value = id.Remove(startPosition, preamble.Length);
                    Contract.Assert(value.Length != 0,
                                    "Removing {0}, from fileId resulted in zero length string, Id: {1}",
                                    preamble,
                                    id);
                }
                else
                {
                    value = id;
                }

                bool converted = Int32.TryParse(value, out int fileId);
                Contract.Assert(true == converted, "Bad fileId value, original id: {0}, value: {1}", id, value);

                return fileId;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42169");
                Log.WriteLine(ee);
                throw ee;
            }
        }

        /// <summary>
        /// SubmitFile implementation for unit testing
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="fileStream">filestream object</param>
        /// <returns>DocumentSubmitResult instance that contains error info iff an error occurs</returns>
        static public async  Task<DocumentSubmitResult> SubmitFile(string fileName, Stream fileStream)
        {
            var fileApi = FileApiMgr.GetInterface(Utils.CurrentApiContext);

            try
            {
                if (String.IsNullOrEmpty(fileName))
                {
                    return MakeDocumentSubmitResult(fileId: -1, isError: true, message: "File name is empty", code: -1);
                }

                var workflow = fileApi.GetWorkflow;
                var uploads = workflow.DocumentFolder;
                Contract.Assert(!String.IsNullOrEmpty(uploads), "folder path is null or empty");

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

                return AddFile(fileApi, fullPath, "ELI42148");
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));
                return MakeDocumentSubmitResult(fileId: -1, isError: true, message: ex.Message, code: -1);
            }
            finally
            {
                // return the fileApi object to the pool
                fileApi.InUse = false;
            }
        }



        /// <summary>
        /// get a "safe" filename - appends a GUID so there is never a file name collision issue
        /// </summary>
        /// <param name="path">The path of the file</param>
        /// <param name="filename">The (base) name of the file, including extension</param>
        /// <returns>a filename that can be used safely</returns>
        public static string GetSafeFilename(string path, string filename)
        {
            Contract.Assert(!String.IsNullOrEmpty(path) && !String.IsNullOrEmpty(filename), "Either path or filename is empty");

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
        /// <param name="fileApi">file API, contains all the API context and workflow</param>
        /// <param name="fullPath">path + filename - path is expected to exist at this point</param>
        /// <param name="eliCode">eliCode from caller</param>
        /// <param name="submitType">File or Text - affects the return value: [File|Text]+Id</param>
        /// <param name="caller">Set by runtime - do NOT pass this argument!</param>
        /// <returns>DocumentSubmitresult instance that contains error info iff an error has occurred</returns>
        static public DocumentSubmitResult AddFile(FileApi fileApi,
                                                   string fullPath,
                                                   string eliCode,
                                                   DocumentSubmitType submitType = DocumentSubmitType.File,
                                                   [CallerMemberName] string caller = null)
        {
            try
            {
                // Now add the file to the FAM queue
                var fileProcessingDB = fileApi.Interface;
                var workflow = fileApi.GetWorkflow;

                var fileRecord =
                    fileProcessingDB.AddFile(fullPath,                                                 // full path to file
                                             workflow.StartAction,                                     // action name
                                             workflow.Id,                                              // workflow ID
                                             EFilePriority.kPriorityNormal,                            // file priority
                                             false,                                                    // force status change
                                             false,                                                    // file modified
                                             UCLID_FILEPROCESSINGLib.EActionStatus.kActionPending,     // action status
                                             true,                                                     // skip page count
                                             out bool bAlreadyExists,                                  // returns whether file already existed
                                             out EActionStatus previousActionStatus);                  // returns the previous action status (if file already existed)

                return MakeDocumentSubmitResult(fileId: fileRecord.FileID,
                                                isError: false,
                                                message: "",
                                                code: 0,
                                                submitType: submitType);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract(eliCode);
                ee.AddDebugData("Method caller", caller, encrypt: false);
                Log.WriteLine(ee);

                return MakeDocumentSubmitResult(fileId: -1, isError: true, message: ee.Message, code: -1);
            }
        }

        /// <summary>
        /// implementation for SubmitText
        /// </summary>
        /// <param name="submittedText">text to submit</param>
        /// <returns>DocumentSubmitResult instance that contains error info iff an error occurs</returns>
        static public async Task<DocumentSubmitResult> SubmitText(string submittedText)
        {
            var fileApi = FileApiMgr.GetInterface(Utils.CurrentApiContext);

            try
            {
                var workflow = fileApi.GetWorkflow; 
                var uploads = workflow.DocumentFolder;
                Contract.Assert(!String.IsNullOrEmpty(uploads), "folder path is null or empty");

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

                return AddFile(fileApi, fullPath, "ELI42108", DocumentSubmitType.Text);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));
                return MakeDocumentSubmitResult(fileId: -1, isError: true, message: ex.Message, code: -1);
            }
            finally
            {
                fileApi.InUse = false;
            }
        }

        /// <summary>
        /// implementation of GetStatus
        /// </summary>
        /// <param name="stringId">file id</param>
        /// <returns>List of ProcessingStatus, can contain error info</returns>
        static public List<ProcessingStatus> GetStatus(string stringId)
        {
            var fileApi = FileApiMgr.GetInterface(Utils.CurrentApiContext);

            try
            {
                var fileProcessingDB = fileApi.Interface;
                Contract.Assert(fileProcessingDB != null, "fileProcessingDB is null, cannot submit text file to FAM queue");

                int fileId = DocumentData.ConvertIdToFileId(stringId);
                EActionStatus status = EActionStatus.kActionFailed;
                var workflow = fileApi.GetWorkflow;

                try
                {
                    status = fileProcessingDB.GetFileStatus(fileId,
                                                            workflow.StartAction,
                                                            vbAttemptRevertIfLocked: false);
                    /*
                    TODO - remove above and enable this code instead (Note that the API might be different):
                    status = fileProcessingDB.GetWorkflowStatus(fileId, workflow.StartAction);
                    */
                }
                catch (Exception ex)
                {
                    var ee = ex.AsExtract("ELI42109");
                    Log.WriteLine(ee);

                    return MakeListOf(
                                MakeProcessingStatus(DocumentProcessingStatus.NotApplicable,
                                                     isError: true,
                                                     message: ee.Message,
                                                     code: -1));
                }

                var ps = MakeProcessingStatus(ConvertToStatus(status, fileId));
                return MakeListOf(ps);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));

                return MakeListOf(
                            MakeProcessingStatus(DocumentProcessingStatus.NotApplicable,
                                                 isError: true,
                                                 message: ex.Message,
                                                 code: -1));
            }
            finally
            {
                fileApi.InUse = false;
            }
        }


        static DocumentProcessingStatus ConvertToStatus(EActionStatus actionStatus, int fileId)
        {
            switch (actionStatus)
            {
                case EActionStatus.kActionCompleted:
                    return DocumentProcessingStatus.Done;

                case EActionStatus.kActionFailed:
                case EActionStatus.kActionSkipped:
                    return DocumentProcessingStatus.Failed;

                case EActionStatus.kActionPending:
                case EActionStatus.kActionProcessing:
                case EActionStatus.kActionUnattempted:
                    return DocumentProcessingStatus.Processing;

                default:
                    Contract.Violated(Inv($"Unknown value: {Convert.ToInt32(actionStatus)} for EActionStatus, for FileID: {fileId}"));
                    return DocumentProcessingStatus.Failed;
            }
        }

        /// <summary>
        /// GetSourceFileName 
        /// </summary>
        /// <param name="Id">file id</param>
        /// <returns>the full path + filename of the original source file</returns>
        static public (string filename, string errorMessage, bool error) GetSourceFileName(string Id)
        {
            string filename = "";
            var fileApi = FileApiMgr.GetInterface(Utils.CurrentApiContext);

            try
            {
                var fileProcessingDB = fileApi.Interface;
                Contract.Assert(fileProcessingDB != null, "null fileProcessingDb, cannot add file to FAM queue");

                var fileId = ConvertIdToFileId(Id);
                filename = fileProcessingDB.GetFileNameFromFileID(fileId);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42110");
                Log.WriteLine(ee);

                var message = ee.Message + ", " + Inv($"while getting filename from fileId: {Id}");
                return (filename: "", errorMessage: message, error: true);
            }
            finally
            {
                fileApi.InUse = false;
            }

            try
            {
                Contract.Assert(!String.IsNullOrEmpty(filename), "Error getting the filename for fileId: {0}", Id);

                if (!System.IO.File.Exists(filename))
                {
                    var message = Inv($"The file: {filename}, does not exist");
                    Log.WriteLine(message);
                    return (filename: "", errorMessage: message, error: true);
                }

                return (filename: filename, errorMessage: "", error: false);
            }
            catch (Exception ex)
            {
                var message = Inv($"Exception: {ex.Message}, while returning file: {filename}, for fileId: {Id}");
                Log.WriteLine(message);
                return (filename: "", errorMessage: message, error: true);
            }
        }

        /// <summary>
        /// GetResult - used by several API calls
        /// </summary>
        /// <param name="id">the database file id to use</param>
        /// <returns>returns a tuple of filename, error flag, error message</returns>
        static public (string filename, bool error, string errorMessage) GetResult(string id)
        {
            var fileId = DocumentData.ConvertIdToFileId(id);
            var fileApi = FileApiMgr.GetInterface(Utils.CurrentApiContext);
            var getFileTag = fileApi.GetWorkflow.OutputFileMetadataField;
            Contract.Assert(!String.IsNullOrEmpty(getFileTag), "Workflow does not have a defined OutputFileMetaDataField");

            string filename = "";

            try
            {
                filename = fileApi.Interface.GetMetadataFieldValue(fileId, getFileTag);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42119");
                ee.AddDebugData("FileID", fileId, encrypt: false);
                ee.AddDebugData("OutputFileMetadataField", getFileTag, encrypt: false);
                Log.WriteLine(ee);

                return (filename, error: true, errorMessage: ee.Message);
            }
            finally
            {
                fileApi.InUse = false;
            }

            if (String.IsNullOrEmpty(filename))
            {
                return (filename: "", error: true, errorMessage: Inv($"No result file exists for id: {id}"));
            }

            return (filename, error: false, errorMessage: "");
        }

        /// <summary>
        /// GetTextResult implementation
        /// </summary>
        /// <param name="textId">file id</param>
        /// <returns>TextResult instance, may contain error info</returns>
        static public async Task<TextResult> GetTextResult(string textId)
        {
            try
            {
                var (filename, isError, errMessage) = GetResult(textId);
                if (isError)
                {
                    return MakeTextResult(text: "", isError: true, errorMessage: errMessage);
                }

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
                var message = Inv($"Exception: {ex.Message}, while returning text for textId: {textId}");
                Log.WriteLine(message);
                return MakeTextResult(text: "", isError: true, errorMessage: message);
            }
        }

        /// <summary>
        /// GetDocumentType (API) implementation
        /// </summary>
        /// <param name="id">file id</param>
        /// <returns>document type (string)</returns>
        public string GetDocumentType(string id)
        {
            var results = GetAttributeSetForFile(id);
            if (results == null)
            {
                var message = Inv($"results retrieval failed for Id: {id}, attributeSetName: {_fileApi.GetWorkflow.OutputAttributeSet}");
                Log.WriteLine(message);

                return "Unknown";
            }

            return GetDocumentType(results);
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
                var ee = ex.AsExtract("ELI42121");
                Log.WriteLine(ee);

                return "Unknown";
            }
        }

    }
}