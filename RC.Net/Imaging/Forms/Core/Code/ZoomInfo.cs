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
        double _scaleFactor;

        /// <summary>
        /// The image orientation in degrees.
        /// </summary>
        int _orientation;

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
        /// <param name="orientation">The image orientation in degrees.</param>
        public ZoomInfo(Point center, double scaleFactor, int orientation)
        {
            _zoomCenter = center;
            _scaleFactor = scaleFactor;
            _orientation = orientation;
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
        /// <returns>The scale factor applied to the image.</returns>
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
        /// Gets or sets the image orientation in degrees.
        /// </summary>
        /// <value>The image orientation in degrees.</value>
        public int Orientation
        {
            get
            {
                return _orientation;
            }

            set
            {
                _orientation = value;
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
            return (int)_scaleFactor.GetHashCode() ^ _zoomCenter.GetHashCode()
                ^ _orientation.GetHashCode();
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
            if (_scaleFactor != other._scaleFactor ||  _orientation != other._orientation)
            {
                return false;
            }

            if (_zoomCenter == other._zoomCenter)
            {
                return true;
            }
            else
            {
                // Sometimes rounding issues cause the center to be 1 pixel different than the zoom
                // info used to set the current zoom. Allow for 1 pixel difference.
                return (Math.Abs(_zoomCenter.X - other._zoomCenter.X) <= 1 &&
                        Math.Abs(_zoomCenter.Y - other._zoomCenter.Y) <= 1);
            }
        }

        #endregion IEquatable<ZoomInfo> Members
    }
}
