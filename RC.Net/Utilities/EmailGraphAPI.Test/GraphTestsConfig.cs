using Extract.Utilities;
using System.Net;
using System.Security;

namespace Extract.Email.GraphClient.Test
{
    public class GraphTestsConfig
    {
        public string DatabaseServer { get; set; } = "(local)";
        public string DatabaseName { get; set; } = "TestGraphAPI";
        public string EmailUserName { get; set; } = "email_test@extractsystems.com";
        public string EmailPassword { get; set; } = "an.Ass5.hogs.a.mimic";
        public string AzureClientId { get; set; } = "6311c46a-18a8-4f8c-9702-e0d9b02eb7d2";
        public string AzureTenantID { get; set; } = "bd07e2c0-7f9a-478c-a4f2-0d3865717565";
        public string AzureInstance { get; set; } = "https://login.microsoftonline.com";
        public string SharedEmailAddress { get; set; } = "emailsuppliertest@extractsystems.com";
        public string FolderToSaveEmails { get; set; } = FileSystemMethods.GetTemporaryFolder().FullName;
        public bool SupplyTestEmails { get; set; } = true;
    }
}
