using Extract.Utilities;
using System.ComponentModel;

namespace Extract.Redaction.CustomComponentsHelper
{
    /// <summary>
    /// A <see cref="PathTagsBase"/> derived class that represents the path tags
    /// for redaction text.
    /// </summary>
    [DisplayName("Redaction Text Path Tags")]
    public class RedactionTextPathTags : PathTagsBase
    {
        #region Constants
        
        /// <summary>
        /// The tag for exemption codes.
        /// </summary>
        static readonly string _EXEMPTION_CODES_TAG = "<ExemptionCodes>";

        /// <summary>
        /// The tag for field types.
        /// </summary>
        static readonly string _FIELD_TYPE_TAG = "<FieldType>";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionTextPathTags"/> class.
        /// </summary>
        public RedactionTextPathTags()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionTextPathTags"/> class.
        /// </summary>
        /// <param name="exemptionCode">The exemption code.</param>
        /// <param name="fieldType">Type of the field.</param>
        public RedactionTextPathTags(string exemptionCode, string fieldType)
            : base()
        {
            AddTag(_EXEMPTION_CODES_TAG, exemptionCode ?? string.Empty);
            AddTag(_FIELD_TYPE_TAG, fieldType ?? string.Empty);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the exemption code tag.
        /// </summary>
        public static string ExemptionCodeTag
        {
            get
            {
                return _EXEMPTION_CODES_TAG;
            }
        }

        /// <summary>
        /// Gets the field type tag.
        /// </summary>
        public static string FieldTypeTag
        {
            get
            {
                return _FIELD_TYPE_TAG;
            }
        }

        #endregion Properties
    }
}
