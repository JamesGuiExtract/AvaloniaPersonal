using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// The event arguments for the <see cref="PaginationLayoutEngine.LayoutCompleted"/> event.
    /// </summary>
    internal class LayoutCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="redundantControls">The redundant <see cref="PaginationControl"/>s.</param>
        public LayoutCompletedEventArgs(params PaginationControl[] redundantControls)
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
        /// Gets the <see cref="PaginationControl"/>s that are now redundant and can be removed.
        /// </summary>
        public PaginationControl[] RedundantControls
        {
            get;
            private set;
        }
    }


    /// <summary>
    /// Event args for <see cref="PaginationPanel.CreatingOutputDocument"/>
    /// and <see cref="PaginationPanel.OutputDocumentCreated"/> events.
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
        /// <param name="rotatedPages">collection of original page number that has been rotated, AND associated 
        /// rotation amount in degrees from original orientation.
        public CreatingOutputDocumentEventArgs(IEnumerable<PageInfo> sourcePageInfo,
            int pageCount, long fileSize, bool? suggestedPaginationAccepted, int position,
            PaginationDocumentData documentData, ReadOnlyCollection<(string documentName, int page, int rotation)> rotatedPages,
            bool pagesEqualButRotated)
            : base()
        {
            SourcePageInfo = sourcePageInfo.ToList().AsReadOnly();
            PageCount = pageCount;
            FileSize = fileSize;
            SuggestedPaginationAccepted = suggestedPaginationAccepted;
            Position = position;
            DocumentData = documentData;
            RotatedPages = rotatedPages;
            PagesEqualButRotated = pagesEqualButRotated;
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
        public PaginationDocumentData DocumentData
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

        /// <summary>
        /// Contains a read-only collection of pages that have been rotated (ImageOrientation != 0)
        /// </summary>
        public ReadOnlyCollection<(string documentName, int page, int rotation)> RotatedPages
        {
            get;
            private set;
        }

        /// <summary>
        /// Contains a value that describes the condition when a set of document pages are original but for
        /// one or more of the pages having been rotated - true when this condition pertains.
        /// </summary>
        public bool PagesEqualButRotated
        {
            get;
            set;
        }

        /// <summary>
        /// Set after the output document is added to the database. Used by handlers of the
        /// <see cref="PaginationPanel.OutputDocumentCreated"/> event
        /// </summary>
        public int FileID
        {
            get;
            internal set;
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

        /// <summary>
        /// Whether the page is deleted
        /// </summary>
        public bool Deleted { get; set; }
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
        /// <see cref="PaginationDocumentData"/> where data was modified, but the document pages
        /// have not been modified compared to pagination on disk.</param>
        /// <param name="unmodifiedPaginationSources"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public PaginatedEventArgs(IEnumerable<string> paginatedDocumentSources,
            IEnumerable<string> disregardedPaginationSources,
            IEnumerable<KeyValuePair<string, PaginationDocumentData>> unmodifiedPaginationSources)
            : base()
        {
            PaginatedDocumentSources = paginatedDocumentSources.ToList().AsReadOnly();
            DisregardedPaginationSources = disregardedPaginationSources.ToList().AsReadOnly();
            UnmodifiedPaginationSources = unmodifiedPaginationSources.ToList().AsReadOnly();
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
        /// All documents applied as they exist on disk (including documents referenced in
        /// <see cref="DisregardedPaginationSources"/>).
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ReadOnlyCollection<KeyValuePair<string, PaginationDocumentData>> UnmodifiedPaginationSources
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
        /// Gets the names of the source documents of (documents that contributed pages to) the
        /// <see cref="OutputDocument"/> for which data is needed.
        /// </summary>
        public ReadOnlyCollection<string> SourceDocNames
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the <see cref="PaginationDocumentData"/> instance to use.
        /// </summary>
        public PaginationDocumentData DocumentData
        {
            get;
            set;
        }
    }

    /// <summary>
    /// The event arguments for the <see cref="PageLayoutControl.DocumentDataPanelRequest"/>
    /// event.
    /// </summary>
    internal class DocumentDataPanelRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDataRequestEventArgs"/> class.
        /// </summary>
        /// <param name="outputDocument"></param>
        public DocumentDataPanelRequestEventArgs(OutputDocument outputDocument)
        {
            try
            {
                OutputDocument = outputDocument;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40255");
            }
        }

        /// <summary>
        /// Gets the <see cref="OutputDocument"/> to which the requested
        /// <see cref="IPaginationDocumentDataPanel"/> is to relate.
        /// </summary>
        public OutputDocument OutputDocument
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the <see cref="IPaginationDocumentDataPanel"/> to use to display and edit
        /// document data.
        /// </summary>
        public IPaginationDocumentDataPanel DocumentDataPanel
        {
            get;
            set;
        }
    }

    /// <summary>
    /// The event arguments for the <see cref="PaginationPanel.CommittingChanges"/>
    /// event.
    /// </summary>
    public class CommittingChangesEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the
        /// <see cref="PaginationPanel.CommittingChanges"/> event was handled.
        /// </summary>
        /// <value><see langword="true"/> if the changes have been committed by the object receiving
        /// the event; otherwise, <see langword="false"/>.
        /// </value>
        public bool Handled
        {
            get;
            set;
        }
    }

    /// <summary>
    /// The event arguments for the <see cref="PaginationPanel.CommittingChanges"/>
    /// event.
    /// </summary>
    public class FileTaskSessionRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTaskSessionRequestEventArgs"/> class.
        /// </summary>
        /// <param name="fileID">The ID of the file for which the corresponding file task session ID
        /// is needed.</param>
        public FileTaskSessionRequestEventArgs(int fileID)
        {
            FileID = fileID;
        }

        /// <summary>
        /// Gets the ID of the file for which the corresponding file task session ID
        /// is needed.
        /// </summary>
        public int FileID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the corresponding file task session ID for the <see cref="FileID"/>.
        /// </summary>
        public int? FileTaskSessionID
        {
            get;
            set;
        }
    }

    /// <summary>
    /// The event arguments for the <see cref="PageLayoutControl.PageDeleted"/> event.
    /// </summary>
    public class PageLoadRequestEventArgs : HandledEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageLoadRequestEventArgs"/> class.
        /// </summary>
        /// <param name="pageNumber">The name of the souce document to be opened.</param>
        /// <param name="sourceDocName">The names of the source document(s) to which the document
        /// data would pertain.</param>
        public PageLoadRequestEventArgs(string sourceDocName, int pageNumber)
        {
            try
            {
                SourceDocName = sourceDocName;
                PageNumber = pageNumber;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI41338", ex);
            }
        }

        /// <summary>
        /// Gets the name of the souce document to be opened.
        /// </summary>
        public string SourceDocName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="Page"/> that was deleted.
        /// </summary>
        public int PageNumber
        {
            get;
            private set;
        }
    }
}
