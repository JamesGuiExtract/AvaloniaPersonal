using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a read only collection of <see cref="ConfidenceLevel"/> objects.
    /// </summary>
    public class ConfidenceLevelsCollection : ReadOnlyCollection<ConfidenceLevel>
    {
        #region ConfidenceLevelsCollection Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfidenceLevelsCollection"/> class.
        /// </summary>
        public ConfidenceLevelsCollection() : base(new ConfidenceLevel[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfidenceLevelsCollection"/> class.
        /// </summary>
        internal ConfidenceLevelsCollection(IList<ConfidenceLevel> levels)
            : base(levels)
        {
        }

        #endregion ConfidenceLevelsCollection Constructors
    }
}
