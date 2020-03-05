using DatabaseMigrationWizard.Database;
using Extract.Licensing;
using FirstFloor.ModernUI.Windows.Controls;


namespace DatabaseMigrationWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public ConnectionInformation ConnectionInformation { get; set; }

        public MainWindow(ConnectionInformation connectionInformation)
        {
            this.ConnectionInformation = connectionInformation;
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            InitializeComponent();
        }
    }
}
