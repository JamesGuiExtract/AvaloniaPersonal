using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DatabaseMigrationWizard.Database
{
    public class ConnectionInformation : INotifyPropertyChanged
    {
        private string _DatabaseName = string.Empty;

        private string _DatabaseServer = string.Empty;

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

        /// <summary>
        /// Called by each of the property Set accessors when property changes
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}