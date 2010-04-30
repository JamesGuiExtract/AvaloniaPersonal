using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization.Configuration;

namespace Extract.Utilities
{
    #region Public Methods

    /// <summary>
    /// Wrapper for <see cref="ApplicationSettingsBase"/> objects that contain assembly settings
    /// loaded from a .NET application config file that allows the settings to be loaded/saved to a
    /// specified location and, optionally, dynamically updated as property values are changed.
    /// </summary>
    public class ConfigSettings<T> where T : ApplicationSettingsBase, new()
    {
        #region Constants

        /// <summary>
        /// The sectionGroup name for read-only properties.
        /// </summary>
        static readonly string _APPLICATION_SETTINGS_GROUP = "applicationSettings";

        /// <summary>
        /// The sectionGroup name for read/write properties.
        /// </summary>
        static readonly string _USER_SETTINGS_GROUP = "userSettings";

        /// <summary>
        /// The sectionGroup name object properties to be applied via reflection.
        /// </summary>
        static readonly string _OBJECT_SETTINGS_GROUP = "objectSettings";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The ApplicationSettingsBase instance containing the config file properties
        /// </summary>
        T _settings;

        /// <summary>
        /// Used to map the _config instance to the correct config file
        /// </summary>
        readonly ExeConfigurationFileMap _configFileMap = new ExeConfigurationFileMap();

        /// <summary>
        /// The Configuration instance used to load/save the settings to/from disk.
        /// </summary>
        Configuration _config;

        /// <summary>
        /// The filename (wihthout path) of the open configuration.
        /// </summary>
        string _fileName;

        /// <summary>
        /// If <see langword="true"/>, properties will be saved to disk as soon as they are
        /// modified and re-freshed from disk as necessary every time the Settings property is
        /// accessed. If <see langword="false"/> the propeties will only be saved to 
        /// disk on-demand and will never be refreshed from disk.
        /// </summary>
        bool _dynamic;

        /// <summary>
        /// The last known modification time of the open configuration file. Used to determine if
        /// settings need to be re-loaded from disk.
        /// </summary>
        DateTime _configFileLastModified;

        /// <summary>
        /// Keeps track of properties modified since the last save.
        /// </summary>
        readonly List<string> _modifiedProperties = new List<string>();

        /// <summary>
        /// Indicates if the settings are currently being refreshed from disk.
        /// </summary>
        bool _refreshing;

        /// <summary>
        /// Mutex object to exclusive access while creating file-specific mutex's
        /// </summary>
        static readonly object _lock = new object();

        /// <summary>
        /// File-specific mutexs to lock access while creating/saving config files
        /// </summary>
        static readonly Dictionary<string, object> _fileLocks = new Dictionary<string, object>();

        #endregion Fields

        /// <overloads>Initializes a new ConfigSettings instance.</overloads>
        /// <summary>
        /// Initializes a new ConfigSettings instance.
        /// </summary>
        /// <param name="configFileName">The config file to use as a source for the settings.
        /// </param>
        public ConfigSettings(string configFileName)
            : this(configFileName, true, true)
        {
        }

        /// <summary>
        /// Initializes a new ConfigSettings instance.
        /// </summary>
        /// <param name="configFileName">The config file to use as a source for the settings.
        /// </param>
        /// <param name="dynamic">If <see langword="true"/>, properties will be saved to disk as 
        /// soon as they are modified and re-freshed from disk as necessary every time the Settings 
        /// property is accessed. If <see langword="false"/> the propeties will only be saved to 
        /// disk on-demand and will never be refreshed from disk.</param>
        /// <param name="createIfMissing"><see langword="true"/> if a new config file instance
        /// should be created if there is no config file at the specified location,
        /// <see langword="false"/> if an error should occur if the file is missing.</param>
        public ConfigSettings(string configFileName, bool dynamic, bool createIfMissing)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30041", this.GetType().ToString());

                // Create a new instance (will have the default settings)
                _settings = new T();

