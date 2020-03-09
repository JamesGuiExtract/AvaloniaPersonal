using DatabaseMigrationWizard.Database;
using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class Home : UserControl, INotifyPropertyChanged
    {
        public ConnectionInformation ConnectionInformation { get; set; }

        public Collection<string> DatabaseNames { get; } = new Collection<string>();

        private bool keepUpdatingDBStatus = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow MainWindow { get; set; }

        public Home()
        {
            InitializeComponent();
            this.Loaded += Home_Loaded;
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            this.ConnectionInformation = this.MainWindow.ConnectionInformation;
            this.DataContext = this;
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

        /// <summary>
        /// Switches the status on the main page from red to green if there is a valid connection
        /// Checks every 5 seconds.
        /// </summary>
        private void UpdateDatabaseStatus()
        {
            try
            {
                while (keepUpdatingDBStatus)
                {
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        this.ConnectionInformation.DatabaseName = this.DatabaseNameTextBox.Text;
                    });

                    App.Current.Dispatcher.Invoke(delegate
                    {
                        this.DatabaseServerStatus.Background = Universal.IsDBServerValid(this.ConnectionInformation) ?  Brushes.Green : Brushes.Red;
                    });

                    App.Current.Dispatcher.Invoke(delegate
                    {
                        this.DatabaseNameStatus.Background = Universal.IsDBDatabaseValid(this.ConnectionInformation) ? Brushes.Green : Brushes.Red;
                    });

                    Thread.Sleep(5000);
                }
            }
            catch (Exception) { }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var mainWindow = ((MainWindow)Application.Current.MainWindow);
            this.ConnectionInformation.DatabaseName = this.DatabaseNameTextBox.Text;
            var fileProcessingDb = new FileProcessingDB()
            {
                DatabaseServer = ConnectionInformation.DatabaseServer,
                DatabaseName = ConnectionInformation.DatabaseName
            };
            try
            {
                fileProcessingDb.LoginUser("admin", PasswordBox.Password);
                if(!mainWindow.MainLinks.Links.Where(link => link.DisplayName.Equals("import") || link.DisplayName.Equals("export")).Any())
                {
                    mainWindow.MainLinks.Links.Add(new Link() { DisplayName = "import", Source = new Uri("/Pages/Import.xaml", UriKind.Relative) });
                    mainWindow.MainLinks.Links.Add(new Link() { DisplayName = "export", Source = new Uri("/Pages/Export.xaml", UriKind.Relative) });
                }

                this.PasswordStatus.Background = Brushes.Green;
                PasswordBox.Password = "";
            }
            catch(Exception)
            {
                MessageBox.Show("Invalid password, or the connection is invalid (red box up above). Please try again.");
            }
        }

        private void ConnectionInformationChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UpdateConnectionInformation(false);
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49731").Display();
            }
        }

        private void UpdateConnectionInformation(bool updateDatabaseNames)
        {
            var mainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            var linksToRemove = mainWindow.MainLinks.Links.Where(link => link.DisplayName.Equals("import") || link.DisplayName.Equals("export")).ToList();
            this.PasswordStatus.Background = Brushes.Red;
            foreach (Link link in linksToRemove)
            {
                mainWindow.MainLinks.Links.Remove(link);
            }
            if(updateDatabaseNames)
            {
                this.DatabaseNames.ToList().ForEach(databaseName => this.DatabaseNames.Remove(databaseName));
                Universal.GetDatabaseNames(this.ConnectionInformation).ToList().ForEach(databaseName => this.DatabaseNames.Add(databaseName));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DatabaseNames"));
            }
        }

        private void ConnectionInformationChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateConnectionInformation(true);
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49732").Display();
            }
        }
    }
}
