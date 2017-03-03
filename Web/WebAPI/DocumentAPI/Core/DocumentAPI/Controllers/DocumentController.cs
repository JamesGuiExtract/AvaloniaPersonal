﻿using DocumentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;                        // FileStream
using System.Net.Http;                  // for HttpClient
using System.Text;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;
using static DocumentAPI.Utils;

namespace DocumentAPI.Controllers
{
    /// <summary>
    /// args DTO for SubmitText
    /// </summary>
    public class SubmitTextArgs
    {
        /// <summary>
        /// text argument, required
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// WorkflowName - optional, if not specified then default workflow for user is used
        /// </summary>
        public string WorkflowName { get; set; }
    }

    /// <summary>
    /// The Document API class
    /// </summary>
    [Route("api/[controller]")]
    public class DocumentController : Controller
    {
        /// <summary>
        /// Convert from a string Id to a file Id, removing optional File or Text preamble as necessary.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int ConvertIdToFileId(string id)
        {
            Contract.Assert(!String.IsNullOrEmpty(id), "id is empty");

            int startPosition;
            string value;
            string preamble;
            if (id.Contains("File"))
            {
                preamble = "File";
            }
            else if (id.Contains("Text"))
            {
                preamble = "Text";
            }
            else
            {
                preamble = "";
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

            int fileId = Convert.ToInt32(value);
            return fileId;
        }

        /// <summary>
        /// Gets result set for a submitted file that has finished processing
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetResultSet/{id}")]
        //[Produces(typeof(DocumentAttributeSet))]
        [ProducesResponseType(typeof(DocumentAttributeSet), 200)]   // BIG NOTE: This used to be SwaggerResponse, changed in "Swashbuckle": "6.0.0-beta902"!
        public DocumentAttributeSet GetResultSet(string id)
        {
            if (!ModelState.IsValid || String.IsNullOrEmpty(id))
            {
                Log.WriteLine("string id is empty");
                return MakeDocumentAttributeSetError("id argument cannot be empty");
            }

            try
            {
                // Make sure to set the attribute mgr FAMDB, or the call to GetAttributeSetForFile will throw.
                var attrMgr = Utils.AttrDbMgr;

                var fileId = ConvertIdToFileId(id);
                const int mostRecentSet = -1;

                var results = attrMgr.GetAttributeSetForFile(fileID: fileId,
                                                             attributeSetName: Utils.AttributeSetName,
                                                             relativeIndex: mostRecentSet);
                if (results == null)
                {
                    Log.WriteLine(Inv($"results retrieval failed for Id: {id}"));
                    return MakeDocumentAttributeSetError("Results retrieval failed");
                }

                return AttributeMapper.MapAttributesToDocumentAttributeSet(results);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception: {ex.Message}, while getting ASFF for fileId: {id}, resetting attribute manager"));
                Utils.ResetAttributeMgr();

                return MakeDocumentAttributeSetError(ex.Message);
            }
        }

        /// <summary>
        /// get a "safe" filename - handles filename collisions in the upload directory by appending a GUID on collision.
        /// </summary>
        /// <param name="path">The path of the file</param>
        /// <param name="filename">The (base) name of the file, including extension</param>
        /// <returns>a filename that can be used safely</returns>
        private string GetSafeFilename(string path, string filename)
        {
            Contract.Assert(!String.IsNullOrEmpty(path) && !String.IsNullOrEmpty(filename), "Either path or filename is empty");

            string nameOnly = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            string guid = Guid.NewGuid().ToString();
            string newName = $"{nameOnly}_{ guid}{ extension}";
            string fullfilename = Path.Combine(path, newName);

            return fullfilename;
        }

        /// <summary>
        /// Upload 1 to N files for document processing
        /// </summary>
        /// <returns></returns>
        [HttpPost("SubmitFile")]
        public async Task<DocumentSubmitResult> SubmitFile()
        {
            try
            {
                Contract.Assert(!String.IsNullOrEmpty(environment.WebRootPath), "WebRootPath is null or empty");
                string path = environment.WebRootPath;
                var uploads = Path.Combine(path, "uploads");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                string fileName = Request.Headers["X-FileName"];
                if (String.IsNullOrEmpty(fileName))
                {
                    return MakeDocumentSubmitResult(fileId: -1, isError: true, message: "File name is empty", code: -1);
                }

                var fullPath = GetSafeFilename(uploads, fileName);

                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    await Request.Body.CopyToAsync(fs);

                    try
                    {
                        // Now add the file to the FAM queue
                        var fileProcessingDB = Utils.FileDbMgr;
                        Contract.Assert(fileProcessingDB != null, "null fileProcessingDb, cannot add file to FAM queue");

                        bool bAlreadyExists;
                        UCLID_FILEPROCESSINGLib.EActionStatus previousActionStatus;

                        var fileRecord = 
                            fileProcessingDB.AddFile(fullPath,                                                 // full path to file
                                                     "A01_ExtractData",                                        // action name - TODO - remove hard-coded name, use Worflow.EntryName when available
                                                     EFilePriority.kPriorityNormal,                            // file priority
                                                     false,                                                    // force status change
                                                     false,                                                    // file modified
                                                     UCLID_FILEPROCESSINGLib.EActionStatus.kActionPending,     // action status
                                                     true,                                                     // skip page count
                                                     out bAlreadyExists,                                       // returns whether file already existed
                                                     out previousActionStatus);                                // returns the previous action status (if file already existed)

                        return MakeDocumentSubmitResult(fileId: fileRecord.FileID);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(Inv($"Error: {ex.Message}, resetting the fileProcessingDB"));
                        Utils.ResetFileProcessingDB();

                        return MakeDocumentSubmitResult(fileId: -1, isError: true, message: ex.Message, code: -1);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));
                return MakeDocumentSubmitResult(fileId: -1, isError: true, message: ex.Message, code: -1);
            }
        }

