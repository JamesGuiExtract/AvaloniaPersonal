using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.ApplicationPages;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

// Using statements to make dealing with folder settings more readable
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>;
using IdShieldFolderSettingsCollection =
System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>>;

namespace Extract.SharePoint.Redaction.Layouts
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

                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
                if (settings == null)
                {
                    return;
                }

                textFolder.Text = settings.LocalWorkingFolder;
                textExceptionIpAddress.Text = settings.ExceptionServiceIPAddress;
                IdShieldFolderSettingsCollection allSettings =
                    FolderProcessingSettings.DeserializeFolderSettings(settings.FolderSettings);
                if (allSettings != null)
                {
                    bool filled = false;
                    foreach (KeyValuePair<Guid, SiteFolderSettingsCollection> pair in allSettings)
                    {
                        using (SPSite site = new SPSite(pair.Key))
                        {
                            string tempSiteId = site.ID.ToString("D", CultureInfo.CurrentCulture);
                            string siteLabel = "Site Url: " + site.ServerRelativeUrl
                                + " Site ID: " + tempSiteId;
                            dropWatchedSites.Items.Add(siteLabel);

                            if (!filled)
                            {
                                foreach (string folder in pair.Value.Keys)
                                {
                                    listWatchedFolders.Items.Add(folder);
                                }

                                filled = true;
                            }
                        }
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
                string folder = listWatchedFolders.SelectedValue;
                Guid siteId = GetSiteIdFromSelection();

                // Get the string from the drop down list
                IdShieldSettings.RemoveFolderWatching(folder, siteId, false);

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
                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
                if (settings == null)
                {
                    return;
                }

                SiteFolderSettingsCollection siteSettings =
                    FolderProcessingSettings.DeserializeFolderSettings(
                    settings.FolderSettings, siteId);

                FolderProcessingSettings folderSettings = null;
                if (siteSettings.TryGetValue(listWatchedFolders.SelectedValue, out folderSettings))
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
        protected void HandleOkButtonClick(object sender, EventArgs e)
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

                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(true);
                settings.LocalWorkingFolder = folder;
                settings.ExceptionServiceIPAddress = textExceptionIpAddress.Text.Trim();
                settings.Update();
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration,
                    "ELI30553");
                throw;
            }

            // Redirect back to the application management page
            // This method raises a thread aborted exception (by design) and so should not
            // be wrapped in a try catch
            SPUtility.Redirect("/applications.aspx", SPRedirectFlags.Default, this.Context);
        }

        /// <summary>
        /// Handles the cancel button click from the configuration page
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        protected void HandleCancelButtonClick(object sender, EventArgs e)
        {
            // Redirect back to the application management page
            // This method raises a thread aborted exception (by design) and so should not
            // be wrapped in a try catch
            SPUtility.Redirect("/applications.aspx", SPRedirectFlags.Default, this.Context);
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
        /// <param name="siteId"></param>
        void UpdateListForCurrentSiteSelection(Guid siteId)
        {
            listWatchedFolders.Items.Clear();
            IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
            SiteFolderSettingsCollection siteSettings =
                FolderProcessingSettings.DeserializeFolderSettings(
                settings.FolderSettings, siteId);
            if (siteSettings != null)
            {
                foreach (string key in siteSettings.Keys)
                {
                    listWatchedFolders.Items.Add(key);
                }
            }
        }
    }
}
