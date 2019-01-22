using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripStatusLabel"/> that displays a description of how to use 
    /// the currently activated cursor tool.
    /// </summary>
    public partial class UserActionToolStripStatusLabel : ToolStripStatusLabel, IImageViewerControl
    {
        #region UserActionToolStripStatusLabel Constants

        /// <summary>
        /// The text that displays on the label in the Visual Studio designer.
        /// </summary>
        private static readonly string _DEFAULT_DESIGNTIME_TEXT = "User action label";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(UserActionToolStripStatusLabel).ToString();

        #endregion UserActionToolStripStatusLabel Constants

        #region UserActionToolStripStatusLabel Fields

        /// <summary>
        /// The image viewer with which the <see cref="UserActionToolStripStatusLabel"/> is 
        /// associated.
        /// </summary>
        IDocumentViewer _imageViewer;

        #endregion UserActionToolStripStatusLabel Fields

        #region UserActionToolStripStatusLabel Constructors

        /// <summary>
        /// Initializes a new <see cref="UserActionToolStripStatusLabel"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public UserActionToolStripStatusLabel() 
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23130",
					_OBJECT_NAME);

                InitializeComponent();

                // Left justify the text
                base.TextAlign = ContentAlignment.MiddleLeft;

                // If this is design-time, set the text of this label so it is visible.
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    base.Text = _DEFAULT_DESIGNTIME_TEXT;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23131", ex);
            }
        }

        #endregion UserActionToolStripStatusLabel Constructors

        #region UserActionToolStripStatusLabel Methods

        /// <summary>
        /// Sets the text on the <see cref="UserActionToolStripStatusLabel"/> based on the 
        /// currently selected cursor tool.
        /// </summary>
        private void SetState()
        {
            // Ensure an image viewer is open and an image is available before displaying a status.
            if (_imageViewer == null)
            {
                base.Text = "";
            }
            else if (!_imageViewer.IsImageAvailable)
            {
                // If the image viewer exists, but an image is not currently loaded, display the image
                // viewer's default status message.
                base.Text = _imageViewer.DefaultStatusMessage;
            }
            else
            {
                // Set the text based on the active cursor tool
                switch (_imageViewer.CursorTool)
                {
                    case CursorTool.AngularHighlight:
                        base.Text = "Click and drag to draw an angular highlight.";
                        break;

                    case CursorTool.AngularRedaction:
                        base.Text = "Click and drag to draw an angular redaction.";
                        break;

                    case CursorTool.DeleteLayerObjects:
                        base.Text = "Click and drag to delete multiple objects.";
                        break;

                    case CursorTool.EditHighlightText:
                        base.Text = "Click on a highlight to modify its text.";
                        break;

                    case CursorTool.Pan:
                        base.Text = "Click and drag to pan the image.";
                        break;

                    case CursorTool.RectangularHighlight:
                        base.Text = "Click and drag to draw a rectangular highlight.";
                        break;

                    case CursorTool.RectangularRedaction:
                        base.Text = "Click and drag to draw a rectangular redaction.";
                        break;

                    case CursorTool.WordHighlight:
                        base.Text = "Click on an outlined word or drag across one to draw a highlight for it.";
                        break;

                    case CursorTool.WordRedaction:
                        base.Text = "Click on an outlined word or drag across one to draw a redaction for it";
                        break;

                    case CursorTool.SelectLayerObject:
                        base.Text = "Click on an object to select it.";
                        break;

                    case CursorTool.SetHighlightHeight:
                        base.Text = "Click and drag to set the height of angular highlights.";
                        break;

                    case CursorTool.ZoomWindow:
                        base.Text = "Click and drag to zoom in on a region.";
                        break;

                    case CursorTool.ExtractImage:
                        base.Text = "Click and drag to extract a subimage.";
                        break;

                    case CursorTool.None:
                        base.Text = "";
                        break;

                    default:

                        throw new ExtractException("ELI21516", "Unexpected cursor tool type.");
                }
            }
        }

        #endregion UserActionToolStripStatusLabel Methods

        #region UserActionToolStripStatusLabel Overrides

        /// <summary>
        /// Gets the text that is displayed on the 
        /// <see cref="UserActionToolStripStatusLabel"/>.
        /// </summary>
        /// <value>The set property has been obsoleted and disabled.</value>
        /// <return>The text displayed on the <see cref="UserActionToolStripStatusLabel"/>.
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

        #endregion UserActionToolStripStatusLabel Overrides

        #region UserActionToolStripStatusLabel Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.CursorToolChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.CursorToolChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.CursorToolChanged"/> event.</param>
        private void HandleCursorToolChanged(object sender, CursorToolChangedEventArgs e)
        {
            try
            {
                SetState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21515", ex);
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
                ExtractException ee = ExtractException.AsExtractException("ELI23170", ex);
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
                base.Text = "Loading document. Please wait...";
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23253", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion UserActionToolStripStatusLabel Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="UserActionToolStripStatusLabel"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="UserActionToolStripStatusLabel"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="UserActionToolStripStatusLabel"/> is 
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
                        _imageViewer.CursorToolChanged -= HandleCursorToolChanged;
                        _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                        _imageViewer.LoadingNewImage -= HandleLoadingNewImage;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.CursorToolChanged += HandleCursorToolChanged;
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                        _imageViewer.LoadingNewImage += HandleLoadingNewImage;
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
                    ExtractException ee = new ExtractException("ELI21514",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
