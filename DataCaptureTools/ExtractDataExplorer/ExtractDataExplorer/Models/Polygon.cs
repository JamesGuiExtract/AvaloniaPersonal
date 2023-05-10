using Extract.ErrorHandling;
using System.Collections.Generic;
using System.Linq;

namespace ExtractDataExplorer.Models
{
    /// <summary>
    /// Part of an attribute's spatial data
    /// </summary>
    /// <remarks>
    /// A polygon is assumed to be a rectangle but for now it is only required to have four points
    /// to be constructed (there are no further assertions)
    /// </remarks>
    public class Polygon
    {
        /// <summary>
        /// The page that this polygon belongs to
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// The points that make up the polygon
        /// </summary>
        public System.Drawing.PointF[] Points { get; }

        /// <summary>
        /// Construct a polygon from points + page number
        /// </summary>
        /// <param name="pageNumber">The page that this polygon belongs to</param>
        /// <param name="points">The points that make up the polygon. Exactly four points are required</param>
        public Polygon(int pageNumber, IList<System.Drawing.PointF> points)
        {
            ExtractException.Assert("ELI56305", "Exactly four points are expected", points?.Count == 4);

            PageNumber = pageNumber;
            Points = points.ToArray();
        }
    }
}
