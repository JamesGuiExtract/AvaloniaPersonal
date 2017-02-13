using System;
using System.Drawing;                   // for Point
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileAPI_VS2017.Models
{
    /// <summary>
    /// enum for Attribute Type
    /// </summary>
    public enum AttributeType
    {
        /// <summary>
        /// The attribute is a redaction
        /// </summary>
        Redaction = 1,

        /// <summary>
        /// The attribute is a clue
        /// </summary>
        Clue,

        /// <summary>
        /// the attribute is a Data element
        /// </summary>
        Data
    };

    /// <summary>
    /// enumeration that represents the relative confidence in a particular redacted element
    /// </summary>
    public enum RedactionConfidence
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
        Low
    }

    /// <summary>
    /// Spatial-line "zone" - a possibly skewed rectangular area. The skew
    /// is represented by height relative to the starting point. Typically 
    /// the end point is vertically displayed by one (when there is no skew), 
    /// so that start and end determine a line, which defines a rectangle 
    /// when the height is applied.
    /// </summary>
    public class SpatialLineZone
    {
        /// <summary>
        /// Start Point (of type: System.Drawing.Point) 
        /// </summary>
        public Point Start { get; set; }

        /// <summary>
        /// End Point
        /// </summary>
        public Point End { get; set; }

        /// <summary>
        /// The height of the bounding rect, relative to the Start point.
        /// </summary>
        public uint Height { get; set; }
    }

    /// <summary>
    /// The bounding rectangle that encloses the attribute
    /// </summary>
    public class SpatialLineBounds
    {
        /// <summary>
        /// the top-left corner of the bounding rectangle.
        /// </summary>
        public Point TopLeft { get; set; }

        /// <summary>
        /// The botton right corner of the bounding rectangle.
        /// </summary>
        public Point BottonRight { get; set; }
    }

    /// <summary>
    /// a line or partial line of text with the corresponding bounds and spatial position (including skew)
    /// </summary>
    public class SpatialLine
    {
        /// <summary>
        /// The zone - includes skew
        /// </summary>
        public SpatialLineZone Zone { get; set; }

        /// <summary>
        /// The bounds - the rectangular boundary that completely encloses the attribute
        /// </summary>
        public SpatialLineBounds Bounds { get; set; }
    }

    /// <summary>
    /// The position information of the attribute, and the associated page number of the document that the attribute is found on.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// The page number where the attribute exists
        /// </summary>
        public uint PageNumber { get; set; }

        /// <summary>
        /// The spatial information of the attribute on the page
        /// </summary>
        public SpatialLine LineInfo { get; set; }
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
        public uint AverageCharacterConfidence { get; set; }

        /// <summary>
        /// The type of the attribute - redaction, clue, or data
        /// </summary>
        public AttributeType AttributeTypeOf { get; set; }

        /// <summary>
        /// The confidence level of the redaction
        /// </summary>
        public RedactionConfidence RedactionConfidenceLevel { get; set; }

        /// <summary>
        /// The spatial position information of the attribute, inculding the page number, bounding rect, and zonal information (bounds plus skew)
        /// </summary>
        public Position SpatialPosition { get; set; }

    }
}
