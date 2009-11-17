using System;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a single redaction or clue.
    /// </summary>
    public class SensitiveItem
    {
        #region Fields

        /// <summary>
        /// The confidence level of the <see cref="SensitiveItem"/>.
        /// </summary>
        readonly ConfidenceLevel _level;

        /// <summary>
        /// The attribute associated with the <see cref="SensitiveItem"/>.
        /// </summary>
        readonly RedactionItem _attribute;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SensitiveItem"/> class.
        /// </summary>
        /// <param name="level">The confidence level of this item.</param>
        /// <param name="attribute">The attribute associated with this item.</param>
        [CLSCompliant(false)]
        public SensitiveItem(ConfidenceLevel level, ComAttribute attribute)
            : this(level, new RedactionItem(attribute))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensitiveItem"/> class.
        /// </summary>
        /// <param name="level">The confidence level of this item.</param>
        /// <param name="attribute">The attribute associated with this item.</param>
        public SensitiveItem(ConfidenceLevel level, RedactionItem attribute)
        {
            _level = level;
            _attribute = attribute;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the value associated with this verification item.
        /// </summary>
        /// <value>The value associated with this verification item.</value>
        public RedactionItem Attribute
        {
            get
            {
                return _attribute;
            }
        }

        /// <summary>
        /// Gets the confidence level associated with this item.
        /// </summary>
        /// <value>The confidence level associated with this item.</value>
        public ConfidenceLevel Level
        {
            get
            {
                return _level;
            }
        }

        #endregion Properties
    }
}
