using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace Extract.Utilities
{
    /// <summary>
    /// A utiity class of file system methods.
    /// </summary>
    public static class FileSystemMethods
    {
        /// <summary>
        /// A static object used as a mutex in the temp file name generation to prevent
        /// multiple threads from generating the same temporary file name.
        /// </summary>
        private static Mutex _tempFileLock = new Mutex(false,
            "C6D3EB7D-5DB9-4FC7-BEAD-0DBA39DBDB4B");

        /// <summary>
        /// Generates a file in the users TEMP folder with an extension of ".tmp".
        /// <para><b>Note:</b></para>
        /// The caller is responsible for deleting the temporary file. After
        /// calling this function the returned file will exist on the system.
        /// </summary>
        /// <overloads>This method has three overloads.</overloads>
        /// <returns>The name of the temporary file that was created.</returns>
        public static string GetTemporaryFileName()
        {
            try
            {
                // Create a file with a .tmp extension
                return GetTemporaryFileName(Path.GetTempPath(), ".tmp");
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25510", ex);
            }
        }

        /// <summary>
        /// Creates an empty file in the users TEMP folder with the specified
        /// extension.
        /// <para><b>Note:</b></para>
        /// The caller is responsible for deleting the temporary file. After
        /// calling this function the returned file will exist on the system.
        /// </summary>
        /// <param name="extension">The extension for the temporary file
        /// that will be generated.</param>
        /// <returns>The name of the temporary file that was created.</returns>
        public static string GetTemporaryFileName(string extension)
        {
            try
            {
                return GetTemporaryFileName(Path.GetTempPath(), extension);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22047", ex);
            }
        }

        /// <summary>
        /// Creates an empty file in the specified folder with the specified
        /// extension.
        /// <para><b>Note:</b></para>
        /// The caller is responsible for deleting the temporary file. After
        /// calling this function the returned file will exist on the system.
        /// </summary>
        /// <param name="extension">The extension for the temporary file
        /// that will be generated.</param>
        /// <param name="folder">The folder in which to create the temporary file. Must
        /// not be <see langword="null"/> or empty string. The folder must exist
        /// on the current system.</param>
        /// <exception cref="ExtractException">If the <paramref name="folder"/>
        /// is <see langword="null"/> or empty string.</exception>
        /// <exception cref="ExtractException">If the <paramref name="folder"/>
        /// specified does not exist.</exception>
        /// <returns>The name of the temporary file that was created.</returns>
        public static string GetTemporaryFileName(string folder, string extension)
        {
            try
            {
                ExtractException.Assert("ELI25508", "Specified folder cannot be found!",
                    !string.IsNullOrEmpty(folder) && Directory.Exists(folder),
                    "Folder Name", folder ?? "NULL");

                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".tmp";
                }
                // Ensure the extension starts with a '.'
                else if (extension[0] != '.')
                {
                    extension = "." + extension;
                }

                // Generate a temp file name
                string fileName = Path.Combine(folder, Path.GetRandomFileName() + extension);

                // Protect the file creation section with mutex
                _tempFileLock.WaitOne();

                // If the fileName already exists, generate a new file name
                while (File.Exists(fileName))
                {
                    fileName = Path.Combine(folder, Path.GetRandomFileName() + extension);
                }

                // File does not exist, create the file so that it will exist
                File.Create(fileName).Close();

                // Return the temporary file name
                return fileName;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI25509",
                    "Failed generating temporary file name!", ex);
            }
            finally
            {
                _tempFileLock.ReleaseMutex();
            }
        }

        /// <summary>
        /// Copies all files from one directory to another directory.  If 
        /// <paramref name="recursive"/> is <see lanword="true"/> will perform
        /// a recursive copy; if <see langword="false"/> will only copy the top level
        /// files.
        /// </summary>
        /// <param name="source">The source directory.</param>
        /// <param name="destination">The destination directory.</param>
        /// <param name="recursive">If <see langword="true"/> will perform a recursive
        /// copy.  If <see langword="false"/> copies top level files.</param>
        public static void CopyDirectory(string source, string destination, bool recursive)
        {
            try
            {
                // Ensure the destination path ends in the directory separator character
                if (destination[destination.Length - 1] != Path.DirectorySeparatorChar)
                {
                    destination += Path.DirectorySeparatorChar;
                }

                // If the destination doesn't exist, create it
                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }

                // For each element in the source directory copy it
                foreach (string element in Directory.GetFileSystemEntries(source))
                {
                    // Check if this is a directory
                    if (Directory.Exists(element))
                    {
                        // If it is a directory and recursion was specified,
                        // copy the sub-directory files
                        if (recursive)
                        {
                            CopyDirectory(element, destination + Path.GetFileName(element),
                                recursive);
                        }
                    }
                    else
                    {
                        // Copy the file
                        File.Copy(element, destination + Path.GetFileName(element), true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23082", ex);
            }
        }

        /// <summary>
        /// Will set the specified path (could be file or directory) attribute to
        /// normal. (Clears the read only flag). If <paramref name="recursive"/> is
        /// <see langword="true"/> and <paramref name="path"/> is a directory then will
        /// recursively set all sub-directories and files to normal as well.
        /// </summary>
        /// <param name="path">The file or directory to change to normal attribute.</param>
        /// <param name="recursive">If <see langword="true"/> will recursively set file
        /// and sub-directories attribute to normal.  If <see langword="false"/> will only
        /// set the specified file/directory attribute.</param>
        public static void MakeWritable(string path, bool recursive)
        {
            try
            {
                // Ensure the file/directory exists
                ExtractException.Assert("ELI23083", "Path must exist!",
                    File.Exists(path) || Directory.Exists(path), "Path", path);

                // If it is a directory and recursive is set to true, then set
                // each file and sub-directory
                if (recursive && Directory.Exists(path))
                {
                    foreach (string element in Directory.GetFileSystemEntries(path))
                    {
                        File.SetAttributes(element, FileAttributes.Normal);
                        if (Directory.Exists(element))
                        {
                            File.SetAttributes(element, FileAttributes.Normal);
                            MakeWritable(element, recursive);
                        }
                    }
                }
                else
                {
                    // Just set the specified file/directory attribute
                    File.SetAttributes(path, FileAttributes.Normal);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23084", ex);
            }
        }

        /// <summary>
        ///  Indicates whether the specified file ends with a known
        /// </summary>
        /// <param name="fileName">The file whose extension should be tested.</param>
        /// <returns><see langword="true"/> if the filename has a know image extension or
        /// <see langword="false"/> if it does not.</returns>
        public static bool HasImageFileExtension(string fileName)
        {
            // Get the file's extention
            string extension = Path.GetExtension(fileName);

            if (!String.IsNullOrEmpty(extension))
            {
                // Test to see if the extension matches a known image extension or has
                // a numbered file extension
                if (Regex.IsMatch(extension, @"\.\d+$") ||
                    extension.Equals(".tif", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".tiff", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".rle", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".dib", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".rst", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".gp4", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".mil", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".cal", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".cg4", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".flc", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".fli", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".tga", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".pct", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".pcx", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".bin", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".pct", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to open a file exclusively.  If it is successful will return
        /// <see langword="true"/> and <paramref name="fileStream"/> will contain the
        /// <see cref="FileStream"/> that was opened (it is up to the caller to close
        /// the stream).
        /// </summary>
        /// <param name="fileName">The name of the file to attempt to open.</param>
        /// <param name="fileStream">Will contain the opened <see cref="FileStream"/> if
        /// the open was successful and <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> if successfully opened the specified file
        /// exclusively, <see langword="false"/> otherwise.</returns>
        public static bool TryOpenExclusive(string fileName, out FileStream fileStream)
        {
            fileStream = null;
            try
            {
                fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read,
                    FileShare.Read);

                return true;
            }
            catch (Exception ex)
            {
                // Check if the file is in use and thus can't be opened exclusively
                if (ex is IOException && ex.Message.Contains("being used by another process"))
                {
                    // Return false, the file is opened by someone else
                    return false;
                }
                else
                {
                    // Some other exception, rethrow it
                    throw ExtractException.AsExtractException("ELI23368", ex);
                }
            }
        }

        /// <overloads>
        /// Returns the absolute path for the specified path relative to the application.
        /// if <paramref name="fileName"/> is already an absolute path, just returns the filename.
        /// </overloads>
        /// <summary>
        /// Returns the absolute path for the specified path relative to the application.
        /// if <paramref name="fileName"/> is already an absolute path, just returns the filename.
        /// </summary>
        /// <param name="fileName">The path to resolve. May not be <see langword="null"/>
        /// or empty string.</param>
        /// <returns>The absolute path to the specified file.</returns>
        // [DNRCAU #303]
        public static string GetAbsolutePath(string fileName)
        {
            try
            {
                return GetAbsolutePath(fileName,
                    Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath));
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24756", ex);
            }
        }

        /// <summary>
        /// Returns the absolute path for the specified path relative to the application.
        /// if <paramref name="fileName"/> is already an absolute path, just returns the filename.
        /// </summary>
        /// <param name="fileName">The path to resolve. May not be <see langword="null"/>
        /// or empty string.</param>
        /// <param name="pathRoot">The path to serve as the root if the specified fileName is a
        /// relative path.</param>
        /// <returns>The absolute path to the specified file.</returns>
        // [DNRCAU #303]
        public static string GetAbsolutePath(string fileName, string pathRoot)
        {
            try
            {
                // Ensure that fileName is not null or empty
                ExtractException.Assert("ELI23514", "File name must not be null or empty string!",
                    !string.IsNullOrEmpty(fileName));

                // Check if the filename is a relative path
                if (!Path.IsPathRooted(fileName))
                {
                    fileName = Path.Combine(pathRoot, fileName);
                    fileName = Path.GetFullPath(fileName);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23513", ex);
            }
        }

        /// <overloads>Will attempt to delete a specified file and return whether the
        /// operation was successful or not.</overloads>
        /// <summary>
        /// Will attempt to delete the specified file. If an exception occurred it will
        /// be placed in the <see cref="ExtractException"/> out parameter.
        /// Returns <see langword="true"/> if the deletion was successful and
        /// <see langword="false"/> if it was not successful.
        /// </summary>
        /// <param name="fileName">The file to delete.</param>
        /// <param name="exception">Will contain the exception (if any) that was
        /// thrown by the operation.</param>
        /// <returns><see langword="true"/> if the deletion was successful and
        /// <see langword="false"/> otherwise.</returns>
        public static bool TryDeleteFile(string fileName, out ExtractException exception)
        {
            return TryDeleteFile(fileName, false, out exception);
        }

        /// <summary>
        /// Will attempt to delete the specified file. If an exception occurred it will
        /// be logged. Returns <see langword="true"/> if the deletion was successful and
        /// <see langword="false"/> if it was not successful.
        /// </summary>
        /// <param name="fileName">The file to delete.</param>
        /// <returns><see langword="true"/> if the deletion was successful and
        /// <see langword="false"/> otherwise.</returns>
        public static bool TryDeleteFile(string fileName)
        {
            ExtractException exception;
            return TryDeleteFile(fileName, true, out exception);
        }

        /// <summary>
        /// Will attempt to delete the specified file and if <paramref name="logException"/>
        /// is <see langword="true"/> will log any exceptions that occur.  Any exceptions that
        /// occur will also be placed in the <see cref="ExtractException"/> out parameter
        /// otherwise the out parameter will be set to <see langword="null"/>.
        /// Returns <see langword="true"/> if the deletion was successful and
        /// <see langword="false"/> if it was not successful.
        /// </summary>
        /// <param name="fileName">The file to delete.</param>
        /// <param name="logException">If <see langword="true"/> will log the exception.</param>
        /// <param name="exception">Will contain the exception (if any) that was
        /// thrown by the operation.</param>
        /// <returns><see langword="true"/> if the deletion was successful and
        /// <see langword="false"/> otherwise.</returns>
        public static bool TryDeleteFile(string fileName, bool logException,
            out ExtractException exception)
        {
            try
            {
                // Attempt to delete the file (not checking for null/empty/existence
                // since these will cause the File.Delete command to throw and exception
                // and thus a return value of false)
                File.Delete(fileName);

                // Deletion was successful, return true
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                exception = ExtractException.AsExtractException("ELI23695", ex);
                exception.AddDebugData("File For Deletion", fileName ?? "NULL", false);

                // Check for exception logging
                if (logException)
                {
                    exception.Log();
                }

                // Exception occurred so return false
                return false;
            }
        }

        /// <summary>
        /// Check if filename is valid on Win32 platforms (does not contain invalid characters)
        /// <para><b>Note:</b></para>
        /// <paramref name="inputFileName"/> should only be the file name, not the path string.
        /// <seealso cref="Path.GetFileName"/>.
        /// </summary>
        /// <param name="inputFileName">Name of the input file.</param>
        /// <returns><see langword="true"/> if input filename is valid otherwise,
        /// <see langword="false"/>.
        /// </returns>
        // Code copied from http://www.bytemycode.com/snippets/snippet/334/
        public static bool IsFileNameValid(string inputFileName)
        {
            try
            {
                // Check if the file name contains invalid file name characters
                Match m = Regex.Match(inputFileName,
                    @"[\\\/\:\*\?\" + Convert.ToChar(34) + @"\<\>\|]");

                // If the match was successful then the file name contained invalid characters
                return !(m.Success);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23865", ex);
            }
        }
    }
}
