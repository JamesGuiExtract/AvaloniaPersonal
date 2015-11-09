using System.Drawing;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a level of verification confidence for a set of clues or redactions.
    /// </summary>
    public class ConfidenceLevel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfidenceLevel"/> class.
        /// </summary>
        public ConfidenceLevel(string shortName, string query, Color color, bool output,
            bool warnIfRedact, bool warnIfNonRedact, bool readOnly, bool highlight,
            Color? fillColor)
        {
            ShortName = shortName;
            Query = query;
            Color = color;
            Output = output;
            WarnIfRedacted = warnIfRedact;
            WarnIfNotRedacted = warnIfNonRedact;
            ReadOnly = readOnly;
            Highlight = highlight;
            FillColor = fillColor;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the abbreviated name of the confidence level.
        /// </summary>
        /// <value>The abbreviated name of the confidence level.</value>
        public string ShortName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the attribute query for data in this confidence level.
        /// </summary>
        /// <value>The attribute query for data in this confidence level.</value>
        public string Query
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the color to display attributes of this confidence level.
        /// </summary>
        /// <value>The color to display attributes of this confidence level.</value>
        public Color Color
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether attributes of this confidence level should be rendered in physical output 
        /// such as printing.
        /// </summary>
        /// <value><see langword="true"/> if attributes should be rendered in physical output;
        /// <see langword="false"/> if attributes should not be rendered in physical output.</value>
        public bool Output
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether a warning should be displayed if the attribute is redacted.
        /// </summary>
        /// <value><see langword="true"/> if a warning should be displayed if the attribute is 
        /// redacted; <see langword="false"/> if a warning should not be displayed if the 
        /// attribute is redacted.</value>
        public bool WarnIfRedacted
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets whether a warning should be displayed if the attribute is not redacted.
        /// </summary>
        /// <value><see langword="true"/> if a warning should be displayed if the attribute is not 
        /// redacted; <see langword="false"/> if a warning should not be displayed if the 
        /// attribute is not redacted.</value>
        public bool WarnIfNotRedacted
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the attribute is read-only.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the attribute is read-only; otherwise, <see langword="false"/>.
        /// </value>
        public bool ReadOnly
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ConfidenceLevel"/> represents a
        /// highlight rather than a redaction.
        /// </summary>
        /// <value><see langword="true"/> if a highlight; otherwise, <see langword="false"/>.
        /// </value>
        public bool Highlight
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the fill <see cref="Color"/> to use for attributes in this category or
        /// <see langword="null"/> to use the default fill color.
        /// </summary>
        /// <value>
        /// The fill <see cref="Color"/> to use for attributes in this category.
        /// </value>
        public Color? FillColor
        {
            get;
            private set;
        }

        #endregion Properties
    }
}
