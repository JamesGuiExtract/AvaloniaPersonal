using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
    /// The event arguments for the <see cref="PageLayoutControl.DocumentSplit"/> event.
    /// </summary>
    internal class DocumentSplitEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDataRequestEventArgs"/> class.
        /// </summary>
        /// <param name="originalDocument"></param>
        /// <param name="newDocument"></param>
        public DocumentSplitEventArgs(OutputDocument originalDocument, OutputDocument newDocument)
        {
            OriginalDocument = originalDocument;
            NewDocument = newDocument;
        }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public OutputDocument OriginalDocument
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public OutputDocument NewDocument
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// The event arguments for the <see cref="PageLayoutControl.DocumentsMerged"/> event.
    /// </summary>
    internal class DocumentsMergedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentsMergedEventArgs"/> class.
        /// </summary>
        /// <param name="resultingDocument"></param>
        /// <param name="discardedDocument"></param>
        public DocumentsMergedEventArgs(OutputDocument resultingDocument, OutputDocument discardedDocument)
        {
            ResultingDocument = resultingDocument;
            DiscardedDocument = discardedDocument;
        }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public OutputDocument ResultingDocument
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public OutputDocument DiscardedDocument
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
        /// <param name="sourcePageInfo"><see cref="PageInfo"/>s that indicate the source document
        /// page for each respective page in the output file that is being produced.</param>
        /// <param name="pageCount">The number of pages the output document will have.</param>
        /// <param name="fileSize">The size in bytes the output document will be.</param>
        /// <param name="suggestedPaginationAccepted"><see langword="true"/> if suggested pagination
        /// was accepted, <see langword="false"/> if suggested pagination was rejected or
        /// <see langword="null"/> if there was no suggested pagination for this document.</param>
        /// <param name="position">The position the document was in <see cref="PaginationPanel"/>.
        /// <para><b>Note</b></para>
        /// This is not a document index. The caller should not try to interpret this value;
        /// it's use should be limited to passing as the position argument of
        /// PaginationPanel.LoadFile.
        /// </param>
        /// <param name="documentData">Data that has been associated with the document.</param>
        public CreatingOutputDocumentEventArgs(IEnumerable<PageInfo> sourcePageInfo,
            int pageCount, long fileSize, bool? suggestedPaginationAccepted, int position,
            IDocumentData documentData)
            : base()
        {
            SourcePageInfo = sourcePageInfo.ToList().AsReadOnly();
            PageCount = pageCount;
            FileSize = fileSize;
            SuggestedPaginationAccepted = suggestedPaginationAccepted;
            Position = position;
            DocumentData = documentData;
        }

        /// <summary>
        /// The <see cref="PageInfo"/>s that indicate the source document page for each respective
        /// page in the output file that is being produced.
        /// </summary>
        public ReadOnlyCollection<PageInfo> SourcePageInfo
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
        /// Gets whether there was suggested pagination and whether it was accepted.
        /// </summary>
        /// <value><see langword="true"/> if suggested pagination was accepted,
        /// <see langword="false"/> if suggested pagination was rejected or <see langword="null"/>
        /// if there was no suggested pagination for this document.</value>
        public bool? SuggestedPaginationAccepted
        {
            get;
            private set;
        }

        /// <summary>
        /// The position the document was in <see cref="PaginationPanel"/>.
        /// <para><b>Note</b></para>
        /// This is not a document index. The caller should not try to interpret this value;
        /// it's use should be limited to passing as the position argument of
        /// PaginationPanel.LoadFile.
        /// </summary>
        public int Position
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets data that has been associated with the document data.
        /// </summary>
        public IDocumentData DocumentData
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
    /// Represents a specific page of a document.
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// The filename of the document.
        /// </summary>
        public string DocumentName
        {
            get;
            set;
        }

        /// <summary>
        /// The page number in the document.
        /// </summary>
        public int Page
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
        /// <param name="disregardedPaginationSources">All documents applied as they exist on disk
        /// but for which there was differing suggested pagination.</param>
        /// <param name="modifiedDocumentData">All documents names and associated
        /// <see cref="IDocumentData"/> where data was modified, but the document pages have not
        /// been modified compared to pagination on disk.</param>   
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public PaginatedEventArgs(IEnumerable<string> paginatedDocumentSources,
            IEnumerable<string> disregardedPaginationSources,
            IEnumerable<KeyValuePair<string, IDocumentData>> modifiedDocumentData)
            : base()
        {
            PaginatedDocumentSources = paginatedDocumentSources.ToList().AsReadOnly();
            DisregardedPaginationSources = disregardedPaginationSources.ToList().AsReadOnly();
            ModifiedDocumentData = modifiedDocumentData.ToList().AsReadOnly();
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

        /// <summary>
        /// All documents applied as they exist on disk but for which there was differing suggested
        /// pagination.
        /// </summary>
        public ReadOnlyCollection<string> DisregardedPaginationSources
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all documents names and associated <see cref="IDocumentData"/> where data was
        /// modified, but the document pages have not been modified compared to pagination on disk.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ReadOnlyCollection<KeyValuePair<string, IDocumentData>> ModifiedDocumentData
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// The event arguments for the <see cref="PaginationPanel.DocumentDataRequest"/> event.
    /// </summary>
    public class DocumentDataRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDataRequestEventArgs"/> class.
        /// </summary>
        /// <param name="sourceDocNames">The names of the source document(s) to which the document
        /// data would pertain.</param>
        public DocumentDataRequestEventArgs(params string[] sourceDocNames)
        {
            try
            {
                SourceDocNames = sourceDocNames.ToList().AsReadOnly();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39791");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyCollection<string> SourceDocNames
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the <see cref="OutputDocument"/> to be associated with the new document.
        /// </summary>
        public IDocumentData DocumentData
        {
            get;
            set;
        }
    }
}
