using Extract.SharePoint.Redaction.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

                if (!IsFileQueuedForVerify(siteId, fileId))
                {
                    Exception notQueuedForVerify = new SPException("File must be Queued for Verification");
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
                            sb.Append("alert('ID Shield For SharePoint Client application could not be found. ");
                            sb.Append("Please ensure that it is running and try again.'); ");
                        }
                    }
                }
                else
                {
                    sb.Append("alert('Path to ID Shield for SharePoint Verification FPS file is not set.'); ");
                }
                sb.Append("window.frameElement.commitPopup(); ");
                sb.Append("</script>");
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
