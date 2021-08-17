using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.SqlDatabase
{
    public abstract class ApplicationRoleConnection : IDisposable
    {

        private bool disposedValue;

        public SqlConnection SqlConnection { get; set; }

        protected SqlApplicationRole ApplicationRole { get; set; }

        readonly bool OwnConnection;

        public ApplicationRoleConnection(SqlConnection sqlConnection)
        {
            try
            {
                OwnConnection = false;
                SqlConnection = sqlConnection;
                if (SqlConnection.State != System.Data.ConnectionState.Open)
                    SqlConnection.Open();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51771");
            }
        }

        public ApplicationRoleConnection(string server, string database, bool enlist = true)
        {
            try
            {
                OwnConnection = true;
                SqlConnection = SqlUtil.NewSqlDBConnection(server, database, enlist);
                SqlConnection.Open();

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51772");
            }
        }

        public ApplicationRoleConnection(string connectionString)
        {
            try
            {
                OwnConnection = true;
                SqlConnection = new SqlConnection(connectionString);
                SqlConnection.Open();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51773");
            }
        }

        protected abstract void AssignRole();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ApplicationRole?.Dispose();
                    ApplicationRole = null;
                    
                    if (OwnConnection) SqlConnection?.Dispose();
                    SqlConnection = null;
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
