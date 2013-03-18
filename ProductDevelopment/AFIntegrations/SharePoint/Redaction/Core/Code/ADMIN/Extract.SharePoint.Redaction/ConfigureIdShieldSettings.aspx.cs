using Microsoft.SharePoint;
using Microsoft.SharePoint.ApplicationPages;
using System;
using System.Globalization;
using System.Reflection;
using System.Web.UI;

// Using statements to make dealing with folder settings more readable
using IdShieldFolderSettingsCollection = System.Collections.Generic.Dictionary<System.Guid,
    System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.Redaction.IdShieldFolderProcessingSettings>>;

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

                // Get the version from the IDShield assembly
                var version = Assembly.GetAssembly(typeof(IdShieldSettings))
                    .GetName()
                    .Version
                    .ToString();
                panelLogo.GroupingText = panelLogo.GroupingText.Replace("Unknown", version);

                dropRandomFolderLength.SelectedIndex = 0;
                textTimeToWait.Text = "1";
                InitializeForm();
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
                textSiteSettingsList.Text = IdShieldHelper.GetSiteSettingsListUrl(siteId);
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
                var siteSettings = IdShieldFolderProcessingSettings.DeserializeFolderSettings(
                    hiddenSerializedSettings.Value, siteId);
                IdShieldFolderProcessingSettings folderSettings = null;
                if (siteSettings.TryGetValue(folder, out folderSettings))
                {
                    var folderId = folderSettings.FolderId;
                    IdShieldHelper.RemoveWatchingForFolder(siteId, folderId);
                }

                textWatchFolderSettings.Text = string.Empty;

                // Reset the form to update the serialized folder settings
                InitializeForm(siteId);
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

                var siteSettings =
                    IdShieldFolderProcessingSettings.DeserializeFolderSettings(
                    hiddenSerializedSettings.Value, siteId);

                IdShieldFolderProcessingSettings folderSettings = null;
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

                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(true);
                settings.LocalWorkingFolder = folder;
                settings.ExceptionServiceIPAddress = textExceptionIpAddress.Text.Trim();
                settings.RandomFolderNameLength = dropRandomFolderLength.SelectedIndex;
                settings.MinutesToWaitToQueuedLater = int.Parse(textTimeToWait.Text,
                    CultureInfo.CurrentCulture);
                settings.RedactNowFpsFile = textFpsFileLocation.Text;
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
        /// Handles the refresh settings button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void HandleRefreshSettings(object sender, EventArgs e)
        {
            try
            {
                var siteId = GetSiteIdFromSelection();
                InitializeForm(siteId);
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldSettingsConfiguration,
                    "ELI31275");
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
        /// <param name="siteId"></param>
        void UpdateListForCurrentSiteSelection(Guid siteId)
        {
            listWatchedFolders.Items.Clear();
            var siteSettings = IdShieldFolderProcessingSettings.DeserializeFolderSettings(
                hiddenSerializedSettings.Value, siteId);
            if (siteSettings != null)
            {
                foreach (string key in siteSettings.Keys)
                {
                    listWatchedFolders.Items.Add(key);
                }
            }
        }

        /// <summary>
        /// Initializes the form.
        /// </summary>
        void InitializeForm()
        {
            InitializeForm(Guid.Empty);
        }

        /// <summary>
        /// Initializes the form.
        /// </summary>
        /// <param name="selectedSiteId">The selected site id.</param>
        void InitializeForm(Guid selectedSiteId)
        {
            // Ensure the drop down list is cleared
            dropWatchedSites.Items.Clear();

            IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
            if (settings == null)
            {
                return;
            }

            // If GUID is empty then this is the page load, load settings from
            // the settings class.
            if (selectedSiteId == Guid.Empty)
            {
                textFolder.Text = settings.LocalWorkingFolder;
                textExceptionIpAddress.Text = settings.ExceptionServiceIPAddress;
                textTimeToWait.Text =
                    settings.MinutesToWaitToQueuedLater.ToString(CultureInfo.CurrentCulture);
                textFpsFileLocation.Text = settings.RedactNowFpsFile;
                var randomLength = settings.RandomFolderNameLength;
                if (randomLength < 0 || randomLength > 3)
                {
                    randomLength = 0;
                }

                dropRandomFolderLength.SelectedIndex = randomLength;
            }

            var siteIds = settings.IdShieldSites;
            string selectedSiteLabel = "";
            if (siteIds.Count > 0)
            {
                var idShieldSettings = new IdShieldFolderSettingsCollection();
                foreach (var siteId in siteIds)
                {
                    using (var site = new SPSite(siteId))
                    {
                        var folderSettings = IdShieldHelper.GetIdShieldFolderSettings(siteId);
                        idShieldSettings.Add(siteId, folderSettings);
                        string tempSiteId = siteId.ToString("D", CultureInfo.CurrentCulture);
                        string siteLabel = "Site Url: " + site.ServerRelativeUrl
                            + " Site ID: " + tempSiteId;
                        dropWatchedSites.Items.Add(siteLabel);
                        if (siteId == selectedSiteId)
                        {
                            selectedSiteLabel = siteLabel;
                        }
                    }
                }

                // Serialize the folder settings (NOTE: this must be done before
                // calling update list for current site selection)
                hiddenSerializedSettings.Value = idShieldSettings.ToSerializedHexString();

                if (!string.IsNullOrEmpty(selectedSiteLabel))
                {
                    dropWatchedSites.SelectedValue = selectedSiteLabel;
                    UpdateListForCurrentSiteSelection(selectedSiteId);
                }
                else
                {
                    dropWatchedSites.SelectedIndex = 0;
                    UpdateListForCurrentSiteSelection(GetSiteIdFromSelection());
                }

                textSiteSettingsList.Text = IdShieldHelper.GetSiteSettingsListUrl(GetSiteIdFromSelection());
            }
            else
            {
                hiddenSerializedSettings.Value = new IdShieldFolderProcessingSettings().ToSerializedHexString();
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
