using Extract;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input
{
    public class ImportOptions : INotifyPropertyChanged
    {
        private string _ImportPath = String.Empty;

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
        public bool ClearDatabase { get; set; }

        public ConnectionInformation ConnectionInformation { get; set; }

        /// <summary>
        /// Handles every time the property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public SqlTransaction Transaction { get; set; }

        public SqlConnection SqlConnection { get; set; }

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
            catch(Exception e)
            {
                throw e.AsExtract("ELI49730");
            }
        }
    }
}
