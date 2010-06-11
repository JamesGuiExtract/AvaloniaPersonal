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
    /// the available redaction <see cref="CursorTool"/> to be used on an 
    /// associated <see cref="ImageViewer"/> control.
    /// </summary>
    [ToolboxBitmap(typeof(RedactionToolStripSplitButton),
        ToolStripButtonConstants._REDACTION_SPLIT_BUTTON_IMAGE)]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public partial class RedactionToolStripSplitButton : ToolStripSplitButton,
        IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(RedactionToolStripSplitButton).ToString();

        #endregion Constants

        #region RedactionToolStripSplitButton Fields

        /// <summary>
        /// Holds the rectangular redaction button image.
        /// </summary>
        private static Image _rectangularRedactionButtonImage = (Image)new Bitmap(
            typeof(RedactionToolStripSplitButton),
            ToolStripButtonConstants._RECTANGULAR_REDACTION_BUTTON_IMAGE);

        /// <summary>
        /// Holds the angular redaction button image.
        /// </summary>
        private static Image _angularRedactionButtonImage = (Image)new Bitmap(
            typeof(RedactionToolStripSplitButton),
            ToolStripButtonConstants._ANGULAR_REDACTION_BUTTON_IMAGE);

        /// <summary>
        /// Image viewer with which this button connects.
        /// </summary>
        private ImageViewer _imageViewer;

        /// <summary>
        /// Stores the currently selected redaction tool for this button so that the redaction tool
        /// can be changed in <see cref="Control.OnClick"/> event.
        /// </summary>
        private CursorTool _redactionTool;

        #endregion

        #region RedactionToolStripSplitButton Constructors

        /// <summary>
        /// Initializes a new <see cref="RedactionToolStripSplitButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public RedactionToolStripSplitButton()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23105",
					_OBJECT_NAME);

                InitializeComponent();

                // Set the tooltip text
                base.ToolTipText = ToolStripButtonConstants._REDACTION_SPLIT_BUTTON_TOOL_TIP;

                // Set the button text
                base.Text = ToolStripButtonConstants._REDACTION_SPLIT_BUTTON_TEXT;

                // Clear the collection
                CollectionMethods.ClearAndDisposeObjects(base.DropDownItems);

                // Add the angular and rectangular redaction images into the button
                base.DropDownItems.Add(ToolStripButtonConstants._ANGULAR_REDACTION_BUTTON_TOOL_TIP,
                    _angularRedactionButtonImage);
                base.DropDownItems.Add(ToolStripButtonConstants._RECTANGULAR_REDACTION_BUTTON_TOOL_TIP,
                    _rectangularRedactionButtonImage);

                // Set the button image to the last used redaction tool
                _redactionTool = RegistryManager.GetLastUsedRedactionTool();
                base.Image = _redactionTool == CursorTool.RectangularRedaction ?
                    _rectangularRedactionButtonImage : _angularRedactionButtonImage;

                // Disable button by default
                base.Enabled = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23106", ex);
            }
        }

        #endregion

        #region RedactionToolStripSplitButton Overrides

        /// <summary>
        /// Gets the image that is displayed on the <see cref="RedactionToolStripSplitButton"/>.
        /// </summary>
        /// <value>The set property has been obsoleted and disabled.</value>
        /// <returns>The image that is displayed on the 
        /// <see cref="RedactionToolStripSplitButton"/>.
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

        #region RedactionToolStripSplitButton Methods

        /// <summary>
        /// Sets the enabled state of the <see cref="RedactionToolStripSplitButton"/>
        /// </summary>
        private void SetEnabledState()
        {
            base.Enabled = _imageViewer != null && _imageViewer.IsImageAvailable;
        }

        /// <summary>
        /// Sets the tool tip text for the button including the keyboard shortcut.
        /// </summary>
        private void SetToolTipText()
        {
            // Get the original tool tip text
            string toolTipText = ToolStripButtonConstants._REDACTION_SPLIT_BUTTON_TEXT;

            // Check if an image viewer is attached
            if (_imageViewer != null)
	        {
		        // Get the shortcut keys associated with this tool
                Keys[] keys = _imageViewer.Shortcuts.GetKeys(_imageViewer.ToggleRedactionTool);
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
        /// Sets the image displayed on the <see cref="RedactionToolStripSplitButton"/>.
        /// </summary>
        /// <param name="image">The image displayed on the 
        /// <see cref="RedactionToolStripSplitButton"/>.</param>
        void SetImage(Image image)
        {
            base.Parent.SuspendLayout();
            base.Image = image;
            base.Parent.ResumeLayout();
        }

        #endregion RedactionToolStripSplitButton Methods

        #region RedactionToolStripSplitButton OnEvents

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
                    _imageViewer.CursorTool = _redactionTool;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22271", ex);
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

                if (text == ToolStripButtonConstants._ANGULAR_REDACTION_BUTTON_TOOL_TIP)
                {
                    _redactionTool = CursorTool.AngularRedaction;
                }
                else if (text == ToolStripButtonConstants._RECTANGULAR_REDACTION_BUTTON_TOOL_TIP)
                {
                    _redactionTool = CursorTool.RectangularRedaction;
                }
                else
                {
                    ExtractException ee = new ExtractException("ELI22272",
                        "Unrecognized redaction tool.");
                    ee.AddDebugData("Tool text", e.ClickedItem.Text, false);
                    throw ee;
                }

                if (_imageViewer != null)
                {
                    _imageViewer.CursorTool = _redactionTool;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22273", ex);
            }
        }

        #endregion

        #region RedactionToolStripSplitButton Event Handlers

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
                ExtractException ee = ExtractException.AsExtractException("ELI22274", ex);
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
                switch (e.CursorTool)
                {
                    case CursorTool.AngularRedaction:
                        SetImage(_angularRedactionButtonImage);
                        break;

                    case CursorTool.RectangularRedaction:
                        SetImage(_rectangularRedactionButtonImage);
                        break;

                    default:
                        // Do nothing, only concerned with the redaction cursor tools
                        break;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23213", ex);
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
                throw ExtractException.AsExtractException("ELI30228", ex);
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
                    ExtractException ee = new ExtractException("ELI22275",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}

