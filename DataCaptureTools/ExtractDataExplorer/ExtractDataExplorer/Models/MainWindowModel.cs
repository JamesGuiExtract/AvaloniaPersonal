namespace ExtractDataExplorer.Models
{
    /// <summary>
    /// A model for the main window, used to persist the last-used settings
    /// </summary>
    public class MainWindowModel
    {
        public bool DarkMode { get; set; }

        public string? AttributesFilePath { get; set; }

        public FilterRequest AttributeFilter { get; set; }

        public bool IsFilterApplied { get; }

        public DocumentModel? DocumentModel { get; set; }

        public MainWindowModel(
            bool darkMode,
            string? attributesFilePath,
            FilterRequest attributeFilter,
            bool isFilterApplied,
            DocumentModel? documentModel)
        {
            DarkMode = darkMode;
            AttributesFilePath = attributesFilePath;
            AttributeFilter = attributeFilter;
            IsFilterApplied = isFilterApplied;
            DocumentModel = documentModel;
        }
    }
}