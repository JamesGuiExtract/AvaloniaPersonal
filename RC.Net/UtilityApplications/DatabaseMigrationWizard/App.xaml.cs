using DatabaseMigrationWizard.Database;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using System.Windows;

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("en")]
namespace DatabaseMigrationWizard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The startup to the application
        /// </summary>
        /// <param name="sender">Unused</param>
        /// <param name="e">The database server, then the database name</param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConnectionInformation connectionInformation = new ConnectionInformation() { DatabaseServer = "(local)", DatabaseName = "ImportTester" }; // Extract_ANONOMYZE
            if (e.Args.Length == 2)
            {
                connectionInformation.DatabaseServer = e.Args[0].ToString();
                connectionInformation.DatabaseName = e.Args[1].ToString();
            }
            
            MainWindow wnd = new MainWindow(connectionInformation);
            wnd.Show();
        }
    }
}
