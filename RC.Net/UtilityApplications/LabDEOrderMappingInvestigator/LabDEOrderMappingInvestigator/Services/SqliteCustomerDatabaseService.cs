using LabDEOrderMappingInvestigator.SqliteModels;
using LinqToDB;
using System;
using System.IO;
using System.Linq;

namespace LabDEOrderMappingInvestigator.Services
{
    /// <summary>
    /// Service with methods to read information from a customer's OMDB file
    /// </summary>
    public interface ICustomerDatabaseService
    {
        /// <summary>
        /// Get the FKBVersion setting from the database, if it exists and is not empty, else returns null
        /// </summary>
        string? GetFKBVersion(string databasePath);

        /// <summary>
        /// Add a new record to map a customer test to an Extract test
        /// </summary>
        void AddESComponentMapEntry(string databasePath, string customerTestCode, string extractTestCode);

        /// <summary>
        /// Remove the record that maps a customer test to an Extract test
        /// </summary>
        void RemoveESComponentMapEntry(string databasePath, string customerTestCode, string extractTestCode);
    }

    /// <summary>
    /// Service with methods to read information from a customer's sqlite OMDB file
    /// </summary>
    public class SqliteCustomerDatabaseService : ICustomerDatabaseService
    {
        public string? GetFKBVersion(string databasePath)
        {
            _ = databasePath ?? throw new ArgumentNullException(nameof(databasePath));

            try
            {
                if (!File.Exists(databasePath))
                {
                    return null;
                }

                using CustomerOrderMappingDB db = new(SqliteUtils.BuildConnectionOptions(databasePath));
                string? fkbInDatabase = db.Settings.Where(x => x.Name == "FKBVersion").FirstOrDefault()?.Value;

                return string.IsNullOrEmpty(fkbInDatabase) ? null : fkbInDatabase;
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public void AddESComponentMapEntry(string databasePath, string customerTestCode, string extractTestCode)
        {
            using CustomerOrderMappingDB db = new(SqliteUtils.BuildConnectionOptions(databasePath));

            db.ComponentToESComponentMaps.InsertOrUpdate(
                insertSetter: () =>
                    new ComponentToESComponentMap
                    {
                        ComponentCode = customerTestCode,
                        ESComponentCode = extractTestCode
                    },
                onDuplicateKeyUpdateSetter: currentRecord => currentRecord);
        }

        /// <inheritdoc/>
        public void RemoveESComponentMapEntry(string databasePath, string customerTestCode, string extractTestCode)
        {
            using CustomerOrderMappingDB db = new(SqliteUtils.BuildConnectionOptions(databasePath));

            db.ComponentToESComponentMaps
                .Where(x => x.ComponentCode == customerTestCode && x.ESComponentCode == extractTestCode)
                .Delete();
        }
    }
}
