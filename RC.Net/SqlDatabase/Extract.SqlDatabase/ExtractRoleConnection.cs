using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Extract.SqlDatabase
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1802:Use literals where appropriate", Justification = "Readonly strings are encrypted by dotfuscator")]
    public sealed class ExtractRoleConnection : SqlAppRoleConnection
    {
        internal override string RoleName => "ExtractRole";
        internal override string RolePassword => "Change2This3Password";

        // This enables support for DbProviderFactories.GetFactory()
        protected override DbProviderFactory DbProviderFactory => ExtractRoleFactory.Instance;

        public ExtractRoleConnection(string server, string database, bool enlist = true)
            : base(server, database, enlist)
        {
        }

        public ExtractRoleConnection(string connectionString)
            : base(connectionString)
        {
        }

        internal ExtractRoleConnection(SqlConnection connection)
            :base(connection)
        {
        }

        /// <summary>
        /// Attempts to create the DbConnection specified by the provided <paramref name="connectionString"/>
        /// as a connection to a FAM database with the Extract application role assigned.
        /// </summary>
        /// <param name="connectionString">The database connection string</param>
        /// <param name="dbConnection">The resulting connection, if successful.
        /// NOTE: If successful and the database connection string is returned, it will already be open.</param>
        /// <returns><c>true</c> if the database connection was established and Extract application role
        /// was successfully applied; <c>false</c> if not successful for any reason (connection string doesn't
        /// specify an SQL database, couldn't connect to the database, or the database is not a FAM DB.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Changing exception to bool")]
        public static bool TryGetConnection(string connectionString, ref DbConnection dbConnection)
        {
            dbConnection = null;
            try
            {
                dbConnection = new ExtractRoleConnection(connectionString);
                dbConnection.Open();
                return true;
            }
            catch
            {
                dbConnection?.Dispose();
                dbConnection = null;
                return false;
            }
        }
    }
}
