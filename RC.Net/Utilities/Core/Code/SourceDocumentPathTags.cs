using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents the source document path tag.
    /// </summary>
    [DisplayName("Source Document")]
    public class SourceDocumentPathTags : PathTagsBase
    {
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
            tagsToValues.Add("<SourceDocName>", sourceDocument);
            return tagsToValues;
        }

        #endregion SourceDocumentPathTags Methods
    }
}
