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
    /// enlarge all selected layer objects.
    /// </summary>
    public partial class EnlargeSelectionToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region EnlargeSelectionToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="EnlargeSelectionToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public EnlargeSelectionToolStripMenuItem()
            : base(ToolStripButtonConstants._ENLARGE_SELECTION_MENU_ITEM_TEXT,
            ToolStripButtonConstants._ENLARGE_SELECTION_MENU_ITEM_IMAGE,
            typeof(EnlargeSelectionToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion EnlargeSelectionToolStripMenuItem Constructors

        #region EnlargeSelectionToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return ImageViewer?.Shortcuts.GetKeys(ImageViewer.EnlargeSelectedZones);
        }

        /// <summary>
        /// Sets the enabled state of the <see cref="EnlargeSelectionToolStripMenuItem"/>.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Enable if any layer object is selected; otherwise, disable.
            Enabled = ImageViewer != null && 
                ImageViewer.LayerObjects.Selection.Count > 0;
        }

        #endregion EnlargeSelectionToolStripMenuItem Methods

        #region EnlargeSelectionToolStripMenuItem OnEvents

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
                if (ImageViewer != null && ImageViewer.IsImageAvailable)
                {
                    ImageViewer.EnlargeSelectedZones();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31606");
            }
        }

        #endregion EnlargeSelectionToolStripMenuItem OnEvents

        #region EnlargeSelectionToolStripMenuItem Event Handlers

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
                ExtractException ee = ExtractException.AsExtractException("ELI31607", ex);
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
                ExtractException ee = ExtractException.AsExtractException("ELI31608", ex);
                ee.Display();
            }
        }

        #endregion EnlargeSelectionToolStripMenuItem Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="EnlargeSelectionToolStripMenuItem"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="EnlargeSelectionToolStripMenuItem"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="EnlargeSelectionToolStripMenuItem"/> is 
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
                    ExtractException ee = new ExtractException("ELI31609",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
