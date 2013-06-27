using System;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="QueryNode"/> consisting of literal text and, optionally, extract systems
    /// path tags.
    /// </summary>
    internal class LiteralQueryNode : QueryNode
    {
        /// <summary>
        /// Initializes a new <see cref="LiteralQueryNode"/> instance.
        /// </summary>
        /// <param name="query">The literal text to be returned during evaluation.</param>
        public LiteralQueryNode(string query)
            : base()
        {
            QueryText = query;

            // A literal query node is the only node type that should default to not being
            // parameterized in an SQL query or expression (since the text would almost certainly
            // be a core part the SQL query or expression rather than a variable).
            Parameterize = false;
        }

        /// <summary>
        /// Evaluates the query.
        /// </summary>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        public override QueryResult Evaluate()
        {
            try
            {
                // [DataEntry:858]
                // Do not unnecessarily call SourceDocumentPathTags.Expand()-- this method is called a
                // lot and SourceDocumentPathTags.Expand() uses an expensive COM call.
                bool containsPossiblePathTag = (QueryText.Contains("$") ||
                    QueryText.IndexOf("<SourceDocName>", StringComparison.Ordinal) >= 0);
                string expandedQuery = containsPossiblePathTag ?
                    AttributeStatusInfo.PathTags.Expand(QueryText) : QueryText;

                // Treat separate lines as separate values.
                string[] parsedQuery =
                    expandedQuery.Split(new string[] { Environment.NewLine },
                    StringSplitOptions.RemoveEmptyEntries);

                return new QueryResult(this, parsedQuery);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI34484", ex);
                ee.AddDebugData("Query node type", GetType().Name, false);
                ee.AddDebugData("Query", QueryText ?? "null", false);
                throw ee;
            }
        }
    }
}
