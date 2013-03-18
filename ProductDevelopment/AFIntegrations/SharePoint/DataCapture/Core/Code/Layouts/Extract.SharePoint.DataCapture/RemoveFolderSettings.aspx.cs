﻿using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System;
using System.Linq;
using System.Text;

namespace Extract.SharePoint.DataCapture.Layouts
{
    /// <summary>
    /// Code behind file for the remove folder settings asp page.
    /// </summary>
    public partial class RemoveFolderSettings : LayoutsPageBase
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {   
            try
            {
                if (IsPostBack)
                {
                    return;
                }

                if (!ExtractSharePointHelper.IsMember(SPContext.Current.Web,
                    DataCaptureHelper.ExtractDataCaptureGroupName))
                {
                    SetUIToIndicateNoPermission();
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
                var settings = DataCaptureHelper.GetDataCaptureFolderSettings(siteGuid);
                if (settings.Count > 0)
                {
                    using (var site = new SPSite(siteGuid))
                    {
                        var folderId = ExtractSharePointHelper.GetFolderId(site.RootWeb, currentFolder);
                        hiddenFolderId.Value = folderId.ToString();

                        if (settings.Values
                            .Where(s => s.FolderId == folderId)
                            .Count() == 0)
                        {
                            SetUIToIndicateNoFolderWatching();
                            return;
                        }
                    }
                }
                else
                {
                    SetUIToIndicateNoFolderWatching();
                    return;
                }

                labelMessage.Text = "Remove the watching for this folder?";
            }
            catch (Exception ex)
            {
                DataCaptureHelper.LogException(ex, ErrorCategoryId.DataCaptureRemoveFolderWatch,
                    "ELI31479");
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
        /// Sets the UI to indicate to the user that they do not have permission
        /// to change the configuration of the current folder.
        /// </summary>
        void SetUIToIndicateNoPermission()
        {
            labelFolder.Visible = false;
            panelTop.GroupingText = "";
            buttonYes.Visible = false;
            buttonNo.Text = "OK";
            labelMessage.Text = "You must be a member of the "
                + "Data Capture Administrator group to use this control.";
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
                SPUtility.ValidateFormDigest();
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    watchRemoved = DataCaptureHelper.RemoveWatchingForFolder(
                        new Guid(hiddenSiteId.Value), new Guid(hiddenFolderId.Value));
                });

                var sb = new StringBuilder("<script type=\"text/javascript\">");
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
                DataCaptureHelper.LogException(ex, ErrorCategoryId.DataCaptureRemoveFolderWatch,
                    "ELI31480");
                throw;
            }
        }
    }
}
