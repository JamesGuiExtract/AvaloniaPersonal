using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripButton"/> that activates the Print View command.
    /// </summary>
    public partial class PrintViewToolStripButton : ImageViewerCommandToolStripButton
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="PrintViewToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PrintViewToolStripButton()
            : base(ToolStripButtonConstants._PRINT_VIEW_BUTTON_IMAGE,
            ToolStripButtonConstants._PRINT_VIEW_BUTTON_TOOL_TIP,
            typeof(PrintImageToolStripButton),
            ToolStripButtonConstants._PRINT_VIEW_BUTTON_TEXT)
        {
            // Initialize the component
            InitializeComponent();
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> event.
        /// </param>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                base.OnClick(e);

                if (base.ImageViewer != null && base.ImageViewer.IsImageAvailable)
                {
                    // Allow print dialog to handle user's first mouse click [DotNetRCAndUtils #58]
                    Parent.Capture = false;
                    base.ImageViewer.PrintView();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29259", ex);
            }
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectPrintView);
        }

        #endregion Methods
    }
}
