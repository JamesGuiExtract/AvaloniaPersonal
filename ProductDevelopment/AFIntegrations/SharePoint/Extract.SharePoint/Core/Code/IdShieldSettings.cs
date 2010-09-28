using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

// Using statements to make dealing with folder settings more readable
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>;
using IdShieldFolderSettingsCollection =
System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>>;

namespace Extract.SharePoint.Redaction
{

    /// <summary>
    /// Class to manage settings for ID Shield
    /// </summary>
    [System.Runtime.InteropServices.GuidAttribute("2CC24FE0-EED7-4FED-A64F-38327DAC5795")]
    public class IdShieldSettings : SPPersistedObject
    {
        #region Constants

        /// <summary>
        /// The name for this SPPersisted settings object
        /// </summary>
        internal static readonly string _ID_SHIELD_SETTINGS_NAME = "ESIdShieldSettings";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The local folder that is used for processing
        /// </summary>
        [Persisted]
        string _localWorkingFolder;

        /// <summary>
        /// The serialized version of the folder settings class
        /// </summary>
        [Persisted]
        string _folderSettings;

        /// <summary>
        /// The ip address of the server running the Extract exception service
        /// </summary>
        [Persisted]
        string _exceptionServiceIPAddress;

        /// <summary>
        /// Collection of file Ids that were seen in the added event, but were
        /// 0 bytes at the time.
        /// </summary>
        [Persisted]
        List<Guid> _addedZeroByteFiles;

        /// <summary>
        /// Collection of site ids that currently have the ID Shield feature activated.
        /// </summary>
        [Persisted]
        List<Guid> _activeSiteIds;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldSettings"/> class.
        /// </summary>
        public IdShieldSettings()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldSettings"/> class.
        /// </summary>
        /// <param name="parent">The parent object that contains the persisted settings.</param>
        public IdShieldSettings(SPPersistedObject parent)
            : base(_ID_SHIELD_SETTINGS_NAME, parent)
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Indicates whether this persisted class has additional update access
        /// </summary>
        /// <returns><see langword="true"/></returns>
        protected override bool HasAdditionalUpdateAccess()
        {
            return true;
        }

        /// <summary>
        /// Removes the ID Shield settings from the persisted store. 
        /// </summary>
        internal static void RemoveIdShieldSettings()
        {
            IdShieldSettings settings = GetIdShieldSettings(false);
            if (settings != null)
            {
                settings.Delete();
            }
        }

        /// <summary>
        /// Gets the persisted <see cref="IdShieldSettings"/> object. If
        /// <paramref name="createNew"/> is <see langword="true"/> then
        /// will create a new instance of the <see cref="IdShieldSettings"/>
        /// if no object has been persisted to SharePoint yet. If
        /// <paramref name="createNew"/> is <see langword="false"/> and
        /// no settings have been created yet, this method will return
        /// <see langword="null"/>.
        /// </summary>
        /// <param name="createNew">Whether a new settings object should be
        /// created or not if there is not an existing one in the SharePoint farm.</param>
        /// <returns>The persisted <see cref="IdShieldSettings"/> object.</returns>
        internal static IdShieldSettings GetIdShieldSettings(bool createNew)
        {
            IdShieldSettings settings =
                SPFarm.Local.GetChild<IdShieldSettings>(_ID_SHIELD_SETTINGS_NAME);
            if (settings == null && createNew)
            {
                settings = new IdShieldSettings(SPFarm.Local);
            }

            return settings;
        }

