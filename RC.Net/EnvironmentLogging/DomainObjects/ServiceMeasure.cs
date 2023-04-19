using Extract.ErrorHandling;
using Extract.FileActionManager.Utilities.FAMServiceManager;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ExtractEnvironmentService
{
    public sealed class ServiceMeasure : ExtractMeasureBase, IExtractMeasure, IDomainObject
    {

        public ServiceMeasure() { }

        /// <summary>
        /// Generates a DTO for this object
        /// </summary>
        /// <returns>ServiceLogV1 - DTO</returns>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            try
            {
                var dto = new ServiceMeasureV1()
                {
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
                throw ex.AsExtract("ELI54034");
            }
        }

        /// <summary>
        /// Copies the values from DTO into this object
        /// </summary>
        /// <param name="log">ServiceLogV1 - DTO</param>
        public void CopyFrom(ServiceMeasureV1 log)
        {
            _ = log ?? throw new ArgumentNullException(nameof(log));

            Customer = log.Customer;
            Context = log.Context;
            Entity = log.Entity;
            MeasurementType = log.MeasurementType;
            MeasurementInterval = log.MeasurementInterval;
            PinThread = log.PinThread;
            Enabled = log.Enabled;
        }

        /// <inheritdoc />
        public ReadOnlyCollection<Dictionary<string, string>> Execute()
        {
            try
            {
                var logs = new List<Dictionary<string, string>>();
                var services = FAMService.FAMServiceModule.getInstalled();
                foreach (FAMService.FAMService service in services)
                {
                    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                    keyValuePairs.Add("Name", service.Name);
                    keyValuePairs.Add("StartMode", service.StartMode.ToString());
                    keyValuePairs.Add("State", service.State.ToString());
                    if (service.PID != null) keyValuePairs.Add("PID", service.PID.ToString());
                    logs.Add(keyValuePairs);
                }
                return logs.AsReadOnly();
            } 
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI54177");
            }
        }
    }
}
