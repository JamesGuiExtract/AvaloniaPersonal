using Extract.AttributeFinder;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
        /// The old revisions COM attribute.
        /// </summary>
        ComAttribute _revisionsAttribute;

        /// <summary>
        /// All non-sensitive and metadata attributes.
        /// </summary>
        List<ComAttribute> _metadata;

        /// <summary>
        /// The previous verification sessions.
        /// </summary>
        ComAttribute[] _verificationSessions;

        /// <summary>
        /// The unique verification session id.
        /// </summary>
        int _verificationSessionId;

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

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionFileLoader"/> class.
        /// </summary>
        public RedactionFileLoader(ConfidenceLevelsCollection levels)
        {
            LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldVerificationObject,
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

        #endregion Properties

        #region Methods

        /// <overloads>Loads the contents of the voa file from the specified file.</overloads>
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
                _sensitiveItems = new List<SensitiveItem>();
                _metadata = new List<ComAttribute>();
                _verificationSessionId = 0;
                _nextId = 1;

                _fileName = fileName;
                _sourceDocument = sourceDocument;
                _comAttribute = new AttributeCreator(sourceDocument);

                if (File.Exists(fileName))
                {
                    // Load the attributes from the file
                    LoadVoa(fileName);
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
        /// <param name="fileName">The vector of attributes (VOA) file to load.</param>
        void LoadVoa(string fileName)
        {
            // Load the attributes from the file
            IUnknownVector attributes = new IUnknownVector();
            attributes.LoadFrom(fileName, false);

            // Ensure the schema of this VOA file is accurate
            ValidateSchema(attributes);

            // Get the document type
            _documentType = GetDocumentType(attributes);

            // Get the previous revision
            _revisionsAttribute = GetRevisionsAttribute(attributes);
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

            // Ensure all sensitive items have attribute ids and are in their initial toggled state
            InitializeSensitiveItems(_sensitiveItems);

            // Determine the next attribute id
            _nextId = GetNextId(_sensitiveItems, _revisionsAttribute);

            // Get the previous verification sessions
            _verificationSessions = AttributeMethods.GetAttributesByName(attributes, "_VerificationSession");
            _verificationSessionId = GetSessionId(_verificationSessions);

            // Get the previous redaction sessions
            _redactionSessions = AttributeMethods.GetAttributesByName(attributes, "_RedactedFileOutputSession");

            // Get the previous surround context sessions
            _surroundContextSessions = AttributeMethods.GetAttributesByName(attributes,
                "_SurroundContextSession");
            _surroundContextSessionId = GetSessionId(_surroundContextSessions);

            // Store the remaining attributes
            int count = attributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute attribute = (ComAttribute)attributes.At(i);
                _metadata.Add(attribute);
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

                // This is a non output attribute if its archive action is turned off.
                IUnknownVector subAttributes = attribute.SubAttributes;
                ComAttribute[] archiveAction = 
                    AttributeMethods.GetAttributesByName(subAttributes, "ArchiveAction");
                if (archiveAction.Length == 1 && IsTurnedOffArchiveAction(archiveAction[0]))
                {
                    // Remove the archive action
                    subAttributes.RemoveValue(archiveAction[0]);

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
        /// Determines whether the specified archive action is marked as 'turned off'.
        /// </summary>
        /// <param name="archiveAction">The archive action attribute to test.</param>
        /// <returns><see langword="true"/> if <paramref name="archiveAction"/> is marked as 
        /// 'turned off'; <see langword="false"/> if <paramref name="archiveAction"/> is not 
        /// marked as 'turned off'.</returns>
        static bool IsTurnedOffArchiveAction(ComAttribute archiveAction)
        {
            if (archiveAction == null)
            {
                return false;
            }

            SpatialString value = archiveAction.Value;
            return string.Equals(value.String, "TurnedOff", StringComparison.OrdinalIgnoreCase);
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
                if (assigned)
                {
                    // Set the toggled state if this is the first time the attribute was loaded
                    // [FIDSC #4026]
                    item.Attribute.Redacted = item.Level.Output;
                }
            }
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
        /// <returns>The revisions attribute for <paramref name="vector"/>; if no such attribute 
        /// exists, it is created and added to <paramref name="vector"/>.</returns>
        ComAttribute GetRevisionsAttribute(IUnknownVector vector)
        {
            // Look for an existing revisions attribute
            ComAttribute revisionsAttribute = AttributeMethods.GetSingleAttributeByName(vector, "_OldRevisions");
            if (revisionsAttribute == null)
            {
                // Create a new revisions attribute
                revisionsAttribute = _comAttribute.Create("_OldRevisions");
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
        [CLSCompliant(false)]
        public void SaveVerificationSession(string fileName, RedactionFileChanges changes, 
            TimeInterval time, VerificationSettings settings)
        {
            try
            {
                ComAttribute sessionData = CreateVerificationOptionsAttribute(settings);

                // Calculate the new sensitive items
                SaveSession("_VerificationSession", _verificationSessionId, 
                    fileName, changes, time, sessionData);

                // Update the session id
                _verificationSessionId++;
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
                SaveSession("_SurroundContextSession", _surroundContextSessionId,
                    fileName, changes, time, sessionData);

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
        /// Modifies the VOA file and appends a session metadata COM attribute.
        /// </summary>
        /// <param name="sessionName">The name of the session metadata COM attribute.</param>
        /// <param name="lastSessionId">The unique ID of the previous session.</param>
        /// <param name="fileName">The full path of the VOA file to save.</param>
        /// <param name="changes">The changes made to the original VOA file.</param>
        /// <param name="time">The time elapsed during the session being saved.</param>
        /// <param name="sessionData">Additional session data to add as a sub attribute.</param>
        void SaveSession(string sessionName, int lastSessionId, string fileName, 
            RedactionFileChanges changes, TimeInterval time, ComAttribute sessionData)
        {
            List<SensitiveItem> sensitiveItems = new List<SensitiveItem>(_sensitiveItems);

            // Update any changed items (deleted, modified, and added)
            IUnknownVector attributes = GetChangedAttributes(sensitiveItems, changes);

            // Create session attribute
            ComAttribute session = 
                CreateSessionAttribute(sessionName, lastSessionId, time);

            // Append additional session information
            AttributeMethods.AppendChildren(session, sessionData);

            // Append change entries
            AppendChangeEntries(session, changes);
            attributes.PushBack(session);

            // Save the attributes
            SaveAttributesTo(sensitiveItems, attributes, fileName);
        }

        /// <summary>
        /// Gets the changes made to the VOA file.
        /// </summary>
        /// <param name="sensitiveItems">The sensitive items after changes were made.</param>
        /// <param name="changes">The changes that were made.</param>
        /// <returns>The attributes after changes are made.</returns>
        IUnknownVector GetChangedAttributes(List<SensitiveItem> sensitiveItems, RedactionFileChanges changes)
        {
            RedactionItem[] oldDeleted = UpdateDeletedItems(sensitiveItems, changes.Deleted);
            RedactionItem[] oldModified = UpdateModifiedItems(sensitiveItems, changes.Modified);
            UpdateAddedItems(sensitiveItems, changes.Added);

            // Get the non output redactions
            RedactionItem[] oldNonOutput = GetNonOutputRedactions(sensitiveItems);

            // Copy the original metadata attributes in to a vector
            IUnknownVector attributes = CopyMetadataAttributes();

            // Update the revisions attribute
            ComAttribute revisions = GetRevisionsAttribute(attributes);
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
            output.SaveTo(fileName, false);

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
        static RedactionItem[] UpdateDeletedItems(IList<SensitiveItem> items,
            ICollection<RedactionItem> deleted)
        {
            // Make an array to hold the previous version of the modified attributes
            RedactionItem[] oldDeleted = new RedactionItem[deleted.Count];

            // Iterate over each deleted attribute
            int i = 0;
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
                oldDeleted[i] = items[index].Attribute;
                i++;

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
        static RedactionItem[] UpdateModifiedItems(IList<SensitiveItem> items,
            ICollection<RedactionItem> modified)
        {
            // Make an array to hold the previous version of the modified attributes
            RedactionItem[] oldModified = new RedactionItem[modified.Count];

            // Iterate over each modified attribute
            int i = 0;
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
                oldModified[i] = old.Attribute;
                i++;

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
        static RedactionItem[] GetNonOutputRedactions(IEnumerable<SensitiveItem> redactions)
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

            return nonOutputRedactions.ToArray();
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
                ICopyableObject copy = (ICopyableObject)attribute;
                attributes.PushBack(copy.Clone());
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
            ComAttribute source = _comAttribute.Create("_SourceDocName", _sourceDocument);
            ComAttribute data = _comAttribute.Create("_IDShieldDataFile", _fileName);

            // Session attribute
            ComAttribute session = _comAttribute.Create(sessionName, lastSessionId + 1);
            AttributeMethods.AppendChildren(session, user, time, source, data);

            return session;
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
            ComAttribute timeStarted = _comAttribute.Create("_TimeStarted", time.Start.ToLongTimeString());
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
            ComAttribute options = _comAttribute.Create("_Options");
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
                    subAttributes.PushBack(changeEntry);
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
            IEnumerable<RedactionItem> oldModified, RedactionItem[] oldNonOutput)
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
            ICopyableObject copy = (ICopyableObject) item.ComAttribute;
            ComAttribute result = (ComAttribute) copy.Clone();

            // Append the archive action attribute to this attribute
            ComAttribute archiveAction = _comAttribute.Create("ArchiveAction", "TurnedOff");
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
            foreach (SensitiveItem item in verified)
            {
                RedactionItem redactionItem = item.Attribute;
                if (redactionItem.Redacted)
                {
                    output.PushBack(redactionItem.ComAttribute);
                }
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
                ComAttribute attribute = (ComAttribute) vector.At(i);
                attributes.Add(attribute);
            }

            return attributes;
        }

        #endregion Methods
    }
}
