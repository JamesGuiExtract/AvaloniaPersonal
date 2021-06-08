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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Specifies the method of XML schema validation that should be used by the\
    /// <see cref="ValidateXmlTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("5E80FE00-7055-478B-ACFA-F8BB3DF5A8B6")]
    public enum XmlSchemaValidation
    {
        /// <summary>
        /// No schema validation should be performed; only XML syntax will be validated.
        /// </summary>
        None = 0,

        /// <summary>
        /// Any in-line schema defined in the XML files themselves will be used to perform schema
        /// validation.
        /// </summary>
        InlineSchema = 1,

        /// <summary>
        /// A specified schema will be used to perform schema validation.
        /// </summary>
        SpecifiedSchema = 2
    }

    /// <summary>
    /// Interface definition for the <see cref="ValidateXmlTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("5F6F8E06-E63F-4518-82FC-C32E9144458B")]
    [CLSCompliant(false)]
    public interface IValidateXmlTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask,
        ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets the name of the XML file to validate.
        /// </summary>
        /// <value>
        /// The name of the XML file to validate.
        /// </value>
        string XmlFileName { get; set; }

        /// <summary>
        /// Gets or sets whether the file should be failed if any warnings are generated during
        /// validation.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the file should be failed if any warnings are generated during
        /// validation; <see langword="false"/> if the file should be considered valid if only
        /// validation warning (but no errors) are encountered.
        /// </value>
        bool TreatWarningsAsErrors { get; set; }

        /// <summary>
        /// Gets or sets the method of schema validation that should be used.
        /// </summary>
        /// <value>
        /// The method of schema validation that should be used.
        /// </value>
        XmlSchemaValidation XmlSchemaValidation { get; set; }

        /// <summary>
        /// Gets or sets whether the XML file should be failed if no in-line schema is specified.
        /// </summary>
        bool RequireInlineSchema { get; set; }

        /// <summary>
        /// Gets or sets an explicitly named schema definition file to use to validate the XML
        /// schema.
        /// </summary>
        string SchemaFileName { get; set; }
    }

    /// <summary>
    /// A <see cref="IFileProcessingTask"/> that allows validation of XML file syntax and schema.
    /// </summary>
    [ComVisible(true)]
    [Guid("2972F82D-DCD6-4EBB-9046-B9DB5769D57D")]
    [ProgId("Extract.FileActionManager.FileProcessors.ValidateXmlTask")]
    public class ValidateXmlTask : IValidateXmlTask
    {
         #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Validate XML";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the XML file to validate.
        /// </summary>
        string _xmlFileName = "";

        /// <summary>
        /// Indicates whether the file should be failed if any warnings are generated during
        /// validation.
        /// </summary>
        bool _treatWarningsAsErrors;

        /// <summary>
        /// The method of schema validation that should be used.
        /// </summary>
        XmlSchemaValidation _xmlSchemaValidation = XmlSchemaValidation.InlineSchema;

        /// <summary>
        /// Indicates whether the XML file should be failed if no in-line schema is specified.
        /// </summary>
        bool _requireInlineSchema;

        /// <summary>
        /// An explicitly named schema definition file to use to validate the XML schema.
        /// </summary>
        string _schemaFileName = "";

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateXmlTask"/> class.
        /// </summary>
        public ValidateXmlTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateXmlTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="ValidateXmlTask"/> from which settings should
        /// be copied.</param>
        public ValidateXmlTask(ValidateXmlTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38372");
            }
        }

        #endregion Constructors

        #region IValidateXMLTask Members

        /// <summary>
        /// Gets or sets the name of the XML file to validate.
        /// </summary>
        /// <value>
        /// The name of the XML file to validate.
        /// </value>
        public string XmlFileName
        {
            get
            {
                return _xmlFileName;
            }

            set
            {
                if (!string.Equals(_xmlFileName, value, StringComparison.Ordinal))
                {
                    _xmlFileName = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the file should be failed if any warnings are generated during
        /// validation.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the file should be failed if any warnings are generated during
        /// validation; <see langword="false"/> if the file should be considered valid if only
        /// validation warning (but no errors) are encountered.
        /// </value>
        public bool TreatWarningsAsErrors
        {
            get
            {
                return _treatWarningsAsErrors;
            }

            set
            {
                if (value != _treatWarningsAsErrors)
                {
                    _treatWarningsAsErrors = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the method of schema validation that should be used.
        /// </summary>
        /// <value>
        /// The method of schema validation that should be used.
        /// </value>
        public XmlSchemaValidation XmlSchemaValidation
        {
            get
            {
                return _xmlSchemaValidation;
            }

            set
            {
                if (value != _xmlSchemaValidation)
                {
                    _xmlSchemaValidation = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the XML file should be failed if no in-line schema is specified.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the XML file should be failed if no in-line schema is
        /// specified; <see langword="false"/> if schema validation can be skipped if no in-line
        /// schema is provided.
        /// </value>
        public bool RequireInlineSchema
        {
            get
            {
                return _requireInlineSchema;
            }

            set
            {
                if (value != _requireInlineSchema)
                {
                    _requireInlineSchema = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets an explicitly named schema definition file to use to validate the XML
        /// schema when <see cref="XmlSchemaValidation"/> is
        /// <see cref="T:XmlSchemaValidation.SpecifiedSchema"/>.
        /// </summary> 
        /// <value>
        /// An explicitly named schema definition file to use to validate the XML schema.
        /// </value>
        public string SchemaFileName
        {
            get
            {
                return _schemaFileName;
            }

            set
            {
                if (!string.Equals(_schemaFileName, value, StringComparison.Ordinal))
                {
                    _schemaFileName = value;
                    _dirty = true;
                }
            }
        }

        #endregion IValidateXMLTask Members

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
        /// Performs configuration needed to create a valid <see cref="ValidateXmlTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI38373", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (ValidateXmlTask)Clone();

                using (var dialog = new ValidateXmlTaskSettingsDialog(cloneOfThis))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dialog.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI38374",
                    "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                return (!string.IsNullOrWhiteSpace(XmlFileName) &&
                    (XmlSchemaValidation != XmlSchemaValidation.SpecifiedSchema || 
                        !string.IsNullOrWhiteSpace(SchemaFileName)));
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38375", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ValidateXmlTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ValidateXmlTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new ValidateXmlTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI38376", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ValidateXmlTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as ValidateXmlTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to ValidateXMLTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI38377", "Unable to copy object.", ex);
            }
        }

        #endregion ICopyableObject Members

        #region IFileProcessingTask Members

        /// <summary>
        /// Gets the minimum stack size needed for the thread in which this task is to be run.
        /// </summary>
        /// <value>
        /// The minimum stack size needed for the thread in which this task is to be run.
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
            // Do nothing, this task is not cancellable
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            // Nothing to do
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
        /// while the Standby call is still occurring. If this happens, the return value of Standby
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
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use to expand path tags and
        /// functions.</param>
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
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI38378", _COMPONENT_DESCRIPTION);

                if (XmlSchemaValidation == XmlSchemaValidation.SpecifiedSchema &&
                    SchemaFileName.IndexOf("<SourceDocName>", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    FileActionManagerPathTags pathTags = new FileActionManagerPathTags(
                        pFAMTM, null);

                    string schemaFileName = pathTags.Expand(SchemaFileName);

                    // If appears we can derive the name XML schema definition file or it an
                    // absolute path name (no path tags), validate the file on init vs failing files
                    // during processing.
                    if (File.Exists(schemaFileName) ||
                        !Regex.IsMatch(schemaFileName, @"<|>|(\$[\s\S]+?\([\s\S]*?\))"))
                    {
                        try
                        {
                            XmlReaderSettings testSettings = new XmlReaderSettings();
                            testSettings.Schemas.Add(null, schemaFileName);
                        }
                        catch (Exception ex)
                        {
                            var ee = new ExtractException("ELI38391", "Invalid XML schema definition.", ex);
                            ee.AddDebugData("Schema filename", SchemaFileName, false);
                            throw ee;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38379", "Unable to initialize \"Validate XML\" task.");
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
        /// <returns>An <see cref="EFileProcessingResult"/> indicating the result of the
        /// processing.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord,
            int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI38389", _COMPONENT_DESCRIPTION);

                FileActionManagerPathTags pathTags = new FileActionManagerPathTags(
                    pFAMTM, pFileRecord.Name);

                string xmlFileName = pathTags.Expand(XmlFileName);
                ExtractException.Assert("ELI38392", "XML file not found.", File.Exists(xmlFileName),
                    "XML filename", xmlFileName);

                XmlReaderSettings xmlReaderSettings = GetXMLReaderSettings(pathTags);
                var validationErrorList = new List<ValidationEventArgs>();
                xmlReaderSettings.ValidationEventHandler += (o, args) =>
                    validationErrorList.Add(args);

                try
                {
                    using var inputStream = new FileStream(xmlFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var reader = UtilityMethods.GetXmlStreamReader(inputStream, defaultEncoding: System.Text.Encoding.GetEncoding("Windows-1252"));
                    using var xmlReader = XmlReader.Create(reader, xmlReaderSettings);

                    // Parse the file. 
                    while (xmlReader.Read());

                    // Validate in-line schema exists if required, only after reading the XML;
                    // The in-line schema is not initialized until the file is read.
                    if (XmlSchemaValidation == XmlSchemaValidation.InlineSchema &&
                        RequireInlineSchema && xmlReader.Settings.Schemas.Count == 0)
                    {
                        throw new ExtractException("ELI38399", "No in-line schema definition found.");
                    }

                    if (validationErrorList.Count > 0)
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, 
                            "{0} issues were found", validationErrorList.Count);

                        var ee = new ExtractException("ELI38390", message);
                        foreach (var error in validationErrorList.Take(10))
                        {
                            ee.AddDebugData((error.Severity == XmlSeverityType.Error)
                                    ? "Error"
                                    : "Warning",
                                error.Message, false);
                        }

                        if (validationErrorList.Count > 10)
                        {
                            ee.AddDebugData("Notice", string.Format(CultureInfo.CurrentCulture,
                                    "{0} additional issues not listed.",
                                    validationErrorList.Count - 10),
                                false);
                        }

                        throw ee;
                    }
                }
                catch (Exception ex)
                {
                    var ee = new ExtractException("ELI38394", "XML file is invalid.", ex);
                    ee.AddDebugData("XML filename", xmlFileName, false);
                    throw ee;
                }

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38380", "Unable to process the file.");
            }
        }

        #endregion IFileProcessingTask Members

        #region IAccessRequired Members

        /// <summary>
        /// Returns bool value indicating if the task requires admin access.
        /// </summary>
        /// <returns><see langword="true"/> if the task requires admin access
        /// <see langword="false"/> if task does not require admin access.</returns>
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
            try
            {
                return LicenseUtilities.IsLicensed(_LICENSE_ID);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI38381",
                    "Unable to determine license status.", ex);
            }
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
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    _xmlFileName = reader.ReadString();
                    _treatWarningsAsErrors = reader.ReadBoolean();
                    _xmlSchemaValidation = (XmlSchemaValidation)reader.ReadInt32();
                    _requireInlineSchema = reader.ReadBoolean();
                    _schemaFileName = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI38382",
                    "Unable to load object from stream.", ex);
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
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(_xmlFileName);
                    writer.Write(_treatWarningsAsErrors);
                    writer.Write((int)_xmlSchemaValidation);
                    writer.Write(_requireInlineSchema);
                    writer.Write(_schemaFileName);

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
                throw ExtractException.CreateComVisible("ELI38383",
                    "Unable to save object to stream", ex);
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

        #region Private Members

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
        /// Copies the specified <see cref="ValidateXmlTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="ValidateXmlTask"/> from which to copy.</param>
        void CopyFrom(ValidateXmlTask task)
        {
            _xmlFileName = task.XmlFileName;
            _treatWarningsAsErrors = task.TreatWarningsAsErrors;
            _xmlSchemaValidation = task.XmlSchemaValidation;
            _requireInlineSchema = task.RequireInlineSchema;
            _schemaFileName = task.SchemaFileName;

            _dirty = true;
        }

        /// <summary>
        /// Gets a <see cref="XmlReaderSettings"/> instance with =validation settings that reflect
        /// this task's settings.
        /// </summary>
        /// <param name="pathTags">The <see cref="FileActionManagerPathTags"/> to use to expand any
        /// path tags or functions.</param>
        /// <returns>The <see cref="XmlReaderSettings"/> instance.</returns>
        XmlReaderSettings GetXMLReaderSettings(FileActionManagerPathTags pathTags)
        {
            // Set the validation settings.
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.ValidationType =
                (XmlSchemaValidation == XmlSchemaValidation.None)
                    ? ValidationType.None
                    : ValidationType.Schema;

            switch (XmlSchemaValidation)
            {
                case FileProcessors.XmlSchemaValidation.None:
                    {
                        xmlReaderSettings.ValidationFlags = XmlSchemaValidationFlags.None;
                    }
                    break;

                case XmlSchemaValidation.InlineSchema:
                    {
                        xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                        xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                    }
                    break;

                case XmlSchemaValidation.SpecifiedSchema:
                    {
                        string schemaFileName = pathTags.Expand(SchemaFileName);
                        ExtractException.Assert("ELI40345", "XML schema definition file not found.",
                            File.Exists(schemaFileName), "Schema definition filename", schemaFileName);
                        xmlReaderSettings.Schemas.Add(null, schemaFileName);
                    }
                    break;

                default:
                    {
                        ExtractException.ThrowLogicException("ELI38401");
                    }
                    break;
            }

            if (TreatWarningsAsErrors)
            {
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            }

            return xmlReaderSettings;
        }

        #endregion Private Members
    }
}
