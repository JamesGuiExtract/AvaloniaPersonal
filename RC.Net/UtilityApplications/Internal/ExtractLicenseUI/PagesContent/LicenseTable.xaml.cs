using ExtractLicenseUI.Utility;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExtractLicenseUI.PagesContent
{
    /// <summary>
    /// Interaction logic for LicenseTable.xaml
    /// </summary>
    public partial class LicenseTable : UserControl
    {

        public LicenseTable()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens a license from double clicking on the row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var datagridRow = (DataGridRow)sender;
                NavigateToLicense((Database.ExtractLicense)datagridRow.Item, LicenseNavigationOptions.ViewLicense);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to navigate to License page.\n" + ex.Message);
            }
        }

        /// <summary>
        /// Opens a license from clicking the view button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                {
                    if (vis is DataGridRow)
                    {
                        var row = (DataGridRow)vis;
                        NavigateToLicense((Database.ExtractLicense)row.Item, LicenseNavigationOptions.ViewLicense);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to navigate to License page.\n" + ex.Message);
            }
        }

        private void NavigateToLicense(Database.ExtractLicense license, LicenseNavigationOptions licenseNavigationOptions)
        {
            ((MainWindow)Application.Current.MainWindow).LicenseContainer.NavigateToLicense(license, licenseNavigationOptions);
        }

        private void CreateNewLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.NavigateToLicense(new Database.ExtractLicense(), LicenseNavigationOptions.NewLicense);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to navigate to License page.\n" + ex.Message);
            }
        }
    }
}
