using Extract;
using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
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
        public static string HostNameValidation(Session session)
        {
            if (session == null)
            {
                throw new NullReferenceException("Session cannot be null");
            }
            try
            {
                var validationMessage = string.Empty;

                if (session["CREATE_DOCUMENTAPI_SITE"] == "1")
                {
                    var docAPIHost = GetHostEntry(session["DOCUMENTAPI_DNS_ENTRY"]);
                    validationMessage += $"\n{(docAPIHost != null ? ((char)0x221A).ToString() : "X")}  Extract Document API Host Name Valid";
                    validationMessage += $"\n{(docAPIHost != null && docAPIHost.AddressList.Contains(GetLocalIPAddress()) ? ((char)0x221A).ToString() : "X")}  Extract Document API IP matches Domain";                    
                }
                if (session["CREATE_VERIFY_SITE"] == "1")
                {
                    var verifyHost = GetHostEntry(session["IDSVERIFY_DNS_ENTRY"]);
                    validationMessage += $"\n{(verifyHost != null ? ((char)0x221A).ToString() : "X")}  Extract Verify Host Name Valid";
                    validationMessage += $"\n{(verifyHost != null && verifyHost.AddressList.Contains(GetLocalIPAddress()) ? ((char)0x221A).ToString() : "X")}  Extract Verify API IP matches Domain";
                }
                if (session["CREATE_VERIFY_SITE"] == "1")
                {
                    var appBackendHost = GetHostEntry(session["APPBACKEND_DNS_ENTRY"]);
                    validationMessage += $"\n{(appBackendHost != null ? ((char)0x221A).ToString() : "X")}  Extract AppBackend API Host Name Valid";
                    validationMessage += $"\n{(appBackendHost != null && appBackendHost.AddressList.Contains(GetLocalIPAddress()) ? ((char)0x221A).ToString() : "X")}  Extract AppBackend API IP matches Domain";
                }
                if (session["CREATE_VERIFY_SITE"] == "1" && session["CREATE_WINDOWS_AUTHORIZATION_SITE"] == "1")
                {
                    var winAuthHost = GetHostEntry(session["WINDOWSAUTHORIZATION_DNS_ENTRY"]);
                    validationMessage += $"\n{(winAuthHost != null ? ((char)0x221A).ToString() : "X")}  Extract Authorization API Host Name Valid";
                    validationMessage += $"\n{(winAuthHost != null && winAuthHost.AddressList.Contains(GetLocalIPAddress()) ? ((char)0x221A).ToString() : "X")}  Extract Authorization API IP matches Domain";
                }


                return validationMessage;
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI51520").Log();
                throw;
            }
        }

        private static IPHostEntry GetHostEntry(string entryToCheck)
        {
            if(entryToCheck == null)
            {
                return null;
            }
            try
            {
                return Dns.GetHostEntry(entryToCheck);
            }
            catch (Exception e) when (e is SocketException || e is ArgumentOutOfRangeException) {
                return null;
            }
        }

        [CustomAction]
        public static ActionResult UserNameAndPermissionValidation(Session session)
        {
            IntPtr accessToken = IntPtr.Zero;
            bool usernameAndPasswordValid;
            bool userHasPermissionsToDB = false;
            WebsiteVerificationModel websiteVerificationModel = null;

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
                        }
                        catch (SqlException) { }
                    }
                    ctx.Undo();
                }

                string validationMessage = HostNameValidation(session) + $"\n{(usernameAndPasswordValid ? ((char)0x221A).ToString() : "X")}  User Name and password"
                    + $"\n{(userHasPermissionsToDB ? ((char)0x221A).ToString() : "X")}  Database Access";

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

        private static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
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
    }

    internal static class NativeMethods
    {
        /// This is how you are supposed to impersonate users apparently.
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        public static extern bool LogonUser(string userName, string domain, string password, int logonType, int logonProvider, ref IntPtr accessToken);
    }
}
