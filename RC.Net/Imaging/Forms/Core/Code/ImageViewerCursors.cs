using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a collection of mouse cursors associated with a particular
    /// <see cref="CursorTool"/>.
    /// </summary>
    struct ImageViewerCursors
    {
        /// <summary>
        /// The normal <see cref="Cursor"/> to be displayed by the
        /// <see cref="CursorTool"/>.
        /// </summary>
        Cursor _tool;

        /// <summary>
        /// The <see cref="Cursor"/> to be displayed by the
        /// <see cref="CursorTool"/> when it has been activated (i.e. mouse down).
        /// </summary>
        Cursor _active;

        /// <summary>
        /// Gets and sets the normal <see cref="Cursor"/>
        /// </summary>
        /// <returns>The normal <see cref="Cursor"/> to be displayed. <see langword="null"/> 
        /// if the default cursor should be used.</returns>
        /// <value>The normal <see cref="Cursor"/> to be displayed.</value>
        public Cursor Tool
        {
            get
            {
                return _tool;
            }
            set
            {
                _tool = value;
            }
        }

        /// <summary>
        /// Gets and sets the active <see cref="Cursor"/>
        /// </summary>
        /// <returns>The active <see cref="Cursor"/> to be displayed.</returns>
        /// <value>The active <see cref="Cursor"/> to be displayed.</value>
        public Cursor Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
            }
        }
    }
}