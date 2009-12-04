using System;

namespace Extract.FileActionManager.Forms
{	
    /// <summary>
    /// Provides data for the <see cref="FileTagDropDown.FileTagAdded"/> event.
    /// </summary>
    public class FileTagAddedEventArgs : EventArgs
    {
        /// <summary>
        /// The file tag that was added.
        /// </summary>
        readonly FileTag _fileTag;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTagAddedEventArgs"/> class.
        /// </summary>
        /// <param name="fileTag">The file tag that was added.</param>
        public FileTagAddedEventArgs(FileTag fileTag)
        {
            _fileTag = fileTag;
        }

        /// <summary>
        /// Gets the file tag that was added.
        /// </summary>
        /// <value>The file tag that was added.</value>
        public FileTag FileTag
        {
            get
            {
                return _fileTag;
            }
        }
    }
}