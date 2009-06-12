using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ImageViewerCommandToolStripMenuItem"/> that displays the
    /// most recently used image file list.
    /// </summary>
    [ToolboxBitmap(typeof(OpenImageMruToolStripMenuItem),
        ToolStripButtonConstants._OPEN_IMAGE_MRU_MENU_ITEM_IMAGE)]
    public partial class OpenImageMruToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region OpenImageMruToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="OpenImageMruToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public OpenImageMruToolStripMenuItem()
            : base(ToolStripButtonConstants._OPEN_IMAGE_MRU_MENU_ITEM_TEXT,
            ToolStripButtonConstants._OPEN_IMAGE_MRU_MENU_ITEM_IMAGE,
            typeof(OpenImageMruToolStripMenuItem))
        {
            InitializeComponent();

            // Populate the MRU list
            LoadMruList();
        }

        #endregion

        #region OpenImageMruToolStripMenuItem Methods

        /// <summary>
        /// Populates the MRU image file list drop down with the current MRU image file list
        /// from the registry.
        /// </summary>
        private void LoadMruList()
        {
            // Clear the drop down list
            CollectionMethods.ClearAndDispose(base.DropDownItems);

            // Get the MRU list from the registry
            List<string> mruList = RegistryManager.GetMostRecentlyUsedImageFiles();

            // Populate the drop down menu
            foreach (string fileName in mruList)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    base.DropDownItems.Add(
                        new ToolStripMenuItem(fileName, null, null, fileName));
                }
            }
        }

        /// <summary>
        /// Hides the MruList drop down and all parent menu items.
        /// <para><b>Note:</b></para>
        /// This method will not throw any exceptions, it will log them
        /// internally, since its primary function is to hide the MRU list
        /// drop down before displaying an exception in a catch handler.
        /// </summary>
        private void HideMruList()
        {
            try
            {
                base.HideDropDown();

                // Also need to ensure that any parent menu items are hidden.
                // Iterate through parent controls until we find something that is
                // not a tool strip menu item hiding the menu items as we go.
                ToolStripMenuItem item = base.OwnerItem as ToolStripMenuItem;
                while (item != null)
                {
                    // Hide the menu item
                    item.HideDropDown();

                    // Check if owner is a tool strip menu item
                    item = item.OwnerItem as ToolStripMenuItem;
                }
            }
            catch (Exception ex)
            {
                // Just log any exception, do not want to throw an exception
                // from this method as it is used inside of catch handlers
                ExtractException ee = ExtractException.AsExtractException("ELI21792", ex);
                ee.Log();
            }
        }

        #endregion

        #region OpenImageMruToolStripMenuItem Overrides

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Enable this button if there is an image viewer
            base.Enabled = base.ImageViewer != null;
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

        #region OpenImageMruToolStripMenuItem Events

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDropDownShow(EventArgs e)
        {
            base.OnDropDownShow(e);

            try
            {
                if (!base.DesignMode)
                {
                    // Populate the MRU list
                    LoadMruList();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21580", ex);
                ee.AddDebugData("Event args", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="ToolStripDropDownItem.DropDownItemClicked"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ToolStripItemClickedEventArgs"/> that contains the event 
        /// data.</param>
        protected override void OnDropDownItemClicked(ToolStripItemClickedEventArgs e)
        {
            try
            {
                // Notify the base class
                base.OnDropDownItemClicked(e);

                // Open the file name stored on the menu item if and only if 
                // this button is connected to an image viewer control.
                if (base.ImageViewer != null)
                {
                    // Open the image
                    base.ImageViewer.OpenImage(e.ClickedItem.Text, true);
                }
            }
            catch (ExtractException ee)
            {
                // Hide the MRU list before displaying the exception
                // [DotNetRCAndUtils #108] - JDS - 07/23/2008
                HideMruList();

                ee.Display();
            }
            catch (Exception ex)
            {
                // Hide the MRU list before displaying the exception
                // [DotNetRCAndUtils #108] - JDS - 07/23/2008
                HideMruList();

                // Wrap the exception as an extract exception and display it
                ExtractException ee = ExtractException.AsExtractException("ELI21962", ex);
                ee.AddDebugData("Event Args", e, false);
                ee.Display();
            }
        }

        #endregion
    }
}
