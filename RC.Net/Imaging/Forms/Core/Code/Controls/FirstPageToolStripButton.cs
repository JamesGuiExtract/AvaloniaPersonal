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
    /// Represents a <see cref="ToolStripButton"/> that allows the user to navigate to the first 
    /// page on an associated <see cref="ImageViewer"/> control.
    /// </summary>
    [ToolboxBitmap(typeof(FirstPageToolStripButton),
        ToolStripButtonConstants._FIRST_PAGE_BUTTON_IMAGE)]
    public partial class FirstPageToolStripButton : ImageViewerCommandToolStripButton
    {
        #region FirstPageToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="FirstPageToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public FirstPageToolStripButton()
            : base(ToolStripButtonConstants._FIRST_PAGE_BUTTON_IMAGE,
            ToolStripButtonConstants._FIRST_PAGE_BUTTON_TOOL_TIP,
            typeof(FirstPageToolStripButton), null)
        {
            InitializeComponent();
        }

        #endregion

        #region FirstPageToolStripButton Overrides

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Get the image viewer attached to this control
            IDocumentViewer imageViewer = base.ImageViewer; 

            // Enable this button if an image is open and it is not on the first page.
            base.Enabled = imageViewer != null && imageViewer.IsImageAvailable &&
                imageViewer.PageNumber > 1;
        }

        #endregion

        #region FirstPageToolStripButton OnEvents

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                // Go to the first page if a connection has been established to the image viewer.
                if (base.ImageViewer != null)
                {
                    base.ImageViewer.GoToFirstPage();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21316", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                // Notify the base class of the click event
                base.OnClick(e);
            }
        }

        #endregion

        #region FirstPageToolStripButton Event Handlers

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
                ExtractException.Display("ELI21304", ex);
            }
        }

        #endregion

        #region FirstPageToolStripButton Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.GoToFirstPage);
        }

        #endregion FirstPageToolStripButton Methods

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="FirstPageToolStripButton"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="FirstPageToolStripButton"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="FirstPageToolStripButton"/> is 
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

                    // Call the base class set property
                    base.ImageViewer = value;

                    // Register for events
                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.PageChanged += HandlePageChanged;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI21305",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
