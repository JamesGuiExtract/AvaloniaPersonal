using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public interface ISerialize
    {
        /// <summary>
        /// Will write out a table to the given writer, and connect using the provided db connection.
        /// </summary>
        /// <param name="dbConnection">The connection to the database</param>
        /// <param name="writer">The writer to use to write the file</param>
        void SerializeTable(DbConnection dbConnection, StreamWriter writer);
    }
}
