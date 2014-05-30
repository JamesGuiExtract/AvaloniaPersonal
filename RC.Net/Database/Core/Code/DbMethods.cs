using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Extract.Database
{
    /// <summary>
    /// A collection of database utility methods.
    /// </summary>
    public static class DBMethods
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DBMethods).ToString();

        /// <summary>
        /// Provides access to settings in the config file.
        /// </summary>
        static ConfigSettings<Properties.Settings> _config = new ConfigSettings<Properties.Settings>();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Cache the <see cref="DbProviderFactory"/> looked up by <see cref="GetDBProvider"/>
        /// per thread so that subsequent calls don't have to go through the full lookup process.
        /// </summary>
        [ThreadStatic]
        static KeyValuePair<DbConnection, DbProviderFactory> _lastProvider;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Generates a <see cref="DbCommand"/> based on the specified query, parameters and database
        /// connection.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/> for which the command is
        /// to apply.</param>
        /// <param name="query">The <see cref="DbCommand"/>'s <see cref="DbCommand.CommandText"/>
        /// value.</param>
        /// <param name="parameters">A <see cref="Dictionary{T, T}"/> of parameter names and values
        /// that need to be parameterized for the command if specified, <see langword="null"/> if
        /// parameters are not being used. Note that if parameters are being used, the parameter
        /// names must have already been inserted into <see paramref="query"/>.</param>
        /// <returns>The generated <see cref="DbCommand"/>.</returns>
        public static DbCommand CreateDBCommand(DbConnection dbConnection, string query,
            Dictionary<string, string> parameters)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI26727", _OBJECT_NAME);

                ExtractException.Assert("ELI26731", "Null argument exception!",
                    dbConnection != null);
                ExtractException.Assert("ELI26732", "Null argument exception!",
                    !string.IsNullOrEmpty(query));

                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = query;

                // If parameters are being used, specify them.
                if (parameters != null)
                {
                    // We need a DbProviderFactory to create the parameters.
                    DbProviderFactory providerFactory = GetDBProvider(dbConnection);

                    // [DataEntry:1273]
                    // In case the parameters are not named, order will be important, so ensure the
                    // parameters are added in order of their key (interpreted as a number, if possible).
                    int intValue = 0;
                    foreach (KeyValuePair<string, string> parameter in parameters
                        .OrderBy(parameter =>
                            (parameter.Key.Length > 1 && 
                             int.TryParse(parameter.Key.Substring(1), out intValue))
                                ? (IComparable)intValue
                                : (IComparable)parameter.Key))
                    {
                        DbParameter dbParameter = providerFactory.CreateParameter();
                        dbParameter.Direction = ParameterDirection.Input;
                        dbParameter.ParameterName = parameter.Key;
                        dbParameter.Value = parameter.Value;
                        dbCommand.Parameters.Add(dbParameter);
                    }
                }

                return dbCommand;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26730", ex);
                ee.AddDebugData("Query", query, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the <see cref="DbProviderFactory"/> that corresponds with the specified
        /// <see cref="DbConnection"/>.
        /// <para><b>Note</b></para>
        /// MSDN doc claims the availability of a DbProviderFactories.GetFactory() override
        /// in .Net 4.0 that takes a DbConnection... but that doesn't seem to be the case.
        /// http://msdn.microsoft.com/en-us/library/hh323136(v=vs.100).aspx
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/> for which the provider is
        /// needed.</param>
        /// <returns>The <see cref="DbProviderFactory"/> that corresponds with the specified
        /// <see cref="DbConnection"/>.</returns>
        public static DbProviderFactory GetDBProvider(DbConnection dbConnection)
        {
            try
            {
                if (_lastProvider.Key == dbConnection)
                {
                    return _lastProvider.Value;
                }

                // Use GetProviderMatchScore to select the provider that has the closest version to
                // dbConnection.ServerVersion from the providers that correspond with the connection
                // type.
                var providerRow = DbProviderFactories
                    .GetFactoryClasses()
                    .Rows.Cast<DataRow>()
                    .Select(row => new Tuple<DataRow, int>(
                        row, GetProviderMatchScore(row, dbConnection)))
                    .Where(item => item.Item2 > 0)
                    .OrderByDescending(item => item.Item2)
                    .Select(item => item.Item1)
                    .FirstOrDefault();

                DbProviderFactory providerFactory = (providerRow == null)
                    ? DbProviderFactories.GetFactory(_config.Settings.DefaultDBProviderFactoryName)
                    : DbProviderFactories.GetFactory(providerRow);

                // Cache the provider per-thread so that subsequent calls don't have to go through
                // the full lookup process.
                _lastProvider = new KeyValuePair<DbConnection, DbProviderFactory>(
                    dbConnection, providerFactory);

                return providerFactory;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36825");
            }
        }

        /// <overloads>
        /// Executes the supplied <see paramref="query"/> on the specified
        /// <see paramref="dbConnection"/>.
        /// </overloads>
        /// <summary>
        /// Executes the supplied <see paramref="query"/> on the specified
        /// <see paramref="dbConnection"/>.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="query">The query to execute.</param>
        /// <returns>A <see cref="DataTable"/> representing the results of the query.</returns>
        public static DataTable ExecuteDBQuery(DbConnection dbConnection, string query)
        {
            try
            {
                return ExecuteDBQuery(dbConnection, query, null);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34571");
            }
        }

        /// <summary>
        /// Gets the query results as a string array.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="query">The query to execute.</param>
        /// <returns>A string array representing the results of the query where each
        /// value is a separate row and where the values are delimited by tabs.</returns>
        public static string[] GetQueryResultsAsStringArray(DbConnection dbConnection, string query)
        {
            try
            {
                using (DataTable resultsTable = ExecuteDBQuery(dbConnection, query))
                {
                    return resultsTable.ToStringArray("\t");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36989");
            }
        }

        /// <summary>
        /// Executes the supplied <see paramref="query"/> on the specified
        /// <see paramref="dbConnection"/>.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="query">The query to execute.</param>
        /// <param name="parameters">Parameters to be used in the query. They key for each parameter
        /// must begin with the appropriate symbol ("@" for T-SQL and SQL CE, ":" for Oracle) and
        /// that key should appear in the <see paramref="query"/>.</param>
        /// <returns>A <see cref="DataTable"/> representing the results of the query.</returns>
        public static DataTable ExecuteDBQuery(DbConnection dbConnection, string query,
            Dictionary<string, string> parameters)
        {
            try
            {
                using (var command = DBMethods.CreateDBCommand(dbConnection, query, parameters))
                {
                    return ExecuteDBQuery(command);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34572");
            }
        }

        /// <summary>
        /// Gets the query results as a string array.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="query">The query to execute.</param>
        /// <param name="parameters">Parameters to be used in the query. They key for each parameter
        /// must begin with the appropriate symbol ("@" for T-SQL and SQL CE, ":" for Oracle) and
        /// that key should appear in the <see paramref="query"/>.</param>
        /// <param name="columnSeparator">The string used to separate multiple column results.
        /// (Will not be included in any result with less than 2 columns)</param>
        /// <returns>A string array representing the results of the query where each
        /// value is a separate row and where the values are delimited by
        /// <see paramref="columnSeparator"/>.</returns>
        public static string[] GetQueryResultsAsStringArray(DbConnection dbConnection, string query,
            Dictionary<string, string> parameters, string columnSeparator)
        {
            try
            {
                using (DataTable resultsTable = ExecuteDBQuery(dbConnection, query, parameters))
                {
                    return resultsTable.ToStringArray(columnSeparator);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36978");
            }
        }


        /// <summary>
        /// Executes a query against the specified database connection and returns the
        /// result as a string array.
        /// </summary>
        /// <param name="dbCommand">The <see cref="DbCommand"/> defining the query to be applied.
        /// </param>
        /// <returns>A <see cref="DataTable"/> representing the results of the query.</returns>
        public static DataTable ExecuteDBQuery(DbCommand dbCommand)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI26758", _OBJECT_NAME);

                ExtractException.Assert("ELI26151", "Null argument exception!", dbCommand != null);

                using (DbDataReader sqlReader = dbCommand.ExecuteReader())
                using (DataSet dataSet = new DataSet())
                {
                    dataSet.Locale = CultureInfo.CurrentCulture;
                    // Use a DataSet to turn off enforcement of constaints primarily for backward
                    // compatibility-- the old ExecuteDBQuery method that returned a string array
                    // did not enforce constraints.
                    DataTable dataTable = new DataTable();
                    dataTable.Locale = CultureInfo.CurrentCulture;
                    dataSet.Tables.Add(dataTable);
                    dataSet.EnforceConstraints = false;

                    dataTable.Load(sqlReader);

                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee =
                    new ExtractException("ELI26150", "Database query failed.", ex);

                if (dbCommand != null)
                {
                    ee.AddDebugData("Query", dbCommand.CommandText, false);

                    try
                    {
                        foreach (DbParameter parameter in dbCommand.Parameters)
                        {
                            ee.AddDebugData("Parameter " + parameter.ParameterName,
                                parameter.Value.ToString(), false);
                        }
                    }
                    catch (Exception ex2)
                    {
                        ExtractException.Log("ELI27106", ex2);
                    }
                }

                throw ee;
            }
        }

        /// <summary>
        /// Returns the data in <see paramref="dataTable"/> as a string array.
        /// </summary>
        /// <param name="dataTable">The <see cref="DataTable"/> containing the data to return as a
        /// string array.</param>
        /// <param name="columnSeparator">The string used to separate multiple column results.
        /// (Will not be included in any result with less than 2 columns)</param>
        /// <returns></returns>
        public static string[] ToStringArray(this DataTable dataTable, string columnSeparator)
        {
            try
            {
                List<string> results = new List<string>();

                // Loop throw each row of the results.
                for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                {
                    StringBuilder result = new StringBuilder();

                    // Keep track of all column delimiters that are appended. They are only added
                    // once it is confirmed that there is more data in the row.
                    StringBuilder pendingColumnDelimiters = new StringBuilder();

                    for (int columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
                    {
                        // If not the first column result, a column separator may be needed.
                        if (columnIndex > 0)
                        {
                            pendingColumnDelimiters.Append(columnSeparator);
                        }

                        string columnValue = dataTable.Rows[rowIndex][columnIndex].ToString();

                        if (!string.IsNullOrEmpty(columnValue))
                        {
                            // If there is data to write, go ahead and commit all pending
                            // column delimiters.
                            result.Append(pendingColumnDelimiters.ToString());

                            // Reset the pending column delimiters
                            pendingColumnDelimiters = new StringBuilder();

                            result.Append(columnValue);
                        }
                    }

                    results.Add(result.ToString());
                }

                return results.ToArray();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36979");
            }
        }

        /// <summary>
        /// Adds the specified <see paramref="data"/> to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="data">A string array representing the rows of data to add to the table. The
        /// column delimiters are assumed to be tab characters.
        /// </param>
        /// <returns>A <see cref="DataTable"/> with the data from <see paramref="data"/>.</returns>
        public static DataTable ToDataTable(this string[] data)
        {
            try 
	        {	        
		        return data.ToDataTable("\t");
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI36980");
	        }
        }

        /// <summary>
        /// Adds the specified <see paramref="data"/> to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="data">A string array representing the rows of data to add to the table. The
        /// column delimiters are assumed to be tab characters.
        /// </param>
        /// <param name="columnSeparator">The string used to separate multiple columns in the
        /// <see paramref="data"/>.</param>
        /// <returns>A <see cref="DataTable"/> with the data from <see paramref="data"/>.</returns>
        public static DataTable ToDataTable(this string[] data, string columnSeparator)
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Locale = CultureInfo.CurrentCulture;
                if (data.Length == 0)
                {
                    return dataTable;
                }

                int columnCount = 
                    data.Max(row =>
                        Enumerable.Range(0, row.Length - 1)
                        .Count(index => row.IndexOf(columnSeparator, index) >= 0) + 1);
                for (int i = 0; i < columnCount; i++)
                {
                    dataTable.Columns.Add();
                }
                foreach (string row in data)
                {
                    dataTable.Rows.Add(row.Split('\t'));
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36981");
            }
        }

        #endregion Methods

        #region Private Members

        /// <summary>
        /// Determines whether the <see paramref="providerDataRow"/> appears to be the one used by
        /// the <see paramref="dbConnection"/> based on the namespace and assembly and, if so,
        /// closely the <see cref="DbConnection.ServerVersion"/> and the provider's server version
        /// match.
        /// </summary>
        /// <param name="providerDataRow"><see cref="DataRow"/> representing a
        /// <see cref="DbProviderFactory"/>.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <returns>-1 if the provider doesn't correspond to the connection; 0 if the provider
        /// corresponds, but the version numbers are complete different; 1 if they correspond and
        /// only the major version matches, 2 if the minor version matches as well, 3 if all but
        /// the revision match and 4 if the version numbers are identical.
        /// </returns>
        static int GetProviderMatchScore(DataRow providerDataRow, DbConnection dbConnection)
        {
            var connectionNameParser=
                new AssemblyQualifiedNameParser(dbConnection.GetType().AssemblyQualifiedName);
            var providerNameParser =
                new AssemblyQualifiedNameParser(providerDataRow["AssemblyQualifiedName"].ToString());

            // Check that the provider and connection are from the same namespace
            if (connectionNameParser.Namespace != providerNameParser.Namespace ||
                connectionNameParser.PublicKeyToken != providerNameParser.PublicKeyToken)
            {
                // -1 indicates that this provider isn't a match for the current connection.
                return -1;
            }

            // https://extract.atlassian.net/browse/ISSUE-12161
            // At this point it appears the provider is a match. However, at least for SQLServerCE,
            // there can be multiple matching versions installed and using the wrong version will
            // lead to errors. It doesn't appear to be the case that version numbers can be matched
            // up exactly (at least in all cases), but the matching candidates can be ranked by how
            // closely the version numbers correspond; 

            // Score a point for each component of the version that matches.
            int score = 0;
            if (providerNameParser.Version.Major == connectionNameParser.Version.Major)
            {
                score++;
                if (providerNameParser.Version.Minor == connectionNameParser.Version.Minor)
                {
                    score++;
                    if (providerNameParser.Version.Build == connectionNameParser.Version.Build)
                    {
                        score++;
                        if (providerNameParser.Version.Revision == connectionNameParser.Version.Revision)
                        {
                            score++;
                        }
                    }
                }
            }

            return score;
        }

        #endregion Private Members
    }
}
