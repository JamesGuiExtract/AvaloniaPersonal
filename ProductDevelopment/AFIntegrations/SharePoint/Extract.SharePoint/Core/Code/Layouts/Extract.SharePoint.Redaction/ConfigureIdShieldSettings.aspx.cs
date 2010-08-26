using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System;

namespace Extract.SharePoint.Redaction.Layouts
{
    /// <summary>
    /// Code behind file for the ID Shield configuration page
    /// </summary>
    public partial class ConfigureIdShieldSettings : LayoutsPageBase
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
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration);
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
                SPUtility.ValidateFormDigest();
                SPSecurity.RunWithElevatedPrivileges(delegate()
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
                });

                string response = "<script type=\"text/javascript\">"
                    + " window.frameElement.commitPopup(); "
                        + "</script>";
                // Send the response to close the dialog
                Context.Response.Write(response);
                Context.Response.Flush();
                Context.Response.End();
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration);
                throw;
            }
        }
    }
}
