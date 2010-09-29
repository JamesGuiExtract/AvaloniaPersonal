using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;

// Using statements to make dealing with folder settings more readable
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>;
using IdShieldFolderSettingsCollection =
System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>>;

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
                string siteId = Request.QueryString["siteid"];
                hiddenSiteId.Value = siteId;
                Guid siteGuid = new Guid(siteId);
                string currentFolder = ExtractSharePointHelper.GetSiteRelativeFolderPath(
                    Request.QueryString["folder"], siteGuid);

                textCurrentFolderName.Text = currentFolder;

                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
                if (settings == null)
                {
                    radioParallel.Checked = true;
                    ToggleAllOutputLocations();
                    return;
                }

                SortedDictionary<string, FolderProcessingSettings> folderSettings =
                    FolderProcessingSettings.DeserializeFolderSettings(settings.FolderSettings,
                    siteGuid);

                // Search the collection of all folders being watched
                string rootKey = string.Empty;
                FolderProcessingSettings temp = null;
                bool watchingSubfolders = false;
                bool watchingCurrentFolder = false;
                bool watchingParentRecursively = false;
                foreach (KeyValuePair<string, FolderProcessingSettings> pair in folderSettings)
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
                        && key.StartsWith(currentFolder, StringComparison.Ordinal))
                    {
                        watchingSubfolders = true;
                    }
                }

                if (watchingParentRecursively)
                {
                    // Hide the unnecessary controls
                    HideNormalControls();
                    panelCannotWatch.Visible = true;
                    labelCannotWatch.Text = "Cannot add watch to this folder since "
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
                    checkAdded.Checked = temp.ProcessAddedFiles;
                    checkModified.Checked = temp.ProcessModifiedFiles;
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

                ToggleAllOutputLocations();
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

                FolderProcessingSettings currentFolderSettings = new FolderProcessingSettings(
                    textCurrentFolderName.Text, textFileExtension.Text, checkRecursively.Checked,
                    checkAdded.Checked, checkModified.Checked, location, locationString);


                // Need to run with elevated privileges in order to update the
                // ID Shield settings object
                SPUtility.ValidateFormDigest();
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(true);
                    IdShieldFolderSettingsCollection siteSettings =
                                FolderProcessingSettings.DeserializeFolderSettings(settings.FolderSettings);
                    SiteFolderSettingsCollection folderSettings;
                    Guid siteId = new Guid(hiddenSiteId.Value);
                    if (!siteSettings.TryGetValue(siteId, out folderSettings))
                    {
                        folderSettings = new SiteFolderSettingsCollection();
                    }
                    folderSettings[textCurrentFolderName.Text] = currentFolderSettings;
                    siteSettings[siteId] = folderSettings;

                    settings.FolderSettings =
                        FolderProcessingSettings.SerializeFolderSettings(siteSettings);

                    // Update the settings
                    settings.Update();
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
        /// Handles the radioio button changed event for each of the folder setting
        /// radioion button controls.
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
