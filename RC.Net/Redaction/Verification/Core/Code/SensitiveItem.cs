using System;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a single redaction or clue used for verification
    /// </summary>
    public class VerificationItem
    {
        #region VerificationItem Fields

        /// <summary>
        /// The confidence level of the <see cref="VerificationItem"/>.
        /// </summary>
        readonly ConfidenceLevel _level;

        /// <summary>
        /// The attribute associated with the <see cref="VerificationItem"/>.
        /// </summary>
        readonly ComAttribute _attribute;

        #endregion VerificationItem Fields

        #region VerificationItem Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationItem"/> class.
        /// </summary>
        /// <param name="level">The confidence level of this item.</param>
        /// <param name="attribute">The attribute associated with this item.</param>
        [CLSCompliant(false)]
        public VerificationItem(ConfidenceLevel level, ComAttribute attribute)
        {
            _level = level;
            _attribute = attribute;
        }

        #endregion VerificationItem Constructors

        #region VerificationItem Properties

        /// <summary>
        /// Gets the value associated with this verification item.
        /// </summary>
        /// <value>The value associated with this verification item.</value>
        [CLSCompliant(false)]
        public ComAttribute Attribute
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

        #endregion VerificationItem Properties
    }
}
