using Extract.SharePoint.Redaction.Utilities;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using System.Text;

namespace Extract.SharePoint.Redaction
{
    /// <summary>
    /// Static class of helper methods for the ID Shield SharePoint features.
    /// </summary>
    internal static class IdShieldHelper
    {
        #region Constants

        // Constant for IDS group name
        internal static readonly string IdShieldAdministratorsGroupName = "Extract ID Shield Administrators";

        // Constants for the ID Shield settings list fields
        internal static readonly string ListId = "ListId";
        internal static readonly string FolderId = "FolderId";
        internal static readonly string FileExtensionList = "FileExtensionList";
        internal static readonly string Recursive = "Recursive";
        internal static readonly string ReprocessProcessed = "ReprocessProcessed";
        internal static readonly string AddedFiles = "AddedFiles";
        internal static readonly string ModifiedFiles = "ModifiedFiles";
        internal static readonly string ProcessExistingFiles = "ProcessExistingFiles";
        internal static readonly string OutputLocationType = "OutputLocationType";
        internal static readonly string OutputLocationString = "OutputLocationString";
        internal static readonly string QueueWithFieldValue = "QueueWithFieldValue";
        internal static readonly string FieldForQueuing = "FieldForQueuing";
        internal static readonly string ValueToQueueOn = "ValueToQueueOn";

        // Constants for the ID Shield status field
        internal static readonly string IdShieldStatusColumn = "IDShieldStatus";
        internal static readonly string IdShieldStatusColumnDisplay = "IDS Status";

        // Constants for the ID Shield Reference column that replaces the redacted and unredacted columns
        internal static readonly string IdShieldReferenceColumn = "IDSReference";
        internal static readonly string IdShieldReferenceColumnDisplay = "IDS Reference";

        /// <summary>
        /// Name for the hidden list that is created to handle tracking files to ignore in
        /// the add event receiver
        /// </summary>
        internal static readonly string _HIDDEN_IGNORE_FILE_LIST = "C888293C-0DC0-4F3E-8B3F-4151929E5CE0";

        /// <summary>
        /// The value to be replaced in the hidden site folder settings list string so that each site
        /// will have its own hidden site folder settings list.
        /// </summary>
        internal static readonly string _HIDDEN_REPLACE_VALUE = "<REPLACE_ME>";

        /// <summary>
        /// The name of the hidden list containing the folder settings for the site.
        /// </summary>
        internal static readonly string _HIDDEN_SITE_FOLDER_SETTINGS_NAME =
            "IdShieldSettings-" + _HIDDEN_REPLACE_VALUE;

        #endregion Constants

        /// <summary>
        /// Builds the name of the hidden list containing the folder settings for the specified site.
        /// </summary>
        /// <param name="siteId">The id of the site to get the list name for.</param>
        /// <returns>The name of the hidden list containing the site settings.</returns>
        static string GetFolderSettingsListName(Guid siteId)
        {
            return _HIDDEN_SITE_FOLDER_SETTINGS_NAME.Replace(_HIDDEN_REPLACE_VALUE, siteId.ToString());
        }

