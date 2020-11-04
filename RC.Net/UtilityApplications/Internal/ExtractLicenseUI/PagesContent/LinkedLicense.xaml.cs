using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ExtractLicenseUI.PagesContent
{
    /// <summary>
    /// Interaction logic for TransferLicense.xaml
    /// </summary>
    public partial class LinkedLicense : UserControl
    {
        public Database.Organization SelectedOrganization { get; set; }
        private ModernDialog ModernDialog { get; set; }

        public Database.ExtractLicense License { get; set; }

        public LinkedLicense(Database.Organization organization, ModernDialog modernDialog)
        {
            this.SelectedOrganization = organization;
            InitializeComponent();
            this.LicenseGrid.DataContext = this;
            this.ModernDialog = modernDialog;
        }

        private void LinkLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                {
                    if (vis is DataGridRow)
                    {
                        var row = (DataGridRow)vis;
                        this.License = (Database.ExtractLicense)row.Item;
                    }
                }

                ModernDialog.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to link license\n" + ex.Message);
            }
        }
    }
}
