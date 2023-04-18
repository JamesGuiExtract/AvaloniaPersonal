using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using NLog.Config;
using System;
using System.IO;
using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using System.Text.Json;
using ReactiveUI;
using Microsoft.Extensions.Logging;
using NLog;
using Extract.ErrorsAndAlerts.ElasticDTOs;

namespace AlertManager.Services
{
    /// <inheritdoc/>
    public class AlertActionLogger : IAlertActionLogger
    {
        /// <inheritdoc/>
        public void LogResolution(AlertsObject alert)
        {
            string commonAppData = "";
            string configPath = "";
            commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            try
            {
                configPath = Path.Combine(commonAppData, "Extract Systems\\Configuration\\NLog.config");
                NLog.LogManager.Configuration = new XmlLoggingConfiguration(configPath);
                for(int i = 0; i < alert.Actions.Count; i++)
                {
                    alert.Actions[i].ActionType = TypeOfResolutionAlerts.Resolved.ToString();

                    NLog.ILogger logger = LogManager.GetLogger(NLogTargetConstants.AlertsTarget);
                    Log(alert.Actions[i], logger); 
                }
            }
            catch(Exception e)
            {
                var ex = new ExtractException("ELI53797", e.Message);
                ex.AddDebugData("Alert Object Data", JsonSerializer.Serialize(alert));
                ex.AddDebugData("Configuration path ", configPath);
                ex.AddDebugData("Folder path", commonAppData);

                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

        }

        public void Log(AlertActionDto alert, NLog.ILogger logger)
        {
            if (logger != null)
            {
                logger.Info(alert);
            }

        }
    }
}
