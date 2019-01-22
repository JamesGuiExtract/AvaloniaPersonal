using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripButton"/> that activates the Rotate clockwise command.
    /// </summary>
    [ToolboxBitmap(typeof(InvertColorsToolStripButton),
        ToolStripButtonConstants._INVERT_COLOR_BUTTON_IMAGE)]
    public partial class InvertColorsToolStripButton : ImageViewerCommandToolStripButton
    {
        #region InvertColorsToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="InvertColorsToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public InvertColorsToolStripButton()
            : base(ToolStripButtonConstants._INVERT_COLOR_BUTTON_IMAGE,
            "Invert image colors",
            typeof(InvertColorsToolStripButton),
            "Invert image colors")
        {
            // Initialize the component
            InitializeComponent();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="InvertColorsToolStripButton"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="InvertColorsToolStripButton"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="InvertColorsToolStripButton"/> is 
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
                        base.ImageViewer.InvertColorsStatusChanged -= HandleInvertColorsStatusChanged;
                    }

                    // Call the base class set property
                    base.ImageViewer = value;

                    // Set the checked state
                    SetCheckedState();

                    // Register for events
                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.InvertColorsStatusChanged += HandleInvertColorsStatusChanged;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI36801",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="E:ImageViewer.InvertColorsStatusChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleInvertColorsStatusChanged(object sender, EventArgs e)
        {
            try
            {
                // Set the state of the toggle depending on the state of ImageViewer.InvertColors.
                SetCheckedState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36802");
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

        #endregion Overrides

        #region InvertColorsToolStripButton Events

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
                if (ImageViewer != null && ImageViewer.IsImageAvailable)
                {
                    ImageViewer.InvertColors = !ImageViewer.InvertColors;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36803");
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion InvertColorsToolStripButton Events

        #region Private Members

        /// <summary>
        /// Sets the state of the toggle depending on the state of
        /// <see cref="P:ImageViewer.InvertColors"/>.
        /// </summary>
        void SetCheckedState()
        {
            if (base.ImageViewer != null)
            {
                // If the ImageViewer is currently inverting colors, set the button as checked.
                base.Checked = base.ImageViewer.InvertColors;
            }
        }

        #endregion Private Members
    }
}
