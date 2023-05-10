namespace ExtractDataExplorer.Models
{
    public class DocumentModel
    {
        public string? DocumentPath { get; set; }

        public DocumentModel(string? documentPath)
        {
            DocumentPath = documentPath;
        }
    }
}
