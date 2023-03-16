using DynamicData.Kernel;
using Extract;
using Extract.Web.ApiConfiguration.Models;
using System;
using System.Security.Claims;
using System.Threading;
using UCLID_FILEPROCESSINGLib;
using static WebAPI.Utils;

namespace WebAPI
{
    /// <summary>
    /// Container for a fileProcessingDB instance, along with instance data and methods. This container will be used
    /// in a manager API that contains a collection of FileApis.
    /// </summary>
    public class FileApi : IFileApi
    {
        static readonly AutoResetEvent _instanceReleased = new(false);

        private readonly ApiContext _apiContext;
        private string _sessionId = "";
        private bool _inUse;
        private readonly FileProcessingDB _fileProcessingDB = null;

        /// <summary>
        /// Initializes a new <see cref="FileApi"/> instance.
        /// </summary>
        /// <param name="apiContext">The <see cref="ApiContext"/> defining the database environment
        /// for this instance.</param>
        /// <param name="setInUse"><c>true</c> to set the InUse flag on object creation;
        /// otherwise, <c>false</c>.</param>
        public FileApi(ApiContext apiContext, bool setInUse = false)
        {
            try
            {
                FAMDBUtils dbUtils = new FAMDBUtils();
                Type mgrType = Type.GetTypeFromProgID(dbUtils.GetFAMDBProgId());
                _fileProcessingDB = (FileProcessingDB)Activator.CreateInstance(mgrType);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception creating FileProcessingDB instance: {ex.Message}"), "ELI43249");
                throw;
            }

            try
            {
                _apiContext = apiContext;

                _fileProcessingDB.DatabaseServer = apiContext.DatabaseServerName;
                _fileProcessingDB.DatabaseName = apiContext.DatabaseName;
                apiContext.WebConfiguration.IfHasValue(config => _fileProcessingDB.ActiveWorkflow = config.WorkflowName);
                _fileProcessingDB.NumberOfConnectionRetries = apiContext.NumberOfConnectionRetries;
                _fileProcessingDB.ConnectionRetryTimeout = apiContext.ConnectionRetryTimeout;

                InUse = setInUse;

                Log.WriteLine("Created a new FileApi object, " +
                              Inv($"for workflowName: {WorkflowName}, ",
                              $"DatabaseServerName: {DatabaseServer}, ",
                              $"DatabaseName: {DatabaseName}"), "ELI43250");
            }
            catch (HTTPError httpError)
            {
                try
                {
                    _fileProcessingDB.CloseAllDBConnections();
                }
                catch { }

                throw httpError;
            }
            catch (Exception ex)
            {
                try
                {
                    _fileProcessingDB.CloseAllDBConnections();
                }
                catch { }

                var ee = ex.AsExtract("ELI43264");
                ee.AddDebugData("Message", "Exception reported from MakeAssociatedWorkflow", encrypt: false);
                ee.AddDebugData("Workflow name", WorkflowName, encrypt: false);
                ee.AddDebugData("Database server name", DatabaseServer, encrypt: false);
                ee.AddDebugData("Database name", DatabaseName, encrypt: false);

                throw ee;
            }
        }

        /// <summary>
        /// Raised when this instance's <see cref="InUse"/> flag is being set to <c>false</c> to
        /// indicate it is now available to be used by another request.
        /// </summary>
        public event EventHandler<EventArgs> Releasing;

        /// <summary>
        /// Gets the fileProcessingDB instance
        /// </summary>
        public FileProcessingDB FileProcessingDB
        {
            get
            {
                return _fileProcessingDB;
            }
        }

        /// <summary>
        /// The database server name configured for this instance
        /// </summary>
        public string DatabaseServer
        {
            get
            {
                return _apiContext.DatabaseServerName;
            }
        }

        /// <summary>
        /// The database name configured for this instance
        /// </summary>
        public string DatabaseName
        {
            get
            {
                return _apiContext.DatabaseName;
            }
        }

        internal void ResumeSession(int requestedFAMSessionId)
        {
            try
            {
                if (_fileProcessingDB.ResumeWebSession(requestedFAMSessionId, out int fileTaskSessionID, out int fileID))
                {
                    DocumentSession =
                    (
                        true,
                        fileTaskSessionID,
                        fileID,
                        DateTime.Now
                    );
                }
                else
                {
                    DocumentSession = (false, 0, 0, new DateTime());
                }
            }
            catch (Exception ex)
            {
                var sessionExists = false;
                try
                {
                    sessionExists = _fileProcessingDB.IsFAMSessionOpen(requestedFAMSessionId);
                }
                catch (Exception ex_)
                {
                    ex_.ExtractLog("ELI46722");
                    throw ex.AsExtract("ELI46723");
                }

                if (sessionExists)
                {
                    throw ex;
                }
                else
                {
                    throw new HTTPError("ELI46721", 401, "Session does not exist");
                }
            }
        }

        /// <summary>
        /// Get the workflow name configured for this instance
        /// </summary>
        public string WorkflowName
        {
            get
            {
                return _apiContext.WebConfiguration.ValueOrThrow(() => new ExtractException("ELI54113", "No configuration set")).WorkflowName;
            }
        }

        /// <inheritdoc/>
        public Optional<ICommonWebConfiguration> WebConfiguration => _apiContext.WebConfiguration;

        /// <inheritdoc/>
        public EWorkflowType WorkflowType
        {
            get
            {
                string workflowName = WebConfiguration.ValueOrThrow(() => new ExtractException("ELI54116", "No configuration set")).WorkflowName;
                return FileProcessingDB.GetWorkflowDefinition(FileProcessingDB.GetWorkflowID(workflowName)).Type;
            }
        }

