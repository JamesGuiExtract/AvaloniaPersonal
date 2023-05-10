using ExtractDataExplorer.Models;

namespace ExtractDataExplorer.Services
{
    public interface IExpandPathTagsService
    {
        string? ExpandPathTags(string? sourceDocName, string? pathToExpand);
    }
}