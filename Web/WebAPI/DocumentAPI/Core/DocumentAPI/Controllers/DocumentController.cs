using Extract;
using DocumentAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
                Contract.Assert(ModelState.IsValid && Id > 0, "Invalid input Id");
                
                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(ClaimsToContext(User), useAttributeDbMgr: true))
                {
                    var result = data.GetDocumentResultSet(Id);
                    return result.Error.ErrorOccurred ? (IActionResult)BadRequest(result) : Ok(result);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43653");
                Log.WriteLine(ee);
                var result = MakeDocumentAttributeSetError(ee.Message);
                return BadRequest(result);
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
                Contract.Assert(file != null, "Null file has been submitted");
                Contract.Assert(file.Length > 0, "Zero length file: {0}, has been submitted", file.FileName);

                var fileName = file.FileName;
                Contract.Assert(!String.IsNullOrWhiteSpace(fileName), "Empty filename");

                var fileStream = file.OpenReadStream();
                Contract.Assert(fileStream != null, "Null filestream");

                using (var data = new DocumentData(ClaimsToContext(User)))
                {
                    var result = await data.SubmitFile(fileName, fileStream);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43654");
                Log.WriteLine(ee);
                return BadRequest(MakeDocumentSubmitResult(fileId: -1, isError: true, message: ee.Message, code: -1));
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
                Contract.Assert(ModelState.IsValid && !String.IsNullOrWhiteSpace(args.Text), "Submitted text is empty");

                using (var data = new DocumentData(ClaimsToContext(User)))
                {
                    var result = await data.SubmitText(args.Text);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43655");
                Log.WriteLine(ee);
                return BadRequest(MakeDocumentSubmitResult(fileId: -1, isError: true, message: ee.Message, code: -1));
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
                Contract.Assert(Id > 0, "Id is not valid");

                using (var data = new DocumentData(ClaimsToContext(User)))
                {
                    var result = data.GetStatus(Id);
                    return Ok(result);
                }
            }   
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI43652");
                Log.WriteLine(ee);

                var err = MakeProcessingStatus(DocumentProcessingStatus.NotApplicable,
                                               isError: true,
                                               message: ee.Message,
                                               code: -1);
                return BadRequest(err);
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
                using (var data = new DocumentData(ClaimsToContext(User)))
                {
                    var (fileName, errorMsg, error) = data.GetSourceFileName(Id);
                    if (error)
                    {
                        return BadRequest(errorMsg);
                    }

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
                var ee = ex.AsExtract("ELI43656");
                Log.WriteLine(ee);
                return BadRequest(ee.Message);
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
                using (var data = new DocumentData(ClaimsToContext(User)))
                {
                    var (filename, isError, errMessage) = data.GetResult(Id);
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
                var ee = ex.AsExtract("ELI43657");
                var message = Inv($"Exception: {ex.Message}, while returning file for fileId: {Id}");
                Log.WriteLine(ee);
                return BadRequest(ee.Message);
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
                using (var data = new DocumentData(ClaimsToContext(User)))
                {
                    return Ok(await data.GetTextResult(Id));
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43658");
                Log.WriteLine(ee);
                return BadRequest(MakeTextResult("", isError: true, errorMessage: ee.Message));
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
                using (var data = new DocumentData(ClaimsToContext(User), useAttributeDbMgr: true))
                {
                    var result = data.GetDocumentType(Id);
                    return result.Error.ErrorOccurred ? (IActionResult)BadRequest(result) : Ok(result);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43659");
                Log.WriteLine(ee);
                return BadRequest(MakeTextResult("", isError: true, errorMessage: ee.Message));
            }
        }
    }
}
