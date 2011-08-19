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
        /// Gets and sets the normal <see cref="Cursor"/>
        /// </summary>
        /// <returns>The normal <see cref="Cursor"/> to be displayed. <see langword="null"/> 
        /// if the default cursor should be used.</returns>
        /// <value>The normal <see cref="Cursor"/> to be displayed.</value>
        public Cursor Tool
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the active <see cref="Cursor"/>
        /// </summary>
        /// <returns>The active <see cref="Cursor"/> to be displayed.</returns>
        /// <value>The active <see cref="Cursor"/> to be displayed.</value>
        public Cursor Active
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the <see cref="Cursor"/> to be displayed by the
        /// <see cref="CursorTool"/> when the shift key is down.
        /// </summary>
        /// <value>The <see cref="Cursor"/> to be displayed by the
        /// <see cref="CursorTool"/> when the shift key is down.</value>
        public Cursor ShiftState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the <see cref="Cursor"/> to be displayed by the
        /// <see cref="CursorTool"/> when the ctrl and shift keys are down.
        /// </summary>
        /// <value>The <see cref="Cursor"/> to be displayed by the
        /// <see cref="CursorTool"/> when the ctrl and shift keys are down.</value>
        public Cursor CtrlShiftState
        {
            get;
            set;
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
                if (Tool != null)
                {
                    Tool.Dispose();
                    Tool = null;
                }
               
                if (Active != null)
                {
                    Active.Dispose();
                    Active = null;
                }

                if (ShiftState != null)
                {
                    ShiftState.Dispose();
                    ShiftState = null;
                }

                if (CtrlShiftState != null)
                {
                    CtrlShiftState.Dispose();
                    CtrlShiftState = null;
                }
            }
        }

        #endregion IDisposable Members
    }
}