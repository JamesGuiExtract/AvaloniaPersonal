using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.SharePoint
{
    /// <summary>
    /// Enumeration indicating what type of events that the receiver should listen for.
    /// </summary>
    [Flags]
    [System.Runtime.InteropServices.GuidAttribute("54372C02-B371-497E-8F17-91187F1DF0B3")]
    internal enum FileEventType
    {
        FileNone = 0x0,

        FileAdded = 0x1,

        FileModified = 0x2,

        FileAddAndModify = FileAdded | FileModified
    }

    /// <summary>
    /// Class to hold the settings for the folder that will be processed.
    /// </summary>
    [System.Runtime.InteropServices.GuidAttribute("68CF2931-ECA7-4962-BD0A-04812042E8D1")]
    internal class FolderProcessingSettings : SPPersistedObject
    {
        #region Fields

        /// <summary>
        /// The unique ID for the watched folder.
        /// </summary>
        [Persisted]
        Guid _folderId;

        /// <summary>
        /// Semicolon separeted list of file extensions to watch.
        /// </summary>
        [Persisted]
        string _fileExtensions;

        /// <summary>
        /// List of the file extensions to watch (parsed from the _fileExtensions value)
        /// </summary>
        [Persisted]
        List<string> _fileExtensionList;

        /// <summary>
        /// Whether folders should be watched recursively
        /// </summary>
        [Persisted]
        bool _recurse;

        /// <summary>
        /// The event types to listen for
        /// </summary>
        [Persisted]
        FileEventType _eventType;

        #endregion Fields

        #region Constuctors

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderProcessingSettings"/> class.
        /// </summary>
        public FolderProcessingSettings()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderProcessingSettings"/> class.
        /// </summary>
        /// <param name="folderId">The folder id.</param>
        /// <param name="fileExtensions">The file extensions to watch for.</param>
        /// <param name="recursive">If watching should be recursive.</param>
        /// <param name="added">If watching added events.</param>
        /// <param name="modified">If watching modified events.</param>
        /// <param name="parent">The parent.</param>
        protected FolderProcessingSettings(Guid folderId, string fileExtensions, bool recursive,
            bool added, bool modified, SPPersistedObject parent)
            : base(Guid.NewGuid().ToString("N"), parent)
        {
            _folderId = folderId;
            FileExtensions = fileExtensions;
            _recurse = recursive;
            ProcessAddedFiles = added;
            ProcessModifiedFiles = modified;
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
                return _fileExtensions ?? string.Empty;
            }
            set
            {
                _fileExtensions = value ?? string.Empty;
                _fileExtensions = _fileExtensions.Replace(" ", "");
                UpdateFileExtensionList();
            }
        }

        /// <summary>
        /// Gets or sets whether subfolders should be recursively searched.
        /// </summary>
        public bool RecurseSubfolders
        {
            get
            {
                return _recurse;
            }
        }

        /// <summary>
        /// Gets or sets whether added files should be processed.
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
        /// Gets or sets whether modified files should be processed.
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
        /// Gets the event types to listen for.
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
        /// Indicates whether this persisted class has additional update access
        /// </summary>
        /// <returns><see langword="true"/></returns>
        protected override bool HasAdditionalUpdateAccess()
        {
            return true;
        }

        /// <summary>
        /// Updates the file extension list based on the current value of
        /// <see cref="FileExtensions"/>.
        /// </summary>
        void UpdateFileExtensionList()
        {
            if (_fileExtensionList == null)
            {
                _fileExtensionList = new List<string>();
            }
            else
            {
                _fileExtensionList.Clear();
            }

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
            if (_fileExtensionList != null)
            {
                foreach (string pattern in _fileExtensionList)
                {
                    if (StringType.StrLikeText(fileName, pattern))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the folder path.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <returns>Either the path to the folder/list or <see cref="String.Empty"/> if
        /// the item does not exist.</returns>
        internal string GetFolderPath(SPWeb web)
        {
            string folderPath = string.Empty; 
            try
            {
                SPFolder folder = web.GetFolder(_folderId);
                if (folder.Exists)
                {
                    folderPath = folder.ServerRelativeUrl;
                }

                // Its not a folder, try getting it as a list
                if (string.IsNullOrEmpty(folderPath))
                {
                    SPList list = web.Lists[_folderId];
                    folderPath = list.RootFolder.ServerRelativeUrl;
                }
            }
            catch
            {
            }

            return folderPath;
        }

        /// <summary>
        /// Computes a human readable string version of the folder settings.
        /// </summary>
        /// <returns>A human readable stringized version of the folder settings.</returns>
        virtual internal string ComputeHumanReadableSettingString()
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
            else
            {
                sb.Replace("Watching for ", "Not watching for any events");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion Methods
    }
}
