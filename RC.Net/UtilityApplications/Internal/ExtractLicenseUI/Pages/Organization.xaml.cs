using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ExtractLicenseUI.Database;
using System.Data;
using System;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

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
                this.RefreshLicenses();
                this.NotifyPropertyChanged(nameof(SelectedOrganization));
            }
        }

        private readonly DatabaseReader DatabaseReader = new DatabaseReader();

        public event PropertyChangedEventHandler PropertyChanged;

        public Organization()
        {
            Thread thread = new Thread(new ThreadStart(PopulateOrganizations));
            thread.Start();
            InitializeComponent();
            this.Form.DataContext = this;
            ((MainWindow)Application.Current.MainWindow).Organization = this;
        }

        /// <summary>
        /// Reads the organizations from the database, and assigns them to a local property.
        /// </summary>
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

        public void RefreshOrganizations()
        {
            this.PopulateOrganizations();
            if(!this.SelectedOrganization.Guid.Equals(Guid.Empty))
            {
                IEnumerable<Database.Organization> updatedCustomers = this.Organizations.Where(m => m.Guid == this.SelectedOrganization.Guid);
                this.SelectedOrganization = updatedCustomers.First();
            }
        }

        /// <summary>
        /// Refreshes the licenses on the main page. (useful if you add a new license).
        /// </summary>
        public void RefreshLicenses()
        {
            this.SelectedOrganization.Licenses = this.DatabaseReader.GetExtractLicenses(this.SelectedOrganization.Guid);
            this.SelectedOrganization.Contacts = this.DatabaseReader.GetOrganizationContacts(this.SelectedOrganization.Guid);
            this.OrganizationLinks.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Called by each of the property Set accessors when property changes
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
