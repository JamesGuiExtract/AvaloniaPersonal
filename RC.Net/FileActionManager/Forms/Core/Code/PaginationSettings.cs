using Extract.Interop;
using System;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Encapsulates the settings used for pagination.
    /// </summary>
    [CLSCompliant(false)]
    public class PaginationSettings
    {
        #region Constants

        /// <summary>
        /// The current version number for this object
        /// Versions:
        /// 1. Initial version
        /// 2. Added OutputExpectedPaginationAttributesFiles and ExpectedPaginationAttributesOutputPath
        /// 3. Added RequireAllPagesToBeViewed.
        /// </summary>
        const int _CURRENT_VERSION = 3;

        /// <summary>
        /// The default path for expected pagination attributes file
        /// </summary>
        const string _DEFAULT_EXPECTED_OUTPUT_PATH = "<SourceDocName>.pagination.evoa";

        /// <summary>
        /// A special path tag used for paginated output doc name that is a iterator of the number
        /// of paginated output documents created with at least one page of the given source
        /// document.
        /// </summary>
        public const string SubDocIndexTag = "<SubDocIndex>";

        /// <summary>
        /// A special path tag used for paginated output doc name to indicate the first page number
        /// of the source document used.
        /// </summary>
        public const string FirstPageTag = "<FirstPageNumber>";

        /// <summary>
        /// A special path tag used for paginated output doc name to indicate the last page number
        /// of the source document used.
        /// </summary>
        public const string LastPageTag = "<LastPageNumber>";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSettings"/> class.
        /// </summary>
        public PaginationSettings()
            : this(
            paginationSourceAction: null,
            paginationOutputPath: "$InsertBeforeExtension(<SourceDocName>,_<SubDocIndex>)",
            paginationOutputAction: null,
            paginatedOutputPriority: EFilePriority.kPriorityAboveNormal,
            outputExpectedPaginationAttributesFiles: false,
            expectedPaginationAttributesOutputPath: _DEFAULT_EXPECTED_OUTPUT_PATH,
            requireAllPagesToBeViewed: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSettings"/> class.
        /// </summary>
        /// <param name="source">This instance will be initialized as a copy of the specified
        /// value.</param>
        public PaginationSettings(PaginationSettings source)
            : this(
            paginationSourceAction: source.PaginationSourceAction,
            paginationOutputPath: source.PaginationOutputPath,
            paginationOutputAction: source.PaginationOutputAction,
            paginatedOutputPriority: source.PaginatedOutputPriority,
            outputExpectedPaginationAttributesFiles: source.OutputExpectedPaginationAttributesFiles,
            expectedPaginationAttributesOutputPath: source.ExpectedPaginationAttributesOutputPath,
            requireAllPagesToBeViewed: source.RequireAllPagesToBeViewed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSettings" /> class.
        /// </summary>
        /// <param name="paginationSourceAction">The action (if any) into which paginated sources
        /// should be moved to pending.</param>
        /// <param name="paginationOutputPath">The path tag expression defining the filename that
        /// should be given to a pagination output document.</param>
        /// <param name="paginationOutputAction">The action into which manually paginated output
        /// documents should be moved to pending..</param>
        /// <param name="paginatedOutputPriority">The minimum priority that should be assigned a
        /// pagination output document. The priority assigned may be higher than this if one of the
        /// source documents had a higher priority.</param>
        /// <param name="outputExpectedPaginationAttributesFiles">Whether to output expected
        /// pagination attributes.</param>
        /// <param name="expectedPaginationAttributesOutputPath">The expected pagination attributes
        /// path.</param>
        /// <param name="requireAllPagesToBeViewed">Whether the user should have to view all pages
        /// for pagination.</param>
        public PaginationSettings(string paginationSourceAction,
            string paginationOutputPath, string paginationOutputAction,
            EFilePriority paginatedOutputPriority, bool outputExpectedPaginationAttributesFiles,
            string expectedPaginationAttributesOutputPath, bool requireAllPagesToBeViewed)
        {
            PaginationSourceAction = paginationSourceAction;
            PaginationOutputPath = paginationOutputPath;
            PaginationOutputAction = paginationOutputAction;
            PaginatedOutputPriority = paginatedOutputPriority;
            OutputExpectedPaginationAttributesFiles = outputExpectedPaginationAttributesFiles;
            ExpectedPaginationAttributesOutputPath = expectedPaginationAttributesOutputPath;
            RequireAllPagesToBeViewed = requireAllPagesToBeViewed;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The action (if any) into which paginated sources should be moved to pending.
        /// </summary>
        public string PaginationSourceAction
        {
            get;
            private set;
        }

        /// <summary>
        /// The path tag expression defining the filename that should be given to a pagination
        /// output document.
        /// </summary>
        public string PaginationOutputPath
        {
            get;
            private set;
        }

        /// <summary>
        /// The action into which manually paginated output documents should be moved to pending.
        /// </summary>
        public string PaginationOutputAction
        {
            get;
            private set;
        }

        /// <summary>
        /// The minimum priority that should be assigned a pagination output document. The priority
        /// assigned may be higher than this if one of the source documents had a higher priority.
        /// </summary>
        public EFilePriority PaginatedOutputPriority
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether to output expected pagination attributes.
        /// </summary>
        public bool OutputExpectedPaginationAttributesFiles
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the file path to output expected pagination attributes to
        /// </summary>
        public string ExpectedPaginationAttributesOutputPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets whether the user should have to view all pages for pagination.
        /// </summary>
        public bool RequireAllPagesToBeViewed
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="PaginationSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="PaginationSettings"/>.</param>
        /// <returns>A <see cref="PaginationSettings"/> created from the specified
        /// <see cref="IStreamReader"/>.</returns>
        public static PaginationSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                int version = reader.ReadInt32();
                if (version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI39853",
                        "Cannot load newer version of PaginationSettings");
                    ee.AddDebugData("Current Version", _CURRENT_VERSION, false);
                    ee.AddDebugData("Version To Load", version, false);
                    throw ee;
                }

                string paginationSourceAction = reader.ReadString();
                string paginationOutputPath = reader.ReadString();
                string paginationOutputAction = reader.ReadString();
                EFilePriority paginatedOutputPriority = (EFilePriority)reader.ReadInt32();

                bool outputExpectedPaginationAttributesFiles = false;
                string expectedPaginationAttributesOutputPath = _DEFAULT_EXPECTED_OUTPUT_PATH;

                if (version >= 2)
                {
                    outputExpectedPaginationAttributesFiles = reader.ReadBoolean();
                    expectedPaginationAttributesOutputPath = reader.ReadString();
                }

                bool requireAllPagesToBeViewed = false;

                if (version >= 3)
                {
                    requireAllPagesToBeViewed = reader.ReadBoolean();
                }

                return new PaginationSettings(paginationSourceAction,
                    paginationOutputPath, paginationOutputAction, paginatedOutputPriority,
                    outputExpectedPaginationAttributesFiles, expectedPaginationAttributesOutputPath,
                    requireAllPagesToBeViewed);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI39854", "Unable to read pagination settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="PaginationSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="PaginationSettings"/> will be written.</param>
        public void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(_CURRENT_VERSION);
                writer.Write(PaginationSourceAction);
                writer.Write(PaginationOutputPath);
                writer.Write(PaginationOutputAction);
                writer.Write((int)PaginatedOutputPriority);
                writer.Write(OutputExpectedPaginationAttributesFiles);
                writer.Write(ExpectedPaginationAttributesOutputPath);
                writer.Write(RequireAllPagesToBeViewed);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI39855", "Unable to write pagination settings.", ex);
            }
        }

        #endregion Methods
    }
}
