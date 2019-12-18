using Extract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;
using WebAPI.Models;

using static WebAPI.Utils;
using ComAttribute = UCLID_AFCORELib.Attribute;

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
        /// for the SessionLogin call.
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
        /// Decrypts the username from the authorization api, and checks to see if the user has an account
        /// </summary>
        /// <param name="user">A user with an encrypted username, and possible workflow</param>
        /// <returns></returns>
        [HttpPost("WindowsLogin")]
        [ProducesResponseType(200, Type = typeof(LoginToken))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult WindowsLogin([FromBody] User user)
        {
            try
            {
                var decryptedText = Encryption.AESThenHMAC.SimpleDecryptWithPassword(user.Username);
                var tokenTime = DateTime.Parse(decryptedText.Split('|')[1]);
                if (! (tokenTime  > DateTime.Now))
                {
                    throw new HTTPError("ELI49485", "This token has expired and is therefore invalid. Please try again");
                }

                user.Username = decryptedText.Split('|')[0];
                HTTPError.AssertRequest("ELI49476", !string.IsNullOrEmpty(user.Username), "Username is empty");

                // The user may have specified a workflow - if so then ensure that the API context uses
                // the specified workflow.
                var context = LoginContext(user.WorkflowName);
                using (var data = new DocumentData(context))
                {
                    // Token is specific to user and FAMSessionId
                    var token = AuthUtils.GenerateToken(user, context);

                    return Ok(new { token, user.Username });
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI49475");
            }
        }

        /// <summary>
        /// API call to change a user's password.
        /// </summary>
        /// <param name="oldPassword">The old password</param>
        /// <param name="newPassword">The New password</param>
        /// <returns>Returns status OK if successful</returns>
        [HttpPost("ChangePassword")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        public IActionResult ChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                var requireSession = User.GetClaim(_FAM_SESSION_ID) != "0";
                using (var data = new DocumentData(User, requireSession))
                {
                    if (requireSession)
                    {
                        data.ChangePassword(User.GetUsername(), oldPassword, newPassword);
                        return Ok();
                    }
                    else
                    {
                        throw new HTTPError("ELI48437", "A session token is required to change your password");
                    }
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI48436");
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
                var requireSession = User.GetClaim(_FAM_SESSION_ID) != "0";
                using (var data = new DocumentData(User, requireSession))
                {
                    if (requireSession)
                    {
                        data.CloseSession();
                    }

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45233");
            }
        }

        /// <summary>
        /// Authenticates a user session. Prefix the returned access_token with "Bearer " to use for authorization
        /// in subsequent API calls.
        /// </summary>
        [HttpPost("SessionLogin")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(LoginToken))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult SessionLogin()
        {
            try
            {
                ExtractException.Assert("ELI46738", "User claims are missing.", User.HasExpectedClaims());
                var workflow = this.User.GetClaim(_WORKFLOW_NAME);

                var context = LoginContext(workflow);
                using (var sessionData = new DocumentData(context))
                {
                    // Starts an active FAM session via FileProcessingDB and ties the active context to the session
                    sessionData.OpenSession(User, Request.GetIpAddress(), "WebRedactionVerification", forQueuing: false, endSessionOnDispose: false);

                    var user = new User
                    {
                        Username = User.GetUsername(),
                        WorkflowName = User.GetClaim(Utils._WORKFLOW_NAME)
                    };

                    // Get the expires date from the claims
                    DateTime expires = DateTime.Parse(User.GetClaim(_EXPIRES_TIME).Trim('"'), null, DateTimeStyles.RoundtripKind);

                    // Token is specific to user and FAMSessionId
                    var token = AuthUtils.GenerateToken(user, context, expires);

                    return Ok(token);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46719");
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
                ExtractException.Assert("ELI46737", "GetSettings requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

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

                    var userName = this.User.GetUsername();
                    var result = data.GetQueueStatus(userName);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45524");
            }
        }

        /// <summary>
        /// Gets a list of queued files
        /// </summary>
        /// <param name="filter">A string that must be present in one of the <see cref="QueuedFileDetails"/> string fields</param>
        /// <param name="fromBeginning">Sort file IDs in ascending order before selecting the subset</param>
        /// <param name="pageIndex">Skip pageIndex * pageSize records from the beginning/end</param>
        /// <param name="pageSize">The maximum records to return</param>
        [HttpGet("QueuedFiles")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(QueuedFilesResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult GetQueuedFiles(string filter, bool fromBeginning = true, int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    var userName = this.User.GetUsername();
                    var result = data.GetQueuedFiles(userName, false, filter, fromBeginning, pageIndex, pageSize);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI47043");
            }
        }

        /// <summary>
        /// Gets a list of skipped files for the current user
        /// </summary>
        /// <param name="filter">A string that must be present in one of the <see cref="QueuedFileDetails"/> string fields</param>
        /// <param name="fromBeginning">Sort file IDs in ascending order before selecting the subset</param>
        /// <param name="pageIndex">Skip pageIndex * pageSize records from the beginning/end</param>
        /// <param name="pageSize">The maximum records to return</param>
        [HttpGet("SkippedFiles")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(QueuedFilesResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult GetSkippedFiles(string filter, bool fromBeginning = true, int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: false))
                {
                    var userName = this.User.GetUsername();
                    var result = data.GetQueuedFiles(userName, true, filter, fromBeginning, pageIndex, pageSize);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI47044");
            }
        }

        /// <summary>
        /// Reserves a document. The document will not be accessible by others
        /// until CloseDocument is called.
        /// </summary>
        /// <param name="docID">The file ID to open. If -1, the next queued document will be opened.
        /// </param>
        /// <param name="processSkipped">If <paramref name="docID"/> is -1, if this is <c>true</c> then the document to open
        /// will be the next one in the skipped queue for the user, if <c>false</c> the next document in the pending queue will be opened
        /// </param>
        [HttpPost("OpenDocument/{docID}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(DocumentIdResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        [ProducesResponseType(423, Type = typeof(ErrorResult))]
        public IActionResult OpenDocument(int docID = -1, bool processSkipped = false)
        {
            try
            {
                ExtractException.Assert("ELI46725", "OpenDocument requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

                using (var data = new DocumentData(User, requireSession: true))
                {
                    var documentId = data.OpenDocument(docID, processSkipped, false, this.User.GetUsername());

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
                ExtractException.Assert("ELI46726", "CloseDocument requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

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
                ExtractException.Assert("ELI46727", "SkipDocument requires an active Session Login token.",
                   User.GetClaim(Utils._FAM_SESSION_ID) != "0");

                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46701", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.SetComment(skipData?.Comment ?? string.Empty);
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
                ExtractException.Assert("ELI46728", "PutComment requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46707", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.SetComment(commentData?.Comment ?? string.Empty);

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
                ExtractException.Assert("ELI46729", "GetComment requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

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
                ExtractException.Assert("ELI46730", "FailDocument requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

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
                ExtractException.Assert("ELI46731", "GetPageInfo requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

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
        // For NSwag auto-client generation; see also: MapType<FileResult> in ConfigureServices
        [SwaggerResponse(200, typeof(FileResult))]
        // Specify FileResult base type. If PhysicalFileResult is specified, the swagger
        // documentation lists the model as the return type with the physical path providing the
        // file which can be confusing for a consumer needing to stream the file.
        [ProducesResponseType(200, Type = typeof(FileResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetDocumentPage(int docID, int page)
        {
            try
            {
                ExtractException.Assert("ELI46732", "GetDocument requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46704", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    var imageData = data.GetPageImage(docID, page, cacheData: true);

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
                ExtractException.Assert("ELI46733", "GetDocumentData requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46705", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    // https://extract.atlassian.net/browse/WEB-59
                    // Per discussion with GGK, non-spatial attributes will not be sent to the web app.
                    // https://extract.atlassian.net/browse/ISSUE-16202
                    // For redaction verification, split multi-page attributes into separate attributes
                    // per page so the front-end doesn't need code to deal with multi-page attributes.
                    var result = data.GetDocumentData(
                        data.DocumentSessionFileId, includeNonSpatial: false, verboseSpatialData: false,
                        splitMultiPageAttributes: true, cacheData: true);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45279");
            }
        }

        /// <summary>
        /// Updates the document data for the specified page. Edits via this call are not applied to
        /// the document data set as a whole until POST DocumentData is invoked.
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        /// <param name="page">The page for which data is being edited</param>
        /// <param name="documentData">The new document data for the page (this completely replaces
        /// the previous document data for the page.</param>
        [HttpPut("DocumentData/{docID}/{page}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult EditPageData(int docID, int page, [FromBody] List<DocumentAttribute> documentData)
        {
            try
            {
                ExtractException.Assert("ELI46734", "SaveDocumentData requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46699", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.EditPageData(data.DocumentSessionFileId, page, documentData);

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45271");
            }
        }

        /// <summary>
        /// Commits all edits applied via PUT DocumentData/{docID}/{page} to a new data set for the
        /// document as a whole.
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        [HttpPost("DocumentData/{docID}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult CommitDocumentData(int docID)
        {
            try
            {
                ExtractException.Assert("ELI49550", "SaveDocumentData requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI49551", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.CommitCachedDocumentData(docID);

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI49552");
            }
        }

        /// <summary>
        /// Gets edited pages of data for the document that were made in previous sessions but were
        /// never POSTed to create a new document data set.
        /// </summary>
        /// <param name="docID">The document for which the data is requested</param>
        /// <returns>The json representation of the uncommitted pages of data. Only those pages that
        /// were editied will be included; data will not be returned for unedited pages.</returns>
        [HttpGet("UncommittedDocumentData/{docID}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(UncommittedDocumentDataResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult GetUncommittedDocumentData(int docID)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    var uncommittedData = data.GetUncommittedDocumentData(docID);

                    return Ok(uncommittedData);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45271");
            }
        }

        /// <summary>
        /// Deletes all cache data for this document not associated with this session including
        /// edited page data never committed.
        /// </summary>
        /// <param name="docID">The document for which the data is to be deleted</param>
        [HttpDelete("UncommittedDocumentData/{docID}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult DeleteOldCacheData(int docID)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    data.DiscardOldCacheData(docID);

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI49532");
            }
        }

        /// <summary>
        /// Gets the metadata field for a document.
        /// </summary>
        /// <param name="docID">The document to obtain the metadatafield for</param>
        /// <param name="metadataField">The metadatafield to obtain</param>
        /// <returns>The json result of the metadata field</returns>
        [HttpGet("MetadataField/{docID}/{metadataField}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(MetadataFieldResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult GetMetadataField(int docID, string metadataField)
        {
            try
            {
                ExtractException.Assert("ELI47202", "GetMetadataField requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI47192", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    return Ok(data.GetMetadataField(metadataField));
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI47197");
            }
        }

        /// <summary>
        /// Sets a metadata field in the database
        /// </summary>
        /// <param name="docID">The document id to set the metadata field for</param>
        /// <param name="metadataField">The metadata field to assign to</param>
        /// <param name="metadataFieldValue">The value you want to assign the metadata field to.</param>
        /// <returns></returns>
        [HttpPut("MetadataField/{docID}/{metadataField}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult SetMetadataField(int docID, string metadataField, string metadataFieldValue = "")
        {
            try
            {
                ExtractException.Assert("ELI47199", "SetMetadataField requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI47195", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    data.SetMetadataField(metadataField, metadataFieldValue);

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI47198");
            }
        }

        /// <summary>Transform a DocumentAttribute in some way</summary>
        /// <param name="docID">The currently open document ID</param>
        /// <param name="pageNumber">The page scope of the operation</param>
        /// <param name="parameters">The parameters</param>
        [HttpPost("ProcessAnnotation/{docID}/{pageNumber}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(DocumentAttribute))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult ProcessAnnotation(int docID, int pageNumber, [FromBody] ProcessAnnotationParameters parameters)
        {
            try
            {
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI46749", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    var fileName = data.GetSourceFileName(docID);
                    var translator = new AttributeTranslator(fileName, parameters.Annotation);
                    var attribute = (ComAttribute) translator.ComAttribute;

                    var annotationProcessorType = Type.GetTypeFromProgID("Extract.AttributeFinder.Rules.AnnotationProcessor");
                    var annotationProcessor = (UCLID_AFCORELib.IAnnotationProcessor)Activator.CreateInstance(annotationProcessorType);
                    var updated = annotationProcessor.ProcessAttribute(fileName, pageNumber, attribute, parameters.OperationType, parameters.Definition);
                    var mapper = new AttributeMapper(null, data.WorkflowType);
                    var updatedAttribute =  mapper.MapAttribute(updated, false);

                    return Ok(updatedAttribute);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46750");
            }
        }

        /// <summary>
        /// The word zone data, grouped by line.
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
                ExtractException.Assert("ELI46735", "GetPageWordZones requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

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

        /// <summary>
        /// Gets the results of a search as <see cref="DocumentAttribute"/>s
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        /// <param name="searchParameters">The query and options for the search</param>
        [HttpPost("Search/{docID}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(DocumentDataResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public IActionResult PostSearch(int docID, [FromBody] SearchParameters searchParameters)
        {
            try
            {
                ExtractException.Assert("ELI48304", "GetSearchResults requires an active Session Login token.",
                    User.GetClaim(Utils._FAM_SESSION_ID) != "0");

                // using ensures that the underlying FileApi.InUse flag is cleared on exit
                using (var data = new DocumentData(User, requireSession: true))
                {
                    ExtractException.Assert("ELI48305", "The supplied document ID doesn't match the open session's document ID",
                        docID == data.DocumentSessionFileId);

                    var result = data.GetSearchResults(docID: data.DocumentSessionFileId, searchParameters: searchParameters);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI48306");
            }
        }

        /// <summary>
        /// Caches image and OCR data for the specified page in the database. The cached data will remain
        /// in the database until this document or verification session are closed.
        /// <para><b>Note</b></para>
        /// GET DocumentPage will automatically trigger the data of the page subsequent to the
        /// requested page to be cached without use of this method.
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        /// <param name="page">The page for which data should be cached.</param>
        /// <returns></returns>
        [HttpPost("CachePageDataAsync/{docID}/{pageNumber}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404, Type = typeof(ErrorResult))]
        public async Task<IActionResult> CachePageDataAsync(int docID, int page)
        {
            try
            {
                int documentSessionID;

                // While data caching occurs asynchronously and does not require an instance with an
                // active open document sesssion, a document session ID is required. That ID needs
                // to be retrieved from the FAMDB, so we do need an instance with a session to get
                // that ID.
                using (var data = new DocumentData(User, requireSession: true))
                {
                    documentSessionID = data.DocumentSessionId;
                }

                HTTPError.Assert("ELI49438", "No document session active", documentSessionID > 0);

                // The caching thread won't be confined to time the session is active. If another
                // API call closes the session before the caching completes, FileProcessingDB will
                // simply ignore the cache request.
                await DocumentData.CachePageDataAsync(documentSessionID, docID, page).ConfigureAwait(false);

                return NoContent();
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI48389");
            }
        }
    }
}
