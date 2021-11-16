using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using UCLID_FILEPROCESSINGLib;

namespace WebAPI.Models
{
    /// The data model (service?) used by the DocumentController and AppBackendApi to do document stuff
    public interface IDocumentData : IDisposable
    {
        /// The document session file identifier
        int DocumentSessionFileId { get; }
        /// The document session identifier
        int DocumentSessionId { get; }
        /// Gets the workflow type
        EWorkflowType WorkflowType { get; }
        /// <summary>
        /// Asserts that a document session is open
        /// </summary>
        /// <param name="eliCode">The ELI code to use for the thrown exception</param>
        void AssertDocumentSession(string eliCode);
        /// <summary>
        /// Assert that a file exists
        /// </summary>
        /// <param name="eliCode">The ELI code to use for the thrown exception</param>
        /// <param name="fileId">The ID of the file to check</param>
        void AssertFileExists(string eliCode, int fileId);
        /// <summary>
        /// Assert that a file exists
        /// </summary>
        /// <param name="eliCode">The ELI code to use for the thrown exception</param>
        /// <param name="fileName">The path of the file to check</param>
        void AssertFileExists(string eliCode, string fileName);
        /// <summary>
        /// Assert that a file is in the workflow
        /// </summary>
        /// <param name="eliCode">The ELI code to use for the thrown exception</param>
        /// <param name="fileId">The ID of the file to check</param>
        void AssertRequestFileId(string eliCode, int fileId);
        /// <summary>
        /// Assert that a page exists for a file
        /// </summary>
        /// <param name="eliCode">The ELI code to use for the thrown exception</param>
        /// <param name="fileId">The ID of the file to check</param>
        /// <param name="page">The page to check</param>
        void AssertRequestFilePage(string eliCode, int fileId, int page);
        /// Allows a user to change their password.
        void ChangePassword(string userName, string oldPassword, string newPassword);
        /// <summary>
        /// Releases the document
        /// </summary>
        /// <param name="setStatusTo"><see cref="EActionStatus.kActionCompleted"/> to commit the document so that it advances in the
        /// workflow; other values to save the document but set the file's status in the EditAction to a non-completed value.</param>
        /// <param name="exception">Exception to log if <see paramref="setStatusTo"/> is <see cref="EActionStatus.kActionFailed"/></param>
        /// <param name="activityTime">Duration, in ms, for updating the activityTime of the file task session record</param>
        /// <param name="overheadTime">Duration, in ms, for updating the overheadTime of the file task session record</param>
        /// <param name="closedBecauseOfInactivity">Whether this close is because the session timed-out because the user was inactive for too long</param>
        void CloseDocument(EActionStatus setStatusTo, Exception exception = null, int activityTime = -1, int overheadTime = -1, bool closedBecauseOfInactivity = false);
        /// Closes the web application session
        void CloseSession();
        /// Commits all document data edits made via <see cref="EditPageData"/> to a new attribute
        /// set for the file.
        void CommitCachedDocumentData(int fileId);
        /// Marks the document as deleted in the workflow. Does not necessarily mean the document is
        /// physically deleted, though depending on how the workflow is configured, it could be.
        void DeleteDocument(int fileId);
        /// Deletes all cache data rows that are not associated with this document session.
        void DiscardOldCacheData(int fileId);
        /// <summary>
        /// Edits a given page of a document by replacing the existing attributes with the specified
        /// <see paramref="inputData"/>.
        /// <para><b>Note</b></para>
        /// Edits made via this call will only become part of a new attribute set after calling
        /// <see cref="CommitCachedDocumentData"/>.
        /// </summary>
        /// <param name="fileId">The ID of the file for which to edit data.</param>
        /// <param name="page">The page number for which data is to be edited.</param>
        /// <param name="inputData">The new <see cref="DocumentAttribute"/>s to apply to the page.</param>
        void EditPageData(int fileId, int page, List<DocumentAttribute> inputData);
        /// Gets the FileActionComment of the EditAction for the open file
        CommentData GetComment();
        /// <summary>
        /// Gets the document attribute set
        /// </summary>
        /// <param name="fileId">The ID of the file for which to retrieve data.</param>
        /// <param name="includeNonSpatial"><c>true</c> to include non-spatial attributes in the resulting data;
        /// otherwise, <c>false</c>. NOTE: If false, a non-spatial attribute will be excluded even if it has
        /// spatial children.</param>
        /// <param name="verboseSpatialData"><c>false</c> to include only the spatial data needed for
        /// extract software to represent spatial strings; <c>true</c> to include data that may be
        /// useful to 3rd party integrators.</param>
        /// <param name="splitMultiPageAttributes"><c>true</c> to split multi-page attributes into a separate
        /// attribute for every page; <c>false</c> to map multi-page attributes as they are.</param>
        /// <param name="cacheData">Specifies if the attribute data for this file should be cached
        /// as a side effect of the call. Caching is required if data is to be edited via <see cref="EditPageData"/>
        /// and <see cref="CommitCachedDocumentData"/>. In order to be cached,
        /// <see paramref="splitMultiPageAttributes"/> must be <c>true</c>.</param>
        /// <returns>DocumentAttributeSet instance, including error info iff there is an error</returns>
        DocumentDataResult GetDocumentData(int fileId, bool includeNonSpatial, bool verboseSpatialData, bool splitMultiPageAttributes, bool cacheData);
        /// Get the document type for a file
        TextData GetDocumentType(int id);
        /// <summary>
        /// Gets the metadata field value for the specified file
        /// </summary>
        /// <param name="fileId">The document for which metadata should be retrieved.</param>
        /// <param name="metaDataField">The field to obtain the value from</param>
        MetadataFieldResult GetMetadataField(int fileId, string metaDataField);
        /// <summary>
        /// Gets the page image.
        /// <para><b>Note</b></para>
        /// This call has the side-effect of triggering the caching of data for the subsequent document page.
        /// </summary>
        /// <param name="pageNum">The page number.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="cacheData"><c>true</c> to cache the image and uss data for the next page as
        /// a side effect of this call.</param>
        /// <returns>An array of bytes representing a PDF image of the page.</returns>
        byte[] GetPageImage(int fileId, int pageNum, bool cacheData);
        /// Gets information about all the pages in the specified document
        PagesInfoResult GetPagesInfo(int fileId);
        /// <summary>
        /// Gets a page of queued/skipped files for the workflow's Edit action
        /// </summary>
        /// <param name="userName">The currently logged in user's name</param>
        /// <param name="skippedFiles">Whether to return files skipped for this user rather than pending files</param>
        /// <param name="filter">Search string to filter the results</param>
        /// <param name="fromBeginning">Sort file IDs in ascending order before selecting the subset</param>
        /// <param name="pageIndex">Skip pageIndex * pageSize records from the beginning/end</param>
        /// <param name="pageSize">The maximum records to return</param>
        QueuedFilesResult GetQueuedFiles(string userName, bool skippedFiles, string filter, bool fromBeginning, int pageIndex, int pageSize);
        /// Gets the number of document, pages and active users in the current verification queue.
        QueueStatusResult GetQueueStatus(string userName);
        /// Get filename, error flag and error message for a file
        (string filename, bool error, string errorMessage) GetResult(int fileId);
        /// <summary>
        /// Gets the results of a search as <see cref="DocumentAttribute"/>s
        /// </summary>
        /// <param name="docID">The currently open document ID</param>
        /// <param name="searchParameters">The query and options for the search</param>
        DocumentDataResult GetSearchResults(int docID, SearchParameters searchParameters);
        /// Gets the settings for the web application
        WebAppSettingsResult GetSettings();
        /// Gets the full path of the original source file
        string GetSourceFileName(int fileId);
        /// Get a list of ProcessingStatuses for a document
        ProcessingStatusResult GetStatus(int fileId);
        /// Get the recognized text for a document or a single page
        PageTextResult GetText(int Id, int page = -1);
        /// Get the recognized text for a document
        PageTextResult GetTextResult(int Id);
        /// Gets all pages of uncommitted data edits from document sessions other than this one
        /// so long as a new attribute set has not been stored for the document more recently than
        /// the edits were made.
        UncommittedDocumentDataResult GetUncommittedDocumentData(int fileId);
        /// The word zone data for a page, grouped by line.
        WordZoneDataResult GetWordZoneData(int fileId, int page);
        /// <summary>
        /// Checks-out a document
        /// </summary>
        /// <param name="taskGuid">The GUID identifying the source of the operation in the database.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="processSkipped">If <paramref name="id"/> is -1, if this is <c>true</c> then the document to open
        /// will be the next one in the skipped queue for the user, if <c>false</c> the next document in the pending queue will be opend</param>
        /// <param name="dataUpdateOnly"><c>true</c> if the session is being opened only for updating data, in which case the session
        /// will be allowed for completed/failed files; otherwise, <c>false</c>.</param>
        /// <param name="userName"> The username of the user who is making the call. Only required for the recursive version </param>
        /// <param name="retries"> the number of times to retry. </param>
        DocumentIdResult OpenDocument(string taskGuid, int id, bool processSkipped = false, bool dataUpdateOnly = false, string userName = "", int retries = 10);
        /// <summary>
        /// Opens a session for the specified ClaimsPrincipal.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> this instance is specific to.</param>
        /// <param name="remoteIpAddress">The IP address of the web application user.s</param>
        /// <param name="apiName">The name the API should be identified in the established session.</param>
        /// <param name="forQueuing"><c>true</c> if this session is to queue files; <c>false</c> for
        /// processing.</param>
        /// <param name="endSessionOnDispose">Whether to call EndSession() when this instance is disposed</param>
        void OpenSession(ClaimsPrincipal claimsPrincipal, string remoteIpAddress, string apiName, bool forQueuing, bool endSessionOnDispose);
        /// <summary>
        /// Opens a session for the specified user.
        /// </summary>
        /// <param name="user">The <see cref="User"/> this instance is specific to.</param>
        /// <param name="remoteIpAddress">The IP address of the web application user.</param>
        /// <param name="apiName">The name the API should be identified in the established session.</param>
        /// <param name="forQueuing"><c>true</c> if this session is to queue files; <c>false</c> for
        /// processing.</param>
        /// <param name="endSessionOnDispose">Whether to call EndSession() when this instance is disposed</param>
        void OpenSession(User user, string remoteIpAddress, string apiName, bool forQueuing, bool endSessionOnDispose);
        /// <summary>
        /// Patches attributes in the existing document attribute set.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="patchData">The updated data.</param>
        void PatchDocumentData(int fileId, DocumentDataPatch patchData);
        /// <summary>
        /// Replaces the document attribute set.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="inputData">The updated data.</param>
        void PutDocumentResultSet(int fileId, DocumentDataInput inputData);
        /// Sets the FileActionComment of the EditAction for the open file
        void SetComment(string comment);
        /// <summary>
        /// Sets the metadatafield value in the database.
        /// </summary>
        /// <param name="fileId">The document for which metadata should be retrieved.</param>
        /// <param name="metadataField">The metadata field to assign</param>
        /// <param name="metadataFieldValue">The metadatafield value</param>
        void SetMetadataField(int fileId, string metadataField, string metadataFieldValue);
        /// Submits a file to be added to the database
        DocumentIdResult SubmitFile(string fileName, Stream fileStream);
        /// Submits text to be added to the database
        DocumentIdResult SubmitText(string submittedText);
    }
}