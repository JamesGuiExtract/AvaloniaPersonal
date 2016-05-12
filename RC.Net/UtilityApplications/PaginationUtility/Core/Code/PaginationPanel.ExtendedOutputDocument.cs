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
            /// A <see cref="PageStylist"/> that uses a red asterisk to indicated pages that have been
            /// "modified" (that are part of a pending <see cref="OutputDocument"/> that does not match
            /// the document as it currently exists on disk).
            /// </summary>
            static NewOutputPageStylist _NEW_OUTPUT_STYLIST = new NewOutputPageStylist();

            /// <summary>
            /// A <see cref="PageStylist"/> that indicates document data has been modified via a
            /// pencil glyph.
            /// </summary>
            static EditedDocumentPageStylist _EDITED_STYLIST = new EditedDocumentPageStylist();

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
            /// The document data that is associated with this instance (VOA file data, for instance).
            /// </summary>
            IDocumentData _documentData;

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
            /// Raised when the <see cref="DocumentData"/> for this instance is modified.
            /// </summary>
            public event EventHandler<EventArgs> DocumentDataChanged;

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
            /// Gets or sets document data that is associated with this instance (VOA file data,
            /// for instance).
            /// </summary>
            /// <value>
            /// The document data that is associated with this instance.
            /// </value>
            public IDocumentData DocumentData
            {
                get
                {
                    return _documentData;
                }

                set
                {
                    try
                    {
                        if (value != _documentData)
                        {
                            if (_documentData != null)
                            {
                                _documentData.ModifiedChanged -= HandleDocumentData_ModifiedChanged;
                            }

                            _documentData = value;

                            if (_documentData != null)
                            {
                                _documentData.ModifiedChanged += HandleDocumentData_ModifiedChanged;
                            }

                            OnDocumentDataChanged();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex.AsExtract("ELI39795");
                    }
                }
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
                    pageControl.AddStylist(_NEW_OUTPUT_STYLIST);
                    pageControl.AddStylist(_EDITED_STYLIST);
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
                    pageControl.AddStylist(_NEW_OUTPUT_STYLIST);
                    pageControl.AddStylist(_EDITED_STYLIST);
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

            /// <summary>
            /// Handles the <see cref="IDocumentData.ModifiedChanged"/> event for
            /// <see cref="DocumentData"/>.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
            /// </param>
            void HandleDocumentData_ModifiedChanged(object sender, EventArgs e)
            {
                try
                {
                    OnDocumentDataChanged();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39796");
                }
            }

            /// <summary>
            /// Raises the <see cref="DocumentDataChanged"/> event.
            /// </summary>
            void OnDocumentDataChanged()
            {
                var eventHandler = DocumentDataChanged;
                if (eventHandler != null)
                {
                    eventHandler(this, new EventArgs());
                }
            }
        }
	}
}
