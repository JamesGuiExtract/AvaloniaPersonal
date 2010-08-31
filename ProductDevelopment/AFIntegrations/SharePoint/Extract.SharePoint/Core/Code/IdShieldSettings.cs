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
        /// The feature ID string
        /// </summary>
        internal static readonly string _IDSHIELD_FEATURE_ID = "2d595bc7-785f-4ae5-a1db-65708f23c0d0";

        /// <summary>
        /// The feature ID GUID
        /// </summary>
        internal static readonly Guid _IDSHIELD_FEATURE_GUID = new Guid(_IDSHIELD_FEATURE_ID);

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
