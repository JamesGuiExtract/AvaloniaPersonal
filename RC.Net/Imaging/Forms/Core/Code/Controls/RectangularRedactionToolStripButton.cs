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
    /// Represents a <see cref="ToolStripButton"/> that enables the rectangular redaction 
    /// <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(RectangularRedactionToolStripButton), 
        ToolStripButtonConstants._RECTANGULAR_REDACTION_BUTTON_IMAGE)]
    public partial class RectangularRedactionToolStripButton : ImageViewerCursorToolStripButton
    {
        /// <summary>
        /// Initializes a new <see cref="RectangularRedactionToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public RectangularRedactionToolStripButton()
            : base(CursorTool.RectangularRedaction,
            ToolStripButtonConstants._RECTANGULAR_REDACTION_BUTTON_TEXT,
            ToolStripButtonConstants._RECTANGULAR_REDACTION_BUTTON_IMAGE, 
            ToolStripButtonConstants._RECTANGULAR_REDACTION_BUTTON_TOOL_TIP, 
            typeof(RectangularRedactionToolStripButton))
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
