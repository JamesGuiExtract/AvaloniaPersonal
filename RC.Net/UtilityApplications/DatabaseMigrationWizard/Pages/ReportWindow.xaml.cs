using Extract;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DatabaseMigrationWizard.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class ReportWindow : UserControl
    {
        public MainWindow MainWindow { get; set; }

        public ReportWindow()
        {
            this.MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            InitializeComponent();
            this.DataGrid1.DataContext = this.MainWindow.Reporting;
            this.MainWindow.ReportWindow = this;
            if(this.MainWindow.Reporting.Where(m => m.Classification.Equals("Warning") || m.Classification.Equals("Error")).Any())
            {
                this.CommitPrompt.Visibility = Visibility.Visible;
            }
        }

        private void ImportCommit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                MainWindow.Import.CommitTransaction();
                this.CommitPrompt.Visibility = Visibility.Hidden;
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
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI49753").Display();
            }
        }
    }
}
