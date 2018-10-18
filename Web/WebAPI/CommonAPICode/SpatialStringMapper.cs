using Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;
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
                IUnknownVector zones = spatialString.GetOriginalImageRasterZones();

                var numberOfZones = zones.Size();
                for (int i = 0; i < numberOfZones; ++i)
                {
                    RasterZone zone = (RasterZone)zones.At(i);

                    var spatialLineZone = MakeSpatialZone(zone, spatialString.String);

                    var thisPageInfo = (SpatialPageInfo)pageInfos.GetValue(zone.PageNumber);

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
        public static List<SpatialLineZone> MapSpatialStringToWordZoneData(this SpatialString spatialString)
        {
            try
            {
                var wordZoneData = new List<SpatialLineZone>();
                var words = spatialString.GetWords();
                int count = words.Size();
                for (int i = 0; i < count; i++)
                {
                    var word = (SpatialString)words.At(i);
                    if (word.HasSpatialInfo())
                    {
                        wordZoneData.Add(word.MakeLineInfo(false).Single().SpatialLineZone);
                    }
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
                if (!spatialString.HasSpatialInfo())
                {
                    return null;
                }

                Position position = new Position();

                IUnknownVector lines = spatialString.GetLines();

                SortedSet<int> setOfPages = new SortedSet<int>();
                var lineCount = lines.Size();
                if (lineCount > 0)
                {
                    position.LineInfo = new List<SpatialLine>();
                    position.Pages = new List<int>();

                    for (int i = 0; i < lineCount; ++i)
                    {
                        SpatialString line = (SpatialString)lines.At(i);
                        if (!line.HasSpatialInfo())
                        {
                            continue;
                        }

                        var spatialLine = MakeLineInfo(line, includeBounds: verboseSpatialData);
                        position.LineInfo.AddRange(spatialLine);

                        var pages = spatialLine.Select(sl => sl.SpatialLineZone.PageNumber);
                        foreach (var page in pages)
                        {
                            setOfPages.Add(page);
                        }
                    }

                    position.Pages = setOfPages.ToList();
                }

                return position;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45364");
            }
        }

        /// <summary>
        /// Returns an enumeration of all attributes and their subattributes, recursively.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s to enumerate.</param>
        /// <returns>An enumeration of the attribute and its subattributes, recursively</returns>
        public static IEnumerable<(IAttribute attribute, IAttribute parent)> Enumerate(this IUnknownVector attributes)
        {
            foreach (var attribute in attributes.ToIEnumerable<IAttribute>())
            {
                yield return (attribute, null);

                foreach (var (descendant, parent) in Enumerate(attribute.SubAttributes))
                {
                    yield return (descendant, parent ?? attribute);
                }
            }
        }

        /// <summary>
        /// Converts comVector into an enumerable.
        /// </summary>
        /// <typeparam name="T">The type of object in the vector.</typeparam>
        /// <param name="comVector">The <see cref="IIUnknownVector"/> to convert.</param>
        /// <returns>An enumerable of type T.</returns>
        public static IEnumerable<T> ToIEnumerable<T>(this IIUnknownVector comVector)
        {
            int size = comVector.Size();

            for (int i = 0; i < size; i++)
            {
                yield return (T)comVector.At(i);
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

            pageBounds.SetBounds(0, 0, pageInfo.Width, pageInfo.Height);
            var rect = zone.GetRectangularBounds(pageBounds);

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
