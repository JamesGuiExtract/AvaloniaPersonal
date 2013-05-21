using Extract.SharePoint.Redaction.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

// Using statements to make dealing with folder settings more readable
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.Redaction.IdShieldFolderProcessingSettings>;

namespace Extract.SharePoint.Redaction.Layouts
{
    /// <summary>
    /// Helper asp page used to pass data to the Verify Now protocol
    /// </summary>
    public partial class VerifyNow : LayoutsPageBase
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Do nothing if this load is the result of a post back
            if (IsPostBack)
            {
                return;
            }
            try
            {
                // Make sure there are settings that can be used
                var settings = IdShieldSettings.GetIdShieldSettings(false);
                if (string.IsNullOrEmpty(settings.VerifyFpsFile))
                {
                    Exception noFPSException = new Exception("No Verification FPS file setup in ID Shield Configuration.");
                    throw noFPSException;
                }

                
                var siteId = Request.QueryString["siteid"];
                var listId = Request.QueryString["listidvalue"];
                var fileId = Request.QueryString["fileid"];
                string ipAddress = string.Empty;
                var ipa = IPAddress.Parse(Request.UserHostAddress);
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ipa.ToString();
                }
                else if (ipa.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    ipAddress = ipa.ToString().Replace(':', '-').Replace("%", "s")
                        + ".ipv6-literal.net";
                }
                else
                {
                    throw new SPException("Unable to parse IP address: " + ipa.ToString());
                }

                if (!IsFileFolderBeingWatched(siteId, fileId))
                {
                    Exception folderWatchedEx = new Exception("The folder is not being watched.");
                    throw folderWatchedEx;
                }

                if (!IsFileQueuedForVerify(siteId, fileId))
                {
                    Exception notQueuedForVerify = new Exception("File must be Queued for Verification");
                    throw notQueuedForVerify;
                }
                hiddenLocalMachineIp.Value = ipAddress;
                hiddenSiteId.Value = siteId;
                hiddenListId.Value = listId;
                hiddenFileId.Value = fileId;

                timerClose.Enabled = true;
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldRedactNowHelper, "ELI35863");
                Label1.Visible = false;
                ErrorLabel.Text = "Error: " + ex.Message;
                ErrorLabel.Visible = true;
                ErrorLabel.Enabled = true;
            }
        }

        /// <summary>
        /// Handles the timer tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void HandleTimerTick(object sender, EventArgs e)
        {
            try
            {
                timerClose.Enabled = false;
                var sb = new StringBuilder("<script type=\"text/javascript\">");

                sb.Append("window.frameElement.commitPopup(); ");
                sb.Append("</script>");
                var settings = IdShieldSettings.GetIdShieldSettings(false);
                if (settings != null && !string.IsNullOrEmpty(settings.VerifyFpsFile))
                {
                    var siteId = hiddenSiteId.Value;
                    var listId = hiddenListId.Value;
                    var fileId = hiddenFileId.Value;
                    using (var site = new SPSite(new Guid(siteId)))
                    {
                        var data = new IDSForSPClientData(site.Url, listId, fileId, settings.VerifyFpsFile, settings.LocalWorkingFolder);
                        if (!IdShieldHelper.VerifyNowHelper(data, hiddenLocalMachineIp.Value))
                        {
                            sb.Append("alert('ID Shield for SP client application could not be found. ");
                            sb.Append("Please ensure that it is running and try again.'); ");
                        }
                    }
                }
                else
                {
                    sb.Append("alert('Path to ID Shield for SP Verification FPS file is not set.'); ");
                }
                Context.Response.Write(sb.ToString());
                Context.Response.Flush();
                Context.Response.End();
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldRedactNowHelper, "ELI35868");
            }
        }


        /// <summary>
        /// Checks if the folder for the file represented by <see paramref="fileID"/> is being watched
        /// </summary>
        /// <param name="siteID">String representation of the SiteID</param>
        /// <param name="fileID">String representation of the file ID</param>
        /// <returns><see langword="true"/> if the folder the file is in is being watched
        /// <see langword="false"/> if the folder is not being watched</returns>
        bool IsFileFolderBeingWatched(string siteID, string fileID)
        {
            return !(getFolderSettings(siteID, fileID) == null);
        }

        /// <summary>
        /// Gets the folder settings for the site given by <see paramref="siteID"/> and the folder the
        /// file given by <see paramref="fileID"/>
        /// </summary>
        /// <param name="siteID">String representation of the SiteID</param>
        /// <param name="fileID">String representation of the file ID</param>
        /// <returns>The folder settings for the files folder.</returns>
        IdShieldFolderProcessingSettings getFolderSettings(string siteID, string fileID)
        {
            Guid siteGUID = new Guid(siteID);

            SiteFolderSettingsCollection folderSettings = IdShieldHelper.GetIdShieldFolderSettings(siteGUID);

            SPFile file = Web.GetFile(new Guid(fileID));

            string folder = file.Item.Url;
            string fileName = file.Name;
            folder = (folder[0] != '/' ? folder.Insert(0, "/") : folder).Replace("/" + fileName, "");

            foreach (KeyValuePair<string, IdShieldFolderProcessingSettings> pair in folderSettings)
            {
                if (IsFolderBeingWatched(folder, pair.Key, pair.Value.RecurseSubfolders))
                {
                    // Since folder is being watched we can assume the output has been set
                    return pair.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks whether the specified folder is being watched based on the 
        /// specified watch path and recursion settings.
        /// </summary>
        /// <param name="folder">The folder to check.</param>
        /// <param name="watchPath">The root watch path to compare.</param>
        /// <param name="recurseSubFolders">Whether sub folders are recursively watched.</param>
        /// <returns><see langword="true"/> if the folder is being watched and
        /// <see langword="false"/> otherwise.</returns>
        static bool IsFolderBeingWatched(string folder, string watchPath, bool recurseSubFolders)
        {
            return folder.Equals(watchPath, StringComparison.Ordinal)
                || (recurseSubFolders && folder.StartsWith(watchPath + "/", StringComparison.Ordinal));
        }

        /// <summary>
        /// Checks if the IDS Status for the given file is "Queued For Verification"
        /// </summary>
        /// <param name="siteID">String representation of the SiteID</param>
        /// <param name="fileID">String representation of the file ID</param>
        /// <returns>true if file is "Queued For Verification and false if not</returns>
        bool IsFileQueuedForVerify(string siteID, string fileID)
        {
            // Get the file
            SPFile file = Web.GetFile(new Guid(fileID));

            // Check the status
            return ((string)file.Item[IdShieldHelper.IdShieldStatusColumn]) == ExtractProcessingStatus.QueuedForVerification.AsString();
        }
    }
}
