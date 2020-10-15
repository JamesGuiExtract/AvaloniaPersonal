using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExtractLicenseUI.Database;
using System.Linq;
using System;

namespace ExtractLicenseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Contact : UserControl, INotifyPropertyChanged
    {
        private Database.Organization _SelectedOrganization = new Database.Organization();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow MainWindow { get; }

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
            }
        }

        public Contact()
        {
            InitializeComponent();
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            this.Form.DataContext = this;
            this.Loaded += Contact_Loaded;
        }

        private void Contact_Loaded(object sender, RoutedEventArgs e)
        {
            this.MainWindow.ContactWindow = this;
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
        /// Deletes a contact from the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (var row = sender as Visual; row != null; row = VisualTreeHelper.GetParent(row) as Visual)
                {
                    if (row is DataGridRow dataGridRow && dataGridRow.Item is Database.Contact contact) 
                    { 
                        using var databaseWriter = new DatabaseWriter();
                        databaseWriter.DeleteContact(contact);
                        this.SelectedOrganization.Contacts.Remove(contact);
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error deleting a contact from the database. \n" + ex.Message);
            }

        }


        /// <summary>
        /// When a row loses focus update/insert that contact in the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridRow_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!this.SelectedOrganization.Contacts.Where(contact => string.IsNullOrEmpty(contact.FirstName) || string.IsNullOrEmpty(contact.EmailAddress)).Any()
                        && ((DataGridRow)sender).Item.GetType().Equals(typeof(Database.Contact)))
                {
                    var contact = (Database.Contact)((DataGridRow)sender).Item;
                    using var databaseWriter = new DatabaseWriter();
                    databaseWriter.InsertUpdateContact(contact, this.SelectedOrganization);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error inserting or updating contact in the database.\n" + ex.Message);
            }

        }
    }
}
