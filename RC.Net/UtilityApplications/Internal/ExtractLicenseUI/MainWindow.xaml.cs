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
        readonly Link _LicenseLink = new Link() { DisplayName = "License", Source = new Uri("/Pages/License.xaml", UriKind.Relative) };
        readonly Link _ContactLink = new Link() { DisplayName = "Contact", Source = new Uri("/Pages/Contact.xaml", UriKind.Relative) };

        private License _LicenseWindow { get; set; }

        private Contact _ContactWindow { get; set; }

        public Organization OrganizationWindow { get; set; }

        public event EventHandler LicenseWindowCreated;

        public event EventHandler ContactWindowCreated;

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

        public Contact ContactWindow
        {
            get
            {
                return this._ContactWindow;
            }
            set
            {
                if (value != this._ContactWindow)
                {
                    this._ContactWindow = value;
                    this.ContactWindowCreated?.Invoke(this, new EventArgs());
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

        public void NavigateToContact(Database.Organization organization)
        {
            this.ContentSource = _ContactLink.Source;
            if (ContactWindow == null)
            {
                this.ContactWindowCreated += (o, e) =>
                {
                    this.ContactWindow.SelectedOrganization = organization;
                };
            }
            else
            {
                this.ContactWindow.SelectedOrganization = organization;
            }
        }
    }
}
