using DatabaseMigrationWizard.Database;
using DatabaseMigrationWizard.Database.Output;
using DatabaseMigrationWizard.Pages;
using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using Extract.Licensing;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DatabaseMigrationWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow, INotifyPropertyChanged
    {
        public ConnectionInformation ConnectionInformation { get; set; }

        public ExportOptions ExportOptions { get; set; }

        private bool _UIEnabled = true;
        private bool _ShowDatabase = true;
        private bool _ShowImport = true;
        private bool _ShowExport = true;
        private ReportWindow _ReportWindow;

        Link _DatabaseLink = new Link() { DisplayName = "Database", Source = new Uri("/Pages/Database.xaml", UriKind.Relative) };
        Link _ImportLink = new Link() { DisplayName = "Import", Source = new Uri("/Pages/Import.xaml", UriKind.Relative) };
        Link _ExportLink = new Link() { DisplayName = "Export", Source = new Uri("/Pages/Export.xaml", UriKind.Relative) };
        Link _ReportLink = new Link() { DisplayName = "Report", Source = new Uri("/Pages/ReportWindow.xaml", UriKind.Relative) };

        private bool _ImportHasErrorsOrWarnings = false;
        private string _ImportStatusMessage = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler ReportWindowCreated;

        public ObservableCollection<Report> ImportReporting { get; } = new ObservableCollection<Report>();

        public Import Import { get; set; }

        public ReportWindow ReportWindow 
        {
            get
            {
                return this._ReportWindow;
            }
            set
            {
                try
                {
                    if (value != this._ReportWindow)
                    {
                        this._ReportWindow = value;
                        this.ReportWindowCreated?.Invoke(this, new EventArgs());
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI49848");
                }
            }
        }

        public bool UIEnabled
        {
            get
            {
                return this._UIEnabled;
            }
            set
            {
                try
                {
                    if (value != _UIEnabled)
                    {
                        this._UIEnabled = value;
                        this.NotifyPropertyChanged();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI49851");
                }
            }
        }

        public bool ShowDatabase
        {
            get
            {
                return this._ShowDatabase;
            }
            set
            {
                try
                {
                    if (value != _ShowDatabase)
                    {
                        this._ShowDatabase = value;
                        this.NotifyPropertyChanged();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI49852");
                }
            }
        }

        public bool ShowImport
        {
            get
            {
                return this._ShowImport;
            }
            set
            {
                try
                {
                    if (value != _ShowImport)
                    {
                        this._ShowImport = value;
                        this.NotifyPropertyChanged();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI49853");
                }
            }
        }

        public bool ShowExport
        {
            get
            {
                return this._ShowExport;
            }
            set
            {
                try
                {
                    if (value != _ShowExport)
                    {
                        this._ShowExport = value;
                        this.NotifyPropertyChanged();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI49854");
                }
            }
        }

        /// <summary>
        /// Specifies the default path to use for the import/export tabs.
        /// It will be set from either a value passed from command-line arguments (if available)
        /// </summary>
        public string DefaultPath { get; set; }

        public bool ImportHasErrorsOrWarnings
        {
            get
            {
                return this._ImportHasErrorsOrWarnings;
            }
            set
            {
                try
                {
                    if (value != this._ImportHasErrorsOrWarnings)
                    {
                        this._ImportHasErrorsOrWarnings = value;
                        this.NotifyPropertyChanged();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI49849");
                }
            }
        }

        public string ImportStatusMessage
        {
            get
            {
                return this._ImportStatusMessage;
            }
            set
            {
                try
                {
                    if (value != this._ImportStatusMessage)
                    {
                        this._ImportStatusMessage = value;
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImportStatusMessage"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI49850");
                }
            }
        }

        public MainWindow(ConnectionInformation connectionInformation)
        {
            try
            {
                this.MainWindowSetup(connectionInformation);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49729");
            }
        }

        public MainWindow(ConnectionInformation connectionInformation, ExportOptions exportOptions)
        {
            try
            {
                this.MainWindowSetup(connectionInformation);
                this.ExportOptions = exportOptions;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49800");
            }
        }


        /// <summary>
        /// Updates import report data using the specified <see cref="Reports"/> or <c>null</c> to
        /// clear any existing import report data.
        /// </summary>
        internal void ApplyImportReport(IEnumerable<Report> reports)
        {
            this.ImportReporting.Clear();

            foreach (Report report in reports)
            {
                this.ImportReporting.Add(report);
            }

            this.ImportHasErrorsOrWarnings = reports.Any(m =>
                m.Classification.Equals("Warning") || m.Classification.Equals("Error"));

        }

        /// <summary>
        /// Clears out the reporting and removes status messages/links.
        /// </summary>
        internal void ResetImportReporting()
        {
            this.ImportReporting.Clear();
            Import?.ResetProgress();
            this.ImportStatusMessage = "";
            this.ImportHasErrorsOrWarnings = false;
            UpdateLinks();
        }

        /// <summary>
        /// Displays current import reports in the report tab.
        /// </summary>
        /// <param name="promptForCommit"><c>true</c> if the report tab should display a prompt to commit
        /// the import operation (and be responsible for calling commit/rollback according to the response)
        /// or <c>false</c> if the report has already been committed.</param>
        internal void ShowImportReport(bool promptForCommit)
        {
            UpdateLinks();
            this.ContentSource = _ReportLink.Source;
            if (ReportWindow == null)
            {
                // If the report window is not yet created, schedule the report to be displayed as soon as it is.
                this.ReportWindowCreated += (o, e) =>
                {
                    ReportWindow.ShowReport(promptForCommit);
                };
            }
            else
            {
                ReportWindow.ShowReport(promptForCommit);
            }
        }

        /// <summary>
        /// Updates the displayed page links based on the current application state.
        /// </summary>
        private void UpdateLinks()
        {
            int index = 0;

            setLinkVisibility(_DatabaseLink, ShowDatabase);
            setLinkVisibility(_ImportLink, ShowImport && ConnectionInformation.ConnectionInfoValidated);
            setLinkVisibility(_ExportLink, ShowExport && ConnectionInformation.ConnectionInfoValidated);
            setLinkVisibility(_ReportLink, ConnectionInformation.ConnectionInfoValidated
                && (!string.IsNullOrEmpty(ImportStatusMessage) || ImportReporting.Any()));
            
            void setLinkVisibility(Link link, bool makeVisible)
            {
                var exisitingIndex = MainLinks.Links.IndexOf(link);

                if (makeVisible)
                {
                    if (exisitingIndex >= 0)
                    {
                        index = exisitingIndex + 1;
                    }
                    else if (index >= MainLinks.Links.Count)
                    {
                        MainLinks.Links.Add(link);
                        if (this.ContentSource == null)
                        {
                            this.ContentSource = link.Source;
                        }
                        index++;
                    }
                    else
                    {
                        MainLinks.Links.Insert(index++, link);
                    }
                }
                else if (!makeVisible && exisitingIndex >= 0)
                {
                    index = exisitingIndex;
                    MainLinks.Links.Remove(link);
                }
            }
        }

        private void MainWindowSetup(ConnectionInformation connectionInformation)
        {
            this.ConnectionInformation = connectionInformation ?? new ConnectionInformation();
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI49728", typeof(MainWindow).ToString());
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            this.ConnectionInformation.PropertyChanged += ConnectionInfo_PropertyChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateLinks();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49824");
            }
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!UIEnabled)
            {
                e.Cancel = true;
                MessageBox.Show("Closing is not supported while importing/exporting. Please wait for the operation to complete");
            }
        }

        private void ConnectionInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("ConnectionInfoValidated", StringComparison.OrdinalIgnoreCase))
            { 
                if (ConnectionInformation.ConnectionInfoValidated)
                {
                    // TODO: Set DB server/name in status bar here
                }
                else
                {
                    // TODO: Set "Not connected" in status bar here
                    this.ResetImportReporting();
                }

                UpdateLinks();
            }
        }

        /// <summary>
        /// Called by each of the property Set accessors when property changes
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
