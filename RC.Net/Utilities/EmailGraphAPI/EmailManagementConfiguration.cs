using System.Security;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Utilities.EmailGraphApi
{
    public class EmailManagementConfiguration
    {
        public string UserName { get; set; }
        public SecureString Password { get; set; }
        public FileProcessingDB FileProcessingDB { get; set; }
        public string SharedEmailAddress { get; set; }
        public string QueuedMailFolderName { get; set; }
        public string InputMailFolderName { get; set; }
        public string Authority { get; set; }
    }
}