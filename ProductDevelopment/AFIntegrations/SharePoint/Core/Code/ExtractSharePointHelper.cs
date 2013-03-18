using Extract.ExceptionService;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using Microsoft.SharePoint.Utilities;

namespace Extract.SharePoint
{
    /// <summary>
    /// Enumeration for Extract processing status values
    /// </summary>
    public enum ExtractProcessingStatus
    {
        /// <summary>
        /// File has not been processed
        /// </summary>
        NotProcessed = 0,

        /// <summary>
        /// File is waiting to be exported
        /// </summary>
        ToBeQueued = 1,

        /// <summary>
        /// File has been exported
        /// </summary>
        QueuedForProcessing = 2,

        /// <summary>
        /// Processing of the file has failed
        /// </summary>
        ProcessingFailed = 3,

        /// <summary>
        /// Processing of the file has completed
        /// </summary>
        ProcessingComplete = 4,

        /// <summary>
        /// File has been queued for verification
        /// </summary>
        QueuedForVerification = 5
    }

    /// <summary>
    /// Class that implements extension methods for the <see cref="ExtractProcessingStatus"/> enumeration.
    /// </summary>
    public static class ExtractStatusExtensions
    {
        /// <summary>
        /// Extension method that returns the <see cref="ExtractProcessingStatus"/> name as a string.
        /// </summary>
        /// <param name="status">The status to return as a string.</param>
        /// <returns>The value of the status as a string.</returns>
        public static string AsString(this ExtractProcessingStatus status)
        {
            switch (status)
            {
                case ExtractProcessingStatus.NotProcessed:
                    return "Not Processed";

                case ExtractProcessingStatus.ToBeQueued:
                    return "To Be Queued";

                case ExtractProcessingStatus.QueuedForProcessing:
                    return "Queued For Processing";

                case ExtractProcessingStatus.ProcessingFailed:
                    return "Processing Failed";

                case ExtractProcessingStatus.ProcessingComplete:
                    return "Processing Complete";

                case ExtractProcessingStatus.QueuedForVerification:
                    return "Queued For Verification";

                default:
                    throw new ArgumentException("Not a valid Extract processing status.", "status");
            }
        }
    }

    /// <summary>
    /// Collection of helper methods for all Extract Systems SharePoint projects
    /// </summary>
    public static class ExtractSharePointHelper
    {
        #region Methods

