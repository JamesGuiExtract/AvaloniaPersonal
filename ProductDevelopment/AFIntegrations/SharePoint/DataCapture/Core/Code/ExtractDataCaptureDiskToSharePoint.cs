using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

// Using statements to make dealing with folder settings more readable
using FolderSettingsCollection =
System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>>;
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>;

namespace Extract.SharePoint.DataCapture
{
    /// <summary>
    /// SharePoint timer job that sweeps the local working folder for files that have
    /// finished/failed processing and then pushes the extracted data back into SharePoint
    /// </summary>
    public class ExtractDataCaptureDiskToSharePoint : SPJobDefinition
    {
        #region Constants

        /// <summary>
        /// The title for this timer
        /// </summary>
        const string _TITLE = "Extract Systems Data Capture Disk to SharePoint";

        /// <summary>
        /// The description for this timer
        /// </summary>
        const string _DESCRIPTION = "This timer object will periodically search the configured local "
            + "working folder for processed files and will then push the "
            + "extracted data back into the SharePoint site.";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The local working folder for the Data Capture feature
        /// </summary>
        string _localWorkingFolder;

        /// <summary>
        /// The collection of folder settings keyed by the site id.
        /// </summary>
        FolderSettingsCollection _folderSettings;

        /// <summary>
        /// Collection of sites which have the Data Capture feature activated.
        /// </summary>
        HashSet<Guid> _activeSites;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="ExtractDataCaptureDiskToSharePoint"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractDataCaptureDiskToSharePoint"/> class.
        /// </summary>
        public ExtractDataCaptureDiskToSharePoint()
            : base()
        {
            base.Title = _TITLE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractDataCaptureDiskToSharePoint"/> class.
        /// </summary>
        /// <param name="jobName">The name for this job.</param>
        /// <param name="service">The SharePoint service this job is associated with.</param>
        /// <param name="server">The SharePoint server this job is associated with.</param>
        public ExtractDataCaptureDiskToSharePoint(string jobName, SPService service, SPServer server)
            : base(jobName, service, server, SPJobLockType.Job)
        {
            base.Title = _TITLE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractDataCaptureDiskToSharePoint"/> class.
        /// </summary>
        /// <param name="jobName">The name for this job.</param>
        /// <param name="webApplication">The SharePoint web application this job is
        /// associated with.</param>
        public ExtractDataCaptureDiskToSharePoint(string jobName, SPWebApplication webApplication)
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
                base.Execute(targetInstanceId);

                UpdateSettings();
                var sites = WebApplication.Sites;
                foreach (SPSite site in sites)
                {
                    var siteId = site.ID;
                    if (_activeSites.Contains(siteId))
                    {
                        SearchAndHandleExistingFiles(site);
                    }
                }
            }
            catch (Exception ex)
            {
                DataCaptureHelper.LogException(ex,
                    ErrorCategoryId.DataCaptureDiskToSharePoint, "ELI31496");
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

            // Handle processed files
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
        static void HandleFailedFiles(string[] fileNames, string workingFolder, SPSite site)
        {
            var failedString = ExtractProcessingStatus.ProcessingFailed.AsString();
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

                    var pair = ExtractSharePointHelper.BuildListAndFileGuid(fileName);
                    var list = site.RootWeb.Lists[pair.Key];
                    var item = list.GetItemByUniqueId(pair.Value);

                    string spFileName = site.Url + "/" + item.Url;

                    string fileWithoutExtension =
                        Path.GetFileNameWithoutExtension(fileName);
                    filesToClean[directory].Add(fileWithoutExtension);

                    // Log a file failed exception to the exception service
                    var exception = new ProcessingFailedException("ELI31497", "Failed processing file.");
                    exception.Data.Add("Exported File Name", fileWithoutExtension);
                    exception.Data.Add("SP Failed File", spFileName);
                    DataCaptureHelper.LogException(exception, ErrorCategoryId.DataCaptureDiskToSharePoint,
                        "ELI31498");

                    // Update the data capture status column (do this last so that the above exception is still logged
                    // even if updating the column fails).
                    item[DataCaptureHelper.ExtractDataCaptureStatusColumn] = failedString;
                    item.Update();
                }
                catch (Exception ex)
                {
                    DataCaptureHelper.LogException(ex, ErrorCategoryId.DataCaptureDiskToSharePoint,
                        "ELI31499");
                }
            }

            CleanupLocalFiles(filesToClean, workingFolder);
        }

