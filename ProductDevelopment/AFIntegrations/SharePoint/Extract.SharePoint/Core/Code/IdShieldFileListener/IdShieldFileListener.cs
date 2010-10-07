using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

// Using statements to make dealing with folder settings more readable
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>;
using IdShieldFolderSettingsCollection =
System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>>;

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
        /// Holds the folder serialization string so that it can be compared
        /// and deserialized if the settings are updated.
        /// </summary>
        string _folderSettingsSerializationString = string.Empty;

        /// <summary>
        /// The output folder that files should be written to for processing
        /// </summary>
        string _outputFolder = string.Empty;

        /// <summary>
        /// Collection of zero byte files from the add event
        /// </summary>
        HashSet<Guid> _zeroByteFiles = new HashSet<Guid>();

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
        /// An item was updated.
        /// </summary>
        /// <param name="properties">The properties associated with the item event.</param>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            try
            {
                HandleSharePointFileEvent(properties, FileEventType.FileModified);
            }
            catch (Exception ex)
            {
                LogException(ex, "ELI30569");
            }

            base.ItemUpdated(properties);
        }

        /// <summary>
        /// An item was deleted
        /// </summary>
        /// <param name="properties">The properties associated with the item event.</param>
        public override void ItemDeleted(SPItemEventProperties properties)
        {
            try
            {
                HandleSharePointItemDeleted(properties);
            }
            catch (Exception ex)
            {
                LogException(ex, "ELI30613");
            }

            base.ItemDeleted(properties);
        }

        /// <summary>
        /// An item is updating.
        /// </summary>
        /// <param name="properties">The properties associated with the item event.</param>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            try
            {
                if (properties.Cancel)
                {
                    return;
                }

                SPListItem item = properties.ListItem;
                if (item != null && item.FileSystemObjectType == SPFileSystemObjectType.Folder)
                {
                    // Get the old and new folder values
                    string oldFolder = properties.BeforeUrl;
                    string newFolder = properties.AfterUrl;
                    if (!oldFolder.StartsWith("/", StringComparison.Ordinal))
                    {
                        oldFolder = "/" + oldFolder;
                    }
                    if (!newFolder.StartsWith("/", StringComparison.Ordinal))
                    {
                        newFolder = "/" + newFolder;
                    }

                    // Update the settings
                    IdShieldSettings.UpdateSettingsForRenamedFolder(oldFolder, newFolder,
                        properties.SiteId);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "ELI30614");
            }

            base.ItemUpdating(properties);
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Handles the ItemDeleted event.
        /// </summary>
        /// <param name="properties">The item properties associated with the
        /// ItemDeleted event.</param>
        static void HandleSharePointItemDeleted(SPItemEventProperties properties)
        {
            string folder = null;
            try
            {
                // Get the possible folder name
                folder = properties.BeforeUrl;
                if (!folder.StartsWith("/", StringComparison.Ordinal))
                {
                    folder = "/" + folder;
                }

                // Attempt to remove the folder watching (this call does nothing if the
                // folder is not currently being watched).
                IdShieldSettings.RemoveFolderWatching(folder, properties.Web.Site.ID, true);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    ex.Data.Add("Folder Name", folder);
                }

                throw;
            }
        }

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

                // Check for an output folder (if none is configured then do nothing)
                if (!string.IsNullOrEmpty(_outputFolder))
                {
                    // Get the folder name for the item
                    string folder = item.Url;
                    fileName = item.Name;
                    folder = (folder[0] != '/' ?
                        folder.Insert(0, "/") : folder).Replace("/" + fileName, "");

                    // Attempt to get the settings for the folder
                    foreach (KeyValuePair<string, FolderProcessingSettings> pair in _folderSettings)
                    {
                        // Export the file if:
                        // 1. This is a modified event and the file id is contained in the
                        //      zero byte file list
                        // OR
                        // 1. The folder is being watched for the specified event
                        // 2. This folder is being watched
                        // 3. The file matches the watch pattern
                        if ((eventType == FileEventType.FileModified
                                && _zeroByteFiles.Contains(item.UniqueId))
                            || ((pair.Value.EventTypes & eventType) != 0
                                && IsFolderBeingWatched(folder, pair.Key, pair.Value.RecurseSubfolders)
                                && pair.Value.DoesFileMatchPattern(fileName)
                            ))
                        {
                            using (SPSite tempSite = new SPSite(properties.SiteId))
                            using (SPWeb web = tempSite.OpenWeb(item.Web.ID))
                            {
                                SPList list = web.Lists[item.ParentList.ID];
                                SPListItem fileItem = list.GetItemByUniqueId(item.UniqueId);



                                byte[] bytes = fileItem.File.OpenBinary(SPOpenBinaryOptions.SkipVirusScan);
                                if (bytes.Length == 0 && eventType == FileEventType.FileAdded)
                                {
                                    // Add this file ID to a list of pending ID's to be
                                    // handled in the update method
                                    IdShieldSettings.AddZeroByteFileId(fileItem.File.UniqueId);
                                }
                                else if (bytes.Length > 0)
                                {
                                    // get the folder name without the leading '/' and
                                    // convert all other '/' to '\'
                                    folder = folder.Substring(1).Replace('/', '\\');
                                    string outputFolder = Path.Combine(_outputFolder, folder);
                                    if (!Directory.Exists(outputFolder))
                                    {
                                        Directory.CreateDirectory(outputFolder);
                                    }

                                    // Write the file to the processing folder
                                    string outFileName = Path.Combine(outputFolder, fileItem.File.Name);
                                    File.WriteAllBytes(outFileName, bytes);

                                    if (_zeroByteFiles.Contains(fileItem.File.UniqueId))
                                    {
                                        IdShieldSettings.RemoveZeroByteFileId(fileItem.File.UniqueId);
                                    }
                                }
                            }

                            // File was exported OR placed in the zero byte list,
                            // break from foreach loop
                            break;
                        }
                    } // End foreach loop
                }
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
            using (SPWeb web = site.RootWeb)
            {
                SPList list = web.Lists.TryGetList(IdShieldHelper._HIDDEN_LIST_NAME);
                if (list != null)
                {
                    SPQuery q = new SPQuery();
                    q.Query = "<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>"
                        + fullFileUrl + "</Value></Eq></Where>";
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
                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
                if (settings == null)
                {
                    _outputFolder = string.Empty;
                    _folderSettings = null;
                    _folderSettingsSerializationString = string.Empty;
                    return;
                }
                string temp = settings.FolderSettings;
                if (temp.Length != _folderSettingsSerializationString.Length
                    || !temp.Equals(_folderSettingsSerializationString, StringComparison.Ordinal))
                {
                    _folderSettings =
                        FolderProcessingSettings.DeserializeFolderSettings(temp, siteId);
                    _folderSettingsSerializationString = temp;
                }

                if (!string.IsNullOrEmpty(settings.LocalWorkingFolder))
                {
                    _outputFolder = Path.Combine(settings.LocalWorkingFolder, siteId.ToString());
                }

                _zeroByteFiles = new HashSet<Guid>(settings.AddedZeroByteFiles);
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
