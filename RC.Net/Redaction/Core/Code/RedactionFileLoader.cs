using Extract.AttributeFinder;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Redaction
{
    /// <summary>
    /// Loads the contents of a vector of attributes (VOA) file used for redaction or verification.
    /// </summary>
    public class RedactionFileLoader
    {
        #region Constants

        /// <summary>
        /// The current vector of attributes (VOA) schema version.
        /// </summary>
        static readonly int _VERSION = 1;

        static readonly string _OBJECT_NAME = typeof (RedactionFileLoader).ToString();

        const string _PERSISTED_CONTEXT_TYPE = "_PersistedContext";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The confidence levels of attributes.
        /// </summary>
        readonly ConfidenceLevelsCollection _levels;

        /// <summary>
        /// The name of the vector of attributes (VOA) file.
        /// </summary>
        string _fileName;

        /// <summary>
        /// The name of the source document.
        /// </summary>
        string _sourceDocument;

        /// <summary>
        /// The type of the document; or <see langword="null"/> if the document is unknown.
        /// </summary>
        string _documentType;

        /// <summary>
        /// Clues and redactions.
        /// </summary>
        List<SensitiveItem> _sensitiveItems;

        /// <summary>
        /// Non-metadata attributes that are non-spatial or that are of a type not defined in
        /// _levels (HCData, MCData, ...)
        /// </summary>
        List<ComAttribute> _insensitiveAttributes;

        /// <summary>
        /// The old revisions COM attribute.
        /// </summary>
        ComAttribute _revisionsAttribute;

        /// <summary>
        /// All non-sensitive and metadata attributes.
        /// </summary>
        List<ComAttribute> _metadata;

        /// <summary>
        /// All previous sessions.
        /// </summary>
        ComAttribute[] _allSessions;

        /// <summary>
        /// The previous verification sessions.
        /// </summary>
        ComAttribute[] _verificationSessions;

        /// <summary>
        /// The previous on-demand sessions.
        /// </summary>
        ComAttribute[] _onDemandSessions;

        /// <summary>
        /// The unique verification session id.
        /// </summary>
        int _verificationSessionId;

        /// <summary>
        /// The unique on-demand session id.
        /// </summary>
        int _onDemandSessionId;

        /// <summary>
        /// The previous redaction sessions.
        /// </summary>
        ComAttribute[] _redactionSessions;

        /// <summary>
        /// The previous surround context sessions.
        /// </summary>
        ComAttribute[] _surroundContextSessions;

        /// <summary>
        /// The unique surround context session id.
        /// </summary>
        int _surroundContextSessionId;

        /// <summary>
        /// Creates COM attributes.
        /// </summary>
        AttributeCreator _comAttribute;

        /// <summary>
        /// The unique id of the next created attribute.
        /// </summary>
        long _nextId;

        /// <summary>
        /// This class is not currently able to correctly handle saving multiple times for a given
        /// file. Keep track of whether we have already saved.
        /// </summary>
        bool _alreadySaved;

        /// <summary>
        /// AFUtility gets used a lot for queries, so keep one here for re-use.
        /// </summary>
        AFUtility _utility = new AFUtility();

        /// <summary>
        /// The pages visited in the prior verification session
        /// </summary>
        List<int> _visitedPages;

        /// <summary>
        /// The last visited page in the prior verification session
        /// </summary>
        int _currentPage;

        /// <summary>
        /// The number of pages in the current document
        /// </summary>
        int _numberOfDocumentPages;
        #endregion Fields

        /// <summary>
        /// A flag that indicates if verify all pages mode is enabled or not.
        /// </summary>
        bool _verifyAllPagesMode;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionFileLoader"/> class.
        /// </summary>
        public RedactionFileLoader(ConfidenceLevelsCollection levels)
        {
            LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects,
                "ELI28215", _OBJECT_NAME);

            _levels = levels;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the clues and redactions.
        /// </summary>
        /// <value>The clues and redactions.</value>
        public ReadOnlyCollection<SensitiveItem> Items
        {
            get
            {
                return _sensitiveItems.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets all non-metadata attributes that are non-spatial or that are of a type not defined
        /// in _levels (HCData, MCData, ...)
        /// </summary>
        /// <value>The clues and redactions.</value>
        [CLSCompliant(false)]
        public ReadOnlyCollection<ComAttribute> InsensitiveAttributes
        {
            get
            {
                return _insensitiveAttributes.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the file name associated with this file.
        /// </summary>
        /// <value>The file name associated with this file.</value>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        /// <summary>
        /// Gets the document type associated with this file; <see langword="null"/> if no 
        /// document type is specified.
        /// </summary>
        /// <value>The document type associated with this file; <see langword="null"/> if no 
        /// document type is specified.</value>
        public string DocumentType
        {
            get 
            {
                return _documentType;
            }
        }

        /// <summary>
        /// Gets the possible confidence levels of loaded redactions and clues.
        /// </summary>
        /// <value>The possible confidence levels of loaded redactions and clues.</value>
        public ConfidenceLevelsCollection ConfidenceLevels
        {
            get
            {
                return _levels;
            }
        }

        /// <summary>
        /// Gets the old revisions COM attribute.
        /// </summary>
        /// <value>The old revisions COM attribute.</value>
        [CLSCompliant(false)]
        public ComAttribute RevisionsAttribute
        {
            get 
            {
                return _revisionsAttribute;
            }
        }

        /// <summary>
        /// Gets all session attributes.
        /// </summary>
        /// <value>The verification session attributes.</value>
        [CLSCompliant(false)]
        public ReadOnlyCollection<ComAttribute> AllSessions
        {
            get
            {
                return new ReadOnlyCollection<ComAttribute>(_allSessions);
            }
        }

        /// <summary>
        /// Gets the verification session attributes.
        /// </summary>
        /// <value>The verification session attributes.</value>
        [CLSCompliant(false)]
        public ReadOnlyCollection<ComAttribute> VerificationSessions
        {
            get 
            {
                return new ReadOnlyCollection<ComAttribute>(_verificationSessions);
            }
        }

        /// <summary>
        /// Gets the on-demand session attributes.
        /// </summary>
        /// <value>The on-demand session attributes.</value>
        [CLSCompliant(false)]
        public ReadOnlyCollection<ComAttribute> OnDemandSessions
        {
            get 
            {
                return new ReadOnlyCollection<ComAttribute>(_onDemandSessions);
            }
        }

        /// <summary>
        /// Gets the redaction session attributes.
        /// </summary>
        /// <value>The redaction session attributes.</value>
        [CLSCompliant(false)]
        public ReadOnlyCollection<ComAttribute> RedactionSessions
        {
            get
            {
                return new ReadOnlyCollection<ComAttribute>(_redactionSessions);
            }
        }

        /// <summary>
        /// Gets the surround context session attributes.
        /// </summary>
        /// <value>The surround context session attributes.</value>
        [CLSCompliant(false)]
        public ReadOnlyCollection<ComAttribute> SurroundContextSessions
        {
            get
            {
                return new ReadOnlyCollection<ComAttribute>(_surroundContextSessions);
            }
        }

        /// <summary>
        /// Gets the id to be assigned to the next attribute created.
        /// </summary>
        public long NextId
        {
            get
            {
                return _nextId;
            }
        }

        /// <summary>
        /// Gets the source document
        /// </summary>
        public string SourceDocument
        {
            get
            {
                return _sourceDocument;
            }
        }

        /// <summary>
        /// Gets the pages visited during the last verification session.
        /// </summary>
        /// <value>
        /// The visited pages.
        /// </value>
        public ReadOnlyCollection<int> VisitedPages
        {
            get
            {
                return new ReadOnlyCollection<int>(_visitedPages.OrderBy(page => page).ToList());
            }
        }

        /// <summary>
        /// Gets the page that was current during the last verification session.
        /// </summary>
        /// <value>
        /// The current page.
        /// </value>
        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }
        }

        /// <summary>
        /// Gets or sets the number of document pages.
        /// </summary>
        /// <value>
        /// The number of document pages.
        /// </value>
        public int NumberOfDocumentPages
        {
            get
            {
                return _numberOfDocumentPages;
            }
            set
            {
                _numberOfDocumentPages = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [verify all pages mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [verify all pages mode]; otherwise, <c>false</c>.
        /// </value>
        public bool VerifyAllPagesMode
        {
            get
            {
                return _verifyAllPagesMode;
            }
            set
            {
                _verifyAllPagesMode = value;
            }
        }

        /// <summary>
        /// Gets the visited sensitive item indexes as a list of int.
        /// </summary>
        /// <returns>List of int with indexes of visited items.</returns>
        public ReadOnlyCollection<int> GetVisitedSensitiveItemIndexes
        {
            get
            {
                try
                {
                    var indexList = Enumerable
                                        .Range(0, _sensitiveItems.Count)
                                        .Where(i => _sensitiveItems[i].PriorVerificationVisitedThis)
                                        .ToList();

                    return new ReadOnlyCollection<int>(indexList);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39843");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <overloads>Loads voa file data.</overloads>
        /// <summary>
        /// Loads the contents of the voa file from the specified file.
        /// </summary>
        /// <param name="fileName">The vector of attributes (VOA) file to load.</param>
        /// <param name="sourceDocument">The source document corresponding to the 
        /// <paramref name="fileName"/>.</param>
        public void LoadFrom(string fileName, string sourceDocument)
        {
            try
            {
                _alreadySaved = false;

                Initialize(fileName, sourceDocument);

                if (File.Exists(fileName))
                {
                    // Load the attributes from the file
                    IUnknownVector attributes = new IUnknownVector();
                    attributes.LoadFrom(fileName, false);

                    LoadData(attributes);
                    attributes.ReportMemoryUsage();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28074",
                    "Unable to load voa file.", ex);
                ee.AddDebugData("Voa file", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Loads the contents of the voa file from the specified file.
        /// </summary>
        /// <param name="attributes">The attribute hierarchy to load.</param>
        /// <param name="sourceDocument">The source document corresponding to the 
        /// <paramref name="attributes"/>.</param>
        [CLSCompliant(false)]
        public void LoadFrom(IUnknownVector attributes, string sourceDocument)
        {
            try
            {
                Initialize(string.Empty, sourceDocument);

                LoadData(attributes);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI32267",
                    "Unable to load voa file data.", ex);
                throw ee;
            }
        }

        /// <summary>
        /// Initializes fields prior to loading new data for the specified
        /// <see paramref="fileName"/> and <see paramref="sourceDocument"/>.
        /// </summary>
        /// <param name="fileName">Name of the VOA file from which data will be loaded.</param>
        /// <param name="sourceDocument">The source document (image) the <see paramref="fileName"/>
        /// pertains to.</param>
        void Initialize(string fileName, string sourceDocument)
        {
            _sensitiveItems = new List<SensitiveItem>();
            _insensitiveAttributes = new List<ComAttribute>();
            _metadata = new List<ComAttribute>();
            _verificationSessionId = 0;
            _onDemandSessionId = 0;
            _nextId = 1;
            _allSessions = _verificationSessions = _onDemandSessions = null;

            _fileName = fileName;
            _sourceDocument = sourceDocument;
            _comAttribute = new AttributeCreator(sourceDocument);
        }

        /// <summary>
        /// Loads the data from the specified <see paramref="attributes"/>.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        void LoadData(IUnknownVector attributes)
        {
            // Ensure the schema of this VOA file is accurate
            ValidateSchema(attributes);

            // Get the document type
            _documentType = GetDocumentType(attributes);

            // Get the previous revision
            _revisionsAttribute = GetRevisionsAttribute(attributes, _comAttribute);
            IUnknownVector revisions = _revisionsAttribute.SubAttributes;

            // Query and add attributes at each confidence level
            AFUtility utility = new AFUtility();
            foreach (ConfidenceLevel level in _levels)
            {
                IUnknownVector current = utility.QueryAttributes(attributes, level.Query, true);
                AddAttributes(current, level);

                IUnknownVector previous = utility.QueryAttributes(revisions, level.Query, false);
                AddNonOutputAttributes(previous, level, revisions);
            }

            // Get all previous sessions
            _allSessions = AttributeMethods.GetAttributesByName(attributes,
                Constants.AllSessionMetaDataNames);

            // Get the previous verification sessions
            _verificationSessions = AttributeMethods.GetAttributesByName(attributes,
                Constants.VerificationSessionMetaDataName);
            _verificationSessionId = GetSessionId(_verificationSessions);

            // Get the previous on-demand sessions
            _onDemandSessions = AttributeMethods.GetAttributesByName(attributes,
                Constants.OnDemandSessionMetaDataName);
            _onDemandSessionId = GetSessionId(_onDemandSessions);

            // Get the previous redaction sessions
            _redactionSessions = AttributeMethods.GetAttributesByName(attributes,
                Constants.RedactionSessionMetaDataName);

            // Get the previous surround context sessions
            _surroundContextSessions = AttributeMethods.GetAttributesByName(attributes,
                Constants.SurroundContextSessionMetaDataName);
            _surroundContextSessionId = GetSessionId(_surroundContextSessions);

            // Determine the next attribute id
            // NOTE: This must be done before the call to initialize sensitive items
            _nextId = GetNextId(_sensitiveItems, _revisionsAttribute);

            // Ensure all sensitive items have attribute ids and are in their initial toggled state
            // NOTE: This must be done after initializing _verificationSessions
            InitializeSensitiveItems(_sensitiveItems);

            GetVisitedPages();
            GetCurrentPage();

            // Store the remaining attributes
            int count = attributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute attribute = (ComAttribute)attributes.At(i);
                _metadata.Add(attribute);

                if (!attribute.Name.StartsWith("_", StringComparison.OrdinalIgnoreCase))
                {
                    _insensitiveAttributes.Add(attribute);
                }
            }
        }

        /// <summary>
        /// Gets all the pages visited during the all of the verification sessions.
        /// </summary>
        void GetVisitedPages()
        {
            try
            {
                _visitedPages = GetPagesVisitedDuringAllSessions();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39800");
            }
        }

        /// <summary>
        /// Gets the page that was the current page during the last verification session.
        /// </summary>
        void GetCurrentPage()
        {
            try
            {
                var lastSession = GetLastVerificationSession();
                if (null == lastSession)
                {
                    return;
                }

                var currentPageAttr = AttributeMethods.GetSingleAttributeByName(lastSession.SubAttributes,
                                                                                Constants.CurrentPageMetaDataName);
                if (null == currentPageAttr)
                {
                    return;
                }

                var current = currentPageAttr.Value.String;
                var page = Int32.Parse(current, CultureInfo.InvariantCulture);

                _currentPage = page;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39801");
            }
        }


        /// <summary>
        /// Ensures the specified vector of attributes contains a file information attribute with 
        /// the expected schema version and product name.
        /// </summary>
        /// <param name="attributes">The vector of attributes to validate.</param>
        void ValidateSchema(IUnknownVector attributes)
        {            
            // Create the VOA file information attribute if it doesn't already exist
            ComAttribute fileInfo = AttributeMethods.GetSingleAttributeByName(attributes, "_VOAFileInfo");
            if (fileInfo == null)
            {
                fileInfo = CreateFileInfoAttribute();
                attributes.PushBack(fileInfo);
                return;
            }

            IUnknownVector subAttributes = fileInfo.SubAttributes;

            // Validate product name
            ComAttribute productName = AttributeMethods.GetSingleAttributeByName(subAttributes, "_ProductName");
            string product = productName.Value.String;
            if (!product.Equals("IDShield", StringComparison.OrdinalIgnoreCase))
            {
                ExtractException ee = new ExtractException("ELI28198",
                    "Voa file created for different product.");
                ee.AddDebugData("Product", product, false);
                throw ee;
            }

            // Validate version
            ComAttribute schema = AttributeMethods.GetSingleAttributeByName(subAttributes, "_SchemaVersion");
            int version = int.Parse(schema.Value.String, CultureInfo.CurrentCulture);
            if (version > _VERSION)
            {
                ExtractException ee = new ExtractException("ELI28199",
                    "Unrecognized schema version.");
                ee.AddDebugData("Version", version, false);
                ee.AddDebugData("Maximum recognized version", _VERSION, false);
                throw ee;
            }
        }

        /// <summary>
        /// Creates the file information attribute.
        /// </summary>
        /// <returns>The file information attribute.</returns>
        ComAttribute CreateFileInfoAttribute()
        {
            ComAttribute fileInfo = _comAttribute.Create("_VOAFileInfo");

            // Product name
            ComAttribute productNameAttribute = _comAttribute.Create("_ProductName", "IDShield");

            // Schema version
            string version = _VERSION.ToString(CultureInfo.CurrentCulture);
            ComAttribute schemaVersion = _comAttribute.Create("_SchemaVersion", version);

            AttributeMethods.AppendChildren(fileInfo, productNameAttribute, schemaVersion);

            return fileInfo;
        }

        /// <summary>
        /// Gets the document type from the specified vector of attributes.
        /// </summary>
        /// <param name="attributes">The vector of attributes to search.</param>
        /// <returns>The first document type in <paramref name="attributes"/>.</returns>
        static string GetDocumentType(IIUnknownVector attributes)
        {
            int count = attributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute attribute = (ComAttribute)attributes.At(i);
                if (attribute.Name.Equals("DocumentType", StringComparison.OrdinalIgnoreCase))
                {
                    return attribute.Value.String;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds attributes by confidence level and stores them in <see cref="_sensitiveItems"/>.
        /// </summary>
        /// <param name="attributes">The attributes to add.</param>
        /// <param name="level">The confidence level of the <paramref name="attributes"/>.</param>
        void AddAttributes(IUnknownVector attributes, ConfidenceLevel level)
        {
            // Iterate over the attributes
            int count = attributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute attribute = (ComAttribute)attributes.At(i);

                // Add this item if it is spatial, otherwise store it in the original collection
                SpatialString value = attribute.Value;
                if (value.HasSpatialInfo())
                {
                    _sensitiveItems.Add(new SensitiveItem(level, attribute));
                }
                else
                {
                    _insensitiveAttributes.Add(attribute);
                    _metadata.Add(attribute);
                }
            }
        }

        /// <summary>
        /// Adds attributes that are marked for non-output by confidence level and stores them in 
        /// <see cref="_sensitiveItems"/>.
        /// </summary>
        /// <param name="attributes">The attributes from which non-output attributes should be 
        /// added.</param>
        /// <param name="level">The confidence level of the <paramref name="attributes"/>.</param>
        /// <param name="revisions">The previous revisions of attributes.</param>
        void AddNonOutputAttributes(IUnknownVector attributes, ConfidenceLevel level, 
            IUnknownVector revisions)
        {
            // Iterate over the attributes
            int count = attributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute attribute = (ComAttribute) attributes.At(i);

                // Check to see if this is a non-output attribute.
                if (IsTurnedOffAttribute(attribute, true))
                {
                    // Add this non output attribute to the collection of sensitive items
                    SensitiveItem item = new SensitiveItem(level, attribute);
                    item.Attribute.Redacted = false;
                    _sensitiveItems.Add(item);

                    // Remove it from the previous revisions
                    revisions.RemoveValue(attribute);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified <see paramref="attribute"/> is turned-off attribute.
        /// </summary>
        /// <param name="attribute">The <see cref="ComAttribute"/> to test.</param>
        /// <param name="removeArchiveAction"><see langword="true"/> to remove the 'ArchiveAction'
        /// attribute if it is a turned-off attribute; otherwise <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if the <paramref name="attribute"/> has an 'AchiveAction'
        /// sub-attribute with the value 'turned off'; otherwise <see langword="false"/>.
        /// </returns>
        static bool IsTurnedOffAttribute(ComAttribute attribute, bool removeArchiveAction)
        {
            IUnknownVector subAttributes = attribute.SubAttributes;
            ComAttribute archiveActionAttribute = null;

            ComAttribute[] archiveActionAttributes = 
                AttributeMethods.GetAttributesByName(subAttributes, Constants.ArchiveAction);
            if (archiveActionAttributes.Length == 1)
            {
                archiveActionAttribute = archiveActionAttributes[0];
            }

            if (archiveActionAttribute != null)
            {
                SpatialString value = archiveActionAttribute.Value;
                if (string.Equals(value.String, Constants.TurnedOff, StringComparison.OrdinalIgnoreCase))
                {
                    if (removeArchiveAction)
                    {
                        subAttributes.RemoveValue(archiveActionAttribute);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the last verification session.
        /// </summary>
        /// <returns>last session attribute</returns>
        ComAttribute GetLastVerificationSession()
        {
            try
            {
                return GetVerificationSession(_verificationSessionId);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39799");
            }
        }

        /// <summary>
        /// Gets the specified verification session.
        /// </summary>
        /// <param name="sessionId">The session identifier of the session to retrieve</param>
        /// <returns>null if not found, or the ComAttribute of the requested session</returns>
        ComAttribute GetVerificationSession(int sessionId)
        {
            try
            {
                foreach (var attribute in _verificationSessions)
                {
                    var value = attribute.Value.String;
                    if (!String.IsNullOrWhiteSpace(value))
                    {
                        int currentSessionId = int.Parse(value, CultureInfo.CurrentCulture);
                        if (sessionId == currentSessionId)
                        {
                            return attribute;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39838");
            }

        }

        /// <summary>
        /// Gets the GUIDs string that specifies all of the previously visited redaction items.
        /// </summary>
        /// <returns>comma-delimited list of GUID strings for all previously visited sensitive items, 
        /// or empty if not found</returns>
        string[] GetVisitedRedactionItemsAsGUIDs()
        {
            try
            {
                HashSet<string> storeOfGuids = new HashSet<string>();

                foreach (var session in _verificationSessions)
                {
                    var visitedItemsAttr =
                        AttributeMethods.GetSingleAttributeByName(session.SubAttributes,
                                                                  Constants.VisitedRedactionItemsMetaDataName);
                    if (null == visitedItemsAttr)
                    {
                        continue;
                    }

                    string guids = visitedItemsAttr.Value.String;
                    var arrayOfGuids = guids.Split(',');
                    foreach (var guid in arrayOfGuids)
                    {
                        storeOfGuids.Add(guid.Trim());
                    }
                }

                return storeOfGuids.ToArray();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39783");
            }
        }

        /// <summary>
        /// Sets the state of all the prior verification visited items.
        /// </summary>
        void SetPriorVerificationVisitedState()
        {
            try
            {
                var visitedItemsGuids = GetVisitedRedactionItemsAsGUIDs();
                foreach (var guid in visitedItemsGuids)
                {
                    SensitiveItem item = _sensitiveItems.Find(i => i.GUID == guid);
                    if (item != null)
                    {
                        item.PriorVerificationVisitedThis = true;
                    }                    
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39782");
            }
        }


        /// <summary>
        /// Assigns the ID and initial redacted state of the specified sensitive items.
        /// </summary>
        /// <param name="sensitiveItems">The sensitive items to initialize.</param>
        void InitializeSensitiveItems(IEnumerable<SensitiveItem> sensitiveItems)
        {
            foreach (SensitiveItem item in sensitiveItems)
            {
                bool assigned = AssignId(item.Attribute);
                if (assigned || _verificationSessions.Length == 0)
                {
                    // Set the toggled state if this is the first time the attribute was verified
                    // https://extract.atlassian.net/browse/ISSUE-6306 [FIDSC #4026]
                    // https://extract.atlassian.net/browse/ISSUE-7569
                    item.Attribute.Redacted = item.Level.Output;
                }
            }

            SetPriorVerificationVisitedState();
        }

        /// <summary>
        /// Adds an unique id attribute to the specified attribute if it does not already contain 
        /// one.
        /// </summary>
        /// <param name="attribute">The attribute to which an id may be added.</param>
        /// <returns><see langword="true"/> if the attribute was assigned an ID;
        /// <see langword="false"/> if the attribute already had an ID.</returns>
        bool AssignId(RedactionItem attribute)
        {
            bool idAdded = attribute.AssignIdIfNeeded(_nextId, _sourceDocument);
            if (idAdded)
            {
                _nextId++;
            }

            return idAdded;
        }

        /// <summary>
        /// Calculates the unique id of the next created attribute.
        /// </summary>
        /// <returns>The unique id of the next created attribute.</returns>
        static long GetNextId(IEnumerable<SensitiveItem> items, ComAttribute revisionsAttribute)
        {
            // Iterate over the clues and redactions for the next id.
            long nextId = 1;
            foreach (SensitiveItem item in items)
            {
                nextId = GetNextId(item.Attribute, nextId);
            }

            // Iterate over all previously deleted attributes for the next id.
            IUnknownVector subAttributes = revisionsAttribute.SubAttributes;
            int count = subAttributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute attribute = (ComAttribute)subAttributes.At(i);
                RedactionItem redaction = new RedactionItem(attribute);
                nextId = GetNextId(redaction, nextId);
            }

            return nextId;
        }

        /// <summary>
        /// Determines the relative next id between the id of the specified attribute and the 
        /// specified value.
        /// </summary>
        /// <param name="attribute">The attribute whose id should be checked.</param>
        /// <param name="nextId">The current next id.</param>
        /// <returns>Returns <paramref name="nextId"/> if it is smaller than the id of 
        /// <paramref name="attribute"/>; returns the id after the id of 
        /// <paramref name="attribute"/> if it is larger than <paramref name="nextId"/>.</returns>
        static long GetNextId(RedactionItem attribute, long nextId)
        {
            long id = attribute.GetId();
            return id < nextId ? nextId : id + 1;
        }

        /// <summary>
        /// Gets or creates the revisions attribute for the specified vector.
        /// </summary>
        /// <param name="vector">The vector to search for a revisions attribute.</param>
        /// <param name="attributeCreator">If not <see langword="null"/> and a revisions attribute
        /// does not already exist, this <see cref="AttributeCreator"/> will be used to create one.
        /// </param>
        /// <returns>The revisions attribute for <paramref name="vector"/>; if no such attribute 
        /// exists and <see paramref="attributeCreator"/> was provided, it is created and added to
        /// <paramref name="vector"/>.</returns>
        static ComAttribute GetRevisionsAttribute(IUnknownVector vector, AttributeCreator attributeCreator)
        {
            // Look for an existing revisions attribute
            ComAttribute revisionsAttribute = AttributeMethods.GetSingleAttributeByName(vector, "_OldRevisions");
            if (revisionsAttribute == null && attributeCreator != null)
            {
                // Create a new revisions attribute
                revisionsAttribute = attributeCreator.Create("_OldRevisions");
                vector.PushBack(revisionsAttribute);
            }

            return revisionsAttribute;
        }

        /// <summary>
        /// Gets the id of the most recent session.
        /// </summary>
        /// <param name="sessions">An array of attributes to search for the most recent session.
        /// </param>
        /// <returns>The id of the most recent verification session in <paramref name="sessions"/>; 
        /// or zero if no verification session attribute exists.
        /// </returns>
        static int GetSessionId(ComAttribute[] sessions)
        {
            int result = 0;

            // Iterate over each verification session attribute
            foreach (ComAttribute attribute in sessions)
            {
                SpatialString value = attribute.Value;
                if (value != null)
                {
                    // If this session is later than the current session id, store it
                    int sessionId = int.Parse(value.String, CultureInfo.CurrentCulture);
                    if (sessionId > result)
                    {
                        result = sessionId;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Saves the <see cref="RedactionFileLoader"/> with the specified changes and information.
        /// </summary>
        /// <param name="fileName">The full path to location where the file should be saved.</param>
        /// <param name="changes">The attributes that have been added, modified, and deleted.</param>
        /// <param name="time">The interval of screen time spent verifying the file.</param>
        /// <param name="settings">The settings used during verification.</param>
        /// <param name="standAloneMode"><see langword="true"/> if the verification session was run
        /// independent of the FAM and database; <see langword="false"/> otherwise.</param>
        /// <param name="allowDuplicateSave">Ordinarily multiple saves in the same session need to
        /// be disallowed since this class may otherwise duplicate redactions. However, in some
        /// isolated cases this may be okay. <see langword="true"/> allows a save even if it as
        /// already been saved. <see langword="false"/> asserts that the VOA file has not been
        /// previously saved.</param>
        /// <param name="sessionContext">session context data to be saved</param>
        [CLSCompliant(false)]
        public void SaveVerificationSession(string fileName,
            RedactionFileChanges changes, TimeInterval time, VerificationSettings settings,
            bool standAloneMode, bool allowDuplicateSave, SessionContext sessionContext)
        {
            try
            {
                ComAttribute sessionData = CreateVerificationOptionsAttribute(settings);

                string sessionName = standAloneMode
                    ? Constants.OnDemandSessionMetaDataName
                    : Constants.VerificationSessionMetaDataName;

                int sessionId = standAloneMode
                    ? _onDemandSessionId++
                    : _verificationSessionId++;

                // Calculate the new sensitive items
                SaveSession(sessionName, sessionId, fileName, changes, time, sessionData,
                    allowDuplicateSave, sessionContext);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28165",
                    "Unable to save verification file.", ex);
                ee.AddDebugData("Voa file", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Saves the <see cref="RedactionFileLoader"/> with the specified changes and information. 
        /// </summary>
        /// <param name="fileName">The full path to location where the file should be saved.</param>
        /// <param name="changes">The attributes that have been added, modified, and deleted.</param>
        /// <param name="time">The interval of screen time spent verifying the file.</param>
        /// <param name="settings">The settings used during verification.</param>
        [CLSCompliant(false)]
        public void SaveSurroundContextSession(string fileName, RedactionFileChanges changes,
            TimeInterval time, SurroundContextSettings settings)
        {
            try
            {
                ComAttribute sessionData = CreateSurroundContextOptionsAttribute(settings);

                // Calculate the new sensitive items
                SaveSession(Constants.SurroundContextSessionMetaDataName, _surroundContextSessionId,
                    fileName, changes, time, sessionData, false);

                // Update the session id
                _surroundContextSessionId++;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI29900",
                    "Unable to save updated VOA file.", ex);
                ee.AddDebugData("Voa file", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Saves the <see cref="RedactionFileLoader"/> with the information about a VOA file merge
        /// operation.
        /// </summary>
        /// <param name="sessionName">The name of the session (compare or merge).</param>
        /// <param name="fileName">The full path to location where the file should be saved.</param>
        /// <param name="sourceFile1">The name of the first VOA file whose data was merged into
        /// <see paramref="fileName"/>.</param>
        /// <param name="sourceFile2">The name of the second VOA file whose data was merged into
        /// <see paramref="fileName"/>.</param>
        /// <param name="set1Mappings">The keys of this dictionary are the original IDs of
        /// each attribute from <see paramref="sourceFile1"/> while the values are a list of
        /// <see cref="ComAttribute"/>s that associate the attribute with the ID of an attribute in
        /// <see paramref="fileName"/>.</param>
        /// <param name="set2Mappings">The keys of this dictionary are the original IDs of
        /// each attribute from <see paramref="sourceFile2"/> while the values are a list of
        /// <see cref="ComAttribute"/>s that associate the attribute with the ID of an attribute in
        /// <see paramref="fileName"/>.</param>
        /// <param name="time">The interval of time spent comparing an merging the source files.
        /// </param>
        /// <param name="settings">The settings used during the compare/merge operation.</param>
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void SaveVOAFileMergeSession(string sessionName, string fileName, string sourceFile1,
            string sourceFile2, Dictionary<string, List<ComAttribute>> set1Mappings,
            Dictionary<string, List<ComAttribute>> set2Mappings, TimeInterval time,
            VOAFileMergeTaskSettings settings)
        {
            try
            {
                // Create session attribute
                // (This will always be the one and only session, so the last session ID is 0).
                ComAttribute session = CreateSessionAttribute(sessionName, 0, time);

                // Rename _IDShieldDataFile to _OutputFile
                ComAttribute outputFile = AttributeMethods.GetSingleAttributeByName(
                    session.SubAttributes, Constants.IDShieldDataFileMetadata);
                outputFile.Name = Constants.OutputFileMetadata;

                // Create an attribute containing the session's settings.
                ComAttribute sessionSettings = CreateGenericOptionsAttribute(settings);

                AttributeMethods.AppendChildren(session, sessionSettings);

                // Add the metadata from each of the source data files.
                AddSourceMetadata(session, sourceFile1, set1Mappings);
                AddSourceMetadata(session, sourceFile2, set2Mappings);

                // Organize the metadata attributes for output.
                IUnknownVector attributes = PrepareOutput(_sensitiveItems, null);

                // Add merge session metadata
                attributes.PushBack(session);

                // Save the attributes
                SaveAttributesTo(_sensitiveItems, attributes, fileName);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI32268",
                    "Unable to save updated VOA file.", ex);
                ee.AddDebugData("Voa file", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Adds to the <see paramref="session"/> metadata for the specified
        /// <see paramref="sourceFile"/>.
        /// </summary>
        /// <param name="session">The session to which the metadata should be added.</param>
        /// <param name="sourceDataFile">Name of the source file for which metadata should be added.
        /// </param>
        /// <param name="attributeMappings">The keys of this dictionary are the original IDs of
        /// each attribute from <see paramref="sourceFile"/> while the values are a list of
        /// <see cref="ComAttribute"/>s that associate the attribute with the ID of an attribute in
        /// the output.</param>
        void AddSourceMetadata(ComAttribute session, string sourceDataFile,
            Dictionary<string, List<ComAttribute>> attributeMappings)
        {
            ComAttribute sourceData =
                _comAttribute.Create(Constants.IDShieldDataFileMetadata, sourceDataFile);

            IUnknownVector sourceAttributes = new IUnknownVector();
            sourceAttributes.LoadFrom(sourceDataFile, false);

            // Find all turned-off attributes from the old revisions attribute.
            ComAttribute revisionsAttribute = GetRevisionsAttribute(sourceAttributes, null);
            IEnumerable<ComAttribute> turnedOffAttributes = (revisionsAttribute == null)
                ? new List<ComAttribute>()
                : revisionsAttribute.SubAttributes
                    .ToIEnumerable<ComAttribute>()
                    .Where(attribute => IsTurnedOffAttribute(attribute, false));

            // Iterate all current sensitive items (including turned-off attributes) from the source
            // in order to apply all mapping metadata from attributeMappings that applies to the
            // items.
            foreach (ComAttribute attribute in sourceAttributes
                .ToIEnumerable<ComAttribute>()
                .Where(attribute => !attribute.Name.StartsWith("_", StringComparison.OrdinalIgnoreCase))
                .Union(turnedOffAttributes))
            {
                ComAttribute idRevision = AttributeMethods.GetSingleAttributeByName(
                    attribute.SubAttributes, Constants.IDAndRevisionMetadata);
                if (idRevision != null)
                {
                    List<ComAttribute> mappings;
                    if (attributeMappings.TryGetValue(idRevision.Value.String, out mappings))
                    {
                        AttributeMethods.AppendChildren(attribute, mappings.ToArray());
                    }
                }
            }

            AttributeMethods.AppendChildren(sourceData,
                sourceAttributes.ToIEnumerable<ComAttribute>().ToArray());

            AttributeMethods.AppendChildren(session, sourceData);
        }

        /// <summary>
        /// Finds any subattribute named "_visited".
        /// </summary>
        /// <param name="subattributes">vector of subattributes to search for named subattribute.</param>
        /// <returns>return an IUnknownVector, either empty (no match), or populated (on match)</returns>
        IUnknownVector GetVisitedSubAttribute(IUnknownVector subattributes)
        {
            var attr = _utility.QueryAttributes(subattributes, 
                                                strQuery: Constants.VisitedItemMetaDataName, 
                                                bRemoveMatches: false);
            return attr;
        }

        /// <summary>
        /// Modifies the VOA file and appends a session metadata COM attribute.
        /// </summary>
        /// <param name="sessionName">The name of the session metadata COM attribute.</param>
        /// <param name="lastSessionId">The unique ID of the previous session.</param>
        /// <param name="fileName">The full path of the VOA file to save.</param>
        /// <param name="changes">The changes made to the original VOA file.</param>
        /// <param name="time">The time elapsed during the session being saved.</param>
        /// <param name="sessionData">Additional session data to add as a sub attribute.</param>
        /// <param name="allowDuplicateSave">Ordinarily multiple saves in the same session need to
        /// be disallowed since this class may otherwise duplicate redactions (see [FlexIDSCore:5028,
        /// 5029]). However, in some isolated cases this may be okay such as when IDSOD is creating
        /// a VOA file to use for redacted output. <see langword="true"/> allows a save even if
        /// it as already been saved. <see langword="false"/> asserts that the VOA file has not been
        /// previously saved.</param>
        /// <param name="sessionContext">session context data to save</param>
        void SaveSession(string sessionName, int lastSessionId, string fileName, 
            RedactionFileChanges changes, TimeInterval time, ComAttribute sessionData,
            bool allowDuplicateSave, SessionContext sessionContext = null)
        {
            if (!allowDuplicateSave && _alreadySaved)
            {
                new ExtractException("ELI34362",
                    "Unexpected VOA save detected; attribute duplication is possible.").Log();
            }

            List<SensitiveItem> sensitiveItems = new List<SensitiveItem>(_sensitiveItems);

            // Update any changed items (deleted, modified, and added)
            IUnknownVector attributes = PrepareOutput(sensitiveItems, changes);

            // Create session attribute
            ComAttribute session = 
                CreateSessionAttribute(sessionName, lastSessionId, time);

            // Append additional session information
            AttributeMethods.AppendChildren(session, sessionData);

            // Append change entries
            AppendChangeEntries(session, changes);

            // Append session context entries
            if (null != sessionContext)
            {
                AppendSessionContext(session, sessionContext, sensitiveItems);
            }

            attributes.PushBack(session);

            // Save the attributes
            SaveAttributesTo(sensitiveItems, attributes, fileName);

            _alreadySaved = true;
        }

        /// <summary>
        /// Prepares the attributes to be output to a VOA file by filing changes under added,
        /// modified and deleted attributes as well as archiving disabled (turned-off) attributes
        /// to the revisions attribute.
        /// </summary>
        /// <param name="sensitiveItems">The sensitive items after changes were made.</param>
        /// <param name="changes">The changes that were made. (Can be <see langword="null"/>
        /// if there are no changes).</param>
        /// <returns>The attributes after changes are made.</returns>
        IUnknownVector PrepareOutput(List<SensitiveItem> sensitiveItems, RedactionFileChanges changes)
        {
            IEnumerable<RedactionItem> oldDeleted;
            IEnumerable<RedactionItem> oldModified;

            if (changes == null)
            {
                oldDeleted = new RedactionItem[] { };
                oldModified = new RedactionItem[] { };
            }
            else
            {
                oldDeleted = UpdateDeletedItems(sensitiveItems, changes.Deleted);
                oldModified = UpdateModifiedItems(sensitiveItems, changes.Modified);
                UpdateAddedItems(sensitiveItems, changes.Added);
            }

            // Get the non output redactions
            IEnumerable<RedactionItem> oldNonOutput = GetNonOutputRedactions(sensitiveItems);

            // Copy the original metadata attributes in to a vector
            IUnknownVector attributes = CopyMetadataAttributes();

            // Update the revisions attribute
            ComAttribute revisions = GetRevisionsAttribute(attributes, _comAttribute);
            AddRevisions(revisions, oldDeleted, oldModified, oldNonOutput);
            return attributes;
        }

        /// <summary>
        /// Saves the sensitive items and other attributes to the specified file.
        /// </summary>
        /// <param name="sensitiveItems">The sensitive items to save.</param>
        /// <param name="attributes">The non-sensitive items to save.</param>
        /// <param name="fileName">The file to which the attributes should be saved.</param>
        void SaveAttributesTo(List<SensitiveItem> sensitiveItems, IUnknownVector attributes, 
            string fileName)
        {
            IUnknownVector output = GetOutputVector(sensitiveItems, attributes);
            output.SaveTo(fileName, false, typeof(AttributeStorageManagerClass).GUID.ToString("B"));

            // Update the sensitive items
            _sensitiveItems = sensitiveItems;

            // Update the attributes that don't need verification
            _metadata = GetAttributesFromVector(attributes);
        }

        /// <summary>
        /// Updates the specified verification items with deleted attributes.
        /// </summary>
        /// <param name="items">The items to be updated.</param>
        /// <param name="deleted">The attributes to delete.</param>
        /// <returns>The previous version of the deleted attributes.</returns>
        static IEnumerable<RedactionItem> UpdateDeletedItems(IList<SensitiveItem> items,
            ICollection<RedactionItem> deleted)
        {
            // Make an list to hold the previous version of the modified attributes
            List<RedactionItem> oldDeleted = new List<RedactionItem>(deleted.Count);

            // Iterate over each deleted attribute
            foreach (RedactionItem attribute in deleted)
            {
                long targetId = attribute.GetId();

                // Find the corresponding previous version
                int index = GetIndexFromAttributeId(items, targetId);
                if (index < 0)
                {
                    throw new ExtractException("ELI28213",
                        "Missing original version of deleted attribute.");
                }

                // Store the previous version
                oldDeleted.Add(items[index].Attribute);

                // Remove the deleted attribute
                items.RemoveAt(index);
            }

            return oldDeleted;
        }

        /// <summary>
        /// Updates the specified verification items with modified attributes.
        /// </summary>
        /// <param name="items">The items to be updated.</param>
        /// <param name="modified">The attributes to modify.</param>
        /// <returns>The previous version of the modified attributes.</returns>
        static IEnumerable<RedactionItem> UpdateModifiedItems(IList<SensitiveItem> items,
            ICollection<RedactionItem> modified)
        {
            // Make an list to hold the previous version of the modified attributes
            List<RedactionItem> oldModified = new List<RedactionItem>(modified.Count);

            // Iterate over each modified attribute
            foreach (RedactionItem attribute in modified)
            {
                long targetId = attribute.GetId();

                // Find the corresponding previous version
                int index = GetIndexFromAttributeId(items, targetId);
                if (index < 0)
                {
                    throw new ExtractException("ELI28212",
                        "Missing original version of modified attribute.");
                }

                // Store the previous version
                SensitiveItem old = items[index];

                // [FlexIDSCore:5029]
                // Non-redacted attributes will be collected in GetNonOutputRedactions.
                if (attribute.Redacted)
                {
                    oldModified.Add(old.Attribute);
                }

                // Update the version number of the modified attribute
                attribute.IncrementRevision();

                // Store the new version
                items[index] = new SensitiveItem(old.Level, attribute);
            }

            return oldModified;
        }

        /// <summary>
        /// Updates the specified verification items with added attributes.
        /// </summary>
        /// <param name="items">The items to be updated.</param>
        /// <param name="added">The attributes to add.</param>
        void UpdateAddedItems(ICollection<SensitiveItem> items, IEnumerable<RedactionItem> added)
        {
            foreach (RedactionItem attribute in added)
            {
                // Each new attribute needs a unique id
                AssignId(attribute);

                // Add the attribute
                SensitiveItem item = new SensitiveItem(_levels.Manual, attribute);
                items.Add(item);
            }
        }

        /// <summary>
        /// Gets the index of the item in the specified collection that has the specified 
        /// attribute id.
        /// </summary>
        /// <param name="items">The collection to search.</param>
        /// <param name="id">The attribute id of an item in <paramref name="items"/>.</param>
        /// <returns>The index of the item in <paramref name="items"/> with the specified 
        /// attribute <paramref name="id"/>; or -1 if no such item exists.</returns>
        static int GetIndexFromAttributeId(IList<SensitiveItem> items, long id)
        {
            for (int i = 0; i < items.Count; i++)
            {
                SensitiveItem item = items[i];
                long itemId = item.Attribute.GetId();
                if (id == itemId)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets redactions that have been marked to not be output.
        /// </summary>
        /// <param name="redactions">The redactions to check for non-output.</param>
        /// <returns>The redactions in <paramref name="redactions"/> that are marked to not be 
        /// output.</returns>
        static IEnumerable<RedactionItem> GetNonOutputRedactions(IEnumerable<SensitiveItem> redactions)
        {
            List<RedactionItem> nonOutputRedactions = new List<RedactionItem>();
            foreach (SensitiveItem item in redactions)
            {
                RedactionItem redactionItem = item.Attribute;
                if (!redactionItem.Redacted)
                {
                    nonOutputRedactions.Add(redactionItem);
                }
            }

            return nonOutputRedactions;
        }

        /// <summary>
        /// Creates a copy of all the COM metadata attributes.
        /// </summary>
        /// <returns>A copy of all the COM metadata attributes.</returns>
        IUnknownVector CopyMetadataAttributes()
        {
            IUnknownVector attributes = new IUnknownVector();
            foreach (ComAttribute attribute in _metadata)
            {
                ICloneIdentifiableObject copy = (ICloneIdentifiableObject)attribute;
                attributes.PushBack(copy.CloneIdentifiableObject());
            }

            return attributes;
        }

        /// <summary>
        /// Creates a new session attribute.
        /// </summary>
        /// <param name="sessionName">The name of the session attribute to create.</param>
        /// <param name="lastSessionId">The unique ID of the last session in the file</param>
        /// <param name="screenTime">The duration of time spent during verification.</param>
        /// <returns>A new session attribute.</returns>
        ComAttribute CreateSessionAttribute(string sessionName, int lastSessionId, TimeInterval screenTime)
        {
            // User and screenTime information
            ComAttribute user = CreateUserAttribute();
            ComAttribute time = CreateScreenTimeAttribute(screenTime);

            // File information
            ComAttribute source = _comAttribute.Create(Constants.SourceDocNameMetadata, _sourceDocument);
            ComAttribute data = _comAttribute.Create(Constants.IDShieldDataFileMetadata, _fileName);

            // Session attribute
            ComAttribute session = _comAttribute.Create(sessionName, lastSessionId + 1);
            AttributeMethods.AppendChildren(session, user, time, source, data);

            return session;
        }

        /// <summary>
        /// Adds the value to attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">The value.</param>
        void AddPersistedContext(ComAttribute attribute, int value)
        {
            try
            {
                var strValue = value.ToString(CultureInfo.InvariantCulture);
                AddPersistedContext(attribute, strValue);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39751");
            }
        }

        void AddPersistedContext(ComAttribute attribute, string value)
        {
            try
            {
                SpatialString ss = new SpatialString();
                ss.CreateNonSpatialString(value, SourceDocument);

                attribute.Value = ss;
                attribute.Type = _PERSISTED_CONTEXT_TYPE;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40375");
            }
        }
        

        /// <summary>
        /// Creates the session context attribute, with children corresponding to the four parameters.
        /// </summary>
        /// <param name="visitedPages">The visited pages.</param>
        /// <param name="currentPageIndex">Index of the current page.</param>
        /// <param name="visitedRedactionItems">The visited redaction items.</param>
        /// <param name="sensitiveItems">List of sensitive items, used to retrieve GUID for each
        /// listed sensitive attribute that is currently selected (if any)</param>
        /// <param name="parent">parent attribute to attach subattributes too</param>
        void CreateSessionContextAttribute(List<int> visitedPages,
                                           int currentPageIndex,
                                           List<int> visitedRedactionItems,
                                           List<SensitiveItem> sensitiveItems,
                                           ComAttribute parent)
        {
            try
            {
                // child attributes
                ComAttribute pages = _comAttribute.Create("_VisitedPages");
                IEnumerable<int> visitedPagesOnesRelative = visitedPages.Select(page => page + 1);
                string pageRange = visitedPagesOnesRelative.ToRangeString();
                AddPersistedContext(pages, pageRange);

                ComAttribute currentPage = _comAttribute.Create("_CurrentPage");
                AddPersistedContext(currentPage, currentPageIndex);

                ComAttribute visitedRedactions = _comAttribute.Create(Constants.VisitedRedactionItemsMetaDataName);
                List<String> visitedSensitiveItemGUIDs = new List<string>();
                foreach (var index in visitedRedactionItems)
                {
                    ExtractException.Assert("ELI39763",
                                            String.Format(CultureInfo.InvariantCulture,
                                                          "Index value: {0} from the visitedRedactionItems list exceeds" +
                                                          " the sensitive items count: {1}.",
                                                          index,
                                                          sensitiveItems.Count),
                                            index < sensitiveItems.Count);

                    string guid = sensitiveItems[index].GUID;
                    visitedSensitiveItemGUIDs.Add(guid);
                }

                string guidsListed = String.Join(", ", visitedSensitiveItemGUIDs);
                AddPersistedContext(visitedRedactions, guidsListed);

                AttributeMethods.AppendChildren(parent,
                                                pages,
                                                currentPage,
                                                visitedRedactions);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39745");
            }
        }

        /// <summary>
        /// Appends session context to the parent attribute.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="sessionContext">The session context to append.</param>
        /// <param name="sensitiveItems">List of sensitive items, used to retrieve attribute GUID for the 
        /// selected redaction items.</param>
        void AppendSessionContext(ComAttribute parent, SessionContext sessionContext, List<SensitiveItem> sensitiveItems)
        {
            try
            {
                CreateSessionContextAttribute(sessionContext.VisitedPages,
                                              sessionContext.CurrentPageIndex,
                                              sessionContext.VisitedRedactions,
                                              sensitiveItems,
                                              parent);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39744");
            }
        }

        /// <summary>
        /// Appends metadata about what changes were made.
        /// </summary>
        /// <param name="session">The session metadata COM attribute to which the changes should 
        /// be appended.</param>
        /// <param name="changes">The changes that were made.</param>
        void AppendChangeEntries(ComAttribute session, RedactionFileChanges changes)
        {
            ComAttribute added = CreateChangeEntriesAttribute("_EntriesAdded", changes.Added);
            ComAttribute deleted = CreateChangeEntriesAttribute("_EntriesDeleted", changes.Deleted);
            ComAttribute modified = CreateChangeEntriesAttribute("_EntriesModified", changes.Modified);

            AttributeMethods.AppendChildren(session, added, deleted, modified);
        }

        /// <summary>
        /// Creates the user information attribute.
        /// </summary>
        /// <returns>The user information attribute.</returns>
        ComAttribute CreateUserAttribute()
        {
            ComAttribute userInfo = _comAttribute.Create("_UserInfo");
            ComAttribute loginId = _comAttribute.Create("_LoginID", Environment.UserName);
            ComAttribute computerName = _comAttribute.Create("_Computer", Environment.MachineName);
            AttributeMethods.AppendChildren(userInfo, loginId, computerName);

            return userInfo;
        }

        /// <summary>
        /// Creates the verification time attribute.
        /// </summary>
        /// <param name="time">The duration of time spent verifying the document.</param>
        /// <returns>The verification time attribute.</returns>
        ComAttribute CreateScreenTimeAttribute(TimeInterval time)
        {
            ComAttribute timeInfo = _comAttribute.Create("_TimeInfo");
            ComAttribute date = _comAttribute.Create("_Date", time.Start.ToShortDateString());
            ComAttribute timeStarted = _comAttribute.Create("_TimeStarted",
                time.Start.ToString("h:mm:ss.fff tt", CultureInfo.CurrentCulture));
            ComAttribute totalSeconds = _comAttribute.Create("_TotalSeconds", time.ElapsedSeconds);
            AttributeMethods.AppendChildren(timeInfo, date, timeStarted, totalSeconds);

            return timeInfo;
        }

        /// <summary>
        /// Creates the verification options attribute.
        /// </summary>
        /// <param name="settings">The settings used during verification.</param>
        /// <returns>The verification options attribute.</returns>
        ComAttribute CreateVerificationOptionsAttribute(VerificationSettings settings)
        {
            ComAttribute options = _comAttribute.Create("_VerificationOptions");
            string value = settings.General.VerifyAllPages ? "Yes" : "No";
            ComAttribute verifyAllPages = _comAttribute.Create("_VerifyAllPages", value);
            AttributeMethods.AppendChildren(options, verifyAllPages);

            return options;
        }

        /// <summary>
        /// Creates a metadata attribute describing <see cref="SurroundContextSettings"/>.
        /// </summary>
        /// <param name="settings">The settings to add.</param>
        /// <returns>A metadata attribute describing <see cref="SurroundContextSettings"/>.
        /// </returns>
        ComAttribute CreateSurroundContextOptionsAttribute(SurroundContextSettings settings)
        {
            ComAttribute options = _comAttribute.Create(Constants.OptionsMetadata);
            ComAttribute typesToExtend = _comAttribute.Create("_TypesToExtend",
                GetTypesToExtend(settings));

            int words = settings.RedactWords ? settings.MaxWords : 0;
            ComAttribute wordsToExtend = _comAttribute.Create("_WordsToExtend", words);

            string value = settings.ExtendHeight ? "Yes" : "No";
            ComAttribute extendHeight = _comAttribute.Create("_ExtendHeight", value);

            AttributeMethods.AppendChildren(options, typesToExtend, wordsToExtend, extendHeight);

            return options;
        }

        /// <summary>
        /// Creates a metadata attribute with the value of all properties in
        /// <see paramref="settings"/>.
        /// </summary>
        /// <param name="settings">The settings object whose values should be written to a
        /// metadata <see cref="ComAttribute"/>.</param>
        /// <returns>A metadata attribute describing the <see paramref="settings"/>.
        /// </returns>
        ComAttribute CreateGenericOptionsAttribute(object settings)
        {
            ComAttribute optionsAttribute = _comAttribute.Create(Constants.OptionsMetadata);

            List<ComAttribute> options = new List<ComAttribute>();
            foreach (PropertyInfo property in (settings.GetType().GetProperties()))
            {
                ComAttribute attribute = _comAttribute.Create("_" + property.Name,
                    property.GetValue(settings, null).AsString());
                options.Add(attribute);
            }

            AttributeMethods.AppendChildren(optionsAttribute, options.ToArray());

            return optionsAttribute;
        }

        /// <summary>
        /// Gets the data types to extend from the <see cref="SurroundContextSettings"/> object.
        /// </summary>
        /// <param name="settings">The <see cref="SurroundContextSettings"/> object from which to 
        /// get data types.</param>
        /// <returns>The data types to extend.</returns>
        static string GetTypesToExtend(SurroundContextSettings settings)
        {
            if (settings.ExtendAllTypes)
            {
                return "All types";
            }

            string[] types = settings.GetDataTypes();
            return string.Join(", ", types);
        }

        /// <summary>
        /// Creates an entries changed attribute.
        /// </summary>
        /// <param name="name">The name of entries changed attribute.</param>
        /// <param name="entries">The attributes that changed.</param>
        /// <returns>An entries changed attribute.</returns>
        ComAttribute CreateChangeEntriesAttribute(string name, ICollection<RedactionItem> entries)
        {
            ComAttribute changeEntries = _comAttribute.Create(name);

            // Append entries if necessary
            if (entries.Count > 0)
            {
                IUnknownVector subAttributes = changeEntries.SubAttributes;
                foreach (RedactionItem entry in entries)
                {
                    ComAttribute changeEntry = entry.GetIdAttribute();
                    ICopyableObject clone = (ICopyableObject)changeEntry;
                    subAttributes.PushBack((clone == null) ? changeEntry : clone.Clone());
                }
            }

            return changeEntries;
        }

        /// <summary>
        /// Adds the specified revisions to the revisions attributes.
        /// </summary>
        /// <param name="revisions">The revisions attribute.</param>
        /// <param name="oldDeleted">Previous versions of deleted attributes.</param>
        /// <param name="oldModified">Previous versions of modified attributes.</param>
        /// <param name="oldNonOutput">Previous versions of the attributes marked to not be 
        /// output.</param>
        void AddRevisions(ComAttribute revisions, IEnumerable<RedactionItem> oldDeleted,
            IEnumerable<RedactionItem> oldModified, IEnumerable<RedactionItem> oldNonOutput)
        {
            IUnknownVector subAttributes = revisions.SubAttributes;
            foreach (RedactionItem deleted in oldDeleted)
            {
                subAttributes.PushBack(deleted.ComAttribute);
            }
            foreach (RedactionItem modified in oldModified)
            {
                subAttributes.PushBack(modified.ComAttribute);
            }
            foreach (RedactionItem nonOutput in oldNonOutput)
            {
                ComAttribute attribute = CreateNonOutputAttribute(nonOutput);
                subAttributes.PushBack(attribute);
            }
        }

        /// <summary>
        /// Creates a attribute that is marked for non-output from the specified redaction item.
        /// </summary>
        /// <param name="item">The item for which a non-output attribute should be created.</param>
        /// <returns>An attribute that is marked for non-output from the specified 
        /// <paramref name="item"/>.</returns>
        ComAttribute CreateNonOutputAttribute(RedactionItem item)
        {
            // Copy the original attribute so other references to this object are not changed
            ICloneIdentifiableObject copy = (ICloneIdentifiableObject)item.ComAttribute;
            ComAttribute result = (ComAttribute) copy.CloneIdentifiableObject();

            // Append the archive action attribute to this attribute
            ComAttribute archiveAction =
                _comAttribute.Create(Constants.ArchiveAction, Constants.TurnedOff);
            AttributeMethods.AppendChildren(result, archiveAction);

            return result;
        }

        /// <summary>
        /// Gets the vector of attributes from combining the verified items with the metadata 
        /// attributes.
        /// </summary>
        /// <param name="verified">The items that were verified.</param>
        /// <param name="metadata">The COM metadata attributes.</param>
        /// <returns>A vector containing the attributes from <paramref name="verified"/> and 
        /// <paramref name="metadata"/>.</returns>
        static IUnknownVector GetOutputVector(IEnumerable<SensitiveItem> verified, 
            IUnknownVector metadata)
        {
            // Add the verified attributes to the output vector
            IUnknownVector output = new IUnknownVector();
            foreach (ComAttribute attribute in verified
                .Where(redactionItem => redactionItem.Attribute.Redacted)                
                .Select(redactionItem => redactionItem.Attribute.ComAttribute))
            {
                output.PushBack(attribute);
            }

            // Append the metadata attributes
            output.Append(metadata);

            return output;
        }

        /// <summary>
        /// Gets a list of the COM attributes in the specified vector of attributes.
        /// </summary>
        /// <param name="vector">The vector of COM attributes.</param>
        /// <returns>A list of COM attributes in <paramref name="vector"/></returns>
        static List<ComAttribute> GetAttributesFromVector(IUnknownVector vector)
        {
            // Prepare a list for the result
            int count = vector.Size();
            List<ComAttribute> attributes = new List<ComAttribute>(count);

            // Add each attribute to the result
            for (int i = 0; i < count; i++)
            {
                ComAttribute attribute = (ComAttribute)vector.At(i);
                attributes.Add(attribute);
            }

            return attributes;
        }

        /// <summary>
        /// Gets the visited pages as zero relative collection.
        /// </summary>
        /// <returns>returns a VisitedItemsCollection of bools that correspond to zero-relative
        /// page indexes, true for pages that have been visited, false otherwise</returns>
        public VisitedItemsCollection VisitedPagesAsZeroRelativeCollection()
        {
            try
            {
                var pages = VisitedPages;
                var pagesZeroRelative = pages.Select(page => page - 1);
                VisitedItemsCollection visitedPages = new VisitedItemsCollection(_numberOfDocumentPages);
                foreach (int index in pagesZeroRelative)
                {
                    visitedPages[index] = true;
                }

                return visitedPages;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39847");
            }
        }

        /// <summary>
        /// Get the list of visited pages from the specified session attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        static List<int> VisitedPagesFromSessionAttribute(ComAttribute attribute)
        {
            try
            {
                var visitedPages = AttributeMethods.GetSingleAttributeByName(attribute.SubAttributes,
                                                                             Constants.VisitedPagesMetaDataName);
                if (null == visitedPages)
                {
                    return new List<int>();
                }

                var pages = visitedPages.Value.String;
                if (String.IsNullOrWhiteSpace(pages))
                {
                    return new List<int>();
                }

                UtilityMethods.ValidatePageNumbers(pages);
                var pagesAsInts = UtilityMethods.GetSortedPageNumberFromString(pages,
                                                                          totalPages: Int32.MaxValue,
                                                                          throwExceptionOnPageOutOfRange: false);
                return pagesAsInts.ToList();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39839");
            }
        }

        /// <summary>
        /// Gets the pages visited during all verification sessions.
        /// </summary>
        /// <returns>list of all visited pages</returns>
        List<int> GetPagesVisitedDuringAllSessions()
        {
            List<int> allVisitedPages = new List<int>();

            for (int i = 1; i <= _verificationSessions.Count(); ++i)
            {
                var attr = GetVerificationSession(i);
                if (null == attr)
                {
                    continue;
                }

                allVisitedPages.AddRange(VisitedPagesFromSessionAttribute(attr));
            }

            return allVisitedPages.Distinct().ToList();
        }

        /// <summary>
        /// Haves all pages been visited?
        /// This method walks through ALL of the verification sessions to determine this.
        /// </summary>
        /// <returns>true if all pages have been visited in one or more verification sessions</returns>
        bool HaveAllPagesBeenVisited()
        {
            return _numberOfDocumentPages == NumberOfVisitedPages();
        }

        /// <summary>
        /// Gets the number of visited pages.
        /// </summary>
        /// <returns>number of visited pages</returns>
        int NumberOfVisitedPages()
        {
            return GetPagesVisitedDuringAllSessions().Count;
        }

        /// <summary>
        /// Determines whether the document has been verified previously. Opening the document will always
        /// signal that the first item and the first page have been visited, so here items are only 
        /// considered to be visited if more than the first has been marked as visited. 
        /// </summary>
        /// <returns>true if document has been verified before, else false</returns>
        public bool DocumentHasBeenVerifiedPreviously()
        {
            try
            {
                if (0 == _verificationSessions.Count())
                {
                    return false;
                }

                var itemsVisited = (_sensitiveItems.Where(item => true == item.PriorVerificationVisitedThis)).Count();
                return itemsVisited > 1 || (_numberOfDocumentPages > 1 && VisitedPages.Count > 1);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39841");
            }
        }

        /// <summary>
        /// Gets the index of the last visited sensitive item.
        /// </summary>
        /// <returns>row index value, wrapped in a ReadOnlyCollection of int, for ease-of-use.
        /// NOTE that if there are no sensitive items, then index is adjusted to zero so that the 
        /// collection can be used as-is to safely select a gridview row.</returns>
        public ReadOnlyCollection<int> IndexOfLastVisitedSensitiveItem()
        {
            try
            {
                int index = _sensitiveItems.FindLastIndex(item => item.PriorVerificationVisitedThis);

                const int noItemsVisited = -1;
                index = noItemsVisited == index ? 0 : index;

                List<int> selections = new List<int>();
                selections.Add(index);

                return new ReadOnlyCollection<int>(selections);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39848");
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// This is a data transfer class, just to package and move case session context data easily.
    /// </summary>
    public class SessionContext
    {
        /// <summary>
        /// Gets or sets the visited redactions.
        /// </summary>
        /// <value>
        /// The visited redactions.
        /// </value>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<int> VisitedRedactions { get; private set; }

        /// <summary>
        /// Gets or sets the visited pages.
        /// </summary>
        /// <value>
        /// The visited pages.
        /// </value>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<int> VisitedPages { get; private set; }

        /// <summary>
        /// Gets or sets the selected redaction items.
        /// </summary>
        /// <value>
        /// The selected redaction items.
        /// </value>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<int> SelectedRedactionItems { get; private set; }

        /// <summary>
        /// Gets or sets the index of the current page.
        /// </summary>
        /// <value>
        /// The index of the current page.
        /// </value>
        public int CurrentPageIndex { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionContext"/> class.
        /// </summary>
        /// <param name="visitedRedactions">The visited redactions.</param>
        /// <param name="visitedPages">The visited pages.</param>
        /// <param name="selectedRedactionItems">The selected redaction items.</param>
        /// <param name="currentPageIndex">Index of the current page.</param>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public SessionContext(List<int> visitedRedactions,
                              List<int> visitedPages,
                              List<int> selectedRedactionItems,
                              int currentPageIndex)
        {
            VisitedRedactions = visitedRedactions;
            VisitedPages = visitedPages;
            SelectedRedactionItems = selectedRedactionItems;
            CurrentPageIndex = currentPageIndex;
        }
    }
}
