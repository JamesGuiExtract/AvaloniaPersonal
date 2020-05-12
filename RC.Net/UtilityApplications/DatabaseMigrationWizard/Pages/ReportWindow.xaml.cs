using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

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

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ObservableCollection<Report> FilteredReport = new ObservableCollection<Report>();

        public ReportWindow()
        {
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            InitializeComponent();
            this.DataGrid1.DataContext = this.FilteredReport;
            this.MainWindow.ReportWindow = this;
            SetDefaultFilters();
            SetButtonNumberCount();
            this.CommitPrompt.Visibility = this.MainWindow.CommitSuccessful ? Visibility.Hidden : Visibility.Visible;
            if(this.MainWindow.CommitSuccessful)
            {
                this.UpdateCommitStatusMessage();
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
                MainWindow.Import.CommitTransaction();
                this.CommitPrompt.Visibility = Visibility.Hidden;

                this.UpdateCommitStatusMessage();
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
                MainWindow.Import.RollbackTransaction();
                this.CommitPrompt.Visibility = Visibility.Hidden;

                this.UpdateCommitStatusMessage();
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
        public void UpdateCommitStatusMessage()
        {
            try
            {
                this.CommitStatus.Visibility = Visibility.Visible;
                this.CommitStatus.Text = this.MainWindow.CommitSuccessful ? "The import was succesful!" : "The import failed";
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
                if (this.MainWindow.Reporting.Where(m => m.Classification.Equals("Warning") || m.Classification.Equals("Error")).Any())
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
                this.ErrorButton.Content = $"Errors({this.MainWindow.Reporting.Where(m => m.Classification.Equals("Error")).Count().ToString(CultureInfo.InvariantCulture)})";
                this.WarningButton.Content = $"Warnings({this.MainWindow.Reporting.Where(m => m.Classification.Equals("Warning")).Count().ToString(CultureInfo.InvariantCulture)})";
                this.InfoButton.Content = $"Info({this.MainWindow.Reporting.Where(m => m.Classification.Equals("Info")).Count().ToString(CultureInfo.InvariantCulture)})";
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
            this.MainWindow.Reporting.Where(m =>
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
