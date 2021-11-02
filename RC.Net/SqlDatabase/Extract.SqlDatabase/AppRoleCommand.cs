using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.SqlDatabase
{
    /// <summary>
    /// This class behaves as SQLCommand class but is needed so the SqlAppRoleConnection connection is part of the command
    /// </summary>
    public class AppRoleCommand : DbCommand
    {


        public AppRoleCommand() { BaseSqlCommand = new SqlCommand(); }

        public AppRoleCommand(string sqlText, SqlAppRoleConnection connection):
            this()
        {
            BaseSqlCommand.CommandText = sqlText;
            Connection = connection;    
        }
        
        /// <summary>
        /// SqlCommand that is encapsulated by this class
        /// </summary>
        public SqlCommand BaseSqlCommand { get; set; }



        public override string CommandText { get => BaseSqlCommand.CommandText; set => BaseSqlCommand.CommandText = value; }
        public override int CommandTimeout { get => BaseSqlCommand.CommandTimeout; set => BaseSqlCommand.CommandTimeout = value; }
        public override CommandType CommandType { get => BaseSqlCommand.CommandType; set => BaseSqlCommand.CommandType = value; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get => BaseSqlCommand.UpdatedRowSource; set => BaseSqlCommand.UpdatedRowSource = value; }


        private SqlAppRoleConnection _connection;

        /// <summary>
        /// Makes sure that the connection that is used by the BaseSqlCommand is the one tied to the SqlAppRoleConnection
        /// </summary>
        public new SqlAppRoleConnection Connection
        {
            get => (SqlAppRoleConnection)DbConnection;
            set => DbConnection = value;
        }

        protected override DbConnection DbConnection
        {
            get => _connection;
            set
            {
                _connection = (SqlAppRoleConnection)value;

                BaseSqlCommand.Connection = _connection?.BaseSqlConnection;
            } 
        }
        
        public new SqlParameterCollection Parameters { get => BaseSqlCommand.Parameters; }

        protected override DbParameterCollection DbParameterCollection => BaseSqlCommand.Parameters;

        public new SqlTransaction Transaction { get => DbTransaction as SqlTransaction; set => DbTransaction = value; }

        protected override DbTransaction DbTransaction { get => BaseSqlCommand.Transaction; set =>  BaseSqlCommand.Transaction = (SqlTransaction)value; }

        public override void Cancel()
        {
            BaseSqlCommand.Cancel();
        }

        public override int ExecuteNonQuery()
        {
            return BaseSqlCommand.ExecuteNonQuery(); 
        }

        public override object ExecuteScalar()
        {
            return BaseSqlCommand.ExecuteScalar();
        }

        public override void Prepare()
        {
            BaseSqlCommand.Prepare();
        }

        public new SqlParameter CreateParameter()
        {
            return BaseSqlCommand.CreateParameter();
        }

        protected override DbParameter CreateDbParameter()
        {
            return BaseSqlCommand.CreateParameter();
        }

        public new async Task<SqlDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default)
        {
            return await BaseSqlCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        }

        public new  SqlDataReader ExecuteReader()
        {
            return (SqlDataReader)ExecuteDbDataReader(default);
        }

        protected async override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            return await BaseSqlCommand.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false);
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return BaseSqlCommand.ExecuteReader(behavior);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseSqlCommand?.Dispose();
                BaseSqlCommand = null;
                _connection = null;
            }
            base.Dispose(disposing);
        }
    }
}
