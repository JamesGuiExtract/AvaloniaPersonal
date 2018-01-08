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
                // Create instance of the saved type
                IDatabaseService service = (IDatabaseService)JsonConvert.DeserializeObject(settings,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });

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
