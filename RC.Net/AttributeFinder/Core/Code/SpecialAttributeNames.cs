namespace Extract.AttributeFinder
{
    public class SpecialAttributeNames
    {
        #region Pagination

        /// <summary>
        /// The top-level attribute in the pagination schema
        /// </summary>
        public static readonly string Document = "Document";

        /// <summary>
        /// The name of page attributes (e.g., that have pagination protofeature subattributes)
        /// </summary>
        public static readonly string Page = "Page";

        /// <summary>
        /// Contains one or more page ranges that describe the Document. Includes 'deleted' pages
        /// </summary>
        public static readonly string Pages = "Pages";

        /// <summary>
        /// Contains one or more page ranges that describe the Document's 'deleted' pages
        /// </summary>
        public static readonly string DeletedPages = "DeletedPages";

        /// <summary>
        /// Flag set by the auto paginate task (value of "True" or "False") to be used for computing accuracy stats
        /// </summary>
        public static readonly string QualifiedForAutomaticOutput = "QualifiedForAutomaticOutput";

        /// <summary>
        /// The parent node for all 'real' data
        /// </summary>
        public static readonly string DocumentData = "DocumentData";

        /// <summary>
        /// The name of the flag attributes used to indicate a situation that won't work for pagination training,
        /// e.g., rearranged or duplicated pages
        /// https://extract.atlassian.net/browse/ISSUE-14923
        /// </summary>
        public static readonly string IncompatibleWithPaginationTraining = "IncompatibleWithPaginationTraining";

        #endregion
    }
}
