using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ImageViewerCommandToolStripMenuItem"/> that activates the next
    /// page command.
    /// </summary>
    [ToolboxBitmap(typeof(PageNavigationToolStripMenuItem),
        ToolStripButtonConstants._PAGE_NAVIGATION_TEXTBOX_IMAGE)]
    public partial class PageNavigationToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region PageNavigationToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="PageNavigationToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PageNavigationToolStripMenuItem()
            : base(ToolStripButtonConstants._PAGE_NAVIGATION_MENUITEM_TEXT,
            ToolStripButtonConstants._PAGE_NAVIGATION_TEXTBOX_IMAGE,
            typeof(PageNavigationToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region PageNavigationToolStripMenuItem Overrides

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Get the image viewer this control is attached to
            var imageViewer = base.ImageViewer;

            // Enable this button if an image is open and has more than 1 page 
            base.Enabled = imageViewer != null && imageViewer.IsImageAvailable
                && imageViewer.PageCount > 1;
        }

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return null;
        }

        #endregion

        #region PageNavigationToolStripMenuItem Events

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
                // Get the image viewer
                var imageViewer = base.ImageViewer;

                if (imageViewer != null && imageViewer.IsImageAvailable
                    && imageViewer.PageCount > 1)
                {
                    // Get the parent form
                    Control parent = ((Control)imageViewer).Parent;
                    while (parent != null && !(parent is Form))
                    {
                        parent = parent.Parent;
                    }

                    // Loop until the user cancels the dialog or enters a valid page number
                    while (true)
                    {
                        // Display the input box
                        string result = "";
                        if (Extract.Utilities.Forms.InputBox.Show(parent, "Enter page number (1 - "
                            + imageViewer.PageCount.ToString(CultureInfo.CurrentCulture)
                            + "):", "Goto Page", ref result) == DialogResult.Cancel)
                        {
                            // User canceled, break out of loop
                            break;
                        }

                        // Try to parse the user entry as an integer
                        int pageNumber = 0;

                        // Check if valid page number, if not show message to user
                        // and repeat the loop
                        if (!Int32.TryParse(result, out pageNumber)
                            || pageNumber > imageViewer.PageCount
                            || pageNumber < 1)
                        {
                            MessageBox.Show("Please enter a valid page number!",
                                "Invalid page number", MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                        }
                        else
                        {
                            // Valid page number, move to page and break from loop
                            imageViewer.PageNumber = pageNumber;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21969", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion
    }
}
