using Microsoft.SharePoint.Administration;
using System;
using System.Text;

namespace Extract.SharePoint
{
    /// <summary>
    /// Enumeration to indicate where redacted files should be placed when processed.
    /// </summary>
    internal enum IdShieldOutputLocation
    {
        ParallelFolderPrefix = 0,
        ParallelFolderSuffix = 1,
        Subfolder = 2,
        PrefixFilename = 3,
        SuffixFilename = 4,
        MirrorDocumentLibrary = 5
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Runtime.InteropServices.GuidAttribute("63360925-5783-448D-B968-F2685EA80D9F")]
    internal class IdShieldFolderProcessingSettings : FolderProcessingSettings
    {
        #region Fields

        /// <summary>
        /// The location in SharePoint that the redacted files should be placed
        /// </summary>
        [Persisted]
        int _outputLocation;

        /// <summary>
        /// The output location string (meaning of this string is related to _outputLocation)
        /// </summary>
        [Persisted]
        string _outputLocationString;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldFolderProcessingSettings"/> class.
        /// </summary>
        public IdShieldFolderProcessingSettings()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldFolderProcessingSettings"/> class.
        /// </summary>
        /// <param name="folderId">The folder id.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="recursive">if set to <see langword="true"/> [recursive].</param>
        /// <param name="added">if set to <see langword="true"/> [added].</param>
        /// <param name="modified">if set to <see langword="true"/> [modified].</param>
        /// <param name="location">The location.</param>
        /// <param name="locationString">The location string.</param>
        /// <param name="parent">The parent.</param>
        public IdShieldFolderProcessingSettings(Guid folderId, string extensions,
            bool recursive, bool added, bool modified, IdShieldOutputLocation location,
            string locationString, SPPersistedObject parent)
            : base(folderId, extensions, recursive, added, modified, parent)
        {
            _outputLocation = (int) location;
            _outputLocationString = locationString;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the id shield output location.
        /// </summary>
        /// <value>The id shield output location.</value>
        public IdShieldOutputLocation IdShieldOutputLocation
        {
            get
            {
                return (IdShieldOutputLocation) _outputLocation;
            }
        }

        /// <summary>
        /// Gets the output location string for the redacted file. This values
        /// meaning is based on the value of <see cref="IdShieldOutputLocation"/>.
        /// </summary>
        /// <value>The location string value associated with the
        /// <see cref="IdShieldOutputLocation"/></value>
        public string IdShieldOutputLocationString
        {
            get
            {
                return _outputLocationString ?? string.Empty;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Computes a human readable string version of the folder settings.
        /// </summary>
        /// <returns>
        /// A human readable stringized version of the folder settings.
        /// </returns>
        internal override string ComputeHumanReadableSettingString()
        {
            StringBuilder sb = new StringBuilder(base.ComputeHumanReadableSettingString());

            sb.Append("Output files to ");
            IdShieldOutputLocation outputLocation = (IdShieldOutputLocation)_outputLocation;
            switch (outputLocation)
            {
                case IdShieldOutputLocation.PrefixFilename:
                    sb.Append("the same folder with a file name prefixed with: ");
                    break;

                case IdShieldOutputLocation.SuffixFilename:
                    sb.Append("the same folder with a file name suffixed with: ");
                    break;

                case IdShieldOutputLocation.ParallelFolderPrefix:
                    sb.Append("a parallel folder of the same name prefixed with: ");
                    break;

                case IdShieldOutputLocation.ParallelFolderSuffix:
                    sb.Append("a parallel folder of the same name suffixed with: ");
                    break;

                case IdShieldOutputLocation.Subfolder:
                    sb.Append("a sub folder with the name: ");
                    break;

                case IdShieldOutputLocation.MirrorDocumentLibrary:
                    sb.Append("a mirrored document library called: ");
                    break;
            }
            sb.AppendLine(_outputLocationString);

            return sb.ToString();
        }

        #endregion Methods
    }
}
