using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using System;
using System.Collections.Generic;
using UCLID_FILEPROCESSINGLib;
using Extract.ErrorHandling;
using System.Configuration;

namespace AlertManager.Services
{
    public class DBService : IDBService
    {
        IFileProcessingDB? fileProcessingDB = null;

        public IFileProcessingDB? GetFileProcessingDB { get => fileProcessingDB; }
        public DBService(IFileProcessingDB? fileProcessingDB)
        {
            this.fileProcessingDB = fileProcessingDB == null ? new FileProcessingDB() : fileProcessingDB;
        }
        /// <summary>
        /// Sets the file status of a file to whatever fileStatus is
        /// </summary>
        /// <param name="fileNumber">id in database of file to set</param>
        /// <param name="fileStatus">status of file to set to</param>
        /// <param name="databaseName">name of database</param>
        /// <param name="databaseServer">name of database server</param>
        /// <param name="workFlowId">id in database of the workflow</param>
        /// <param name="actionId">id in database of the action</param>
        /// <returns>true upon successful completion, throws a error upon issue</returns>
        public void SetFileStatus(int fileNumber, EActionStatus fileStatus, string databaseName,
            string databaseServer, int actionId)
        {
            try
            {
                if(fileProcessingDB == null)
                {
                    throw new Exception("Null value for file processing db");
                }

                fileProcessingDB.DatabaseName = databaseName;
                fileProcessingDB.DatabaseServer = databaseServer;

                string activeWorkflow = fileProcessingDB.GetWorkflowNameFromActionID(actionId);
                string action = fileProcessingDB.GetActionName(actionId);

                fileProcessingDB.ActiveWorkflow = activeWorkflow;

                EActionStatus actionStatusOut;

                int workFlowId = fileProcessingDB.GetWorkflowID(activeWorkflow);
                
                if(workFlowId < 0)
                {
                    throw new Exception("Issue with workflow configuration");
                }

                fileProcessingDB.SetStatusForFile(fileNumber, action, workFlowId, fileStatus, true, true, out actionStatusOut);

            }
            catch (Exception e)
            {
                //TODO global exception handler implimented in Jira https://extract.atlassian.net/browse/ISSUE-19023
                throw e.AsExtractException("ELI53990");
               
            }

        }

        /// <summary>
        /// Returns a list of file information in the form of a file object from a list of file ids
        /// </summary>
        /// <param name="listOfFileIds">list of id in database of file to retrieve</param>
        /// <param name="databaseName">>name of database</param>
        /// <param name="databaseServer">name of database server</param>
        /// <param name="workFlowId">id in database of the workflow</param>
        /// <param name="actionId">id in database of the action</param>
        /// <returns></returns>
        public List<FileObject> GetFileObjects(List<int> listOfFileIds, string databaseName,
            string databaseServer, int actionId)
        {
            string fileName;

            List<FileObject> fileObjects = new List<FileObject>();

            try
            {
                if (fileProcessingDB == null)
                {
                    throw new Exception("Null value for file processing db");
                }

                fileProcessingDB.DatabaseName = databaseName;
                fileProcessingDB.DatabaseServer = databaseServer;

                string? activeWorkflow = fileProcessingDB.GetWorkflowNameFromActionID(actionId);

                fileProcessingDB.ActiveWorkflow = activeWorkflow;

                string action = fileProcessingDB.GetActionName(actionId);

                foreach (int i in listOfFileIds)
                {
                    EActionStatus fileStatus = fileProcessingDB.GetFileStatus(i, action, true);
                    fileName = fileProcessingDB.GetFileNameFromFileID(i);
                    FileObject o = new(fileName, fileStatus, i);

                    fileObjects.Add(o);
                }
            }
            catch (Exception e)
            {
                //TODO global exception handler implimented in Jira https://extract.atlassian.net/browse/ISSUE-19023
                throw e.AsExtractException("ELI53991");
            }

            //also get files to process advanced
            return fileObjects;
        }

    }

}