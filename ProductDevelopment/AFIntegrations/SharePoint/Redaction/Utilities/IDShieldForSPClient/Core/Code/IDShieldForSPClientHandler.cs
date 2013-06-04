using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using SP = Microsoft.SharePoint.Client;

namespace Extract.SharePoint.Redaction.Utilities
{
    class IDShieldForSPClientHandler : IIDShieldForSPClient
    {
        #region Constants

        /// <summary>
        /// Path to the redact now helper working folder
        /// </summary>
        static readonly string _WORKING_FOLDER = Path.Combine(Path.GetTempPath(), Application.ProductName);

        /// <summary>
        /// CAML query for getting the specific file from SharePoint.
        /// </summary>
        static readonly string _FILE_ID_REPLACE = "<ReplaceFileId>";
        static readonly string _QUERY_FILE = "<View Scope='RecursiveAll'>"
            + "<Query><Where><Eq><FieldRef Name='UniqueId' />"
            + "<Value Type='Guid'>" + _FILE_ID_REPLACE
            + "</Value></Eq></Where></Query></View>";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The path to the run fps file executable.
        /// </summary>
        static string _runFpsFileLocation = GetPathToRunFpsFile();

        #endregion Fields

        #region IIDShieldForSPClient Interface Methods

        /// <summary>
        /// Interface method used to launch the specified file for local processing.
        /// </summary>
        /// <param name="data">The data for the file to process.</param>
        public void ProcessFile(IDSForSPClientData data)
        {
            try
            {
                var thread = new Thread(ProcessFileThread);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start(data);
            }
            catch (Exception ex)
            {
                ex.LogExceptionWithHelperApp("ELI31456");
                throw new FaultException("Unable to process file. " + ex.Message);
            }
        }

        /// <summary>
        /// Interface method used to launch the specified file for verification
        /// </summary>
        /// <param name="data">The data for the file to process.</param>
        public void VerifyFile(IDSForSPClientData data)
        {
            try
            {
                var thread = new Thread(VerifyFileThread);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start(data);
            }
            catch (Exception ex)
            {
                ex.LogExceptionWithHelperApp("ELI35834");
                throw new FaultException("Unable to process files. " + ex.Message);
            }
        }
        #endregion IIDShieldForSPClient Interface Methods

        #region Methods

        /// <summary>
        /// Thread method that handles exporting the file from SharePoint and launching RunFpsFile.
        /// </summary>
        /// <param name="redactData"></param>
        static void ProcessFileThread(object redactData)
        {
            string fileToClean = null;
            try
            {
                var data = redactData as IDSForSPClientData;

                // Check for valid FPS file
                if (!File.Exists(data.FpsFileLocation))
                {
                    throw new FileNotFoundException("FPS file could not be found.", data.FpsFileLocation);
                }

                var fileInformation = ExportFileToWorkingFolder(data);
                fileToClean = fileInformation.Key;

                var info = new ProcessStartInfo(_runFpsFileLocation,
                    string.Concat("\"", data.FpsFileLocation, "\" \"",
                    fileInformation.Key, "\" /ignoreDb"));

                using (var process = new Process())
                {
                    process.StartInfo = info;
                    process.Start();
                    process.WaitForExit();
                }

                var redactedFile = Path.Combine(Path.GetDirectoryName(fileInformation.Key),
                    Path.GetFileNameWithoutExtension(fileInformation.Key)) + ".redacted"
                    + Path.GetExtension(fileInformation.Key);

                if (File.Exists(redactedFile))
                {
                    PromptAndSaveFile(redactedFile, fileInformation.Value, data.SiteUrl);
                }
            }
            catch (Exception ex)
            {
                // Attempt to log the exception
                ex.LogExceptionWithHelperApp("ELI31457");

                // Display a message to the user
                ex.DisplayInMessageBox();
            }
            finally
            {
                if (fileToClean != null)
                {
                    CleanupWorkingFiles(fileToClean);
                }
            }
        }

        /// <summary>
        /// Prompts for saving file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="originalFile">The original file.</param>
        /// <param name="siteUrl">The site URL.</param>
        static void PromptAndSaveFile(string fileName, string originalFile,
            string siteUrl)
        {
            using (var dialog = new IDShieldForSPSaveFileForm(fileName, originalFile,
                siteUrl))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.SaveToSharePoint)
                    {
                        UploadFileToSharePoint(dialog.DestinationFilePath, fileName,
                            siteUrl, dialog.ListId);
                    }
                    else
                    {
                        File.Copy(fileName, dialog.DestinationFilePath, true);
                    }
                }
            }
        }

        /// <summary>
        /// Uploads the file to SharePoint.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="siteUrl">The site URL.</param>
        /// <param name="listId">The list id.</param>
        static void UploadFileToSharePoint(string destination, string fileName,
            string siteUrl, Guid listId)
        {
            using (var context = new SP.ClientContext(siteUrl))
            {
                var web = context.Web;
                var list = web.Lists.GetById(listId);

                var fileCreation = new SP.FileCreationInformation();
                fileCreation.Content = File.ReadAllBytes(fileName);
                fileCreation.Url = siteUrl + destination;
                fileCreation.Overwrite = true;

                var file = list.RootFolder.Files.Add(fileCreation);
                context.Load(file);
                context.ExecuteQuery();
            }
        }

        /// <summary>
        /// Cleanups the working files.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        static void CleanupWorkingFiles(string fileName)
        {
            try
            {
                var directory = Path.GetDirectoryName(fileName);
                var fileNoExt = Path.GetFileNameWithoutExtension(fileName);
                foreach (var file in
                    Directory.GetFiles(directory, fileNoExt + ".*", SearchOption.TopDirectoryOnly))
                {
                    File.Delete(file);
                }

                Directory.Delete(directory, true);
            }
            catch (Exception ex)
            {
                ex.LogExceptionWithHelperApp("ELI31461");
            }
        }

        /// <summary>
        /// Gets the path to run FPS file.
        /// </summary>
        /// <returns></returns>
        static string GetPathToRunFpsFile()
        {
#if DEBUG
            var root = @"C:\Program Files (x86)\Extract Systems\CommonComponents\RunFpsFile.exe";
#else
            var root = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),
                "RunFpsFile.exe");
