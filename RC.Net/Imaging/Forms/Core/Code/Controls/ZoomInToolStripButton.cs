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
    /// Represents a <see cref="ToolStripButton"/> that activates the Zoom in command.
    /// </summary>
    [ToolboxBitmap(typeof(ZoomInToolStripButton), 
        ToolStripButtonConstants._ZOOM_IN_BUTTON_IMAGE)]
    public partial class ZoomInToolStripButton : ImageViewerCommandToolStripButton
    {
        #region ZoomInToolStripButton constructors

        /// <summary>
        /// Initializes a new <see cref="ZoomInToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public ZoomInToolStripButton()
            : base(ToolStripButtonConstants._ZOOM_IN_BUTTON_IMAGE,
            ToolStripButtonConstants._ZOOM_IN_BUTTON_TOOL_TIP,
            typeof(ZoomInToolStripButton),
            ToolStripButtonConstants._ZOOM_IN_BUTTON_TEXT)
        {
            // Initialize the component
            InitializeComponent();
        }

        #endregion

        #region ZoomInToolStripButton Events

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
                    base.ImageViewer.ZoomIn();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21311", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region ZoomInToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectZoomIn);
        }

        #endregion ZoomInToolStripButton Methods
    }
}
