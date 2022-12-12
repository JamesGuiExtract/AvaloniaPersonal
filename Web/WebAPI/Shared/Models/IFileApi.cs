using Extract.Web.ApiConfiguration.Models;
using System;
using System.Security.Claims;
using UCLID_FILEPROCESSINGLib;

namespace WebAPI
{
    /// Provides access to a FileProcessingDB
    public interface IFileApi
    {
        /// The database server name configured for this instance
        string DatabaseName { get; }
        /// The database name configured for this instance
        string DatabaseServer { get; }
        /// The open state, ID, file ID and start time of a document session
        (bool IsOpen, int Id, int FileId, DateTime StartTime) DocumentSession { get; set; }
        /// The ID of the session in the FAM database with which this instance has been associated
        int FAMSessionId { get; }
        /// Gets the fileProcessingDB instance
        FileProcessingDB FileProcessingDB { get; }
        /// Is this FileApi instance currently being used?
        bool InUse { get; set; }
        /// The session ID (for instances specific to a <see cref="ClaimsPrincipal"/>
        string SessionId { get; }
        /// Gets the number of requests this instance was used for since the last time it was closed.
        int UsesSinceClose { get; set; }
        /// Get the web configuration
        ICommonWebConfiguration WebConfiguration { get; }
        /// Returns the redcation web configuration if applicable
        IRedactionWebConfiguration RedactionWebConfiguration { get; }
        /// Returns the API web configuration if applicable.
        IDocumentApiWebConfiguration APIWebConfiguration { get; }
        /// Get the workflow name configured for this instance
        string WorkflowName { get; }
        /// Get the workflow type
        EWorkflowType WorkflowType { get; }

        /// Raised when this instance's <see cref="InUse"/> flag is being set to <c>false</c> to
        /// indicate it is now available to be used by another request.
        event EventHandler<EventArgs> Releasing;

        /// <summary>
        /// Aborts a session that appears to have been abandoned. This will release any locked files
        /// and make this instance available for use in other sessions.
        /// </summary>
        /// <param name="famSessionId">The ID of the FAM session to be aborted. While this call will
        /// release any ties to any FAM session, The session specified here does not have to be
        /// associated with this instance.</param>
        void AbortSession(int famSessionId = 0);
        /// Assigns this instance to a specific context's session ID. Until the session is
        /// ended/aborted, it will not be available for use in other sessions.
        void AssignSession(ApiContext apiContext);
        /// Ends any associated session in the FAM database and makes this instance available for
        /// other API sessions to use.
        void EndSession();
        /// Disassociates a session to make this instance available for other API sessions to use.
        void SuspendSession();
    }
}