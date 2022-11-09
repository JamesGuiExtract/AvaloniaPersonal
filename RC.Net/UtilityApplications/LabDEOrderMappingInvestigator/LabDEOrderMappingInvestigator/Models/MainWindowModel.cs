using Avalonia.Controls;

namespace LabDEOrderMappingInvestigator.Models
{
    /// <summary>
    /// Serialized properties of the <see cref="ViewModels.MainWindowViewModel"/>
    /// </summary>
    /// <param name="SolutionPath">Folder containing LabDE Database Files and Rules folders</param>
    /// <param name="DocumentPath">Path of an image/PDF file</param>
    /// <param name="ExpectedDataPathTagFunction">Path tag function to transform the document path
    /// into the post-verify VOA file path</param>
    /// <param name="FoundDataPathTagFunction">Path tag function to transform the document path
    /// into the pre-verify (rules output) VOA file path</param>
    /// <param name="MappingSuggestionsLabTestFilter">The filter used for the <see cref="ViewModels.MappingSuggestionsOutputMessageViewModel"/></param>
    /// <param name="Width">The window's width</param>
    /// <param name="Height">The window's height</param>
    /// <param name="WindowState">The window state (maximized/normal)</param>
    public record class MainWindowModel(
        string? SolutionPath,
        string? DocumentPath,
        string? ExpectedDataPathTagFunction,
        string? FoundDataPathTagFunction,
        LabTestFilter? MappingSuggestionsLabTestFilter,
        double? Width,
        double? Height,
        WindowState? WindowState);
}
