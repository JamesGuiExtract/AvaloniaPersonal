using Microsoft.Deployment.WindowsInstaller;
using System;

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
                warning = string.IsNullOrEmpty(startActionID) ? warning + "\nThe start action is required for the DocumentAPI" : warning;
                warning = string.IsNullOrEmpty(EndActionID) ? warning + "\nThe end action is required for the DocumentAPI" : warning;
                warning = string.IsNullOrEmpty(documentFolder) ? warning + "\nThe document folder is required for the DocumentAPI" : warning;
                warning = string.IsNullOrEmpty(outputAttributeSetID) ? warning + "\nThe output attribute set is needed for creating redactions" : warning;
                warning = string.IsNullOrEmpty(OutputFileMetadataFieldID) ? warning + "\nThe File metadata field is required to call the output action" : warning;
                warning = string.IsNullOrEmpty(EditActionID) ? warning + "\nThe verify action needs to be set for verification." : warning;
                warning = string.IsNullOrEmpty(PostEditActionID) ? warning + "\nThe post edit action is needed to re-generate output files" : warning;
                warning = string.IsNullOrEmpty(settings) && Session["CREATE_VERIFY_SITE"].Equals("1", StringComparison.OrdinalIgnoreCase) ? warning + "\nThe redaction verification settings have not been set" : warning;
            }
            
            return warning;
        }
    }
}
