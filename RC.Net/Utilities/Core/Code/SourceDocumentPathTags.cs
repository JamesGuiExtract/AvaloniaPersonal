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
        public SourceDocumentPathTags(string sourceDocument)
            : base(GetTagsToValues(sourceDocument))
        {
        }

        #endregion SourceDocumentPathTags Constructors

        #region SourceDocumentPathTags Methods

        /// <summary>
        /// Gets the path tags mapped to their expanded form.
        /// </summary>
        /// <param name="sourceDocument">The source document name.</param>
        /// <returns>The path tags mapped to their expanded form.</returns>
        static Dictionary<string, string> GetTagsToValues(string sourceDocument)
        {
            Dictionary<string, string> tagsToValues = new Dictionary<string, string>(1);
            tagsToValues.Add(_SOURCE_DOC_NAME_TAG, sourceDocument);
            return tagsToValues;
        }

        #endregion SourceDocumentPathTags Methods

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

        #endregion Properties

    }
}
