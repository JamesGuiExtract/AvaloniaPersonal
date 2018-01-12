using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Keeps a cache of objects and monitors source files for changes
    /// </summary>
    [CLSCompliant(false)]
    public static class FileDerivedResourceCache
    {
        #region Constants

        const string _AUTO_ENCRYPT_KEY = @"Software\Extract Systems\AttributeFinder\Settings\AutoEncrypt";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Static finalizer to clean-up cached lucene search provider temporary files stored in the memory cache
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        static readonly Finalizer finalizer = new Finalizer();
        sealed class Finalizer
        {
            ~Finalizer()
            {
                try
                {
                    var cache = MemoryCache.Default;
                    foreach (var key in _sourceLists.Keys)
                    {
                        cache.Remove(key);                                                
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Locks to prevent multiple threads from trying to create the same suggestion provider
        /// This list is also used to clean-up temporary files on application exit
        /// </summary>
        static ConcurrentDictionary<string, object> _sourceLists =
            new ConcurrentDictionary<string, object>(StringComparer.Ordinal);

        /// <summary>
        /// A <see cref="ThreadLocal{MiscUtils}"/> for any thread that needs one
        /// </summary>
        static ThreadLocal<MiscUtils> _miscUtils = new ThreadLocal<MiscUtils>(() => new MiscUtilsClass());

        #endregion Fields

        #region Properties

        /// <summary>
        /// A <see cref="ThreadLocal{MiscUtils}"/> for any thread that needs one
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
        public static MiscUtils ThreadLocalMiscUtils => _miscUtils.Value;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets or creates an object based on a primary source path and the caller
        /// </summary>
        /// <typeparam name="T">The type of the cached object</typeparam>
        /// <param name="primaryPath">The paths of the files that the cached object is derived from</param>
        /// <param name="creator">A function to create the cached object</param>
        /// <param name="slidingExpiration">The time after last access before the cache entry is removed</param>
        /// <param name="removedCallback">A function that will be called when the cache entry is removed</param>
        /// <remarks>
        /// If any of the monitored paths end in ".etf" then auto-encryption will be run and
        /// the non-etf files will be monitored for changes as well.
        /// All cache entries will be removed if the application exits normally.
        /// </remarks>
        public static T GetCachedObject<T>(string path, Func<T> creator, TimeSpan slidingExpiration,
            CacheEntryRemovedCallback removedCallback)
            where T : class
        {
            return GetCachedObject(Enumerable.Repeat(path, 1), creator, slidingExpiration, removedCallback);
        }

        /// <summary>
        /// Gets or creates an object based on a primary source path and the caller
        /// </summary>
        /// <typeparam name="T">The type of the cached object</typeparam>
        /// <param name="primaryPath">The paths of the files that the cached object is derived from</param>
        /// <param name="creator">A function to create the cached object</param>
        /// <param name="slidingExpiration">The time after last access before the cache entry is removed</param>
        /// <param name="removedCallback">A function that will be called when the cache entry is removed</param>
        /// <remarks>
        /// If any of the monitored paths end in ".etf" then auto-encryption will be run and
        /// the non-etf files will be monitored for changes as well.
        /// All cache entries will be removed if the application exits normally.
        /// </remarks>
        public static T GetCachedObject<T>(IEnumerable<string> paths, Func<T> creator, TimeSpan slidingExpiration,
            CacheEntryRemovedCallback removedCallback)
            where T : class
        {
            try
            {
                List<string> monitorPaths = new List<string>();

                string key = typeof(T).AssemblyQualifiedName;
                foreach (var p in paths.OrderBy(p=>p))
                {
                    string fullPath = Path.GetFullPath(p);
                    key += fullPath;
                    monitorPaths.Add(fullPath);
                    if (fullPath.EndsWith(".etf", StringComparison.OrdinalIgnoreCase))
                    {
                        ThreadLocalMiscUtils.AutoEncryptFile(fullPath, _AUTO_ENCRYPT_KEY);
                        monitorPaths.Add(Path.ChangeExtension(fullPath, null));
                    }
                }

                var cache = MemoryCache.Default;
                var result = cache.Get(key) as T;
                if (result == null)
                {
                    lock (_sourceLists.GetOrAdd(key, _ => new object()))
                    {
                        result = cache.Get(key) as T;
                        if (result == null)
                        {
                            CacheItemPolicy policy = new CacheItemPolicy();
                            policy.ChangeMonitors.Add(new HostFileChangeMonitor(monitorPaths.Where(p => File.Exists(p)).ToList()));
                            policy.SlidingExpiration = slidingExpiration;
                            policy.RemovedCallback = removedCallback;

                            result = creator();
                            cache.Set(key, result, policy);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45451");
            }
        }

        #endregion Methods
    }
}
