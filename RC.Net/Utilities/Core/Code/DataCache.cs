using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extract.Licensing;
using System.Collections.Concurrent;
using System.Threading;

namespace Extract.Utilities
{
    /// <summary>
    /// Produces a score indicating the likely benefit of a particular data item to be cached by
    /// <see cref="DataCache{TKeyType, TDataType}"/> relative to other data items. If space is needed
    /// in the cache, items with a lower score will be evicted prior to items with higher scores.
    /// </summary>
    /// <typeparam name="TDataType">The type of data in the cache.</typeparam>
    /// <param name="dataInstance"></param>
    /// <returns>A score indicating the likely benefit of a particular data item to be cached
    /// relative to other data items.</returns>
    public delegate double ScoreCachedData<TDataType>(TDataType dataInstance);
    
    /// <summary>
    /// Provides generic management of data to be cached where the size of the cache can be capped
    /// in such a way as to preserve in the cache, the data whose caching produces the most benefit
    /// over data that does not.
    /// </summary>
    /// <typeparam name="TKeyType">The data type used to look up cached data items. Must implement
    /// <see cref="IComparable{T}"/>.</typeparam>
    /// <typeparam name="TDataType">The type of data to be cached.</typeparam>
    public class DataCache<TKeyType, TDataType>
        where TKeyType : IComparable<TKeyType>
        where TDataType : class
    {
        #region Constants
        
        /// <summary>
        /// The object name used in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataCache<TKeyType, TDataType>).ToString();

        #endregion Constants

        #region DataItem

        /// <summary>
        /// A wrapper for each data item to be cached; includes data needed for ranking of how
        /// beneficial it is for this data is to be cached.
        /// </summary>
        class DataItem : IComparable<DataItem>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DataCache{TKeyType, TDataType}.DataItem"/>
            /// class.
            /// </summary>
            /// <param name="key">The key with which the data item will be retrieved.</param>
            /// <param name="data">The data to cache.</param>
            /// <param name="score">A score indicating how beneficial it is for this data item is to
            /// be cached.</param>
            public DataItem(TKeyType key, TDataType data, double score)
            {
                Key = key;
                Data = data;
                Score = score;
            }

            /// <summary>
            /// Gets or sets the key with which the data item will be retrieved.
            /// </summary>
            /// <value>
            /// The key with which the data item will be retrieved.
            /// </value>
            public TKeyType Key
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the data to cache.
            /// </summary>
            /// <value>
            /// The data to cache.
            /// </value>
            public TDataType Data
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a score indicating how beneficial it is for this data item is to be
            /// cached.
            /// </summary>
            /// <value>
            /// The score indicating how beneficial it is for this data item is to be cached.
            /// </value>
            public double Score
            {
                get;
                set;
            }

            /// <summary>
            /// Compares the current instance with another <see cref="DataItem"/> and returns an
            /// integer that indicates whether the current instance precedes, follows, or occurs in
            /// the same position in the sort order as the other <see cref="DataItem"/>.
            /// </summary>
            /// <param name="other">A <see cref="DataItem"/> to compare with this instance.</param>
            /// <returns>Less than zero if this instance precedes <see paramref="other"/>;
            /// Zero if this instance is in the same position as <see paramref="other"/>;
            /// Greater than zero if this instance follows <see paramref="other"/>.</returns>
            public int CompareTo(DataItem other)
            {
                // If other is not a valid object reference, this instance is greater.
                if (other == null)
                {
                    return 1;
                }

                // Compare based on score.
                int score = Score.CompareTo(other.Score);

                // Use the key as a tie breaker to ensure this data item never returns zero when
                // compared to another data item (otherwise DataCache._rankedItem will treat this
                // as a duplicate of already cached data).
                return (score == 0)
                    ? Key.CompareTo(other.Key)
                    : score;
            }
        }

        #endregion DataItem

        #region Fields

        /// <summary>
        /// The maximum number of <see typeref="TDataType"/> instances that can be cached.
        /// </summary>
        int _maxCacheCount;

        /// <summary>
        /// To prevent cache-thrashing, when the cache is full or nearly full, new data must score
        /// at least in this percentile of the cached data in order to added to the cache.
        /// </summary>
        int _qualifyingPercentile = 20;

