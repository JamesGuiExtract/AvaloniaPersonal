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

        // TODO: Create CreateDBCommand overrides for all database types to be supported.

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
                    // We need a DbProviderFactory to create the parameters. MSDN doc claims the
                    // availability of a DbProviderFactories.GetFactory() override in .Net 4.0 that
                    // takes a DbConnection... but that doesn't seem to be the case. Instead,
                    // compare all available factories to find one that is defined in the same
                    // namespace/assembly as the connection.
                    DataRow providerRow = DbProviderFactories
                        .GetFactoryClasses()
                        .Rows.Cast<DataRow>()
                        .SingleOrDefault(row => IsProviderDataRowForConnection(row, dbConnection));

                    // In case we are not able to identify the correct factory, allow a default that
                    // can be specified in a config file if necessary.
                    DbProviderFactory providerFactory = (providerRow == null)
                        ? DbProviderFactories.GetFactory(_config.Settings.DefaultDBProviderFactoryName)
                        : DbProviderFactories.GetFactory(providerRow);

                    // In case the parameters are not named, order will be important, so ensure the
                    // parameters are added in order of their key.
                    foreach (KeyValuePair<string, string> parameter in parameters
                        .OrderBy(parameter =>
                            Int32.Parse(parameter.Key.Substring(1), CultureInfo.InvariantCulture)))
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
        /// Determines whether the <see paramref="providerDataRow"/> appears to be the correct one to
        /// use with the <see paramref="dbConnection"/> based on the namespace and assembly.
        /// </summary>
        /// <param name="providerDataRow"><see cref="DataRow"/> representing a
        /// <see cref="DbProviderFactory"/>.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <returns><see langword="true"/> if the <see paramref="providerDataRow"/> appears to be the
        /// correct one to use with the <see paramref="dbConnection"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        static bool IsProviderDataRowForConnection(DataRow providerDataRow, DbConnection dbConnection)
        {
            // Break out the components of the connection's AssemblyQualifiedName.
            string[] connectionAssemblyQualifiedNameParts =
                dbConnection.GetType().AssemblyQualifiedName.Split(',');

            // Break out the components of the providerDataRow's AssemblyQualifiedName.
            string[] assemblyQualifiedNameParts =
                providerDataRow["AssemblyQualifiedName"].ToString().Split(',');

            // Check that the provider namespace matches the connection namespace and that the
            // PublicKeyTokens match. I was initially checking the version number as well, but I
            // found that for SQL CE, version 3.5.0.0 was the only one in the provider list even
            // though 3.5.1.0 is GAC'd as well (and the connection was version 3.5.1.0).
            return (assemblyQualifiedNameParts[0].Contains(dbConnection.GetType().Namespace + ".") &&
                assemblyQualifiedNameParts[4] == connectionAssemblyQualifiedNameParts[4]);
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
        /// <returns>A string array representing the results of the query where each
        /// value is a separate row and where the values are delimited by tabs.</returns>
        public static string[] ExecuteDBQuery(DbConnection dbConnection, string query)
        {
            try
            {
                return ExecuteDBQuery(dbConnection, query, null, "\t");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34571");
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
        /// <param name="columnSeparator">The string that should delimit each column's value.</param>
        /// <returns>A string array representing the results of the query where each
        /// value is a separate row and where the values are delimited by
        /// <see paramref="columnSeparator"/></returns>
        public static string[] ExecuteDBQuery(DbConnection dbConnection, string query,
            Dictionary<string, string> parameters, string columnSeparator)
        {
            try
            {
                using (var command = DBMethods.CreateDBCommand(dbConnection, query, parameters))
                {
                    return ExecuteDBQuery(command, columnSeparator);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34572");
            }
        }

        /// <summary>
        /// Executes a query against the specified database connection and returns the
        /// result as a string array.
        /// </summary>
        /// <param name="dbCommand">The <see cref="DbCommand"/> defining the query to be applied.
        /// </param>
        /// <param name="columnSeparator">The string used to separate multiple column results.
        /// (Will not be included in any result with less than 2 columns)</param>
        /// <returns>An array of <see cref="string"/>s, each representing a result row from the
        /// query.</returns>
        public static string[] ExecuteDBQuery(DbCommand dbCommand, string columnSeparator)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI26758", _OBJECT_NAME);

                ExtractException.Assert("ELI26151", "Null argument exception!", dbCommand != null);

                using (DbDataReader sqlReader = dbCommand.ExecuteReader())
                {
                    List<string> results = new List<string>();

                    // Loop throw each row of the results.
                    while (sqlReader.Read())
                    {
                        StringBuilder result = new StringBuilder();

                        // Keep track of all column delimiters that are appended. They are
                        // only added once it is confirmed that there is more data in the
                        // row.
                        StringBuilder pendingColumnDelimiters = new StringBuilder();

                        for (int i = 0; i < sqlReader.FieldCount; i++)
                        {
                            // If not the first column result, a column separator may be needed.
                            if (i > 0)
                            {
                                pendingColumnDelimiters.Append(columnSeparator);
                            }

                            // Append a result only if there is a value to append.
                            if (!sqlReader.IsDBNull(i))
                            {
                                string columnValue = sqlReader.GetValue(i).ToString();

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
                        }

                        results.Add(result.ToString());
                    }

                    return results.ToArray();
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
    }
}