#endif
            return root;
        }

        /// <summary>
        /// Exports the file from SharePoint to the working folder.
        /// </summary>
        /// <param name="data">The redact now data.</param>
        /// <returns>The name of the downloaded file and the
        /// path to the original file.</returns>
        static KeyValuePair<string, string> ExportFileToWorkingFolder(IDSForSPClientData data)
        {
            using (var context = new SP.ClientContext(data.SiteUrl))
            {
                // Get the data from the items
                var item = GetListItemOfFile(context, data);
                var fileName = item["FileLeafRef"].ToString();
                var url = item["FileRef"].ToString();

                var fileInfo = SP.File.OpenBinaryDirect(context, url);
                var source = fileInfo.Stream;

                // Build path for working directory (create if necessary)
                string destDir = Path.Combine(_WORKING_FOLDER, data.FileId.ToString("N"));
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                // Write the file to the working folder
                string destinationName = Path.Combine(destDir, fileName);
                using (var destination = new FileStream(destinationName, FileMode.Create))
                {
                    source.ReadFromStream(destination);
                }

                return new KeyValuePair<string, string>(destinationName, url);
            }
        }

        /// <summary>
        /// Thread method that handles exporting the file from SharePoint and launching RunFpsFile.
        /// </summary>
        /// <param name="verifyData">Data needed for opening the Verification UI</param>
        static void VerifyFileThread(object verifyData)
        {
            try
            {
                var data = verifyData as IDSForSPClientData;

                if (!File.Exists(data.FpsFileLocation))
                {
                    throw new FileNotFoundException("FPS file could not be found.", data.FpsFileLocation);
                }

                if (!Directory.Exists(data.WorkingFolder))
                {
                    Exception directoryEx = new DirectoryNotFoundException("Working folder could not be found.");
                    directoryEx.Data.Add("Working Folder", data.WorkingFolder);
                    throw directoryEx;
                }

                // Need to get the file name that should be in the verification folder (pdf files will probably be named .pdf.tif
                string fileToVerify = GetFileToVerify(data);

                if (!File.Exists(fileToVerify))
                {
                    Exception fileEx = new FileNotFoundException("File to verify does not exist.");
                    fileEx.Data.Add("File", fileToVerify);
                    throw fileEx;
                };

                var info = new ProcessStartInfo(_runFpsFileLocation,
                    string.Concat("\"", data.FpsFileLocation, "\" \"",
                    fileToVerify, "\" /process"));

                using (var process = new Process())
                {
                    process.StartInfo = info;
                    process.Start();
                    process.WaitForExit();
                }

            }
            catch (Exception ex)
            {
                // Attempt to log the exception
                ex.LogExceptionWithHelperApp("ELI35835");

                // Display a message to the user
                ex.DisplayInMessageBox();
            }

        }

        /// <summary>
        /// Gets the full path to the file to verify using the IDSForSPCLientData
        /// </summary>
        /// <param name="data">Data about the file to verify that was sent from the server</param>
        /// <returns>Full path to the file to verify</returns>
        static string GetFileToVerify(IDSForSPClientData data)
        {
            using (var context = new SP.ClientContext(data.SiteUrl))
            {
                var item = GetListItemOfFile(context, data);
                var fileName = item["FileLeafRef"].ToString();
                var pathWithSiteID = Path.Combine(data.WorkingFolder, context.Site.Id.ToString());
                string workingPath = Path.Combine(pathWithSiteID, data.ListId.ToString());
                string fileToVerify = Path.Combine(workingPath,
                                    data.FileId.ToString() + Path.GetExtension(fileName));

                // For pdf files check for rasterized version of the file
                if (Path.GetExtension(fileToVerify).ToLower() == ".pdf")
                {
                    // check if that file exists with a .pdf.tif extension
                    if (File.Exists(fileToVerify + ".tif"))
                    {
                        fileToVerify = fileToVerify + ".tif";
                    }
                }

                return fileToVerify;
            }

        }

        /// <summary>
        /// Gets the ListItem for the file using the data
        /// </summary>
        /// <param name="context">The sharepoint context that contains the file</param>
        /// <param name="data">The data needed to get the file</param>
        /// <returns>The ListItem for the file</returns>
        static SP.ListItem GetListItemOfFile(SP.ClientContext context, IDSForSPClientData data)
        {
            // Get the list containing the file
            var list = context.Web.Lists.GetById(data.ListId);
            var query = new SP.CamlQuery();
            query.ViewXml = _QUERY_FILE.Replace(_FILE_ID_REPLACE,
                data.FileId.ToString("B"));

            var items = list.GetItems(query);

            var site = context.Site;
            context.Load(site);
            context.Load(list);
            context.Load(items);
            context.ExecuteQuery();

            if (items.Count != 1)
            {
                var message = items.Count == 0 ? "File was not found."
                    : "More than one item found with matching id.";

                // Too many items returned
                var ee = new ArgumentException(message);
                ee.Data.Add("Site URL", data.SiteUrl);
                ee.Data.Add("List Id", data.ListId.ToString());
                ee.Data.Add("File Id", data.FileId.ToString());

                throw ee;
            }
            return items[0];
        }

        #endregion Methods
    }

}
