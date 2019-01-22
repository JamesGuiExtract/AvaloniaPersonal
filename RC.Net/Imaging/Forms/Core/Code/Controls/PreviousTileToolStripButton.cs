using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripButton"/> that allows the user to navigate to the previous 
    /// zoom region.
    /// </summary>
    public partial class PreviousTileToolStripButton : ImageViewerCommandToolStripButton
    {
        #region PreviousTileToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="PreviousTileToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PreviousTileToolStripButton()
            : base(ToolStripButtonConstants._PREVIOUS_TILE_BUTTON_IMAGE,
            ToolStripButtonConstants._PREVIOUS_TILE_BUTTON_TOOL_TIP,
            typeof(PreviousTileToolStripButton), null)
        {
            InitializeComponent();
        }

        #endregion PreviousTileToolStripButton Constructors

        #region PreviousTileToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectPreviousTile);
        }

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Get the image viewer to which this control is attached
            IDocumentViewer imageViewer = base.ImageViewer;

            // Enable this button if an image is open and it is not on the first tile.
            base.Enabled = imageViewer != null && imageViewer.IsImageAvailable &&
                !imageViewer.IsFirstTile;
        }
        
        #endregion PreviousTileToolStripButton Methods

        #region PreviousTileToolStripButton OnEvents

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                base.OnClick(e);

                if (ImageViewer != null)
                {
                    // 1/23/2012 SNK
                    // For reasons, when the toolstrip button or menu item to advance to the previous
                    // document with seamless navigation, the ImageViewer doesn't always seem to be
                    // properly refreshed at the time of this call. Specifically, the
                    // PhysicalViewRectangle does not seem to be correct. I believe it is perhaps
                    // reflecting its value from before the new image was loaded.
                    // I'm working around the issue by calling SelectNextTile via BeginInvoke since
                    // presumably after the currently queued events are processed the ImageViewer
                    // will be properly updated.
                    ImageViewer.BeginInvoke((MethodInvoker)(() => ImageViewer.SelectPreviousTile()));
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21851", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion PreviousTileToolStripButton OnEvents

        #region PreviousTileToolStripButton Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.ZoomChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ZoomChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ZoomChanged"/> event.</param>
        private void HandleZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            try
            {
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21852", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.OrientationChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.OrientationChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.OrientationChanged"/> event.</param>
        private void HandleOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            try
            {
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21877", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the ScrollPositionChanged event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// ScrollPositionChanged event.</param>
        /// <param name="e">The event data associated with the
        /// ScrollPositionChanged event.</param>
        private void HandleScrollPositionChanged(object sender, EventArgs e)
        {
            try
            {
                // Enable or disable button based on state of image viewer control
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23058", ex);
            }
        }

        #endregion PreviousTileToolStripButton Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="PreviousTileToolStripButton"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="PreviousTileToolStripButton"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="PreviousTileToolStripButton"/> is 
        /// connected. <see langword="null"/> if no connections are established.</returns>
        [Browsable(false)]
        [CLSCompliant(false)]
        public override IDocumentViewer ImageViewer
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
                        base.ImageViewer.OrientationChanged -= HandleOrientationChanged;
                        base.ImageViewer.ScrollPositionChanged -= HandleScrollPositionChanged;
                    }

                    // Store the new image viewer internally
                    base.ImageViewer = value;

                    // Register for events
                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.ZoomChanged += HandleZoomChanged;
                        base.ImageViewer.OrientationChanged += HandleOrientationChanged;
                        base.ImageViewer.ScrollPositionChanged += HandleScrollPositionChanged;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI21853",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
