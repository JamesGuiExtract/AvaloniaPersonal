using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Extract.SharePoint.Redaction
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class IdShieldFileListener : SPItemEventReceiver
    {
        #region Constants

        static readonly string _LOCK_FILE_NAME = "FACC71A83B794B5E97177200F48F6677.lock";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Collection to manage the current folder watch settings.
        /// </summary>
        SortedDictionary<string, FolderProcessingSettings> _folderSettings =
            new SortedDictionary<string, FolderProcessingSettings>();

        /// <summary>
        /// Holds the folder serialization string so that it can be compared
        /// and deserialized if the settings are updated.
        /// </summary>
        string _folderSettingsSerializationString = string.Empty;

        /// <summary>
        /// The output folder that files should be written to for processing
        /// </summary>
        string _outputFolder = string.Empty;

        /// <summary>
        /// The url for the SPWeb that the event listener is attached to
        /// </summary>
        string _url;

        /// <summary>
        /// Url relative to the server for the site
        /// </summary>
        string _serverRelativeUrl;

        /// <summary>
        /// The id for the site.
        /// </summary>
        Guid _siteId;

        /// <summary>
        /// Mutex used to serialize access to the UpdateSettings calls.
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldFileListener"/> class.
        /// </summary>
        public IdShieldFileListener()
            : base()
        {
            try
            {
                // Store the URLs and the site ID
                _url = SPContext.Current.Site.Url;
                _serverRelativeUrl = SPContext.Current.Site.ServerRelativeUrl;
                _siteId = SPContext.Current.Site.ID;

                // Launch the folder watcher thread
                Thread folderWatcher = new Thread(FolderWatcherThread);
                folderWatcher.Start();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            try
            {
                base.ItemAdded(properties);
                HandleSharePointFileEvent(properties, FileEventType.FileAdded);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            try
            {
                base.ItemUpdated(properties);
                HandleSharePointFileEvent(properties, FileEventType.FileModified);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Handles the file event, checking the current settings and exports any files
        /// that match the current settings to the specified processing folder.
        /// </summary>
        /// <param name="properties">The properties for the file event.</param>
        /// <param name="eventType">The type of event that is being handled.</param>
        void HandleSharePointFileEvent(SPItemEventProperties properties,
            FileEventType eventType)
        {
            // Get the item and check that it is a file item
            SPListItem item = properties.ListItem;
            if (item.FileSystemObjectType != SPFileSystemObjectType.File)
            {
                return;
            }

            // Update the settings
            UpdateSettings();

            // Check for an output folder (if none is configured then do nothing)
            if (!string.IsNullOrEmpty(_outputFolder))
            {
                // Get the folder name for the item
                string folder = item.File.Url;
                folder = (folder[0] != '/' ?
                    folder.Insert(0, "/") : folder).Replace("/" + item.File.Name, "");

                // Attempt to get the settings for the folder
                foreach (KeyValuePair<string, FolderProcessingSettings> pair in _folderSettings)
                {
                    // Export the file if:
                    // 1. This folder is being watched
                    // 2. The folder is being watched for the specified event
                    // 3. The file matches the watch pattern
                    if ((folder.Equals(pair.Key, StringComparison.Ordinal)
                        || (folder.StartsWith(pair.Key, StringComparison.Ordinal)
                        && pair.Value.RecurseSubfolders))
                        && (pair.Value.EventTypes & eventType) != 0
                        && pair.Value.DoesFileMatchPattern(item.File.Name))
                    {
                        // get the folder name without the leading '/' and
                        // convert all other '/' to '\'
                        folder = folder.Substring(1).Replace('/', '\\');
                        string outputFolder = Path.Combine(_outputFolder, folder);
                        if (!Directory.Exists(outputFolder))
                        {
                            Directory.CreateDirectory(outputFolder);
                        }

                        // Write the file to the processing folder
                        string fileName = Path.Combine(outputFolder, item.File.Name);
                        byte[] bytes = item.File.OpenBinary(SPOpenBinaryOptions.SkipVirusScan);
                        File.WriteAllBytes(fileName, bytes);

                        // File was exported, break from foreach loop
                        break;
                    }
                } // End foreach loop
            }
        }

        /// <summary>
        /// Checks the feature and ensures the settings are updated.
        /// </summary>
        void UpdateSettings()
        {
            lock (_lock)
            {
                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
                if (settings == null)
                {
                    _outputFolder = string.Empty;
                    _folderSettings = null;
                    _folderSettingsSerializationString = string.Empty;
                    return;
                }
                string temp = settings.FolderSettings;
                if (temp.Length != _folderSettingsSerializationString.Length
                    || !temp.Equals(_folderSettingsSerializationString, StringComparison.Ordinal))
                {
                    _folderSettings =
                        FolderProcessingSettings.DeserializeFolderSettings(temp, _serverRelativeUrl);
                    _folderSettingsSerializationString = temp;
                }

                if (!string.IsNullOrEmpty(settings.LocalWorkingFolder))
                {
                    _outputFolder = Path.Combine(settings.LocalWorkingFolder,
                        _serverRelativeUrl.Substring(1).Replace('/', '\\'));
                }
            }
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
                    || (folder.StartsWith(pair.Key, StringComparison.Ordinal)
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
        /// Thread method launched to perform folder watching for processed files.
        /// </summary>
        void FolderWatcherThread()
        {
            try
            {
                using (LockFileManager lockFile = new LockFileManager())
                using (FileSystemWatcher processedWatcher = new FileSystemWatcher())
                using (FileSystemWatcher failedWatcher = new FileSystemWatcher())
                {
                    while (string.IsNullOrEmpty(_outputFolder))
                    {
                        // Sleep to give the web a chance to populate the activated feature
                        Thread.Sleep(1000);

                        using (SPSite site = new SPSite(_url))
                        {
                            SPFeature feature = IdShieldHelper.GetIdShieldFeature(site);
                            if (feature == null)
                            {
                                // If the feature is null it is not activated
                                // just return to exit thread
                                return;
                            }

                            // Update the settings
                            UpdateSettings();
                        }
                    }

                    // Attempt to place a lock file (singleton thread helper)
                    if (!lockFile.TryCreateLockFile(Path.Combine(_outputFolder, _LOCK_FILE_NAME)))
                    {
                        // Lock file exists, just return to exit thread
                        return;
                    }

                    try
                    {
                        LogThreadStart();

                        // Search for any existing .processed files
                        SearchAndHandleExistingFiles();

                        // Watch for new files
                        processedWatcher.Path = _outputFolder;
                        processedWatcher.Filter = "*.processed";
                        processedWatcher.NotifyFilter = NotifyFilters.FileName;
                        processedWatcher.Created += HandleFileCreated;
                        processedWatcher.IncludeSubdirectories = true;
                        failedWatcher.Path = _outputFolder;
                        failedWatcher.Filter = "*.failed";
                        failedWatcher.NotifyFilter = NotifyFilters.FileName;
                        failedWatcher.Created += HandleFileFailed;
                        failedWatcher.IncludeSubdirectories = true;

                        processedWatcher.EnableRaisingEvents = true;
                        failedWatcher.EnableRaisingEvents = true;

                        SPFeature featureTemp;
                        do
                        {
                            // Wait for the feature to deactivate
                            Thread.Sleep(5000);
                            using (SPSite site = new SPSite(_url))
                            {
                                featureTemp = IdShieldHelper.GetIdShieldFeature(site);
                            }
                        }
                        while (featureTemp != null);

                        // Feature deactivated, stop watching for processed files
                        processedWatcher.EnableRaisingEvents = false;
                        failedWatcher.EnableRaisingEvents = false;
                    }
                    finally
                    {
                        LogThreadExit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        /// <summary>
        /// Logs an application trace indicating that the folder watching has started
        /// </summary>
        void LogThreadStart()
        {
            try
            {
                SPException ee = new SPException(
                    "Application Trace: ID Shield feature folder watching thread started.");
                ee.Data.Add("Current Site Id", _siteId.ToString());
                ee.Data.Add("Current Site Url", _serverRelativeUrl);
                LogException(ee);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Logs an application trace indicating that folder watching has ended
        /// </summary>
        void LogThreadExit()
        {
            try
            {
                SPException ee = new SPException(
                    "Application Trace: ID Shield feature folder watching thread exited.");
                ee.Data.Add("Current Site Id", _siteId.ToString());
                ee.Data.Add("Current Site Url", _serverRelativeUrl);
                LogException(ee);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Searches for existing processed files and calls the file handler on them.
        /// </summary>
        void SearchAndHandleExistingFiles()
        {
            string[] fileNames = Directory.GetFiles(_outputFolder,
                "*.processed", SearchOption.AllDirectories);

            HandleProcessedFiles(fileNames);

            fileNames = Directory.GetFiles(_outputFolder,
                "*.failed", SearchOption.AllDirectories);

            HandleFailedFiles(fileNames);
        }

        /// <summary>
        /// Handles a collection of files that have failed to process.
        /// </summary>
        /// <param name="fileNames">The failed files to handle.</param>
        void HandleFailedFiles(string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                try
                {
                    string directory = Path.GetDirectoryName(fileName);
                    string fileWithoutExtension =
                        Path.GetFileNameWithoutExtension(fileName);
                    string folder = directory.Replace(_outputFolder, "").Replace("\\", "/");
                    string spFileName = folder + "/" + fileWithoutExtension;

                    // Log a file failed exception to the exception service
                    SPException exception = new SPException("Failed processing file: "
                        + spFileName);
                    exception.Data.Add("SP Failed File", spFileName);
                    LogException(exception);

                    // Cleanup all files related to this file
                    CleanupLocalFiles(fileWithoutExtension, directory);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
        }

        /// <summary>
        /// Handles a collection of processed files, looking for the .redacted version
        /// and adding it to SharePoint.
        /// </summary>
        /// <param name="fileNames">The processed files to handle.</param>
        void HandleProcessedFiles(string[] fileNames)
        {
            try
            {
                using (SPSite site = new SPSite(_url))
                using (SPWeb web = site.OpenWeb())
                {
                    UpdateSettings();
                    if (_folderSettings != null)
                    {
                        foreach (string fileName in fileNames)
                        {
                            string directory = Path.GetDirectoryName(fileName);
                            string fileWithoutExtension =
                                Path.GetFileNameWithoutExtension(fileName);
                            // Build path to redacted file
                            string redactedFile = Path.Combine(directory, fileWithoutExtension)
                                + ".redacted";

                            // Build the destination file name
                            string destinationFileName = GetDestinationFileName(fileName,
                                _outputFolder + "\\", _folderSettings);

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
                                try
                                {
                                    // Read the redacted file from the disk
                                    byte[] bytes = File.ReadAllBytes(redactedFile);

                                    // Upload the redacted file into SharePoint
                                    // NOTE: Need to turn off event firing while the file is
                                    // added to prevent inifinte looping
                                    EventFiringEnabled = false;
                                    web.Files.Add(destinationUrl, bytes, true);
                                    web.Update();
                                    EventFiringEnabled = true;
                                }
                                catch (Exception ex)
                                {
                                    ex.Data.Add("File To Add", fileName);
                                    ex.Data.Add("Destination Url", destinationUrl);
                                    throw;
                                }
                            }

                            // Cleanup all files related to this file
                            CleanupLocalFiles(fileWithoutExtension, directory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        /// <summary>
        /// Cleans up all files for the current processed file, deleting empty directories
        /// as it goes.
        /// </summary>
        /// <param name="sourceFile">The file that finished processing (ex. 123.tif)</param>
        /// <param name="directory">The directory containing the file.</param>
        void CleanupLocalFiles(string sourceFile, string directory)
        {
            try
            {
                string baseFileName = Path.GetFileNameWithoutExtension(sourceFile);

                // Search for and delete any files related to the base file in the directory.
                string[] files = Directory.GetFiles(directory, baseFileName + ".*");
                foreach (string file in files)
                {
                    File.Delete(file);
                }

                // Check for empty directory
                while (!directory.Equals(_outputFolder, StringComparison.OrdinalIgnoreCase)
                    && Directory.Exists(directory)
                    && Directory.GetFiles(directory).Length == 0
                    && Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory);
                    directory = Path.GetDirectoryName(directory);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
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
        /// Attempts to logs exceptions to the exception logging service.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        static void LogException(Exception ex)
        {
            IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldFileReceiver);
        }

        #endregion Methods

        #region File Watcher Event Handlers

        /// <summary>
        /// Handles the file created event for the processed file watcher.
        /// </summary>
        /// <param name="source">The object which triggered the event.</param>
        /// <param name="e">The file data associated with the event.</param>
        void HandleFileCreated(object source, FileSystemEventArgs e)
        {
            HandleProcessedFiles(new string[] { e.FullPath });
        }

        /// <summary>
        /// Handles the file created event for the failed file watcher.
        /// </summary>
        /// <param name="source">The object which triggered the event.</param>
        /// <param name="e">The file data associated with the event.</param>
        void HandleFileFailed(object source, FileSystemEventArgs e)
        {
            HandleFailedFiles(new string[] { e.FullPath });
        }

        #endregion File Watcher Event Handlers
    }
}