                _fileName = Path.GetFileName(configFileName);

                // Lock around construction in case either this or another instance is creating
                // an new configuration file.
                lock (FileLock)
                {
                    _dynamic = dynamic;

                    if (!File.Exists(configFileName))
                    {
                        if (createIfMissing)
                        {
                            CreateConfigFile(configFileName);
                        }
                        else
                        {
                            ExtractException ee =
                                new ExtractException("ELI29688", "Missing configuration file!");
                            ee.AddDebugData("Filename", configFileName, false);
                            throw ee;
                        }
                    }

                    // Open the config file
                    _configFileMap.ExeConfigFilename = configFileName;
                    _config = ConfigurationManager.OpenMappedExeConfiguration(
                        _configFileMap, ConfigurationUserLevel.None);

                    // Load the settings (_OBJECT_SETTINGS_GROUP settings will be read at the time
                    // ApplyObjectSettings is called).
                    LoadSectionInformation(_APPLICATION_SETTINGS_GROUP);
                    LoadSectionInformation(_USER_SETTINGS_GROUP);

                    // Keep track of the config file last modified time so we know when the _config
                    // instance needs to be refreshed from disk.
                     _configFileLastModified = File.GetLastWriteTime(_config.FilePath);

                    _settings.PropertyChanged += HandlePropertyChanged;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI25407",
                    "Error loading configuration file!", ex);
                ee.AddDebugData("Configuration filename", configFileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the <see cref="ApplicationSettingsBase"/> instance containing the properties from
        /// the config file.
        /// <para><b>Note</b></para>
        /// In dynamic mode, it is important to access settings through this property with every
        /// call to ensure properties are in sync with the file on disk.
        /// </summary>
        /// <returns>The <see cref="ApplicationSettingsBase"/> instance.</returns>
        public T Settings
        {
            get
            {
                if (_dynamic)
                {
                    // If the config file has been modified since it was last reloaded/refreshed,
                    // grab the updated values from disk.
                    Refresh(true);
                }

                return _settings;
            }
        }

        /// <summary>
        /// Applies settings from the config file to the specified object using reflection.
        /// </summary>
        /// <param name="instance">The object to apply the settings to.</param>
        public void ApplyObjectSettings(object instance)
        {
            try
            {
                Type objectType = instance.GetType();

                // Locate the objectSettings group
                ConfigurationSectionGroup configSectionGroup =
                    _config.SectionGroups[_OBJECT_SETTINGS_GROUP];

                if (configSectionGroup != null)
                {
                    // Retrieve the settings associated with the type of the passed in object.
                    string settingsType = objectType.ToString();
                    DefaultSection configSection =
                        (DefaultSection)configSectionGroup.Sections[settingsType];

                    // Assuming we found an appropriate section, parse the XML and use reflection
                    // to apply the settings to the passed in object.
                    if (configSection != null)
                    {
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.InnerXml = configSection.SectionInformation.GetRawXml();
                        XmlNode rootNode = xmlDocument.FirstChild;

                        // Loop through each setting and apply the value from the config file using
                        // reflection.
                        ProcessObjectSettings(rootNode.ChildNodes, instance);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28803",
                    "Failed to apply config file settings!", ex);
                throw ee;
            }
        }

        /// <summary>
        /// Saves modified <see cref="ApplicationSettingsBase"/> user properties to disk.
        /// </summary>
        public void Save()
        {
            try
            {
                // If there are no modified properties, there is nothing to do.
                if (_modifiedProperties.Count == 0)
                {
                    return;
                }

                // If the file has been modified since it was loaded, an error will result when
                // trying to save. Therefore, lock around saving so that it is not possible for the
                // file to be updated between the time Refresh is called and the time the file is
                // written.
                lock (FileLock)
                {
                    // Update the _config instance so that it won't throw an exception when saving
                    // if the config file was updated since it was last loaded/refreshed.
                    Refresh(false);

                    // Locate the userSettings group (which contains the writable settings)
                    ConfigurationSectionGroup configSectionGroup =
                        _config.SectionGroups[_USER_SETTINGS_GROUP];

                    ExtractException.Assert("ELI29690", "userSettings section missing!",
                        configSectionGroup != null);

                    // Retrieve the settings associated with the Setting's class's type.
                    string settingsType = typeof(T).ToString();
                    ClientSettingsSection configSection =
                        (ClientSettingsSection)configSectionGroup.Sections[settingsType];

                    // Loop through each modified setting and apply them.
                    foreach (string modifiedProperty in _modifiedProperties)
                    {
                        // Create a new SettingElement if it was not already present in the config
                        // file.
                        SettingElement setting = configSection.Settings.Get(modifiedProperty);
                        if (setting == null)
                        {
                            XmlDocument xmlDocument = new XmlDocument();
                            xmlDocument.InnerXml = configSection.SectionInformation.GetRawXml();

                            setting = new SettingElement(modifiedProperty, SettingsSerializeAs.String);
                            setting.Value.ValueXml = xmlDocument.CreateElement("value");
                            configSection.Settings.Add(setting);
                        }

                        ApplySettingValue(configSection.Settings.Get(modifiedProperty));
                    }

                    // If ForceSave is not set, the data will not be saved.
                    configSection.SectionInformation.ForceSave = true;

                    _config.Save(ConfigurationSaveMode.Full);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI29682",
                    "Failed to save config file!", ex);
                ee.AddDebugData("Filename", _config.FilePath, false);
                throw ee;
            }
        }

        #endregion Public Methods

        /// <summary>
        /// Handles the case that a _settings property was changed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                // Refreshing may trigger PropertyChanged events, don't save in this case.
                if (!_refreshing)
                {
                    // If a save is already in progress, wait until that operation is complete
                    // before applying a new change.
                    lock (FileLock)
                    {
                        _modifiedProperties.Add(e.PropertyName);
                    }

                    // If in dynamic mode, save right away.
                    if (_dynamic)
                    {
                        Save();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI29691",
                    "Error saving settings change!", ex);
                ee.AddDebugData("Setting Name", e.PropertyName, false);
                ee.Display();
            }
        }

        #region Private Members

        /// <summary>
        /// Returns a mutex object for all instances of sharing the same _fileName
        /// </summary>
        /// <returns>A mutex objec to use when updating the config file.</returns>
        object FileLock
        {
            get
            {
                lock (_lock)
                {
                    object fileLock;
                    if (!_fileLocks.TryGetValue(_fileName, out fileLock))
                    {
                        fileLock = new object();
                        _fileLocks[_fileName] = fileLock;
                    }

                    return fileLock;
                }
            }
        }

        /// <summary>
        /// Creates a new config file with a userSettings section so that any modified
        /// user settings can be applied.
        /// </summary>
        /// <param name="configFileName">The name of the file to create.</param>
        void CreateConfigFile(string configFileName)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();

                // XML declaration
                XmlDeclaration declaration =
                    xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDocument.AppendChild(declaration);

                // configuration node
                XmlElement configElement = xmlDocument.CreateElement("configuration");
                xmlDocument.AppendChild(configElement);

                // configSections node
                XmlElement configSectionsElement =
                    xmlDocument.CreateElement("configSections");
                configElement.AppendChild(configSectionsElement);

                // userSettings definition node
                XmlElement userSettingsDefinitionElement =
                    CreateQualifiedTypeElement("sectionGroup", _USER_SETTINGS_GROUP,
                    typeof(System.Configuration.UserSettingsGroup), xmlDocument);
                configSectionsElement.AppendChild(userSettingsDefinitionElement);

                // userSettings section definition node
                XmlElement settingsSectionElement =
                    CreateQualifiedTypeElement("section", _settings.GetType().ToString(),
                    typeof(System.Configuration.ClientSettingsSection), xmlDocument);
                userSettingsDefinitionElement.AppendChild(settingsSectionElement);

                XmlAttribute allowExeDefinitionAttribute =
                    xmlDocument.CreateAttribute("allowExeDefinition");
                allowExeDefinitionAttribute.Value = "MachineToLocalUser";
                settingsSectionElement.Attributes.Append(allowExeDefinitionAttribute);

                XmlAttribute requirePermissionAttribute =
                    xmlDocument.CreateAttribute("requirePermission");
                requirePermissionAttribute.Value = "false";
                settingsSectionElement.Attributes.Append(requirePermissionAttribute);

                // userSettings node
                XmlElement userSettingsElement =
                    xmlDocument.CreateElement(_USER_SETTINGS_GROUP);
                configElement.AppendChild(userSettingsElement);

                // type specific node
                XmlElement typeSpecificElement =
                    xmlDocument.CreateElement(_settings.GetType().ToString());
                userSettingsElement.AppendChild(typeSpecificElement);

                // [FlexIDSCore:4131]
                // Create the directory first, if necessary.
                string directoryName = Path.GetDirectoryName(configFileName);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                xmlDocument.Save(configFileName);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29692", "Failed to create config file!", ex);
            }
        }

