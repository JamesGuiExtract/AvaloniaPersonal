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
        /// </summary>
        const int _CURRENT_VERSION = 1;

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
            paginatedOutputPriority: EFilePriority.kPriorityAboveNormal)
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
            paginatedOutputPriority: source.PaginatedOutputPriority)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSettings"/> class.
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
        public PaginationSettings(string paginationSourceAction,
            string paginationOutputPath, string paginationOutputAction,
            EFilePriority paginatedOutputPriority)
        {
            PaginationSourceAction = paginationSourceAction;
            PaginationOutputPath = paginationOutputPath;
            PaginationOutputAction = paginationOutputAction;
            PaginatedOutputPriority = paginatedOutputPriority;
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

                return new PaginationSettings(paginationSourceAction,
                    paginationOutputPath, paginationOutputAction, paginatedOutputPriority);
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
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI39855", "Unable to write pagination settings.", ex);
            }
        }

        #endregion Methods
    }
}
