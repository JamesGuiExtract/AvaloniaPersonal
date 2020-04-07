using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Encapsulates the required functionality of an application hosting a
    /// <see cref="PageLayoutControl"/>.
    /// </summary>
    internal interface IPaginationUtility
    {
        /// <summary>
        /// Creates a new <see cref="OutputDocument"/> based on the specified
        /// <see paramref="originalDocName"/>.
        /// </summary>
        /// <param name="originalDocName">The name that should be used as the basis for the new
        /// document's filename.</param>
        /// <returns>The new <see cref="OutputDocument"/>.</returns>
        OutputDocument CreateOutputDocument(string originalDocName);

        /// <summary>
        /// Generates a new name an <see cref="OutputDocument"/> based on the specified
        /// <see paramref="originalDocName"/> that is unique compared to any pending output
        /// document, any existing document or any document that has been output by this instance
        /// whether or not the file still exists.
        /// </summary>
        /// <param name="originalDocName">The filename that should serve as the basis for the new
        /// document name.</param>
        /// <returns>The unique document name.</returns>
        string GenerateOutputDocumentName(string originalDocName);

        /// <summary>
        /// Gets the <see cref="Page"/> instance(s) that represent the page(s) of the specified
        /// <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">The image file for which <see cref="Page"/> instance(s) should be
        /// retrieved.</param>
        /// <param name="pageNumber">If <see langword="null"/>, all pages will be retrieved;
        /// otherwise only the specified page will be retrieved.</param>
        /// <returns>The <see cref="Page"/> instance(s) that represent the page(s) of the specified
        /// <see paramref="fileName"/>.</returns>
        IEnumerable<Page> GetDocumentPages(string fileName, int? pageNumber);

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a cut operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        ToolStripItem CutMenuItem { get; }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a copy operation or 
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        ToolStripItem CopyMenuItem { get; }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a paste operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        ToolStripItem PasteMenuItem { get; }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a delete operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        ToolStripItem DeleteMenuItem { get; }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger an un-delete operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        ToolStripItem UnDeleteMenuItem { get; }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a print operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        ToolStripItem PrintMenuItem { get; }

        /// <summary>
        /// A <see cref="ToolStripItem"/> that opens the data panel for editing.
        /// </summary>
        ToolStripItem EditDocumentDataMenuItem { get; }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to toggle a document separator ahead of the
        /// currently selected page or <see langword="null"/> if no such item is available.
        /// </summary>
        ToolStripItem ToggleDocumentSeparatorMenuItem { get; }

        /// <summary>
        /// A <see cref="ToolStripButton"/> intended to trigger output the currently selected
        /// document(s) or <see langword="null"/> if no such item is available.
        /// </summary>
        ToolStripButton OutputDocumentToolStripButton { get; }
	
        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger output the currently selected
        /// document(s) or <see langword="null"/> if no such item is available.
        /// </summary>
        ToolStripMenuItem OutputSelectedDocumentsMenuItem { get; }
    }
}
