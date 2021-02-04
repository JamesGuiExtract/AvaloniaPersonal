﻿using Extract;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using static System.FormattableString;
using static WebInstallerCustomActions.ValidationResultsForm;

namespace WebInstallerCustomActions
{
    class WebsiteVerificationModel
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

        public Session Session { get; set; }
        public bool InstallingDocumentAPI { get; set; }
        public bool InstallingVerification { get; set; }
        public bool InstallingAuthenticationAPI { get; set; }
        public bool UsernameAndPasswordValid { get; set; }
        public bool UserHasPermissionsToDB { get; set; }
        public bool WorkflowExists { get; set; } = false;
        public string StartActionID { get; set; }
        public string EndActionID { get; set; }
        public string DocumentFolder { get; set; }
        public string OutputAttributeSetID { get; set; }
        public string OutputFileMetadataFieldID { get; set; }
        public string EditActionID { get; set; }
        public string PostEditActionID { get; set; }
        public string Settings { get; set; }

        public WebsiteVerificationModel(Session session)
        {
            try
            {
                this.Session = session;
                IntPtr accessToken = IntPtr.Zero;
                UsernameAndPasswordValid = NativeMethods.LogonUser(
                    session["APPPOOL_USER_NAME"], session["APPPOOL_USER_DOMAIN"], 
                    session["APPPOOL_USER_PASSWORD"], 
                    5, // LOGON32_LOGON_SERVICE 
                    0, ref accessToken);
                if (!UsernameAndPasswordValid)
                {
                    UsernameAndPasswordValid = NativeMethods.LogonUser(
                        session["APPPOOL_USER_NAME"], session["APPPOOL_USER_DOMAIN"],
                        session["APPPOOL_USER_PASSWORD"],
                        2, // LOGON32_LOGON_INTERACTIVE 
                        0, ref accessToken);
                }

                InstallingDocumentAPI = Session["CREATE_DOCUMENTAPI_SITE"].Equals("1", StringComparison.OrdinalIgnoreCase);
                InstallingVerification = Session["CREATE_VERIFY_SITE"].Equals("1", StringComparison.OrdinalIgnoreCase);
                InstallingAuthenticationAPI = InstallingVerification && Session["CREATE_WINDOWS_AUTHORIZATION_SITE"].Equals("1", StringComparison.OrdinalIgnoreCase);

                string dbServer = session["DATABASE_SERVER"];
                string dbName = session["DATABASE_NAME"];
                if (UsernameAndPasswordValid
                    && !string.IsNullOrWhiteSpace(dbServer)
                    && !string.IsNullOrWhiteSpace(dbName))
                {
                    using WindowsIdentity identity = new WindowsIdentity(accessToken);
                    using WindowsImpersonationContext ctx = identity.Impersonate();

                    using SqlConnection connection = new SqlConnection(
                        $"Data Source={session["DATABASE_SERVER"]};Initial Catalog={session["DATABASE_NAME"]};Persist Security Info=true;Integrated Security=SSPI");
                    {
                        try
                        {
                            connection.Open();
                            UserHasPermissionsToDB = true;
                            ReadWorkflowConfiguration(connection);
                            connection.Close();
                        }
                        catch (SqlException) { }
                    }

                    ctx.Undo();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51523");
            }
        }

        void ReadWorkflowConfiguration(SqlConnection connection)
        {
            try
            {
                string workflow = Session["DATABASE_WORKFLOW"];

                if (!string.IsNullOrWhiteSpace(workflow))
                {
                    using SqlCommand command = new SqlCommand(WorkflowSQL, connection);
                    command.Parameters.AddWithValue("@Name", workflow);
                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        WorkflowExists = true;
                        StartActionID = reader["StartActionID"].ToString();
                        EndActionID = reader["EndActionID"].ToString();
                        DocumentFolder = reader["DocumentFolder"].ToString();
                        OutputAttributeSetID = reader["OutputAttributeSetID"].ToString();
                        OutputFileMetadataFieldID = reader["OutputFileMetadataFieldID"].ToString();
                        EditActionID = reader["EditActionID"].ToString();
                        PostEditActionID = reader["PostEditActionID"].ToString();
                        Settings = reader["Settings"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51522");
            }
        }

        public void ShowValidationMessages()
        {
            ExtractException.Assert("ELI51535", "Session cannot be null", Session != null);

            using var validationResultsForm = new ValidationResultsForm();

            AddSiteMessages(validationResultsForm);
            AddUserAccessMessages(validationResultsForm);
            AddWorkflowMessages(validationResultsForm);

            validationResultsForm.ShowDialog();
        }

        void AddSiteMessages(ValidationResultsForm validationResultsForm)
        {
            validationResultsForm.AddHeading("Host configuration:");

            if (!InstallingDocumentAPI && !InstallingVerification)
            {
                validationResultsForm.AddError("No Extract sites have been selected to be installed.");
            }

            var specifiedSites = new List<SiteInfo>();

            if (InstallingDocumentAPI)
            {
                specifiedSites.Add(new SiteInfo(Session, "Extract Document API", "DOCUMENTAPI_DNS_ENTRY", "DocumentAPI"));
            }

            if (InstallingVerification)
            {
                specifiedSites.Add(new SiteInfo(Session, "Extract Redaction Verify", "IDSVERIFY_DNS_ENTRY", "IDSVerify"));
                specifiedSites.Add(new SiteInfo(Session, "Extract App Backend API", "APPBACKEND_DNS_ENTRY", "AppBackendAPI"));

                if (InstallingAuthenticationAPI)
                {
                    specifiedSites.Add(new SiteInfo(Session, "Extract Authorization API", "WINDOWSAUTHORIZATION_DNS_ENTRY", "AuthorizationAPI"));
                }
            }

            foreach (var site in specifiedSites)
            {
                AddHostMessages(validationResultsForm, site);
            }

            foreach (var hostName in specifiedSites
                .GroupBy(site => site.Bindings.FirstOrDefault(), StringComparer.OrdinalIgnoreCase)
                .Where(group => !string.IsNullOrEmpty(group.Key) && group.Count() > 1)
                .Select(group => group.Key))
            {
                validationResultsForm.AddWarning(
                    Invariant($"'{hostName}' is the host name for multiple sites and will conflict without use of different ports"));
            }

            AddDuplicateSiteMessages(validationResultsForm, specifiedSites);
        }

        void AddDuplicateSiteMessages(ValidationResultsForm validationResultsForm, List<SiteInfo> specifiedSites)
        {
            using var iisManager = new ServerManager();
            var existingSites = iisManager.Sites.Select(site => new SiteInfo(site));

            foreach (var siteName in existingSites
                .Select(site => site.Name)
                .Intersect(specifiedSites.Select(site => site.Name), StringComparer.OrdinalIgnoreCase)
                .Distinct())
            {
                validationResultsForm.AddWarning(Invariant($"Site '{siteName}' is already installed"));
            }

            foreach (var physicalPath in existingSites
                .Select(site => site.PhysicalPath)
                .Intersect(specifiedSites.Select(site => site.PhysicalPath), StringComparer.OrdinalIgnoreCase)
                .Distinct())
            {
                validationResultsForm.AddWarning(Invariant($"A site is already installed at path '{Path.Combine(Session["INSTALLLOCATION"], physicalPath)}'"));
            }

            foreach (var hostName in existingSites
                .SelectMany(site => site.Bindings)
                .Intersect(specifiedSites.SelectMany(site => site.Bindings), StringComparer.OrdinalIgnoreCase)
                .Distinct())
            {
                validationResultsForm.AddWarning(
                    Invariant($"A site is already installed using host name '{hostName}' and will conflict without use of different ports"));
            }
        }

        static void AddHostMessages(ValidationResultsForm validationResultsForm, SiteInfo site)
        {
            var host = site.Bindings.FirstOrDefault();
            if (host.Contains('/') || host.Contains(':'))
            {
                validationResultsForm.AddError(
                    Invariant($"'{host}' should be domain name or IP only; no slashes, port or http prefix should be used"));
                return;
            }

            var docAPIHost = GetHostEntry(host);

            if (docAPIHost == null)
            {
                validationResultsForm.AddError(Invariant($"{site.Name} host name is missing or invalid"));
            }
            else
            {
                validationResultsForm.AddValid(Invariant($"'{site.Name}' host name valid"));

                var status = docAPIHost.AddressList.Contains(GetLocalIPAddress()) ? ValidationResult.Valid : ValidationResult.Warning;
                validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                    ? Invariant($"'{host}' host name verified to target this machine")
                    : Invariant($"Could not confirm '{host}' host name targets this machine"));
            }
        }

        void AddUserAccessMessages(ValidationResultsForm validationResultsForm)
        {
            validationResultsForm.AddHeading("User access configuration:");

            var status = UsernameAndPasswordValid ? ValidationResult.Valid : ValidationResult.Error;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "User credentials are valid"
                : "Invalid username or password");

            if (UsernameAndPasswordValid)
            {
                status = UserHasPermissionsToDB ? ValidationResult.Valid : ValidationResult.Error;
                validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                    ? "Database access confirmed"
                    : "Database invalid or user lacks appropriate database access");
            }
        }

