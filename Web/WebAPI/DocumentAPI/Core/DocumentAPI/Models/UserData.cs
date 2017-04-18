﻿using Extract;
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
                if (!String.IsNullOrEmpty(workflowName))
                {
                    return FindAssociatedWorkflow(workflowName);
                }

                return true;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42178");
                Log.WriteLine(ee);

                return false;
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
                var mappedWorkflows = _fileApi.Interface.GetWorkflows();
                for (int i = 0; i < mappedWorkflows.Size; ++i)
                {
                    // key is the name and the value is the ID
                    mappedWorkflows.GetKeyValue(i, pstrKey: out string name, pstrValue: out string id);
                    if (name.IsEquivalent(workflowName))
                    {
                        return true;
                    }
                }

                return false;
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
