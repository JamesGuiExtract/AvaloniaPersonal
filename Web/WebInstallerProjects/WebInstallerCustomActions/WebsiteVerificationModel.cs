using Microsoft.Deployment.WindowsInstaller;
using System;
using System.IO;

namespace WebInstallerCustomActions
{
    class WebsiteVerificationModel
    {
        public WebsiteVerificationModel(Session session)
        {
            this.Session = session;
        }
        public Session Session { get; set; }
        public bool WorkflowExists { get; set; } = false;
        public string startActionID { get; set; }
        public string EndActionID { get; set; }
        public string documentFolder { get; set; }
        public string outputAttributeSetID { get; set; }
        public string OutputFileMetadataFieldID { get; set; }
        public string EditActionID { get; set; }
        public string PostEditActionID { get; set; }
        public string settings { get; set; }

        public string GetWarnings()
        {
            string warning = string.Empty;
            if (!this.WorkflowExists)
            {
                warning += "\nThe workflow does not exist in the database.";
            }
            else
            {
                warning = string.IsNullOrEmpty(documentFolder) ? " Workflow input folder is invalid" : $"\n{(HasPermissionsToFolder(this.documentFolder) ? ((char)0x221A).ToString() : "X")}  Workflow input folder access";
                warning = string.IsNullOrEmpty(startActionID) ? warning + "\nWorkflow configuration: The start action is required for the DocumentAPI" : warning;
                warning = string.IsNullOrEmpty(EndActionID) ? warning + "\nWorkflow configuration: The end action is required for the DocumentAPI" : warning;
                warning = string.IsNullOrEmpty(documentFolder) ? warning + "\nWorkflow configuration: The document folder is required for the DocumentAPI" : warning;
                warning = string.IsNullOrEmpty(outputAttributeSetID) ? warning + "\nWorkflow configuration: The output attribute set is needed if 'get OutputFile' is to be called (e.g. to get a redacted copy of the document)" : warning;
                warning = string.IsNullOrEmpty(OutputFileMetadataFieldID) ? warning + "\nWorkflow configuration: The File metadata field is required to call the output action" : warning;
                warning = string.IsNullOrEmpty(EditActionID) ? warning + "\nWorkflow configuration: The verify action needs to be set for verification." : warning;
                warning = string.IsNullOrEmpty(PostEditActionID) ? warning + "\nWorkflow configuration: The post edit action is needed to re-generate output files" : warning;
                warning = string.IsNullOrEmpty(settings) && Session["CREATE_VERIFY_SITE"].Equals("1", StringComparison.OrdinalIgnoreCase) ? warning + "\nWorkflow configuration: The redaction verification settings have not been set" : warning;
            }
            
            return warning;
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
}
