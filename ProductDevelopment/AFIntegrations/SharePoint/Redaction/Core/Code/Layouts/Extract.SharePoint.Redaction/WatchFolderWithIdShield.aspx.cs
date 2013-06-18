using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Data;


namespace Extract.SharePoint.Redaction.Layouts
{
    /// <summary>
    /// Code behind file for the folder configuration page.
    /// </summary>
    public partial class WatchFolderWithIdShield : LayoutsPageBase
    {
        /// <summary>
        /// Handles the Page load event for the folder configuration page.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
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
                    IdShieldHelper.IdShieldAdministratorsGroupName))
                {
                    HideNormalControls();
                    panelMessage.Visible = true;
                    labelMessage.Text = "You must be a member of the "
                        + "ID Shield Administrator group to use this control.";
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

                    List<string> supportedTypes = new List<string>();

                    supportedTypes.Add("Boolean");
                    supportedTypes.Add("Text");
                    supportedTypes.Add("Choice");

                    // Load the list values
                    SPFieldCollection fields = site.RootWeb.Fields;
                    DataTable fieldList = ExtractSharePointHelper.GetFieldsListForFolder(site.RootWeb, currentFolder, supportedTypes);

                    fieldSelectionList.DataSource = fieldList;
                    fieldSelectionList.DataValueField = "InternalName";
                    fieldSelectionList.DataTextField = "Title";
                    fieldSelectionList.DataBind();
                    
                    var folderSettings = IdShieldHelper.GetIdShieldFolderSettings(siteGuid);
                    if (folderSettings.Count == 0)
                    {
                        radioParallel.Checked = true;
                        radioFilesAdded.Checked = true;
                        checkDoNotProcessExisting.Enabled = true;
                        fieldSelectionList.Enabled = false;
                        textValue.Enabled = false;
                        ToggleAllOutputLocations();
                        return;
                    }

                    // Search the collection of all folders being watched
                    string rootKey = string.Empty;
                    IdShieldFolderProcessingSettings temp = null;
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
                        if (!processAdded && !temp.QueueWithFieldValue)
                        {
                            radioManualSelect.Checked = true; ;
                        }
                        radioFilesAdded.Checked = processAdded;
                        if (processAdded)
                        {
                            checkDoNotProcessExisting.Checked = !temp.ProcessExisting;
                        }
                        radioByValue.Checked = temp.QueueWithFieldValue;
                        fieldSelectionList.SelectedValue = temp.FieldForQueuing;
                        textValue.Text = temp.ValueToQueueOn;
                        hiddenOutputLocation.Value = temp.OutputLocation.ToString("G");
                        switch (temp.OutputLocation)
                        {
                            case IdShieldOutputLocation.ParallelFolderPrefix:
                            case IdShieldOutputLocation.ParallelFolderSuffix:
                                radioParallel.Checked = true;
                                textParallel.Text = temp.OutputLocationString;
                                dropFolderName.SelectedIndex =
                                    temp.OutputLocation == IdShieldOutputLocation.ParallelFolderPrefix ?
                                    0 : 1;
                                break;

                            case IdShieldOutputLocation.Subfolder:
                                radioSubfolder.Checked = true;
                                textSubfolder.Text = temp.OutputLocationString;
                                break;

                            case IdShieldOutputLocation.PrefixFilename:
                            case IdShieldOutputLocation.SuffixFilename:
                                radioSameFolder.Checked = true;
                                textPreSuffix.Text = temp.OutputLocationString;
                                dropFileName.SelectedIndex =
                                    temp.OutputLocation == IdShieldOutputLocation.PrefixFilename ?
                                    0 : 1;
                                break;

                            case IdShieldOutputLocation.MirrorDocumentLibrary:
                                radioMirrorLibrary.Checked = true;
                                textMirrorOut.Text = temp.OutputLocationString;
                                break;
                        }
                    }
                    else
                    {
                        radioParallel.Checked = true;
                    }

                    fieldSelectionList.Enabled = radioByValue.Checked;
                    textValue.Enabled = radioByValue.Checked;
                    checkDoNotProcessExisting.Enabled = radioFilesAdded.Checked;
                    
                    ToggleAllOutputLocations();
                }
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldWatchFolderConfiguration,
                    "ELI30556");
                throw;
            }
        }

        /// <summary>
        /// Handles the drop down changed event for the same folder prefix/suffix drop down.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void PrefixSuffixDropDownChanged(object sender, EventArgs e)
        {
            IdShieldOutputLocation location = dropFileName.SelectedIndex == 0 ?
                IdShieldOutputLocation.PrefixFilename : IdShieldOutputLocation.SuffixFilename;
            hiddenOutputLocation.Value = location.ToString("G");
        }

        /// <summary>
        /// Handles the drop down changed event for the parallel folder prefix/suffix
        /// drop down.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void PrefixSuffixFolderDropDownChanged(object sender, EventArgs e)
        {
            IdShieldOutputLocation location = dropFolderName.SelectedIndex == 0 ?
                IdShieldOutputLocation.ParallelFolderPrefix :
                IdShieldOutputLocation.ParallelFolderSuffix;
            hiddenOutputLocation.Value = location.ToString("G");
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

                string locationString = string.Empty;
                IdShieldOutputLocation location = (IdShieldOutputLocation)
                    Enum.Parse(typeof(IdShieldOutputLocation), hiddenOutputLocation.Value);
                switch (location)
                {
                    case IdShieldOutputLocation.ParallelFolderPrefix:
                    case IdShieldOutputLocation.ParallelFolderSuffix:
                        locationString = textParallel.Text;
                        break;

                    case IdShieldOutputLocation.Subfolder:
                        locationString = textSubfolder.Text;
                        break;

                    case IdShieldOutputLocation.PrefixFilename:
                    case IdShieldOutputLocation.SuffixFilename:
                        locationString = textPreSuffix.Text;
                        break;

                    case IdShieldOutputLocation.MirrorDocumentLibrary:
                        locationString = textMirrorOut.Text;
                        break;
                }
                locationString = locationString.Trim();

                var currentFolderSettings = new IdShieldFolderProcessingSettings(
                    new Guid(hiddenListId.Value), new Guid(hiddenFolderId.Value),
                    textCurrentFolderName.Text, textFileExtension.Text, checkRecursively.Checked,
                    checkReprocess.Checked, radioFilesAdded.Checked, true, !checkDoNotProcessExisting.Checked,
                    location, locationString, radioByValue.Checked, fieldSelectionList.SelectedValue, textValue.Text);


                // Need to run with elevated privileges in order to update the
                // ID Shield settings object
                SPUtility.ValidateFormDigest();
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    IdShieldHelper.AddOrUpdateFolderSettings(new Guid(hiddenSiteId.Value),
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
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldWatchFolderConfiguration,
                    "ELI30557");
                throw;
            }
        }

        /// <summary>
        /// Handles the radio button changed event for the added or by value radio buttons
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void RadioAddedOrByValueChanged(object sender, EventArgs e)
        {
            try
            {
                checkDoNotProcessExisting.Enabled = radioFilesAdded.Checked;
                fieldSelectionList.Enabled = radioByValue.Checked;
                textValue.Enabled = radioByValue.Checked;
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldWatchFolderConfiguration, "ELI35678");
            }
        }

        /// <summary>
        /// Handles the radio button changed event for each of the folder setting
        /// radio button controls.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void RadioButtonChanged(object sender, EventArgs e)
        {
            ToggleAllOutputLocations();
        }

        /// <summary>
        /// Toggles all the output locations based on their current check state
        /// </summary>
        void ToggleAllOutputLocations()
        {
            ToggleParallel(radioParallel.Checked);
            ToggleSubfolder(radioSubfolder.Checked);
            ToggleSameFolder(radioSameFolder.Checked);
            ToggleMirrorLocation(radioMirrorLibrary.Checked);
        }

        /// <summary>
        /// Handles enabling/disabling the text control associated with the parallel
        /// folder setting.
        /// </summary>
        /// <param name="enabled">Whether to enable/disable the controls.</param>
        void ToggleParallel(bool enabled)
        {
            textParallel.Enabled = enabled;
            dropFolderName.Enabled = enabled;
            if (enabled)
            {
                IdShieldOutputLocation location =
                    dropFolderName.SelectedIndex == 0 ?
                    IdShieldOutputLocation.ParallelFolderPrefix :
                    IdShieldOutputLocation.ParallelFolderSuffix;
                hiddenOutputLocation.Value = location.ToString("G");
            }
        }

        /// <summary>
        /// Handles enabling/disabling the text control associated with the sub folder
        /// folder setting.
        /// </summary>
        /// <param name="enabled">Whether to enable/disable the controls.</param>
        void ToggleSubfolder(bool enabled)
        {
            textSubfolder.Enabled = enabled;
            if (enabled)
            {
                hiddenOutputLocation.Value =
                    IdShieldOutputLocation.Subfolder.ToString("G");
            }
        }

        /// <summary>
        /// Handles enabling/disabling the text and drop down control associated
        /// with the same folder folder setting.
        /// </summary>
        /// <param name="enabled">Whether to enable/disable the controls.</param>
        void ToggleSameFolder(bool enabled)
        {
            dropFileName.Enabled = enabled;
            textPreSuffix.Enabled = enabled;
            if (enabled)
            {
                IdShieldOutputLocation location =
                    dropFileName.SelectedIndex == 0 ? IdShieldOutputLocation.PrefixFilename :
                    IdShieldOutputLocation.SuffixFilename;
                hiddenOutputLocation.Value = location.ToString("G");
            }
        }

        /// <summary>
        /// Handles enabling/disabling the text control associated
        /// with the mirrored document library folder setting.
        /// </summary>
        /// <param name="enabled">Whether to enable/disable the controls.</param>
        void ToggleMirrorLocation(bool enabled)
        {
            textMirrorOut.Enabled = enabled;
            if (enabled)
            {
                hiddenOutputLocation.Value =
                    IdShieldOutputLocation.MirrorDocumentLibrary.ToString("G");
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
            panelOutputSettings.Visible = false;
            Form.Controls.Remove(panelFileSpecification);
            Form.Controls.Remove(panelFolderSettings);
            Form.Controls.Remove(panelOutputSettings);
            buttonOk.Visible = false;
            buttonCancel.Text = "OK";
        }
    }
}
