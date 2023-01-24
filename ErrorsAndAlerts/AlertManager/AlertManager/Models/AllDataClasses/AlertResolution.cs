using AlertManager.Models.AllEnums;
using NLog;
using NLog.Attributes;
using System;
using System.IO;
using System.Windows.Input;

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
            this.AlertId = "";
            this.ResolutionComment = "";
            InitializeLoggerFromConfig();
        }

        //constructor that initializes all fields with the parameters
        public AlertResolution(string alertId,
            string resolutionType, TypeOfResolutionAlerts typeOfResolution, DateTime resolutionTime)
        {
            this.AlertId = alertId;
            this.ResolutionComment = resolutionType;
            this.ResolutionTime = resolutionTime;
            this.ResolutionType = typeOfResolution;
            InitializeLoggerFromConfig();
        }

        private void InitializeLoggerFromConfig()
        {
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string configPath = Path.Combine(commonAppData, "Extract Systems\\Configuration\\NLog-AlertResolution.config");
            if (!File.Exists(configPath))
            {
                // TODO: Add a default configuration
            }

            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configPath);
            Logger = LogManager.GetCurrentClassLogger();
        }

        //fields that contains the data
        public string AlertId { get; set; } = "";
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
