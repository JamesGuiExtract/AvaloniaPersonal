using System.Collections.Generic;
using System.ComponentModel;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents the source document path tag.
    /// </summary>
    [DisplayName("Source Document")]
    public class SourceDocumentPathTags : PathTagsBase
    {
        #region Constants

        /// <summary>
        /// Constant for the source doc name tag.
        /// </summary>
        static readonly string _SOURCE_DOC_NAME_TAG = "<SourceDocName>";

        /// <summary>
        /// Constant string for the common components directory tag.
        /// </summary>
        static readonly string _COMMON_COMPONENTS_DIRECTORY_TAG = "<CommonComponentsDir>";

        #endregion Constants

        #region SourceDocumentPathTags Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceDocumentPathTags"/> class.
        /// </summary>
        public SourceDocumentPathTags()
            : this("")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceDocumentPathTags"/> class.
        /// </summary>
        /// <param name="sourceDocument">The value for the SourceDocName tag.</param>
        public SourceDocumentPathTags(string sourceDocument)
            : base()
        {
            try
            {
                StringData = sourceDocument;
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI38072");
            }
        }

        #endregion SourceDocumentPathTags Constructors

        #region Properties

        /// <summary>
        /// Gets the source document name tag.
        /// </summary>
        /// <value>The source document name tag.</value>
        public static string SourceDocumentTag
        {
            get
            {
                return _SOURCE_DOC_NAME_TAG;
            }
        }

        /// <summary>
        /// Gets the CommonComponentsDir tag name.
        /// </summary>
        /// <value>The CommonComponentsDir tag name.</value>
        public static string CommonComponentsDir
        {
            get
            {
                return _COMMON_COMPONENTS_DIRECTORY_TAG;
            }
        }

        #endregion Properties

    }
}
