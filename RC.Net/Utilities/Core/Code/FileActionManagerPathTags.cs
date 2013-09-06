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

        /// <summary>
        /// Constant string for the FPS filename tag.
        /// </summary>
        public static readonly string FpsFileNameTag = "<FPSFileName>";

        /// <summary>
        /// Constant string for the remote source doc tag.
        /// </summary>
        public static readonly string RemoteSourceDocumentTag = "<RemoteSourceDocName>";
        
        /// <summary>
        /// Constant string for the common components directory tag.
        /// </summary>
        public static readonly string CommonComponentsDirectoryTag = "<CommonComponentsDir>";

        #endregion Constants

        #region FileActionManagerPathTags Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTags"/> class.
        /// </summary>
        public FileActionManagerPathTags()
            : this("", "", "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTags"/> class.
        /// </summary>
        /// <param name="tagsToValues">A dictionary that maps path tags to their expanded value.
        /// </param>
        public FileActionManagerPathTags(IDictionary<string, string> tagsToValues)
            : base(tagsToValues)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTags"/> class.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="fpsDirectory">The directory of the fps file.</param>
        /// <param name="fpsFileName">The full filename of the fps file.</param>
        public FileActionManagerPathTags(string sourceDocument, string fpsDirectory,
            string fpsFileName)
            : base(GetTagsToValues(sourceDocument, fpsDirectory, fpsFileName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTags"/> class.
        /// </summary>
        /// <param name="sourceDocument">The source document</param>
        /// <param name="fpsDirectory">The directory of the fps file.</param>
        /// <param name="fpsFileName">The full filename of the fps file.</param>
        /// <param name="remoteSourceDocName">The remote source doc name</param>
        public FileActionManagerPathTags(string sourceDocument, string fpsDirectory,
            string fpsFileName, string remoteSourceDocName)
            : base(GetTagsToValues(
                sourceDocument, fpsDirectory, fpsFileName, remoteSourceDocName))
        {
        }

        #endregion FileActionManagerPathTags Constructors

        #region FileActionManagerPathTags Methods

        /// <overloads>Updates the tag values.</overloads>
        /// <summary>
        /// Updates the tag values based on the current <see paramref="sourceDocument"/> and
        /// <see paramref="fpsDirectory"/>.
        /// </summary>
        /// <param name="sourceDocument">The current source document.</param>
        /// <param name="fpsDirectory">The current FPS directory.</param>
        /// <param name="fpsFileName">The full filename of the fps file.</param>
        public void UpdateTagValues(string sourceDocument, string fpsDirectory, string fpsFileName)
        {
            try
            {
                Dictionary<string, string> newTagValues =
                    GetTagsToValues(sourceDocument, fpsDirectory, fpsFileName);
                foreach (KeyValuePair<string, string> tagValue in newTagValues)
                {
                    SetTagValue(tagValue.Key, tagValue.Value);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33213");
            }
        }

        /// <summary>
        /// Updates the tag values based on the current <see paramref="sourceDocument"/>,
        /// <see paramref="fpsDirectory"/> and <see paramref="remoteSourceDocName"/>.
        /// </summary>
        /// <param name="sourceDocument">The current source document.</param>
        /// <param name="fpsDirectory">The current FPS directory.</param>
        /// <param name="fpsFileName">The full filename of the fps file.</param>
        /// <param name="remoteSourceDocName">The current remote source document.</param>
        public void UpdateTagValues(string sourceDocument, string fpsDirectory, string fpsFileName,
            string remoteSourceDocName)
        {
            try
            {
                Dictionary<string, string> newTagValues =
                    GetTagsToValues(sourceDocument, fpsDirectory, fpsFileName, remoteSourceDocName);
                foreach (KeyValuePair<string, string> tagValue in newTagValues)
                {
                    SetTagValue(tagValue.Key, tagValue.Value);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33214");
            }
        }

        /// <summary>
        /// Gets the path tags mapped to their expanded form.
        /// </summary>
        /// <param name="sourceDocument">The source document name.</param>
        /// <param name="fpsDirectory">The fps file.</param>
        /// <param name="fpsFileName">The full filename of the fps file.</param>
        /// <returns>The path tags mapped to their expanded form.</returns>
        static Dictionary<string, string> GetTagsToValues(string sourceDocument,
            string fpsDirectory, string fpsFileName)
        {
            Dictionary<string, string> tagsToValues = new Dictionary<string,string>(4);
            tagsToValues.Add(SourceDocumentTag, sourceDocument);
            tagsToValues.Add(FpsFileDirectoryTag, fpsDirectory);
            tagsToValues.Add(FpsFileNameTag, fpsFileName);
            tagsToValues.Add(CommonComponentsDirectoryTag, FileSystemMethods.CommonComponentsPath);
            return tagsToValues;
        }

        /// <summary>
        /// Gets the path tags mapped to their expanded form
        /// </summary>
        /// <param name="sourceDocument">The source document</param>
        /// <param name="fpsDirectory">the fps file directory.</param>
        /// <param name="fpsFileName">The full filename of the fps file.</param>
        /// <param name="remoteSourceDocName">The remote source document</param>
        /// <returns>The path tags mapped to their expanded form.</returns>
        static Dictionary<string, string> GetTagsToValues(string sourceDocument,
            string fpsDirectory, string fpsFileName, string remoteSourceDocName)
        {
            Dictionary<string, string> tagsToValues = new Dictionary<string, string>(5);
            tagsToValues.Add(SourceDocumentTag, sourceDocument);
            tagsToValues.Add(FpsFileDirectoryTag, fpsDirectory);
            tagsToValues.Add(FpsFileNameTag, fpsFileName);
            tagsToValues.Add(RemoteSourceDocumentTag, remoteSourceDocName);
            tagsToValues.Add(CommonComponentsDirectoryTag, FileSystemMethods.CommonComponentsPath);
            return tagsToValues;
        }

        #endregion FileActionManagerPathTags Methods
    }
}
