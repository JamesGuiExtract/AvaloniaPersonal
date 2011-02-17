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
        /// Version 2:
        /// bool ReplaceCharacters -> RedactionMethod RedactionMethod
        /// string ReplacementValue -> string ReplacementCharacter
        /// Added: bool AddCharactersToRedaction
        /// Added: int MaxNumberAddedCharacters
        /// Added: string ReplacementText
        /// </summary>
        internal const int _CURRENT_VERSION = 2;

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

        /// <summary>
        /// Used to generate a random number of characters for <see cref="GetNumberCharactersToAdd"/>.
        /// </summary>
        Random _randomNumberGenerator;

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
                if (_settings.RedactionMethod == RedactionMethod.ReplaceCharacters &&
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

                // Load the redactions
                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFileRecord.Name, pFAMTM.FPSFileDir);
                string voaFile = pathTags.Expand(_settings.DataFile);
                _voaLoader.LoadFrom(voaFile, pFileRecord.Name);

                // Build up a list of redaction zones where each redaction zone may encompass 2 or
                // more overlapping redactions. Item1 of each <see cref="Tuple(int, int)"/> in this
                // list is the starting index of the zone and Item2 is the ending index of the zone.
                List<Tuple<int, int>> redactionZones = new List<Tuple<int, int>>();

                // Loop through each attribute to redact
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

                    // Combine this range of indexes with indexes previously slated for redaction.
                    MergeWithExistingRedactionZones(redactionZones, startIndex, endIndex);
                }

                // Create a redacted version of source.String using the calculated redaction zones.
                string redactedString = CreateRedactions(source.String, redactionZones);

                // Generate the output file.
                string outputFileName = pathTags.Expand(_settings.OutputFileName);
                File.WriteAllText(outputFileName, redactedString);

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

        /// <summary>
        /// Given and existing list of <see paramref="redactionZones"/> and a new span of indexes to
        /// redact, adds the new span of indexes by either creating a new image zone or merging it
        /// with one or more existing redaction zones.
        /// </summary>
        /// <param name="redactionZones">A list of existing spans of indexes to redact where Item1
        /// of each <see cref="Tuple"/> is the starting index of the zone and Item2 is the
        /// ending index of the zone.</param>
        /// <param name="startIndex">The index of the first character in the span to redact.</param>
        /// <param name="endIndex">The index of the last character in the span to redact.</param>
        static void MergeWithExistingRedactionZones(List<Tuple<int, int>> redactionZones,
            int startIndex, int endIndex)
        {
            // Create a new Tuple to represent the new redaction.
            Tuple<int, int> newRedactionZone = new Tuple<int, int>(startIndex, endIndex);

            // Loop though each existing redaction zone to compare it to the new zone.
            int firstOverlappingIndex = -1;
            for (int i = 0; i < redactionZones.Count; i++)
            {
                Tuple<int, int> redactionZone = redactionZones[i];

                // If the zone exactly matches an existing zone, nothing needs to be done.
                if (redactionZone.Equals(newRedactionZone))
                {
                    return;
                }
                // If the zone overlaps an existing zone, merge it.
                else if ((newRedactionZone.Item1 > redactionZone.Item2) ==
                         (newRedactionZone.Item2 < redactionZone.Item1))
                {
                    // Create the merged span.
                    newRedactionZone = new Tuple<int, int>(
                        (int)Math.Min(newRedactionZone.Item1, redactionZone.Item1),
                        (int)Math.Max(newRedactionZone.Item2, redactionZone.Item2));
                    
                    // If the zone has not yet been merged, modify the existing zone and make note
                    // of which one was modified.
                    if (firstOverlappingIndex == -1)
                    {
                        redactionZones[i] = newRedactionZone;
                        firstOverlappingIndex = i;
                    }
                    // If the zone was already merged, the most recent overlapping zone is no
                    // longer needed.
                    else
                    {
                        redactionZones[firstOverlappingIndex] = newRedactionZone;
                        redactionZones.RemoveAt(i);
                        i--;
                    }
                }
            }

            // If the new zone was not merged with an existing zone, add the new zone to
            // redactionZones.
            if (firstOverlappingIndex == -1)
            {
                redactionZones.Add(newRedactionZone);
            }
        }

        /// <summary>
        /// Creates a copy of <see paramref="sourceString"/> where the text of each specified
        /// <see paramref="redactionZones"/> has been redacted.
        /// </summary>
        /// <param name="sourceString">The <see cref="string"/> to redact.</param>
        /// <param name="redactionZones">The character indexes to redact where Item1 of each
        /// <see cref="Tuple"/> is the starting index of each zone to redact and
        /// Item2 is the ending index of the zone.</param>
        /// <returns>A redacted version of <see paramref="sourceString"/>.</returns>
        string CreateRedactions(string sourceString, List<Tuple<int, int>> redactionZones)
        {
            StringBuilder outputString = new StringBuilder(sourceString);

            // Initialize a list to map charater indexes in outputString back to the corresponding
            // index of sourceString. (The indexes of charIndexes are the character indexes in
            // outputString while the values of charIndexes are the character indexes in
            // sourceString).
            List<int> charIndexes = new List<int>(Enumerable.Range(0, sourceString.Length));

            // Loop through each redactionZone to redact the text in outputString.
            foreach (Tuple<int, int> redactionZone in redactionZones)
            {
                int outputIndex = charIndexes.FindIndex((index) => redactionZone.Item1 == index);
                ExtractException.Assert("ELI31693", "Missing text file index.", outputIndex != -1);

                // Calculate the text to replace.
                int lengthToReplace = redactionZone.Item2 - redactionZone.Item1 + 1;
                string textToReplace = sourceString.Substring(redactionZone.Item1, lengthToReplace);
                string replacementText = string.Empty;

                // Calculate the text replacement text.
                switch (_settings.RedactionMethod)
                {
                    case RedactionMethod.ReplaceCharacters:
                        {
                            if (_settings.CharactersToReplace == CharacterClass.All)
                            {
                                if (!string.IsNullOrEmpty(_settings.ReplacementCharacter))
                                {
                                    int replacementLength = lengthToReplace;
                                    if (_settings.AddCharactersToRedaction)
                                    {
                                        replacementLength += GetNumberCharactersToAdd();
                                    }

                                    replacementText = new string(
                                        _settings.ReplacementCharacter[0], replacementLength);
                                }
                            }
                            else
                            {
                                replacementText = _regex.Replace(
                                    textToReplace, _settings.ReplacementCharacter ?? "");
                            }
                        }
                        break;

                    case RedactionMethod.ReplaceText:
                        {
                            replacementText = _settings.ReplacementText;
                        }
                        break;

                    case RedactionMethod.SurroundWithXml:
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
                        break;
                }

                // Replace the text
                outputString.Remove(outputIndex, lengthToReplace);
                outputString.Insert(outputIndex, replacementText);

                // Update charIndexes to reflect the updated outputString.
                charIndexes.RemoveRange(outputIndex, lengthToReplace);
                charIndexes.InsertRange(outputIndex,
                    Enumerable.Repeat<int>(-1, replacementText.Length));
            }

            return outputString.ToString();
        }

        /// <summary>
        /// Gets the number characters to add to the next redacted item when the
        /// <see cref="CreateRedactedTextSettings.AddCharactersToRedaction"/> option is
        /// <see langword="true"/>.
        /// </summary>
        /// <returns>The number characters to add to the next redacted item.</returns>
        int GetNumberCharactersToAdd()
        {
            if (_randomNumberGenerator == null)
            {
                _randomNumberGenerator = new Random((int)DateTime.Now.Ticks);
            }

            return _randomNumberGenerator.Next(_settings.MaxNumberAddedCharacters + 1);
        }

        #endregion Private Members
    }
}
