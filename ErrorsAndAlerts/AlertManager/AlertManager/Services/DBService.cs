﻿using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using System;
using System.Collections.Generic;
using UCLID_FILEPROCESSINGLib;
using Extract.ErrorHandling;

namespace AlertManager.Services
{
    public class DBService : IDBService
    {
        readonly IFileProcessingDB fileProcessingDB;

        public IFileProcessingDB GetFileProcessingDB => fileProcessingDB;

        public DBService(IFileProcessingDB fileProcessingDB)
        {
            this.fileProcessingDB = fileProcessingDB ?? throw new ArgumentNullException(nameof(fileProcessingDB));
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
        public bool SetFileStatus(int fileNumber, EActionStatus fileStatus, string databaseName,
            string databaseServer, int actionId)
        {
            try
            {
                if (fileProcessingDB == null)
                {
                    throw new Exception("Null value for file processing db");
                }

                fileProcessingDB.DatabaseName = databaseName;
                fileProcessingDB.DatabaseServer = databaseServer;

                fileProcessingDB.ResetDBConnection(true, false);

                string activeWorkflow = fileProcessingDB.GetWorkflowNameFromActionID(actionId);
                string action = fileProcessingDB.GetActionName(actionId);

                fileProcessingDB.ActiveWorkflow = activeWorkflow;

                EActionStatus actionStatusOut;

                int workFlowId = fileProcessingDB.GetWorkflowID(activeWorkflow);
                
                if(workFlowId < 0)
                {
                    throw new Exception("Issue with workflow configuration");
                }

                fileProcessingDB.SetStatusForFile(
                    nID: fileNumber,
                    strAction: action,
                    nWorkflowID: workFlowId,
                    eStatus: fileStatus,
                    vbQueueChangeIfProcessing: false,
                    vbAllowQueuedStatusOverride: true,
                    poldStatus: out actionStatusOut);

                return true;
            }
            catch (Exception e)
            {
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
        public List<FileObject> GetFileObjects(IList<int> listOfFileIds, string databaseName,
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

                fileProcessingDB.ResetDBConnection(true, false);

                string? activeWorkflow = fileProcessingDB.GetWorkflowNameFromActionID(actionId) ?? "";

                fileProcessingDB.ActiveWorkflow = activeWorkflow;

                string action = fileProcessingDB.GetActionName(actionId);

                foreach (int id in listOfFileIds)
                {
                    EActionStatus fileStatus = fileProcessingDB.GetFileStatus(id, action, true);
                    fileName = fileProcessingDB.GetFileNameFromFileID(id);
                    FileObject file = new(fileName, fileStatus, id);

                    fileObjects.Add(file);
                }
            }
            catch (Exception e)
            {
                throw e.AsExtractException("ELI53991");
            }

            //also get files to process advanced
            return fileObjects;
        }

    }

}