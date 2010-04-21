using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// This class installs the FAM service.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        #region Fields

        /// <summary>
        /// Collection of settings to be added to the database on creation
        /// </summary>
        static Dictionary<string, string> _defaultSettings = InitializeDefaultSettings();

        /// <summary>
        /// Indicates whether the installer created the database file or not.
        /// </summary>
        bool _installerCreatedDatabase;

        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInstaller"/> class.
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Raises the <see cref="Installer.AfterInstall"/>event.
        /// </summary>
        /// <param name="savedState">Saved state information for the install.</param>
        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            try
            {
                string databaseFile = ESFAMService.DatabaseFile;

                // Check if the database file exists
                if (File.Exists(databaseFile))
                {
                    // Build new database file name (add timestamp before extension)
                    // and replace ':' with '_'
                    string fileName = Path.GetFileNameWithoutExtension(databaseFile)
                        + "." + DateTime.Now.ToString("s", CultureInfo.CurrentCulture)
                        + Path.GetExtension(databaseFile);
                    fileName = fileName.Replace(":", "_");

                    // Buld the new file name
                    fileName = FileSystemMethods.PathCombine(
                        Path.GetDirectoryName(databaseFile), fileName);

                    FileSystemMethods.MoveFile(databaseFile, fileName, false);
                }
                else
                {
                    // Ensure the path exists
                    Directory.CreateDirectory(Path.GetDirectoryName(databaseFile));
                }

                using (SqlCeEngine engine = new SqlCeEngine(ESFAMService.DatabaseConnectionString))
                {
                    // Create the database
                    engine.CreateDatabase();
                    _installerCreatedDatabase = true;
                }

                using (SqlCeConnection connection =
                    new SqlCeConnection(ESFAMService.DatabaseConnectionString))
                {
                    // If the connection is closed, open it
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    // Now add the FPSFile table
                    AddFPSFileTable(connection);
                    AddSettingsTable(connection);
                }

                base.OnAfterInstall(savedState);
            }
            catch (Exception ex)
            {
                // Log any exception and rethrow it
                ExtractException ee = ExtractException.AsExtractException("ELI28500", ex);
                ee.Log();
                throw ee;
            }
        }

        /// <summary>
        /// Raises the <see cref="Installer.AfterRollback"/> event.
        /// </summary>
        /// <param name="savedState">The saved state information for the rollback.</param>
        protected override void OnAfterRollback(System.Collections.IDictionary savedState)
        {
            try
            {
                // If a rollback is occurring in an install, then need to check if this
                // install created the database file and delete it.
                if (_installerCreatedDatabase)
                {
                    ExtractException ee;
                    if (!FileSystemMethods.TryDeleteFile(ESFAMService.DatabaseFile, out ee))
                    {
                        if (ee != null)
                        {
                            ee.Display();
                        }
                        else
                        {
                            MessageBox.Show("Unable to delete the database file.",
                                "Failed To Delete", MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                        }
                    }
                }

                base.OnAfterRollback(savedState);
            }
            catch (Exception ex)
            {
               // Log any exception and rethrow it
                ExtractException ee = ExtractException.AsExtractException("ELI28502", ex);
                ee.Log();
                throw ee;
            }
        }

        /// <summary>
        /// Adds the FPSFile table to the database.
        /// </summary>
        static void AddFPSFileTable(SqlCeConnection connection)
        {
            try
            {
                // Create the query for table creation
                string query = "CREATE TABLE FPSFile (ID int IDENTITY(1,1) PRIMARY KEY, "
                    + "AutoStart BIT DEFAULT 1 NOT NULL, FileName NVARCHAR(512) NOT NULL)";

                // Create the command to perform the table creation
                using (SqlCeCommand command = new SqlCeCommand(query, connection))
                {
                    // Create the table
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28491", ex);
            }
        }

        /// <summary>
        /// Adds the settings table to the database with default setting values
        /// </summary>
        static void AddSettingsTable(SqlCeConnection connection)
        {
            try
            {
                // Create the query for table creation
                string query = "CREATE TABLE Settings (Name NVARCHAR(100) PRIMARY KEY, "
                    + "Value NVARCHAR(512))";
                // Create the command to perform the table creation
                using (SqlCeCommand command = new SqlCeCommand(query, connection))
                {
                    // Create the table
                    command.ExecuteNonQuery();
                }
                foreach (KeyValuePair<string, string> setting in _defaultSettings)
                {
                    query = "INSERT INTO Settings (Name, Value) VALUES('"
                        + setting.Key + "', '" + setting.Value + "')";
                    using (SqlCeCommand command = new SqlCeCommand(query, connection))
                    {
                        // Insert the value
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29137", ex);
            }
        }

        /// <summary>
        /// Initializes the default settings dictionary
        /// </summary>
        /// <returns></returns>
        static Dictionary<string, string> InitializeDefaultSettings()
        {
            // Add the sleep time setting
            Dictionary<string, string> defaultSettings = new Dictionary<string, string>();
            defaultSettings.Add(ESFAMService.SleepTimeOnStartupKey,
                ESFAMService.DefaultSleepTimeOnStartup.ToString(CultureInfo.InvariantCulture));

            // Add the dependent services setting
            defaultSettings.Add(ESFAMService.DependentServices, "");

            // Add the number of files to process setting
            defaultSettings.Add(ESFAMService.NumberOfFilesToProcess,
                ESFAMService.DefaultNumberOfFilesToProcess.ToString(CultureInfo.InvariantCulture));

            // Add the schema version information
            defaultSettings.Add(ESFAMService.ServiceDatabaseSchemaVersion,
                ESFAMService.CurrentDatabaseSchemaVersion.ToString(CultureInfo.InvariantCulture));

            return defaultSettings;
        }
    }
}