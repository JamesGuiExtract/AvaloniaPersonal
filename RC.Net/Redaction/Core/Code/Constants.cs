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
        /// The name to apply to the metadata attribute for VOA file merge sessions.
        /// </summary>
        public static readonly string VOAFileMergeSessionMetaDataName = "_VOAFileMergeSession";

        /// <summary>
        /// The name to apply to the metadata attribute for VOA file compare sessions.
        /// </summary>
        public static readonly string VOAFileCompareSessionMetaDataName = "_VOAFileCompareSession";

        /// <summary>
        /// The attribute names of all possible metadata sessions.
        /// </summary>
        public static readonly string[] AllSessionMetaDataNames = new string[] {
            VerificationSessionMetaDataName, RedactionSessionMetaDataName,
            SurroundContextSessionMetaDataName, VOAFileMergeSessionMetaDataName,
            VOAFileCompareSessionMetaDataName };

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

        /// <summary>
        /// The names for FileInfo elements.
        /// </summary>
        public static readonly string FileInfo = "FileInfo";

        /// <summary>
        /// The names for SourceDocName elements / metadata attribute names.
        /// </summary>
        public static readonly string SourceDocName = "SourceDocName";
        public static readonly string SourceDocNameMetadata = "_" + SourceDocName;

        /// <summary>
        /// The names for IDShieldDataFile elements / metadata attribute names.
        /// </summary>
        public static readonly string IDShieldDataFile = "IDShieldDataFile";
        public static readonly string IDShieldDataFileMetadata = "_" + IDShieldDataFile;

        /// <summary>
        /// The names for OutputFile elements / metadata attribute names.
        /// </summary>
        public static readonly string OutputFile = "OutputFile";
        public static readonly string OutputFileMetadata = "_" + OutputFile;

        /// <summary>
        /// The names for IDAndRevision elements / metadata attribute names.
        /// </summary>
        public static readonly string IDAndRevision = "IDAndRevision";
        public static readonly string IDAndRevisionMetadata = "_" + IDAndRevision;

        /// <summary>
        /// The names for ImportedToID elements / metadata attribute names.
        /// </summary>
        public static readonly string ImportedToID = "ImportedToID";
        public static readonly string ImportedToIDMetadata = "_" + ImportedToID;
        
        /// <summary>
        /// The names for MergedToID elements / metadata attribute names.
        /// </summary>
        public static readonly string MergedToID = "MergedToID";
        public static readonly string MergedToIDMetadata = "_" + MergedToID;

        /// <summary>
        /// The names for Options elements / metadata attribute names.
        /// </summary>
        public static readonly string Options = "Options";
        public static readonly string OptionsMetadata = "_" + Options;

        /// <summary>
        /// The names for ID elements.
        /// </summary>
        public static readonly string ID = "ID";

        /// <summary>
        /// The names for ArchiveAction elements.
        /// </summary>
        public static readonly string ArchiveAction = "ArchiveAction";

        /// <summary>
        /// The ArchiveAction value to indicate a redaction has been turned off.
        /// </summary>
        public static readonly string TurnedOff = "TurnedOff";
    }
}
