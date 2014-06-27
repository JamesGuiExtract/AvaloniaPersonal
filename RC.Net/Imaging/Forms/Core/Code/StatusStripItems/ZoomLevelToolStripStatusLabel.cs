using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripStatusLabel"/> that displays current zoom level of the
    /// displayed image as a percentage of the number of physical image pixels diplayed to the
    /// number of pixels in the displayed portion of the image.
    /// </summary>
    public partial class ZoomLevelToolStripStatusLabel : ToolStripStatusLabel, IImageViewerControl
    {
        #region ZoomLevelToolStripStatusLabel Constants

        /// <summary>
        /// The text that displays on the label in the Visual Studio designer.
        /// </summary>
        private static readonly string _DEFAULT_DESIGNTIME_TEXT = "(Zoom)";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(ZoomLevelToolStripStatusLabel).ToString();

        #endregion ZoomLevelToolStripStatusLabel Constants

        #region ZoomLevelToolStripStatusLabel Fields

        /// <summary>
        /// The image viewer with which the <see cref="ZoomLevelToolStripStatusLabel"/> is 
        /// associated.
        /// </summary>
        ImageViewer _imageViewer;

        #endregion ZoomLevelToolStripStatusLabel Fields

        #region ZoomLevelToolStripStatusLabel Constructors

        /// <summary>
        /// Initializes a new <see cref="ZoomLevelToolStripStatusLabel"/> class.
        /// </summary>
        public ZoomLevelToolStripStatusLabel() 
            : base("")
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI36767",
					_OBJECT_NAME);

                InitializeComponent();

                // If this is design-time, set the text of this label so it is visible.
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    base.Text = _DEFAULT_DESIGNTIME_TEXT;
                }

                // Set the label to fixed width and the text alignment to right
                base.AutoSize = false;
                base.Width = 80;
                base.TextAlign = ContentAlignment.MiddleLeft;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI36768", ex);
            }
        }

        #endregion ZoomLevelToolStripStatusLabel Constructors

        #region ZoomLevelToolStripStatusLabel Methods

        /// <summary>
        /// Sets the zoom level status label depending on the current state of the image viewer.
        /// </summary>
        void SetLabelText()
        {
            // Ensure an image is open
            if (_imageViewer == null || !_imageViewer.IsImageAvailable)
            {
                base.Text = "";
            }
            else
            {
                NumberFormatInfo format =
                    new CultureInfo(CultureInfo.InvariantCulture.LCID).NumberFormat;
                format.PercentGroupSeparator = "";

                // Set the text based on the current zoom level of the open image
                base.Text = string.Format(format, "Zoom: {0:P0}", _imageViewer.ScaleFactor);
            }
        }

        #endregion ZoomLevelToolStripStatusLabel Methods

        #region ZoomLevelToolStripStatusLabel Overrides

        /// <summary>
        /// Gets the text that is displayed on the 
        /// <see cref="ZoomLevelToolStripStatusLabel"/>.
        /// </summary>
        /// <value>The set property has been obsoleted and disabled.</value>
        /// <return>The text displayed on the <see cref="ZoomLevelToolStripStatusLabel"/>.
        /// </return>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                // Do nothing
            }
        }

        #endregion ZoomLevelToolStripStatusLabel Overrides

        #region ZoomLevelToolStripStatusLabel Event Handlers

        /// <summary>
        /// Handles the <see cref="T:ImageViewer.ImageFileChanged"/> event of the
        /// <see cref="_imageViewer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.ImageFileChangedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                SetLabelText();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37101");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ImageViewer.PageChanged"/> event of the
        /// <see cref="_imageViewer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.PageChangedEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                SetLabelText();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36766");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ImageViewer.ZoomChanged"/> event of the
        /// <see cref="_imageViewer"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.ZoomChangedEventArgs"/> instance
        /// containing the event data.</param>
        void HandleZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            try
            {
                SetLabelText();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36765");
            }
        }

        #endregion ZoomLevelToolStripStatusLabel Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="ZoomLevelToolStripStatusLabel"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="ZoomLevelToolStripStatusLabel"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="ZoomLevelToolStripStatusLabel"/> is 
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
                    // Unregister from previously subscribed-to events
                    if (_imageViewer != null)
                    {
                        _imageViewer.PageChanged -= HandlePageChanged;
                        _imageViewer.ZoomChanged -= HandleZoomChanged;
                        _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.PageChanged += HandlePageChanged;
                        _imageViewer.ZoomChanged += HandleZoomChanged;
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                    }

                    // Set the text based on the state of the image viewer
                    // if and only if this is not design time
                    if (!base.DesignMode)
                    {
                        SetLabelText();
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI36796",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
