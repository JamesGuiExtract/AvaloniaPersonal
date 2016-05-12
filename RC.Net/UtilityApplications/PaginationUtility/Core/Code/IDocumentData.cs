using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Represents data associated with an <see cref="OutputDocument"/>.
    /// </summary>
    public interface IDocumentData
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IDocumentData"/> is modified.
        /// </summary>
        /// <value><see langword="true"/> if modified; otherwise, <see langword="false"/>.
        /// </value>
        bool Modified
        {
            get;
        }

        /// <summary>
        /// Raised when the value of <see cref="Modified"/> has been changed.
        /// </summary>
        event EventHandler<EventArgs> ModifiedChanged;

        /// <summary>
        /// Reverts this instance back to its original state.
        /// </summary>
        void Revert();

        /// <summary>
        /// Defines the current state as the original state for this instance.
        /// </summary>
        void SetOriginalForm();
    }
}
