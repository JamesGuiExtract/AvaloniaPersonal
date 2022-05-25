using System.Collections.Generic;
using System.Linq;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Container to represent the original file and page numbers that make up part of an output document
    /// </summary>
    public class SourceDocumentPages
    {
        readonly string _sourceDocName;
        readonly List<int> _pageNumbers;

        /// <summary>
        /// Create from a list of <see cref="Page"/>s
        /// </summary>
        /// <param name="sourceDocName">The original document path</param>
        /// <param name="pages">The original page numbers that are part of the output document</param>
        internal SourceDocumentPages(string sourceDocName, IEnumerable<Page> pages)
        {
            _sourceDocName = sourceDocName;
            _pageNumbers = pages.Select(page => page.OriginalPageNumber).ToList();
        }

        /// <summary>
        /// The original document path
        /// </summary>
        public string SourceDocName => _sourceDocName;

        /// <summary>
        /// The original page numbers that are part of the output document
        /// </summary>
        /// <remarks>
        /// Pages are in output order. May contain duplicate numbers
        /// </remarks>
        public IList<int> PageNumbers => _pageNumbers;
    }
}
