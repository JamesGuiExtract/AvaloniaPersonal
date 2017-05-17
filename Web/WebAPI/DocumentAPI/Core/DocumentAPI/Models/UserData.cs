using Extract;
using System;
using System.Collections.Concurrent;

namespace DocumentAPI.Models
{
    /// <summary>
    /// data model for User(Controller)
    /// </summary>
    public class UserData
    {
        FileApi _fileApi;

        /// <summary>
        /// UserData CTOR
        /// </summary>
        /// <param name="fileApi">the fileApi object to use</param>
        public UserData(FileApi fileApi)
        {
            _fileApi = fileApi;
        }

        /// <summary>
        /// check for match between user and known user
        /// </summary>
        /// <param name="user"></param>
        /// <remarks>verifying a user currently requires at least one and often two COM calls:
        /// 1) login
        /// 2) optionally must verify the workflow name if it is specified on login</remarks>
        /// <returns>true if matches</returns>
        public bool MatchUser(User user)
        {
            try
            {
                var fileProcessingDB = _fileApi.Interface;

                fileProcessingDB.LoginUser(user.Username, user.Password);

                // Here when login worked - now check the Workflow name
                var workflowName = user.WorkflowName;
                if (!String.IsNullOrWhiteSpace(workflowName))
                {
                    return FindAssociatedWorkflow(workflowName);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43409");
            }
            finally
            {
                _fileApi.InUse = false;
            }
        }

        /// <summary>
        /// find a workflow that corresponds to the specified workflow name
        /// </summary>
        /// <param name="workflowName">specified workflow name</param>
        /// <returns>true if the workflow name matches an existing workflow</returns>
        bool FindAssociatedWorkflow(string workflowName)
        {
            try
            {
                return (_fileApi.Interface.GetWorkflowID(workflowName) > 0);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42180");
                ee.AddDebugData("(login) Verifying Workflow name failed", workflowName, encrypt: false);
                Log.WriteLine(ee);

                throw ee;
            }
        }
    }
}
