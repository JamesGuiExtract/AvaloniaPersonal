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
    /// Represents a <see cref="ToolStripButton"/> that enables the rectangular highlight 
    /// <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(RectangularHighlightToolStripButton), 
        ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_IMAGE)]
    public partial class RectangularHighlightToolStripButton : ImageViewerCursorToolStripButton
    {
        /// <summary>
        /// Initializes a new <see cref="RectangularHighlightToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public RectangularHighlightToolStripButton()
            : base(CursorTool.RectangularHighlight,
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_TEXT,
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_IMAGE, 
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_TOOL_TIP, 
            typeof(RectangularHighlightToolStripButton))
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

            return null;
        }
    }
}
