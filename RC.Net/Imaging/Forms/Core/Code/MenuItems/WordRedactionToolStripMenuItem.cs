using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the word
    /// redaction <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(WordRedactionToolStripMenuItem),
        ToolStripButtonConstants._WORD_REDACTION_BUTTON_IMAGE)]
    public partial class WordRedactionToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region WordRedactionToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="WordRedactionToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public WordRedactionToolStripMenuItem()
            : base(CursorTool.WordRedaction,
            ToolStripButtonConstants._WORD_REDACTION_MENU_ITEM_TEXT,
            ToolStripButtonConstants._WORD_REDACTION_BUTTON_IMAGE,
            typeof(WordRedactionToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region WordRedactionToolStripMenuItem Events

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
                    base.ImageViewer.CursorTool = CursorTool.WordRedaction;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI31311", ex);
                ee.AddDebugData("Event arguments", e, false);
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
                ExtractException.Display("ELI31312", ex);
            }
        }

        #endregion

        #region WordRedactionToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return ImageViewer == null ? null :
                ImageViewer.Shortcuts.GetKeys(base.ImageViewer.ToggleRedactionTool);
        }

        /// <summary>
        /// Sets the <see cref="ToolStripItem.Enabled"/> and <see cref="ToolStripMenuItem.Checked"/> 
        /// properties depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetMenuItemState()
        {
            base.SetMenuItemState();
            Enabled = Enabled && ImageViewer.AllowHighlight;
        }

        /// <summary>
        /// Gets or sets the image viewer with which to establish a connection.
        /// </summary>
        /// <value>The image viewer with which to establish a connection. <see langword="null"/> 
        /// indicates the connection should be disconnected from the current image viewer.</value>
        /// <returns>The image viewer with which a connection is established. 
        /// <see langword="null"/> if no image viewer is connected.</returns>
        /// <seealso cref="IImageViewerControl"/>
        public override ImageViewer ImageViewer
        {
            get
            {
                return base.ImageViewer;
            }
            set
            {
                try
                {
                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.AllowHighlightStatusChanged -=
                            HandleAllowHighlightStatusChanged;
                    }

                    base.ImageViewer = value;

                    if (base.ImageViewer != null)
                    {
                        base.ImageViewer.AllowHighlightStatusChanged +=
                            HandleAllowHighlightStatusChanged;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31313", ex);
                }
            }
        }

        #endregion WordRedactionToolStripMenuItem Methods
    }
}

