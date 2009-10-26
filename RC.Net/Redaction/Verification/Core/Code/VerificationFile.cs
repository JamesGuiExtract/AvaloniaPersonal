using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Extract.Licensing;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents the contents of a vector of attributes (VOA) file used for verification.
    /// </summary>
    public class VerificationFile
    {
        #region Constants

        /// <summary>
        /// The current vector of attributes (VOA) schema version.
        /// </summary>
        static readonly int _VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The confidence levels of attributes in the <see cref="RedactionGridView"/>.
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
        /// All items queried for verification.
        /// </summary>
        List<VerificationItem> _itemsToVerify;

        /// <summary>
        /// All attributes that are not being verified.
        /// </summary>
        List<ComAttribute> _attributes;

        /// <summary>
        /// The unique session id.
        /// </summary>
        int _sessionId;

        /// <summary>
        /// The unique id of the next created attribute.
        /// </summary>
        long _nextId;

        /// <summary>
        /// Validates the license state.
        /// </summary>
        static readonly LicenseStateCache _license = 
            new LicenseStateCache(LicenseIdName.IDShieldVerificationObject, "Verification file");

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationFile"/> class.
        /// </summary>
        public VerificationFile(ConfidenceLevelsCollection levels)
        {
            _license.Validate("ELI28215");

            _levels = levels;
            _manual = GetManualConfidenceLevel(levels);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the verifiable items that contain spatial information.
        /// </summary>
        /// <value>The verifiable items that contain spatial information.</value>
        public ReadOnlyCollection<VerificationItem> Items
        {
            get
            {
                return _itemsToVerify.AsReadOnly();
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
                _itemsToVerify = new List<VerificationItem>();
                _attributes = new List<ComAttribute>();
                _sessionId = 0;
                _nextId = 1;

                _fileName = fileName;
                _sourceDocument = sourceDocument;

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

            // Query and add attributes at each confidence level
            AFUtility utility = new AFUtility();
            foreach (ConfidenceLevel level in _levels)
            {
                IUnknownVector vector = utility.QueryAttributes(attributes, level.Query, true);
                AddAttributes(vector, level);
            }

            // Ensure all the items for verification have attribute ids
            foreach (VerificationItem item in _itemsToVerify)
            {
                AssignId(item.Attribute);
            }

            // Determine the next attribute id
            _nextId = GetNextId(_itemsToVerify, attributes);

            // Get the current session id
            _sessionId = GetSessionId(attributes);

            // Store the attributes that aren't being verified
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
            ComAttribute fileInfo = GetSingleAttributeByName(attributes, "_VOAFileInfo");
            if (fileInfo == null)
            {
                fileInfo = CreateFileInfoAttribute();
                attributes.PushBack(fileInfo);
                return;
            }

            IUnknownVector subAttributes = fileInfo.SubAttributes;

            // Validate product name
            ComAttribute productName = GetSingleAttributeByName(subAttributes, "_ProductName");
            string product = productName.Value.String;
            if (!product.Equals("IDShield", StringComparison.OrdinalIgnoreCase))
            {
                ExtractException ee = new ExtractException("ELI28198",
                    "Voa file created for different product.");
                ee.AddDebugData("Product", product, false);
                throw ee;
            }

            // Validate version
            ComAttribute schema = GetSingleAttributeByName(subAttributes, "_SchemaVersion");
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
        /// Gets a single attribute by name from the specified vector of attributes. Throws an 
        /// exception if more than one attribute is found.
        /// </summary>
        /// <param name="attributes">The attributes to search.</param>
        /// <param name="name">The name of the attribute to find.</param>
        /// <returns>The only attribute in <paramref name="attributes"/> with the specified name; 
        /// if no such attribute exists, returns <see langword="null"/>.</returns>
        static ComAttribute GetSingleAttributeByName(IUnknownVector attributes, string name)
        {
            ComAttribute[] idAttributes = GetAttributesByName(attributes, name);

            if (idAttributes.Length == 0)
            {
                return null;
            }
            else if (idAttributes.Length == 1)
            {
                return idAttributes[0];
            }

            throw new ExtractException("ELI28197",
                "More than one " + name + " attribute found.");
        }

        /// <summary>
        /// Gets an array of COM attributes that have the specified name.
        /// </summary>
        /// <param name="attributes">A vector of COM attributes to search.</param>
        /// <param name="name">The name of the attributes to return.</param>
        /// <returns>An array of COM attributes in <paramref name="attributes"/> that have the 
        /// specified <paramref name="name"/>.</returns>
        static ComAttribute[] GetAttributesByName(IUnknownVector attributes, string name)
        {
            List<ComAttribute> result = new List<ComAttribute>();

            // Iterate over each attribute
            int count = attributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute attribute = (ComAttribute)attributes.At(i);

                // If this attribute matches the specified name, add it to the result
                string attributeName = attribute.Name;
                if (attributeName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(attribute);
                }
            }

            return result.ToArray();
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
        /// Adds attributes by confidence level and stores them in <see cref="_itemsToVerify"/>.
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
                    VerificationItem item = new VerificationItem(level, attribute);
                    _itemsToVerify.Add(item);
                }
                else
                {
                    _attributes.Add(attribute);
                }
            }
        }

        /// <summary>
        /// Adds an unique id attribute to the specified attribute if it does not already contain 
        /// one.
        /// </summary>
        /// <param name="attribute">The attribute to which an id may be added.</param>
        void AssignId(ComAttribute attribute)
        {
            ComAttribute id = GetIdAttribute(attribute);
            if (id == null)
            {
                AddIdAttribute(attribute);
            }
        }

        /// <summary>
        /// Gets the id attribute associated with the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute from which to get the id attribute.</param>
        /// <returns>The id attribute associated with <paramref name="attribute"/>.</returns>
        static ComAttribute GetIdAttribute(ComAttribute attribute)
        {
            return GetSingleAttributeByName(attribute.SubAttributes, "_IDAndRevision");
        }

        /// <summary>
        /// Creates and adds an id attribute for the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute to which the id attribute should be added.</param>
        void AddIdAttribute(ComAttribute attribute)
        {
            ComAttribute revisionId = CreateComAttribute("_IDAndRevision", _nextId, "1");

            attribute.SubAttributes.PushBack(revisionId);

            _nextId++;
        }

        /// <summary>
        /// Calculates the unique id of the next created attribute.
        /// </summary>
        /// <returns>The unique id of the next created attribute.</returns>
        long GetNextId(IEnumerable<VerificationItem> items, IUnknownVector vector)
        {
            // Iterate over the items to verify for the next id.
            long nextId = 1;
            foreach (VerificationItem item in items)
            {
                nextId = GetNextId(item.Attribute, nextId);
            }

            // Iterate over all previously deleted attributes for the next id.
            ComAttribute revisions = GetRevisionsAttribute(vector);
            IUnknownVector subAttributes = revisions.SubAttributes;
            int count = subAttributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute attribute = (ComAttribute) subAttributes.At(i);
                nextId = GetNextId(attribute, nextId);
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
        static long GetNextId(ComAttribute attribute, long nextId)
        {
            long id = GetAttributeId(attribute);
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
            ComAttribute revisionsAttribute = GetSingleAttributeByName(vector, "_OldRevisions");
            if (revisionsAttribute == null)
            {
                // Create a new revisions attribute
                revisionsAttribute = CreateComAttribute("_OldRevisions");
                vector.PushBack(revisionsAttribute);
            }

            return revisionsAttribute;
        }

        /// <summary>
        /// Gets the unique id of the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute from which the unique id should be determined.
        /// </param>
        /// <returns>The unique id of <paramref name="attribute"/>; or -1 if 
        /// <paramref name="attribute"/> does not have an attribute</returns>
        static long GetAttributeId(ComAttribute attribute)
        {
            ComAttribute idAttribute = GetIdAttribute(attribute);
            if (idAttribute != null)
            {
                return long.Parse(idAttribute.Value.String, CultureInfo.CurrentCulture);
            }

            return -1;
        }

        /// <summary>
        /// Gets the id of the most recent verification session.
        /// </summary>
        /// <param name="attributes">A vector of attributes to search for the most recent 
        /// verification session.</param>
        /// <returns>The id of the most recent verification session in 
        /// <paramref name="attributes"/>; or zero if no verification session attribute exists.
        /// </returns>
        static int GetSessionId(IUnknownVector attributes)
        {
            int result = 0;

            // Iterate over each verification session attribute
            ComAttribute[] sessions = GetAttributesByName(attributes, "_VerificationSession");
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
        /// Saves the <see cref="VerificationFile"/> with the specified changes and information. 
        /// </summary>
        /// <param name="fileName">The full path to location where the file should be saved.</param>
        /// <param name="changes">The attributes that have been added, modified, and deleted.</param>
        /// <param name="time">The interval of screen time spent verifying the file.</param>
        /// <param name="settings">The settings used during verification.</param>
        [CLSCompliant(false)]
        public void SaveTo(string fileName, VerificationFileChanges changes, TimeInterval time, 
            VerificationSettings settings)
        {
            try
            {
                // Calculate the new items to verify
                List<VerificationItem> itemsToVerify = new List<VerificationItem>(_itemsToVerify);

                // Update any changed items (deleted, modified, and added)
                ComAttribute[] oldDeleted = UpdateDeletedItems(itemsToVerify, changes.Deleted);
                ComAttribute[] oldModified = UpdateModifiedItems(itemsToVerify, changes.Modified);
                UpdateAddedItems(itemsToVerify, changes.Added);

                // Create other verification attributes that don't undergo verification
                IUnknownVector attributes = 
                    GetUnverifiedAttributes(settings, time, changes, oldDeleted, oldModified);

                // Save the attributes
                IUnknownVector output = GetOutputVector(itemsToVerify, attributes);
                output.SaveTo(fileName, false);

                // Update the items to verify
                _itemsToVerify = itemsToVerify;

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
        static ComAttribute[] UpdateDeletedItems(IList<VerificationItem> items,
            ICollection<ComAttribute> deleted)
        {
            // Make an array to hold the previous version of the modified attributes
            ComAttribute[] oldDeleted = new ComAttribute[deleted.Count];

            // Iterate over each deleted attribute
            int i = 0;
            foreach (ComAttribute attribute in deleted)
            {
                long targetId = GetAttributeId(attribute);

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
        static ComAttribute[] UpdateModifiedItems(IList<VerificationItem> items,
            ICollection<ComAttribute> modified)
        {
            // Make an array to hold the previous version of the modified attributes
            ComAttribute[] oldModified = new ComAttribute[modified.Count];

            // Iterate over each modified attribute
            int i = 0;
            foreach (ComAttribute attribute in modified)
            {
                long targetId = GetAttributeId(attribute);

                // Find the corresponding previous version
                int index = GetIndexFromAttributeId(items, targetId);
                if (index < 0)
                {
                    throw new ExtractException("ELI28212",
                        "Missing original version of modified attribute.");
                }

                // Store the previous version
                VerificationItem old = items[index];
                oldModified[i] = old.Attribute;
                i++;

                // Update the version number of the modified attribute
                IncrementRevision(attribute);

                // Store the new version
                items[index] = new VerificationItem(old.Level, attribute);
            }

            return oldModified;
        }

        /// <summary>
        /// Updates the specified verification items with added attributes.
        /// </summary>
        /// <param name="items">The items to be updated.</param>
        /// <param name="added">The attributes to add.</param>
        void UpdateAddedItems(ICollection<VerificationItem> items, IEnumerable<ComAttribute> added)
        {
            foreach (ComAttribute attribute in added)
            {
                // Each new attribute needs a unique id
                AssignId(attribute);

                // Add the attribute
                VerificationItem item = new VerificationItem(_manual, attribute);
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
        static int GetIndexFromAttributeId(IList<VerificationItem> items, long id)
        {
            for (int i = 0; i < items.Count; i++)
            {
                VerificationItem item = items[i];
                long itemId = GetAttributeId(item.Attribute);
                if (id == itemId)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Increment the version number of the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute whose revision should be incremented.</param>
        static void IncrementRevision(ComAttribute attribute)
        {
            ComAttribute idAttribute = GetIdAttribute(attribute);
            int revision = 1 + int.Parse(idAttribute.Type, CultureInfo.CurrentCulture);
            idAttribute.Type = revision.ToString(CultureInfo.CurrentCulture);
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
        /// <returns>All the attributes in a verification vector of attributes (VOA) file, other 
        /// than the ones that were directly verified by the user.</returns>
        IUnknownVector GetUnverifiedAttributes(VerificationSettings settings, TimeInterval time,
            VerificationFileChanges changes, ComAttribute[] oldDeleted, ComAttribute[] oldModified)
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
            AddRevisions(revisions, oldDeleted, oldModified);

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
            VerificationFileChanges changes)
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
            ComAttribute loginId = CreateComAttribute(Environment.UserName);
            ComAttribute computerName = CreateComAttribute(Environment.MachineName);
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
            ComAttribute date = CreateComAttribute("_Date", time.Start.Date);
            ComAttribute timeStarted = CreateComAttribute("_TimeStarted", time.Start.ToLocalTime());
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
        ComAttribute CreateChangeEntriesAttribute(string name, ICollection<ComAttribute> entries)
        {
            ComAttribute changeEntries = CreateComAttribute(name);

            // Append entries if necessary
            if (entries.Count > 0)
            {
                IUnknownVector subAttributes = changeEntries.SubAttributes;
                foreach (ComAttribute entry in entries)
                {
                    ComAttribute changeEntry = GetSingleAttributeByName(entry.SubAttributes, "_IDAndRevision");
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
        static void AddRevisions(ComAttribute revisions, IEnumerable<ComAttribute> oldDeleted,
            IEnumerable<ComAttribute> oldModified)
        {
            IUnknownVector subAttributes = revisions.SubAttributes;
            foreach (ComAttribute deleted in oldDeleted)
            {
                subAttributes.PushBack(deleted);
            }
            foreach (ComAttribute modified in oldModified)
            {
                subAttributes.PushBack(modified);
            }
        }

        /// <summary>
        /// Gets the vector of attributes from combining the verified items with the unverified 
        /// items.
        /// </summary>
        /// <param name="verified">The items that were verified.</param>
        /// <param name="unverified">The attributes that were not verified.</param>
        /// <returns>A vector containing the attributes from <paramref name="verified"/> and 
        /// <paramref name="unverified"/>.</returns>
        static IUnknownVector GetOutputVector(IEnumerable<VerificationItem> verified, 
            IUnknownVector unverified)
        {
            // Add the verified attributes to the output vector
            IUnknownVector output = new IUnknownVector();
            foreach (VerificationItem item in verified)
            {
                output.PushBack(item.Attribute);
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
