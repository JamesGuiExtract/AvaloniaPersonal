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
    /// Represents a <see cref="ToolStripButton"/> that activates the Zoom out command.
    /// </summary>
    [ToolboxBitmap(typeof(ZoomOutToolStripButton), 
        ToolStripButtonConstants._ZOOM_OUT_BUTTON_IMAGE)]
    public partial class ZoomOutToolStripButton : ImageViewerCommandToolStripButton
    {
        #region ZoomOutToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="ZoomOutToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public ZoomOutToolStripButton()
            : base(ToolStripButtonConstants._ZOOM_OUT_BUTTON_IMAGE,
            ToolStripButtonConstants._ZOOM_OUT_BUTTON_TOOL_TIP,
            typeof(ZoomOutToolStripButton),
            ToolStripButtonConstants._ZOOM_OUT_BUTTON_TEXT)
        {
            // Initialize the component
            InitializeComponent();
        }

        #endregion

        #region ZoomOutToolStripButton Events

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
                    base.ImageViewer.ZoomOut();
                }

                base.OnClick(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21312", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        #endregion

        #region ZoomOutToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectZoomOut);
        }

        #endregion ZoomOutToolStripButton Methods
    }
}