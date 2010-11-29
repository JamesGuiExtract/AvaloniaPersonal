using Extract.Licensing;
using System;
using System.Data.Linq;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Extract.Database
{
    /// <summary>
    /// A collection of helper methods for working with Sql Compact databases.
    /// </summary>
    public static class SqlCompactMethods
    {
        /// <summary>
        /// Object name used in license validation calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(SqlCompactMethods).ToString();

        /// <summary>
        /// Builds the DB connection string.
        /// </summary>
        /// <param name="compactDBFile">The compact DB file.</param>
        /// <returns>The connection string for connecting to the DB.</returns>
        public static string BuildDBConnectionString(string compactDBFile)
        {
            try
            {
                var sb = new StringBuilder("Data Source='");
                sb.Append(compactDBFile);
                sb.Append("';");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31093", ex);
            }
        }

        /// <summary>
        /// Gets the db schema manager.
        /// </summary>
        /// <param name="compactDBFile">The compact database file.</param>
        /// <returns></returns>
        public static string GetDBSchemaManagerName(string compactDBFile)
        {
            return GetDBSchemaManagerName(compactDBFile, "Settings");
        }

        /// <summary>
        /// Gets the db schema manager.
        /// </summary>
        /// <param name="compactDBFile">The compact database file.</param>
        /// <param name="settingsTable">The settings table.</param>
        /// <returns>The name of the schema manager to use.</returns>
        public static string GetDBSchemaManagerName(string compactDBFile, string settingsTable)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(compactDBFile))
                {
                    throw new ArgumentException("SDF file name cannot be null or empty.",
                        "compactDBFile");
                }
                if (string.IsNullOrWhiteSpace(settingsTable))
                {
                    throw new ArgumentException("Settings table name cannot be null or empty.",
                        "settingsTable");
                }

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI31099", _OBJECT_NAME);

                compactDBFile = Path.GetFullPath(compactDBFile);
                if (!File.Exists(compactDBFile))
                {
                    throw new FileNotFoundException("SDF file was not found.", compactDBFile);
                }

                using (var connection = new SqlCeConnection(compactDBFile))
                {
                    using (DataContext context = new DataContext(connection))
                    {
                        string query = string.Format(CultureInfo.InvariantCulture,
                            "SELECT [Value] FROM [{0}] WHERE [Name] = '{1}'",
                            settingsTable, DatabaseHelperMethods.DatabaseSchemaManagerKey);

                        var managerName = context.ExecuteQuery<string>(query).FirstOrDefault();
                        if (string.IsNullOrWhiteSpace(managerName))
                        {
                            var ee = new ExtractException("ELI31086",
                                "No database schema manager setting found.");
                            ee.AddDebugData("Settings Table", settingsTable, false);
                            ee.AddDebugData("Manager Key Searched",
                                DatabaseHelperMethods.DatabaseSchemaManagerKey, false);
                            throw ee;
                        }

                        return managerName;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31087", ex);
            }
        }

        /// <summary>
        /// Gets the database schema updater object from the schema manager.
        /// If the manager does not implement <see cref="IDatabaseSchemaUpdater"/>
        /// then a <see langword="null"/> value will be returned.
        /// <para><b>Note:</b></para>
        /// The schema manager class must be contained in this assembly.
        /// </summary>
        /// <param name="schemaManager">The schema manager class to get.</param>
        /// <returns>The schema updater for the specified manager class.</returns>
        [CLSCompliant(false)]
        public static IDatabaseSchemaUpdater GetUpdaterForSchemaManager(string schemaManager)
        {
            try
            {
                var assembly = Assembly.GetAssembly(typeof(SqlCompactMethods));
                return GetUpdaterForSchemaManager(assembly, schemaManager);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31088", ex);
            }
        }

        /// <summary>
        /// Gets the database schema updater object from the schema manager.
        /// If the manager does not implement <see cref="IDatabaseSchemaUpdater"/>
        /// then a <see langword="null"/> value will be returned.
        /// </summary>
        /// <param name="assembly">The assembly to load the manager from.</param>
        /// <param name="schemaManager">The schema manager class to get.</param>
        /// <returns>
        /// The schema updater for the specified manager class.
        /// </returns>
        [CLSCompliant(false)]
        public static IDatabaseSchemaUpdater GetUpdaterForSchemaManager(Assembly assembly,
            string schemaManager)
        {
            try
            {
                if (assembly == null)
                {
                    throw new ArgumentNullException("assembly");
                }
                if (string.IsNullOrWhiteSpace(schemaManager))
                {
                    throw new ArgumentException("Schema manager cannot be null or empty.",
                        "schemaManager");
                }

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI31100", _OBJECT_NAME);

                object temp = assembly.CreateInstance(schemaManager, true);
                if (temp == null)
                {
                    var ee = new ExtractException("ELI31089",
                        "Could not load schema manager from assembly.");
                    ee.AddDebugData("Assembly Name", assembly.FullName, true);
                    ee.AddDebugData("Schema Manager Name", schemaManager, true);
                    throw ee;
                }
                var schemaUpdater = temp as IDatabaseSchemaUpdater;

                return schemaUpdater;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31090", ex);
            }
        }
    }
}