        /// <summary>
        /// Creates a new node with the specified name and type attributes.
        /// </summary>
        /// <param name="node">The name of the node to create.</param>
        /// <param name="name">The value to assign to the name attribute.</param>
        /// <param name="type">The type to assign to the type attribute.</param>
        /// <param name="xmlDocument">The <see cref="XmlDocument"/> in which the node is being
        /// created.</param>
        /// <returns></returns>
        static XmlElement CreateQualifiedTypeElement(string node, string name, Type type,
            XmlDocument xmlDocument)
        {
            XmlAttribute nameAttribute = xmlDocument.CreateAttribute("name");
            nameAttribute.Value = name;

            XmlAttribute typeAttribute = xmlDocument.CreateAttribute("type");
            typeAttribute.Value = type.AssemblyQualifiedName;

            XmlElement userSettingsElement = xmlDocument.CreateElement(node);
            userSettingsElement.Attributes.Append(nameAttribute);
            userSettingsElement.Attributes.Append(typeAttribute);

            return userSettingsElement;
        }

        /// <summary>
        /// Loads settings from the specified _config section into _settings.
        /// </summary>
        /// <param name="sectionName">The name of the section to be loaded.</param>
        void LoadSectionInformation(string sectionName)
        {
            // Locate the applicationSettings.
            ConfigurationSectionGroup configSectionGroup = _config.SectionGroups[sectionName];

            if (configSectionGroup != null)
            {
                // Retrieve the settings associated with the Setting's class's type.
                string settingsType = typeof(T).ToString();
                ClientSettingsSection configSection =
                    (ClientSettingsSection)configSectionGroup.Sections[settingsType];

                // Loop through each setting and apply the value from the config file. 
                foreach (SettingElement setting in configSection.Settings)
                {
                    object settingObject = _settings[setting.Name];
                    if (settingObject == null)
                    {
                        ExtractException ee = new ExtractException("ELI28826",
                            "Invalid Application Setting");
                        ee.AddDebugData("Setting name", setting.Name, false);
                        throw ee;
                    }

                    // Check the type of the setting
                    Type settingType = settingObject.GetType();

                    // Convert the string XML value to the appropriate type.
                    _settings[setting.Name] =
                        TypeDescriptor.GetConverter(settingType).ConvertFromString(
                            setting.Value.ValueXml.InnerXml);
                }
            }
        }

