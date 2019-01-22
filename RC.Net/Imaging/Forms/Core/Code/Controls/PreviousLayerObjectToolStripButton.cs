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
    /// layer object on an associated <see cref="ImageViewer"/> control.
    /// </summary>
    [ToolboxBitmap(typeof(PreviousLayerObjectToolStripButton),
        ToolStripButtonConstants._PREVIOUS_LAYER_OBJECT_BUTTON_IMAGE)]
    public partial class PreviousLayerObjectToolStripButton : ImageViewerCommandToolStripButton
    {
        #region PreviousLayerObjectToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="PreviousLayerObjectToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PreviousLayerObjectToolStripButton()
            : base(ToolStripButtonConstants._PREVIOUS_LAYER_OBJECT_BUTTON_IMAGE,
            ToolStripButtonConstants._PREVIOUS_LAYER_OBJECT_BUTTON_TOOL_TIP,
            typeof(PreviousLayerObjectToolStripButton), null)
        {
            InitializeComponent();
        }

        #endregion

        #region PreviousLayerObjectToolStripButton Overrides

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Button is enabled if there is an image viewer, an image open and 
            // there is a previous layer object to navigate to
            base.Enabled = base.ImageViewer != null && base.ImageViewer.IsImageAvailable
                && base.ImageViewer.CanGoToPreviousLayerObject;
        }

        #endregion

        #region PreviousLayerObjectToolStripButton OnEvents

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                // Notify the base class of the click event
                base.OnClick(e);

                // Go to the previous visible layer object
                base.ImageViewer.GoToPreviousVisibleLayerObject(true);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22419", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        #endregion

        #region PreviousLayerObjectToolStripButton Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        private void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                // Enable or disable button based on state of image viewer control
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22420", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.PageChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.PageChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.PageChanged"/> event.</param>
        private void HandlePageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                // Enable or disable button based on state of image viewer control
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22427", ex);
            }
        }

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
                // Enable or disable button based on state of image viewer control
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22421", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        private void HandleLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                // Enable or disable button based on state of image viewer control
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22422", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        private void HandleLayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                // Enable or disable button based on state of image viewer control
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22423", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.</param>
        private void HandleLayerObjectChanged(object sender, LayerObjectChangedEventArgs e)
        {
            try
            {
                // Enable or disable button based on state of image viewer control
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22424", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectVisibilityChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectVisibilityChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectVisibilityChanged"/> event.</param>
        private void HandleLayerObjectVisibilityChanged(object sender,
            LayerObjectVisibilityChangedEventArgs e)
        {
            try
            {
                // Enable or disable button based on state of image viewer control
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22425", ex);
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
                ExtractException.Display("ELI22682", ex);
            }
        }


        #endregion

        #region PreviousLayerObjectToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.GoToPreviousLayerObject);
        }

        #endregion PreviousLayerObjectToolStripButton Methods

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="PreviousLayerObjectToolStripButton"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="PreviousLayerObjectToolStripButton"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="PreviousLayerObjectToolStripButton"/> is 
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
                        base.ImageViewer.ImageFileChanged -= HandleImageFileChanged;
                        base.ImageViewer.PageChanged -= HandlePageChanged;
                        base.ImageViewer.ZoomChanged -= HandleZoomChanged;
                        base.ImageViewer.ScrollPositionChanged -= HandleScrollPositionChanged;
                        base.ImageViewer.LayerObjects.LayerObjectAdded -= HandleLayerObjectAdded;
                        base.ImageViewer.LayerObjects.LayerObjectChanged -= HandleLayerObjectChanged;
                        base.ImageViewer.LayerObjects.LayerObjectDeleted -= HandleLayerObjectDeleted;
                        base.ImageViewer.LayerObjects.LayerObjectVisibilityChanged -= HandleLayerObjectVisibilityChanged;
                        base.ImageViewer.LayerObjects.Selection.LayerObjectAdded -= HandleLayerObjectAdded;
                        base.ImageViewer.LayerObjects.Selection.LayerObjectDeleted -= HandleLayerObjectDeleted;
                    }

                    // Call the base class set property
                    base.ImageViewer = value;

                    // Register for events
                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.ImageFileChanged += HandleImageFileChanged;
                        base.ImageViewer.PageChanged += HandlePageChanged;
                        base.ImageViewer.ZoomChanged += HandleZoomChanged;
                        base.ImageViewer.ScrollPositionChanged += HandleScrollPositionChanged;
                        base.ImageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
                        base.ImageViewer.LayerObjects.LayerObjectChanged += HandleLayerObjectChanged;
                        base.ImageViewer.LayerObjects.LayerObjectDeleted += HandleLayerObjectDeleted;
                        base.ImageViewer.LayerObjects.LayerObjectVisibilityChanged += HandleLayerObjectVisibilityChanged;
                        base.ImageViewer.LayerObjects.Selection.LayerObjectAdded += HandleLayerObjectAdded;
                        base.ImageViewer.LayerObjects.Selection.LayerObjectDeleted += HandleLayerObjectDeleted;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI22426",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
