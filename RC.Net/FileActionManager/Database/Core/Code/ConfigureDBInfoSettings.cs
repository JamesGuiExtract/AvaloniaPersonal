using Extract.Licensing;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// COM class for launching the ser processing schedule dialog.
    /// </summary>
    [ComVisible(true)]
    [ProgId("Extract.FileActionManager.Database.ConfigureDBInfoSettings")]
    [Guid("F86BB12C-EB1C-44EA-B5EA-9A428A601608")]
    public class ConfigureDBInfoSettings : IConfigureDBInfoSettings
    {
        #region Constants

        /// <summary>
        /// Object name used in license validation calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ConfigureDBInfoSettings).ToString();

        #endregion Constants

        #region IConfigureDBInfoSettings Members

        /// <summary>
        /// Prompts for settings.
        /// </summary>
        /// <param name="pDBManager">The database manager to use.</param>
        /// <returns><see langword="true"/> if the db info settings were updated,
        /// and <see langword="false"/> otherwise.</returns>
        [CLSCompliant(false)]
        public bool PromptForSettings(FileProcessingDB pDBManager)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI31932", _OBJECT_NAME);

                string server = pDBManager.DatabaseServer;
                string database = pDBManager.DatabaseName;

                using (var dialog = new FAMDatabaseOptionsDialog(server, database))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        return dialog.SettingsUpdated;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31933", "Unable to configure DB info settings.");
            }
        }

        #endregion
    }
}
