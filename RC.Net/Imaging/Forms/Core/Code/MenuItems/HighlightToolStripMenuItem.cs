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
    /// Represents a <see cref="ToolStripMenuItem"/> that allows the user to iterate through
    /// the available highlight <see cref="CursorTool"/> to be used on an 
    /// associated <see cref="ImageViewer"/> control.
    /// </summary>
    [ToolboxBitmap(typeof(HighlightToolStripMenuItem),
        ToolStripButtonConstants._HIGHLIGHT_SPLIT_BUTTON_IMAGE)]
    public partial class HighlightToolStripMenuItem : ToolStripMenuItem, IImageViewerControl
    {
        #region HighlightToolStripMenuItem Fields

        /// <summary>
        /// Holds the rectangular highlight button image.
        /// </summary>
        private static Image _rectangularHighlightButtonImage = (Image)new Bitmap(
            typeof(HighlightToolStripMenuItem),
            ToolStripButtonConstants._RECTANGULAR_HIGHLIGHT_BUTTON_IMAGE);

        /// <summary>
        /// Holds the angular highlight button image.
        /// </summary>
        private static Image _angularHighlightButtonImage = (Image)new Bitmap(
            typeof(HighlightToolStripMenuItem),
            ToolStripButtonConstants._ANGULAR_HIGHLIGHT_BUTTON_IMAGE);

        /// <summary>
        /// Holds the word highlight button image.
        /// </summary>
        private static Image _wordHighlightButtonImage = (Image)new Bitmap(
            typeof(HighlightToolStripMenuItem),
            ToolStripButtonConstants._WORD_HIGHLIGHT_BUTTON_IMAGE);

        /// <summary>
        /// Image viewer with which this menu item connects.
        /// </summary>
        private ImageViewer _imageViewer;

        /// <summary>
        /// Stores the currently selected highlight tool for this button so that the highlight tool
        /// can be changed in <see cref="Control.OnClick"/> event.
        /// </summary>
        private CursorTool _highlightTool;

        #endregion

        #region HighlightToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="HighlightToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public HighlightToolStripMenuItem()
        {
            InitializeComponent();

            // Set the tooltip text
            base.ToolTipText = ToolStripButtonConstants._HIGHLIGHT_SPLIT_BUTTON_TOOL_TIP;

            // Set the menu item text
            base.Text = ToolStripButtonConstants._HIGHLIGHT_SPLIT_BUTTON_TEXT;

            // Set the display style to both image and text
            base.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;

            // Get the last used highlight tool
            _highlightTool = RegistryManager.GetLastUsedHighlightTool();

            // Set the image based on the last used highlight tool
            switch (_highlightTool)
            {
                case CursorTool.AngularHighlight:
                    base.Image = _angularHighlightButtonImage;
                    break;

                case CursorTool.RectangularHighlight:
                    base.Image = _rectangularHighlightButtonImage;
                    break;

                case CursorTool.WordHighlight:
                    base.Image = _wordHighlightButtonImage;
                    break;

                default:
                    throw new ExtractException("ELI31306", "Unexpected cursor tool");
            }

            // Disable menu item by default
            base.Enabled = false;
        }

        #endregion

        #region HighlightToolStripMenuItem Methods

        /// <summary>
        /// Sets the enabled and checked state of the <see cref="HighlightToolStripMenuItem"/>
        /// </summary>
        private void SetMenuItemState()
        {
            base.Enabled = _imageViewer != null && _imageViewer.IsImageAvailable
                && _imageViewer.AllowHighlight;
            base.Checked = _imageViewer != null &&
                (_imageViewer.CursorTool == CursorTool.AngularHighlight ||
                _imageViewer.CursorTool == CursorTool.RectangularHighlight);
        }

        /// <summary>
        /// Sets the shortcut key text for the menu item.
        /// </summary>
        private void SetShortcutKeys()
        {
            if (_imageViewer == null)
            {
                base.ShortcutKeyDisplayString = "";
            }
            else
            {
                // Get the shortcut keys associated with this menu item
                Keys[] shortcutKeys = 
                    _imageViewer.Shortcuts.GetKeys(_imageViewer.ToggleHighlightTool);

                // Display the shortcut key text
                base.ShortcutKeyDisplayString = ShortcutsManager.GetDisplayString(shortcutKeys);
            }
        }

        #endregion

        #region HighlightToolStripMenuItem OnEvents

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event. 
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        /// <seealso cref="Control.OnClick"/>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                if (_imageViewer != null && _imageViewer.AllowHighlight)
                {
                    // If the current cursor tool is one of the highlight tools, set
                    // the _cursorTool value to the opposite tool
                    if (_imageViewer.CursorTool == CursorTool.AngularHighlight)
                    {
                        _highlightTool = CursorTool.RectangularHighlight;
                    }
                    else if (_imageViewer.CursorTool == CursorTool.RectangularHighlight)
                    {
                        _highlightTool = CursorTool.AngularHighlight;
                    }

                    // Set the cursor tool to the appropriate highlight tool
                    _imageViewer.CursorTool = _highlightTool;

                    // Update the menu item state
                    SetMenuItemState();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21571", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region HighlightToolStripMenuItem Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        private void HandleImageChanged(object sender, ImageFileChangedEventArgs e)
        {
            // Set the menu item state
            SetMenuItemState();
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
            switch (e.CursorTool)
            {
                case CursorTool.AngularHighlight:
                    base.Image = _angularHighlightButtonImage;
                    break;

                case CursorTool.RectangularHighlight:
                    base.Image = _rectangularHighlightButtonImage;
                    break;

                case CursorTool.WordHighlight:
                    base.Image = _wordHighlightButtonImage;
                    break;

                default:
                    // Do nothing, only concerned with the highlight cursor tools
                    break;
            }

            // Update the menu item state
            SetMenuItemState();
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
                SetShortcutKeys();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21914", ex);
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
                SetMenuItemState();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30233", ex);
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

                    // Set the menu item state
                    SetMenuItemState();

                    // Set the shortcut key
                    SetShortcutKeys();
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21573",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
