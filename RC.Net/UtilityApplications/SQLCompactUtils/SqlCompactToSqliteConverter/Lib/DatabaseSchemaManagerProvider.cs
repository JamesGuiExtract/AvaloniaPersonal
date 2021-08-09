using ErikEJ.SqlCeScripting;
using Extract.Database;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Globalization;

namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    /// Determines the schema manager implementation for a database
    public interface IDatabaseSchemaManagerProvider
    {
        /// <summary>
        /// Determines the <see cref="IDatabaseSchemaManager"/> for a Sql Compact database file
        /// </summary>
        /// <param name="databasePath">The path to the database file</param>
        /// <returns>The schema manager for the database or null if no manager was found</returns>
        IDatabaseSchemaManager GetSqlCompactSchemaManager(string databasePath);
    }

    /// <inheritdoc/>
    public class DatabaseSchemaManagerProvider : IDatabaseSchemaManagerProvider
    {
        public IDatabaseSchemaManager GetSqlCompactSchemaManager(string databasePath)
        {
            IDatabaseSchemaManager updater = null;

            List<string> tableNames = GetSqlCompactTableNames(databasePath);

            // Code below mostly just copied from SQLCDBEditorForm.cs
            if (tableNames.Contains("Settings"))
            {
                using SqlCeConnection connection = new(SqlCompactMethods.BuildDBConnectionString(databasePath, true));
                DbProviderFactory providerFactory = DBMethods.GetDBProvider(connection);
                using DbDataAdapter adapter = providerFactory.CreateDataAdapter();
                using DataTable table = new();
                adapter.SelectCommand = DBMethods.CreateDBCommand(connection, "SELECT * FROM Settings", null);
                table.Locale = CultureInfo.CurrentCulture;

                // Fill the table with the data from the dataAdapter
                adapter.Fill(table);

                // Look for the schema manager
                DataRow[] result = table.Select("Name = '"
                    + DatabaseHelperMethods.DatabaseSchemaManagerKey + "'");
                if (result.Length == 1)
                {
                    // Build the name to the assembly containing the manager
                    string className = result[0]["Value"].ToString();
                    updater =
                        UtilityMethods.CreateTypeFromTypeName(className) as IDatabaseSchemaManager;
                    if (updater == null)
                    {
                        var ee = new ExtractException("ELI31154",
                            "Database contained an entry for schema manager, "
                        + "but it does not contain a schema updater.");
                        ee.AddDebugData("Class Name", className, false);
                        throw ee;
                    }
                }
                else
                {
                    // No schema updater defined. Check for FPSFile table
                    if (tableNames.Contains("FPSFile"))
                    {
                        updater = (IDatabaseSchemaManager)UtilityMethods.CreateTypeFromTypeName(
                            "Extract.FileActionManager.Database.FAMServiceDatabaseManager");
                    }
                }
            }
            else
            {
                // Don't check for LabDE order mapper tables because the OrderMapperDatabaseSchemaManager schema updater has not been maintained
            }

            return updater;
        }

        // Use SqlCeScripting to get all the tables from a DB
        private static List<string> GetSqlCompactTableNames(string databasePath)
        {
            using IRepository repository = new DBRepository($"Data Source='{databasePath}';");
            return repository.GetAllTableNames();
        }
    }
}
