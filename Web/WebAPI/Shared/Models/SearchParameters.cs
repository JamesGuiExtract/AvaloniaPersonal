namespace WebAPI
{
    /// <summary>
    /// Type of search
    /// </summary>
    public enum QueryType
    {
        /// <summary>
        /// Query interpreted as plain-text
        /// </summary>
        Literal = 0,

        /// <summary>
        /// Query interpreted as a regular expression
        /// </summary>
        Regex = 1,
    }

    /// <summary>
    /// Parmeter object for performing a search
    /// </summary>
    public class SearchParameters
    {
        /// <summary>
        /// The pattern to search for
        /// </summary>
        public string Query;

        /// <summary>
        /// The type of search pattern
        /// </summary>
        public QueryType QueryType;

        /// <summary>
        /// Whether the pattern must match letter case
        /// </summary>
        public bool CaseSensitive;

        /// <summary>
        /// The page number to search on, null for all pages
        /// </summary>
        public int? PageNumber;

        /// <summary>
        /// The type to set for the resulting attributes (e.g., SSN)
        /// </summary>
        public string ResultType;
    }
}
