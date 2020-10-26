using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Extract.Licensing.Internal;
using ExtractLicenseUI.Database;
using ExtractLicenseUI.Utility;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ExtractLicenseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class License : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<PackageHeader> _PackageHeaders = new ObservableCollection<PackageHeader>();
        private ObservableCollection<PackageHeader> _ClonedPackageHeaders = new ObservableCollection<PackageHeader>();
        private ObservableCollection<ExtractVersion> _ExtractVersions = new ObservableCollection<ExtractVersion>();
        private Database.Organization _SelectedOrganization = new Database.Organization();

        public ObservableCollection<PackageHeader> PackageHeaders { 
            get { return this._PackageHeaders; }
            set 
            {
                _PackageHeaders = value;
                this.NotifyPropertyChanged(nameof(PackageHeaders));
            } 
        }

        public ObservableCollection<PackageHeader> ClonedPackageHeaders
        {
            get { return this._ClonedPackageHeaders; }
            set
            {
                _ClonedPackageHeaders = value;
                this.NotifyPropertyChanged(nameof(ClonedPackageHeaders));
            }
        }

        public ObservableCollection<ExtractVersion> ExtractVersions
        {
            get {
                return new ObservableCollection<ExtractVersion>(this._ExtractVersions.OrderByDescending(m => m.Version).ToList()); 
            }
            set
            {
                _ExtractVersions = value;
                this.NotifyPropertyChanged(nameof(ExtractVersions));
            }
        }

        public Database.Organization SelectedOrganization {
            get
            {
                return this._SelectedOrganization;
            }
            set
            {
                _SelectedOrganization = value;
                this.NotifyPropertyChanged(nameof(SelectedOrganization));
            }
        }

        public MainWindow MainWindow { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public License()
        {
            using (var databaseReader = new DatabaseReader())
            {
                this.ExtractVersions = databaseReader.ReadVersions();
            }
                
            InitializeComponent();
            this.Form.DataContext = this;
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            this.Loaded += LicenseControlLoaded;
        }

        /// <summary>
        /// Saves the license to the database if the form is valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!IsValid(this.Form))
            {
                MessageBox.Show("Please correct all of the validation errors before saving. License was NOT saved", "ExtractLicenseUI", MessageBoxButton.OK);
            }
            else if(!this.GetSelectedPackages().Any())
            {
                MessageBox.Show("You must select at least one package to license", "ExtractLicenseUI", MessageBoxButton.OK);
            }
            else 
            {
                if(string.IsNullOrEmpty(this.SelectedOrganization.SelectedLicense.LicenseName))
                {
                    this.SelectedOrganization.SelectedLicense.LicenseName = this.GenerateLicenseName();
                }
                
                this.SelectedOrganization.SelectedLicense.GenerateNewLicenseKey(this.SelectedOrganization, this.GetSelectedPackages());
                using var databaseWriter = new DatabaseWriter();
                databaseWriter.WriteLicense(this.SelectedOrganization);
                databaseWriter.WritePackages(this.SelectedOrganization.SelectedLicense, this.GetSelectedPackages());
                this.MainWindow.OrganizationWindow.RefreshLicenses();
                this.ConfigureNavigationOption(LicenseNavigationOptions.ViewLicense);
            }
        }

        private string GenerateLicenseName()
        {
            string licenseName = string.Empty;
            if(!String.IsNullOrEmpty(this.SelectedOrganization.Reseller))
            {
                licenseName += this.SelectedOrganization.Reseller + "_";
            }
            if (!String.IsNullOrEmpty(this.SelectedOrganization.State))
            {
                licenseName += this.SelectedOrganization.State + "_";
            }
            licenseName += this.SelectedOrganization.CustomerName + "_";
            if(this.PackageHeaders.Where(m => m.Name.ToUpper(CultureInfo.InvariantCulture).Contains("FLEX INDEX") && m.PackagesChecked).Any())
            {
                licenseName += "FlexIndex_";
            }
            if (this.PackageHeaders.Where(m => m.Name.ToUpper(CultureInfo.InvariantCulture).Contains("ID SHIELD") && m.PackagesChecked).Any())
            {
                licenseName += "IDShield_";
            }
            if (this.PackageHeaders.Where(m => m.Name.ToUpper(CultureInfo.InvariantCulture).Contains("LABDE") && m.PackagesChecked).Any())
            {
                licenseName += "LabDE_";
            }
            if(this.GetSelectedPackages().Where(m => m.Name.ToUpper(CultureInfo.InvariantCulture).Contains("SERVER")).Any())
            {
                licenseName += "Server_";
            }
            else
            {
                licenseName += "Client_";
            }
            licenseName += this.SelectedOrganization.SelectedLicense.RestrictByDiskSerialNumber 
                ? this.SelectedOrganization.SelectedLicense.MachineName + "_" 
                : "Universal_";
            licenseName += this.SelectedOrganization.SelectedLicense.IsPermanent
                ? "Full"
                : this.SelectedOrganization.SelectedLicense.ExpiresOn != null 
                    ? ((DateTime)this.SelectedOrganization.SelectedLicense.ExpiresOn).Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) 
                    : "Invalid Date";
            return licenseName;
        }

        /// <summary>
        /// Handles the cloning of a license.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloneButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ConfigureNavigationOption(LicenseNavigationOptions.CloneLicense);
        }

        /// <summary>
        /// Handles the logic for making a license permanent/expiring.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsPermanent_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ExpiresOn.IsEnabled = !(bool)this.IsPermanent.IsChecked;
            if ((bool)this.IsPermanent.IsChecked)
            {
                this.ExpiresOn.SelectedDate = null;
            }
        }

        /// <summary>
        /// Updates the packages depending on the version selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExtractVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using var databaseReader = new DatabaseReader();
            this.PackageHeaders = databaseReader.ReadPackages(this.SelectedOrganization.SelectedLicense.ExtractVersion);
        }

        /// <summary>
        /// Called by each of the property Set accessors when property changes
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets all of the selected packages from the packages being displayed.
        /// </summary>
        /// <returns></returns>
        private Collection<Package> GetSelectedPackages()
        {
            Collection<Package> selectedPackages = new Collection<Package>();
            foreach(var packageheader in this.PackageHeaders)
            {
                foreach(var package in packageheader.Packages)
                {
                    if(package.IsChecked)
                    {
                        selectedPackages.Add(package);
                    }
                }
            }
            return selectedPackages;
        }

        /// <summary>
        /// Handles all of the logic for cloning a license, and re-assigns the selected license
        /// to the new cloned one.
        /// </summary>
        private void CloneLicense()
        {
            this.ClonedPackageHeaders = this.PackageHeaders;

            var newLicense = new ExtractLicense();
            newLicense.IsPermanent = this.SelectedOrganization.SelectedLicense.IsPermanent;
            newLicense.ExpiresOn = this.SelectedOrganization.SelectedLicense.ExpiresOn;
            newLicense.RequestKey = this.SelectedOrganization.SelectedLicense.RequestKey;
            newLicense.IsActive = this.SelectedOrganization.SelectedLicense.IsActive;
            newLicense.Comments = this.SelectedOrganization.SelectedLicense.Comments;
            newLicense.IsProduction = this.SelectedOrganization.SelectedLicense.IsProduction;
            newLicense.SignedTransferForm = this.SelectedOrganization.SelectedLicense.SignedTransferForm;
            newLicense.SDKPassword = this.SelectedOrganization.SelectedLicense.SDKPassword;
            newLicense.ExtractVersion = this.SelectedOrganization.SelectedLicense.ExtractVersion;
            this.SelectedOrganization.SelectedLicense = newLicense;

            using var databaseReader = new DatabaseReader();
            this.PackageHeaders = databaseReader.ReadPackages(newLicense.ExtractVersion);

            this.CopyVersionCheckmarks();
        }

        private void CopyVersionCheckmarks()
        {
            foreach(var clonedPackageHeader in this.ClonedPackageHeaders)
            {
                var packagesToCheck = this.PackageHeaders
                    .Where(header => header.Name.Equals(clonedPackageHeader.Name, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(x => x.Packages);
                foreach(var package in packagesToCheck)
                {
                    if(clonedPackageHeader.Packages.Where(clonedPackage => clonedPackage.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase)).Any())
                    {
                        package.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if every object is valid in the object tree.
        /// </summary>
        /// <param name="obj">The dependency object to check</param>
        /// <returns></returns>
        private bool IsValid(DependencyObject obj)
        {
            // The dependency object is valid if it has no errors and all
            // of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(obj) &&
            LogicalTreeHelper.GetChildren(obj)
            .OfType<DependencyObject>()
            .All(IsValid);
        }

        /// <summary>
        /// Anything that changes the requirements on request key should call this.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateRequestKey(object sender, RoutedEventArgs e)
        {
            this.RequestKey.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        /// <summary>
        /// Fired only after the window has loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LicenseControlLoaded(object sender, RoutedEventArgs e)
        {
            this.MainWindow.LicenseWindow = this;
        }

        /// <summary>
        /// Generates a request key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateRequestKey_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedOrganization.SelectedLicense.RequestKey = LicenseInfo.GenerateUserString();
        }

        /// <summary>
        /// Saves the currently active license to a file with a folder dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveLicenseToFile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                using (CommonOpenFileDialog fileDialog = new CommonOpenFileDialog()
                {
                    Multiselect = false,
                    EnsurePathExists = true
                })
                {
                    fileDialog.InitialDirectory = @"C:\ProgramData\Extract Systems\LicenseFiles";
                    fileDialog.Title = "Please select a folder to generate the file in";
                    fileDialog.DefaultFileName = this.SelectedOrganization.SelectedLicense.LicenseName + @".lic";
                    var fileDialogResult = fileDialog.ShowDialog();
                    if (fileDialogResult == CommonFileDialogResult.Ok)
                    {
                        this.SelectedOrganization.SelectedLicense.GenerateLicenseFile(fileDialog.FileName);
                    }
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Unable to save the license to a file. Make sure you use a valid file name and path.");
            }
        }


        /// <summary>
        /// Copies the active license to the clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyLicenseToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileName = Path.Combine(Path.GetTempPath(), SelectedOrganization.SelectedLicense.LicenseName + ".lic");
                this.SelectedOrganization.SelectedLicense.GenerateLicenseFile(fileName);
                this.CopyFileToClipboard(fileName);
            }
            catch(Exception)
            {
                MessageBox.Show("Unable to copy license to clipboard");
            }
        }

        /// <summary>
        /// Allows the user to save the unlock code to a file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveUnlockCodeToFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (CommonOpenFileDialog fileDialog = new CommonOpenFileDialog()
                {
                    IsFolderPicker = true,
                    Multiselect = false,
                    EnsurePathExists = true
                })
                {
                    fileDialog.InitialDirectory = @"C:\ProgramData\Extract Systems\LicenseFiles";
                    fileDialog.Title = "Please select a folder to generate the unlock code in";
                    var fileDialogResult = fileDialog.ShowDialog();
                    if (fileDialogResult == CommonFileDialogResult.Ok)
                    {
                        this.SelectedOrganization.SelectedLicense.GenerateUnlockCode(this.SelectedOrganization, fileDialog.FileName + @"\");
                    }
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Unable to save unlock code to file. Ensure you use a valid file path");
            }
        }


        /// <summary>
        /// Copies the unlock code to the clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyUnlockCodeToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tempFolder = Path.GetTempPath();
                var fileName = Path.Combine(tempFolder, LicenseInfo.ExpiringLicenseUnlockFileName);
                this.SelectedOrganization.SelectedLicense.GenerateUnlockCode(this.SelectedOrganization, tempFolder);
                this.CopyFileToClipboard(fileName);
            }
            catch(Exception)
            {
                MessageBox.Show("Unable to copy unlock code to clipboard.");
            }
        }


        /// <summary>
        /// Copies the file to the clipboard.
        /// </summary>
        /// <param name="filePath"></param>
        private void CopyFileToClipboard(string filePath)
        {
            try
            {
                var stringCollection = new System.Collections.Specialized.StringCollection();
                stringCollection.Add(filePath);
                Clipboard.SetFileDropList(stringCollection);
            }
            catch(Exception)
            {
                MessageBox.Show("Unable to copy file to clipboard.");
            }
        }


        /// <summary>
        /// Opens the hyper link in default browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.SelectedOrganization.SalesforceHyperlink);
            }
            catch (Exception)
            {
                MessageBox.Show($@"Cannot open link to: {this.SelectedOrganization.SalesforceHyperlink}");
            }
        }

        /// <summary>
        /// Configures the license page depending on what "mode" you are in.
        /// </summary>
        /// <param name="licenseNavigationOption"></param>
        public void ConfigureNavigationOption(LicenseNavigationOptions licenseNavigationOption)
        {
            switch (licenseNavigationOption)
            {
                case LicenseNavigationOptions.CloneLicense:
                    this.CloneLicense();
                    break;
                case LicenseNavigationOptions.EditLicense: break;
                case LicenseNavigationOptions.NewLicense:
                    this.PackageHeaders = new ObservableCollection<PackageHeader>();
                    this.ClonedPackageHeaders.Clear();
                    break;
                case LicenseNavigationOptions.ViewLicense:
                    using (var databaseReader = new DatabaseReader())
                        this.PackageHeaders = databaseReader.ReadPackages(this.SelectedOrganization.SelectedLicense);
                    break;
                case LicenseNavigationOptions.None: break;
            }
            UpdateFormControls(licenseNavigationOption);
        }

        /// <summary>
        /// Enabled/disables form controls depending on what "mode" your in.
        /// </summary>
        /// <param name="option">The licenseNavigationOption to restrict controls for.</param>
        private void UpdateFormControls(LicenseNavigationOptions option)
        {
            this.DisableAllControls();
            switch (option)
            {
                case LicenseNavigationOptions.EditLicense:
                    this.Comments.IsEnabled = true;
                    this.SignedTransferForm.IsEnabled = true;
                    this.IsActive.IsEnabled = true;
                    this.IsProduction.IsEnabled = true;
                    this.LicenseName.IsEnabled = true;
                    this.GenerateLicenseNameButton.Visibility = System.Windows.Visibility.Visible;
                    this.UpdateButton.Visibility = System.Windows.Visibility.Visible;
                    break;
                case LicenseNavigationOptions.CloneLicense:
                    this.CloneLabel.Visibility = System.Windows.Visibility.Visible;
                    this.ClonedPackageSelector.Visibility = System.Windows.Visibility.Visible;
                    goto case LicenseNavigationOptions.NewLicense;
                case LicenseNavigationOptions.NewLicense:
                    this.RequestKey.IsEnabled = true;
                    this.IsPermanent.IsEnabled = true;
                    this.LicenseName.IsEnabled = true;
                    this.ExpiresOn.IsEnabled = true;
                    this.IsActive.IsEnabled = true;
                    this.Comments.IsEnabled = true;
                    this.IsProduction.IsEnabled = true;
                    this.SignedTransferForm.IsEnabled = true;
                    this.ExtractVersion.IsEnabled = true;
                    this.SaveButton.Visibility = System.Windows.Visibility.Visible;
                    this.GenerateRequestKey.Visibility = System.Windows.Visibility.Visible;
                    this.GenerateLicenseNameButton.Visibility = System.Windows.Visibility.Visible;
                    this.UseDiskSerialNumber.IsEnabled = true;
                    break;
                case LicenseNavigationOptions.ViewLicense:
                    this.CloneButton.Visibility = System.Windows.Visibility.Visible;
                    this.SaveLicenseToFile.Visibility = System.Windows.Visibility.Visible;
                    this.CopyLicenseToClipboard.Visibility = System.Windows.Visibility.Visible;
                    this.SaveUnlockCodeToFile.Visibility = this.SelectedOrganization.SelectedLicense.IsPermanent? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    this.CopyUnlockCodeToClipboard.Visibility = this.SelectedOrganization.SelectedLicense.IsPermanent ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    this.EditLicense.Visibility = System.Windows.Visibility.Visible; 
                    break;
                case LicenseNavigationOptions.None: break;
            }

        }

        /// <summary>
        /// Event handler for the generate license name button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateLicenseName_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedOrganization.SelectedLicense.LicenseName = GenerateLicenseName();
        }

        private void EditLicense_Click(object sender, RoutedEventArgs e)
        {
            this.ConfigureNavigationOption(LicenseNavigationOptions.EditLicense);
        }

        private void DisableAllControls()
        {
            this.ExtractVersion.IsEnabled = false;
            this.RequestKey.IsEnabled = false;
            this.IsPermanent.IsEnabled = false;
            this.ExpiresOn.IsEnabled = false;
            this.IssuedOn.IsEnabled = false;
            this.TransferLicense.IsEnabled = false;
            this.Comments.IsEnabled = false;
            this.SDKPassword.IsEnabled = false;
            this.SignedTransferForm.IsEnabled = false;
            this.IsActive.IsEnabled = false;
            this.IsProduction.IsEnabled = false;
            this.UseDiskSerialNumber.IsEnabled = false;
            this.LicenseName.IsEnabled = false;
            this.ClonedPackageSelector.Visibility = System.Windows.Visibility.Collapsed;
            this.GenerateRequestKey.Visibility = System.Windows.Visibility.Collapsed;
            this.SaveButton.Visibility = System.Windows.Visibility.Collapsed;
            this.CloneButton.Visibility = System.Windows.Visibility.Collapsed;
            this.SaveLicenseToFile.Visibility = System.Windows.Visibility.Collapsed;
            this.CopyLicenseToClipboard.Visibility = System.Windows.Visibility.Collapsed;
            this.SaveUnlockCodeToFile.Visibility = System.Windows.Visibility.Collapsed;
            this.CopyUnlockCodeToClipboard.Visibility = System.Windows.Visibility.Collapsed;
            this.GenerateLicenseNameButton.Visibility = System.Windows.Visibility.Collapsed;
            this.EditLicense.Visibility = System.Windows.Visibility.Collapsed;
            this.UpdateButton.Visibility = System.Windows.Visibility.Collapsed;
            this.CloneLabel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            using var databaseWriter = new DatabaseWriter();
            databaseWriter.WriteLicense(this.SelectedOrganization);
            this.ConfigureNavigationOption(LicenseNavigationOptions.ViewLicense);
        }
    }
}
