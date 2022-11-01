using LabDEOrderMappingInvestigator.Services;

namespace LabDEOrderMappingInvestigator.Models
{
    /// <summary>
    /// Arguments for the <see cref="AnalysisService"/>
    /// </summary>
    /// <param name="CustomerOMDBPath">Path to the customers order mapping database file</param>
    /// <param name="ExtractOMDBPath">Path the the URS order mapping database (part of the FKB)</param>
    /// <param name="SourceDocName">Path to the image file being investigated</param>
    /// <param name="ExpectedDataPath">Path to the post-verification VOA file</param>
    /// <param name="FoundDataPath">Path to the rules output VOA file</param>
    /// <param name="InitialLabTestFilter">Optional filter to use to limit the tests shown in the view</param>
    public record AnalyzeESComponentMapArgs(
        string CustomerOMDBPath,
        string ExtractOMDBPath,
        string SourceDocName,
        string ExpectedDataPath,
        string FoundDataPath,
        LabTestFilter? InitialLabTestFilter);
}
