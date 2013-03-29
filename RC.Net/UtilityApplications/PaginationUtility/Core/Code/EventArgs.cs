using System;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// The event arguments for the <see cref="PageLayoutControl.PageDeleted"/> event.
    /// </summary>
    internal class PageDeletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageDeletedEventArgs"/> class.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> that was deleted.</param>
        /// <param name="outputDocument">Gets the <see cref="OutputDocument"/> the
        /// <see paramref="page"/> was deleted from.</param>
        public PageDeletedEventArgs(Page page, OutputDocument outputDocument)
        {
            try
            {
                Page = page;
                OutputDocument = outputDocument;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI35422", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="Page"/> that was deleted.
        /// </summary>
        public Page Page
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="OutputDocument"/> the <see cref="Page"/> was deleted from.
        /// </summary>
        public OutputDocument OutputDocument
        {
            get;
            private set;
        }
    }
}
