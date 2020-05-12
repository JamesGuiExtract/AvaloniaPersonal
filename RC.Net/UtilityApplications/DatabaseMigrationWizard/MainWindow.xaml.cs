using DatabaseMigrationWizard.Database;
using DatabaseMigrationWizard.Database.Output;
using DatabaseMigrationWizard.Pages;
using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using Extract.Licensing;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace DatabaseMigrationWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow, INotifyPropertyChanged
    {
        public ConnectionInformation ConnectionInformation { get; set; }

        public ExportOptions ExportOptions { get; set; }

        private bool _UIEnabled = true;

        private bool _CommitSuccessful = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Report> Reporting { get; } = new ObservableCollection<Report>();

        public Import Import { get; set; }

        public ReportWindow ReportWindow { get; set; }

        public bool UIEnabled
        {
            get
            {
                return this._UIEnabled;
            }
            set
            {
                this._UIEnabled = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UIEnabled"));
            }
        }

        public bool CommitSuccessful
        {
            get
            {
                return this._CommitSuccessful;
            }
            set
            {
                this._CommitSuccessful = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CommitSuccessful"));
            }
        }

        public MainWindow(ConnectionInformation connectionInformation)
        {
            try
            {
                this.MainWindowSetup(connectionInformation);
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49729");
            }
        }

        public MainWindow(ConnectionInformation connectionInformation, ExportOptions exportOptions)
        {
            try
            {
                this.MainWindowSetup(connectionInformation);
                this.ExportOptions = exportOptions;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49800");
            }
        }

        private void MainWindowSetup(ConnectionInformation connectionInformation)
        {
            this.ConnectionInformation = connectionInformation;
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI49728", typeof(MainWindow).ToString());
            InitializeComponent();
            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!UIEnabled)
            {
                e.Cancel = true;
                MessageBox.Show("Closing is not supported while importing/exporting. Please wait for the operation to complete");
            }
        }
    }
}
