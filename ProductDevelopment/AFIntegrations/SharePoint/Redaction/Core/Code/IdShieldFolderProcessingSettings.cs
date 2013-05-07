using System;
using System.Runtime.Serialization;
using System.Text;

// Using statements to make dealing with folder settings more readable
using IdShieldFolderSettingsCollection =
System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.Redaction.IdShieldFolderProcessingSettings>>;
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.Redaction.IdShieldFolderProcessingSettings>;

namespace Extract.SharePoint.Redaction
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
    /// Class to hold the settings for the folder that will be processed.
    /// </summary>
    [Serializable]
    internal class IdShieldFolderProcessingSettings : FolderProcessingSettings
    {
        #region Constants

        /// <summary>
        /// Current version of the settings class.
        /// </summary>
        static readonly int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The location in SharePoint that the redacted files should be placed
        /// </summary>
        IdShieldOutputLocation _outputLocation;

        /// <summary>
        /// The output location string (meaning of this string is related to _outputLocation)
        /// </summary>
        string _outputLocationString;

        #endregion Fields

        #region Constuctors

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderProcessingSettings"/> class.
        /// </summary>
        public IdShieldFolderProcessingSettings() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderProcessingSettings"/> class.
        /// </summary>
        /// <param name="listId">The unique ID for the parent list of the watched folder.</param>
        /// <param name="folderId">The unique ID for the folder.</param>
        /// <param name="folderPath">The path of the folder to watch.</param>
        /// <param name="fileExtensions">The list of file extensions to watch for.</param>
        /// <param name="recurse">Whether to recursively watch subfolders or not.</param>
        /// <param name="reprocess">Reprocess processed files.</param>
        /// <param name="added">Whether to watch files that are added.</param>
        /// <param name="processExisting">Whether existing files should be processed.</param>
        /// <param name="outputLocation">The output location for the redacted file.</param>
        /// <param name="outputLocationString">The output location for the redacted file
        /// (this value's meaning is determined by <paramref name="outputLocation"/>.</param>
        /// <param name="queueOnValue">Flag to indicate if a field value should be checked to determine 
        /// if file should be queued</param>
        /// <param name="nameOfValueField">Name of field to check for specified value</param>
        /// <param name="valueToQueueOn">Value of field to determine file should be queued</param>
        public IdShieldFolderProcessingSettings(Guid listId, Guid folderId, string folderPath,
            string fileExtensions, bool recurse, bool reprocess, bool added, bool processExisting,
            IdShieldOutputLocation outputLocation, string outputLocationString,
            bool queueOnValue, string nameOfValueField, string valueToQueueOn)
            : this(listId, folderId, folderPath, fileExtensions, recurse, reprocess, added,
            false, processExisting, outputLocation, outputLocationString, 
            queueOnValue, nameOfValueField, valueToQueueOn)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderProcessingSettings"/> class.
        /// </summary>
        /// <param name="listId">The unique ID for the parent list of the watched folder.</param>
        /// <param name="folderId">The unique ID for the folder.</param>
        /// <param name="folderPath">The path of the folder to watch.</param>
        /// <param name="fileExtensions">The list of file extensions to watch for.</param>
        /// <param name="recurse">Whether to recursively watch subfolders or not.</param>
        /// <param name="reprocess">Reprocess processed files.</param>
        /// <param name="added">Whether to watch files that are added.</param>
        /// <param name="modified">Whether to watch files that are modified.</param>
        /// <param name="processExisting">Whether existing files should be processed.</param>
        /// <param name="outputLocation">The output location for the redacted file.</param>
        /// <param name="outputLocationString">The output location for the redacted file
        /// (this value's meaning is determined by <paramref name="outputLocation"/>.</param>
        /// <param name="queueOnValue">Flag to indicate if a field value should be checked to determine 
        /// if file should be queued</param>
        /// <param name="fieldForQueuing">Name of field to check for specified value</param>
        /// <param name="valueToQueueOn">Value of field to determine file should be queued</param>
        public IdShieldFolderProcessingSettings(Guid listId, Guid folderId, string folderPath,
            string fileExtensions, bool recurse, bool reprocess, bool added, bool modified,
            bool processExisting, IdShieldOutputLocation outputLocation, string outputLocationString,
            bool queueOnValue, string fieldForQueuing, string valueToQueueOn)
            : base(listId, folderId, folderPath, fileExtensions, recurse, reprocess, added,
            modified, processExisting, queueOnValue, fieldForQueuing, valueToQueueOn)
        {
            _outputLocation = outputLocation;
            _outputLocationString = outputLocationString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderProcessingSettings"/> class
        /// from a serialized version of the class.
        /// </summary>
        /// <param name="info">The serialization info to read from.</param>
        /// <param name="context">The context for the serialization stream.</param>
        protected IdShieldFolderProcessingSettings(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version = info.GetInt32("IdShieldCurrentVersion");
            if (version > _CURRENT_VERSION)
            {
                var ex = new FormatException("Unrecognized object version.");
                ex.Data["VersionLoaded"] = version;
                ex.Data["MaxVersion"] = _CURRENT_VERSION;
                throw ex;
            }

            _outputLocation = (IdShieldOutputLocation)Enum.Parse(typeof(IdShieldOutputLocation),
                info.GetString("OutputLocation"));
            _outputLocationString = info.GetString("OutputLocationString");
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the output location for the for the redacted file.
        /// </summary>
        public IdShieldOutputLocation OutputLocation
        {
            get
            {
                return _outputLocation;
            }
        }

        /// <summary>
        /// Gets the output location string for the redacted file. This values
        /// meaning is based on the value of <see cref="OutputLocation"/>.
        /// </summary>
        public string OutputLocationString
        {
            get
            {
                return _outputLocationString;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Deserializes the folder settings collection and returns the folder settings
        /// collection for the specified site.
        /// </summary>
        /// <param name="settings">The settings string to deserialize.</param>
        /// <param name="siteId">The site ID to get settings for..</param>
        /// <returns>The folder settings collection for the specified server relative site
        /// location.</returns>
        internal new static SiteFolderSettingsCollection
            DeserializeFolderSettings(string settings, Guid siteId)
        {
            var value = settings.DeserializeFromHexString<IdShieldFolderSettingsCollection>();
            SiteFolderSettingsCollection result;
            if (value.TryGetValue(siteId, out result))
            {
                return result;
            }

            return new SiteFolderSettingsCollection();
        }

        /// <summary>
        /// Computes a human readable string version of the folder settings.
        /// </summary>
        /// <returns>A human readable stringized version of the folder settings.</returns>
        public override string ComputeHumanReadableSettingString()
        {
            var sb = new StringBuilder(base.ComputeHumanReadableSettingString());

            sb.Append("Output files to ");
            switch (_outputLocation)
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

        #region ISerializable Members

        /// <summary>
        /// Gets the current settings as object as a serialization stream.
        /// </summary>
        /// <param name="info">The serialization info to write to.</param>
        /// <param name="context">The context for the serialization stream.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("IdShieldCurrentVersion", _CURRENT_VERSION);
            info.AddValue("OutputLocation", _outputLocation.ToString("G"));
            info.AddValue("OutputLocationString", _outputLocationString);
        }

        #endregion
    }
}