        void AddWorkflowMessages(ValidationResultsForm validationResultsForm)
        {
            if (!UserHasPermissionsToDB
                || (!InstallingDocumentAPI && !InstallingVerification))
            {
                return;
            }

            validationResultsForm.AddHeading("Workflow configuration:");

            var status = this.WorkflowExists ? ValidationResult.Valid : ValidationResult.Error;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The database contains the specified workflow"
                : "The workflow does not exist in the database");
            
            if (this.WorkflowExists)
            {
                if (string.IsNullOrEmpty(DocumentFolder))
                {
                    if (InstallingDocumentAPI)
                    {
                        validationResultsForm.AddWarning("Input folder is required for API method 'POST Document'");
                    }
                }
                else
                {
                    status = HasPermissionsToFolder(this.DocumentFolder) ? ValidationResult.Valid : ValidationResult.Error;
                    validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                        ? "Input folder access is confirmed"
                        : "Input folder does not exist or user does not have access");
                }

                if (InstallingDocumentAPI)
                {
                    status = !string.IsNullOrEmpty(StartActionID) ? ValidationResult.Valid : ValidationResult.Error;
                    validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                        ? "The start action is configured"
                        : "The start action is required");

                    status = !string.IsNullOrEmpty(EndActionID) ? ValidationResult.Valid : ValidationResult.Error;
                    validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                        ? "The end action is configured"
                        : "The end action is required");
                }

                status = !string.IsNullOrEmpty(OutputAttributeSetID)
                    ? ValidationResult.Valid
                    : InstallingVerification ? ValidationResult.Error : ValidationResult.Warning;
                validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                    ? "The output attribute set is configured"
                    : InstallingVerification
                        ? "The output attribute set has not been specified"
                        : "The output attribute set is required for 'GET/PUT/PATCH DocumentData' API methods");

                status = !string.IsNullOrEmpty(EditActionID)
                    ? ValidationResult.Valid
                    : InstallingVerification ? ValidationResult.Error : ValidationResult.Warning;
                validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                    ? "The verify/update action is configured"
                    : InstallingVerification
                        ? "The verify/update action has not been specified"
                        : "The verify/update action is required for 'PUT/PATCH DocumentData' API methods");

                status = !string.IsNullOrEmpty(PostEditActionID)
                      ? ValidationResult.Valid
                      : ValidationResult.Warning;
                validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                    ? "The post-verify/update action is configured"
                    : InstallingVerification
                        ? "The post-verify/update action is required if post-verification processing is necessary (e.g. create a redacted copy of image)"
                        : "The post-verify/update action is required if 'PUT/PATCH DocumentData' API methods should trigger edit post-edit processing");

                if (InstallingDocumentAPI)
                {
                    status = !string.IsNullOrEmpty(OutputFileMetadataFieldID) ? ValidationResult.Valid : ValidationResult.Warning;
                    validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                        ? "The output file path is configured"
                        : "The output file path metatdata field is required for API method 'GET OutputFile' (e.g. to get a redacted copy of the document)");
                }

                if (InstallingVerification)
                {
                    status = !string.IsNullOrEmpty(Settings) ? ValidationResult.Valid : ValidationResult.Error;
                    validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                        ? "The redaction verification settings have been configured"
                        : "The redaction verification settings have not been configured");
                }
            }
        }

        static IPHostEntry GetHostEntry(string entryToCheck)
        {
            if (string.IsNullOrWhiteSpace(entryToCheck))
            {
                return null;
            }
            try
            {
                return Dns.GetHostEntry(entryToCheck);
            }
            catch (Exception e) when (e is SocketException || e is ArgumentOutOfRangeException)
            {
                return null;
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

            return null;
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

        class SiteInfo
        {
            public SiteInfo(Site iisSite)
            {
                Name = iisSite.Name;
                Bindings = iisSite.Bindings.Select(b => b.Host).ToList();
                PhysicalPath = iisSite.Applications["/"].VirtualDirectories["/"].PhysicalPath;
            }

            public SiteInfo(Session session, string siteName, string hostNameKey, string directory)
            {
                Name = siteName;
                Bindings = new List<string>(new[] { session[hostNameKey] });
                PhysicalPath = Path.Combine(session["INSTALLLOCATION"], directory);
            }

            public string Name { get; set; }
            public List<string> Bindings { get; set; }
            public string PhysicalPath { get; set; }
        }
    }

    internal static class NativeMethods
    {
        /// This is how you are supposed to impersonate users apparently.
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LogonUser(string userName, string domain, string password, int logonType, int logonProvider, ref IntPtr accessToken);
    }
}
