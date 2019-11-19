using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripSplitButton"/> that allows the user to choose between
    /// the available highlight <see cref="CursorTool"/> to be used on an 
    /// associated <see cref="ImageViewer"/> control.
    /// </summary>
    [ToolboxBitmap(typeof(HighlightToolStripSplitButton),
        ToolStripButtonConstants._HIGHLIGHT_SPLIT_BUTTON_IMAGE)]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public partial class HighlightToolStripSplitButton : ToolStripSplitButton,
        IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(HighlightToolStripSplitButton).ToString();

        #endregion Constants

        #region HighlightToolStripSplitButton Fields

        /// <summary>
        /// Holds the rectangular highlight button image.
        /// </summary>
        private static Image _rectangularHighlightButtonImage = (Image)new Bitmap(
            typeof(HighlightToolStripSplitButton),
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_IMAGE);

        /// <summary>
        /// Holds the angular highlight button image.
        /// </summary>
        private static Image _angularHighlightButtonImage = (Image)new Bitmap(
            typeof(HighlightToolStripSplitButton),
            ToolStripButtonConstants._ANGULAR_HIGHLIGHT_BUTTON_IMAGE);

        /// <summary>
        /// Holds the word highlight button image.
        /// </summary>
        private static Image _wordHighlightButtonImage = (Image)new Bitmap(
            typeof(HighlightToolStripSplitButton),
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_IMAGE);

        /// <summary>
        /// Image viewer with which this button connects.
        /// </summary>
        private ImageViewer _imageViewer;

        /// <summary>
        /// Stores the currently selected highlight tool for this button so that the highlight tool
        /// can be changed in <see cref="Control.OnClick"/> event.
        /// </summary>
        private CursorTool _highlightTool;

        #endregion

        #region HighlightToolStripSplitButton Constructors

        /// <summary>
        /// Initializes a new <see cref="HighlightToolStripSplitButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public HighlightToolStripSplitButton()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23099",
					_OBJECT_NAME);

                InitializeComponent();

                // Set the tooltip text
                base.ToolTipText = ToolStripButtonConstants._HIGHLIGHT_SPLIT_BUTTON_TOOL_TIP;

                // Set the button text
                base.Text = ToolStripButtonConstants._HIGHLIGHT_SPLIT_BUTTON_TEXT;

                // Clear the collection
                CollectionMethods.ClearAndDisposeObjects(base.DropDownItems);

                // Add the angular and rectangular highlight images into the button
                base.DropDownItems.Add(ToolStripButtonConstants._ANGULAR_HIGHLIGHT_BUTTON_TOOL_TIP,
                    _angularHighlightButtonImage);
                base.DropDownItems.Add(ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_TOOL_TIP,
                    _rectangularHighlightButtonImage);

                // Set the button image to the last used highlight tool
                _highlightTool = RegistryManager.GetLastUsedHighlightTool();
                base.Image = _highlightTool == CursorTool.RectangularHighlight ?
                    _rectangularHighlightButtonImage : _angularHighlightButtonImage;

                // Disable button by default
                base.Enabled = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23100", ex);
            }
        }

        #endregion

        #region HighlightToolStripSplitButton Overrides

        /// <summary>
        /// Gets the image that is displayed on the <see cref="HighlightToolStripSplitButton"/>.
        /// </summary>
        /// <value>The set property has been obsoleted and disabled.</value>
        /// <returns>The image that is displayed on the 
        /// <see cref="HighlightToolStripSplitButton"/>.
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

        #region HighlightToolStripSplitButton Methods

        /// <summary>
        /// Sets the enabled state of the <see cref="HighlightToolStripSplitButton"/>
        /// </summary>
        private void SetEnabledState()
        {
            base.Enabled = _imageViewer != null && _imageViewer.IsImageAvailable
                && _imageViewer.AllowHighlight;
        }

        /// <summary>
        /// Sets the tool tip text for the button including the keyboard shortcut.
        /// </summary>
        private void SetToolTipText()
        {
            // Get the original tool tip text
            string toolTipText = ToolStripButtonConstants._HIGHLIGHT_SPLIT_BUTTON_TEXT;

            // Check if an image viewer is attached
            if (_imageViewer != null)
	        {
		        // Get the shortcut keys associated with this tool
                Keys[] keys = _imageViewer.Shortcuts.GetKeys(_imageViewer.ToggleHighlightTool);
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
        /// Sets the image displayed on the <see cref="HighlightToolStripSplitButton"/>.
        /// </summary>
        /// <param name="image">The image displayed on the 
        /// <see cref="HighlightToolStripSplitButton"/>.</param>
        void SetImage(Image image)
        {
            base.Parent.SuspendLayout();
            base.Image = image;
            base.Parent.ResumeLayout();
        }

        #endregion HighlightToolStripSplitButton Methods

        #region HighlightToolStripSplitButton OnEvents

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

                // Only allow setting the tool if there is an image viewer
                // and it allows highlights
                if (_imageViewer != null && _imageViewer.AllowHighlight)
                {
                    _imageViewer.CursorTool = _highlightTool;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21376", ex);
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

                string text = e.ClickedItem.Text;

                if (text == ToolStripButtonConstants._ANGULAR_HIGHLIGHT_BUTTON_TOOL_TIP)
                {
                    _highlightTool = CursorTool.AngularHighlight;
                }
                else if (text == ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_TOOL_TIP)
                {
                    _highlightTool = CursorTool.RectangularHighlight;
                }
                else
                {
                    ExtractException ee = new ExtractException("ELI21377",
                        "Unrecognized highlight tool.");
                    ee.AddDebugData("Tool text", e.ClickedItem.Text, false);
                    throw ee;
                }

                if (_imageViewer != null)
                {
                    _imageViewer.CursorTool = _highlightTool;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI21378", ex);
            }
        }

        #endregion

        #region HighlightToolStripSplitButton Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        private void HandleImageChanged(object sender, ImageFileChangedEventArgs e)
        {
            // Set the enabled state
            SetEnabledState();
        }

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
                ExtractException ee = ExtractException.AsExtractException("ELI21579", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.CursorToolChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.CursorToolChanged"/> event.</param>
        private void HandleCursorToolChanged(object sender, CursorToolChangedEventArgs e)
        {
            try
            {
                // Adjust both the image and the last tool when the cursor tool changes
                // [DNRCAU #458]
                switch (e.CursorTool)
                {
                    case CursorTool.AngularHighlight:
                        SetImage(_angularHighlightButtonImage);
                        _highlightTool = CursorTool.AngularHighlight;
                        break;

                    case CursorTool.RectangularHighlight:
                        SetImage(_rectangularHighlightButtonImage);
                        _highlightTool = CursorTool.RectangularHighlight;
                        break;

                    case CursorTool.WordHighlight:
                        SetImage(_wordHighlightButtonImage);
                        _highlightTool = CursorTool.RectangularHighlight;
                        break;

                    default:
                        // Do nothing, only concerned with the highlight cursor tools
                        break;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23214", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.AllowHighlightStatusChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleAllowHighlightStatusChanged(object sender, EventArgs e)
        {
            try
            {
                SetEnabledState();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30225", ex);
            }
        }

        #endregion

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer with which to establish a connection.
        /// </summary>
        /// <value>The image viewer with which to establish a connection. <see langword="null"/> 
        /// indicates the connection should be disconnected from the current image viewer.</value>
        /// <returns>The image viewer with which a connection is established. 
        /// <see langword="null"/> if no image viewer is connected.</returns>
        /// <seealso cref="IImageViewerControl"/>
        public ImageViewer ImageViewer
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
                        _imageViewer.ImageFileChanged -= HandleImageChanged;
                        _imageViewer.CursorToolChanged -= HandleCursorToolChanged;
                        _imageViewer.Shortcuts.ShortcutKeyChanged -= HandleShortcutKeyChanged;
                        _imageViewer.AllowHighlightStatusChanged -= HandleAllowHighlightStatusChanged;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Check if an image viewer was specified
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged += HandleImageChanged;
                        _imageViewer.CursorToolChanged += HandleCursorToolChanged;
                        _imageViewer.Shortcuts.ShortcutKeyChanged += HandleShortcutKeyChanged;
                        _imageViewer.AllowHighlightStatusChanged += HandleAllowHighlightStatusChanged;
                    }

                    // Set the button state
                    SetEnabledState();

                    // Set the tool tip text
                    SetToolTipText();

                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21379",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
