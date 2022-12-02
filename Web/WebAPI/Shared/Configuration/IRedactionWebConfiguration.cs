using System.Collections.Generic;

namespace WebAPI.Configuration
{
    public interface IRedactionWebConfiguration : IWebConfiguration
    {
        public string ActiveDirectoryGroup { get; set; }
        public IList<string> RedactionTypes { get; set; }
        public bool EnableAllUserPendingQueue { get; set; }
        public string DocumentTypeFileLocation { get; set; }
    }
}
