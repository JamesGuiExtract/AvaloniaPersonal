using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Administration;
using System.Text;

namespace Extract.SharePoint.Layouts.Extract.SharePoint.Redaction
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
            // Check whether the page has been loaded already
            // (Note: in ASP land, when a serverside button handler
            // is called, step 1 is to reload the page posting the click event
            // so a hidden field is used to indicate whether the settings have
            // already been loaded for the session of not.
            if (string.IsNullOrEmpty(hiddenLoaded.Value))
            {
                SPFeature feature = Web.Features[ExtractSharePointHelper._IDSHIELD_FEATURE_GUID];
                if (feature != null)
                {
                    SPFeatureProperty localFolder =
                        feature.Properties[ExtractSharePointHelper._ID_SHIELD_LOCAL_FOLDER];
                    if (localFolder != null)
                    {
                        txtFolder.Text = localFolder.Value;
                    }
                }

                hiddenLoaded.Value = "Loaded";
            }
        }

        /// <summary>
        /// Handles the cancel button click from the configuration page.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void btnCancelClick(object sender, EventArgs e)
        {
            // Send the response to close the dialog.
            Context.Response.Write(
                "<script type=\"text/javascript\">"
                + "window.frameElement.commitPopup();"
                + "</script>"
                );
            Context.Response.Flush();
            Context.Response.End();
        }

        /// <summary>
        /// Handles the OK button click from the configuration page.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void btnOkClick(object sender, EventArgs e)
        {
            string response = "<script type=\"text/javascript\">"
                + " <==REPLACEME==> "
                    + "</script>";
            try
            {
                string folder = txtFolder.Text;
                SPFeature feature = Web.Features[ExtractSharePointHelper._IDSHIELD_FEATURE_GUID];
                if (feature != null)
                {
                    SPFeatureProperty localFolder =
                        feature.Properties[ExtractSharePointHelper._ID_SHIELD_LOCAL_FOLDER];
                    if (localFolder != null)
                    {
                        localFolder.Value = folder;
                    }
                    else
                    {
                        localFolder = new SPFeatureProperty(
                            ExtractSharePointHelper._ID_SHIELD_LOCAL_FOLDER, folder);
                        feature.Properties.Add(localFolder);
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
