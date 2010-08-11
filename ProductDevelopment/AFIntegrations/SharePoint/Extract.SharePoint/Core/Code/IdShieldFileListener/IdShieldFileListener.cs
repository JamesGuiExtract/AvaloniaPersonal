using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
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
        Dictionary<string, FolderProcessingSettings> _folderSettings =
            new Dictionary<string, FolderProcessingSettings>();

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
        /// Mutex used to serialize access to the UpdateSettings calls.
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldFileListener"/> class.
        /// </summary>
        public IdShieldFileListener() : base()
        {
            try
            {
                // Store the URL
                _url = SPContext.Current.Web.Url;

                // Launch the folder watcher thread
                Thread folderWatcher = new Thread(FolderWatcherThread);
                folderWatcher.Start();
            }
            catch (Exception ex)
            {
                ExtractSharePointLoggingService.LogError(ErrorCategoyId.IdShieldFileReceiver,
                    ex);
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
                ExtractSharePointLoggingService.LogError(ErrorCategoyId.IdShieldFileReceiver, ex);
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
                ExtractSharePointLoggingService.LogError(ErrorCategoyId.IdShieldFileReceiver, ex);
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
            UpdateSettings(GetIdShieldFeature(properties.Web));

            // Check for an output folder (if none is configured then do nothing)
            if (!string.IsNullOrEmpty(_outputFolder))
            {
                // Get the folder name for the item
                string folder = item.File.Url;
                folder = folder.Replace("/" + item.File.Name, "");

                // Attempt to get the settings for the folder
                FolderProcessingSettings settings = null;
                if (_folderSettings.TryGetValue("/" + folder, out settings))
                {
                    // Check if the event and file match the settings
                    if ((settings.EventTypes & eventType) != 0
                        && settings.DoesFileMatchPattern(item.File.Name))
                    {
                        // Ensure the processing folder exists
                        folder = folder.Replace('/', '\\');
                        string outputFolder = Path.Combine(_outputFolder, folder);
                        if (!Directory.Exists(outputFolder))
                        {
                            Directory.CreateDirectory(outputFolder);
                        }

                        // Write the file to the processing folder
                        string fileName = Path.Combine(outputFolder, item.File.Name);
                        byte[] bytes = item.File.OpenBinary(SPOpenBinaryOptions.SkipVirusScan);
                        File.WriteAllBytes(fileName, bytes);
                    }
                }
            }
        }

        /// <summary>
        /// Checks the feature and ensures the settings are updated.
        /// </summary>
        /// <param name="feature">The feature to get the settings from.</param>
        void UpdateSettings(SPFeature feature)
        {
            lock (_lock)
            {
                if (feature != null)
                {
                    // Get the folder settings from the feature
                    SPFeatureProperty property =
                        feature.Properties[ExtractSharePointHelper._FOLDERS_TO_PROCESS];
                    if (property != null)
                    {
                        string temp = property.Value;
                        if (temp.Length != _folderSettingsSerializationString.Length
                            || !temp.Equals(_folderSettingsSerializationString, StringComparison.Ordinal))
                        {
                            _folderSettings =
                                FolderProcessingSettings.DeserializeFolderSettings(temp);
                            _folderSettingsSerializationString = temp;
                        }
                    }

                    // Get the processing folder setting
                    property =
                        feature.Properties[ExtractSharePointHelper._ID_SHIELD_LOCAL_FOLDER];
                    if (property != null)
                    {
                        _outputFolder = property.Value;
                    }
                }
                else
                {
                    _outputFolder = string.Empty;
                    _folderSettings = null;
                    _folderSettingsSerializationString = string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the ID Shield feature from the specified SharePoint web.
        /// </summary>
        /// <param name="web">The web to search for the feature.</param>
        /// <returns>The ID Shield feature (or <see langword="null"/> if it is
        /// not installed.</returns>
        SPFeature GetIdShieldFeature(SPWeb web)
        {
            return web.Features[ExtractSharePointHelper._IDSHIELD_FEATURE_GUID];
        }

        /// <summary>
        /// Gets the name of the destination file based on the current settings.
        /// </summary>
        /// <param name="fullPath">The full path to the .processed file.</param>
        /// <param name="folderSettings">The settings collection to use to
        /// build the destination file name.</param>
        /// <returns>The destination for the file within the SP document library.</returns>
        static string GetDestinationFileName(string fullPath,
            Dictionary<string, FolderProcessingSettings> folderSettings)
        {
            string destination = string.Empty;
            string watchPath = string.Empty;
            string path = fullPath;
            string fileName = Path.GetFileNameWithoutExtension(fullPath);
            string folder = Path.GetDirectoryName(path);
            string folderUpOne = Path.GetDirectoryName(folder);
            string topFolder = folder.Replace(folderUpOne, "");
            folder = folder.Replace(watchPath, "/").Replace("/", "\\");
            folderUpOne = folderUpOne.Replace(watchPath, "/").Replace("/", "\\");

            // Attempt to get the folder settings
            FolderProcessingSettings settings = null;
            if (folderSettings.TryGetValue(folder, out settings))
            {
                // Compute the destination setting
                switch (settings.OutputLocation)
                {
                    case IdShieldOutputLocation.ParallelFolderPrefix:
                        destination = folderUpOne + "/" + settings.OutputLocationString
                            + "_" + topFolder + "/" + fileName;
                        break;

                    case IdShieldOutputLocation.ParallelFolderSuffix:
                        destination = folderUpOne + "/" + topFolder + "_"
                            + settings.OutputLocationString + "/" + fileName;
                        break;

                    case IdShieldOutputLocation.SubFolder:
                        destination = folderUpOne + "/" + settings.OutputLocationString
                            + "/" + fileName;
                        break;

                    case IdShieldOutputLocation.PrefixFilename:
                    case IdShieldOutputLocation.SuffixFilename:
                        destination = folder + "/";
                        if (settings.OutputLocation == IdShieldOutputLocation.PrefixFilename)
                        {
                            destination += settings.OutputLocationString + "_"
                                + fileName;
                        }
                        else
                        {
                            destination += fileName + "_" + settings.OutputLocationString;
                        }
                        break;

                    case IdShieldOutputLocation.CustomOutputLocation:
                        break;
                }
            }

            return destination;
        }

        /// <summary>
        /// Thread method launched to perform folder watching for processed files.
        /// </summary>
        void FolderWatcherThread()
        {
            using (LockFileManager lockFile = new LockFileManager())
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                SPFeature feature = null;
                while (string.IsNullOrEmpty(_outputFolder))
                {
                    // Sleep to give the web a chance to populate the activated feature
                    Thread.Sleep(1000);

                    using (SPSite site = new SPSite(_url))
                    using (SPWeb web = site.OpenWeb())
                    {
                        feature = GetIdShieldFeature(web);
                        if (feature == null)
                        {
                            // If the feature is null it is not activated
                            // just return to exit thread
                            return;
                        }

                        // Update the settings
                        UpdateSettings(feature);
                    }
                }

                // Attempt to place a lock file (singleton thread helper)
                if (!lockFile.TryCreateLockFile(Path.Combine(_outputFolder, _LOCK_FILE_NAME)))
                {
                    // Lock file exists, just return to exit thread
                    return;
                }

                // Search for any existing .processed files
                SearchAndHandleProcessedFiles();

                // Watch for new files
                watcher.Path = _outputFolder;
                watcher.Filter = "*.processed";
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.Created += HandleFileCreated;
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;

                do
                {
                    // Wait for the feature to deactivate
                    Thread.Sleep(5000);
                    using (SPSite site = new SPSite(_url))
                    using (SPWeb web = site.OpenWeb())
                    {
                        feature = GetIdShieldFeature(web);
                    }
                }
                while (feature != null);

                // Feature deactivated, stop watching for processed files
                watcher.EnableRaisingEvents = false;
            }
        }

        /// <summary>
        /// Searches for existing processed files and calls the file handler on them.
        /// </summary>
        void SearchAndHandleProcessedFiles()
        {
            string[] fileNames = Directory.GetFiles(_outputFolder,
                "*.processed", SearchOption.AllDirectories);

            HandleProcessedFiles(fileNames);
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
                    UpdateSettings(GetIdShieldFeature(web));
                    if (_folderSettings != null)
                    {
                        foreach (string fileName in fileNames)
                        {
                            // Build path to redacted file
                            string redactedFile = Path.Combine(Path.GetDirectoryName(fileName),
                                Path.GetFileNameWithoutExtension(fileName)) + ".redacted";
                            string destinationFileName = GetDestinationFileName(fileName,
                                _folderSettings);
                            if (File.Exists(redactedFile)
                                && !string.IsNullOrEmpty(destinationFileName))
                            {
                                string destFolder = destinationFileName.Substring(0,
                                    destinationFileName.LastIndexOf("/"));

                                SPFolder folder = web.GetFolder(destFolder);
                                if (!folder.Exists)
                                {
                                    folder = web.RootFolder;
                                    foreach (string subFolder in destFolder.Split('/'))
                                    {
                                        if (!folder.SubFolders[subFolder].Exists)
                                        {
                                            folder = folder.SubFolders.Add(subFolder);
                                        }
                                        else
                                        {
                                            folder = folder.SubFolders[subFolder];
                                        }
                                    }
                                }

                                byte[] bytes = File.ReadAllBytes(redactedFile);
                                web.Files.Add(destinationFileName, bytes, true);
                                web.Update();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractSharePointLoggingService.LogError(ErrorCategoyId.IdShieldFileReceiver,
                    ex);
            }
        }

        #endregion Methods

        #region File Watcher Event Handlers

        /// <summary>
        /// Handles the file created event.
        /// </summary>
        /// <param name="source">The object which triggered the event.</param>
        /// <param name="e">The file data associated with the event.</param>
        void HandleFileCreated(object source, FileSystemEventArgs e)
        {
            HandleProcessedFiles(new string[] {e.FullPath});
        }

        #endregion File Watcher Event Handlers
    }
}
