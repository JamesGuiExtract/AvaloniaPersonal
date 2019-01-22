using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ImageViewerCommandToolStripMenuItem"/> that activates the last
    /// page command.
    /// </summary>
    [ToolboxBitmap(typeof(LastPageToolStripMenuItem),
       ToolStripButtonConstants._LAST_PAGE_BUTTON_IMAGE_SMALL)]
    public partial class LastPageToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region LastPageToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="LastPageToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public LastPageToolStripMenuItem()
            : base(ToolStripButtonConstants._LAST_PAGE_MENU_ITEM_TEXT,
            ToolStripButtonConstants._LAST_PAGE_BUTTON_IMAGE_SMALL,
            typeof(LastPageToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region LastPageToolStripMenuItem Overrides

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Get the image viewer this control is attached to
            var imageViewer = base.ImageViewer;

            // Enable this button if an image is open and it is not on the last page.
            base.Enabled = imageViewer != null && imageViewer.IsImageAvailable &&
                imageViewer.PageNumber < imageViewer.PageCount;
        }

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.GoToLastPage);
        }

        #endregion

        #region LastPageToolStripMenuItem Events

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
                var imageViewer = base.ImageViewer;

                if (imageViewer != null && imageViewer.IsImageAvailable
                    && imageViewer.PageNumber < imageViewer.PageCount)
                {
                    base.ImageViewer.GoToLastPage();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21405", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region LastPageToolStripMenuItem Event Handlers

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
                SetEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21836", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion LastPageToolStripMenuItem Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="LastPageToolStripMenuItem"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="LastPageToolStripMenuItem"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="LastPageToolStripMenuItem"/> is 
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
                        base.ImageViewer.PageChanged -= HandlePageChanged;
                    }

                    // Store the new image viewer internally
                    base.ImageViewer = value;

                    // Register for events
                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.PageChanged += HandlePageChanged;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI21837",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
