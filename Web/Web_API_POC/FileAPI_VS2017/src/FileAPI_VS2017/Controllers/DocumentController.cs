using FileAPI_VS2017.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;                        // FileStream
using System.Net.Http;                  // for HttpClient
using System.Text;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;
using static FileAPI_VS2017.Utils;

namespace FileAPI_VS2017.Controllers
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
        /// Convert from a string Id to a file Id - for now this is trivial, later there may be another step.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int ConvertIdToFileId(string id)
        {
            int fileId = Convert.ToInt32(id);
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

            string fullname = Path.Combine(path, filename);
            if (!System.IO.File.Exists(fullname))
            {
                return fullname;
            }

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
        public async Task<IActionResult> SubmitFile()
        {
            try
            {
                string path = String.IsNullOrEmpty(environment.WebRootPath) ? "c:\\temp\\fileApi" : environment.WebRootPath;
                var uploads = Path.Combine(path, "uploads");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                string fileName = Request.Headers["X-FileName"];
                if (String.IsNullOrEmpty(fileName))
                {
                    return BadRequest("Filename is empty");
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

                        fileProcessingDB.AddFile(fullPath,                                                 // full path to file
                                                 "A01_ExtractData",                                        // action name
                                                 EFilePriority.kPriorityNormal,                            // file priority
                                                 false,                                                    // force status change
                                                 false,                                                    // file modified
                                                 UCLID_FILEPROCESSINGLib.EActionStatus.kActionPending,     // action status
                                                 true,                                                     // skip page count
                                                 out bAlreadyExists,                                       // returns whether file already existed
                                                 out previousActionStatus);                                // returns the previous action status (if file already existed)

                        // TODO - need to get the FileID from FAM
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(Inv($"Error: {ex.Message}, resetting the fileProcessingDB"));
                        Utils.ResetFileProcessingDB();

                        return BadRequest(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));
                return BadRequest(ex.Message);
            }

            return new ObjectResult("Ok");
        }

        /// <summary>
        /// submit text for processing
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("SubmitText")]
        public IActionResult SubmitText([FromBody]SubmitTextArgs args)
        {
            if (!ModelState.IsValid || String.IsNullOrEmpty(args.Text))
            {
                return BadRequest("args.Text is empty");
            }

            try
            {
                string path = String.IsNullOrEmpty(environment.WebRootPath) ? "c:\\temp\\fileApi" : environment.WebRootPath;

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

                        fileProcessingDB.AddFile(fullPath,                                                 // full path to file
                                                 "A01_ExtractData",                                        // action name
                                                 EFilePriority.kPriorityNormal,                            // file priority
                                                 false,                                                    // force status change
                                                 false,                                                    // file modified
                                                 UCLID_FILEPROCESSINGLib.EActionStatus.kActionPending,     // action status
                                                 false,                                                    // skip page count
                                                 out bAlreadyExists,                                       // returns whether file already existed
                                                 out previousActionStatus);                                // returns the previous action status (if file already existed)

                        // TODO - need to get the FileID from FAM
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(Inv($"Error: {ex.Message}, resetting the fileProcessingDB"));
                        Utils.ResetFileProcessingDB();

                        return BadRequest(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));
                return BadRequest(ex.Message);
            }

            return new ObjectResult("Ok");  // TODO - need to send back a fileID here
        }

        /// <summary>
        /// get a list of 1..N processing status instances that corespond to the stringId of the submitted document
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

            // TODO - this is stubbed, must call FAM to get status of file...

            return MakeListProcessingStatus(isError: false,
                                            message: "",
                                            status: DocumentProcessingStatus.Processing);
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
        public IActionResult GetDocumentType([FromQuery] string documentId)
        {
            // TODO - implement...
            return Ok("abstract of judgement");
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