        /// <summary>
        /// Removes the watching for particular folder within a specified site ID.
        /// </summary>
        /// <param name="folder">The folder to stop watching.</param>
        /// <param name="siteId">The site ID for the folder.</param>
        /// <param name="recurse">If <see langword="true"/> then will remove any folder
        /// watching settings for subfolders as well.</param>
        /// <returns><see langword="true"/> if watching was removed and
        /// <see langword="false"/> otherwise.</returns>
        internal static bool RemoveFolderWatching(string folder, Guid siteId, bool recurse)
        {
            bool watchRemoved = false;
            IdShieldSettings settings = GetIdShieldSettings(false);
            if (settings != null)
            {
                IdShieldFolderSettingsCollection siteSettings
                    = FolderProcessingSettings.DeserializeFolderSettings(settings.FolderSettings);
                SiteFolderSettingsCollection folderSettings;
                if (siteSettings.TryGetValue(siteId, out folderSettings))
                {
                    watchRemoved = folderSettings.Remove(folder);
                    if (recurse)
                    {
                        // Since a folder is being removed, need to iterate the list of
                        // all watched folders and ensure we remove the watching for
                        // all subfolders (as these will be deleted when the parent
                        // folder is deleted)
                        string folderPath = folder + "/";
                        List<string> foldersToRemove = new List<string>();
                        foreach (string watchFolder in folderSettings.Keys)
                        {
                            if (watchFolder.StartsWith(folderPath, StringComparison.Ordinal))
                            {
                                foldersToRemove.Add(watchFolder);
                            }
                        }

                        watchRemoved |= foldersToRemove.Count > 0;
                        foreach (string folderToRemove in foldersToRemove)
                        {
                            folderSettings.Remove(folderToRemove);
                        }
                    }

                    // Only update settings if folder was removed from settings
                    if (watchRemoved)
                    {
                        settings.FolderSettings =
                            FolderProcessingSettings.SerializeFolderSettings(siteSettings);
                        settings.Update();
                    }
                }
            }

            return watchRemoved;
        }

        /// <summary>
        /// Handles updating folder watching settings when a watched folder has been
        /// renamed.
        /// </summary>
        /// <param name="oldFolder">The original folder.</param>
        /// <param name="newFolder">The renamed folder.</param>
        /// <param name="siteId">The site ID for the renamed folder.</param>
        internal static void UpdateSettingsForRenamedFolder(string oldFolder,
            string newFolder, Guid siteId)
        {
            IdShieldSettings settings = GetIdShieldSettings(false);
            if (settings != null)
            {
                IdShieldFolderSettingsCollection siteSettings
                    = FolderProcessingSettings.DeserializeFolderSettings(settings.FolderSettings);
                SiteFolderSettingsCollection siteFolderSettings;
                if (siteSettings.TryGetValue(siteId, out siteFolderSettings))
                {
                    FolderProcessingSettings folderSettings = null;;
                    if (siteFolderSettings.TryGetValue(oldFolder, out folderSettings))
                    {
                        // Remove the old folder
                        siteFolderSettings.Remove(oldFolder);

                        // Add the settings back with the new folder name
                        siteFolderSettings.Add(newFolder, folderSettings);

                        // Reserialize the settings
                        settings.FolderSettings =
                            FolderProcessingSettings.SerializeFolderSettings(siteSettings);
                        settings.Update();
                    }
                }
            }
        }

        /// <summary>
        /// Adds the unique file id to the list of files that were seen in the
        /// file added event but were 0 bytes.
        /// </summary>
        /// <param name="fileId">The unique file id for the file to add to the list.</param>
        internal static void AddZeroByteFileId(Guid fileId)
        {
            IdShieldSettings settings = GetIdShieldSettings(true);
            settings.InternalAddZeroByteFileId(fileId);
            settings.Update();
        }

        /// <summary>
        /// Removes the unique file id from the list of files that were seen in the
        /// file added event but were 0 bytes.
        /// </summary>
        /// <param name="fileId">The unique file id for the file to remove from the list.</param>
        internal static void RemoveZeroByteFileId(Guid fileId)
        {
            IdShieldSettings settings = GetIdShieldSettings(false);
            if (settings != null)
            {
                settings.InternalRemoveZeroByteFileId(fileId);
                settings.Update();
            }
        }

        /// <summary>
        /// Adds the specified unique file id to the collection of zero byte file ids.
        /// </summary>
        /// <param name="fileId">The file id to add.</param>
        void InternalAddZeroByteFileId(Guid fileId)
        {
            if (_addedZeroByteFiles == null)
            {
                _addedZeroByteFiles = new List<System.Guid>();
            }

            // Search for the item id
            int index = _addedZeroByteFiles.BinarySearch(fileId);
            if (index < 0)
            {
                // The item was not found, insert it in the proper sorted location
                _addedZeroByteFiles.Insert(~index, fileId);
            }
        }

