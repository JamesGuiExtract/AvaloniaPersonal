using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System;

namespace Extract.SharePoint.DataCapture.Layouts
{
    /// <summary>
    /// Code behind file for the process folder settings asp page.
    /// </summary>
    public partial class ProcessFolderSettings : LayoutsPageBase
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Do nothing if this load is the result of a post back
            if (IsPostBack)
            {
                return;
            }
            try
            {
                if (!ExtractSharePointHelper.IsMember(SPContext.Current.Web,
                    DataCaptureHelper.ExtractDataCaptureGroupName))
                {
                    HideNormalControls();
                    panelMessage.Visible = true;
                    labelMessage.Text = "You must be a member of the "
                        + "Data Capture Administrator group to use this control.";
                    return;
                }

                string siteId = Request.QueryString["siteid"];
                hiddenSiteId.Value = siteId;
                Guid siteGuid = new Guid(siteId);
                string currentFolder = ExtractSharePointHelper.GetSiteRelativeFolderPath(
                    Request.QueryString["folder"], siteGuid);
                textCurrentFolderName.Text = currentFolder;
                string listid = Request.QueryString["listidvalue"];
                hiddenListId.Value = listid.Substring(1, listid.Length - 2);

                using (var site = new SPSite(siteGuid))
                {
                    var folderId = ExtractSharePointHelper.GetFolderId(site.RootWeb, currentFolder);
                    hiddenFolderId.Value = folderId.ToString();

                    // Store the title of the list for future validation
                    var list = site.RootWeb.Lists[new Guid(listid)];
                    hiddenListName.Value = list.Title;

                    var folderSettings = DataCaptureHelper.GetDataCaptureFolderSettings(siteGuid);
                    if (folderSettings.Count == 0)
                    {
                        checkDoNotProcessExisting.Enabled = false;
                        return;
                    }

                    // Search the collection of all folders being watched
                    string rootKey = string.Empty;
                    FolderProcessingSettings temp = null;
                    bool watchingSubfolders = false;
                    bool watchingCurrentFolder = false;
                    bool watchingParentRecursively = false;
                    foreach (var pair in folderSettings)
                    {
                        string key = pair.Key;
                        if (!watchingCurrentFolder
                            && currentFolder.Equals(key, StringComparison.Ordinal))
                        {
                            watchingCurrentFolder = true;
                            temp = pair.Value;
                        }
                        else if (currentFolder.StartsWith(key + "/", StringComparison.Ordinal))
                        {
                            if (pair.Value.RecurseSubfolders)
                            {
                                rootKey = key;
                                watchingParentRecursively = true;
                                temp = pair.Value;
                                break;
                            }
                        }
                        else if (!watchingSubfolders
                            && key.StartsWith(currentFolder + "/", StringComparison.Ordinal))
                        {
                            watchingSubfolders = true;
                        }
                    }

                    if (watchingParentRecursively)
                    {
                        // Hide the unnecessary controls
                        HideNormalControls();
                        panelMessage.Visible = true;
                        labelMessage.Text = "Cannot add watch to this folder since "
                            + "this folder is watched recursively by the settings for "
                            + rootKey;

                        return;
                    }
                    if (watchingSubfolders)
                    {
                        checkRecursively.Enabled = false;
                    }
                    if (watchingCurrentFolder)
                    {
                        if (checkRecursively.Enabled)
                        {
                            checkRecursively.Checked = temp.RecurseSubfolders;
                        }
                        textFileExtension.Text = temp.FileExtensions;
                        checkReprocess.Checked = temp.Reprocess;
                        bool processAdded = temp.ProcessAddedFiles;
                        checkAdded.Checked = processAdded;
                        if (processAdded)
                        {
                            checkDoNotProcessExisting.Checked = !temp.ProcessExisting;
                        }
                    }

                    checkDoNotProcessExisting.Enabled = checkAdded.Checked;
                }
            }
            catch (Exception ex)
            {
                DataCaptureHelper.LogException(ex, ErrorCategoryId.DataCaptureWatchFolderConfiguration,
                    "ELI31482");
                throw;
            }
        }

        /// <summary>
        /// Handles the OK button click for the configuration page.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (!IsValid)
                {
                    return;
                }

                var currentFolderSettings = new FolderProcessingSettings(
                    new Guid(hiddenListId.Value), new Guid(hiddenFolderId.Value),
                    textCurrentFolderName.Text, textFileExtension.Text, checkRecursively.Checked,
                    checkReprocess.Checked, checkAdded.Checked, !checkDoNotProcessExisting.Checked);


                // Need to run with elevated privileges in order to update the
                // data capture settings object
                SPUtility.ValidateFormDigest();
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    DataCaptureHelper.AddOrUpdateFolderSettings(new Guid(hiddenSiteId.Value),
                        currentFolderSettings);
                });

                // Send a response to close the dialog
                Context.Response.Write(
                    "<script type=\"text/javascript\">"
                    + "window.frameElement.commitPopup();"
                    + "</script>"
                    );
                Context.Response.Flush();
                Context.Response.End();
            }
            catch (Exception ex)
            {
                DataCaptureHelper.LogException(ex, ErrorCategoryId.DataCaptureWatchFolderConfiguration,
                    "ELI31483");
                throw;
            }
        }

        /// <summary>
        /// Handles the check added changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void HandleCheckAddedChanged(object sender, EventArgs e)
        {
            try
            {
                bool enable = checkAdded.Checked;
                checkDoNotProcessExisting.Enabled = enable;
                if (!enable)
                {
                    checkDoNotProcessExisting.Checked = false;
                }
            }
            catch (Exception ex)
            {
                DataCaptureHelper.LogException(ex, ErrorCategoryId.DataCaptureWatchFolderConfiguration,
                    "ELI31484");
                throw;
            }
        }

        /// <summary>
        /// Hides the regular set of controls and renames the cancel button
        /// to OK so that the cannot add label can be displayed.
        /// </summary>
        void HideNormalControls()
        {
            panelFileSpecification.Visible = false;
            panelFolderSettings.Visible = false;
            Form.Controls.Remove(panelFileSpecification);
            Form.Controls.Remove(panelFolderSettings);
            buttonOk.Visible = false;
            buttonCancel.Text = "OK";
        }
    }
}
