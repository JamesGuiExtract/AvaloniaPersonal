using System;
using Extract.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extract.ETL
{
    /// <summary>
    /// Class for DatabaseService related helper functions
    /// </summary>
    public static class DatabaseServiceHelper
    {
        /// <summary>
        /// Creates a IDatabaseService object using the settings
        /// </summary>
        /// <param name="id">ID from the record in DatabaseService Table</param>
        /// <param name="settings">Settings that were created by IDatabaseService.GetSettings method</param>
        /// <returns>Returns an instance of IDatabaseService from the settings</returns>
        public static IDatabaseService CreateServiceFromSettings(int id, string settings)
        {
            try
            {
                JObject settingsObject = JObject.Parse(settings);

                // Get the type from the settings object
                string typeString = (string)settingsObject["Type"];
                Type serviceType = JsonConvert.DeserializeObject<Type>(typeString);

                // Create instance of the saved type
                IDatabaseService service = (IDatabaseService)Activator.CreateInstance(serviceType);

                service.Load(id, settings);
                return service;

            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI45370", "Unable to create database service from settings.", ex);
                ee.AddDebugData("ID", id, false);
                ee.AddDebugData("Settings", settings, false);
                throw ee;
            }
        }
    }
}
