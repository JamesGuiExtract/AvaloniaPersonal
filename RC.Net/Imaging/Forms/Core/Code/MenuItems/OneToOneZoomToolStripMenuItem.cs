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
    /// Represents a <see cref="ImageViewerCommandToolStripMenuItem"/> that activates the fit
    /// to page command.
    /// </summary>
    [ToolboxBitmap(typeof(OneToOneZoomToolStripMenuItem),
        ToolStripButtonConstants._ONE_TO_ONE_ZOOM_BUTTON_IMAGE_SMALL)]
    public partial class OneToOneZoomToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region OneToOneZoomToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="OneToOneZoomToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public OneToOneZoomToolStripMenuItem()
            : base(ToolStripButtonConstants._ONE_TO_ONE_ZOOM_MENU_ITEM_TEXT,
            ToolStripButtonConstants._ONE_TO_ONE_ZOOM_BUTTON_IMAGE_SMALL,
            typeof(OneToOneZoomToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region OneToOneZoomToolStripMenuItem Overrides

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Enable this button if there is an image viewer
            base.Enabled = base.ImageViewer != null;
        }

        /// <summary>
        /// Sets the checked state depending on the fit mode of the associated image viewer.
        /// </summary>
        void SetCheckedState()
        {
            if (base.ImageViewer != null)
            {
                // If the fit mode has changed to OneToOneZoom then set this button as checked
                base.Checked = base.ImageViewer.FitMode == FitMode.OneToOneZoom;
            }
        }

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return null;
//            return base.ImageViewer == null ? null :
//                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.ToggleOneToOneZoomMode);
        }

        #endregion

        #region OneToOneZoomToolStripMenuItem Events

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
                base.OnClick(e);

                if (base.ImageViewer != null)
                {
                    // If the fit mode is currently OneToOneZoom then set fit mode to none
                    // otherwise set the fit mode to OneToOneZoom
                    base.ImageViewer.FitMode = base.ImageViewer.FitMode == FitMode.OneToOneZoom ?
                        FitMode.None : FitMode.OneToOneZoom;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI36772", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        #endregion

        #region OneToOneZoomToolStripMenuItem Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.FitModeChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.FitModeChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.FitModeChanged"/> event.</param>
        private void HandleFitModeChanged(object sender, FitModeChangedEventArgs e)
        {
            try
            {
                SetCheckedState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI36773", ex);
            }
        }

        #endregion

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="OneToOneZoomToolStripMenuItem"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="OneToOneZoomToolStripMenuItem"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="OneToOneZoomToolStripMenuItem"/> is 
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
                        base.ImageViewer.FitModeChanged -= HandleFitModeChanged;
                    }

                    // Call the base class set property
                    base.ImageViewer = value;

                    // Set the checked state
                    SetCheckedState();

                    // Register for events
                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.FitModeChanged += HandleFitModeChanged;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI36774",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
