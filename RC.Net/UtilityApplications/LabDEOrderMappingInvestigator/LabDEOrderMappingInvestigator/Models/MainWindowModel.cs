namespace LabDEOrderMappingInvestigator.Models
{
    /// <summary>
    /// Serialized properties of the <see cref="ViewModels.MainWindowViewModel"/>
    /// </summary>
    /// <param name="ProjectPath">Folder containing a LabDE Solution folder</param>
    /// <param name="DocumentPath">Path of an image/PDF file</param>
    /// <param name="ExpectedDataPathTagFunction">Path tag function to transform the document path
    /// into the post-verify VOA file path</param>
    public record class MainWindowModel(
        string? ProjectPath,
        string? DocumentPath,
        string? ExpectedDataPathTagFunction);
}
