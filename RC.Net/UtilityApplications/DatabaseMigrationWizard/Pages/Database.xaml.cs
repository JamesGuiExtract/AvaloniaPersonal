using DatabaseMigrationWizard.Database;
using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UCLID_FILEPROCESSINGLib;

namespace DatabaseMigrationWizard.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl, INotifyPropertyChanged
    {
        public ConnectionInformation ConnectionInformation { get; set; }

        public ObservableCollection<string> DatabaseNames { get; } = new ObservableCollection<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow MainWindow { get; set; }

        private bool hasLoadedOnce = false;

        public Home()
        {
            InitializeComponent();
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            this.ConnectionInformation = this.MainWindow.ConnectionInformation;
            this.DataContext = this;
            this.Loaded += Home_Loaded;
        }

        private async void Home_Loaded(object sender, RoutedEventArgs e)
        {
            // For some reason loaded fires multiple times. I could not find a more approperiate event though.
            if(!hasLoadedOnce)
            {
                hasLoadedOnce = true;
                await this.RemoveTabsAndUpdateStatusIcons(false);
            }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var fileProcessingDb = new FileProcessingDB()
            {
                DatabaseServer = ConnectionInformation.DatabaseServer,
                DatabaseName = ConnectionInformation.DatabaseName
            };
            try
            {
                fileProcessingDb.LoginUser("admin", PasswordBox.Password);
                if(!this.MainWindow.MainLinks.Links.Where(link => link.DisplayName.Equals("import") || link.DisplayName.Equals("export")).Any())
                {
                    this.MainWindow.MainLinks.Links.Add(new Link() { DisplayName = "import", Source = new Uri("/Pages/Import.xaml", UriKind.Relative) });
                    this.MainWindow.MainLinks.Links.Add(new Link() { DisplayName = "export", Source = new Uri("/Pages/Export.xaml", UriKind.Relative) });
                    this.MainWindow.MainLinks.Links.Add(new Link() { DisplayName = "report", Source = new Uri("/Pages/ReportWindow.xaml", UriKind.Relative) });
                }

                this.PasswordStatus.Background = Brushes.Green;
                this.PasswordBox.Password = "";
            }
            catch(Exception)
            {
                MessageBox.Show("Invalid password, or the connection is invalid (red box up above). Please try again.");
            }
        }

        private async void DatabaseServerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                await RemoveTabsAndUpdateStatusIcons(true);
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI49731").Display();
            }
        }

        private async void DatabaseNameTextBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {              
                await RemoveTabsAndUpdateStatusIcons(false);
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI49732").Display();
            }
        }

        private async void DatabaseNameComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                await RemoveTabsAndUpdateStatusIcons(false);
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI49780").Display();
            }
        }

        private async Task RemoveTabsAndUpdateStatusIcons(bool updateDatabaseNames)
        {
            this.MainWindow.MainLinks.Links.Where(link => !link.DisplayName.Equals("Database")).ToList()
                                            .ForEach(link => this.MainWindow.MainLinks.Links.Remove(link));

            this.PasswordStatus.Background = Brushes.Red;
            this.PasswordBox.Password = string.Empty;

            bool validServerName = false;
            bool validDatabaseName = false;
            await Task.Run(new Action(() =>
            {
                validServerName = Universal.IsDBServerValid(this.ConnectionInformation);
                validDatabaseName = Universal.IsDBDatabaseValid(this.ConnectionInformation);
            }));

            this.DatabaseNameStatus.Background = validDatabaseName ? Brushes.Green : Brushes.Red;
            this.DatabaseServerStatus.Background = validServerName ? Brushes.Green : Brushes.Red;
            if(updateDatabaseNames && !string.IsNullOrEmpty(this.ConnectionInformation.DatabaseServer))
            {
                this.DatabaseNames.ToList().ForEach(databaseName => this.DatabaseNames.Remove(databaseName));
                if (validServerName)
                {
                    Universal.GetDatabaseNames(this.ConnectionInformation).ToList().ForEach(databaseName => this.DatabaseNames.Add(databaseName));
                }
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DatabaseNames"));
            }
        }
    }
}
