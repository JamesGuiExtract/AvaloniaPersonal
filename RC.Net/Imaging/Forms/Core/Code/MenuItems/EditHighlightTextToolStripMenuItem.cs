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
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the edit
    /// highlight height <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(EditHighlightTextToolStripMenuItem),
        ToolStripButtonConstants._EDIT_HIGHLIGHT_TEXT_BUTTON_IMAGE)]
    public partial class EditHighlightTextToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region EditHighlightTextToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="EditHighlightTextToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public EditHighlightTextToolStripMenuItem()
            : base(CursorTool.EditHighlightText,
            ToolStripButtonConstants._EDIT_HIGHLIGHT_TEXT_MENU_ITEM_TEXT,
            ToolStripButtonConstants._EDIT_HIGHLIGHT_TEXT_BUTTON_IMAGE,
            typeof(EditHighlightTextToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region EditHighlightTextToolStripMenuItem Events

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
                    base.ImageViewer.CursorTool = CursorTool.EditHighlightText;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21428", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region EditHighlightTextToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectEditHighlightTextTool);
        }

        #endregion EditHighlightTextToolStripMenuItem Methods
    }
}
