using System;
using System.Collections.Generic;
using System.IO;

namespace Extract.Utilities
{
    /// <summary>
    /// Maintains temporary copies of specified source files. This includes updating the temporary
    /// copies with changes from the source in a way that doesn't interfere with an other object
    /// instances that are still using the old version.
    /// </summary>
    public class TemporaryFileCopyManager : IDisposable
    {
        /// <summary>
        /// A class to manage temporary local copies of one or more "master" files in order to prevent
        /// sharing violations.
        /// </summary>
        class TemporaryFileCopy : IDisposable
        {
            /// <summary>
            /// Used to store a local copy of the database if necessary.
            /// </summary>
            TemporaryFile _localTemporaryFile;

            /// <summary>
            /// The source file.
            /// </summary>
            string _originalDatabaseFileName;

            /// <summary>
            /// The last time the source file was modified.
            /// </summary>
            DateTime _lastModificationTime;

            /// <summary>
            /// Keeps track of the file copy each <see cref="object"/> instance is referencing.
            /// </summary>
            Dictionary<object, TemporaryFile> _temporaryFileReferences =
                new Dictionary<object, TemporaryFile>();

            /// <summary>
            /// Keeps track of all <see langword="object"/> instances referencing each local
            /// file copy.
            /// </summary>
            Dictionary<TemporaryFile, List<object>> _objectReferences =
                new Dictionary<TemporaryFile, List<object>>();

            /// <summary>
            /// Indicates whether the contents of the temporary file may be sensitive.
            /// </summary>
            bool _sensitive;

            #region Constructors

