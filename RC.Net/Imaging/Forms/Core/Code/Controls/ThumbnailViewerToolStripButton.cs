using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Extract.Utilities.Forms;
using TD.SandDock;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// A derived <see cref="ToolStripButton"/> that contains the image for the thumbnail viewer.
    /// </summary>
    [ToolboxBitmap(typeof(ThumbnailViewerToolStripButton),
        ToolStripButtonConstants._THUMBNAIL_VIEWER_BUTTON_IMAGE)]
    public partial class ThumbnailViewerToolStripButton : ToolStripButtonBase
    {
        #region Fields

        /// <summary>
        /// The <see cref="DockableWindow"/> associated with this
        /// thumbnail viewer.
        /// </summary>
        DockableWindow _dockableWindow;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailViewerToolStripButton"/>
        /// class.
        /// </summary>
        public ThumbnailViewerToolStripButton()
            : base(typeof(ThumbnailViewerToolStripButton),
            ToolStripButtonConstants._THUMBNAIL_VIEWER_BUTTON_IMAGE)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the <see cref="DockableWindow"/> that this control is
        /// associated with.
        /// <para><b>Note:</b></para>
        /// This should not be set until the <see cref="Form"/> containing
        /// the dockable window has been displayed and its state has been
        /// restored (if the state has been saved). If this property is
        /// set earlier then the toggled state of the button may appear wrong.
        /// </summary>
        public DockableWindow DockableWindow
        {
            get
            {
                return _dockableWindow;
            }
            set
            {
                try
                {
                    _dockableWindow = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30176", ex);
                }
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event. If
        /// <see cref="ThumbnailViewerToolStripButton.DockableWindow"/> is not
        /// <see langword="null"/> then will toggle the display of the window.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                // Check if a dockable window has been assigned
                if (_dockableWindow != null)
                {
                    // If the window is opened or collapsed, close it
                    if (_dockableWindow.IsOpen || _dockableWindow.Collapsed)
                    {
                        _dockableWindow.Close();
                    }
                    else
                    {
                        // Window is not open, open it
                        _dockableWindow.Open();
                    }
                }

                base.OnClick(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30155", ex);
            }
        }

        #endregion Overrides
    }
}
