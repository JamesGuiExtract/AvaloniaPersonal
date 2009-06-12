using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the rectangular
    /// highlight <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(RectangularHighlightToolStripMenuItem),
        ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_IMAGE)]
    public partial class RectangularHighlightToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region RectangularHighlightToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="RectangularHighlightToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public RectangularHighlightToolStripMenuItem()
            : base(CursorTool.RectangularHighlight,
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_MENU_ITEM_TEXT,
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_IMAGE,
            typeof(RectangularHighlightToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region RectangularHighlightToolStripMenuItem Events

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        /// <seealso cref="Control.OnClick"/>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                if (base.ImageViewer != null && base.ImageViewer.IsImageAvailable)
                {
                    base.ImageViewer.CursorTool = CursorTool.RectangularHighlight;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21430", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region RectangularHighlightToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectRectangularHighlightTool);
        }

        #endregion RectangularHighlightToolStripMenuItem Methods
    }
}
