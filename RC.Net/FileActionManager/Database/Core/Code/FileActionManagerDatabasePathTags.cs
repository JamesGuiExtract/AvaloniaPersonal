using Extract.Utilities;
using System;
using System.Collections.Generic;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// Represents File Action Manager database document tags.
    /// </summary>
    [CLSCompliant(false)]
    public class FileActionManagerDatabasePathTags : FileActionManagerPathTags
    {
        #region Constants

        /// <summary>
        /// Constant string for the action name tag
        /// </summary>
        public static readonly string ActionTag = "<ActionName>";

        /// <summary>
        /// Constant string for the database name tag
        /// </summary>
        public static readonly string DatabaseTag = "<DatabaseName>";

        /// <summary>
        /// Constant string for the database server name tag
        /// </summary>
        public static readonly string DatabaseServerTag = "<DatabaseServerName>";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTags"/> class.
        /// </summary>
        /// <param name="sourceDocument">The source document</param>
        /// <param name="fpsDirectory">The directory of the fps file.</param>
        /// <param name="fpsFileName">The full filename of the fps file.</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/>.</param>
        /// <param name="actionId">The current action in <see paramref="fileProcessingDB"/>.</param>
        public FileActionManagerDatabasePathTags(string sourceDocument, string fpsDirectory,
                string fpsFileName, IFileProcessingDB fileProcessingDB, int actionId)
            : base(GetTagsToValues(
                sourceDocument, fpsDirectory, fpsFileName, fileProcessingDB, actionId))
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Gets the path tags mapped to their expanded form
        /// </summary>
        /// <param name="sourceDocument">The source document</param>
        /// <param name="fpsDirectory">the fps file directory.</param>
        /// <param name="fpsFileName">The full filename of the fps file.</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/>.</param>
        /// <param name="actionId">The current action in <see paramref="fileProcessingDB"/>.</param>
        /// <returns>The path tags mapped to their expanded form.</returns>
        static Dictionary<string, string> GetTagsToValues(string sourceDocument,
            string fpsDirectory, string fpsFileName, IFileProcessingDB fileProcessingDB,
            int actionId)
        {
            Dictionary<string, string> tagsToValues = new Dictionary<string, string>(7);
            tagsToValues.Add(SourceDocumentTag, sourceDocument);
            tagsToValues.Add(FpsFileDirectoryTag, fpsDirectory);
            tagsToValues.Add(FpsFileNameTag, fpsFileName);
            tagsToValues.Add(ActionTag, (fileProcessingDB == null || actionId == 0)
                ? "" : fileProcessingDB.GetActionName(actionId));
            tagsToValues.Add(DatabaseTag, (fileProcessingDB == null)
                ? "" : fileProcessingDB.DatabaseName);
            tagsToValues.Add(DatabaseServerTag, (fileProcessingDB == null)
                ? "" : fileProcessingDB.DatabaseServer);
            tagsToValues.Add(CommonComponentsDirectoryTag, FileSystemMethods.CommonComponentsPath);
            return tagsToValues;
        }

        #endregion Methods
    }
}
