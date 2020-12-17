using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Extract.Utilities.SecureFileDeleters
{
    /// <summary>
    /// A base class for secure file delete implementations that provides most of the code that
    /// should be required by any implementation.
    /// </summary>
    // This class is intentionally missing a default constructor so that it cannot be directly
    // instantiated via COM. Only derived classes such as DoD5220E should be used.
    [SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable")]
    [ComVisible(true)]
    public class SecureFileDeleterBase
    {
        #region Constants

        /// <summary>
        /// The buffer size to use when overwriting files with data.
        /// </summary>
        static readonly int _BUFFER_SIZE = 8192;

        /// <summary>
        /// When obfuscating file data, the most recent time to assign to any of the files'
        /// date/time stamps.
        /// </summary>
        static readonly DateTime _RANDOM_DATE_TIME_MAX =
            DateTime.Now - new TimeSpan(365, 0, 0, 0);

        /// <summary>
        /// The number of seconds in a year.
        /// </summary>
        const int _SECONDS_PER_YEAR = 365 * 24 * 60 * 60;

        /// <summary>
        /// When obfuscating file data, the span of time prior to _RANDOM_DATE_TIME_MAX the files'
        /// date/time stamps may be set to.
        /// </summary>
        static readonly int _RANDOM_TIME_SPAN = _SECONDS_PER_YEAR * 3;

        #endregion Constants

        #region Delegates

        /// <summary>
        /// A delegate to be used to provide data that should be used to overwrite a file in an
        /// overwrite pass.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="bytesToWrite">The number of bytes to fill the buffer with</param>
        internal protected delegate void OverwritePass(Byte[] buffer, int bytesToWrite);

        #endregion Delegates

        #region Fields

        /// <summary>
        /// Generates random numbers for the <see cref="GetRandomDateTime"/> method.
        /// </summary>
        static Random _randomGenerator = new Random();

        /// <summary>
        /// The registry settings from which the secure file delete options are to be retrieved.
        /// </summary>
        // Intentionally not dynamic since the values will not be read dynamically on the c++ side.
        readonly RegistrySettings<Properties.Settings> _registry;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureFileDeleterBase"/> class.
        /// </summary>
        internal protected SecureFileDeleterBase()
        {
            try 
	        {
                _registry = new RegistrySettings<Properties.Settings>(
                    @"SOFTWARE\Extract Systems\ReusableComponents\Extract.Utilities");

                OverwritePasses = new Collection<OverwritePass>();
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI32840");
	        }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The calls that are to provide data for overwriting the files' data. The number of
        /// overwrite passes are determined by the number of calls in this collection.
        /// </summary>
        /// <value>
        /// The calls that are to provide data for overwriting the files' data.
        /// </value>
        internal protected Collection<OverwritePass> OverwritePasses
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to verify that the data written in the overwrite
        /// passes was written correctly.
        /// </summary>
        /// <value><see langword="true"/> to verify the file was overwritten as intended; otherwise,
        /// <see langword="false"/>.</value>
        internal protected bool Verify
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the name and date/time stamps should be
        /// obfuscated before deletion.
        /// </summary>
        /// <value><see langword="true"/> if the name and date/time stamps; otherwise,
        /// <see langword="false"/>.</value>
        internal protected bool Obfuscate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of times to rename files before deletion. This value is used
        /// only if <see cref="Obfuscate"/> is <see langword="true"/>.
        /// </summary>
        /// <value>
        /// The number of times to rename files before deletion
        /// </value>
        internal protected int RenameRepetitions
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Securely deletes the specified <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">Name of the file to securely delete.</param>
        /// <param name="throwIfUnableToDeleteSecurely"><see langword="true"/> if an exception should
        /// be thrown before actually deleting the file if the file could not be securely
        /// overwritten prior to deletion. If <see langword="false"/>, problems overwriting the file
        /// will be logged if the <see cref="Properties.Settings.LogSecureDeleteErrors"/> value is
        /// <see langword="true"/>, otherwise they will be ignored.</param>
        /// <param name="doRetries"><see langword="true"/> if retries should be attempted when
        /// sharing violations occur; <see langword="false"/> otherwise.</param>
        [ComVisible(false)]
        public virtual void SecureDeleteFile(string fileName, bool throwIfUnableToDeleteSecurely,
            bool doRetries)
        {
            try
            {
                FileSystemMethods.ValidateFileExistence(fileName, "ELI32841");

                FileAttributes attributes = File.GetAttributes(fileName);
                ExtractException.Assert("ELI32904", "Cannot delete readonly file.",
                    !attributes.HasFlag(FileAttributes.ReadOnly));

                Collection<ExtractException> exceptions = new Collection<ExtractException>();

                if (doRetries)
                {
                    FileSystemMethods.PerformFileOperationWithRetry(() =>
                    {
                        SecureDeleteFileHelper(fileName, throwIfUnableToDeleteSecurely, exceptions);
                    },
                    true);
                }
                else
                {
                    SecureDeleteFileHelper(fileName, throwIfUnableToDeleteSecurely, exceptions);
                }

                // If the file was deleted, there were no problems overwriting the file or
                // throwIfUnableToDeleteSecurely is false. Log any the exceptions if specified.
                if (exceptions.Count > 0 && _registry.Settings.LogSecureDeleteErrors)
                {
                    ExtractException aggregateException = new ExtractException("ELI32855",
                        "An error occurred while securely deleting a file; the file may be recoverable.",
                        exceptions.AsAggregateException());
                    aggregateException.AddDebugData("Filename", fileName, false);
                    aggregateException.Log();
                }
            }
            catch (Exception ex)
            {
                // If the file was not deleted, either is throwIfUnableToDeleteSecurely true and the
                // the file was not properly overwritten, or the delete failed outright. Throw an
                // exception.
                ExtractException ee = new ExtractException("ELI32867",
                    "An error occurred while securely deleting a file; the file could not be deleted.",
                    ex);
                ee.AddDebugData("Filename", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// A helper function for <see cref="SecureDeleteFile"/> that makes one attempt at securely
        /// deleting <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">Name of the file to securely delete.</param>
        /// <param name="throwIfUnableToDeleteSecurely"><see langword="true"/> if an exception should
        /// be thrown before actually deleting the file if the file could not be securely
        /// overwritten prior to deletion. If <see langword="false"/>, problems overwriting the file
        /// will be logged if the <see cref="Properties.Settings.LogSecureDeleteErrors"/> value is
        /// <see langword="true"/>, otherwise they will be ignored.</param>
        /// <param name="exceptions">If <see paramref="throwIfUnableToDeleteSecurely"/> is 
        /// <see langword="false"/>, any exceptions encountered should be added to this
        /// <see cref="Collection{T}"/>.</param>
        private void SecureDeleteFileHelper(string fileName, bool throwIfUnableToDeleteSecurely,
            Collection<ExtractException> exceptions)
        {
            // Overwrite the file as configured. This includes setting the file size to zero
            // after completing the overwriting.
            if (OverwritePasses.Count() > 0)
            {
                // Throws any exceptions, if so configured. Otherwise they are added to the
                // passed collection.
                OverwriteFile(fileName, throwIfUnableToDeleteSecurely, exceptions);
            }

            // Keep track of the file name through rename repetitions.
            string currentFileName = fileName;

            // Obfuscate the file name and timestamps as configured.
            if (Obfuscate)
            {
                // Any exceptions are added to the passed collection.
                ObfuscateFileAttributes(fileName, exceptions);

                if (RenameRepetitions > 0)
                {
                    // Any exceptions are added to the passed collection.
                    currentFileName = RenameFile(fileName, RenameRepetitions, exceptions);
                }
            }

            // Finally, delete the file.
            File.Delete(currentFileName);
        }

        #endregion Methods

        #region Protected Members

        /// <summary>
        /// Overwrites the data in the specified <see paramref="fileName"/> and sets the file size
        /// to zero to make it unrecoverable.
        /// </summary>
        /// <param name="fileName">Name of the file to overwrite.</param>
        /// <param name="throwIfUnableToDeleteSecurely"><see langword="true"/> if an exception
        /// should be thrown if there are any problems overwriting the file or the security of the
        /// overwrite cannot be guaranteed, <see langword="false"/> to add any such exceptions
        /// <see paramref="exceptions"/>.</param>
        /// <param name="exceptions">If <see paramref="throwIfUnableToDeleteSecurely"/> is 
        /// <see langword="false"/>, any exceptions encountered should be added to this
        /// <see cref="Collection{T}"/>.</param>
        internal protected virtual void OverwriteFile(string fileName, bool throwIfUnableToDeleteSecurely,
            Collection<ExtractException> exceptions)
        {
            try
            {
                FileAttributes attributes = File.GetAttributes(fileName);

                try
                {
                    // Verify that the file does not have any attributes that could prevent an
                    // overwrite from rendering the file unrecoverable.
                    ExtractException.Assert("ELI32843",
                        "File could not be securely deleted because it is compressed.",
                        !attributes.HasFlag(FileAttributes.Compressed));
                    ExtractException.Assert("ELI32844",
                        "File could not be securely deleted because it is encrypted.",
                        !attributes.HasFlag(FileAttributes.Encrypted));
                    ExtractException.Assert("ELI32845",
                        "File could not be securely deleted because it is a sparse file.",
                        !attributes.HasFlag(FileAttributes.SparseFile));
                }
                catch (Exception ex)
                {
                    // If throwIfUnableToDeleteSecurely is set, throw the exception before
                    // overwriting to provide the user the opportunity to review what could
                    // not be securely deleted.
                    ExtractException ee = ex.AsExtract("ELI32869");
                    if (throwIfUnableToDeleteSecurely)
                    {
                        throw ee;
                    }
                    else
                    {
                        exceptions.Add(ee);
                    }
                }

                // Open a FileStream in WriteThrough mode to ensure the written data goes straight
                // to disk rather that only to a cache.
                using (FileStream fileStream = new FileStream(fileName, FileMode.Open,
                    FileAccess.ReadWrite, FileShare.None, _BUFFER_SIZE, FileOptions.WriteThrough))
                {
                    long fileLength = (int)fileStream.Length;

                    // Create a writeBuffer and a readBuffer to verify if Verify is set.
                    byte[] writeBuffer = new byte[Math.Min(_BUFFER_SIZE, fileLength)];
                    byte[] readBuffer = Verify ? new byte[Math.Min(_BUFFER_SIZE, fileLength)] : null;

                    // Loop once for each configured OverwritePass
                    foreach (OverwritePass overwritePass in OverwritePasses)
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);

                        // Overwrite the file _BUFFER_SIZE bytes at a time until the file is
                        // completely overwritten.
                        int bytesInBlock;
                        for (long bytesToWrite = fileLength;
                                bytesToWrite > 0;
                                bytesToWrite -= bytesInBlock)
                        {
                            // Get the needed number of bytes for this pass.
                            bytesInBlock = (int)Math.Min(writeBuffer.Length, bytesToWrite);

                            // Call the OverwritePass delegate to obtain the data to overwrite.
                            overwritePass(writeBuffer, bytesInBlock);

                            // Write the data.
                            fileStream.Write(writeBuffer, 0, bytesInBlock);
                            fileStream.Flush();

                            // Verify the data was written correctly (if specified).
                            if (Verify)
                            {
                                VerifyData(fileStream, writeBuffer, readBuffer, bytesInBlock);
                            }
                        }
                    }

                    // Set the length of the file to zero as another layer protection against the
                    // file being recovered.
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.SetLength(0);
                    fileStream.Close();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI32846");
                if (throwIfUnableToDeleteSecurely)
                {
                    throw ee;
                }
                else
                {
                    exceptions.Add(ee);
                }
            }
        }

        /// <summary>
        /// Verifies that the bytes at the end of the <see paramref="stream"/> match the bytes in
        /// <see paramref="writeBuffer"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to verify.</param>
        /// <param name="writeBuffer">The data the <see cref="Stream"/> should end with.</param>
        /// <param name="readBuffer">A buffer to store data read from the <see paramref="stream"/>.
        /// </param>
        /// <param name="bytesToVerify">The number of bytes to verify.</param>
        /// <requires><see paramref="readBuffer"/> and <see paramref="readBuffer"/> must be at least
        /// as large as <see paramref="bytesToVerify"/>.</requires>
        /// <throws><see cref="ExtractException"/> if the verification fails.</throws>
        internal protected static void VerifyData(Stream stream, byte[] writeBuffer, byte[] readBuffer,
            int bytesToVerify)
        {
            try
            {
                ExtractException.Assert("ELI32870", "Internal logic error.",
                    readBuffer.Length >= bytesToVerify && writeBuffer.Length >= bytesToVerify);

                // Read back from the file the data that was just written.
                stream.Seek(-bytesToVerify, SeekOrigin.Current);
                int bytesVerified;
                for (bytesVerified = stream.Read(readBuffer, 0, bytesToVerify);
                     bytesVerified != 0 && bytesVerified < bytesToVerify;
                     bytesVerified += stream.Read(readBuffer, 0, bytesToVerify - bytesVerified))

                // Verify all bytes were read back correctly.
                ExtractException.Assert("ELI32848", "Overwrite verification failed.",
                    bytesVerified == bytesToVerify);

                for (int i = 0; i < bytesToVerify; i++)
                {
                    if (readBuffer[i] != writeBuffer[i])
                    {
                        throw new ExtractException("ELI32849", "Overwrite verification failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32851");
            }
        }

        /// <summary>
        /// Renames the specified file to a random name the specified number of times.
        /// </summary>
        /// <param name="fileName">The name of the file to rename.</param>
        /// <param name="count">The number of times to rename the file.</param>
        /// <param name="exceptions">Any exceptions encountered will be added to this
        /// <see paramref="Collection{T}"/>.</param>
        /// <returns>The final name of the file after the renaming.</returns>
        internal protected virtual string RenameFile(string fileName, int count,
            Collection<ExtractException> exceptions)
        {
            string currentFileName = fileName;

            try
            {
                // Rename in the same directory to ensure the file will not actually be copied then
                // deleted. This also guards against the possibility of different permissions in a
                // different directory.
                string directoryName = Path.GetDirectoryName(fileName);

                // Loop for each rename attempt.
                for (int i = 0; i < count; i++)
                {
                    string newFileName = FileSystemMethods.GetTemporaryFileName(directoryName);
                    if (File.Exists(newFileName))
                    {
                        // Ensure the file name doesn't already exist.
                        i--;
                        continue;
                    }

                    File.Move(currentFileName, newFileName);

                    currentFileName = newFileName;

                    // Re-obfuscate the file attributes each time for good measure.
                    ObfuscateFileAttributes(newFileName, exceptions);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex.AsExtract("ELI32847"));
            }

            return currentFileName;
        }

        /// <summary>
        /// Sets the file to not indexed and assigns random date/time stamps.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="exceptions">The exceptions.</param>
        internal protected static void ObfuscateFileAttributes(string fileName,
            Collection<ExtractException> exceptions)
        {
            try
            {
                FileAttributes attributes = File.GetAttributes(fileName);

                // This prevents the file from being visible in many applications (including windows
                // explorer).
                if (!attributes.HasFlag(FileAttributes.NotContentIndexed))
                {
                    attributes |= FileAttributes.NotContentIndexed;
                    File.SetAttributes(fileName, attributes);
                }

                // Obscure the file's original date and time.
                DateTime randomDateTime = GetRandomDateTime();
                File.SetCreationTime(fileName, randomDateTime);
                File.SetLastAccessTime(fileName, randomDateTime);
                File.SetLastWriteTime(fileName, randomDateTime);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex.AsExtract("ELI32850"));
            }
        }

        /// <summary>
        /// Returns a random <see cref="DateTime"/> from any time in the
        /// <see cref="_RANDOM_TIME_SPAN"/> prior to <see cref="_RANDOM_DATE_TIME_MAX"/>.
        /// </summary>
        /// <returns>The random <see cref="DateTime"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        internal protected static DateTime GetRandomDateTime()
        {
            try
            {
                TimeSpan timeSpan = new TimeSpan(0, 0, _randomGenerator.Next(_RANDOM_TIME_SPAN));
                DateTime randomDateTime = _RANDOM_DATE_TIME_MAX - timeSpan;
                return randomDateTime;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32852");
            }
        }

        /// <summary>
        /// Fills the provided <see paramref="buffer"/> with the same byte value.
        /// </summary>
        /// <param name="buffer">The buffer to fill.</param>
        /// <param name="bytesToWrite">The number of bytes to fill the buffer with</param>
        /// <param name="value">The <see langword="byte"/> that should fill the buffer.</param>
        internal protected virtual void StaticOverwrite(Byte[] buffer, int bytesToWrite, byte value)
        {
            try
            {
                int length = buffer.Length;
                for (int i = 0; i < length; i++)
                {
                    buffer[i] = value;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32853");
            }
        }

        /// <summary>
        /// Fills the provided <see paramref="buffer"/> with random data.
        /// </summary>
        /// <param name="buffer">The buffer to fill.</param>
        /// <param name="bytesToWrite">The number of bytes to fill the buffer with</param>
        internal protected virtual void RandomOverwrite(Byte[] buffer, int bytesToWrite)
        {
            try
            {
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(buffer);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32854");
            }
        }

        #endregion Protected Members
    }
}
