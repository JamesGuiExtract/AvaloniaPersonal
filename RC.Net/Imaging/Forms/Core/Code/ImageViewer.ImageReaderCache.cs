using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Extract.Imaging.Forms
{
    partial class ImageViewer
    {
        /// <summary>
        /// A thread-safe class for pre-loading and caching an <see cref="ImageReader"/> for a
        /// specified file.
        /// </summary>
        class ImageReaderCache : IDisposable
        {
            #region Statics

            /// <summary>
            /// The currently cached <see cref="ImageReader"/>s.
            /// </summary>
            static volatile Dictionary<string, ImageReaderCache> _cachedReaders =
                new Dictionary<string, ImageReaderCache>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// Protects access to _cachedReaders
            /// </summary>
            static object _staticLock = new object();

            #endregion Statics

            #region Fields

            /// <summary>
            /// Protects loading/retrieval of the <see cref="ImageReader"/>.
            /// </summary>
            object _lock = new object();

            /// <summary>
            /// The name of the file for which a reader is to be cached.
            /// </summary>
            string _filename;

            /// <summary>
            /// The modification of the file as of the time the reader was cached.
            /// </summary>
            public DateTime _fileModificationTime;

            /// <summary>
            /// The cached <see cref="ImageReader"/>
            /// </summary>
            ImageReader _reader;

            /// <summary>
            /// <see cref="ImageReader"/>s which had been cached, but are now stale and should no
            /// longer be used.
            /// </summary>
            List<ImageReader> _oldReaders = new List<ImageReader>();

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new <see cref="ImageReaderCache"/> instance.
            /// </summary>
            /// <param name="filename">The name of the document for which an
            /// <see cref="ImageReader"/> should be cached.</param>
            ImageReaderCache(string filename)
            {
                try
                {
                    _filename = filename;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30747", ex);
                }
            }

            #endregion Constructors

            #region Static methods

            /// <summary>
            /// Loads into the cache an <see cref="ImageReader"/> for the specified file using the
            /// specified <see paramref="codecs"/>.
            /// </summary>
            /// <param name="fileName">The name of the file for which and <see cref="ImageReader"/>
            /// should be cached.</param>
            /// <param name="codecs">The <see cref="ImageCodecs"/> to use to create the
            /// <see cref="ImageReader"/> if one has not already been cached.</param>
            public static void CacheReader(string fileName, ImageCodecs codecs)
            {
                ImageReaderCache cacheEntry = null;
                try
                {
                    // Look for a previously cached reader.
                    lock (_staticLock)
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
            public static ImageReader GetReader(string fileName, ImageCodecs codecs,
                bool cacheIfMissing)
            {
                ImageReaderCache cacheEntry = null;

                try
                {
                    // Look for a previously cached reader.
                    lock (_staticLock)
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
            public static bool RemoveReader(string fileName, ImageReader activeReader = null)
            {
                try
                {
                    bool removedSpecifiedReader = false;

                    lock (_staticLock)
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

            /// <summary>
            /// Disposes of everything currently in the image reader cache.
            /// </summary>
            public static void DisposeCache()
            {
                CollectionMethods.ClearAndDispose(_cachedReaders);
            }

            #endregion Static methods

            #region IDisposable

            /// <overloads>Releases resources used by the <see cref="ImageReaderCache"/>.
            /// </overloads>
            /// <summary>
            /// Releases all resources used by the <see cref="ImageReaderCache"/>.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases all unmanaged resources used by the <see cref="ImageReaderCache"/>.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
            void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Dispose of managed resources

                    if (_reader != null)
                    {
                        _reader.Dispose();
                        _reader = null;
                    }
                    CollectionMethods.ClearAndDispose(_oldReaders);
                }

                // Dispose of unmanaged resources
            }

            #endregion IDisposable

            #region Private methods

            /// <summary>
            /// Gets and <see cref="ImageReader"/> for this <see cref="ImageReaderCache"/> instance.
            /// </summary>
            /// <param name="codecs">The <see cref="ImageCodecs"/> to use to create the
            /// <see cref="ImageReader"/> if one has not already been cached. If
            /// <see langword="null"/>, a previously cached reader may be returned, but if no reader
            /// is currently cached, <see langword="null"/> will be returned.</param>
            /// <returns>The <see cref="ImageReader"/> for this cache entry.</returns>
            ImageReader GetReader(ImageCodecs codecs)
            {
                try
                {
                    lock (_lock)
                    {
                        // If an ImageCodecs was not provided to create a new ImageReader or the
                        // current ImageReader is up-to-date, return it.
                        if (codecs == null ||
                            (_reader != null &&
                                File.GetLastWriteTime(_filename) == _fileModificationTime))
                        {
                            return _reader;
                        }

                        // Keep track of stale readers so they can be disposed of later. Do not
                        // dispose of it now in case it is still be referenced outside this class.
                        if (_reader != null)
                        {
                            _oldReaders.Add(_reader);
                        }

                        // Create an initialize the new ImageReader.
                        _fileModificationTime = File.GetLastWriteTime(_filename);
                        _reader = codecs.CreateReader(_filename);
                        _reader.CachePage(1);
                        return _reader;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30748", ex);
                }
            }

            #endregion Private methods
        }
    }
}
