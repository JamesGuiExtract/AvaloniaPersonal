using Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using WebAPI.Models;

namespace WebAPI
{
    /// <summary>
    /// This static class converts spatial info from SpatialString instances to data models used for
    /// API call return values.
    /// </summary>
    public static class SpatialStringMapper
    {
        #region Methods

        /// <summary>
        /// Creates and populates a SpatialLine instance.
        /// </summary>
        /// <param name="spatialString">a SpatialString object</param>
        /// <param name="includeBounds"></param>
        /// <returns>a populated SpatialLine list</returns>
        static public List<SpatialLine> MakeLineInfo(this SpatialString spatialString, bool includeBounds)
        {
            try
            {
                var lines = new List<SpatialLine>();
                if (!spatialString.HasSpatialInfo())
                {
                    return lines;
                }

                var pageInfos = spatialString.SpatialPageInfos;
                Contract.Assert(pageInfos != null, "spatial string: {0}, has null spatialPageInfos", spatialString.String);

                IUnknownVector zones = spatialString.GetOriginalImageRasterZones();
                Contract.Assert(zones != null, "spatial string: {0}, has null imageRasterZones", spatialString.String);

                var numberOfZones = zones.Size();
                for (int i = 0; i < numberOfZones; ++i)
                {
                    RasterZone zone = (RasterZone)zones.At(i);
                    Contract.Assert(zone != null,
                                    "null raster zone, index: {0}, numberOfZones: {1}, spatialString: {2}",
                                    i,
                                    numberOfZones,
                                    spatialString.String);

                    var spatialLineZone = MakeSpatialZone(zone, spatialString.String);

                    var thisPageInfo = (SpatialPageInfo)pageInfos.GetValue(zone.PageNumber);
                    Contract.Assert(thisPageInfo != null, "pageInfos.getValue returned null for spatialString: {0}", spatialString.String);

                    var spatialLineBounds = includeBounds ? MakeSpatialLineBounds(spatialString, zone, thisPageInfo) : null;

                    SpatialLine spatialLine = new SpatialLine();
                    spatialLine.SpatialLineBounds = spatialLineBounds;
                    spatialLine.SpatialLineZone = spatialLineZone;
                    lines.Add(spatialLine);
                }

                return lines;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45363");
            }
        }

        /// <summary>
        /// Maps the spatial string to word zone data.
        /// </summary>
        /// <param name="spatialString">The spatial string.</param>
        /// <returns></returns>
        public static WordZoneData MapSpatialStringToWordZoneData(this SpatialString spatialString)
        {
            try
            {
                var wordZoneData = new WordZoneData();
                var words = spatialString.GetWords();
                int count = words.Size();
                for (int i = 0; i < count; i++)
                {
                    var word = (SpatialString)words.At(i);
                    wordZoneData.Zones.Add(word.MakeLineInfo(false).Single().SpatialLineZone);
                }

                return wordZoneData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45359");
            }
        }

        /// <summary>
        /// Adds spatial information to an existing Position object
        /// </summary>
        /// <param name="spatialString">a SpatialString object</param>
        /// <param name="verboseSpatialData"><c>false</c> to include only the spatial data needed for
        /// extract software to represent spatial strings; <c>true</c> to include data that may be
        /// useful to 3rd party integrators.</param>
        /// <returns>Position object</returns>
        public static Position SetSpatialPosition(this SpatialString spatialString, bool verboseSpatialData)
        {
            try
            {
                Position position = new Position();

                IUnknownVector lines = spatialString.GetLines();
                Contract.Assert(lines != null, "null return from SpatialString.GetLines, spatial string: {0}", spatialString.String);

                SortedSet<int> setOfPages = new SortedSet<int>();
                var lineCount = lines.Size();
                if (lineCount > 0)
                {
                    position.LineInfo = new List<SpatialLine>();
                    if (verboseSpatialData)
                    {
                        position.Pages = new List<int>();
                    }

                    for (int i = 0; i < lineCount; ++i)
                    {
                        SpatialString line = (SpatialString)lines.At(i);
                        Contract.Assert(line != null, "Failed to get SpatialString line from index: {0}, spatial string: {1}", i, spatialString.String);
                        if (!line.HasSpatialInfo())
                        {
                            continue;
                        }

                        var spatialLine = MakeLineInfo(line, includeBounds: verboseSpatialData);
                        position.LineInfo.AddRange(spatialLine);

                        if (verboseSpatialData)
                        {
                            var pages = spatialLine.Select(sl => sl.SpatialLineBounds.PageNumber);
                            foreach (var page in pages)
                            {
                                setOfPages.Add(page);
                            }
                        }
                    }

                    if (verboseSpatialData)
                    {
                        position.Pages = setOfPages.ToList();
                    }
                }

                return position;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45364");
            }
        }

        #endregion Methods

        #region Private Members

        /// <summary>
        /// Creates and populates a SpatialLineBounds instance.
        /// </summary>
        /// <param name="spatialString">instance of a SpatialString</param>
        /// <param name="zone">the associated rasterZone</param>
        /// <param name="pageInfo">PageInfo for this zone</param>
        /// <returns>a populated SpatialLineBounds instance</returns>
        static SpatialLineBounds MakeSpatialLineBounds(SpatialString spatialString,
                                                               RasterZone zone,
                                                               SpatialPageInfo pageInfo)
        {
            var bounds = new SpatialLineBounds();
            bounds.Text = spatialString.String;
            bounds.PageNumber = zone.PageNumber;

            LongRectangle pageBounds = new LongRectangle();
            Contract.Assert(pageBounds != null, "pageBounds allocation failed");

            pageBounds.SetBounds(0, 0, pageInfo.Width, pageInfo.Height);
            var rect = zone.GetRectangularBounds(pageBounds);
            Contract.Assert(rect != null, "zone.getrectangularBounds returned null bounds");

            bounds.Top = rect.Top;
            bounds.Left = rect.Left;
            bounds.Bottom = rect.Bottom;
            bounds.Right = rect.Right;

            return bounds;
        }

        /// <summary>
        /// Creates and populates a SpatialLineZone instance (corresponds to a RasterZone).
        /// </summary>
        /// <param name="rasterZone">a RasterZone instance</param>
        /// <param name="spatialStringValue">text of the spatial string</param>
        /// <returns>a SpatialLineZone instance</returns>
        static SpatialLineZone MakeSpatialZone(RasterZone rasterZone, string spatialStringValue)
        {
            int startX = 0, startY = 0, endX = 0, endY = 0, height = 0, page = 0;
            rasterZone.GetData(ref startX, ref startY, ref endX, ref endY, ref height, ref page);

            var zone = new SpatialLineZone();

            zone.PageNumber = page;
            zone.Text = spatialStringValue;
            zone.StartX = startX;
            zone.StartY = startY;
            zone.EndX = endX;
            zone.EndY = endY;
            zone.Height = height;

            return zone;
        }

        #endregion Private Members
    }
}
