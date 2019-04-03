using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// A result representing the location of words in a document based on OCR results.
    /// </summary>
    public class WordZoneDataResult
    {
        /// <summary>
        /// Gets a list of SpatialLineZones instances, each of which represents one word
        /// from OCR results, grouped by spatial line. NOTE: Non-spatial text will not be included in these zones.
        /// </summary>
        public List<List<SpatialLineZone>> Zones { get; set; } = new List<List<SpatialLineZone>>();
    }
}
