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
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the angular
    /// highlight <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(AngularHighlightToolStripMenuItem),
       ToolStripButtonConstants._ANGULAR_HIGHLIGHT_BUTTON_IMAGE_SMALL)]
    public partial class AngularHighlightToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region AngularHighlightToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="AngularHighlightToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public AngularHighlightToolStripMenuItem()
            : base(CursorTool.AngularHighlight,
            ToolStripButtonConstants._ANGULAR_HIGHLIGHT_MENU_ITEM_TEXT,
            ToolStripButtonConstants._ANGULAR_HIGHLIGHT_BUTTON_IMAGE_SMALL,
            typeof(AngularHighlightToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region AngularHighlightToolStripMenuItem Events

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
                if (base.ImageViewer != null && base.ImageViewer.IsImageAvailable
                    && base.ImageViewer.AllowHighlight)
                {
                    base.ImageViewer.CursorTool = CursorTool.AngularHighlight;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21431", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
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
                ExtractException.Display("ELI30230", ex);
            }
        }

        #endregion

        #region AngularHighlightToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectAngularHighlightTool);
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
                    throw ExtractException.AsExtractException("ELI30229", ex);
                }
            }
        }

        #endregion AngularHighlightToolStripMenuItem Methods
    }
}
