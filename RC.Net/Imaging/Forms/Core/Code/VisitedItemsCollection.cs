using System;
using System.Collections.Generic;
using System.Collections;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a collection of visited items.
    /// </summary>
    public class VisitedItemsCollection : IEnumerable<int>
    {
        #region Fields

        readonly int[] _items;

        readonly int _count;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VisitedItemsCollection"/> class.
        /// </summary>
        public VisitedItemsCollection(int count)
        {
            try
            {
                checked
                {
                    _items = new int[(count + 31) / 32];
                }
                _count = count;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27735",
                    "Unable to create visited item collection.", ex);
                ee.AddDebugData("Count", count, false);
                throw ee;
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether the specified item is visited.
        /// </summary>
        /// <param name="index">The zero-based index of the item to check.</param>
        /// <value><see langword="true"/> if the specified item is visited;
        /// <see langword="false"/> if the specified item is not visited.</value>
        /// <returns><see langword="true"/> if the specified item is visited;
        /// <see langword="false"/> if the specified item is not visited.</returns>
        public bool this[int index]
        {
            get
            {
                return (_items[index / 32] & (1 << (index % 32))) != 0;
            }
            set
            {
                try
                {
                    if (value)
                    {
                        _items[index / 32] |= 1 << (index % 32);
                    }
                    else
                    {
                        _items[index / 32] &= ~(1 << (index % 32));
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI27739",
                        "Unable to set visit item.", ex);
                    ee.AddDebugData("Index", index, false);
                    throw ee;
                }
            }
        }

        #endregion Properties

        #region IEnumerable<int> Members

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="VisitedItemsCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="VisitedItemsCollection"/>.</returns>
        /// <seealso cref="IEnumerator{T}"/>
        public IEnumerator<int> GetEnumerator()
        {
            int i = 0;
            int j = -1;
            for (int item = 0; item < _count; item++)
            {
                j++;
                if (j == 32)
                {
                    i++;
                    j = 0;
                }

                if ((_items[i] & (1 << j)) != 0)
                {
                    yield return item;
                }
            }
        }

        #endregion IEnumerable<int> Members

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="VisitedItemsCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="VisitedItemsCollection"/>.</returns>
        /// <seealso cref="IEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (int item in this)
            {
                yield return item;
            }
        }

        #endregion IEnumerable Members
    }
}
