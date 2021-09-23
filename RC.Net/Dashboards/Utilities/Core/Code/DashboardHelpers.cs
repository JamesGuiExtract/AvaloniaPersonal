using DevExpress.DashboardCommon;
using DevExpress.DataAccess;
using DevExpress.DataAccess.Sql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Extract.Dashboard.Utilities
{
    public static class DashboardHelpers
    {
        private static readonly string appRoleQuery = @"exec sys.sp_setapprole 'ExtractRole','Change2This3Password', 'none', 0";
        static public string AppRoleQueryName<TSqlSource>(this TSqlSource dashboardSqlDataSource) where TSqlSource : SqlDataSource
        {
            return $"DoNotDelete_AppRole{dashboardSqlDataSource.ConnectionName}";
        }
        static public void AddAppRoleQuery(DevExpress.DashboardCommon.Dashboard activeDashboard)
        {
            try
            {
                if (activeDashboard is null) return;

                var sqlDataSources = activeDashboard.DataSources
                        .OfType<DashboardSqlDataSource>();

                foreach (var sqlDataSource in sqlDataSources)
                {
                    AddAppRoleQuery(sqlDataSource);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51906");
            }
        }

        static public void AddAppRoleQuery(SqlDataSource sqlDataSource)
        {
            try
            {
                if (sqlDataSource is null) return;

                DashboardSqlDataSource.AllowCustomSqlQueries = true;
                sqlDataSource.Connection.Close();
                sqlDataSource.ConnectionOptions.CloseConnection = false;
                sqlDataSource.ConnectionOptions.DbCommandTimeout = 0;
                string queryName = sqlDataSource.AppRoleQueryName();

                var setAppRoleQuery = sqlDataSource.Queries.FirstOrDefault(q => q.Name == queryName);
                if (setAppRoleQuery != null) sqlDataSource.Queries.Remove(setAppRoleQuery);

                var customQuery = new CustomSqlQuery(queryName, appRoleQuery);

                sqlDataSource.Queries.Insert(0, customQuery);
                sqlDataSource.ValidateCustomSqlQuery += HandleCustomSqlQuery;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51907");
            }
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
            return sqlQuery == appRoleQuery || !sqlCommandsNotAllowed.Any(s => sql.Contains(s));
        }

    }
}
