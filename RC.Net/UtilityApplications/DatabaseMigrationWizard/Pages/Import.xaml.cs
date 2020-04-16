using DatabaseMigrationWizard.Database.Input;
using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace DatabaseMigrationWizard.Pages
{
    /// <summary>
    /// Interaction logic for Import.xaml
    /// </summary>
    public partial class Import : System.Windows.Controls.UserControl
    {
        public ImportOptions ImportOptions { get; set; } = new ImportOptions();

        public ObservableCollection<string> Processing { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> Completed { get; } = new ObservableCollection<string>();

        public MainWindow MainWindow { get; set; }

        public Import()
        {
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            this.ImportOptions.ConnectionInformation = this.MainWindow.ConnectionInformation;
            InitializeComponent();
            this.DataContext = this;
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Processing.Clear();
                Completed.Clear();
                this.MainWindow.UIEnabled = false;
                if (ImportOptions.ClearDatabase)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show($"DatabaseServer: {this.ImportOptions.ConnectionInformation.DatabaseServer} \nDatabaseName:{this.ImportOptions.ConnectionInformation.DatabaseName}\nAre you 100% sure you want to clear this database? This action cannot be undone!", "Database Migration Wizard", MessageBoxButton.YesNo);
                    switch (result)
                    {
                        case MessageBoxResult.No:
                            return;
                        case MessageBoxResult.Yes:
                            break;
                    }
                }
                await Task.Run(() =>
                {
                    new ImportHelper(this.ImportOptions, GetProgressTracker()).Import();
                });
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI49725").Display();
            }
            finally
            {
                this.MainWindow.UIEnabled = true;
            }
        }

        /// <summary>
        /// Opens a windows folder browser dialog to select the export path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = Universal.SelectFolder();
            this.ImportOptions.ImportPath = String.IsNullOrEmpty(selectedFolder) ? this.ImportOptions.ImportPath : selectedFolder;
        }

        /// <summary>
        /// Opens the folder the export path is currently pointed to.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderBrowser_Click(object sender, RoutedEventArgs e)
        {
            Universal.BrowseToFolder(this.ImportOptions.ImportPath);
        }

        private IProgress<string> GetProgressTracker()
        {
            return new Progress<string>(processedItem =>
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    if (Processing.Contains(processedItem))
                    {
                        Processing.Remove(processedItem);
                        Completed.Add(processedItem);
                    }
                    else
                    {
                        Processing.Add(processedItem);
                    }
                });
            });
        }
    }
}
