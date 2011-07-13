using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Extract.Testing.Utilities
{
    /// <summary>
    /// Manages different test images.
    /// </summary>
    public class TestFileManager<T> : IDisposable
    {
        #region TestImageManager Fields

        /// <summary>
        /// A <see cref="Dictionary{T, K}"/> keyed by the resource identifiers and containing
        /// the corresponding temporary files.
        /// </summary>
        Dictionary<string, TemporaryFile> _files = new Dictionary<string, TemporaryFile>();

        /// <summary>
        /// The assembly that this <see cref="TestImageManager"/> is associated with.
        /// This is the assembly that will be used to retrieve the embedded resource images.
        /// </summary>
        Assembly _assembly = Assembly.GetAssembly(typeof(T));

        #endregion TestImageManager Fields

        #region TestImageManager Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestImageManager"/> class.
        /// </summary>
        public TestFileManager()
        {
        }

        #endregion TestImageManager Constructors

        #region TestImageManager Methods

        /// <overloads>
        /// Get the temporary file name corresponding to the specified resource name.
        /// </overloads>
        /// <summary>
        /// Get the temporary file name corresponding to the specified resource name.
        /// </summary>
        /// <param name="resourceName">The resource to get a temporary file for.
        /// Must not be <see langword="null"/> or empty string.</param>
        /// <returns>The name of the temporary file.</returns>
        public string GetFile(string resourceName)
        {
            try
            {
                return GetFile(resourceName, null);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25572", ex);
            }
        }

        /// <summary>
        /// Get the temporary file name corresponding to the specified resource name.
        /// </summary>
        /// <param name="resourceName">The resource to get a temporary file for.
        /// Must not be <see langword="null"/> or empty string.</param>
        /// <param name="fileName">The name of the temporary file to create. If
        /// <see langword="null"/> or empty string then an auto-generated temporary
        /// file name will be used.  Otherwise the resource will be streamed to the
        /// specified file name.</param>
        /// <returns>The name of the temporary file.</returns>
        public string GetFile(string resourceName, string fileName)
        {
            try
            {
                // Ensure the resourceName is not null or empty string
                ExtractException.Assert("ELI25567", "Resource name cannot be null or empty!",
                    !string.IsNullOrEmpty(resourceName));

                TemporaryFile tempFile;
                if (!_files.TryGetValue(resourceName, out tempFile))
                {
                    // Create the temporary image file
                    tempFile = CreateTemporaryFile(resourceName, fileName);

                    // Add the temporary file to the dictionary
                    _files.Add(resourceName, tempFile);
                }
                else if (fileName != null && 
                    !fileName.Equals(tempFile.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    // Move the file to the new location
                    File.Move(tempFile.FileName, fileName);

                    // Remove the old file from the collection and dispose of the
                    // temporary file
                    _files.Remove(resourceName);
                    tempFile.Dispose();

                    // Create a new temporary file with the specified name
                    // and add it to the collection
                    tempFile = new TemporaryFile(new FileInfo(fileName), false);
                    _files.Add(resourceName, tempFile);
                }

                return tempFile.FileName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25514", ex);
            }
        }

        /// <summary>
        /// Removes the temporary file for the specified resource from the system.
        /// </summary>
        /// <param name="resourceName">The name of the resource.
        /// Must not be <see langword="null"/> or empty string.</param>
        public void RemoveFile(string resourceName)
        {
            try
            {
                // Ensure the resourceName is not null or empty string
                ExtractException.Assert("ELI25568", "Resource name cannot be null or empty!",
                    !string.IsNullOrEmpty(resourceName));

                // Check if the temporary file is contained in the collection
                TemporaryFile tempFile;
                if (_files.TryGetValue(resourceName, out tempFile))
                {
                    // Dispose the temporary file and remove it from the collection
                    _files.Remove(resourceName);
                    tempFile.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25565", ex);
            }
        }

        /// <summary>
        /// Creates a temporary file from an embedded resource.
        /// </summary>
        /// <param name="resource">The name of the embedded resource to stream
        /// to the temporary file.</param>
        /// <param name="fileName">The name to assign to the temporary file. If
        /// <see langword="null"/> or empty string will auto-generate a temporary
        /// file name.</param>
        /// <returns>A <see cref="TemporaryFile"/> for the embedded resource.</returns>
        TemporaryFile CreateTemporaryFile(string resource, string fileName)
        {
            // If specified file name is null or empty just generate a new temp file.
            // If a file name is specified then associate a tempoary file with the specified name.
            TemporaryFile tempFile = null;
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    tempFile = new TemporaryFile(false);
                }
                else
                {
                    // Create a new TemporaryFile associated with the specified file name
                    FileInfo fileInfo = new FileInfo(fileName);
                    tempFile = new TemporaryFile(fileInfo, false);
                }

                GeneralMethods.WriteEmbeddedResourceToFile<T>(_assembly,
                    resource, tempFile.FileName);

                return tempFile;
            }
            catch
            {
                if (tempFile != null)
                {
                    tempFile.Dispose();
                }

                throw;
            }
        }

        #endregion TestImageManager Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="TestImageManager"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="TestImageManager"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="TestImageManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of each of the temporary files (this will delete the files)
                if (_files != null)
                {
                    CollectionMethods.ClearAndDispose(_files);
                    _files = null;
                }
            }

            // No unmanaged resources
        }

        #endregion IDisposable Members
    }
}
