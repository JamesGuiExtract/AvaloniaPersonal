using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using System.Text.Json;

namespace AlertManager.Services
{
    /// <inheritdoc/>
    public class AlertResolutionLogger : IAlertResolutionLogger
    {
        /// <inheritdoc/>
        public void LogResolution(AlertsObject alert)
        {
            string commonAppData = "";
            string configPath = "";
            try
            {
                commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                configPath = Path.Combine(commonAppData, "Extract Systems\\Configuration\\NLog-AlertResolution.config");
                NLog.LogManager.Configuration = new XmlLoggingConfiguration(configPath);
                alert.Resolution.AlertId = alert.AlertId;
                alert.Resolution.ResolutionType = TypeOfResolutionAlerts.Resolved;
                alert.Resolution.Log();
            }
            catch(Exception e)
            {
                var ex = new ExtractException("ELI53797", "Failed To log Alert Resolution");
                ex.AddDebugData("Alert Object Data", JsonSerializer.Serialize(alert));
                ex.AddDebugData("Configuration path ", configPath);
                ex.AddDebugData("Folder path", commonAppData);
                ex.AddDebugData("error stack trace", e.StackTrace);
                throw ex.AsExtractException("ELI53798");
            }

        }
    }
}
