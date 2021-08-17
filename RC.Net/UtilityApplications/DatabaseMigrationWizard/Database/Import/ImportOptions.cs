﻿using Extract; 
using Extract.SqlDatabase;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;

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

        public SqlAppRoleConnection SqlConnection { get; set; }

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
    }
}
