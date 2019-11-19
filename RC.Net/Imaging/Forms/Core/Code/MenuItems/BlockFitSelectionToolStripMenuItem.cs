using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ImageViewerCommandToolStripMenuItem"/> that allows the user to 
    /// block fit all selected layer objects.
    /// </summary>
    public partial class BlockFitSelectionToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region BlockFitSelectionToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="BlockFitSelectionToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public BlockFitSelectionToolStripMenuItem()
            : base(ToolStripButtonConstants._BLOCK_FIT_SELECTION_MENU_ITEM_TEXT,
            ToolStripButtonConstants._BLOCK_FIT_SELECTION_MENU_ITEM_IMAGE,
            typeof(BlockFitSelectionToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion BlockFitSelectionToolStripMenuItem Constructors

        #region BlockFitSelectionToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            ImageViewer imageViewer = base.ImageViewer;
            return imageViewer == null ? null :
                imageViewer.Shortcuts.GetKeys(imageViewer.BlockFitSelectedZones);
        }

        /// <summary>
        /// Sets the enabled state of the <see cref="BlockFitSelectionToolStripMenuItem"/>.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Enable if any layer object is selected; otherwise, disable.
            base.Enabled = base.ImageViewer != null && 
                base.ImageViewer.LayerObjects.Selection.Count > 0;
        }

        #endregion BlockFitSelectionToolStripMenuItem Methods

        #region BlockFitSelectionToolStripMenuItem OnEvents

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
                ImageViewer imageViewer = base.ImageViewer;
                if (imageViewer != null && imageViewer.IsImageAvailable)
                {
                    imageViewer.BlockFitSelectedZones();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI30082", ex);
                ee.Display();
            }
        }

        #endregion BlockFitSelectionToolStripMenuItem OnEvents

        #region BlockFitSelectionToolStripMenuItem Event Handlers

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
                ExtractException ee = ExtractException.AsExtractException("ELI30083", ex);
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
                ExtractException ee = ExtractException.AsExtractException("ELI30084", ex);
                ee.Display();
            }
        }

        #endregion BlockFitSelectionToolStripMenuItem Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="BlockFitSelectionToolStripMenuItem"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="BlockFitSelectionToolStripMenuItem"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="BlockFitSelectionToolStripMenuItem"/> is 
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
                    ExtractException ee = new ExtractException("ELI30085",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
