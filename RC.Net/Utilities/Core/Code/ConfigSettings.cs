using Extract;
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
    /// Exposes settings from a .NET application config file.
    /// </summary>
    static public class ConfigSettings
    {
        /// <summary>
        /// Applies settings from the specified config file to the specified object.
        /// </summary>
        /// <param name="configFileName">The config file to use as a source for the settings.
        /// <param name="instance">The object to apply the settings to.</param>
        /// </param>
        public static void ApplySettings(string configFileName, object instance)
        {
            try
            {
                Type objectType = instance.GetType();

                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configFileName;

                // Open the config file
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(
                    configFileMap, ConfigurationUserLevel.None);

                // Locate the objectSettings group
                ConfigurationSectionGroup configSectionGroup =
                    config.SectionGroups["objectSettings"];

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
                ee.AddDebugData("Configuration filename", configFileName, false);
                throw ee;
            }
        }
 
        /// <summary>
        /// Creates and initializes an application settings instance of type T using the specified
        /// config file.
        /// </summary>
        /// <param name="configFileName">The config file to use as a source for the settings.
        /// </param>
        /// <returns>An initialized <see cref="ApplicationSettingsBase"/> derivation of type T.
        /// </returns>
        /// Despite the fact that no parameter of type T is being passed in, it is still useful for
        /// this method to be generic in order to create the new <see cref="ApplicationSettingsBase"/>
        /// instance .
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T InitializeSettings<T>(string configFileName)
             where T : System.Configuration.ApplicationSettingsBase, new()
        {
            try
            {
                // Create a new instance (will have the default settings)
                T settings = new T();

                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configFileName;

                // Open the config file
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(
                    configFileMap, ConfigurationUserLevel.None);

                // Locate the applicationSettings.
                ConfigurationSectionGroup configSectionGroup =
                    config.SectionGroups["applicationSettings"];

                // Retrieve the settings associated with the Setting's class's type.
                string settingsType = typeof(T).ToString();
                ClientSettingsSection configSection =
                    (ClientSettingsSection)configSectionGroup.Sections[settingsType];

                // Loop through each setting and apply the value from the config file. 
                foreach (SettingElement setting in configSection.Settings)
                {
                    object settingObject = settings[setting.Name];
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
                    settings[setting.Name] =
                        TypeDescriptor.GetConverter(settingType).ConvertFromString(
                            setting.Value.ValueXml.InnerXml);
                }

                return settings;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI25407",
                    "Invalid or missing configuration file!", ex);
                ee.AddDebugData("Configuration filename", configFileName, false);
                throw ee;
            }
        }

        #endregion Public Methods

        #region Private Members

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
