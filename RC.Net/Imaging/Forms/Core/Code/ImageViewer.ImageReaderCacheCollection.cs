using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extract.Utilities;

namespace Extract.Imaging.Forms
{
    partial class ImageViewer
    {
        /// <summary>
        /// A thread-safe class for managing a collection of <see cref="ImageReaderCache"/>
        /// instances for pre-loading and caching an <see cref="ImageReader"/>s for multiple files.
        /// </summary>
        class ImageReaderCacheCollection : IDisposable
        {
            #region Fields

            /// <summary>
            /// The current set of <see cref="ImageReaderCache"/>s for each file.
            /// </summary>
            volatile Dictionary<string, ImageReaderCache> _cachedReaders =
                new Dictionary<string, ImageReaderCache>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// Protects access to _cachedReaders
            /// </summary>
            object _lock = new object();

            #endregion Fields

            #region Methods

            /// <summary>
            /// Loads into the cache an <see cref="ImageReader"/> for the specified file using the
            /// specified <see paramref="codecs"/>.
            /// </summary>
            /// <param name="fileName">The name of the file for which and <see cref="ImageReader"/>
            /// should be cached.</param>
            /// <param name="codecs">The <see cref="ImageCodecs"/> to use to create the
            /// <see cref="ImageReader"/> if one has not already been cached.</param>
            public void CacheReader(string fileName, ImageCodecs codecs)
            {
                ImageReaderCache cacheEntry = null;
                try
                {
                    // Look for a previously cached reader.
                    lock (_lock)
                    {
                        if (!_cachedReaders.TryGetValue(fileName, out cacheEntry))
                        {
                            cacheEntry = new ImageReaderCache(fileName);
                            _cachedReaders[fileName] = cacheEntry;
                        }
                    }

                    cacheEntry.GetReader(codecs);
                }
                catch (Exception ex)
                {
                    if (cacheEntry != null)
                    {
                        RemoveReader(fileName);
                    }

                    throw ExtractException.AsExtractException("ELI30722", ex);
                }
            }

            /// <summary>
            /// Retrieves an <see cref="ImageReader"/> for the specified file.
            /// </summary>
            /// <param name="fileName">The name of the file for which an
            /// <see cref="ImageReader"/> is needed.</param>
            /// <param name="codecs">The <see cref="ImageCodecs"/> to use to create the
            /// <see cref="ImageReader"/> if one has not already been cached. If
            /// <see langword="null"/>, a previously cached reader may be returned, but if no reader
            /// is currently cached, <see langword="null"/> will be returned.</param>
            /// <param name="cacheIfMissing"><see langword="true"/> to add the
            /// <see cref="ImageReader"/> to the cache if it is not already cached,
            /// <see langword="false"/> if a new <see cref="ImageReader"/> instance should not be
            /// cached.</param>
            /// <returns>An <see cref="ImageReader"/> for the specified file.</returns>
            public ImageReader GetReader(string fileName, ImageCodecs codecs,
                bool cacheIfMissing)
            {
                ImageReaderCache cacheEntry = null;

                try
                {
                    // Look for a previously cached reader.
                    lock (_lock)
                    {
                        if (!_cachedReaders.TryGetValue(fileName, out cacheEntry) &&
                            cacheIfMissing)
                        {
                            cacheEntry = new ImageReaderCache(fileName);
                            _cachedReaders[fileName] = cacheEntry;
                        }
                    }

                    if (cacheEntry == null)
                    {
                        // Create an uncached reader as long as a codecs was provided.
                        return (codecs == null) ? null : codecs.CreateReader(fileName);
                    }
                    else
                    {
                        // Retrieve a cached reader
                        return cacheEntry.GetReader(codecs);
                    }
                }
                catch (Exception ex)
                {
                    if (cacheEntry != null)
                    {
                        RemoveReader(fileName);
                    }

                    throw ExtractException.AsExtractException("ELI30723", ex);
                }
            }

            /// <summary>
            /// Removes the <see cref="ImageReader"/> for the specified file from the cache. If no
            /// such <see cref="ImageReader"/> is cached, the method has no effect.
            /// </summary>
            /// <param name="fileName">The name of the file for which any cached
            /// <see cref="ImageReader"/> should be removed.</param>
            /// <param name="activeReader">If not <see langword="null"/>, the return value will
            /// indicate if this instance was disposed when removing the cache entry.</param>
            /// <returns><see langword="true"/> if the specified <see paramref="activeReader"/> 
            /// was disposed of, <see langword="false"/> otherwise.</returns>
            public bool RemoveReader(string fileName, ImageReader activeReader = null)
            {
                try
                {
                    bool removedSpecifiedReader = false;

                    lock (_lock)
                    {
                        // Look for a cached entry for this file.
                        ImageReaderCache cachEntry;
                        if (_cachedReaders.TryGetValue(fileName, out cachEntry))
                        {
                            // If the cache entry contains the specified reader, it will be
                            // disposed of.
                            if (activeReader != null)
                            {
                                if (activeReader == cachEntry._reader ||
                                    cachEntry._oldReaders.Contains(activeReader))
                                {
                                    removedSpecifiedReader = true;
                                }
                            }

                            cachEntry.Dispose();
                            _cachedReaders.Remove(fileName);
                        }
                    }

                    return removedSpecifiedReader;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30724", ex);
                }
            }

            #endregion Methods

            #region IDisposable

            /// <overloads>Releases resources used by the <see cref="ImageReaderCacheCollection"/>.
            /// </overloads>
            /// <summary>
            /// Releases all resources used by the <see cref="ImageReaderCacheCollection"/>.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases all unmanaged resources used by the <see cref="ImageReaderCacheCollection"/>.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
            void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Dispose of managed resources
                    CollectionMethods.ClearAndDispose(_cachedReaders);
                }

                // Dispose of unmanaged resources
            }

            #endregion IDisposable
        }
    }
}
