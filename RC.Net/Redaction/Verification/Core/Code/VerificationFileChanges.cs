using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents changes made to COM attributes in a vector of attributes (VOA) file.
    /// </summary>
    public class VerificationFileChanges
    {
        #region VerificationFileChanges Fields

        /// <summary>
        /// COM attributes that were added.
        /// </summary>
        readonly ReadOnlyCollection<ComAttribute> _added;

        /// <summary>
        /// COM attributes that were deleted.
        /// </summary>
        readonly ReadOnlyCollection<ComAttribute> _deleted;

        /// <summary>
        /// COM attributes that were modified.
        /// </summary>
        readonly ReadOnlyCollection<ComAttribute> _modified;

        #endregion VerificationFileChanges Fields

        #region VerificationFileChanges Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationFileChanges"/> class.
        /// </summary>
        public VerificationFileChanges(IList<ComAttribute> added, IList<ComAttribute> deleted, 
            IList<ComAttribute> modified)
        {
            _added = new ReadOnlyCollection<ComAttribute>(added);
            _deleted = new ReadOnlyCollection<ComAttribute>(deleted);
            _modified = new ReadOnlyCollection<ComAttribute>(modified);
        }

        #endregion VerificationFileChanges Constructors

        #region VerificationFileChanges Properties

        /// <summary>
        /// Gets the COM attributes that were added.
        /// </summary>
        /// <value>The COM attributes that were added.</value>
        [CLSCompliant(false)]
        public ReadOnlyCollection<ComAttribute> Added
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
        [CLSCompliant(false)]
        public ReadOnlyCollection<ComAttribute> Deleted
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
        [CLSCompliant(false)]
        public ReadOnlyCollection<ComAttribute> Modified
        {
            get
            {
                return _modified;
            }
        }

        #endregion VerificationFileChanges Properties
    }
}
