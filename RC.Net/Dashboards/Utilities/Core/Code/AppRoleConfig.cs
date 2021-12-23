using DevExpress.DashboardCommon;
using DevExpress.DataAccess.Sql;
using Extract.SqlDatabase;
using System;
using System.Linq;

namespace Extract.Dashboard.Utilities
{
    internal class AppRoleConfig : IDisposable
    {
        private ExtractReportingRoleConnection _reportingConnection;
        public ExtractReportingRoleConnection ReportingConnection { get => _reportingConnection; }

        private bool disposedValue;

        public AppRoleConfig(string connectionString)
        {
            _reportingConnection = new(DashboardHelpers.RemoveProviderString(connectionString));
        }
        public void AddAppRoleQuery(DevExpress.DashboardCommon.Dashboard activeDashboard)
        {
            try
            {
                if (!SqlAppRoleConnection.UseApplicationRoles || activeDashboard is null)
                {
                    return;
                }

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

        public bool AddAppRoleQuery(SqlDataSource sqlDataSource)
        {
            try
            {
                if (sqlDataSource is null || !SqlAppRoleConnection.UseApplicationRoles)
                {
                    return false;
                }

                DashboardSqlDataSource.AllowCustomSqlQueries = true;
                sqlDataSource.Connection.Close();
                sqlDataSource.ConnectionOptions.CloseConnection = false;
                sqlDataSource.ConnectionOptions.DbCommandTimeout = 0;
                string queryName = sqlDataSource.AppRoleQueryName();

                // This makes sure the database can be opened with the approle
                if (ReportingConnection.State != System.Data.ConnectionState.Open)
                {
                    ReportingConnection.Open();
                }

                var setAppRoleQuery = sqlDataSource.Queries.FirstOrDefault(q => q.Name == queryName);
                if (setAppRoleQuery != null) sqlDataSource.Queries.Remove(setAppRoleQuery);

                var customQuery = new CustomSqlQuery(queryName, ReportingConnection.ReportAccessQuery());

                sqlDataSource.Queries.Insert(0, customQuery);
                sqlDataSource.ValidateCustomSqlQuery += DashboardHelpers.HandleCustomSqlQuery;
                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51907");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_reportingConnection != null)
                {
                    _reportingConnection.Close();
                    _reportingConnection.Dispose();
                    _reportingConnection = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}