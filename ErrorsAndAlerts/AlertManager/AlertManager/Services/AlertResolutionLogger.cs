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

namespace AlertManager.Services
{
    /// <inheritdoc/>
    public class AlertResolutionLogger : IAlertResolutionLogger
    {
        /// <inheritdoc/>
        public void LogResolution(AlertsObject alert)
        {
            string commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string configPath = Path.Combine(commonAppData, "Extract Systems\\Configuration\\NLog-AlertResolution.config");
            NLog.LogManager.Configuration = new XmlLoggingConfiguration(configPath);
            alert.Resolution.AlertId = alert.AlertId;
            alert.Resolution.ResolutionType = TypeOfResolutionAlerts.Resolved;
            alert.Resolution.Log();
        }
    }
}
