using Extract.ExceptionService;
using Extract.Sharepoint;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.ServiceModel;
using System.Text;

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
        /// File has been queued for verification
        /// </summary>
        QueuedForVerification = 4,

        /// <summary>
        /// Processing completed with redacted image
        /// </summary>
        Redacted = 5,

        /// <summary>
        /// Processing Completed without redacted image
        /// </summary>
        NoRedactions = 6,

        /// <summary>
        /// Processing completed - this is used by DataCapture
        /// </summary>
        ProcessingComplete = 7
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

                case ExtractProcessingStatus.QueuedForVerification:
                    return "Queued For Verification";

                case ExtractProcessingStatus.Redacted:
                    return "Redacted";

                case ExtractProcessingStatus.NoRedactions:
                    return "No Redactions";

                case ExtractProcessingStatus.ProcessingComplete:
                    return "Processing Complete";

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
                    query.Query = new camlFieldRef(fieldName).IsNull().Where();
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
            camlFieldRef column = new camlFieldRef(columnName);
            camlValue status = new camlValue("CHOICE", ExtractProcessingStatus.ToBeQueued.AsString());
            camlFieldRef modified = new camlFieldRef("Modified");
            camlValue valueModified = new camlValue("DateTime",SPUtility.CreateISO8601DateTimeFromSystemDateTime(
                DateTime.Now.AddMinutes(minutesToWait)), true);
            
            query.Query = column.Eq(status).And(modified.Leq(valueModified)).Where();

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
            camlFieldRef folderIdField = new camlFieldRef(folderIdColumn);
            camlValue id = new camlValue("Guid", folderId.ToString());
            query.Query = folderIdField.Eq(id).Where();
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
        /// <returns>A partial Caml query that can be included in a Caml Where or be joined with other items </returns>
        static string BuildQueryForProcessingExistingItems(string extractStatusColumn,
            bool reprocessExisting, string columnToCheckForEmpty)
        {
            var statusList = new List<ExtractProcessingStatus>();
            statusList.Add(ExtractProcessingStatus.NotProcessed);
            if (reprocessExisting)
            {
                statusList.AddRange(new ExtractProcessingStatus[] {
                    ExtractProcessingStatus.Redacted,
                    ExtractProcessingStatus.NoRedactions,
                    ExtractProcessingStatus.ProcessingComplete,
                    ExtractProcessingStatus.ProcessingFailed,
                    ExtractProcessingStatus.QueuedForProcessing,
                    ExtractProcessingStatus.QueuedForVerification
                });
            }

            bool checkForEmpty = !string.IsNullOrEmpty(columnToCheckForEmpty);

            camlFieldRef statusColumn = new camlFieldRef(extractStatusColumn);
            camlValues statusValues = new camlValues();

            // Add status values to look for to values list
            foreach (var value in statusList)
            {
                statusValues.Add(new camlValue("Choice", value.AsString()));
            }

            // if checking for empty column add the Caml needed to check
            if (checkForEmpty)
            {
                camlFieldRef columnToCheck = new camlFieldRef(columnToCheckForEmpty);
                return statusColumn.In(statusValues).And(columnToCheck.IsNull());
            }

            return statusColumn.In(statusValues);
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
                string CamlQuery = BuildQueryForProcessingExistingItems(extractStatusColumn,
                    folderSettings.Reprocess, additionalEmptyColumnToCheck);

                // Need to add the field check if being used
                if ( folderSettings.QueueWithFieldValue)
                {
                    CamlQuery = CamlQuery.And(FieldEqualCaml(site.RootWeb, folderSettings));
                }

                SPListItemCollection items = getFilesByQuery(site, folderSettings,
                    CamlQuery.Where());
                var toBeQueued = ExtractProcessingStatus.ToBeQueued.AsString();
                for (int i = items.Count - 1; i >= 0; --i)
                {
                    SPListItem item = items[i];

                    if (folderSettings.DoesFileMatchPattern(item.File.Name) && 
                        (!folderSettings.QueueWithFieldValue) || IsFieldEqual(item, folderSettings))
                    {
                        item[extractStatusColumn] = toBeQueued;
                        item.Update();
                    }
                }
            }
        }

        /// <summary>
        /// Gets files from the site usingthe <see param="folderSettings"/> and <see param="queryString"/>
        /// </summary>
        /// <param name="site">Site to get the files from.</param>
        /// <param name="folderSettings">Folder settings to use to get the files</param>
        /// <param name="queryString">Caml Query to execute</param>
        /// <returns>SPListItemCollection containing the results of the query</returns>
        public static SPListItemCollection getFilesByQuery(SPSite site, FolderProcessingSettings folderSettings, string queryString)
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

            query.Query = queryString;
            return parentList.GetItems(query);
        }

        /// <summary>
        /// Builds the Field equal part for a Caml Query
        /// </summary>
        /// <param name="web">Web site to ues to get the fields</param>
        /// <param name="folderSettings">FolderSettings that contains the folder, field being queued</param>
        /// <returns>Caml formated check for specific field value</returns>
        public static string FieldEqualCaml(SPWeb web, FolderProcessingSettings folderSettings)
        {
            // Get the field to check
            string field = folderSettings.FieldForQueuing;

            // the Settings.ValueToQueueOn my need to be adjusted based on the field type
            //  check box requires 1(true) or 0 (false)
            string value = folderSettings.ValueToQueueOn;

            // Set up the field to check
            camlFieldRef fieldToQuery = new camlFieldRef(field);

            // Get the folder name
            SPFolder folder = web.GetFolder(folderSettings.FolderId);
            string folderName = folder.Name;

            // Set up for the value to check for
            camlValue valueToQuery = new camlValue(GetFieldType(web, folderName, field), value);

            return fieldToQuery.Eq(valueToQuery);
        }
        
        /// <summary>
        /// Checks that the field in the settings.NameOfFieldForQueueWithFieldValue has the value in 
        /// settings.ValueToQueue on
        /// </summary>
        /// <param name="item">List item that for the file to check</param>
        /// <param name="folderSettings">Settings that contain the field and value to check</param>
        /// <returns>True if the file has a field equal to the value.</returns>
        public static bool IsFieldEqual(SPListItem item, FolderProcessingSettings folderSettings)
        {
            // Set up to check only the value for the item
            camlFieldRef idField = new camlFieldRef("ID");
            camlValue fileID = new camlValue("Counter", item.ID.ToString());
            
            // Get the results for the query
            var list = ExtractSharePointHelper.getFilesByQuery(item.Web.Site, folderSettings,
                FieldEqualCaml(item.Web, folderSettings).And(idField.Eq(fileID)).Where());

            // Return true if any results matched the query
            return list.Count > 0;
        }

        /// <summary>
        /// Returns a list of the fieldnames for the document library for the given folder
        /// </summary>
        /// <param name="web">The sharepoint web</param>
        /// <param name="folderName">Folder to use </param>
        /// <param name="fieldTypeList">List of field types to return</param>
        /// <returns>A DataTable with Title, InternalName and SPField record. The table will be orderd by the 
        /// internal name of the fields</returns>
        public static DataTable GetFieldsListForFolder(SPWeb web, string folderName, List<string> fieldTypeList)
        {
            // Sorted list to order by the internal name
            SortedList<string, SPField> sortedFieldList = new SortedList<string, SPField>();

            // Get the fields for the document folder
            var folder = web.GetFolder(string.Concat(web.Url, folderName));
            SPDocumentLibrary library = folder.DocumentLibrary;
            foreach (SPField f in library.Fields)
            {
                // Only include visible fields and fields with the type requested
                if (!f.Hidden && fieldTypeList.Contains(f.TypeAsString))
                {
                    sortedFieldList.Add(f.InternalName, f);
                }
            }
           
            // Create a typed DataTable
            DataTable fieldTable = new DataTable("FieldTable");
            fieldTable.Columns.Add("Title", Type.GetType("System.String"));
            fieldTable.Columns.Add("InternalName", Type.GetType("System.String"));
            fieldTable.Columns.Add("SharepointField", typeof(SPField));

            // Add records to the DataTable from the sorted list
            foreach (var p in sortedFieldList)
            {
                DataRow dr = fieldTable.NewRow();
                dr["Title"] = p.Value.Title;
                dr["InternalName"] = p.Value.InternalName;
                dr["SharepointField"] = p.Value;
                fieldTable.Rows.Add(dr);
            }

            // return the table
            return fieldTable;
        }

        /// <summary>
        /// Gets the type of the field for the given folder and field name
        /// </summary>
        /// <param name="web">The web object used to get the folder</param>
        /// <param name="folderName">The folder that contains the field</param>
        /// <param name="fieldName">The name of the field to get the type of</param>
        /// <returns>The type of the field as a string, if the returned string is empty the field was not found</returns>
        public static string GetFieldType(SPWeb web, string folderName,  string fieldName)
        {
            // Get the fields for the document folder
            var folder = web.GetFolder(string.Concat(web.Url, folderName));
            SPDocumentLibrary library = folder.DocumentLibrary;

            foreach (SPField f in library.Fields)
            {
                if (f.InternalName == fieldName)
                {
                    return f.TypeAsString;
                }
            }
            return "";
        }

        #endregion Methods
    }
}
