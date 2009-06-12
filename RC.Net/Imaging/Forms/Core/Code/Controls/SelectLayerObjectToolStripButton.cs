using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripButton"/> that enables the "Select object" 
    /// <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(SelectLayerObjectToolStripButton), 
        ToolStripButtonConstants._SELECT_LAYER_OBJECT_BUTTON_IMAGE)]
    public partial class SelectLayerObjectToolStripButton : ImageViewerCursorToolStripButton
    {
        #region SelectLayerObjectToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="SelectLayerObjectToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public SelectLayerObjectToolStripButton()
            : base(CursorTool.SelectLayerObject,
            ToolStripButtonConstants._SELECT_LAYER_OBJECT_BUTTON_TEXT,
            ToolStripButtonConstants._SELECT_LAYER_OBJECT_BUTTON_IMAGE,
            ToolStripButtonConstants._SELECT_LAYER_OBJECT_BUTTON_TOOL_TIP,
            typeof(SelectLayerObjectToolStripButton))
        {
            InitializeComponent();
        }

        #endregion SelectLayerObjectToolStripButton Constructors

        #region SelectLayerObjectToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectSelectLayerObjectsTool);
        }

        #endregion SelectLayerObjectToolStripButton Methods
    }
}
