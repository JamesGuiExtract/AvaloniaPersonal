using Extract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Net;
using WebAPI.Models;

using static WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Backed API support for web verification applications.
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    [BindRequired]
    public class AppBackendController : Controller
    {
        /// <summary>
        /// Authenticates a user. The returned JWT token will be used to access session/document state in subsequent calls.
        /// </summary>
        /// <param name="user">A User object (name, password, optional claim)</param>
        [HttpPost("Login")]
        public IActionResult Login([FromBody] User user)
        {
            try
            {
                RequestAssertion.AssertSpecified("ELI45183", user.Username, "Username is empty");
                RequestAssertion.AssertSpecified("ELI45184", user.Password, "Password is empty");

                // The user may have specified a workflow - if so then ensure that the API context uses
                // the specified workflow.
                var context = LoginContext(user.WorkflowName);
                using (var userData = new UserData(context))
                using (var data = new DocumentData(context))
                {
                    userData.LoginUser(user);

                    // IPAddress is used to identify the caller via the "Machine" column in FAMSession. If no RemoteIpAddress
                    // exists, this is likely a unit test; assume 127.0.0.1.
                    string ipAddress = (Request.HttpContext.Connection.RemoteIpAddress ?? IPAddress.Parse("127.0.0.1")).ToString();

                    // Starts an active FAM session via FileProcessingDB and ties the active context to the session
                    data.OpenSession(user, ipAddress);

                    // Token is specific to user and FAMSessionId
                    var token = AuthUtils.GenerateToken(user, context);

                    return Ok(token);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45188");
            }
        }

        /// <summary>
        /// Logs out, thereby ending the session established by Login.
        /// </summary>
        [HttpPost("Logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    data.CloseSession();

                    var result = new GenericResult()
                    {
                        Error = MakeError(isError: false, message: "", code: 0)
                    };

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45233");
            }
        }

        /// <summary>
        /// Gets settings for the application.
        /// </summary>
        [HttpGet("GetSettings")]
        [Produces(typeof(WebAppSettings))]
        [Authorize]
        public IActionResult GetSettings()
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    var result = data.GetSettings();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45273");
            }
        }

        /// <summary>
        /// Gets the number of document, pages and active users in the current verification queue.
        /// </summary>
        [HttpGet("GetQueueStatus")]
        [Produces(typeof(QueueStatus))]
        [Authorize]
        public IActionResult GetQueueStatus()
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    var result = data.GetQueueStatus();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45524");
            }
        }

        /// <summary>
        /// Reserves a document. The document will not be accessible by others
        /// until CloseDocument is called.
        /// </summary>
        /// <param name="id">The file ID to open. If -1, the next queued document will be opened.
        /// </param>
        [HttpPost("OpenDocument/{id}")]
        [Produces(typeof(DocumentId))]
        [Authorize]
        public IActionResult OpenDocument(int id = -1)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    var documentId = data.OpenDocument(id);

                    return Ok(documentId);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45245");
            }
        }

        /// <summary>
        /// Releases the document so that it is again available to others in the workflow.
        /// </summary>
        /// <param name="commit"><c>true</c> if the document is to be committed as complete in
        /// verification; <c>false</c> to keep the specified document in verification.</param>
        [HttpPost("CloseDocument")]
        [Authorize]
        public IActionResult CloseDocument(bool commit)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    data.CloseDocument(commit);

                    var result = new GenericResult()
                    {
                        Error = MakeError(isError: false, message: "", code: 0)
                    };

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45246");
            }
        }

        /// <summary>
        /// Get page size and orientation info for a document
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetPageInfo")]
        [Produces(typeof(PagesInfo))]
        [Authorize]
        public IActionResult GetPageInfo()
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    int fileId = data.DocumentSessionFileId;
                    data.AssertRequestFileId("ELI45172", fileId);
                    data.AssertRequestFileExists("ELI45173", fileId);

                    data.GetSourceFileName(fileId);
                    var pagesInfo = data.GetPagesInfo(fileId);

                    return Ok(pagesInfo);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError<PagesInfo>(ex, "ELI45189");
            }
        }

        /// <summary>
        /// Gets the specified document page as a PDF file.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetDocumentPage/{Page}")]
        [Produces(typeof(FileResult))]
        [Authorize]
        public IActionResult GetDocumentPage(int page)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    int fileId = data.DocumentSessionFileId;
                    var imageData = data.GetPageImage(fileId, page);

                    return File(imageData, "application/pdf", $"{fileId}-{page}.pdf");
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45190");
            }
        }

        /// <summary>
        /// Gets the document data.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetDocumentData")]
        [ProducesResponseType(typeof(DocumentAttributeSet), 200)]
        [Authorize]
        public IActionResult GetDocumentData()
        {
            try
            {
                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(User, requireSession: true))
                {
                    // https://extract.atlassian.net/browse/WEB-59
                    // Per discussion with GGK, non-spatial attributes will not be sent to the web app.
                    var result = data.GetDocumentResultSet(
                        data.DocumentSessionFileId, includeNonSpatial: false, verboseSpatialData: false);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45279");
            }
        }

        /// <summary>
        /// Saves the document data.
        /// </summary>
        /// <param name="documentData">The document data.</param>
        /// <returns></returns>
        [HttpPost("SaveDocumentData")]
        [Produces(typeof(GenericResult))]
        [Authorize]
        public IActionResult SaveDocumentData([FromBody] BareDocumentAttributeSet documentData)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    data.UpdateDocumentResultSet(data.DocumentSessionFileId, documentData);

                    var result = new GenericResult()
                    {
                        Error = MakeError(isError: false, message: "", code: 0)
                    };

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45271");
            }
        }

        /// <summary>
        /// Gets the page word zones.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        [HttpGet("GetPageWordZones")]
        [Produces(typeof(WordZoneData))]
        [Authorize]
        public IActionResult GetPageWordZones(int page)
        {
            try
            {
                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(User, requireSession: true))
                {
                    var result = data.GetWordZoneData(page);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45357");
            }
        }
    }
}
