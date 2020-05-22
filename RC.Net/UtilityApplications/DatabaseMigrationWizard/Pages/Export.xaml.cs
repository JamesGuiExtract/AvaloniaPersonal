using DatabaseMigrationWizard.Database.Output;
using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace DatabaseMigrationWizard.Pages
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export : System.Windows.Controls.UserControl
    {
        public ExportOptions ExportOptions { get; set; } = new ExportOptions();

        public ObservableCollection<string> Processing { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> Completed { get; } = new ObservableCollection<string>();

        public MainWindow MainWindow { get; set; }

        public Export()
        {
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            this.ExportOptions = MainWindow.ExportOptions ?? new ExportOptions();
            // Just in case the user changes the connection information on the main page
            this.ExportOptions.ConnectionInformation = this.MainWindow.ConnectionInformation;
            this.ExportOptions.ExportPath = MainWindow.DefaultPath;
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// Handles the Export button event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(Directory.GetFiles(this.ExportOptions.ExportPath).Length > 0)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show($"Files were detected in the output directory. Do you want to overwrite these?", "Database Migration Wizard", MessageBoxButton.YesNo);
                    switch (result)
                    {
                        case MessageBoxResult.No:
                            return;
                        case MessageBoxResult.Yes:
                            Array.ForEach(Directory.GetFiles(this.ExportOptions.ExportPath), delegate (string path) { File.Delete(path); });
                            break;
                    }
                }

                Processing.Clear();
                Completed.Clear();
                this.MainWindow.UIEnabled = false;
                await Task.Run(() =>
                {
                    ExportHelper.Export(this.ExportOptions, GetProgressTracker());

                    // Update the saved default path only if different from MainWindow.DefaultPath
                    // (So as not to save an export path specified via command-line param)
                    if (this.ExportOptions.ExportPath != MainWindow.DefaultPath)
                    {
                        Properties.Settings.Default.DefaultPath = this.ExportOptions.ExportPath;
                        Properties.Settings.Default.Save();
                    }
                });
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49735").Display();
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
            try
            {
                string selectedFolder = Universal.SelectFolder();
                this.ExportOptions.ExportPath = String.IsNullOrEmpty(selectedFolder) ? this.ExportOptions.ExportPath : selectedFolder;
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49734").Display();
            }
        }

        /// <summary>
        /// Opens the folder the export path is currently pointed to.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderBrowser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Universal.BrowseToFolder(this.ExportOptions.ExportPath);
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49733").Display();
            }
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
