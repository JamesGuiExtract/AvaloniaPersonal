using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Defines the possible positions for a <see cref="IFFIFileSelectionPane"/> in the FFI.
    /// </summary>
    public enum SelectionPanePosition
    {
        /// <summary>
        /// The pane will appear in the same spot the file selection pane it is replacing normally
        /// appears.
        /// </summary>
        Default,

        /// <summary>
        /// The pane will appear across the top of the FFI.
        /// </summary>
        Top
    }

    /// <summary>
    /// Defines a custom pane that is used to define the current file selection in the FFI.
    /// </summary>
    public interface IFFIFileSelectionPane
    {
        /// <summary>
        /// Raised when the file list indicated by the selection pane has changed.
        /// </summary>
        event EventHandler<EventArgs> RefreshRequired;

        /// <summary>
        /// Gets the title of the pane.
        /// </summary>
        string Title
        {
            get;
        }

        /// <summary>
        /// The position of the pane in the FFI.
        /// </summary>
        SelectionPanePosition PanePosition
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="Control"/> that is to be added into the FFI.
        /// </summary>
        Control Control
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether FFI menu main and context menu options should be limited
        /// to basic non-custom options. The main database menu and custom file handlers context
        /// menu options will not be shown.
        /// </summary>
        /// <value><see langword="true"/> to limit menu options to basic options only; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool BasicMenuOptionsOnly
        {
            get;
        }

        /// <summary>
        /// The IDs of the files to be populated in the FFI file list.
        /// </summary>
        IEnumerable<int> SelectedFileIds
        {
            get;
        }

        /// <summary>
        /// Gets or sets the accept action to be run when this instance needs to trigger, e.g., its
        /// parent form to close as if the accept button were clicked.
        /// https://extract.atlassian.net/browse/ISSUE-14308
        /// </summary>
        Action<object, EventArgs> AcceptFunction { get; set; }
    }
}
