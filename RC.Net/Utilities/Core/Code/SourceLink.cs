using System.IO;

namespace Extract.Utilities
{
    /// <summary>
    /// Class used to manage a list of Source items that can be files or in the database
    /// such as Reports and Dashboards
    /// </summary>
    public class SourceLink
    {
        public SourceLink(string sourceName, bool isFile)
        {
            SourceName = sourceName;
            IsFile = isFile;

            if (isFile)
                DisplayName = Path.GetFileNameWithoutExtension(SourceName);
            else
                DisplayName = SourceName;
        }

        public string DisplayName { get; }

        public string SourceName { get; }

        public bool IsFile { get; }

        public string CategoryName { get; set; }
    }
}
