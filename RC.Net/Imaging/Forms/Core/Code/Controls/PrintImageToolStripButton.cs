using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripButton"/> that activates the Print image command.
    /// </summary>
    [ToolboxBitmap(typeof(PrintImageToolStripButton), 
        ToolStripButtonConstants._PRINT_IMAGE_BUTTON_IMAGE)]
    public partial class PrintImageToolStripButton : ImageViewerCommandToolStripButton
    {
        #region PrintImageToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="PrintImageToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PrintImageToolStripButton()
            : base (ToolStripButtonConstants._PRINT_IMAGE_BUTTON_IMAGE,
            ToolStripButtonConstants._PRINT_IMAGE_BUTTON_TOOL_TIP,
            typeof(PrintImageToolStripButton),
            ToolStripButtonConstants._PRINT_IMAGE_BUTTON_TEXT)
        {
            // Initialize the component
            InitializeComponent();
        }

        #endregion

        #region PrintImageToolStripButton Events

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
                    // Allow print dialog to handle user's first mouse click [DotNetRCAndUtils #58]
                    base.Parent.Capture = false;
                    base.ImageViewer.Print();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21313", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region PrintImageToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectPrint);
        }

        #endregion PrintImageToolStripButton Methods
    }
}