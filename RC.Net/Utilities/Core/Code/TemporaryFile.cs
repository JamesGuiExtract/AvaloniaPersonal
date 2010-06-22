using Extract.Licensing;
using System;
using System.IO;

namespace Extract.Utilities
{
    /// <summary>
    /// Creates a unique temporary file and will ensure that the file is deleted
    /// when <see cref="Dispose()"/> is called.  Useful to use in a using statement.
    /// </summary>
    /// <example>Using TemporaryFile<para/>
    /// <code lang="C#">
    /// using(TemporaryFile tempFile = new TemporaryFile())
    /// {
    ///     performFileOperations(tempFile.FileName);
    /// } // The temporary file will be deleted no matter how the using statement is exited
    /// </code>
    /// </example>
    public sealed class TemporaryFile : IDisposable
    {
        #region Fields

        /// <summary>
        /// The object name used in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(TemporaryFile).ToString();

        /// <summary>
        /// The name of the temporary file that was generated
        /// </summary>
        string _fileName;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new <see cref="TemporaryFile"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="TemporaryFile"/> class. The resulting temporary
        /// file will be in the TEMP directory and have the extension ".tmp".
        /// </summary>
        public TemporaryFile() : this(Path.GetTempPath(), ".tmp")
        {
        }

        /// <summary>
        /// Initializes a new <see cref="TemporaryFile"/> class. The resulting temporary
        /// file will be in the TEMP folder and have the extension specified by
        /// <paramref name="extension"/>.
        /// </summary>
        /// <param name="extension">The extension for the temporary file.</param>
        public TemporaryFile(string extension) : this(Path.GetTempPath(), extension)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="TemporaryFile"/> class. The resulting temporary
        /// file will be created in the folder specified by <paramref name="folder"/>
        /// and have the extension specified by <paramref name="extension"/>.
        /// </summary>
        /// <param name="extension">The extension for the temporary file.</param>
        /// <param name="folder">The folder to create the temporary file in. Must not be
        /// <see langword="null"/> or the empty string. The specified folder must exist
        /// on the system.</param>
        public TemporaryFile(string folder, string extension)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30260", _OBJECT_NAME);

                _fileName = FileSystemMethods.GetTemporaryFileName(folder, extension);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25512", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="TemporaryFile"/> class to manage the file
        /// specified in the <see cref="FileInfo"/> object.
        /// </summary>
        /// <param name="fileInfo">A <see cref="FileInfo"/> object for the file to be managed
        /// by the <see cref="TemporaryFile"/> object.</param>
        public TemporaryFile(FileSystemInfo fileInfo)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30261", _OBJECT_NAME);

                _fileName = fileInfo.FullName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25571", ex);
            }
        }

        #endregion Constructors

        #region Destructors

        /// <summary>
        /// Implement finalize to ensure the temporary file is deleted even when Dispose is not
        /// called. Though using Finalize is generally not advisable, since COM objects such as
        /// LabResultsCustomComponents.OrderMapper may not have their dispose method called, this
        /// ensures files don't get left behind.
        /// </summary>
        ~TemporaryFile()
        {
            Dispose(false);
        }

        #endregion Destructors

        #region Properties

        /// <summary>
        /// Gets the name of the temporary file.
        /// </summary>
        /// <returns>The name of the temporary file.</returns>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Copies the specified file to a temporary file having the same extension
        /// and returns the <see cref="TemporaryFile"/> containing the reference
        /// to the temporary file.
        /// </summary>
        /// <param name="fileName">The name of the file to copy.</param>
        /// <returns>The temporary file that was created and copied to.</returns>
        public static TemporaryFile CopyToTemporaryFile(string fileName)
        {
            TemporaryFile tempFile = null;
            try
            {
                // Check that the argument is valid
                bool isNullOrEmpty = string.IsNullOrEmpty(fileName);
                if (isNullOrEmpty || !File.Exists(fileName))
                {
                    throw new ArgumentException(
                        (isNullOrEmpty ? "Argument cannot be null or empty." : "File must exist."),
                        "fileName");
                }

                tempFile = new TemporaryFile(Path.GetExtension(fileName));
                File.Copy(fileName, tempFile.FileName, true);

                return tempFile;
            }
            catch (Exception ex)
            {
                // If an exception occurred, ensure the temp file is cleaned up.
                if (tempFile != null)
                {
                    tempFile.Dispose();
                }

                ExtractException ee = ExtractException.AsExtractException("ELI30262", ex);
                ee.AddDebugData("File Name", fileName, false);
                throw ee;
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="TemporaryFile"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="TemporaryFile"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="TemporaryFile"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources

            }

            // Dispose of ummanaged resources
            try
            {
                // If the temporary file still exists then delete it.
                if (!string.IsNullOrEmpty(_fileName) && File.Exists(_fileName))
                {
                    if (disposing)
                    {
                        // Try delete and log any exceptions, do not throw exceptions
                        // from Dispose.
                        ExtractException ex;
                        if (FileSystemMethods.TryDeleteFile(_fileName, out ex))
                        {
                            _fileName = null;
                        }
                        else
                        {
                            ExtractException.Log("ELI25511", ex);
                        }
                    }
                    else
                    {
                        // Do not use FileSystemMethods.TryDeleteFile since it creates an exception and
                        // allocating memory in finalizers is not advised.
                        File.Delete(_fileName);
                    }
                }
            }
            catch
            {
                // Just eat any expections... don't use ExtractException to log (which would result
                // in allocated memory which we don't want to do in a finalizer.
            }
        }

        #endregion
    }
}
