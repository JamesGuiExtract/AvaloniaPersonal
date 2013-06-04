using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Extract.SharePoint.DataCapture
{
    /// <summary>
    /// SharePoint timer job that looks through all active sites and exports
    /// files that are in the ToBeQueuedNow and ToBeQueuedLater status
    /// </summary>
    public class ExtractDataCaptureSharePointToDisk : SPJobDefinition
    {
        #region Constants

        /// <summary>
        /// The title for this timer
        /// </summary>
        const string _TITLE = "Extract Systems Data Capture SharePoint to Disk";

        /// <summary>
        /// The description for this timer
        /// </summary>
        const string _DESCRIPTION = "This timer object will search each active site "
            + "searching for files that are in the ToBeQueued state "
            + "and export them to the local working folder.";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The local working folder for the data capture feature
        /// </summary>
        string _localWorkingFolder;

        /// <summary>
        /// Collection of sites which have the data capture feature activated.
        /// </summary>
        HashSet<Guid> _activeSites;

        /// <summary>
        /// The length of string to generate for a random folder name.
        /// </summary>
        int _randomFolderLength = 0;

        /// <summary>
        /// The number of minutes a file must be in ToBeQueuedLater status before being exported.
        /// </summary>
        int _minutesToWait = 0;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractDataCaptureSharePointToDisk"/> class.
        /// </summary>
        public ExtractDataCaptureSharePointToDisk()
            : base()
        {
            base.Title = _TITLE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractDataCaptureSharePointToDisk"/> class.
        /// </summary>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="service">The service.</param>
        /// <param name="server">The server.</param>
        public ExtractDataCaptureSharePointToDisk(string jobName, SPService service, SPServer server)
            : base(jobName, service, server, SPJobLockType.Job)
        {
            base.Title = _TITLE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractDataCaptureSharePointToDisk"/> class.
        /// </summary>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="webApplication">The web application.</param>
        public ExtractDataCaptureSharePointToDisk(string jobName, SPWebApplication webApplication)
            : base(jobName, webApplication, null, SPJobLockType.Job)
        {
            base.Title = _TITLE;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Executes the job definition.
        /// </summary>
        /// <param name="targetInstanceId">For target types of <see cref="T:Microsoft.SharePoint.Administration.SPContentDatabase"/> this is the database ID of the content database being processed by the running job. This value is Guid.Empty for all other target types.</param>
        public override void Execute(Guid targetInstanceId)
        {
            try
            {
                UpdateSettings();
                if (!string.IsNullOrEmpty(_localWorkingFolder))
                {
                    var sites = WebApplication.Sites;
                    foreach (SPSite site in sites)
                    {
                        var siteId = site.ID;
                        if (_activeSites.Contains(siteId))
                        {
                            SearchAndExportFiles(siteId);
                        }
                    }
                }

                base.Execute(targetInstanceId);
            }
            catch (Exception ex)
            {
                DataCaptureHelper.LogException(ex,
                    ErrorCategoryId.IdShieldSharePointToDisk, "ELI31489");
                throw;
            }
        }

        /// <summary>
        /// Checks the feature settings and ensures the settings are updated.
        /// </summary>
        void UpdateSettings()
        {
            var settings = DataCaptureSettings.GetDataCaptureSettings(false);
            if (settings == null)
            {
                _localWorkingFolder = string.Empty;
                _activeSites = new HashSet<Guid>();
                _minutesToWait = 0;
                _randomFolderLength = 0;
                return;
            }

            _localWorkingFolder = settings.LocalWorkingFolder;
            _activeSites = new HashSet<Guid>(settings.ActiveSites);
            _minutesToWait = -1 * settings.MinutesToWaitToQueuedLater;
            _randomFolderLength = settings.RandomFolderNameLength;
        }

        /// <summary>
        /// Searches for and exports files that need to be processed for the specified site.
        /// </summary>
        /// <param name="siteId">The ID for the site to export files from.</param>
        void SearchAndExportFiles(Guid siteId)
        {
            // Build the path to the working folder
            var workingFolder = ExtractSharePointHelper.BuildLocalWorkingFolderPath(
                        siteId, _localWorkingFolder);

            // Create the query to get items ready to be queued
            var query = ExtractSharePointHelper.BuildFileExportCamlQuery(
                DataCaptureHelper.ExtractDataCaptureStatusColumn, _minutesToWait);

            using (var site = new SPSite(siteId))
            {
                // Loop through all document library collections
                var web = site.RootWeb;
                var listCollection = web.GetListsOfType(SPBaseType.DocumentLibrary);
                foreach (SPList list in listCollection)
                {
                    // Check if the library has the IDS status column
                    var field = list.Fields.TryGetFieldByStaticName(
                        DataCaptureHelper.ExtractDataCaptureStatusColumn);
                    if (field != null)
                    {
                        // Build the output folder path
                        var outputFolder = Path.Combine(workingFolder, list.ID.ToString());
                        var items = list.GetItems(query);
                        string queued = ExtractProcessingStatus.QueuedForProcessing.AsString();
                        for (int i = items.Count - 1; i >= 0; --i)
                        {
                            SPListItem item = items[i];
                            if (item.FileSystemObjectType == SPFileSystemObjectType.File)
                            {
                                var exportFolder = _randomFolderLength > 0 ? Path.Combine(outputFolder,
                                    ExtractSharePointHelper.BuildRandomAlphaNumericString(_randomFolderLength))
                                    : outputFolder;

                                // Create the directory if it does not exist
                                if (!Directory.Exists(exportFolder))
                                {
                                    Directory.CreateDirectory(exportFolder);
                                }

                                // Write the file to the processing folder
                                var file = item.File;
                                byte[] bytes = file.OpenBinary(SPOpenBinaryOptions.SkipVirusScan);
                                string outFileName = Path.Combine(exportFolder,
                                    file.UniqueId.ToString() + Path.GetExtension(file.Name));
                                File.WriteAllBytes(outFileName, bytes);

                                ExtractSharePointHelper.DoWithCheckoutIfRequired("ELI35898", file, "Updated Data Capture status.", () =>
                                {
                                    // Mark the item as queued
                                    item[DataCaptureHelper.ExtractDataCaptureStatusColumn] = queued;
                                    item.Update();
                                });
                            }
                        }
                    }
                }
            }
        }

        #endregion Methods
    }
}
