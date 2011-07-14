using Extract.Interfaces;
using Microsoft.Win32;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Extract.Utilities
{
    /// <summary>
    /// A utility class of file system methods.
    /// </summary>
    public static class FileSystemMethods
    {
        #region Fields

        /// <summary>
        /// A static object used as a mutex in the temp file name generation to prevent
        /// multiple threads from generating the same temporary file name.
        /// </summary>
        // Note: This mutex name has been copied from TemporaryFile class in BaseUtils.  Do
        // not change this mutex name without changing the other mutex name.
        static readonly Mutex _tempFileLock =
            ThreadingMethods.GetGlobalNamedMutex("Global\\260BA215-4090-4172-B696-FC86B52269B4");

        /// <summary>
        /// Either "C:\Program Files\Extract Systems" or "C:\Program Files (x86)\Extract Systems"
        /// depending on the OS.
        /// </summary>
        static readonly string _EXTRACT_SYSTEMS_PATH = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Extract Systems");

        /// <summary>
        /// The path to the common components folder.
        /// </summary>
        static readonly string _COMMON_COMPONENTS_PATH = Path.Combine(
            _EXTRACT_SYSTEMS_PATH, "CommonComponents");

        /// <summary>
        /// The full path to the Extract Systems application data folder.
        /// </summary>
        static readonly string _APPLICATION_DATA_PATH = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Extract Systems");

        /// <summary>
        /// The full path to the extract systems common application data folder
        /// </summary>
        static readonly string _COMMON_APPLICATION_DATA_PATH = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Extract Systems");

        /// <summary>
        /// The registry sub key path for the shell folders (this key is located under HKLM)
        /// </summary>
        static readonly string _REG_SUBKEY_SHELL_FOLDERS =
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";

        /// <summary>
        /// The registry sub key path for the file access retry values (this key is under HKLM)
        /// </summary>
        static readonly string _FILE_ACCESS_KEY =
            @"Software\Extract Systems\ReusableComponents\BaseUtils";

        /// <summary>
        /// The number of times to retry a file access operation if the operation
        /// fails due to a sharing violation.
        /// </summary>
        static int _fileAccessRetries = -1;

        /// <summary>
        /// The amount of time to sleep between file access retries.
        /// </summary>
        static int _fileAccessRetrySleepTime = -1;

        /// <summary>
        /// Mutex used to prevent multiple threads from trying to update the file access
        /// retry values.
        /// </summary>
        static object _fileAccessLock = new object();

        /// <summary>
        /// The <see cref="ISecureFileDeleter"/> instance to use to securely delete files.
        /// </summary>
        static ISecureFileDeleter _secureFileDeleter;

        /// <summary>
        /// Mutex used to prevent multiple threads from trying instantiate _secureFileDeleter.
        /// </summary>
        static object _secureFileDeleterLock = new object();

        /// <summary>
        /// The registry settings from which the the secure file delete options are to be retrieved.
        /// </summary>
        // Intentionally not dynamic since the values will not be read dynamically on the c++ side.
        static readonly RegistrySettings<Properties.Settings> _registry;

        #endregion Fields

        /// <summary>
        /// Static initialization for the <see cref="FileSystemMethods"/> class.
        /// </summary>
        // FXCops warns that static fields should be assigned inline for performance reasons, but
        // initialize here so exception can be caught.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static FileSystemMethods()
        {
            try
            {
                _registry = new RegistrySettings<Properties.Settings>(
                    @"SOFTWARE\Extract Systems\ReusableComponents\Extract.Utilities", false);

                // Create the SecureDeleteAllSensitiveFiles registry value so that it is
                // immediately available for a user to find. The "SecureDeleter" setting is not one
                // a user will likely ever need to set.
                _registry.GeneratePropertyValues("SecureDeleteAllSensitiveFiles");
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32872");
            }    
        }

        /// <summary>
        /// Gets the path to the extract systems folder in the common application
        /// data location of a remote machine. The path will be returned in the format
        /// of a UNC path using an adimistrative share.
        /// <example>
        /// If the path on the remote machine 'Chuck' is 'C:\ProgramData' then the
        /// returned path will be '\\Chuck\C$\ProgramData\Extract Systems'
        /// </example>
        /// </summary>
        /// <param name="machineName">The remote machine to get the path from.</param>
        /// <returns>The path on the remote machine.</returns>
        public static string GetRemoteExtractCommonApplicationDataPath(string machineName)
        {
            try
            {
                var key = RegistryMethods.OpenRemoteRegistryKey(RegistryHive.LocalMachine,
                        machineName, _REG_SUBKEY_SHELL_FOLDERS);
                var path = Path.Combine(key.GetValue("Common AppData").ToString(),
                    "Extract Systems").Replace(":\\", "$\\");
                return @"\\" + machineName + "\\" + path;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32157");
            }
        }

        #region Properties

        /// <summary>
        /// Gets the full path to the Extract Systems common application data folder.
        /// </summary>
        /// <value>The full path to the Extract Systems common application data folder.</value>    
        public static string CommonApplicationDataPath
        {
            get
            {
                return _COMMON_APPLICATION_DATA_PATH;
            }
        }

        /// <summary>
        /// Gets the full path to the Extract Systems local user application data folder.
        /// </summary>
        /// <value>The full path to the Extract Systems local user application data folder.</value>
        public static string ApplicationDataPath
        {
            get
            {
                return _APPLICATION_DATA_PATH;
            }
        }

        /// <summary>
        /// Gets the full path to the Extract Systems program files directory.
        /// </summary>
        /// <returns>The full path to the Extract Systems program files directory.</returns>
        public static string ExtractSystemsPath
        {
            get
            {
                return _EXTRACT_SYSTEMS_PATH;
            }
        }

        /// <summary>
        /// Gets the full path to the CommonComponents directory.
        /// </summary>
        /// <value>The full path to the CommonComponents directory.</value>
        public static string CommonComponentsPath
        {
            get
            {
                return _COMMON_COMPONENTS_PATH;
            }
        }

        #endregion Properties

        #region Methods

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
                ExtractException.Assert("ELI25508", "Specified folder cannot be found.",
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
                    "Failed generating temporary file name.", ex);
            }
            finally
            {
                _tempFileLock.ReleaseMutex();
            }
        }

        /// <summary>
        /// Moves a file providing the option to overwrite if necessary.
        /// <para><b>Note</b></para>
        /// The value of the SecureDeleteAllSensitiveFiles registry entry will dictate whether the
        /// old file is deleted securely when moving it to a different volume.
        /// </summary>
        /// <param name="source">The file to move.</param>
        /// <param name="destination">The location to which <paramref name="source"/> should be 
        /// moved.</param>
        /// <param name="overwrite"><see langword="true"/> if the <paramref name="destination"/> 
        /// should be overwritten; <see langword="false"/> if an exception should be thrown if the 
        /// <paramref name="destination"/> exists.</param>
        public static void MoveFile(string source, string destination, bool overwrite)
        {
            try
            {
                MoveFile(source, destination, overwrite,
                    _registry.Settings.SecureDeleteAllSensitiveFiles);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32917");
            }
        }

        /// <summary>
        /// Moves a file providing the option to overwrite if necessary.
        /// </summary>
        /// <param name="source">The file to move.</param>
        /// <param name="destination">The location to which <paramref name="source"/> should be
        /// moved.</param>
        /// <param name="overwrite"><see langword="true"/> if the <paramref name="destination"/>
        /// should be overwritten; <see langword="false"/> if an exception should be thrown if the
        /// <paramref name="destination"/> exists.</param>
        /// <param name="secureMoveFile"><see langword="true"/> to delete the old file securely
        /// when moving the file to a different volume; <see langword="false"/> otherwise.</param>
        public static void MoveFile(string source, string destination, bool overwrite,
            bool secureMoveFile)
        {
            try
            {
                // Attempt move first if possible, since this is fastest
                if (!File.Exists(destination))
                {
                    // If secureMoveFile is specified, use Directory.Move instead of File.Move;
                    // Directory.Move will fail if the destination is on another drive, providing
                    // the opportunity to manually copy then secure delete the original.
                    if (secureMoveFile &&
                        AttemptIOOperation(() => Directory.Move(source, destination)))
                    {
                        return;
                    }
                    else if (!secureMoveFile &&
                             AttemptIOOperation(() => File.Move(source, destination)))
                    {
                        return;
                    }
                }
                // The file already exists. If not overwriting, throw an exception.
                else if (!overwrite && File.Exists(destination))
                {
                    ExtractException ee = new ExtractException("ELI28454",
                        "Destination file already exists.");
                    throw ee;
                }

                // Attempt to overwrite the file. This is slower than moving the file.
                File.Copy(source, destination, true);

                // Ensure the source file is writeable before trying to delete it.
                FileAttributes attributes = File.GetAttributes(source);
                if (attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    attributes ^= FileAttributes.ReadOnly;
                    File.SetAttributes(source, attributes);
                }

                DeleteFile(source, secureMoveFile);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28532",
                    "Unable to move file.", ex);
                ee.AddDebugData("Source file", source, false);
                ee.AddDebugData("Destination file", destination, false);
                ee.AddDebugData("Overwrite", overwrite, false);
                throw ee;
            }
        }

        /// <summary>
        /// Helper method for MoveFile. Attempts the specified IO operation.
        /// </summary>
        /// <param name="IOOperation">The IO operation.</param>
        /// <returns><see langword="true"/> if the operation succeeded, <see langword="false"/> if
        /// the operation failed with an <see cref="IOException"/>.</returns>
        /// <throws>Any exception encountered other than an <see cref="IOException"/>. Most common
        /// exceptions would be <see cref="DirectoryNotFoundException"/> or 
        /// <see cref="FileNotFoundException"/>.</throws>
        static bool AttemptIOOperation(Action IOOperation)
        {
            try
            {
                IOOperation();

                return true;
            }
            catch (IOException)
            {
                // Return false to allow a move attempt to be made by copying then deleting the
                // original.
                return false;
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
                            CopyDirectory(element, destination + Path.GetFileName(element), true);
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
                ExtractException.Assert("ELI23083", "Path must exist.",
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
                            MakeWritable(element, true);
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
            try
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
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26462", ex);
            }
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
                return GetAbsolutePath(fileName, null);
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
        /// relative path; if <see langword="null"/> uses the application directory.</param>
        /// <returns>The absolute path to the specified <paramref name="fileName"/>.</returns>
        // [DNRCAU #303]
        public static string GetAbsolutePath(string fileName, string pathRoot)
        {
            try
            {
                // Ensure that fileName is not null or empty
                ExtractException.Assert("ELI23514", "File name must not be null or empty string.",
                    !string.IsNullOrEmpty(fileName));

                // Check if the filename is a relative path
                if (!Path.IsPathRooted(fileName))
                {
                    string root = pathRoot ?? Path.GetDirectoryName(Application.ExecutablePath);
                    fileName = Path.Combine(root, fileName);
                    fileName = Path.GetFullPath(fileName);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23513", ex);
            }
        }

        /// <summary>
        /// Attempts to convert the specified path to a UNC path.
        /// <para><b>Warning</b></para>
        /// The performance (speed) of this method may be poor. 
        /// <para><b>Note</b></para>
        /// When attempting to generate a UNC for a local path there may be more than one shared
        /// directory that can be used to generate the UNC. It is indeterminant which will be
        /// used in this case. Administrative shares will not be used as the basis for a UNC path.
        /// </summary>
        /// <param name="path">The path to be converted to a UNC path.</param>
        /// <param name="convertLocalPath">If <see langword="true"/> paths referencing a file on
        /// the local system will be converted to a UNC path (if possible), <see langword="false"/>
        /// if local paths should not be converted.</param>
        /// <returns><see langword="true"/> if the the path was successfully converted to a UNC
        /// path, <see langword="false"/> if the path was not converted in which case the path
        /// remains the same as it was passed in.</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        public static bool ConvertToNetworkPath(ref string path, bool convertLocalPath)
        {
            // This method is prone to poor performance. To help identify if it is causing any
            // serious problems, log an application trace anytime execution takes longer than a
            // second.
            DateTime startTime = DateTime.Now;

            try
            {
                // Check to see if the path already appears to be a UNC path.
                if (path.StartsWith("\\\\", StringComparison.Ordinal))
                {
                    return true;
                }

                // Ensure we are working with an absolute path first.
                string workingPath = GetAbsolutePath(path);

                // Separate the volume from the rest of the path.
                string[] pathParts = workingPath.Split(Path.VolumeSeparatorChar);
                string volume = pathParts[0] + Path.VolumeSeparatorChar;

                ExtractException.Assert("ELI27316", "Invalid path", pathParts.Length == 2);

                // Obtain a drive management object associated with the volume name.
                using (ManagementObject driveObject = new ManagementObject())
                {
                    driveObject.Path = new ManagementPath("Win32_LogicalDisk='" + volume + "'");

                    // Check what type of drive it is.
                    object driveType = driveObject["DriveType"];

                    // If it is a network drive, it can be converted
                    if (driveType != null &&
                        Convert.ToUInt32(driveType, CultureInfo.CurrentCulture) == 4)
                    {
                        // The ProviderName is the UNC equivalent of the volume name.
                        object providerName = driveObject["ProviderName"];
                        if (providerName != null)
                        {
                            path = providerName + pathParts[1];
                            return true;
                        }
                    }
                }

                // If local paths are not to be converted, there is nothing more to be attempted.
                if (!convertLocalPath)
                {
                    return false;
                }

                // If attempting to convert local paths, search all shared directories for one that
                // the specified path is included in.
                foreach (ManagementObject sharedDirectory in
                    SystemMethods.GetWmiObjects("Win32_ShareToDirectory"))
                {
                    try
                    {
                        // Administrative shares will have the high bit of the Type property set;
                        // ignore these.
                        object shareTypeProperty =
                            SystemMethods.GetWmiProperty(sharedDirectory, "Share.Type");
                        if (shareTypeProperty != null && ((uint)shareTypeProperty & 0x80000000) != 0)
                        {
                            continue;
                        }

                        // Find the local path of the share.
                        object localPathProperty =
                            SystemMethods.GetWmiProperty(sharedDirectory, "Share.Path");
                        if (localPathProperty == null)
                        {
                            continue;
                        }
                        string localPath = localPathProperty.ToString();

                        // Test to see if the local path of the share matches the beginning of the
                        // working path. If so, it can be used to generate a UNC.
                        if (workingPath.IndexOf(localPath, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // Generate a UNC address for the share using the local host name combined
                            // with the share name.
                            object shareNameProperty =
                                SystemMethods.GetWmiProperty(sharedDirectory, "Share.Name");
                            if (shareNameProperty != null)
                            {
                                StringBuilder sharePath = new StringBuilder();
                                sharePath.Append("\\\\");
                                sharePath.Append(Dns.GetHostName());
                                sharePath.Append("\\");
                                sharePath.Append(shareNameProperty.ToString());

                                // Combine the share UNC path with the part of the working path not
                                // included in the share path.
                                path = sharePath + workingPath.Substring(localPath.Length);
                                return true;
                            }
                        }
                    }
                    finally
                    {
                        sharedDirectory.Dispose();
                    }
                }

                // The path could not be converted.
                return false;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27313", "UNC path conversion failed.", ex);
                ee.AddDebugData("Path", path, false);
                throw ee;
            }
            finally
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                if (elapsed.TotalSeconds > 5)
                {
                    ExtractException ee = new ExtractException("ELI27699",
                        "Application trace: Poor performance calculating network path.");
                    ee.AddDebugData("Elapsed time", elapsed.ToString(), false);
                    ee.AddDebugData("Path", path, false);
                    ee.Log();
                }
            }
        }

        /// <overloads>
        /// Deletes the specified file.
        /// </overloads>
        /// <summary>
        /// Deletes the specified <see paramref="fileName"/>.
        /// <para><b>Note</b></para>
        /// The value of the SecureDeleteAllSensitiveFiles registry entry will dictate whether the
        /// file is deleted securely.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        public static void DeleteFile(string fileName)
        {
            try
            {
                DeleteFile(fileName, _registry.Settings.SecureDeleteAllSensitiveFiles);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32873");
            }
        }

        /// <summary>
        /// Deletes the specified <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <param name="secureDeleteFile"><see langword="true"/> to delete the file securely;
        /// <see langword="false"/> otherwise.</param>
        public static void DeleteFile(string fileName, bool secureDeleteFile)
        {
            try
            {
                DeleteFile(fileName, secureDeleteFile, false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32874");
            }
        }

        /// <summary>
        /// Deletes the specified <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <param name="secureDeleteFile"><see langword="true"/> to delete the file securely;
        /// <see langword="false"/> otherwise.</param>
        /// <param name="throwIfUnableToDeleteSecurely">If <see langword="true"/>, when securely
        /// deleteing a file, but the file could not be securely overwritten, an exception will be
        /// throw before attempting the actual deletion.</param>
        public static void DeleteFile(string fileName, bool secureDeleteFile,
            bool throwIfUnableToDeleteSecurely)
        {
            try 
	        {
                if (secureDeleteFile)
                {
                    lock (_secureFileDeleterLock)
                    {
                        if (_secureFileDeleter == null)
                        {
                            // Instantiate the deleter
                            Type deleterType =
                                Type.GetTypeFromProgID(_registry.Settings.SecureDeleter);
                            if (deleterType == null)
                            {
                                ExtractException ee = new ExtractException("ELI32875",
                                    "Failed to find registered secure file deleter.");
                                ee.AddDebugData("Deleter Type", _registry.Settings.SecureDeleter,
                                    false);
                                throw ee;
                            }

                            _secureFileDeleter =
                                (ISecureFileDeleter)Activator.CreateInstance(deleterType);
                        }
                    }

                    _secureFileDeleter.SecureDeleteFile(fileName, throwIfUnableToDeleteSecurely);
                }
                else
                {
                    // The secure file deleter takes care of sharing violation retires, so the
                    // retry logic here should encapsulate only normal deletions.
                    PerformFileOperationWithRetryOnSharingViolation(() => File.Delete(fileName));
                }
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI32876");
	        }
        }

        /// <overloads>Will attempt to delete a specified file and return whether the
        /// operation was successful or not.</overloads>
        /// <summary>
        /// Will attempt to delete the specified file. If an exception occurred it will
        /// be placed in the <see cref="ExtractException"/> out parameter.
        /// Returns <see langword="true"/> if the deletion was successful and
        /// <see langword="false"/> if it was not successful.
        /// <para><b>Note</b></para>
        /// The value of the SecureDeleteAllSensitiveFiles registry entry will dictate whether the
        /// file is deleted securely.
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
        /// <para><b>Note</b></para>
        /// The value of the SecureDeleteAllSensitiveFiles registry entry will dictate whether the
        /// file is deleted securely.
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
        /// <para><b>Note</b></para>
        /// The value of the SecureDeleteAllSensitiveFiles registry entry will dictate whether the
        /// file is deleted securely.
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
            return TryDeleteFile(fileName, logException,
                _registry.Settings.SecureDeleteAllSensitiveFiles, out exception);
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
        /// <param name="secureDeleteFile"><see langword="true"/> to delete the file securely;
        /// <see langword="false"/> otherwise.</param>
        /// <param name="exception">Will contain the exception (if any) that was
        /// thrown by the operation.</param>
        /// <returns><see langword="true"/> if the deletion was successful and
        /// <see langword="false"/> otherwise.</returns>
        public static bool TryDeleteFile(string fileName, bool logException, bool secureDeleteFile,
            out ExtractException exception)
        {
            try
            {
                // Attempt to delete the file (not checking for null/empty/existence
                // since these will cause the File.Delete command to throw and exception
                // and thus a return value of false)
                DeleteFile(fileName, secureDeleteFile);

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

        /// <summary>
        /// Determines if the specified path references a resource on the local machine.
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <returns><see langword="true"/> if the path references a resource on the local machine;
        /// <see langword="false"/> otherwise.</returns>
        public static bool IsPathLocal(string path)
        {
            try
            {
                // Convert the path to a UNC path and obtain a URI for it.
                ConvertToNetworkPath(ref path, false);

                // If the result is not a UNC path, we know it is local.
                if (!path.StartsWith("\\\\", StringComparison.Ordinal))
                {
                    return true;
                }

                // Obtain all IP addresses that reference the host for the specified path as well as
                // for the local host.
                IPAddress[] pathHostAddresses = Dns.GetHostAddresses(new Uri(path).Host);
                IPAddress[] localHostAddresses = Dns.GetHostAddresses(Dns.GetHostName());

                // Check to see if any IPs between the two lists match.
                foreach (IPAddress pathHostAddress in pathHostAddresses)
                {
                    // Check for 127.0.0.1
                    if (IPAddress.IsLoopback(pathHostAddress))
                    {
                        return true;
                    }

                    // Check to see if this pathHostAddress matches any of the IPs for the local
                    // host.
                    foreach (IPAddress localHostAddress in localHostAddresses)
                    {
                        if (localHostAddress.Equals(pathHostAddress))
                        {
                            return true;
                        }
                    }
                }

                // No match could be found; the path is not local
                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27312", ex);
            }
        }

        /// <summary>
        /// Combines the list of strings into a single path.
        /// <para><b>Note:</b></para>
        /// This method does not check the path for existence but will validate
        /// each string for invalid path characters.
        /// </summary>
        /// <param name="list">The list of strings to combine. Must not
        /// be <see langword="null"/> and <see cref="Array.Length"/> must be
        /// &gt;= 2.</param>
        /// <returns>The combined path string.</returns>
        public static string PathCombine(params string[] list)
        {
            try
            {
                if (list == null || list.Length < 2)
                {
                    ExtractException ee = new ExtractException("ELI28503",
                        "List must contain at least 2 items.");
                    ee.AddDebugData("List Size", list != null ?
                        list.Length.ToString(CultureInfo.CurrentCulture) : "<Null>", false);
                    throw ee;
                }

                // Updated to use the .Net 4.0 path combine
                return Path.Combine(list);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28504", ex);
            }
        }

        /// <summary>
        /// Gets the full path to the specified file and drops the file's extension.
        /// </summary>
        /// <param name="path">The path to the file whose extension should be dropped.</param>
        /// <returns>The full path to the <paramref name="path"/> without the file extension.
        /// </returns>
        public static string GetFullPathWithoutExtension(string path)
        {
            string fullPath = GetAbsolutePath(path);
            string directory = Path.GetDirectoryName(fullPath);
            string file = Path.GetFileNameWithoutExtension(fullPath);

            return Path.Combine(directory, file);
        }

        /// <summary>
        /// Builds the time stamped backup file name.
        /// </summary>
        /// <param name="fileName">Name of the file to compute timestamped backup file name.</param>
        /// <param name="beforeExtension">if set to <see langword="true"/> then the date/time
        /// stamp will be placed before the extension, if <see langword="false"/> then
        /// the date/time stamp will be placed after the extension.</param>
        /// <example>
        /// This sample shows how to call, and what the return value is from calling this function
        /// fileName = "C:\test\test.txt"
        /// <code lang="C#">
        /// string fileName = @"C:\test\test.txt";
        /// var backupFileNameBeforeExt = BuildTimeStampedBackupFileName(fileName, true);
        /// var backupFileNameAfterExt = BuildTimeStampedBackupFileName(fileName, false);
        /// 
        /// Console.WriteLine(backupFileNameBeforeExt); // C:\test\test.2010-11-29T09_15_30.txt"
        /// Console.WriteLine(backupFileNameAfterExt); // C:\test\test.txt.2010-11-29T09_15_30"
        /// </code>
        /// </example>
        /// <returns>The date/time stamped file name that can be used to backup the current file.
        /// </returns>
        public static string BuildTimeStampedBackupFileName(string fileName, bool beforeExtension)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new ArgumentException("Filename cannot be null or empty string",
                        "fileName");
                }

                // Ensure path is rooted
                if (!Path.IsPathRooted(fileName))
                {
                    fileName = Path.GetFullPath(fileName);
                }

                string dateTime = "." + DateTime.Now.ToString("s",
                    CultureInfo.CurrentCulture).Replace(":", "_");
                string extension = Path.GetExtension(fileName);

                var sb = new StringBuilder();
                sb.Append(Path.Combine(Path.GetDirectoryName(fileName),
                    Path.GetFileNameWithoutExtension(fileName)));
                if (beforeExtension)
                {
                    sb.Append(dateTime);
                    sb.Append(extension);
                }
                else
                {
                    sb.Append(extension);
                    sb.Append(dateTime);
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31097", ex);
            }
        }

        /// <summary>
        /// Validates a files existence. Throws an exception if the file is not found.
        /// </summary>
        /// <param name="fileName">The name of the file to validate.</param>
        /// <param name="eliCode">The eli code to associate with the exception if the
        /// file is not found.</param>
        public static void ValidateFileExistence(string fileName, string eliCode)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    var ee = new ExtractException(eliCode, "File cannot be found.",
                        new FileNotFoundException());
                    ee.AddDebugData("File Name", fileName, false);
                    throw ee;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32325");
            }
        }

        /// <summary>
        /// Gets the count of retries and the time to sleep in between attempts for
        /// file access sharing violations.
        /// </summary>
        /// <returns>The retry count and sleep time.</returns>
        static Tuple<int, int> GetRetryCountAndSleepTime()
        {
            if (_fileAccessRetries == -1 || _fileAccessRetrySleepTime == -1)
            {
                lock (_fileAccessLock)
                {
                    if (_fileAccessRetries == -1)
                    {
                        // Get the values from the registry
                        var key = Registry.LocalMachine.OpenSubKey(_FILE_ACCESS_KEY);
                        var accessRetries = key.GetValue("FileAccessRetries", "50").ToString();
                        _fileAccessRetries = int.Parse(accessRetries, CultureInfo.InvariantCulture);
                        var sleepTime = key.GetValue("FileAccessTimeout", "250").ToString();
                        _fileAccessRetrySleepTime = int.Parse(sleepTime, CultureInfo.InvariantCulture);
                    }
                }
            }

            return new Tuple<int, int>(_fileAccessRetries, _fileAccessRetrySleepTime);
        }

        /// <summary>
        /// Performs the specified file operation within a looping structure to retry the operation
        /// if the failure is due to a sharing violation.
        /// </summary>
        /// <param name="fileOperation">The operation to perform.</param>
        public static void PerformFileOperationWithRetryOnSharingViolation(Action fileOperation)
        {
            try
            {
                var retryCountAndSleepTime = GetRetryCountAndSleepTime();
                int maxAttempts = retryCountAndSleepTime.Item1;
                int sleepTime = retryCountAndSleepTime.Item2;
                int attempts = 1;
                do
                {
                    try
                    {
                        fileOperation();
                        break;
                    }
                    catch (IOException ex)
                    {
                        if (ex.GetWindowsErrorCode() != Win32ErrorCode.SharingViolation)
                        {
                            throw ex.AsExtract("ELI32411");
                        }
                        else if (attempts >= maxAttempts)
                        {
                            var ee = new ExtractException("ELI32412",
                                "File operation failed after retries.", ex);
                            ee.AddDebugData("Number Of Attempts", attempts, false);
                            ee.AddDebugData("Max Number Of Attempts", maxAttempts, false);
                            throw ee;
                        }
                    }

                    attempts++;
                    Thread.Sleep(sleepTime);
                }
                while (true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32413");
            }
        }

        #endregion Methods
    }
}