        /// <summary>
        /// Handles a collection of files that have been queued to verify.
        /// </summary>
        /// <param name="fileNames">The queued files to handle.</param>
        /// <param name="site">The site to handle queued files for.</param>
        static void HandleQueuedVerifiedFiles(string[] fileNames, SPSite site)
        {
            var queuedString = ExtractProcessingStatus.QueuedForVerification.AsString();
            foreach (string fileName in fileNames)
            {
                try
                {
                    var pair = ExtractSharePointHelper.BuildListAndFileGuid(fileName);
                    var list = site.RootWeb.Lists[pair.Key];
                    var item = list.GetItemByUniqueId(pair.Value);

                    // Update the data capture status column
                    item[DataCaptureHelper.ExtractDataCaptureStatusColumn] = queuedString;
                    item.Update();
                }
                catch (Exception ex)
                {
                    DataCaptureHelper.LogException(ex, ErrorCategoryId.DataCaptureDiskToSharePoint,
                        "ELI31523");
                }
                finally
                {
                    try
                    {
                        // Delete the file (log exception if unable to delete)
                        File.Delete(fileName);
                    }
                    catch (Exception ex2)
                    {
                        DataCaptureHelper.LogException(ex2, ErrorCategoryId.DataCaptureDiskToSharePoint,
                            "ELI31524");
                    }
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
            try
            {
                // Get the folder settings
                SiteFolderSettingsCollection folderSettings;
                if (!_folderSettings.TryGetValue(siteId, out folderSettings))
                {
                    return;
                }

                // Collection of list ids to a dictionary of fields for that list
                var listFields = new Dictionary<Guid, Dictionary<string, string>>();
                var listDuplicateFields = new Dictionary<Guid, HashSet<string>>();

                // Build collection of the files to add and the folders/files to clean after adding
                Dictionary<string, List<string>> filesToClean = new Dictionary<string, List<string>>();
                using (SPSite site = new SPSite(siteId))
                {
                    SPWeb web = site.RootWeb;
                    foreach (string fileName in fileNames)
                    {
                        string sharePointFileName = null;
                        string sharePointListName = null;
                        SPListItem item = null;
                        SPList list = null;
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
                            var listIdFileId = ExtractSharePointHelper.BuildListAndFileGuid(fileName);

                            // Build path to xml file
                            string xmlData = Path.Combine(directory, fileWithoutExtension)
                                + ".xml";
                            if (!File.Exists(xmlData))
                            {
                                var xmlFiles = Directory.GetFiles(directory,
                                    Path.GetFileNameWithoutExtension(fileWithoutExtension)
                                    + ".*.xml");
                                if (xmlFiles.Length == 1)
                                {
                                    xmlData = xmlFiles[0];
                                }
                            }

                            try
                            {
                                list = web.Lists[listIdFileId.Key];
                                item = list.GetItemByUniqueId(listIdFileId.Value);
                                sharePointListName = list.Title;
                                sharePointFileName = item.File.Url;
                            }
                            catch (Exception)
                            {
                                // Do not clean up this set of files
                                filesToClean[directory].Remove(fileWithoutExtension);
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
                                    ee.Data.Add("Eli Code", "ELI31500");
                                }
                                ee.Data.Add("Source Document ID", listIdFileId.Value);

                                throw ee;
                            }

                            // Ensure the xml file exists
                            if (File.Exists(xmlData))
                            {
                                // Get the collection of fields
                                Dictionary<string, string> fields = null;
                                if (!listFields.TryGetValue(listIdFileId.Key, out fields))
                                {
                                    // Get the collection of fields for the list
                                    var tempFields = list.Fields;
                                    fields = new Dictionary<string, string>(tempFields.Count, StringComparer.OrdinalIgnoreCase);
                                    foreach (SPField field in tempFields)
                                    {
                                        var temp = field.Title.Replace(" ", "");
                                        if (!fields.TryAdd(temp, field.InternalName))
                                        {
                                            HashSet<string> dups = null;
                                            if (!listDuplicateFields.TryGetValue(listIdFileId.Key, out dups))
                                            {
                                                dups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                                listDuplicateFields[listIdFileId.Key] = dups;
                                            }
                                            dups.Add(temp);
                                        }
                                    }

                                    listFields[listIdFileId.Key] = fields;
                                }
                                HashSet<string> duplicates = null;
                                listDuplicateFields.TryGetValue(listIdFileId.Key, out duplicates);

                                // Parse the XML file looking for attributes to update
                                var attributeList = ParseXmlData(xmlData);
                                foreach (var pair in attributeList)
                                {
                                    string internalColumn = null;
                                    if (fields.TryGetValue(pair.Key, out internalColumn))
                                    {
                                        if (duplicates == null || !duplicates.Contains(pair.Key))
                                        {
                                            item[internalColumn] = EvaluateAttributeValue(attributeList, pair.Value); ;
                                        }
                                        else
                                        {
                                            var ee = new FieldAccessException("Multiple matching fields for attribute.");
                                            ee.Data["Attribute Name"] = pair.Key;
                                            throw ee;
                                        }
                                    }
                                }
                            }

                            item[DataCaptureHelper.ExtractDataCaptureStatusColumn] =
                                ExtractProcessingStatus.ProcessingComplete.AsString();
                            item.Update();
                        }
                        catch (Exception ex)
                        {
                            if (item != null)
                            {
                                try
                                {
                                    item = list.GetItemByUniqueId(item.UniqueId);

                                    // Attempt to set status to failed if anything went wrong
                                    item[DataCaptureHelper.ExtractDataCaptureStatusColumn] =
                                        ExtractProcessingStatus.ProcessingFailed.AsString();
                                    item.Update();
                                }
                                catch
                                {
                                }
                            }

                            // Add the destination list and file name
                            ex.Data["Share Point List"] = sharePointListName ?? "<Unknown>";
                            ex.Data["Share Point File"] = sharePointFileName ?? "<Unknown>";

                            DataCaptureHelper.LogException(ex,
                                ErrorCategoryId.DataCaptureDiskToSharePoint, "ELI31501");
                        }
                    }
                }

                CleanupLocalFiles(filesToClean, workingFolder);
            }
            catch (Exception ex2)
            {
                DataCaptureHelper.LogException(ex2, ErrorCategoryId.DataCaptureDiskToSharePoint,
                    "ELI31502");
            }
        }

