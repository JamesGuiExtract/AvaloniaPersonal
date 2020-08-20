using Extract.FileActionManager.Database;
using Extract.Utilities;
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
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
        /// Raises the <see cref="Installer.BeforeInstall"/> event
        /// </summary>
        /// <param name="savedState">Saved state information for the install.</param>
        /// <remarks>
        /// Overridden in order to get the name of the service from the cmdline parameters
        /// </remarks>
        protected override void OnBeforeInstall(IDictionary savedState)
        {
            SetServiceName();
            base.OnBeforeInstall(savedState);
        }

        /// <summary>
        /// Raises the <see cref="Installer.BeforeUninstall"/> event
        /// </summary>
        /// <param name="savedState">Saved state information for the install.</param>
        /// <remarks>
        /// Overridden in order to get the name of the service from the cmdline parameters
        /// </remarks>
        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            SetServiceName();
            base.OnBeforeUninstall(savedState);
        }

        /// <summary>
        /// Raises the <see cref="Installer.AfterInstall"/>event.
        /// </summary>
        /// <param name="savedState">Saved state information for the install.</param>
        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            try
            {
                string databaseFile = ESFAMService.GetDatabaseFileName(_serviceInstaller.ServiceName);
                var manager = new FAMServiceDatabaseManager(databaseFile);
                _installerCreatedDatabase = manager.CreateDatabase(true);

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
                    if (!FileSystemMethods.TryDeleteFile(ESFAMService.GetDatabaseFileName(_serviceInstaller.ServiceName), out ee))
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

        // Set the service and display names for the installer if they were provided via the cmdline
        void SetServiceName()
        {
            if (Context.Parameters.ContainsKey("ServiceName"))
            {
                _serviceInstaller.ServiceName = Context.Parameters["ServiceName"];
            }

            if (Context.Parameters.ContainsKey("DisplayName"))
            {
                _serviceInstaller.DisplayName = Context.Parameters["DisplayName"];
            }
        }
    }
}