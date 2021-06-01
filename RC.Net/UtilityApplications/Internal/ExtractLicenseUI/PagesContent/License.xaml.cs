using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Extract.Licensing.Internal;
using ExtractLicenseUI.Database;
using ExtractLicenseUI.PagesContent;
using ExtractLicenseUI.Utility;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ExtractLicenseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class License : UserControl, INotifyPropertyChanged, IContent
    {
        private ObservableCollection<PackageHeader> _PackageHeaders = new ObservableCollection<PackageHeader>();
        private ObservableCollection<PackageHeader> _ClonedPackageHeaders = new ObservableCollection<PackageHeader>();
        private ObservableCollection<ExtractVersion> _ExtractVersions = new ObservableCollection<ExtractVersion>();
        private Database.ExtractLicense _SelectedLicense = new Database.ExtractLicense();

        public ObservableCollection<PackageHeader> PackageHeaders
        {
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
            get
            {
                return new ObservableCollection<ExtractVersion>(this._ExtractVersions.OrderByDescending(m => m.Version).ToList());
            }
            set
            {
                _ExtractVersions = value;
                this.NotifyPropertyChanged(nameof(ExtractVersions));
            }
        }

        public Database.ExtractLicense SelectedLicense
        {
            get
            {
                return this._SelectedLicense;
            }
            set
            {
                _SelectedLicense = value;
                this.NotifyPropertyChanged(nameof(SelectedLicense));
            }
        }

        public MainWindow MainWindow { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public License()
        {
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            using (var databaseReader = new DatabaseReader())
            {
                this.ExtractVersions = databaseReader.ReadVersions();
            }

            InitializeComponent();
            this.Form.DataContext = this;
        }

        /// <summary>
        /// Saves the license to the database if the form is valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (!IsValid(this.Form))
                {
                    MessageBox.Show("Please correct all of the validation errors before saving. License was NOT saved", "ExtractLicenseUI", MessageBoxButton.OK);
                }
                else if (!this.GetSelectedPackages().Any())
                {
                    MessageBox.Show("You must select at least one package to license", "ExtractLicenseUI", MessageBoxButton.OK);
                }
                else
                {
                    if (string.IsNullOrEmpty(this.SelectedLicense.LicenseName))
                    {
                        this.SelectedLicense.LicenseName = this.GenerateLicenseName();
                    }

                    this.SelectedLicense.GenerateNewLicenseKey(this.MainWindow.Organization.SelectedOrganization, this.SelectedLicense, this.GetSelectedPackages());
                    using var databaseWriter = new DatabaseWriter();
                    databaseWriter.WriteLicense(this.MainWindow.Organization.SelectedOrganization, this.SelectedLicense);
                    databaseWriter.WritePackages(this.SelectedLicense, this.GetSelectedPackages());
                    this.MainWindow.Organization.RefreshLicenses();
                    this.ConfigureNavigationOption(LicenseNavigationOptions.ViewLicense);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to save license.\n" + ex.Message);
            }
        }

        private string GenerateLicenseName()
        {
            string licenseName = string.Empty;
            try
            {
                if (!String.IsNullOrEmpty(this.MainWindow.Organization.SelectedOrganization.Reseller))
                {
                    licenseName += this.MainWindow.Organization.SelectedOrganization.Reseller + "_";
                }
                if (!String.IsNullOrEmpty(this.MainWindow.Organization.SelectedOrganization.State))
                {
                    licenseName += this.MainWindow.Organization.SelectedOrganization.State + "_";
                }
                licenseName += this.MainWindow.Organization.SelectedOrganization.CustomerName + "_";
                if (this.PackageHeaders.Where(m => m.Name.ToUpper(CultureInfo.InvariantCulture).Contains("FLEX INDEX") && m.PackagesChecked).Any())
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
                if (this.GetSelectedPackages().Where(m => m.Name.ToUpper(CultureInfo.InvariantCulture).Contains("SERVER")).Any())
                {
                    licenseName += "Server_";
                }
                else
                {
                    licenseName += "Client_";
                }
                licenseName += this.SelectedLicense.RestrictByDiskSerialNumber
                    ? this.SelectedLicense.MachineName + "_"
                    : "Universal_";
                licenseName += this.SelectedLicense.IsPermanent
                    ? "Full"
                    : this.SelectedLicense.ExpiresOn != null
                        ? ((DateTime)this.SelectedLicense.ExpiresOn).Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                        : "Invalid Date";
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to generate license name.\n" + ex.Message);
            }
            
            return licenseName;
        }

        /// <summary>
        /// Handles the cloning of a license.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloneButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                this.ConfigureNavigationOption(LicenseNavigationOptions.CloneLicense);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to clone license.\n" + ex.Message);
            }
        }

        /// <summary>
        /// Handles the logic for making a license permanent/expiring.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsPermanent_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                this.ExpiresOn.IsEnabled = !(bool)this.IsPermanent.IsChecked;
                if ((bool)this.IsPermanent.IsChecked)
                {
                    this.ExpiresOn.SelectedDate = null;
                    this.PayRoyalties.IsEnabled = true;
                    this.SelectedLicense.PayRoyalties = this.MainWindow.Organization.SelectedOrganization.CustomerName.Equals("Extract Systems", StringComparison.InvariantCultureIgnoreCase) ? false : true;
                }
                else
                {
                    this.PayRoyalties.IsEnabled = false;
                    this.SelectedLicense.PayRoyalties = false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to make license permanent.\n" + ex.Message);
            }
        }

        /// <summary>
        /// Updates the packages depending on the version selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExtractVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                using var databaseReader = new DatabaseReader();
                this.PackageHeaders = databaseReader.ReadPackages(this.SelectedLicense.ExtractVersion);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to read versions from db. \n" + ex.Message);
            }
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
            foreach (var packageheader in this.PackageHeaders)
            {
                foreach (var package in packageheader.Packages)
                {
                    if (package.IsChecked)
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
            newLicense.IsPermanent = this.SelectedLicense.IsPermanent;
            newLicense.ExpiresOn = this.SelectedLicense.ExpiresOn;
            newLicense.RequestKey = this.SelectedLicense.RequestKey;
            newLicense.IsActive = this.SelectedLicense.IsActive;
            newLicense.Comments = this.SelectedLicense.Comments;
            newLicense.IsProduction = this.SelectedLicense.IsProduction;
            newLicense.SignedTransferForm = this.SelectedLicense.SignedTransferForm;
            newLicense.SDKPassword = this.SelectedLicense.SDKPassword;
            newLicense.ExtractVersion = this.SelectedLicense.ExtractVersion;
            this.SelectedLicense = newLicense;

            using var databaseReader = new DatabaseReader();
            this.PackageHeaders = databaseReader.ReadPackages(newLicense.ExtractVersion);

            this.CopyVersionCheckmarks();
        }

        /// <summary>
        /// When cloning a license, check all of the packages that have the same package header and name.
        /// </summary>
        private void CopyVersionCheckmarks()
        {
            foreach (var clonedPackageHeader in this.ClonedPackageHeaders)
            {
                var packagesToCheck = this.PackageHeaders
                    .Where(header => header.Name.Equals(clonedPackageHeader.Name, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(x => x.Packages);
                foreach(var package in packagesToCheck)
                {
                    if (clonedPackageHeader.Packages.Where(clonedPackage => clonedPackage.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase)).Any())
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
        /// Generates a request key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateRequestKey_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedLicense.RequestKey = LicenseInfo.GenerateUserString();
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
                    fileDialog.DefaultFileName = this.SelectedLicense.LicenseName + @".lic";
                    var fileDialogResult = fileDialog.ShowDialog();
                    if (fileDialogResult == CommonFileDialogResult.Ok)
                    {
                        this.SelectedLicense.GenerateLicenseFile(fileDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save the license to a file. Make sure you use a valid file name and path.\n" + ex.Message);
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
                var fileName = Path.Combine(Path.GetTempPath(), SelectedLicense.LicenseName + ".lic");
                this.SelectedLicense.GenerateLicenseFile(fileName);
                this.CopyFileToClipboard(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to copy license to clipboard\n" + ex.Message);
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
                        this.SelectedLicense.GenerateUnlockCode(this.MainWindow.Organization.SelectedOrganization, fileDialog.FileName + @"\");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save unlock code to file. Ensure you use a valid file path\n" + ex.Message);
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
                this.SelectedLicense.GenerateUnlockCode(this.MainWindow.Organization.SelectedOrganization, tempFolder);
                this.CopyFileToClipboard(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to copy unlock code to clipboard.\n" + ex.Message);
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
            catch (Exception ex)
            {
                MessageBox.Show("Unable to copy file to clipboard.\n" + ex.Message);
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
                        this.PackageHeaders = databaseReader.ReadPackages(this.SelectedLicense);
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
                    this.PayRoyalties.IsEnabled = (bool)this.SelectedLicense.IsPermanent ? true: false;
                    this.Comments.IsEnabled = true;
                    this.SignedTransferForm.IsEnabled = true;
                    this.IsActive.IsEnabled = true;
                    this.IsProduction.IsEnabled = true;
                    this.LicenseName.IsEnabled = true;
                    this.GenerateLicenseNameButton.Visibility = System.Windows.Visibility.Visible;
                    this.UpdateButton.Visibility = System.Windows.Visibility.Visible;
                    this.TransferLicense.Visibility = System.Windows.Visibility.Visible;
                    this.UpgradedLicense.Visibility = System.Windows.Visibility.Visible;
                    break;
                case LicenseNavigationOptions.CloneLicense:
                    this.CloneLabel.Visibility = System.Windows.Visibility.Visible;
                    this.ClonedPackageSelector.Visibility = System.Windows.Visibility.Visible;
                    goto case LicenseNavigationOptions.NewLicense;
                case LicenseNavigationOptions.NewLicense:
                    this.PayRoyalties.IsEnabled = true;
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
                    this.TransferLicense.Visibility = System.Windows.Visibility.Visible;
                    this.UpgradedLicense.Visibility = System.Windows.Visibility.Visible;
                    break;
                case LicenseNavigationOptions.ViewLicense:
                    this.CloneButton.Visibility = System.Windows.Visibility.Visible;
                    this.SaveLicenseToFile.Visibility = System.Windows.Visibility.Visible;
                    this.CopyLicenseToClipboard.Visibility = System.Windows.Visibility.Visible;
                    this.SaveUnlockCodeToFile.Visibility = this.SelectedLicense.IsPermanent ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    this.CopyUnlockCodeToClipboard.Visibility = this.SelectedLicense.IsPermanent ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
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
            this.SelectedLicense.LicenseName = GenerateLicenseName();
        }

        private void EditLicense_Click(object sender, RoutedEventArgs e)
        {
            this.ConfigureNavigationOption(LicenseNavigationOptions.EditLicense);
        }

        private void DisableAllControls()
        {
            this.PayRoyalties.IsEnabled = false;
            this.ExtractVersion.IsEnabled = false;
            this.RequestKey.IsEnabled = false;
            this.IsPermanent.IsEnabled = false;
            this.ExpiresOn.IsEnabled = false;
            this.IssuedOn.IsEnabled = false;
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
            this.TransferLicense.Visibility = System.Windows.Visibility.Collapsed;
            this.UpgradedLicense.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var databaseWriter = new DatabaseWriter();
                databaseWriter.WriteLicense(this.MainWindow.Organization.SelectedOrganization, this.SelectedLicense);
                this.ConfigureNavigationOption(LicenseNavigationOptions.ViewLicense);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to update license.\n" + ex.Message);
            }
        }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
        }

        public void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        public void OnNavigatedTo(NavigationEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(LoadLicense));
            thread.Start();
        }

        /// <summary>
        /// This is needed because on first load the page has not loaded. Therefore
        /// any modifications to controls does not take (buttons wont be hidden/disabled etc).
        /// </summary>
        private void LoadLicense()
        {
            Thread.Sleep(200);
            this.Dispatcher.Invoke(() =>
            {
                this.SelectedLicense = this.MainWindow.LicenseContainer.License;
                this.ConfigureNavigationOption(this.MainWindow.LicenseContainer.LicenseNavigationOption);
            });
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
        }

        /// <summary>
        /// Opens the transfer license dialog, and if a license
        /// is selected obtains its guid, otherwise nullfies the field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TransferLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SelectedLicense.TransferLicense = this.CreateLinkedLicenseDialog("Transfer License");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to link a license.\n" + ex.Message);
            }
        }

        private void UpgradedLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SelectedLicense.UpgradedLicense = this.CreateLinkedLicenseDialog("Upgraded License");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to link a license.\n" + ex.Message);
            }
        }

        private ExtractLicense CreateLinkedLicenseDialog(string title)
        {
            var modernDialog = new ModernDialog
            {
                Title = title,
                ResizeMode = ResizeMode.CanResize,
                Width = 1250,
                MaxWidth = 1250,
            };

            var transferLicense = new LinkedLicense(this.MainWindow.Organization.SelectedOrganization, modernDialog);

            modernDialog.Content = transferLicense;
            modernDialog.Buttons = new Button[] { modernDialog.CancelButton };
            modernDialog.ShowDialog();

            if (modernDialog.MessageBoxResult.Equals(MessageBoxResult.Cancel))
            {
                return null;
            }
            else
            {
                return transferLicense.License;
            }
        }
    }
}
