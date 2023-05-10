namespace ExtractDataExplorer.Services
{
    /// <summary>
    /// A service that can be used to figure out what image to show for an attribute.
    /// Since files can be moved after they were created it isn't enough to just use
    /// the SourceDocName that is stored in the attribute value (SpatialString)
    /// </summary>
    public interface ISourceDocNameResolver
    {
        /// <summary>
        /// Try to get a valid path to an image/pdf file in order to show an attribute's spatial data
        /// </summary>
        /// <param name="originalSourceDocName">The source doc name specified in the attribute value, e.g.</param>
        /// <param name="attributesFilePath">The path to the VOA file containing the attribute we are trying to show</param>
        /// <param name="sourceDocName">The resolved source doc name or the empty string if the attempt failed</param>
        /// <returns><c>true</c> if a source doc name was resolved, otherwise <c>false</c></returns>
        bool TryGetSourceDocName(string? originalSourceDocName, string? attributesFilePath, out string sourceDocName);
    }
}
