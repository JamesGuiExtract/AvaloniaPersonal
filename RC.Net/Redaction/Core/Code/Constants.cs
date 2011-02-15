using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Redaction
{
    /// <summary>
    /// Global constants related to the Extract.Redation namespace.
    /// </summary>
    static class Constants
    {
        /// <summary>
        /// The name to apply to the metadata attribute for verification sessions.
        /// </summary>
        public static readonly string VerificationSessionMetaDataName = "_VerificationSession";

        /// <summary>
        /// The name to apply to the metadata attribute for redaction sessions.
        /// </summary>
        public static readonly string RedactionSessionMetaDataName = "_RedactedFileOutputSession";

        /// <summary>
        /// The name to apply to the metadata attribute for surround context sessions.
        /// </summary>
        public static readonly string SurroundContextSessionMetaDataName = "_SurroundContextSession";

        /// <summary>
        /// The attribute names of all possible metadata sessions.
        /// </summary>
        public static readonly string[] AllSessionMetaDataNames = new string[] {
            VerificationSessionMetaDataName, RedactionSessionMetaDataName,
            SurroundContextSessionMetaDataName };

        /// <summary>
        /// The type assigned to attributes where there's high confidence the data is sensitive.
        /// </summary>
        public static readonly string HCData = "HCData";

        /// <summary>
        /// The type assigned to attributes where there's medium confidence the data is sensitive.
        /// </summary>
        public static readonly string MCData = "MCData";

        /// <summary>
        /// The type assigned to attributes where there's low confidence the data is sensitive.
        /// </summary>
        public static readonly string LCData = "LCData";

        /// <summary>
        /// The type assigned to manually created attributes identifying sensitive data.
        /// </summary>
        public static readonly string Manual = "Manual";

// Commented out until needed so FXCop doesn't complain that Clues is unused.
//        /// <summary>
//        /// The type assigned to attributes that suggest the presence of sensitive data.
//        /// </summary>
//        public static readonly string Clues = "Clues";
    }
}
