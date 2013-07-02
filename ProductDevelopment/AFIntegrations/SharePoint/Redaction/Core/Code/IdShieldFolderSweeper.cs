using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

// Using statements to make dealing with folder settings more readable
using IdShieldFolderSettingsCollection =
System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.Redaction.IdShieldFolderProcessingSettings>>;
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.Redaction.IdShieldFolderProcessingSettings>;

namespace Extract.SharePoint.Redaction
{
    /// <summary>
    /// SharePoint timer job that sweeps the local working folder for files that have
    /// finished/failed processing and then pushes them back into SharePoint.
    /// </summary>
    public class IdShieldFolderSweeper : SPJobDefinition
    {
        #region Helper Class

        /// <summary>
        /// Helper class to store the file upload data
        /// </summary>
        class FileUploadData
        {
            /// <summary>
            /// Gets or sets the original list id.
            /// </summary>
            /// <value>The original list id.</value>
            public Guid OriginalListId { get; set; }

            /// <summary>
            /// Gets or sets the original file id.
            /// </summary>
            /// <value>The original file id.</value>
            public Guid OriginalFileId { get; set; }

            /// <summary>
            /// Gets or sets the original file URL.
            /// </summary>
            /// <value>The original file URL.</value>
            public string OriginalFileUrl { get; set; }

            /// <summary>
            /// Gets or sets the destination URL.
            /// </summary>
            /// <value>The destination URL.</value>
            public string DestinationUrl { get; set; }

            /// <summary>
            /// Gets or sets the redacted file.
            /// </summary>
            /// <value>The redacted file.</value>
            public string RedactedFile { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FileUploadData"/> class.
            /// </summary>
            /// <param name="originalListId">The original list id.</param>
            /// <param name="originalFileId">The original file id.</param>
            /// <param name="originalFileUrl">The original file URL.</param>
            /// <param name="destinationUrl">The destination URL.</param>
            /// <param name="redactedFile">The redacted file.</param>
            public FileUploadData(Guid originalListId, Guid originalFileId, string originalFileUrl,
                string destinationUrl, string redactedFile)
            {
                OriginalListId = originalListId;
                OriginalFileId = originalFileId;
                OriginalFileUrl = originalFileUrl;
                DestinationUrl = destinationUrl;
                RedactedFile = redactedFile;
            }
        }

        #endregion Helper Class

        #region Constants

        /// <summary>
        /// The title for this timer
        /// </summary>
        const string _TITLE = "Extract Systems ID Shield Disk to SharePoint";

        /// <summary>
        /// The description for this timer
        /// </summary>
        const string _DESCRIPTION = "This timer object will periodically search the configured local "
            + "working folder for processed files and will then push the "
            + "processed files back into the SharePoint site.";

        /// <summary>
        /// Regular expression used to find the root document library name in a folder path.
        /// </summary>
        const string _FIND_ROOT_LIBRARY_REGEX = @"(?<=\A/)[^/]+(?=/)";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The local working folder for the ID Shield feature
        /// </summary>
        string _localWorkingFolder;

        /// <summary>
        /// The collection of folder settings keyed by the site id.
        /// </summary>
        IdShieldFolderSettingsCollection _folderSettings;

        /// <summary>
        /// Collection of sites which have the ID Shield feature activated.
        /// </summary>
        HashSet<Guid> _activeSites;

        /// <summary>
        /// Regex used to find the root library from a folder path url.
        /// </summary>
        static Regex _findRoot = new Regex(_FIND_ROOT_LIBRARY_REGEX);

