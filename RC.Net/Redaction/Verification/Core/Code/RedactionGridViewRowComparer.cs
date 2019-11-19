using Extract.AttributeFinder;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a comparison operation between two <see cref="RedactionGridViewRow"/>s.
    /// </summary>
    public class RedactionGridViewRowComparer : IComparer, IComparer<RedactionGridViewRow>
    {
        #region RedactionGridViewComparer Fields

        /// <summary>
        /// Spatially compares COM attributes.
        /// </summary>
        readonly AttributeComparer _attributeComparer = new AttributeComparer();

        #endregion RedactionGridViewComparer Fields

        #region RedactionGridViewComparer Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRowComparer"/> class.
        /// </summary>
        public RedactionGridViewRowComparer()
        {
        }

        #endregion RedactionGridViewComparer Constructors

        #region IComparer Members

        /// <summary>
        /// Compares two objects.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>Less than zero if <paramref name="x"/> is less than <paramref name="y"/>;
        /// zero if <paramref name="x"/> equals <paramref name="y"/>; greater than zero if 
        /// <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
        public int Compare(object x, object y)
        {
            return Compare(x as RedactionGridViewRow, y as RedactionGridViewRow);
        }

        #endregion IComparer Members

        #region IComparer<RedactionGridViewRow> Members

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
        public int Compare(RedactionGridViewRow x, RedactionGridViewRow y)
        {
            // If these are the same object, they are equal
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

            if (x.RedactionItem == null)
            {
                return -1;
            }
            else if (y.RedactionItem == null)
            {
                return 1;
            }

            // Compare the attributes spatially
            return _attributeComparer.Compare(x.RedactionItem.ComAttribute, y.RedactionItem.ComAttribute);
        }

        #endregion IComparer<RedactionGridViewRow> Members
    }
}
