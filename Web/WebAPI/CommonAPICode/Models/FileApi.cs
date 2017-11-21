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
        private ClaimsPrincipal _sessionOwner;
        private string _sessionId = "";

        private FileProcessingDB _fileProcessingDB = null;

        /// <summary>
        /// Initializes a new <see cref="FileApi"/> instance.
        /// </summary>
        /// <param name="apiContext">The <see cref="ApiContext"/> defining the database environment
        /// for this instance.</param>
        /// <param name="setInUse"><c>true</c> to set the InUse flag on object creation;
        /// otherwise, <c>false</c>.</param>
        /// <param name="sessionOwner">The <see cref="ClaimsPrincipal"/> this instance should be
        /// specific to or <c>null</c> if this instance should not be specific to a particular user.</param>
        public FileApi(ApiContext apiContext, bool setInUse = false, ClaimsPrincipal sessionOwner = null)
        {
            try
            {
                FAMDBUtils dbUtils = new FAMDBUtils();
                Type mgrType = Type.GetTypeFromProgID(dbUtils.GetFAMDBProgId());
                _fileProcessingDB = (FileProcessingDB)Activator.CreateInstance(mgrType);
                Contract.Assert(_fileProcessingDB != null, "Failed to create FileProcessingDB instance");

                _sessionOwner = sessionOwner;
                if (sessionOwner != null)
                {
                    _sessionId = sessionOwner.GetClaim("jti");
                }
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
        /// Get the fileProcessingDB instance
        /// </summary>
        public FileProcessingDB FileProcessingDB
        {
            get
            {
                return _fileProcessingDB;
            }
        }

        /// <summary>
        /// get/set the database server name configured for this instance
        /// </summary>
        public string DatabaseServer
        {
            get
            {
                return _apiContext.DatabaseServerName;
            }
        }

        /// <summary>
        /// Get/set the database name configured for this instance
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
                Contract.Assert(_workflow != null, "Workflow is not set (null)");
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

                Contract.Assert(Id > 0, "Invalid workflow name: {0}", workflowName);

                var definition = FileProcessingDB.GetWorkflowDefinition(Id);
                Contract.Assert(definition != null, "Failed to get workflow definition for Id: {0}", Id);

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
