using Extract.Licensing;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;

namespace Extract.SqlDatabase
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1802:Use literals where appropriate", Justification = "Readonly strings are encrypted by dotfuscator")]
    public sealed class ExtractReportingRoleConnection : SqlAppRoleConnection
    {
    	internal override string InternalRoleName => ExtractReportingRole;
    	
		private static readonly string _appRoleQuery = @"exec sys.sp_setapprole '{0}','{1}', 'none', 0";

        // This enables support for DbProviderFactories.GetFactory()
        protected override DbProviderFactory DbProviderFactory => ExtractReportingRoleFactory.Instance;

        public ExtractReportingRoleConnection(string server, string database, bool enlist = true)
            : base(SqlUtil.NewSqlDBConnection(server, database, enlist))
        {
            // Ensure calling assembly is signed by Extract
            ExtractException.Assert("ELI53057", "Failed internal verification",
                LicenseUtilities.VerifyAssemblyData(Assembly.GetCallingAssembly()));
        }

        public ExtractReportingRoleConnection(string connectionString)
            : base(SqlUtil.NewSqlDBConnection(connectionString))
        {
            // Ensure calling assembly is signed by Extract
            ExtractException.Assert("ELI53058", "Failed internal verification",
                LicenseUtilities.VerifyAssemblyData(Assembly.GetCallingAssembly()));
        }

        internal ExtractReportingRoleConnection(SqlConnection connection)
            : base(connection)
        {
        }

        internal string ReportAccessQuery()
        {
            // Ensure calling assembly is signed by Extract
            ExtractException.Assert("ELI53074", "Failed internal verification",
                LicenseUtilities.VerifyAssemblyData(Assembly.GetCallingAssembly()));

            return string.Format(CultureInfo.CurrentCulture, _appRoleQuery, RoleName, new string(GetRolePassword(out bool _)));
        }
    }

    internal sealed class ExtractReportingRoleFactory : BaseRoleFactory
    {
        public static readonly ExtractReportingRoleFactory Instance = new ExtractReportingRoleFactory();

        private ExtractReportingRoleFactory()
        {
        }

        // CreateConnection is specifically not implemented to ensure app role authenticated
        // connections are not created when not specifically required.
    }
}
