using Extract.ErrorHandling;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using UCLID_FILEPROCESSINGLib;

namespace ExtractEnvironmentService
{
    public sealed class QueueDepthMeasure : ExtractMeasureBase, IExtractMeasure, IDomainObject
    {
        public string DatabaseServer { get; set; }
        public string DatabaseName { get; set; }

        private FileProcessingDB FileProcessingDB { get; set; }

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
                    PinThread = this.PinThread,
                    Enabled = this.Enabled
                };
                return new DataTransferObjectWithType(dto);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI54033");
            }
        }

        /// <summary>
        /// Copies the values from DTO into this object
        /// </summary>
        /// <param name="log">QueueDepthLogV1 - DTO</param>
        public void CopyFrom(QueueDepthMeasureV1 log)
        {
            _ = log ?? throw new ArgumentNullException(nameof(log));

            DatabaseServer = log.DatabaseServer;
            DatabaseName = log.DatabaseName;
            Customer = log.Customer;
            Context = log.Context;
            Entity = log.Entity;
            MeasurementType = log.MeasurementType;
            MeasurementInterval = log.MeasurementInterval;
            PinThread = log.PinThread;
            Enabled = log.Enabled;
            try
            {
                FileProcessingDB = new FileProcessingDB
                {
                    DatabaseServer = DatabaseServer,
                    DatabaseName = DatabaseName
                };
            }
            catch (Exception ex)
            {
                var extractEx = ex.AsExtract("ELI54013");
                extractEx.AddDebugData("Database Server Name", DatabaseServer);
                extractEx.AddDebugData("Database Name", DatabaseName);
                extractEx.Log();
                return;
            }
        }

        /// <inheritdoc />
        public ReadOnlyCollection<Dictionary<string, string>> Execute()
        {
            try
            {
                ExceptionExtensionMethods.Assert("ELI54185",
                    "Database connection not configured",
                    FileProcessingDB != null);

                var logs = new List<Dictionary<string, string>>();
                var actions = FileProcessingDB.GetActions();
                var actionsDict = actions.ComToDictionary();
                foreach (KeyValuePair<string, string> action in actionsDict)
                {
                    var stats = FileProcessingDB.GetStatsAllWorkflows(action.Key, false);
                    Dictionary<string, string> keyValuePairs = new()
                    {
                        { "Action", action.Key },
                        { "CompleteDocuments", stats.NumDocumentsComplete.ToString(CultureInfo.InvariantCulture) },
                        { "FailedDocuments", stats.NumDocumentsFailed.ToString(CultureInfo.InvariantCulture) },
                        { "PendingDocuments", stats.NumDocumentsPending.ToString(CultureInfo.InvariantCulture) },
                        { "SkippedDocuments", stats.NumDocumentsSkipped.ToString(CultureInfo.InvariantCulture) }
                    };
                    logs.Add(keyValuePairs);
                }
                return logs.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI54014");
            }
        }
    }
}
