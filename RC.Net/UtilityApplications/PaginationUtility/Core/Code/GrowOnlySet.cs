using System.Collections;
using System.Collections.Generic;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Wraps a <see cref="HashSet{T}"/> to prevent deletions
    /// </summary>
    /// <remarks>
    /// The HashSet implementation preserves insertion order as long as there are no deletions.
    /// This is undocumented so it probably shouldn't be relied upon.
    /// Instead, use <see cref="Count"/> with <see cref="Add"/> to track insertion order.
    /// </remarks>
    internal class GrowOnlySet<T> : IEnumerable<T>
    {
        readonly HashSet<T> _set = new();

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_set).GetEnumerator();
        }

        public int Count => _set.Count;

        /// <summary>
        /// Add item to the set or throw an exception if the item is already in the set
        /// </summary>
        internal void Add(T outputDocument)
        {
            if (!_set.Add(outputDocument))
            {
                throw new ExtractException("ELI53485", "Item is already contained in the set");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_set).GetEnumerator();
        }
    }
}
