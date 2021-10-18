using DevExpress.DashboardCommon.ViewerData;
using DevExpress.DataAccess.ConnectionParameters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Extract.Dashboard.Utilities
{
    public static class DashboardExtensionMethods
    {

        /// <summary>
        ///  Returns a dictionary that has all Dimension values associated with the row represented by axistValueTuple
        /// </summary>
        /// <param name="axisValueTuple"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(this AxisPointTuple axisValueTuple)
        {
            try
            {
                var dict = new Dictionary<string, object>();

                var startingAxisPoint = axisValueTuple.GetAxisPoint();

                // If no starting point return null
                if (startingAxisPoint is null)
                {
                    return null;
                }

                // The axis points for the row are all in a linked list with the top level parent countaining all the
                // rows so want to go to the first child
                var axisPoint = startingAxisPoint;
                do
                {
                    dict[axisPoint.Dimension.DataMember] = axisPoint.GetDimensionValue(axisPoint.Dimension).Value;
                    axisPoint = axisPoint.Parent;
                }
                while (axisPoint?.Parent != null);

                return dict;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47054");
            }
        }

        /// <summary>
        /// Creates a SQLConnectionStringBuilder that uses the settings in the parameters
        /// </summary>
        /// <param name="parameters">Parameters for a data connection</param>
        /// <returns>Returns a builder populated with the settings in parameters</returns>
        public static SqlConnectionStringBuilder CreateSqlConnectionBuilderFromParameters(this DataConnectionParametersBase parameters)
        {
            try
            {
                var sqlParameters = parameters as SqlServerConnectionParametersBase;
                var customParameters = parameters as CustomStringConnectionParameters;

                SqlConnectionStringBuilder builder = null;

                if (customParameters != null)
                {
                    var settings = customParameters.ConnectionString.Split(new char[] { ';' }).ToList();
                    var provider = settings.Where(s => s.StartsWith("XpoProvider", StringComparison.OrdinalIgnoreCase));
                    builder = new SqlConnectionStringBuilder(string.Join(";", settings.Except(provider).ToArray()));
                }
                else if (sqlParameters != null)
                {
                    builder = new SqlConnectionStringBuilder();
                    builder.InitialCatalog = sqlParameters.DatabaseName;
                    builder.DataSource = sqlParameters.ServerName;
                }

                return builder;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51905");
            }
        }

        /// <summary>
        /// Creates the CustomStringConnectionParameters based off of a DataConnectionParameterBase object that sets the 
        /// ApplicationIntent=ReadOnly
        /// </summary>
        /// <param name="parameters">The DataConnectionParameterBase object to get the current connection info from</param>
        /// <param name="serverName">Server to change the connection to, will be empty if not changing the server</param>
        /// <param name="databaseName">Database to change the connection to, will be empty if not changing the database</param>
        /// <returns>Returns the CustomStringConnectionParameters for ReadOnly connection to cluster, Null if the parameters 
        /// were not for SQL database </returns>
        public static CustomStringConnectionParameters CreateConnectionParametersForReadOnly(this DataConnectionParametersBase parameters,
                                                                                             string serverName,
                                                                                             string databaseName,
                                                                                             string applicationName)
        {
            try
            {
                SqlConnectionStringBuilder builder = parameters.CreateSqlConnectionBuilderFromParameters();
                if (builder is null)
                    return null;

                if (!string.IsNullOrWhiteSpace(serverName) && !string.IsNullOrWhiteSpace(databaseName))
                {
                    builder.DataSource = serverName;
                    builder.InitialCatalog = databaseName;
                }

                builder.ApplicationIntent = ApplicationIntent.ReadOnly;
                builder.ApplicationName = applicationName;
                builder.IntegratedSecurity = true;
                builder.MultiSubnetFailover = true;
                builder.Pooling = false;
                var connectionString = "XpoProvider=MSSqlServer;" + builder.ConnectionString;
                return new CustomStringConnectionParameters(connectionString);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51904");
            }
        }
    }
}
