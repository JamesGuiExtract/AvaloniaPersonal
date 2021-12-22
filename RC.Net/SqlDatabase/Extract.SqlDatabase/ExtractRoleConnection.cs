using Extract.Licensing;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;

namespace Extract.SqlDatabase
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1802:Use literals where appropriate", Justification = "Readonly strings are encrypted by dotfuscator")]
    public sealed class ExtractRoleConnection : SqlAppRoleConnection
    {
        internal override string InternalRoleName => "ExtractRole";

        // This enables support for DbProviderFactories.GetFactory()
        protected override DbProviderFactory DbProviderFactory => ExtractRoleFactory.Instance;

        public ExtractRoleConnection(string server, string database, bool enlist = true)
            : base(SqlUtil.NewSqlDBConnection(server, database, enlist))
        {
            // Ensure calling assembly is signed by Extract
            ExtractException.Assert("ELI53054", "Failed internal verification",
                LicenseUtilities.VerifyAssemblyData(Assembly.GetCallingAssembly()));
        }

        public ExtractRoleConnection(string connectionString)
            : base(SqlUtil.NewSqlDBConnection(connectionString))
        {
            // Ensure calling assembly is signed by Extract
            ExtractException.Assert("ELI53055", "Failed internal verification",
                LicenseUtilities.VerifyAssemblyData(Assembly.GetCallingAssembly()));
        }

        internal ExtractRoleConnection(SqlConnection connection)
            :base(connection)
        {
        }

        /// <summary>
        /// Attempts to create a <see cref="SqlConnection"/> using the provided <paramref name="connectionString"/>
        /// as a connection to a FAM database with the Extract application role assigned.
        /// </summary>
        /// <param name="connectionString">The database connection string</param>
        /// <param name="connection">The resulting connection, if successful.
        /// NOTE: If successful and the database connection string is returned, it will already be open.</param>
        /// <returns><c>true</c> if the database connection was established and Extract application role
        /// was successfully applied; <c>false</c> if not successful for any reason (connection string doesn't
        /// specify a SQL database, couldn't connect to the database, or the database is not a FAM DB)</returns>
        public static bool TryOpenConnection(string connectionString, out ExtractRoleConnection connection)
        {
            // Ensure calling assembly is signed by Extract
            ExtractException.Assert("ELI53060", "Failed internal verification",
                LicenseUtilities.VerifyAssemblyData(Assembly.GetCallingAssembly()));

            connection = null;
            try
            {
                connection = new ExtractRoleConnection(connectionString);
                connection.Open();
                return true;
            }
            catch
            {
                connection?.Dispose();
                connection = null;
                return false;
            }
        }
    }

    internal sealed class ExtractRoleFactory : BaseRoleFactory
    {
        public static readonly ExtractRoleFactory Instance = new ExtractRoleFactory();

        private ExtractRoleFactory()
        {
        }

        // CreateConnection is specifically not implemented to ensure app role authenticated
        // connections are not created when not specifically required.
    }
}
