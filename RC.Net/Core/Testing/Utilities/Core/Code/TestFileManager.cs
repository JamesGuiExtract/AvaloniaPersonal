using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Extract.Testing.Utilities
{
    /// <summary>
    /// Manages different test images.
    /// </summary>
    public class TestFileManager<T> : IDisposable
    {
        #region TestFileManager Fields

        /// <summary>
        /// A <see cref="Dictionary{T, K}"/> keyed by the resource identifiers and containing
        /// the corresponding temporary files.
        /// </summary>
        Dictionary<string, TemporaryFile> _files = new Dictionary<string, TemporaryFile>();

        /// <summary>
        /// The name of a subdirectory in the temp directory where any test files should be placed
        /// </summary>
        string _subdirectoryName;

        /// <summary>
        /// The assembly that this <see cref="TestFileManager{T}"/> is associated with.
        /// This is the assembly that will be used to retrieve the embedded resource images.
        /// </summary>
        Assembly _assembly = Assembly.GetAssembly(typeof(T));

        #endregion TestFileManager Fields

        #region TestFileManager Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFileManager{T}"/> class.
        /// </summary>
        /// <param name="subdirectoryName">The name of a subdirectory in the temp directory where
        /// any test files should be placed (when an explicit file path is not specified). If
        /// <c>null</c>, a subdirectory name will be generated.</param>
        public TestFileManager(string subdirectoryName = null)
        {
            if (subdirectoryName == null)
            {
                _subdirectoryName = UtilityMethods.FormatInvariant($"{typeof(T).Name}_{Guid.NewGuid()}");
            }
            else
            {
                _subdirectoryName = subdirectoryName;
            }
        }

        #endregion TestFileManager Constructors

        #region TestFileManager Methods

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

                // If this resource was already retrieved, remove the previous file
                if (_files.TryGetValue(resourceName, out var previousFile))
                {
                    previousFile.Dispose();
                }

                // Create the temporary image file
                TemporaryFile tempFile = CreateTemporaryFile(resourceName, fileName);

                // I have run into at least one unit test where I periodically getting errors
                // stating the test file is missing or sharing violations.
                FileSystemMethods.WaitForFileToBeReadable(tempFile.FileName);

                // Add the temporary file to the collection
                _files[resourceName] = tempFile;

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
        /// <param name="fileName">The name to assign to the temporary file.  If
        /// <see langword="null"/> or empty string then an auto-generated temporary
        /// file name will be used. Otherwise the resource will be streamed to the
        /// specified file name.</param>
        /// <returns>A <see cref="TemporaryFile"/> for the embedded resource.</returns>
        TemporaryFile CreateTemporaryFile(string resource, string fileName)
        {
            // If specified file name is null or empty just generate a new temp file.
            // If a file name is specified then associate a temporary file with the specified name.
            TemporaryFile tempFile = null;
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    tempFile = GeneralMethods.WriteEmbeddedResourceToTemporaryFile<T>(
                        _assembly, resource, _subdirectoryName);
                }
                else
                {
                    var directory = Path.GetDirectoryName(fileName);
                    Directory.CreateDirectory(directory);

                    // Create a new TemporaryFile associated with the specified file name
                    FileInfo fileInfo = new FileInfo(fileName);
                    tempFile = new TemporaryFile(fileInfo, false);
                    
                    GeneralMethods.WriteEmbeddedResourceToFile<T>(
                        _assembly, resource, tempFile.FileName);
                }

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

        /// <summary>
        /// Check whether the specified resource name (scoped by type param's namespace) exists
        /// </summary>
        /// <param name="resource">The resource name to check</param>
        public bool ResourceExists(string resource)
        {
            return _assembly.GetManifestResourceStream(typeof(T), resource) != null;
        }

        #endregion TestFileManager Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="TestFileManager{T}"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            // Prevent nunit error after tests are run:
            // "Unhandled Exception: NUnit.Engine.NUnitEngineException: Remote test agent exited with non-zero exit code -2146233020"
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <overloads>Releases resources used by the <see cref="TestFileManager{T}"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="TestFileManager{T}"/>.
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

            if (!string.IsNullOrWhiteSpace(_subdirectoryName))
            {
                string directory = Path.Combine(Path.GetTempPath(), _subdirectoryName);
                for (int tries = 1; Directory.Exists(directory); tries++)
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch (Exception e)
                    when (tries < 5 && (e is IOException || e is UnauthorizedAccessException))
                    {
                        Thread.Sleep(500);
                    }
                }
            }
        }

        #endregion IDisposable Members
    }
}
