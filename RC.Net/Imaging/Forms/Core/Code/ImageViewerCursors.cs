using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a collection of mouse cursors associated with a particular
    /// <see cref="CursorTool"/>.
    /// <para>Note:</para>
    /// Only call the dispose method of this class if the <see cref="Cursor"/>
    /// objects maintained by this class are not shared.
    /// </summary>
    internal class ImageViewerCursors : IDisposable
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

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ImageViewerCursors"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ImageViewerCursors"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ImageViewerCursors"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_tool != null)
                {
                    try
                    {
                        _tool.Dispose();
                    }
                    catch (Exception ex)
                    {
                        ExtractException.Log("ELI30254", ex);
                    }
                    _tool = null;
                }
                if (_active != null)
                {
                    try
                    {
                        _active.Dispose();
                    }
                    catch (Exception ex)
                    {
                        ExtractException.Log("ELI30255", ex);
                    }
                    _active = null;
                }
            }
        }

        #endregion IDisposable Members
    }
}