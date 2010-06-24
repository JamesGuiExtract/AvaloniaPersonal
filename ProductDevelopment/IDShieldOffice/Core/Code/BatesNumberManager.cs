using Extract;
using Extract.Drawing;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace IDShieldOffice
{
    /// <summary>
    /// Represents a manager for Bates numbering of documents.
    /// </summary>
    internal class BatesNumberManager : IUserConfigurableComponent, IDisposable
    {
        #region BatesNumberManager Constants

        /// <summary>
        /// The warning message that appears when attempting to apply Bates numbers off the 
        /// visible page.
        /// </summary>
        static readonly string _OFF_PAGE_WARNING_MESSAGE =
            "At least one Bates number will not fully appear on the page."
            + Environment.NewLine + Environment.NewLine
            + "Do you wish to apply Bates numbers anyway?";

        /// <summary>
        /// The title of the warning message that appears when attempting to apply Bates numbers 
        /// off the visible page.
        /// </summary>
        static readonly string _OFF_PAGE_WARNING_TITLE =
            "Apply Bates numbers off page?";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(BatesNumberManager).ToString();

        /// <summary>
        /// The tag that will be added to the <see cref="TextLayerObject"/> created for
        /// each bates number that is applied.
        /// </summary>
        internal static readonly string _BATES_NUMBER_TAG = "Bates";

        #endregion BatesNumberManager Constants

        #region BatesNumberManager Fields

        /// <summary>
        /// Whether a Bates number is required before a document can be saved.
        /// </summary>
        bool _requireBates = RegistryManager._REQUIRE_BATES_DEFAULT;

        /// <summary>
        /// The format settings for Bates numbers.
        /// </summary>
        BatesNumberFormat _format = new BatesNumberFormat();

        /// <summary>
        /// The property page associated with the <see cref="BatesNumberManager"/>.
        /// </summary>
        BatesNumberManagerPropertyPage _propertyPage;

        /// <summary>
        /// The font to use to display a Bates number.
        /// </summary>
        Font _font = new Font(RegistryManager._BATES_FONT_FAMILY_DEFAULT, 
            RegistryManager._BATES_FONT_SIZE_DEFAULT, RegistryManager._BATES_FONT_STYLE_DEFAULT);

        /// <summary>
        /// The number of horizontal inches from the side of the page where the Bates number is 
        /// placed.
        /// </summary>
        float _horizontalInches = RegistryManager._BATES_HORIZONTAL_INCHES_DEFAULT;

        /// <summary>
        /// The number of vertical inches from the side of the page where the Bates number is 
        /// placed.
        /// </summary>
        float _verticalInches = RegistryManager._BATES_VERTICAL_INCHES_DEFAULT;

        /// <summary>
        /// The alignment of the anchor point for the page.
        /// </summary>
        AnchorAlignment _pageAnchorAlignment = RegistryManager._BATES_PAGE_ANCHOR_ALIGNMENT_DEFAULT;

        /// <summary>
        /// The alignment of the anchor point for the Bates number.
        /// </summary>
        AnchorAlignment _anchorAlignment = RegistryManager._BATES_ANCHOR_ALIGNMENT_DEFAULT;

        /// <summary>
        /// The image viewer on which the Bates numbers will be applied.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// The bates number generator to use.
        /// </summary>
        BatesNumberGenerator _generator;

        #endregion BatesNumberManager Fields

        #region BatesNumberManager Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BatesNumberManager"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which Bates number will be applied.
        /// </param>
        public BatesNumberManager(ImageViewer imageViewer)
        {
            // Validate the license
            LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23183",
                _OBJECT_NAME);

            _imageViewer = imageViewer;
        }

        #endregion BatesNumberManager Constructors

        #region BatesNumberManager Properties

        /// <summary>
        /// Gets or sets if a Bates number is required before a document can be saved.
        /// </summary>
        /// <value><see langword="true"/> if a Bates number is required before a document can be 
        /// saved; <see langword="false"/> if a document can be saved without a Bates number.
        /// </value>
        /// <returns><see langword="true"/> if a Bates number is required before a document can be 
        /// saved; <see langword="false"/> if a document can be saved without a Bates number.
        /// </returns>
        public bool RequireBates
        {
            get
            {
                return _requireBates;
            }
            set
            {
                _requireBates = value;
            }
        }

        /// <summary>
        /// Gets or sets the format for Bates numbers.
        /// </summary>
        /// <value>The format for Bates numbers.</value>
        /// <returns>The format for Bates numbers.</returns>
        public BatesNumberFormat Format
        {
            get
            {
                return _format;
            }
            set
            {
                if (_format != value)
                {
                    if (_format != null)
                    {
                        _format.Dispose();
                    }

                    _format = value;

                    if (_generator != null)
                    {
                        _generator.Dispose();
                    }
                    _generator = new BatesNumberGenerator(value);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="BatesNumberGenerator"/> for this object.
        /// </summary>
        /// <returns>The <see cref="BatesNumberGenerator"/>.</returns>
        public BatesNumberGenerator Generator
        {
            get
            {
                return _generator;
            }
        }

        /// <summary>
        /// Gets or sets the font to use when displaying a Bates number.
        /// </summary>
        /// <value>The font to use when displaying a Bates number.</value>
        /// <returns>The font to use when displaying a Bates number.</returns>
        public Font Font
        {
            get
            {
                return _font;
            }
            set
            {
                // No need to set if the fonts are already the same
                if (_font != value)
                {
                    // Dispose of the previous font
                    if (_font != null)
                    {
                        _font.Dispose();
                    }

                    // Store the new font
                    _font = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal inches from the side of the page where the Bates number is 
        /// placed.
        /// </summary>
        /// <value>The horizontal inches from the side of the page where the Bates number is 
        /// placed.</value>
        /// <returns>The horizontal inches from the side of the page where the Bates number is 
        /// placed.</returns>
        public float HorizontalInches
        {
            get
            {
                return _horizontalInches;
            }
            set
            {
                _horizontalInches = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical inches from the side of the page where the Bates number is 
        /// placed.
        /// </summary>
        /// <value>The vertical inches from the side of the page where the Bates number is 
        /// placed.</value>
        /// <returns>The vertical inches from the side of the page where the Bates number is 
        /// placed.</returns>
        public float VerticalInches
        {
            get
            {
                return _verticalInches;
            }
            set
            {
                _verticalInches = value;
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the anchor point for the page.
        /// </summary>
        /// <value>The alignment of the anchor point for the page.</value>
        /// <returns>The alignment of the anchor point for the page.</returns>
        public AnchorAlignment PageAnchorAlignment
        {
            get
            {
                return _pageAnchorAlignment;
            }
            set
            {
                _pageAnchorAlignment = value;
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the anchor point for the Bates number.
        /// </summary>
        /// <value>The alignment of the anchor point for the Bates number.</value>
        /// <returns>The alignment of the anchor point for the Bates number.</returns>
        public AnchorAlignment AnchorAlignment
        {
            get
            {
                return _anchorAlignment;
            }
            set
            {
                _anchorAlignment = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="ImageViewer"/> where the Bates number will be applied.
        /// </summary>
        /// <returns>The <see cref="ImageViewer"/> where the Bates number will be applied.</returns>
        internal ImageViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
        }

        #endregion BatesNumberManager Properties

        #region BatesNumberManager Methods

        /// <summary>
        /// Applies bates numbers to all the pages of the specified image viewer.
        /// </summary>
        /// <returns><see langword="true"/> if the method results in bates numbers being applied.  
        /// <see langword="false"/> if bates numbers were not applied due to the user cancelling
        /// the operation.</returns>
        internal bool ApplyBatesNumbers()
        {
            // Ensure an image is open
            if (_imageViewer == null || !_imageViewer.IsImageAvailable)
            {
                throw new ExtractException("ELI22314", "No image is open.");
            }

            // Check if bates numbers have already been added
            foreach (LayerObject layerObject in _imageViewer.LayerObjects)
            {
                if (layerObject.Tags.Contains(_BATES_NUMBER_TAG))
                {
                    throw new ExtractException("ELI22462", "Bates numbers already exist.");
                }
            }

            // Show the wait cursor
            using (new TemporaryWaitCursor())
            {
                // TODO: Go directly to image rather than use the image viewer control
                // Store the original page number
                int originalPageNumber = _imageViewer.PageNumber;
                try
                {
                    // Create the Bates number generator
                    if (_generator == null)
                    {
                        _generator = new BatesNumberGenerator(_format);
                    }

                    // Create a Bates number for each page
                    bool prompted = false;
                    List<TextLayerObject> batesNumbers = 
                        new List<TextLayerObject>(_imageViewer.PageCount);
                    for (int i = 1; i <= _imageViewer.PageCount; i++)
                    {
                        // Go to the ith page
                        _imageViewer.PageNumber = i;

                        // Add and increment Bates number
                        TextLayerObject batesNumber = new TextLayerObject(_imageViewer, i, 
                            "Bates number", _generator.GetNextNumberString(i), (Font)_font.Clone(), 
                            GetAnchorPoint(), _anchorAlignment, null, null);
                        batesNumber.Tags.Add(_BATES_NUMBER_TAG);
                        batesNumbers.Add(batesNumber);

                        if (!prompted)
                        {
                            // Check if the Bates number appears off the page at all.
                            if (!_imageViewer.Contains(batesNumber))
                            {
                                DialogResult result = MessageBox.Show(
                                    _OFF_PAGE_WARNING_MESSAGE, _OFF_PAGE_WARNING_TITLE, 
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, 
                                    MessageBoxDefaultButton.Button1, 0);

                                if (result == DialogResult.Yes)
                                {
                                    prompted = true;
                                }
                                else
                                {
                                    // Cancel
                                    return false;
                                }
                            }
                        }
                    }

                    // Add the Bates numbers to the image viewer
                    LayerObject previous = null;
                    foreach (LayerObject layerObject in batesNumbers)
                    {
                        _imageViewer.LayerObjects.Add(layerObject);

                        // Link to the previous Bates number
                        if (previous != null)
                        {
                            previous.AddLink(layerObject);    
                        }

                        // Increment the previous layer object
                        previous = layerObject;
                    }

                    // Commit the changes to the next Bates number
                    _generator.Commit();
                }
                finally
                {
                    // Restore the original page number
                    _imageViewer.PageNumber = originalPageNumber;
                }
            }

            // Redraw the image viewer
            _imageViewer.Invalidate();

            return true;
        }

        /// <summary>
        /// Retrieves the anchor point in logical (image) coordinates.
        /// </summary>
        /// <returns>The anchor point in logical (image) coordinates.</returns>
        Point GetAnchorPoint()
        {
            return GetAnchorPoint(_pageAnchorAlignment, _horizontalInches, _verticalInches,
                _imageViewer);
        }

        /// <summary>
        /// Retrieves the anchor point in logical (image) coordinates.
        /// </summary>
        /// <param name="pageAnchorAlignment">The alignment of the anchor point for the
        /// page.</param>
        /// <param name="horizontalInches">The number of horizontal inches from the side
        /// of the page where the Bates number is placed.</param>
        /// <param name="verticalInches">The number of vertical inches from the side of
        /// the page where the Bates number is placed.</param>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> where the Bates number
        /// will be applied.</param>
        /// <returns>The anchor point in logical (image) coordinates.</returns>
        internal static Point GetAnchorPoint(AnchorAlignment pageAnchorAlignment,
            float horizontalInches, float verticalInches, ImageViewer imageViewer)
        {
            // Calculate the positive offset in logical (image) coordinates
            Size offset = GetAnchorPointOffset(horizontalInches, verticalInches, imageViewer);

            // Calculate the top left coordinate based on the anchor alignment
            Point anchorPoint;
            switch (pageAnchorAlignment)
            {
                case AnchorAlignment.LeftBottom:
                    anchorPoint = new Point(offset.Width, imageViewer.ImageHeight - offset.Height);
                    break;

                case AnchorAlignment.RightBottom:
                    anchorPoint = new Point(imageViewer.ImageWidth - offset.Width,
                        imageViewer.ImageHeight - offset.Height);
                    break;

                case AnchorAlignment.LeftTop:
                    anchorPoint = new Point(offset);
                    break;

                case AnchorAlignment.RightTop:
                    anchorPoint = new Point(imageViewer.ImageWidth - offset.Width, offset.Height);
                    break;

                default:
                    ExtractException ee = new ExtractException("ELI22368",
                        "Unexpected anchor alignment.");
                    ee.AddDebugData("Anchor alignment", pageAnchorAlignment, false);
                    throw ee;
            }

            return anchorPoint;
        }

        /// <summary>
        /// Retrieves the positive offset of the Bates anchor point in logical (image) pixels.
        /// </summary>
        /// <param name="horizontalInches">The number of horizontal inches from the side
        /// of the page where the Bates number is placed.</param>
        /// <param name="verticalInches">The number of vertical inches from the side of
        /// the page where the Bates number is placed.</param>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> where the Bates number
        /// will be applied.</param>
        /// <returns>The positive offset of the Bates anchor point in logical (image) pixels.
        /// </returns>
        static Size GetAnchorPointOffset(float horizontalInches, float verticalInches,
            ImageViewer imageViewer)
        {
            return new Size((int)(horizontalInches * imageViewer.ImageDpiX + 0.5),
                (int)(verticalInches * imageViewer.ImageDpiY + 0.5));
        }

        /// <summary>
        /// Prompts the user whether to output a document without Bates number if 
        /// <see cref="RequireBates"/> is <see langword="true"/>.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="RequireBates"/> is 
        /// <see langword="false"/>, the image already contains Bates numbers, or the user chose 
        /// to continue without Bates numbers; <see langword="false"/> if the user chose to 
        /// cancel.</returns>
        public bool PromptForRequiredBatesNumber()
        {
            // If Bates numbers aren't required, we are done.
            if (!_requireBates)
            {
                return true;
            }

            // Ensure that the image contains Bates numbers.
            bool hasBates = false;
            foreach (LayerObject layerObject in _imageViewer.LayerObjects)
            {
                if (layerObject.Tags.Contains(_BATES_NUMBER_TAG))
                {
                    hasBates = true;
                    break;
                }
            }

            // Check whether Bates numbers were applied
            if (hasBates)
            {
                return true;
            }

            // Prepare a message to the user
            string text = 
                "ID Shield Office has been configured to require Bates numbers on output documents," 
                + Environment.NewLine + "but this document does not have Bates numbers on it."
                + Environment.NewLine + "Are you sure you want to continue?";

            // Prompt the user whether to apply Bates numbers
            DialogResult result = MessageBox.Show(text, "Create without Bates numbers?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, 
                MessageBoxDefaultButton.Button1, 0);

            // Return the user's response
            return result != DialogResult.No;
        }

        /// <summary>
        /// Reads a <see cref="BatesNumberManager"/> from the registry.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the Bates number manager operates.
        /// </param>
        /// <returns>A <see cref="BatesNumberManager"/> read from the registry.</returns>
        public static BatesNumberManager FromRegistry(ImageViewer imageViewer)
        {
            // Create the Bates number manager
            BatesNumberManager manager = new BatesNumberManager(imageViewer);

            // Get the settings from the registry
            manager.ReadFromRegistry();

            // Return the result
            return manager;
        }

        /// <summary>
        /// Reads the settings from the registry.
        /// </summary>
        public void ReadFromRegistry()
        {
            // Read the format settings from the registry
            _format.ReadFromRegistry();

            // Read the other settings from the registry
            _anchorAlignment = RegistryManager.BatesAnchorAlignment;
            _pageAnchorAlignment = RegistryManager.BatesPageAnchorAlignment;
            _font = RegistryManager.BatesFont;
            _horizontalInches = RegistryManager.BatesHorizontalInches;
            _verticalInches = RegistryManager.BatesVerticalInches;
            _requireBates = RegistryManager.RequireBates;
        }

        /// <summary>
        /// Writes the <see cref="BatesNumberManager"/> to the registry.
        /// </summary>
        public void WriteToRegistry()
        {
            // Write the format settings to the registry
            _format.WriteToRegistry();

            // Get the settings from the registry
            RegistryManager.BatesAnchorAlignment = _anchorAlignment;
            RegistryManager.BatesPageAnchorAlignment = _pageAnchorAlignment;
            RegistryManager.BatesFont = _font;
            RegistryManager.BatesHorizontalInches = _horizontalInches;
            RegistryManager.BatesVerticalInches = _verticalInches;
            RegistryManager.RequireBates = _requireBates;
        }

        /// <summary>
        /// Reset the <see cref="BatesNumberManager"/> settings to their defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            // Reset the format settings from the registry
            _format.ResetToDefaults();

            // Read the other settings from the registry
            _anchorAlignment = RegistryManager._BATES_ANCHOR_ALIGNMENT_DEFAULT;
            _pageAnchorAlignment = RegistryManager._BATES_PAGE_ANCHOR_ALIGNMENT_DEFAULT;
            _font = new Font(RegistryManager._BATES_FONT_FAMILY_DEFAULT,
            RegistryManager._BATES_FONT_SIZE_DEFAULT, RegistryManager._BATES_FONT_STYLE_DEFAULT);
            _horizontalInches = RegistryManager._BATES_HORIZONTAL_INCHES_DEFAULT;
            _verticalInches = RegistryManager._BATES_VERTICAL_INCHES_DEFAULT;
            _requireBates = RegistryManager._REQUIRE_BATES_DEFAULT;
        }

        #endregion BatesNumberManager Methods

        #region IUserConfigurableComponent Members

        /// <summary>
        /// Gets or sets the property page of the <see cref="BatesNumberManager"/>.
        /// </summary>
        /// <return>The property page of the <see cref="BatesNumberManager"/>.</return>
        public UserControl PropertyPage
        {
            get
            {
                // Create the property page if not already created
                if (_propertyPage == null)
                {
                    _propertyPage = new BatesNumberManagerPropertyPage(this);
                }

                return _propertyPage;
            }
        }

        #endregion IUserConfigurableComponent Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="BatesNumberManager"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="BatesNumberManager"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BatesNumberManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_font != null)
                {
                    _font.Dispose();
                    _font = null;
                }
                if (_propertyPage != null)
                {
                    _propertyPage.Dispose();
                    _propertyPage = null;
                }
                if (_generator != null)
                {
                    _generator.Dispose();
                    _generator = null;
                }
                if (_format != null)
                {
                    _format.Dispose();
                    _format = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
