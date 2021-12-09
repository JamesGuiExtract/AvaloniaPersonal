using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using UCLID_COMUTILSLib;

namespace Extract.Utilities
{
    /// <summary>
    /// Wrapper for <see cref="ApplicationSettingsBase"/> that allows the settings to be
    /// loaded/saved to a specified config file and, optionally, dynamically updated as property
    /// values are changed.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class ConfigSettings<T> : ExtractSettingsBase<T> where T : ApplicationSettingsBase, new()
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
        /// Used to map the _config instance to the correct config file
        /// </summary>
        readonly ExeConfigurationFileMap _configFileMap = new ExeConfigurationFileMap();

        /// <summary>
        /// The Configuration instance used to load/save the settings to/from disk.
        /// </summary>
        Configuration _config;

        /// <summary>
        /// The <see cref="ClientSettingsSection"/> that persists application scoped properties.
        /// </summary>
        ClientSettingsSection _applicationConfigSection;

        /// <summary>
        /// The <see cref="ClientSettingsSection"/> that persists user scoped properties.
        /// </summary>
        ClientSettingsSection _userConfigSection;

        /// <summary>
        /// The filename of the open configuration.
        /// </summary>
        string _configFileName;

        /// <summary>
        /// Mutex object to exclusive access while creating file-specific mutex's
        /// </summary>
        static readonly object _lock = new object();

        /// <summary>
        /// File-specific mutexes to lock access while creating/saving config files
        /// </summary>
        static readonly Dictionary<string, object> _fileLocks =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new ConfigSettings instance.</overloads>
        /// <summary>
        /// Initializes a new ConfigSettings instance.
        /// </summary>
        public ConfigSettings()
            : this(null, true, true)
        {
        }

        /// <summary>
        /// Initializes a new ConfigSettings instance.
        /// <param name="useCommonAppData">Use CommonApplicationData folder for the
        /// config file.</param>
        /// </summary>
        public ConfigSettings(bool useCommonAppData)
            : this( (useCommonAppData
                        ? Path.Combine(FileSystemMethods.CommonApplicationDataPath,
                          Path.GetFileName(Assembly.GetAssembly(typeof(T)).Location) + ".config")
                        : null
                    ), true, true)
        {
        }

        /// <summary>
        /// Initializes a new ConfigSettings instance.
        /// </summary>
        /// <param name="configFileName">The config file to use as a source for the settings. If
        /// <see langword="null"/>, an appropriately named config file name will be automatically
        /// generated within the users ApplicationData folder.</param>
        public ConfigSettings(string configFileName)
            : this(configFileName, true, true, null)
        {
        }

        /// <summary>
        /// Initializes a new ConfigSettings instance.
        /// </summary>
        /// <param name="configFileName">The config file to use as a source for the settings.
        /// </param>
        /// <param name="dynamic">If <see langword="true"/>, properties will be saved to disk as 
        /// soon as they are modified and re-freshed from disk as necessary every time the Settings 
        /// property is accessed. If <see langword="false"/> the properties will only be loaded and
        /// saved to disk when explicitly requested.</param>
        /// <param name="createIfMissing"><see langword="true"/> if a new config file instance
        /// should be created if there is no config file at the specified location,
        /// <see langword="false"/> if an error should occur if the file is missing.</param>
        public ConfigSettings(string configFileName, bool dynamic, bool createIfMissing)
            : this(configFileName, null, dynamic, createIfMissing, null)
        {
        }

