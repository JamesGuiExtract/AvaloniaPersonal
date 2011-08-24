using Extract.AttributeFinder;
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
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.Redaction
{
    /// <summary>
    /// An IFileProcessingTask which merges redaction data from two different VOA files.
    /// </summary>
    [ComVisible(true)]
    [Guid("54D4B5BC-32EC-470C-8DBE-E30FFBF370E9")]
    [ProgId("Extract.Redaction.VOAFileMergeTask")]
    public class VOAFileMergeTask : IFileProcessingTask, IConfigurableObject, IMustBeConfiguredObject,
        IAccessRequired, ICategorizedComponent, ICopyableObject, ILicensedComponent, IPersistStream
    {
        #region Constants

        /// <summary>
        /// The COM object name.
        /// </summary>
        internal const string _COMPONENT_DESCRIPTION = "Redaction: Merge ID Shield data files";

        /// <summary>
        /// Current task version.
        /// Version 2: Added UseMutualOverlapPercent
        /// </summary>
        internal const int _CURRENT_VERSION = 2;

        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to
        /// <see cref="VOAFileMergeTask"/> since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The ID Shield ini settings.
        /// </summary>
        InitializationSettings _idShieldSettings = new InitializationSettings();

        /// <summary>
        /// The settings for this object.
        /// </summary>
        VOAFileMergeTaskSettings _settings;

        /// <summary>
        /// Used to compare and merge the files.
        /// </summary>
        SpatialAttributeMergeUtils _attributeMerger;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileMergeTask"/> class.
        /// </summary>
        public VOAFileMergeTask()
        {
            _settings = new VOAFileMergeTaskSettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileMergeTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="VOAFileMergeTask"/> from which
        /// settings should be copied.</param>
        public VOAFileMergeTask(VOAFileMergeTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32038");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the settings for the task.
        /// </summary>
        [ComVisible(false)]
        public VOAFileMergeTaskSettings TaskSettings
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
                    throw ex.AsExtract("ELI32431");
                }
            }
        }

        #endregion Properties

        #region IFileProcessingTask Members

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI32041",
                    _COMPONENT_DESCRIPTION);

                _attributeMerger = InitializeAttributeMerger(_idShieldSettings,
                    _settings.UseMutualOverlap, _settings.OverlapThreshold);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32042",
                    "Unable to initialize 'Merge ID Shield data files' task.", ex);
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI32043",
                    _COMPONENT_DESCRIPTION);

                CompareMergeFiles(_settings, _idShieldSettings, _attributeMerger, pFileRecord.Name,
                    pFAMTM);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32044",
                    "Failed to merge ID Shield data files.", ex);
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI32045",
                    _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32046",
                    "Unable to cancel 'Merge ID Shield data files' task.", ex);
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI32047",
                    _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32048",
                    "Unable to close 'Merge ID Shield data files' task.", ex);
            }
        }

        #endregion IFileProcessingTask Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid
        /// <see cref="VOAFileMergeTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI32049",
                    _COMPONENT_DESCRIPTION);

                // Allow the user to set the verification settings
                using (var dialog = new VOAFileMergeTaskSettingsDialog(_settings))
                {
                    bool result = dialog.ShowDialog() == DialogResult.OK;

                    // Store the result
                    if (result)
                    {
                        _settings = dialog.VOAFileMergeTaskSettings;
                        _dirty = true;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32050", "Error running configuration.");
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
                return (_settings != null &&
                        !string.IsNullOrWhiteSpace(_settings.DataFile1) &&
                        !string.IsNullOrWhiteSpace(_settings.DataFile2) &&
                        !string.IsNullOrWhiteSpace(_settings.OutputFile));
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32131",
                    "Failed to check " + _COMPONENT_DESCRIPTION + " configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

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
        /// Creates a copy of the <see cref="VOAFileMergeTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="VOAFileMergeTask"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new VOAFileMergeTask(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32051",
                    "Failed to clone " + _COMPONENT_DESCRIPTION + " object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="VOAFileMergeTask"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                CopyFrom((VOAFileMergeTask)pObject);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32052",
                    "Failed to copy " + _COMPONENT_DESCRIPTION + " object.");
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
                    _settings = VOAFileMergeTaskSettings.ReadFrom(reader);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32053", "Unable to load verification task.");
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
                throw ex.CreateComVisible("ELI32054",
                    "Unable to save replaced indexed text settings.");
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

        #region Internal Members

        /// <summary>
        /// Compares and, depending on the <see paramref="settings"/>, merges the VOA files
        /// specified in <see paramref="settings"/>.
        /// </summary>
        /// <param name="settings">The <see cref="VOAFileMergeTaskSettings"/> that describe how the
        /// source files should be compared/merged. This may be a
        /// <see cref="VOAFileCompareConditionSettings"/> instace if the purpose of the call is a
        /// comparsion of the source files.</param>
        /// <param name="idShieldSettings">The ID shield ini settings to use.</param>
        /// <param name="attributeMerger">The <see cref="SpatialAttributeMergeUtils"/> to use to
        /// compare and merge the source files.</param>
        /// <param name="sourceDocName">The source document name.</param>
        /// <param name="pFAMTM">A <see cref="FAMTagManager"/> for expanding tags.</param>
        static internal bool CompareMergeFiles(VOAFileMergeTaskSettings settings,
            InitializationSettings idShieldSettings, SpatialAttributeMergeUtils attributeMerger,
            string sourceDocName, FAMTagManager pFAMTM)
        {
            string dataFile1 = null;
            string dataFile2 = null;
            string outputFile = null;

            try
            {
                // Start the timer
                IntervalTimer timer = IntervalTimer.StartNew();

                // Load the redactions
                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(sourceDocName, pFAMTM.FPSFileDir);
                dataFile1 = pathTags.Expand(settings.DataFile1);
                dataFile2 = pathTags.Expand(settings.DataFile2);

                ExtractException.Assert("ELI32280", "Source ID Shield data file does not exist.",
                    File.Exists(dataFile1) && File.Exists(dataFile2));

                // Create a RedactionFileLoader to load redactions from the data files.
                RedactionFileLoader voaLoader =
                    new RedactionFileLoader(
                        new ConfidenceLevelsCollection(idShieldSettings.ConfidenceLevels));

                // Load spatial attributes from dataFile1
                voaLoader.LoadFrom(dataFile1, sourceDocName);
                IEnumerable<ComAttribute> set1OriginalAttributes = voaLoader.Items
                    .Select(item => item.Attribute.ComAttribute)
                    .Union(voaLoader.InsensitiveAttributes
                        .Where(attribute => attribute.Value.HasSpatialInfo()));
                IUnknownVector attributeVector1 = set1OriginalAttributes.ToIUnknownVector();

                // Find attributes from dataFile1 that have been turned off and archived under the
                // revisions attribute.
                HashSet<ComAttribute> turnedOffAttributes = new HashSet<ComAttribute>(
                    voaLoader.Items
                        .Where(item => !item.Attribute.Redacted)
                        .Select(item => item.Attribute.ComAttribute));

                IEnumerable<ComAttribute> set1NonSpatial = voaLoader.InsensitiveAttributes
                    .Where(attribute => !attribute.Value.HasSpatialInfo());

                long nextId = voaLoader.NextId;

                // Load spatial attributes from dataFile2
                voaLoader.LoadFrom(dataFile2, sourceDocName);
                IEnumerable<ComAttribute> set2OriginalAttributes = voaLoader.Items
                    .Select(item => item.Attribute.ComAttribute)
                    .Union(voaLoader.InsensitiveAttributes
                        .Where(attribute => attribute.Value.HasSpatialInfo()));
                IUnknownVector attributeVector2 = set2OriginalAttributes.ToIUnknownVector();

                // Find attributes from dataFile2 that have been turned off and archived under the
                // revisions attribute.
                foreach (ComAttribute turnedOffAttribute in voaLoader.Items
                    .Where(item => !item.Attribute.Redacted)
                    .Select(item => item.Attribute.ComAttribute))
                {
                    turnedOffAttributes.Add(turnedOffAttribute);
                }

                IEnumerable<ComAttribute> set2NonSpatial = voaLoader.InsensitiveAttributes
                    .Where(attribute => !attribute.Value.HasSpatialInfo());

                // The IDs of the output items will start with the next higher number than any
                // existing item id in either of the two source files.
                nextId = Math.Max(nextId, voaLoader.NextId);

                // Load spatial info (used to normalize the attribute raster zones into the same
                // coordinate system for comparison).
                SpatialString docText = new SpatialString();
                docText.LoadFrom(sourceDocName + ".uss", false);

                // Use a unique value that allows attributes merged this session to be identified.
                string mergedValue =
                    string.Format(CultureInfo.CurrentCulture, "_{0}_", DateTime.Now.Ticks);
                attributeMerger.SpecifiedValue = mergedValue;

                bool attributeSetsMatch =
                    attributeMerger.CompareAttributeSets(attributeVector1, attributeVector2, docText);

                // If the provided settings object is an VOAFileCompareConditionSettings instance,
                // whether we merge will depend on the settings. Otherwise this is a merge task and,
                // thus, we will merge.
                VOAFileCompareConditionSettings compareSettings =
                    settings as VOAFileCompareConditionSettings;

                bool conditionResult = false;
                bool mergeFiles = true;
                if (compareSettings != null)
                {
                    conditionResult = (attributeSetsMatch == compareSettings.ConditionMetIfMatching);
                    mergeFiles = compareSettings.CreateOutput &&
                        (conditionResult || !compareSettings.CreateOutputOnlyOnCondition);
                }

                // Create the output file if applicable.
                if (mergeFiles)
                {
                    AttributeCreator attributeCreator = new AttributeCreator(sourceDocName);
                    outputFile = pathTags.Expand(settings.OutputFile);

                    // Apply the merges to both sets. The output will be the merged redactions plus
                    // any un-merged redactions from either set.
                    attributeMerger.ApplyMerges(attributeVector1);
                    attributeMerger.ApplyMerges(attributeVector2);

                    // Convert the resulting vectors to IEnumerables for more efficient iteration.
                    IEnumerable<ComAttribute> set1Attributes =
                        attributeVector1.ToIEnumerable<ComAttribute>();
                    IEnumerable<ComAttribute> set2Attributes =
                        attributeVector2.ToIEnumerable<ComAttribute>();

                    // Create collections to store the final output as well as attributes to map each
                    // source attribute to an attribute in the output (merged or not).
                    IUnknownVector outputAttributes = new IUnknownVector();
                    Dictionary<string, List<ComAttribute>> set1Mappings =
                        new Dictionary<string, List<ComAttribute>>();
                    Dictionary<string, List<ComAttribute>> set2Mappings =
                        new Dictionary<string, List<ComAttribute>>();

                    // The IDS of items that should be turned off in the output file.
                    HashSet<string> turnedOffIDs = new HashSet<string>();

                    // Find all sensitive items from either file that haven't been merged and add them
                    // to the output attributes while adding a mapping entry that ties the item from the
                    // original file to the corresponding item in the output file.
                    AddUnmergedAttributes(attributeCreator, set1Attributes, mergedValue, set1Mappings,
                        turnedOffAttributes, turnedOffIDs, outputAttributes, ref nextId);
                    AddUnmergedAttributes(attributeCreator, set2Attributes, mergedValue, set2Mappings,
                        turnedOffAttributes, turnedOffIDs, outputAttributes, ref nextId);

                    // Find all sensitive items have been merged and add them to the output attributes
                    // while adding a mapping entries that tie original items to the merged item.
                    AddMergedAttributes(attributeCreator, set1Attributes, set1OriginalAttributes,
                        mergedValue, set1Mappings, set2Mappings, turnedOffAttributes, turnedOffIDs,
                        outputAttributes, ref nextId);

                    // Replace all attributes with the unique value with "Merged" in the final output.
                    foreach (ComAttribute mergedAttribute in attributeVector1
                        .ToIEnumerable<ComAttribute>()
                        .Where(attribute => attribute.Value.String == mergedValue))
                    {
                        mergedAttribute.Value.ReplaceAndDowngradeToHybrid("Merged");
                    }

                    // Merge all non-spatial attributes.
                    List<ComAttribute> mergedNonSpatialAttributes = MergeNonSpatialAttributes(
                        new List<ComAttribute>(set1NonSpatial), new List<ComAttribute>(set2NonSpatial));
                    outputAttributes.Append(mergedNonSpatialAttributes.ToIUnknownVector<ComAttribute>());

                    outputAttributes.SaveTo(outputFile, false);

                    // Load the output using the RedactionFileLoader in order to add the metadata for
                    // this session.
                    voaLoader.LoadFrom(outputFile, sourceDocName);

                    // Turn off any items that need to be turned off.
                    foreach (SensitiveItem item in voaLoader.Items)
                    {
                        string id = item.Attribute.ComAttribute.SubAttributes
                            .ToIEnumerable<ComAttribute>()
                            .Where(subattribute => subattribute.Name == Constants.IDAndRevisionMetadata)
                            .Single().Value.String;

                        if (turnedOffIDs.Contains(id))
                        {
                            item.Attribute.Redacted = false;
                        }
                    }

                    TimeInterval interval = timer.Stop();

                    string sessionName = (compareSettings == null) 
                        ? Constants.VOAFileMergeSessionMetaDataName
                        : Constants.VOAFileCompareSessionMetaDataName;

                    voaLoader.SaveVOAFileMergeSession(sessionName, outputFile, dataFile1, dataFile2,
                        set1Mappings, set2Mappings, interval, settings);
                }

                return conditionResult;
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI32270", ex);
                ee.AddDebugData("Data File 1", dataFile1, false);
                ee.AddDebugData("Data File 2", dataFile2, false);
                ee.AddDebugData("Output File", outputFile, false);
                throw ee;
            }
        }

        #endregion Internal Members

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
        /// Copies the specified <see cref="VOAFileMergeTask"/> instance into
        /// this one.
        /// </summary>
        /// <param name="task">The <see cref="VOAFileMergeTask"/> from which to
        /// copy.</param>
        public void CopyFrom(VOAFileMergeTask task)
        {
            try
            {
                if (task._settings == null)
                {
                    _settings = null;
                }
                else
                {
                    _settings = new VOAFileMergeTaskSettings(task._settings);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32137");
            }
        }

        /// <summary>
        /// Initializes the attribute merger.
        /// </summary>
        /// <param name="idShieldSettings">The ID shield ini settings to use.</param>
        /// <param name="useMutualOverlap"><see langword="true"/> if
        /// <see paramref="overlapThreshold"/> must be met when comparing redaction A to B as well
        /// as B to A. <see langword="false"/> if <see paramref="overlapThreshold"/> needs to be met
        /// in only one of the two comparisons (such as a small redaction contained in a larger one.
        /// </param>
        /// <param name="overlapThreshold">The percentage of mutual overlap required to consider
        /// two redactions as equivalent.</param>
        internal static SpatialAttributeMergeUtils InitializeAttributeMerger(
            InitializationSettings idShieldSettings, bool useMutualOverlap, double overlapThreshold)
        {
            try
            {
                SpatialAttributeMergeUtils attributeMerger = new SpatialAttributeMergeUtils();

                attributeMerger.OverlapPercent = overlapThreshold;
                attributeMerger.UseMutualOverlap = useMutualOverlap;
                attributeMerger.NameMergeMode = EFieldMergeMode.kPreserveField;
                attributeMerger.TypeMergeMode = EFieldMergeMode.kCombineField;
                attributeMerger.PreserveAsSubAttributes = true;
                attributeMerger.CreateMergedRegion = false;

                // Add the standard confidence levels to the name merge priority.
                VariantVector nameMergePriority = new VariantVector();
                nameMergePriority.PushBack("Manual");
                nameMergePriority.PushBack("HCData");
                nameMergePriority.PushBack("MCData");
                nameMergePriority.PushBack("LCData");
                nameMergePriority.PushBack("Clues");

                // And any non-standard confidence level query from the ini file.
                foreach (string name in idShieldSettings.ConfidenceLevels
                    .Select(confidenceLevel => confidenceLevel.Query)
                    .Where(name => nameMergePriority.Find(name) == -1))
                {
                    nameMergePriority.PushBack(name);
                }

                attributeMerger.NameMergePriority = nameMergePriority;

                // [FlexIDSCore:4626]
                // Do not allow clues to merge with any attribute that is not also a clue.
                VariantVector mergeExclusionQueries = new VariantVector();
                mergeExclusionQueries.PushBack("Clues");

                attributeMerger.MergeExclusionQueries = mergeExclusionQueries;

                return attributeMerger;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32277");
            }
        }

        /// <summary>
        /// Adds all unmerged <see cref="ComAttribute"/>s from <see paramref="sourceAttributes"/> to
        /// <see paramref="outputAttributes"/> while adding any relevant mapping data to
        /// <see paramref="attributeMappings"/>.
        /// </summary>
        /// <param name="attributeCreator">The <see cref="AttributeCreator"/> to use when creating
        /// new <see cref="ComAttribute"/>s (for metadata).</param>
        /// <param name="sourceAttributes">The enumerable of <see cref="ComAttribute"/>s from which
        /// the unmerged results should be extracted.</param>
        /// <param name="mergedValue">The value by which merged attributes can be identified.</param>
        /// <param name="attributeMappings">The keys of this dictionary are the original IDs of each
        /// attribute from <see paramref="sourceAttributes"/> while the values are a list of
        /// <see cref="ComAttribute"/>s that associate the attribute with the ID of an attribute in
        /// <see paramref="outputAttributes"/>.</param>
        /// <param name="turnedOffAttributes">A <see cref="HashSet{T}"/> of
        /// <see cref="ComAttribute"/>s that were turned off in the source files.</param>
        /// <param name="turnedOffIDs">A <see cref="HashSet{T}"/> of attribute IDs that should be
        /// turned off in the output.</param>
        /// <param name="outputAttributes">The <see cref="IUnknownVector"/> of
        /// <see cref="ComAttribute"/>s to which the unmerged attributes should be added.</param>
        /// <param name="nextId">The id to be assigned to the next <see cref="ComAttribute"/>
        /// created.</param>
        static void AddUnmergedAttributes(AttributeCreator attributeCreator,
            IEnumerable<ComAttribute> sourceAttributes, string mergedValue,
            Dictionary<string, List<ComAttribute>> attributeMappings,
            HashSet<ComAttribute> turnedOffAttributes, HashSet<string> turnedOffIDs,
            IUnknownVector outputAttributes, ref long nextId)
        {
            foreach (ComAttribute sourceAttribute in sourceAttributes
                .Where(attribute => attribute.Value.String != mergedValue))
            {
                outputAttributes.PushBack(sourceAttribute);

                ComAttribute idAndRevisionAttribute = sourceAttribute.SubAttributes
                    .ToIEnumerable<ComAttribute>()
                    .Where(attribute => attribute.Name == Constants.IDAndRevisionMetadata)
                    .Single();

                string oldID = idAndRevisionAttribute.Value.String;
                string newId = nextId++.ToString(CultureInfo.InvariantCulture);
                ComAttribute mappingAttribute =
                    attributeCreator.Create(Constants.ImportedToIDMetadata, newId);

                idAndRevisionAttribute.Value.ReplaceAndDowngradeToNonSpatial(newId);
                idAndRevisionAttribute.Type = "_1";

                List<ComAttribute> mappings;
                if (!attributeMappings.TryGetValue(oldID, out mappings))
                {
                    mappings = new List<ComAttribute>();
                    attributeMappings[oldID] = mappings;
                }
                mappings.Add(mappingAttribute);

                if (turnedOffAttributes.Contains(sourceAttribute))
                {
                    turnedOffIDs.Add(newId);
                }
            }
        }

        /// <summary>
        /// Adds all merged <see cref="ComAttribute"/>s from <see paramref="set1Attributes"/> to
        /// <see paramref="outputAttributes"/> while adding any relevant mapping data to
        /// <see paramref="set1AttributeMappings"/> and <see paramref="set2AttributeMappings"/>.
        /// </summary>
        /// <param name="attributeCreator">The <see cref="AttributeCreator"/> to use when creating
        /// new <see cref="ComAttribute"/>s (for metadata).</param>
        /// <param name="set1Attributes">The enumerable of <see cref="ComAttribute"/>s from which
        /// the merged results should be extracted.</param>
        /// <param name="set1OriginalAttributes">The enumerable of <see cref="ComAttribute"/>s
        /// representing the original contents of set 1. Used to check whether a contributing
        /// attribute to a merged result is from set 1 or set 2.</param>
        /// <param name="mergedValue">The value by which merged attributes can be identified.</param>
        /// <param name="set1AttributeMappings">The keys of this dictionary are the original IDs of
        /// each attribute from <see paramref="set1Attributes"/> while the values are a list of
        /// <see cref="ComAttribute"/>s that associate the attribute with the ID of an attribute in
        /// <see paramref="outputAttributes"/>.</param>
        /// <param name="set2AttributeMappings">Like <see paramref="set2AttributeMappings"/>, but
        /// for the attributes from set 2.</param>
        /// <param name="turnedOffAttributes">A <see cref="HashSet{T}"/> of
        /// <see cref="ComAttribute"/>s that were turned off in the source files.</param>
        /// <param name="turnedOffIDs">A <see cref="HashSet{T}"/> of attribute IDs that should be
        /// turned off in the output.</param>
        /// <param name="outputAttributes">The <see cref="IUnknownVector"/> of
        /// <see cref="ComAttribute"/>s to which the unmerged attributes should be added.</param>
        /// <param name="nextId">The id to be assigned to the next <see cref="ComAttribute"/>
        /// created.</param>
        static void AddMergedAttributes(AttributeCreator attributeCreator,
            IEnumerable<ComAttribute> set1Attributes,
            IEnumerable<ComAttribute> set1OriginalAttributes, string mergedValue,
            Dictionary<string, List<ComAttribute>> set1AttributeMappings,
            Dictionary<string, List<ComAttribute>> set2AttributeMappings,
            HashSet<ComAttribute> turnedOffAttributes, HashSet<string> turnedOffIDs,
            IUnknownVector outputAttributes, ref long nextId)
        {
            foreach (ComAttribute mergedAttribute in set1Attributes
                .Where(attribute => attribute.Value.String == mergedValue))
            {
                string newId = nextId++.ToString(CultureInfo.InvariantCulture);
                bool turnedOff = true;

                foreach (ComAttribute sourceAttribute in mergedAttribute.SubAttributes
                    .ToIEnumerable<ComAttribute>())
                {
                    ComAttribute oldIdRevision = sourceAttribute.SubAttributes
                        .ToIEnumerable<ComAttribute>()
                        .Where(attribute => attribute.Name == Constants.IDAndRevisionMetadata)
                        .Single();

                    string oldID = oldIdRevision.Value.String;
                    ComAttribute mappingAttribute = attributeCreator.Create(Constants.MergedToIDMetadata, newId);

                    Dictionary<string, List<ComAttribute>> sourceMappings =
                        set1OriginalAttributes.Contains(sourceAttribute)
                            ? set1AttributeMappings : set2AttributeMappings;

                    List<ComAttribute> mappings;
                    if (!sourceMappings.TryGetValue(oldID, out mappings))
                    {
                        mappings = new List<ComAttribute>();
                        sourceMappings[oldID] = mappings;
                    }
                    mappings.Add(mappingAttribute);

                    if (!turnedOffAttributes.Contains(sourceAttribute))
                    {
                        turnedOff = false;
                    }
                }

                // Remove the sub-attributes now that we have loaded all mapping data from them.
                mergedAttribute.SubAttributes = null;

                // Generate a new IDAndRevision attribute using the new id.
                ComAttribute newIdRevision =
                    attributeCreator.Create(Constants.IDAndRevisionMetadata, newId, "_1");
                AttributeMethods.AppendChildren(mergedAttribute, newIdRevision);

                if (turnedOff)
                {
                    turnedOffIDs.Add(newId);
                }

                outputAttributes.PushBack(mergedAttribute);
            }
        }

        /// <summary>
        /// Merges the non spatial attributes by combining identical attributes while adding all
        /// other attributes to the output verbatim (could result in multiple DocumentTypes,
        /// for example).
        /// </summary>
        /// <param name="set1NonSpatial">The non-spatial <see cref="ComAttribute"/>s from the first
        /// source file.</param>
        /// <param name="set2NonSpatial">The non-spatial <see cref="ComAttribute"/>s from the second
        /// source file.</param>
        /// <returns>The merged collection of non-spatial attributes.</returns>
        static List<ComAttribute> MergeNonSpatialAttributes(List<ComAttribute> set1NonSpatial,
            List<ComAttribute> set2NonSpatial)
        {
            List<ComAttribute> mergedNonSpatialAttributes = new List<ComAttribute>();

            // Loop to look for any identical attributes in set2NonSpatial for each attribute in
            // set1NonSpatial.
            for (int i = 0; i < set1NonSpatial.Count; i++)
            {
                ComAttribute attribute1 = set1NonSpatial[i];
                bool merged = false;

                for (int j = 0; j < set2NonSpatial.Count; j++)
                {
                    ComAttribute attribute2 = set2NonSpatial[j];

                    if (attribute1.Name.Equals(
                            attribute2.Name, StringComparison.OrdinalIgnoreCase) &&
                        attribute1.Value.String.Equals(
                            attribute2.Value.String, StringComparison.OrdinalIgnoreCase) &&
                        attribute1.Type.Equals(
                            attribute2.Type, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!merged)
                        {
                            merged = true;
                            mergedNonSpatialAttributes.Add(attribute1);
                        }

                        set2NonSpatial.RemoveAt(j);
                        j--;
                    }
                }

                if (merged)
                {
                    set1NonSpatial.RemoveAt(i);
                    i--;
                }
            }

            // Add all unmerged attributes to the results and return.
            mergedNonSpatialAttributes.AddRange(set1NonSpatial);
            mergedNonSpatialAttributes.AddRange(set2NonSpatial);

            return mergedNonSpatialAttributes;
        }

        #endregion Private Members
    }
}
