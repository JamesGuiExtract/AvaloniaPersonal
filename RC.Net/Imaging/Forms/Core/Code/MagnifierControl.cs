using System;
using System.ComponentModel;
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
                _imageViewer = value;
            }
        }

        #endregion IImageViewerControl Members
    }
}
