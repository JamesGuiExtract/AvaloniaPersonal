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
            ConnectionInformation connectionInformation = new ConnectionInformation();
            bool importArgument = false;
            bool exportArgument = false;
            string filePath = string.Empty;
            string uexFileName = string.Empty;
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
                        case Import:
                            importArgument = true;
                            break;
                        case Export:
                            exportArgument = true;
                            break;
                        case Path:
                            ValidateNextArgument(Path, e, i);
                            i += 1;
                            filePath = e.Args[i];
                            break;
                        case DatabaseName:
                            ValidateNextArgument(DatabaseName, e, i);
                            i += 1;
                            connectionInformation.DatabaseName = e.Args[i];
                            break;
                        case DatabaseServer:
                            ValidateNextArgument(DatabaseServer, e, i);
                            i += 1;
                            connectionInformation.DatabaseServer = e.Args[i];
                            break;
                        case "/":
                            Console.WriteLine(@"The supported options are /import /export /path filepath /databasename databasename and /databaseserver databaseserver. ");
                            break;
                        case EF:
                            ValidateNextArgument(EF, e, i);
                            i += 1;
                            uexFileName = e.Args[i];
                            break;
                        default:
                            throw new ExtractException("ELI49796", $"The argument {e.Args[i]} is not supported");
                    }
                }
                MainWindow wnd = null;
                if (importArgument && exportArgument)
                {
                    throw new ExtractException("ELI49797", "You cannot specify both the import and export arguments. Please choose one.");
                }
                else if(importArgument)
                {
                    ImportHelper importHelper = new ImportHelper(new ImportOptions() 
                    {
                        ImportPath = filePath,
                        ConnectionInformation = connectionInformation, 
                        ClearDatabase = false 
                    }, new Progress<string>((garbage) => { }));
                    importHelper.Import();
                    importHelper.CommitTransaction();
                    Console.WriteLine("Import was successful");
                    System.Environment.Exit(0);
                }
                else if(exportArgument)
                {
                    wnd = new MainWindow(connectionInformation, new ExportOptions() { ExportPath = filePath, ConnectionInformation = connectionInformation });
                }
                else
                {
                    wnd = new MainWindow(connectionInformation);
                }
                wnd.Show();
            }
            catch(Exception ex)
            {
                if(!string.IsNullOrEmpty(uexFileName))
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
