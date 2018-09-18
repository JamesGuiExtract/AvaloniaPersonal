using Extract;
using WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static WebAPI.Utils;
using Microsoft.AspNetCore.Cors;

namespace WebAPI.Controllers
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
    [EnableCors("AllowAll")]
    public class DocumentController : Controller
    {
        /// <summary>
        /// Get the result set for a document that has finished processing
        /// </summary>
        /// <param name="Id">file ID</param>
        /// <returns>a DocumentAttributeSet, which contains error info iff there was an error</returns>
        [HttpGet("GetResultSet/{Id}")]
        [ProducesResponseType(typeof(DocumentAttributeSet), 200)] 
        public IActionResult GetResultSet(int Id)
        {
            try
            {
                this.AssertModel("ELI45200");
                
                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45199", Id);

                    var result = data.GetDocumentResultSet(Id);
                    return result.Error.ErrorOccurred ? (IActionResult)BadRequest(result) : Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError<DocumentAttributeSet>(ex, "ELI43653");
            }
        }

        /// <summary>
        /// Submit a file for document processing
        /// </summary>
        /// <param name="file">input file - NOTE: if the argument name is changed here, it MUST 
        /// also be changed in FileUploadOperation.Apply, NonBodyParameter.Name - if not, then Swagger UI
        /// for this action will be broken!</param>
        /// <returns>a DocumentSubmitResult, which contains error info iff there was an error</returns>
        [HttpPost("SubmitFile")]
        [Produces(typeof(DocumentSubmitResult))]
        public async Task<IActionResult> SubmitFile(IFormFile file)
        {
            try
            {
                RequestAssertion.AssertSpecified("ELI45196", file, "Null file has been submitted");
                RequestAssertion.AssertCondition("ELI45197", file.Length > 0,
                    Utils.Inv($"Zero length file submitted: {file.FileName} has been submitted"));
                RequestAssertion.AssertSpecified("ELI45198", file.FileName, "Empty filename");

                var fileStream = file.OpenReadStream();
                Contract.Assert(fileStream != null, "Null filestream");

                using (var data = new DocumentData(User, requireSession: false))
                {
                    var result = await data.SubmitFile(file.FileName, fileStream);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError<DocumentSubmitResult>(ex, "ELI43654");
            }
        }

        /// <summary>
        /// Submit text for document processing
        /// </summary>
        /// <param name="args">a SubmitTextArgs instance</param>
        /// <returns>a DocumentSubmitResult, which contains error info iff there was an error</returns>
        [HttpPost("SubmitText")]
        [Produces(typeof(DocumentSubmitResult))]
        public async Task<IActionResult> SubmitText([FromBody]SubmitTextArgs args)
        {
            try
            {
                this.AssertModel("ELI45195");
                RequestAssertion.AssertSpecified("ELI45194", args?.Text, "Submitted text is empty");

                using (var data = new DocumentData(User, requireSession: false))
                {
                    var result = await data.SubmitText(args.Text);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError<DocumentSubmitResult>(ex, "ELI43655");
            }
        }

        /// <summary>
        /// Get the status that corresponds to the Id of the submitted document
        /// </summary>
        /// <param name="Id">file Id</param>
        /// <returns>List of ProcessingStatus</returns>
        [HttpGet("GetStatus/{Id}")]
        [Produces(typeof(ProcessingStatus))]
        public IActionResult GetStatus(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45193", Id);

                    var result = data.GetStatus(Id);
                    return Ok(result);
                }
            }   
            catch (Exception ex)
            {
                return this.GetAsHttpError<ProcessingStatus>(ex, "ELI43652");
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
        /// Get the original source file of a submitted document
        /// </summary>
        /// <param name="Id">the file id</param>
        /// <returns>the original image file associated with the file id</returns>
        [HttpGet("GetSourceFile/{Id}")]
        [Produces(typeof(PhysicalFileResult))]
        public IActionResult GetSourceFile(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45191", Id);
                    data.AssertRequestFileExists("ELI45192", Id);

                    var fileName= data.GetSourceFileName(Id);
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
                return this.GetAsHttpError(ex, "ELI43656");
            }
        }

        /// <summary>
        /// Get the result file for the specified input document
        /// </summary>
        /// <param name="Id">file id</param>
        /// <returns>result file</returns>
        [HttpGet("GetFileResult/{Id}")]
        [Produces(typeof(PhysicalFileResult))]
        public IActionResult GetFileResult(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45202", Id);
                    data.AssertRequestFileExists("ELI45203", Id);

                    var (filename, isError, errMessage) = data.GetResult(Id);
                    if (isError)
                    {
                        throw new Exception(errMessage);
                    }

                    var fileContentType = FileContentType(filename);
                    var fileDownloadName = Path.GetFileName(filename);
                    Contract.Assert(!String.IsNullOrWhiteSpace(fileDownloadName), 
                                    "path.GetFileName returned empty value for filename: {0}", 
                                    filename);

                    return PhysicalFile(filename, fileContentType, fileDownloadName);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI43657");
            }
        }

        /// <summary>
        /// Gets the text result for a specified input document
        /// </summary>
        /// <param name="Id">file id - may be prepended by "Text"</param>
        /// <returns>TextResult instance</returns>
        [HttpGet("GetTextResult/{Id}")]
        [Produces(typeof(TextResult))]
        public async Task<IActionResult> GetTextResult(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45204", Id);

                    return Ok(await data.GetTextResult(Id));
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError<TextResult>(ex, "ELI43658");
            }
        }

        /// <summary>
        /// Get the type of the submitted document (document classification)
        /// </summary>
        /// <param name="Id">file Id of the document</param>
        /// <returns>string containing the type of the document</returns>
        [HttpGet("GetDocumentType/{Id}")]
        [Produces(typeof(TextResult))]
        public IActionResult GetDocumentType(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45205", Id);
                    data.AssertRequestFileExists("ELI45206", Id);

                    var result = data.GetDocumentType(Id);
                    return result.Error.ErrorOccurred ? (IActionResult)BadRequest(result) : Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError<TextResult>(ex, "ELI43659");
            }
        }
    }
}
