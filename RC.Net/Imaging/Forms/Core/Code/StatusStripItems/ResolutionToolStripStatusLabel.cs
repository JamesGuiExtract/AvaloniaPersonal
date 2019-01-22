using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripStatusLabel"/> that displays the vertical and horizontal 
    /// dots per inch (dpi) of the currently open image.
    /// </summary>
    public partial class ResolutionToolStripStatusLabel : ToolStripStatusLabel, IImageViewerControl
    {
        #region ResolutionToolStripStatusLabel Constants

        /// <summary>
        /// The text that displays on the label in the Visual Studio designer.
        /// </summary>
        private static readonly string _DEFAULT_DESIGNTIME_TEXT = "(Resolution)";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(ResolutionToolStripStatusLabel).ToString();

        #endregion ResolutionToolStripStatusLabel Constants

        #region ResolutionToolStripStatusLabel Fields

        /// <summary>
        /// The image viewer with which the <see cref="ResolutionToolStripStatusLabel"/> is 
        /// associated.
        /// </summary>
        IDocumentViewer _imageViewer;

        #endregion ResolutionToolStripStatusLabel Fields

        #region ResolutionToolStripStatusLabel Constructors

        /// <summary>
        /// Initializes a new <see cref="ResolutionToolStripStatusLabel"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public ResolutionToolStripStatusLabel() 
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23128",
					_OBJECT_NAME);

                InitializeComponent();

                // If this is design-time, set the text of this label so it is visible.
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    base.Text = _DEFAULT_DESIGNTIME_TEXT;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23129", ex);
            }
        }

        #endregion ResolutionToolStripStatusLabel Constructors

        #region ResolutionToolStripStatusLabel Methods

        /// <summary>
        /// Sets the text on the <see cref="ResolutionToolStripStatusLabel"/> based on the 
        /// currently open image.
        /// </summary>
        private void SetState()
        {
            // Ensure an image is open
            if (_imageViewer == null || !_imageViewer.IsImageAvailable)
            {
                base.Text = "";
            }
            else
            {
                // Set the text based on the resolution of the open image
                base.Text =  _imageViewer.ImageDpiX.ToString(CultureInfo.CurrentCulture)
                    + " x " + _imageViewer.ImageDpiY.ToString(CultureInfo.CurrentCulture);
            }
        }

        #endregion ResolutionToolStripStatusLabel Methods

        #region ResolutionToolStripStatusLabel Overrides

        /// <summary>
        /// Gets the text that is displayed on the 
        /// <see cref="ResolutionToolStripStatusLabel"/>.
        /// </summary>
        /// <value>The set property has been obsoleted and disabled.</value>
        /// <return>The text displayed on the <see cref="ResolutionToolStripStatusLabel"/>.
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

        #endregion ResolutionToolStripStatusLabel Overrides

        #region ResolutionToolStripStatusLabel Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.PageChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.PageChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.PageChanged"/> event.</param>
        private void HandlePageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                SetState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21518", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.LoadingNewImage"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.LoadingNewImage"/> event.</param>
        /// <param name="e">The event data associated with the
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.LoadingNewImage"/> event.</param>
        private void HandleLoadingNewImage(object sender, LoadingNewImageEventArgs e)
        {
            try
            {
                SetState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23252", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        private void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                SetState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23375", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion ResolutionToolStripStatusLabel Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="ResolutionToolStripStatusLabel"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="ResolutionToolStripStatusLabel"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="ResolutionToolStripStatusLabel"/> is 
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
                        _imageViewer.PageChanged -= HandlePageChanged;
                        _imageViewer.LoadingNewImage -= HandleLoadingNewImage;
                        _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.PageChanged += HandlePageChanged;
                        _imageViewer.LoadingNewImage += HandleLoadingNewImage;
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                    }

                    // Set the text based on the state of the image viewer
                    // if and only if this is not design time
                    if (!base.DesignMode)
                    {
                        SetState();
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI21519",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
