using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.SharePoint.Redaction.Layouts
{
    /// <summary>
    /// Code behind for the remove folder watching page.
    /// </summary>
    public partial class RemoveWatchFolderWithIdShield : LayoutsPageBase
    {
        /// <summary>
        /// Handles the Page load event for the remove folder watching page.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (IsPostBack)
                {
                    return;
                }

                string currentFolder = Request.Params["folder"];

                if (!string.IsNullOrEmpty(currentFolder))
                {
                    textFolder.Text = currentFolder;
                    SPFeature feature = Web.Features[IdShieldSettings._IDSHIELD_FEATURE_GUID];
                    SPFeatureProperty property =
                        feature.Properties[IdShieldSettings._FOLDER_PROCESSING_SETTINGS_STRING];
                    if (property != null)
                    {
                        SortedDictionary<string, FolderProcessingSettings> folderSettings =
                            FolderProcessingSettings.DeserializeFolderSettings(property.Value);
                        string message = string.Empty;
                        if (folderSettings.ContainsKey(currentFolder))
                        {
                            message = "Remove the watching for this folder?";
                        }
                        else
                        {
                            // Hide the yes button and change the No button to OK
                            buttonYes.Visible = false;
                            buttonNo.Text = "OK";
                            message = "This folder is not currently being watched.";
                        }

                        labelMessage.Text = message;
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
        /// Handles the OK button click for the remove watch folder page.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void HandleYesButtonClick(object sender, EventArgs e)
        {
            try
            {
                bool watchRemoved = false;
                SPFeature feature = IdShieldHelper.GetIdShieldFeature(Web);
                if (feature == null)
                {
                    return;
                }

                SPFeatureProperty property =
                    feature.Properties[IdShieldSettings._FOLDER_PROCESSING_SETTINGS_STRING];
                if (property != null)
                {
                    SortedDictionary<string, FolderProcessingSettings> folderSettings =
                        FolderProcessingSettings.DeserializeFolderSettings(property.Value);
                    watchRemoved = folderSettings.Remove(textFolder.Text);
                    property.Value = FolderProcessingSettings.SerializeFolderSettings(
                        folderSettings);
                    feature.Properties.Update();
                }

                StringBuilder sb = new StringBuilder("<script type=\"text/javascript\">");
                if (watchRemoved)
                {
                    sb.Append("alert('Folder: ");
                    sb.Append(textFolder.Text);
                    sb.Append(" will no longer be watched'); ");
                }
                sb.Append("window.frameElement.commitPopup();");
                sb.Append("</script>");
                Context.Response.Clear();
                Context.Response.Write(sb.ToString());
                Context.Response.Flush();
                Context.Response.End();
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(Web, ex);
                throw;
            }
        }
    }
}
