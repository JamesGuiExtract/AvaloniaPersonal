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
        /// A class for pre-loading and caching an <see cref="ImageReader"/> for a specified file.
        /// </summary>
        class ImageReaderCache : IDisposable
        {
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
            internal ImageReader _reader;

            /// <summary>
            /// <see cref="ImageReader"/>s which had been cached, but are now stale and should no
            /// longer be used.
            /// </summary>
            internal List<ImageReader> _oldReaders = new List<ImageReader>();

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new <see cref="ImageReaderCache"/> instance.
            /// </summary>
            /// <param name="filename">The name of the document for which an
            /// <see cref="ImageReader"/> should be cached.</param>
            internal ImageReaderCache(string filename)
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

            #region Methods

            /// <summary>
            /// Gets an <see cref="ImageReader"/> for this <see cref="ImageReaderCache"/> instance.
            /// </summary>
            /// <param name="codecs">The <see cref="ImageCodecs"/> to use to create the
            /// <see cref="ImageReader"/> if one has not already been cached. If
            /// <see langword="null"/>, a previously cached reader may be returned, but if no reader
            /// is currently cached, <see langword="null"/> will be returned.</param>
            /// <returns>The <see cref="ImageReader"/> for this instance.</returns>
            internal ImageReader GetReader(ImageCodecs codecs)
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

            #endregion Methods

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
        }
    }
}
