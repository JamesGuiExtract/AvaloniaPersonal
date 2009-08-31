using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents redaction statistics.
    /// </summary>
    public class RedactionCounts
    {
        #region RedactionCounts Fields

        readonly int _highConfidence;
        readonly int _mediumConfidence;
        readonly int _lowConfidence;
        readonly int _clues;
        readonly int _manual;
        readonly int _total;

        #endregion RedactionCounts Fields

        #region RedactionCounts Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionCounts"/> class.
        /// </summary>
        public RedactionCounts(int highConfidence, int mediumConfidence, int lowConfidence, 
            int clues, int manual, int total)
        {
            _highConfidence = highConfidence;
            _mediumConfidence = mediumConfidence;
            _lowConfidence = lowConfidence;
            _clues = clues;
            _manual = manual;
            _total = total;
        }

        #endregion RedactionCounts Constructors

        #region RedactionCounts Properties

        /// <summary>
        /// Gets the number of high confidence redactions found.
        /// </summary>
        /// <value>The number of high confidence redactions found.</value>
        public int HighConfidence
        {
            get
            {
                return _highConfidence;
            }
        }

        /// <summary>
        /// Gets the number of medium confidence redactions found.
        /// </summary>
        /// <value>The number of medium confidence redactions found.</value>
        public int MediumConfidence
        {
            get
            {
                return _mediumConfidence;
            }
        }

        /// <summary>
        /// Gets the number of low confidence redactions found.
        /// </summary>
        /// <value>The number of low confidence redactions found.</value>
        public int LowConfidence
        {
            get
            {
                return _lowConfidence;
            }
        }

        /// <summary>
        /// Gets the number of clues found.
        /// </summary>
        /// <value>The number of clues found.</value>
        public int Clues
        {
            get
            {
                return _clues;
            }
        }

        /// <summary>
        /// Gets the number of manual redactions in the final output.
        /// </summary>
        /// <value>The number of manual redactions in the final output.</value>
        public int Manual
        {
            get
            {
                return _manual;
            }
        }

        /// <summary>
        /// Gets the total number of redactions in the final output.
        /// </summary>
        /// <value>The total number of redactions in the final output.</value>
        public int Total
        {
            get
            {
                return _total;
            }
        }

        #endregion RedactionCounts Properties
    }
}
