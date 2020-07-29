using ExtractLicenseUI.Database;
using ExtractLicenseUI.Utility;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExtractLicenseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        Link _LicenseLink = new Link() { DisplayName = "License", Source = new Uri("/Pages/License.xaml", UriKind.Relative) };
        private License _LicenseWindow { get; set; }

        public Organization OrganizationWindow { get; set; }

        public event EventHandler LicenseWindowCreated;

        public License LicenseWindow
        {
            get
            {
                return this._LicenseWindow;
            }
            set
            {
                if (value != this._LicenseWindow)
                {
                    this._LicenseWindow = value;
                    this.LicenseWindowCreated?.Invoke(this, new EventArgs());
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.ContentSource = MainLinks.Links.First().Source;
        }

        public void NavigateToLicense(Database.Organization organization, LicenseNavigationOptions licenseNavigationOption)
        {
            this.ContentSource = _LicenseLink.Source;
            if (LicenseWindow == null)
            {
                this.LicenseWindowCreated += (o, e) =>
                {
                    this.LicenseWindow.SelectedOrganization = organization;
                    this.LicenseWindow.ConfigureNavigationOption(licenseNavigationOption);
                };
            }
            else
            {
                this.LicenseWindow.SelectedOrganization = organization;
                this.LicenseWindow.ConfigureNavigationOption(licenseNavigationOption);
            }
        }
    }
}
