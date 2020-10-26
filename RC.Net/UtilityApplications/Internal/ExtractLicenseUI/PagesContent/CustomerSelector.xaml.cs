using ExtractLicenseUI.Database;
using ExtractLicenseUI.Utility;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ExtractLicenseUI.PagesContent
{
    /// <summary>
    /// Interaction logic for CustomerSelector.xaml
    /// </summary>
    public partial class CustomerSelector : UserControl
    {
        public MainWindow MainWindow { get; set; }

        public CustomerSelector()
        {
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            InitializeComponent();
            this.MainBorder.DataContext = this;
        }

        /// <summary>
        /// Updates selected customer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomerName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedOrg = (Database.Organization)this.CustomerName.SelectedItem;
                this.MainWindow.Organization.SelectedOrganization = selectedOrg;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to update customer.\n" + ex.Message);
            }
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
                System.Diagnostics.Process.Start(this.MainWindow.Organization.SelectedOrganization.SalesforceHyperlink);
            }
            catch (Exception)
            {
                MessageBox.Show($@"Cannot open link to: {this.MainWindow.Organization.SelectedOrganization.SalesforceHyperlink}");
            }
        }
    }
}
