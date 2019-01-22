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
    /// Represents a <see cref="ImageViewerCommandToolStripMenuItem"/> that allows the user to 
    /// delete all selected layer objects.
    /// </summary>
    [ToolboxBitmap(typeof(DeleteSelectionToolStripMenuItem),
       ToolStripButtonConstants._DELETE_SELECTION_BUTTON_IMAGE_SMALL)]
    public partial class DeleteSelectionToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region DeleteSelectionToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="DeleteSelectionToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public DeleteSelectionToolStripMenuItem()
            : base(ToolStripButtonConstants._DELETE_SELECTION_MENU_ITEM_TEXT,
            ToolStripButtonConstants._DELETE_SELECTION_BUTTON_IMAGE_SMALL,
            typeof(DeleteSelectionToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion DeleteSelectionToolStripMenuItem Constructors

        #region DeleteSelectionToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            var imageViewer = base.ImageViewer;
            return imageViewer?.Shortcuts.GetKeys(imageViewer.SelectRemoveSelectedLayerObjects);
        }

        /// <summary>
        /// Sets the enabled state of the <see cref="DeleteSelectionToolStripMenuItem"/>.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Enable if any layer object is selected; otherwise, disable.
            base.Enabled = base.ImageViewer != null && 
                base.ImageViewer.LayerObjects.Selection.Count > 0;
        }

        #endregion DeleteSelectionToolStripMenuItem Methods

        #region DeleteSelectionToolStripMenuItem OnEvents

        /// <summary>
        /// Raises the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="ToolStripItem.Click"/> 
        /// event.</param>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            try
            {
                var imageViewer = base.ImageViewer;
                if (imageViewer != null && imageViewer.IsImageAvailable)
                {
                    imageViewer.SelectRemoveSelectedLayerObjects();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22340", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion DeleteSelectionToolStripMenuItem OnEvents

        #region DeleteSelectionToolStripMenuItem Event Handlers

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
                ExtractException ee = ExtractException.AsExtractException("ELI22332", ex);
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
                ExtractException ee = ExtractException.AsExtractException("ELI22333", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion DeleteSelectionToolStripMenuItem Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="DeleteSelectionToolStripMenuItem"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="DeleteSelectionToolStripMenuItem"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="DeleteSelectionToolStripMenuItem"/> is 
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
                    ExtractException ee = new ExtractException("ELI23247",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
