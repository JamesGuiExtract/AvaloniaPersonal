using System;
using System.Collections.Generic;
using System.Text;
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
    public class TemporaryFile : IDisposable
    {
        #region Fields

        /// <summary>
        /// The name of the temporary file that was generated
        /// </summary>
        private string _fileName;

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
        public TemporaryFile(FileInfo fileInfo)
        {
            try
            {
                _fileName = fileInfo.FullName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25571", ex);
            }
        }

        #endregion Constructors

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
        protected virtual void Dispose(bool disposing)
        {
            // Dispose of managed resources
            if (disposing)
            {
                // If the temporary file still exists then delete it.
                if (File.Exists(_fileName))
                {
                    // Try delete and log any exceptions, do not throw exceptions
                    // from Dispose
                    ExtractException ex;
                    if (!FileSystemMethods.TryDeleteFile(_fileName, out ex))
                    {
                        ExtractException.Log("ELI25511", ex);
                    }
                }
            }
        }

        #endregion
    }
}
