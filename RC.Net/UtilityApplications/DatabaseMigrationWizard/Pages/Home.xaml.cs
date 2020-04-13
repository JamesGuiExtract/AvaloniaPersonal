using DatabaseMigrationWizard.Database;
using DatabaseMigrationWizard.Pages.Utility;
using FirstFloor.ModernUI.Presentation;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UCLID_FILEPROCESSINGLib;

namespace DatabaseMigrationWizard.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        ConnectionInformation connectionInformation;

        private bool keepUpdatingDBStatus = true;

        public Home()
        {
            InitializeComponent();
            this.Loaded += Home_Loaded;
            this.connectionInformation = ((MainWindow)System.Windows.Application.Current.MainWindow).ConnectionInformation;
            this.DataContext = this.connectionInformation;
            new Thread(() => 
            { 
                this.UpdateDatabaseStatus(); 
            }).Start();
        }

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Closing += window_Closing;
        }

        private void window_Closing(object sender, global::System.ComponentModel.CancelEventArgs e)
        {
            keepUpdatingDBStatus = false;
        }

        private void UpdateDatabaseStatus()
        {
            try
            {
                while (keepUpdatingDBStatus)
                {
                    if (Universal.IsDBConfigurationValid(this.connectionInformation))
                    {
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            this.DatabaseStatus.Background = Brushes.Green;
                        });
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            this.DatabaseStatus.Background = Brushes.Red;
                        });
                    }
                    Thread.Sleep(5000);
                }
            }
            catch (Exception) { }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var mainWindow = ((MainWindow)Application.Current.MainWindow);
            var fileProcessingDb = new FileProcessingDB()
            {
                DatabaseServer = connectionInformation.DatabaseServer,
                DatabaseName = connectionInformation.DatabaseName
            };
            try
            {
                fileProcessingDb.LoginUser("admin", PasswordBox.Password);
                mainWindow.MainLinks.Links.Add(new Link() { DisplayName = "import", Source = new Uri("/Pages/Import.xaml", UriKind.Relative) });
                mainWindow.MainLinks.Links.Add(new Link() { DisplayName = "export", Source = new Uri("/Pages/Export.xaml", UriKind.Relative) });
                PasswordBox.Password = "";
            }
            catch(Exception)
            {
                MessageBox.Show("Invalid password, or the connection is invalid (red box up above). Please try again.");
            }
        }

        private void ConnectionInformationChanged(object sender, TextChangedEventArgs e)
        {
            var mainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            var linksToRemove = mainWindow.MainLinks.Links.Where(link => link.DisplayName.Equals("import") || link.DisplayName.Equals("export")).ToList();
            foreach(Link link in linksToRemove)
            {
                mainWindow.MainLinks.Links.Remove(link);
            }
        }
    }
}
