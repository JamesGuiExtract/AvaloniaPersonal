using Microsoft.SharePoint.Administration;
using System;

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
