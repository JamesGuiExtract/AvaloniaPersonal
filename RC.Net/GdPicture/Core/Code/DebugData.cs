using System.Collections.Generic;

namespace Extract.GdPicture
{
    public class DebugData
    {
        public Dictionary<string, string> AdditionalDebugData { get; } = new Dictionary<string, string>();

        public string? FilePath { get; }
        public int? PageNumber { get; }

        public DebugData(string? filePath = null, int? pageNumber = null)
        {
            FilePath = filePath;
            PageNumber = pageNumber;
        }

        public void AddDebugData(string key, string value)
        {
            AdditionalDebugData.Add(key, value);
        }
    }
}
