namespace ExtractDataExplorer.Models
{
    /// <summary>
    /// Load file details
    /// </summary>
    public class LoadRequest
    {
        /// <summary>
        /// The file path to load
        /// </summary>
        public string? Path { get; }

        /// <summary>
        /// Create an instance
        /// </summary>
        public LoadRequest(string? path)
        {
            Path = path;
        }
    }
}
