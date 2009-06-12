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
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the set
    /// highlight height <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(SetHighlightHeightToolStripMenuItem),
        ToolStripButtonConstants._SET_HIGHLIGHT_HEIGHT_BUTTON_IMAGE)]
    public partial class SetHighlightHeightToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region SetHighlightHeightToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="SetHighlightHeightToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public SetHighlightHeightToolStripMenuItem()
            : base(CursorTool.SetHighlightHeight,
            ToolStripButtonConstants._SET_HIGHLIGHT_HEIGHT_MENU_ITEM_TEXT,
            ToolStripButtonConstants._SET_HIGHLIGHT_HEIGHT_BUTTON_IMAGE,
            typeof(SetHighlightHeightToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region SetHighlightHeightToolStripMenuItem Events

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
                    base.ImageViewer.CursorTool = CursorTool.SetHighlightHeight;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21427", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region SetHighlightHeightToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return null;
        }

        #endregion SetHighlightHeightToolStripMenuItem Methods
    }
}
