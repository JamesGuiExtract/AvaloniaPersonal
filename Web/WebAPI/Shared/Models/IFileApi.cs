using DynamicData.Kernel;
using Extract.Web.ApiConfiguration.Models;
using System;
using System.Security.Claims;
using UCLID_FILEPROCESSINGLib;

namespace WebAPI
{
    /// <summary>
    /// Provides access to a FileProcessingDB
    /// </summary>
    public interface IFileApi
    {
        /// <summary>
        /// The database server name configured for this instance
    /// </summary>
        string DatabaseName { get; }
        /// <summary>
        /// The database name configured for this instance
        /// </summary>
        string DatabaseServer { get; }
        /// <summary>
        /// The open state, ID, file ID and start time of a document session
        /// </summary>
        (bool IsOpen, int Id, int FileId, DateTime StartTime) DocumentSession { get; set; }
        /// <summary>
        /// The ID of the session in the FAM database with which this instance has been associated
        /// </summary>
        int FAMSessionId { get; }
        /// <summary>
        /// Gets the fileProcessingDB instance
        /// </summary>
        FileProcessingDB FileProcessingDB { get; }
        /// <summary>
        /// Is this FileApi instance currently being used?
        /// </summary>
        bool InUse { get; set; }
        /// <summary>
        /// The session ID (for instances specific to a <see cref="ClaimsPrincipal"/>
        /// </summary>
        string SessionId { get; }
        /// <summary>
        /// Gets the number of requests this instance was used for since the last time it was closed.
        /// </summary>
        int UsesSinceClose { get; set; }
        /// <summary>
        /// Get the web configuration
        /// </summary>
        Optional<ICommonWebConfiguration> WebConfiguration { get; }
        /// <summary>
        /// Returns the redcation web configuration if applicable
        /// </summary>
        IRedactionWebConfiguration RedactionWebConfiguration { get; }
        /// <summary>
        /// Whether this instance is using a <see cref="IRedactionWebConfiguration"/>
        /// </summary>
        bool HasRedactionWebConfiguration { get; }
        /// <summary>
        /// Returns the API web configuration if applicable.
        /// </summary>
        IDocumentApiWebConfiguration APIWebConfiguration { get; }
        /// <summary>
        /// Whether this instance is using a <see cref="IDocumentApiWebConfiguration"/>
        /// </summary>
        bool HasAPIWebConfiguration { get; }
        /// <summary>
        /// Get the workflow name configured for this instance
        /// </summary>
        string WorkflowName { get; }
        /// <summary>
        /// Get the workflow type
        /// </summary>
        EWorkflowType WorkflowType { get; }

        /// <summary>
        /// Raised when this instance's <see cref="InUse"/> flag is being set to <c>false</c> to
        /// indicate it is now available to be used by another request.
        /// </summary>
        event EventHandler<EventArgs> Releasing;

        /// <summary>
        /// Aborts a session that appears to have been abandoned. This will release any locked files
        /// and make this instance available for use in other sessions.
        /// </summary>
        /// <param name="famSessionId">The ID of the FAM session to be aborted. While this call will
        /// release any ties to any FAM session, The session specified here does not have to be
        /// associated with this instance.</param>
        void AbortSession(int famSessionId = 0);
        /// <summary>
        /// Assigns this instance to a specific context's session ID. Until the session is
        /// ended/aborted, it will not be available for use in other sessions.
        /// </summary>
        void AssignSession(ApiContext apiContext);
        /// <summary>
        /// Ends any associated session in the FAM database and makes this instance available for
        /// other API sessions to use.
        /// </summary>
        void EndSession();
        /// <summary>
        /// Disassociates a session to make this instance available for other API sessions to use.
        /// </summary>
        void SuspendSession();
    }
}