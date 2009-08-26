using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a level of verification confidence for a set of clues or redactions.
    /// </summary>
    public class ConfidenceLevel
    {
        #region ConfidenceLevel Fields

        readonly string _longName;
        readonly string _shortName;
        readonly string _query;
        readonly Color _color;
        readonly bool _output;

        #endregion ConfidenceLevel Fields

        #region ConfidenceLevel Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfidenceLevel"/> class.
        /// </summary>
        public ConfidenceLevel(string longName, string shortName, string query, Color color, 
            bool output)
        {
            _longName = longName;
            _shortName = shortName;
            _query = query;
            _color = color;
            _output = output;
        }

        #endregion ConfidenceLevel Constructors

        #region ConfidenceLevel Properties

        /// <summary>
        /// Gets the full name of the confidence level.
        /// </summary>
        /// <value>The full name of the confidence level.</value>
        public string LongName
        {
            get
            {
                return _longName;
            }
        }

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

        #endregion ConfidenceLevel Properties
    }
}
