using EnterpriseDT.Net.Ftp;
using EnterpriseDT.Util;
using Extract.Licensing;
using System;
using System.IO;
using System.Reflection;

namespace Extract.Utilities.Ftp
{
    /// <summary>
    /// Class used for methods shared methods related to ftp server
    /// </summary>
    public static class FtpMethods
    {
        #region Static Fields

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME =
           typeof(FtpMethods).ToString();

        /// <summary>
        /// License owner for the edtftpnetpro library
        /// </summary>
        static readonly string _FTP_API_LICENSE_OWNER = "ExtractSystems";

        /// <summary>
        /// License key for the edtftpnetpro library
        /// </summary>
        static readonly string _FTP_API_LICENSE_KEY = "064-7556-4340-7862";

        #endregion

        #region Public Static Methods
        
        /// <summary>
        /// Sets the LicenseOwner and LicenseKey properties for the ftpConnection. This method can 
        /// only be called by an Extract Systems Assembly
        /// </summary>
        /// <param name="ftpConnection">Connection to be initialized</param>
        [CLSCompliant(false)]
        public static void InitializeFtpApiLicense(ExFTPConnection ftpConnection)
        {
            try
            {
                // Verify this object is called from Extract code
                if (!LicenseUtilities.VerifyAssemblyData(Assembly.GetCallingAssembly()))
                {
                    var ee = new ExtractException("ELI32435",
                        "Object is not usable in current configuration.");
                    ee.AddDebugData("Object Name", _OBJECT_NAME, false);
                    throw ee;
                }

                ftpConnection.LicenseOwner = _FTP_API_LICENSE_OWNER;
                ftpConnection.LicenseKey = _FTP_API_LICENSE_KEY;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32436");
            }
        }

        /// <summary>
        /// Sets the current working folder on the FTP server if it is not the same as the
        /// directory of currentFile
        /// </summary>
        /// <param name="runningConnection">Connection to the FTP server</param>
        /// <param name="currentFile">Current file used to determine new working folder on FTP
        /// server.</param>
        /// <param name="createIfNotExists"><see langword="true"/> to create the directory if it
        /// doesn't not exist; <see langword="false"/> to throw an exception if the
        /// <see paramref="currentFile"/> doesn't exist.</param>
        [CLSCompliant(false)]
        public static void SetCurrentFtpWorkingFolder(FTPConnection runningConnection,
            string currentFile, bool createIfNotExists)
        {
            try
            {
                // Determine the current working folder on the ftp server 
                string currentFileDir = PathUtil.GetFolderPath(currentFile.Replace('\\', '/'));

                // Working folder must begin with /
                if (currentFileDir.Length > 0 && currentFileDir[0] != '/')
                {
                    currentFileDir = "/" + currentFileDir;
                }
                else if (string.IsNullOrWhiteSpace(currentFileDir))
                {
                    currentFileDir = "/";
                }

                // Only change the working directory if it needs to be changed.
                if (currentFileDir != runningConnection.ServerDirectory)
                {
                    // Directory must exist before changing
                    if (!runningConnection.DirectoryExists(currentFileDir))
                    {
                        if (createIfNotExists)
                        {
                            runningConnection.CreateDirectory(currentFileDir);
                        }
                        else
                        {
                            throw new ExtractException("ELI34091", "Folder does not exist.");
                        }
                    }

                    runningConnection.ChangeWorkingDirectory(currentFileDir);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI32450", "Unable to set Ftp working folder.", ex);
                ee.AddDebugData("Destination filename", currentFile, false);
                throw ee;
            }
        }

        /// <summary>
        /// Normalizes a remote path by ensuring it begins and ends with '/'
        /// </summary>
        /// <param name="remotePath">The remote path to normalize</param>
        /// <returns>The normalized path.</returns>
        public static string NormalizeRemotePath(string remotePath)
        {
            try
            {
                // The FTP path should have / instead of \ so need to convert all / to \
                remotePath = remotePath.Replace('\\', '/');

                // Defaulte the remote working folder to / if empty
                if (string.IsNullOrEmpty(remotePath))
                {
                    remotePath = "/";
                }

                // add / to the end of remoteWorkingFolderBase if needed
                if (remotePath.LastIndexOf('/') != remotePath.Length - 1)
                {
                    remotePath += "/";
                }

                // Need to fix up the remoteWorkingFolderBase - it may need to have a / added to the front
                if (remotePath.Length > 0 && remotePath[0] != '/')
                {
                    remotePath = "/" + remotePath;
                }

                return remotePath;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34100");
            }
        }
        
        #endregion
    }
}
