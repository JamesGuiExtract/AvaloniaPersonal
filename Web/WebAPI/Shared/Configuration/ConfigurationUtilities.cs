using Extract;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

namespace WebAPI.Configuration
{
    internal class ConfigurationUtilities
    {
        // Get the raw json configuraitons.
        public static IList<string> GetWebConfigConfigurations(FileProcessingDB fileprocessingDb)
        {
            var configurationNamesAndSettings = fileprocessingDb.GetWebAPIConfigurations();
            Collection<string> configurations = new();

            var keys = configurationNamesAndSettings.GetKeys().ToIEnumerable<string>().ToList();
            foreach(string key in keys)
            {
                configurations.Add(configurationNamesAndSettings.GetValue(key));
            }

            return configurations;
        }

        public static string GetWebConfigurationForConfigurationName(FileProcessingDB fileprocessingDb, string configurationName)
        {
            var configurationNamesAndSettings = fileprocessingDb.GetWebAPIConfigurations();
            var configuration = configurationNamesAndSettings.GetValue(configurationName);

            if(string.IsNullOrEmpty(configuration))
            {
                throw new ExtractException("ELI53704", $"The configuration name: {configurationName} could not be found. Ensure it exists in the WebAPIConfiguraiton table.");
            }

            return configuration;
        }
    }
}
