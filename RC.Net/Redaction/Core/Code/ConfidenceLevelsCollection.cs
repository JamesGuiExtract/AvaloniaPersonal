using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a read only collection of <see cref="ConfidenceLevel"/> objects.
    /// </summary>
    public class ConfidenceLevelsCollection : ReadOnlyCollection<ConfidenceLevel>
    {
        #region Fields

        readonly ConfidenceLevel _manual;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfidenceLevelsCollection"/> class.
        /// </summary>
        public ConfidenceLevelsCollection() 
            : this(new ConfidenceLevel[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfidenceLevelsCollection"/> class.
        /// </summary>
        internal ConfidenceLevelsCollection(IList<ConfidenceLevel> levels)
            : base(levels)
        {
            _manual = GetManualConfidenceLevel(levels);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="ConfidenceLevel"/> associated with manual redactions.
        /// </summary>
        /// <value>The <see cref="ConfidenceLevel"/> associated with manual redactions.</value>
        public ConfidenceLevel Manual
        {
            get
            {
                return _manual;
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

        #endregion Methods
    }
}
