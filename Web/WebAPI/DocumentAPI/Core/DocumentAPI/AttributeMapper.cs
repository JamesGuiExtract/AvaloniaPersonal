using DocumentAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using static DocumentAPI.Utils;


namespace DocumentAPI
{
    /// <summary>
    /// This class maps FAM Attribute to Web API DocumentAttribute
    /// </summary>
    public class AttributeMapper
    {
        /// <summary>
        /// Is the attribute name an IDShield name?
        /// </summary>
        /// <param name="name">name value</param>
        /// <returns>true iff it is an IDShield name</returns>
        private static bool IsIdShieldName(string name)
        {
            if (name.IsEquivalent("HCData") ||
                name.IsEquivalent("MCData") ||
                name.IsEquivalent("LCData") ||
                name.IsEquivalent("Clues")   ||
                name.IsEquivalent("Manual") )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// For IDShield there is some moderate translation of the name that is performed here.
        /// </summary>
        /// <param name="originalName">attribute name</param>
        private static string DetermineName(string originalName)
        {
            var name = originalName;
            if (IsIdShieldName(name))
            {
                if (name.IsEquivalent("HCData") ||
                    name.IsEquivalent("MCData") ||
                    name.IsEquivalent("LCData") ||
                    name.IsEquivalent("Manual") )
                {
                    name = "Data";
                }
                else if (name.IsEquivalent("Clues"))
                {
                    // This is done to ensure that differently cased variants of "clues" get translated reliably
                    // into "Clues" always.
                    name = "Clues";
                }
            }

            return name;
        }

        /// <summary>
        /// Determines type of attribute - note that ORIGINAL name is passed to this routine
        /// Based on AttributeType, but expressed as a string for read-ability
        /// </summary>
        /// <param name="originalName">original Attribute.Name</param>
        /// <returns></returns>
        private static string TypeOfAttribute(string originalName)
        {
            if (IsIdShieldName(originalName))
            {
                return Enum.GetName(typeof(AttributeType), AttributeType.Redaction);
            }

            return Enum.GetName(typeof(AttributeType), AttributeType.Data);
        }

        private static Dictionary<string, string> mapRedactionConfidenceValues = 
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"HCData", Enum.GetName(typeof(ConfidenceLevel), ConfidenceLevel.High) },
                {"MCData", Enum.GetName(typeof(ConfidenceLevel), ConfidenceLevel.Medium) },
                {"LCData", Enum.GetName(typeof(ConfidenceLevel), ConfidenceLevel.Low)},
                {"Manual", Enum.GetName(typeof(ConfidenceLevel), ConfidenceLevel.Manual) }
            };

        /// <summary>
        /// determines redaction confidence - Note that ORIGINAL name is used here!
        /// Note that this is based on ConfidenceLevel enumeration, but returned as the string name
        /// </summary>
        /// <param name="originalName">original Attribute.Name</param>
        /// <returns></returns>
        private static string DetermineRedactionConfidence(string originalName)
        {
            string confidence;
            var found = mapRedactionConfidenceValues.TryGetValue(originalName, out confidence);
            if (found)
            {
                return confidence;
            }

            return Enum.GetName(typeof(ConfidenceLevel), ConfidenceLevel.NotApplicable);
        }

        /// <summary>
        /// Creates and populates a SpatialLineBounds instance.
        /// </summary>
        /// <param name="spatialString">instance of a SpatialString</param>
        /// <param name="zone">the associated rasterZone</param>
        /// <param name="pageInfo">PageInfo for this zone</param>
        /// <returns>a populated SpatialLineBounds instance</returns>
        private static SpatialLineBounds MakeSpatialLineBounds(SpatialString spatialString, 
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
            bounds.Botton = rect.Bottom;
            bounds.Right = rect.Right;

            return bounds;
        }

        /// <summary>
        /// Creates and populates a SpatialLineZone instance (coresponds to a RasterZone).
        /// </summary>
        /// <param name="rasterZone">a RasterZone instance</param>
        /// <param name="spatialStringValue">text of the spatial string</param>
        /// <returns>a SpatialLineZone instance</returns>
        private static SpatialLineZone MakeSpatialZone(RasterZone rasterZone, string spatialStringValue)
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

        /// <summary>
        /// Creates and populates a SpatialLine instance.
        /// </summary>
        /// <param name="spatialString">a SpatialString object</param>
        /// <returns>a populated SpatialLine list</returns>
        private static List<SpatialLine> MakeLineInfo(SpatialString spatialString)
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

                var spatialLineBounds = MakeSpatialLineBounds(spatialString, zone, thisPageInfo);

