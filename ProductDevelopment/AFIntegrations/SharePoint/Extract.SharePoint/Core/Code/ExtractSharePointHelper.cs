using Microsoft.SharePoint;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Extract.SharePoint
{
    internal static class ExtractSharePointHelper
    {
        #region Constants

        /// <summary>
        /// Path to the settings folder.
        /// </summary>
        readonly static string _APPLICATION_DATA_FOLDER = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Path.Combine("Extract Systems", "SharePoint"));

        /// <summary>
        /// Constant string used to define the folders to process property setting index.
        /// </summary>
        internal readonly static string _FOLDERS_TO_PROCESS = "FoldersToProcess";

        /// <summary>
        /// Constant string used to define the local ID Shield folder that files
        /// will be exported to.
        /// </summary>
        internal readonly static string _ID_SHIELD_LOCAL_FOLDER = "LocalIdShieldFolder";

        /// <summary>
        /// The feature ID string
        /// </summary>
        internal static readonly string _IDSHIELD_FEATURE_ID = "2d595bc7-785f-4ae5-a1db-65708f23c0d0";

        /// <summary>
        /// The feature ID GUID
        /// </summary>
        internal static readonly Guid _IDSHIELD_FEATURE_GUID = new Guid(_IDSHIELD_FEATURE_ID);

        /// <summary>
        /// Constants for the XML configuration element containing feature configuration
        /// settings.
        /// </summary>
        static readonly string _EXTRACT_CONFIGURATION_ELEMENT = "ExtractSharePointFeatureConfig";

        #endregion Constants

        #region Methods

        /// <summary>
        /// Helper method to persist feature settings.
        /// </summary>
        /// <param name="feature">The feature settings to persist.</param>
        internal static void SaveFeatureSettings(SPFeature feature)
        {
            if (!Directory.Exists(_APPLICATION_DATA_FOLDER))
            {
                Directory.CreateDirectory(_APPLICATION_DATA_FOLDER);
            }

            string fileName = Path.Combine(_APPLICATION_DATA_FOLDER,
                feature.DefinitionId.ToString() + ".config");

            XmlTextWriter writer = null;
            try
            {
                writer = new XmlTextWriter(fileName, Encoding.ASCII);
                writer.WriteStartDocument();
                writer.WriteStartElement(_EXTRACT_CONFIGURATION_ELEMENT);
                try
                {
                    foreach (SPFeatureProperty property in feature.Properties)
                    {
                        writer.WriteElementString(property.Name, property.Value);
                    }
                }
                finally
                {
                    writer.WriteEndElement();
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(_APPLICATION_DATA_FOLDER, "SaveException.txt"),
                    ex.Message + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Helper method to load feature settings.
        /// </summary>
        /// <param name="feature">The feature to load settings for.</param>
        internal static void LoadFeatureSettings(SPFeature feature)
        {
            string fileName = Path.Combine(_APPLICATION_DATA_FOLDER,
                feature.DefinitionId.ToString() + ".config");
            if (File.Exists(fileName))
            {
                XmlTextReader reader = null;
                try
                {
                    reader = new XmlTextReader(fileName);
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element
                            && !reader.LocalName.Equals(_EXTRACT_CONFIGURATION_ELEMENT,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            string setting = reader.LocalName;
                            if (!reader.IsEmptyElement
                                && reader.Read()
                                && reader.NodeType == XmlNodeType.Text)
                            {
                                SPFeatureProperty property = feature.Properties[setting];
                                if (property == null)
                                {
                                    property = new SPFeatureProperty(setting, reader.Value);
                                    feature.Properties.Add(property);
                                }
                                else
                                {
                                    property.Value = reader.Value;
                                }
                            }
                        }
                    }

                    feature.Properties.Update();
                }
                catch (Exception ex)
                {
                    File.AppendAllText(Path.Combine(_APPLICATION_DATA_FOLDER, "LoadException.txt"),
                        ex.Message + Environment.NewLine + ex.StackTrace);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
        }

        #endregion Methods
    }
}
