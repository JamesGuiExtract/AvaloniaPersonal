using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace Extract.ErrorHandling
{
    public class ExceptionSettings
    {
        public bool UseNetLogging { get; private set; }
        public bool ConfigurationLoaded { get; private set; }

        readonly string DefaultExtractSettingsFile = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                , "Extract Systems\\Configuration\\ExceptionSettings.config");

        public ExceptionSettings()
        {
            LoadConfigurationSettings(DefaultExtractSettingsFile);
        }
        public ExceptionSettings(string configFile)
        {
            LoadConfigurationSettings(configFile);
        }

        public void LoadConfigurationSettings(string fileName)
        {
            ExeConfigurationFileMap configMap = new();
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string configPath = Path.Combine(commonAppData, "Extract Systems\\Configuration\\ExceptionSettings.config");
            if (!File.Exists(configPath))
            {

                UseNetLogging = false;
                ConfigurationLoaded = true;
                return;
            }
            configMap.ExeConfigFilename = configPath;
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            UseNetLogging = config.AppSettings.Settings["UseNetLogging"].Value == "1";

            ConfigurationLoaded = true;
        }
    }
}
