using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ImageViewerCommandToolStripMenuItem"/> that activates the 
    /// print preview command.
    /// </summary>
    [ToolboxBitmap(typeof(PrintPreviewToolStripMenuItem),
       ToolStripButtonConstants._PRINT_PREVIEW_BUTTON_IMAGE_SMALL)]
    public partial class PrintPreviewToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region PrintPreviewToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="PrintPreviewToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PrintPreviewToolStripMenuItem()
            : base(ToolStripButtonConstants._PRINT_PREVIEW_MENU_ITEM_TEXT,
            ToolStripButtonConstants._PRINT_PREVIEW_BUTTON_IMAGE_SMALL,
            typeof(PrintPreviewToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region PrintPreviewToolStripMenuItem Events

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
                base.OnClick(e);

                if (base.ImageViewer != null && base.ImageViewer.IsImageAvailable)
                {
                    base.ImageViewer.PrintPreview();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23025", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        #endregion

        #region PrintPreviewToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return null;
        }

        #endregion PrintPreviewToolStripMenuItem Methods
    }
}
