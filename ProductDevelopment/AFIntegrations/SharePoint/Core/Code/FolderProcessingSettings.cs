using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

// Using statements to make dealing with folder settings more readable
using FolderSettingsCollection =
System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>>;
using SiteFolderSettingsCollection =
System.Collections.Generic.SortedDictionary<string, Extract.SharePoint.FolderProcessingSettings>;

namespace Extract.SharePoint
{
    /// <summary>
    /// Enumeration indicating what type of events that the receiver should listen for.
    /// </summary>
    [Flags]
    public enum FileEventType
    {
        /// <summary>
        /// Indicates no file events to watch
        /// </summary>
        FileNone = 0x0,

        /// <summary>
        /// Indicates watching for file added events
        /// </summary>
        FileAdded = 0x1,

        /// <summary>
        /// Indicates watching for file modified events
        /// </summary>
        FileModified = 0x2,

        /// <summary>
        /// Indicates watching for both added and modified events.
        /// </summary>
        FileAddAndModify = FileAdded | FileModified
    }

    /// <summary>
    /// Class to hold the settings for the folder that will be processed.
    /// </summary>
    [Serializable]
    public class FolderProcessingSettings : ISerializable
    {
        #region Constants

        /// <summary>
        /// Current version of the settings class.
        /// <para>Versions:</para>
        /// Version 2 - Added FolderId field.
        /// Version 3 - Added ListId field.
        /// Version 4 - Added Reprocess and ProcessExisting fields.
        /// </summary>
        static readonly int _CURRENT_VERSION = 4;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The folder Id.
        /// </summary>
        Guid _folderId;

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
        /// The event types to listen for
        /// </summary>
        FileEventType _eventType;

        /// <summary>
        /// The parent list id.
        /// </summary>
        Guid _listId;

        /// <summary>
        /// Indicates whether previously processed files should be processed again.
        /// </summary>
        bool _reprocess;

        /// <summary>
        /// Indicates whether existing files should be processed when turning folder watching on.
        /// </summary>
        bool _processExisting;

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
        /// <param name="listId">The unique ID for the parent list of the watched folder.</param>
        /// <param name="folderId">The unique ID for the folder.</param>
        /// <param name="folderPath">The path of the folder to watch.</param>
        /// <param name="fileExtensions">The list of file extensions to watch for.</param>
        /// <param name="recurse">Whether to recursively watch subfolders or not.</param>
        /// <param name="reprocess">Reprocess processed files.</param>
        /// <param name="added">Whether to watch files that are added.</param>
        /// <param name="processExisting">Whether existing files should be processed.</param>
        public FolderProcessingSettings(Guid listId, Guid folderId, string folderPath,
            string fileExtensions, bool recurse, bool reprocess, bool added, bool processExisting)
            : this(listId, folderId, folderPath, fileExtensions, recurse, reprocess, added,
            false, processExisting)
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
        public FolderProcessingSettings(Guid listId, Guid folderId, string folderPath,
            string fileExtensions, bool recurse, bool reprocess, bool added, bool modified,
            bool processExisting)
        {
            _listId = listId;
            _folderId = folderId;
            _folderPath = folderPath;
            FileExtensions = fileExtensions;
            _recurse = recurse;
            ProcessAddedFiles = added;
            ProcessModifiedFiles = modified;
            _reprocess = reprocess;
            _processExisting = processExisting;
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

            _folderId = Guid.Empty;
            _listId = Guid.Empty;
            if (version >= 2)
            {
                _folderId = (Guid)info.GetValue("FolderId", typeof(Guid));
            }
            if (version >= 3)
            {
                _listId = (Guid)info.GetValue("ListId", typeof(Guid));
            }
            if (version >= 4)
            {
                _reprocess = info.GetBoolean("Reprocess");
                _processExisting = info.GetBoolean("ProcessExisting");
            }

            _folderPath = info.GetString("FolderPath");
            FileExtensions = info.GetString("FileExtensions");
            _recurse = info.GetBoolean("Recurse");
            ProcessAddedFiles = info.GetBoolean("Added");
            ProcessModifiedFiles = info.GetBoolean("Modified");
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the parent list id.
        /// </summary>
        /// <value>The parent list id.</value>
        public Guid ListId
        {
            get
            {
                return _listId;
            }
        }

        /// <summary>
        /// Gets the folder id.
        /// </summary>
        /// <value>The folder id.</value>
        public Guid FolderId
        {
            get
            {
                return _folderId;
            }
        }

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
        /// Gets/sets the event types to listen for.
        /// </summary>
        public FileEventType EventTypes
        {
            get
            {
                return _eventType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="FolderProcessingSettings"/> is reprocess.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if reprocess; otherwise, <see langword="false"/>.
        /// </value>
        public bool Reprocess
        {
            get
            {
                return _reprocess;
            }
        }

        /// <summary>
        /// Gets a value indicating whether process existing.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if process existing; otherwise, <see langword="false"/>.
        /// </value>
        public bool ProcessExisting
        {
            get
            {
                return _processExisting;
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
        public bool DoesFileMatchPattern(string fileName)
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
        /// Computes a human readable string version of the folder settings.
        /// </summary>
        /// <returns>A human readable stringized version of the folder settings.</returns>
        public virtual string ComputeHumanReadableSettingString()
        {
            StringBuilder sb = new StringBuilder();
            if (_recurse)
            {
                sb.Append("Recursively watching ");
            }
            else
            {
                sb.Append("Watching ");
            }
            sb.AppendLine("folder for files with extensions: ");
            sb.AppendLine(_fileExtensions);
            if (_reprocess)
            {
                sb.AppendLine("Reprocessing already processed files");
            }
            if (_eventType != FileEventType.FileNone)
            {
                sb.Append("Watching for ");
                if (_eventType == FileEventType.FileAddAndModify)
                {
                    sb.AppendLine("added and modified events");
                }
                else if (_eventType == FileEventType.FileAdded)
                {
                    sb.AppendLine("added events");
                }
                else if (_eventType == FileEventType.FileModified)
                {
                    sb.AppendLine("modified events");
                }

                if (!_processExisting)
                {
                    sb.AppendLine("Not processing existing files");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Deserializes the folder settings collection and returns the folder settings
        /// collection for the specified site.
        /// </summary>
        /// <param name="settings">The settings string to deserialize.</param>
        /// <param name="siteId">The site ID to get settings for..</param>
        /// <returns>The folder settings collection for the specified server relative site
        /// location.</returns>
        public static SiteFolderSettingsCollection
            DeserializeFolderSettings(string settings, Guid siteId)
        {
            var value = settings.DeserializeFromHexString<FolderSettingsCollection>();
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
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CurrentVersion", _CURRENT_VERSION);
            info.AddValue("FolderId", _folderId, typeof(Guid));
            info.AddValue("ListId", _listId, typeof(Guid));
            info.AddValue("FolderPath", _folderPath);
            info.AddValue("FileExtensions", _fileExtensions);
            info.AddValue("Recurse", _recurse);
            info.AddValue("Added", ProcessAddedFiles);
            info.AddValue("Modified", ProcessModifiedFiles);
            info.AddValue("Reprocess", _reprocess);
            info.AddValue("ProcessExisting", _processExisting);
        }

        #endregion
    }
}
