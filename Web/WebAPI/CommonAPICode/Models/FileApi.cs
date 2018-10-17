using Extract;
using System;
using System.Security.Claims;
using UCLID_FILEPROCESSINGLib;
using static WebAPI.Utils;


namespace WebAPI.Models
{
    /// <summary>
    /// Container for a fileProcessingDB instance, along with instance data and methods. This container will be used
    /// in a manager API that contains a collection of FileApis.
    /// </summary>
    public class FileApi
    {
        private ApiContext _apiContext;
        private Workflow _workflow;
        private string _sessionId = "";

        private FileProcessingDB _fileProcessingDB = null;

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
                _fileProcessingDB.ActiveWorkflow = apiContext.WorkflowName;
                _fileProcessingDB.NumberOfConnectionRetries = apiContext.NumberOfConnectionRetries;
                _fileProcessingDB.ConnectionRetryTimeout = apiContext.ConnectionRetryTimeout;

                _workflow = MakeAssociatedWorkflow(WorkflowName);

                InUse = setInUse;

                Log.WriteLine("Created a new FileApi object, " +
                              Inv($"for workflowName: {WorkflowName}, ",
                              $"DatabaseServerName: {DatabaseServer}, ",
                              $"DatabaseName: {DatabaseName}"), "ELI43250");
            }
            catch (Exception exp)
            {
                // Close DB connection
                try
                {
                    _fileProcessingDB.CloseAllDBConnections();
                }
                catch { }

                var ee = exp.AsExtract("ELI43264");
                ee.AddDebugData("Message", "Exception reported from MakeAssociatedWorkflow", encrypt: false);
                ee.AddDebugData("Workflow name", WorkflowName, encrypt: false);
                ee.AddDebugData("Database server name", DatabaseServer, encrypt: false);
                ee.AddDebugData("Database name", DatabaseName, encrypt: false);

                throw ee;
            }
        }

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
        /// Gets/sets the database server name configured for this instance
        /// </summary>
        public string DatabaseServer
        {
            get
            {
                return _apiContext.DatabaseServerName;
            }
        }

        /// <summary>
        /// Gets/sets the database name configured for this instance
        /// </summary>
        public string DatabaseName
        {
            get
            {
                return _apiContext.DatabaseName;
            }
        }

        /// <summary>
        /// get/set the workflow name configured for this instance
        /// </summary>
        public string WorkflowName
        {
            get
            {
                return _apiContext.WorkflowName;
            }
        }

        /// <summary>
        /// get the workflow associated with this FileApi
        /// </summary>
        public Workflow Workflow
        {
            get
            {
                return _workflow;
            }
        }

        /// <summary>
        /// Gets session ID (for instances specific to a <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public string SessionId
        {
            get
            {
                return _sessionId;
            }
        }

        /// <summary>
        /// Gets the ID of the session in the FAM database with which this instance has been associated.
        /// </summary>
        public int FAMSessionId
        {
            get
            {
                return FileProcessingDB.FAMSessionID;
            }
        }

        /// <summary>
        /// Is this FileApi instance currently being used?
        /// </summary>
        public bool InUse { get; set; }

        /// <summary>
        /// Gets or sets the open state, ID, file ID and start time of a document session.
        /// </summary>
        public (bool IsOpen, int Id, int FileId, DateTime StartTime) DocumentSession { get; set; }

        /// <summary>
        /// Gets a value indicating whether the session is expired.
        /// </summary>
        public bool Expired
        {
            get;
            set;
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
        /// Ends any associated session in the FAM database and makes this instance available for
        /// other API sessions to use.
        /// </summary>
        public void EndSession()
        {
            try
            {
                _sessionId = null;
                _apiContext.FAMSessionId = 0;

                try
                {
                    if (FAMSessionId != 0)
                    {
                        FileProcessingDB.UnregisterActiveFAM();
                        FileProcessingDB.RecordFAMSessionStop();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI46259");
                }
                finally
                {
                    FileProcessingDB.CloseAllDBConnections();
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
                InUse = false;

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
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46253");
            }
        }

        Workflow MakeAssociatedWorkflow(string workflowName)
        {
            try
            {
                int Id = -1;
                try
                {
                    Id = FileProcessingDB.GetWorkflowID(workflowName);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI43263");
                }

                HTTPError.Assert("ELI46387", Id > 0, "Invalid workflow name", ("Workflow", workflowName, false));

                var definition = FileProcessingDB.GetWorkflowDefinition(Id);
                HTTPError.Assert("ELI46388", definition != null,
                    "Failed to get workflow definition", ("WorkflowID", Id, false));

                var workflow = new Workflow(definition, DatabaseServer, DatabaseName);
                return workflow;
            }
            catch (ExtractException)
            {
                // Don't log the returned exception here, as it is either already logged, or in the case of an
                // ADO exception from the FileProcessingDB, it is not very informative.
                throw;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42159");
                ee.AddDebugData("Workflow name", workflowName, encrypt: false);
                ee.AddDebugData("Database server name", DatabaseServer, encrypt: false);
                ee.AddDebugData("Database name", DatabaseName, encrypt: false);
                Log.WriteLine(ee);

                throw ee;
            }
        }
    }
}
