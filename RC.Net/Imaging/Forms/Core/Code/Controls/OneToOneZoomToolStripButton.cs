using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripButton"/> that sets the <see cref="FitMode"/>
    /// to <see cref="FitMode.OneToOneZoom"/>.
    /// </summary>
    [ToolboxBitmap(typeof(OneToOneZoomToolStripButton),
        ToolStripButtonConstants._ONE_TO_ONE_ZOOM_BUTTON_IMAGE)]
    public partial class OneToOneZoomToolStripButton : ImageViewerCommandToolStripButton
    {
        #region OneToOneZoomToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="OneToOneZoomToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public OneToOneZoomToolStripButton()
            : base(ToolStripButtonConstants._ONE_TO_ONE_ZOOM_BUTTON_IMAGE,
            ToolStripButtonConstants._ONE_TO_ONE_ZOOM_BUTTON_TOOL_TIP,
            typeof(OneToOneZoomToolStripButton),
            ToolStripButtonConstants._ONE_TO_ONE_ZOOM_BUTTON_TEXT)
        {
            // Initialize the component
            InitializeComponent();
        }

        #endregion

        #region OneToOneZoomToolStripButton Methods

        /// <summary>
        /// Sets the enabled state of <see cref="OneToOneZoomToolStripButton"/> based on the state 
        /// of its associated <see cref="ImageViewer"/> control.
        /// </summary>
        protected override void SetEnabledState()
        {
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
        }

        #endregion OneToOneZoomToolStripButton Methods

        #region OneToOneZoomToolStripButton Events

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
                if (base.ImageViewer != null)
                {
                    // If this fit mode is currently selected then set fit mode to none
                    // otherwise set the fit mode to FitToWith
                    base.ImageViewer.FitMode = base.Checked ? FitMode.None : FitMode.OneToOneZoom;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI36769", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region OneToOneZoomToolStripButton Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.FitModeChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.FitModeChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.FitModeChanged"/> event.</param>
        private void HandleFitModeChanged(object sender, FitModeChangedEventArgs e)
        {
            try
            {
                SetCheckedState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI36770", ex);
            }
        }

        #endregion

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="OneToOneZoomToolStripButton"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="OneToOneZoomToolStripButton"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="OneToOneZoomToolStripButton"/> is 
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
                    ExtractException ee = new ExtractException("ELI36771",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
