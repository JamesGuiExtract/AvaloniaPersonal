using Extract;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
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
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;
using static System.FormattableString;
using static WebInstallerCustomActions.ValidationResultsForm;

namespace WebInstallerCustomActions
{
    class WebsiteVerificationModel
    {
        public Session Session { get; set; }
        public bool InstallingDocumentAPI { get; set; }
        public bool InstallingVerification { get; set; }
        public bool InstallingAuthenticationAPI { get; set; }
        public bool UsernameAndPasswordValid { get; set; }
        public bool UserHasPermissionsToDB { get; set; }
        public bool CanReadConfigurations { get; set; }
        public IConfigurationDatabaseService ConfigurationDatabaseService { get; set; }

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

                    using SqlConnection connection = new(
                        $"Data Source={dbServer};Initial Catalog={dbName};Persist Security Info=true;Integrated Security=SSPI");
                    {
                        try
                        {
                            connection.Open();
                            UserHasPermissionsToDB = true;
                            CanReadConfigurations = true;
                            connection.Close();
                        }
                        catch (SqlException) { }
                    }

                    ctx.Undo();

                    ConfigurationDatabaseService = new ConfigurationDatabaseService(new FileProcessingDBClass() { DatabaseName = dbName, DatabaseServer = dbServer });
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51523");
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

            validationResultsForm.AddHeading("Configurations:");

            if (!CanReadConfigurations)
            {
                validationResultsForm.AddMessage(ValidationResult.Error, "Unable to read database configurations.");
                return;
            }

            if (InstallingDocumentAPI)
            {
                AddDocumentAPIMessages(validationResultsForm);
            }

            if (InstallingVerification)
            {
                AddRedactionMessages(validationResultsForm);
            }
        }

        private void AddRedactionMessages(ValidationResultsForm validationResultsForm)
        {
            validationResultsForm.AddHeading("Verification Default Configuration:");
            ValidationResult status;
            IRedactionWebConfiguration defaultConfiguration;

            try
            {
                defaultConfiguration = ConfigurationDatabaseService.RedactionWebConfigurations
                .Single(config => config.ConfigurationName.Equals(this.Session["VERIFICATION_CONFIGURATION"]));
            }
            catch (Exception)
            {
                validationResultsForm.AddMessage(ValidationResult.Error, $"Unable to find a matching configuration for {this.Session["VERIFICATION_CONFIGURATION"]}. Ensure there is a redaction configuration with that name.");
                return;
            }

            AddSharedConfigurationMessages(validationResultsForm, defaultConfiguration);

            status = defaultConfiguration.RedactionTypes.Count > 0
                  ? ValidationResult.Valid
                  : ValidationResult.Warning;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The redaction types have been setup."
                : "The redaction types have not been configured. This is not required, but will leave dropdowns empty.");

