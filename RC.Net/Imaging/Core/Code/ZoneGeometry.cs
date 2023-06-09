using Extract.Drawing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents the side of a <see cref="ZoneGeometry"/> zone to be operated upon.
    /// </summary>
    public enum Side
    {
        /// <summary>
        /// The left side.
        /// </summary>
        Left = 0,

        /// <summary>
        /// The top side.
        /// </summary>
        Top = 1,

        /// <summary>
        /// The right side.
        /// </summary>
        Right = 2,

        /// <summary>
        /// The bottom side.
        /// </summary>
        Bottom = 3
    };

    /// <summary>
    /// Represents the rounding method to use when retrieving zone coordinates.
    /// </summary>
    public enum RoundingMode
    {

        /// <summary>
        /// Do not round. Coordinates may represent fractions of a pixel.
        /// </summary>
        None = 0,

        /// <summary>
        /// Round to the nearest whole pixel.
        /// </summary>
        Simple = 1,

        /// <summary>
        /// Round to the next whole pixel in the opposite direction as the raster zone center to
        /// ensure the zone doesn't shrink and expose or "leak" pixels as a result of rounding.
        /// </summary>
        Safe = 2
    }

    /// <summary>
    /// Allows for resizing and fitting of <see cref="RasterZone"/>s.
    /// </summary>
    public class ZoneGeometry : ICloneable
    {
        #region Constants

        /// <summary>
        /// 
        /// </summary>
        const float _EQUIVALENCE_TOLERANCE = 0.001F;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The page the zone is on.
        /// </summary>
        int _pageNumber;

        /// <summary>
        /// The angle of the zone relative to horizontal (in radians).
        /// </summary>
        double _angle;

        /// <summary>
        /// Points representing the 4 corners of the zone.
        /// </summary>
        PointF[] _vertices;

        /// <summary>
        /// The width of the zone.
        /// </summary>
        float _width;

        /// <summary>
        /// The height of the zone.
        /// </summary>
        float _height;

        /// <summary>
        /// A <see cref="CancellationToken"/> to allow a fitting operation to be canceled.
        /// </summary>
        CancellationToken? _cancelToken;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneGeometry"/> class.
        /// </summary>
        /// <param name="zone">The <see cref="RasterZone"/> the instances is to be based on.</param>
        public ZoneGeometry(RasterZone zone)
            : this(zone, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneGeometry"/> class.
        /// </summary>
        /// <param name="zone">The <see cref="RasterZone"/> the instances is to be based on.</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> to allow a fitting operation
        /// to be canceled.</param>
        public ZoneGeometry(RasterZone zone, CancellationToken? cancelToken)
        {
            try
            {
                _cancelToken = cancelToken;
                _pageNumber = zone.PageNumber;

                // [LegacyRCAndUtils #5205]
                // Calculate the angle from horizontal as a number between -180 and 180
                _angle = GeometryMethods.GetAngleDelta(0, zone.ComputeSkew(false), false);

                _vertices = zone.GetBoundaryPoints();

                // To initialize the width and height, retrieve a rectangle in the zone's coordinate
                // system.
                PointF theta;
                RectangleF rectangle = GetWorkingRectangle(Side.Left, out theta);
                _width = rectangle.Width;
                _height = rectangle.Height;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31326", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>The height.</value>
        public float Height
        {
            get
            {
                return _height;
            }
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>The width.</value>
        public float Width
        {
            get
            {
                return _width;
            }
        }

        /// <summary>
        /// Gets or sets the padding to use when auto-fitting a zone to pixel content.
        /// </summary>
        /// <value>The amount of padding (in pixels).</value>
        public static int AutoFitZonePadding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum <see cref="Size"/> a zone should be be following any sizing or
        /// fitting operation.
        /// </summary>
        /// <value>
        /// The minimum <see cref="Size"/> a zone should be be following any sizing or fitting
        /// operation.
        /// </value>
        public static Size MinSize
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Fits the specified side of the region to pixel content in the image if possible.
        /// </summary>
        /// <param name="side">The <see cref="Side"/> of the region to be fitted.</param>
        /// <param name="probe">The <see cref="PixelProbe"/> to check pixel data.</param>
        /// <param name="shrink"><see langword="true"/> to attempt to adjust the side by making the
        /// zone smaller; <see langword="false"/> to attempt to expand the zone.</param>
        /// <param name="findBlack"><see langword="true"/> to look for the next row containing black
        /// pixels; <see langword="false"/> to look for the next row without any black pixels.
        /// </param>
        /// <param name="fuzzyFactor">The factor by which pixel content or the distance from the
        /// first to the last pixel in a row must decline, then increase in order for the row of the
        /// minimum value to qualify as the edge of pixel content. If <see langword="null"/>, no
        /// fuzzy logic will be used to find an edge.</param>
        /// <param name="fuzzyBuffer">After qualifying a "fuzzy" edge of pixel content, the distance
        /// to keep searching for a true edge before using the fuzzy edge.</param>
        /// <param name="buffer">If a qualifying row is found, how many rows prior to the qualifying
        /// row to move the side. This will never result in the side moving in the opposite
        /// direction specified by <see paramref="side"/>.</param>
        /// <param name="min">The minimum number of rows to move the side.</param>
        /// <param name="max">The maximum number of rows to move the side. Specify 0 if there is no
        /// maximum value other than the other side of the zone when <see paramref="shrink"/> is
        /// <see langword="true"/>.</param>
        public bool FitEdge(Side side, PixelProbe probe, bool shrink = true, bool findBlack = true,
            float? fuzzyFactor = null, int fuzzyBuffer = 0, float? buffer = null, float min = 0F,
            float max = 0F)
        {
            try
            {
                // The buffer is the distance from the first row of pixels, not the number of rows
                // of pixels in between (as with AutoFitZonePadding).
                float bufferValue = buffer ?? AutoFitZonePadding + 1;

                // Search for the edge of pixel content
                float? edge = FindEdge(side, probe, shrink, findBlack, fuzzyFactor, fuzzyBuffer,
                    bufferValue, min, max);

                // If an edge was found, adjust the side of the zone accordingly.
                if (edge != null)
                {
                    if (edge.Value != 0F)
                    {
                        InflateSide(side, shrink ? -edge.Value : edge.Value);
                    }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31327", ex);
            }
        }

        /// <summary>
        /// Finds the edge of pixel content specified in pixels from the specified
        /// <see cref="Side"/> of the zone.
        /// </summary>
        /// <param name="side">The <see cref="Side"/> of the region from which to search.</param>
        /// <param name="probe">The <see cref="PixelProbe"/> to check pixel data.</param>
        /// <param name="shrink"><see langword="true"/> to search toward the inside of the specified
        /// <see paramref="side"/>; <see langword="false"/> to search toward the outside.</param>
        /// <param name="findBlack"><see langword="true"/> to look for the next row containing black
        /// pixels; <see langword="false"/> to look for the next row without any black pixels.
        /// </param>
        /// <param name="fuzzyFactor">The factor by which pixel content or the distance from the
        /// first to the last pixel in a row must decline, then increase in order for the row of the
        /// minimum value to qualify as the edge of pixel content. If <see langword="null"/>, no
        /// fuzzy logic will be used to find an edge.</param>
        /// <param name="fuzzyBuffer">After qualifying a "fuzzy" edge of pixel content, the distance
        /// to keep searching for a true edge before using the fuzzy edge.</param>
        /// <param name="buffer">If a qualifying row is found, how many rows prior to the qualifying
        /// row return. This will never result in a negative number for a found edge.
        /// </param>
        /// <param name="min">The minimum number of rows to search.</param>
        /// <param name="max">The maximum number of rows to search. Specify 0 if there is no
        /// maximum value other than the other side of the zone when <see paramref="shrink"/> is
        /// <see langword="true"/>.</param>
        /// <returns>The number of pixels from the side to a qualifying edge of pixel content.
        /// -1 if no edge was found.</returns>
        public float? FindEdge(Side side, PixelProbe probe, bool shrink, bool findBlack,
            float? fuzzyFactor, int fuzzyBuffer, float buffer, float min, float max)
        {
            try
            {
                // Retrieve a working rectangle relative to the zone's coordinate system and
                // with the side to be operated upon as the left side.
                PointF theta;
                RectangleF rectangle = GetWorkingRectangle(side, out theta);

                // If shrinking and a max row was not sepecified or the specified max is wider
                // than the working rectangle, adjust accordingly.
                if (shrink && (max <= 0 || max > (int)rectangle.Width))
                {
                    max = (int)rectangle.Width;
                }

                // Round pixels in a way that ensures zone bounds encompass all content they are
                // intended to.
                bool roundPixelUp = findBlack;

                FuzzyEdgeQualifier countQualifier = null;
                FuzzyEdgeQualifier spreadQualifier = null;
                bool useFuzzyFactor = fuzzyFactor.HasValue;

                // If an edge is allowed to be found using "fuzzy" logic, initialize
                // FuzzyEdgeQualifier based on the base raster zone line pixel content.
                if (useFuzzyFactor)
                {
                    // Fuzzy logic doesn't make sense when trying to find black pixel content.
                    ExtractException.Assert("ELI31349", "Internal image searching error",
                        !findBlack);

                    bool offPage;
                    int initialSpread;
                    int initialCount = CheckRowPixels(rectangle, 0, theta, probe,
                        roundPixelUp, false, out offPage, out initialSpread);

                    countQualifier = new FuzzyEdgeQualifier(initialCount, fuzzyFactor.Value);
                    spreadQualifier = new FuzzyEdgeQualifier(initialSpread, fuzzyFactor.Value);
                }

                // If an edge is found, this value will be set.
                float? edge = null;

                // Loop through the range of specified rows looking for one that qualifies as
                // the edge of pixel content.
                for (float row = shrink ? min : -min;
                        max == 0 || Math.Abs(row) <= (float)max;
                        row += shrink ? 1 : -1)
                {
                    if (_cancelToken != null)
                    {
                        _cancelToken.Value.ThrowIfCancellationRequested();
                    }

                    // Scan the current row's pixels.
                    bool offPage;
                    int spread;
                    int count = CheckRowPixels(rectangle, row, theta, probe, roundPixelUp,
                        !useFuzzyFactor, out offPage, out spread);

                    if (count > 0 == findBlack)
                    {
                        // We found a true edge of pixel content.
                        edge = row;
                        break;
                    }
                    else if (useFuzzyFactor)
                    {
                        // If we haven't yet found the edge of pixel content, see if we have
                        // found a row that qualifies as an edge using fuzzy logice.
                        edge = countQualifier.GetQualifyingRow(row, count, fuzzyBuffer);
                        if (edge.HasValue)
                        {
                            break;
                        }

                        edge = spreadQualifier.GetQualifyingRow(row, spread, fuzzyBuffer);
                        if (edge.HasValue)
                        {
                            break;
                        }
                    }

                    // If none of the pixels in the row were onpage, stop the search.
                    if (offPage)
                    {
                        break;
                    }
                }

                // If no row was found, if using fuzzy logic ask for the best qualified row
                // (if such a row exists).
                if (!edge.HasValue && useFuzzyFactor)
                {
                    edge = countQualifier.GetCandidateRow();
                    if (!edge.HasValue)
                    {
                        edge = spreadQualifier.GetCandidateRow();
                    }
                }

                // Return the number of rows from the base, minus the buffer.
                if (edge.HasValue)
                {
                    return Math.Max(Math.Abs(edge.Value) - buffer, min);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31328", ex);
            }
        }

        /// <summary>
        /// Inflates the specified <see paramraf="side"/> the specified distance.
        /// </summary>
        /// <param name="side">The <see cref="Side"/> to move.</param>
        /// <param name="distance">The distance to move the side (negative to shrink).</param>
        public void InflateSide(Side side, float distance)
        {
            try
            {
                // Retrieve a working rectangle relative to the zone's coordinate system and
                // with the side to be operated upon as the left side.
                PointF theta;
                RectangleF rectangle = GetWorkingRectangle(side, out theta);

                int minSize = (side == Side.Left || side == Side.Right)
                    ? MinSize.Width
                    : MinSize.Height;

                // Ensure we don't make the zone smaller than Highlight.MinSize.
                if (rectangle.Width + distance < minSize)
                {
                    distance = -rectangle.Width + minSize;
                }

                // Adjust the rectangle by the specified distance.
                rectangle.Location =
                    new PointF(rectangle.Location.X - distance, rectangle.Location.Y);
                rectangle.Width += distance;

                // Set the new vertices.
                _vertices[0] = rectangle.Location;
                _vertices[1] = new PointF(rectangle.Right, rectangle.Top);
                _vertices[2] = new PointF(rectangle.Right, rectangle.Bottom);
                _vertices[3] = new PointF(rectangle.Left, rectangle.Bottom);

                // Rotate the vertices into the image coordinate system.
                for (int i = 0; i < 4; i++)
                {
                    _vertices[i] = GeometryMethods.Rotate(_vertices[i], theta);
                }

                // Update the width/height field as appropriate.
                if (side == Side.Left || side == Side.Right)
                {
                    _width += distance;
                }
                else
                {
                    _height += distance;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32821");
            }
        }

        /// <summary>
        /// Converts this instance to a <see cref="RasterZone"/>.
        /// </summary>
        /// <param name="roundingMode">The <see cref="RoundingMode"/> to use when generating the
        /// <see cref="RasterZone"/>.</param>
        /// <returns>The <see cref="RasterZone"/> equivalent of this instance.</returns>
        public RasterZone ToRasterZone(RoundingMode roundingMode)
        {
            try
            {
                PointF start;
                PointF end;
                float height;
                GetZoneCoordinates(roundingMode, out start, out end, out height);

                // Create the raster zone
                return new RasterZone(start, end, height, _pageNumber);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32824");
            }
        }

        /// <summary>
        /// Gets the zone coordinates.
        /// </summary>
        /// <param name="roundingMode">The <see cref="RoundingMode"/> to use when retrieving the
        /// coordinates.</param>
        /// <param name="start">The zone's start point.</param>
        /// <param name="end">The zone's end point.</param>
        /// <param name="height">The zone's height.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        public void GetZoneCoordinates(RoundingMode roundingMode, out PointF start, out PointF end,
            out float height)
        {
            try
            {
                // Retrieve a working rectangle relative to the zone's coordinate system and
                // with the side to be operated upon as the left side.
                PointF theta;
                RectangleF rectangle = GetWorkingRectangle(Side.Left, out theta);

                // Compute the start and end points.            
                float midPoint = rectangle.Location.Y + (_height / 2);

                start = new PointF(rectangle.Left, midPoint);
                end = new PointF(rectangle.Right, midPoint);

                if (roundingMode == RoundingMode.Safe)
                {
                    // For the "safe" rounding mode move the start and end points half a pixel away
                    // from the center, to ensure they are rounded to the next whole pixel away from
                    // the raster zone. (Add slightly less than half a pixel to prevent modifying
                    // coordinates if the starting point was an integer).
                    start.X -= 0.499F;
                    end.X += 0.499F;
                }

                start = GeometryMethods.Rotate(start, theta);
                end = GeometryMethods.Rotate(end, theta);

                if (roundingMode != RoundingMode.None)
                {
                    start.X = (float)Math.Round(start.X);
                    start.Y = (float)Math.Round(start.Y);
                    end.X = (float)Math.Round(end.X);
                    end.Y = (float)Math.Round(end.Y);

                    if (roundingMode == RoundingMode.Simple)
                    {
                        // Ensure height is even by rounding to the nearest even number. If the
                        // height is odd, the top and bottom of the zone will be a .5 value. When
                        // such a zone is displayed, those values will be rounded off, potentially
                        // exposing pixel content. 
                        height = (int)Math.Round(_height / 2, MidpointRounding.AwayFromZero) * 2;
                    }
                    else
                    {
                        // Because the rounded coordinates are likely to be at a slightly different
                        // angle than _angle, toward either end of the raster zone this allows for
                        // the possibility that pixels will be leaked above or below the raster zone.
                        // Before rounding up to the next even integer for height, add in the the
                        // vertical distance the ends of the raster zone have been moved by rounding
                        // of the start and end point.
                        double angle = GeometryMethods.GetAngle(start, end);
                        double angleOffset = GeometryMethods.GetAngleDelta(_angle, angle, false);
                        double verticalOffset = Math.Abs(Math.Sin(angleOffset) * _width);

                        // Ensure height is even. If the height is odd, the top and bottom of the zone will
                        // be a .5 value. When such a zone is displayed, those values will be rounded off,
                        // potentially exposing pixel content. Expand the zone by a pixel to prevent this.
                        height = (int)Math.Ceiling((_height + verticalOffset) / 2) * 2;
                    }
                }
                else
                {
                    height = _height;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32825");
            }
        }

        /// <summary>
        /// Rotates the orientation of the zone by the specified number of degrees.
        /// </summary>
        /// <param name="degrees">The degrees to rotate the orientation. Must be a multiple of 90.
        /// </param>
        public void RotateOrientation(double degrees)
        {
            try
            {
                ExtractException.Assert("ELI32819",
                        "Orientation can only be rotated by multiples of 90 degrees",
                        ((int)degrees % 90) == 0);

                double radians = GeometryMethods.ConvertDegreesToRadians(degrees);
                _angle = GeometryMethods.GetAngleDelta(0, _angle + radians, false);

                if (((int)degrees % 180) != 0)
                {
                    UtilityMethods.Swap(ref _height, ref _width);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32822");
            }
        }

        /// <summary>
        /// Updates the vertices based on the specified <see paramref="zone"/>
        /// </summary>
        /// <param name="zone">The <see cref="RasterZone"/> whose coordinates should be used to
        /// update the vertices.</param>
        public void UpdateVertices(RasterZone zone)
        {
            try
            {
                _vertices = zone.GetBoundaryPoints();

                PointF theta;
                RectangleF rectangle = GetWorkingRectangle(Side.Left, out theta);
                _width = rectangle.Width;
                _height = rectangle.Height;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32823");
            }
        }

        /// <summary>
        /// Determines if the specified points define a line along a similar angle to the zone
        /// defined by this <see cref="ZoneGeometry"/> instance that passes across the left side of
        /// this zone and whose plane passes between both pairs of vertices (whether or not the end
        /// point is past the zone's right side).
        /// </summary>
        /// <param name="startPoint">The starting <see cref="PointF"/> of the line to test.</param>
        /// <param name="endPoint">The ending <see cref="PointF"/> of the line to test.</param>
        /// <returns><see langword="true"/> if the line passes through the zone defined by this
        /// <see cref="ZoneGeometry"/> instance, <see langword="false"/> otherwise.</returns>
        public bool LinePassesThrough(PointF startPoint, PointF endPoint)
        {
            try
            {
                // Create a theta value to translate the provied points into the this zone's
                // coordinate system.
                PointF workingTheta =
                    new PointF((float)Math.Sin(-_angle), (float)Math.Cos(-_angle));

                // Convert the points into this zone's coordinate system.
                startPoint = GeometryMethods.Rotate(startPoint, workingTheta);
                endPoint = GeometryMethods.Rotate(endPoint, workingTheta);

                // Obtain a working rectangle to compare to the provided points.
                PointF thisTheta;
                RectangleF workingRectangle = GetWorkingRectangle(Side.Left, out thisTheta);

                // If the points are on opposite sides of this zone's left side, test whether it
                // passes through both pairs of vertices.
                if (startPoint.X < workingRectangle.Left && endPoint.X > workingRectangle.Left)
                {
                    // Compute the angle of the provided points.
                    double parameterAngle = GeometryMethods.GetAngle(startPoint, endPoint);
                    GeometryMethods.GetAngleDelta(0, parameterAngle, false);

                    // Compute the difference in angle between the provided line and this zone.
                    double diffAngle = parameterAngle - _angle;

                    // Rotate the endPoint into its own coordinate system.
                    workingTheta =
                        new PointF((float)Math.Sin(-diffAngle), (float)Math.Cos(-diffAngle));
                    endPoint = GeometryMethods.Rotate(endPoint, workingTheta);

                    // Move the endpoint so that it is aligned with the right side of this zone.
                    endPoint.X = workingRectangle.Right;

                    // Move the endpoint back into this raster zones coordinate system to determine
                    // the spot in which the provided plane intersects the right side of this zone.
                    workingTheta =
                        new PointF((float)Math.Sin(diffAngle), (float)Math.Cos(diffAngle));
                    endPoint = GeometryMethods.Rotate(endPoint, workingTheta);

                    // If the point of intersection is below the top of this zone, but above the
                    // bottom consider that it passes through this zone.
                    if (endPoint.Y > workingRectangle.Top && endPoint.Y < workingRectangle.Bottom)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31462", ex);
            }
        }

        /// <summary>
        /// Creates a <see cref="Bitmap"/> containing the pixel data of this zone
        /// </summary>
        /// <param name="probe">The <see cref="PixelProbe"/> to be used to get the pixel data</param>
        /// <returns>An RGB 24bpp <see cref="Bitmap"/> (formatted with <see cref="PixelFormat.Format24bppRgb"/>)</returns>
        public Bitmap GetZoneAsBitmap(PixelProbe probe)
        {
            try
            {
                // Retrieve a working rectangle relative to the zone's coordinate system
                RectangleF rectangle = GetWorkingRectangle(Side.Left, out PointF theta);

                // Create a bitmap object to be filled
                int width = Convert.ToInt32(rectangle.Width);
                int height = Convert.ToInt32(rectangle.Height);
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

                // Fill an array with the pixel data
                var rect = new Rectangle(0, 0, width, height);
                var bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                var numberOfBytes = bitmapData.Stride * height;
                var bitmapBytes = new byte[numberOfBytes];

                PointF pixel = new PointF(rectangle.Left, rectangle.Top);
                for (int y = 0; pixel.Y <= rectangle.Bottom && y < height; pixel.Y++, y++)
                {
                    pixel.X = rectangle.Left;
                    for (int x = 0; pixel.X <= rectangle.Right && x < width; pixel.X++, x++)
                    {
                        var rotatedPixel = GeometryMethods.Rotate(pixel, theta);
                        Point point = theta.X > 0 ? Point.Ceiling(rotatedPixel) : Point.Truncate(rotatedPixel);

                        var color = probe.GetPixelColor(point.X, point.Y);
                        var i = (y * width + x) * 3;
                        bitmapBytes[i] = color.R;
                        bitmapBytes[i + 1] = color.G;
                        bitmapBytes[i + 2] = color.B;
                    }
                }

                // Set the bitmap's data
                var ptr = bitmapData.Scan0;
                Marshal.Copy(bitmapBytes, 0, ptr, bitmapBytes.Length);
                bitmap.UnlockBits(bitmapData);

                return bitmap;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44668");
            }
        }

        #endregion Methods

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                // Most members can simply be copied by value.
                ZoneGeometry clone = (ZoneGeometry)MemberwiseClone();

                // Vertices array needs to be deep copied.
                clone._vertices = new PointF[4];
                _vertices.CopyTo(clone._vertices, 0);

                return clone;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31350", ex);
            }
        }

        #endregion ICloneable Members

        #region Private Members

        /// <summary>
        /// In order to be able to operate on any side of the zone with the same logic, this method
        /// creates a rectangle relative to the zone's coordinate system and with the side to be
        /// operated upon as the left side.
        /// </summary>
        /// <param name="side">The <see cref="Side"/> of the zone the work is to be related to.
        /// </param>
        /// <param name="theta">X and Y theta values used to rotate coordinates from the returned
        /// rectangle back into the image coordinate system.</param>
        /// <returns>The <see cref="RectangleF"/> representing the appropriately rotated zone.
        /// </returns>
        RectangleF GetWorkingRectangle(Side side, out PointF theta)
        {
            // Rotate the zone to horizontal
            double workingAngle = -_angle;

            // If necessary, rotate it further such that the side to be worked on becomes the left
            // side of the working zone.
            workingAngle -= (double)side * Math.PI / 2;

            // Create theta values to translate the image coordinates into the working coordinate
            // system.
            PointF workingTheta =
                new PointF((float)Math.Sin(workingAngle), (float)Math.Cos(workingAngle));

            // Calculate the vertices of this rectangle.
            PointF[] workingVertices = _vertices
                .Select(p => GeometryMethods.Rotate(p, workingTheta))
                .ToArray();

            // Reverse the transform so that it can be used to convert working coordinate to
            // image coordiates.
            theta = new PointF((float)Math.Sin(-workingAngle), (float)Math.Cos(-workingAngle));

            // Return the rectangle.
            return GeometryMethods.GetBoundingRectangle(workingVertices);
        }

        /// <summary>
        /// Scans a row of pixels to obtain the number of black pixels in the row as well as the
        /// distance between the first and last black pixel found.
        /// </summary>
        /// <param name="rectangle">A working rectangle with the side to be operated on as the left
        /// side.</param>
        /// <param name="offset">The x-coordinate offest of the row to scan from the left side of
        /// the working rectangle.</param>
        /// <param name="theta">X and Y theta values that will convert the working coordinates into
        /// image coordinates.</param>
        /// <param name="probe">A <see cref="PixelProbe"/> that allows access to the image pixels.
        /// </param>
        /// <param name="roundPixelUp"><see langword="true"/> to round pixel coordinates up before
        /// checking pixel color, <see langword="true"/> to round down.</param>
        /// <param name="allowShortCircuit"><see langword="true"/> if the caller is concerned only
        /// with whether there are any black pixels; in this case the method will return immediately
        /// after finding the first black pixel.</param>
        /// <param name="offPage"><see langword="true"/> if all of the row's pixels are off-page.
        /// <see langword="false"/> if at least one is on-page.</param>
        /// <param name="spread">The distance between the first and last black pixel found in the
        /// row.</param>
        /// <returns>The number of black pixels found in the row.</returns>
        static int CheckRowPixels(RectangleF rectangle, float offset, PointF theta,
            PixelProbe probe, bool roundPixelUp, bool allowShortCircuit, out bool offPage, out int spread)
        {
            int count = 0;
            offPage = true;
            spread = 0;

            int? firstPixel = null;
            int? lastPixel = null;

            // Iterate through each pixel in the row (parallel to the left side of the working
            // rectangle), from the top to the bottom.
            for (PointF pixel = new PointF(rectangle.Left + offset, rectangle.Top);
                 pixel.Y <= rectangle.Bottom;
                 pixel.Y += 1)
            {
                bool pixelOffPage;
                bool pixelIsBlack =
                    IsPixelBlack(pixel, theta, probe, roundPixelUp, out pixelOffPage);
                if (!pixelOffPage && offPage)
                {
                    // If the pixel is not offpage, the row is not entirely offpage.
                    offPage = false;
                }

                if (pixelIsBlack)
                {
                    if (allowShortCircuit)
                    {
                        return 1;
                    }

                    // Increment the count, keep track of the first and last black pixel.
                    count++;
                    if (!firstPixel.HasValue)
                    {
                        firstPixel = (int)pixel.Y;
                    }
                    lastPixel = (int)pixel.Y;
                }
            }

            spread = (count == 0) ? 0 : (lastPixel.Value - firstPixel.Value);

            return count;
        }

        /// <summary>
        /// Determines if the specified pixel is black.
        /// </summary>
        /// <param name="pixel">The pixel to test (in working coordinates).</param>
        /// <param name="theta">X and Y theta values that will convert the working coordinates into
        /// image coordinates.</param>
        /// <param name="probe">A <see cref="PixelProbe"/> that allows access to the image pixels.
        /// </param>
        /// <param name="roundPixelUp"><see langword="true"/> to round pixel coordinates up before
        /// checking pixel color, <see langword="true"/> to round down.</param>
        /// <param name="offPage"><see langword="true"/> if pixel is off-page.</param>
        /// <returns>
        /// <see langword="true"/> if the specified pixel is black; <see langword="false"/> if the
        /// specified pixel is white.
        /// </returns>
        static bool IsPixelBlack(PointF pixel, PointF theta, PixelProbe probe,
            bool roundPixelUp, out bool offPage)
        {
            // Convert the pixel coordinates into image coordinates.
            pixel = GeometryMethods.Rotate(pixel, theta);

            // [FlexIDSCore:4603]
            // Since we are not back in the image coordinate system, roundPixelUp will depend on the
            // value of theta.X.
            Point point = (roundPixelUp == theta.X > 0) ? Point.Ceiling(pixel) : Point.Truncate(pixel);

            offPage = !probe.Contains(point);

            return !offPage && probe.IsPixelBlack(point);
        }

        #endregion Private Members

        #region FuzzyEdgeQualifier

        /// <summary>
        /// When scanning rows of image pixels for the edge of content, this helper class allows
        /// an edge to be found using "fuzzy" logic. If a given stat for the rows pixels drop below
        /// a specified level relative to the other rows, it can be considered an edge of content.
        /// </summary>
        class FuzzyEdgeQualifier
        {
            /// <summary>
            /// The factor by which the data value being checked must decline, then increase in
            /// order for the row of the minimum value to qualify as the edge of pixel content.
            /// </summary>
            float _factor;

            /// <summary>
            /// The initial value of the data being tracked by this instance.
            /// </summary>
            public int _initialValue;

            /// <summary>
            /// Specifies any candidate row that has been found.
            /// </summary>
            public float? _candidateRow;

            /// <summary>
            /// Specifies any qualifying row that has been found.
            /// </summary>
            public float? _qualifiedRow;

            /// <summary>
            /// The value a row must be below to be a candidate.
            /// </summary>
            public int _minimaThreshold;

            /// <summary>
            /// The lowest value found for any row.
            /// </summary>
            public int _lowValue;

            /// <summary>
            /// The value a row following a candidate row must be above to be a qualifying row.
            /// </summary>
            public int? _maximaThreshold;

            /// <summary>
            /// Initializes a new instance of the <see cref="FuzzyEdgeQualifier"/> class.
            /// </summary>
            /// <param name="initialValue">The initial value.</param>
            /// <param name="factor">The factor by which the data value being checked must decline,
            /// then increase in order for the row of the minimum value to qualify as the edge of
            /// pixel content.</param>
            public FuzzyEdgeQualifier(int initialValue, float factor)
            {
                // Do not allow 0 as the initial value so that multiplication with the specified
                // factor is alwasy meaningful.
                _initialValue = (initialValue == 0) ? 1 : initialValue;
                _lowValue = _initialValue;
                _factor = factor;

                // Determine the value at which a row will become a candidate edge.
                float threshold = (float)_initialValue * factor;

                if (threshold < 1)
                {
                    // If the threshold value is less than 1, set it to zero to ensure there will be
                    // no qualifying row.
                    _minimaThreshold = 0;
                }
                else
                {
                    // Otherwise set to the next higher int so we can use the < comparison.
                    _minimaThreshold = (int)Math.Ceiling(threshold);
                }
            }

            /// <summary>
            /// Based on the specified row and value as well as previous calls to this method,
            /// determine if any row has qualified fuzzily as an edge of content.
            /// </summary>
            /// <param name="row">The index of the latest row data.</param>
            /// <param name="value">A value that corresponds to a measure of row pixel content.
            /// </param>
            /// <param name="buffer">After qualifying a "fuzzy" edge of pixel content, the distance
            /// to keep searching for a true edge before using the fuzzy edge.</param>
            /// <returns>A qualifying row index or <see langword="null"/> if no qualifying row has
            /// yet been found.</returns>
            public float? GetQualifyingRow(float row, int value, int buffer)
            {
                if (_qualifiedRow.HasValue)
                {
                    // If we have a qualified row, continue searching for a true edge for a bit more.
                    if (Math.Abs(row - _qualifiedRow.Value) > buffer)
                    {
                        return _qualifiedRow;
                    }
                }
                else if (value < _minimaThreshold)
                {
                    // For any row with a value below the threshold to be a candidate edge,
                    // make it a candiated edge (error toward a larger zone).
                    _candidateRow = row;

                    if (value < _lowValue)
                    {
                        // If this is a new low value, use it as the basis for a reduced maxima
                        // threshold.
                        _lowValue = value;
                        _maximaThreshold = (int)Math.Floor((float)value / _factor);
                    }
                }
                else if (_maximaThreshold.HasValue && value > _maximaThreshold.Value)
                {
                    // If the value has increased by the specified factor over the low point, the
                    // candidate row is not qualified.
                    _qualifiedRow = _candidateRow.Value;
                }

                return null;
            }

            /// <summary>
            /// Based on data from previous calls to <see cref="GetQualifyingRow"/> returns any
            /// candidate edge of content that has been found (even if it isn't fully qualified).
            /// </summary>
            /// <returns></returns>
            public float? GetCandidateRow()
            {
                return _candidateRow;
            }
        }

        #endregion FuzzyEdgeQualifier
    }
}