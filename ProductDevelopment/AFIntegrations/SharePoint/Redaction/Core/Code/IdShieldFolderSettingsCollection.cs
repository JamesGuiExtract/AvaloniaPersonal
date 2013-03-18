using Microsoft.SharePoint.Administration;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Extract.SharePoint
{
    /// <summary>
    /// An <see cref="SPPersistedObject"/> class containing a collection of
    /// <see cref="IdShieldFolderProcessingSettings"/>.
    /// </summary>
    [System.Runtime.InteropServices.GuidAttribute("FEB75691-0910-4E6E-ACA2-019C17D8C0DF")]
    internal class IdShieldFolderSettingsCollection : SPPersistedObject,
        IDictionary<Guid, IdShieldFolderProcessingSettings>
    {
        #region Fields

        /// <summary>
        /// Dictionary containing the collected settings. The key is the unique folder
        /// Id. The values are the settings for the folder.
        /// </summary>
        [Persisted]
        Dictionary<Guid, IdShieldFolderProcessingSettings> _collection =
            new Dictionary<Guid,IdShieldFolderProcessingSettings>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldFolderSettingsCollection"/> class.
        /// </summary>
        public IdShieldFolderSettingsCollection()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldFolderSettingsCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public IdShieldFolderSettingsCollection(SPPersistedObject parent)
            : base(Guid.NewGuid().ToString("N"), parent)
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Indicates whether this persisted class has additional update access
        /// </summary>
        /// <returns><see langword="true"/></returns>
        protected override bool HasAdditionalUpdateAccess()
        {
            return true;
        }

        #endregion Methods

        #region IDictionary<Guid,IdShieldFolderProcessingSettings> Members

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(Guid key, IdShieldFolderProcessingSettings value)
        {
            _collection.Add(key, value);
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///	<see langword="true"/> if the collection contains the specified key; otherwise,
        ///	<see langword="false"/>.
        /// </returns>
        public bool ContainsKey(Guid key)
        {
            return _collection.ContainsKey(key);
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<Guid> Keys
        {
            get
            {
                return _collection.Keys;
            }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><see langword="true"/> if the object was removed and
        /// <see langword="false"/> otherwise.</returns>
        public bool Remove(Guid key)
        {
            return _collection.Remove(key);
        }

        /// <summary>
        /// Tries to get the value from the collection.
        /// <para>If the value is found then <paramref name="value"/>
        /// will contain the value.</para>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><see langword="true"/> if the value is found and
        /// <see langword="false"/> otherwise.</returns>
        public bool TryGetValue(Guid key, out IdShieldFolderProcessingSettings value)
        {
            return _collection.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<IdShieldFolderProcessingSettings> Values
        {
            get
            {
                return _collection.Values;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Extract.SharePoint.IdShieldFolderProcessingSettings"/>
        /// with the specified key.
        /// </summary>
        /// <value>The <see cref="IdShieldFolderProcessingSettings"/> for the specified key.</value>
        public IdShieldFolderProcessingSettings this[Guid key]
        {
            get
            {
                return _collection[key];
            }
            set
            {
                _collection[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<Guid,IdShieldFolderProcessingSettings>> Members

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(KeyValuePair<Guid, IdShieldFolderProcessingSettings> item)
        {
            _collection.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _collection.Clear();
        }

        /// <summary>
        /// Determines whether the collection contains the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// <see langword="true"/> if specified item is contained; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Contains(KeyValuePair<Guid, IdShieldFolderProcessingSettings> item)
        {
            ICollection<KeyValuePair<Guid, IdShieldFolderProcessingSettings>> temp =
                _collection as ICollection<KeyValuePair<Guid, IdShieldFolderProcessingSettings>>;
            return temp.Contains(item);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(KeyValuePair<Guid, IdShieldFolderProcessingSettings>[] array, int arrayIndex)
        {
            ICollection<KeyValuePair<Guid, IdShieldFolderProcessingSettings>> temp =
                _collection as ICollection<KeyValuePair<Guid, IdShieldFolderProcessingSettings>>;
            temp.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return _collection.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is read only; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                ICollection<KeyValuePair<Guid, IdShieldFolderProcessingSettings>> temp =
                    _collection as ICollection<KeyValuePair<Guid, IdShieldFolderProcessingSettings>>;
                return temp.IsReadOnly;
            }
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><see langword="true"/> if the specified item is removed and
        /// <see langword="false"/> otherwise.</returns>
        public bool Remove(KeyValuePair<Guid, IdShieldFolderProcessingSettings> item)
        {
            ICollection<KeyValuePair<Guid, IdShieldFolderProcessingSettings>> temp =
                _collection as ICollection<KeyValuePair<Guid, IdShieldFolderProcessingSettings>>;

            return temp.Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<Guid,IdShieldFolderProcessingSettings>> Members

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator for the collection.</returns>
        IEnumerator<KeyValuePair<Guid, IdShieldFolderProcessingSettings>>
            IEnumerable<KeyValuePair<Guid, IdShieldFolderProcessingSettings>>.GetEnumerator()
        {
            IEnumerable<KeyValuePair<Guid, IdShieldFolderProcessingSettings>> ienumerable =
                _collection as IEnumerable<KeyValuePair<Guid, IdShieldFolderProcessingSettings>>;
            if (ienumerable != null)
            {
                return ienumerable.GetEnumerator();
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }
}
