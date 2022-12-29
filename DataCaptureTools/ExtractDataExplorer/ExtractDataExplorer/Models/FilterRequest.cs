using Extract.Utilities;

namespace ExtractDataExplorer.Models
{
    /// <summary>
    /// Type of the query part of the filter
    /// </summary>
    public enum AttributeQueryType
    {
        /// <summary>
        /// Attribute Finder Query syntax (Extract System's XPath-like query syntax used in the RDT)
        /// </summary>
        AFQuery,

        /// <summary>
        /// XPath v1.0
        /// </summary>
        XPath
    }

    /// <summary>
    /// Filter properties
    /// </summary>
    public class FilterRequest
    {
        /// <summary>
        /// Optional query to filter by, instead of or in addition to the page range (further limit the results)
        /// </summary>
        public string? Query { get; }

        /// <summary>
        /// The type to interpret <see cref="Query"/> as
        /// </summary>
        public AttributeQueryType QueryType { get; }

        /// <summary>
        /// Whether to start XPath queries at /* instead of the default of /
        /// </summary>
        public bool StartAtElement { get; }

        /// <summary>
        /// Optional page range to filter by, instead of or in addition to the query (further limit the results)
        /// </summary>
        public string? PageRange { get; }

        /// <summary>
        /// Whether or not to actually use the page filter
        /// </summary>
        public bool IsPageFilterEnabled { get; }

        /// <summary>
        /// An empty filter (will match nothing so don't waste time perform any filtering)
        /// </summary>
        public static FilterRequest Empty { get; } =
            new(query: "",
                queryType: AttributeQueryType.AFQuery,
                startAtElement: true,
                pageRange: "",
                isPageFilterEnabled: false);

        /// <summary>
        /// Create a filter request
        /// </summary>
        public FilterRequest(
            string? query,
            AttributeQueryType queryType,
            bool startAtElement,
            string? pageRange,
            bool isPageFilterEnabled)
        {
            Query = query;
            QueryType = queryType;
            StartAtElement = startAtElement;
            PageRange = pageRange;
            IsPageFilterEnabled = isPageFilterEnabled;
        }

        /// <summary>
        /// Whether this filter can do anything meaningful
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Query) &&
                (!IsPageFilterEnabled || string.IsNullOrWhiteSpace(PageRange));
        }

        /// <summary>
        /// Structural equality
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is FilterRequest request &&
                Query == request.Query &&
                QueryType == request.QueryType &&
                StartAtElement == request.StartAtElement &&
                (IsPageFilterEnabled ? PageRange : "") ==
                    (request.IsPageFilterEnabled ? request.PageRange : "");
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(Query)
                .Hash(QueryType)
                .Hash(StartAtElement)
                .Hash(IsPageFilterEnabled ? PageRange : "");
        }

        /// <inheritdoc/>
        public static bool operator ==(FilterRequest? left, FilterRequest? right)
        {
            return left is null && right is null || left is not null && left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(FilterRequest? left, FilterRequest? right)
        {
            return !(left == right);
        }
    }
}
