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
    /// Helper asp page used to pass data to the redact now protocol
    /// </summary>
    public partial class RedactNowHelper : LayoutsPageBase
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Do nothing if this load is the result of a post back
                if (IsPostBack)
                {
                    return;
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
                IdShieldHelper.LogException(ex, ErrorCategoryId.IdShieldRedactNowHelper,
                    "ELI31506");
                throw;
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
            var settings = IdShieldSettings.GetIdShieldSettings(false);
            if (settings != null && !string.IsNullOrEmpty(settings.RedactNowFpsFile))
            {
                var siteId = hiddenSiteId.Value;
                var listId = hiddenListId.Value;
                var fileId = hiddenFileId.Value;
                using (var site = new SPSite(new Guid(siteId)))
                {
                    var data = new RedactNowData(site.Url, listId, fileId, settings.RedactNowFpsFile);
                    if (!IdShieldHelper.RedactNowHelper(data, hiddenLocalMachineIp.Value))
                    {
                        sb.Append("alert('ID Shield for SP client application could not be found. ");
                        sb.Append("Please ensure that it is running and try again.'); ");
                    }
                }
            }
            else
            {
                sb.Append("alert('Path to ID Shield for SP Client FPS file is not set.'); ");
            }
            sb.Append("window.frameElement.commitPopup(); ");
            sb.Append("</script>");

            Context.Response.Write(sb.ToString());
            Context.Response.Flush();
            Context.Response.End();
        }

        /// <summary>
        /// Sets the UI to indicate to the user that they do not have permission
        /// to change the configuration of the current folder.
        /// </summary>
        void SetUIToIndicateNoPermission()
        {

            imageIdShield.Visible = false;
            Label1.Visible = false;
            panelMessage.Visible = true;
            panelButtons.Visible = true;
            labelMessage.Text = "You must be a member of the "
                + "ID Shield Administrator group to use this control.";
        }

    }
}
