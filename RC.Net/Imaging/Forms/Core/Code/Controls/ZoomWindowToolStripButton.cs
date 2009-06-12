using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripButton"/> that enables the Zoom window
    /// <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(ZoomWindowToolStripButton), 
        ToolStripButtonConstants._ZOOM_WINDOW_BUTTON_IMAGE)]
    public partial class ZoomWindowToolStripButton : ImageViewerCursorToolStripButton
    {
        /// <summary>
        /// Initializes a new <see cref="ZoomWindowToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public ZoomWindowToolStripButton()
            : base(CursorTool.ZoomWindow, 
            ToolStripButtonConstants._ZOOM_WINDOW_BUTTON_TEXT,
            ToolStripButtonConstants._ZOOM_WINDOW_BUTTON_IMAGE, 
            ToolStripButtonConstants._ZOOM_WINDOW_BUTTON_TOOL_TIP, 
            typeof(ZoomWindowToolStripButton))
        {
            // Initialize the component
            InitializeComponent();
        }

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ?
                null : base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectZoomWindowTool);
        }
    }
}
