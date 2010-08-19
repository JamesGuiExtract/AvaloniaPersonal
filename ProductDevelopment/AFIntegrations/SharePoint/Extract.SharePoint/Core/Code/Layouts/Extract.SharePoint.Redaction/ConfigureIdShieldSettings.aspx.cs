using Microsoft.SharePoint;
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

                SPFeature feature = IdShieldHelper.GetIdShieldFeature(Web);
                if (feature != null)
                {
                    SPFeatureProperty localFolder =
                        feature.Properties[IdShieldSettings._LOCAL_WORKING_FOLDER_SETTING_STRING];
                    if (localFolder != null)
                    {
                        textFolder.Text = localFolder.Value;
                    }

                    SPFeatureProperty ipAddress =
                        feature.Properties[IdShieldSettings._IP_ADDRESS_SETTING_STRING];
                    if (ipAddress != null)
                    {
                        textExceptionIpAddress.Text = ipAddress.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(Web, ex);
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
            if (IsValid)
            {
                string response = "<script type=\"text/javascript\">"
                    + " <==REPLACEME==> "
                        + "</script>";
                try
                {
                    // Remove trailing '\'
                    string folder = textFolder.Text;
                    if (folder.EndsWith("\\", StringComparison.Ordinal))
                    {
                        folder = folder.Substring(0, folder.Length - 1);
                    }

                    SPFeature feature = Web.Features[IdShieldSettings._IDSHIELD_FEATURE_GUID];
                    if (feature != null)
                    {
                        SPFeatureProperty localFolder =
                            feature.Properties[IdShieldSettings._LOCAL_WORKING_FOLDER_SETTING_STRING];
                        if (localFolder != null)
                        {
                            localFolder.Value = folder;
                        }
                        else
                        {
                            localFolder = new SPFeatureProperty(
                                IdShieldSettings._LOCAL_WORKING_FOLDER_SETTING_STRING, folder);
                            feature.Properties.Add(localFolder);
                        }

                        SPFeatureProperty ipAddress =
                            feature.Properties[IdShieldSettings._IP_ADDRESS_SETTING_STRING];
                        if (ipAddress != null)
                        {
                            ipAddress.Value = textExceptionIpAddress.Text.Trim();
                        }
                        else
                        {
                            ipAddress = new SPFeatureProperty(
                                IdShieldSettings._IP_ADDRESS_SETTING_STRING,
                                textExceptionIpAddress.Text);
                            feature.Properties.Add(ipAddress);
                        }

                        feature.Properties.Update();
                    }

                    response = response.Replace("<==REPLACEME==>",
                        "window.frameElement.commitPopup();");
                }
                catch (Exception ex)
                {
                    response = response.Replace("<==REPLACEME==>",
                        "alert('" + ex.Message
                        + "'); window.frameElement.commitPopup();");
                }
                finally
                {
                    // Send the response to close the dialog
                    Context.Response.Write(response);
                    Context.Response.Flush();
                    Context.Response.End();
                }
            }
        }
    }
}
