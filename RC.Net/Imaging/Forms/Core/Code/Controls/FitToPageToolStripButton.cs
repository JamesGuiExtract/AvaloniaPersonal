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
    /// to <see cref="FitMode.FitToPage"/>.
    /// </summary>
    [ToolboxBitmap(typeof(FitToPageToolStripButton), 
        ToolStripButtonConstants._FIT_TO_PAGE_BUTTON_IMAGE)]
    public partial class FitToPageToolStripButton : ImageViewerCommandToolStripButton
    {
        #region FitToPageToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="FitToPageToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public FitToPageToolStripButton()
            : base(ToolStripButtonConstants._FIT_TO_PAGE_BUTTON_IMAGE,
            ToolStripButtonConstants._FIT_TO_PAGE_BUTTON_TOOL_TIP,
            typeof(FitToPageToolStripButton),
            ToolStripButtonConstants._FIT_TO_PAGE_BUTTON_TEXT)
        {
            // Initialize the component
            InitializeComponent();
        }

        #endregion

        #region FitToPageToolStripButton Methods

        /// <summary>
        /// Sets the enabled state of <see cref="FitToPageToolStripButton"/> based on the state of 
        /// its associated <see cref="ImageViewer"/> control.
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
                // If the fit mode has changed to FitToPage then set this button as checked
                base.Checked = base.ImageViewer.FitMode == FitMode.FitToPage;
            }
        }

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.ToggleFitToPageMode);
        }

        #endregion FitToPageToolStripButton Methods

        #region FitToPageToolStripButton Events

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
                    // otherwise set the fit mode to FitToPage
                    base.ImageViewer.FitMode = base.Checked ? FitMode.None : FitMode.FitToPage;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21328", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region FitToPageToolStripButton Event Handlers

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
                ExtractException.Display("ELI21343", ex);
            }
        }

        #endregion

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="FitToPageToolStripButton"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="FitToPageToolStripButton"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="FitToPageToolStripButton"/> is 
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
                    ExtractException ee = new ExtractException("ELI21331",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
