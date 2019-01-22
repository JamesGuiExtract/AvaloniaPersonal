using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Specifies how the current state of background opeations such as OCR and word highlight
    /// loading.
    /// </summary>
    public partial class BackgroundProcessStatusLabel : ToolStripStatusLabel, 
        IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(BackgroundProcessStatusLabel).ToString();

        /// <summary>
        /// The text that displays on the label in the Visual Studio designer.
        /// </summary>
        static readonly string _DEFAULT_DESIGNTIME_TEXT = "(Background status)";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Image viewer with which this status label connects.
        /// </summary>
        IDocumentViewer _imageViewer;

        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundProcessStatusLabel"/> class.
        /// </summary>
        public BackgroundProcessStatusLabel()
            : base(" ")
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32617",
					_OBJECT_NAME);

                InitializeComponent();

                // If this is design-time, set the text of this label so it is visible.
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    base.Text = _DEFAULT_DESIGNTIME_TEXT;
                }

                base.AutoSize = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI32618", ex);
            }
        }

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="BackgroundProcessStatusLabel"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="BackgroundProcessStatusLabel"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="BackgroundProcessStatusLabel"/> is 
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
                        _imageViewer.BackgroundProcessStatusUpdate -= HandleBackgroundProcessStatusUpdate;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.BackgroundProcessStatusUpdate += HandleBackgroundProcessStatusUpdate;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI32621",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members

        #region Event Handlers

        /// <summary>
        /// Handles the case that a background process status update has occured.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The
        /// <see cref="Extract.Imaging.Forms.BackgroundProcessStatusUpdateEventArgs"/> instance
        /// containing the event data.</param>
        void HandleBackgroundProcessStatusUpdate(object sender,
            BackgroundProcessStatusUpdateEventArgs e)
        {
            try
            {
                Text = e.Status;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32620");
            }
        }

        #endregion Event Handlers
    }
}
