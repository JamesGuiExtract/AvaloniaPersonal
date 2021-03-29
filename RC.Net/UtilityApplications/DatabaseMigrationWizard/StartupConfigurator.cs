using DatabaseMigrationWizard.Database;
using DatabaseMigrationWizard.Database.Input;
using Extract;
using System;
using System.IO;
using UCLID_FILEPROCESSINGLib;

namespace DatabaseMigrationWizard
{
    class StartupConfigurator
    {
        private ConnectionInformation connectionInformation = null;
        private string password = string.Empty;
        private bool importArgument = false;
        private bool exportArgument = false;
        private bool createDatabase = false;
        private string uexFileName = string.Empty;
        private string filePath = string.Empty;

        /// <summary>
        /// Provides a centralized place to provide feedback to the user.
        /// </summary>
        /// <param name="message">The message to give to the user</param>
        internal static void LogMessageToUser(string message)
        {
            Console.WriteLine(message);
        }

        internal bool Start(string[] arguments)
        {
            bool useUI = false;
            try
            {
                ParseArguments(arguments);

                if (createDatabase)
                {
                    CreateDatabase(connectionInformation, filePath);
                    return true;
                }
                else
                {
                    useUI = true;
                    connectionInformation?.ValidateConnection(password, onetimePassword: !string.IsNullOrEmpty(password));
                    
                    new MainWindow(connectionInformation)
                    {
                        ShowDatabase = !importArgument && !exportArgument,
                        ShowImport = importArgument,
                        ShowExport = exportArgument,
                        DefaultPath = string.IsNullOrWhiteSpace(filePath)
                        ? Properties.Settings.Default.DefaultPath
                        : filePath
                    }.Show();
                }
            }
            catch(Exception ex)
            {
                if (!string.IsNullOrEmpty(uexFileName))
                {
                    LogMessageToUser(ex.Message);
                    LogMessageToUser("Please check your exception file for the error details");
                    ex.AsExtract("ELI49808").Log(uexFileName);
                }
                else if(useUI)
                {
                    ex.AsExtract("ELI51604").Display();
                }
                else
                {
                    throw;
                }
            }

            return false;
        }

        private static void CreateDatabase(ConnectionInformation connectionInformation, string filePath)
        {
            var fileProcessingDb = new FileProcessingDB()
            {
                DatabaseServer = connectionInformation.DatabaseServer,
                DatabaseName = connectionInformation.DatabaseName
            };
            fileProcessingDb.CreateNewDB(connectionInformation.DatabaseName, "");
            var importHelper = new ImportHelper(new ImportOptions()
            {
                ConnectionInformation = connectionInformation,
                ImportCoreTables = true,
                ImportLabDETables = DirectoryContainsLabDEJsonFiles(filePath),
                ImportPath = filePath
            }
                                                , new Progress<string>(_ => { }));
            importHelper.Import();
            importHelper.CommitTransaction();
            LogMessageToUser("Database was imported successfully.");
        }

        /// <summary>
        /// Checks to see if the provided directory has any file names containing the string LABDE.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <returns></returns>
        private static bool DirectoryContainsLabDEJsonFiles(string directory)
        {
            bool labDEFound = false;
            foreach (var file in Directory.GetFiles(directory))
            {
                if (Path.GetFileName(file).ToUpperInvariant().Contains("LABDE"))
                {
                    labDEFound = true;
                    break;
                }
            }
            return labDEFound;
        }

        private static void ValidateNextArgument(string[] arguments, int index)
        {
            if (index + 1 >= arguments.Length)
            {
                throw new ExtractException("ELI49801", $"You must provide an argument after the {arguments[index]}");
            }
        }

