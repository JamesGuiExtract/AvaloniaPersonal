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
    /// Represents a <see cref="ImageViewerCommandToolStripMenuItem"/> that allows the user to
    /// rotate all document pages counterclockwise.
    /// </summary>
    [ToolboxBitmap(typeof(RotateAllDocumentPagesCounterclockwiseToolStripMenuItem),
       ToolStripButtonConstants._ROTATE_ALL_DOC_PAGES_COUNTERCLOCKWISE_BUTTON_IMAGE_SMALL)]
    public partial class RotateAllDocumentPagesCounterclockwiseToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="RotateAllDocumentPagesCounterclockwiseToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public RotateAllDocumentPagesCounterclockwiseToolStripMenuItem()
            : base(ToolStripButtonConstants._ROTATE_ALL_DOC_PAGES_COUNTERCLOCKWISE_MENU_ITEM_TEXT,
            ToolStripButtonConstants._ROTATE_ALL_DOC_PAGES_COUNTERCLOCKWISE_BUTTON_IMAGE_SMALL,
            typeof(RotateAllDocumentPagesCounterclockwiseToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Events

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
                    base.ImageViewer.SelectRotateAllDocumentPagesCounterclockwise();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI36817", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(
                base.ImageViewer.SelectRotateAllDocumentPagesCounterclockwise);
        }

        #endregion Methods
    }
}
