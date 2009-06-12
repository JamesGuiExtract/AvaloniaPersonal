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
    /// Represents a <see cref="ToolStripButton"/> that activates the Zoom previous command.
    /// </summary>
    [ToolboxBitmap(typeof(ZoomPreviousToolStripButton), 
        ToolStripButtonConstants._ZOOM_PREVIOUS_BUTTON_IMAGE)]
    public partial class ZoomPreviousToolStripButton : ImageViewerCommandToolStripButton
    {
        #region ZoomPreviousToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="ZoomPreviousToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public ZoomPreviousToolStripButton()
            : base(ToolStripButtonConstants._ZOOM_PREVIOUS_BUTTON_IMAGE,
            ToolStripButtonConstants._ZOOM_PREVIOUS_BUTTON_TOOL_TIP,
            typeof(ZoomPreviousToolStripButton),
            ToolStripButtonConstants._ZOOM_PREVIOUS_BUTTON_TEXT)
        {
            // Initialize the component
            InitializeComponent();
        }

        #endregion

        #region ZoomPreviousToolStripButton Overrides

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Get the image viewer this control is attached to
            ImageViewer imageViewer = base.ImageViewer;

            // Enable this button if an image is open and CanZoomPrevious is true
            base.Enabled = imageViewer != null && imageViewer.IsImageAvailable &&
                imageViewer.CanZoomPrevious;
        }

        #endregion

        #region ZoomPreviousToolStripButton Events

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
                if (base.ImageViewer != null && base.ImageViewer.IsImageAvailable
                    && base.ImageViewer.CanZoomPrevious)
                {
                    base.ImageViewer.ZoomPrevious();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21349", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region ZoomPreviousToolStripButton Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ZoomChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ZoomChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ZoomChanged"/> event.</param>
        private void HandleZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            try
            {
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21460", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion ZoomPreviousToolStripButton Event Handlers

        #region ZoomPreviousToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectZoomPrevious);
        }

        #endregion ZoomPreviousToolStripButton Methods

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="NextPageToolStripButton"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="NextPageToolStripButton"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="NextPageToolStripButton"/> is 
        /// connected. <see langword="null"/> if no connections are established.</returns>
        [Browsable(false)]
        public override ImageViewer ImageViewer
        {
            get
            {
                return base.ImageViewer;
            }
            set
            {
                try
                {
                    // Unregister from previously subscribed-to events
                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.ZoomChanged -= HandleZoomChanged;
                    }

                    // Call the base class set property
                    base.ImageViewer = value;

                    // Register for events
                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.ZoomChanged += HandleZoomChanged;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI21459",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
