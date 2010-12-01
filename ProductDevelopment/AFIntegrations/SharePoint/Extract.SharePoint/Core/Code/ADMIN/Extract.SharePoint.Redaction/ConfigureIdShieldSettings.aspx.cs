using Microsoft.SharePoint;
using Microsoft.SharePoint.ApplicationPages;
using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Extract.SharePoint.Redaction.Administration.Layouts
{
    /// <summary>
    /// Code behind file for the ID Shield configuration page
    /// </summary>
    public partial class ConfigureIdShieldSettings : DialogAdminPageBase
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

                var settings = IdShieldProcessingFeatureSettings.GetIdShieldSettings(false);
                if (settings == null)
                {
                    return;
                }

                textFolder.Text = settings.LocalWorkingFolder;
                textExceptionIpAddress.Text = settings.ExceptionServiceIpAddress;
                var allSettings = settings.AllSiteFolderSettings;
                if (allSettings != null)
                {
                    foreach (Guid key in allSettings.Keys)
                    {
                        using (SPSite site = new SPSite(key))
                        {
                            string tempSiteId = site.ID.ToString("D", CultureInfo.CurrentCulture);
                            string siteLabel = "Site Url: " + site.ServerRelativeUrl
                                + " Site ID: " + tempSiteId;
                            dropWatchedSites.Items.Add(siteLabel);
                        }
                    }

                    if (dropWatchedSites.Items.Count > 0)
                    {
                        dropWatchedSites.SelectedIndex = 0;
                        UpdateListForCurrentSiteSelection(GetSiteIdFromSelection());
                    }
                }
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration,
                    "ELI30552");
                throw;
            }
        }

        /// <summary>
        /// Handles the drop down changed event for the watched sites drop down list.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void HandleWatchedSitesChanged(object sender, EventArgs e)
        {
            try
            {
                Guid siteId = GetSiteIdFromSelection();
                UpdateListForCurrentSiteSelection(siteId);
                textWatchFolderSettings.Text = string.Empty;
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration,
                    "ELI30634");
                throw;
            }
        }

        /// <summary>
        /// Handles the remove watching button click event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void HandleRemoveWatching(object sender, EventArgs e)
        {
            try
            {
                if (listWatchedFolders.SelectedIndex == -1)
                {
                    return;
                }

                // Get the selected string from the list
                var selectedItem = listWatchedFolders.SelectedItem;
                var folderId = new Guid(selectedItem.Value);
                Guid siteId = GetSiteIdFromSelection();

                // Remove the folder watching
                IdShieldProcessingFeatureSettings.RemoveFolderWatching(folderId, siteId);

                // Update list for current selected site
                UpdateListForCurrentSiteSelection(siteId);

                textWatchFolderSettings.Text = string.Empty;
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration,
                    "ELI30635");
                throw;
            }
        }

        /// <summary>
        /// Handles the selection changed event for the watch folder list
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void HandleWatchListSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                // Clear the watch folder settings text box
                textWatchFolderSettings.Text = string.Empty;

                if (listWatchedFolders.SelectedIndex == -1)
                {
                    return;
                }

                Guid siteId = GetSiteIdFromSelection();
                var settings = IdShieldProcessingFeatureSettings.GetIdShieldSettings(false);
                if (settings == null)
                {
                    return;
                }

                IdShieldFolderSettingsCollection siteSettings = null;
                if (!settings.AllSiteFolderSettings.TryGetValue(siteId, out siteSettings))
                {
                    return;
                }

                var folderId = new Guid(listWatchedFolders.SelectedItem.Value);
                IdShieldFolderProcessingSettings folderSettings = null;
                if (siteSettings.TryGetValue(folderId, out folderSettings))
                {
                    textWatchFolderSettings.Text = folderSettings.ComputeHumanReadableSettingString();
                }
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration,
                    "ELI30645");
                throw;
            }
        }

        /// <summary>
        /// Handles the OK button click from the configuration page.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void HandleSaveButtonClick(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            try
            {
                // Remove trailing '\'
                string folder = textFolder.Text.Trim();
                if (folder.EndsWith("\\", StringComparison.Ordinal))
                {
                    folder = folder.Substring(0, folder.Length - 1);
                }

                var settings = IdShieldProcessingFeatureSettings.GetIdShieldSettings(true);
                settings.LocalWorkingFolder = folder;
                settings.ExceptionServiceIpAddress = textExceptionIpAddress.Text.Trim();
                settings.Update();

                DisplaySettingsUpdatedMessage();
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration,
                    "ELI30553");
                throw;
            }
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> for the selected site from the site list drop down.
        /// </summary>
        /// <returns>The <see cref="Guid"/> for the selected site.</returns>
        Guid GetSiteIdFromSelection()
        {
            string temp = dropWatchedSites.Text;
            if (string.IsNullOrEmpty(temp))
            {
                return new Guid();
            }
            temp = temp.Substring(temp.LastIndexOf("Site ID: ", StringComparison.Ordinal) + 9);
            Guid siteId = new Guid(temp);

            return siteId;
        }

        /// <summary>
        /// Updates the list control with the list of watched folders for the
        /// selected site.
        /// </summary>
        /// <param name="siteId">The site ID.</param>
        void UpdateListForCurrentSiteSelection(Guid siteId)
        {
            listWatchedFolders.Items.Clear();
            var settings = IdShieldProcessingFeatureSettings.GetIdShieldSettings(false);
            if (settings == null)
            {
                return;
            }
            IdShieldFolderSettingsCollection siteSettings = null;
            if (!settings.AllSiteFolderSettings.TryGetValue(siteId, out siteSettings))
            {
                return;
            }

            using (var site = new SPSite(siteId))
            {
                SPWeb web = site.RootWeb;
                foreach (var settingPair in siteSettings)
                {
                    if (settingPair.Value != null)
                    {
                        var item = new ListItem(settingPair.Value.GetFolderPath(web),
                            settingPair.Key.ToString("N", CultureInfo.InvariantCulture));
                        listWatchedFolders.Items.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Displays the "Settings updated" message to the user when the save
        /// button is clicked.
        /// </summary>
        protected void DisplaySettingsUpdatedMessage()
        {
            string csname = "PopupScript";
            Type cstype = GetType();

            ClientScriptManager cs = Page.ClientScript;

            if (!cs.IsStartupScriptRegistered(cstype, csname))
            {
                string text = "alert('Settings updated.');";
                cs.RegisterStartupScript(cstype, csname, text, true);
            }
        }
    }
}
