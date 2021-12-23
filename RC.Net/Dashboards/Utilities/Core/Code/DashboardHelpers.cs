using DevExpress.DashboardCommon;
using DevExpress.DataAccess;
using DevExpress.DataAccess.Sql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Extract.Dashboard.Utilities
{
    internal static class DashboardHelpers
    {
        static readonly string _StartOfSetAppRole = "exec sys.sp_setapprole";

        static public string AppRoleQueryName<TSqlSource>(this TSqlSource dashboardSqlDataSource) where TSqlSource : SqlDataSource
        {
            return $"DoNotDelete_AppRole{dashboardSqlDataSource.ConnectionName}";
        }

        static public void HandleCustomSqlQuery(object sender, ValidateCustomSqlQueryEventArgs e)
        {
            e.Valid = IsValidCustomSqlQuery(e.CustomSqlQuery.Sql);
        }
        static public void HandleDashboardCustomSqlQuery(object sender, ValidateDashboardCustomSqlQueryEventArgs e)
        {
            e.Valid = IsValidCustomSqlQuery(e.CustomSqlQuery.Sql);
        }

        private static bool IsValidCustomSqlQuery(string sqlQuery)
        {
            var sqlCommandsNotAllowed = new List<string>() { "INSERT", "MERGE", "DELETE", "DROP", "CREATE", "EXEC" };
            var sql = sqlQuery.ToUpper(CultureInfo.InvariantCulture);
            if (sqlQuery.StartsWith(_StartOfSetAppRole, StringComparison.OrdinalIgnoreCase))
            {
                sql = sqlQuery.Substring(_StartOfSetAppRole.Length).ToUpper(CultureInfo.InvariantCulture);
            }
            return !sqlCommandsNotAllowed.Any(s => sql.Contains(s));
        }

        internal static string RemoveProviderString(string connectionString)
        {
            string providerString = "XpoProvider=MSSqlServer;";

            return connectionString.Replace(providerString, string.Empty);
        }
    }
}
