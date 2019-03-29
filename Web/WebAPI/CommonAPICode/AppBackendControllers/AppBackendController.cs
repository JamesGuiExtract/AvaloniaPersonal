using Extract;
using UCLID_FILEPROCESSINGLib;
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
        /// Authenticates a user. Prefix the returned access_token with "Bearer " to use for authorization
        /// in subsequent API calls.
        /// </summary>
        /// <param name="user">Login credentials. WorkflowName is optional;
        /// specify only if a special workflow is required.</param>
        [HttpPost("Login")]
        [ProducesResponseType(200, Type = typeof(LoginToken))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult Login([FromBody] User user)
        {
            try
            {
                HTTPError.AssertRequest("ELI45183", !string.IsNullOrEmpty(user.Username), "Username is empty");
                HTTPError.AssertRequest("ELI45184", !string.IsNullOrEmpty(user.Password), "Password is empty");

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
                    data.OpenSession(user, ipAddress, false);

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
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult Logout()
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    data.CloseSession();

                    return NoContent();
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
        [HttpGet("Settings")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(WebAppSettingsResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
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
        /// <param name="docID">The open document ID. If docID > 0 then this call will act as a ping to keep the current session alive.
        /// </param>
        [HttpGet("QueueStatus")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(QueueStatusResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult GetQueueStatus(int docID = -1)
        {
            try
            {
                bool fileIsOpen = docID > 0;
                using (var data = new DocumentData(User, requireSession: fileIsOpen))
                {
                    ExtractException.Assert("ELI46697", "The supplied document ID doesn't match the open session's document ID",
                        !fileIsOpen || docID == data.DocumentSessionFileId);

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
        /// <param name="docID">The file ID to open. If -1, the next queued document will be opened.
        /// </param>
        [HttpPost("OpenDocument/{docID}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(DocumentIdResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        [ProducesResponseType(423, Type = typeof(ErrorResult))]
        public IActionResult OpenDocument(int docID = -1)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    var documentId = data.OpenDocument(docID);

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
        /// <param name="docID">The currently opened document ID</param>
        /// <param name="commit"><c>true</c> if the document is to be committed as complete in
        /// verification; <c>false</c> to keep the specified document in verification.</param>
        /// <param name="duration">Optional duration, in ms, to use for updating the file task session record</param>
        [HttpPost("CloseDocument/{docID}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult CloseDocument(int docID, bool commit, int duration = -1)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46698", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.CloseDocument(commit ? EActionStatus.kActionCompleted : EActionStatus.kActionPending, duration);

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45246");
            }
        }

        /// <summary>
        /// Skip the document so that it will not be in the queue
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        /// <param name="skipData">Contains the Duration in ms, to use for updating the file task session record
        /// and the comment to apply to the file</param>
        [HttpPost("SkipDocument/{docID}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult SkipDocument(int docID, [FromBody]  SkipDocumentData skipData = null)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46701", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.SetComment(skipData?.Comment ?? "");
                    data.CloseDocument(EActionStatus.kActionSkipped, skipData?.Duration ?? -1);

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46689");
            }
        }

        /// <summary>
        /// Apply a comment to a document
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        /// <param name="commentData">Contains the comment to apply to the document</param>
        [HttpPut("Comment/{docID}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult AddComment(int docID, [FromBody]  CommentData commentData = null)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46707", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.SetComment(commentData?.Comment ?? "");

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46708");
            }
        }

        /// <summary>
        /// The comment of a document
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        /// <returns>The comment applied to the open document</returns>
        [HttpGet("Comment/{docID}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult GetComment(int docID)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46714", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    var commentData = data.GetComment();

                    return Ok(commentData);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46715");
            }
        }


        /// <summary>
        /// Fail the document so that it will not be in the queue
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        /// <param name="duration">Optional duration, in ms, to use for updating the file task session record</param>
        [HttpPost("FailDocument/{docID}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult FailDocument(int docID, int duration = -1)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46702", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.CloseDocument(EActionStatus.kActionFailed, duration);

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46690");
            }
        }

        /// <summary>
        /// Get page size and orientation info for a document
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        /// <returns></returns>
        [HttpGet("PageInfo/{docID}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(PagesInfoResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult GetPageInfo(int docID)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46703", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.AssertRequestFileId("ELI45172", docID);
                    data.AssertFileExists("ELI45173", docID);

                    data.GetSourceFileName(docID);
                    var pagesInfo = data.GetPagesInfo(docID);

                    return Ok(pagesInfo);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45189");
            }
        }

        /// <summary>
        /// Gets the specified document page as a PDF file.
        /// </summary>
        /// <param name="page">The page to retrieve</param>
        /// <param name="docID">The currently open document ID</param>
        /// <returns></returns>
        [HttpGet("DocumentPage/{docID}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(FileResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetDocumentPage(int docID, int page)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46704", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    var imageData = data.GetPageImage(docID, page);

                    return File(imageData, "application/pdf", $"{docID}-{page}.pdf");
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
        /// <param name="docID">The currently open document ID</param>
        /// <returns></returns>
        [HttpGet("DocumentData/{docID}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(DocumentDataResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetDocumentData(int docID)
        {
            try
            {
                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46705", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    // https://extract.atlassian.net/browse/WEB-59
                    // Per discussion with GGK, non-spatial attributes will not be sent to the web app.
                    var result = data.GetDocumentData(
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
        /// <param name="docID">The document ID that the data is for</param>
        /// <param name="documentData">The document data.</param>
        /// <returns></returns>
        [HttpPut("DocumentData/{docID}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult SaveDocumentData(int docID, [FromBody] DocumentDataInput documentData)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46699", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.PutDocumentResultSet(data.DocumentSessionFileId, documentData);

                    return NoContent();
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
        /// <param name="docID">The currently open document ID</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        [HttpGet("PageWordZones/{docID}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(WordZoneDataResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetPageWordZones(int docID, int page)
        {
            try
            {
                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46700", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    var result = data.GetWordZoneData(docID, page);

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
