using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input
{
    public class ImportOptions : INotifyPropertyChanged
    {
        private string _ImportPath = @"C:\TableExports";

        public string ImportPath
        {
            get
            {
                return this._ImportPath;
            }
            set
            {
                this._ImportPath = value;
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ImportPath"));
            }
        }
        public bool ClearDatabase { get; set; }

        public ConnectionInformation ConnectionInformation { get; set; }

        /// <summary>
        /// Handles every time the property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
