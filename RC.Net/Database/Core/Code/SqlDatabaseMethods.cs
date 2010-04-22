using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CSharpDatabaseUtilities
{
    /// <summary>
    /// Represents a grouping of methods for accessing SQL databases.
    /// </summary>
    public static class SqlDatabaseMethods
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(SqlDatabaseMethods).ToString();

        #endregion Constants

        #region Static Public Methods
        
        /// <summary>
        /// getSqlServerList returns a list of SQL servers found on the network.
        /// </summary>
        /// <returns>
        /// List of strings representing the names of the SQL servers on the network.
        /// </returns>
        // This method performs a computation and so should not be a property.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static ReadOnlyCollection<string> GetSqlServerList()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI29982",
                    _OBJECT_NAME);

                // Initialize server list
                List<string> serverList = new List<string>();

                // Load the db server combo list
                SqlDataSourceEnumerator sqldseServers = SqlDataSourceEnumerator.Instance;

                // Get the DataTable of database servers
                using (DataTable dtServers = sqldseServers.GetDataSources())
                {
                    foreach (DataRow dr in dtServers.Rows)
                    {
                        // Need to build the server string with the server name and the instance name
                        StringBuilder strBld = new StringBuilder(dr["ServerName"].ToString());
                        string instanceName = dr["InstanceName"].ToString();
                        if (instanceName.Length != 0)
                        {
                            strBld.Append("\\");
                            strBld.Append(instanceName);
                        }

                        // Add server to the list
                        serverList.Add(strBld.ToString());
                    }
                }

                return serverList.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21249", "Unable to obtain SQL server list.", ex);
            }
        }
        
        /// <summary>
        /// getDBNameList returns a list of Database names on the server that is represented by the connection
        /// string obtained from the sqlConnStrBld parameter.  This methed is intended to be called within a 
        /// Background worker DoWork event handler so that a cancel request can be handled. The caller of this
        /// method is responsible for setting up the result to indicate the process was canceled.
        /// </summary>
        /// <param name="server">The name of the server to get the DB name list from.</param>
        /// <returns>
        /// List of string that contains the databases that are on the server.
        /// </returns>
        public static ReadOnlyCollection<string> GetDatabaseNameList(string server)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI29983",
                    _OBJECT_NAME);

                SqlConnectionStringBuilder sqlConnStrBld = new SqlConnectionStringBuilder();

                // Setup SQL connections settings
                sqlConnStrBld.IntegratedSecurity = true;
                sqlConnStrBld.MultipleActiveResultSets = true;
                sqlConnStrBld.DataSource = server;

                // Initialize Database name list
                List<string> listResults = new List<string>();

                // Set the connection timeout to 10 seconds
                sqlConnStrBld.ConnectTimeout = 10;

                // Get the data table of databses
                using (DataTable dtDatabases = GetDataTable(sqlConnStrBld.ConnectionString))
                {
                    using (SqlConnection sqlTableConn = new SqlConnection(sqlConnStrBld.ConnectionString))
                    {
                        sqlTableConn.Open();
                        foreach (DataRow dr in dtDatabases.Rows)
                        {
                            string name = dr["database_name"].ToString();

                            // Need to skip the system databases
                            if (name == "master" || name == "model" || name == "msdb" || name == "tempdb")
                            {
                                continue;
                            }

                            try
                            {
                                // Change to that DB
                                sqlTableConn.ChangeDatabase(name);
                            }
                            catch (Exception ex)
                            {
                                // Log this exception if verbose logging is enabled
                                if (RegistryManager.VerboseLogging)
                                {
                                    ExtractException.Log("ELI23470", ex);
                                }

                                // If there is an error changing the db, the database should not
                                // be available for selection, so continue to the next database.
                                continue;
                            }

                            // Get the tables in that DB
                            using (DataTable dtTables = sqlTableConn.GetSchema("Tables"))
                            {
                                DataRow[] dtExistingTables = dtTables.Select("table_type='BASE TABLE'");

                                // if no rows with 'BASE TABLE' type db is empty
                                if (dtExistingTables.GetLength(0) == 0)
                                {
                                    // Include the empty database in the list
                                    listResults.Add(name);
                                }

                                // Check for our database
                                // Get rows that have table name of 'DBInfo'
                                dtExistingTables = dtTables.Select("table_name='DBInfo'");

                                // if there is one the database is an Extract Database
                                if (dtExistingTables.GetLength(0) == 1)
                                {
                                    // Add database to list
                                    listResults.Add(name);
                                }
                            }
                        }
                    }
                }

                return listResults.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21248", "Unable to obtain database list.", ex);
            }
        }

        #endregion Static Public Methods

        #region Private Methods

        /// <summary>
        /// getDBDataTable returns a DataTable object that contains the databases that are on the server that 
        /// is connected to using given connection string.
        /// </summary>
        /// <param name="strConnectionString">
        /// Connection string that has the server to get the list of databases from.
        /// </param>
        /// <returns>
        /// DataTable that contains the databases on the server
        /// </returns>
        static DataTable GetDataTable(string strConnectionString)
        {
            DataTable dtDatabases;
            using (SqlConnection sqlConn = new SqlConnection())
            {
                // Set connection string for the connection
                sqlConn.ConnectionString = strConnectionString;
                try
                {
                    // Open the connection
                    sqlConn.Open();
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI21577", 
                        "Unable to obtain DataTable with supplied connection string.", ex);
                    ee.AddDebugData("ConnectionString", strConnectionString, false);
                    throw ee;
                }

                // Setup the DataTable of databases
                dtDatabases = sqlConn.GetSchema("Databases");
            }

            return dtDatabases;
        }

        #endregion Private Methods
    }
}
