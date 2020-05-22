using DatabaseMigrationWizard.Database;
using DatabaseMigrationWizard.Database.Input;
using DatabaseMigrationWizard.Database.Output;
using Extract;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
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
            ConnectionInformation connectionInformation = null;
            string password = string.Empty;
            bool importArgument = false;
            bool exportArgument = false;
            string filePath = string.Empty;
            string uexFileName = string.Empty;
            const string Password = "/PASSWORD";
            const string Import = "/IMPORT";
            const string Export = "/EXPORT";
            const string Path = "/PATH";
            const string DatabaseName = "/DATABASENAME";
            const string DatabaseServer = "/DATABASESERVER";
            const string EF = "/EF";

            try
            {
                for (int i = 0; i < e.Args.Length; i++)
                {
                    var arugment = e.Args[i].ToUpperInvariant();
                    switch (arugment)
                    {
                        case Password:
                            ValidateNextArgument(Path, e, i);
                            i += 1;
                            password = e.Args[i];
                            break;
                        case Import:
                            importArgument = true;
                            break;
                        case Export:
                            exportArgument = true;
                            break;
                        case Path:
                            ValidateNextArgument(Path, e, i);
                            i += 1;
                            filePath = e.Args[i].Trim('\"');
                            break;
                        case DatabaseName:
                            ValidateNextArgument(DatabaseName, e, i);
                            i += 1;
                            connectionInformation = connectionInformation ?? new ConnectionInformation();
                            connectionInformation.DatabaseName = e.Args[i].Trim('\"');
                            break;
                        case DatabaseServer:
                            ValidateNextArgument(DatabaseServer, e, i);
                            i += 1;
                            connectionInformation = connectionInformation ?? new ConnectionInformation();
                            connectionInformation.DatabaseServer = e.Args[i].Trim('\"');
                            break;
                        case "/":
                            Console.WriteLine(@"The supported options are /import /export /path filepath /databasename databasename and /databaseserver databaseserver. ");
                            break;
                        case EF:
                            ValidateNextArgument(EF, e, i);
                            i += 1;
                            uexFileName = e.Args[i].Trim('\"');
                            break;
                        default:
                            throw new ExtractException("ELI49796", $"The argument {e.Args[i]} is not supported");
                    }
                }

                ExtractException.Assert("ELI49797",
                    "You cannot specify both the import and export arguments. Please choose one.",
                    !importArgument || !exportArgument);

                if (connectionInformation != null)
                {
                    // To launch the wizard without the database tab, a password is required. (furnished by another
                    // Extract application already running in the admin context)
                    ExtractException.Assert("ELI49840",
                        "Incomplete database connection information",
                        !string.IsNullOrWhiteSpace(connectionInformation.DatabaseServer)
                        && !string.IsNullOrWhiteSpace(connectionInformation.DatabaseName)
                        && !string.IsNullOrWhiteSpace(password));

                    connectionInformation.ValidateConnection(password, onetimePassword: true);
                }

                MainWindow wnd = new MainWindow(connectionInformation);
                wnd.ShowDatabase = !importArgument && !exportArgument;
                wnd.ShowImport = !exportArgument;
                wnd.ShowExport = !importArgument;
                wnd.DefaultPath = string.IsNullOrWhiteSpace(filePath)
                    ? DatabaseMigrationWizard.Properties.Settings.Default.DefaultPath
                    : filePath;

                wnd.Show();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(uexFileName))
                {
                    ex.AsExtract("ELI49808").Log(uexFileName);
                }
                else
                {
                    ex.AsExtract("ELI49727").Display();
                }

                System.Environment.Exit(1);
            }
        }

        private static void ValidateNextArgument(string commandName, StartupEventArgs e, int index)
        {
            if (index + 1 > e.Args.Length)
            {
                throw new ExtractException("ELI49801", $"You must provide an argument after the {commandName}");
            }
        }
    }
}
