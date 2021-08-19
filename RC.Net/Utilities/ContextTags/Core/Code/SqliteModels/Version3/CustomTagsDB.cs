using System;
using System.IO;
using System.Linq;

namespace Extract.Utilities.ContextTags.SqliteModels.Version3
{
    /// DataConnection mapping for the CustomTags database
    public partial class CustomTagsDB : LinqToDB.Data.DataConnection
    {
        private static readonly string _databaseDefinition = GetDDL();
        
        /// The current schema version for the CustomTags database
        public static int SchemaVersion => 3;

        /// Adds tables and indexes to an empty database
        public void CreateDatabaseStructure()
        {
            try
            {
                Command.CommandText = _databaseDefinition;
                Command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51814");
            }
        }

        /// <summary>
        /// Gets the context name associated with the supplied directory
        /// </summary>
        /// <remarks>
        /// In case some users are using a mapped drive, the directory will be converted to a UNC path
        /// to help ensure that all users accessing the same folder will be associated with the same context.
        /// </remarks>
        /// <returns>The name of the associated context or <c>null</c> if no context is found</returns>
        public string GetContextNameForDirectory(string directory)
        {
            if (String.IsNullOrEmpty(directory))
            {
                return null;
            }

            directory = CustomTagsDBMethods.GetCanonicalFPSFileDir(directory);

            return Contexts
                .Where(context => context.FPSFileDir == directory)
                .Select(context => context.Name)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the context name associated with the directory that the database is located
        /// </summary>
        /// <remarks>
        /// In case some users are using a mapped drive, the directory will be converted to a UNC path
        /// to help ensure that all users accessing the same folder will be associated with the same context.
        /// </remarks>
        /// <returns>The name of the associated context or <c>null</c> if none were found</returns>
        public string GetContextNameForDatabaseDirectory()
        {
            return GetContextNameForDirectory(CustomTagsDBMethods.GetDatabaseDirectory(this));
        }

        /// Whether this instance has been disposed
        public bool IsDisposed => Disposed;

        private static string GetDDL()
        {
            using var stream = typeof(CustomTagsDB).Assembly.GetManifestResourceStream(typeof(CustomTagsDB), "DDL.sql");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