        /// <summary>
        /// Removes the specified unique file id from the collection of zero byte file ids.
        /// </summary>
        /// <param name="fileId">The file id to remove.</param>
        void InternalRemoveZeroByteFileId(Guid fileId)
        {
            if (_addedZeroByteFiles != null)
            {
                int index = _addedZeroByteFiles.BinarySearch(fileId);
                if (index >= 0)
                {
                    _addedZeroByteFiles.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Adds the specified site id to the collection of active site ids.
        /// </summary>
        /// <param name="siteId">The site id to add to the collection.</param>
        public static void AddActiveFeatureSiteId(Guid siteId)
        {
            IdShieldSettings settings = GetIdShieldSettings(true);
            settings.InternalAddActiveFeatureSiteId(siteId);
            settings.Update();
        }

        /// <summary>
        /// Removes the specified site id from the collection of active site ids.
        /// </summary>
        /// <param name="siteId">The site id to remove from the collection.</param>
        public static void RemoveActiveFeatureSiteId(Guid siteId)
        {
            IdShieldSettings settings = GetIdShieldSettings(false);
            if (settings != null)
            {
                settings.InternalRemoveActiveFeatureSiteId(siteId);
                settings.Update();
            }
        }

        /// <summary>
        /// Adds the specified site id to the collection of active site ids.
        /// </summary>
        /// <param name="siteId">The site id to add to the collection.</param>
        void InternalAddActiveFeatureSiteId(Guid siteId)
        {
            if (_activeSiteIds == null)
            {
                _activeSiteIds = new List<Guid>();
            }

            // Search for the item id
            int index = _activeSiteIds.BinarySearch(siteId);
            if (index < 0)
            {
                // The item was not found, insert it in the proper sorted location
                _activeSiteIds.Insert(~index, siteId);
            }
        }

        /// <summary>
        /// Removes the specified site id from the collection of active site ids.
        /// </summary>
        /// <param name="siteId">The site id to remove from the collection.</param>
        void InternalRemoveActiveFeatureSiteId(Guid siteId)
        {
            if (_activeSiteIds != null)
            {
                int index = _activeSiteIds.BinarySearch(siteId);
                if (index >= 0)
                {
                    _activeSiteIds.RemoveAt(index);
                }
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets/sets the local processing folder.
        /// </summary>
        public string LocalWorkingFolder
        {
            get
            {
                return _localWorkingFolder;
            }
            set
            {
                _localWorkingFolder = (value ?? string.Empty).Trim();
            }
        }

        /// <summary>
        /// Gets/sets the serialized version of the folder settings.
        /// </summary>
        public string FolderSettings
        {
            get
            {
                return _folderSettings ?? string.Empty;
            }
            set
            {
                _folderSettings = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets/sets the ip address for the Extract exception service.
        /// </summary>
        public string ExceptionServiceIPAddress
        {
            get
            {
                return _exceptionServiceIPAddress ?? string.Empty;
            }
            set
            {
                _exceptionServiceIPAddress = (value ?? string.Empty).Trim();
            }
        }

        /// <summary>
        /// Gets the list of zero byte files that were seen during the add event.
        /// </summary>
        public ReadOnlyCollection<Guid> AddedZeroByteFiles
        {
            get
            {
                if (_addedZeroByteFiles == null)
                {
                    _addedZeroByteFiles = new List<Guid>();
                }
                return _addedZeroByteFiles.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the sorted list of ids for sites which have activated the ID Shield feature.
        /// </summary>
        public ReadOnlyCollection<Guid> ActiveSites
        {
            get
            {
                if (_activeSiteIds == null)
                {
                    _activeSiteIds = new List<Guid>();
                }
                return _activeSiteIds.AsReadOnly();
            }

        }

        /// <summary>
        /// Gets the count of sites that currently have the IDShield feature activated.
        /// </summary>
        public int ActiveSiteCount
        {
            get
            {
                return _activeSiteIds != null ? _activeSiteIds.Count : 0;
            }
        }

        /// <summary>
        /// Gets the GUID for this class.
        /// </summary>
        public static Guid Guid
        {
            get
            {
                return typeof(IdShieldSettings).GUID;
            }
        }

        #endregion Properties
    }
}
