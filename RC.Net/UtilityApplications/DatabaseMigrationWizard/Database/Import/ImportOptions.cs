using Extract; 
using Extract.SqlDatabase;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;

namespace DatabaseMigrationWizard.Database.Input
{
    public class ImportOptions : INotifyPropertyChanged, IDisposable
    {
        private string _ImportPath = String.Empty;
        private SqlAppRoleConnection _SqlConnection;
        private bool disposedValue;

        public string ImportPath
        {
            get
            {
                return this._ImportPath;
            }
            set
            {
                this._ImportPath = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImportPath"));
            }
        }

        public ConnectionInformation ConnectionInformation { get; set; }

        /// <summary>
        /// Used to determine if we want to import LabDE tables.
        /// </summary>
        public bool ImportLabDETables { get; set; } = false;

        /// <summary>
        /// Used to determine if we want to import everything that is not a labde tables
        /// Or in other words the tables required for setting up our software.
        /// </summary>
        public bool ImportCoreTables { get; set; } = true;

        /// <summary>
        /// Handles every time the property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public SqlTransaction Transaction { get; set; }

        /// This object owns this connection;
        /// The connection will be disposed if replaced and when this ImportOptions instance is disposed
        // TODO: This class and ImportHelper are not separated very cleanly.
        // Perhaps the connection and transaction should be moved to ImportHelper but that would require a lot of changes
        public SqlAppRoleConnection SqlConnection
        {
            get
            {
                return _SqlConnection;
            }
            set
            {
                if (value != _SqlConnection)
                {
                    _SqlConnection?.Dispose();
                    _SqlConnection = value;
                }
            }
        }

        public void ExecuteCommand(string command)
        {
            try
            {
                using (DbCommand dbCommand = SqlConnection.CreateCommand())
                {
                    dbCommand.Transaction = this.Transaction;
                    dbCommand.CommandText = command;
                    dbCommand.CommandTimeout = 0;
                    dbCommand.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI49730");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Transaction?.Dispose();
                    this.Transaction = null;
                    this.SqlConnection?.Dispose();
                }

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
