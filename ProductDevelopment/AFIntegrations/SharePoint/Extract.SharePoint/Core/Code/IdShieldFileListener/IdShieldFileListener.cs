using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;

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
        Dictionary<string, FolderProcessingSettings> _folderSettings =
            new Dictionary<string, FolderProcessingSettings>();

        /// <summary>
        /// Holds the folder serialization string so that it can be compared
        /// and deserialized if the settings are updated.
        /// </summary>
        string _folderSettingsSerializationString = string.Empty;

        /// <summary>
        /// The output folder that files should be written to for processing
        /// </summary>
        string _outputFolder = string.Empty;

        #endregion Fields

        #region Event Handlers

        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);
            HandleFileEvent(properties, FileEventType.FileAdded);
        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
            HandleFileEvent(properties, FileEventType.FileModified);
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Handles the file event, checking the current settings and exports any files
        /// that match the current settings to the specified processing folder.
        /// </summary>
        /// <param name="properties">The properties for the file event.</param>
        /// <param name="eventType">The type of event that is being handled.</param>
        void HandleFileEvent(SPItemEventProperties properties, FileEventType eventType)
        {
            // Get the item and check that it is a file item
            SPListItem item = properties.ListItem;
            if (item.FileSystemObjectType != SPFileSystemObjectType.File)
            {
                return;
            }

            // Update the settings
            UpdateSettings(GetIdShieldFeature(properties.Web));

            // Check for an output folder (if none is configured then do nothing)
            if (!string.IsNullOrEmpty(_outputFolder))
            {
                // Get the folder name for the item
                string folder = item.File.Url;
                folder = folder.Replace("/" + item.File.Name, "");

                // Attempt to get the settings for the folder
                FolderProcessingSettings settings = null;
                if (_folderSettings.TryGetValue("/" + folder, out settings))
                {
                    // Check if the event and file match the settings
                    if ((settings.EventTypes & eventType) != 0
                        && settings.DoesFileMatchPattern(item.File.Name))
                    {
                        // Ensure the processing folder exists
                        folder = folder.Replace('/', '\\');
                        string outputFolder = Path.Combine(_outputFolder, folder);
                        if (!Directory.Exists(outputFolder))
                        {
                            Directory.CreateDirectory(outputFolder);
                        }

                        // Write the file to the processing folder
                        string fileName = Path.Combine(outputFolder, item.File.Name);
                        byte[] bytes = item.File.OpenBinary(SPOpenBinaryOptions.SkipVirusScan);
                        File.WriteAllBytes(fileName, bytes);
                    }
                }
            }
        }

        /// <summary>
        /// Checks the feature and ensures the settings are updated.
        /// </summary>
        /// <param name="feature">The feature to get the settings from.</param>
        void UpdateSettings(SPFeature feature)
        {
            // Get the folder settings from the feature
            SPFeatureProperty property =
                feature.Properties[ExtractSharePointHelper._FOLDERS_TO_PROCESS];
            if (property != null)
            {
                string temp = property.Value;
                if (temp.Length != _folderSettingsSerializationString.Length
                    || !temp.Equals(_folderSettingsSerializationString, StringComparison.Ordinal))
                {
                    _folderSettings = FolderProcessingSettings.DeserializeFolderSettings(temp);
                    _folderSettingsSerializationString = temp;
                }
            }

            // Get the processing folder setting
            property = feature.Properties[ExtractSharePointHelper._ID_SHIELD_LOCAL_FOLDER];
            if (property != null)
            {
                _outputFolder = property.Value;
            }
        }

        /// <summary>
        /// Gets the ID Shield feature from the specified SharePoint web.
        /// </summary>
        /// <param name="web">The web to search for the feature.</param>
        /// <returns>The ID Shield feature (or <see langword="null"/> if it is
        /// not installed.</returns>
        SPFeature GetIdShieldFeature(SPWeb web)
        {
            return web.Features[ExtractSharePointHelper._IDSHIELD_FEATURE_GUID];
        }

        #endregion Methods
    }
}
