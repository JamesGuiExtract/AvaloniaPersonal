using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        /// <summary>
        /// The default minutes to wait to queue files that are in the to be queued later status
        /// </summary>
        const int _DEFAULT_MINUTES_TO_WAIT_FOR_QUEUE = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The local folder that is used for processing
        /// </summary>
        [Persisted]
        string _localWorkingFolder;

        /// <summary>
        /// The ip address of the server running the Extract exception service
        /// </summary>
        [Persisted]
        string _exceptionServiceIPAddress;

        /// <summary>
        /// Collection of site ids that currently have the ID Shield feature activated.
        /// </summary>
        [Persisted]
        List<Guid> _activeSiteIds;

        /// <summary>
        /// List of Id's of sites that ID shield has been activated for.
        /// </summary>
        [Persisted]
        List<Guid> _idShieldSites;

        /// <summary>
        /// The length for the random folder name to be generated.
        /// </summary>
        [Persisted]
        int _randomFolderNameLength;

        /// <summary>
        /// The amount of time to wait before a to be queued later file is queued.
        /// </summary>
        [Persisted]
        int _minutesToWaitToQueueLater;

        /// <summary>
        /// The path to the fps file that the redact now button will launch.
        /// </summary>
        [Persisted]
        string _redactNowFpsFile;

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
                settings.Unprovision();
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
            if (_idShieldSites == null)
            {
                _idShieldSites = new List<Guid>();
            }

            // Search for the item id
            int index = _activeSiteIds.BinarySearch(siteId);
            if (index < 0)
            {
                // The item was not found, insert it in the proper sorted location
                _activeSiteIds.Insert(~index, siteId);
            }
            index = _idShieldSites.BinarySearch(siteId);
            if (index < 0)
            {
                _idShieldSites.Insert(~index, siteId);
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
        /// Gets or sets the length of the random folder name.
        /// </summary>
        /// <value>The length of the random folder name.</value>
        public int RandomFolderNameLength
        {
            get
            {
                return _randomFolderNameLength;
            }
            set
            {
                _randomFolderNameLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the minutes to wait to queued later.
        /// </summary>
        /// <value>The minutes to wait to queued later.</value>
        public int MinutesToWaitToQueuedLater
        {
            get
            {
                return _minutesToWaitToQueueLater > 0 ?
                    _minutesToWaitToQueueLater : _DEFAULT_MINUTES_TO_WAIT_FOR_QUEUE;
            }
            set
            {
                _minutesToWaitToQueueLater = value;
            }
        }

        /// <summary>
        /// Gets or sets the redact now FPS file.
        /// </summary>
        /// <value>The redact now FPS file.</value>
        public string RedactNowFpsFile
        {
            get
            {
                return _redactNowFpsFile;
            }
            set
            {
                _redactNowFpsFile = value;
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
        /// Gets the sorted list of ids for sites that have been activated for the ID Shield feature.
        /// </summary>
        public ReadOnlyCollection<Guid> IdShieldSites
        {
            get
            {
                if (_idShieldSites == null)
                {
                    _idShieldSites = new List<Guid>();
                }
                return _idShieldSites.AsReadOnly();
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
