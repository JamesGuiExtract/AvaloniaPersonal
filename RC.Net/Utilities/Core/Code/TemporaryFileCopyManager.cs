﻿using System;
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
            /// Used to store a temporary local copy of the file.
            /// </summary>
            TemporaryFile _localTemporaryFile;

            /// <summary>
            /// The source file.
            /// </summary>
            string _originalFileName;

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
            /// <param name="originalFileName">The name of the source file. This file will be used
            /// directly only if it is not being accessed via a network share.
            /// </param>
            /// <param name="sensitive"><see langword="true"/>if the contents of the temporary file
            /// may be sensitive; otherwise, <see langword="false"/>.</param>
            public TemporaryFileCopy(object referencingInstance,
                string originalFileName, bool sensitive)
            {
                try
                {
                    _originalFileName = originalFileName;

                    _lastModificationTime = File.GetLastWriteTime(_originalFileName);

                    _sensitive = sensitive;
                    _localTemporaryFile = new TemporaryFile(sensitive);
                    FileSystemMethods.PerformFileOperationWithRetry(() =>
                        File.Copy(originalFileName, _localTemporaryFile.FileName, true),
                        true);

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
            /// Determines whether _originalFileName file has been modified since last creating a
            /// temporary copy.
            /// </summary>
            /// <returns><see langword="true"/> if there currently is a temporary copy and
            /// _originalFileName has been modified; otherwise, <see langword="false"/>.
            /// </returns>
            public bool HasFileBeenModified()
            {
                try
                {
                    if (string.IsNullOrEmpty(_originalFileName))
                    {
                        return false;
                    }
                    else
                    {
                        DateTime modificationTime =
                            File.GetLastWriteTime(_originalFileName);

                        return (modificationTime != _lastModificationTime);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37776");
                }
            }

            /// <summary>
            /// The filename of the temporary copy to use. The temporary copy will be created
            /// if necessary. Also, if the original file has been updated since the previous copy
            /// was last created or updated, a new local copy created from the updated original
            /// and the path of the new local copy will be returned.
            /// </summary>
            /// <param name="referencingInstance">The <see langword="object"/> instance for
            /// which the path to the local copy is needed. The at the path specified is guaranteed
            /// to exist unmodified until the next call to GetFileName from the specified instance.
            /// (or until this <see cref="TemporaryFileCopy"/> instance is disposed)</param>
            /// <returns>The filename of the local file copy to use.</returns>
            public string GetCurrentTemporaryFileName(object referencingInstance)
            {
                try
                {
                    DateTime modificationTime = File.GetLastWriteTime(_originalFileName);

                    // If the original file has been modified, copy it to a new temporary file.
                    if (modificationTime != _lastModificationTime)
                    {
                        _localTemporaryFile = new TemporaryFile(_sensitive);

                        _lastModificationTime = File.GetLastWriteTime(_originalFileName);
                        FileSystemMethods.PerformFileOperationWithRetry(() =>
                            File.Copy(_originalFileName, _localTemporaryFile.FileName, true),
                            true);
                    }

                    // Update the reference for the specified instance so that it references the new
                    // _localTemporaryFile and not the now outdated temporary file (the outdated one
                    // will be deleted if this is the last instance that was referencing it).
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
                    // If the instance still references an existing temporary file, remove the
                    // reference.
                    _temporaryFileReferences.Remove(referencingInstance);

                    List<object> objectReferences = _objectReferences[temporaryFile];
                    objectReferences.Remove(referencingInstance);

                    // If no other references are found for this temporary file, the temporary file
                    // can be disposed of.
                    if (objectReferences.Count == 0)
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
            /// Ensures the specified <see paramref="referencingInstance"/> is referencing the
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
        /// Determines whether <see paramref="originalFileName"/> has been modified since last
        /// creating a temporary copy of it.
        /// </summary>
        /// <param name="originalFileName">The name of the file to check for modification.</param>
        /// <returns><see langword="true"/> if there currently is a temporary copy and
        /// <see paramref="originalFileName"/> has been modified; otherwise, <see langword="false"/>.
        /// </returns>
        public bool HasFileBeenModified(string originalFileName)
        {
            try
            {
                TemporaryFileCopy temporaryFileCopy;

                // Consider the file unmodified if we do not currently have a temporary copy of it.
                if (!_localFileCopies.TryGetValue(originalFileName, out temporaryFileCopy))
                {
                    return false;
                }

                return temporaryFileCopy.HasFileBeenModified();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37777");
            }
        }

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
        /// which the path to the local copy is needed. The file at the path specified is
        /// guaranteed to exist unmodified until the next call to GetCurrentTemporaryFileName using
        /// the specified instance or until it is explicitly dereferenced by all referencing
        /// instances.</param>
        /// <param name="sensitive"><see langword="true"/> if the contents of the temporary file may
        /// be sensitive; otherwise, <see langword="false"/>.</param>
        /// <param name="makeWritable"><see langword="true"/> if the temporary file copy should be
        /// made writable regardless of the original copy's status.; otherwise,
        /// <see langword="false"/>.</param>
        /// <returns>The filename of the temporary copy to use.</returns>
        public string GetCurrentTemporaryFileName(string originalFileName,
            object referencingInstance, bool sensitive, bool makeWritable)
        {
            try
            {
                // Lock to ensure multiple copies of the same file aren't created.                
                lock (_lock)
                {
                    TemporaryFileCopy temporaryFileCopy;
                    string temporaryCopyFileName = null;

                    // If there is not an existing TemporaryFileCopy instance available for the
                    // specified file, create a new one.                    
                    if (!_localFileCopies.TryGetValue(originalFileName, out temporaryFileCopy))
                    {
                        temporaryFileCopy =
                            new TemporaryFileCopy(referencingInstance, originalFileName, sensitive);
                        _localFileCopies[originalFileName] = temporaryFileCopy;

                        temporaryCopyFileName =
                            temporaryFileCopy.GetCurrentTemporaryFileName(referencingInstance);

                        if (makeWritable)
                        {
                            FileAttributes fileAttributes = File.GetAttributes(temporaryCopyFileName);
                            if (fileAttributes.HasFlag(FileAttributes.ReadOnly))
                            {
                                File.SetAttributes(temporaryCopyFileName, 
                                    fileAttributes & ~FileAttributes.ReadOnly);
                            }
                        }
                    }

                    return temporaryCopyFileName ??
                        temporaryFileCopy.GetCurrentTemporaryFileName(referencingInstance);
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
                // Lock to ensure multiple copies of the same file aren't created.                
                lock (_lock)
                {
                    // Remove any existing reference to a local file copy. Dispose of the
                    // local copy if this was the last instance referencing it.
                    TemporaryFileCopy temporaryFileCopy;             
                    if (_localFileCopies.TryGetValue(originalFileName, out temporaryFileCopy))
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
