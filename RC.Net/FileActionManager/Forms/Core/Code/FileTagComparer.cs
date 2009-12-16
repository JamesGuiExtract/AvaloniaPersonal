using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Represents a <see cref="FileTag"/> comparison operation.
    /// </summary>
    public class FileTagComparer : IComparer, IComparer<FileTag>
    {
        #region Fields

        /// <summary>
        /// Compares the names of the file tags.
        /// </summary>
        readonly StringComparer _comparer = StringComparer.OrdinalIgnoreCase;

        #endregion Fields

        #region IComparer Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, 
        /// or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>Less than zero if <paramref name="x"/> is less than <paramref name="y"/>;
        /// zero if <paramref name="x"/> equals <paramref name="y"/>; greater than zero if 
        /// <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
        public int Compare(object x, object y)
        {
            return Compare(x as FileTag, y as FileTag);
        }

        #endregion IComparer Members

        #region IComparer<FileTag> Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, 
        /// or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>Less than zero if <paramref name="x"/> is less than <paramref name="y"/>;
        /// zero if <paramref name="x"/> equals <paramref name="y"/>; greater than zero if 
        /// <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
        // This message is being suppressed as Compare should not throw any exceptions
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public int Compare(FileTag x, FileTag y)
        {
            // If these are the same object they are equal
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            // null comes before non-null
            if (x == null)
            {
                return -1;
            }
            else if (y == null)
            {
                return 1;
            }

            return _comparer.Compare(x.Name, y.Name);
        }

        #endregion IComparer<FileTag> Members
    }
}
