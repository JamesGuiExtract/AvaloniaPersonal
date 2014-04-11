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
                if (base.ImageViewer != null && base.ImageViewer.IsImageAvailable)
                {
                    base.ImageViewer.InvertColors();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI0");
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion InvertColorsToolStripButton Events
    }
}
