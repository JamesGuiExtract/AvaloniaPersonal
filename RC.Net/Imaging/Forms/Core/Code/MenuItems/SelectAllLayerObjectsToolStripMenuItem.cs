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
    /// select all layer objects.
    /// </summary>
    public partial class SelectAllLayerObjectsToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region SelectAllLayerObjectsToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="SelectAllLayerObjectsToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public SelectAllLayerObjectsToolStripMenuItem()
            : base(ToolStripButtonConstants._SELECT_ALL_MENU_ITEM_TEXT,
            ToolStripButtonConstants._SELECT_ALL_MENU_ITEM_IMAGE,
            typeof(SelectAllLayerObjectsToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion SelectAllLayerObjectsToolStripMenuItem Constructors

        #region SelectAllLayerObjectsToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            ImageViewer imageViewer = base.ImageViewer;
            return imageViewer == null ? null : 
                imageViewer.Shortcuts.GetKeys(imageViewer.SelectSelectAllLayerObjects);
        }

        /// <summary>
        /// Sets the enabled state of the <see cref="ImageViewerCommandToolStripMenuItem"/>
        /// </summary>
        protected override void SetEnabledState()
        {
            // Enable me if any layer object is visible and selectable
            if (base.ImageViewer != null && base.ImageViewer.IsImageAvailable)
            {
                foreach (LayerObject layerObject in base.ImageViewer.LayerObjects)
                {
                    if (layerObject.Selectable && layerObject.Visible)
                    {
                        base.Enabled = true;
                        return;
                    }
                }
            }

            // Disable me
            base.Enabled = false;
        }

        #endregion SelectAllLayerObjectsToolStripMenuItem Methods

        #region SelectAllLayerObjectsToolStripMenuItem OnEvents

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
                    imageViewer.SelectSelectAllLayerObjects();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22341", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion SelectAllLayerObjectsToolStripMenuItem OnEvents

        #region SelectAllLayerObjectsToolStripMenuItem Event Handlers
    	
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
                ExtractException ee = ExtractException.AsExtractException("ELI22342", ex);
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
                ExtractException ee = ExtractException.AsExtractException("ELI22344", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22367", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }
    		
	    #endregion SelectAllLayerObjectsToolStripMenuItem Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="SelectAllLayerObjectsToolStripMenuItem"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="SelectAllLayerObjectsToolStripMenuItem"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="SelectAllLayerObjectsToolStripMenuItem"/> is 
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
                        LayerObjectsCollection layerObjects = base.ImageViewer.LayerObjects;
                        layerObjects.LayerObjectAdded -= HandleLayerObjectAdded;
                        layerObjects.LayerObjectDeleted -= HandleLayerObjectDeleted;
                        layerObjects.LayerObjectVisibilityChanged -= HandleLayerObjectVisibilityChanged;
                    }

                    // Store the new image viewer
                    base.ImageViewer = value;

                    // Register for events
                    if (base.ImageViewer != null)
                    {
                        LayerObjectsCollection layerObjects = base.ImageViewer.LayerObjects;
                        layerObjects.LayerObjectAdded += HandleLayerObjectAdded;
                        layerObjects.LayerObjectDeleted += HandleLayerObjectDeleted;
                        layerObjects.LayerObjectVisibilityChanged += HandleLayerObjectVisibilityChanged;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI22331",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