        /// <summary>
        /// Parses the XML data.
        /// </summary>
        /// <param name="xmlDataFile">The XML data file.</param>
        /// <returns>A collection of attribute nodes to values from the xml file. The collection returned
        /// can contain duplicates</returns>
        static List<KeyValuePair<string, string>> ParseXmlData(string xmlDataFile)
        {
            var document = new XmlDocument();
            document.Load(xmlDataFile);

            var root = document.DocumentElement;
            if (!root.Name.Equals("FlexData", StringComparison.OrdinalIgnoreCase))
            {
                throw new XmlException("Invalid Extract Data Capture Xml. Missing root element.");
            }
            var nodeList = root.SelectNodes("/FlexData/*[FullText!='']");
            var nodeData = new List<KeyValuePair<string, string>>();
            foreach (XmlNode node in nodeList)
            {
                var name = node.Name;
                var textNode = node.SelectSingleNode("./FullText");
                var pair = new KeyValuePair<string, string>(name, textNode.InnerText);
                nodeData.Add(pair);
            }

            return nodeData;
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
                    DataCaptureHelper.LogException(ex, ErrorCategoryId.DataCaptureDiskToSharePoint,
                        "ELI31503");
                }
            }
        }

        /// <summary>
        /// Checks the feature and ensures the settings are updated.
        /// </summary>
        void UpdateSettings()
        {
            var settings = DataCaptureSettings.GetDataCaptureSettings(false);
            if (settings == null)
            {
                _localWorkingFolder = string.Empty;
                _folderSettings = null;
                return;
            }

            _folderSettings = new FolderSettingsCollection();
            foreach (var siteId in settings.DataCaptureSites)
            {
                var folderSettings = DataCaptureHelper.GetDataCaptureFolderSettings(siteId);
                _folderSettings.Add(siteId, folderSettings);
            }

            _localWorkingFolder = settings.LocalWorkingFolder;
            _activeSites = new HashSet<Guid>(settings.ActiveSites);
        }

        /// <summary>
        /// Evaluates the value as either a literal string or a __COUNT__ function
        /// </summary>
        /// <param name="attributes">List of attributes</param>
        /// <param name="value">Either a literal string or a function __COUNT__(arg1,arg2,arg3...)</param>
        /// <returns>value if __COUNT__ is not present, or count of the attribute names present in attributes list
        /// that are in the argument list of the __COUNT__ function </returns>
        string EvaluateAttributeValue(List<KeyValuePair<string,string>> attributes, string value )
        {
            // Check for the __COUNT__ function in the value
            if (!value.Contains("__COUNT__"))
            {
                return value;
            }

            // Get a collection of the arguments
            MatchCollection arguments = Regex.Matches(value, @"(?<=\(\s*|,\s*)\w+");

            // Get a list of uppercase arguments
            var argList = arguments.OfType<Match>().Select(m => m.Value.ToUpper());
            
            // Count the attributes with the name from the parsed argument list
            int count = attributes.Count(k => argList.Contains(k.Key.ToUpper()));

            // Return the count a string
            return count.ToString();
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