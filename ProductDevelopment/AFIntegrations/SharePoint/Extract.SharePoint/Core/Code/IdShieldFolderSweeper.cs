using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Using statements to make dealing with folder settings more readable
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>;
using IdShieldFolderSettingsCollection =
System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>>;

namespace Extract.SharePoint.Redaction
{
    /// <summary>
    /// SharePoint timer job that sweeps the local working folder for files that have
    /// finished/failed processing and then pushes them back into SharePoint.
    /// </summary>
    public class IdShieldFolderSweeper : SPJobDefinition
    {
        #region Constants

        /// <summary>
        /// The title for this timer
        /// </summary>
        const string _TITLE = "Extract Systems ID Shield Folder Sweeper";

        /// <summary>
        /// The description for this timer
        /// </summary>
        const string _DESCRIPTION = "This timer object will sweep the configured local "
            + "working folder, searching for processed files and will then push the "
            + "processed files back into the SharePoint site.";

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
                foreach (SPSite site in this.WebApplication.Sites)
                {
                    if (_activeSites.Contains(site.ID))
                    {
                        SearchAndHandleExistingFiles(site);
                    }
                }

                base.Execute(targetInstanceId);
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex,
                    ErrorCategoryId.IdShieldFolderSweeper, "ELI30599");
            }
        }

        /// <summary>
        /// Searches for existing processed files and calls the file handler on them.
        /// </summary>
        void SearchAndHandleExistingFiles(SPSite site)
        {
            // If there is no working folder, just return
            if (string.IsNullOrEmpty(_localWorkingFolder))
            {
                return;
            }

            string path = Path.Combine(_localWorkingFolder, site.ID.ToString());

            // Ensure the path exists
            if (!Directory.Exists(path))
            {
                return;
            }

            string[] fileNames = Directory.GetFiles(path,
                "*.processed", SearchOption.AllDirectories);

            HandleProcessedFiles(fileNames, path, site);

            fileNames = Directory.GetFiles(path,
                "*.failed", SearchOption.AllDirectories);

            HandleFailedFiles(fileNames, path, site.ServerRelativeUrl);
        }

        /// <summary>
        /// Handles a collection of files that have failed to process.
        /// </summary>
        /// <param name="fileNames">The failed files to handle.</param>
        /// <param name="workingFolder">The path to the local working folder.</param>
        /// <param name="siteUrl">The url for the current site.</param>
        static void HandleFailedFiles(string[] fileNames, string workingFolder, string siteUrl)
        {
            Dictionary<string, List<string>> filesToClean = new Dictionary<string, List<string>>();
            foreach (string fileName in fileNames)
            {
                try
                {
                    string directory = Path.GetDirectoryName(fileName);
                    if (!filesToClean.ContainsKey(directory))
                    {
                        filesToClean.Add(directory, new List<string>());
                    }

                    string fileWithoutExtension =
                        Path.GetFileNameWithoutExtension(fileName);
                    filesToClean[directory].Add(fileWithoutExtension);

                    string folder = directory.Replace(workingFolder, "").Replace("\\", "/");
                    string spFileName = siteUrl + "/" + folder + "/" + fileWithoutExtension;

                    // Log a file failed exception to the exception service
                    SPException exception = new SPException("Failed processing file: "
                        + spFileName);
                    exception.Data.Add("SP Failed File", spFileName);
                    IdShieldHelper.LogException(exception, ErrorCategoryId.IdShieldFolderSweeper,
                        "ELI30572");
                }
                catch (Exception ex)
                {
                    IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldFolderSweeper,
                        "ELI30573");
                }
            }

            CleanupLocalFiles(filesToClean, workingFolder);
        }

        /// <summary>
        /// Handles a collection of processed files, looking for the .redacted version
        /// and adding it to SharePoint.
        /// </summary>
        /// <param name="fileNames">The processed files to handle.</param>
        /// <param name="workingFolder">The path to the local working folder.</param>
        /// <param name="site">The current site to push redacted files into.</param>
        void HandleProcessedFiles(string[] fileNames, string workingFolder, SPSite site)
        {
            try
            {
                // Get the folder settings
                SiteFolderSettingsCollection folderSettings;
                if (!_folderSettings.TryGetValue(site.ID, out folderSettings))
                {
                    return;
                }

                // Build collection of the files to add and the folders/files to clean after adding
                Dictionary<string, string> filesToAdd = new Dictionary<string, string>();
                Dictionary<string, List<string>> filesToClean = new Dictionary<string, List<string>>();
                using (SPWeb web = site.OpenWeb())
                {
                    foreach (string fileName in fileNames)
                    {
                        try
                        {
                            string directory = Path.GetDirectoryName(fileName);
                            if (!filesToClean.ContainsKey(directory))
                            {
                                filesToClean.Add(directory, new List<string>());
                            }

                            string fileWithoutExtension =
                                Path.GetFileNameWithoutExtension(fileName);
                            filesToClean[directory].Add(fileWithoutExtension);

                            // Build path to redacted file
                            string redactedFile = Path.Combine(directory, fileWithoutExtension)
                                + ".redacted";

                            // Build the destination file name
                            string destinationFileName = GetDestinationFileName(fileName,
                                workingFolder + "\\", folderSettings);

                            // Ensure the redacted file exists and the destination
                            // file name is not null or empty
                            if (File.Exists(redactedFile)
                                && !string.IsNullOrEmpty(destinationFileName))
                            {
                                string destFolder = destinationFileName.Substring(0,
                                    destinationFileName.LastIndexOf("/", StringComparison.Ordinal));

                                // Create the destination folder if necessary
                                EnsureDestinationFolderExists(web, destFolder);

                                string destinationUrl = web.Url + destinationFileName;
                                filesToAdd.Add(destinationUrl, redactedFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            IdShieldHelper.LogException(ex,
                                ErrorCategoryId.IdShieldFolderSweeper, "ELI30600");
                        }
                    }
                }

                UploadFilesToSharePoint(filesToAdd, site.Url);
                CleanupLocalFiles(filesToClean, workingFolder);
            }
            catch (Exception ex2)
            {
                IdShieldHelper.LogException(ex2, ErrorCategoryId.IdShieldFolderSweeper,
                    "ELI30574");
            }
        }

        /// <summary>
        /// Will upload the collection of site urls and local redacted files into SharePoint.
        /// </summary>
        /// <param name="filesToAdd">Collection of site urls to local redacted files.</param>
        /// <param name="url">The url of the site to upload the files to.</param>
        static void UploadFilesToSharePoint(Dictionary<string, string> filesToAdd, string url)
        {
            // Add the list of files to ignore
            AddFilesToIgnore(filesToAdd.Keys, url);

            // Upload the redacted file into SharePoint
            using (SPSite tempSite = new SPSite(url))
            using (SPWeb tempWeb = tempSite.OpenWeb())
            {
                foreach (KeyValuePair<string, string> pair in filesToAdd)
                {
                    try
                    {
                        // Read the redacted file from the disk
                        byte[] bytes = File.ReadAllBytes(pair.Value);

                        tempWeb.Files.Add(pair.Key, bytes, true);
                    }
                    catch (Exception ex)
                    {
                        ex.Data.Add("Destination Url", pair.Key);
                        IdShieldHelper.LogException(ex,
                            ErrorCategoryId.IdShieldFolderSweeper, "ELI30598");
                    }
                }
                tempWeb.Update();
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
                    IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldFolderSweeper,
                        "ELI30575");
                }
            }
        }

        /// <summary>
        /// Attempts to open the destination folder and if it doesn't exist will attempt
        /// to create it.
        /// </summary>
        /// <param name="web">The SP web to create the folder on.</param>
        /// <param name="destFolder">The web relative path to the destination folder.</param>
        static void EnsureDestinationFolderExists(SPWeb web, string destFolder)
        {
            string url = string.Empty;
            try
            {
                url = web.Url + destFolder;
                SPFolder folder = web.GetFolder(url);
                if (!folder.Exists)
                {
                    string[] folders = destFolder.Split(new char[] { '/' },
                        StringSplitOptions.RemoveEmptyEntries);
                    string rootFolder = folders[0];
                    SPList list = GetDocumentList(web, rootFolder);
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

        /// <summary>
        /// Gets the root list from the SharePoint web (will create the list
        /// if it does not exist).
        /// </summary>
        /// <param name="web">The web to get the list from.</param>
        /// <param name="folderName">The folder name to find/create.</param>
        /// <returns>The root list for the item.</returns>
        static SPList GetDocumentList(SPWeb web, string folderName)
        {
            SPList rootList = web.Lists.TryGetList(folderName);
            if (rootList == null)
            {
                web.AllowUnsafeUpdates = true;
                web.Lists.Add(folderName, "Redacted Documents", SPListTemplateType.DocumentLibrary);
                web.Update();
                web.AllowUnsafeUpdates = false;
            }

            rootList = web.Lists[folderName];
            return rootList;
        }

        /// <summary>
        /// Gets the name of the destination file based on the current settings.
        /// </summary>
        /// <param name="fullPath">The full path to the .processed file.</param>
        /// <param name="folderSettings">The settings collection to use to
        /// build the destination file name.</param>
        /// <param name="watchPath">The watch folder that is being monitored for
        /// processed documents.</param>
        /// <returns>The destination for the file within the SP document library.</returns>
        static string GetDestinationFileName(string fullPath, string watchPath,
            SortedDictionary<string, FolderProcessingSettings> folderSettings)
        {
            StringBuilder destination = new StringBuilder();
            string path = fullPath;
            string fileName = Path.GetFileNameWithoutExtension(fullPath);
            string folder = Path.GetDirectoryName(path);
            string folderUpOne = Path.GetDirectoryName(folder) + "\\";
            string topFolder = folder.Replace(folderUpOne, "");
            folder = folder.Replace(watchPath, "/").Replace("\\", "/");

            // Find the folder settings
            foreach (KeyValuePair<string, FolderProcessingSettings> pair in folderSettings)
            {
                FolderProcessingSettings settings = pair.Value;
                if (folder.Equals(pair.Key, StringComparison.Ordinal)
                    || (folder.StartsWith(pair.Key + "/", StringComparison.Ordinal)
                    && settings.RecurseSubfolders))
                {
                    // Compute the destination setting
                    switch (settings.OutputLocation)
                    {
                        case IdShieldOutputLocation.ParallelFolderPrefix:
                        case IdShieldOutputLocation.ParallelFolderSuffix:
                            // Need to configure the folderUpOne name properly
                            // for file/folder access in SP
                            folderUpOne = folderUpOne.Replace(watchPath, "/");
                            folderUpOne = folderUpOne.Remove(folderUpOne.Length - 1).Replace("\\", "/");

                            destination.Append(folderUpOne);
                            destination.Append("/");
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

                        case IdShieldOutputLocation.CustomOutputLocation:
                            destination.Append(settings.OutputLocationString);
                            if (!settings.OutputLocationString.EndsWith("/", StringComparison.Ordinal))
                            {
                                destination.Append("/");
                            }
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
            _folderSettings = FolderProcessingSettings.DeserializeFolderSettings(settings.FolderSettings);
            _localWorkingFolder = settings.LocalWorkingFolder;
            _activeSites = new HashSet<Guid>(settings.ActiveSites);
        }

        /// <summary>
        /// Adds the list of file urls to the hidden list of files to ignore when they
        /// are seen in the add event handler.
        /// </summary>
        /// <param name="fileUrls">The file urls to add to the list.</param>
        /// <param name="siteUrl">The url for the site containing the hidden list.</param>
        static void AddFilesToIgnore(IEnumerable<string> fileUrls, string siteUrl)
        {
            using (SPSite tempSite = new SPSite(siteUrl))
            {
                SPWeb web = tempSite.RootWeb;
                if (web != null)
                {
                    SPList list = web.Lists.TryGetList(IdShieldHelper._HIDDEN_LIST_NAME);
                    if (list != null)
                    {
                        foreach (string fileUrl in fileUrls)
                        {
                            SPListItem item = list.AddItem();
                            item["Title"] = fileUrl;
                            item.Update();
                        }

                        list.Update();
                    }
                    web.Update();
                }
            }
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
