using DatabaseMigrationWizard.Database.Input;
using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;

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
            this.ImportOptions.ImportPath = MainWindow.DefaultPath;
            InitializeComponent();
            this.DataContext = this;
            this.ImportPath.Text = MainWindow.DefaultPath;
            this.MainWindow.Closing += Import_Closing;
            this.MainWindow.Import = this;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ImportHelper = new ImportHelper(this.ImportOptions, GetProgressTracker());
                this.MainWindow.ResetImportReporting();
                this.Processing.Clear();
                this.Completed.Clear();
                this.MainWindow.UIEnabled = false;

                this.ExecuteImport();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI49725").Display();
            }
        }

        /// <summary>
        /// Ends the transaction for the current import operation and re-enables the UI.
        /// </summary>
        /// <param name="commit"><c>true</c> to commit the transaction; <c>false</c> to roll back
        /// the transaction (cancel the operation).</param>
        public void EndTransaction(bool commit, string statusMessage = null)
        {
            try
            {
                try
                {
                    if (commit)
                    {
                        this.ImportHelper?.CommitTransaction();
                    }
                    else
                    {
                        this.ImportHelper?.RollbackTransaction();
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI49827");
                    statusMessage = "The import failed";
                }

                this.MainWindow.UIEnabled = true;
                this.ImportHelper?.Dispose();

                if (statusMessage == null)
                {
                    statusMessage = commit
                        ? "The import was successful!"
                        : "The import failed";
                }

                this.MainWindow.ImportStatusMessage = statusMessage;
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49812");
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

                    // Update the saved default path only if different from MainWindow.DefaultPath
                    // (So as not to save an export path specified via command-line param)
                    if (this.ImportOptions.ImportPath != MainWindow.DefaultPath)
                    {
                        Properties.Settings.Default.DefaultPath = this.ImportOptions.ImportPath;
                        Properties.Settings.Default.Save();
                    }

                    this.PopulateReportingInformation(startTime);

                    App.Current.Dispatcher.Invoke(delegate
                    {
                        if (!MainWindow.ImportHasErrorsOrWarnings)
                        {
                            this.EndTransaction(commit: true);
                        }

                        MainWindow.ShowImportReport(promptForCommit: MainWindow.ImportHasErrorsOrWarnings);
                    });
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI49751");

                    try
                    {
                        this.PopulateReportingInformation(startTime);
                    }
                    catch (Exception ee)
                    {
                        ExtractException extractException = ee.AsExtract("ELI50368");
                        extractException.AddDebugData("Reporting", "Unable to populate reporting information. Make sure your database is up to date.");
                        extractException.ExtractDisplay("ELI50369");
                    }

                    App.Current.Dispatcher.Invoke(delegate
                    {
                        try
                        {
                            this.EndTransaction(commit: false);
                            MainWindow.ShowImportReport(promptForCommit: false);
                        }
                        // Consumed because the object could already be disposed of.
                        catch (Exception) { }
                    });
                }
            });
        }

        /// <summary>
        /// Clears any import status related data from previous import operations from the page UI.
        /// </summary>
        public void ResetProgress()
        {
            try
            {
                this.Processing.Clear();
                this.Completed.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49844");
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

            this.MainWindow.ApplyImportReport(reports);
        }

        /// <summary>
        /// Prompts the user validating they want to clear the database. If they do execute it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.MainWindow.UIEnabled = false;
                MessageBoxResult result = MessageBox.Show($"DatabaseServer: {this.ImportOptions.ConnectionInformation.DatabaseServer} \nDatabaseName:{this.ImportOptions.ConnectionInformation.DatabaseName}\nAre you 100% sure you want to clear this database? This action cannot be undone!", "Database Migration Wizard", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Yes:
                        new ImportHelper(this.ImportOptions, GetProgressTracker()).ClearDatabase();
                        MessageBox.Show("Database has been cleared!");
                        break;
                }

                this.MainWindow.UIEnabled = true;
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49882").Display();
            }
        }
    }
}
