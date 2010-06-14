using Extract.Drawing;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents data associated with tracking interactive cursor tool events.
    /// </summary>
    internal sealed class TrackingData : IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(TrackingData).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The control on which the tracking event takes place.
        /// </summary>
        readonly Control _control;

        /// <summary>
        /// The point where the cursor was when the mouse button was first pressed during a 
        /// tracking event.
        /// </summary>
        /// <seealso cref="StartPoint"/>
        Point _startPoint;

        /// <summary>
        /// The height of angular regions in physical (client) coordinates.
        /// </summary>
        int _height;

        /// <summary>
        /// The region in physical (client) coordinates described by the interactive tracking 
        /// event in progress.
        /// </summary>
        /// <seealso cref="Region"/>
        /// <seealso cref="UpdateRectangularRegion"/>
        Region _region = new Region();

        /// <summary>
        /// The rectangle in physical (client) coordinates described by the interactive tracking 
        /// event in progress.
        /// </summary>
        /// <seealso cref="Rectangle"/>
        /// <seealso cref="UpdateRectangularRegion"/>
        Rectangle _rectangle;

        /// <summary>
        /// An array of two points in screen coordinates describing a line segment.
        /// </summary>
        /// <seealso cref="Line"/>
        /// <seealso cref="UpdateLine"/>
        readonly Point[] _line;

        /// <summary>
        /// The area within which all tracking data should be contained.
        /// </summary>
        readonly Rectangle _cropWithin;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new instance of the <see cref="TrackingData"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingData"/> class for a basic 
        /// interactive event.
        /// </summary>
        /// <param name="control">The control on which the tracking event takes place.</param>
        /// <param name="startX">The physical (client) x-coordinate of the start of the 
        /// interactive event being tracked.</param>
        /// <param name="startY">The physical (client) y-coordinate of the start of the 
        /// interactive event being tracked.</param>
        /// <param name="cropWithin">The rectangle within which all regions must be cropped.
        /// </param>
        public TrackingData(Control control, float startX, float startY, Rectangle cropWithin)
            : this(control, startX, startY, cropWithin, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingData"/> class that tracks an 
        /// angular region.
        /// </summary>
        /// <param name="control">The control on which the tracking event takes place.</param>
        /// <param name="startX">The physical (client) x-coordinate of the start of the 
        /// interactive event being tracked.</param>
        /// <param name="startY">The physical (client) y-coordinate of the start of the 
        /// interactive event being tracked.</param>
        /// <param name="cropWithin">The rectangle within which all regions must be cropped.
        /// </param>
        /// <param name="height">The height in physical (client) pixel for calculated angular 
        /// regions.</param>
        public TrackingData(Control control, float startX, float startY, Rectangle cropWithin, 
            int height)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23133",
					_OBJECT_NAME);

                // Store the parameters
                _control = control;
                _startPoint = new Point((int)(startX + 0.5F), (int)(startY + 0.5F));
                _cropWithin = cropWithin;
                _height = height;

                // Clear the region
                _region.MakeEmpty();

                // Construct a default line segment in screen coordinates
                Point startPointInScreen = _control.PointToScreen(_startPoint);
                _line = new Point[] { startPointInScreen, startPointInScreen };

                // Capture mouse events
                control.Capture = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23134", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the starting point of the tracking event in physical (client) coordinates.
        /// </summary>
        /// <value>The starting point of the tracking event in physical (client) coordinates.
        /// </value>
        /// <return>The starting point of the tracking event in physical (client) coordinates.
        /// </return>
        public Point StartPoint
        {
            get
            {
                return _startPoint;
            }
            set
            {
                _startPoint = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of angular regions in physical (client) coordinates.
        /// </summary>
        /// <value>The height of angular regions in physical (client) coordinates.</value>
        /// <returns>The height of angular regions in physical (client) coordinates.</returns>
        public int Height
        {
            // This method is uncalled, but the set property is needed 
            // and set-only properties are discouraged.
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }
        
        /// <summary>
        /// Gets the region being tracked.
        /// </summary>
        /// <value>The region being tracked.</value>
        public Region Region
        {
            get
            {
                return _region;
            }
        }

        /// <summary>
        /// Gets the rectangular area being tracked.
        /// </summary>
        /// <value>The rectangular area being tracked.</value>
        public Rectangle Rectangle
        {
            get
            {
                return _rectangle;
            }
        }

        /// <summary>
        /// Gets the line segment in screen coordinates described by an array of two points.
        /// </summary>
        /// <value>The line segment in screen coordinates described by an array of two points.
        /// </value>
        public Point[] Line
        {
            get
            {
                return _line;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the <see cref="Region"/> using the point specified.
        /// </summary>
        /// <param name="x">The physical (client) x coordinate of the mouse cursor.</param>
        /// <param name="y">The physical (client) y coordinate of the mouse cursor.</param>
        public void UpdateAngularRegion(int x, int y)
        {
            // Dispose of the previous region if it exists
            if (_region != null)
            {
                _region.Dispose();
            }

            // Update the region
            _region = Highlight.GetAngularRegion(_startPoint, new Point(x, y), _height);
            _region.Intersect(_cropWithin);
        }

        /// <summary>
        /// Updates the <see cref="Rectangle"/> and <see cref="Region"/> using the point 
        /// specified.
        /// </summary>
        /// <param name="x">The physical (client) x coordinate of the mouse cursor.</param>
        /// <param name="y">The physical (client) y coordinate of the mouse cursor.</param>
        public void UpdateRectangularRegion(int x, int y)
        {
            // Calculate the new rectangle
            UpdateRectangle(x, y);

            // Dispose of the previous region if it exists
            if (_region != null)
            {
                _region.Dispose();
            }

            // Update the region
            _region = new Region(_rectangle);
        }

        /// <summary>
        /// Updates the <see cref="Rectangle"/> using the point specified.
        /// </summary>
        /// <param name="x">The physical (client) x coordinate of the mouse cursor.</param>
        /// <param name="y">The physical (client) y coordinate of the mouse cursor.</param>
        public void UpdateRectangle(int x, int y)
        {
            // Calculate the x coordinates of the rectangle
            int left = _startPoint.X;
            int right = x;
            if (_startPoint.X > x)
            {
                UtilityMethods.Swap(ref left, ref right);
            }

            // Calculate the y coordinates of the rectangle
            int top = _startPoint.Y;
            int bottom = y;
            if (_startPoint.Y > y)
            {
                UtilityMethods.Swap(ref top, ref bottom);
            }

            // Create the new rectangle
            _rectangle = Rectangle.FromLTRB(left, top, right, bottom);
            _rectangle.Intersect(_cropWithin);
        }

        /// <summary>
        /// Updates the <see cref="Line"/> property using the point specified.
        /// </summary>
        /// <param name="x">The physical (client) x coordinate of the mouse cursor.</param>
        /// <param name="y">The physical (client) y coordinate of the mouse cursor.</param>
        public void UpdateLine(int x, int y)
        {
            // Ensure the end point is within the cropping rectangle
            Point endPoint = GeometryMethods.GetClippedEndPoint(
                _startPoint, new Point(x, y), _cropWithin);

            // Store the new endpoint in screen coordinates
            _line[1] = _control.PointToScreen(endPoint);
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="TrackingData"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="TrackingData"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="TrackingData"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed objects
                if (_region != null)
                {
                    _region.Dispose();
                    _region = null;
                }
                // End the tracking event
                if (_control != null)
                {
                    _control.Capture = false;
                }
            }

            // No unmanaged resources to free
        }

        #endregion IDisposable Members
    }
}
