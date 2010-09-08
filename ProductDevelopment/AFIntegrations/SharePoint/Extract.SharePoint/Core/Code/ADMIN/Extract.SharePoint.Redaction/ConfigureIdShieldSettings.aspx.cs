using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.ApplicationPages;
using Microsoft.SharePoint.Utilities;
using System;
using System.Globalization;

namespace Extract.SharePoint.Redaction.Layouts
{
    /// <summary>
    /// Code behind file for the ID Shield configuration page
    /// </summary>
    public partial class ConfigureIdShieldSettings : DialogAdminPageBase
    {
        /// <summary>
        /// Called when the page loads.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Check whether this is a postback or not
                if (IsPostBack)
                {
                    return;
                }

                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
                if (settings != null)
                {
                    textFolder.Text = settings.LocalWorkingFolder;
                    textExceptionIpAddress.Text = settings.ExceptionServiceIPAddress;
                }
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration,
                    "ELI30552");
                throw;
            }
        }

        /// <summary>
        /// Handles the OK button click from the configuration page.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void HandleOkButtonClick(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            try
            {
                // Remove trailing '\'
                string folder = textFolder.Text.Trim();
                if (folder.EndsWith("\\", StringComparison.Ordinal))
                {
                    folder = folder.Substring(0, folder.Length - 1);
                }

                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(true);
                settings.LocalWorkingFolder = folder;
                settings.ExceptionServiceIPAddress = textExceptionIpAddress.Text.Trim();
                settings.Update();
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration,
                    "ELI30553");
                throw;
            }

            // Redirect back to the application management page
            // This method raises a thread aborted exception (by design) and so should not
            // be wrapped in a try catch
            SPUtility.Redirect("/applications.aspx", SPRedirectFlags.Default, this.Context);
        }

        /// <summary>
        /// Handles the cancel button click from the configuration page
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void HandleCancelButtonClick(object sender, EventArgs e)
        {
            // Redirect back to the application management page
            // This method raises a thread aborted exception (by design) and so should not
            // be wrapped in a try catch
            SPUtility.Redirect("/applications.aspx", SPRedirectFlags.Default, this.Context);
        }
    }
}
