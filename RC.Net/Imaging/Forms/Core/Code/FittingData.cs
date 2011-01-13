using System.Drawing;
using System.Collections.Generic;
using System;
using Extract.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents the side of a <see cref="FittingData"/> zone to be operated upon.
    /// </summary>
    enum Side
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
        Bottom = 3,
    };

    /// <summary>
    /// Represents zone data and methods to act upon that data for the autofitting algorithm.
    /// </summary>
    class FittingData : ICloneable
    {
        #region Fields

        /// <summary>
        /// The page the zone is on.
        /// </summary>
        public int _pageNumber;

        /// <summary>
        /// The angle of the zone relative to horizontal.
        /// </summary>
        public double _angle;

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
        /// Initializes a new instance of the <see cref="FittingData"/> class.
        /// </summary>
        /// <param name="zone">The <see cref="RasterZone"/> the instances is to be based on.</param>
        public FittingData(RasterZone zone)
            : this(zone, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FittingData"/> class.
        /// </summary>
        /// <param name="zone">The <see cref="RasterZone"/> the instances is to be based on.</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> to allow a fitting operation
        /// to be canceled.</param>
        public FittingData(RasterZone zone, CancellationToken? cancelToken)
        {
            try
            {
                _cancelToken = cancelToken;
                _pageNumber = zone.PageNumber;

                // [LegacyRCAndUtils #5205]
                // Calculate the angle from horizontal as a number between -180 and 180
                _angle = GeometryMethods.GetAngleDelta(0, zone.ComputeSkew(true), true);

                _vertices = zone.GetBoundaryPoints();

                // To initialize the width and height, retrieve a rectangle in the zone's coordinate
                // system.
                using (Matrix transform = new Matrix())
                {
                    RectangleF rectangle = GetWorkingRectangle(Side.Left, transform);
                    _width = rectangle.Width;
                    _height = rectangle.Height;
                }
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
        /// fuzzy logic will be used to find an edge (default = <see langword="null"/>).</param>
        /// <param name="buffer">If a qualifying row is found, how many rows prior to the qualifying
        /// row to move the side. This will never result in the side moving in the opposite
        /// direction specified by <see paramref="side"/>. (default = 1)</param>
        /// <param name="min">The minimum number of rows to move the side. (default = 0)</param>
        /// <param name="max">The maximum number of rows to move the side. Specify 0 if there is no
        /// maximum value other than the other side of the zone when <see paramref="shrink"/> is
        /// <see langword="true"/>. (default = 0)</param>
        public bool FitEdge(Side side, PixelProbe probe, bool shrink = true, bool findBlack = true,
            float? fuzzyFactor = null, float buffer = 1, float min = 0, float max = 0)
        {
            try
            {
                // Search for the edge of pixel content
                float? edge = FindEdge(side, probe, shrink, findBlack, fuzzyFactor, buffer, min, max);

                // If an edge was found, adjust the side of the zone accordingly.
                if (edge != null)
                {
                    InflateSide(side, shrink ? -edge.Value : edge.Value);
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
        /// fuzzy logic will be used to find an edge (default = <see langword="null"/>).</param>
        /// <param name="buffer">If a qualifying row is found, how many rows prior to the qualifying
        /// row return. This will never result in a negative number for a found edge. (default = 1)
        /// </param>
        /// <param name="min">The minimum number of rows to search. (default = 0)</param>
        /// <param name="max">The maximum number of rows to search. Specify 0 if there is no
        /// maximum value other than the other side of the zone when <see paramref="shrink"/> is
        /// <see langword="true"/>. (default = 0)</param>
        /// <returns>The number of pixels from the side to a qualifying edge of pixel content.
        /// -1 if no edge was found.</returns>
        public float? FindEdge(Side side, PixelProbe probe, bool shrink = true, bool findBlack = true,
            float? fuzzyFactor = null, float buffer = 0, float min = 0, float max = 0)
        {
            try
            {
                using (Matrix transform = new Matrix())
                {
                    // Retrieve a working rectangle relative to the zone's coordinate system and
                    // with the side to be operated upon as the left side.
                    RectangleF rectangle = GetWorkingRectangle(side, transform);

                    // Reverse the transform so that it can be used to convert working coordinate to
                    // image coordiates.
                    transform.Invert();

                    // If shrinking and a max row was not sepecified or the specified max is wider
                    // than the working rectangle, adjust accordingly.
                    if (shrink && (max <= 0 || max > (int)rectangle.Width))
                    {
                        max = (int)rectangle.Width;
                    }

                    // Round pixels in a way that ensures zone bounds encompass all content they are
                    // intended to.
                    bool roundPixelUp = (findBlack == shrink);

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
	                    int initialCount = CheckRowPixels(rectangle, 0, transform, probe,
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
	                    int count = CheckRowPixels(rectangle, row, transform, probe, roundPixelUp,
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
                            edge = countQualifier.GetQualifyingRow(row, count);
                            if (edge.HasValue)
                            {
                                break;
                            }

                            edge = spreadQualifier.GetQualifyingRow(row, spread);
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
            using (Matrix transform = new Matrix())
            {
                // Retrieve a working rectangle relative to the zone's coordinate system and
                // with the side to be operated upon as the left side.
                RectangleF rectangle = GetWorkingRectangle(side, transform);

                // Ensure we don't move one side past the opposing side.
                if (rectangle.Width + distance < 0)
                {
                    distance = -rectangle.Width;
                }

                // Adjust the rectangle by the specified distance.
                rectangle.Location =
                    new PointF(rectangle.Location.X - distance, rectangle.Location.Y);
                rectangle.Width += distance;

                // Obtain the new vertices.
                PointF[] updatedVertices = 
                { 
                    rectangle.Location, 
                    new PointF(rectangle.Right, rectangle.Top),
                    new PointF(rectangle.Right, rectangle.Bottom),
                    new PointF(rectangle.Left, rectangle.Bottom)
                };

                // Invert the transform so the vertices can be translated back into image
                // coordinates.
                transform.Invert();

                transform.TransformPoints(updatedVertices);
                updatedVertices.CopyTo(_vertices, 0);

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
        }

        /// <summary>
        /// Converts this instance to a <see cref="RasterZone"/>.
        /// </summary>
        /// <returns>The <see cref="RasterZone"/> equivalent of this instance.</returns>
        public RasterZone ToRasterZone()
        {
            using (Matrix transform = new Matrix())
            {
                // Retrieve a working rectangle relative to the zone's coordinate system and
                // with the side to be operated upon as the left side.
                RectangleF rectangle = GetWorkingRectangle(Side.Left, transform);

                // Compute the start and end points.
                float midPoint = rectangle.Location.Y + (_height / 2);

                PointF[] startAndEndPoints = new PointF[2];
                startAndEndPoints[0] = new PointF(rectangle.Left, midPoint);
                startAndEndPoints[1] = new PointF(rectangle.Right, midPoint);

                // Invert the transform so the computed coordinates can be converted back into image
                // coordinates.
                transform.Invert();
                transform.TransformPoints(startAndEndPoints);

                // Create the raster zone
                return new RasterZone((int)Math.Round(startAndEndPoints[0].X), (int)Math.Round(startAndEndPoints[0].Y),
                                      (int)Math.Round(startAndEndPoints[1].X), (int)Math.Round(startAndEndPoints[1].Y),
                                      (int)Math.Ceiling(_height), _pageNumber);
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
                FittingData clone = (FittingData)MemberwiseClone();

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
        /// <param name="transform">A <see cref="Matrix"/> used to rotate the vertices into the
        /// working coordinate system. This method has the side effect of leaving the appropriate
        /// transforms from the image coordinate system to the working coordinate system in place
        /// when this method returns.</param>
        /// <returns>The <see cref="RectangleF"/> representing the appropriately rotated zone.
        /// </returns>
        RectangleF GetWorkingRectangle(Side side, Matrix transform)
        {
            // Rotate the zone to horizontal
            transform.Rotate(-(float)_angle);

            // If necessary, rotate it further such that the side to be worked on becomes the left
            // side of the working zone.
            if (side != Side.Left)
            {
                transform.Rotate((float)side * -90);
            }

            // Calculate the vertices of this rectangle.
            PointF[] workingVertices = new PointF[4];
            _vertices.CopyTo(workingVertices, 0);
            transform.TransformPoints(workingVertices);

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
        /// <param name="transform">A <see cref="Matrix"/> that will convert the working coordinates
        /// into image coordinates.</param>
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
        static int CheckRowPixels(RectangleF rectangle, float offset, Matrix transform,
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
                    IsPixelBlack(pixel, transform, probe, roundPixelUp, out pixelOffPage);
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
        /// <param name="transform">A <see cref="Matrix"/> that will convert the working coordinates
        /// into image coordinates.</param>
        /// <param name="probe">A <see cref="PixelProbe"/> that allows access to the image pixels.
        /// </param>
        /// <param name="roundPixelUp"><see langword="true"/> to round pixel coordinates up before
        /// checking pixel color, <see langword="true"/> to round down.</param>
        /// <param name="offPage"><see langword="true"/> if pixel is off-page.</param>
        /// <returns>
        /// <see langword="true"/> if the specified pixel is black; <see langword="false"/> if the
        /// specified pixel is white.
        /// </returns>
        static bool IsPixelBlack(PointF pixel, Matrix transform, PixelProbe probe,
            bool roundPixelUp, out bool offPage)
        {
            // Convert the pixel coordinates into image coordinates.
            PointF[] pixelArray = new PointF[] { pixel };
            transform.TransformPoints(pixelArray);

            Point point = roundPixelUp ? Point.Ceiling(pixelArray[0]) : Point.Truncate(pixelArray[0]);

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
            /// If a row is found to be qualified fuzzily as an edge of pixel content, search this
            /// many rows more of content for true edge before returning the fuzzy edge.
            /// </summary>
            const int _QUALIFIED_ROW_BUFFER = 25;

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
            /// <returns>A qualifying row index or <see langword="null"/> if no qualifying row has
            /// yet been found.</returns>
            public float? GetQualifyingRow(float row, int value)
            {
                if (_qualifiedRow.HasValue)
                {
                    // If we have a qualified row, continue searching for a true edge for a bit more.
                    if (Math.Abs(row - _qualifiedRow.Value) > _QUALIFIED_ROW_BUFFER)
                    {
                        return _qualifiedRow;
                    }
                }
                else if (value < _minimaThreshold)
                {
                    // The value for this row is below the threshold to be a candidate edge.
                    _candidateRow = row;
                    _minimaThreshold = value;
                    _maximaThreshold = (int)Math.Floor((float)value / _factor);
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