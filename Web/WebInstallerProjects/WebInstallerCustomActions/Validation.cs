using Extract;
using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebInstallerCustomActions
{
    public static class Validation
    {
        private const string WorkflowSQL =
@"
SELECT 
	[Workflow].[Name]
    , [Workflow].[StartActionID]
    , [Workflow].[EndActionID]
    , [Workflow].[DocumentFolder]
    , [Workflow].[OutputAttributeSetID]
    , [Workflow].[OutputFileMetadataFieldID]
    , [Workflow].[EditActionID]
    , [Workflow].[PostEditActionID]
	, [WebAppConfig].[Settings]

FROM 
	[dbo].[Workflow]
		LEFT OUTER JOIN [dbo].[WebAppConfig]
			ON dbo.Workflow.ID = dbo.WebAppConfig.WorkflowID
			AND dbo.WebAppConfig.Type = 'RedactionVerificationSettings'
WHERE
	[Workflow].Name = @Name";


        [CustomAction]
        public static string DNSValidation(Session session)
        {
            if (session == null)
            {
                throw new NullReferenceException("Session cannot be null");
            }
            try
            {
                var validationMessage = string.Empty;

                validationMessage = string.IsNullOrEmpty(session["APPBACKEND_DNS_ENTRY"]) ? validationMessage : validationMessage + $"\nExtract AppBackend API is valid:{TestDNSEntry(session["APPBACKEND_DNS_ENTRY"])}";
                validationMessage = string.IsNullOrEmpty(session["DOCUMENTAPI_DNS_ENTRY"]) ? validationMessage : validationMessage + $"\nExtract Document API is valid:{TestDNSEntry(session["DOCUMENTAPI_DNS_ENTRY"])}";
                validationMessage = string.IsNullOrEmpty(session["IDSVERIFY_DNS_ENTRY"]) ? validationMessage : validationMessage + $"\nExtract Verify is valid:{TestDNSEntry(session["IDSVERIFY_DNS_ENTRY"])}";
                validationMessage = string.IsNullOrEmpty(session["WINDOWSAUTHORIZATION_DNS_ENTRY"]) ? validationMessage : validationMessage + $"\nExtract Authorization API is valid:{TestDNSEntry(session["WINDOWSAUTHORIZATION_DNS_ENTRY"])}";

                if (!string.IsNullOrEmpty(validationMessage))
                {
                    return $"This test simply checks if the DNS entry provided points to the IP of the computer you are using. If a site is hosted on a different computer then this validation check is not valid.\n" + validationMessage;
                }
            
                return validationMessage;
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI51513").Log();
                throw;
            }
        }

        [CustomAction]
        public static ActionResult UserNameAndPermissionValidaion(Session session)
        {
            IntPtr accessToken = IntPtr.Zero;
            bool usernameAndPasswordValid;
            bool userHasPermissionsToDB = false;
            WebsiteVerificationModel websiteVerificationModel = null;
            bool userHasPermissionToFolder = false;

            if (session == null)
            {
                throw new NullReferenceException("Session cannot be null");
            }
            try
            {
                usernameAndPasswordValid = NativeMethods.LogonUser(session["APPPOOL_USER_NAME"], session["APPPOOL_USER_DOMAIN"], session["APPPOOL_USER_PASSWORD"], 2, 0, ref accessToken);

                if (usernameAndPasswordValid)
                {
                    using WindowsIdentity identity = new WindowsIdentity(accessToken);
                    using WindowsImpersonationContext ctx = identity.Impersonate();
                    using SqlConnection connection = new SqlConnection($"Data Source={session["DATABASE_SERVER"]};Initial Catalog={session["DATABASE_NAME"]};Persist Security Info=true;Integrated Security=SSPI");
                    {
                        try
                        {
                            connection.Open();
                            userHasPermissionsToDB = true;
                            websiteVerificationModel = VerifyWorkflowInformation(session["DATABASE_WORKFLOW"], connection, session);
                            connection.Close();
                            if (websiteVerificationModel.WorkflowExists)
                            {
                                userHasPermissionToFolder = HasPermissionsToFolder(websiteVerificationModel.documentFolder);
                            }

                        }
                        catch (SqlException) { }
                    }
                    ctx.Undo();
                }

                string validationMessage = DNSValidation(session) + $"\nUser Name and password are valid: {usernameAndPasswordValid}"
                    + $"\nUser has access to the database: {userHasPermissionsToDB}"
                    + $"\nUser has permissions to the folder specified in the workflow: {userHasPermissionToFolder}";

                if (websiteVerificationModel != null)
                {
                    validationMessage += websiteVerificationModel.GetWarnings();
                }

                MessageBox.Show(validationMessage);

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI51513").Log();
                throw;
            }
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is just testing the dns entry. If it fails that is fine.")]
        private static bool TestDNSEntry(string entry)
        {
            var isEntryValid = false;
            try
            {
                foreach (IPAddress ip in Dns.GetHostAddresses(entry))
                {
                    if (ip.ToString().Equals(GetLocalIPAddress(), StringComparison.OrdinalIgnoreCase))
                    {
                        isEntryValid = true;
                    }
                }
            }
            catch (Exception) { }

            return isEntryValid;
        }

        private static WebsiteVerificationModel VerifyWorkflowInformation(string workflow, SqlConnection connection, Session session)
        {
            WebsiteVerificationModel websiteVerificationModel = new WebsiteVerificationModel(session);
            try
            {
                using SqlCommand command = new SqlCommand(WorkflowSQL, connection);
                command.Parameters.AddWithValue("@Name", workflow);
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    websiteVerificationModel.WorkflowExists = true;
                    websiteVerificationModel.startActionID = reader["StartActionID"].ToString();
                    websiteVerificationModel.EndActionID = reader["EndActionID"].ToString();
                    websiteVerificationModel.documentFolder = reader["DocumentFolder"].ToString();
                    websiteVerificationModel.outputAttributeSetID = reader["OutputAttributeSetID"].ToString();
                    websiteVerificationModel.OutputFileMetadataFieldID = reader["OutputFileMetadataFieldID"].ToString();
                    websiteVerificationModel.EditActionID = reader["EditActionID"].ToString();
                    websiteVerificationModel.PostEditActionID = reader["PostEditActionID"].ToString();
                    websiteVerificationModel.settings = reader["Settings"].ToString();
                }
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI51518").Log();
                throw;
            }
            return websiteVerificationModel;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "If an exception occurs here it is fine.")]
        private static bool HasPermissionsToFolder(string folder)
        {
            bool hasPermission = false;
            try
            {
                File.WriteAllText(folder + "\\test.txt", "Testing permissions.");
                File.Delete(folder + "\\test.txt");
                hasPermission = true;
            }
            catch (Exception) { }
            return hasPermission;
        }
    }

    internal static class NativeMethods
    {
        /// This is how you are supposed to impersonate users apparently.
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        public static extern bool LogonUser(string userName, string domain, string password, int logonType, int logonProvider, ref IntPtr accessToken);
    }
}
