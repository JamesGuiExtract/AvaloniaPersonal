using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Extract.Rules
{
    /// <summary>
    /// Represents a collection of <see cref="MatchResult"/> objects.
    /// </summary>
    public class MatchResultCollection : IList<MatchResult>
    {
        #region Fields

        readonly List<MatchResult> _matchResults = new List<MatchResult>();

        #endregion Fields

        #region Methods

        /// <summary>
        /// Adds the elements of the specified collection to the end of the 
        /// <see cref="MatchResultCollection"/>
        /// </summary>
        /// <param name="matchResults">The collection whose elements should be added to the end of 
        /// the <see cref="MatchResultCollection"/>.</param>
        public void AddRange(IEnumerable<MatchResult> matchResults)
        {
            _matchResults.AddRange(matchResults);
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="MatchResultCollection"/> using the 
        /// default comparer.
        /// </summary>
        public void Sort()
        {
            _matchResults.Sort();
        }

        /// <summary>
        /// Returns a read-only wrapper for the <see cref="MatchResultCollection"/>.
        /// </summary>
        /// <returns>A collection that acts as a read-only wrapper around the 
        /// <see cref="MatchResultCollection"/>.</returns>
        public ReadOnlyCollection<MatchResult> AsReadOnly()
        {
            return _matchResults.AsReadOnly();
        }

        #endregion Methods

        #region IList<MatchResult> Members

        /// <summary>
        /// Determines the index of a specific item in the <see cref="IList{T}"/>.
        /// </summary>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="IList{T}"/>.</param>
        public int IndexOf(MatchResult item)
        {
            return _matchResults.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="IList{T}"/> at the specified index.
        /// </summary>
        /// <param name="item">The object to insert into the <see cref="IList{T}"/>.</param>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        public void Insert(int index, MatchResult item)
        {
            _matchResults.Insert(index, item);
        }

        /// <summary>
        /// Removes the <see cref="IList{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            _matchResults.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        public MatchResult this[int index]
        {
            get
            {
                return _matchResults[index];
            }
            set
            {
                _matchResults[index] = value;
            }
        }

        #endregion IList<MatchResult> Members

        #region ICollection<MatchResult> Members

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
        public void Add(MatchResult item)
        {
            _matchResults.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}"/>.
        /// </summary>
        public void Clear()
        {
            _matchResults.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="ICollection{T}"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if item is found in the <see cref="ICollection{T}"/>; 
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
        public bool Contains(MatchResult item)
        {
            return _matchResults.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}"/> to an <see cref="Array"/>, 
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of 
        /// the elements copied from <see cref="ICollection{T}"/>. The <see cref="Array"/> must 
        /// have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which 
        /// copying begins.</param>
        public void CopyTo(MatchResult[] array, int arrayIndex)
        {
            _matchResults.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="ICollection{T}"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return _matchResults.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="ICollection{T}"/> is read-only; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed from the 
        /// <see cref="ICollection{T}"/>; <see langword="false"/> if item is not found in the 
        /// original <see cref="ICollection{T}"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
        public bool Remove(MatchResult item)
        {
            return _matchResults.Remove(item);
        }

        #endregion ICollection<MatchResult> Members

        #region IEnumerable<MatchResult> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<MatchResult> GetEnumerator()
        {
            return _matchResults.GetEnumerator();
        }

        #endregion IEnumerable<MatchResult> Members

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _matchResults.GetEnumerator();
        }

        #endregion IEnumerable Members
    }
}