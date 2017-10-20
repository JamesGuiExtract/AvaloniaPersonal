using Extract.AttributeFinder;
using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using System.Xml;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;

namespace Extract.Redaction
{
    /// <summary>
    /// Interface definition for the Meta data task
    /// </summary>
    [ComVisible(true)]
    [Guid("4C969864-74F1-4C2E-8253-4D8EAC3D0041")]
    [CLSCompliant(false)]
    public interface IMetadataTask : ICategorizedComponent, IConfigurableObject, ICopyableObject,
                                IFileProcessingTask, ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets the the path to the input ID Shield data file. May contain tags.
        /// </summary>
        /// <value>The the path to the input ID Shield data file. May contain tags.</value>
        string DataFile { get; set; }

        /// <summary>
        /// Gets the path to the verification metadata xml file. May contain tags.
        /// </summary>
        /// <returns>The path to the verification metadata xml file. May contain tags.</returns>
        string MetadataFile { get; set; }
    }

    /// <summary>
    /// Represents a file processing task that performs verification of redactions.
    /// </summary>
    [ComVisible(true)]
    [Guid("7F567E34-CEBA-4C50-B2C9-B53BD13784FA")]
    [ProgId("Extract.Redaction.MetadataTask")]
    public class MetadataTask : IMetadataTask
    {
        #region Constants

        const string _COMPONENT_DESCRIPTION = "Redaction: Create metadata XML";

        const int _TASK_VERSION = 2;

        const int _METADATA_VERSION = 5;
        
        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="MetadataTask"/> 
        /// since it was created; <see langword="false"/> if no changes have been made since it
        /// was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Settings for creating metadata xml.
        /// </summary>
        MetadataSettings _settings;

        /// <summary>
        /// The processing vector of attributes (VOA) file;
        /// </summary>
        RedactionFileLoader _voaFile;

        /// <summary>
        /// The master list of valid exemption codes.
        /// </summary>
        MasterExemptionCodeList _masterCodes;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataTask"/> class.
        /// </summary>
        public MetadataTask()
        {
            _settings = new MetadataSettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataTask"/> class.
        /// </summary>
        public MetadataTask(MetadataTask task)
        {
            CopyFrom(task);
        }
        
        #endregion Constructors

        #region IMetadatTask Members

        /// <summary>
        /// Gets the the path to the input ID Shield data file. May contain tags.
        /// </summary>
        /// <value>
        /// The the path to the input ID Shield data file. May contain tags.
        /// </value>
        public string DataFile
        {
            get
            {
                return _settings.DataFile;
            }
            set
            {
                try
                {
                    bool dirty = !string.Equals(_settings.DataFile, value, StringComparison.Ordinal);
                    _settings.DataFile = value;
                    _dirty |= dirty;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32414", "Unable to update data file.");
                }
            }
        }

        /// <summary>
        /// Gets the path to the verification metadata xml file. May contain tags.
        /// </summary>
        /// <returns>The path to the verification metadata xml file. May contain tags.</returns>
        public string MetadataFile
        {
            get
            {
                return _settings.MetadataFile;
            }
            set
            {
                try
                {
                    bool dirty = !string.Equals(_settings.MetadataFile, value,
                        StringComparison.Ordinal);
                    _settings.MetadataFile = value;
                    _dirty |= dirty;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32415", "Unable to update metadata file.");
                }
            }
        }

        #endregion IMetadatTask Members

        #region Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// <see cref="ExtractCategories.FileProcessorsGuid"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// <see cref="ExtractCategories.FileProcessorsGuid"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="MetadataTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="MetadataTask"/> from which to copy.</param>
        public void CopyFrom(MetadataTask task)
        {
            _settings = task._settings;
        }

        /// <summary>
        /// Creates the specified xml file from the currently processing vector of attributes (VOA) 
        /// file.
        /// </summary>
        /// <param name="xmlFileName">The xml file to create.</param>
        void WriteXml(string xmlFileName)
        {
            TemporaryFile file = null;
            XmlWriter writer = null; 
            try 
	        {
                file = new TemporaryFile(".xml", true);
	            writer = XmlWriter.Create(file.FileName);
	            if (writer == null)
	            {
	                throw new ExtractException("ELI28571", 
	                    "Unable to write xml.");
	            }

                WriteXml(writer);

	            writer.Close();
	            writer = null;

                // Create the output directory if it doesn't already exist
                string xmlDirectory = Path.GetDirectoryName(xmlFileName);
	            Directory.CreateDirectory(xmlDirectory);

	            FileSystemMethods.MoveFile(file.FileName, xmlFileName, true);
	        }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
                if (file != null)
                {
                    file.Dispose();
                }
            }
        }

        /// <summary>
        /// Writes the information from the currently processing vector of attributes (VOA) file 
        /// to the xml writer.
        /// </summary>
        /// <param name="writer">The writer to perform the xml writing operation.</param>
        void WriteXml(XmlWriter writer)
        {
            // Root
            writer.WriteStartDocument();
            writer.WriteStartElement("IDShieldMetadata");
            writer.WriteAttributeString("Version", _METADATA_VERSION.ToString(CultureInfo.CurrentCulture));

            // Document Info
            WriteDocumentType(writer, _voaFile.DocumentType);

            // Current and previous revisions
            WriteCurrentItems(writer, _voaFile.Items);
            WriteRevisions(writer, _voaFile.RevisionsAttribute);

            // Verification, redaction and surround context sessions
            if (_voaFile.AllSessions.Count > 0)
            {
                WriteSessions(writer, _voaFile.AllSessions);
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        /// <summary>
        /// Writes the document type information to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="documentType">The document type to write.</param>
        static void WriteDocumentType(XmlWriter writer, string documentType)
        {
            if (documentType != null)
            {
                writer.WriteStartElement("DocumentInfo");
                writer.WriteElementString("DocumentType", documentType);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes the current redactions and clues to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="items">The current redactions and clues.</param>
        void WriteCurrentItems(XmlWriter writer, ICollection<SensitiveItem> items)
        {
            if (items.Count > 0)
            {
                writer.WriteStartElement("CurrentRevisions");

                foreach (SensitiveItem item in items)
                {
                    RedactionItem attribute = item.Attribute;

                    WriteRedaction(writer, attribute);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes the previous redactions and clues to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="revisions">The previous redaction and clues.</param>
        void WriteRevisions(XmlWriter writer, ComAttribute revisions)
        {
            if (revisions != null)
            {
                IUnknownVector subAttributes = revisions.SubAttributes;
                int count = subAttributes.Size();
                if (count > 0)
                {
                    writer.WriteStartElement("OldRevisions");
                    for (int i = 0; i < count; i++)
                    {
                        ComAttribute attribute = (ComAttribute) subAttributes.At(i);
                        if (attribute.Value.HasSpatialInfo())
                        {
                            RedactionItem item = new RedactionItem(attribute);

                            WriteRedaction(writer, item);
                        }
                    }
                    writer.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Writes a redaction or clue to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="item">The redaction or clue to write.</param>
        void WriteRedaction(XmlWriter writer, RedactionItem item)
        {
            // Category
            string category = item.Category;
            if (category.Equals("Clues", StringComparison.OrdinalIgnoreCase))
            {
                writer.WriteStartElement("Clue");
            }
            else
            {
                writer.WriteStartElement("Redaction");
                writer.WriteAttributeString("Category", category);
            }

            // Type
            writer.WriteAttributeString("Type", item.RedactionType);

            // ID and revision
            WriteRevisionId(writer, item);

            // Enabled
            writer.WriteAttributeString("Enabled", item.Redacted ? "1" : "0");

            // Zones
            WriteZones(writer, item);

            // Exemption codes
            WriteExemptions(writer, item);

            // Metadata the maps the ID in the source file to the ID the item was merged
            WriteMappings(writer, item);

            writer.WriteEndElement();
        }
        /// <summary>
        /// Writes the attribute ID and revision ID to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="item">The item containing the IDs to write.</param>
        static void WriteRevisionId(XmlWriter writer, RedactionItem item)
        {
            long id = item.GetId();
            writer.WriteAttributeString(Constants.ID, id.ToString(CultureInfo.CurrentCulture));
            int revision = item.GetRevision();
            writer.WriteAttributeString("Revision", revision.ToString(CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Writes the redaction zones to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="item">The item containing to the zones to write.</param>
        static void WriteZones(XmlWriter writer, RedactionItem item)
        {
            IUnknownVector zones = item.SpatialString.GetOriginalImageRasterZones();
            int count = zones.Size();
            for (int i = 0; i < count; i++)
            {
                // Get zone data
                ComRasterZone zone = (ComRasterZone) zones.At(i);
                int startX = 0, startY = 0, endX = 0, endY = 0, height = 0, page = 0;
                zone.GetData(ref startX, ref startY, ref endX, ref endY, ref height, ref page);
                            
                // Write the zone to xml
                writer.WriteStartElement("Zone");
                writer.WriteAttributeString("StartX", startX.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("StartY", startY.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("EndX", endX.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("EndY", endY.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("Height", height.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("PageNumber", page.ToString(CultureInfo.CurrentCulture));
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes the exemptions codes to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="item">The item containing to the exemption codes to write.</param>
        void WriteExemptions(XmlWriter writer, RedactionItem item)
        {
            ExemptionCodeList exemptions = item.GetExemptions(_masterCodes);
            if (!exemptions.IsEmpty)
            {
                writer.WriteStartElement("ExemptionCode");
                writer.WriteAttributeString("Category", exemptions.Category);
                writer.WriteAttributeString("Code", exemptions.ToString());
                writer.WriteEndElement();    
            }
        }

        /// <summary>
        /// Writes the elements that map attributes from source data files to the output data file.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="item">The item containing to the mapping(s) to write.</param>
        static void WriteMappings(XmlWriter writer, RedactionItem item)
        {
            foreach (ComAttribute mapping in item.ComAttribute.SubAttributes
                .ToIEnumerable<ComAttribute>())
            {
                string name = mapping.Name;
                if (name == Constants.ImportedToIDMetadata ||
                    name == Constants.MergedToIDMetadata)
                {
                    writer.WriteElementString(name.Substring(1), mapping.Value.String);
                }
            }
        }

        /// <summary>
        /// Writes all sessions to xml
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sessions"></param>
        void WriteSessions(XmlWriter writer, IEnumerable<ComAttribute> sessions)
        {
            foreach (ComAttribute session in sessions)
            {
                string sessionName = session.Name;

                if (sessionName.Equals(Constants.VerificationSessionMetaDataName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    WriteVerificationSession(writer, session);
                }
                else if (sessionName.Equals(Constants.RedactionSessionMetaDataName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    WriteRedactionSession(writer, session);
                }
                else if (sessionName.Equals(Constants.SurroundContextSessionMetaDataName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    WriteSurroundContextSession(writer, session);
                }
                else if (sessionName.Equals(Constants.VOAFileMergeSessionMetaDataName,
                    StringComparison.OrdinalIgnoreCase) ||
                         sessionName.Equals(Constants.VOAFileCompareSessionMetaDataName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    WriteVOAFileMergeSession(writer, session);
                }
                else
                {
                    ExtractException.ThrowLogicException("ELI30095");
                }
            }
        }

        /// <summary>
        /// Writes the verification sessions to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="session">The attribute containing the verification session data.</param>
        static void WriteVerificationSession(XmlWriter writer, ComAttribute session)
        {
            writer.WriteStartElement("IDShieldVerificationSession");
            writer.WriteAttributeString(Constants.ID, session.Value.String);

            IUnknownVector subAttributes = session.SubAttributes;

            // User Info and Time Info
            WriteUserInfo(writer, subAttributes);
            WriteTimeInfo(writer, subAttributes);

            // File Info and Verification Options
            WriteVerificationFileInfo(writer, subAttributes);
            WriteAttributeAsXml(writer, subAttributes, "VerificationOptions", "VerifyAllPages");

            // Entries Added, Deleted, and Modified
            WriteEntries(writer, subAttributes, "EntriesAdded");
            WriteEntries(writer, subAttributes, "EntriesDeleted");
            WriteEntries(writer, subAttributes, "EntriesModified");

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the verification FileInfo metadata attribute.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="attributes">A collection of attributes containing the attribute to write.
        /// </param>
        static void WriteVerificationFileInfo(XmlWriter writer, IUnknownVector attributes)
        {
            writer.WriteStartElement(Constants.FileInfo);

            WriteValueByName(writer, attributes, Constants.SourceDocName);
            WriteValueByName(writer, attributes, Constants.IDShieldDataFile);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the redaction session to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="session">The attribute containing the redaction session data.</param>
        static void WriteRedactionSession(XmlWriter writer, ComAttribute session)
        {
            writer.WriteStartElement("RedactedFileOutputSession");
            writer.WriteAttributeString(Constants.ID, session.Value.String);

            IUnknownVector subAttributes = session.SubAttributes;

            // User Info and Time Info
            WriteUserInfo(writer, subAttributes);
            WriteTimeInfo(writer, subAttributes);

            // File Info
            WriteRedactionFileInfo(writer, subAttributes);
            
            // Output Options
            WriteOutputOptions(writer, subAttributes);

            // Entries Added, Deleted, Modified
            WriteEntries(writer, subAttributes, "EntriesRedacted");

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the surround context session to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="session">The attribute containing the surround context data.</param>
        static void WriteSurroundContextSession(XmlWriter writer, ComAttribute session)
        {
            writer.WriteStartElement("SurroundContextSession");
            writer.WriteAttributeString(Constants.ID, session.Value.String);

            IUnknownVector subAttributes = session.SubAttributes;

            // User Info and Time Info
            WriteUserInfo(writer, subAttributes);
            WriteTimeInfo(writer, subAttributes);

            // File Info and Options
            WriteVerificationFileInfo(writer, subAttributes);
            WriteAttributeAsXml(writer, subAttributes, Constants.Options, 
                "TypesToExtend", "WordsToExtend", "ExtendHeight");

            // Entries Modified
            WriteEntries(writer, subAttributes, "EntriesModified");

            writer.WriteEndElement();
        }

        
        /// <summary>
        /// Writes the VOA file merge or compare session to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="session">The attribute containing the surround context data.</param>
        void WriteVOAFileMergeSession(XmlWriter writer, ComAttribute session)
        {
            writer.WriteStartElement(session.Name.Substring(1));
            writer.WriteAttributeString(Constants.ID, session.Value.String);

            IUnknownVector subAttributes = session.SubAttributes;

            // User Info and Time Info
            WriteUserInfo(writer, subAttributes);
            WriteTimeInfo(writer, subAttributes);

            // Options
            WriteAttributeAsXml(writer, subAttributes, Constants.Options);

            // File Info
            writer.WriteStartElement(Constants.FileInfo);

            WriteValueByName(writer, subAttributes, Constants.SourceDocName);
            WriteValueByName(writer, subAttributes, Constants.OutputFile);

            // For each source data file, output all data nested under an _IDShieldDataFile
            // attribute. For each current attribute, an element will be included to map it to an
            // redaction in the output file.
            foreach (ComAttribute sourceFileAttribute in subAttributes
                .ToIEnumerable<ComAttribute>()
                .Where(attribute => attribute.Name == Constants.IDShieldDataFileMetadata))
            {
                writer.WriteStartElement(Constants.IDShieldDataFile);
                writer.WriteAttributeString("Name", sourceFileAttribute.Value.String);

                // Load the sub-attribute heirarchy as if it were a separate file.
                RedactionFileLoader voaLoader = new RedactionFileLoader(_voaFile.ConfidenceLevels);
                voaLoader.LoadFrom(sourceFileAttribute.SubAttributes, sourceFileAttribute.Value.String);

                // Document Info
                WriteDocumentType(writer, voaLoader.DocumentType);

                // Current and previous revisions (includes mapping elements)
                WriteCurrentItems(writer, voaLoader.Items);
                WriteRevisions(writer, voaLoader.RevisionsAttribute);

                // Verification, redaction and surround context sessions
                if (voaLoader.AllSessions.Count > 0)
                {
                    WriteSessions(writer, voaLoader.AllSessions);
                }

                writer.WriteEndElement();
            }

            // FileInfo
            writer.WriteEndElement();

            // VOAFileMergeSession
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the verification FileInfo metadata attribute.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="attributes">A collection of attributes containing the attribute to write.
        /// </param>
        static void WriteRedactionFileInfo(XmlWriter writer, IUnknownVector attributes)
        {
            writer.WriteStartElement(Constants.FileInfo);

            WriteValueByName(writer, attributes, Constants.SourceDocName);
            WriteValueByName(writer, attributes, Constants.IDShieldDataFile);
            WriteValueByName(writer, attributes, Constants.OutputFile);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the redaction output options to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="attributes">A collection of attributes containing the output options 
        /// attribute.</param>
        static void WriteOutputOptions(XmlWriter writer, IUnknownVector attributes)
        {
            ComAttribute attribute = AttributeMethods.GetSingleAttributeByName(attributes, "_OutputOptions");

            writer.WriteStartElement("OutputOptions");

            if (attribute != null)
            {
                IUnknownVector subAttributes = attribute.SubAttributes;

                WriteValueByName(writer, subAttributes, "RetainExistingRedactionsInOutputFile");
                WriteValueByName(writer, subAttributes, "OutputFileExistedPriorToOutputOperation");
                WriteValueByName(writer, subAttributes, "RetainExistingAnnotations");
                WriteValueByName(writer, subAttributes, "ApplyRedactionsAsAnnotations");

                WriteAttributeAsXml(writer, subAttributes, "RedactionTextAndColorSettings",
                    "TextFormat", "FillColor", "BorderColor", "Font");
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the user info to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="attributes">A collection of attributes containing the user info 
        /// attribute.</param>
        static void WriteUserInfo(XmlWriter writer, IUnknownVector attributes)
        {
            WriteAttributeAsXml(writer, attributes, "UserInfo", "LoginID", "Computer");
        }

        /// <summary>
        /// Writes the time info to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="attributes">A collection of attributes containing the time info 
        /// attribute.</param>
        static void WriteTimeInfo(XmlWriter writer, IUnknownVector attributes)
        {
            WriteAttributeAsXml(writer, attributes, "TimeInfo", "Date", "TimeStarted", "TotalSeconds");
        }

        /// <summary>
        /// Writes the specified attributes and its specified sub attributes to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="attributes">A collection of attributes containing the attribute to write.
        /// </param>
        /// <param name="name">The name of the attribute to write.</param>
        /// <param name="subAttributeNames">The names of the subattributes to write.
        /// If no attribute names are specified, all sub-attributes will be written.</param>
        static void WriteAttributeAsXml(XmlWriter writer, IUnknownVector attributes, string name,
            params string[] subAttributeNames)
        {
            ComAttribute attribute = AttributeMethods.GetSingleAttributeByName(attributes, "_" + name);

            writer.WriteStartElement(name);

            if (attribute != null)
            {
                IUnknownVector subAttributes = attribute.SubAttributes;
                if (subAttributeNames.Length > 0)
                {
                    foreach (string subAttributeName in subAttributeNames)
                    {
                        WriteValueByName(writer, subAttributes, subAttributeName);
                    }
                }
                else
                {
                    foreach (ComAttribute subAttribute in subAttributes.ToIEnumerable<ComAttribute>())
                    {
                        writer.WriteElementString(subAttribute.Name.Substring(1),
                            subAttribute.Value.String);
                    }
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the contents of the specified attribute to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="attributes">A collection of attributes containing the attribute to write.
        /// </param>
        /// <param name="name">The name of the attribute to write.</param>
        static void WriteValueByName(XmlWriter writer, IUnknownVector attributes, string name)
        {
            ComAttribute attribute = AttributeMethods.GetSingleAttributeByName(attributes, "_" + name);
            if (attribute != null)
            {
                writer.WriteElementString(name, attribute.Value.String);
            }
        }

        /// <summary>
        /// Writes the contents of the specified revision entries to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="attributes">A collection of attributes containing the revision entries 
        /// attribute to write.
        /// </param>
        /// <param name="name">The name of the attribute to write.</param>
        static void WriteEntries(XmlWriter writer, IUnknownVector attributes, string name)
        {
            ComAttribute attribute = AttributeMethods.GetSingleAttributeByName(attributes, "_" + name);

            if (attribute != null)
            {
                IUnknownVector subAttributes = attribute.SubAttributes;

                writer.WriteStartElement(name);

                int size = subAttributes.Size();
                for (int i = 0; i < size; i++)
                {
                    ComAttribute idRevision = (ComAttribute)subAttributes.At(i);
                    if (idRevision.Name == Constants.IDAndRevisionMetadata)
                    {
                        writer.WriteStartElement("Entry");

                        writer.WriteAttributeString(Constants.ID, idRevision.Value.String);

                        string revision = idRevision.Type.Substring(1);
                        writer.WriteAttributeString("Revision", revision);

                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
            }
        }

        #endregion Methods

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="MetadataTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI28513",
					_COMPONENT_DESCRIPTION);

                // Allow the user to configure the settings
                using (MetadataSettingsDialog dialog = new MetadataSettingsDialog(_settings))
                {
                    bool result = dialog.ShowDialog() == DialogResult.OK;

                    // Store the result
                    if (result)
                    {
                        _settings = dialog.MetadataSettings;
                        _dirty = true;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI28514",
                    "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="MetadataTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="MetadataTask"/> instance.</returns>
        public object Clone()
        {
            return new MetadataTask(this);
        }

        /// <summary>
        /// Copies the specified <see cref="MetadataTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            CopyFrom((MetadataTask)pObject);
        }

        #endregion ICopyableObject Members
        
        #region IFileProcessingTask Members

        /// <summary>
        /// Gets the minimum stack size needed for the thread in which this task is to be run.
        /// </summary>
        /// <value>
        /// The the minimum stack size needed for the thread in which this task is to be run.
        /// </value>
        [CLSCompliant(false)]
        public uint MinStackSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns a value indicating that the task does not display a UI
        /// </summary>
        public bool DisplaysUI
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI28515",
					_COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI28516",
                    "Unable to cancel 'Create metadata XML' task.", ex);
            }
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI28517",
					_COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI28518",
                    "Unable to close 'Create metadata XML' task.", ex);
            }
        }

        /// <summary>
        /// Called to notify the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <see langword="true"/>. If the processor wants to cancel processing,
        ///	it should return <see langword="false"/>. If the processor does not immediately know
        ///	whether processing should be cancelled right away, it may block until it does know,
        ///	and return at that time.
        /// <para><b>Note</b></para>
        /// This call will be made on a different thread than the other calls, so the Standby call
        /// must be thread-safe. This allows the file processor to block on the Standby call, but
        /// it also means that call to <see cref="ProcessFile"/> or <see cref="Close"/> may come
        /// while the Standby call is still ocurring. If this happens, the return value of Standby
        /// will be ignored; however, Standby should promptly return in this case to avoid
        /// needlessly keeping a thread alive.
        /// </summary>
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        public bool Standby()
        {
            return true;
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="pFileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the task to carry out requests for files to be checked out, released or re-ordered
        /// in the queue.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            IFileRequestHandler pFileRequestHandler)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI28519",
					_COMPONENT_DESCRIPTION);

                // Retrieve the confidence levels if necessary
                if (_voaFile == null)
                {
                    InitializationSettings settings = new InitializationSettings();
                    ConfidenceLevelsCollection levels = settings.ConfidenceLevels;
                    _voaFile = new RedactionFileLoader(levels);
                }

                // Retrieve the master list of exemption codes
                _masterCodes = new MasterExemptionCodeList();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI28520",
                    "Unable to initialize 'Create metadata XML' task.", ex);
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
		/// <param name="pFileRecord">The file record that contains the info of the file being 
		/// processed.</param>
		/// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">A File Action Manager Tag Manager for expanding tags.</param>
        /// <param name="pDB">The File Action Manager database.</param>
        /// <param name="pProgressStatus">Object to provide progress status updates to caller.
        /// </param>
        /// <param name="bCancelRequested"><see langword="true"/> if cancel was requested; 
        /// <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if processing should continue; <see langword="false"/> 
        /// if all file processing should be cancelled.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord, int nActionID,
            FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI28521",
					_COMPONENT_DESCRIPTION);

                FileActionManagerPathTags tags =
                    new FileActionManagerPathTags(pFAMTM, pFileRecord.Name);

                // Load voa
                string voaFileName = tags.Expand(_settings.DataFile);
                if (!File.Exists(voaFileName))
                {
                    ExtractException ee = new ExtractException("ELI28573",
                        "Voa file not found.");
                    ee.AddDebugData("Voa file", voaFileName, false);
                    throw ee;
                }
                _voaFile.LoadFrom(voaFileName, pFileRecord.Name);

                // Write xml
                string xmlFileName = tags.Expand(_settings.MetadataFile);
                WriteXml(xmlFileName);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI28522",
                    "Unable to process file.", ex);
            }
        }

		#endregion IFileProcessingTask Members

		#region IAccessRequired Members

		/// <summary>
		/// Returns bool value indicating if the task requires admin access
		/// </summary>
		/// <returns><see langword="true"/> if the task requires admin access
		/// <see langword="false"/> if task does not require admin access</returns>
		public bool RequiresAdminAccess()
		{
			return false;
		}

		#endregion IAccessRequired Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.IDShieldCoreObjects);
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns><see cref="HResult.Ok"/> if changes have been made; 
        /// <see cref="HResult.False"/> if changes have not been made.</returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _TASK_VERSION))
                {
                    // Read the settings
                    _settings = MetadataSettings.ReadFrom(reader);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI28523",
                    "Unable to load 'Create metadata XML' task.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            try 
            {
                using (IStreamWriter writer = new IStreamWriter(_TASK_VERSION))
                {
                    // Serialize the settings
                    _settings.WriteTo(writer);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI28524",
                    "Unable to save 'Create metadata XML' task.", ex);
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in 
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStream Members
    }
}
