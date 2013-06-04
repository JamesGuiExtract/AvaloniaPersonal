using Extract.Sharepoint;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;

// Using statements to make dealing with folder settings more readable
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.Redaction.IdShieldFolderProcessingSettings>;

namespace Extract.SharePoint.Redaction
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class IdShieldFileListener : SPItemEventReceiver
    {
        #region Fields

        /// <summary>
        /// Collection to manage the current folder watch settings.
        /// </summary>
        SiteFolderSettingsCollection _folderSettings =
            new SiteFolderSettingsCollection();

        /// <summary>
        /// Mutex used to serialize access to the UpdateSettings calls.
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Event Handlers

        /// <summary>
        /// An item was added.
        /// </summary>
        /// <param name="properties">The properties associated with the item event.</param>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            try
            {
                HandleSharePointFileEvent(properties, FileEventType.FileAdded);
            }
            catch (Exception ex)
            {
                LogException(ex, "ELI30568");
            }

            base.ItemAdded(properties);
        }

        /// <summary>
        /// An item was updated
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            try
            {
                HandleSharePointFileEvent(properties, FileEventType.FileModified);
            }
            catch (Exception ex)
            {
                LogException(ex, "ELI35629");
            }
            base.ItemUpdated(properties);
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Handles the file event, checking the current settings and exports any files
        /// that match the current settings to the specified processing folder.
        /// </summary>
        /// <param name="properties">The properties for the file event.</param>
        /// <param name="eventType">The type of event that is being handled.</param>
        void HandleSharePointFileEvent(SPItemEventProperties properties,
            FileEventType eventType)
        {
            SPSite site = null;
            string fileName = string.Empty;
            try
            {
                // Get the item and check that it is a file item
                SPListItem item = properties.ListItem;
                if (item.FileSystemObjectType != SPFileSystemObjectType.File
                    || item.File == null)
                {
                    return;
                }

                // Update the settings
                site = item.Web.Site;
                UpdateSettings(site.ID);

                // Build full url for file being exported
                string fullFileUrl = site.Url + "/" + item.File.Url;

                // Check if this file should be ignored
                if (IgnoreFile(site.ID, fullFileUrl))
                {
                    return;
                }

                // Get the folder name for the item
                string folder = item.Url;
                fileName = item.Name;
                folder = (folder[0] != '/' ?
                    folder.Insert(0, "/") : folder).Replace("/" + fileName, "");

                // Attempt to get the settings for the folder
                foreach (KeyValuePair<string, IdShieldFolderProcessingSettings> pair in _folderSettings)
                {
                    IdShieldFolderProcessingSettings idShieldSettings = pair.Value;

                    if (shouldBeSetToBeQueued(eventType, folder, pair.Key, fileName, item, pair.Value))
                    {
                         ExtractSharePointHelper.DoWithCheckoutIfRequired("ELI35883", item.File, "IDS Status changed.", () =>
                         {
                            // Set the field to ToBeQueued
                            item[IdShieldHelper.IdShieldStatusColumn] =
                                ExtractProcessingStatus.ToBeQueued.AsString();

                            item.Update();
                         });

                        // File was set to ToBeQueuedLater, break from foreach loop
                        break;
                    }
                } // End foreach loop
            }
            catch (Exception ex)
            {
                SPException ee = new SPException("Unable to handle SharePoint file event.", ex);
                if (!string.IsNullOrEmpty(fileName))
                {
                    ee.Data["File To Handle"] = fileName;
                }
                if (site != null)
                {
                    try
                    {
                        // Attempt to add additional site debug data [FIDSI #189]
                        ee.Data["Current Site Url"] = site.ServerRelativeUrl;
                        ee.Data["Current Site Id"] = site.ID.ToString();
                    }
                    catch
                    {
                    }
                }

                throw ee;
            }
        }

        /// <summary>
        /// Checks the conditions for queueing the file
        /// </summary>
        /// <param name="eventType">The type of event that is being handled</param>
        /// <param name="folder">The folder the <see param="fileName"/> is in</param>
        /// <param name="folderWatched">The folder the <see param="settings"/> are from</param>
        /// <param name="fileName">Name of the files that was added or modified</param>
        /// <param name="item">The item being added or modified</param>
        /// <param name="settings">The settings that are used to determine if the file should be queued</param>
        /// <returns><see langword="true"/> if file should be set to be queued <see langword="false"/> if file
        /// should not be queued</returns>
        bool shouldBeSetToBeQueued(
            FileEventType eventType,
            string folder,
            string folderWatched,
            string fileName,
            SPListItem item,
            IdShieldFolderProcessingSettings settings)
        {
            // If this is not an event we are handling file should not be queued
            if ((settings.EventTypes & eventType) == 0)
            {
                return false;
            }

            // If folder is not being watched file should not be queued
            if (!IsFolderBeingWatched(folder, folderWatched, settings.RecurseSubfolders))
            {
                return false;
            }

            // If file does not match the pattern of file being watched file should not be queued
            if (!settings.DoesFileMatchPattern(fileName))
            {
                return false;
            }

            // Check if the QueueWithFieldValue settings should be checked
            if (settings.QueueWithFieldValue)
            {
                // if event is not the file modified event the file should not be queued
                if (eventType != FileEventType.FileModified)
                {
                    return false;
                }

                // if the value of the field does not match value we are looking for file should not be queued
                if (!ExtractSharePointHelper.IsFieldEqual(item, settings))
                {
                    return false;
                }

                // if the IDShieldStatus is not processed, queue the file
                if ((string)item[IdShieldHelper.IdShieldStatusColumn] ==
                    ExtractProcessingStatus.NotProcessed.AsString())
                {
                    return true;
                }

                // No conditions were met for the QueueWithFieldValue so file should not be queued
                return false;
            }

            // Only set file to be queued if this is a file added event
            if (eventType == FileEventType.FileAdded)
            {
                return true;
            }


            return false;

        }

        /// <summary>
        /// Checks whether the specified file should be ignored. If it is in the ignore
        /// list this method will return <see langword="true"/>, it also has the side
        /// effect that the item will be removed from the ignore list.
        /// </summary>
        /// <param name="siteId">The unique ID for the site containing the list of
        /// files to ignore.</param>
        /// <param name="fullFileUrl">The full URL to the file (this is the value
        /// that will be in the hidden list if the file should be ignored).</param>
        /// <returns><see langword="true"/> if the file should be ignored and
        /// <see langword="false"/> otherwise.</returns>
        static bool IgnoreFile(Guid siteId, string fullFileUrl)
        {
            bool result = false;
            using (SPSite site = new SPSite(siteId))
            {
                SPList list = ExtractSharePointHelper.GetSpecifiedList(site,
                    IdShieldHelper._HIDDEN_IGNORE_FILE_LIST);
                if (list != null)
                {
                    SPQuery q = new SPQuery();
                    q.Query = new camlFieldRef("Title").Eq(new camlValue("Text", fullFileUrl)).Where();

                    SPListItemCollection items = list.GetItems(q);
                    result = items != null && items.Count > 0;
                }

                return result;
            }
        }

        /// <summary>
        /// Checks whether the specified folder is being watched based on the 
        /// specified watch path and recursion settings.
        /// </summary>
        /// <param name="folder">The folder to check.</param>
        /// <param name="watchPath">The root watch path to compare.</param>
        /// <param name="recurseSubFolders">Whether sub folders are recursively watched.</param>
        /// <returns><see langword="true"/> if the folder is being watched and
        /// <see langword="false"/> otherwise.</returns>
        static bool IsFolderBeingWatched(string folder, string watchPath, bool recurseSubFolders)
        {
            return folder.Equals(watchPath, StringComparison.Ordinal)
                || (recurseSubFolders && folder.StartsWith(watchPath + "/", StringComparison.Ordinal));
        }

        /// <summary>
        /// Checks the feature and ensures the settings are updated.
        /// </summary>
        void UpdateSettings(Guid siteId)
        {
            lock (_lock)
            {
                _folderSettings = IdShieldHelper.GetIdShieldFolderSettings(siteId);
            }
        }

        /// <summary>
        /// Attempts to logs exceptions to the exception logging service.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="eliCode">The ELI code to associate with the exception.</param>
        static void LogException(Exception ex, string eliCode)
        {
            ex.Data.Add("User Name", Environment.UserName);
            try
            {
                ex.Data.Add("User Domain Name", Environment.UserDomainName);
            }
            catch
            {
            }

            IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldFileReceiver,
                eliCode);
        }

        #endregion Methods
    }
}
