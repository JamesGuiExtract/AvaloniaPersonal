using DatabaseMigrationWizard.Database;
using DatabaseMigrationWizard.Pages.Utility;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
    }
}
