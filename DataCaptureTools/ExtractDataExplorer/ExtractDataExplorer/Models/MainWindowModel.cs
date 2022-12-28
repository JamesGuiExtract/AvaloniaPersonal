namespace ExtractDataExplorer.Models
{
    public class MainWindowModel
    {
        public bool DarkMode { get; set; }

        public string? AttributesFilePath { get; set; }

        public MainWindowModel(bool darkMode, string? attributesFilePath)
        {
            DarkMode = darkMode;
            AttributesFilePath = attributesFilePath;
        }
    }
}