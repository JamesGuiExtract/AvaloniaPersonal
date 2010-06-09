using Extract.Drawing;
using Extract.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace Extract.DataEntry
{
    /// <summary>
    /// Provides helper methods used to generates spatial hints for missing data.
    /// </summary>
    internal class SpatialHintGenerator
    {
        #region Constants

        // The amount of difference in skew between raster zones that will be tolerated before
        // ignoring the zone when computing a direct hint to prevent the discrepancy from
        // placing the hint incorrectly.
        private const double _SKEW_TOLERANCE = 5.0;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Cached hint information for a set of row or column raster zones that was calculated
        /// during a previous call to GetRowColumnIntersectionSpatialHint.
        /// </summary>
        private Dictionary<int, Dictionary<int, RowOrColumnHint>> _rowColumnSet1Cache = 
            new Dictionary<int, Dictionary<int, RowOrColumnHint>>();

        /// <summary>
        /// Cached hint information for a set of row or column raster zones that was calculated
        /// during a previous call to GetRowColumnIntersectionSpatialHint.
        /// </summary>
        private Dictionary<int, Dictionary<int, RowOrColumnHint>> _rowColumnSet2Cache =
            new Dictionary<int, Dictionary<int, RowOrColumnHint>>();

        #endregion Fields

        #region Structs

        /// <summary>
        /// A struct to represent the starting and ending coordinate of the hint in the appropriate
        /// dimension.
        /// </summary>
        public struct HintRange
        {
            /// <summary>
            /// The orientation (in degrees) of the hint range against a normal vertically oriented
            /// page.
            /// </summary>
            public double _orientation;

            /// <summary>
            /// The starting coordinate of the hint.
            /// </summary>
            public int _start;

            /// <summary>
            /// The ending coordinate of the hint.
            /// </summary>
            public int _end;
        }

        /// <summary>
        /// Contains spatial hint information that pertains to a row or column of data.
        /// </summary>
        private struct RowOrColumnHint
        {
            /// <summary>
            /// The page on which the spatial hint is located.
            /// </summary>
            public int _page;

            /// <summary>
            /// <see langword="true"/> if the hint pertains to horizontally aligned data (row),
            /// <see langword="false"/> if the hint pertains to vertically aligned data (column),
            /// or <see langword="null"/> if the hint pertains to either or neither vertically or
            /// horizontally aligned data.
            /// </summary>
            public bool? _horizontal;

            /// <summary>
            /// A <see cref="HintRange"/> that applies horizontally.
            /// </summary>
            public HintRange? _horizontalHintRange;

            /// <summary>
            /// A <see cref="HintRange"/> that applies vertically.
            /// </summary>
            public HintRange? _verticalHintRange;
        }

        #endregion Structs

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="SpatialHintGenerator"/> instance.
        /// </summary>
        public SpatialHintGenerator()
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Attempts to compute a spatial hint using the intersection of the spatial range of
        /// zoneSet1 in one dimension and zoneSet2 in the other dimension.  In order for a hint to
        /// be calculated, at least 2 zones in one of the sets on a given page must overlap either
        /// horizontally or vertically. If there is overlap both horizontally or vertically in a
        /// single set on the given page, the dimension with a larger amount of overlap will be 
        /// assumed the dimension for which the set should be used to calculate hint coordinates. 
        /// If both sets appear to represent data aligned in the same dimension, no hint will be 
        /// calculated. If a hint is generated, it will represent the furthest extent of zones
        /// in the dimension for which they are to apply.
        /// <para><b>Requirements</b></para>
        /// <see cref="ClearHintCache"/> must be called any time the data associated with any set
        /// previously provided to the <see cref="SpatialHintGenerator"/> instance has changed,
        /// otherwise it may generate inaccurate hints.
        /// </summary>
        /// <param name="set1Index">An index to identify the set <see cref="RasterZone"/>s provided
        /// via the zoneSet1 parameter.</param>
        /// <param name="zoneSet1">A set of <see cref="RasterZone"/>s may represent data aligned in
        /// one dimension (horizontally or vertically).</param>
        /// <param name="set2Index">An index to identify the set <see cref="RasterZone"/>s provided
        /// via the zoneSet2 parameter.</param>
        /// <param name="zoneSet2">A set of <see cref="RasterZone"/>s may represent data aligned in
        /// one dimension (horizontally or vertically)-- preferably not the same dimension as for
        /// zoneSet1.</param>
        /// <returns>An enumeration of a single <see cref="RasterZone"/> representing
        /// the horizontal and vertical intersection of the zones if a hint was able to be
        /// calculated, or <see langword="null"/> if a hint was not able to be calculated with the
        /// provided data.</returns>
        public RasterZone GetRowColumnIntersectionSpatialHint(int set1Index,
            IEnumerable<RasterZone> zoneSet1, int set2Index, IEnumerable<RasterZone> zoneSet2)
        {
            try
            {
                Dictionary<int, RowOrColumnHint> set1Hints = null;
                Dictionary<int, RowOrColumnHint> set2Hints = null;

                // For each provided set, attempt to find associated a cached set of 
                // RowOrColumnHints or generate a one for each page on which zones in 
                // the set appear if no cached data is available.
                if (!_rowColumnSet1Cache.TryGetValue(set1Index, out set1Hints))
                {
                    set1Hints = GetSpatialRangeHints(zoneSet1);
                    _rowColumnSet1Cache[set1Index] = set1Hints;
                }

                if (!_rowColumnSet2Cache.TryGetValue(set2Index, out set2Hints))
                {
                    set2Hints = GetSpatialRangeHints(zoneSet2);
                    _rowColumnSet2Cache[set2Index] = set2Hints;
                }

                // Iterate through all the RowOrColumnHints generated for set1 looking for one that
                // applies to the same page as a RowOrColumnHint generated for set2.
                foreach (RowOrColumnHint hint1 in set1Hints.Values)
                {
                    RowOrColumnHint hint2;
                    if (set2Hints.TryGetValue(hint1._page, out hint2))
                    {
                        // If neither RowOrColumnHint found a dimension for which it should apply,
                        // no overall hint can be calculated.
                        if (hint1._horizontal == null && hint2._horizontal == null)
                        {
                            return null;
                        }

                        // If both RowOrColumnHints appear to apply to the same dimension, no 
                        // overall hint can be calculated.
                        if (hint1._horizontal != null && hint2._horizontal != null &&
                            hint1._horizontal.Value == hint2._horizontal.Value)
                        {
                            return null;
                        }

                        // If hint 1 pertains to horizontal coordinates and hint 2 pertains to vertical
                        // coordinates.
                        if ((hint1._horizontal != null && hint1._horizontal.Value) ||
                            (hint1._horizontal == null && !hint2._horizontal.Value))
                        {
                            // Ensure both hint ranges are defined
                            if (hint1._horizontalHintRange != null && hint2._verticalHintRange != null)
                            {
                                return CreateIntersectionHint(hint1._horizontalHintRange.Value,
                                                          hint2._verticalHintRange.Value, hint1._page);
                            }
                        }
                        // If hint 2 pertains to horizontal coordinates and hint 1 pertains to vertical
                        // coordinates.
                        else if ((hint2._horizontal != null && hint2._horizontal.Value) ||
                                 (hint2._horizontal == null && !hint1._horizontal.Value))
                        {
                            // Ensure both hint ranges are defined
                            if (hint2._horizontalHintRange != null && hint1._verticalHintRange != null)
                            {
                                return CreateIntersectionHint(hint2._horizontalHintRange.Value,
                                                          hint1._verticalHintRange.Value, hint1._page);
                            }
                            
                        }
                    }
                }

                // No RowOrColumnHints sharing the same page were found.
                return null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25004", ex);
            }
        }

        /// <summary>
        /// Clears cached hint data that <see cref="SpatialHintGenerator"/> accumulate over
        /// repeated calls for spatial hints in order to improve efficiency of repeated calls
        /// on the same data set.  This call is required for accurate spatial hint generation
        /// after spatial from previous calls has changed.
        /// </summary>
        public void ClearHintCache()
        {
            try
            {
                _rowColumnSet1Cache.Clear();
                _rowColumnSet2Cache.Clear();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25167", ex);
            }
        }

        #endregion Methods

        #region Private Members

        /// <summary>
        /// Creates a spatial hint, if possible, based on the intersection of the specified
        /// horizontal and vertical hint ranges.
        /// </summary>
        /// <param name="horizontalHintRange">The horizontal <see cref="HintRange"/> to use to
        /// generate the hint.</param>
        /// <param name="verticalHintRange">The vertical <see cref="HintRange"/> to use to generate
        /// the hint.</param>
        /// <param name="page">The page on which the hint should be generated.</param>
        /// <returns>A <see cref="RasterZone"/> representing the spatial hint.</returns>
        private static RasterZone CreateIntersectionHint(HintRange horizontalHintRange, 
            HintRange verticalHintRange, int page)
        {
            // If the orientations of the two hint ranges are outside of the tolerated range, a hint
            // cannot be reliably generated.
            double angleDelta = GeometryMethods.GetAngleDelta(
                horizontalHintRange._orientation, verticalHintRange._orientation, true);
            if (Math.Abs(angleDelta) > _SKEW_TOLERANCE)
            {
                return null;
            }

            // Create the start and end points for the raster zone based on a combination of the
            // coordinates of the hint ranges.
            int height = verticalHintRange._end - verticalHintRange._start;
            int halfHeight = height / 2;
            Point[] rasterZonePoints = { 
                new Point(horizontalHintRange._start, verticalHintRange._start + halfHeight),
                new Point(horizontalHintRange._end, verticalHintRange._start + halfHeight) };

            // Rotate the points so that they align with the orientation of the hint ranges'
            // orientations.
            using (Matrix transform = new Matrix())
            {
                double orientation =
                    (verticalHintRange._orientation + horizontalHintRange._orientation) / 2;
                transform.Rotate((float)orientation);
                transform.TransformPoints(rasterZonePoints);
            }

            return new RasterZone(rasterZonePoints[0], rasterZonePoints[1], height, page);
        }

        /// <summary>
        /// Attempts to generate a <see cref="RowOrColumnHint"/> for each page that contains a zone
        /// from the provided set of zones. If possible, it will attempt to determine whether the
        /// zones should define a range of coordinates either horizontally or vertically, otherwise a
        /// range of coordinates for both dimensions will be generated and it will be left up to the
        /// caller to decide if it should apply vertically or horizontally.
        /// </summary>
        /// <param name="zones">An enumeration of <see cref="RasterZone"/>s which to
        /// use in defining a range of coordinates for the resulting <see cref="RowOrColumnHint"/>s.
        /// </param>
        /// <returns>A dictionary in which the key is an <see langword="int"/> that
        /// represents the page number the corresponding <see cref="RowOrColumnHint"/> value applies
        /// to.</returns>
        private static Dictionary<int, RowOrColumnHint> GetSpatialRangeHints(
            IEnumerable<RasterZone> zones)
        {
            // The return value
            Dictionary<int, RowOrColumnHint> hintRanges = new Dictionary<int, RowOrColumnHint>();

            // A dictionary used to sort the provided zones by page.
            Dictionary<int, List<RasterZone>> pagesOfZones = new Dictionary<int, List<RasterZone>>();

            // Iterate through all the provided zones and sort them by page.
            foreach (RasterZone zone in zones)
            {
                if (!pagesOfZones.ContainsKey(zone.PageNumber))
                {
                    pagesOfZones[zone.PageNumber] = new List<RasterZone>();
                }

                pagesOfZones[zone.PageNumber].Add(zone);
            }

            // For each page of zones, generate a RowOrColumnHint instance based on the zones for that
            // page.
            foreach (int page in pagesOfZones.Keys)
            {
                // Get the RasterZones that apply to this page.
                List<RasterZone> pageZones = pagesOfZones[page];

                // Get a hint range that applies horizontally.
                double horizontalOverlap;
                HintRange? horizontalHintRange = GetHintRange(pageZones, true, out horizontalOverlap);

                // Get a hint range that applies vertically.
                double verticalOverlap;
                HintRange? verticalHintRange = GetHintRange(pageZones, false, out verticalOverlap);

                // Create the RowOrColumnHint.
                RowOrColumnHint hint = new RowOrColumnHint();
                hint._page = page;

                // If the zones overlap more horizontally than vertically, apply the RowOrColumnHint
                // horizontally.
                if (horizontalOverlap > verticalOverlap)
                {
                    hint._horizontal = true;
                    hint._horizontalHintRange = horizontalHintRange;
                    hint._verticalHintRange = null;
                }
                // If the zones overlap more vertically than horizontally, apply the RowOrColumnHint
                // vertically.
                else if (verticalOverlap > horizontalOverlap)
                {
                    hint._horizontal = false;
                    hint._verticalHintRange = verticalHintRange;
                    hint._horizontalHintRange = null;
                }
                // If it is not clear based on overlap to which dimension the RowOrColumnHint should
                // apply, fill in a hint range for both dimensions.
                else
                {
                    hint._horizontal = null;
                    hint._horizontalHintRange = horizontalHintRange;
                    hint._verticalHintRange = verticalHintRange;
                }

                hintRanges[page] = hint;
            }

            return hintRanges;
        }

        /// <summary>
        /// Generate a <see cref="HintRange"/> in the specified dimension.
        /// </summary>
        /// <param name="zones">A list of <see cref="RasterZone"/>s to be used to generate the 
        /// <see cref="HintRange"/>.</param>
        /// <param name="horizontal"><see langword="true"/> if the <see cref="HintRange"/> is to
        /// apply horizontally or <see langword="false"/> if the <see cref="HintRange"/> is to apply
        /// vertically.</param>
        /// <param name="averageOverlap">The overall percentage of the <see cref="RasterZone"/>s 
        /// that overlap in the given dimension (0 if none of the zones overlap with another zone,
        /// 1 if all zones share the exact same coordinates in the specified dimension.</param>
        /// <returns>The <see cref="HintRange"/> instance that applies to the specified 
        /// <see cref="RasterZone"/> in the specified dimension or <see langword="null"/> if a hint
        /// range can not be generated using the provided <see cref="RasterZone"/>s.</returns>
        public static HintRange? GetHintRange(List<RasterZone> zones, bool horizontal, 
            out double averageOverlap)
        {
            // Initialize a new HintRange instance and the averageOverlap
            HintRange hintRange = new HintRange();
            hintRange._start = -1;
            hintRange._end = -1;
            averageOverlap = 0;

            // Compile the orientations of provided raster zones.
            double?[] orientations = new double?[zones.Count];
            double totalOrientation = 0;
            for (int i = 0; i < zones.Count; i++)
            {
                orientations[i] = zones[i].ComputeSkew(true);
                totalOrientation += orientations[i].Value;
            }

            // Determine the initial average orientation.
            int includedZones = zones.Count;
            double averageOrientation = totalOrientation / includedZones;

            // Exclude zones whose orientations are not within _SKEW_TOLERANCE of the average
            // orientation.
            for (int i = 0; i < zones.Count; i++)
            {
                double angleDelta = GeometryMethods.GetAngleDelta(
                    orientations[i].Value, averageOrientation, true);

                if (Math.Abs(angleDelta) > _SKEW_TOLERANCE)
                {
                    totalOrientation -= orientations[i].Value;
                    includedZones--;

                    // A null entry indicates the zone is not included.
                    orientations[i] = null;
                }
            }

            // If there are no remaining raster zones, a hint cannot be generated.
            if (includedZones == 0)
            {
                return null;
            }

            // Re-compute the average orientation based only on the raster zones to be included
            // in computing the hint.
            averageOrientation = totalOrientation / includedZones;
            hintRange._orientation = (float)averageOrientation;

            // Create a cache for the start and end coordinate of each zone.
            int[] startPosition = new int[zones.Count];
            int[] endPosition = new int[zones.Count];

            // Create a transform matrix for GetStart and GetEnd to rotate the hint coordinates back into image coordinates.
            using (Matrix transform = new Matrix())
            {
                transform.Rotate((float)-averageOrientation);   
 
                // Populate the cache with zone coordinates and set the HintRange using the widest
                // range of start and end coordinates in the list.
                for (int i = 0; i < zones.Count; i++)
                {
                    if (orientations[i] == null)
                    {
                        continue;
                    }

                    startPosition[i] = GetStart(zones[i], horizontal, transform);
                    if (hintRange._start == -1 || startPosition[i] < hintRange._start)
                    {
                        hintRange._start = startPosition[i];
                    }

                    endPosition[i] = GetEnd(zones[i], horizontal, transform);
                    if (hintRange._end == -1 || endPosition[i] > hintRange._end)
                    {
                        hintRange._end = endPosition[i];
                    }
                }
            }

            // Initialize counters to keep track of the total overlap and total length of all zones
            // to be compared.
            int totalOverlap = 0;
            int totalLength = 0;

            // Iterate to calculate the overlap for each pair of zones.
            for (int i = 0; i < zones.Count; i++)
            {
                for (int j = i + 1; j < zones.Count; j++)
                {
                    ExtractException.Assert("ELI24999", "Internal logic error!",
                        zones[i].PageNumber == zones[j].PageNumber);

                    // If the zones overlap in the given dimension, increment the overlap
                    // accordingly.
                    if (startPosition[i] < endPosition[j] && endPosition[i] > startPosition[j])
                    {
                        totalOverlap += Math.Min(endPosition[i], endPosition[j]) -
                                        Math.Max(startPosition[i], startPosition[j]);
                    }

                    // Increment the total length using an average of the length of zones being
                    // compared (to ensure that if both zones share the same coordinates that 
                    // overlap == length.
                    totalLength += ((endPosition[i] - startPosition[i]) +
                                   (endPosition[j] - startPosition[j])) / 2;
                }
            }

            if (totalLength == 0)
            {
                averageOverlap = 0;
            }
            else
            {
                // Calculate the average amount of overlap in the zones.
                averageOverlap = ((double)totalOverlap / (double)totalLength);
            }

            return hintRange;
        }

        /// <summary>
        /// Retrieves the starting coordinate of the specified <see cref="RasterZone"/> in the
        /// specified dimension.
        /// </summary>
        /// <param name="zone">The <see cref="RasterZone"/> for which the starting coordinate is
        /// needed.</param>
        /// <param name="horizontal"><see langword="true"/> to retrieve the left-most x-coordinate,
        /// <see langword="false"/> to retrieve the top-most y-coordinate.</param>
        /// <param name="transform">The <see cref="Matrix"/> to use to rotate the provided
        /// <see cref="RasterZone"/> into the hint's coordinate system before finding the starting
        /// position.</param>
        /// <returns>The starting coordinate in the specifed dimension.</returns>
        private static int GetStart(RasterZone zone, bool horizontal, Matrix transform)
        {
            // Get the bounds of the raster zone in the hint's coordinate system.
            Rectangle bounds = GetTransformedRasterZoneBounds(zone, transform);

            if (horizontal)
            {
                return bounds.Left;
            }
            else
            {
                return bounds.Top;
            }
        }

        /// <summary>
        /// Retrieves the ending coordinate of the specified <see cref="RasterZone"/> in the
        /// specified dimension.
        /// </summary>
        /// <param name="zone">The <see cref="RasterZone"/> for which the ending coordinate is
        /// needed.</param>
        /// <param name="horizontal"><see langword="true"/> to retrieve the right-most x-coordinate,
        /// <see langword="false"/> to retrieve the bottom-most y-coordinate.</param>
        /// <param name="transform">The <see cref="Matrix"/> to use to rotate the provided
        /// <see cref="RasterZone"/> into the hint's coordinate system before finding the starting
        /// position.</param>
        /// <returns>The ending coordinate in the specifed dimension.</returns>
        private static int GetEnd(RasterZone zone, bool horizontal, Matrix transform)
        {
            // Get the bounds of the raster zone in the hint's coordinate system.
            Rectangle bounds = GetTransformedRasterZoneBounds(zone, transform);

            if (horizontal)
            {
                return bounds.Right;
            }
            else
            {
                return bounds.Bottom;
            }
        }

        /// <summary>
        /// Obtains the bounds of specified <see cref="RasterZone"/> transformed into the coordinate
        /// system defined by the supplied <see cref="Matrix"/>.
        /// </summary>
        /// <param name="zone">The <see cref="RasterZone"/> whose bounds are to be calculated.
        /// </param>
        /// <param name="transform">The <see cref="Matrix"/> used to transform the specified zone
        /// before calculating the bounds.</param>
        /// <returns>A <see cref="Rectangle"/> describing the transformed bounds of the supplied
        /// zone.</returns>
        private static Rectangle GetTransformedRasterZoneBounds(RasterZone zone, Matrix transform)
        {
            // Transform the zone's start and end point using the supplied matrix.
            Point[] rasterZonePoints = { new Point(zone.StartX, zone.StartY),
                new Point(zone.EndX, zone.EndY) };
            transform.TransformPoints(rasterZonePoints);

            // Create a new zone using the transformed coordinates.
            RasterZone rotatedRasterZone = new RasterZone(rasterZonePoints[0], rasterZonePoints[1],
                zone.Height, zone.PageNumber);

            // Return the new zone's bounds.
            return rotatedRasterZone.GetRectangularBounds();
        }

        #endregion Private Members
    }
}
