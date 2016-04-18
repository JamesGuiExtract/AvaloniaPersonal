using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

    /// <summary>
    /// The event arguments for the <see cref="PageLayoutControl.PagesPendingLoad"/> event.
    /// </summary>
    internal class PagesPendingLoadEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagesPendingLoadEventArgs"/> class.
        /// </summary>
        /// <param name="pages">The <see cref="Page"/>s are pending to be loaded.</param>
        public PagesPendingLoadEventArgs(Page[] pages)
        {
            try
            {
                Pages = new ReadOnlyCollection<Page>(pages);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI35669", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="Page"/>s are pending to be loaded.
        /// </summary>
        public ReadOnlyCollection<Page> Pages
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// The event arguments for the <see cref="PageLayoutControl.PagesDereferenced"/> event.
    /// </summary>
    internal class PagesDereferencedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagesDereferencedEventArgs"/> class.
        /// </summary>
        /// <param name="pages">The <see cref="Page"/>s are being dereferenced.</param>
        public PagesDereferencedEventArgs(Page[] pages)
        {
            try
            {
                Pages = new ReadOnlyCollection<Page>(pages);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI39511", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="Page"/>s are being dereferenced.
        /// </summary>
        public ReadOnlyCollection<Page> Pages
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// The event arguments for the <see cref="PaginationLayoutEngine.RedundantControlsFound"/> event.
    /// </summary>
    internal class RedundantControlsFoundEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedundantControlsFoundEventArgs"/> class.
        /// </summary>
        /// <param name="redundantControls">The redundant <see cref="PaginationControl"/>s.</param>
        public RedundantControlsFoundEventArgs(params PaginationControl[] redundantControls)
        {
            try
            {
                RedundantControls = redundantControls;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI35652", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="PaginationControl"/>s.
        /// </summary>
        public PaginationControl[] RedundantControls
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Event args for a <see cref="PaginationPanel.CreatingOutputDocument"/> event.
    /// </summary>
    public class CreatingOutputDocumentEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new <see cref="CreatingOutputDocumentEventArgs"/> instance.
        /// </summary>
        /// <param name="sourceDocumentNames">The source documents from which the pages of the output
        /// document originate.</param>
        /// <param name="pageCount">The number of pages the output document will have.</param>
        /// <param name="fileSize">The size in bytes the output document will be.</param>
        public CreatingOutputDocumentEventArgs(IEnumerable<string> sourceDocumentNames,
            int pageCount, long fileSize)
            : base()
        {
            SourceDocumentNames = sourceDocumentNames.ToList().AsReadOnly();
            PageCount = pageCount;
            FileSize = fileSize;
        }

        /// <summary>
        /// The source documents from which the pages of the output document originate.
        /// </summary>
        public ReadOnlyCollection<string> SourceDocumentNames
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of pages the output document will have.
        /// </summary>
        public int PageCount
        {
            get;
            private set;
        }

        /// <summary>
        /// The size in bytes the output document will be.
        /// </summary>
        public long FileSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the file name to which the output document will be written.
        /// </summary>
        public string OutputFileName
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Event args for a <see cref="PaginationPanel.Paginated"/> event.
    /// </summary>
    public class PaginatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new <see cref="PaginatedEventArgs"/> instance.
        /// </summary>
        /// <param name="paginatedDocumentSources">The source documents that were used to generate
        /// the paginated output. These documents will no longer be referenced by the
        /// <see cref="PaginationPanel"/>.</param>
        public PaginatedEventArgs(IEnumerable<string> paginatedDocumentSources)
            : base()
        {
            PaginatedDocumentSources = paginatedDocumentSources.ToList().AsReadOnly();
        }

        /// <summary>
        /// The source documents that were used to generate the paginated output. These documents
        /// will no longer be referenced by the <see cref="PaginationPanel"/>
        /// </summary>
        public ReadOnlyCollection<string> PaginatedDocumentSources
        {
            get;
            private set;
        }
    }
}
