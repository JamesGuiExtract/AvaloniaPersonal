using Extract.ErrorHandling;
using Extract.FileActionManager.Utilities.FAMServiceManager;
using Extract.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.EnvironmentLog
{
    public class QueueDepthMeasure : ExtractMeasureBase, IDomainObject
    {
        public string DatabaseServer { get; set; }
        public string DatabaseName { get; set; }

        private FileProcessingDB fileProcessingDb { get; set; }

        public QueueDepthMeasure() { }

        /// <summary>
        /// Generates a DTO for this object
        /// </summary>
        /// <returns>QueueDepthLogV1 - DTO</returns>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            try
            {
                var dto = new QueueDepthMeasureV1()
                {
                    DatabaseServer = this.DatabaseServer,
                    DatabaseName = this.DatabaseName,
                    Customer = this.Customer,
                    Context = this.Context,
                    Entity = this.Entity,
                    MeasurementType = this.MeasurementType,
                    MeasurementInterval = this.MeasurementInterval,
                    Enabled = this.Enabled
                };
                return new DataTransferObjectWithType(dto);
            }
            catch (Exception ex)
            {
                throw ex.AsExtractException("ELI54033");
            }
        }

        /// <summary>
        /// Copies the values from DTO into this object
        /// </summary>
        /// <param name="log">QueueDepthLogV1 - DTO</param>
        public void CopyFrom(QueueDepthMeasureV1 log)
        {
            DatabaseServer = log.DatabaseServer;
            DatabaseName = log.DatabaseName;
            Customer = log.Customer;
            Context = log.Context;
            Entity = log.Entity;
            MeasurementType = log.MeasurementType;
            MeasurementInterval = log.MeasurementInterval;
            Enabled = log.Enabled;
            try
            {
                fileProcessingDb = new FileProcessingDB
                {
                    DatabaseServer = DatabaseServer,
                    DatabaseName = DatabaseName
                };
            }
            catch (Exception ex)
            {
                var extractEx = ex.AsExtractException("ELI54013");
                extractEx.AddDebugData("Database Server Name", DatabaseServer);
                extractEx.AddDebugData("Database Name", DatabaseName);
                extractEx.Log();
                return;
            }
        }

        /// <inheritdoc />
        public override List<Dictionary<string, string>> Execute()
        {
            if (fileProcessingDb != null)
            {
                try
                {
                    var logs = new List<Dictionary<string, string>>();
                    var actions = fileProcessingDb.GetActions();
                    var actionsDict = actions.ComToDictionary();
                    foreach (KeyValuePair<string, string> action in actionsDict)
                    {
                        var stats = fileProcessingDb.GetStatsAllWorkflows(action.Key, false);
                        Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                        keyValuePairs.Add("Action", action.Key);
                        keyValuePairs.Add("CompleteDocuments", stats.NumDocumentsComplete.ToString());
                        keyValuePairs.Add("FailedDocuments", stats.NumDocumentsFailed.ToString());
                        keyValuePairs.Add("PendingDocuments", stats.NumDocumentsPending.ToString());
                        keyValuePairs.Add("SkippedDocuments", stats.NumDocumentsSkipped.ToString());
                        logs.Add(keyValuePairs);
                    }
                    return logs;
                }
                catch (Exception ex)
                {
                    ex.AsExtractException("ELI54014").Log();
                }
            }
            else
            {
                new ExtractException("ELI54035", "DB connection has not been made");
            }
            return new List<Dictionary<string, string>>();
        }
    }
}
