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
    }
}
