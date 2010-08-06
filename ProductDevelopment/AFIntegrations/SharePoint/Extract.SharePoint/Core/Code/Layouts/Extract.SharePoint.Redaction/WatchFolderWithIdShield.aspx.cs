using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections.Generic;

namespace Extract.SharePoint.Layouts.Extract.SharePoint.Redaction
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
            // Check if the page has been loaded or not yet
            if (string.IsNullOrEmpty(hiddenLoaded.Value))
            {
                string currentFolder = Request.QueryString["folder"];
                txtCurrentFolderName.Text = currentFolder;

                SPFeature feature = Web.Features[ExtractSharePointHelper._IDSHIELD_FEATURE_GUID];
                SPFeatureProperty property =
                    feature.Properties[ExtractSharePointHelper._FOLDERS_TO_PROCESS];
                if (property != null)
                {
                    Dictionary<string, FolderProcessingSettings> folderSettings =
                        FolderProcessingSettings.DeserializeFolderSettings(property.Value);

                    FolderProcessingSettings temp;
                    if (folderSettings.TryGetValue(currentFolder, out temp))
                    {
                        txtFileExtension.Text = temp.FileExtensions;
                        chkRecursively.Checked = temp.RecurseSubfolders;
                        chkAdded.Checked = temp.ProcessAddedFiles;
                        chkModified.Checked = temp.ProcessModifiedFiles;
                        hiddenOutputLocation.Value = temp.OutputLocation.ToString("G");
                        switch (temp.OutputLocation)
                        {
                            case IdShieldOutputLocation.ParallelFolderPrefix:
                            case IdShieldOutputLocation.ParallelFolderSuffix:
                                radParallel.Checked = true;
                                txtParallel.Text = temp.OutputLocationString;
                                dropFolderName.SelectedIndex =
                                    temp.OutputLocation == IdShieldOutputLocation.ParallelFolderPrefix ?
                                    0 : 1;
                                break;

                            case IdShieldOutputLocation.SubFolder:
                                radSubFolder.Checked = true;
                                txtSubFolder.Text = temp.OutputLocationString;
                                break;

                            case IdShieldOutputLocation.PrefixFilename:
                            case IdShieldOutputLocation.SuffixFilename:
                                radSameFolder.Checked = true;
                                txtPreSuffix.Text = temp.OutputLocationString;
                                dropFilename.SelectedIndex =
                                    temp.OutputLocation == IdShieldOutputLocation.PrefixFilename ?
                                    0 : 1;
                                break;

                            case IdShieldOutputLocation.CustomOutputLocation:
                                radCustomOutput.Checked = true;
                                txtCustomOut.Text = temp.OutputLocationString;
                                break;
                        }
                    }
                }
                else
                {
                    radParallel.Checked = true;
                }

                toggleParallel(radParallel.Checked);
                toggleSubfolder(radSubFolder.Checked);
                toggleSameFolder(radSameFolder.Checked);
                toggleCustom(radCustomOutput.Checked);

                // Page is loaded, set the loaded value
                hiddenLoaded.Value = "Loaded";
            }
        }

        /// <summary>
        /// Handles the drop down changed event for the same folder prefix/suffix drop down.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void PrefixSuffixDropdownChanged(object sender, EventArgs e)
        {
            IdShieldOutputLocation location = dropFilename.SelectedIndex == 0 ?
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
        protected void btnOkClick(object sender, EventArgs e)
        {
            string locationString = string.Empty;
            IdShieldOutputLocation location = (IdShieldOutputLocation)
                Enum.Parse(typeof(IdShieldOutputLocation), hiddenOutputLocation.Value);
            switch(location)
            {
                case IdShieldOutputLocation.ParallelFolderPrefix:
                case IdShieldOutputLocation.ParallelFolderSuffix:
                    locationString = txtParallel.Text;
                    break;

                case IdShieldOutputLocation.SubFolder:
                    locationString = txtSubFolder.Text;
                    break;

                case IdShieldOutputLocation.PrefixFilename:
                case IdShieldOutputLocation.SuffixFilename:
                    locationString = txtPreSuffix.Text;
                    break;

                case IdShieldOutputLocation.CustomOutputLocation:
                    locationString = txtCustomOut.Text;
                    break;
            }

            FolderProcessingSettings currentFolderSettings = new FolderProcessingSettings(
                txtCurrentFolderName.Text, txtFileExtension.Text, chkRecursively.Checked,
                chkAdded.Checked, chkModified.Checked, location, locationString);

            Dictionary<string, FolderProcessingSettings> folderSettings;

            // Get the ID Shield feature
            SPFeature feature = Web.Features[ExtractSharePointHelper._IDSHIELD_FEATURE_GUID];
            SPFeatureProperty property =
                feature.Properties[ExtractSharePointHelper._FOLDERS_TO_PROCESS];
            if (property != null)
            {
                folderSettings =
                    FolderProcessingSettings.DeserializeFolderSettings(property.Value);
            }
            else
            {
                folderSettings = new Dictionary<string, FolderProcessingSettings>();
            }
            folderSettings[txtCurrentFolderName.Text] = currentFolderSettings;

            string value = FolderProcessingSettings.SerializeFolderSettings(folderSettings);
            if (property == null)
            {
                property = new SPFeatureProperty(ExtractSharePointHelper._FOLDERS_TO_PROCESS,
                    value);
                feature.Properties.Add(property);
            }
            else
            {
                property.Value = value;
            }

            // Update the properties
            feature.Properties.Update();

            // Send a response to close the dialog
            Context.Response.Write(
                "<script type=\"text/javascript\">"
                + "window.frameElement.commitPopup();"
                + "</script>"
                );
            Context.Response.Flush();
            Context.Response.End();
        }

        /// <summary>
        /// Handles the cancel button click for the configuration page.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void btnCancelClick(object sender, EventArgs e)
        {
            // Send a response to close the dialog
            Context.Response.Write(
                "<script type=\"text/javascript\">"
                + "window.frameElement.commitPopup();"
                + "</script>"
                );
            Context.Response.Flush();
            Context.Response.End();
        }

        /// <summary>
        /// Handles the radio button changed event for each of the folder setting
        /// radion button controls.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void RadioButtonChanged(object sender, EventArgs e)
        {
            toggleParallel(radParallel.Checked);
            toggleSubfolder(radSubFolder.Checked);
            toggleSameFolder(radSameFolder.Checked);
            toggleCustom(radCustomOutput.Checked);
        }

        /// <summary>
        /// Handles enabling/disabling the text control associated with the parallel
        /// folder setting.
        /// </summary>
        /// <param name="enabled">Whether to enable/disable the controls.</param>
        void toggleParallel(bool enabled)
        {
            txtParallel.Enabled = enabled;
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
        void toggleSubfolder(bool enabled)
        {
            txtSubFolder.Enabled = enabled;
            if (enabled)
            {
                hiddenOutputLocation.Value =
                    IdShieldOutputLocation.SubFolder.ToString("G");
            }
        }

        /// <summary>
        /// Handles enabling/disabling the text and drop down control associated
        /// with the same folder folder setting.
        /// </summary>
        /// <param name="enabled">Whether to enable/disable the controls.</param>
        void toggleSameFolder(bool enabled)
        {
            dropFilename.Enabled = enabled;
            txtPreSuffix.Enabled = enabled;
            if (enabled)
            {
                IdShieldOutputLocation location =
                    dropFilename.SelectedIndex == 0 ? IdShieldOutputLocation.PrefixFilename :
                    IdShieldOutputLocation.SuffixFilename;
                hiddenOutputLocation.Value = location.ToString("G");
            }
        }

        /// <summary>
        /// Handles enabling/disabling the text control associated
        /// with the custom folder folder setting.
        /// </summary>
        /// <param name="enabled">Whether to enable/disable the controls.</param>
        void toggleCustom(bool enabled)
        {
            txtCustomOut.Enabled = enabled;
            if (enabled)
            {
                hiddenOutputLocation.Value =
                    IdShieldOutputLocation.CustomOutputLocation.ToString("G");
            }
        }
    }
}