        /// <summary>
        /// HashSet to contain the files that could not be imported previously to keep the 
        /// exception log from getting multiple exceptions for the same problem
        /// </summary>
        HashSet<string> _filesPreviouslyAttempted = new HashSet<string>();

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="IdShieldFolderSweeper"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldFolderSweeper"/> class.
        /// </summary>
        public IdShieldFolderSweeper()
            : base()
        {
            base.Title = _TITLE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldFolderSweeper"/> class.
        /// </summary>
        /// <param name="jobName">The name for this job.</param>
        /// <param name="service">The SharePoint service this job is associated with.</param>
        /// <param name="server">The SharePoint server this job is associated with.</param>
        public IdShieldFolderSweeper(string jobName, SPService service, SPServer server)
            : base(jobName, service, server, SPJobLockType.Job)
        {
            base.Title = _TITLE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldFolderSweeper"/> class.
        /// </summary>
        /// <param name="jobName">The name for this job.</param>
        /// <param name="webApplication">The SharePoint web application this job is
        /// associated with.</param>
        public IdShieldFolderSweeper(string jobName, SPWebApplication webApplication)
            : base(jobName, webApplication, null, SPJobLockType.Job)
        {
            base.Title = _TITLE;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Executes the job definition on the local machine and is intended to be used only
        /// by the timer service. <see cref="SPJobDefinition.Execute"/>
        /// </summary>
        /// <param name="targetInstanceId">For target types of <see cref="SPContentDatabase"/>
        /// this is the database ID of the context database being processed by the running job.
        /// This value is <see cref="Guid.Empty"/> for all other target types.</param>
        public override void Execute(Guid targetInstanceId)
        {
            try
            {
                UpdateSettings();
                var sites = WebApplication.Sites;
                foreach (SPSite site in sites)
                {
                    var siteId = site.ID;
                    RemoveFilesToIgnore(siteId);
                    if (_activeSites.Contains(siteId))
                    {
                        SearchAndHandleExistingFiles(site);
                    }
                }

                base.Execute(targetInstanceId);
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex,
                    ErrorCategoryId.IdShieldDiskToSharePoint, "ELI30599");
                throw;
            }
        }

        /// <summary>
        /// Searches for existing processed files and calls the file handler on them.
        /// </summary>
        /// <param name="site">The site to handle files for.</param>
        void SearchAndHandleExistingFiles(SPSite site)
        {
            var siteId = site.ID;

            // If there is no working folder, just return
            if (string.IsNullOrEmpty(_localWorkingFolder))
            {
                return;
            }

            // Get the path to the local working folder for the site.
            string path = ExtractSharePointHelper.BuildLocalWorkingFolderPath(
                siteId, _localWorkingFolder);

            // Ensure the path exists
            if (!Directory.Exists(path))
            {
                return;
            }

            // Handle failed files first [FIDSI #194]
            string[] fileNames = Directory.GetFiles(path,
                "*.failed", SearchOption.AllDirectories);

            HandleFailedFiles(fileNames, path, site);

            fileNames = Directory.GetFiles(path,
                "*.processed", SearchOption.AllDirectories);

            HandleProcessedFiles(fileNames, path, siteId);

            // Handle queued for verify files
            fileNames = Directory.GetFiles(path,
                "*.InVerificationQueue", SearchOption.AllDirectories);
            HandleQueuedVerifiedFiles(fileNames, site);
        }

        /// <summary>
        /// Handles a collection of files that have failed to process.
        /// </summary>
        /// <param name="fileNames">The failed files to handle.</param>
        /// <param name="workingFolder">The path to the local working folder.</param>
        /// <param name="site">The site to handle failed files for.</param>
        void HandleFailedFiles(string[] fileNames, string workingFolder, SPSite site)
        {
            var failedString = ExtractProcessingStatus.ProcessingFailed.AsString();
            Dictionary<string, List<string>> filesToClean = new Dictionary<string, List<string>>();
            foreach (string fileName in fileNames)
            {
                try
                {
                    var pair = ExtractSharePointHelper.BuildListAndFileGuid(fileName);
                    var list = site.RootWeb.Lists[pair.Key];
                    var item = list.GetItemByUniqueId(pair.Value);

                    bool filePreviouslyAttempted = _filesPreviouslyAttempted.Contains(fileName);
                    bool hasFileCompleted = false;

                    ExtractSharePointHelper.DoWithCheckoutIfRequired("ELI35885", item.File, "IDS Status changed.", () =>
                    {
                        string directory = Path.GetDirectoryName(fileName);
                        if (!filesToClean.ContainsKey(directory))
                        {
                            filesToClean.Add(directory, new List<string>());
                        }

                        string spFileName = site.Url + "/" + item.Url;

                        string fileWithoutExtension =
                            Path.GetFileNameWithoutExtension(fileName);
                        filesToClean[directory].Add(fileWithoutExtension);

                        // Log a file failed exception to the exception service
                        var exception = new ProcessingFailedException("ELI31491", "Failed processing file.");
                        exception.Data.Add("Exported File Name", fileWithoutExtension);
                        exception.Data.Add("SP Failed File", spFileName);
                        IdShieldHelper.LogException(exception, ErrorCategoryId.IdShieldDiskToSharePoint,
                            "ELI30572");

                        // Update the IDS status column (do this last so that the above exception is still logged
                        // even if updating the column fails).
                        item[IdShieldHelper.IdShieldStatusColumn] = failedString;
                        item.Update();
                    }, !filePreviouslyAttempted);

                    // update the _filePreviouslyAttempted HashSet
                    if (filePreviouslyAttempted)
                    {
                        if (hasFileCompleted)
                        {
                            _filesPreviouslyAttempted.Remove(fileName);
                        }
                    }
                    else if (!hasFileCompleted)
                    {
                        _filesPreviouslyAttempted.Add(fileName);
                    }
                }
                catch (Exception ex)
                {
                    IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldDiskToSharePoint,
                        "ELI30573");
                }
            }

            CleanupLocalFiles(filesToClean, workingFolder);
        }

        /// <summary>
        /// Handles a collection of files that have been queued to verify.
        /// </summary>
        /// <param name="fileNames">The queued files to handle.</param>
        /// <param name="site">The site to handle queued files for.</param>
        void HandleQueuedVerifiedFiles(string[] fileNames, SPSite site)
        {
            var queuedString = ExtractProcessingStatus.QueuedForVerification.AsString();
            foreach (string fileName in fileNames)
            {
                try
                {
                    var pair = ExtractSharePointHelper.BuildListAndFileGuid(fileName);
                    var list = site.RootWeb.Lists[pair.Key];
                    var item = list.GetItemByUniqueId(pair.Value);
                    
                    bool filePreviouslyAttempted = _filesPreviouslyAttempted.Contains(fileName);
                    bool hasFileCompleted = false;

                    ExtractSharePointHelper.DoWithCheckoutIfRequired("ELI35886", item.File, "IDS Status changed", () =>
                    {
                        // Update the IDS status column
                        item[IdShieldHelper.IdShieldStatusColumn] = queuedString;
                        item.Update();
                        
                        // Delete the .InVerificationQueue file
                        try
                        {
                            // Delete the file (log exception if unable to delete)
                            File.Delete(fileName);
                        }
                        catch (Exception ex2)
                        {
                            IdShieldHelper.LogException(ex2, ErrorCategoryId.IdShieldDiskToSharePoint,
                                "ELI31526");
                        }
                        hasFileCompleted = true;
                    }, !filePreviouslyAttempted);
                    
                    // update the _filePreviouslyAttempted HashSet
                    if (filePreviouslyAttempted)
                    {
                        if (hasFileCompleted)
                        {
                            _filesPreviouslyAttempted.Remove(fileName);
                        }
                    }
                    else if (!hasFileCompleted)
                    {
                        _filesPreviouslyAttempted.Add(fileName);
                    }
                }
                catch (Exception ex)
                {
                    IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldDiskToSharePoint,
                        "ELI31525");
                }
            }
        }

