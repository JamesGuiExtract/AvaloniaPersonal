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
        /// <param name="original"></param>
        /// <param name="serverName"></param>
        /// <param name="databaseName"></param>
        /// <param name="extractDataSourceDir"></param>
        /// <returns></returns>
        XDocument ConvertDashboardDataSources(XDocument original, string serverName, string databaseName,
            string extractDataSourceDir);
    }
}