        /// <summary>
        /// Logs the specified exception to the Extract exception logging service
        /// located at the specified IP address.
        /// </summary>
        /// <param name="ipAddress">The ip address to log to.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="eliCode">The ELI code for this exception.</param>
        public static void LogExceptionTcp(string ipAddress, Exception ex, string eliCode)
        {
            ChannelFactory<IExtractExceptionLogger> factory = null;
            try
            {
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    // Build the url
                    StringBuilder url = new StringBuilder("net.tcp://");
                    url.Append(ipAddress);
                    url.Append("/");
                    url.Append(ExceptionLoggerData.WcfTcpEndPoint);

                    factory = new ChannelFactory<IExtractExceptionLogger>(new NetTcpBinding(),
                        new EndpointAddress(url.ToString()));

                    IExtractExceptionLogger logger = factory.CreateChannel();
                    
                    // TODO: Add the computer name, user name, process ID and software version
                    logger.LogException(new ExceptionLoggerData(ex, eliCode, "", "", DateTime.UtcNow.ToFileTime(), 0, ""));

                    factory.Close();
                }

                // Always log to SharePoint log
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex, eliCode);
            }
            catch (Exception ex2)
            {
                if (factory != null)
                {
                    factory.Abort();
                    factory = null;
                }

                // Unable to use logging service, send the error to the sharepoint log
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex, eliCode);
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.ExceptionLogger, ex2,
                    "ELI30548");
            }
        }

        /// <summary>
        /// Gets the site relative path for the specified folder and the site Id
        /// </summary>
        /// <param name="folderUrl">The server relative url for the folder.</param>
        /// <param name="siteId">The id for the site to compute the relative folder for.</param>
        /// <returns>The site relative path to the specified folder.</returns>
        public static string GetSiteRelativeFolderPath(string folderUrl, Guid siteId)
        {
            string folder = string.Empty;
            using (SPSite site = new SPSite(siteId))
            {
                string siteUrl = site.ServerRelativeUrl;
                int index = folderUrl.IndexOf(siteUrl, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    folder = folderUrl.Substring(index + siteUrl.Length);
                }
                else
                {
                    folder = folderUrl;
                }
            }

            // Ensure the folder starts with a '/'
            if (!folder.StartsWith("/", StringComparison.Ordinal))
            {
                folder = "/" + folder;
            }

            return folder;
        }

        /// <summary>
        /// Gets the folder id.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="folderPath">The current folder.</param>
        /// <returns>The unique Id for the folder.</returns>
        public static Guid GetFolderId(SPWeb web, string folderPath)
        {
            var folder = web.GetFolder(string.Concat(web.Url, folderPath));
            if (!folder.Exists)
            {
                throw new SPException("Cannot find folder.");
            }

            return folder.UniqueId;
        }

        /// <summary>
        /// Gets the folder path.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="folderId">The ID for the folder/list to retrieve the path for.</param>
        /// <returns>Either the path to the folder/list or <see cref="String.Empty"/> if
        /// the item does not exist.</returns>
        public static string GetFolderPath(SPWeb web, Guid folderId)
        {
            string folderPath = string.Empty;
            try
            {
                SPFolder folder = web.GetFolder(folderId);
                if (folder.Exists)
                {
                    folderPath = folder.Url;
                }

                // Its not a folder, try getting it as a list
                if (string.IsNullOrEmpty(folderPath))
                {
                    SPList list = web.Lists[folderId];
                    folderPath = list.RootFolder.Url;
                }
            }
            catch
            {
            }

            if (!string.IsNullOrEmpty(folderPath) && folderPath[0] != '/')
            {
                return folderPath.Insert(0, "/");
            }

            return folderPath;
        }

        /// <summary>
        /// Determines whether the specified web user is member of the specified group.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>
        /// <see langword="true"/> if the specified web user is a member of the specified group;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsMember(SPWeb web, string groupName)
        {
            return web.IsCurrentUserMemberOfGroup(web.Groups[groupName].ID);
        }

        /// <summary>
        /// Builds the local working folder path.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <returns>Path to local working folder for the specified site.</returns>
        public static string BuildLocalWorkingFolderPath(Guid siteId, string workingFolder)
        {
            return Path.Combine(workingFolder, siteId.ToString());
        }

        /// <summary>
        /// Builds the random alpha numeric string.
        /// </summary>
        /// <param name="length">The length of the string to return.</param>
        /// <returns>A random string of specified length.</returns>
        public static string BuildRandomAlphaNumericString(int length)
        {
            const string source = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var sb = new StringBuilder(length);
            var rand = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(source[rand.Next(0, source.Length)]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds the list and file GUID.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>A pair containing the list GUID as key and file GUID as value.</returns>
        public static KeyValuePair<Guid, Guid> BuildListAndFileGuid(string fileName)
        {
            // Get the list name and file ID from the file name
            var fileGuid = new Guid(
                Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fileName)));
            var testDir = Path.GetDirectoryName(fileName);

            // Get the GUID from file name, if the length returned is less than 32
            // (min length for a GUID not containing '-'), then get the file name from
            // one folder up (since the short string is the random folder name)
            var listGuidVal = Path.GetFileName(testDir);
            if (listGuidVal.Length < 32)
            {
                listGuidVal = Path.GetFileName(Path.GetDirectoryName(testDir));
            }
            var listGuid = new Guid(listGuidVal);

            return new KeyValuePair<Guid, Guid>(listGuid, fileGuid);
        }

        /// <summary>
        /// Creates the specified list (if it does not alreay exist).
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="listName">Name of the list.</param>
        /// <param name="hidden">if set to <see langword="true"/> will create a hidden list.</param>
        public static void CreateSpecifiedList(Guid siteId, string listName, bool hidden)
        {
            using (var site = new SPSite(siteId))
            {
                var web = site.RootWeb;

                // Attempt to get the list from the site
                var list = web.Lists.TryGetList(listName);
                if (list == null)
                {
                    web.AllowUnsafeUpdates = true;
                    try
                    {
                        var listId = web.Lists.Add(listName, "", SPListTemplateType.GenericList);
                        web.Update();
                        if (hidden)
                        {
                            list = web.Lists[listId];
                            list.Hidden = true;
                            list.Update();
                        }
                    }
                    finally
                    {
                        web.AllowUnsafeUpdates = false;
                    }
                }
            }
        }


        /// <summary>
        /// Adds the specified status column.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="rootListId">The root list id.</param>.
        /// <param name="fieldName">The name of the field to add.</param>
        /// <param name="fieldDisplayName">The display name for the field to add.</param>
        public static void AddExtractStatusColumn(Guid siteId, Guid rootListId,
            string fieldName, string fieldDisplayName)
        {
            using (var site = new SPSite(siteId))
            {
                // Check if the list contains the IDS_Status column
                var list = site.RootWeb.Lists[rootListId];
                var field = list.Fields.TryGetFieldByStaticName(fieldName);
                if (field == null)
                {
                    var sb = new StringBuilder("<Field Type='Choice' DisplayName='", 400);
                    sb.Append(fieldName);
                    sb.Append("' Name='");
                    sb.Append(fieldName);
                    sb.Append("' Format='Dropdown' Indexed='TRUE' ReadOnly='TRUE'><CHOICES>");
                    foreach (ExtractProcessingStatus status in Enum.GetValues(typeof(ExtractProcessingStatus)))
                    {
                        sb.Append("<CHOICE>");
                        sb.Append(status.AsString());
                        sb.Append("</CHOICE>");
                    }
                    sb.Append("</CHOICES><Default>");
                    sb.Append(ExtractProcessingStatus.NotProcessed.AsString());
                    sb.Append("</Default>");
                    sb.Append("<Description>The current Extract processing status.</Description></Field>");

                    list.Fields.AddFieldAsXml(sb.ToString(), true, SPAddFieldOptions.AddFieldToDefaultView);
                    field = list.Fields[fieldName];
                    field.Title = fieldDisplayName;
                    field.Update();

                    // Get all files that currently do not have an Extract processing status value
                    var query = new SPQuery();
                    query.Query = "<Where><IsNull><FieldRef Name='" + fieldName
                        + "' /></IsNull></Where>";
                    query.ViewFields = "<FieldRef Name=' " + fieldName + "' />";
                    query.ViewAttributes = "Scope='Recursive'";
                    var items = list.GetItems(query);
                    if (items != null && items.Count > 0)
                    {
                        string defaultStatus = ExtractProcessingStatus.NotProcessed.AsString();
                        for (int i = 0; i < items.Count; ++i)
                        {
                            var item = items[i];
                            item[fieldName] = defaultStatus;
                            item.Update();
                        }
                    }

                    list.Update();
                }
            }
        }

        /// <summary>
        /// Gets the specified list.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="listName">Name of the list.</param>
        /// <returns>The specified list or <see langword="null"/> if not found.</returns>
        public static SPList GetSpecifiedList(SPSite site, string listName)
        {
            var list = site.RootWeb.Lists.TryGetList(listName);
            return list;
        }

        /// <summary>
        /// Builds the file export caml query.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="minutesToWait">The minutes to wait.</param>
        /// <returns></returns>
        public static SPQuery BuildFileExportCamlQuery(string columnName, int minutesToWait)
        {
            var query = new SPQuery();
            query.Query = string.Concat("<Where><And><Eq><FieldRef Name='",
                columnName, "'/><Value Type='CHOICE'>",
                ExtractProcessingStatus.ToBeQueued.AsString(),
                "</Value></Eq><Leq><FieldRef Name='Modified'/><Value IncludeTimeValue='TRUE' ",
                "Type='DateTime'>", SPUtility.CreateISO8601DateTimeFromSystemDateTime(
                DateTime.Now.AddMinutes(minutesToWait)), "</Value></Leq></And></Where>");
            query.ViewAttributes = "Scope='Recursive'";

            return query;

        }

        /// <summary>
        /// Builds the query for folder id.
        /// </summary>
        /// <param name="folderIdColumn">The folder ID column name.</param>
        /// <param name="folderId">The folder id.</param>
        /// <returns>The query for selecting the list item containing the specified folder id.</returns>
        public static SPQuery BuildQueryForFolderId(string folderIdColumn, Guid folderId)
        {
            var query = new SPQuery();
            query.Query = string.Concat("<Where><Eq><FieldRef Name='",
                folderIdColumn, "'/><Value Type='Guid'>",
                folderId.ToString(), "</Value></Eq></Where>");
            return query;
        }

        /// <summary>
        /// Builds the query for existing items.
        /// </summary>
        /// <param name="extractStatusColumn">The extract status column to read from.</param>
        /// <param name="reprocessExisting">Whether or not existing items should be reprocessed.</param>
        /// <param name="columnToCheckForEmpty">If not <see langword="null"/> or <see cref="string.Empty"/>
        /// then this column will be added with a check to return only rows in which this columns value has
        /// not been set.</param>
        /// <returns>The CAML query to get existing list items.</returns>
        static string BuildQueryForProcessingExistingItems(string extractStatusColumn,
            bool reprocessExisting, string columnToCheckForEmpty)
        {
            var statusList = new List<ExtractProcessingStatus>();
            statusList.Add(ExtractProcessingStatus.NotProcessed);
            if (reprocessExisting)
            {
                statusList.AddRange(new ExtractProcessingStatus[] {
                    ExtractProcessingStatus.ProcessingComplete,
                    ExtractProcessingStatus.ProcessingFailed,
                    ExtractProcessingStatus.QueuedForProcessing,
                    ExtractProcessingStatus.QueuedForVerification
                });
            }

            bool checkForEmpty = !string.IsNullOrEmpty(columnToCheckForEmpty);
            var sb = new StringBuilder(512);
            sb.Append("<Where>");
            if (checkForEmpty)
            {
                sb.Append("<And>");
            }
            sb.Append("<In>");
            sb.Append("<FieldRef Name='");
            sb.Append(extractStatusColumn);
            sb.Append("' /><Values>");
            foreach (var value in statusList)
            {
                sb.Append("<Value Type='CHOICE'>");
                sb.Append(value.AsString());
                sb.Append("</Value>");
            }
            sb.Append("</Values></In>");
            if (checkForEmpty)
            {
                sb.Append("<IsNull><FieldRef Name='");
                sb.Append(columnToCheckForEmpty);
                sb.Append("' /></IsNull>");
                sb.Append("</And>");
            }
            sb.Append("</Where>");
            return sb.ToString();
        }

        /// <summary>
        /// Marks the files to be queued.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="folderSettings">The folder settings.</param>
        /// <param name="extractStatusColumn">The extract status column to set.</param>
        /// <param name="additionalEmptyColumnToCheck">If not <see langword="null"/> or <see cref="string.Empty"/>
        /// then this column will be added into the query for selecting files to mark as to be queued.
        /// In order to be marked to be queued, this columns data must be null for the particular row.</param>
        public static void MarkFilesToBeQueued(Guid siteId, FolderProcessingSettings folderSettings,
            string extractStatusColumn, string additionalEmptyColumnToCheck)
        {
            using (var site = new SPSite(siteId))
            {
                var parentList = site.RootWeb.Lists[folderSettings.ListId];
                var query = new SPQuery();

                // Set recursive if needed
                if (folderSettings.RecurseSubfolders)
                {
                    query.ViewAttributes = "Scope='Recursive'";
                }

                // Set the starting folder
                if (parentList.RootFolder.UniqueId == folderSettings.FolderId)
                {
                    query.Folder = parentList.RootFolder;
                }
                else
                {
                    query.Folder = parentList.GetItemByUniqueId(folderSettings.FolderId).Folder;
                }

                query.Query = BuildQueryForProcessingExistingItems(extractStatusColumn,
                    folderSettings.Reprocess, additionalEmptyColumnToCheck);
                SPListItemCollection items = parentList.GetItems(query);
                var toBeQueued = ExtractProcessingStatus.ToBeQueued.AsString();
                for (int i = items.Count - 1; i >= 0; --i)
                {
                    SPListItem item = items[i];
                    if (folderSettings.DoesFileMatchPattern(item.File.Name))
                    {
                        item[extractStatusColumn] = toBeQueued;
                        item.Update();
                    }
                }
            }
        }

        #endregion Methods
    }
}
