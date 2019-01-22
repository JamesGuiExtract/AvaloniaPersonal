using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripButton"/> that enables the rectangular highlight 
    /// <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(RectangularHighlightToolStripButton), 
        ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_IMAGE)]
    public partial class RectangularHighlightToolStripButton : ImageViewerCursorToolStripButton
    {
        /// <summary>
        /// Initializes a new <see cref="RectangularHighlightToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public RectangularHighlightToolStripButton()
            : base(CursorTool.RectangularHighlight,
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_TEXT,
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_IMAGE, 
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_TOOL_TIP, 
            typeof(RectangularHighlightToolStripButton))
        {
            // Initialize the component
            InitializeComponent();
        }

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
                // Update whether or not the cursor tool can change on click
                AllowCursorToolChangeOnClick = ImageViewer != null && ImageViewer.AllowHighlight;

                // Call the base class
                base.OnClick(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30214", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.AllowHighlightStatusChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleAllowHighlightStatusChanged(object sender, EventArgs e)
        {
            try
            {
                SetButtonState(true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30226", ex);
            }
        }

        /// <summary>
        /// Sets the <see cref="ToolStripItem.Enabled"/> and <see cref="ToolStripButton.Checked"/> 
        /// properties depending on the state of the associated image viewer control.
        /// <para><b>Note:</b></para>
        /// This override always updates the enabled state based on the current state
        /// and whether <see cref="Extract.Imaging.Forms.DocumentViewer.AllowHighlight"/> is <see langword="true"/>
        /// or not.
        /// </summary>
        /// <param name="updateEnabledStatus"><see langword="true"/> to update the enabled status
        /// of the button based on whether an image is open; <see langword="false"/> to update only
        /// the checked state of the button.</param>
        protected override void SetButtonState(bool updateEnabledStatus)
        {
            base.SetButtonState(updateEnabledStatus);

            // Update the enabled state based on whether highlighting is allowed or not
            Enabled = Enabled && ImageViewer.AllowHighlight;
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

        /// <summary>
        /// Gets or sets the image viewer with which to establish a connection.
        /// </summary>
        /// <value>The image viewer with which to establish a connection. <see langword="null"/> 
        /// indicates the connection should be disconnected from the current image viewer.</value>
        /// <returns>The image viewer with which a connection is established. 
        /// <see langword="null"/> if no image viewer is connected.</returns>
        /// <seealso cref="IImageViewerControl"/>
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
                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.AllowHighlightStatusChanged -= HandleAllowHighlightStatusChanged;
                    }

                    base.ImageViewer = value;

                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.AllowHighlightStatusChanged += HandleAllowHighlightStatusChanged;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30218", ex);
                }
            }
        }
    }
}
