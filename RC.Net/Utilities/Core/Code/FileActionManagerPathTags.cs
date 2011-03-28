using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents File Action Manager document tags.
    /// </summary>
    [DisplayName("File Action Manager")]
    public class FileActionManagerPathTags : PathTagsBase
    {
        #region Constants

        /// <summary>
        /// Constant string for the source doc tag.
        /// </summary>
        public static readonly string SourceDocumentTag = "<SourceDocName>";

        /// <summary>
        /// Constant string for the FPS file directory tag.
        /// </summary>
        public static readonly string FpsFileDirectoryTag = "<FPSFileDir>";

        #endregion Constants

        #region FileActionManagerPathTags Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTags"/> class.
        /// </summary>
        public FileActionManagerPathTags()
            : this("", "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTags"/> class.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="fpsDirectory">The directory of the fps file.</param>
        public FileActionManagerPathTags(string sourceDocument, string fpsDirectory)
            : base(GetTagsToValues(sourceDocument, fpsDirectory))
        {
        }

        #endregion FileActionManagerPathTags Constructors

        #region FileActionManagerPathTags Methods

        /// <summary>
        /// Gets the path tags mapped to their expanded form.
        /// </summary>
        /// <param name="sourceDocument">The source document name.</param>
        /// <param name="fpsDirectory">The fps file.</param>
        /// <returns>The path tags mapped to their expanded form.</returns>
        static Dictionary<string, string> GetTagsToValues(string sourceDocument,
            string fpsDirectory)
        {
            Dictionary<string, string> tagsToValues = new Dictionary<string,string>(2);
            tagsToValues.Add(SourceDocumentTag, sourceDocument);
            tagsToValues.Add(FpsFileDirectoryTag, fpsDirectory);
            return tagsToValues;
        }

        #endregion FileActionManagerPathTags Methods
    }
}
