using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using NLog.Config;
using System;
using System.IO;
using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using System.Text.Json;
using ReactiveUI;

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
            commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            try
            {
                configPath = Path.Combine(commonAppData, "Extract Systems\\Configuration\\NLog.config");
                NLog.LogManager.Configuration = new XmlLoggingConfiguration(configPath);
                alert.Resolution.AlertId = alert.AlertId;
                alert.Resolution.ResolutionType = TypeOfResolutionAlerts.Resolved;
                alert.Resolution.Log();
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
    }
}
