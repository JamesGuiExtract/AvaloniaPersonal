using System;

namespace TokenGen
{
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
                return _databaseServerName;
            }
            set
            {
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
                return _databaseName;
            }
            set
            {
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
                return _workflowName;
            }
            set
            {
                _workflowName = value;
            }
        }

    }
}
