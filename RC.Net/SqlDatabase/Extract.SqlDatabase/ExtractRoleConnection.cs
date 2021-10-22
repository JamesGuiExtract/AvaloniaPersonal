using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Extract.SqlDatabase
{
    public sealed class ExtractRoleConnection : SqlAppRoleConnection
    {
        private static readonly string password = "Change2This3Password";
        private static readonly string role = "ExtractRole";


        static ExtractRoleConnection()
        {
            ExtractRoleFactory.RegisterProviderForInstance();
        }

        public ExtractRoleConnection(string server, string database, bool enlist = true)
            : base(SqlUtil.NewSqlDBConnection(server, database, enlist))
        {
        }

        public ExtractRoleConnection(string connectionString)
            : base(SqlUtil.NewSqlDBConnection(connectionString))
        {
        }

        internal ExtractRoleConnection(SqlConnection connection)
            :base(connection)
        {
        }

        protected override void AssignRole()
        {
            try
            {
                SetApplicationRole(role, password);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51836");
            }
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
