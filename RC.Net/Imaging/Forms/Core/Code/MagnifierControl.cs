using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Extract.Licensing;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a control that displays a magnified view of the region around the
    /// <see cref="ImageViewer"/> cursor.
    /// </summary>
    public partial class MagnifierControl : UserControl, IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(MagnifierControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The image viewer associated with the <see cref="MagnifierControl"/>.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// Indicates whether the <see cref="MagnifierControl"/> is currently being painted.
        /// </summary>
        bool _painting;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="MagnifierControl"/> class.
        /// </summary>
        public MagnifierControl()
        {
            try
            {
                InitializeComponent();

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI31380",
                    _OBJECT_NAME);

                // Double-buffer to prevent flickering
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.DoubleBuffer, true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31381", ex);
            }
        }

        #endregion Constructors

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="MagnifierControl"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="MagnifierControl"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="MagnifierControl"/> is 
        /// connected. <see langword="null"/> if no connections are established.</returns>
        [Browsable(false)]
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
                    if (value != _imageViewer)
                    {
                        if (_imageViewer != null)
                        {
                            _imageViewer.PostImagePaint -= HandleImageViewerPostImagePaint;
                            _imageViewer.MouseMove -= HandleImageViewerMouseMove;
                        }

                        _imageViewer = value;

                        _imageViewer.PostImagePaint += HandleImageViewerPostImagePaint;
                        _imageViewer.MouseMove += HandleImageViewerMouseMove;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31454", ex);
                }
            }
        }

        #endregion IImageViewerControl Members

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/>
        /// that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                _painting = true;

                base.OnPaint(e);

                if (_imageViewer != null)
                {
                    // Have the image viewer draw the area centered around the mouse zoomed in such
                    // that 1 image pixel = 1 screen pixel.
                    Point imageCenterPoint = _imageViewer.PointToClient(Control.MousePosition);
                    _imageViewer.PaintToGraphics(e.Graphics, e.ClipRectangle, imageCenterPoint, 1F);

                    // Draw the imageviewer cursor with the hotspot in the center (which corresponds
                    // to the point the cursor is in the imageviewer).
                    int cursorLeft = (ClientRectangle.Width / 2) - _imageViewer.Cursor.HotSpot.X;
                    int cursorTop = (ClientRectangle.Height / 2) - _imageViewer.Cursor.HotSpot.Y;
                    Rectangle cursorRect = new Rectangle(new Point(cursorLeft, cursorTop), Cursor.Size);
                    _imageViewer.Cursor.Draw(e.Graphics, cursorRect);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31449", ex);
            }
            finally
            {
                _painting = false;
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ImageViewer"/> PostImagePaint event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/>
        /// instance containing the event data.</param>
        void HandleImageViewerPostImagePaint(object sender, PaintEventArgs e)
        {
            try
            {
                if (!_painting)
                {
                    // As long as the post paint event wasn't a result of this control's paint,
                    // force a refresh via the message queue; Invalidate often won't trigger a
                    // paint for quite some time during an image viewer tracking event, but forcing
                    // an immediate refresh while the ImageViewer is still painting will prevent the
                    // ImageViewer from being drawn correctly.
                    BeginInvoke((MethodInvoker)(() => Refresh()));
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31451", ex);
            }
        }

        /// <summary>
        /// Handles the image viewer <see cref="Control.MouseMove"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/>
        /// instance containing the event data.</param>
        void HandleImageViewerMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Whenever the image viewer is invalidated, invalidate this control to update the
                // area of the image that is zoomed in.
                Invalidate();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31450", ex);
            }
        }

        #endregion Event Handlers
    }
}