                SpatialLine spatialLine = new SpatialLine();
                spatialLine.SpatialLineBounds = spatialLineBounds;
                spatialLine.SpatialLineZone = spatialLineZone;
                lines.Add(spatialLine);
            }

            return lines;
        }

        /// <summary>
        /// Adds spatial information to an existing Position object
        /// </summary>
        /// <param name="spatialString">a SpatialString object</param>
        /// <returns>Position object</returns>
        private static Position SetSpatialPosition(SpatialString spatialString)
        {
            Position position = new Position();

            IUnknownVector lines = spatialString.GetLines();
            Contract.Assert(lines != null, "null return from SpatialString.GetLines, spatial string: {0}", spatialString.String);

            SortedSet<int> setOfPages = new SortedSet<int>();
            var lineCount = lines.Size();
            if (lineCount > 0)
            {
                position.LineInfo = new List<SpatialLine>();
                position.Pages = new List<int>();

                for (int i = 0; i < lineCount; ++i)
                {
                    SpatialString line = (SpatialString)lines.At(i);
                    Contract.Assert(line != null, "Failed to get SpatialString line from index: {0}, spatial string: {1}", i, spatialString.String);
                    if (!line.HasSpatialInfo())
                    {
                        continue;
                    }

                    var spatialLine = MakeLineInfo(line);
                    position.LineInfo.AddRange(spatialLine);

                    var pages = spatialLine.Select(sl => sl.SpatialLineBounds.PageNumber);
                    foreach (var page in pages)
                    {
                        setOfPages.Add(page);
                    }
                }

                position.Pages = setOfPages.ToList();
            }

            return position;
        }

        /// <summary>
        /// map an IAttribute to a (top-level) DocumentAtribute instance
        /// </summary>
        /// <param name="attr">Attribute instance</param>
        /// <returns>DocumentAttribute object</returns>
        private static DocumentAttribute MapAttribute(IAttribute attr)
        {
            var docAttr = new DocumentAttribute();

            try
            {
                docAttr.Name = DetermineName(attr.Name);
                docAttr.Type = attr.Type;

                SpatialString spatialString = attr.Value;
                Contract.Assert(spatialString != null, "spatial string is null, attribute name: {0}", docAttr.Name);
                docAttr.Value = spatialString.String;

                int minConf = 0;
                int maxConf = 0;
                int avgConf = 0;
                spatialString.GetCharConfidence(ref minConf, ref maxConf, ref avgConf);
                docAttr.AverageCharacterConfidence = avgConf;

                docAttr.ConfidenceLevel = DetermineRedactionConfidence(attr.Name);
                docAttr.HasPositionInfo = spatialString.HasSpatialInfo();

                docAttr.SpatialPosition = SetSpatialPosition(spatialString);

                // Process subattributes, if any
                var childAttrs = attr.SubAttributes;

                docAttr.ChildAttributes = new List<DocumentAttribute>();
                if (childAttrs != null)
                {
                    var childAttrsCount = childAttrs.Size();
                    if (childAttrsCount > 0)
                    {
                        for (int j = 0; j < childAttrsCount; ++j)
                        {
                            var childAttr = (IAttribute)childAttrs.At(j);
                            var childDocAttr = MapAttribute(childAttr);
                            docAttr.ChildAttributes.Add(childDocAttr);
                        }
                    }
                }

                return docAttr;
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception: {ex.Message}, while processing attribute: {docAttr.Name}"));
                throw;
            }
        }

        /// <summary>
        /// map IUnknownVector (of iAttribute) to a document attribute set.
        /// </summary>
        /// <param name="attributes">IUnknownVector of IAttribute(s)</param>
        /// <returns>corresponding DocumentAttributeSet</returns>
        public static DocumentAttributeSet MapAttributesToDocumentAttributeSet(IUnknownVector attributes)
        {
            IAttribute attr = null;

            try
            {
                var rootDocAttrSet = new DocumentAttributeSet();
                rootDocAttrSet.Attributes = new List<DocumentAttribute>();

                for (int i = 0; i < attributes.Size(); ++i)
                {
                    attr = (IAttribute)attributes.At(i);
                    Contract.Assert(attr != null, 
                                    "Invalid top-level attribute at index: {0}, unknownVector.Size: {1}", 
                                    i, 
                                    attributes.Size());

                    Log.WriteLine(Inv($"mapping attribute named: {attr.Name}"));

                    var docAttr = MapAttribute(attr);
                    rootDocAttrSet.Attributes.Add(docAttr);
                }

                return rootDocAttrSet;
            }
            catch (Exception ex)
            {
                if (attr != null)
                {
                    Log.WriteLine(Inv($"Exception: {ex.Message}, while processing attribute: {attr.Name}"));
                }
                else
                {
                    Log.WriteLine(Inv($"Exception: {ex.Message}, while processing attribute (null)"));
                }

                return MakeDocumentAttributeSetError(ex.Message);
            }
        }
    }
}