        /// <summary>
        /// submit text for processing
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("SubmitText")]
        public DocumentSubmitResult SubmitText([FromBody]SubmitTextArgs args)
        {
            if (!ModelState.IsValid || String.IsNullOrEmpty(args.Text))
            {
                return MakeDocumentSubmitResult(fileId: -1, isError: true, message: "File name is empty", code: -1);
            }

            try
            {
                Contract.Assert(!String.IsNullOrEmpty(environment.WebRootPath), "WebRootPath is null or empty");
                string path = environment.WebRootPath;

                var uploads = Path.Combine(path, "uploads");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var fullPath = GetSafeFilename(uploads, "SubmittedText.txt");

                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    byte[] text = Encoding.ASCII.GetBytes(args.Text);
                    fs.Write(text, 0, args.Text.Length);
                    fs.Close();

                    // Now add the file to the FAM queue
                    try
                    {
                        var fileProcessingDB = Utils.FileDbMgr;
                        Contract.Assert(fileProcessingDB != null, "fileProcessingDB is null, cannot submit text file to FAM queue");

                        bool bAlreadyExists;
                        UCLID_FILEPROCESSINGLib.EActionStatus previousActionStatus;

                        var fileRecord = 
                            fileProcessingDB.AddFile(fullPath,                                                 // full path to file
                                                     "A01_ExtractData",                                        // action name
                                                     EFilePriority.kPriorityNormal,                            // file priority
                                                     false,                                                    // force status change
                                                     false,                                                    // file modified
                                                     UCLID_FILEPROCESSINGLib.EActionStatus.kActionPending,     // action status
                                                     true,                                                     // skip page count
                                                     out bAlreadyExists,                                       // returns whether file already existed
                                                     out previousActionStatus);                                // returns the previous action status (if file already existed)

                        return MakeDocumentSubmitResult(fileId: fileRecord.FileID);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(Inv($"Error: {ex.Message}, resetting the fileProcessingDB"));
                        Utils.ResetFileProcessingDB();

                        return MakeDocumentSubmitResult(fileId: -1, isError: true, message: ex.Message, code: -1);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));
                return MakeDocumentSubmitResult(fileId: -1, isError: true, message: ex.Message, code: -1);
            }
        }

        DocumentProcessingStatus ConvertToStatus(EActionStatus actionStatus, int fileId)
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
        /// get a list of 1..N processing status instances that correspond to the stringId of the submitted document
        /// </summary>
        /// <param name="stringId"></param>
        /// <returns></returns>
        [HttpGet("GetStatus")]
        public List<ProcessingStatus> GetStatus([FromQuery] string stringId)
        {
            if (String.IsNullOrEmpty(stringId))
            {
                return MakeListProcessingStatus(isError: true, 
                                                message: "stringId argument is empty", 
                                                status: DocumentProcessingStatus.Failed, 
                                                code: -1);
            }

            try
            {
                // TODO - test this...
                var fileProcessingDB = Utils.FileDbMgr;
                Contract.Assert(fileProcessingDB != null, "fileProcessingDB is null, cannot submit text file to FAM queue");

                int fileId = ConvertIdToFileId(stringId);
                var actionStatus = fileProcessingDB.GetFileStatus(fileId,
                                                                  "A01_ExtractData",               // TODO - remove hard-wired name, use Workflow.EntryName when available
                                                                  vbAttemptRevertIfLocked: false); // TODO - verify: is this correct?

                var ps = MakeProcessingStatus(ConvertToStatus(actionStatus, fileId));
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
        }

        // Add File GetFileResult(string fileId)
        /// <summary>
        /// Gets a result file for the specified input document
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpGet("GetFileResult")]
        public byte[] GetFileResult([FromQuery] string fileId)
        {
            return new byte[10];
        }

        // Add GetTextResult
        // TODO - may unify the Get????Result() methods, why not?
        /// <summary>
        /// Gets a text result for a specified input document
        /// </summary>
        /// <param name="textId"></param>
        /// <returns></returns>
        [HttpGet("GetTextResult")]
        public byte[] GetTextResult([FromQuery] string textId)
        {
            return new byte[10];
        }

        /// <summary>
        /// Gets the type of the submitted document (document classification)
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet("GetDocumentType")]
        public string GetDocumentType([FromQuery] string documentId)
        {
            // TODO - implement...
            return "abstract of judgement";
        }

        /// <summary>
        /// Gets a result file - experimental!
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetFileResultEx")]
        public async Task<FileStreamResult> GetFileResultEx()
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:58926/");
                var stream = await client.GetStreamAsync("wwwroot/uploads/SubmittedText.txt");

                var mediaType = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text /plain");
                var fsr = new FileStreamResult(stream, mediaType)
                {
                    FileDownloadName = "column_dob.txt"
                };
                return fsr;
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception: {ex.Message}"));
                return new FileStreamResult(null, "");
            }
        }
    }
}