            status = !string.IsNullOrEmpty(defaultConfiguration.DocumentTypeFileLocation)
                  ? ValidationResult.Valid
                  : ValidationResult.Warning;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The document types file location has been set."
                : "The document types file location has not been set. Document types may not be populated in the web application.");

            validationResultsForm.AddMessage(ValidationResult.Valid, $"This configuration enables the all pending queue: {defaultConfiguration.EnableAllUserPendingQueue}");
            string limitingGroups = defaultConfiguration.ActiveDirectoryGroups == null ? string.Empty : string.Join(", ", defaultConfiguration.ActiveDirectoryGroups);
            validationResultsForm.AddMessage(ValidationResult.Valid, $"This configuration is limited by these AD groups: {limitingGroups}");
        }

        private void AddDocumentAPIMessages(ValidationResultsForm validationResultsForm)
        {
            ValidationResult status;
            validationResultsForm.AddHeading("DocumentAPI Default Configuration:");
            IDocumentApiWebConfiguration defaultConfiguration;

            try
            {
                defaultConfiguration = ConfigurationDatabaseService.DocumentAPIWebConfigurations
                .Single(config => config.ConfigurationName.Equals(this.Session["DOCUMENTAPI_CONFIGURATION"]));
            }
            catch (Exception)
            {
                validationResultsForm.AddMessage(ValidationResult.Error,
                    string.IsNullOrEmpty(this.Session["DOCUMENTAPI_CONFIGURATION"])
                    ? "Configuration not specified for the document API."
                    : $"Unable to find a matching configuration for {this.Session["DOCUMENTAPI_CONFIGURATION"]}. Ensure there is a document API configuration with that name."
                    );
                return;
            }

            if (string.IsNullOrEmpty(defaultConfiguration.DocumentFolder))
            {
                if (InstallingDocumentAPI)
                {
                    validationResultsForm.AddWarning("Input folder is required for API method 'POST Document'");
                }
            }
            else
            {
                status = HasPermissionsToFolder(defaultConfiguration.DocumentFolder) ? ValidationResult.Valid : ValidationResult.Error;
                validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                    ? "Input folder access is confirmed"
                    : "Input folder does not exist or user does not have access");
            }

            status = !string.IsNullOrEmpty(defaultConfiguration.StartWorkflowAction) ? ValidationResult.Valid : ValidationResult.Error;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The start action is configured"
                : "The start action is required");

            status = !string.IsNullOrEmpty(defaultConfiguration.EndWorkflowAction) ? ValidationResult.Valid : ValidationResult.Error;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The end action is configured"
                : "The end action is required");

            status = !string.IsNullOrEmpty(defaultConfiguration.PostWorkflowAction) ? ValidationResult.Valid : ValidationResult.Warning;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The post workflow action is configured"
                : "The post workflow action is not configured");

            AddSharedConfigurationMessages(validationResultsForm, defaultConfiguration);

            status = !string.IsNullOrEmpty(defaultConfiguration.OutputFileNameMetadataField) ? ValidationResult.Valid : ValidationResult.Warning;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The output file path is configured"
                : "The output file path metatdata field is required for API method 'GET OutputFile' (e.g. to get a redacted copy of the document)");

            status = !string.IsNullOrEmpty(defaultConfiguration.OutputFileNameMetadataInitialValueFunction) ? ValidationResult.Valid : ValidationResult.Warning;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The Output File Name Metadata Initial Value Function is configured"
                : "The Output File Name Metadata Initial Value Function is not configured");
        }

        private void AddSharedConfigurationMessages(ValidationResultsForm validationResultsForm, ICommonWebConfiguration defaultConfiguration)
        {
            ValidationResult status = !string.IsNullOrEmpty(defaultConfiguration.WorkflowName)
                  ? ValidationResult.Valid
                  : ValidationResult.Error;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The workflow name is configured"
                : "The workflow name is NOT configured. This is required to process documents.");

            status = !string.IsNullOrEmpty(defaultConfiguration.AttributeSet)
                    ? ValidationResult.Valid
                    : InstallingVerification ? ValidationResult.Error : ValidationResult.Warning;
            validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The attribute set is configured"
                : InstallingVerification
                    ? "The attribute set has not been specified"
                    : "The attribute set is required for 'GET/PUT/PATCH DocumentData' API methods");

            status = !string.IsNullOrEmpty(defaultConfiguration.ProcessingAction)
                ? ValidationResult.Valid
                : InstallingVerification ? ValidationResult.Error : ValidationResult.Warning;

            if (defaultConfiguration is IRedactionWebConfiguration)
            {
                validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The verify action is configured"
                : "The verify action has not been specified");
            }

            if (defaultConfiguration is IDocumentApiWebConfiguration)
            {
                validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The processing action is configured"
                : "The processing action is required for 'PUT/PATCH DocumentData' API methods");
            }

            status = !string.IsNullOrEmpty(defaultConfiguration.PostProcessingAction)
                  ? ValidationResult.Valid
                  : ValidationResult.Warning;

            if (defaultConfiguration is IRedactionWebConfiguration)
            {
                validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The post verify action is configured"
                : "The post verify action is required if post-verification processing is necessary (e.g. create a redacted copy of image)");
            }

            if (defaultConfiguration is IDocumentApiWebConfiguration)
            {
                validationResultsForm.AddMessage(status, status == ValidationResult.Valid
                ? "The post processing action is configured"
                : "The post processing action is required if 'PUT/PATCH DocumentData' API methods should trigger edit post-edit processing");
            }

            validationResultsForm.AddMessage(ValidationResult.Valid, $"This configuration is a default: {defaultConfiguration.IsDefault}");
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
