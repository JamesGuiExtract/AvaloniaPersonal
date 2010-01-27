using Extract.AttributeFinder;
using Extract.Licensing;
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
        /// The confidence level for manual redactions.
        /// </summary>
        readonly ConfidenceLevel _manual;

        /// <summary>
        /// The name of the vector of attributes (VOA) file.
        /// </summary>
        string _fileName;

        /// <summary>
        /// The name of the source document.
        /// </summary>
        string _sourceDocument;

        /// <summary>
        /// The type of the document; or <see langword="null"/> if the document is uncategorized.
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
        List<ComAttribute> _attributes;

        /// <summary>
        /// The previous verification sessions.
        /// </summary>
        ComAttribute[] _verificationSessions;

        /// <summary>
        /// The unique verification session id.
        /// </summary>
        int _sessionId;

        /// <summary>
        /// The previous redaction sessions.
        /// </summary>
        ComAttribute[] _redactionSessions;

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
            _manual = GetManualConfidenceLevel(levels);
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
        /// Gets the <see cref="ConfidenceLevel"/> associated with manual redactions.
        /// </summary>
        /// <value>The <see cref="ConfidenceLevel"/> associated with manual redactions.</value>
        public ConfidenceLevel ManualConfidenceLevel
        {
            get
            {
                return _manual;
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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Get the confidence level associated with manual redactions.
        /// </summary>
        /// <param name="levels">The valid confidence levels.</param>
        /// <returns>The confidence level associated with manual redactions.</returns>
        static ConfidenceLevel GetManualConfidenceLevel(IEnumerable<ConfidenceLevel> levels)
        {
            foreach (ConfidenceLevel level in levels)
            {
                if (level.ShortName == "Man")
                {
                    return level;
                }
            }

            return null;
        }

        /// <overloads>Loads the contents of the voa file from the specified file.</overloads>
        /// <summary>
        /// Loads the contents of the voa file from the specified file.
        /// </summary>
        /// <param name="fileName">The vector of attributes (VOA) file to load.</param>
        /// <param name="sourceDocument">The source document corresponding to the 
        /// <paramref name="fileName"/>.</param>
        public void LoadFrom(string fileName, string sourceDocument)
        {
            LoadFrom(fileName, sourceDocument, true);
        }

        /// <summary>
        /// Loads the contents of the voa file from the specified file.
        /// </summary>
        /// <param name="fileName">The vector of attributes (VOA) file to load.</param>
        /// <param name="sourceDocument">The source document corresponding to the 
        /// <paramref name="fileName"/>.</param>
        /// <param name="toggleOffRedactions"><see langword="true"/> if non-output redactions 
        /// should be toggled off; <see langword="false"/> if non-output redactions should remain 
        /// in their current state.</param>
        public void LoadFrom(string fileName, string sourceDocument, bool toggleOffRedactions)
        {
            try
            {
                _sensitiveItems = new List<SensitiveItem>();
                _attributes = new List<ComAttribute>();
                _sessionId = 0;
                _nextId = 1;

                _fileName = fileName;
                _sourceDocument = sourceDocument;

                if (File.Exists(fileName))
                {
                    // Load the attributes from the file
                    LoadVoa(fileName, toggleOffRedactions);
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
        /// <param name="toggleOffRedactions"><see langword="true"/> if non-output redactions 
        /// should be toggled off; <see langword="false"/> if non-output redactions should remain 
        /// in their current state.</param>
        void LoadVoa(string fileName, bool toggleOffRedactions)
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
                AddAttributes(current, level, toggleOffRedactions);

                IUnknownVector previous = utility.QueryAttributes(revisions, level.Query, false);
                AddNonOutputAttributes(previous, level, revisions);
            }

            // Ensure all sensitive items have attribute ids
            foreach (SensitiveItem item in _sensitiveItems)
            {
                AssignId(item.Attribute);
            }

            // Determine the next attribute id
            _nextId = GetNextId(_sensitiveItems, _revisionsAttribute);

            // Get the previous verification sessions
            _verificationSessions = AttributeMethods.GetAttributesByName(attributes, "_VerificationSession");
            _sessionId = GetSessionId(_verificationSessions);

            // Get the previous redaction session
            _redactionSessions = AttributeMethods.GetAttributesByName(attributes, "_RedactedFileOutputSession");

            // Store the remaining attributes
            int count = attributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute attribute = (ComAttribute)attributes.At(i);
                _attributes.Add(attribute);
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
            ComAttribute fileInfo = CreateComAttribute("_VOAFileInfo");

            // Product name
            ComAttribute productNameAttribute = CreateComAttribute("_ProductName", "IDShield");

            // Schema version
            string version = _VERSION.ToString(CultureInfo.CurrentCulture);
            ComAttribute schemaVersion = CreateComAttribute("_SchemaVersion", version);

            AppendChildAttributes(fileInfo, productNameAttribute, schemaVersion);

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
        /// <param name="toggleOffRedactions"><see langword="true"/> if non-output redactions 
        /// should be toggled off; <see langword="false"/> if non-output redactions should remain 
        /// in their current state.</param>
        void AddAttributes(IUnknownVector attributes, ConfidenceLevel level, 
            bool toggleOffRedactions)
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
                    SensitiveItem item = new SensitiveItem(level, attribute);
                    if (toggleOffRedactions && !level.Output)
                    {
                        item.Attribute.Redacted = false;
                    }

                    _sensitiveItems.Add(item);
                }
                else
                {
                    _attributes.Add(attribute);
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
        /// Adds an unique id attribute to the specified attribute if it does not already contain 
        /// one.
        /// </summary>
        /// <param name="attribute">The attribute to which an id may be added.</param>
        void AssignId(RedactionItem attribute)
        {
            bool idAdded = attribute.AssignIdIfNeeded(_nextId, _sourceDocument);
            if (idAdded)
            {
                _nextId++;
            }
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
                revisionsAttribute = CreateComAttribute("_OldRevisions");
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
                // Calculate the new items to verify
                List<SensitiveItem> itemsToVerify = new List<SensitiveItem>(_sensitiveItems);

                // Update any changed items (deleted, modified, and added)
                RedactionItem[] oldDeleted = UpdateDeletedItems(itemsToVerify, changes.Deleted);
                RedactionItem[] oldModified = UpdateModifiedItems(itemsToVerify, changes.Modified);
                UpdateAddedItems(itemsToVerify, changes.Added);

                // Get the non output redactions
                RedactionItem[] oldNonOutput = GetNonOutputRedactions(itemsToVerify);

                // Create other verification attributes that don't undergo verification
                IUnknownVector attributes = GetUnverifiedAttributes(settings, time, changes, 
                    oldDeleted, oldModified, oldNonOutput);

                // Save the attributes
                IUnknownVector output = GetOutputVector(itemsToVerify, attributes);
                output.SaveTo(fileName, false);

                // Update the items to verify
                _sensitiveItems = itemsToVerify;

                // Update the attributes that don't need verification
                _attributes = GetAttributesFromVector(attributes);

                // Update the session id
                _sessionId++;
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
                SensitiveItem item = new SensitiveItem(_manual, attribute);
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
        static RedactionItem[] GetNonOutputRedactions(List<SensitiveItem> redactions)
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
        /// Creates all the attributes in a verification vector of attributes (VOA) file, other 
        /// than the ones that were directly verified by the user.
        /// </summary>
        /// <param name="settings">The settings used during verification.</param>
        /// <param name="time">The amount of screen time spent verifying.</param>
        /// <param name="changes">The changes made to the original verification file.</param>
        /// <param name="oldDeleted">The previous version of the deleted attributes.</param>
        /// <param name="oldModified">The previous version of the modified attributes.</param>
        /// <param name="oldNonOutput">The previous version of the attributes that are marked to 
        /// not be output.</param>
        /// <returns>All the attributes in a verification vector of attributes (VOA) file, other 
        /// than the ones that were directly verified by the user.</returns>
        IUnknownVector GetUnverifiedAttributes(VerificationSettings settings, TimeInterval time,
            RedactionFileChanges changes, RedactionItem[] oldDeleted, RedactionItem[] oldModified, 
            RedactionItem[] oldNonOutput)
        {
            // Copy the original unverified attributes in to a vector
            IUnknownVector attributes = new IUnknownVector();
            foreach (ComAttribute attribute in _attributes)
            {
                ICopyableObject copy = (ICopyableObject)attribute;
                attributes.PushBack(copy.Clone());    
            }

            // Add session attribute
            ComAttribute session = CreateSessionAttribute(time, settings, changes);
            attributes.PushBack(session);

            // Copy and update the revision attribute
            ComAttribute revisions = GetRevisionsAttribute(attributes);
            AddRevisions(revisions, oldDeleted, oldModified, oldNonOutput);

            return attributes;
        }

        /// <summary>
        /// Creates a new session attribute.
        /// </summary>
        /// <param name="screenTime">The duration of time spent during verification.</param>
        /// <param name="settings">The verification settings used.</param>
        /// <param name="changes">The changes made during verification.</param>
        /// <returns>A new session attribute.</returns>
        ComAttribute CreateSessionAttribute(TimeInterval screenTime, VerificationSettings settings,
            RedactionFileChanges changes)
        {
            // User and screenTime information
            ComAttribute user = CreateUserAttribute();
            ComAttribute time = CreateScreenTimeAttribute(screenTime);

            // File information
            ComAttribute source = CreateComAttribute("_SourceDocName", _sourceDocument);
            ComAttribute data = CreateComAttribute("_IDShieldDataFile", _fileName);

            // Verification options
            ComAttribute options = CreateVerificationOptionsAttribute(settings);

            // Entries changed
            ComAttribute added = CreateChangeEntriesAttribute("_EntriesAdded", changes.Added);
            ComAttribute deleted = CreateChangeEntriesAttribute("_EntriesDeleted", changes.Deleted);
            ComAttribute modified = CreateChangeEntriesAttribute("_EntriesModified", changes.Modified);

            // Session attribute
            ComAttribute session = CreateComAttribute("_VerificationSession", _sessionId + 1);
            AppendChildAttributes(session, user, time, source, data, options, added, deleted, modified);

            return session;
        }

        /// <summary>
        /// Creates the user information attribute.
        /// </summary>
        /// <returns>The user information attribute.</returns>
        ComAttribute CreateUserAttribute()
        {
            ComAttribute userInfo = CreateComAttribute("_UserInfo");
            ComAttribute loginId = CreateComAttribute("_LoginID", Environment.UserName);
            ComAttribute computerName = CreateComAttribute("_Computer", Environment.MachineName);
            AppendChildAttributes(userInfo, loginId, computerName);

            return userInfo;
        }

        /// <summary>
        /// Creates the verification time attribute.
        /// </summary>
        /// <param name="time">The duration of time spent verifying the document.</param>
        /// <returns>The verification time attribute.</returns>
        ComAttribute CreateScreenTimeAttribute(TimeInterval time)
        {
            ComAttribute timeInfo = CreateComAttribute("_TimeInfo");
            ComAttribute date = CreateComAttribute("_Date", time.Start.ToShortDateString());
            ComAttribute timeStarted = CreateComAttribute("_TimeStarted", time.Start.ToLongTimeString());
            ComAttribute totalSeconds = CreateComAttribute("_TotalSeconds", time.ElapsedSeconds);
            AppendChildAttributes(timeInfo, date, timeStarted, totalSeconds);

            return timeInfo;
        }

        /// <summary>
        /// Creates the verification options attribute.
        /// </summary>
        /// <param name="settings">The settings used during verification.</param>
        /// <returns>The verification options attribute.</returns>
        ComAttribute CreateVerificationOptionsAttribute(VerificationSettings settings)
        {
            ComAttribute options = CreateComAttribute("_VerificationOptions");
            string value = settings.General.VerifyAllPages ? "Yes" : "No";
            ComAttribute verifyAllPages = CreateComAttribute("_VerifyAllPages", value);
            AppendChildAttributes(options, verifyAllPages);

            return options;
        }

        /// <summary>
        /// Creates an entries changed attribute.
        /// </summary>
        /// <param name="name">The name of entries changed attribute.</param>
        /// <param name="entries">The attributes that changed.</param>
        /// <returns>An entries changed attribute.</returns>
        ComAttribute CreateChangeEntriesAttribute(string name, ICollection<RedactionItem> entries)
        {
            ComAttribute changeEntries = CreateComAttribute(name);

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
            ComAttribute archiveAction = CreateComAttribute("ArchiveAction", "TurnedOff");
            AppendChildAttributes(result, archiveAction);

            return result;
        }

        /// <summary>
        /// Gets the vector of attributes from combining the verified items with the unverified 
        /// items.
        /// </summary>
        /// <param name="verified">The items that were verified.</param>
        /// <param name="unverified">The attributes that were not verified.</param>
        /// <returns>A vector containing the attributes from <paramref name="verified"/> and 
        /// <paramref name="unverified"/>.</returns>
        static IUnknownVector GetOutputVector(IEnumerable<SensitiveItem> verified, 
            IUnknownVector unverified)
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

            // Append the unverified attributes
            output.Append(unverified);

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

        /// <summary>
        /// Creates a COM attribute with the specified name.
        /// </summary>
        /// <param name="name">The name of the COM attribute to create.</param>
        /// <returns>A COM attribute with the specified <paramref name="name"/>.</returns>
        ComAttribute CreateComAttribute(string name)
        {
            return CreateComAttribute(name, null, null);
        }

        /// <summary>
        /// Creates a COM attribute with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the COM attribute to create.</param>
        /// <param name="value">The value of the COM attribute to create. Will be converted to a 
        /// string.</param>
        /// <returns>A COM attribute with the specified <paramref name="name"/> and 
        /// <paramref name="value"/>.</returns>
        ComAttribute CreateComAttribute(string name, IConvertible value)
        {
            return CreateComAttribute(name, value, null);
        }

        /// <summary>
        /// Creates a COM attribute with the specified name, value, and type.
        /// </summary>
        /// <param name="name">The name of the COM attribute to create.</param>
        /// <param name="value">The value of the COM attribute to create. Will be converted to a 
        /// string.</param>
        /// <param name="type">The type of the COM attribute to create.</param>
        /// <returns>A COM attribute with the specified <paramref name="name"/>, 
        /// <paramref name="value"/>, and <paramref name="type"/>.</returns>
        ComAttribute CreateComAttribute(string name, IConvertible value, string type)
        {
            // Create an attribute with the specified name
            ComAttribute attribute = new ComAttribute();
            attribute.Name = name;

            // Set the value if specified
            attribute.Value = CreateNonSpatialString(value ?? "");

            // Set the type if specified
            if (type != null)
            {
                attribute.Type = type;
            }

            return attribute;
        }

        /// <summary>
        /// Creates a non-spatial string with the specified value.
        /// </summary>
        /// <param name="value">The value of the non-spatial string to create. Will be converted 
        /// to a string.</param>
        /// <returns>A non-spatial string with the specified <paramref name="value"/>.</returns>
        SpatialString CreateNonSpatialString(IConvertible value)
        {
            SpatialString spatialString = new SpatialString();
            string text = value.ToString(CultureInfo.CurrentCulture);
            spatialString.CreateNonSpatialString(text, _sourceDocument);

            return spatialString;
        }

        /// <summary>
        /// Appends attributes as children of the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute to which attributes should be appended.</param>
        /// <param name="attributesToAppend">The attributes to append as children to 
        /// <paramref name="attribute"/>.</param>
        static void AppendChildAttributes(ComAttribute attribute, 
            params ComAttribute[] attributesToAppend)
        {
            IUnknownVector subAttributes = attribute.SubAttributes;
            foreach (ComAttribute append in attributesToAppend)
            {
                subAttributes.PushBack(append);
            }
        }

        #endregion Methods
    }
}
