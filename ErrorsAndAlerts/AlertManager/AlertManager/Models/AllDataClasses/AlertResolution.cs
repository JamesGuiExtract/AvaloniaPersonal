using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using NLog;
using System;
using System.IO;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// Public class that serves to contain all data for alerts
    /// </summary>
    [Serializable]
    public class AlertResolution
    {
        //generic constructor
        public AlertResolution()
        {
            this.EventId = "";
            this.ResolutionComment = "";
            InitializeLoggerFromConfig();
        }

        //constructor that initializes all fields with the parameters
        public AlertResolution(string alertId,
            string resolutionComment, TypeOfResolutionAlerts typeOfResolution, DateTime resolutionTime)
        {
            this.EventId = alertId;
            this.ResolutionComment = resolutionComment;
            this.ResolutionTime = resolutionTime;
            this.ResolutionType = typeOfResolution;
            InitializeLoggerFromConfig();
        }

        private void InitializeLoggerFromConfig()
        {
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string configPath = Path.Combine(commonAppData, "Extract Systems\\Configuration\\NLog.config");
            if (!File.Exists(configPath))
            {
                // TODO: Add a default configuration
            }

            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configPath);
            Logger = LogManager.GetLogger(NLogTargetConstants.AlertsTarget);
        }

        //fields that contains the data
        public string EventId { get; set; } = "";
        public string ResolutionComment { get; set; } = "";
        public DateTime? ResolutionTime { get; set; } = null;
        public TypeOfResolutionAlerts ResolutionType { get; set; } = new();
        private LogLevel? LoggingLevel { get; set; } 
        private ILogger? Logger { get; set; } 

        public void Log()
        {
            LoggingLevel = LogLevel.Info;
            if(Logger != null)
            {
                Logger.Info(this);
            }
            
        }
    }
}
