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
    /// Represents a <see cref="ToolStripMenuItem"/> that allows the user to navigate to the next 
    /// zoom region.
    /// </summary>
    public partial class NextTileToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region NextTileToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="NextTileToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public NextTileToolStripMenuItem()
            : base("Next tile", ToolStripButtonConstants._NEXT_TILE_BUTTON_IMAGE_SMALL,
                typeof(NextTileToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion NextTileToolStripMenuItem Constructors

        #region NextTileToolStripMenuItem Methods

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Get the image viewer to which this control is attached
            ImageViewer imageViewer = base.ImageViewer;

            // Enable this menu item if an image is open and it is not on the last tile.
            base.Enabled = imageViewer != null && imageViewer.IsImageAvailable &&
                !imageViewer.IsLastTile;
        }

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectNextTile);
        }

        #endregion NextTileToolStripMenuItem Methods

        #region NextTileToolStripMenuItem OnEvents

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
                    // [FlexIDSCore:4999]
                    // For reasons, when the toolstrip button or menu item to advance to the next
                    // document with seamless navigation, the ImageViewer doesn't always seem to be
                    // properly refreshed at the time of this call. Specifically, the
                    // PhysicalViewRectangle does not seem to be correct. I believe it is perhaps
                    // reflecting its value from before the new image was loaded.
                    // I'm working around the issue by calling SelectNextTile via BeginInvoke since
                    // presumably after the currently queued events are processed the ImageViewer
                    // will be properly updated.
                    ImageViewer.BeginInvoke((MethodInvoker)(() => ImageViewer.SelectNextTile()));
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21878", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion NextTileToolStripMenuItem OnEvents

        #region NextTileToolStripMenuItem Event Handlers

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
                ExtractException ee = ExtractException.AsExtractException("ELI21879", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.OrientationChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.OrientationChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.OrientationChanged"/> event.</param>
        private void HandleOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            try
            {
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21880", ex);
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
                ExtractException.Display("ELI23059", ex);
            }
        }

        #endregion NextTileToolStripMenuItem Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="NextTileToolStripMenuItem"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="NextTileToolStripMenuItem"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="NextTileToolStripMenuItem"/> is 
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
                    ExtractException ee = new ExtractException("ELI21881",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
