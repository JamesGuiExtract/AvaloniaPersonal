using Extract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    /// <summary>
    /// A REST API <see cref="Controller"/> to add/retrieve and interact with a FAM workflow.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    public class DocumentController : Controller
    {
        /// <summary>
        /// Submit a document for processing
        /// </summary>
        /// <param name="file">The input file</param>
        /// <returns>A <see cref="DocumentIdResult"/> containing the ID for the submitted document.</returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DocumentIdResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        // NOTE: If method name changes, ContentTypeSpecifier and FileUploadOperation should be updated to match.
        public async Task<IActionResult> PostDocument(IFormFile file)
        {
            try
            {
                HTTPError.AssertRequest("ELI45196", file != null, "Null file has been submitted");
                HTTPError.AssertRequest("ELI45197", file.Length > 0, "Zero length file has been submitted");
                HTTPError.AssertRequest("ELI45198", !string.IsNullOrWhiteSpace(file.FileName), "Empty filename");

                var fileStream = file.OpenReadStream();
                HTTPError.Assert("ELI46331", fileStream != null, "Null filestream");

                using (var data = new DocumentData(User, requireSession: false))
                {
                    var result = await data.SubmitFile(file.FileName, fileStream);
                    string url = Request.Path.HasValue
                        ? Request.GetDisplayUrl()
                        : "https://unknown/api/Document";
                    return Created(new Uri($"{url}/{result.Id}"), result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI43654");
            }
        }

        /// <summary>
        /// Gets the source document
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <returns>The original image file associated with the file id</returns>
        [HttpGet("{Id}")]
        [ProducesResponseType(200, Type = typeof(PhysicalFileResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetDocument(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45191", Id);
                    data.AssertFileExists("ELI45192", Id);

                    var fileName = data.GetSourceFileName(Id);
                    var fileContentType = FileContentType(fileName);
                    var fileDownloadName = Path.GetFileName(fileName);
                    fileDownloadName = RemoveGUIDFromName(fileDownloadName);

                    HTTPError.Assert("ELI46359", !String.IsNullOrWhiteSpace(fileDownloadName),
                        "Failed to parse download filename", ("Filename", fileName, false));

                    return PhysicalFile(fileName, fileContentType, fileDownloadName);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI43656");
            }
        }

        /// <summary>
        /// Delete the specified document from the workflow.
        /// </summary>
        /// <param name="Id">The document ID</param>
        [HttpDelete("{Id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult DeleteDocument(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.DeleteDocument(Id);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI43656");
            }
        }

        /// <summary>
        /// Submit text for document processing
        /// </summary>
        /// <param name="textData">The text to submit for processing</param>
        /// <returns>A <see cref="DocumentIdResult"/> with the added document ID.</returns>
        [HttpPost("Text")]
        [ProducesResponseType(201, Type = typeof(DocumentIdResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        // NOTE: If method name changes, ContentTypeSpecifier should be updated to match.
        public async Task<IActionResult> PostText([FromBody]string textData)
        {
            try
            {
                HTTPError.AssertRequest("ELI45194", !string.IsNullOrEmpty(textData),
                    "Submitted text is empty");

                using (var data = new DocumentData(User, requireSession: false))
                {
                    var result = await data.SubmitText(textData);
                    string url = Request.Path.HasValue
                        ? Request.GetDisplayUrl()
                        : "https://unknown/api/Document/Text";
                    url = url.Replace("/Text", $"/{result.Id}/Text");
                    return Created(new Uri(url), result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI43655");
            }
        }

        /// <summary>
        /// Gets the text of a specified document. If the document was submitted as an image file,
        /// the result will be the OCR text for the document (divided into pages). If the document
        /// was submitted as text, the result will be the submitted text.
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <returns><see cref="PageTextResult"/> with the text. In the case that the document was
        /// submitted as text, the text will be added to a single page with a page number of 0.
        /// </returns>
        [HttpGet("{Id}/Text")]
        [ProducesResponseType(200, Type = typeof(PageTextResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetText(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI46279", Id);

                    var textResult = data.GetText(Id);

                    return Ok(data.GetText(Id));
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46280");
            }
        }

        /// <summary>
        /// Gets the status of the specified document.
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <returns>A <see cref="ProcessingStatusResult"/>.</returns>
        [HttpGet("{Id}/Status")]
        [ProducesResponseType(200, Type = typeof(ProcessingStatusResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
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
                return this.GetAsHttpError(ex, "ELI43652");
            }
        }

        /// <summary>
        /// Gets the page size and orientation info for a document.
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <returns></returns>
        [HttpGet("{Id}/PageInfo")]
        [ProducesResponseType(200, Type = typeof(PagesInfoResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetPageInfo(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    var pagesInfo = data.GetPagesInfo(Id);

                    return Ok(pagesInfo);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46274");
            }
        }

        /// <summary>
        /// Gets the text from the specified page of an input document.
        /// NOTE: Cannot be used to retrieve text for a document submitted as text.
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <returns>TextResult instance</returns>
        /// <param name="Page">The page number for which text should be retrieved.</param>
        [HttpGet("{Id}/Page/{Page}/Text")]
        [ProducesResponseType(200, Type = typeof(PageTextResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetPageText(int Id, int Page)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI46305", Id);

                    return Ok(data.GetText(Id, Page));
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46306");
            }
        }

        /// <summary>
        /// Gets each word from a document page along with its spatial location on the document.
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <param name="Page">The page number</param>
        /// <returns></returns>
        [HttpGet("{Id}/Page/{Page}/WordZones")]
        [ProducesResponseType(200, Type = typeof(WordZoneDataResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetPageWordZones(int Id, int Page)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    var result = data.GetWordZoneData(Id, Page);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45357");
            }
        }

        /// <summary>
        /// Gets the document type
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <returns>string containing the type of the document</returns>
        [HttpGet("{Id}/DocumentType")]
        [ProducesResponseType(200, Type = typeof(TextData))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        // NOTE: If method name changes, ContentTypeSpecifier should be updated to match.
        public IActionResult GetDocumentType(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45205", Id);

                    var result = data.GetDocumentType(Id);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI43659");
            }
        }

        /// <summary>
        /// Gets the found data for a document
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <returns>A <see cref="DocumentDataResult"/> containing the document data.</returns>
        [HttpGet("{Id}/Data")]
        [ProducesResponseType(200, Type = typeof(DocumentDataResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetDocumentData(int Id)
        {
            try
            {
                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45199", Id);

                    var result = data.GetDocumentData(Id, includeNonSpatial: true, verboseSpatialData: true);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI43653");
            }
        }

        /// <summary>
        /// Replaces the data for the specified document.
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <param name="documentData">The replacement data for the document</param>
        [HttpPut("{Id}/Data")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        [ProducesResponseType(423, Type = typeof(ErrorResult))]
        public IActionResult PutDocumentData(int Id, [FromBody]DocumentDataInput documentData)
        {
            try
            {
                this.AssertModel("ELI46284");

                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(User, requireSession: true))
                {
                    // IPAddress is used to identify the caller via the "Machine" column in FAMSession. If no RemoteIpAddress
                    // exists, this is likely a unit test; assume 127.0.0.1.
                    string ipAddress = (Request.HttpContext.Connection.RemoteIpAddress ?? IPAddress.Parse("127.0.0.1")).ToString();

                    var user = new User()
                    {
                        Username = User.GetUsername(),
                        WorkflowName = User.GetClaim(Utils._WORKFLOW_NAME)
                    };
                    
                    data.OpenSession(user, ipAddress);
                    data.OpenDocument(Id);

                    try
                    {
                        data.PutDocumentResultSet(data.DocumentSessionFileId, documentData);
                        data.CloseDocument(setStatusTo: EActionStatus.kActionCompleted);
                        data.CloseSession();
                    }
                    catch (Exception ex)
                    {
                        // Make best attempt to make sure the document is released and session closed
                        // upon an error.
                        try
                        {
                            data.CloseDocument(setStatusTo: EActionStatus.kActionFailed, exception: ex);
                        }
                        catch { }

                        try
                        {
                            data.CloseSession();
                        }
                        catch { }

                        throw ex.AsExtract("ELI46286");
                    }

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI43653");
            }
        }

        /// <summary>
        /// Adds, updates or deletes specified document data attributes.
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <param name="documentData">The changes to apply to the document data</param>
        [HttpPatch("{Id}/Data")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        [ProducesResponseType(423, Type = typeof(ErrorResult))]
        public IActionResult PatchDocumentData(int Id, [FromBody]DocumentDataPatch documentData)
        {
            try
            {
                this.AssertModel("ELI46317");

                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(User, requireSession: true))
                {
                    // IPAddress is used to identify the caller via the "Machine" column in FAMSession. If no RemoteIpAddress
                    // exists, this is likely a unit test; assume 127.0.0.1.
                    string ipAddress = (Request.HttpContext.Connection.RemoteIpAddress ?? IPAddress.Parse("127.0.0.1")).ToString();

                    var user = new User()
                    {
                        Username = User.GetUsername(),
                        WorkflowName = User.GetClaim(Utils._WORKFLOW_NAME)
                    };

                    data.OpenSession(user, ipAddress);
                    data.OpenDocument(Id);

                    try
                    {
                        data.PatchDocumentData(data.DocumentSessionFileId, documentData);
                        data.CloseDocument(setStatusTo: EActionStatus.kActionCompleted);
                        data.CloseSession();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            data.CloseDocument(setStatusTo: EActionStatus.kActionFailed, exception: ex);
                        }
                        catch { }

                        try
                        {
                            data.CloseSession();
                        }
                        catch { }

                        throw ex.AsExtract("ELI46318");
                    }

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46319");
            }
        }

        /// <summary>
        /// Gets the output file for the specified input document (e.g., a redacted copy or a searchable PDF)
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <returns>A <see cref="PhysicalFileResult"/>.</returns>
        [HttpGet("{Id}/OutputFile")]
        [ProducesResponseType(200, Type = typeof(PhysicalFileResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        // NOTE: If method name changes, ContentTypeSpecifier should be updated to match.
        public IActionResult GetOutputFile(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45202", Id);

                    var (filename, isError, errMessage) = data.GetResult(Id);
                    HTTPError.Assert("ELI46417", StatusCodes.Status404NotFound,
                        System.IO.File.Exists(filename),
                        "Output document not found", ("ID", Id, true));

                    var fileContentType = FileContentType(filename);
                    var fileDownloadName = Path.GetFileName(filename);
                    fileDownloadName = RemoveGUIDFromName(fileDownloadName);

                    HTTPError.Assert("ELI46360", !String.IsNullOrWhiteSpace(fileDownloadName),
                        "Failed to parse download filename", ("Filename", filename, false));

                    return PhysicalFile(filename, fileContentType, fileDownloadName);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI43657");
            }
        }

        /// <summary>
        /// Gets the output text result; valid only for submitted text.
        /// </summary>
        /// <param name="Id">The document ID</param>
        /// <returns>TextResult instance</returns>
        [HttpGet("{Id}/OutputText")]
        [ProducesResponseType(200, Type = typeof(PageTextResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetOutputText(int Id)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    data.AssertRequestFileId("ELI45204", Id);

                    return Ok(data.GetTextResult(Id));
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI43658");
            }
        }

        #region Private Members

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
            HTTPError.Assert("ELI46361", !String.IsNullOrWhiteSpace(filename), "Missing filename");

            var ext = Path.GetExtension(filename);
            HTTPError.Assert("ELI46362", !String.IsNullOrWhiteSpace(ext), "Missing file extension",
                ("Filename", filename, false));

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
        /// Regex to remove GUIDs from API submitted filenames.
        /// </summary>
        static Regex _guidRegex = new Regex(@"^[0-9,A-F]{8}-([0-9,A-F]{4}-){3}[0-9,A-F]{12}_",
            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Removes the guid added to API submitted filenames.
        /// </summary>
        /// <param name="fileDownloadName">The filename that (may) contain a guid.</param>
        static string RemoveGUIDFromName(string fileDownloadName)
        {
            fileDownloadName = _guidRegex.Replace(fileDownloadName, replacement: "", count: 1, startat: 0);

            return fileDownloadName;
        }

        #endregion Private Members
    }
}
