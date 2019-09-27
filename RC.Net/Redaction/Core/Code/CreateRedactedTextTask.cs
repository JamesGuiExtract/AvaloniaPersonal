using Extract.AttributeFinder;
using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Parsers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
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
        CreateRedactedTextSettings _settings = new CreateRedactedTextSettings();

        /// <summary>
        /// Loads attributes to be redacted from voa files.
        /// </summary>
        RedactionFileLoader _voaLoader;

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
                throw ex.AsExtract("ELI31634");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the settings for the task.
        /// </summary>
        [ComVisible(false)]
        public CreateRedactedTextSettings TaskSettings
        {
            get
            {
                return _settings;
            }
            set
            {
                try
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }

                    _settings = value;
                    _dirty = true;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI32416");
                }
            }
        }

        #endregion Properties

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
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI31639",
                    _COMPONENT_DESCRIPTION);

                // Create a voa file loader to load the attributes to be redacted.
                List<ConfidenceLevel> confidenceLevels = new List<ConfidenceLevel>();
                if (_settings.RedactAllTypes)
                {
                    // Create a ConfidenceLevel that queries for all attributes.
                    confidenceLevels.Add(
                        new ConfidenceLevel(
                            "*", "*", Color.Black, true, false, false, false, false, null));
                }
                else
                {
                    foreach (string dataType in _settings.DataTypes
                        .Where(type => _settings.IsTypeToRedact(type)))
                    {
                        // Create a ConfidenceLevel that query for each specified data type that is to
                        // be redacted.
                        confidenceLevels.Add(
                            new ConfidenceLevel(
                                dataType, dataType, Color.Black, true, false, false, false, false, null));
                    }
                }

                _voaLoader = new RedactionFileLoader(new ConfidenceLevelsCollection(confidenceLevels));
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

                bool isRTF = pFileRecord.Name.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase);

                // Load the redactions
                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFAMTM, pFileRecord.Name);
                string voaFile = pathTags.Expand(_settings.DataFile);

                if (!File.Exists(voaFile))
                {
                    ExtractException ee = new ExtractException("ELI32205", "VOA file not found.");
                    ee.AddDebugData("File name", voaFile, false);
                    throw ee;
                }

                _voaLoader.LoadFrom(voaFile, pFileRecord.Name);

                // Load the "spatial" string containing the text index data.
                SpatialString displayString = new SpatialString();
                string ussFileName = pFileRecord.Name + ".uss";
                if (File.Exists(ussFileName))
                {
                    displayString.LoadFrom(ussFileName, false);
                }
                else
                {
                    displayString.LoadFrom(pFileRecord.Name, false);
                }
                displayString.ReportMemoryUsage();

                // [FlexIDSCore:4598]
                // Throw an exception if the OCR data isn't text index data.
                LongRectangle bounds = displayString.GetOCRImageBounds();
                if (bounds.Top != 0 ||
                    bounds.Bottom != 2)
                {
                    throw new ExtractException("ELI32207",
                        "\"" + _COMPONENT_DESCRIPTION + "\" task can be used only on text files.");
                }

                // https://extract.atlassian.net/browse/ISSUE-12345
                // In order to avoid problems with assumptions about character encoding leading to
                // corruption of high-ANSI chars when converting to/from strings, CreateRedactions
                // has been modified to use byte arrays as input and output.
                byte[] sourceFileBytes = File.ReadAllBytes(pFileRecord.Name);

                IEnumerable<IRasterZone> redactionZones =
                    from sensitiveItem in _voaLoader.Items
                    where sensitiveItem.Attribute.Redacted
                    let value = sensitiveItem.Attribute.ComAttribute.Value
                    where value.HasSpatialInfo()
                    from rasterZone in value.GetOCRImageRasterZones().ToIEnumerable<IRasterZone>()
                    select rasterZone;

                byte[] redactedBytes = GetRedactedBytes(sourceFileBytes, displayString, redactionZones, bounds, isRTF);

                // Generate the output file.
                string outputFileName = pathTags.Expand(_settings.OutputFileName);

                // Create the directory if it's missing
                Directory.CreateDirectory(Path.GetDirectoryName(outputFileName));

                File.WriteAllBytes(outputFileName, redactedBytes);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31642",
                    "Unable to create redacted text.", ex);
            }
        }


        #endregion IFileProcessingTask Members

        #region Public Methods

        /// <summary>
        /// Redact input using the provided raster zones
        /// </summary>
        /// <param name="sourceText">The full source text as a byte array</param>
        /// <param name="displayString">The spatial info of the visible text (what a user would see in verification and what rules would operate on)</param>
        /// <param name="redactionZones">The <see cref="RasterZone"/>s that describe the redactions to be made</param>
        /// <param name="sourceIsRichText">Whether the source is a rich text file</param>
        /// <returns>A byte array where the bytes specified by the redaction zones have been replaced</returns>
        [CLSCompliant(false)]
        public byte[] RedactBytes(byte[] sourceText, SpatialString displayString, IEnumerable<IRasterZone> redactionZones, bool sourceIsRichText)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI48344",
                    _COMPONENT_DESCRIPTION);

                // [FlexIDSCore:4598]
                // Throw an exception if the OCR data isn't text index data.
                LongRectangle pageBounds = displayString.GetOCRImageBounds();
                ExtractException.Assert("ELI48345", "Unexpected page bounds for text-based SpatialString",
                    pageBounds.Top == 0 && pageBounds.Bottom == 2);

                return GetRedactedBytes(sourceText, displayString, redactionZones, pageBounds, sourceIsRichText);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48343");
            }
        }

        #endregion Public Methods

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
        /// Initializes an object from the <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> from which the object should be loaded.
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
        /// <para><b>Note:</b></para>
        /// This method requires that the <paramref name="redactionZones"/> do not overlap and
        /// are sorted.
        /// <para><b>Note2:</b></para>
        /// https://extract.atlassian.net/browse/ISSUE-12345
        /// In order to avoid problems with assumptions about character encoding leading to
        /// corruption of high-ANSI chars when converting to/from strings, CreateRedactions has
        /// been modified to use byte arrays as input and output.
        /// </summary>
        /// <param name="sourceFileBytes">A <see langword="byte"/> array to redact.</param>
        /// <param name="redactionZones">The character indexes to redact where Item1 of each
        /// <see cref="Tuple"/> is the starting index of each zone to redact and
        /// Item2 is the ending index of the zone.</param>
        /// <param name="isRTF">Whether the input is rich text</param>
        /// <returns>A redacted version of <see paramref="sourceFileBytes"/>.</returns>
        byte[] CreateRedactions(byte[] sourceFileBytes, List<Tuple<int, int>> redactionZones, bool isRTF)
        {
            // If input is rich text, get a string version of it in order to test the 'safety' of redactions
            // (whether a redaction could change the meaning of preceding codes)
            string sourceFileText = null;
            if (isRTF)
            {
                sourceFileText = Encoding.GetEncoding("windows-1252").GetString(sourceFileBytes);
            }

            // Set the initial capacity of the SB to the 110% of the source string length. This
            // will reduce the number of times the SB needs to increase its length
            List<byte> redactedOutput = new List<byte>(
                (int)Math.Round(sourceFileBytes.Length * 1.10));

            // Loop through each redactionZone to redact the text into redactedOutput.
            int nextSourceIndex = 0;
            foreach (Tuple<int, int> redactionZone in redactionZones)
            {
                int length = redactionZone.Item1 - nextSourceIndex;
                redactedOutput.AddRange(
                    sourceFileBytes
                    .Skip(nextSourceIndex)
                    .Take(length));

                nextSourceIndex += length;

                // Calculate the text to replace.
                int lengthToReplace = redactionZone.Item2 - redactionZone.Item1 + 1;
                nextSourceIndex += lengthToReplace;
                IEnumerable<byte> textToReplace =
                    sourceFileBytes
                    .Skip(redactionZone.Item1)
                    .Take(lengthToReplace);
                byte[] replacementText = null;

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

                                    replacementText = Enumerable.Repeat(
                                        (byte)_settings.ReplacementCharacter[0], replacementLength)
                                            .ToArray();
                                }
                            }
                            else
                            {
                                replacementText = textToReplace.ToArray();

                                for (int i = 0; i < replacementText.Length; i++)
                                {
                                    char c = (char)replacementText[i];

                                    if (char.IsLetterOrDigit(c))
                                    {
                                        replacementText[i] = (byte)_settings.ReplacementCharacter[0];
                                    }
                                }
                            }
                        }
                        break;

                    case RedactionMethod.ReplaceText:
                        {
                            replacementText =
                                Encoding.Default.GetBytes(_settings.ReplacementText);
                        }
                        break;

                    case RedactionMethod.SurroundWithXml:
                        {
                            List<byte> replacementTextBuilder = new List<byte>(
                                textToReplace.Count() + (_settings.XmlElementName.Length * 2) + 5);

                            replacementTextBuilder.Add((byte)'<');
                            replacementTextBuilder.AddRange(
                                Encoding.Default.GetBytes(_settings.XmlElementName));
                            replacementTextBuilder.Add((byte)'>');
                            replacementTextBuilder.AddRange(textToReplace);
                            replacementTextBuilder.AddRange(Encoding.Default.GetBytes("</"));
                            replacementTextBuilder.AddRange(
                                Encoding.Default.GetBytes(_settings.XmlElementName));
                            replacementTextBuilder.Add((byte)'>');

                            replacementText = replacementTextBuilder.ToArray();
                        }
                        break;
                }

                ExtractException.Assert("ELI37176", "Internal logic error.", replacementText != null);

                // Insert a space if there is danger of breaking an RTF code otherwise
                if (isRTF && RichTextUtilities.CouldRichTextRedactionChangePrecedingCode(sourceFileText, redactionZone.Item1, (char)replacementText[0]))
                {
                    redactedOutput.Add((byte)' ');
                }

                // Replace the text
                redactedOutput.AddRange(replacementText);
            }

            if (nextSourceIndex < sourceFileBytes.Length)
            {
                redactedOutput.AddRange(sourceFileBytes.Skip(nextSourceIndex));
            }

            return redactedOutput.ToArray();
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

        /// <summary>
        /// Sort function for the redaction zones
        /// </summary>
        /// <param name="left">The left zone.</param>
        /// <param name="right">The right zone.</param>
        /// <returns>The result of comparing the left and right zones.</returns>
        static int SortZones(Tuple<int, int> left, Tuple<int, int> right)
        {
            if (left == null)
            {
                if (right == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (right == null)
                {
                    return 1;
                }
                else
                {
                    return left.Item1.CompareTo(right.Item1);
                }
            }
        }

        // Redact input using the provided raster zones
        byte[] GetRedactedBytes(byte[] sourceFileBytes, SpatialString displayString, IEnumerable<IRasterZone> redactionZones, LongRectangle pageBounds, bool isRTF)
        {
            // Build up a list of redaction zones where each redaction zone may encompass 2 or
            // more overlapping redactions. Item1 of each <see cref="Tuple(int, int)"/> in this
            // list is the starting index of the zone and Item2 is the ending index of the zone.
            List<Tuple<int, int>> redactionSpans = new List<Tuple<int, int>>();

            // Loop through each raster zone in the attribute.
            foreach (RasterZone rasterZone in redactionZones)
            {
                // Text is "indexed" by storing the index of each character as the "left"
                // coordinate in the attribute's spatial string.
                ILongRectangle boundingRect =
                    rasterZone.GetRectangularBounds(pageBounds);
                int startIndex = boundingRect.Left;
                int endIndex = boundingRect.Right;

                // Combine this range of indexes with indexes previously slated for redaction.
                MergeWithExistingRedactionZones(redactionSpans, startIndex, endIndex);
            }

            var (redactableIndexes, rightIndexIsExclusive) = GetRedactableIndexes(displayString);

            redactionSpans = redactionSpans
                .SelectMany(span => SplitSpans(span, redactableIndexes, rightIndexIsExclusive))
                .ToList();

            // Sort the redaction zones by starting index
            redactionSpans.Sort(SortZones);

            // Create a redacted version of sourceFileBytes using the calculated redaction spans.
            byte[] redactedBytes = CreateRedactions(sourceFileBytes, redactionSpans, isRTF);

            // Validate RTF format and throw an exception if the result is not valid
            if (isRTF)
            {
                try
                {
                    // Validate RTF groups with RichTextExtractor
                    string rtfString = Encoding.GetEncoding("windows-1252").GetString(redactedBytes);
                    RichTextExtractor.GetTextPositions(rtfString, displayString.SourceDocName, throwParseExceptions: true);
                }
                catch (Exception ex)
                {
                    throw new ExtractException("ELI48335", "Invalid rich text after redaction", ex);
                }
            }

            return redactedBytes;
        }

        // Split zones into pieces so that they don't cover up control words or other text that wasn't part of the text shown to users/rules
        static IEnumerable<Tuple<int, int>> SplitSpans(Tuple<int, int> span, HashSet<int> redactableIndexes, bool rightIndexIsExclusive)
        {
            int start = span.Item1;
            int end = span.Item2;
            if (rightIndexIsExclusive)
            {
                end--;
            }

            // Trim start
            while (start <= end && !redactableIndexes.Contains(start))
            {
                start++;
            }

            for (int splitEnd = start + 1; splitEnd <= end; splitEnd++)
            {
                if (!redactableIndexes.Contains(splitEnd))
                {
                    // Found a gap so output the first span if it is valid
                    if (redactableIndexes.Contains(start))
                    {
                        yield return new Tuple<int, int>(start, splitEnd - 1);
                    }

                    // Skip the gap
                    start = splitEnd;
                    while (start <= end && !redactableIndexes.Contains(start))
                    {
                        start++;
                    }

                    // Setup next iteration
                    splitEnd = start - 1;
                }
            }

            // Trim end
            while (end >= start && !redactableIndexes.Contains(end))
            {
                end--;
            }

            // Output trimmed span if it is valid
            if (redactableIndexes.Contains(start))
            {
                yield return new Tuple<int, int>(start, end);
            }
        }

        // Get collection of indexes into the full source text that are also in the display text (text shown to users in verify redactions task or to rules)
        static (HashSet<int> redactableIndexes, bool rightIndexIsExclusive) GetRedactableIndexes(SpatialString displayString)
        {
            bool rightIndexIsExclusive = true;
            HashSet<int> redactableIndexes = new HashSet<int>();
            int len = displayString.Size;
            for (int i = 0; i < len; i++)
            {
                var letter = displayString.GetOCRImageLetter(i);

                // Don't redact newlines because that would make the output ugly
                char letterChar = (char)letter.Guess1;
                if (letterChar == '\r' || letterChar == '\n')
                {
                    continue;
                }

                var left = letter.Left;
                var right = letter.Right;
                // Adjust right index to be exclusive.
                // (Right index of text-based letters are exclusive in 11.3.1 but used to be inclusive)
                if (left == right)
                {
                    right++;
                    rightIndexIsExclusive = false;
                }
                // Allow for multi-byte escape sequences for one display char
                for (int j = letter.Left; j < right; j++)
                {
                    redactableIndexes.Add(j);
                }
            }

            return (redactableIndexes, rightIndexIsExclusive);
        }

        #endregion Private Members
    }
}
