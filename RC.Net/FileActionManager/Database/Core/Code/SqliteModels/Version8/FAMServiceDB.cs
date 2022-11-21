using LinqToDB.Data;
using System;
using System.IO;

namespace Extract.FileActionManager.Database.SqliteModels.Version8
{
    /// DataConnection mapping for the FAMService database
	public partial class FAMServiceDB : LinqToDB.Data.DataConnection
	{
        private static readonly string _databaseDefinition = GetDDL();

        /// The current schema version for the FAM service database
        public static int SchemaVersion => 8;

        /// Adds tables and indexes to an empty database
        public virtual void CreateDatabaseStructure()
        {
            try
            {
                this.SetCommand(_databaseDefinition).Execute();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51813");
            }
        }

        private static string GetDDL()
        {
            using var stream = typeof(FAMServiceDB).Assembly.GetManifestResourceStream(typeof(FAMServiceDB), "DDL.sql");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
	}
}
