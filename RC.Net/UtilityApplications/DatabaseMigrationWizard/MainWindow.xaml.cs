using DatabaseMigrationWizard.Database;
using Extract;
using Extract.Licensing;
using FirstFloor.ModernUI.Windows.Controls;
using System;
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

        private bool _UIEnabled = true;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public MainWindow(ConnectionInformation connectionInformation)
        {
            try
            {
                this.ConnectionInformation = connectionInformation;
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI49728", typeof(MainWindow).ToString());
                InitializeComponent();
                this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49729");
            }
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
