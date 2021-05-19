using Extract;
using System;
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
        /// <param name="e">
        /// /import - This will run the import. In order for this to work, you must specifiy the database name/server and filepath
        /// /export - will simply populate the /path on the export page if specified.
        /// /path - The file path which houses documents for an export or import
        /// /datbaseName - the database name
        /// /databaseServer- the database server.
        /// </param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var startupConfigurator = new StartupConfigurator();
            try
            {
                var exit = startupConfigurator.Start(e.Args);
                if (exit)
                {
                    Environment.Exit(0);
                }
            }
            catch(Exception ex)
            {
                startupConfigurator.LogMessageToUser(ex.Message);
                startupConfigurator.LogMessageToUser("Please check your exception file for the error details");
                ex.ExtractLog("ELI51605");
                System.Environment.Exit(1);
            }
        }
    }
}
