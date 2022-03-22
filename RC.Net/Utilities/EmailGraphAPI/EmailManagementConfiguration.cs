using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient
{
    public class EmailManagementConfiguration
    {
        public string ExternalLoginDescription { get; set; }
        public FileProcessingDB FileProcessingDB { get; set; }
        public string SharedEmailAddress { get; set; }
        public string QueuedMailFolderName { get; set; }
        public string InputMailFolderName { get; set; }
        public string FilepathToDownloadEmails { get; set; }

        public EmailManagementConfiguration ShallowCopy()
        {
            return (EmailManagementConfiguration)MemberwiseClone();
        }
    }
}