        /// <summary>
        /// Initializes a new ConfigSettings instance.
        /// </summary>
        /// <param name="configFileName">The config file to use as a source for the settings.
        /// </param>
        /// <param name="dynamic">If <see langword="true"/>, properties will be saved to disk as 
        /// soon as they are modified and re-freshed from disk as necessary every time the Settings 
        /// property is accessed. If <see langword="false"/> the properties will only be loaded and
        /// saved to disk when explicitly requested.</param>
        /// <param name="createIfMissing"><see langword="true"/> if a new config file instance
        /// should be created if there is no config file at the specified location,
        /// <see langword="false"/> if an error should occur if the file is missing.</param>
        /// <param name="tagUtility">The <see cref="ITagUtility"/> that should be used to expand
        /// setting values as they are loaded.</param>
        [CLSCompliant(false)]
        public ConfigSettings(string configFileName, bool dynamic, bool createIfMissing,
            ITagUtility tagUtility)
            : this(configFileName, null, dynamic, createIfMissing, tagUtility)
        {
        }

        /// <summary>
        /// Initializes a new ConfigSettings instance.
        /// </summary>
        /// <param name="configFileName">The config file to use as a source for the settings. If
        /// <see langword="null"/>, an appropriately named config file name will be automatically
        /// generated within the users ApplicationData folder.</param>
        /// <param name="defaultConfigFileName">If not <see langword="null"/>, settings not present
        /// in <paramref name="configFileName"/> may be provided defaults from this config file.
        /// </param>
        /// <param name="dynamic">If <see langword="true"/>, properties will be saved to disk as 
        /// soon as they are modified and re-freshed from disk as necessary every time the Settings 
        /// property is accessed. If <see langword="false"/> the properties will only be loaded and
        /// saved to disk when explicitly requested.</param>
        /// <param name="createIfMissing"><see langword="true"/> if a new config file instance
        /// should be created if there is no config file at the specified location,
        /// <see langword="false"/> if an error should occur if the file is missing.</param>
        /// <param name="tagUtility">The <see cref="ITagUtility"/> that should be used to expand
        /// setting values as they are loaded.</param>
        [CLSCompliant(false)]
        public ConfigSettings(string configFileName, string defaultConfigFileName, bool dynamic,
            bool createIfMissing, ITagUtility tagUtility)
            : base(dynamic, tagUtility)
        {
            try
            {
                // If a filename was not specified, create one based on the name of the
                // UserApplicationDataPath and the assembly that defines T.
                if (configFileName == null)
                {
                    string appDataPath = FileSystemMethods.UserApplicationDataPath;

                    // Use the common app data path if the user's path resolves to something in the system dir
                    // (C:\windows\system32\config\systemprofile\AppData\Local)
                    // This will happen when running as a service
                    // https://extract.atlassian.net/browse/ISSUE-17874
                    if (IsPathInSystemFolder(appDataPath))
                    {
                        appDataPath = FileSystemMethods.CommonApplicationDataPath;
                    }

                    _configFileName = Assembly.GetAssembly(typeof(T)).Location;
                    _configFileName = Path.GetFileName(_configFileName);
                    _configFileName = Path.Combine(
                        appDataPath, _configFileName + ".config");
                }
                else
                {
                    _configFileName = Path.GetFullPath(configFileName);
                }

                // Lock around construction in case either this or another instance is creating
                // an new configuration file.
                lock (FileLock)
                {
                    if (!File.Exists(_configFileName))
                    {
                        if (createIfMissing)
                        {
                            CreateConfigFile(_configFileName);
                        }
                        else
                        {
                            ExtractException ee =
                                new ExtractException("ELI29688", "Missing configuration file!");
                            ee.AddDebugData("Filename", _configFileName, false);
                            throw ee;
                        }
                    }

                    // If a separate config file has been specified to provide overridable defaults,
                    // initialize those defaults before loading the primary config file.
                    if (!string.IsNullOrEmpty(defaultConfigFileName))
                    {
                        // Open the config file
                        _configFileMap.ExeConfigFilename = defaultConfigFileName;
                        Load();
                    }

                    // Open the primary config file
                    _configFileMap.ExeConfigFilename = _configFileName;
                    Load();
                }

                Constructed = true;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI25407",
                    "Error loading configuration file!", ex);
                ee.AddDebugData("Configuration filename", configFileName ?? _configFileName, false);
                throw ee;
            }
        }

        #endregion Constructors

        #region Public Methods

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
        /// Gets the XML associated with the specified section.
        /// </summary>
        /// <param name="sectionName">The name of the section to get.</param>
        /// <returns>An <see cref="IXPathNavigable"/> initialized to the specified section or
        /// <see langword="null"/> if the section could not be found.</returns>
        public IXPathNavigable GetSectionXml(string sectionName)
        {
            try
            {
                // Locate the objectSettings group
                DefaultSection section = (DefaultSection)_config.Sections[sectionName];

                // Assuming we found an appropriate section, retrieve the XML 
                if (section != null)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.InnerXml = section.SectionInformation.GetRawXml();
                    return xmlDocument.FirstChild as IXPathNavigable;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30546",
                    "Failed to load configuration section.", ex);
                ee.AddDebugData("Section", sectionName, false);
                throw ee;
            }
        }

        #endregion Public Methods

        #region Overrides

        /// <summary>
        /// Specifies that some or all properties should be created in the config file even if the
        /// value has not changed from its default. This allows for easier manipulation of the
        /// settings by the user.
        /// </summary>
        /// <param name="propertyNames">If specified, the names of the properties to generate. If
        /// not specified, values for all members of the wrapped
        /// <see cref="ApplicationSettingsBase"/> type will be generated.</param>
        public override void GeneratePropertyValues(params string[] propertyNames)
        {
            try
            {
                // Update the _config instance so that it won't throw an exception when saving
                // if the config file was updated since it was last loaded/refreshed.
                ResetConfigFileConnection();

                base.GeneratePropertyValues(propertyNames);

                // If ForceSave is not set, the data will not be saved.
                if (_userConfigSection != null)
                {
                    _userConfigSection.SectionInformation.ForceSave = true;
                }
                if (_applicationConfigSection != null)
                {
                    _applicationConfigSection.SectionInformation.ForceSave = true;
                }

                _config.Save(ConfigurationSaveMode.Full);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32892");
            }
        }

        /// <summary>
        /// Loads the data from the config file into the Settings instance.
        /// </summary>
        public override void Load()
        {
            try
            {
                base.Load();

                lock (FileLock)
                {
                    ResetConfigFileConnection();

                    // Load the settings (_OBJECT_SETTINGS_GROUP settings will be read at the time
                    // ApplyObjectSettings is called).
                    LoadSectionInformation(_APPLICATION_SETTINGS_GROUP);
                    LoadSectionInformation(_USER_SETTINGS_GROUP);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32895");
            }
            finally
            {
                Loaded = true;
            }
        }

        /// <summary>
        /// Saves modified <see cref="ApplicationSettingsBase"/> user properties to disk.
        /// </summary>
        public override void Save()
        {
            try
            {
                // If the file has been modified since it was loaded, an error will result when
                // trying to save. Therefore, lock around saving so that it is not possible for the
                // file to be updated between the time Refresh is called and the time the file is
                // written.
                lock (FileLock)
                {
                    // Update the _config instance so that it won't throw an exception when saving
                    // if the config file was updated since it was last loaded/refreshed.
                    ResetConfigFileConnection();

                    base.Save();

                    // If ForceSave is not set, the data will not be saved.
                    if (_userConfigSection != null)
                    {
                        _userConfigSection.SectionInformation.ForceSave = true;
                    }
                    if (_applicationConfigSection != null)
                    {
                        _applicationConfigSection.SectionInformation.ForceSave = true;
                    }

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

        /// <summary>
        /// Commits the specified <see paramref="value"/> of the specified property to the config
        /// file.
        /// </summary>
        /// <param name="propertyName">The name of the property to be applied.</param>
        /// <param name="value">The value to apply.</param>
        protected override void SavePropertyValue(string propertyName, string value)
        {
            try
            {
                bool isUserProperty = IsUserProperty(propertyName);
                ClientSettingsSection configSection = GetConfigSection(isUserProperty);

                // Create a new SettingElement if it was not already present in the config
                // file.
                SettingElement setting = configSection.Settings.Get(propertyName);
                if (setting == null)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.InnerXml = configSection.SectionInformation.GetRawXml();

                    setting = new SettingElement(propertyName, SettingsSerializeAs.String);
                    setting.Value.ValueXml = xmlDocument.CreateElement("value");
                    configSection.Settings.Add(setting);
                }

                if (!string.IsNullOrWhiteSpace(value))
                {
                    setting.Value.ValueXml.InnerText = value;
                }
                else
                {
                    // So that empty elements aren't spread across 2 lines with separate open/close
                    // elements, create a new empty element rather than settings the inner text.
                    var doc = new XmlDocument();
                    setting.Value.ValueXml = doc.CreateElement("value");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32896");
            }
        }

        /// <summary>
        /// Gets the last modified time of the config file.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> the config file was last modified .</returns>
        protected override DateTime GetLastModifiedTime()
        {
            try
            {
                return File.GetLastWriteTime(_configFileName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32893");
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Returns a mutex object for all instances of sharing the same _fileName
        /// </summary>
        /// <returns>A mutex object to use when updating the config file.</returns>
        object FileLock
        {
            get
            {
                lock (_lock)
                {
                    object fileLock;
                    if (!_fileLocks.TryGetValue(_configFileName, out fileLock))
                    {
                        fileLock = new object();
                        _fileLocks[_configFileName] = fileLock;
                    }

                    return fileLock;
                }
            }
        }

        /// <summary>
        /// Resets the _config instance so that it is in sync with the data on disk. This must be
        /// called before loading/saving any modified data.
        /// </summary>
        void ResetConfigFileConnection()
        {
            try
            {
                _config = ConfigurationManager.OpenMappedExeConfiguration(
                        _configFileMap, ConfigurationUserLevel.None);

                _applicationConfigSection = null;
                _userConfigSection = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32894");
            }
        }

        /// <summary>
        /// Creates a new config file at the specified location. If HasUserProperties 
        /// <see langword="true"/>, a UserSettingsGroup will be generated and if
        /// HasApplicationProperties is <see langword="false"/>, an ApplicationSettingsGroup will
        /// be generated. However, properties will be added to those groups only if they are
        /// modified or if <see cref="GeneratePropertyValues"/> is called.
        /// </summary>
        /// <param name="configFileName">The name of the config file to create.</param>
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

                // [FlexIDSCore:4131]
                // Create the directory first, if necessary.
                string directoryName = Path.GetDirectoryName(configFileName);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                // Create section groups only if the Settings objects contains corresponding
                // properties in them.
                if (HasUserProperties)
                {
                    CreateConfigGroup(xmlDocument, configElement, configSectionsElement, true);
                }
                if (HasApplicationProperties)
                {
                    CreateConfigGroup(xmlDocument, configElement, configSectionsElement, false);
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29692", "Failed to create config file!", ex);
            }
        }

        /// <summary>
        /// Gets one of the <see cref="ClientSettingsSection"/>s from the config file (creating it
        /// if necessary).
        /// </summary>
        /// <param name="userSection"><see langword="true"/> to get the section from the
        /// UserSettingsGroup; <see langword="false"/> to get the section from the
        /// ApplicationSettingsGroup.</param>
        /// <param name="recursing"><see langword="true"/> if this method is being called
        /// recursively, to prevent the possibility of infinite recursion</param>
        /// <returns>The appropriate <see cref="ClientSettingsSection"/>.</returns>
        ClientSettingsSection GetConfigSection(bool userSection, bool recursing = false)
        {
            ConfigurationSectionGroup configSectionGroup = null;
            ClientSettingsSection configSection = null;

            if (userSection)
            {
                if (_userConfigSection != null)
                {
                    return _userConfigSection;
                }

                // Locate the UserSettingsGroup (which contains the writable settings)
                configSectionGroup =
                    _config.SectionGroups[_USER_SETTINGS_GROUP];
            }
            else
            {
                if (_applicationConfigSection != null)
                {
                    return _applicationConfigSection;
                }

                // Locate the ApplicationSettingsGroup (which contains the read-only settings)
                configSectionGroup = _config.SectionGroups[_APPLICATION_SETTINGS_GROUP];
            }

            // If the group does not exist, create it, then re-attempt the GetConfigSection call.
            // (CreateConfigGroup will create the config section as well.).
            if (configSectionGroup == null && !recursing)
            {
                CreateConfigGroup(userSection);
                return GetConfigSection(userSection, true);
            }

            ExtractException.Assert("ELI29690", "Settings group missing!",
                configSectionGroup != null);

            // Retrieve the settings associated with the Setting's class's type.
            string settingsType = typeof(T).ToString();
            configSection = (ClientSettingsSection)configSectionGroup.Sections[settingsType];

            // If the section does not exist, create it, then re-attempt the GetConfigSection call.
            if (configSection == null && !recursing)
            {
                CreateConfigSection(userSection);
                return GetConfigSection(userSection, true);
            }

            ExtractException.Assert("ELI32902", "Settings section missing!",
                configSectionGroup != null);

            // Cache the section for faster access next time.
            if (userSection)
            {
                _userConfigSection = configSection;
            }
            else
            {
                _applicationConfigSection = configSection;
            }

            return configSection;
        }

        /// <summary>
        /// Creates an ApplicationSettingsGroup or UserSettingsGroup (along with a config section
        /// for the assembly) depending upon the value of <see paramref="userSection"/>.
        /// </summary>
        /// <param name="userSection"><see langword="true"/> to create a UserSettingsGroup,
        /// <see langword="false"/> to create an ApplicationSettingsGroup,</param>
        void CreateConfigGroup(bool userSection)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(_configFileName);

            // Get configuration and configSections nodes
            XmlElement configElement = xmlDocument.DocumentElement;
            XmlElement configSectionsElement = configElement["configSections"];

            CreateConfigGroup(xmlDocument, configElement, configSectionsElement, userSection);
        }

        /// <summary>
        /// Creates a settings group (along with a config section for the assembly) depending upon
        /// the value of <see paramref="userSection"/>.
        /// </summary>
        /// <param name="xmlDocument">The <see cref="XmlDocument"/> in which the group is being
        /// created.</param>
        /// <param name="configElement">The <see cref="XmlElement"/> for the configuration node.
        /// </param>
        /// <param name="configSectionsElement">The <see cref="XmlElement"/> element for the config
        /// sections node.</param>
        /// <param name="userGroup"><see langword="true"/> to create a UserSettingsGroup,
        /// <see langword="false"/> to create an ApplicationSettingsGroup,</param>
        void CreateConfigGroup(XmlDocument xmlDocument, XmlElement configElement,
            XmlElement configSectionsElement, bool userGroup)
        {
            string sectionName = userGroup ? _USER_SETTINGS_GROUP : _APPLICATION_SETTINGS_GROUP;
            Type sectionType = userGroup
                ? typeof(System.Configuration.UserSettingsGroup)
                : typeof(System.Configuration.ApplicationSettingsGroup);

            // Create a definition for the group.
            XmlElement groupDefinitionElement =
                CreateQualifiedTypeElement("sectionGroup", sectionName, sectionType, xmlDocument);
            configSectionsElement.AppendChild(groupDefinitionElement);

            // Create the group itself.
            XmlElement groupElement = xmlDocument.CreateElement(sectionName);
            configElement.AppendChild(groupElement);

            // Create the corresponding config section.
            CreateConfigSection(xmlDocument, groupDefinitionElement, groupElement, userGroup);

            // Save the file, then refresh the config file connection.
            xmlDocument.Save(_configFileName);

            if (Constructed)
            {
                ResetConfigFileConnection();
            }
        }

        /// <summary>
        /// Creates a config section in the appropriate group for the current assembly.
        /// </summary>
        /// <param name="userSection"><see langword="true"/> to create the section in the
        /// UserSettingsGroup, <see langword="false"/> to create it in the
        /// ApplicationSettingsGroup,</param>
        void CreateConfigSection(bool userSection)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(_configFileName);

            string sectionName = userSection ? _USER_SETTINGS_GROUP : _APPLICATION_SETTINGS_GROUP;

            // Get configuration and configSections nodes
            XmlElement configElement = xmlDocument.DocumentElement;
            XmlElement configSectionsElement = configElement["configSections"];
            XmlNodeList sectionGroups = configSectionsElement.GetElementsByTagName("sectionGroup");
            XmlElement groupDefinitionElement = sectionGroups
                .OfType<XmlElement>()
                .Where(element => element.GetAttribute("name") == sectionName)
                .Single();
            XmlElement groupElement = configElement[sectionName];

            CreateConfigSection(xmlDocument, groupDefinitionElement, groupElement, userSection);

            // Save the file, then refresh the config file connection. Perform the save in this
            // overload; if it is done in the other the save will be duplicated in
            // CreateConfigGroup.
            xmlDocument.Save(_configFileName);

            if (Constructed)
            {
                ResetConfigFileConnection();
            }
        }

        /// <summary>
        /// Creates a config section in the appropriate group for the current assembly.
        /// </summary>
        /// <param name="xmlDocument">The <see cref="XmlDocument"/> in which the section is being
        /// created.</param>
        /// <param name="groupDefinitionElement">The <see cref="XmlElement"/> for the group's
        /// definition node.</param>
        /// <param name="groupElement">The <see cref="XmlElement"/> for the group node.
        /// </param>
        /// <param name="userSection"><see langword="true"/> to create the section in the
        /// UserSettingsGroup, <see langword="false"/> to create it in the
        /// ApplicationSettingsGroup,</param>
        void CreateConfigSection(XmlDocument xmlDocument, XmlElement groupDefinitionElement,
            XmlElement groupElement, bool userSection)
        {
            // Create a definition for the section and add it to the group's definition.
            XmlElement settingsSectionElement =
                CreateQualifiedTypeElement("section", Settings.GetType().ToString(),
                typeof(System.Configuration.ClientSettingsSection), xmlDocument);
            groupDefinitionElement.AppendChild(settingsSectionElement);

            // A user section should have the MachineToLocalUser scope applied.
            if (userSection)
            {
                XmlAttribute allowExeDefinitionAttribute =
                    xmlDocument.CreateAttribute("allowExeDefinition");
                allowExeDefinitionAttribute.Value = "MachineToLocalUser";
                settingsSectionElement.Attributes.Append(allowExeDefinitionAttribute);
            }

            XmlAttribute requirePermissionAttribute =
                xmlDocument.CreateAttribute("requirePermission");
            requirePermissionAttribute.Value = "false";
            settingsSectionElement.Attributes.Append(requirePermissionAttribute);

            // Create the config section itself.
            XmlElement typeSpecificElement =
                xmlDocument.CreateElement(Settings.GetType().ToString());
            groupElement.AppendChild(typeSpecificElement);
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
                var configSection = configSectionGroup.Sections[settingsType];

                if (configSection != null)
                {
                    if (configSection is not ClientSettingsSection clientConfigSection)
                    {
                        throw new ExtractException("ELI51709",
                            UtilityMethods.FormatInvariant($"Missing client config section for {settingsType}"));
                    }

                    ValidateSettings(clientConfigSection);

                    // Loop through all SettingsProperty to load any specified values from disk.
                    foreach (SettingsProperty setting in Settings.Properties)
                    {
                        string settingName = setting.Name;
                        SettingElement xmlSetting = clientConfigSection.Settings.Get(settingName);
                        SettingsProperty property = Settings.Properties[settingName];
                        if (xmlSetting != null)
                        {
                            if (property.PropertyType == typeof(StringCollection))
                            {
                                var sc = new StringCollection();
                                foreach (var node in xmlSetting.Value.ValueXml.ChildNodes
                                    .Cast<XmlNode>()
                                    .Where(x => x.Name.Equals("ArrayOfString", StringComparison.OrdinalIgnoreCase))
                                    .SingleOrDefault()?.ChildNodes
                                    .Cast<XmlNode>()
                                    .Where(x => x.Name.Equals("string", StringComparison.OrdinalIgnoreCase))
                                    ?? Enumerable.Empty<XmlNode>())
                                {
                                    sc.Add(node.GetNodeValue(TagUtility, false, true));
                                }
                                Settings[settingName] = sc;
                            }
                            else
                            {
                                // Get the string version of the value to apply to this setting,
                                // interpreted as XML and expanding tags as specified.
                                string stringValue =
                                    xmlSetting.Value.ValueXml.GetNodeValue(TagUtility, false, true);

                                // Convert the string XML value to the appropriate type and apply the
                                // value. GetNodeValue will have already expanded tags if appropriate,
                                // so further expansion is never necessary.
                                UpdatePropertyFromString(settingName, stringValue, false);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates that all settings in the supplied <see cref="ClientSettingsSection"/> are
        /// valid for the current _setting type.
        /// </summary>
        /// <param name="configSection">The <see cref="ClientSettingsSection"/> to validate.</param>
        void ValidateSettings(ClientSettingsSection configSection)
        {
            IEnumerable<SettingElement> invalidXmlSettings = configSection.Settings
                .Cast<SettingElement>()
                .Where(setting => Settings.Properties[setting.Name] == null);
            if (invalidXmlSettings.Any())
            {
                ExtractException ee = new ExtractException("ELI28826",
                    "Invalid config file setting(s)");
                ee.AddDebugData("Section", typeof(T).ToString());
                foreach (SettingElement invalidXmlSetting in invalidXmlSettings)
                {
                    ee.AddDebugData("Setting name", invalidXmlSetting.Name, false);
                }
                throw ee;
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
        /// <param name="instance">The <see langword="object"/> to apply the settings to.</param>
        void ProcessObjectSettings(XmlNodeList nodes, object instance)
        {
            Type objectType = instance.GetType();

            foreach (XmlNode node in nodes)
            {
                // Process property settings for specified member classes.
                if (node.Name.Equals("Member", StringComparison.OrdinalIgnoreCase))
                {
                    string memberName = GetXmlNameAttribute(node);

                    bool isField = true;
                    MemberInfo member = objectType.GetField(memberName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    // If not a field, try as a property
                    if (member == null)
                    {
                        isField = false;
                        member = objectType.GetProperty(memberName,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    }

                    if (member == null || !member.GetType().IsClass)
                    {
                        ExtractException ee = new ExtractException("ELI28806", "Unknown member!");
                        ee.AddDebugData("Member name", memberName, false);
                        throw ee;
                    }

                    if (isField)
                    {
                        ProcessObjectSettings(node.ChildNodes, ((FieldInfo)member).GetValue(instance));
                    }
                    else
                    {
                        ProcessObjectSettings(node.ChildNodes, ((PropertyInfo)member).GetValue(instance));
                    }
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

                    // Get the string version of the value to apply to this setting, interpreted
                    // as XML and expanding tags as specified (default to interpreting as XML as
                    // the legacy usage for the objectSettings in config files has been to override
                    // DEP control properties which are often XML queries).
                    string stringValue = node.GetNodeValue(TagUtility, true, false);

                    objectProperty.SetValue(instance,
                        TypeDescriptor.GetConverter(objectProperty.PropertyType).ConvertFromString(
                            stringValue), null);
                }
            }
        }

        /// <summary>
        /// Asserts the existence of and retrieves the value of the name <see cref="XmlAttribute"/>
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

        /// Whether the supplied path is the system path or a subdirectory of the system path
        static bool IsPathInSystemFolder(string appDataPath)
        {
            string systemPath = Environment.GetFolderPath(
                Environment.SpecialFolder.System,
                Environment.SpecialFolderOption.DoNotVerify);

            string systemPathx86 = Environment.GetFolderPath(
                Environment.SpecialFolder.SystemX86,
                Environment.SpecialFolderOption.DoNotVerify);

            return appDataPath.StartsWith(systemPath, StringComparison.OrdinalIgnoreCase)
                || appDataPath.StartsWith(systemPathx86, StringComparison.OrdinalIgnoreCase);
        }

        #endregion Private Members
    }

    /// <summary>
    /// Extension methods for the <see cref="ConfigSettings{T}"/> class.
    /// </summary>
    [CLSCompliant(false)]
    public static class ConfigSettingsExtensionMethods
    {
        /// <summary>
        /// Reads the string representation of the specified <see paramref="node"/>.
        /// NOTE: Path tags/functions are expanded as indicated by
        /// <see paramref="expandTagsDefault"/> or an "expandTags" attribute on the node, but path
        /// tags will not be expanded if the value is to be interpreted as literal XML.
        /// </summary>
        /// <param name="node">The <see cref="XmlNode"/> for which the value should be read.</param>
        /// <param name="tagUtility">The <see cref="ITagUtility"/> that should be used to expand
        /// setting values as they are loaded.</param>
        /// <param name="xmlDefault"><see langword="true"/> to treat the value as literal XML unless
        /// the node has and XML attribute specifying otherwise. If not treated as XML, XML escape
        /// characters will be interpreted.</param>
        /// <param name="expandTagsDefault"><see langword="true"/> to expand any path tag
        /// expressions in the string value; otherwise, <see langword="false"/>.</param>
        /// <returns>The string representation of the specified <see paramref="node"/>.</returns>
        // While the XPathNavigator type may be better for node, I think that would require
        // assumptions about which concrete type node was. Anyway, for the limited context this is
        // being used, I don't think the effort to re-write to use XPathNavigator is justified.
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public static string GetNodeValue(this XmlNode node, ITagUtility tagUtility,
            bool xmlDefault, bool expandTagsDefault)
        {
            try
            {
                bool explicitSetting = false;

                // Check if there is an attribute on the node specifying whether to treat the string
                // as literal XML.
                bool valueIsXml = xmlDefault;
                XmlAttribute xmlAttribute = node.Attributes?.OfType<XmlAttribute>()
                    .Where(attribute =>
                        attribute.Name.Equals("XML", StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();
                if (xmlAttribute != null)
                {
                    valueIsXml = xmlAttribute.Value.ToBoolean();
                    explicitSetting = true;
                }

                // Check if there is an attribute on the node specifying whether to expand any path
                // tags or functions.
                bool expandTags = expandTagsDefault;
                xmlAttribute = node.Attributes?.OfType<XmlAttribute>()
                    .Where(attribute =>
                        attribute.Name.Equals("expandTags", StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();
                if (xmlAttribute != null)
                {
                    expandTags = xmlAttribute.Value.ToBoolean();

                    ExtractException.Assert("ELI37924", "Conflicting config node settings; Path " +
                        "tags cannot be expanded on values interpreted as XML.",
                        !expandTags || !valueIsXml || !explicitSetting, "Setting Value", node.InnerXml);

                    // If the node value is explicitly indicated to have path tags expanded, assume
                    // it is not to be interpreted as XML.
                    valueIsXml = false;
                    expandTags = xmlAttribute.Value.ToBoolean();
                }

                string value;
                if (valueIsXml)
                {
                    value = node.InnerXml;
                }
                else
                {
                    value = node.InnerText;

                    if (expandTags && tagUtility != null)
                    {
                        value = tagUtility.ExpandTagsAndFunctions(value, "", null);
                    }
                }

                return value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37927");
            }
        }
    }
}