            /// <summary>
            /// Initializes a new <see cref="TemporaryFileCopy"/> instance.
            /// </summary>
            /// <param name="referencingInstance">The <see langword="object"/> that is
            /// creating/referencing the <see cref="TemporaryFileCopy"/>.</param>
            /// <param name="originalDatabaseFileName">The name of the source database. This
            /// database will be used directly only if it is not being accessed via a network share.
            /// </param>
            /// <param name="sensitive"><see langword="true"/>if the contents of the temporary file
            /// may be sensitive; otherwise, <see langword="false"/>.</param>
            public TemporaryFileCopy(object referencingInstance,
                string originalDatabaseFileName, bool sensitive)
            {
                try
                {
                    _originalDatabaseFileName = originalDatabaseFileName;

                     _lastModificationTime = File.GetLastWriteTime(_originalDatabaseFileName);

                     _sensitive = sensitive;
                    _localTemporaryFile = new TemporaryFile(sensitive);
                    File.Copy(originalDatabaseFileName, _localTemporaryFile.FileName, true);

                    AddReference(referencingInstance);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27585", ex);
                }
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            /// The filename of the temporary copy to use. The temporary copy will be created
            /// if necessary. Also, if the original file has been updated since the previous copy
            /// was last created or updated, a new local copy created from the updated original
            /// and the path of the new local copy will be returned.
            /// </summary>
            /// <param name="referencingInstance">The <see langword="object"/> instance for
            /// which the path to the local database copy is needed. The database at the path
            /// specified is guaranteed to exist unmodified until the next call to GetFileName from
            /// the specified instance. (or until this <see cref="TemporaryFileCopy"/> instance is
            /// disposed)</param>
            /// <returns>The filename of the local database copy to use.</returns>
            public string GetCurrentTemporaryFileName(object referencingInstance)
            {
                try
                {
                    DateTime modificationTime = File.GetLastWriteTime(_originalDatabaseFileName);

                    // If the original file has been modified, copy it to a new temporary file.
                    if (modificationTime != _lastModificationTime)
                    {
                        _localTemporaryFile = new TemporaryFile(_sensitive);

                        _lastModificationTime = modificationTime;
                        File.Copy(_originalDatabaseFileName, _localTemporaryFile.FileName, true);
                    }

                    // Update the reference for the specified orderMapperInstance so that it
                    // references the new _localTemporaryFile and not the now outdated
                    // temporary file (the outdated one will be deleted if this is the last
                    // instance that was referencing it).
                    UpdateReference(referencingInstance);

                    return _localTemporaryFile.FileName;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30120", ex);
                }
            }

            /// <summary>
            /// Notifies the <see cref="TemporaryFileCopy"/> of an <see langword="object"/>
            /// instance that is referencing it.
            /// </summary>
            /// <param name="referencingInstance">The <see langword="object"/> that is
            /// referencing the <see cref="TemporaryFileCopy"/>.</param>
            public void AddReference(object referencingInstance)
            {
                _temporaryFileReferences[referencingInstance] = _localTemporaryFile;

                List<object> fileReferences;
                if (!_objectReferences.TryGetValue(_localTemporaryFile, out fileReferences))
                {
                    fileReferences = new List<object>(new object[] { referencingInstance });
                    _objectReferences[_localTemporaryFile] = fileReferences;
                }
                else if (!fileReferences.Contains(referencingInstance))
                {
                    fileReferences.Add(referencingInstance);
                }
            }

            /// <overloads>Notifies the <see cref="TemporaryFileCopy"/> of an
            /// <see langword="object"/> instance that is no longer referencing it.
            /// </overloads>
            /// <summary>
            /// Notifies the <see cref="TemporaryFileCopy"/> of an <see cref="object"/> instance
            /// that is not longer referencing it.
            /// </summary>
            /// <param name="referencingInstance">The <see langword="object"/> that is no
            /// longer referencing the <see cref="TemporaryFileCopy"/>.</param>
            /// <returns><see langword="true"/> if there are no more <see langword="object"/>
            /// instances referencing the <see cref="TemporaryFileCopy"/>; <see langword="false"/>
            /// otherwise.</returns>
            public bool Dereference(object referencingInstance)
            {
                TemporaryFile temporaryFile;
                if (_temporaryFileReferences.TryGetValue(referencingInstance, out temporaryFile))
                {
                    // If the orderMapperInstance still references an existing temporary file,
                    // remove the reference.
                    _temporaryFileReferences.Remove(referencingInstance);

                    List<object> orderMapperReferences = _objectReferences[temporaryFile];
                    orderMapperReferences.Remove(referencingInstance);

                    // If no other references are found for this temporary file, the temporary file
                    // can be disposed of.
                    if (orderMapperReferences.Count == 0)
                    {
                        if (_localTemporaryFile == temporaryFile)
                        {
                            _localTemporaryFile = null;
                        }

                        _objectReferences.Remove(temporaryFile);
                        temporaryFile.Dispose();
                    }
                }

                return _temporaryFileReferences.Count == 0;
            }

            /// <summary>
            /// Ensures the specified <see paramref="orderMapperInstance"/> is referencing the
            /// current temporary file copy. Removes references to old file copies if necessary.
            /// </summary>
            /// <param name="referencingInstance">The <see langword="object"/> instance for
            /// which temporary file reference needs to be updated.</param>
            void UpdateReference(object referencingInstance)
            {
                TemporaryFile temporaryFile;
                if (!_temporaryFileReferences.TryGetValue(referencingInstance, out temporaryFile))
                {
                    AddReference(referencingInstance);
                }
                else if (temporaryFile != _localTemporaryFile)
                {
                    Dereference(referencingInstance);
                    AddReference(referencingInstance);
                }
            }

            #endregion Methods

            #region IDisposable Members

            /// <summary>
            /// Releases all resources used by the <see cref="TemporaryFileCopy"/>.
            /// </summary>
            /// 
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <overloads>Releases resources used by the <see cref="TemporaryFileCopy"/>.
            /// </overloads>
            /// <summary>
            /// Releases all unmanaged resources used by the <see cref="TemporaryFileCopy"/>.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Dispose of managed objects
                    List<object> referencingInstances = new List<object>(_temporaryFileReferences.Keys);
                    foreach (object referencingInstance in referencingInstances)
                    {
                        Dereference(referencingInstance);
                    }

                    if (_localTemporaryFile != null)
                    {
                        _localTemporaryFile.Dispose();
                        _localTemporaryFile = null;
                    }
                }

