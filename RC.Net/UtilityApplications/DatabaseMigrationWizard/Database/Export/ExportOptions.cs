using System.ComponentModel;

namespace DatabaseMigrationWizard.Database.Output
{
    /// <summary>
    /// Class that represents all of the settings you can change while exporting a table.
    /// </summary>
    public class ExportOptions : INotifyPropertyChanged
    {
        /// <summary>
        /// The path to export the files to.
        /// </summary>
        private string _ExportPath = string.Empty;

        /// <summary>
        /// Notifies the UI when the export path changes, and updates its value.
        /// </summary>
        public string ExportPath {
            get {
                return this._ExportPath;
            } set {
                this._ExportPath = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportPath"));
            } }

        /// <summary>
        /// Bool used to control if labde tables are exported
        /// </summary>
        public bool ExportLabDETables { get; set; } = false;

        /// <summary>
        /// Bool used to control if core tables are exported.
        /// </summary>
        public bool ExportCoreTables { get; set; } = true;

        /// <summary>
        /// Database connection information.
        /// </summary>
        public ConnectionInformation ConnectionInformation { get; set; }

        /// <summary>
        /// Handles every time the property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
