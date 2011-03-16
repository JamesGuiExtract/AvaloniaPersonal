using Extract.Licensing;
using Leadtools.WinForms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Specifies how the mouse position should be displayed.
    /// </summary>
    public enum MousePositionDisplayOption
    {
        /// <summary>
        /// Reads the display options from the registry.
        /// </summary>
        Registry,

        /// <summary>
        /// Displays only the x and y image coordinates.
        /// </summary>
        PositionOnly,

        /// <summary>
        /// Displays the image coordinates and percentages.
        /// </summary>
        PositionAndPercentage
    }

    /// <summary>
    /// Represents a <see cref="ToolStripStatusLabel"/> that displays the current mouse position 
    /// on an <see cref="ImageViewer"/> control in image coordinates.
    /// </summary>
    public partial class MousePositionToolStripStatusLabel : ToolStripStatusLabel, 
        IImageViewerControl
    {
        #region MousePositionToolStripStatusLabel Constants

        /// <summary>
        /// The text that displays on the label in the Visual Studio designer.
        /// </summary>
        private static readonly string _DEFAULT_DESIGNTIME_TEXT = "X: Y: ";

        /// <summary>
        /// Format string to display position-only mouse coordinates.
        /// </summary>
        private static readonly string _POSITION_FORMAT = "X: {0:0}, Y: {1:0}";

        /// <summary>
        /// Format string to display position and percentage coordinates.
        /// </summary>
        private static readonly string _PERCENTAGE_FORMAT =
            "X: {0:0} ({2:0%}), Y: {1:0} ({3:0%})";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(MousePositionToolStripStatusLabel).ToString();

        #endregion

        #region MousePositionToolStripStatusLabel Fields

        /// <summary>
        /// Image viewer with which this status label connects.
        /// </summary>
        private ImageViewer _imageViewer;

        /// <summary>
        /// How the mouse position should be displayed.
        /// </summary>
        private MousePositionDisplayOption _displayOption;

        /// <summary>
        /// Whether percentages should be displayed.
        /// </summary>
        private bool _displayPercentages;

        #endregion

        #region MousePositionToolStripStatusLabel Constructors

        /// <summary>
        /// Initializes a new <see cref="MousePositionToolStripStatusLabel"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public MousePositionToolStripStatusLabel() 
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23126",
					_OBJECT_NAME);

                InitializeComponent();

                // If this is design-time, set the text of this label so it is visible.
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    base.Text = _DEFAULT_DESIGNTIME_TEXT;
                }

                // Set whether to display percentages based on the registry setting.
                _displayPercentages = ShouldDisplayPercentages();

                // Set the label to fixed width and the text alignment to right
                base.AutoSize = false;
                base.Width = _displayPercentages ? 175 : 100;
                base.TextAlign = ContentAlignment.MiddleLeft;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23127", ex);
            }
        }

        #endregion

        #region MousePositionToolStripStatusLabel Properties

        /// <summary>
        /// Gets or sets the option for how the mouse position should be displayed.
        /// </summary>
        /// <value>The option for how the mouse position should be displayed.</value>
        /// <returns>The option for how the mouse position should be displayed.</returns>
        /// <exception cref="ExtractException"><paramref name="value"/> is not valid.</exception>
        /// <remarks>The x-coordinate percentage is the coordinate's distance from the left of the
        /// image relative to the total width of the image. The y-coordinate percentage is the 
        /// coordinate's distance from the top of the image relative to the total height of the 
        /// image.</remarks>
        public MousePositionDisplayOption DisplayOption
        {
            get
            {
                return _displayOption;
            }
            set
            {
                // Ensure value is a valid MousePositionDisplayOption.
	            ExtractException.Assert("ELI21521", "Unexpected display option.", 
                    Enum.IsDefined(typeof(MousePositionDisplayOption), value));

                _displayOption = value;

                _displayPercentages = ShouldDisplayPercentages();
            }
        }

        #endregion MousePositionToolStripStatusLabel Properties

        #region MousePositionToolStripStatusLabel Methods

        /// <summary>
        /// Whether the current mouse position should also display percentages.
        /// </summary>
        /// <returns><see langword="true"/> if percentages should be displayed; 
        /// <see langword="false"/> if percentages should not be displayed.</returns>
        private bool ShouldDisplayPercentages()
        {
            switch (_displayOption)
            {
                case MousePositionDisplayOption.Registry:

                    // Check the registry
                    return RegistryManager.DisplayPercentages;

                case MousePositionDisplayOption.PositionOnly:
                    
                    // Don't display percentages
                    return false;

                case MousePositionDisplayOption.PositionAndPercentage:

                    // Display percentages
                    return true;

                default:

                    ExtractException ee = new ExtractException("ELI21523", 
                        "Unexpected display option.");
                    ee.AddDebugData("Display option", _displayOption, false);
                    throw ee;
            }
        }

        #endregion MousePositionToolStripStatusLabel Methods

        #region MousePositionToolStripStatusLabel Overrides

        /// <summary>
        /// Gets the text that is displayed on the 
        /// <see cref="MousePositionToolStripStatusLabel"/>.
        /// </summary>
        /// <value>The set property has been obsoleted and disabled.</value>
        /// <return>The text displayed on the <see cref="MousePositionToolStripStatusLabel"/>.
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

        #endregion

        #region MousePositionToolStripStatusLabel Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.MouseMove"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.MouseMove"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.MouseMove"/> event.</param>
        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Check if the mouse cursor is inside the image area of the image viewer control
                if (_imageViewer != null && _imageViewer.IsImageAvailable &&
                    _imageViewer.PhysicalViewRectangle.Contains(e.X, e.Y))
                {
                    // Calculate the current cursor position in logical (image) coordinates
                    Point[] cursorPosition = new Point[] { new Point(e.X, e.Y) };
                    using (Matrix matrix = _imageViewer.Transform.Clone())
                    {
                        matrix.Invert();

                        matrix.TransformPoints(cursorPosition);
                    }

                    // Display the cursor position using the option specified
                    if (_displayPercentages)
                    {
                        // Get the x and y percentages to nearest integer
                        double xPercentage =
                            ((double)cursorPosition[0].X / (double)_imageViewer.ImageWidth);
                        double yPercentage =
                            ((double)cursorPosition[0].Y / (double)_imageViewer.ImageHeight);

                        // Display the cursor position
                        base.Text = String.Format(CultureInfo.CurrentCulture, _PERCENTAGE_FORMAT,
                            cursorPosition[0].X, cursorPosition[0].Y, xPercentage, yPercentage);
                    }
                    else
                    {
                        // Display the cursor position without percentages
                        base.Text = String.Format(CultureInfo.CurrentCulture, _POSITION_FORMAT,
                            cursorPosition[0].X, cursorPosition[0].Y);
                    }
                }
                else
                {
                    // If there is no available image or the mouse is not over the image itself,
                    // don't display any mouse position status text.
                    base.Text = null;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21389", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        private void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                // Clear the status text until the mouse is moved over an image that may
                // (or may not) have been loaded.
                base.Text = null;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23174", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.MouseLeave"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Control.MouseLeave"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="Control.MouseLeave"/> 
        /// event.</param>
        private void HandleMouseLeave(object sender, EventArgs e)
        {
            try
            {
                // Clear the status text when the mouse leaves in image viewer
                base.Text = null;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23215", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }
        
        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.LoadingNewImage"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.LoadingNewImage"/> event.</param>
        /// <param name="e">The event data associated with the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.LoadingNewImage"/> event.</param>
        private void HandleLoadingNewImage(object sender, LoadingNewImageEventArgs e)
        {
            try
            {
                base.Text = null;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23251", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the 
        /// <see cref="MousePositionToolStripStatusLabel"/> is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="MousePositionToolStripStatusLabel"/> 
        /// is connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="MousePositionToolStripStatusLabel"/> 
        /// is connected. <see langword="null"/> if no connections are established.</returns>
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
                        _imageViewer.MouseMove -= HandleMouseMove;
                        _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                        _imageViewer.MouseLeave -= HandleMouseLeave;
                        _imageViewer.LoadingNewImage -= HandleLoadingNewImage;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.MouseMove += HandleMouseMove;
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                        _imageViewer.MouseLeave += HandleMouseLeave;
                        _imageViewer.LoadingNewImage += HandleLoadingNewImage;
                    }
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21388",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
