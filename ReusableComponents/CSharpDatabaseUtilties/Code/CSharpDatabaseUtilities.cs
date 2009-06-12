using Extract;
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
    public static class ExtractDB
    {
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
                // Initialize server list
                List<string> serverList = new List<string>();

                // Load the db server combo list
                SqlDataSourceEnumerator sqldseServers = SqlDataSourceEnumerator.Instance;

                // Get the DataTable of database servers
                DataTable dtServers = sqldseServers.GetDataSources();

                // Add the servers to the return list
                foreach (DataRow dr in dtServers.Rows)
                {
                    // Need to build the server string with the server name and the instance name
                    StringBuilder strBld = new StringBuilder(dr["ServerName"].ToString());
                    if (dr["InstanceName"].ToString().Length != 0)
                    {
                        strBld.Append("\\");
                        strBld.Append(dr["InstanceName"].ToString());
                    }

                    // Add server to the list
                    serverList.Add(strBld.ToString());
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
        public static ReadOnlyCollection<string> GetDBNameList(string server)
        {
            try
            {
                SqlConnectionStringBuilder sqlConnStrBld = new SqlConnectionStringBuilder();

                // Setup SQL connections settings
                sqlConnStrBld.IntegratedSecurity = true;
                sqlConnStrBld.MultipleActiveResultSets = true;
                sqlConnStrBld.DataSource = server;

                // Initialize Database name list
                List<string> listResults = new List<string>();

                // Set the connection timeou to 10 seconds
                sqlConnStrBld.ConnectTimeout = 10;

                // Get the data table of databses
                DataTable dtDatabases = GetDBDataTable(sqlConnStrBld.ConnectionString);

                // Process the list of databases
                using (SqlConnection sqlTableConn = new SqlConnection(sqlConnStrBld.ConnectionString))
                {
                    sqlTableConn.Open();
                    foreach (DataRow dr in dtDatabases.Rows)
                    {
                        string strDB = dr["database_name"].ToString();

                        // Need to skip the system databases
                        if (strDB == "master" || strDB == "model" || strDB == "msdb" || strDB == "tempdb")
                        {
                            continue;
                        }

                        try
                        {
                            // Change to that DB
                            sqlTableConn.ChangeDatabase(strDB);
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
                        DataTable dtTables = sqlTableConn.GetSchema("Tables");

                        // Check for empty database
                        // Get the rows that have a table type of 'BASE TABLE'
                        DataRow[] dtExistingTables = dtTables.Select("table_type='BASE TABLE'");

                        // if no rows with 'BASE TABLE' type db is empty
                        if (dtExistingTables.GetLength(0) == 0)
                        {
                            // Include the empty database in the list
                            listResults.Add(dr["database_name"].ToString());
                        }

                        // Check for our database
                        // Get rows that have table name of 'DBInfo'
                        dtExistingTables = dtTables.Select("table_name='DBInfo'");

                        // if there is one the database is an Extract Database
                        if (dtExistingTables.GetLength(0) == 1)
                        {
                            // Add database to list
                            listResults.Add(dr["database_name"].ToString());
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
        
        #endregion

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
        private static DataTable GetDBDataTable(string strConnectionString)
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

        #endregion
    }
}
