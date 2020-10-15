using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ExtractLicenseUI.Database;
using System.Data;
using System;
using ExtractLicenseUI.Utility;
using System.Runtime.CompilerServices;
using ExtractLicenseUI.DatFileUtility;
using System.Linq;
using System.Threading;

namespace ExtractLicenseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Organization : UserControl, INotifyPropertyChanged, IDisposable
    {
        private Database.Organization _SelectedOrganization = new Database.Organization();
        private Collection<Database.Organization> _Organizations = new Collection<Database.Organization>();

        public Collection<Database.Organization> Organizations {
            get
            {
                return new Collection<Database.Organization>(this._Organizations.OrderBy(m => m.CustomerName).ToList());
            }
            set
            {
                _Organizations = value;
                this.NotifyPropertyChanged(nameof(Organizations));
            }
        }

        public Database.Organization SelectedOrganization
        {
            get
            {
                return this._SelectedOrganization;
            }
            set
            {
                _SelectedOrganization = value;
                this.NotifyPropertyChanged(nameof(SelectedOrganization));
                SortDataGrid(this.LicenseGrid, 4, ListSortDirection.Descending);
            }
        }

        public MainWindow MainWindow { get; }

        private readonly DatabaseReader DatabaseReader = new DatabaseReader();

        public event PropertyChangedEventHandler PropertyChanged;

        public Organization()
        {
            Thread thread = new Thread(new ThreadStart(PopulateOrganizations));
            thread.Start();
            InitializeComponent();
            this.Form.DataContext = this;
            this.LicenseGrid.DataContext = this;
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            this.MainWindow.OrganizationWindow = this;
        }

        private void PopulateOrganizations()
        {
            try
            {
                using var databaseReader = new DatabaseReader();
                this.Organizations = databaseReader.ReadOrganizations();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to load organizations.\n" + ex.Message);
            }
        }

        /// <summary>
        /// Refreshes the licenses on the main page. (useful if you add a new license).
        /// </summary>
        public void RefreshLicenses()
        {
            this.SelectedOrganization.Licenses = this.DatabaseReader.GetExtractLicenses(this.SelectedOrganization.Guid);
            this.SelectedOrganization.Contacts = this.DatabaseReader.GetOrganizationContacts(this.SelectedOrganization.Guid);
        }

        /// <summary>
        /// Updates selected customer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomerName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedOrg = (Database.Organization)this.CustomerName.SelectedItem;
            this.SelectedOrganization = selectedOrg;
            this.SelectedOrganization.Licenses = this.DatabaseReader.GetExtractLicenses(this.SelectedOrganization.Guid);
            this.SelectedOrganization.Contacts = this.DatabaseReader.GetOrganizationContacts(this.SelectedOrganization.Guid);
            this.CreateNewLicense.IsEnabled = true;
            this.ViewContacts.IsEnabled = true;
            SortDataGrid(this.LicenseGrid, 4, ListSortDirection.Descending);
        }

        /// <summary>
        /// Opens a license from double clicking on the row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var datagridRow = (DataGridRow)sender;
            NavigateToLicense((ExtractLicense)datagridRow.Item, LicenseNavigationOptions.ViewLicense);
        }

        /// <summary>
        /// Opens a license from clicking the view button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewLicense_Click(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    NavigateToLicense((ExtractLicense)row.Item, LicenseNavigationOptions.ViewLicense);
                    break;
                }
            }
        }

        /// <summary>
        /// Navigates to the license page.
        /// </summary>
        /// <param name="extractLicense">The license to operate on</param>
        /// <param name="licenseNavigationOption">The mode to see the license in</param>
        private void NavigateToLicense(ExtractLicense extractLicense, LicenseNavigationOptions licenseNavigationOption)
        {
            this.SelectedOrganization.SelectedLicense = extractLicense;

            MainWindow.NavigateToLicense(this.SelectedOrganization, licenseNavigationOption);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/16956251/sort-a-wpf-datagrid-programmatically
        /// </summary>
        /// <param name="dataGrid">The datagrid to sort</param>
        /// <param name="columnIndex">The index of the column to sort</param>
        /// <param name="sortDirection">Ascending or descending.</param>
        private static void SortDataGrid(DataGrid dataGrid, int columnIndex = 0, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            var column = dataGrid.Columns[columnIndex];

            // Clear current sort descriptions
            dataGrid.Items.SortDescriptions.Clear();

            // Add the new sort description
            dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, sortDirection));

            // Apply sort
            foreach (var col in dataGrid.Columns)
            {
                col.SortDirection = null;
            }
            column.SortDirection = sortDirection;

            // Refresh items to display sort
            dataGrid.Items.Refresh();
        }

        private void CreateNewLicense_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToLicense(new ExtractLicense(), LicenseNavigationOptions.NewLicense);
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
        /// Opens the hyperlink in default browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.SelectedOrganization.SalesforceHyperlink);
            }
            catch(Exception)
            {
                MessageBox.Show($@"Cannot open link to: {this.SelectedOrganization.SalesforceHyperlink}");
            }
        }

        private void ViewContacts_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigateToContact(this.SelectedOrganization);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            this.DatabaseReader.Dispose();
        }
    }
}