                // Dispose of unmanaged resources
            }

            #endregion IDisposable Members
        }

        /// <summary>
        /// Object for mutexing temporary file copy creation.
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// For each file path provided, keeps track of the local copy of each file to use.
        /// </summary>
        Dictionary<string, TemporaryFileCopy> _localFileCopies =
            new Dictionary<string, TemporaryFileCopy>();

        /// <summary>
        /// The filename of the temporary copy of the specified source file to use. The temporary
        /// copy will be created if necessary. Also, if the original file has been updated since
        /// the previous copy was last created or updated, a new local copy created from the
        /// updated original and the path of the new local copy will be returned.
        /// <para><b>Note</b></para>
        /// <see cref="Dereference"/> should be called to clean up no longer needed temporary
        /// copies. All existing temporary files will be deleted once the 
        /// <see cref="TemporaryFileCopyManager"/> is disposed, however.
        /// </summary>
        /// <param name="originalFileName">The file for which a temporary copy is needed.</param>
        /// <param name="referencingInstance">The <see langword="object"/> instance for
        /// which the path to the local database copy is needed. The database at the path
        /// specified is guaranteed to exist unmodified until the next call to
        /// GetCurrentTemporaryFileName using the specified instance.</param>
        /// <param name="sensitive"><see langword="true"/>if the contents of the temporary file
        /// may be sensitive; otherwise, <see langword="false"/>.</param>
        /// <returns>The filename of the temporary copy to use.</returns>
        public string GetCurrentTemporaryFileName(string originalFileName,
            object referencingInstance, bool sensitive)
        {
            try
            {
                // Lock to ensure multiple copies of the same database aren't created.                
                lock (_lock)
                {
                    TemporaryFileCopy temporaryFileCopy;

                    // If there is not an existing TemporaryFileCopy instance available for the
                    // specified database, create a new one.                    
                    if (!_localFileCopies.TryGetValue(originalFileName, out temporaryFileCopy))
                    {
                        temporaryFileCopy = new TemporaryFileCopy(this, originalFileName, sensitive);
                        _localFileCopies[originalFileName] = temporaryFileCopy;
                    }

                    return temporaryFileCopy.GetCurrentTemporaryFileName(referencingInstance);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30544", ex);
            }
        }

        /// <summary>
        /// Notifies the <see cref="TemporaryFileCopyManager"/> of an <see cref="object"/> instance
        /// that is not longer referencing the specified file.
        /// </summary>
        /// <param name="originalFileName">The file for which the temporary copy is no longer
        /// needed.</param>
        /// <param name="referencingInstance">The <see langword="object"/> that is no
        /// longer referencing the file.</param>
        public void Dereference(string originalFileName, object referencingInstance)
        {
            try
            {
                // Lock to ensure multiple copies of the same database aren't created.                
                lock (_lock)
                {
                    // Remove any existing reference to a local database copy. Dispose of the
                    // local database copy if this was the last instance referencing it.
                    TemporaryFileCopy temporaryFileCopy;             
                    if (!_localFileCopies.TryGetValue(originalFileName, out temporaryFileCopy))
                    {
                        if (temporaryFileCopy.Dereference(referencingInstance))
                        {
                            _localFileCopies.Remove(originalFileName);
                            temporaryFileCopy.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30545", ex);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="TemporaryFileCopyManager"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="TemporaryFileCopyManager"/>.
        /// </overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="TemporaryFileCopyManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    foreach (TemporaryFileCopy temporaryFileCopy in _localFileCopies.Values)
                    {
                        temporaryFileCopy.Dispose();
                    }
                }
            }
        }

        #endregion IDisposable Members
    }
}
