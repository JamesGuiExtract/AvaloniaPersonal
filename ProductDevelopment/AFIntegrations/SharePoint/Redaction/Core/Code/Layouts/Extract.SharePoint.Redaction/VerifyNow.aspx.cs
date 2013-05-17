using Extract.SharePoint.Redaction.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Extract.SharePoint.Redaction.Layouts.Extract.SharePoint.Redaction
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
                    var data = new RedactNowData(site.Url, listId, fileId, settings.VerifyFpsFile, settings.LocalWorkingFolder);
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
    }
}
