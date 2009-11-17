using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents changes made to COM attributes in a vector of attributes (VOA) file.
    /// </summary>
    public class RedactionFileChanges
    {
        #region Fields

        /// <summary>
        /// COM attributes that were added.
        /// </summary>
        readonly ReadOnlyCollection<RedactionItem> _added;

        /// <summary>
        /// COM attributes that were deleted.
        /// </summary>
        readonly ReadOnlyCollection<RedactionItem> _deleted;

        /// <summary>
        /// COM attributes that were modified.
        /// </summary>
        readonly ReadOnlyCollection<RedactionItem> _modified;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionFileChanges"/> class.
        /// </summary>
        public RedactionFileChanges(IList<RedactionItem> added, IList<RedactionItem> deleted,
            IList<RedactionItem> modified)
        {
            _added = new ReadOnlyCollection<RedactionItem>(added);
            _deleted = new ReadOnlyCollection<RedactionItem>(deleted);
            _modified = new ReadOnlyCollection<RedactionItem>(modified);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the COM attributes that were added.
        /// </summary>
        /// <value>The COM attributes that were added.</value>
        public ReadOnlyCollection<RedactionItem> Added
        {
            get
            {
                return _added;
            }
        }

        /// <summary>
        /// Gets the COM attributes that were deleted.
        /// </summary>
        /// <value>The COM attributes that were deleted.</value>
        public ReadOnlyCollection<RedactionItem> Deleted
        {
            get
            {
                return _deleted;
            }
        }

        /// <summary>
        /// Gets the COM attributes that were modified.
        /// </summary>
        /// <value>The COM attributes that were modified.</value>
        public ReadOnlyCollection<RedactionItem> Modified
        {
            get
            {
                return _modified;
            }
        }

        #endregion Properties
    }
}
