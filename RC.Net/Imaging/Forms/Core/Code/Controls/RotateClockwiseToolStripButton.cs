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
    /// Represents a <see cref="ToolStripButton"/> that activates the Rotate clockwise command.
    /// </summary>
    [ToolboxBitmap(typeof(RotateClockwiseToolStripButton), 
        ToolStripButtonConstants._ROTATE_CLOCKWISE_BUTTON_IMAGE)]
    public partial class RotateClockwiseToolStripButton : ImageViewerCommandToolStripButton
    {
        #region RotateClockwiseToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="RotateClockwiseToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public RotateClockwiseToolStripButton()
            : base(ToolStripButtonConstants._ROTATE_CLOCKWISE_BUTTON_IMAGE,
            ToolStripButtonConstants._ROTATE_CLOCKWISE_BUTTON_TOOL_TIP,
            typeof(RotateClockwiseToolStripButton),
            ToolStripButtonConstants._ROTATE_CLOCKWISE_BUTTON_TEXT)
        {
            // Initialize the component
            InitializeComponent();
        }

        #endregion

        #region RotateClockwiseToolStripButton Events

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
                    base.ImageViewer.SelectRotateClockwise();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21340", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region RotateClockwiseToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectRotateClockwise);
        }

        #endregion RotateClockwiseToolStripButton Methods
    }
}
