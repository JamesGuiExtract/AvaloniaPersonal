using Extract;
using DocumentAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
    }

    /// <summary>
    /// The Document API class
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class DocumentController : Controller
    {
        /// <summary>
        /// Gets result set for a submitted file that has finished processing
        /// </summary>
        /// <returns>a DocumentAttributeSet, which contains error info iff there was an error</returns>
        [HttpGet("GetResultSet/{id}")]
        [ProducesResponseType(typeof(DocumentAttributeSet), 200)] 
        public DocumentAttributeSet GetResultSet(string id)
        {
            if (!ModelState.IsValid || String.IsNullOrWhiteSpace(id))
            {
                Log.WriteLine("string id is empty", "ELI43235");
                return MakeDocumentAttributeSetError("id argument cannot be empty");
            }

            // using ensures that the underlying FileApi.InUse flag is cleared on exit
            using (var data = new DocumentData(ClaimsToContext(User), useAttributeDbMgr: true))
            {
                return data.GetDocumentResultSet(id);
            }
        }


        /// <summary>
        /// Upload 1 to N files for document processing
        /// </summary>
        /// <returns>a DocumentSubmitResult, which contains error info iff there was an error</returns>
        [HttpPost("SubmitFile")]
        public async Task<DocumentSubmitResult> SubmitFile()
        {
            try
            {
                string fileName = Request?.Headers["X-FileName"];
                Contract.Assert(String.IsNullOrWhiteSpace(fileName), "Filename cannot be empty");

                // Request.Body is type FrameRequestStream, inherits from Stream, which is also the parent of FileStream
                Stream fileStream = (Stream)Request.Body;
                Contract.Assert(fileStream != null, "Null filestream");

                using (var data = new DocumentData(ClaimsToContext(User)))
                {
                    return await data.SubmitFile(fileName, fileStream);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.AsExtract("ELI42149"));
                return MakeDocumentSubmitResult(fileId: -1, isError: true, code: -1);
            }
        }


        /// <summary>
        /// submit text for processing
        /// </summary>
        /// <param name="args">a SubmitTextArgs instance</param>
        /// <returns>a DocumentSubmitResult, which contains error info iff there was an error</returns>
        [HttpPost("SubmitText")]
        public async Task<DocumentSubmitResult> SubmitText([FromBody]SubmitTextArgs args)
        {
            if (!ModelState.IsValid || String.IsNullOrWhiteSpace(args.Text))
            {
                return MakeDocumentSubmitResult(fileId: -1, isError: true, message: "File name is empty", code: -1);
            }

            using (var data = new DocumentData(ClaimsToContext(User)))
            {
                return await data.SubmitText(args.Text);
            }
        }

        /// <summary>
        /// get a list of 1..N processing status instances that correspond to the stringId of the submitted document
        /// </summary>
        /// <param name="stringId">file Id</param>
        /// <returns>List of ProcessingStatus</returns>
        [HttpGet("GetStatus")]
        public List<ProcessingStatus> GetStatus([FromQuery] string stringId)
        {
            if (String.IsNullOrWhiteSpace(stringId))
            {
                return MakeListProcessingStatus(isError: true, 
                                                message: "stringId argument is empty", 
                                                status: DocumentProcessingStatus.Failed, 
                                                code: -1);
            }

            var data = new DocumentData(ClaimsToContext(User));
            {
                return data.GetStatus(stringId);
            }
        }

        /// <summary>
        /// This function returns the content (MIME) type associated with the file extension,
        /// or throws if the type is unsupported.
        /// </summary>
        /// <param name="filename">full path + filename</param>
        /// <returns>MIME type string</returns>
        /// <remarks> Supported MIME types:
        /// .tif|.tiff Tagged Image File Format(TIFF) image/tiff
        /// .pdf Adobe Portable Document Format(PDF)    application/pdf
        /// .xml XML application/xml
        /// .zip ZIP archive application/zip
        /// </remarks>
        static string FileContentType(string filename)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(filename), "bad argument - empty string");

            var ext = Path.GetExtension(filename);
            Contract.Assert(!String.IsNullOrWhiteSpace(ext), "extension is empty - filename: {0}", filename);

            if (ext.IsEquivalent(".tif") || ext.IsEquivalent(".tiff"))
            {
                return "image/tiff";
            }
            else if (ext.IsEquivalent(".pdf"))
            {
                return "application/pdf";
            }
            else if (ext.IsEquivalent(".xml"))
            {
                return "application/xml";
            }
            else if (ext.IsEquivalent(".zip"))
            {
                return "application/zip";
            }
            else if (ext.IsEquivalent(".txt"))
            {
                return "text/plain";
            }
            else
            {
                // for VOA, USS, or ?
                return "application/octet-stream";
            }
        }

        /// <summary>
        /// get the original source file associated with the file id
        /// </summary>
        /// <param name="Id">the file id, as a string. Often prepended with "Text" or "File"</param>
        /// <returns>the original image file associated with the file id</returns>
        [HttpGet("GetSourceFile/{Id}")]
        [Produces(typeof(PhysicalFileResult))]
        public IActionResult GetSourceFile(string Id)
        {
            string filename = "";

            try
            {
                using (var data = new DocumentData(ClaimsToContext(User)))
                {
                    var (fileName, errorMsg, error) = data.GetSourceFileName(Id);

                    var fileContentType = FileContentType(fileName);
                    var fileDownloadName = Path.GetFileName(fileName);
                    Contract.Assert(!String.IsNullOrWhiteSpace(fileDownloadName), 
                                    "path.GetFileName returned empty value for filename: {0}", 
                                    fileName);

                    return PhysicalFile(fileName, fileContentType, fileDownloadName);
                }
            }
            catch (Exception ex)
            {
                var message = Inv($"Exception: {ex.Message}, while returning file: {filename}, for fileId: {Id}");
                Log.WriteLine(message, "ELI43236");
                return BadRequest(message);
            }
        }


        /// <summary>
        /// Gets a result file for the specified input document
        /// </summary>
        /// <param name="id">file id</param>
        /// <returns>result file</returns>
        [HttpGet("GetFileResult/{id}")]
        [Produces(typeof(PhysicalFileResult))]
        public IActionResult GetFileResult(string id)
        {
            try
            {
                using (var data = new DocumentData(ClaimsToContext(User)))
                {
                    var (filename, isError, errMessage) = data.GetResult(id);
                    if (isError)
                    {
                        return BadRequest(errMessage);
                    }

                    var fileContentType = FileContentType(filename);
                    var fileDownloadName = Path.GetFileName(filename);
                    Contract.Assert(!String.IsNullOrWhiteSpace(fileDownloadName), 
                                    "path.GetFileName returned empty value for filename: {0}", 
                                    filename);

                    if (!System.IO.File.Exists(filename))
                    {
                        return BadRequest(Inv($"result file: {filename}, not found"));
                    }

                    return PhysicalFile(filename, fileContentType, fileDownloadName);
                }
            }
            catch (Exception ex)
            {
                var message = Inv($"Exception: {ex.Message}, while returning file for fileId: {id}");
                Log.WriteLine(message, "ELI43237");
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Gets a text result for a specified input document
        /// </summary>
        /// <param name="textId">file id - may be prepended by "Text"</param>
        /// <returns>TextResult instance</returns>
        [HttpGet("GetTextResult")]
        [Produces(typeof(TextResult))]
        public async Task<TextResult> GetTextResult([FromQuery] string textId)
        {
            using (var data = new DocumentData(ClaimsToContext(User)))
            {
                return await data.GetTextResult(textId);
            }
        }

        /// <summary>
        /// Gets the type of the submitted document (document classification)
        /// </summary>
        /// <param name="id">file Id of the document</param>
        /// <returns>string containing the type of the document</returns>
        [HttpGet("GetDocumentType/{id}")]
        [Produces(typeof(String))]
        public string GetDocumentType(string id)
        {
            using (var data = new DocumentData(ClaimsToContext(User), useAttributeDbMgr: true))
            {
                return data.GetDocumentType(id);
            }
        }
    }
}
