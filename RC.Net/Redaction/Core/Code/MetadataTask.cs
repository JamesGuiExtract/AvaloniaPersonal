using Extract.AttributeFinder;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using System.Xml;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a file processing task that performs verification of redactions.
    /// </summary>
    [ComVisible(true)]
    [Guid("7F567E34-CEBA-4C50-B2C9-B53BD13784FA")]
    [ProgId("Extract.Redaction.MetadataTask")]
    public class MetadataTask : ICategorizedComponent, IConfigurableObject, ICopyableObject,
                                IFileProcessingTask, ILicensedComponent, IPersistStream
    {
        #region Constants

        const string _COMPONENT_DESCRIPTION = "Redaction: Create metadata xml";

        const int _TASK_VERSION = 2;

        const int _METADATA_VERSION = 4;
        
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
        
        #region Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// <see cref="ExtractGuids.FileProcessors"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// <see cref="ExtractGuids.FileProcessors"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileProcessors);
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
                file = new TemporaryFile(".xml");
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

            // Verification and redaction sessions
            if (_voaFile.VerificationSessions.Count > 0)
            {
                WriteVerificationSessions(writer, _voaFile.VerificationSessions);
            }

            if (_voaFile.RedactionSessions.Count > 0)
            {
                WriteRedactionSessions(writer, _voaFile.RedactionSessions);
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
                        RedactionItem item = new RedactionItem(attribute);

                        WriteRedaction(writer, item);
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
            writer.WriteAttributeString("ID", id.ToString(CultureInfo.CurrentCulture));
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
        /// Writes the verification sessions to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="sessions">The attributes containing the verification session data.</param>
        static void WriteVerificationSessions(XmlWriter writer, IEnumerable<ComAttribute> sessions)
        {
            foreach (ComAttribute session in sessions)
            {
                writer.WriteStartElement("IDShieldVerificationSession");
                writer.WriteAttributeString("ID", session.Value.String);

                IUnknownVector subAttributes = session.SubAttributes;

                // User Info and Time Info
                WriteUserInfo(writer, subAttributes);
                WriteTimeInfo(writer, subAttributes);

                // File Info and Verification Options
                WriteAttributeAsXml(writer, subAttributes, "FileInfo", "SourceDocName", "IDShieldDataFile");
                WriteAttributeAsXml(writer, subAttributes, "VerificationOptions", "VerifyAllPages");

                // Entries Added, Deleted, and Modified
                WriteEntries(writer, subAttributes, "EntriesAdded");
                WriteEntries(writer, subAttributes, "EntriesDeleted");
                WriteEntries(writer, subAttributes, "EntriesModified");

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes the redaction sessions to xml.
        /// </summary>
        /// <param name="writer">The writer to write the xml.</param>
        /// <param name="sessions">The attributes containing the redaction session data.</param>
        static void WriteRedactionSessions(XmlWriter writer, IEnumerable<ComAttribute> sessions)
        {
            foreach (ComAttribute session in sessions)
            {
                writer.WriteStartElement("RedactedFileOutputSession");
                writer.WriteAttributeString("ID", session.Value.String);

                IUnknownVector subAttributes = session.SubAttributes;

                // User Info and Time Info
                WriteUserInfo(writer, subAttributes);
                WriteTimeInfo(writer, subAttributes);

                // File Info
                WriteAttributeAsXml(writer, subAttributes, "FileInfo", "SourceDocName", "IDShieldDataFile", "OutputFile");
                
                // Output Options
                WriteOutputOptions(writer, subAttributes);

                // Entries Added, Deleted, Modified
                WriteEntries(writer, subAttributes, "EntriesRedacted");

                writer.WriteEndElement();
            }
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
        /// <param name="subAttributeNames">The names of the subattributes to write.</param>
        static void WriteAttributeAsXml(XmlWriter writer, IUnknownVector attributes, string name,
            params string[] subAttributeNames)
        {
            ComAttribute attribute = AttributeMethods.GetSingleAttributeByName(attributes, "_" + name);

            writer.WriteStartElement(name);

            if (attribute != null)
            {
                IUnknownVector subAttributes = attribute.SubAttributes;
                foreach (string subAttributeName in subAttributeNames)
                {
                    WriteValueByName(writer, subAttributes, subAttributeName);
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
                    if (idRevision.Name == "_IDAndRevision")
                    {
                        writer.WriteStartElement("Entry");

                        writer.WriteAttributeString("ID", idRevision.Value.String);

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
                    "Unable to cancel 'Create metadata xml' task.", ex);
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
                    "Unable to close 'Create metadata xml' task.", ex);
            }
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
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
                    "Unable to initialize 'Create metadata xml' task.", ex);
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="bstrFileFullName">The file to process.</param>
        /// <param name="nFileID">The ID of the file being processed.</param>
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
        public EFileProcessingResult ProcessFile(string bstrFileFullName, int nFileID, int nActionID,
            FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI28521",
					_COMPONENT_DESCRIPTION);

                FileActionManagerPathTags tags = 
                    new FileActionManagerPathTags(bstrFileFullName, pFAMTM.FPSFileDir);

                // Load voa
                string voaFileName = tags.Expand(_settings.DataFile);
                if (!File.Exists(voaFileName))
                {
                    ExtractException ee = new ExtractException("ELI28573",
                        "Voa file not found.");
                    ee.AddDebugData("Voa file", voaFileName, false);
                    throw ee;
                }
                _voaFile.LoadFrom(voaFileName, bstrFileFullName);

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
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.
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
                    "Unable to load 'Create metadata xml' task.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> into which the object should be saved.
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
                    "Unable to save 'Create metadata xml' task.", ex);
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
