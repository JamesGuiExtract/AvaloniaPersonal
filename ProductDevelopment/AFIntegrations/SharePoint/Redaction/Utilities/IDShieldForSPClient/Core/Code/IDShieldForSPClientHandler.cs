﻿using System;
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
            +"<Query><Where><Eq><FieldRef Name='UniqueId' />"
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
        public void ProcessFile(RedactNowData data)
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
                var data = redactData as RedactNowData;

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
        static KeyValuePair<string, string> ExportFileToWorkingFolder(RedactNowData data)
        {
            using (var context = new SP.ClientContext(data.SiteUrl))
            {
                // Get the list containing the file
                var list = context.Web.Lists.GetById(data.ListId);
                var query = new SP.CamlQuery();
                query.ViewXml = _QUERY_FILE.Replace(_FILE_ID_REPLACE,
                    data.FileId.ToString("B"));

                var items = list.GetItems(query);
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

                // Get the data from the items
                var item = items[0];
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

        #endregion Methods

    }
}
