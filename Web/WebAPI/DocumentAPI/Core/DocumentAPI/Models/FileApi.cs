using Extract;
using System;
using UCLID_FILEPROCESSINGLib;
using static DocumentAPI.Utils;


namespace DocumentAPI.Models
{
    /// <summary>
    /// Container for a fileProcessingDB instance, along with instance data and methods. This container will be used
    /// in a manager API that contains a collection of FileApis.
    /// </summary>
    public class FileApi
    {
        private ApiContext _apiContext;
        private Workflow _workflow;

        private FileProcessingDB _fileProcessingDB = null;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="apiContext">API context object</param>
        /// <param name="setInUse">set the InUse flag on object creation, or not</param>
        public FileApi(ApiContext apiContext, bool setInUse = false)
        {
            try
            {
                FAMDBUtils dbUtils = new FAMDBUtils();
                Type mgrType = Type.GetTypeFromProgID(dbUtils.GetFAMDBProgId());
                _fileProcessingDB = (FileProcessingDB)Activator.CreateInstance(mgrType);
                Contract.Assert(_fileProcessingDB != null, "Failed to create FileProcessingDB instance");
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception creating FileProcessingDB instance: {ex.Message}"));
                throw;
            }

            try
            {
                _apiContext = apiContext;

                _fileProcessingDB.DatabaseServer = DatabaseServer;
                _fileProcessingDB.DatabaseName = DatabaseName;
                _fileProcessingDB.ActiveWorkflow = WorkflowName;

                _workflow = MakeAssociatedWorkflow(WorkflowName);

                InUse = setInUse;

                Log.WriteLine("Created a new FileApi object, " +
                              Inv($"for workflowName: {WorkflowName}, ",
                              $"DatabaseServerName: {DatabaseServer}, ",
                              $"DatabaseName: {DatabaseName}"));
            }
            catch (Exception exp)
            {
                // Close DB connection
                try
                {
                    _fileProcessingDB.CloseAllDBConnections();
                }
                catch { }

                Log.WriteLine(Inv($"Exception setting FileProcessingDB DB context: {exp.Message}"));
                throw;
            }
        }

        /// <summary>
        /// Get the fileProcessingDB instance
        /// </summary>
        public FileProcessingDB Interface
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
            private set
            {
                _apiContext.DatabaseServerName = value;
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
            private set
            {
                _apiContext.DatabaseName = value;
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
            private set
            {
                _apiContext.WorkflowName = value;
            }
        }

        /// <summary>
        /// get the workflow associated with this FileApi
        /// </summary>
        public Workflow GetWorkflow
        {
            get
            {
                Contract.Assert(_workflow != null, "Workflow is not set (null)");
                return _workflow;
            }
        }

        /// <summary>
        /// Is this FileApi instance currently being used?
        /// </summary>
        public bool InUse { get; set; }

        Workflow MakeAssociatedWorkflow(string workflowName)
        {
            try
            {
                var mappedWorkflows = Interface.GetWorkflows();
                for (int i = 0; i < mappedWorkflows.Size; ++i)
                {
                    // key is the name and the value is the ID
                    mappedWorkflows.GetKeyValue(i, pstrKey: out string name, pstrValue: out string id);

                    if (name.IsEquivalent(workflowName))
                    {
                        var bRet = Int32.TryParse(id, result: out int Id);
                        Contract.Assert(bRet, "Failed to convert id: {0}, to an int", id);

                        var definition = Interface.GetWorkflowDefinition(Id);
                        Contract.Assert(definition != null, "Failed to get workflow definition for Id: {0}", Id);

                        var workflow = new Workflow(definition, DatabaseServer, DatabaseName);
                        return workflow;
                    }
                }

                Contract.Violated(Inv($"Workflow named: {workflowName}, not found, database server: {DatabaseServer}, database name: {DatabaseName}"));
                return null;
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
