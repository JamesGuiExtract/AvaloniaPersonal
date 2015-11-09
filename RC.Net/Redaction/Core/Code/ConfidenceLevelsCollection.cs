using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a read only collection of <see cref="ConfidenceLevel"/> objects.
    /// </summary>
    public class ConfidenceLevelsCollection : ReadOnlyCollection<ConfidenceLevel>
    {
        #region Fields

        readonly ConfidenceLevel _manual;

        /// <summary>
        /// The <see cref="AFUtility"/> to use to execute AF queries.
        /// </summary>
        AFUtility _utility;

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
        /// Gets the <see cref="ConfidenceLevel"/> that should be associated with the specified
        /// <see paramref="attribute"/>. If there are multiple matching confidence levels, the first
        /// defined level will be used.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which
        /// <see cref="ConfidenceLevel"/> is needed.</param>
        /// <param name="defaultLevel">The default <see cref="ConfidenceLevel"/> to use if no
        /// matching confidence level is found.</param>
        /// <returns>The <see cref="ConfidenceLevel"/> that should be associated with the specified
        /// <see paramref="attribute"/>.</returns>
        [CLSCompliant(false)]
        public ConfidenceLevel GetConfidenceLevel(IAttribute attribute, ConfidenceLevel defaultLevel)
        {
            try
            {
                // The QueryAttributes method requires a vector of attributes to search.
                var searchVector = new[] { attribute }.ToIUnknownVector();

                foreach (var confidenceLevel in base.Items)
                {
                    if (AFUtility.QueryAttributes(
                        searchVector, confidenceLevel.Query, false).ToIEnumerable<IAttribute>().Any())
                    {
                        // If confidenceLevel.Query allowed the attribute to be selected, this is
                        // the confidence level to use.
                        return confidenceLevel;
                    }
                }

                return defaultLevel;
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI39108");
            }
        }

        #endregion Methods

        #region Private Members

        /// <summary>
        /// Gets the AF utility.
        /// </summary>
        AFUtility AFUtility
        {
            get
            {
                if (_utility == null)
                {
                    _utility = new AFUtility();
                }

                return _utility;
            }
        }

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

        #endregion Private Members
    }
}
