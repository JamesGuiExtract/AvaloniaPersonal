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
    /// to width command.
    /// </summary>
    [ToolboxBitmap(typeof(FitToWidthToolStripMenuItem),
        ToolStripButtonConstants._FIT_TO_WIDTH_BUTTON_IMAGE)]
    public partial class FitToWidthToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region FitToWidthToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="FitToWidthToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public FitToWidthToolStripMenuItem()
            : base(ToolStripButtonConstants._FIT_TO_WIDTH_MENU_ITEM_TEXT,
            ToolStripButtonConstants._FIT_TO_WIDTH_BUTTON_IMAGE,
            typeof(FitToWidthToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region FitToWidthToolStripMenuItem Overrides

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
                // If the fit mode has changed to FitToWidth then set this button as checked
                base.Checked = base.ImageViewer.FitMode == FitMode.FitToWidth;
            }
        }

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.ToggleFitToWidthMode);
        }

        #endregion

        #region FitToWidthToolStripMenuItem Events

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
                    // If the fit mode is currently FitToWidth then set fit mode to none
                    // otherwise set the fit mode to FitToWidth
                    base.ImageViewer.FitMode = base.ImageViewer.FitMode == FitMode.FitToWidth ?
                        FitMode.None : FitMode.FitToWidth;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21569", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        #endregion

        #region FitToPageToolStripMenuItem Event Handlers

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
                ExtractException.Display("ELI22522", ex);
            }
        }

        #endregion

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="FitToWidthToolStripMenuItem"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="FitToWidthToolStripMenuItem"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="FitToWidthToolStripMenuItem"/> is 
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
                    ExtractException ee = new ExtractException("ELI22523",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion

    }
}
