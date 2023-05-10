using Extract.FileConverter;

namespace ExtractDataExplorer
{
    internal static class Utils
    {
        /// <summary>
        /// Trim whitespace and double-quotes from a path.
        /// Returns null if the result is empty.
        /// </summary>
        internal static string? TrimPath(string? path)
        {
            string? trimmed = path?.Trim(' ', '\r', '\n', '\t', '"');
            if (string.IsNullOrEmpty(trimmed))
            {
                return null;
            }

            return trimmed;
        }

        /// <summary>
        /// True if the specified path appears to be an image or pdf file
        /// </summary>
        internal static bool IsImageFile(string filePath)
        {
            FilePathHolder filePathHolder = FilePathHolder.Create(filePath);

            return filePathHolder.FileType switch
            {
                FileType.Image or FileType.Pdf => true,
                _ => false
            };
        }
    }
}
