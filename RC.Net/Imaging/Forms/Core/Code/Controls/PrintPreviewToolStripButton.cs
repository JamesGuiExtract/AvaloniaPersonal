using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripButton"/> that activates the Print preview command.
    /// </summary>
    [ToolboxBitmap(typeof(PrintPreviewToolStripButton),
        ToolStripButtonConstants._PRINT_PREVIEW_BUTTON_IMAGE)]
    public partial class PrintPreviewToolStripButton : ImageViewerCommandToolStripButton
    {
        #region PrintPreviewToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="PrintPreviewToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PrintPreviewToolStripButton()
            : base(ToolStripButtonConstants._PRINT_PREVIEW_BUTTON_IMAGE,
            ToolStripButtonConstants._PRINT_PREVIEW_BUTTON_TOOL_TIP,
            typeof(PrintPreviewToolStripButton),
            ToolStripButtonConstants._PRINT_PREVIEW_BUTTON_TEXT)
        {
            // Initialize the component
            InitializeComponent();
        }

        #endregion

        #region PrintPreviewToolStripButton Events

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
                ExtractException ee = ExtractException.AsExtractException("ELI23024", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        #endregion

        #region PrintPreviewToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return null;
        }

        #endregion PrintPreviewToolStripButton Methods
    }
}
