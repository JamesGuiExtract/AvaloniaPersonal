using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using static System.FormattableString;

namespace DatabaseMigrationWizard.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class ReportWindow : UserControl, INotifyPropertyChanged
    {
        public MainWindow MainWindow { get; set; }

        private bool filterErrors = false;
        private bool filterWarning = false;
        private bool filterInfo = true;

        private int errorCount = 0;
        private int warningCount = 0;
        private int infoCount = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ObservableCollection<Report> FilteredReport = new ObservableCollection<Report>();

        public ReportWindow()
        {
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            InitializeComponent();
            this.DataGrid1.DataContext = this.FilteredReport;
            this.MainWindow.ReportWindow = this;
        }

        public void ShowReport(bool promptForCommit)
        {
            try
            {
                SetDefaultFilters();
                FilterTheReport();
                SetButtonNumberCount();
                UpdateButtonBorders();

                // If there are no errors/warnings just commit the transaction.
                if (promptForCommit)
                {
                    this.CommitPrompt.Visibility = Visibility.Visible;
                    this.CommitStatus.Visibility = Visibility.Hidden;
                }
                else
                {
                    CommitPrompt.Visibility = Visibility.Hidden;
                    ShowStatusMessage();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49826");
            }
        }

        /// <summary>
        /// Called by each of the property Set accessors when property changes
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ImportCommit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                string message = (errorCount > 0)
                    ? Invariant($"Import completed with {errorCount} error(s).")
                    : null;

                MainWindow.Import.EndTransaction(commit: true, message);
                this.CommitPrompt.Visibility = Visibility.Hidden;
                this.ShowStatusMessage();
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49752").Display();
            }
        }

        private void ImportRollback_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                MainWindow.Import.EndTransaction(commit: false, statusMessage: "The import was cancelled");
                this.CommitPrompt.Visibility = Visibility.Hidden;
                this.ShowStatusMessage();
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49753").Display();
            }
        }

        private void ErrorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                filterErrors = !filterErrors;
                FilterTheReport();
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49809").Display();
            }
        }

        private void WarningButton_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                filterWarning = !filterWarning;
                FilterTheReport();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI49810").Display();
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                filterInfo = !filterInfo;
                FilterTheReport();
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49811").Display();
            }
        }

        /// <summary>
        /// Updates the commit status message and makes it visible on the reporting page.
        /// </summary>
        public void ShowStatusMessage()
        {
            try
            {
                this.CommitStatus.Visibility = Visibility.Visible;
                this.CommitStatus.Text = this.MainWindow.ImportStatusMessage;
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49816");
            }
        }

        /// <summary>
        /// Sets the button border blue if activated, grey if not.
        /// </summary>
        private void UpdateButtonBorders()
        {
            this.ErrorButton.BorderBrush = !this.filterErrors ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Gray;
            this.WarningButton.BorderBrush = !this.filterWarning ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Gray;
            this.InfoButton.BorderBrush = !this.filterInfo ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Gray;
        }

        /// <summary>
        /// If there are warnings or errors, display them and filter the information.
        /// If there are no warnings or errors, just show the info
        /// </summary>
        public void SetDefaultFilters()
        {
            try
            {
                if (this.MainWindow.ImportHasErrorsOrWarnings)
                {
                    this.filterErrors = false;
                    this.filterWarning = false;
                    this.filterInfo = true;
                }
                else
                {
                    this.filterErrors = true;
                    this.filterWarning = true;
                    this.filterInfo = false;
                }
                this.FilterTheReport();
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49817");
            }
        }

        /// <summary>
        /// Sets the numbers next to buttons IE: Errors(1337)
        /// </summary>
        public void SetButtonNumberCount()
        {
            try
            {
                errorCount = this.MainWindow.ImportReporting.Count(m => m.Classification.Equals("Error"));
                warningCount = this.MainWindow.ImportReporting.Count(m => m.Classification.Equals("Warning"));
                infoCount = this.MainWindow.ImportReporting.Count(m => m.Classification.Equals("Info"));

                this.ErrorButton.Content = Invariant($"Errors({errorCount})");
                this.WarningButton.Content = Invariant($"Warnings({warningCount})");
                this.InfoButton.Content = Invariant($"Info({infoCount})");
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49818");
            }
        }

        /// <summary>
        /// Filters the report based on what the user selects.
        /// </summary>
        private void FilterTheReport()
        {
            FilteredReport.Clear();
            this.MainWindow.ImportReporting.Where(m =>
            {
                if (!filterWarning && m.Classification.Equals("Warning"))
                {
                    return true;
                }
                if (!filterErrors && m.Classification.Equals("Error"))
                {
                    return true;
                }
                if (!filterInfo && m.Classification.Equals("Info"))
                {
                    return true;
                }
                return false;
            }).ToList().ForEach(m => FilteredReport.Add(m));
            this.NotifyPropertyChanged("FilteredReport");
            this.UpdateButtonBorders();
        }
    }
}
