using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.UtilityApplications.PaginationUtility
{
	partial class PaginationPanel
	{
        /// <summary>
        /// An extension of <see cref="OutputDocument"/> that contains some extensions for use in
        /// a <see cref="PaginationPanel"/>.
        /// </summary>
        class ExtendedOutputDocument : OutputDocument
        {
            /// <summary>
            /// A <see cref="PageStylist"/> that uses a red asterisk to indicated pages that have been
            /// "modified" (that are part of a pending <see cref="OutputDocument"/> that does not match
            /// the document as it currently exists on disk).
            /// </summary>
            static ModifiedPageStylist _MODIFIED_STYLIST = new ModifiedPageStylist();

            /// <summary>
            /// A <see cref="PageStylist"/> that indicates a document page that is part of the only
            /// <see cref="OutputDocument"/> for which pages are currently selected.
            /// </summary>
            static SelectedDocumentStylist _SELECTED_DOC_STYLIST = new SelectedDocumentStylist();

            /// <summary>
            /// Indicates whether singly selected documents (one and only one document that contains
            /// all of the currently selected pages) should be indicated with a blue background.
            /// </summary>
            bool _highlightSinglySelectedDocument;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExtendedOutputDocument"/> class.
            /// </summary>
            /// <param name="fileName">The</param>
            /// <param name="highlightSinglySelectedDocument">Indicates whether singly selected
            /// documents (one and only one document that contains all of the currently selected
            /// pages) should be indicated with a blue background.</param>
            public ExtendedOutputDocument(string fileName, bool highlightSinglySelectedDocument)
                : base(fileName)
            {
                _highlightSinglySelectedDocument = highlightSinglySelectedDocument;
            }

            /// <summary>
            /// Gets or sets a value indicating the there was pagination suggested for the current
            /// document.
            /// </summary>
            /// <value> <see langword="true"/> if pagination was suggested for the current document;
            /// otherwise, <see langword="false"/>.
            /// </value>
            public bool PaginationSuggested
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets VOA data that is associated with this instance.
            /// </summary>
            /// <value>
            /// The VOA data that is associated with this instance.
            /// </value>
            public object DocumentData
            {
                get;
                set;
            }

            /// <summary>
            /// Adds the specified <see paramref="pageControl"/> as the last page of the document.
            /// </summary>
            /// <param name="pageControl">The <see cref="PageThumbnailControl"/> representing the
            /// page to be added.</param>
            public override void AddPage(PageThumbnailControl pageControl)
            {
                try
                {
                    base.AddPage(pageControl);

                    pageControl.AddStylist(_MODIFIED_STYLIST);
                    if (_highlightSinglySelectedDocument)
                    {
                        pageControl.AddStylist(_SELECTED_DOC_STYLIST);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39667");
                }
            }

            /// <summary>
            /// Inserts the specified <see paramref="pageControl"/> as <see paramref="pageNumber"/>
            /// of the document.
            /// </summary>
            /// <param name="pageControl">The <see cref="PageThumbnailControl"/> representing the
            /// page to be inserted.</param>
            /// <param name="pageNumber">The page number the new page should be inserted at.</param>
            public override void InsertPage(PageThumbnailControl pageControl, int pageNumber)
            {
                try
                {
                    base.InsertPage(pageControl, pageNumber);

                    pageControl.AddStylist(_MODIFIED_STYLIST);
                    if (_highlightSinglySelectedDocument)
                    {
                        pageControl.AddStylist(_SELECTED_DOC_STYLIST);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39668");
                }
            }
        }
	}
}
