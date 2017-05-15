using System.Collections.Generic;
using System.Drawing;                   // for Point

namespace DocumentAPI.Models
{
    /// <summary>
    /// enumeration that represents the relative confidence in a particular redacted element
    /// </summary>
    public enum ConfidenceLevel
    {
        /// <summary>
        /// whenever the data item (attribute) is not a redaction type, then the confidence value is not relevant
        /// </summary>
        NotApplicable = 1,

        /// <summary>
        /// high confidence in redacted element
        /// </summary>
        High,

        /// <summary>
        /// medium confidence
        /// </summary>
        Medium,

        /// <summary>
        /// low confidence
        /// </summary>
        Low,

        /// <summary>
        /// element was manually redacted
        /// </summary>
        Manual
    }

    /// <summary>
    /// Spatial-line "zone" - a possibly skewed rectangular area. The skew
    /// is represented by height relative to the starting point. Typically 
    /// the end point is vertically displayed by one (when there is no skew), 
    /// so that start and end determine a line, which defines a rectangle 
    /// when the height is applied.
    /// </summary>
    public class SpatialLineZone // page coordinates - raster zone
    {
        /// <summary>
        /// The page number where the attribute exists, or -1 when the attribute doesn't have spatial info.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The text value of the zone
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// left side of zone
        /// </summary>
        public int StartX { get; set; }

        /// <summary>
        /// top of zone 
        /// </summary>
        public int StartY { get; set; }

        /// <summary>
        /// right side of zone
        /// </summary>
        public int EndX { get; set; }

        /// <summary>
        /// bottom side of zone
        /// </summary>
        public int EndY { get; set; }

        /// <summary>
        /// The height of the bounding rect, relative to the Start point.
        /// </summary>
        public int Height { get; set; }
    }

    /// <summary>
    /// The bounding rectangle that encloses the attribute
    /// </summary>
    public class SpatialLineBounds // page coordinates
    {
        /// <summary>
        /// The start page number where the attribute exists, or -1 when the attribute doesn't have spatial info.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The text value of the zone
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// the top of the bounding rectangle.
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// the left edge of the bounding rectangle.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// The botton of the bounding rectangle.
        /// </summary>
        public int Botton { get; set; }

        /// <summary>
        /// The right edge of the bounding rectangle.
        /// </summary>
        public int Right { get; set; }
    }

    /// <summary>
    /// a line or partial line of text with the corresponding bounds and spatial position (including skew)
    /// </summary>
    public class SpatialLine
    {
        /// <summary>
        /// The zone - includes skew
        /// </summary>
        public SpatialLineZone SpatialLineZone { get; set; }

        /// <summary>
        /// The bounds - the rectangular boundary that completely encloses the attribute
        /// </summary>
        public SpatialLineBounds SpatialLineBounds { get; set; }
    }

    /// <summary>
    /// The position information of the attribute, and the associated page number of the document that the attribute is found on.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// a list of the pages that the attribute spans (if any)
        /// </summary>
        public List<int> Pages { get; set; }
        /// <summary>
        /// The list of lines of spatial information for the attribute - may span pages
        /// </summary>
        public List<SpatialLine> LineInfo { get; set; }
    }

    /// <summary>
    /// A document attribute - a feature that has been identified as significant according to the processing rules
    /// </summary>
    public class DocumentAttribute
    {
        /// <summary>
        /// Name of the attribute
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of the attribute
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The assigned type of the attribute
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The average OCR recognition confidence of each character value in the defined attribute
        /// </summary>
        public int AverageCharacterConfidence { get; set; }

        /// <summary>
        /// The confidence level of the redaction,
        /// based on ConfidenceLevel enumeration, expressed as a string name.
        /// </summary>
        public string ConfidenceLevel { get; set; }

        /// <summary>
        /// Some attributes do not have position info - in that case this will be false and the LineInfo
        /// members will be empty.
        /// </summary>
        public bool HasPositionInfo { get; set; }

        /// <summary>
        /// The spatial position information of the attribute, inculding the page number, bounding rect, and zonal information (bounds plus skew)
        /// </summary>
        public Position SpatialPosition { get; set; }

        /// <summary>
        /// child attributes, 0..N
        /// </summary>
        public List<DocumentAttribute> ChildAttributes { get; set; }
    }
}
