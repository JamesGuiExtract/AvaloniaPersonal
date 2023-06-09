using Extract.Drawing;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Xml;

using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;

namespace Extract.Imaging
{
    /// <summary>
    /// A class for storing the zone information for a region of a image.
    /// </summary>
    public class RasterZone : IComparable<RasterZone>
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(RasterZone).ToString();

        /// <summary>
        /// The allowable difference between a floating point coordinate and the nearest whole
        /// number to allow the coordinate to be rounded off without using the specialized rounding
        /// from the <see cref="ZoneGeometry"/> class.
        /// </summary>
        static readonly float _ROUNDOFF_LEEWAY = 0.001F;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The starting point of the <see cref="RasterZone"/>.
        /// </summary>
        PointF _start;

        /// <summary>
        /// The end point of the <see cref="RasterZone"/>.
        /// </summary>
        PointF _end;

        /// <summary>
        /// The height of the <see cref="RasterZone"/>.
        /// </summary>
        float _height;

        /// <summary>
        /// The page number of the <see cref="RasterZone"/>.
        /// </summary>
        int _pageNumber;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new <see cref="RasterZone"/> class.</overloads>
        /// <summary>
        /// Initliazes a new <see cref="RasterZone"/> class with default arguments.
        /// </summary>
        public RasterZone() :
            this(0, 0, 0, 0, 0, -1)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="RasterZone"/> class with specified
        /// start point, end point, and height on the specified page.
        /// </summary>
        /// <param name="startPoint">A <see cref="Point"/> containing the start point
        /// for the raster zone.</param>
        /// <param name="endPoint">A <see cref="Point"/>containing the end point
        /// for the raster zone.</param>
        /// <param name="height">The height of the raster zone.</param>
        /// <param name="pageNumber">The page that the raster zone is defined for.</param>
        // This is not the compound word "endpoint". This is the "end point", meant in contrast to
        // "start point".
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "endPoint")]
        public RasterZone(Point startPoint, Point endPoint, int height, int pageNumber) :
            this(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, height, pageNumber)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="RasterZone"/> class with specified
        /// start point, end point, and height on the specified page.
        /// </summary>
        /// <param name="startPoint">A <see cref="Point"/> containing the start point
        /// for the raster zone.</param>
        /// <param name="endPoint">A <see cref="Point"/>containing the end point
        /// for the raster zone.</param>
        /// <param name="height">The height of the raster zone.</param>
        /// <param name="pageNumber">The page that the raster zone is defined for.</param>
        // This is not the compound word "endpoint". This is the "end point", meant in contrast to
        // "start point".
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "endPoint")]
        public RasterZone(PointF startPoint, PointF endPoint, float height, int pageNumber)
        {
            // Validate the license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32829",
                _OBJECT_NAME);

            _start = startPoint;
            _end = endPoint;
            _height = height;
            _pageNumber = pageNumber;
        }

        /// <summary>
        /// Initializes a new <see cref="RasterZone"/> class from the specified
        /// <see cref="Rectangle"/> and page number.
        /// </summary>
        /// <param name="rectangle">A <see cref="Rectangle"/> object.</param>
        /// <param name="pageNumber">The page number for this <see cref="RasterZone"/></param>
        public RasterZone(Rectangle rectangle, int pageNumber)
            : this(rectangle.Left,
                ((int)(rectangle.Height / 2.0 + 0.5)) + rectangle.Top, rectangle.Right,
                ((int)(rectangle.Height / 2.0 + 0.5)) + rectangle.Top, rectangle.Height,
                pageNumber)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="RasterZone"/> class from the specified
        /// RasterZone object.
        /// </summary>
        /// <param name="comRasterZone">A RasterZone object.</param>
        [CLSCompliant(false)]
        public RasterZone(ComRasterZone comRasterZone)
        {
            try
            {
                ExtractException.Assert("ELI22052", "comRasterZone cannot be null.",
                    comRasterZone != null);

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23122",
                    _OBJECT_NAME);

                _start.X = comRasterZone.StartX;
                _start.Y = comRasterZone.StartY;
                _end.X = comRasterZone.EndX;
                _end.Y = comRasterZone.EndY;
                _pageNumber = comRasterZone.PageNumber;

                // [LegacyRCAndUtils:6216, 6217]
                // For the time being, to address the issue where RasterZones gain extra padding 
                // every time they are opened and edited in the .Net image viewer convert the height
                // to the .Net side by subtracting 1.
                _height = comRasterZone.Height - 1;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22054", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="RasterZone"/> class with specified
        /// start point, end point, and height on the specified page.
        /// </summary>
        /// <param name="startX">The X-coordinate for the start point.</param>
        /// <param name="startY">The Y-coordinate for the start point.</param>
        /// <param name="endX">The X-coordinate for the end point.</param>
        /// <param name="endY">The Y-coodinate for the end point.</param>
        /// <param name="height">The height of the raster zone.</param>
        /// <param name="pageNumber">The page that the raster zone is defined for.</param>
        public RasterZone(int startX, int startY, int endX, int endY, int height, int pageNumber)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23123",
                    _OBJECT_NAME);

                _start = new Point(startX, startY);
                _end = new Point(endX, endY);
                _height = height;
                _pageNumber = pageNumber;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI22055", "Failed to initialize RasterZone.", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Computes the area of the <see cref="RasterZone"/>.
        /// </summary>
        /// <returns>The area of the <see cref="RasterZone"/>.</returns>
        public double Area()
        {
            try
            {
                return GeometryMethods.Area(_start, _end, _height);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22188", ex);
            }
        }

        /// <summary>
        /// Computes the area of overlap between this <see cref="RasterZone"/>
        /// and a specified <see cref="RasterZone"/>.
        /// </summary>
        /// <param name="otherRasterZone">The <see cref="RasterZone"/> to compute overlap
        /// with. This may not be <see langword="null"/>.</param>
        /// <returns>The area of overlap between this <see cref="RasterZone"/> and a
        /// specified <see cref="RasterZone"/>.</returns>
        /// <exception cref="ExtractException">If <paramref name="otherRasterZone"/>
        /// is <see langword="null"/>.</exception>
        public double GetAreaOverlappingWith(RasterZone otherRasterZone)
        {
            try
            {
                ExtractException.Assert("ELI22189", "Raster zone cannot be null.",
                    otherRasterZone != null);

                // Default overlap to 0.0
                double overlap = 0.0;

                // Compute overlap if zones are on the same page
                if (_pageNumber == otherRasterZone.PageNumber)
                {
                    // Get the area of overlap
                    overlap = GetAreaOverlappingWith(otherRasterZone.ToComRasterZone());
                }
                // Else - zones on different pages, just return default percentage (0.0)

                return overlap;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22190", ex);
            }
        }

        /// <summary>
        /// Computes the area of overlap between this <see cref="RasterZone"/>
        /// and a specified COM RasterZone.
        /// </summary>
        /// <param name="comRasterZone">The COM RasterZone to compute overlap
        /// with. This may not be <see langword="null"/>.</param>
        /// <returns>The area of overlap between this <see cref="RasterZone"/> and a
        /// specified COM RasterZone.</returns>
        /// <exception cref="ExtractException">If <paramref name="comRasterZone"/>
        /// is <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public double GetAreaOverlappingWith(ComRasterZone comRasterZone)
        {
            try
            {
                ExtractException.Assert("ELI22289", "Raster zone cannot be null.",
                    comRasterZone != null);

                // Default overlap to 0.0
                double overlap = 0.0;

                // Ensure the zones are on the same page
                if (_pageNumber == comRasterZone.PageNumber)
                {
                    // TODO: It would be good to compute the area of overlap
                    // on the .NET side without calling into COM, this is a bigger
                    // design effort so for now just use the COM RasterZone to compute
                    // the area of overlap

                    // Get the area of overlap
                    overlap = ToComRasterZone().GetAreaOverlappingWith(comRasterZone);
                }

                return overlap;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22290", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="RasterZone"/>'s coordinates rounded using the specified
        /// <see paramref="roundingMode"/>.
        /// </summary>
        /// <param name="roundingMode">The <see cref="RoundingMode"/> to use.</param>
        /// <param name="start">A <see cref="PointF"/> specifying the start point.</param>
        /// <param name="end">A <see cref="PointF"/> specifying the end point.</param>
        /// <param name="height">The height of the raster zone.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        public void GetRoundedCoordinates(RoundingMode roundingMode, out Point start, out Point end,
            out int height)
        {
            try
            {
                ExtractException.Assert("ELI32827", "Invalid rounding mode.",
                    roundingMode != RoundingMode.None);

                if (Math.Abs(_start.X - Math.Round(_start.X)) > _ROUNDOFF_LEEWAY ||
                    Math.Abs(_start.Y - Math.Round(_start.Y)) > _ROUNDOFF_LEEWAY ||
                    Math.Abs(_end.X - Math.Round(_end.X)) > _ROUNDOFF_LEEWAY ||
                    Math.Abs(_end.Y - Math.Round(_end.Y)) > _ROUNDOFF_LEEWAY ||
                    Math.Abs(_height - Math.Round(_height)) > _ROUNDOFF_LEEWAY)
                {
                    ZoneGeometry zoneGeometry = new ZoneGeometry(this);

                    PointF startF;
                    PointF endF;
                    float heightF;
                    zoneGeometry.GetZoneCoordinates(roundingMode, out startF, out endF, out heightF);

                    start = Point.Round(startF);
                    end = Point.Round(endF);
                    height = (int)Math.Round(heightF);
                }
                else
                {
                    start = Point.Round(_start);
                    end = Point.Round(_end);
                    height = (int)Math.Round(_height);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32826");
            }
        }

        /// <summary>
        /// Computes the smallest <see cref="Rectangle"/> that contains
        /// this <see cref="RasterZone"/>.
        /// </summary>
        /// <returns>The smallest <see cref="Rectangle"/> that contains
        /// this <see cref="RasterZone"/>.</returns>
        // This method performs a calculation so is better suited as a method
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Rectangle GetRectangularBounds()
        {
            try
            {
                Point start;
                Point end;
                int height;
                GetRoundedCoordinates(RoundingMode.Safe, out start, out end, out height);

                // Return the bounding rectangle
                return GeometryMethods.GetBoundingRectangle(start, end, height);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22191", ex);
            }
        }

        /// <summary>
        /// Computes an array of <see cref="PointF"/> objects that represent
        /// the bounds of this <see cref="RasterZone"/>.
        /// </summary>
        /// <returns>An array <see cref="PointF"/> objects that represent the bounds
        /// of this <see cref="RasterZone"/>.</returns>
        public PointF[] GetBoundaryPoints()
        {
            try
            {
                return GeometryMethods.GetVertices(_start, _end, _height);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22192", ex);
            }
        }

        /// <summary>
        /// Creates a RasterZone from this <see cref="RasterZone"/>.
        /// </summary>
        /// <returns>A RasterZone created from this
        /// <see cref="RasterZone"/>.</returns>
        [CLSCompliant(false)]
        public ComRasterZone ToComRasterZone()
        {
            try
            {
                // Create a new COM RasterZone
                ComRasterZone comRasterZone = new ComRasterZone();

                Point start;
                Point end;
                int height;
                GetRoundedCoordinates(RoundingMode.Safe, out start, out end, out height);

                // Copy the .Net RasterZone data to the COM RasterZone
                // [LegacyRCAndUtils:6216, 6217]
                // For the time being, to address the issue where RasterZones gain extra padding 
                // every time they are opened and edited in the .Net image viewer convert the height
                // to the .c++ side by adding 1.
                comRasterZone.CreateFromData(start.X, start.Y, end.X, end.Y, height + 1, _pageNumber);

                return comRasterZone;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22053", ex);
            }
        }

        /// <summary>
        /// Splits a collection of <see cref="RasterZone"/> objects by their page numbers
        /// and returns a dictionary of the new split collections keyed by page number.
        /// </summary>
        /// <param name="rasterZones">An <see cref="IEnumerable{T}"/> collection of
        /// <see cref="RasterZone"/> objects to split. This must not be
        /// <see langword="null"/>.</param>
        /// <returns>A <see cref="Dictionary{T, T}"/> containing <see cref="List{T}"/>
        /// collections of <see cref="RasterZone"/> objects grouped by their
        /// <see cref="RasterZone.PageNumber"/>.
        /// </returns>
        // FxCop does not like the nested generic type in the return from this method because
        // it makes the syntax "complex" (see http://msdn.microsoft.com/en-us/ms182144.aspx).
        // The case sited in the website is the case where the nested type is an in parameter,
        // in this case it is the out parameter.  There is no need to instantiate a nested
        // generic type to call this method.  It will return a nested generic type.  The
        // alternative would be to use an array as one of the objects in the Dictionary,
        // but it would lead to additional complexity in this method and leave the resulting
        // collection almost as complex.
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Dictionary<int, List<RasterZone>> SplitZonesByPage(
            IEnumerable<RasterZone> rasterZones)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23154",
                    _OBJECT_NAME);

                ExtractException.Assert("ELI22531", "Raster zone collection must not be null.",
                    rasterZones != null);

                // Create a the return dictionary object then iterate through each
                // raster zone adding them to the collection keyed by their page number
                Dictionary<int, List<RasterZone>> zonesByPage =
                    new Dictionary<int, List<RasterZone>>();
                foreach (RasterZone rasterZone in rasterZones)
                {
                    // Check if there is already a list for this page
                    List<RasterZone> zonesOnPage;
                    if (zonesByPage.TryGetValue(rasterZone.PageNumber, out zonesOnPage))
                    {
                        // Add the zone to the list
                        zonesOnPage.Add(rasterZone);
                    }
                    else
                    {
                        // No list for this page, create the list and add it to the collection
                        zonesOnPage = new List<RasterZone>(new RasterZone[] { rasterZone });
                        zonesByPage.Add(rasterZone.PageNumber, zonesOnPage);
                    }
                }

                // Return the collection
                return zonesByPage;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22532", ex);
            }
        }

        /// <summary>
        /// Gets a bounding <see cref="RasterZone"/> for all of the <see paramref="rasterZones"/>
        /// on the specified <see paramref="pageNumber"/>.
        /// </summary>
        /// <param name="rasterZones">The source <see cref="RasterZone"/>s.</param>
        /// <param name="pageNumber">The page number for which a <see cref="RasterZone"/> is needed.
        /// </param>
        /// <returns>A <see cref="RasterZone"/> bounding all <see paramref="rasterZones"/> on the
        /// specified <see paramref="pageNumber"/>.</returns>
        public static RasterZone GetBoundingRasterZone(IEnumerable<RasterZone> rasterZones,
            int pageNumber)
        {
            try
            {
                List<RasterZone> pageRasterZones;
                if (RasterZone.SplitZonesByPage(rasterZones).TryGetValue(pageNumber,
                    out pageRasterZones))
                {
                    // If there's only one zone on the line, simply use it.
                    if (pageRasterZones.Count == 1)
                    {
                        return pageRasterZones[0];
                    }

                    // Otherwise, find a bounding rectangle for the zones in a coordinate
                    // system aligned with the zones' average angle.
                    double angle;
                    RectangleF bounds =
                        RasterZone.GetAngledBoundingRectangle(pageRasterZones, out angle);

                    // Create a start and end point for the zone in the raster zones' coordinate
                    // system.
                    float verticalMidPoint = bounds.Top + bounds.Height / 2F;
                    PointF[] points = new PointF[]
                                        {
                                            new PointF(bounds.Left, verticalMidPoint),
                                            new PointF(bounds.Right, verticalMidPoint)
                                        };

                    // Translate these points into the image coordinate system.
                    using (Matrix transform = new Matrix())
                    {
                        transform.Rotate((float)angle);
                        transform.TransformPoints(points);
                    }

                    // Adjust coordinates to ensure the raster zone doesn't shrink from points that
                    // are rounded off in the wrong direction.
                    int startX = (int)((points[0].X < points[1].X)
                        ? points[0].X
                        : Math.Ceiling(points[0].X));
                    int startY = (int)((points[0].Y < points[1].Y)
                        ? points[0].Y
                        : Math.Ceiling(points[0].Y));
                    int endX = (int)((points[1].X < points[0].X)
                        ? points[1].X
                        : Math.Ceiling(points[1].X));
                    int endY = (int)((points[1].Y < points[0].Y)
                        ? points[1].Y
                        : Math.Ceiling(points[1].Y));
                    int height = (int)Math.Ceiling(bounds.Height);

                    // If the height is odd, the top and bottom of the zone will be a .5 value. When
                    // such a zone is displayed, those values will be rounded off, potentially
                    // exposing pixel content. Expand the zone by a pixel to prevent this.
                    if (height % 2 == 1)
                    {
                        height++;
                    }

                    return new RasterZone(startX, startY, endX, endY, height, pageNumber);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34851");
            }
        }

        /// <summary>
        /// Creates a bounding rectangle for the specified collection of zones in a coordinate
        /// system aligned with the zones' average angle.
        /// </summary>
        /// <param name="rasterZones">The <see cref="RasterZone"/>s to be contained in the bounds.
        /// </param>
        /// <param name="averageRotation">The average angle of the <see paramref="rasterZones"/>
        /// and the angle of the coordinate system of the returned rectangle.</param>
        /// <returns>The bounding <see cref="RectangleF"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public static RectangleF GetAngledBoundingRectangle(IList<RasterZone> rasterZones,
            out double averageRotation)
        {
            try
            {
                // Keep track of the average height of all raster zones as well as all start and end
                // points.
                double firstRasterZoneRotation = 0;
                averageRotation = 0;
                Point[] rasterZonePoints = new Point[rasterZones.Count * 4];

                // Compile the raster zone points as well as the height and rotation from all zones.
                for (int i = 0; i < rasterZones.Count; i++)
                {
                    PointF[] currentZoneVertices = rasterZones[i].GetBoundaryPoints();
                    for (int j = 0; j < 4; j++)
                    {
                        rasterZonePoints[(i * 4) + j] =
                            new Point((int)Math.Round(currentZoneVertices[j].X),
                                (int)Math.Round(currentZoneVertices[j].Y));
                    }

                    double rasterZoneRotation = rasterZones[i].ComputeSkew(false);

                    // [DataEntry:352]
                    // Ensure the angle of each zone is relative to the initial raster zone rotation so
                    // there aren't overflow issues when computing the average (eg. -179 vs 180).
                    if (i == 0)
                    {
                        firstRasterZoneRotation = rasterZoneRotation;
                    }
                    else
                    {
                        // [DataEntry:361]
                        // There seem to be cases where some raster zones are backwards. Round the
                        // angle delta to the nearest PI radians.
                        rasterZoneRotation = firstRasterZoneRotation + GeometryMethods.GetAngleDelta(
                            firstRasterZoneRotation, rasterZoneRotation, Math.PI);
                    }
                    averageRotation += rasterZoneRotation;
                }

                // Convert to degrees
                averageRotation = GeometryMethods.ConvertRadiansToDegrees(averageRotation);

                // Obtain the average rotation of the zones.
                averageRotation /= rasterZones.Count;

                // Rotate the raster zone points into a coordinate system relative to the raster zones'
                // rotation.
                using (Matrix transform = new Matrix())
                {
                    transform.Rotate((float)-averageRotation);
                    transform.TransformPoints(rasterZonePoints);
                }

                // Obtain a bounding rectangle for the raster zone points.
                return GeometryMethods.GetBoundingRectangle(rasterZonePoints);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31362", ex);
            }
        }

        /// <summary>
        /// Will expand the raster zone by the specified length and height.
        /// </summary>
        /// <param name="length">The length to expand. Must be zero or greater.</param>
        /// <param name="height">The height to expand. Must be zero or greater.</param>
        /// <exception cref="ExtractException">If <paramref name="length"/> or
        /// <paramref name="height"/> are less than zero.</exception>
        public void ExpandRasterZone(int length, int height)
        {
            try
            {
                ExtractException.Assert("ELI23243", "Height and length must be positive",
                    length >= 0 && height >= 0, "Length", length, "Height", height);

                _height += height;

                // Only perform length computation if the value is > 0
                if (length == 0)
                {
                    return;
                }

                int halfLength = length / 2;

                // Check if the line is vertical
                float deltaX = _start.X - _end.X;
                if (deltaX == 0)
                {
                    // Check which Y point is greater and expand
                    if (_start.Y > _end.Y)
                    {
                        _start.Y += halfLength;
                        _start.Y += length % 2;

                        _end.Y -= halfLength;
                        if (_end.Y < 0)
                        {
                            _end.Y = 0;
                        }
                    }
                    else
                    {
                        _end.Y += halfLength;
                        _end.Y += length % 2;

                        _start.Y -= halfLength;
                        if (_start.Y < 0)
                        {
                            _start.Y = 0;
                        }
                    }
                }
                else
                {
                    // Compute the slope of the line
                    double slope = (_start.Y - _end.Y) / ((double)deltaX);

                    // Check which X point is greater and expand
                    if (_start.X > _end.X)
                    {
                        // Compute the new start value
                        _start.X += halfLength;
                        _start.X += length % 2;
                        _start.Y = (int)
                            (((slope * (_start.X - _end.X)) + _end.Y) + 0.5);

                        // Compute the new end value
                        _end.X -= halfLength;
                        if (_end.X < 0)
                        {
                            _end.X = 0;
                        }
                        _end.Y = (int)
                            (((-1.0 * slope * (_start.X - _end.X)) + _start.Y) + 0.5);
                    }
                    else
                    {
                        // Compute the new start value
                        _end.X += halfLength;
                        _end.X += length % 2;
                        _end.Y = (int)
                            (((-1.0 * slope * (_start.X - _end.X)) + _start.Y) + 0.5);

                        // Compute the new end value
                        _start.X -= halfLength;
                        if (_start.X < 0)
                        {
                            _start.X = 0;
                        }
                        _start.Y = (int)
                            (((slope * (_start.X - _end.X)) + _end.Y) + 0.5);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23244", ex);
            }
        }

        /// <summary>
        /// Computes the skew of this <see cref="RasterZone"/> in either degrees
        /// or radians. If <paramref name="degrees"/> is <see langword="true"/>
        /// then the returned skew will be in degrees, if it is <see langword="false"/>
        /// then the returned skew will be in radians.
        /// </summary>
        /// <param name="degrees">Whether results should be returned in degrees or radians.</param>
        /// <returns>The skew of the <see cref="RasterZone"/> in degrees or radians.</returns>
        public double ComputeSkew(bool degrees)
        {
            try
            {
                double skew = GeometryMethods.GetAngle(_start, _end);

                if (degrees)
                {
                    skew = GeometryMethods.ConvertRadiansToDegrees(skew);
                }

                return skew;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30204", ex);
            }
        }

        /// <summary>
        /// Returns the <see cref="RasterZone"/> as a comma separated string of values.
        /// </summary>
        /// <returns>The <see cref="RasterZone"/> as a comma separated string of values.</returns>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("StartPoint=");
                sb.Append(_start.ToString());
                sb.Append("; EndPoint=");
                sb.Append(_end.ToString());
                sb.Append("; Height=");
                sb.Append(_height);
                sb.Append("; Page=");
                sb.Append(_pageNumber);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30203", ex);
            }
        }

        /// <summary>
        /// Writes the zone data as XML.
        /// </summary>
        /// <param name="writer">The stream to which the zone should be written.</param>
        public void WriteXml(XmlWriter writer)
        {
            try
            {
                writer.WriteStartElement("Zone");

                // Only write zone information if the zone is not empty
                if (this != new RasterZone())
                {
                    // Write the same coordinates that would be persisted via ToComRasterZone (ie,
                    // rounded using RoundingMode.Safe).
                    Point start;
                    Point end;
                    int height;
                    GetRoundedCoordinates(RoundingMode.Safe, out start, out end, out height);

                    writer.WriteStartElement("Start");
                    writer.WriteAttributeString("X", start.X.ToString(CultureInfo.CurrentCulture));
                    writer.WriteAttributeString("Y", start.Y.ToString(CultureInfo.CurrentCulture));
                    writer.WriteEndElement();
                    writer.WriteStartElement("End");
                    writer.WriteAttributeString("X", end.X.ToString(CultureInfo.CurrentCulture));
                    writer.WriteAttributeString("Y", end.Y.ToString(CultureInfo.CurrentCulture));
                    writer.WriteEndElement();
                    writer.WriteElementString("Height", height.ToString(CultureInfo.CurrentCulture));
                    writer.WriteElementString("Page", _pageNumber.ToString(CultureInfo.CurrentCulture));
                }

                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30206", ex);
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the start point of the <see cref="RasterZone"/>.
        /// </summary>
        /// <return>Start point of the <see cref="RasterZone"/>.</return>
        public PointF Start
        {
            get
            {
                return _start;
            }
        }

        /// <summary>
        /// Gets or sets the X-coordinate for the start point of the <see cref="RasterZone"/>.
        /// </summary>
        /// <return>The X-coordinate for the start point of the <see cref="RasterZone"/>.</return>
        /// <value>The X-coordinate for the start point of the <see cref="RasterZone"/>.</value>
        public float StartX
        {
            get
            {
                return _start.X;
            }
            set
            {
                _start.X = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y-coordinate for the start point of the <see cref="RasterZone"/>.
        /// </summary>
        /// <return>The Y-coordinate for the start point of the <see cref="RasterZone"/>.</return>
        /// <value>The Y-coordinate for the start point of the <see cref="RasterZone"/>.</value>
        public float StartY
        {
            get
            {
                return _start.Y;
            }
            set
            {
                _start.Y = value;
            }
        }

        /// <summary>
        /// Gets the end point of the <see cref="RasterZone"/>.
        /// </summary>
        /// <return>End point of the <see cref="RasterZone"/>.</return>
        public PointF End
        {
            get
            {
                return _end;
            }
        }

        /// <summary>
        /// Gets or sets the X-coordinate for the end point of the <see cref="RasterZone"/>.
        /// </summary>
        /// <return>The X-coordinate for the end point of the <see cref="RasterZone"/>.</return>
        /// <value>The X-coordinate for the end point of the <see cref="RasterZone"/>.</value>
        public float EndX
        {
            get
            {
                return _end.X;
            }
            set
            {
                _end.X = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y-coordinate for the end point of the <see cref="RasterZone"/>.
        /// </summary>
        /// <return>The Y-coordinate for the end point of the <see cref="RasterZone"/>.</return>
        /// <value>The Y-coordinate for the end point of the <see cref="RasterZone"/>.</value>
        public float EndY
        {
            get
            {
                return _end.Y;
            }
            set
            {
                _end.Y = value;
            }
        }

        /// <summary>
        /// Gets or sets the height for the <see cref="RasterZone"/>.
        /// </summary>
        /// <return>The height of the <see cref="RasterZone"/>.</return>
        /// <value>The height of the <see cref="RasterZone"/>.</value>
        public float Height
        {
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
        /// Gets or sets the page number for the <see cref="RasterZone"/>
        /// </summary>
        /// <return>The page number of the <see cref="RasterZone"/>.</return>
        /// <value>The page number of the <see cref="RasterZone"/>.</value>
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
            set
            {
                _pageNumber = value;
            }
        }

        #endregion Properties

        #region IComparable<RasterZone> Members

        /// <summary>
        /// Compares this <see cref="RasterZone"/> with another <see cref="RasterZone"/>.
        /// </summary>
        /// <param name="other">A <see cref="RasterZone"/> to compare with this
        /// <see cref="RasterZone"/>.</param>
        /// <returns>An <see cref="int"/> that indicates the relative order of the
        /// <see cref="RasterZone"/> objects that are being compared.</returns>
        public int CompareTo(RasterZone other)
        {
            try
            {
                // Check the page number first
                int returnVal = PageNumber.CompareTo(other.PageNumber);

                // 0 indicates same page, keep comparing
                if (returnVal == 0)
                {
                    // Get the bounds of both raster zones
                    Rectangle myBounds = GetRectangularBounds();
                    Rectangle yourBounds = other.GetRectangularBounds();

                    // If the raster zones horizontally overlap the leftmost one comes first,
                    // otherwise the topmost one comes first
                    if (yourBounds.Bottom > myBounds.Top && myBounds.Bottom > yourBounds.Top)
                    {
                        // Check the left point
                        returnVal = myBounds.Left.CompareTo(yourBounds.Left);

                        // 0 indicates the X values are the same
                        if (returnVal == 0)
                        {
                            // Check the top point
                            returnVal = myBounds.Top.CompareTo(yourBounds.Top);
                        }
                    }
                    else
                    {
                        // Check the top point
                        returnVal = myBounds.Top.CompareTo(yourBounds.Top);

                        // 0 indicates the Y values are the same
                        if (returnVal == 0)
                        {
                            // Check the left point
                            returnVal = myBounds.Left.CompareTo(yourBounds.Left);
                        }
                    }
                }

                // Return the compared value
                return returnVal;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26495", ex);
            }
        }

        /// <summary>
        /// Checks whether the specified <see cref="object"/> is equal to
        /// this <see cref="RasterZone"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with.</param>
        /// <returns><see langword="true"/> if the objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // Check if it is a RasterZone object
            RasterZone rasterZone = obj as RasterZone;
            if (rasterZone == null)
            {
                return false;
            }

            // Check if they are equal
            return this == rasterZone;
        }

        /// <summary>
        /// Checks whether the specified <see cref="RasterZone"/> is equal to
        /// this <see cref="RasterZone"/>.
        /// </summary>
        /// <param name="rasterZone">The <see cref="RasterZone"/> to compare with.</param>
        /// <returns><see langword="true"/> if the zones are equal and
        /// <see langword="false"/> otherwise.</returns>
        public bool Equals(RasterZone rasterZone)
        {
            return this == rasterZone;
        }

        /// <summary>
        /// Returns a hashcode for this <see cref="RasterZone"/>.
        /// </summary>
        /// <returns>The hashcode for this <see cref="RasterZone"/>.</returns>
        public override int GetHashCode()
        {
            return (_start.GetHashCode() ^ _end.GetHashCode() ^ (int)_height ^ _pageNumber);
        }

        /// <summary>
        /// Checks whether the two specified <see cref="RasterZone"/> objects
        /// are equal.
        /// </summary>
        /// <param name="zone1">A <see cref="RasterZone"/> to compare.</param>
        /// <param name="zone2">A <see cref="RasterZone"/> to compare.</param>
        /// <returns><see langword="true"/> if the zones are equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator ==(RasterZone zone1, RasterZone zone2)
        {
            if (ReferenceEquals(zone1, zone2))
            {
                return true;
            }

            if (((object)zone1 == null) || ((object)zone2 == null))
            {
                return false;
            }

            return (
                zone1._pageNumber == zone2._pageNumber &&
                zone1._start == zone2._start &&
                zone1._end == zone2._end &&
                zone1._height == zone2._height
            );
        }

        /// <summary>
        /// Checks whether the two specified <see cref="RasterZone"/> objects
        /// are not equal.
        /// </summary>
        /// <param name="zone1">A <see cref="RasterZone"/> to compare.</param>
        /// <param name="zone2">A <see cref="RasterZone"/> to compare.</param>
        /// <returns><see langword="true"/> if the zones are not equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator !=(RasterZone zone1, RasterZone zone2)
        {
            return !(zone1 == zone2);
        }

        /// <summary>
        /// Checks whether the first specified <see cref="RasterZone"/>
        /// is less than the second specified <see cref="RasterZone"/>.
        /// </summary>
        /// <param name="zone1">A <see cref="RasterZone"/> to compare.</param>
        /// <param name="zone2">A <see cref="RasterZone"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="zone1"/> is less
        /// than <paramref name="zone2"/> and <see langword="false"/> otherwise.</returns>
        public static bool operator <(RasterZone zone1, RasterZone zone2)
        {
            return zone1.CompareTo(zone2) < 0;
        }

        /// <summary>
        /// Checks whether the first specified <see cref="RasterZone"/>
        /// is greater than the second specified <see cref="RasterZone"/>.
        /// </summary>
        /// <param name="zone1">A <see cref="RasterZone"/> to compare.</param>
        /// <param name="zone2">A <see cref="RasterZone"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="zone1"/> is greater
        /// than <paramref name="zone2"/> and <see langword="false"/> otherwise.</returns>
        public static bool operator >(RasterZone zone1, RasterZone zone2)
        {
            return zone1.CompareTo(zone2) > 0;
        }
        #endregion
    }
}