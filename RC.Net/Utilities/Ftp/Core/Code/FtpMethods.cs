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
        /// <param name="currentFile">Current file used to determine new working folder on FTP server</param>
        [CLSCompliant(false)]
        public static void SetCurrentFtpWorkingFolder(FTPConnection runningConnection, string currentFile)
        {
            try
            {
                // Determine the current working folder on the ftp server 
                string currentFileDir = PathUtil.GetFolderPath(currentFile.Replace('\\', '/'));

                // Only change the working directory if it needs to be changed.
                if (currentFileDir != runningConnection.ServerDirectory)
                {
                    // Directory must exist before changing
                    if (!runningConnection.DirectoryExists(currentFileDir))
                    {
                        runningConnection.CreateDirectory(currentFileDir);
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
        /// Returns the local path for the file being downloaded 
        /// </summary>
        /// <param name="currentRemoteWorkingFolder">Current working folder on the ftp server</param>
        /// <param name="localWorkingFolderBase">Base local working folder</param>
        /// <param name="remoteWorkingFolderBase">Base working folder on the ftp server</param>
        /// <returns>The local path rooted to the local working folder </returns>
        public static string GenerateLocalPathCreateIfNotExists(string currentRemoteWorkingFolder,
            string localWorkingFolderBase, string remoteWorkingFolderBase)
        {
            try
            {
                // Generate the path so that the directory structure rooted to 
                // the base local working folder will be the same as the 
                // current ftp working folder rooted to the base remote working folder.
                string pathForFile;
                if (remoteWorkingFolderBase == currentRemoteWorkingFolder)
                {
                    pathForFile = localWorkingFolderBase;
                }
                else
                {
                    pathForFile = Path.Combine(localWorkingFolderBase,
                        currentRemoteWorkingFolder.Remove(0, remoteWorkingFolderBase.Length));
                }

                // Convert / to \ since the ftp server path char may be different than windows
                pathForFile = pathForFile.Replace('/', '\\');

                // Make sure the local folder exists
                if (!Directory.Exists(pathForFile))
                {
                    Directory.CreateDirectory(pathForFile);
                }
                return pathForFile;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI32453", "Unable to generate local path.", ex);
                ee.AddDebugData("RemoteWorkingFolder", currentRemoteWorkingFolder, false);
                ee.AddDebugData("LocalWorkingFolderBase", localWorkingFolderBase, false);
                ee.AddDebugData("remoteWorkingFolderBase", remoteWorkingFolderBase, false);
                throw ee;
            }
        }
        
        #endregion
    }
}
