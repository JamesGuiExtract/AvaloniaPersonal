using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// This class installs the FAM service.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        /// <summary>
        /// Indicates whether the installer created the database file or not.
        /// </summary>
        bool _installerCreatedDatabase;

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
                // Check if the database file exists
                if (!File.Exists(ESFAMService.DatabaseFile))
                {
                    // Ensure the path exists
                    Directory.CreateDirectory(Path.GetDirectoryName(ESFAMService.DatabaseFile));

                    using (SqlCeEngine engine = new SqlCeEngine(ESFAMService.DatabaseConnectionString))
                    {
                        // Create the database
                        engine.CreateDatabase();
                        _installerCreatedDatabase = true;
                    }

                    // Now add the FPSFile table
                    AddFPSFileTable();
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
        static void AddFPSFileTable()
        {
            SqlCeConnection connection = null;
            SqlCeCommand command = null;
            try
            {
                // Create the query for table creation
                string query = "CREATE TABLE FPSFile (AutoStart BIT NOT NULL, "
                    + "FileName NVARCHAR(512) NOT NULL)";

                // Create the database connection
                connection = new SqlCeConnection(ESFAMService.DatabaseConnectionString);

                // If the connection is closed, open it
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                // Create the command to perform the table creation
                command = new SqlCeCommand(query, connection);

                // Create the table
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28491", ex);
            }
            finally
            {
                if (command != null)
                {
                    command.Dispose();
                }
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
        }
    }
}