        /// <summary>
        /// Handles a collection of processed files, looking for the .redacted version
        /// and adding it to SharePoint.
        /// </summary>
        /// <param name="fileNames">The processed files to handle.</param>
        /// <param name="workingFolder">The path to the local working folder.</param>
        /// <param name="siteId">The unique ID for the site to push redacted files into.</param>
        // Raising type Exception here because in the case that this occurs there is no
        // more specific exception. It is being thrown from a place where the code is
        // not expected to ever reach.
        // Passing arguments that are within the code to the ArgumentOutOfRange exception
        // that is being raised. FxCop thinks these parameters should be the method
        // parameters. In most cases this would be correct, but here the ArgumentOutOfRange
        // exception is being used since it best fits what is happening. There are SP
        // specific exceptions that could be raised in this situation that would be more
        // correct, but there have been issues with serializing SP exceptions.
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        void HandleProcessedFiles(string[] fileNames, string workingFolder, Guid siteId)
        {
            SPListItem item = null;
            try
            {
                // Get the folder settings
                SiteFolderSettingsCollection folderSettings;
                if (!_folderSettings.TryGetValue(siteId, out folderSettings))
                {
                    return;
                }

                // Build collection of the files to add and the folders/files to clean after adding
                var filesToAdd = new List<FileUploadData>();
                Dictionary<string, List<string>> filesToClean = new Dictionary<string, List<string>>();
                using (SPSite site = new SPSite(siteId))
                {
                    string spBaseFileName = site.Url + "/";

                    SPWeb web = site.RootWeb;
                    foreach (string fileName in fileNames)
                    {
                        try
                        {
                            string directory = Path.GetDirectoryName(fileName);
                            var listIdFileId = ExtractSharePointHelper.BuildListAndFileGuid(fileName);
                            SPList list = null;
                            try
                            {
                                list = web.Lists[listIdFileId.Key];
                                item = list.GetItemByUniqueId(listIdFileId.Value);
                            }
                            catch
                            {
                                Exception ee = null;
                                if (list == null)
                                {
                                    ee = new ArgumentOutOfRangeException("listIdFileId.Key", "Source document library cannot be found.");
                                    ee.Data.Add("Library ID", listIdFileId.Key);
                                }
                                else if (item == null)
                                {
                                    ee = new ArgumentOutOfRangeException("listIdFileId.Value", "Source document cannot be found.");
                                    ee.Data.Add("Library Title", list.Title);
                                }
                                else
                                {
                                    // Should never get here, but just in case
                                    ee = new Exception("Unable to get source library or document.");
                                    ee.Data.Add("Eli Code", "ELI31493");
                                }
                                ee.Data.Add("Source Document ID", listIdFileId.Value);

                                throw ee;
                            }
                            bool filePreviouslyAttempted = _filesPreviouslyAttempted.Contains(fileName);
                            bool hasFileCompleted = false;
                            ExtractSharePointHelper.DoWithCheckoutIfRequired("ELI35887", item.File, "IDS Status changed and processing finished.", () =>
                            {
                                string fileWithoutExtension =
                                   Path.GetFileNameWithoutExtension(fileName);

                                // Build path to redacted file
                                string redactedFile = Path.Combine(directory, fileWithoutExtension)
                                    + ".redacted";
                                if (!File.Exists(redactedFile))
                                {
                                    string[] redactedFiles = Directory.GetFiles(directory,
                                        Path.GetFileNameWithoutExtension(fileWithoutExtension)
                                        + ".*.redacted");
                                    if (redactedFiles.Length == 1)
                                    {
                                        redactedFile = redactedFiles[0];
                                    }
                                }

                                // Ensure the redacted file exists and the destination
                                // file name is not null or empty
                                if (File.Exists(redactedFile))
                                {
                                    // Build the destination file name
                                    string destinationFileName = GetDestinationFileName(item.Url,
                                        Path.GetExtension(Path.GetFileNameWithoutExtension(redactedFile)),
                                        folderSettings);

                                    if (string.IsNullOrEmpty(destinationFileName))
                                    {
                                        var ee2 = new FileNotFoundException(
                                            "Redacted file was found, but could not compute a "
                                        + "destination folder in SharePoint.");
                                        ee2.Data.Add("Processed File Name", Path.Combine(
                                            directory, fileWithoutExtension));
                                        throw ee2;
                                    }

                                    string destFolder = destinationFileName.Substring(0,
                                        destinationFileName.LastIndexOf("/", StringComparison.Ordinal));

                                    // Create the destination folder if necessary
                                    string adjustedDestFolder = EnsureDestinationFolderExists(siteId, destFolder);

                                    // Rebuild the destination name using the adjustedDestFolder
                                    destinationFileName = adjustedDestFolder + destinationFileName.Substring(
                                        destinationFileName.LastIndexOf("/", StringComparison.Ordinal));

                                    FileUploadData fileToUpload = new FileUploadData(listIdFileId.Key,
                                        listIdFileId.Value, spBaseFileName + item.Url,
                                        web.Url + destinationFileName, redactedFile);

                                    // Upload the file to sharepoint
                                    UploadFileToSharePoint(fileToUpload, siteId);

                                    // Add files to clean 
                                    if (!filesToClean.ContainsKey(directory))
                                    {
                                        filesToClean.Add(directory, new List<string>());
                                    }

                                    filesToClean[directory].Add(fileWithoutExtension);

                                    // Add the file to sharepoint
                                    filesToAdd.Add(fileToUpload);
                                }
                                else
                                {
                                    // No redacted file so set status to no redactions
                                    item[IdShieldHelper.IdShieldStatusColumn] =
                                        ExtractProcessingStatus.NoRedactions.AsString();
                                    item.Update();
                                }
                                hasFileCompleted = true;
                            }, !filePreviouslyAttempted);

                            // update the _filePreviouslyAttempted HashSet
                            if (filePreviouslyAttempted)
                            {
                                if (hasFileCompleted)
                                {
                                    _filesPreviouslyAttempted.Remove(fileName);
                                }
                            }
                            else if (!hasFileCompleted)
                            {
                                _filesPreviouslyAttempted.Add(fileName);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Need to update the IdShieldStatusColumn
                            try
                            {
                                if (item != null)
                                {
                                    ExtractSharePointHelper.DoWithCheckoutIfRequired("ELI35888", item.File, "IDS Status changed", () =>
                                    {
                                        item[IdShieldHelper.IdShieldStatusColumn] =
                                            ExtractProcessingStatus.ProcessingFailed.AsString();
                                        item.Update();
                                    }, true);
                                }
                            }
                            catch (Exception e)
                            {
                                SPException spe = new SPException("Unable to update ID Shield Status Column", e);
                                IdShieldHelper.LogException(spe, ErrorCategoryId.IdShieldDiskToSharePoint, "ELI34704");
                            }
                            IdShieldHelper.LogException(ex,
                                ErrorCategoryId.IdShieldDiskToSharePoint, "ELI30600");
                        }
                    }
                }

                // Add the list of files to ignore
                AddFilesToIgnore(filesToAdd, siteId);
                CleanupLocalFiles(filesToClean, workingFolder);
            }
            catch (Exception ex2)
            {
                IdShieldHelper.LogException(ex2, ErrorCategoryId.IdShieldDiskToSharePoint,
                    "ELI30574");
            }
        }

         /// <summary>
        /// Uploads the given file to the site.  This assumes the file original file is checked out
        /// </summary>
        /// <param name="fileToAdd">The upload data for the file to upload</param>
        /// <param name="siteId">The site the file is to be uploaded to</param>
        static void UploadFileToSharePoint(FileUploadData fileToAdd, Guid siteId)
        {
            // Upload the redacted file into SharePoint
            using (SPSite tempSite = new SPSite(siteId))
            {
                var lists = new Dictionary<Guid, SPList>();
                SPWeb tempWeb = tempSite.RootWeb;
                SPFileCollection spFiles = tempWeb.Files;

                try
                {
                    bool previouslyCheckedOut = false;

                    // Check if there is an existing file
                    var existingFile = tempWeb.GetFile(fileToAdd.DestinationUrl);
                    if (existingFile.Exists)
                    {
                        // If the destination exists need and a checkout is required, check out the file
                        if (existingFile.RequiresCheckout)
                        {
                            if (existingFile.CheckOutType == SPFile.SPCheckOutType.None)
                            {
                                existingFile.CheckOut();
                            }
                            else if (existingFile.CheckedOutByUser.Name == tempWeb.CurrentUser.Name)
                            {
                                previouslyCheckedOut = true;
                            }
                            else
                            {
                                string exceptionString = "Destination file is checked out by " + existingFile.CheckedOutByUser.Name;
                                throw new SPException(exceptionString);
                            }
                        }
                    }

                    SPFile currFile = existingFile;
                    try
                    {
                        // Set the properties collection that will be set when the file is added
                        Hashtable properties = new Hashtable();
                        properties.Add(IdShieldHelper.IdShieldStatusColumn,
                            ExtractProcessingStatus.NotProcessed.AsString());
                        properties.Add(IdShieldHelper.IdShieldReferenceColumn, fileToAdd.OriginalFileUrl + ", Original file");

                        // Read the redacted file from the disk
                        byte[] bytes = File.ReadAllBytes(fileToAdd.RedactedFile);
                        currFile = spFiles.Add(fileToAdd.DestinationUrl, bytes, properties, true);
     
                        // if checked out by current user check the file back in
                        if (currFile != null && currFile.CheckOutType != SPFile.SPCheckOutType.None 
                            && currFile.CheckedOutByUser.Name == tempWeb.CurrentUser.Name)
                        {
                            currFile.CheckIn("Redacted file added");
                        }
                    }
                    catch(Exception uploadEx)
                    {
                        // if checked out by current user check the file back in
                        if (currFile != null && currFile.CheckOutType != SPFile.SPCheckOutType.None
                            && currFile.CheckedOutByUser.Name == tempWeb.CurrentUser.Name && !previouslyCheckedOut)
                        {
                            currFile.UndoCheckOut();
                        }
                        throw uploadEx;
                    }

                    // Get the list
                    SPList list = null;
                    if (!lists.TryGetValue(fileToAdd.OriginalListId, out list))
                    {
                        list = tempWeb.Lists[fileToAdd.OriginalListId];
                        lists[fileToAdd.OriginalListId] = list;
                    }

                    // Update the original file column with the redacted file location
                    var item = list.GetItemByUniqueId(fileToAdd.OriginalFileId);

                    item[IdShieldHelper.IdShieldStatusColumn] = ExtractProcessingStatus.Redacted.AsString();
                    item[IdShieldHelper.IdShieldReferenceColumn] = fileToAdd.DestinationUrl + ", Redacted file";

                    item.Update();
                }
                catch (Exception ex)
                {
                    ex.Data.Add("Site Id", siteId.ToString());
                    ex.Data.Add("Site Url", tempSite.Url);
                    ex.Data.Add("Source File", fileToAdd.OriginalFileUrl);
                    ex.Data.Add("Destination Url", fileToAdd.DestinationUrl);
                    // Rethrow this should be handled in a higher scope

                    throw ex;
                }
            }
        }


        /// <summary>
        /// Cleans up all files for the current processed file, deleting empty directories
        /// as it goes.
        /// </summary>
        /// <param name="filesToClean">A collection of directories and lists of files within
        /// those directories to clean up.</param>
        /// <param name="workingFolder">The path to the local working folder.</param>
        static void CleanupLocalFiles(Dictionary<string, List<string>> filesToClean,
            string workingFolder)
        {
            // Get the collection of keys in reverse sorted order
            List<string> directories = new List<string>(filesToClean.Keys);
            directories.Sort();
            directories.Reverse();
            foreach (string directory in directories)
            {
                List<string> files = filesToClean[directory];
                try
                {
                    foreach (string file in files)
                    {
                        string baseFileName = Path.GetFileNameWithoutExtension(file);

                        // Search for and delete any files related to the base file in the directory.
                        string[] filesToDelete = Directory.GetFiles(directory, baseFileName + ".*");
                        foreach (string fileToDelete in filesToDelete)
                        {
                            File.Delete(fileToDelete);
                        }
                    }

                    // Check for empty directory
                    string directoryToDelete = directory;
                    while (!directoryToDelete.Equals(workingFolder, StringComparison.OrdinalIgnoreCase)
                        && Directory.Exists(directoryToDelete)
                        && Directory.GetFiles(directoryToDelete).Length == 0
                        && Directory.GetDirectories(directoryToDelete).Length == 0)
                    {
                        Directory.Delete(directoryToDelete);
                        directoryToDelete = Path.GetDirectoryName(directoryToDelete);
                    }
                }
                catch (Exception ex)
                {
                    IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldDiskToSharePoint,
                        "ELI30575");
                }
            }
        }

