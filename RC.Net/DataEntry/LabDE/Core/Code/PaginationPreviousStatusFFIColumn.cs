using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// A <see cref="IFAMFileInspectorColumn"/> used in conjunction with a
    /// <see cref="PaginationDuplicateDocumentsFFIColumn"/> to display the action status of files
    /// before being checked out into the FFI being used to manage duplicate documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("DB2E7F28-1ACF-4246-9124-F1C662857ADA")]
    public class PaginationPreviousStatusFFIColumn : PreviousStatusFFIColumn
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationPreviousStatusFFIColumn"/> class.
        /// </summary>
        public PaginationPreviousStatusFFIColumn()
            : base()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44764");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationPreviousStatusFFIColumn"/> class.
        /// </summary>
        /// <param name="duplicateDocumentsColumn">The <see cref="PaginationDuplicateDocumentsFFIColumn"/>
        /// instance this column is being used in conjunction with.</param>
        internal PaginationPreviousStatusFFIColumn(PaginationDuplicateDocumentsFFIColumn duplicateDocumentsColumn)
            : base(duplicateDocumentsColumn)
        {
        }

        #endregion Constructors

        #region IFAMFileInspectorColumn Members

        /// <summary>
        /// Gets the value to display for the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileID">The file ID for which the current value is needed.</param>
        /// <returns>
        /// The value for the specified file.
        /// </returns>
        public override string GetValue(int fileID)
        {
            try
            {
                EActionStatus previousStatus = DuplicateDocumentsColumn.GetPreviousStatus(fileID);

                if (DuplicateDocumentsColumn.OriginalFileIds.Contains(fileID))
                {
                    return DuplicateDocumentsColumn.CurrentOption.Status;
                }
                else if (previousStatus == EActionStatus.kActionCompleted)
                {
                    // If the file is currently complete and the tag and/or metadata for the
                    // document indicate that the document has been ignored or stapled, indicate so
                    // rather than just indicating "Complete".
                    IFileProcessingDB famDb = DuplicateDocumentsColumn.FileProcessingDB;
                    ExtractException.Assert("ELI44765", "Database not available.", famDb != null);

                    IEnumerable<string> tagsOnFile = null;

                    // Retrieve the tags if either a TagForIgnore have been configured.
                    if (!string.IsNullOrWhiteSpace(DuplicateDocumentsColumn.TagForIgnore))
                    {
                        tagsOnFile = famDb.GetTagsOnFile(fileID).ToIEnumerable<string>();
                    }

                    // Has the TagForIgnore been applied?
                    if (!string.IsNullOrWhiteSpace(DuplicateDocumentsColumn.TagForIgnore) &&
                        tagsOnFile.Contains(DuplicateDocumentsColumn.TagForIgnore,
                            StringComparer.OrdinalIgnoreCase))
                    {
                        return DuplicateDocumentsColumn.IgnoreOption.Status;
                    }
                }

                switch (previousStatus)
                {
                    case EActionStatus.kActionCompleted: return "Complete";
                    case EActionStatus.kActionFailed: return "Failed";
                    case EActionStatus.kActionPending: return "Pending";
                    case EActionStatus.kActionProcessing: return "Processing";
                    case EActionStatus.kActionSkipped: return "Skipped";
                    case EActionStatus.kActionUnattempted: return "Unattempted";
                }

                ExtractException.ThrowLogicException("ELI44766");
                return "";
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44767", "Failed to get value.");
            }
        }

        #endregion IFAMFileInspectorColumn Members
    }
}
