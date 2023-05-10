using System.IO;
using static ExtractDataExplorer.Utils;

namespace ExtractDataExplorer.Services
{
    /// <inheritdoc/>
    public class SourceDocNameResolver : ISourceDocNameResolver
    {
        /// <inheritdoc/>
        public bool TryGetSourceDocName(string? originalSourceDocName, string? attributesFilePath, out string sourceDocName)
        {
            // If the source doc name specified in the spatial string exists then use that
            if (TryGetFromOriginalSourceDoc(originalSourceDocName, out sourceDocName))
            {
                return true;
            }

            // Else try to derive the source doc name from the VOA file path
            if (attributesFilePath is null)
            {
                sourceDocName = "";
                return false;
            }

            try
            {
                // Remove extensions from the VOA file path until it matches an image/pdf file
                string previousCandidate = attributesFilePath;
                string candidate = Path.ChangeExtension(previousCandidate, null);

                while (candidate != previousCandidate)
                {
                    if (File.Exists(candidate) && IsImageFile(candidate))
                    {
                        sourceDocName = candidate;
                        return true;
                    }

                    previousCandidate = candidate;
                    candidate = Path.ChangeExtension(previousCandidate, null);
                }

                sourceDocName = "";
                return false;
            }
            catch
            {
                sourceDocName = "";
                return false;
            }
        }

        // Does the specified file exist?
        private static bool TryGetFromOriginalSourceDoc(string? originalSourceDocName, out string sourceDocName)
        {
            try
            {
                if (originalSourceDocName is not null
                    && File.Exists(originalSourceDocName))
                {
                    sourceDocName = originalSourceDocName;
                    return true;
                }

                sourceDocName = "";
                return false;
            }
            catch
            {
                sourceDocName = "";
                return false;
            }
        }
    }
}
