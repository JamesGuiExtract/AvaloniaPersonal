

namespace WebAPI.Models
{
    /// <summary>
    /// Data required when skipping a document
    /// </summary>
    public class SkipDocumentData
    {
        /// <summary>
        /// Not used
        /// </summary>
        public int Duration { get; set; }

        /// The time, in ms, the user spent moving the mouse or using the keyboard
        public int ActivityTime { get; set; }

        /// The time, in ms, the application was busy, preventing the user from being active
        public int OverheadTime { get; set; }

        /// <summary>
        /// Comment to save when skipping a document
        /// </summary>
        public string Comment { get; set; }
    }
}
