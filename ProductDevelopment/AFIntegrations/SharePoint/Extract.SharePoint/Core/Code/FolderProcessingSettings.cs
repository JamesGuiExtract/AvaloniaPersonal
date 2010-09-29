using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

// Using statements to make dealing with folder settings more readable
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>;
using IdShieldFolderSettingsCollection =
System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>>;

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
    /// Enumeration indicating what type of events that the receiver should listen for.
    /// </summary>
    [Flags]
    internal enum FileEventType
    {
        FileNone = 0x0,

        FileAdded = 0x1,

        FileModified = 0x2
    }

    /// <summary>
    /// Class to hold the settings for the folder that will be processed.
    /// </summary>
    [Serializable]
    internal class FolderProcessingSettings : ISerializable
    {
        #region Constants

        /// <summary>
        /// Current version of the settings class.
        /// </summary>
        static readonly int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The path of the folder to watch (relative to the SPWeb root)
        /// </summary>
        string _folderPath;

        /// <summary>
        /// Semicolon separeted list of file extensions to watch.
        /// </summary>
        string _fileExtensions;

        /// <summary>
        /// List of the file extensions to watch (parsed from the _fileExtensions value)
        /// </summary>
        List<string> _fileExtensionList = new List<string>();

        /// <summary>
        /// Whether folders should be watched recursively
        /// </summary>
        bool _recurse;

        /// <summary>
        /// The location in SharePoint that the redacted files should be placed
        /// </summary>
        IdShieldOutputLocation _outputLocation;

        /// <summary>
        /// The output location string (meaning of this string is related to _outputLocation)
        /// </summary>
        string _outputLocationString;

        /// <summary>
        /// The event types to listen for
        /// </summary>
        FileEventType _eventType;

        #endregion Fields

        #region Constuctors

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderProcessingSettings"/> class.
        /// </summary>
        public FolderProcessingSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderProcessingSettings"/> class.
        /// </summary>
        /// <param name="folderPath">The path of the folder to watch.</param>
        /// <param name="fileExtensions">The list of file extensions to watch for.</param>
        /// <param name="recurse">Whether to recursively watch subfolders or not.</param>
        /// <param name="added">Whether to watch files that are added.</param>
        /// <param name="modified">Whether to watch files that are modified.</param>
        /// <param name="outputLocation">The output location for the redacted file.</param>
        /// <param name="outputLocationString">The output location for the redacted file
        /// (this value's meaning is determined by <paramref name="outputLocation"/>.</param>
        public FolderProcessingSettings(string folderPath, string fileExtensions,
            bool recurse, bool added, bool modified, IdShieldOutputLocation outputLocation,
            string outputLocationString)
        {
            _folderPath = folderPath;
            FileExtensions = fileExtensions;
            _recurse = recurse;
            ProcessAddedFiles = added;
            ProcessModifiedFiles = modified;
            _outputLocation = outputLocation;
            _outputLocationString = outputLocationString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderProcessingSettings"/> class
        /// from a serialized version of the class.
        /// </summary>
        /// <param name="info">The serialization info to read from.</param>
        /// <param name="context">The context for the serialization stream.</param>
        protected FolderProcessingSettings(SerializationInfo info, StreamingContext context)
        {
            int version = info.GetInt32("CurrentVersion");
            if (version > _CURRENT_VERSION)
            {
                var ex = new FormatException("Unrecognized object version.");
                ex.Data["VersionLoaded"] = version;
                ex.Data["MaxVersion"] = _CURRENT_VERSION;
                throw ex;
            }

            _folderPath = info.GetString("FolderPath");
            FileExtensions = info.GetString("FileExtensions");
            _recurse = info.GetBoolean("Recurse");
            ProcessAddedFiles = info.GetBoolean("Added");
            ProcessModifiedFiles = info.GetBoolean("Modified");
            _outputLocation = (IdShieldOutputLocation)Enum.Parse(typeof(IdShieldOutputLocation),
                info.GetString("OutputLocation"));
            _outputLocationString = info.GetString("OutputLocationString");
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the file extensions to watch for.
        /// </summary>
        public string FileExtensions
        {
            get
            {
                return _fileExtensions;
            }
            set
            {
                _fileExtensions = value ?? string.Empty;
                _fileExtensions = _fileExtensions.Replace(" ", "");
                UpdateFileExtensionList();
            }
        }

        /// <summary>
        /// Gets whether subfolders should be recursively searched.
        /// </summary>
        public bool RecurseSubfolders
        {
            get
            {
                return _recurse;
            }
        }

        /// <summary>
        /// Gets/sets whether added files should be processed.
        /// </summary>
        public bool ProcessAddedFiles
        {
            get
            {
                return (_eventType & FileEventType.FileAdded) == FileEventType.FileAdded;
            }
            set
            {
                if (value)
                {
                    _eventType |= FileEventType.FileAdded;
                }
                else
                {
                    _eventType &= ~FileEventType.FileAdded;
                }
            }
        }

        /// <summary>
        /// Gets/sets whether modified files should be processed.
        /// </summary>
        public bool ProcessModifiedFiles
        {
            get
            {
                return (_eventType & FileEventType.FileModified) == FileEventType.FileModified;
            }
            set
            {
                if (value)
                {
                    _eventType |= FileEventType.FileModified;
                }
                else
                {
                    _eventType &= ~FileEventType.FileModified;
                }
            }
        }

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

        /// <summary>
        /// Gets/sets the event types to listen for.
        /// </summary>
        public FileEventType EventTypes
        {
            get
            {
                return _eventType;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the file extension list based on the current value of
        /// <see cref="FileExtensions"/>.
        /// </summary>
        void UpdateFileExtensionList()
        {
            _fileExtensionList.AddRange(_fileExtensions.Split(
                new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Checks whether the specified filename matches the current list of file
        /// extensions.
        /// </summary>
        /// <param name="fileName">The filename to check.</param>
        /// <returns><see langword="true"/> if the filename matches
        /// the current file extension list and <see langword="false"/> if it
        /// does not match.</returns>
        internal bool DoesFileMatchPattern(string fileName)
        {
            foreach (string pattern in _fileExtensionList)
            {
                if (StringType.StrLikeText(fileName, pattern))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Helper method that takes a collection of <see cref="FolderProcessingSettings"/>
        /// objects and serializes them to a string of hex digits.
        /// </summary>
        /// <param name="settings">The collection to serialize.</param>
        /// <returns>A string of hex digits representing a binary serialization of
        /// the collection passed in.</returns>
        internal static string SerializeFolderSettings(IdShieldFolderSettingsCollection settings)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, settings);
                string result = BitConverter.ToString(stream.ToArray());
                result = result.Replace("-", "");

                return result;
            }
        }

        /// <summary>
        /// Helper method that takes a hex string serialization of a collection of
        /// <see cref="FolderProcessingSettings"/> objects and deserializes them
        /// back to a collection.
        /// </summary>
        /// <param name="settings">A string of hex digits representing a binary serialization
        /// of a collection of <see cref="FolderProcessingSettings"/>.</param>
        /// <returns>The deserialized collection of <see cref="FolderProcessingSettings"/>.
        /// </returns>
        internal static IdShieldFolderSettingsCollection
            DeserializeFolderSettings(string settings)
        {
            // If the string is empty just return an empty dictionary
            if (string.IsNullOrEmpty(settings))
            {
                return new IdShieldFolderSettingsCollection();
            }

            int length = settings.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(settings.Substring(i, 2), 16);
            }

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                IdShieldFolderSettingsCollection folderSettings =
                    (IdShieldFolderSettingsCollection)serializer.Deserialize(stream);

                return folderSettings;
            }
        }

        /// <summary>
        /// Deserializes the folder settings collection and returns the folder settings
        /// collection for the specified site.
        /// </summary>
        /// <param name="settings">The settings string to deserialize.</param>
        /// <param name="siteId">The site ID to get settings for..</param>
        /// <returns>The folder settings collection for the specified server relative site
        /// location.</returns>
        internal static SiteFolderSettingsCollection
            DeserializeFolderSettings(string settings, Guid siteId)
        {
            IdShieldFolderSettingsCollection value = DeserializeFolderSettings(settings);
            SiteFolderSettingsCollection result;
            if (value.TryGetValue(siteId, out result))
            {
                return result;
            }

            return new SiteFolderSettingsCollection();
        }

        #endregion Methods

        #region ISerializable Members

        /// <summary>
        /// Gets the current settings as object as a serialization stream.
        /// </summary>
        /// <param name="info">The serialization info to write to.</param>
        /// <param name="context">The context for the serialization stream.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CurrentVersion", _CURRENT_VERSION);
            info.AddValue("FolderPath", _folderPath);
            info.AddValue("FileExtensions", _fileExtensions);
            info.AddValue("Recurse", _recurse);
            info.AddValue("Added", ProcessAddedFiles);
            info.AddValue("Modified", ProcessModifiedFiles);
            info.AddValue("OutputLocation", _outputLocation.ToString("G"));
            info.AddValue("OutputLocationString", _outputLocationString);
        }

        #endregion
    }
}
