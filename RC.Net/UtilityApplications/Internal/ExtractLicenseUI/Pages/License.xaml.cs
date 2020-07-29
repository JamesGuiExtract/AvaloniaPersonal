﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
                this.NotifyPropertyChanged("PackageHeaders");
            } 
        }

        public ObservableCollection<PackageHeader> ClonedPackageHeaders
        {
            get { return this._ClonedPackageHeaders; }
            set
            {
                _ClonedPackageHeaders = value;
                this.NotifyPropertyChanged("ClonedPackageHeaders");
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
                this.NotifyPropertyChanged("ExtractVersions");
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
                this.NotifyPropertyChanged("SelectedOrganization");
            }
        }

        public MainWindow MainWindow { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public License()
        {
            this.ExtractVersions = new DatabaseReader().ReadVersions();
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
                this.SelectedOrganization.SelectedLicense.GenerateNewLicenseKey(this.SelectedOrganization, this.GetSelectedPackages());
                new DatabaseWriter().WriteLicense(this.SelectedOrganization);
                new DatabaseWriter().WritePackages(this.SelectedOrganization.SelectedLicense, this.GetSelectedPackages());
                this.MainWindow.OrganizationWindow.RefreshLicenses();
                this.ConfigureNavigationOption(LicenseNavigationOptions.ViewLicense);
            }
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
            this.PackageHeaders = new DatabaseReader().ReadPackages(this.SelectedOrganization.SelectedLicense.ExtractVersion);
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

            this.PackageHeaders = new DatabaseReader().ReadPackages(newLicense.ExtractVersion);
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

        /// <summary>
        /// Copies the active license to the clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyLicenseToClipboard_Click(object sender, RoutedEventArgs e)
        {
            var appdataFolder = CreateAndGetTempFolder();
            this.SelectedOrganization.SelectedLicense.GenerateLicenseFile(appdataFolder);
            this.CopyFileToClipboard(appdataFolder + @"\" + this.SelectedOrganization.SelectedLicense.LicenseName + ".lic");
        }

        /// <summary>
        /// Allows the user to save the unlock code to a file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveUnlockCodeToFile_Click(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// Copies the unlock code to the clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyUnlockCodeToClipboard_Click(object sender, RoutedEventArgs e)
        {
            var appdataFolder = CreateAndGetTempFolder();
            this.SelectedOrganization.SelectedLicense.GenerateUnlockCode(this.SelectedOrganization, appdataFolder);
            this.CopyFileToClipboard(appdataFolder + @"\Extract_UnlockLicense.txt");
        }

        /// <summary>
        /// Reads the appdata folder, and creates a temp file.
        /// </summary>
        /// <returns>A file path to appdata\Extract_systems\Temp</returns>
        private string CreateAndGetTempFolder()
        {
            var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Extract_Systems\temp\";
            System.IO.Directory.CreateDirectory(appdataFolder);
            return appdataFolder;
        }

        /// <summary>
        /// Copies the file to the clipboard.
        /// </summary>
        /// <param name="filePath"></param>
        private void CopyFileToClipboard(string filePath)
        {
            var stringCollection = new System.Collections.Specialized.StringCollection();
            stringCollection.Add(filePath);
            Clipboard.SetFileDropList(stringCollection);
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
                    this.SelectedOrganization.SelectedLicense.AllowErrorValidation = true;
                    break;
                case LicenseNavigationOptions.ViewLicense:
                    this.PackageHeaders = new DatabaseReader().ReadPackages(this.SelectedOrganization.SelectedLicense);
                    this.SelectedOrganization.SelectedLicense.AllowErrorValidation = false;
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
            switch (option)
            {
                case LicenseNavigationOptions.EditLicense: break;
                case LicenseNavigationOptions.CloneLicense:
                    this.ClonedPackageSelector.Visibility = System.Windows.Visibility.Visible;
                    goto case LicenseNavigationOptions.NewLicense;
                case LicenseNavigationOptions.NewLicense:
                    this.LicenseName.IsEnabled = true;
                    this.RequestKey.IsEnabled = true;
                    this.IsPermanent.IsEnabled = true;
                    this.ExpiresOn.IsEnabled = true;
                    this.IsActive.IsEnabled = true;
                    this.Comments.IsEnabled = true;
                    this.IsProduction.IsEnabled = true;
                    this.SignedTransferForm.IsEnabled = true;
                    this.ExtractVersion.IsEnabled = true;
                    this.SaveButton.Visibility = System.Windows.Visibility.Visible;
                    this.GenerateRequestKey.Visibility = System.Windows.Visibility.Visible;
                    this.CloneButton.Visibility = System.Windows.Visibility.Collapsed;
                    this.SaveLicenseToFile.Visibility = System.Windows.Visibility.Collapsed;
                    this.CopyLicenseToClipboard.Visibility = System.Windows.Visibility.Collapsed;
                    this.SaveUnlockCodeToFile.Visibility = System.Windows.Visibility.Collapsed;
                    this.CopyUnlockCodeToClipboard.Visibility = System.Windows.Visibility.Collapsed;
                    this.UseDiskSerialNumber.IsEnabled = true;
                    break;
                case LicenseNavigationOptions.ViewLicense:
                    this.LicenseName.IsEnabled = false;
                    this.RequestKey.IsEnabled = false;
                    this.GenerateRequestKey.Visibility = System.Windows.Visibility.Collapsed;
                    this.IsPermanent.IsEnabled = false;
                    this.ExpiresOn.IsEnabled = false;
                    this.IsActive.IsEnabled = false;
                    this.Comments.IsEnabled = false;
                    this.IsProduction.IsEnabled = false;
                    this.SignedTransferForm.IsEnabled = false;
                    this.ExtractVersion.IsEnabled = false;
                    this.SaveButton.Visibility = System.Windows.Visibility.Collapsed;
                    this.CloneButton.Visibility = System.Windows.Visibility.Visible;
                    this.SaveLicenseToFile.Visibility = System.Windows.Visibility.Visible;
                    this.CopyLicenseToClipboard.Visibility = System.Windows.Visibility.Visible;
                    this.SaveUnlockCodeToFile.Visibility = System.Windows.Visibility.Visible;
                    this.CopyUnlockCodeToClipboard.Visibility = System.Windows.Visibility.Visible;
                    this.SaveUnlockCodeToFile.IsEnabled = this.SelectedOrganization.SelectedLicense.IsPermanent ? false : true;
                    this.CopyUnlockCodeToClipboard.IsEnabled = this.SelectedOrganization.SelectedLicense.IsPermanent ? false : true;
                    this.UseDiskSerialNumber.IsEnabled = false;
                    this.ClonedPackageHeaders.Clear();
                    this.ClonedPackageSelector.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case LicenseNavigationOptions.None: break;
            }

        }
    }
}
