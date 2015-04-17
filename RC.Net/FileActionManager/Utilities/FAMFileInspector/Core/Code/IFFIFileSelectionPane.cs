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
        /// The IDs of the files to be populated in the FFI file list.
        /// </summary>
        IEnumerable<int> SelectedFileIds
        {
            get;
        }
    }
}
