using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Extract.SharePoint.Redaction
{
    [System.Runtime.InteropServices.GuidAttribute("73A99FB1-90BB-460F-B25A-1B7E9E9A1B93")]
    internal class IdShieldProcessingFeatureSettings : ExtractProcessingFeatureSettings
    {
        #region Constants

        /// <summary>
        /// The name appended to these settings when they are persisted
        /// </summary>
        const string _ID_SHIELD_SETTINGS_NAME = "ESIdShieldSettings";

        #endregion Constants

        #region Fields

        /// <summary>
        /// List containing the active site Ids
        /// </summary>
        [Persisted]
        List<Guid> _activeSiteIds;

        /// <summary>
        /// Collection containing all of the folder settings for all sites.
        /// Key - Site Id
        /// Value - Folder settings for the site
        /// </summary>
        [Persisted]
        Dictionary<Guid, IdShieldFolderSettingsCollection> _allSiteFolderSettings;

        [Persisted]
        List<Guid> _zeroByteFiles;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldProcessingFeatureSettings"/> class.
        /// </summary>
        public IdShieldProcessingFeatureSettings()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldProcessingFeatureSettings"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public IdShieldProcessingFeatureSettings(SPPersistedObject parent)
            : base(_ID_SHIELD_SETTINGS_NAME, parent)
        {
            _allSiteFolderSettings = new Dictionary<Guid, IdShieldFolderSettingsCollection>();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Gets the id shield settings.
        /// </summary>
        /// <param name="createNew">if set to <see langword="true"/> and there are no existing
        /// settings then new settings will be created, otherwise a null value will be returned.
        /// </param>
        /// <returns>The persisted ID Shield settings.</returns>
        public static IdShieldProcessingFeatureSettings GetIdShieldSettings(bool createNew)
        {
            IdShieldProcessingFeatureSettings settings =
                SPFarm.Local.GetChild<IdShieldProcessingFeatureSettings>(_ID_SHIELD_SETTINGS_NAME);
            if (settings == null && createNew)
            {
                settings = new IdShieldProcessingFeatureSettings(SPFarm.Local);
            }

            return settings;
        }

        /// <summary>
        /// Removes the folder watching for the specified folder.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="folderId">The folder id.</param>
        public static bool RemoveFolderWatching(Guid siteId, Guid folderId)
        {
            bool watchRemoved = false;
            var settings = GetIdShieldSettings(false);
            if (settings != null)
            {
                IdShieldFolderSettingsCollection collection = null;
                if (settings.AllSiteFolderSettings.TryGetValue(siteId, out collection))
                {
                    watchRemoved = collection.Remove(folderId);
                    if (watchRemoved)
                    {
                        settings.Update();
                    }
                }
            }

            return watchRemoved;
        }

        /// <summary>
        /// Adds the specified site id to the collection of active site ids.
        /// </summary>
        /// <param name="siteId">The site id to add to the collection.</param>
        public static void AddActiveFeatureSiteId(Guid siteId)
        {
            IdShieldProcessingFeatureSettings settings = GetIdShieldSettings(true);
            settings.InternalAddActiveFeatureSiteId(siteId);
            settings.Update();
        }

        /// <summary>
        /// Removes the specified site id from the collection of active site ids.
        /// </summary>
        /// <param name="siteId">The site id to remove from the collection.</param>
        public static void RemoveActiveFeatureSiteId(Guid siteId)
        {
            IdShieldProcessingFeatureSettings settings = GetIdShieldSettings(false);
            if (settings != null)
            {
                settings.InternalRemoveActiveFeatureSiteId(siteId);
                settings.Update();
            }
        }

        /// <summary>
        /// Adds the unique file id to the list of files that were seen in the
        /// file added event but were 0 bytes.
        /// </summary>
        /// <param name="fileId">The unique file id for the file to add to the list.</param>
        internal static void AddZeroByteFileId(Guid fileId)
        {
            var settings = GetIdShieldSettings(true);
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
            var settings = GetIdShieldSettings(false);
            if (settings != null)
            {
                settings.InternalRemoveZeroByteFileId(fileId);
                settings.Update();
            }
        }

        /// <summary>
        /// Gets the site settings.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <returns></returns>
        internal IdShieldFolderSettingsCollection GetSiteSettings(Guid siteId)
        {
            return GetSiteSettings(siteId, false);
        }

        /// <summary>
        /// Gets the site settings.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="create">If <see langword="true"/> then a new <see cref="IdShieldFolderSettingsCollection"/>
        /// will be created if one does not exist.</param>
        /// <returns></returns>
        internal IdShieldFolderSettingsCollection GetSiteSettings(Guid siteId, bool create)
        {
            IdShieldFolderSettingsCollection collection = null;
            if (!_allSiteFolderSettings.TryGetValue(siteId, out collection) && create)
            {
                collection = new IdShieldFolderSettingsCollection(Parent);
                _allSiteFolderSettings[siteId] = collection;
            }

            return collection;
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
                _activeSiteIds.Add(siteId);
            }
            else
            {
                // Search for the item id
                int index = _activeSiteIds.BinarySearch(siteId);
                if (index < 0)
                {
                    // The item was not found, insert it in the proper sorted location
                    _activeSiteIds.Insert(~index, siteId);
                }
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

        /// <summary>
        /// Adds the specified unique file id to the collection of zero byte file ids.
        /// </summary>
        /// <param name="fileId">The file id to add.</param>
        void InternalAddZeroByteFileId(Guid fileId)
        {
            if (_zeroByteFiles == null)
            {
                _zeroByteFiles = new List<Guid>();
                _zeroByteFiles.Add(fileId);
            }
            else
            {
                int index = _zeroByteFiles.BinarySearch(fileId);
                if (index < 0)
                {
                    _zeroByteFiles.Insert(~index, fileId);
                }
            }
        }

        /// <summary>
        /// Removes the specified unique file id from the collection of zero byte file ids.
        /// </summary>
        /// <param name="fileId">The file id to remove.</param>
        void InternalRemoveZeroByteFileId(Guid fileId)
        {
            if (_zeroByteFiles != null)
            {
                int index = _zeroByteFiles.BinarySearch(fileId);
                if (index >= 0)
                {
                    _zeroByteFiles.RemoveAt(index);
                }
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the folder settings for all sites..
        /// </summary>
        /// <value>The folder settings for all sites.</value>
        internal Dictionary<Guid, IdShieldFolderSettingsCollection> AllSiteFolderSettings
        {
            get
            {
                return _allSiteFolderSettings;
            }
        }

        /// <summary>
        /// Gets the active sites.
        /// </summary>
        /// <value>The active sites.</value>
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
        /// Gets the zero byte files.
        /// </summary>
        /// <value>The zero byte files.</value>
        public ReadOnlyCollection<Guid> ZeroByteFiles
        {
            get
            {
                if (_zeroByteFiles == null)
                {
                    _zeroByteFiles = new List<Guid>();
                }
                return _zeroByteFiles.AsReadOnly();
            }
        }

        #endregion Properties
    }
}
