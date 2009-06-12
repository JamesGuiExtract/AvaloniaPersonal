using Extract;
using Extract.DataEntry.Utilities.DataEntryApplication.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// Exposes settings from the DataEntryApplication.exe.config file whether or not the current
    /// process is DataEntryApplication.
    /// </summary>
    internal static class ConfigSettings
    {
        /// <summary>
        /// The cached settings from the "applicationSettings" section of the config file.
        /// </summary>
        private static Settings _appSettings;

        /// <summary>
        /// Initializes config settings using the specified config file.
        /// </summary>
        /// <param name="configFileName">The config file to use as a source for the settings.
        /// </param>
        public static void Initialize(string configFileName)
        {
            try
            {
                // Create a new instance (will have the default settings)
                _appSettings = new Settings();

                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configFileName;

                // Open the config file
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(
                    configFileMap, ConfigurationUserLevel.None);

                // Locate the applicationSettings.
                ConfigurationSectionGroup configSectionGroup =
                    config.SectionGroups["applicationSettings"];

                // Retrieve the settings associated with the Setting's class's type.
                string settingsType = _appSettings.GetType().ToString();
                ClientSettingsSection configSection =
                    (ClientSettingsSection)configSectionGroup.Sections[settingsType];

                // Loop through each setting and apply the value from the config file. 
                foreach (SettingElement setting in configSection.Settings)
                {
                    // Check the type of the setting
                    Type settingType = _appSettings[setting.Name].GetType();

                    // Convert the string XML value to the appropriate type.
                    _appSettings[setting.Name] =
                        TypeDescriptor.GetConverter(settingType).ConvertFromString(
                            setting.Value.ValueXml.InnerXml);
                }

                // Initialize the root directory the DataEntry framework should use when resolving
                // relative paths.
                DataEntryMethods.SolutionRootDirectory = Path.GetDirectoryName(configFileName);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI25407",
                    "Invalid or missing configuration file!", ex);
                ee.AddDebugData("Configuration filename", configFileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Retrives the settings from the "applicationSettings" section of the config file.
        /// <para><b>Requires:</b></para>
        /// <see cref="Initialize"/> must be called prior to accessing this property.
        /// </summary>
        /// <returns>The settings from the "applicationSettings" section of the config file.
        /// </returns>
        public static Settings AppSettings
        {
            get
            {
                ExtractException.Assert("ELI25428",
                    "Settings were accessed before being initialized!", _appSettings != null);

                return _appSettings;
            }
        }
    }
}
