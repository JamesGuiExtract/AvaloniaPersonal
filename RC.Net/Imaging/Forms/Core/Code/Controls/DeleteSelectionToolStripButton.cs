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
    /// Represents a <see cref="ToolStripButton"/> that allows the user delete the selected layer
    /// objects.
    /// </summary>
    [ToolboxBitmap(typeof(DeleteSelectionToolStripButton),
        ToolStripButtonConstants._DELETE_SELECTION_BUTTON_IMAGE)]
    public partial class DeleteSelectionToolStripButton : ImageViewerCommandToolStripButton
    {
        #region DeleteSelectionToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="DeleteSelectionToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public DeleteSelectionToolStripButton()
            : base(ToolStripButtonConstants._DELETE_SELECTION_BUTTON_IMAGE,
            ToolStripButtonConstants._DELETE_SELECTION_BUTTON_TOOL_TIP,
            typeof(DeleteSelectionToolStripButton),
            ToolStripButtonConstants._DELETE_SELECTION_BUTTON_TEXT)
        {
            InitializeComponent();
        }

        #endregion DeleteSelectionToolStripButton Constructors

        #region DeleteSelectionToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            IDocumentViewer imageViewer = base.ImageViewer;
            return imageViewer == null ? null :
                imageViewer.Shortcuts.GetKeys(imageViewer.SelectRemoveSelectedLayerObjects);
        }

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Enable if any layer object is selected; otherwise, disable.
            base.Enabled = base.ImageViewer != null &&
                base.ImageViewer.LayerObjects.Selection.Count > 0;
        }

        #endregion DeleteSelectionToolStripButton Methods

        #region DeleteSelectionToolStripButton OnEvents

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            try
            {
                IDocumentViewer imageViewer = base.ImageViewer;
                if (imageViewer != null && imageViewer.IsImageAvailable)
                {
                    imageViewer.SelectRemoveSelectedLayerObjects();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22471", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion DeleteSelectionToolStripButton OnEvents

        #region DeleteSelectionToolStripButton Event Handlers

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
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22474", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22465", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion DeleteSelectionToolStripButton Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="DeleteSelectionToolStripButton"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="DeleteSelectionToolStripButton"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="DeleteSelectionToolStripButton"/> is 
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
                        LayerObjectsCollection selection = base.ImageViewer.LayerObjects.Selection;
                        selection.LayerObjectAdded -= HandleLayerObjectAdded;
                        selection.LayerObjectDeleted -= HandleLayerObjectDeleted;
                    }

                    // Store the new image viewer
                    base.ImageViewer = value;

                    // Register for events
                    if (base.ImageViewer != null)
                    {
                        LayerObjectsCollection selection = base.ImageViewer.LayerObjects.Selection;
                        selection.LayerObjectAdded += HandleLayerObjectAdded;
                        selection.LayerObjectDeleted += HandleLayerObjectDeleted;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI22468",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
