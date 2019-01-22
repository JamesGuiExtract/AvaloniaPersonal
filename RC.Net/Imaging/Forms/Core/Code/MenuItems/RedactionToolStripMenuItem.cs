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
    /// the available redaction <see cref="CursorTool"/> to be used on an 
    /// associated <see cref="ImageViewer"/> control.
    /// </summary>
    [ToolboxBitmap(typeof(RedactionToolStripMenuItem),
        ToolStripButtonConstants._REDACTION_SPLIT_BUTTON_IMAGE)]
    public partial class RedactionToolStripMenuItem : ToolStripMenuItem, IImageViewerControl
    {
        #region RedactionToolStripMenuItem Fields

        /// <summary>
        /// Holds the rectangular redaction button image.
        /// </summary>
        private static Image _rectangularRedactionButtonImage = (Image)new Bitmap(
            typeof(RedactionToolStripMenuItem),
            ToolStripButtonConstants._RECTANGULAR_REDACTION_BUTTON_IMAGE);

        /// <summary>
        /// Holds the angular redaction button image.
        /// </summary>
        private static Image _angularRedactionButtonImage = (Image)new Bitmap(
            typeof(RedactionToolStripMenuItem),
            ToolStripButtonConstants._ANGULAR_REDACTION_BUTTON_IMAGE_SMALL);

        /// <summary>
        /// Holds the word redaction button image.
        /// </summary>
        private static Image _wordRedactionButtonImage = (Image)new Bitmap(
            typeof(RedactionToolStripMenuItem),
            ToolStripButtonConstants._WORD_REDACTION_BUTTON_IMAGE);

        /// <summary>
        /// Image viewer with which this menu item connects.
        /// </summary>
        private IDocumentViewer _imageViewer;

        /// <summary>
        /// Stores the currently selected redaction tool for this button so that the redaction tool
        /// can be changed in <see cref="Control.OnClick"/> event.
        /// </summary>
        private CursorTool _redactionTool;

        #endregion

        #region RedactionToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="RedactionToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public RedactionToolStripMenuItem()
        {
            InitializeComponent();

            // Set the tooltip text
            base.ToolTipText = ToolStripButtonConstants._REDACTION_SPLIT_BUTTON_TOOL_TIP;

            // Set the menu item text
            base.Text = ToolStripButtonConstants._REDACTION_SPLIT_BUTTON_TEXT;

            // Set the display style to both image and text
            base.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;

            // Get the last used redaction tool
            _redactionTool = RegistryManager.GetLastUsedRedactionTool();

            // Set the image based on the last used redaction tool
            switch (_redactionTool)
            {
                case CursorTool.AngularRedaction:
                    base.Image = _angularRedactionButtonImage;
                    break;

                case CursorTool.RectangularRedaction:
                    base.Image = _rectangularRedactionButtonImage;
                    break;

                case CursorTool.WordRedaction:
                    base.Image = _wordRedactionButtonImage;
                    break;

                default:
                    throw new ExtractException("ELI31307", "Unexpected cursor tool");
            }

            // Disable menu item by default
            base.Enabled = false;
        }

        #endregion

        #region RedactionToolStripMenuItem Methods

        /// <summary>
        /// Sets the enabled and checked state of the <see cref="RedactionToolStripMenuItem"/>
        /// </summary>
        private void SetMenuItemState()
        {
            base.Enabled = _imageViewer != null && _imageViewer.IsImageAvailable
                && _imageViewer.AllowHighlight;
            base.Checked = _imageViewer != null &&
                (_imageViewer.CursorTool == CursorTool.AngularRedaction ||
                _imageViewer.CursorTool == CursorTool.RectangularRedaction);
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
                    _imageViewer.Shortcuts.GetKeys(_imageViewer.ToggleRedactionTool);

                // Display the shortcut key text
                base.ShortcutKeyDisplayString = ShortcutsManager.GetDisplayString(shortcutKeys);
            }
        }

        #endregion

        #region RedactionToolStripMenuItem OnEvents

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event. 
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        /// <seealso cref="Control.OnClick"/>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                base.OnClick(e);

                if (_imageViewer != null && _imageViewer.AllowHighlight)
                {
                    // If the current cursor tool is one of the redaction tools, set
                    // the _cursorTool value to the opposite tool
                    if (_imageViewer.CursorTool == CursorTool.AngularRedaction)
                    {
                        _redactionTool = CursorTool.RectangularRedaction;
                    }
                    else if (_imageViewer.CursorTool == CursorTool.RectangularRedaction)
                    {
                        _redactionTool = CursorTool.AngularRedaction;
                    }

                    // Set the cursor tool to the appropriate redaction tool
                    _imageViewer.CursorTool = _redactionTool;

                    // Update the menu item state
                    SetMenuItemState();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22268", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        #endregion

        #region RedactionToolStripMenuItem Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        private void HandleImageChanged(object sender, ImageFileChangedEventArgs e)
        {
            // Set the menu item state
            SetMenuItemState();
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.CursorToolChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.CursorToolChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.CursorToolChanged"/> event.</param>
        private void HandleCursorToolChanged(object sender, CursorToolChangedEventArgs e)
        {
            switch (e.CursorTool)
            {
                case CursorTool.AngularRedaction:
                    base.Image = _angularRedactionButtonImage;
                    break;

                case CursorTool.RectangularRedaction:
                    base.Image = _rectangularRedactionButtonImage;
                    break;

                case CursorTool.WordRedaction:
                    base.Image = _wordRedactionButtonImage;
                    break;

                default:
                    // Do nothing, only concerned with the redaction cursor tools
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
                ExtractException ee = ExtractException.AsExtractException("ELI22269", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.AllowHighlightStatusChanged"/> event.
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
                throw ExtractException.AsExtractException("ELI30238", ex);
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
                    ExtractException ee = new ExtractException("ELI22270",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}