        /// <summary>
        /// Attempts to open the destination folder and if it doesn't exist will attempt
        /// to create it.
        /// </summary>
        /// <param name="siteId">The id of the site to create the folder on.</param>
        /// <param name="destFolder">The web relative path to the destination folder.</param>
        /// <returns>The destination folder that uses the internal name of the library</returns>
        static string EnsureDestinationFolderExists(Guid siteId, string destFolder)
        {
            string url = string.Empty;
            using (var site = new SPSite(siteId))
            {
                try
                {
                    Guid listId = Guid.Empty;
                    SPWeb web = site.RootWeb;
                    url = web.Url + destFolder;
                    SPFolder folder = web.GetFolder(url);
                    string returnDestFolder = destFolder;
                    if (folder.Exists)
                    {
                        listId = folder.ParentListId;
                    }
                    else
                    {
                        string[] folders = destFolder.Split(new char[] { '/' },
                            StringSplitOptions.RemoveEmptyEntries);
                        string rootFolder = folders[0];
                        
                        SPList list = EnsureRootListExists(site, rootFolder);

                        // Set the root folder to the name from the list
                        rootFolder = list.RootFolder.Name;
                        folders[0] = rootFolder;
                        listId = list.ID;
                        web.AllowUnsafeUpdates = true;
                        try
                        {
                            for (int i = 1; i < folders.Length; i++)
                            {
                                string tempFolder = folders[i];
                                string newRootFolder = rootFolder + "/"
                                    + tempFolder;
                                if (!web.GetFolder(newRootFolder).Exists)
                                {
                                    SPListItem item = list.AddItem(rootFolder,
                                        SPFileSystemObjectType.Folder, tempFolder);
                                    item.Update();
                                }
                                rootFolder = newRootFolder;
                            }
                            web.Update();
                        }
                        finally
                        {
                            web.AllowUnsafeUpdates = false;
                        }
                        // Build up the destination folder 
                        returnDestFolder = "/" + string.Join("/", folders);
                    }
                    if (listId != Guid.Empty)
                    {
                        IdShieldHelper.AddIdShieldReferenceColumn(siteId, listId);
                    }
                    else
                    {
                        SPException spe = new SPException("List Id is empty.");
                        throw spe;
                    }
                    return returnDestFolder;
                }
                catch (Exception ex)
                {
                    SPException exception = new SPException("Unable to create destination folder.",
                        ex);
                    exception.Data.Add("Destination Folder", destFolder);
                    exception.Data.Add("Web Url", url);
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Gets the root list from the SharePoint web (will create the list
        /// if it does not exist).
        /// <para><b>Note:</b></para>
        /// Do not call this method unless you have first called <see cref="EnsureRootListExists"/>
        /// </summary>
        /// <param name="web">The web to get the list from.</param>
        /// <param name="folderName">The folder name to find/create.</param>
        /// <returns>The root list for the item.</returns>
        static SPList GetDocumentList(SPWeb web, string folderName)
        {
            SPList rootList = getDocumentLibrary(web.Lists, folderName);
                
            return rootList;
        }

        /// <summary>
        /// Ensures the root specified list exists and if it does not exist will create it.
        /// </summary>
        /// <param name="site">The site to create the list on if needed.</param>
        /// <param name="folderName">The root list name.</param>
        /// <returns>The SPList object for the Document Library found by looking for 
        /// the document library that has a internal name of folderName</returns>
        static SPList EnsureRootListExists(SPSite site, string folderName)
        {
            var web = site.RootWeb;
            SPListCollection lists = web.Lists;

            // Get the documentLibrary
            SPList rootList = getDocumentLibrary(lists, folderName);

            if (rootList == null)
            {
                // Throw an exception if the document library was not found
                SPException spex = new SPException("Document Library not found");
                spex.Data.Add("Expected Document Library", folderName);
                throw spex;
            }
            return rootList;
        }

        /// <summary>
        /// Gets the name of the destination file based on the current settings.
        /// </summary>
        /// <param name="sourceFileUrl">The site relative url for the source file.</param>
        /// <param name="folderSettings">The settings collection to use to
        /// build the destination file name.</param>
        /// <param name="redactedExtension">The extension of the redacted file.</param>
        /// <returns>The destination for the file within the SP document library.</returns>
        static string GetDestinationFileName(string sourceFileUrl, string redactedExtension,
            SiteFolderSettingsCollection folderSettings)
        {
            if (string.IsNullOrEmpty(sourceFileUrl))
            {
                throw new ArgumentException("Source file url cannot be null or empty.", "sourceFileUrl");
            }
            if (sourceFileUrl[0] != '/')
            {
                sourceFileUrl = sourceFileUrl.Insert(0, "/");
            }

            StringBuilder destination = new StringBuilder(255);
            string fileName = Path.GetFileNameWithoutExtension(sourceFileUrl);
            string folder = Path.GetDirectoryName(sourceFileUrl).Replace('\\', '/');
            string folderUpOne = Path.GetDirectoryName(folder).Replace('\\', '/');
            if (!folderUpOne.EndsWith("/", StringComparison.Ordinal))
            {
                folderUpOne = folderUpOne + "/";
            }
            string topFolder = Path.GetFileName(folder);

            if (!Path.GetExtension(fileName).Equals(
                redactedExtension, StringComparison.OrdinalIgnoreCase))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName) + redactedExtension;
            }

            // Find the folder settings
            foreach (KeyValuePair<string, IdShieldFolderProcessingSettings> pair in folderSettings)
            {
                IdShieldFolderProcessingSettings settings = pair.Value;
                if (folder.Equals(pair.Key, StringComparison.Ordinal)
                    || (folder.StartsWith(pair.Key + "/", StringComparison.Ordinal)
                    && settings.RecurseSubfolders))
                {
                    // Compute the destination setting
                    switch (settings.OutputLocation)
                    {
                        case IdShieldOutputLocation.ParallelFolderPrefix:
                        case IdShieldOutputLocation.ParallelFolderSuffix:
                            destination.Append(folderUpOne);
                            if (settings.OutputLocation == IdShieldOutputLocation.ParallelFolderPrefix)
                            {
                                destination.Append(settings.OutputLocationString);
                                destination.Append("_");
                                destination.Append(topFolder);
                            }
                            else
                            {
                                destination.Append(topFolder);
                                destination.Append("_");
                                destination.Append(settings.OutputLocationString);
                            }

                            destination.Append("/");
                            destination.Append(fileName);
                            break;

                        case IdShieldOutputLocation.Subfolder:
                            destination.Append(folder);
                            destination.Append("/");
                            destination.Append(settings.OutputLocationString);
                            destination.Append("/");
                            destination.Append(fileName);
                            break;

                        case IdShieldOutputLocation.PrefixFilename:
                        case IdShieldOutputLocation.SuffixFilename:
                            destination.Append(folder);
                            destination.Append("/");
                            if (settings.OutputLocation == IdShieldOutputLocation.PrefixFilename)
                            {
                                destination.Append(settings.OutputLocationString);
                                destination.Append("_");
                                destination.Append(fileName);
                            }
                            else
                            {
                                string extension = Path.GetExtension(fileName);
                                string name = Path.GetFileNameWithoutExtension(fileName);
                                destination.Append(name);
                                destination.Append("_");
                                destination.Append(settings.OutputLocationString);
                                destination.Append(extension);
                            }
                            break;

                        case IdShieldOutputLocation.MirrorDocumentLibrary:
                            // Append the closing slash to the folder
                            string dest = folder + "/";

                            // Set destination to parallel location in specified library
                            destination.Append(_findRoot 
                                .Replace(dest, settings.OutputLocationString, 1));

                            destination.Append(fileName);
                            break;
                    }
                    break;
                }
            }

            return destination.ToString();
        }

        /// <summary>
        /// Checks the feature and ensures the settings are updated.
        /// </summary>
        void UpdateSettings()
        {
            IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
            if (settings == null)
            {
                _localWorkingFolder = string.Empty;
                _folderSettings = null;
                return;
            }

            _folderSettings = new IdShieldFolderSettingsCollection();
            foreach (var siteId in settings.IdShieldSites)
            {
                var folderSettings = IdShieldHelper.GetIdShieldFolderSettings(siteId);
                _folderSettings.Add(siteId, folderSettings);
            }

            _localWorkingFolder = settings.LocalWorkingFolder;
            _activeSites = new HashSet<Guid>(settings.ActiveSites);
        }

        /// <summary>
        /// Adds the list of file urls to the hidden list of files to ignore when they
        /// are seen in the add event handler.
        /// </summary>
        /// <param name="fileData">The file urls to add to the list.</param>
        /// <param name="siteId">The guid for the site containing the hidden list.</param>
        static void AddFilesToIgnore(IEnumerable<FileUploadData> fileData, Guid siteId)
        {
            using (SPSite tempSite = new SPSite(siteId))
            {
                var web = tempSite.RootWeb;
                if (web != null)
                {
                    SPList list = web.Lists.TryGetList(IdShieldHelper._HIDDEN_IGNORE_FILE_LIST);
                    if (list != null)
                    {
                        foreach (var file in fileData)
                        {
                            SPListItem item = list.AddItem();
                            item["Title"] = file.DestinationUrl;
                            item.Update();
                        }

                        list.Update();
                    }
                    web.Update();
                }
            }
        }

        /// <summary>
        /// Remove the list of files that have now been uploaded from the list
        /// of files to ignore.
        /// </summary>
        /// <param name="siteId">The guid for the site containing the list of
        /// files to ignore.</param>
        static void RemoveFilesToIgnore(Guid siteId)
        {
            using (SPSite tempSite = new SPSite(siteId))
            {
                var web = tempSite.RootWeb;
                if (web != null)
                {
                    SPList list = web.Lists.TryGetList(IdShieldHelper._HIDDEN_IGNORE_FILE_LIST);
                    if (list != null)
                    {
                        // Build the CAML query to get all items that are at least 10 minutes old
                        var dateTime = DateTime.Now.AddMinutes(-10.0);
                        string queryString = string.Concat(
                            "<Where><Leq><FieldRef Name='Modified'/>",
                            "<Value IncludeTimeValue='TRUE' Type='DateTime'>",
                            SPUtility.CreateISO8601DateTimeFromSystemDateTime(dateTime),
                            "</Value></Leq></Where>");

                        // Get the collection of files from the list and delete each one
                        SPQuery query = new SPQuery();
                        query.Query = queryString;
                        SPListItemCollection items = list.GetItems(query);
                        for (int i = items.Count - 1; i >= 0; i--)
                        {
                            items[i].Delete();
                        }

                        // Update the list
                        list.Update();
                    }

                    // Update the web object
                    web.Update();
                }
            }
        }

        /// <summary>
        /// Gets the document library that has the internal name of folderName
        /// </summary>
        /// <param name="lists">The Site list collection</param>
        /// <param name="folderName">Internal name of library to find</param>
        /// <returns>The matching document library if not found returns null </returns>
        static SPList getDocumentLibrary(SPListCollection lists, string folderName)
        {
            // Search all list for list with rootFolder name of folderName
            foreach (SPList l in lists)
            {
                if (l.RootFolder.Name == folderName)
                {
                    return l;
                }
            }
            return null;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the description for this custom timer
        /// </summary>
        public override string Description
        {
            get
            {
                return _DESCRIPTION;
            }
        }

        #endregion Properties
    }
}
