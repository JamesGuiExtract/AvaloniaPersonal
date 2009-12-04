using System;

namespace Extract.FileActionManager.Forms
{	
    /// <summary>
    /// Provides data for the <see cref="FileTagDropDown.FileTagClicked"/> event.
    /// </summary>
    public class FileTagClickedEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the file tag that was clicked.
        /// </summary>
        readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTagClickedEventArgs"/> class.
        /// </summary>
        /// <param name="name">The name of the file tag that was clicked.</param>
        public FileTagClickedEventArgs(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets the name of the file tag that was clicked.
        /// </summary>
        /// <value>The name of the file tag that was clicked.</value>
        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}