        /// <summary>
        /// The _rankedData index indicated by _qualifyingPercentile which must be matched for new
        /// items to be added to a cache that is already full.
        /// </summary>
        int _qualifyingIndex;

        /// <summary>
        /// The method that is to be used to score <see typeref="TDataType"/> instances in the cache.
        /// </summary>
        ScoreCachedData<TDataType> _scoreDataDelegate;

        /// <summary>
        /// If a <see cref="_scoreDataDelegate"/> is not provided, use _defaultScore as a value that
        /// gets incremented every time it is accessed to score data elements in the order they were
        /// last cached or accessed.
        /// </summary>
        double _defaultScore = 0;

        /// <summary>
        /// To allow for quick retrieval of cached data referenced by a key.
        /// </summary>
        ConcurrentDictionary<TKeyType, DataItem> _cachedData;

        /// <summary>
        /// To rank the cached data items based on their score.
        /// </summary>
        SortedSet<DataItem> _rankedData = new SortedSet<DataItem>();

        /// <summary>
        /// Synchronizes access to cached items.
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCache{TKeyType, TDataType}"/> class.
        /// </summary>
        /// <param name="maxCacheCount">The maximum number of <see typeref="TDataType"/> instances
        /// that can be cached.</param>
        /// <param name="scoreDataDelegate">Delegate to produce a score indicating the likely
        /// benefit of a particular data item to be cached. If <see langword="null"/>, data will be
        /// ranked according to how recently it was cached/accessed with items that have been
        /// cached/accessed more recently scored higher.</param>
        public DataCache(int maxCacheCount, ScoreCachedData<TDataType> scoreDataDelegate)
        {
            try
            {
                // Verify this object is either licensed
                if (!LicenseUtilities.IsLicensed(LicenseIdName.ExtractCoreObjects))
                {
                    var ee = new ExtractException("ELI34470", "Object is not licensed.");
                    ee.AddDebugData("Object Name", _OBJECT_NAME, false);
                    throw ee;
                }

                MaxCacheCount = maxCacheCount;
                _scoreDataDelegate = scoreDataDelegate;
                _cachedData = new ConcurrentDictionary<TKeyType, DataItem>();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34471");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the maximum number of <see typeref="TDataType"/> instances that can be
        /// cached.
        /// </summary>
        /// <value>
        /// The maximum number of <see typeref="TDataType"/> instances that can be cached.
        /// </value>
        public int MaxCacheCount
        {
            get
            {
                return _maxCacheCount;
            }

            set
            {
                try
                {
                    if (value != _maxCacheCount)
                    {
                        _maxCacheCount = value;
                        _qualifyingIndex = (_maxCacheCount * _qualifyingPercentile / 100);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34473");
                }
            }
        }

        /// <summary>
        /// Gets or sets the percentile (0 - 99) in which new data must score compared to the
        /// currently cached data in order to be added to the cache. (Higher values prevent
        /// cache-thrashing at the risk of preventing data from being cached that would be more
        /// useful to cache than some already cached data.
        /// </summary>
        /// <value>
        /// The percentile (0 - 99) in which new data must score compared to the currently cached
        /// data in order to added to the cache. 
        /// </value>
        public int QualifyingPercentile
        {
            get
            {
                return _qualifyingPercentile;
            }

            set
            {
                try
                {
                    if (value != _qualifyingPercentile)
                    {
                        ExtractException.Assert("ELI34474",
                            "QualifyingPercentile must be in the range 0 - 100",
                            value >= 0 && value < 100);

                        _qualifyingPercentile = value;
                        _qualifyingIndex = (_maxCacheCount * _qualifyingPercentile / 100);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34475");
                }
            }
        }
        
        #endregion Properties

        #region Methods

        /// <summary>
        /// Tries to retrieve the <see typeref="TDataType"/> instance referenced by
        /// <see paramref="key"/> from the cache.
        /// </summary>
        /// <param name="key">The key which references the <see typeref="TDataType"/> instance to
        /// retrieve.</param>
        /// <param name="data">The <see typeref="TDataType"/> instance referenced by
        /// <see paramref="key"/> or <see langword="null"/> if the data was not found in the cache.
        /// </param>
        /// <returns><see langword="true"/> if the data was successfully retrieved or
        /// <see langword="false"/> if the data was not found in the cache.</returns>
        public bool TryGetData(TKeyType key, out TDataType data)
        {
            try
            {
                DataItem dataItem;
                if (_cachedData.TryGetValue(key, out dataItem))
                {
                    data = dataItem?.Data;
                    if (data == null)
                    {
                        return false;
                    }

                    bool lockTaken = false;
                    try
                    {
                        Monitor.TryEnter(_lock, ref lockTaken);

                        // Re-rank the retrieved data based on an updated score every time it is
                        // retrieved-- but for performance, don't hang if the lock is already taken.
                        if (lockTaken)
                        {
                            _rankedData.Remove(dataItem);
                            dataItem.Score = (_scoreDataDelegate == null)
                                ? _defaultScore++
                                : _scoreDataDelegate(data);
                            _rankedData.Add(dataItem);
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(_lock);
                        }
                    }

                    return true;
                }

                data = null;
                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34476");
            }
        }

        /// <summary>
        /// Requests that <see paramref="data"/> be added to the cache.
        /// </summary>
        /// <param name="key">The <see typeref="TKeyType"/> value that will be used to reference the
        /// data. (Must implement IComparable{TKeyType} and be unique amongst all data cached.</param>
        /// <param name="data">The <see typeref="TDataType"/> instance to cache.</param>
        /// <returns><see langword="true"/> if the data was added to the cache,
        /// <see langword="false"/> if the data was not added to the cache because the cache is full
        /// (or nearly full) and the data's score was not in at least the
        /// <see cref="QualifyingPercentile"/> of already cached data.
        /// </returns>
        public bool CacheData(TKeyType key, TDataType data)
        {
            try
            {
                if (_maxCacheCount == 0)
                {
                    return false;
                }

                lock (_lock)
                {
                    // If data that is already cached is being re-cached, re-add the data in case it has
                    // been updated.
                    DataItem existingData;
                    if (_cachedData.TryGetValue(key, out existingData))
                    {
                        _rankedData.Remove(existingData);
                        _cachedData.TryRemove(key, out var value);
                    }

                    double score = (_scoreDataDelegate == null)
                        ? _defaultScore++
                        : _scoreDataDelegate(data);

                    // Check if the score meets the QualifyingPercentile if the cache is nearly full to
                    // minimize cache-thrashing. Find the index that represents QualifyingPercentile.
                    // This index should be decreased by the number of open slots such that if more
                    // that QualifyingPercentile of the cache's slots are open, the data will be added
                    // regardless of its score. No need for this check if _scoreDataDelegate has not
                    // been provided.
                    if (_scoreDataDelegate != null)
                    {
                        int referenceIndex = _qualifyingIndex - (MaxCacheCount - _cachedData.Count);
                        if (referenceIndex >= 0)
                        {
                            if (score <= _rankedData.ElementAt(referenceIndex).Score)
                            {
                                return false;
                            }
                        }
                    }

                    // Wrap the data in a DataItem.
                    var newItem = new DataItem(key, data, score);

                    // If the cache if full, evict the lowest scoring DataItem that is currently cached
                    // to make room for the new item.
                    if (_cachedData.Count >= _maxCacheCount)
                    {
                        DataItem dataItemToEvict = _rankedData.First();
                        _rankedData.Remove(dataItemToEvict);
                        _cachedData.TryRemove(dataItemToEvict.Key, out var value);
                    }

                    _cachedData[key] = newItem;
                    _rankedData.Add(newItem);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34477");
            }
        }

        /// <summary>
        /// Outputs the keys for the currently cached <see cref="DataItem"/>s prefixed by their
        /// respective scores (higher score = more expensive to generate), in order from most to
        /// least expensive.
        /// </summary>
        /// <returns>The keys for the currently cached <see cref="DataItem"/>s.</returns>
        public IEnumerable<string> ReportCachedData()
        {
            List<DataItem> dataList = null;
            lock (_lock)
            {
                dataList = _rankedData.Reverse().ToList();
            }

            foreach (DataItem item in dataList)
            {
                yield return item.Score.ToString() + ": " + item.Key;
            }
        }

        /// <summary>
        /// Clears all data from the cache.
        /// </summary>
        public void Clear()
        {
            try
            {
                lock (_lock)
                {
                    _cachedData.Clear();
                    _rankedData.Clear();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34478");
            }
        }

        #endregion Methods
    }
}
