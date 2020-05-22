using Extract;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UCLID_FILEPROCESSINGLib;

namespace DatabaseMigrationWizard.Database
{
    public class ConnectionInformation : INotifyPropertyChanged
    {
        private string _DatabaseName = string.Empty;
        private string _DatabaseServer = string.Empty;
        private bool _ConnectionInfoValidated = false;

        public string DatabaseName
        {
            get
            {
                return this._DatabaseName;
            }
            set
            {
                if(!this._DatabaseName.Equals(value))
                {
                    this._DatabaseName = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string DatabaseServer
        {
            get
            {
                return this._DatabaseServer;
            }
            set
            {
                if(!this._DatabaseServer.Equals(value))
                {
                    this._DatabaseServer = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool ConnectionInfoValidated
        {
            get
            {
                return this._ConnectionInfoValidated;
            }
            set
            {
                if (value != _ConnectionInfoValidated)
                {
                    this._ConnectionInfoValidated = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public void ValidateConnection(string password, bool onetimePassword = false)
        {
            try
            {
                var fileProcessingDb = new FileProcessingDB()
                {
                    DatabaseServer = this.DatabaseServer,
                    DatabaseName = this.DatabaseName
                };
                // "<Admin>" signifies a one-time password is being used.
                fileProcessingDb.LoginUser(onetimePassword ? "<Admin>" : "admin", password);

                ConnectionInfoValidated = true;
            }
            catch (System.Exception ex)
            {
                ConnectionInfoValidated = false;
                throw ex.AsExtract("ELI49825");
            }
        }

        /// <summary>
        /// Called by each of the property Set accessors when property changes
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName != "ConnectionInfoValidated")
            {
                // If any connection properties have changed, the connection should be considered invalid
                // until re-validated.
                ConnectionInfoValidated = false;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}