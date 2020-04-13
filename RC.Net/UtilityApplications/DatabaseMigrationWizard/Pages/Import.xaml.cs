using DatabaseMigrationWizard.Database.Input;
using DatabaseMigrationWizard.Pages.Utility;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace DatabaseMigrationWizard.Pages
{
    /// <summary>
    /// Interaction logic for Import.xaml
    /// </summary>
    public partial class Import : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public ImportOptions ImportOptions { get; set; } = new ImportOptions();

        public ObservableCollection<string> Processing { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> Completed { get; } = new ObservableCollection<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public Import()
        {
            this.ImportOptions.ConnectionInformation = ((MainWindow)System.Windows.Application.Current.MainWindow).ConnectionInformation;
            InitializeComponent();
            this.DataContext = this;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            if(ImportOptions.ClearDatabase)
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

            new ImportHelper(this.ImportOptions, GetProgressTracker()).BeginImport();
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
                if (Processing.Contains(processedItem))
                {
                    Processing.Remove(processedItem);
                    Completed.Add(processedItem);
                }
                else
                {
                    Processing.Add(processedItem);
                }

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Processing"));
            });
        }
    }
}
