using System;
using System.Linq;
using System.Xml.Linq;

namespace Extract.Interfaces
{
    /// <summary>
    /// Interface to do conversion of datasources on a dashboard
    /// </summary>
    public interface IDashboardDataConverter
    {
        /// <summary>
        /// Converts all sql datasources to Extracted datasources
        /// </summary>
        /// <param name="dashboardName">Name of dashboard</param>
        /// <param name="original">Original XDocument</param>
        /// <param name="serverName">Server name the dashbaord is in</param>
        /// <param name="databaseName">Database dashboard is in</param>
        /// <param name="extractDataSourceDir">Directory to store the Extracted data files</param>
        /// <returns>Modified XDocument that uses the Extracted datasource or <c>null</c> if the the dashboard uses parameters</returns>
        XDocument ConvertDashboardDataSources(string dashboardName, XDocument original, string serverName, string databaseName,
            string extractDataSourceDir);
    }
}
