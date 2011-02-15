using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Redaction
{
    /// <summary>
    /// An IFileProcessingTask which produces redacted text file based on VOA file data.
    /// </summary>
    [ComVisible(true)]
    [Guid("1A584AB7-2AC1-4B0D-AD38-718055F21142")]
    [ProgId("Extract.Redaction.CreateRedactedTextTask")]
    public class CreateRedactedTextTask : IFileProcessingTask, IConfigurableObject, IAccessRequired,
        ICategorizedComponent, ICopyableObject, ILicensedComponent, IPersistStream
    {
        #region Constants

        /// <summary>
        /// The COM object name.
        /// </summary>
        internal const string _COMPONENT_DESCRIPTION = "Redaction: Create redacted text";

        /// <summary>
        /// Current task version.
        /// </summary>
        internal const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="CreateRedactedTextTask"/> 
        /// since it was created; <see langword="false"/> if no changes have been made since it
        /// was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The settings for this object.
        /// </summary>
        CreateRedactedTextSettings _settings;

        /// <summary>
        /// Loads attributes to be redacted from voa files.
        /// </summary>
        RedactionFileLoader _voaLoader;

        /// <summary>
        /// Used to see out characters in sensitive data that should be replaced.
        /// </summary>
        Regex _regex;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRedactedTextTask"/> class.
        /// </summary>
        public CreateRedactedTextTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRedactedTextTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="CreateRedactedTextTask"/> from which settings should
        /// be copied.</param>
        public CreateRedactedTextTask(CreateRedactedTextTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31634",
                    "Unabled to create \"Create redacted text\" task.", ex);
            }
        }

        #endregion Constructors

        #region IFileProcessingTask Members

        /// <summary>
        /// Stops processing the current file (ignored by this class).
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI31635",
                    _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31636",
                    "Unable to cancel \"Create redacted text\" task.", ex);
            }
        }

        /// <summary>
        /// Called when all file processing has completed (ignored by this class).
        /// </summary>
        public void Close()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI31637",
                    _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31638",
                    "Unable to close 'Extend redactions to surround context' task.", ex);
            }
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI31639",
                    _COMPONENT_DESCRIPTION);

                // Create a voa file loader to load the attributes to be redacted.
                List<ConfidenceLevel> confidenceLevels = new List<ConfidenceLevel>();
                if (_settings.RedactAllTypes)
                {
                    // Create a ConfidenceLevel that queries for all attributes.
                    confidenceLevels.Add(
                        new ConfidenceLevel("*", "*", Color.Black, true, false, false));
                }
                else
                {
                    foreach (string dataType in _settings.DataTypes
                        .Where(type => _settings.IsTypeToRedact(type)))
                    {
                        // Create a ConfidenceLevel that query for each specified data type that is to
                        // be redacted.
                        confidenceLevels.Add(
                            new ConfidenceLevel(dataType, dataType, Color.Black, true, false, false));
                    }
                }

                _voaLoader = new RedactionFileLoader(new ConfidenceLevelsCollection(confidenceLevels));

                // If replacing only alpha numeric characters, create a Regex to seek out only alpha
                // numic characters.
                if (_settings.ReplaceCharacters &&
                    _settings.CharactersToReplace == CharacterClass.Alphanumeric)
                {
                    _regex = new Regex("\\w");
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31640",
                    "Unable to initialize \"Create redacted text\" task.", ex);
            }
        }

        /// <summary>
        /// Process the source file to create a redacted text file.
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI31641",
                    _COMPONENT_DESCRIPTION);

                // Load the "spatial" string containing the text index data.
                SpatialString source = new SpatialString();
                string ussFileName = pFileRecord.Name + ".uss";
                if (File.Exists(ussFileName))
                {
                    source.LoadFrom(ussFileName, false);
                }
                else
                {
                    source.LoadFrom(pFileRecord.Name, false);
                }

                // Initialize the source string and a list of character indexes for mapping indexes
                // in the source string to the current location in the output string.
                string sourceString = source.String;
                int length = sourceString.Length;
                List<int> charIndexes = new List<int>(Enumerable.Range(0, length));

                StringBuilder outputString = new StringBuilder(sourceString);

                // Load the redactions
                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFileRecord.Name, pFAMTM.FPSFileDir);
                string voaFile = pathTags.Expand(_settings.DataFile);
                _voaLoader.LoadFrom(voaFile, pFileRecord.Name);

                // Loop through each attribute to redact the text
                foreach (SpatialString value in _voaLoader.Items
                    .Where(sensitiveItem => sensitiveItem.Attribute.Redacted)
                    .Select(sensitiveItem => sensitiveItem.Attribute.ComAttribute.Value)
                    .Where(value => value.HasSpatialInfo()))
                {
                    // Text is "indexed" by storing the index of each character as the "left"
                    // coordinate in the attribute's spatial string.
                    ILongRectangle boundingRect = value.GetOCRImageBounds();
                    int startIndex = boundingRect.Left;
                    int endIndex = boundingRect.Right;

                    // Find the corresponding start index and length in the outputString. Any
                    // part of the string that has already been redacted by a different attribute
                    // will not be included in this range of characters.
                    // TODO: Handle case that characters in the middle of the attribute have been
                    // replaced.
                    int outputStartIndex = -1;
                    int sourceIndex;
                    for (sourceIndex = startIndex; sourceIndex <= endIndex; sourceIndex++)
                    {
                        int outputIndex = charIndexes.FindIndex((index) => sourceIndex == index);

                        if (outputStartIndex == -1)
                        {
                            if (outputIndex != -1)
                            {
                                startIndex = sourceIndex;
                                outputStartIndex = outputIndex;
                            }
                        }
                        else if (outputIndex == -1)
                        {
                            break;
                        }
                    }

                    // If all the source text has already been replaced, there is nothing to do for
                    // this attribute.
                    if (outputStartIndex == -1)
                    {
                        continue;
                    }

                    // Calculate the text to replace.
                    int lengthToReplace = sourceIndex - startIndex;
                    string textToReplace = sourceString.Substring(startIndex, lengthToReplace);
                    string replacementText = string.Empty;

                    // Calculate the text replacement text.
                    if (_settings.ReplaceCharacters)
                    {
                        if (_settings.CharactersToReplace == CharacterClass.All)
                        {
                            if (!string.IsNullOrEmpty(_settings.ReplacementValue))
                            {
                                replacementText =
                                    new string(_settings.ReplacementValue[0], lengthToReplace);
                            }
                        }
                        else
                        {
                            replacementText = _regex.Replace(textToReplace, _settings.ReplacementValue ?? "");
                        }
                    }
                    else
                    {
                        StringBuilder replacementBuilder = new StringBuilder(
                            textToReplace.Length + (_settings.XmlElementName.Length * 2) + 5);

                        replacementBuilder.Append("<");
                        replacementBuilder.Append(_settings.XmlElementName);
                        replacementBuilder.Append(">");
                        replacementBuilder.Append(textToReplace);
                        replacementBuilder.Append("</");
                        replacementBuilder.Append(_settings.XmlElementName);
                        replacementBuilder.Append(">");
                        replacementText = replacementBuilder.ToString();
                    }

                    // Replace the text
                    outputString.Remove(outputStartIndex, lengthToReplace);
                    outputString.Insert(outputStartIndex, replacementText);

                    // Update charIndexes to reflect the updated outputString.
                    charIndexes.RemoveRange(outputStartIndex, lengthToReplace);
                    charIndexes.InsertRange(outputStartIndex, 
                        Enumerable.Repeat<int>(-1, replacementText.Length));
                }

                // Generate the output file.
                string outputFileName = pathTags.Expand(_settings.OutputFileName);
                File.WriteAllText(outputFileName, outputString.ToString());

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31642",
                    "Unable to create redacted text.", ex);
            }
        }

        #endregion IFileProcessingTask Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="CreateRedactedTextTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI31643",
                    _COMPONENT_DESCRIPTION);

                // Allow the user to set the verification settings
                using (var dialog = new CreateRedactedTextSettingsDialog(_settings))
                {
                    bool result = dialog.ShowDialog() == DialogResult.OK;

                    // Store the result
                    if (result)
                    {
                        _settings = dialog.CreateRedactedTextSettings;
                        _dirty = true;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31644", "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

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

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="CreateRedactedTextTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="CreateRedactedTextTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new CreateRedactedTextTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31645",
                    "Failed to clone " + _COMPONENT_DESCRIPTION + " object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="CreateRedactedTextTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                CopyFrom((CreateRedactedTextTask)pObject);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31646",
                    "Failed to copy " + _COMPONENT_DESCRIPTION + " object.", ex);
            }
        }

        #endregion ICopyableObject Members

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
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the settings
                    _settings = CreateRedactedTextSettings.ReadFrom(reader);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31647",
                    "Unable to load verification task.", ex);
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
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
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
                throw ExtractException.CreateComVisible("ELI31648",
                    "Unable to save replaced indexed text settings.", ex);
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
        /// Copies the specified <see cref="CreateRedactedTextTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="CreateRedactedTextTask"/> from which to copy.</param>
        public void CopyFrom(CreateRedactedTextTask task)
        {
            _settings = task._settings;
        }

        #endregion Private Members
    }
}
