using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.DataEntry
{
    /// <summary>
    /// Facilitates (in conjunction with <see cref="DataCache{T,T}"/> caching query data that is
    /// frequently used or expensive to calculate.
    /// </summary>
    internal class CachedQueryData<T>
    {
        #region Fields

        /// <summary>
        /// The time at which the process in general was started (used in the <see cref="GetScore"/>
        /// method.
        /// </summary>
        static DateTime _processStartTime = DateTime.Now;

        /// <summary>
        /// The number of milliseconds it took to calculate the data represented by this instance.
        /// </summary>
        double _executionTime;

        /// <summary>
        /// The number of times this data has been used.
        /// </summary>
        int _usageCount = 0;

        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedQueryData{T}"/> class.
        /// </summary>
        /// <param name="data">The data to (potentially) cache.</param>
        /// <param name="excecutionTime">The number of milliseconds it took to calculate the
        /// <see paramref="data"/>.</param>
        public CachedQueryData(T data, double excecutionTime)
        {
            _executionTime = excecutionTime;
            Data = data;
        }

        /// <summary>
        /// Gets the cached data.
        /// </summary>
        public T Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a score indicating the relative value of caching this <see cref="Data"/> as
        /// compared to other data in the cache. This score is used to determine which data to evict
        /// when the <see cref="DataCache{T,T}.MaxCacheCount"/> is reached.
        /// </summary>
        /// <param name="cachedQueryResult">The cached query result.</param>
        /// <returns>A score indicating the relative value of caching this <see cref="Data"/>
        /// compared to other data in the cache. </returns>
        public static double GetScore(CachedQueryData<T> cachedQueryResult)
        {
            try
            {
                // Increment the call count of this data.
                cachedQueryResult._usageCount++;

                // The base score is essentially the total time saved (in ms) by accessing this data
                // via the cache rather than re-computing it each time.
                double score = (cachedQueryResult._usageCount * cachedQueryResult._executionTime);

                // Over time slowly increase the base score (by 1 per minute) to favor recently used
                // data over old data even if the old data would otherwise score better. (Otherwise,
                // it would be hard for any new data to ever evict existing data that had been
                // accessed multiple times)... regardless of how long ago it was last accessed.
                score += (DateTime.Now - _processStartTime).TotalMinutes;

                return score;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34479");
            }
        }
    }
}
