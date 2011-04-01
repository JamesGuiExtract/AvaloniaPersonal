using Extract.AttributeFinder;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a clue or redaction COM attribute.
    /// </summary>
    public class RedactionItem : ICloneable
    {
        #region Fields

        /// <summary>
        /// The COM attribute representing the clue or redaction.
        /// </summary>
        readonly ComAttribute _attribute;

        /// <summary>
        /// <see langword="true"/> if the sensitive item should redacted; <see langword="false"/> 
        /// if the sensitive item should not be redacted.
        /// </summary>
        bool _redacted;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionItem"/> class.
        /// </summary>
        RedactionItem(RedactionItem attribute)
            : this(GetClone(attribute._attribute), null, null, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionItem"/> class.
        /// </summary>
        [CLSCompliant(false)]
        public RedactionItem(ComAttribute attribute)
            : this(attribute, null, null, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionItem"/> class.
        /// </summary>
        [CLSCompliant(false)]
        public RedactionItem(ComAttribute attribute, ExemptionCodeList exemptions, 
            string sourceDocument)
            : this(attribute, exemptions, sourceDocument, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionItem"/> class.
        /// </summary>
        [CLSCompliant(false)]
        public RedactionItem(ComAttribute attribute, ExemptionCodeList exemptions,
            string sourceDocument, bool redacted)
        {
            _attribute = attribute;

            if (exemptions != null)
            {
                SetExemptions(exemptions, sourceDocument);
            }

            _redacted = redacted;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the category (Man, Clue, etc.) of the <see cref="RedactionItem"/>.
        /// </summary>
        /// <value>The category (Man, Clue, etc.) of the <see cref="RedactionItem"/>.</value>
        public string Category
        {
            get 
            {
                return _attribute.Name;
            }
        }

        /// <summary>
        /// Gets the redaction type (SSN, Clues, etc.) of the <see cref="RedactionItem"/>.
        /// </summary>
        /// <value>The redaction type (SSN, Clues, etc.) of the <see cref="RedactionItem"/>.</value>
        public string RedactionType
        {
            get
            {
                return _attribute.Type;
            }
        }

        /// <summary>
        /// Gets the spatial string that represents the clue or redaction.
        /// </summary>
        /// <value>The spatial string that represents the clue or redaction.</value>
        [CLSCompliant(false)]
        public SpatialString SpatialString
        {
            get
            {
                return _attribute.Value;
            }
        }

        /// <summary>
        /// Returns the underlying COM attribute.
        /// </summary>
        /// <value>The underlying COM attribute.</value>
        [CLSCompliant(false)]
        public ComAttribute ComAttribute
        {
            get
            {
                return _attribute;
            }
        }

        /// <summary>
        /// Gets or sets whether this item should be redacted.
        /// </summary>
        /// <value><see langword="true"/> if the item should be redacted;
        /// <see langword="false"/> if the item should not be redacted.</value>
        public bool Redacted
        {
            get
            {
                return _redacted;
            }
            set
            {
                _redacted = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a copy of the specified redaction attribute.
        /// </summary>
        /// <param name="attribute">The attribute to copy.</param>
        /// <returns>A copy of <paramref name="attribute"/>.</returns>
        static ComAttribute GetClone(ComAttribute attribute)
        {
            ICopyableObject copy = (ICopyableObject)attribute;

            return (ComAttribute)copy.Clone();
        }

        /// <summary>
        /// Gets the unique id of the <see cref="RedactionItem"/>.
        /// </summary>
        /// <returns>The unique id of the <see cref="RedactionItem"/>; or -1 if 
        /// <see cref="RedactionItem"/> has not yet been assigned an id.</returns>
        // This method performs complex operations, so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public long GetId()
        {
            try
            {
                ComAttribute idAttribute = GetIdAttribute();
                if (idAttribute != null)
                {
                    return Int64.Parse(idAttribute.Value.String, CultureInfo.CurrentCulture);
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI28566",
                    "Unable to get attribute id.", ex);
            }
        }

        /// <summary>
        /// Sets the id of the <see cref="RedactionItem"/> if it does not already have one.
        /// </summary>
        /// <param name="id">The id to assign.</param>
        /// <param name="sourceDocument">The source document to use if an id is assigned.</param>
        /// <returns><see langword="true"/> if the id was assigned; <see langword="false"/> if the 
        /// attribute already had an id assigned.</returns>
        internal bool AssignIdIfNeeded(long id, string sourceDocument)
        {
            ComAttribute idAttribute = GetIdAttribute();

            bool createIdAttribute = idAttribute == null;
            if (createIdAttribute)
            {
                AddIdAttribute(id, sourceDocument);
            }

            return createIdAttribute;
        }

        /// <summary>
        /// Creates and adds an id attribute to the <see cref="RedactionItem"/>.
        /// </summary>
        /// <param name="id">The unique id to assign to the id attribute.</param>
        /// <param name="sourceDocument">The source document to use for the id attribute.</param>
        void AddIdAttribute(long id, string sourceDocument)
        {
            ComAttribute revisionId = CreateIdAttribute(id, sourceDocument);

            _attribute.SubAttributes.PushBack(revisionId);
        }

        /// <summary>
        /// Creates an id attribute with the specified id.
        /// </summary>
        /// <param name="id">The unique id to assign to the id attribute.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <returns>An id attribute with the specified <paramref name="id"/>.</returns>
        static ComAttribute CreateIdAttribute(long id, string sourceDocument)
        {
            // Create an attribute with the specified name
            ComAttribute attribute = new ComAttribute();
            attribute.Name = Constants.IDAndRevisionMetadata;
            attribute.Value = CreateNonSpatialString(id, sourceDocument);
            attribute.Type = "_1";

            return attribute;
        }

        /// <summary>
        /// Creates a non-spatial string with the specified value.
        /// </summary>
        /// <param name="value">The value of the non-spatial string to create. Will be converted 
        /// to a string.</param>
        /// <param name="sourceDocument">The source document to use for the non spatial string.</param>
        /// <returns>A non-spatial string with the specified <paramref name="value"/>.</returns>
        static SpatialString CreateNonSpatialString(IConvertible value, string sourceDocument)
        {
            SpatialString spatialString = new SpatialString();
            string text = value.ToString(CultureInfo.CurrentCulture);
            spatialString.CreateNonSpatialString(text, sourceDocument);

            return spatialString;
        }

        /// <summary>
        /// Gets the revision id of the <see cref="RedactionItem"/>.
        /// </summary>
        /// <returns>The revision id of <see cref="RedactionItem"/>; or if it does not have a 
        /// revision id.</returns>
        // This method performs complex operations, so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetRevision()
        {
            try
            {
                ComAttribute idAttribute = GetIdAttribute();
                if (idAttribute != null)
                {
                    return GetRevisionFromIdAttribute(idAttribute);
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI28567",
                    "Unable to get revision id.", ex);
            }
        }

        /// <summary>
        /// Increment the revision number of the <see cref="RedactionItem"/>.
        /// </summary>
        internal void IncrementRevision()
        {
            // Get the ID attribute
            ComAttribute idAttribute = GetIdAttribute();

            // Get the revision number incremented by one
            int revision = 1 + GetRevisionFromIdAttribute(idAttribute);

            // Store the new revision number.
            // Prepend underscore because type must start with underscore or alphabetic character.
            idAttribute.Type = "_" + revision.ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the id attribute associated with the <see cref="RedactionItem"/>.
        /// </summary>
        /// <returns>The id attribute associated with the <see cref="RedactionItem"/>.</returns>
        internal ComAttribute GetIdAttribute()
        {
            return AttributeMethods.GetSingleAttributeByName(_attribute.SubAttributes,
                Constants.IDAndRevisionMetadata);
        }

        /// <summary>
        /// Retrieves the revision number from the specified _IDAndRevision attribute.
        /// </summary>
        /// <param name="idAttribute">The _IDAndRevision attribute.</param>
        /// <returns>The revisiion number of the <paramref name="idAttribute"/>.</returns>
        static int GetRevisionFromIdAttribute(ComAttribute idAttribute)
        {
            // Drop the initial underscore from the ID attribute
            string revisionString = idAttribute.Type.Substring(1);

            // Parse the string
            return Int32.Parse(revisionString, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the exemption code list from an exemption codes attribute.
        /// </summary>
        /// <param name="masterCodes">The master collection of valid exemption codes.</param>
        public ExemptionCodeList GetExemptions(MasterExemptionCodeList masterCodes)
        {
            try
            {
                ComAttribute exemptionsAttribute = GetExemptionsComAttribute(_attribute.SubAttributes);
                if (exemptionsAttribute == null)
                {
                    return new ExemptionCodeList();
                }

                string category = exemptionsAttribute.Type.Replace('_', ' ');
                string codes = exemptionsAttribute.Value.String;

                return ExemptionCodeList.Parse(category, codes, masterCodes);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI28568",
                    "Unable to get exemptions.", ex);
            }
        }

        /// <summary>
        /// Sets the exemption codes for the <see cref="RedactionItem"/>.
        /// </summary>
        /// <param name="exemptions">The exemption codes to assign.</param>
        /// <param name="sourceDocument">The name of the source document.</param>
        void SetExemptions(ExemptionCodeList exemptions, string sourceDocument)
        {
            try
            {
                // If there are no exemption codes to assign, just remove the exemption codes attribute.
                if (exemptions.IsEmpty)
                {
                    RemoveExemptionsComAttribute(_attribute);
                    return;
                }

                // Get the attribute's exemption codes attribute
                IUnknownVector subattributes = _attribute.SubAttributes;
                ComAttribute exemptionsAttribute = GetExemptionsComAttribute(subattributes);
                if (exemptionsAttribute == null)
                {
                    // Append a new exemption codes COM attribute
                    exemptionsAttribute = new ComAttribute();
                    exemptionsAttribute.Name = "ExemptionCodes";
                    subattributes.PushBack(exemptionsAttribute);
                }

                // Set the exemption codes
                SpatialString value = new SpatialString();
                value.CreateNonSpatialString(exemptions.ToString(), sourceDocument);
                exemptionsAttribute.Value = value;
                exemptionsAttribute.Type = exemptions.Category.Replace(' ', '_');
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI28569",
                    "Unable to set exemption codes.", ex);
            }
        }

        /// <summary>
        /// Retrieves the exemption codes attribute from a vector of attributes.
        /// </summary>
        /// <param name="attributes">The attributes to check.</param>
        /// <returns>The exemption code attribute if it is one of the <paramref name="attributes"/>;
        /// otherwise returns <see langword="null"/>.</returns>
        static ComAttribute GetExemptionsComAttribute(IUnknownVector attributes)
        {
            if (attributes != null)
            {
                int count = attributes.Size();
                for (int i = 0; i < count; i++)
                {
                    ComAttribute subattribute = (ComAttribute)attributes.At(i);
                    if (subattribute.Name == "ExemptionCodes")
                    {
                        return subattribute;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Removes exemption codes from the specified COM attribute.
        /// </summary>
        /// <param name="attribute">The COM attribute from which to remove exemption codes.</param>
        static void RemoveExemptionsComAttribute(ComAttribute attribute)
        {
            IUnknownVector subattributes = attribute.SubAttributes;
            int count = subattributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute subattribute = (ComAttribute)subattributes.At(i);
                if (subattribute.Name == "ExemptionCodes")
                {
                    subattributes.Remove(i);
                    return;
                }
            }
        }

        #endregion Methods

        #region ICloneable Members

        /// <summary>
        /// Creates a new <see cref="RedactionItem"/> that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new <see cref="RedactionItem"/> that is a copy of this instance.
        /// </returns>
        public RedactionItem Clone()
        {
            return new RedactionItem(this);
        }

        /// <summary>
        /// Creates a new <see cref="RedactionItem"/> that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new <see cref="RedactionItem"/> that is a copy of this instance.
        /// </returns>
        object ICloneable.Clone()
        {
            return new RedactionItem(this);
        }

        #endregion ICloneable Members
    }
}