        /// <summary>
        /// Parses the arguments the user provides, and assigns appropriate class variables.
        /// </summary>
        /// <param name="startupEventArguments">The arguments to parse</param>
        private void ParseArguments(string[] arguments)
        {
            const string Password = "/PASSWORD";
            const string Import = "/IMPORT";
            const string Export = "/EXPORT";
            const string Path = "/PATH";
            const string DatabaseName = "/DATABASENAME";
            const string DatabaseServer = "/DATABASESERVER";
            const string EF = "/EF";
            const string CreateDatabase = "/CREATEDATABASE";

            for (int i = 0; i < arguments.Length; i++)
            {
                var arugment = arguments[i].ToUpperInvariant();
                switch (arugment)
                {
                    case Password:
                        ValidateNextArgument(arguments, i);
                        i += 1;
                        password = arguments[i];
                        break;
                    case Import:
                        importArgument = true;
                        break;
                    case Export:
                        exportArgument = true;
                        break;
                    case Path:
                        ValidateNextArgument(arguments, i);
                        i += 1;
                        filePath = arguments[i].Trim('\"');
                        break;
                    case DatabaseName:
                        ValidateNextArgument(arguments, i);
                        i += 1;
                        connectionInformation = connectionInformation ?? new ConnectionInformation();
                        connectionInformation.DatabaseName = arguments[i].Trim('\"');
                        break;
                    case DatabaseServer:
                        ValidateNextArgument(arguments, i);
                        i += 1;
                        connectionInformation = connectionInformation ?? new ConnectionInformation();
                        connectionInformation.DatabaseServer = arguments[i].Trim('\"');
                        break;
                    case "/?":
                    case "/":
                        LogMessageToUser(
                            "The supported options are: \n" +
                             "/import - Used to specify if only the import page should be shown. Typically used from other extract applications. Requires a one time use password. \n" +
                             "/export - Used to specify if only the export page should be shown. Typically used from other extract applications. Requires a one time use password. \n" +
                             "/path filepath - The filepath to populate in the import and export windows. \n" +
                             "/databasename databasename - Used to specify the database name upon launch. Also requires the database server, and one time password or the create database argument. \n" +
                             "/databaseserver databaseserver - Used to specify the database server upon launch. Also requires the database name, and one time password or the create database argument. \n" +
                             "/createDatabase - Used to create a database. This will NOT launch the UI, and requires the database name/server and filepath. \n" +
                             "/password password - This is a one time password generated from another extract application. The external application must be logged in as admin.");
                        System.Environment.Exit(1);
                        break;
                    case EF:
                        ValidateNextArgument(arguments, i);
                        i += 1;
                        uexFileName = arguments[i].Trim('\"');
                        break;
                    case CreateDatabase:
                        createDatabase = true;
                        break;
                    default:
                        throw new ExtractException("ELI49796", $"The argument {arguments[i]} is not supported");
                }
            }

            CheckForArgumentErrors();
        }

        /// <summary>
        /// Checks for potential errors with the arguments the user provides.
        /// </summary>
        private void CheckForArgumentErrors()
        {
            ExtractException.Assert("ELI49797",
                    "You cannot specify both the import and export arguments. Please choose one.",
                    !(importArgument && exportArgument));

            ExtractException.Assert("ELI51584", "You cannot specify both the export argument, and create database argument", !(exportArgument && createDatabase));

            ExtractException.Assert("ELI51587",
                "The create database argument requires a filepath, database server, and database name",
                !(createDatabase
                && (string.IsNullOrEmpty(connectionInformation?.DatabaseServer)
                || string.IsNullOrEmpty(connectionInformation?.DatabaseName) 
                || string.IsNullOrEmpty(filePath))));

            // To launch the wizard without the database tab, a password is required. (furnished by another
            // Extract application already running in the admin context).
            ExtractException.Assert("ELI49840",
                    "Incomplete database connection information",
                    !(!string.IsNullOrEmpty(password)
                    && (string.IsNullOrEmpty(connectionInformation?.DatabaseServer)
                    || string.IsNullOrEmpty(connectionInformation?.DatabaseName))));
        }
    }
}
