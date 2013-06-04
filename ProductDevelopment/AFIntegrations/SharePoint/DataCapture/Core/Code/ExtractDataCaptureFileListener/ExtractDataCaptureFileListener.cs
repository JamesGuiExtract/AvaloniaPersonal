using Microsoft.SharePoint;
using System;
using System.Collections.Generic;

// Using statements to make dealing with folder settings more readable
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>;

namespace Extract.SharePoint.DataCapture
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class ExtractDataCaptureFileListener : SPItemEventReceiver
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

       /// <summary>
       /// An item was added.
       /// </summary>
       public override void ItemAdded(SPItemEventProperties properties)
       {
            try
            {
                HandleSharePointFileEvent(properties, FileEventType.FileAdded);
            }
            catch (Exception ex)
            {
                LogException(ex, "ELI31481");
            }
           base.ItemAdded(properties);
       }

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

                // Get the folder name for the item
                string folder = item.Url;
                fileName = item.Name;
                folder = (folder[0] != '/' ?
                    folder.Insert(0, "/") : folder).Replace("/" + fileName, "");

                // Attempt to get the settings for the folder
                foreach (KeyValuePair<string, FolderProcessingSettings> pair in _folderSettings)
                {
                    // Set the file as "ToBeQueued" iff
                    // 1. The folder is being watched for the specified event
                    // 2. This folder is being watched
                    // 3. The file matches the watch pattern
                    if ((pair.Value.EventTypes & eventType) != 0
                            && IsFolderBeingWatched(folder, pair.Key, pair.Value.RecurseSubfolders)
                            && pair.Value.DoesFileMatchPattern(fileName)
                        )
                    {
                        ExtractSharePointHelper.DoWithCheckoutIfRequired("ELI35904", item.File, "Updated Data Capture status.", () =>
                        {
                            item[DataCaptureHelper.ExtractDataCaptureStatusColumn] =
                                ExtractProcessingStatus.ToBeQueued.AsString();
                            item.Update();
                        });

                        // File was set to ToBeQueued, break from foreach loop
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
                _folderSettings = DataCaptureHelper.GetDataCaptureFolderSettings(siteId);
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

            DataCaptureHelper.LogException(ex, ErrorCategoryId.DataCaptureFileReceiver,
                eliCode);
        }

        #endregion Methods
    }
}
