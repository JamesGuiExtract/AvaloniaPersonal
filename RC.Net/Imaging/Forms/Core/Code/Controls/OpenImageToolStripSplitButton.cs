using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripSplitButton"/> that allows the user to open an image file 
    /// on an associated <see cref="ImageViewer"/> control.
    /// </summary>
    [ToolboxBitmap(typeof(OpenImageToolStripSplitButton),
        ToolStripButtonConstants._OPEN_IMAGE_BUTTON_IMAGE)]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public partial class OpenImageToolStripSplitButton : ToolStripSplitButton, IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(OpenImageToolStripSplitButton).ToString();

        #endregion Constants

        #region OpenImageToolStripSplitButton Fields

        /// <summary>
        /// The image viewer to which the <see cref="OpenImageToolStripSplitButton"/> is 
        /// connected.
        /// </summary>
        IDocumentViewer _imageViewer;

        #endregion

        #region OpenImageToolStripSplitButton Constructors

        /// <summary>
        /// Initializes a new <see cref="OpenImageToolStripSplitButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public OpenImageToolStripSplitButton()
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel()); 
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23103",
					_OBJECT_NAME);

                InitializeComponent();

                // Set the button's icon
                base.Image = (Image)new Bitmap(typeof(OpenImageToolStripSplitButton),
                    ToolStripButtonConstants._OPEN_IMAGE_BUTTON_IMAGE);

                // Set the button's tool tip text
                base.ToolTipText = ToolStripButtonConstants._OPEN_IMAGE_BUTTON_TOOL_TIP;

                // Disable the event by default
                base.Enabled = false;

                // Get rid of the image margin
                ToolStripDropDownMenu dropDown = base.DropDown as ToolStripDropDownMenu;
                if (dropDown != null)
                {
                    dropDown.ShowImageMargin = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23104", ex);
            }
        }

        #endregion

        #region OpenImageToolStripSplitButton Methods

        /// <summary>
        /// Sets the <see cref="ToolStripButton"/>'s enabled state.
        /// </summary>
        private void SetEnabledState()
        {
            base.Enabled = _imageViewer != null;
        }

        /// <summary>
        /// Sets the tool tip text for the button including the keyboard shortcut.
        /// </summary>
        private void SetToolTipText()
        {
            // Get the original tool tip text
            string toolTipText = ToolStripButtonConstants._OPEN_IMAGE_BUTTON_TOOL_TIP;

            // Check if an image viewer is attached
            if (_imageViewer != null)
	        {
		        // Get the shortcut keys associated with this tool
                Keys[] keys = _imageViewer.Shortcuts.GetKeys(_imageViewer.SelectOpenImage);
                string shortcuts = ShortcutsManager.GetDisplayString(keys);
                if (shortcuts.Length != 0)
                {
                    toolTipText += " (" + shortcuts + ")";
                }
	        }

            // Set the tool tip text
            base.ToolTipText = toolTipText;
        }

        /// <summary>
        /// Populates the MRU image file list drop down with the current MRU image file list
        /// from the registry.
        /// </summary>
        private void LoadMruList()
        {
            // Clear the drop down list
            CollectionMethods.ClearAndDisposeObjects(base.DropDownItems);

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

        #endregion

        #region OpenImageToolStripSplitButton Overrides

        /// <summary>
        /// Gets the image that is displayed on the <see cref="OpenImageToolStripSplitButton"/>.
        /// </summary>
        /// <value>The set property has been obsoleted and disabled.</value>
        /// <returns>The image that is displayed on the 
        /// <see cref="OpenImageToolStripSplitButton"/>.
        /// </returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Image Image
        {
            get
            {
                return base.Image;
            }
            set
            {
                // Prevent the image from being modified
            }
        }

        #endregion

        #region OpenImageToolStripSplitButton OnEvents

        /// <summary>
        /// Raises the <see cref="ToolStripSplitButton.ButtonClick"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnButtonClick(EventArgs e)
        {
            try
            {
                // Ensure the drop down gets hidden [DotNetRCAndUtils #49]
                base.HideDropDown();

                // Display a dialog and allow the user to open an image file.
                _imageViewer.SelectOpenImage();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21269", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                // Notify the base class
                base.OnButtonClick(e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ToolStripDropDownItem.ShowDropDown"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnDropDownShow(EventArgs e)
        {
            try
            {
                if (!base.DesignMode)
                {
                    // Update the MRU list
                    LoadMruList();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI21270", ex);
            }

            // Notify the base class
            base.OnDropDownShow(e);
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
                // Ensure the drop down gets hidden [DotNetRCAndUtils #49]
                base.HideDropDown();

                // Notify the base class
                base.OnDropDownItemClicked(e);

                // Open the file name stored on the menu item if and only if 
                // this button is connected to an image viewer control.
                if (_imageViewer != null)
                {
                    // Open the image
                    _imageViewer.OpenImage(e.ClickedItem.Text, true);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21271", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion

        #region OpenImageToolStripSplitButton Event Handlers

        /// <summary>
        /// Handles the <see cref="ShortcutsManager.ShortcutKeyChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ShortcutsManager.ShortcutKeyChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ShortcutsManager.ShortcutKeyChanged"/> event.</param>
        private void HandleShortcutKeyChanged(object sender, ShortcutKeyChangedEventArgs e)
        {
            try
            {
                SetToolTipText();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21576", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="OpenImageToolStripSplitButton"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="OpenImageToolStripSplitButton"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="OpenImageToolStripSplitButton"/> is 
        /// connected. <see langword="null"/> if no connections are established.</returns>
        [Browsable(false)]
        [CLSCompliant(false)]
        public IDocumentViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
            set
            {
                try
                {
                    // Unregister from previously subscribed-to events
                    if (_imageViewer != null)
                    {
                        _imageViewer.Shortcuts.ShortcutKeyChanged -= HandleShortcutKeyChanged;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.Shortcuts.ShortcutKeyChanged += HandleShortcutKeyChanged;
                    }

                    // Update enabled button state
                    SetEnabledState();
                    
                    // Update the tooltip text
                    SetToolTipText();
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21223",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
