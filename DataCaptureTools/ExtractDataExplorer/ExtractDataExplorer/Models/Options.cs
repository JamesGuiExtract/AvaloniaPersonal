using CommandLine;

namespace ExtractDataExplorer.Models
{
    /// <summary>
    /// CLI parameters
    /// </summary>
    public class Options
    {
        [Option("voa-file", Required = false, HelpText = "Vector of attributes file (.voa, .evoa, .eav) to load")]
        public string? VoaFile { get; set; }

        [Value(0, Required = false, HelpText = "Input file to load, type determined by extension")]
        public string? InputFile { get; set; }
    }
}
