namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    public static class Utils
    {
        /// Trim whitespace and double-quotes from a path
        public static string TrimPath(this string path)
        {
            return path?.Trim(' ', '\r', '\n', '\t', '"');
        }
    }
}
