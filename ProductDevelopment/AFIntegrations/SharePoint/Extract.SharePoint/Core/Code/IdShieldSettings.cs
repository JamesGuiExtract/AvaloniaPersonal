using Microsoft.SharePoint;
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

        /// <summary>
        /// The local working folder setting string for the feature properties
        /// </summary>
        internal static readonly string _LOCAL_WORKING_FOLDER_SETTING_STRING = "IdShieldWorkingFolder";

        /// <summary>
        /// The folder processing setting string for the feature properties
        /// </summary>
        internal static readonly string _FOLDER_PROCESSING_SETTINGS_STRING = "FolderSettings";

        /// <summary>
        /// The exception service server ip address setting string for the feature properties
        /// </summary>
        internal static string _IP_ADDRESS_SETTING_STRING = "ExceptionServerIpAddress";

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
        /// Loads the ID Shield settings from persisted storage into the specified feature.
        /// </summary>
        /// <param name="feature">The feature to add the loaded settings to.</param>
        internal static void LoadIdShieldSettings(SPFeature feature)
        {
            IdShieldSettings settings =
                SPFarm.Local.GetChild<IdShieldSettings>(_ID_SHIELD_SETTINGS_NAME);
            if (settings != null)
            {
                SPFeaturePropertyCollection properties = feature.Properties;

                // Get the local folder setting, if it does not exist create it
                SPFeatureProperty property = properties[_LOCAL_WORKING_FOLDER_SETTING_STRING];
                if (property != null)
                {
                    property.Value = settings.LocalWorkingFolder;
                }
                else
                {
                    property = new SPFeatureProperty(_LOCAL_WORKING_FOLDER_SETTING_STRING,
                        settings.LocalWorkingFolder);
                    properties.Add(property);
                }
                property = null;

                // Get the folder processing setting, if it does not exist create it
                property = properties[_FOLDER_PROCESSING_SETTINGS_STRING];
                if (property != null)
                {
                    property.Value = settings.FolderSettings;
                }
                else
                {
                    property = new SPFeatureProperty(_FOLDER_PROCESSING_SETTINGS_STRING,
                        settings.FolderSettings);
                    properties.Add(property);
                }
                property = null;

                property = properties[_IP_ADDRESS_SETTING_STRING];
                if (property != null)
                {
                    property.Value = settings.ExceptionServiceIPAddress;
                }
                else
                {
                    property = new SPFeatureProperty(_IP_ADDRESS_SETTING_STRING,
                        settings.ExceptionServiceIPAddress);
                    properties.Add(property);
                }

                // Call update to push the settings into the feature
                properties.Update();
            }
        }

        /// <summary>
        /// Stores the current ID Shield settings for the specified feature into the
        /// persisted store.
        /// </summary>
        /// <param name="feature">The feature to store the settings for.</param>
        internal static void StoreIdShieldSettings(SPFeature feature)
        {
            SPFeaturePropertyCollection properties = feature.Properties;
            IdShieldSettings settings =
                SPFarm.Local.GetChild<IdShieldSettings>(_ID_SHIELD_SETTINGS_NAME)
                ?? new IdShieldSettings(SPFarm.Local);
            SPFeatureProperty property = properties[_LOCAL_WORKING_FOLDER_SETTING_STRING];
            if (property != null)
            {
                settings.LocalWorkingFolder = property.Value;
            }
            property = properties[_FOLDER_PROCESSING_SETTINGS_STRING];
            if (property != null)
            {
                settings.FolderSettings = property.Value;
            }
            property = properties[_IP_ADDRESS_SETTING_STRING];
            if (property != null)
            {
                settings.ExceptionServiceIPAddress = property.Value;
            }

            // Call update to push the settings into the farm
            settings.Update();
        }

        /// <summary>
        /// Removes the ID Shield settings from the persisted store. 
        /// </summary>
        internal static void RemoveIdShieldSettings()
        {
            IdShieldSettings settings =
                SPFarm.Local.GetChild<IdShieldSettings>(_ID_SHIELD_SETTINGS_NAME);
            if (settings != null)
            {
                settings.Delete();
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
                return _folderSettings;
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
                return _exceptionServiceIPAddress;
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
