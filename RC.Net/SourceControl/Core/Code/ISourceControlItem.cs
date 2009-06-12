using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents an item under source control.
    /// </summary>
    public interface ISourceControlItem
    {
        /// <summary>
        /// Gets the local path of the source control item.
        /// </summary>
        /// <returns>The local path of the source control item.</returns>
        string LocalSpec
        {
            get;
        }

        /// <summary>
        /// Checks in the source control item.
        /// </summary>
        void CheckIn();

        /// <summary>
        /// Exclusively checks out the source control item.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", 
            MessageId="CheckOut")]
        void CheckOut();

        /// <summary>
        /// Undoes the check out on a source control item.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", 
            MessageId="CheckOut")]
        void UndoCheckOut();

        /// <summary>
        /// Gets whether the source control item is checked out to anyone.
        /// </summary>
        /// <returns><see langword="true"/> if someone has the source control item checked out;
        /// <see langword="false"/> if no one has the source control item checked out.</returns>
        bool IsCheckedOut
        {
            get;
        }

        /// <summary>
        /// Gets whether the source control item is checked out to current user.
        /// </summary>
        /// <returns><see langword="true"/> if the current user has the source control item checked 
        /// out; <see langword="false"/> if the current user does not have the item checked out.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", 
            MessageId="ToMe")]
        bool IsCheckedOutToMe
        {
            get;
        }

        /// <summary>
        /// Gets the name(s) of the user(s) who have the source control item checked out.
        /// </summary>
        /// <returns>The name(s) of the user(s) who have the source control item checked out.
        /// </returns>
        string UserWhoCheckedOut
        {
            get;
        }
    }
}