        /// <summary>
        /// Applies to the specified setting in the _config instance the current value in _settings.
        /// </summary>
        /// <param name="setting">The setting to be applied.</param>
        void ApplySettingValue(SettingElement setting)
        {
            ExtractException.Assert("ELI29685", "Null argument exception!", setting != null);

            try
            {
                object settingObject = _settings[setting.Name];
                ExtractException.Assert("ELI29683", "Invalid Setting!", settingObject != null);

                // Check the type of the setting
                Type settingType = settingObject.GetType();

                setting.Value.ValueXml.InnerText =
                    TypeDescriptor.GetConverter(settingType).ConvertToString(
                        _settings[setting.Name]);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI29684",
                    "Failed to apply config file setting!", ex);
                ee.AddDebugData("Setting name", setting.Name, false);
                throw ee;
            }
        }

        /// <summary>
        /// Re-initializes the _config instance from disk if the config file has been modified since
        /// the last time the _config instance was loaded/refreshed.
        /// </summary>
        /// <param name="reloadValues">If <see langword="true"/> the property values will be loaded
        /// from disk in the process, if <see langword="false"/>, the current _settings values will
        /// not be changed.</param>
        void Refresh(bool reloadValues)
        {
            if (_refreshing)
            {
                return;
            }

            try
            {
                _refreshing = true;

                DateTime lastModified = File.GetLastWriteTime(_config.FilePath);

                if (lastModified > _configFileLastModified)
                {
                    _config = ConfigurationManager.OpenMappedExeConfiguration(
                        _configFileMap, ConfigurationUserLevel.None);

                    if (reloadValues)
                    {
                        LoadSectionInformation(_APPLICATION_SETTINGS_GROUP);
                        LoadSectionInformation(_USER_SETTINGS_GROUP);
                    }

                    _configFileLastModified = lastModified;
                }
            }
            finally
            {
                _refreshing = false;
            }
        }

        /// <summary>
        /// Applies the settings from the specified <see cref="XmlNodeList"/> to properties of the
        /// specified <see langword="object"/>.
        /// <para><b>Note:</b></para>
        /// Only <see langword="public"/> properties may be set, but they can be
        /// set on non-<see langword="public"/> fields of the object as well.
        /// </summary>
        /// <param name="nodes">The <see cref="XmlNodeList"/> containing the settings.</param>
        /// <param name="instance">The <see langword="object"/> to apply the settngs to.</param>
        static void ProcessObjectSettings(XmlNodeList nodes, object instance)
        {
            Type objectType = instance.GetType();

            foreach (XmlNode node in nodes)
            {
                // Process property settings for specified member classes.
                if (node.Name.Equals("Member", StringComparison.OrdinalIgnoreCase))
                {
                    string memberName = GetXmlNameAttribute(node);
                    FieldInfo field = objectType.GetField(memberName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (field == null || !field.GetType().IsClass)
                    {
                        ExtractException ee = new ExtractException("ELI28806", "Unknown member!");
                        ee.AddDebugData("Member name", memberName, false);
                        throw ee;
                    }

                    ProcessObjectSettings(node.ChildNodes, field.GetValue(instance));
                }
                // Apply the specified setting to its corresponding object property.
                else if (node.Name.Equals("Property", StringComparison.OrdinalIgnoreCase))
                {
                    string propertyName = GetXmlNameAttribute(node);
                    PropertyInfo objectProperty = objectType.GetProperty(propertyName);

                    if (objectProperty == null)
                    {
                        ExtractException ee = new ExtractException("ELI28822",
                            "Unknown property!");
                        ee.AddDebugData("Property name", propertyName, false);
                        throw ee;
                    }

                    objectProperty.SetValue(instance,
                        TypeDescriptor.GetConverter(objectProperty.PropertyType).ConvertFromString(
                            node.InnerText), null);
                }
            }
        }

        /// <summary>
        /// Asserts the existance of and retrieves the value of the name <see cref="XmlAttribute"/>
        /// from the specified <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        static string GetXmlNameAttribute(XmlNode node)
        {
            XmlAttribute attribute = node.Attributes["name"];
            ExtractException.Assert("ELI28805", "XML node name attribute missing!", attribute != null);
            return attribute.Value;
        }

        #endregion Private Members
    }
}
