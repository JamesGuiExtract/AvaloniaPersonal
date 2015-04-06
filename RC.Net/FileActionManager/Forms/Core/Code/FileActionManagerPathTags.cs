using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Represents File Action Manager document tags.
    /// </summary>
    [DisplayName("File Action Manager")]
    [CLSCompliant(false)]
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

        /// <summary>
        /// Constant string for the database server tag.
        /// </summary>
        public static readonly string DatabaseServerTag = "<DatabaseServer>";

        /// <summary>
        /// Constant string for the database name tag.
        /// </summary>
        public static readonly string DatabaseNameTag = "<DatabaseName>";

        /// <summary>
        /// Constant string for the database action tag.
        /// </summary>
        public static readonly string DatabaseActionTag = "<DatabaseAction>";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="FAMTagManager"/> that will perform the primary tag and function expansion.
        /// </summary>
        FAMTagManager _famTagManager;

        #endregion Fields

        #region FileActionManagerPathTags Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTags"/> class.
        /// </summary>
        public FileActionManagerPathTags()
            : this(null, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTags"/> class.
        /// </summary>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to be used for primary tag and
        /// function expansion.</param>
        public FileActionManagerPathTags(FAMTagManager tagManager)
            : this(tagManager, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTags"/> class.
        /// </summary>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to be used for primary tag and
        /// function expansion.</param>
        /// <param name="sourceDocName">The current value to use for the SourceDocName tag.</param>
        public FileActionManagerPathTags(FAMTagManager tagManager, string sourceDocName)
            : base()
        {
            try
            {
                _famTagManager = tagManager ?? new FAMTagManager();
                TagUtility = (ITagUtility)_famTagManager;

                SourceDocName = sourceDocName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38067");
            }
        }

        #endregion FileActionManagerPathTags Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether to always report the database tags.
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, DatabaseServer, DatabaseName and DatabaseAction will be
        /// returned by GetBuiltInTags even if they haven't been defined in the CustomTags.sdf.
        /// </value>
        public bool AlwaysShowDatabaseTags
        {
            get
            {
                return _famTagManager.AlwaysShowDatabaseTags;
            }

            set
            {
                _famTagManager.AlwaysShowDatabaseTags = value;
            }
        }

        /// <summary>
        /// Gets or sets the current value to use for the SourceDocName tag.
        /// </summary>
        /// <value>
        /// The current value to use for the SourceDocName tag.
        /// </value>
        public string SourceDocName
        {
            get
            {
                return StringData;
            }

            set
            {
                StringData = value;
            }
        }

        #endregion Properties
    }
}
