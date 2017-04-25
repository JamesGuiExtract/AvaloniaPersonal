using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentAPI.Models
{
    /// <summary>
    /// This class is used to contain the three elements of the FAM FileProcessingDB API context:
    /// 1) Database Server Name
    /// 2) database name
    /// 3) workflow name
    /// </summary>
    public class ApiContext
    {
        string _databaseServerName;
        string _databaseName;
        string _workflowName;

        /// <summary>
        /// this class maintains the essential API context data - Database server name, database name, and workflow name
        /// </summary>
        /// <param name="databaseServerName">server name</param>
        /// <param name="databaseName">database name</param>
        /// <param name="workflowName">workflow name</param>
        public ApiContext(string databaseServerName, string databaseName, string workflowName)
        {
            DatabaseServerName = databaseServerName;
            DatabaseName = databaseName;
            WorkflowName = workflowName;
        }

        /// <summary>
        /// database server name
        /// </summary>
        public string DatabaseServerName
        {
            get
            {
                Contract.Assert(!String.IsNullOrWhiteSpace(_databaseServerName), "Database server name is empty");
                return _databaseServerName;
            }
            set
            {
                Contract.Assert(!String.IsNullOrWhiteSpace(value), "Attempt to set an empty database server name");
                _databaseServerName = value;
            }
        }

        /// <summary>
        /// database name
        /// </summary>
        public string DatabaseName
        {
            get
            {
                Contract.Assert(!String.IsNullOrWhiteSpace(_databaseName), "Database name is empty");
                return _databaseName;
            }
            set
            {
                Contract.Assert(!String.IsNullOrWhiteSpace(value), "Attempt to set an empty database name");
                _databaseName = value;
            }
        }

        /// <summary>
        /// workflow name
        /// </summary>
        public string WorkflowName
        {
            get
            {
                Contract.Assert(!String.IsNullOrWhiteSpace(_workflowName), "Workflow name is empty");
                return _workflowName;
            }
            set
            {
                Contract.Assert(!String.IsNullOrWhiteSpace(value), "Attempt to set an empty workflow name");
                _workflowName = value;
            }
        }

    }
}
