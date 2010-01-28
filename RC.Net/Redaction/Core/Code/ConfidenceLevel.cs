using System.Drawing;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a level of verification confidence for a set of clues or redactions.
    /// </summary>
    public class ConfidenceLevel
    {
        #region Fields

        readonly string _shortName;
        readonly string _query;
        readonly Color _color;
        readonly bool _output;
        readonly bool _warnIfRedact;
        readonly bool _warnIfNonRedact;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfidenceLevel"/> class.
        /// </summary>
        public ConfidenceLevel(string shortName, string query, Color color, bool output, bool warnIfRedact, bool warnIfNonRedact)
        {
            _shortName = shortName;
            _query = query;
            _color = color;
            _output = output;
            _warnIfRedact = warnIfRedact;
            _warnIfNonRedact = warnIfNonRedact;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the abbreviated name of the confidence level.
        /// </summary>
        /// <value>The abbreviated name of the confidence level.</value>
        public string ShortName
        {
            get
            {
                return _shortName;
            }
        }

        /// <summary>
        /// Gets the attribute query for data in this confidence level.
        /// </summary>
        /// <value>The attribute query for data in this confidence level.</value>
        public string Query
        {
            get
            {
                return _query;
            }
        }

        /// <summary>
        /// Gets the color to display attributes of this confidence level.
        /// </summary>
        /// <value>The color to display attributes of this confidence level.</value>
        public Color Color
        {
            get
            {
                return _color;
            }
        }

        /// <summary>
        /// Gets whether attributes of this confidence level should be rendered in physical output 
        /// such as printing.
        /// </summary>
        /// <value><see langword="true"/> if attributes should be rendered in physical output;
        /// <see langword="false"/> if attributes should not be rendered in physical output.</value>
        public bool Output
        {
            get
            {
                return _output;
            }
        }

        /// <summary>
        /// Gets whether a warning should be displayed if the attribute is redacted.
        /// </summary>
        /// <value><see langword="true"/> if a warning should be displayed if the attribute is 
        /// redacted; <see langword="false"/> if a warning should not be displayed if the 
        /// attribute is redacted.</value>
        public bool WarnIfRedacted
        {
            get
            {
                return _warnIfRedact;
            }
        }

        /// <summary>
        /// Gets or sets whether a warning should be displayed if the attribute is not redacted.
        /// </summary>
        /// <value><see langword="true"/> if a warning should be displayed if the attribute is not 
        /// redacted; <see langword="false"/> if a warning should not be displayed if the 
        /// attribute is not redacted.</value>
        public bool WarnIfNotRedacted
        {
            get
            {
                return _warnIfNonRedact;
            }
        }

        #endregion Properties
    }
}
