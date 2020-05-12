using DatabaseMigrationWizard.Database.Input;
using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace DatabaseMigrationWizard.Pages
{
    /// <summary>
    /// Interaction logic for Import.xaml
    /// </summary>
    public partial class Import : System.Windows.Controls.UserControl
    {
        /// <summary>
        /// Various options the user can set while importing.
        /// </summary>
        public ImportOptions ImportOptions { get; set; } = new ImportOptions();

        /// <summary>
        /// Represents the tables that are currently processing.
        /// </summary>
        public ObservableCollection<string> Processing { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Represents the tables that have been successfully imported, but note they have NOT necessarily been commited.
        /// </summary>
        public ObservableCollection<string> Completed { get; } = new ObservableCollection<string>();

        /// <summary>
        /// The main window of the application. See MainWindow.xaml.cs
        /// </summary>
        public MainWindow MainWindow { get; set; }

        /// <summary>
        /// The core logic of this application is housed in the import helper.
        /// This is used for running the import, commit the transacitons, and rolling back.
        /// </summary>
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

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            this.ImportHelper = new ImportHelper(this.ImportOptions, GetProgressTracker());
            try
            {
                this.MainWindow.Reporting.Clear();
                this.Processing.Clear();
                this.Completed.Clear();
                this.MainWindow.UIEnabled = false;
                this.MainWindow.CommitSuccessful = false;
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

                ExecuteImport();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI49725").Display();
            }
        }

        /// <summary>
        /// Commits the transaction, and enables the UI.
        /// </summary>
        public void CommitTransaction()
        {
            try
            {
                this.ImportHelper?.CommitTransaction();
                this.MainWindow.UIEnabled = true;
                this.ImportHelper?.Dispose();
                this.MainWindow.CommitSuccessful = true;
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49812");
            }
        }

        /// <summary>
        /// Rolls back a transaction, and enables the UI.
        /// </summary>
        public void RollbackTransaction()
        {
            try
            {
                this.ImportHelper?.RollbackTransaction();
                this.MainWindow.UIEnabled = true;
                this.ImportHelper?.Dispose();
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49813");
            }
        }

        /// <summary>
        /// Executes the import, and checks for warnings/errors
        /// If there are warnings/errors direct the user to the report tab
        /// Otherwise commit the transaction.
        /// </summary>
        private async void ExecuteImport()
        {
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
                    ex.ExtractDisplay("ELI49751");
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        this.MainWindow.UIEnabled = true;
                        try
                        {
                            this.RollbackTransaction();
                        }
                        // Consumed because the object could already be disposed of.
                        catch (Exception) { }
                    });
                }
            });
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

        /// <summary>
        /// Easy way to track the progress of the import.
        /// This is a two way binding from deep within the import call to the UI.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Switch to the report tab, and commit the transaction if there are no errors/warnings.
        /// </summary>
        /// <param name="startTime">The start time of the import.</param>
        private void CheckForWarningsOrErrors(DateTime startTime)
        {
            PopulateReportingInformation(startTime);

            // If there are no errors/warnings just commit the transaction.
            if (!this.MainWindow.Reporting.Where(m => m.Classification.Equals("Warning") || m.Classification.Equals("Error")).Any())
            {
                this.CommitTransaction();
                this.MainWindow.ReportWindow?.UpdateCommitStatusMessage();
                
                if (this.MainWindow.ReportWindow != null)
                {
                    this.MainWindow.ReportWindow.CommitPrompt.Visibility = Visibility.Hidden;
                }
            }
            else if(this.MainWindow.ReportWindow != null)
            {
                this.MainWindow.ReportWindow.CommitPrompt.Visibility = Visibility.Visible;
                this.MainWindow.ReportWindow.CommitStatus.Visibility = Visibility.Hidden;
            }

            this.MainWindow.ReportWindow?.SetDefaultFilters();
            this.MainWindow.ReportWindow?.SetButtonNumberCount();

            // Switch to the reporting tab.
            ButtonAutomationPeer peer = new ButtonAutomationPeer(HiddenReportButton);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
        }

        /// <summary>
        /// If we are not in the middle of an import, the dispose of the import helper.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Import_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.MainWindow.UIEnabled)
            {
                this.ImportHelper?.Dispose();
            }
        }

        /// <summary>
        /// Obtains reporting information from the ReportingDatabaseMigrationWizard table, filtered by the start time of the import
        /// </summary>
        /// <param name="startTime"></param>
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
                        TableName = reader.GetString(reader.GetOrdinal("TableName")),
                        Command = reader.GetValue(reader.GetOrdinal("Command")).ToString(),
                        New_Value = reader.GetValue(reader.GetOrdinal("New_Value")).ToString(),
                        Old_Value = reader.GetValue(reader.GetOrdinal("Old_Value")).ToString()
                    });
                }
            }

            foreach (Report report in reports)
            {
                this.MainWindow.Reporting.Add(report);
            }
        }
    }
}