        /// <inheritdoc/>
        public IRedactionWebConfiguration RedactionWebConfiguration
        {
            get
            {
                return _apiContext.RedactionWebConfiguration.ValueOrThrow(() => new ExtractException("ELI54114", "Redaction configuration not set"));
            }
        }

        /// <inheritdoc/>
        public bool HasRedactionWebConfiguration => _apiContext.RedactionWebConfiguration.HasValue;

        /// <inheritdoc/>
        public IDocumentApiWebConfiguration APIWebConfiguration
        {
            get
            {
                return _apiContext.DocumentApiWebConfiguration.ValueOrThrow(() => new ExtractException("ELI54115", "Document API configuration not set"));
            }
        }

        /// <inheritdoc/>
        public bool HasAPIWebConfiguration => _apiContext.DocumentApiWebConfiguration.HasValue;

        /// <summary>
        /// The session ID (for instances specific to a <see cref="ClaimsPrincipal"/>
        /// </summary>
        public string SessionId
        {
            get
            {
                return _sessionId;
            }
        }

        /// <summary>
        /// The ID of the session in the FAM database with which this instance has been associated
        /// </summary>
        public int FAMSessionId
        {
            get
            {
                if (string.IsNullOrEmpty(SessionId))
                {
                    return 0;
                }

                return FileProcessingDB.FAMSessionID;
            }
        }

        /// <summary>
        /// Is this FileApi instance currently being used?
        /// </summary>
        public bool InUse
        {
            get
            {
                return _inUse;
            }

            set
            {
                if (value != _inUse)
                {
                    if (!value)
                    {
                        Releasing?.Invoke(this, new EventArgs());
                    }

                    if (value)
                    {
                        UsesSinceClose++;
                    }
                    else
                    {
                        SuspendSession();
                        _instanceReleased.Set();
                    }

                    _inUse = value;
                }
            }
        }

        /// <summary>
        /// Gets the number of requests this instance was used for since the last time it was closed.
        /// </summary>
        public int UsesSinceClose
        {
            get;
            set;
        }

        /// <summary>
        /// The open state, ID, file ID and start time of a document session
        /// </summary>
        public (bool IsOpen, int Id, int FileId, DateTime StartTime) DocumentSession { get; set; }

        /// <summary>
        /// Waits until an instance is no longer in use.
        /// </summary>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        /// <returns><c>true</c> if the an instance was released; <c>false</c> if a timeout was
        /// reached before an instance was released.</returns>
        public static bool WaitForInstanceNoLongerInUse(int millisecondsTimeout)
        {
            try
            {
                return _instanceReleased.WaitOne(millisecondsTimeout);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46624");
            }
        }

        /// <summary>
        /// Assigns this instance to a specific context's session ID. Until the session is
        /// ended/aborted, it will not be available for use in other sessions.
        /// </summary>
        public void AssignSession(ApiContext apiContext)
        {
            try
            {
                _sessionId = apiContext.SessionId;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46256");
            }
        }

        /// <summary>
        /// Disassociates a session to make this instance available for other API sessions to use.
        /// </summary>
        public void SuspendSession()
        {
            DocumentSession = (false, 0, 0, new DateTime());
            _sessionId = null;
            _apiContext.FAMSessionId = 0;
            FileProcessingDB.SuspendWebSession();
        }

        /// <summary>
        /// Ends any associated session in the FAM database and makes this instance available for
        /// other API sessions to use.
        /// </summary>
        public void EndSession()
        {
            try
            {
                _sessionId = null;
                _apiContext.FAMSessionId = 0;
                DocumentSession = (false, 0, 0, new DateTime());

                try
                {
                    if (FileProcessingDB.FAMSessionID != 0)
                    {
                        FileProcessingDB.UnregisterActiveFAM();
                        FileProcessingDB.RecordFAMSessionStop();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI46259");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46260");
            }
        }

        /// <summary>
        /// Aborts a session that appears to have been abandoned. This will release any locked files
        /// and make this instance available for use in other sessions.
        /// </summary>
        /// <param name="famSessionId">The ID of the FAM session to be aborted. While this call will
        /// release any ties to any FAM session, The session specified here does not have to be
        /// associated with this instance.</param>
        public void AbortSession(int famSessionId = 0)
        {
            try
            {
                _sessionId = null;
                _apiContext.FAMSessionId = 0;

                try
                {
                    if (famSessionId != 0)
                    {
                        FileProcessingDB.AbortFAMSession(famSessionId);
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI46251", "Failed to close lost FAM session");
                }
                finally
                {
                    FileProcessingDB.CloseAllDBConnections();
                    UsesSinceClose = 0;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46253");
            }
            finally
            {
                // Unset InUse last so this can't be claimed by another thread before we are done aborting the session
                InUse = false;
            }
        }

        internal bool IsContextEquivalentTo(ApiContext other)
        {
            if (other == null)
            {
                return false;
            }

            if (WebConfiguration.HasValue != other.WebConfiguration.HasValue)
            {
                return false;
            }

            bool configsAreEquivalent = true;

            WebConfiguration.IfHasValue(thisConfig =>
            {
                if (!thisConfig.ConfigurationName.IsEquivalent(other.WebConfiguration.Value.ConfigurationName)
                    || !thisConfig.WorkflowName.IsEquivalent(other.WebConfiguration.Value.WorkflowName))
                {
                    configsAreEquivalent = false;
                }
            });

            return configsAreEquivalent &&
                FileProcessingDB.DatabaseServer.IsEquivalent(other.DatabaseServerName) &&
                FileProcessingDB.DatabaseName.IsEquivalent(other.DatabaseName);
        }
    }
}
