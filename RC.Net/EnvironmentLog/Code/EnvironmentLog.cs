using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.EnvironmentLog
{
    internal class EnvironmentLog
    {
        public string Customer { get; set; } = "";

        public DateTime CollectionTime { get; set; }

        public string Context { get; set; } = "";

        public string Entity { get; set; } = "";

        public string MeasurementType { get; set; } = string.Empty;

        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();

        static ILogger Logger { get; set; }

        public EnvironmentLog(string customer, DateTime collectionTime, string context, string entity, string measurementType, Dictionary<string, string> data)
        {
            Customer = customer;
            CollectionTime = collectionTime;
            Context = context;
            Entity = entity;
            MeasurementType = measurementType;
            if (data.Count > 0) Data = data;
        }

        static EnvironmentLog()
        {
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string configPath = Path.Combine(commonAppData, "Extract Systems\\Configuration\\NLog-EnvironmentLog.config");

            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configPath);
            Logger = LogManager.GetCurrentClassLogger();
        }

        public void Log()
        {
            if (Logger != null)
            {
                Logger.Info(this);
            }
        }
    }
}