        /// <summary>
        /// Creates (if it does not already exist) the hidden list to hold folder settings for the specified
        /// site.
        /// </summary>
        /// <param name="siteId">The unique ID for the site to create the settings list for.</param>
        internal static void CreateFolderSettingsList(Guid siteId)
        {
            using (var site = new SPSite(siteId))
            {
                var web = site.RootWeb;
                string listName = GetFolderSettingsListName(siteId);

                // Attempt to get the list from the site
                var list = web.Lists.TryGetList(listName);
                if (list == null)
                {
                    web.AllowUnsafeUpdates = true;
                    try
                    {
                        var listId = web.Lists.Add(listName, "", SPListTemplateType.GenericList);
                        web.Update();

                        // Get the newly created list and add columns
                        list = web.Lists[listId];
                        list.Fields.Add(ListId, SPFieldType.Guid, false);
                        list.Fields.Add(FolderId, SPFieldType.Guid, true);
                        list.Fields.Add(FileExtensionList, SPFieldType.Text, false);
                        list.Fields.Add(ReprocessProcessed, SPFieldType.Boolean, false);
                        list.Fields.Add(Recursive, SPFieldType.Boolean, false);
                        list.Fields.Add(AddedFiles, SPFieldType.Boolean, false);
                        list.Fields.Add(ModifiedFiles, SPFieldType.Boolean, false);
                        list.Fields.Add(ProcessExistingFiles, SPFieldType.Boolean, false);
                        list.Fields.Add(OutputLocationType, SPFieldType.Integer, false);
                        list.Fields.Add(OutputLocationString, SPFieldType.Text, false);
                        list.Fields.Add(QueueWithFieldValue, SPFieldType.Boolean, false);
                        list.Fields.Add(FieldForQueuing, SPFieldType.Text, false);
                        list.Fields.Add(ValueToQueueOn, SPFieldType.Text, false);

                        list.Hidden = true;
                        list.Update();
                    }
                    finally
                    {
                        web.AllowUnsafeUpdates = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the folder settings list for the specified site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The specified list or <see langword="null"/> if not found.</returns>
        internal static SPList GetFolderSettingsList(SPSite site)
        {
            return ExtractSharePointHelper.GetSpecifiedList(site,
                GetFolderSettingsListName(site.ID));
        }

        /// <summary>
        /// Removes the watching for folder.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="folderId">The folder id.</param>
        internal static bool RemoveWatchingForFolder(Guid siteId, Guid folderId)
        {
            bool watchRemoved = false;
            using (var site = new SPSite(siteId))
            {
                var list = GetFolderSettingsList(site);
                if (list != null)
                {
                    list.ParentWeb.AllowUnsafeUpdates = true;
                    try
                    {
                        var query = ExtractSharePointHelper.BuildQueryForFolderId(FolderId, folderId);
                        var items = list.GetItems(query);
                        if (items.Count > 0)
                        {
                            for (int i = items.Count - 1; i >= 0; i--)
                            {
                                items[i].Delete();
                            }

                            watchRemoved = true;
                        }

                        list.Update();
                    }
                    finally
                    {
                        list.ParentWeb.AllowUnsafeUpdates = false;
                    }
                }
            }

            return watchRemoved;
        }

        /// <summary>
        /// Adds the specified <see cref="FolderProcessingSettings"/> to the hidden list of folder
        /// settings for the specified site, or updates the existing settings based on the
        /// provided settings object.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="folderSettings">The folder settings.</param>
        internal static void AddOrUpdateFolderSettings(Guid siteId,
            IdShieldFolderProcessingSettings folderSettings)
        {
            AddIdShieldStatusColumn(siteId, folderSettings.ListId);
            AddIdShieldReferenceColumn(siteId, folderSettings.ListId);
            using (var site = new SPSite(siteId))
            {
                var list = GetFolderSettingsList(site);
                if (list != null)
                {
                    var query = ExtractSharePointHelper.BuildQueryForFolderId(FolderId, folderSettings.FolderId);
                    var items = list.GetItems(query);
                    SPListItem item = null;
                    if (items.Count == 0)
                    {
                        item = list.AddItem();
                    }
                    else if (items.Count == 1)
                    {
                        item = items[0];
                    }
                    else
                    {
                        throw new SPException("Multiple list items refer to the same folder id.");
                    }

                    // Set the list items
                    item[ListId] = folderSettings.ListId;
                    item[FolderId] = folderSettings.FolderId;
                    item[FileExtensionList] = folderSettings.FileExtensions;
                    item[Recursive] = folderSettings.RecurseSubfolders;
                    item[ReprocessProcessed] = folderSettings.Reprocess;
                    item[AddedFiles] = folderSettings.ProcessAddedFiles;
                    item[ModifiedFiles] = folderSettings.ProcessModifiedFiles;
                    item[ProcessExistingFiles] = folderSettings.ProcessExisting;
                    item[OutputLocationType] = (int)folderSettings.OutputLocation;
                    item[OutputLocationString] = folderSettings.OutputLocationString;
                    item[QueueWithFieldValue] = folderSettings.QueueWithFieldValue;
                    item[FieldForQueuing] = folderSettings.FieldForQueuing;
                    item[ValueToQueueOn] = folderSettings.ValueToQueueOn;

                    // Update the item
                    item.Update();
                }

                // If processing existing files, get all files that match the criteria and set their status to be queued
                if (folderSettings.ProcessAddedFiles && folderSettings.ProcessExisting)
                {
                    ExtractSharePointHelper.MarkFilesToBeQueued(siteId, folderSettings, IdShieldStatusColumn,
                        IdShieldReferenceColumn);
                }
            }
        }

        /// <summary>
        /// Adds IDShield reference column to the specified list.
        /// </summary>
        /// <param name="siteId">The ID of site containing the list to modify.</param>
        /// <param name="rootListId">The ID of the list to modify.</param>
        internal static void AddIdShieldReferenceColumn(Guid siteId, Guid rootListId)
        {
            using (var site = new SPSite(siteId))
            {
                string columnName = IdShieldReferenceColumn;

                // Check if the list contains the specified column
                var list = site.RootWeb.Lists[rootListId];
                var field = list.Fields.TryGetFieldByStaticName(columnName);
                if (field == null)
                {
                    var columnQuery = string.Concat("<Field Type='URL' DisplayName='",
                        columnName, "' Name='",
                        columnName, "' ReadOnly='TRUE'><Description>",
                        "The path to the ", "redacted or unredacted",
                        " version of the file.</Description></Field>");

                    list.Fields.AddFieldAsXml(columnQuery, true, SPAddFieldOptions.AddFieldToDefaultView);
                    field = list.Fields[columnName];
                    field.Title = IdShieldReferenceColumnDisplay;
                    field.Update();

                    list.Update();
                }
            }
        }

        /// <summary>
        /// Adds the id shield status column.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="rootListId">The root list id.</param>
        static void AddIdShieldStatusColumn(Guid siteId, Guid rootListId)
        {
            ExtractSharePointHelper.AddExtractStatusColumn(siteId,
                rootListId, IdShieldStatusColumn, IdShieldStatusColumnDisplay);
        }

        /// <summary>
        /// Gets the folder settings.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <returns>A collection of folder settings for the specified site.</returns>
        internal static SortedDictionary<string, IdShieldFolderProcessingSettings> GetIdShieldFolderSettings(Guid siteId)
        {
            var folderSettings = new SortedDictionary<string, IdShieldFolderProcessingSettings>();
            using (var site = new SPSite(siteId))
            {
                var settingsList = ExtractSharePointHelper.GetSpecifiedList(site,
                    GetFolderSettingsListName(siteId));
                if (settingsList != null)
                {
                    // Store a copy of the collection before iterating (there are performances
                    // issues related to iterating Items collections if you use the list property
                    // directly.
                    SPListItemCollection items = settingsList.Items;
                    foreach (SPListItem item in items)
                    {
                        Guid folderId = new Guid(item[FolderId].ToString());
                        string url = ExtractSharePointHelper.GetFolderPath(item.Web, folderId);
                        if (!string.IsNullOrEmpty(url))
                        {
                            Guid listId = new Guid(item[ListId].ToString());
                            string extension = (string)item[FileExtensionList];
                            bool recursive = (bool)item[Recursive];
                            bool reprocess = (bool)item[ReprocessProcessed];
                            bool added = (bool)item[AddedFiles];
                            bool modified = (bool)item[ModifiedFiles];
                            bool processExisting = (bool)item[ProcessExistingFiles];
                            int outputType = (int)item[OutputLocationType];
                            string outputLocation = (string)item[OutputLocationString];
                            bool queueWithFieldValue = (bool)item[QueueWithFieldValue];
                            string fieldForQueuing = (string)item[FieldForQueuing];
                            string valueToQueueOn = (string)item[ValueToQueueOn];

                            folderSettings.Add(url, new IdShieldFolderProcessingSettings(listId, folderId,
                                url, extension, recursive, reprocess, added, modified, processExisting,
                                (IdShieldOutputLocation)outputType, outputLocation, queueWithFieldValue,
                                fieldForQueuing, valueToQueueOn));
                        }
                        else
                        {
                            try
                            {
                                RemoveWatchingForFolder(siteId, folderId);
                            }
                            catch (Exception ex)
                            {
                                LogException(ex, ErrorCategoryId.IdShieldRemoveFolderWatch, "ELI31273");
                            }
                        }
                    }
                }
            }

            return folderSettings;
        }

        /// <summary>
        /// Gets the site settings list URL.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <returns></returns>
        internal static string GetSiteSettingsListUrl(Guid siteId)
        {
            using (var site = new SPSite(siteId))
            {
                var settingsList = ExtractSharePointHelper.GetSpecifiedList(site,
                    GetFolderSettingsListName(siteId));
                if (settingsList != null)
                {
                    return settingsList.DefaultViewUrl;
                }

                return "";
            }
        }

        /// <summary>
        /// Handles logging the specified exception to the exception service
        /// and to the SharePoint log.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="categoryId">The category id for the exception.</param>
        /// <param name="eliCode">The ELI code for this exception.</param>
        internal static void LogException(Exception ex, ErrorCategoryId categoryId, string eliCode)
        {
            try
            {
                AddMachineNameDebug(ex);

                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
                if (settings != null)
                {
                    ExtractSharePointHelper.LogExceptionTcp(
                        settings.ExceptionServiceIPAddress, ex, eliCode);
                }
            }
            catch (Exception ex2)
            {
                ExtractSharePointLoggingService.LogError(categoryId, ex, eliCode);
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.ExceptionLogger, ex2,
                    "ELI30550");
            }
        }

        /// <summary>
        /// Adds the current machine name as additional debug data to the exception.
        /// </summary>
        /// <param name="ex">The exception to add the data to.</param>
        static void AddMachineNameDebug(Exception ex)
        {
            try
            {
                string debugRoot = "Machine Name";
                string debugKey = debugRoot;
                int i = 1;
                while (ex.Data.Contains(debugKey))
                {
                    debugKey = debugRoot + " " + i.ToString(CultureInfo.InvariantCulture);
                    i++;
                }

                ex.Data.Add(debugKey, Environment.MachineName);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Attempts to pass the redact now data to the local instance of
        /// ID Shield for SP client.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="localHost">The local host.</param>
        /// <returns>Returns <see langword="false"/> if the channel cannot be created
        /// due to the end point not existing (this means the client app is not running);
        /// <see langword="true"/> if it runs successfully.</returns>
        internal static bool RedactNowHelper(IDSForSPClientData data, string localHost)
        {
            ChannelFactory<IIDShieldForSPClient> factory = null;
            try
            {
                if (data != null)
                {
                    factory = CreateFactoryForClient(localHost);

                    IIDShieldForSPClient processFile = factory.CreateChannel();
                    processFile.ProcessFile(data);

                    factory.Close();
                }

                return true;
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                // This exception indicates that the end point for the WCF channel
                // is not available. Close the factory and return false.
                if (factory != null)
                {
                    factory.Abort();
                    factory = null;
                }

                return false;
            }
            catch (Exception ex)
            {
                if (factory != null)
                {
                    factory.Abort();
                    factory = null;
                }

                LogException(ex, ErrorCategoryId.IdShieldRedactNowClientLaunch, "ELI31444");

                throw;
            }
        }

        /// <summary>
        /// Attempts to pass the redact now data to the local instance of
        /// ID Shield for SP client.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="localHost">The local host.</param>
        /// <returns>Returns <see langword="false"/> if the channel cannot be created
        /// due to the end point not existing (this means the client app is not running);
        /// <see langword="true"/> if it runs successfully.</returns>
        internal static bool VerifyNowHelper(IDSForSPClientData data, string localHost)
        {
            ChannelFactory<IIDShieldForSPClient> factory = null;
            try
            {
                if (data != null)
                {
                    factory = CreateFactoryForClient(localHost);

                    IIDShieldForSPClient processFile = factory.CreateChannel();
                    processFile.VerifyFile(data);

                    factory.Close();
                }

                return true;
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                // This exception indicates that the end point for the WCF channel
                // is not available. Close the factory and return false.
                if (factory != null)
                {
                    factory.Abort();
                    factory = null;
                }

                return false;
            }
            catch (Exception ex)
            {
                if (factory != null)
                {
                    factory.Abort();
                    factory = null;
                }

                LogException(ex, ErrorCategoryId.IdShieldRedactNowClientLaunch, "ELI35836");

                throw;
            }
        }

        /// <summary>
        /// Creates the factory to used to open a channel the IDShieldForSPClient on the client machine
        /// </summary>
        /// <param name="localHost">The local host ot connect to</param>
        /// <returns>A channel factory</returns>
        static ChannelFactory<IIDShieldForSPClient> CreateFactoryForClient(
            string localHost )
        {
            ChannelFactory<IIDShieldForSPClient> factory;

            // Build the url
            StringBuilder url = new StringBuilder("http://", 1024);
            url.Append(localHost);
            url.Append(":");
            url.Append(IDSForSPClientData.IdShieldClientPort);
            url.Append("/");
            url.Append(IDSForSPClientData.IdShieldForSPClientEndpoint);

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);

            factory = new ChannelFactory<IIDShieldForSPClient>(binding,
                new EndpointAddress(url.ToString()));
            return factory;
        }
    }
}
