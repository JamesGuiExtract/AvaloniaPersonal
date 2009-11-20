using System;
using System.Drawing;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a particular zoom setting.
    /// </summary>
    public struct ZoomInfo : IEquatable<ZoomInfo>
    {
        #region Fields

        /// <summary>
        /// The center point of the visible image in logical (image) coordinates.
        /// </summary>
        Point _zoomCenter;

        /// <summary>
        /// The scale factor applied to the image.
        /// </summary>
        /// <remarks>This value is undefined if <see cref="_fitMode"/> is not 
        /// <see cref="Extract.Imaging.Forms.FitMode.None"/>.</remarks>
        double _scaleFactor;

        /// <summary>
        /// The mode to scale the image within the visible area.
        /// </summary>
        FitMode _fitMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomInfo"/> class with the specified 
        /// zoom area and fit mode.
        /// </summary>
        /// <param name="center">The center of the visible image area in logical (image) 
        /// coordinates.</param>
        /// <param name="scaleFactor">The ratio of physical (client) pixel size to logical (image) 
        /// pixel size.</param>
        /// <param name="fitMode">The mode to scale the image within the visible area.</param>
        public ZoomInfo(Point center, double scaleFactor, FitMode fitMode)
        {
            _zoomCenter = center;
            _scaleFactor = scaleFactor;
            _fitMode = fitMode;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the center point of the visible image in logical (image) coordinates.
        /// </summary>
        /// <value>The center point of the visible image in logical (image) coordinates.
        /// </value>
        /// <returns>The center point of the visible image in logical (image) coordinates.
        /// </returns>
        public Point Center
        {
            get
            {
                return _zoomCenter;
            }
            set
            {
                _zoomCenter = value;
            }
        }

        /// <summary>
        /// Gets or sets the scale factor applied to the image.
        /// </summary>
        /// <value>The scale factor applied to the image.</value>
        /// <returns>The scale factor applied to the image. This value is undefined if the
        /// <see cref="FitMode"/> property is not 
        /// <see cref="Extract.Imaging.Forms.FitMode.None"/>.</returns>
        public double ScaleFactor
        {
            get
            {
                return _scaleFactor;
            }
            set
            {
                _scaleFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the mode to scale the image within the visible area.
        /// </summary>
        /// <value>The mode to scale the image within the visible area.</value>
        /// <returns>The mode to scale the image within the visible area.</returns>
        public FitMode FitMode
        {
            get
            {
                return _fitMode;
            }
            set
            {
                _fitMode = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is a <see cref="ZoomInfo"/> and 
        /// whether it describes the same zoom setting as this <see cref="ZoomInfo"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="ZoomInfo"/> and 
        /// describes the same zoom setting as this <see cref="ZoomInfo"/>; <see langword="false"/>
        /// if they differ.</returns>
        public override bool Equals(object obj)
        {
            return obj is ZoomInfo ? Equals((ZoomInfo)obj) : false;
        }

        /// <summary>
        /// Returns a hash code for this <see cref="ZoomInfo"/>
        /// </summary>
        /// <returns>A hash value for this <see cref="ZoomInfo"/>.</returns>
        public override int GetHashCode()
        {
            return (int)_fitMode ^ _scaleFactor.GetHashCode() ^ _zoomCenter.GetHashCode();
        }

        #endregion Methods

        #region Operators

        /// <summary>
        /// Compares two <see cref="ZoomInfo"/> objects. The result specifies whether the two 
        /// <see cref="ZoomInfo"/> objects are equal.
        /// </summary>
        /// <param name="left">A <see cref="ZoomInfo"/> to compare.</param>
        /// <param name="right">A <see cref="ZoomInfo"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> describes the same zoom 
        /// setting as <paramref name="right"/>; <see langword="false"/> if <paramref name="left"/>
        /// and <paramref name="right"/> differ.</returns>
        public static bool operator ==(ZoomInfo left, ZoomInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="ZoomInfo"/> objects. The result specifies whether the two 
        /// <see cref="ZoomInfo"/> objects are unequal.
        /// </summary>
        /// <param name="left">A <see cref="ZoomInfo"/> to compare.</param>
        /// <param name="right">A <see cref="ZoomInfo"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// differ; <see langword="false"/> if <paramref name="left"/> describes the same zoom 
        /// setting as <paramref name="right"/>.</returns>
        public static bool operator !=(ZoomInfo left, ZoomInfo right)
        {
            return !left.Equals(right);
        }

        #endregion Operators

        #region IEquatable<ZoomInfo> Members

        /// <summary>
        /// Determines whether this <see cref="ZoomInfo"/> describes the same zoom setting as the 
        /// specified <see cref="ZoomInfo"/>.
        /// </summary>
        /// <param name="other">The <see cref="ZoomInfo"/> object to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> describes the same zoom 
        /// setting as this <see cref="ZoomInfo"/>; <see langword="false"/> if they differ.</returns>
        public bool Equals(ZoomInfo other)
        {
            return _fitMode == other._fitMode &&
                _scaleFactor == other._scaleFactor &&
                _zoomCenter == other._zoomCenter;
        }

        #endregion IEquatable<ZoomInfo> Members
    }
}