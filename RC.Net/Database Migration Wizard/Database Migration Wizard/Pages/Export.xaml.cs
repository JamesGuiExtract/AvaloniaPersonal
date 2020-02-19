using DatabaseMigrationWizard.Database.Output;
using DatabaseMigrationWizard.Pages.Utility;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace DatabaseMigrationWizard.Pages
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public ExportOptions ExportOptions { get; set; } = new ExportOptions();

        public ObservableCollection<string> Processing { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> Completed { get; } = new ObservableCollection<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public Export()
        {
            this.ExportOptions.ConnectionInformation = ((MainWindow)System.Windows.Application.Current.MainWindow).ConnectionInformation;
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// Handles the Export button event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            ExportHelper.BeginExport(this.ExportOptions, GetProgressTracker());
        }

        /// <summary>
        /// Opens a windows folder browser dialog to select the export path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = Universal.SelectFolder();
            this.ExportOptions.ExportPath = String.IsNullOrEmpty(selectedFolder) ? this.ExportOptions.ExportPath : selectedFolder;
        }

        /// <summary>
        /// Opens the folder the export path is currently pointed to.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderBrowser_Click(object sender, RoutedEventArgs e)
        {
            Universal.BrowseToFolder(this.ExportOptions.ExportPath);
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
