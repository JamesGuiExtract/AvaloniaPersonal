

namespace WebAPI.Models
{
    /// <summary>
    /// Data required when skipping a document
    /// </summary>
    public class SkipDocumentData
    {
        /// <summary>
        /// Duration in ms, to use for updating the file task session record when skipping a document
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Comment to save when skipping a document
        /// </summary>
        public string Comment { get; set; }
    }
}
