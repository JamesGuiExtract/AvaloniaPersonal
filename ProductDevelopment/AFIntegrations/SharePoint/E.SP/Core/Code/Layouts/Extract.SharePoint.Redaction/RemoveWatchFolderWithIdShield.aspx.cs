using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System;
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

                string siteId = Request.Params["siteid"];
                string currentFolder = Request.Params["folder"];
                hiddenSiteId.Value = siteId;

                // If no current folder just return
                if (string.IsNullOrEmpty(currentFolder))
                {
                    SetUIToIndicateNoFolderWatching();
                    return;
                }

                Guid siteGuid = new Guid(siteId);
                currentFolder = ExtractSharePointHelper.GetSiteRelativeFolderPath(currentFolder,
                    siteGuid);
                textFolder.Text = currentFolder;
                using (var site = new SPSite(siteGuid))
                {
                    var folderId = ExtractSharePointHelper.GetFolderId(site.RootWeb, currentFolder);
                    hiddenFolderId.Value = folderId.ToString();
                    var settings = IdShieldProcessingFeatureSettings.GetIdShieldSettings(false);
                    if (settings == null)
                    {
                        SetUIToIndicateNoFolderWatching();
                        return;
                    }

                    var siteFolderSettings = settings.GetSiteSettings(siteGuid);

                    if (siteFolderSettings == null || !siteFolderSettings.ContainsKey(folderId))
                    {
                        SetUIToIndicateNoFolderWatching();
                        return;
                    }

                    labelMessage.Text = "Remove the watching for this folder?";
                }
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldRemoveFolderWatch,
                    "ELI30555");
                throw;
            }
        }

        /// <summary>
        /// Sets the UI to indicate to the user that there is no watching
        /// currently configured for this folder
        /// </summary>
        void SetUIToIndicateNoFolderWatching()
        {
            buttonYes.Visible = false;
            buttonNo.Text = "OK";
            labelMessage.Text = "This folder is not currently being watched.";
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
                var siteId = new Guid(hiddenSiteId.Value);
                var folderId = new Guid(hiddenFolderId.Value);
                bool watchRemoved = false;
                SPUtility.ValidateFormDigest();
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    watchRemoved =
                        IdShieldProcessingFeatureSettings.RemoveFolderWatching(siteId, folderId);
                });

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
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldRemoveFolderWatch,
                    "ELI30554");
                throw;
            }
        }
    }
}
