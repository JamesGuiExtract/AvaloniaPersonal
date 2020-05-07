using DatabaseMigrationWizard.Database.Input;
using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using FirstFloor.ModernUI.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
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

        private ImportHelper ImportHelper { get; set; }

        public Import()
        {
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            this.ImportOptions.ConnectionInformation = this.MainWindow.ConnectionInformation;
            InitializeComponent();
            this.DataContext = this;
            this.MainWindow.Closing += Import_Closing;
            this.MainWindow.Import = this;
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            this.ImportHelper = new ImportHelper(this.ImportOptions, GetProgressTracker());
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
                    var startTime = DateTime.Now;
                    try
                    {
                        this.ImportHelper.Import();
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            this.CheckForWarningsOrErrors(startTime);
                        });
                    }
                    catch (Exception ex)
                    {
                        throw ex.AsExtract("ELI49751");
                    }
                });
            }
            catch (Exception ex)
            {
                this.RollbackTransaction();
                ex.AsExtract("ELI49725").Display();
            }
        }

        public void CommitTransaction()
        {
            this.ImportHelper.CommitTransaction();
            this.MainWindow.UIEnabled = true;
            this.ImportHelper.Dispose();
        }

        public void RollbackTransaction()
        {
            this.ImportHelper.RollbackTransaction();
            this.MainWindow.UIEnabled = true;
            this.ImportHelper.Dispose();
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

        private void CheckForWarningsOrErrors(DateTime startTime)
        {
            PopulateReportingInformation(startTime);

            if (this.MainWindow.Reporting.Where(m => m.Classification.Equals("Warning") || m.Classification.Equals("Error")).Any())
            {
                // Switch to the reporting tab.
                ButtonAutomationPeer peer = new ButtonAutomationPeer(HiddenReportButton);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
                if(this.MainWindow.ReportWindow != null)
                {
                    this.MainWindow.ReportWindow.CommitPrompt.Visibility = Visibility.Visible;
                }
            }
            else
            {
                this.CommitTransaction();
            }
        }

        private void Import_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.MainWindow.UIEnabled && this.ImportHelper != null)
            {
                this.ImportHelper.Dispose();
            }
        }

        private void PopulateReportingInformation(DateTime startTime)
        {
            var command = this.ImportOptions.SqlConnection.CreateCommand();
            command.CommandText = $"SELECT * FROM dbo.[ReportingDatabaseMigrationWizard] WHERE DateTime > CONVERT(datetime, '{startTime.ToString(CultureInfo.InvariantCulture)}', 101)";
            command.Transaction = this.ImportOptions.Transaction;
            var reports = new List<Report>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    reports.Add(new Report()
                    {
                        Classification = reader.GetString(reader.GetOrdinal("Classification")),
                        Message = reader.GetString(reader.GetOrdinal("Message")),
                        DateTime = reader.GetDateTime(reader.GetOrdinal("DateTime")),
                        TableName = reader.GetString(reader.GetOrdinal("TableName"))
                    });
                }
            }

            this.MainWindow.Reporting.Clear();
            foreach (Report report in reports)
            {
                this.MainWindow.Reporting.Add(report);
            }
        }
    }
}
