using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripMenuItem"/> that activates the Print View command.
    /// </summary>
    public partial class PrintViewToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="PrintViewToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PrintViewToolStripMenuItem()
            : base(ToolStripButtonConstants._PRINT_VIEW_MENU_ITEM_TEXT,
            ToolStripButtonConstants._PRINT_VIEW_BUTTON_IMAGE_SMALL,
            typeof(PrintImageToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Events

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

                if (base.ImageViewer != null && base.ImageViewer.IsImageAvailable)
                {
                    base.ImageViewer.PrintView();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29262", ex);
            }
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectPrintView);
        }

        #endregion Methods
    }
}
