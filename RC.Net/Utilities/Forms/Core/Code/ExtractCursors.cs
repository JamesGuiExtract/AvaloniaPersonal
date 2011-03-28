using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a container for mouse cursors common to Extract Systems applications.
    /// </summary>
    public static class ExtractCursors
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(ExtractCursors).ToString();

        #endregion Constants

        #region ExtractCursors Fields

        /// <summary>
        /// The cursor that appears when the mouse is over a rotational grip handle.
        /// </summary>
        static Cursor _rotate;

        /// <summary>
        /// The cursor that appears when the mouse is actively rotating a grip handle.
        /// </summary>
        static Cursor _activeRotate;

        /// <summary>
        /// The highlight cursor.
        /// </summary>
        static Cursor _highlight;

        /// <summary>
        /// The rectangular highlight cursor.
        /// </summary>
        static Cursor _rectangularHighlight;

        /// <summary>
        /// The word highlight cursor.
        /// </summary>
        static Cursor _wordHighlight;

        /// <summary>
        /// The delete cursor.
        /// </summary>
        static Cursor _delete;

        /// <summary>
        /// The edit text cursor.
        /// </summary>
        static Cursor _editText;

        /// <summary>
        /// The set highlight height cursor.
        /// </summary>
        static Cursor _setHeight;

        /// <summary>
        /// The pan cursor.
        /// </summary>
        static Cursor _pan;

        /// <summary>
        /// The cursor that appears during a pan event.
        /// </summary>
        static Cursor _activePan;

        /// <summary>
        /// The zoom window cursor.
        /// </summary>
        static Cursor _zoomWindow;

        #endregion ExtractCursors Fields

        #region ExtractCursors Properties

        /// <summary>
        /// Gets the cursor that appears when the mouse is over a rotational grip handle.
        /// </summary>
        /// <value>The cursor that appears when the mouse is over a rotational grip handle.</value>
        public static Cursor Rotate
        {
            get
            {
                if (_rotate == null)
                {
                    _rotate = GetCursor(typeof(ExtractCursors), "Resources.Rotate.cur");
                }

                return _rotate;
            }
        }

        /// <summary>
        /// Gets the cursor that appears when the mouse is actively rotating a grip handle.
        /// </summary>
        /// <value>The cursor that appears when the mouse is actively rotating a grip handle.
        /// </value>
        public static Cursor ActiveRotate
        {
            get
            {
                if (_activeRotate == null)
                {
                    _activeRotate = GetCursor(typeof(ExtractCursors), "Resources.ActiveRotate.cur");
                }

                return _activeRotate;
            }
        }

        /// <summary>
        /// Gets the highlighter cursor.
        /// </summary>
        /// <value>The highlighter cursor.</value>
        public static Cursor Highlight
        {
            get
            {
                if (_highlight == null)
                {
                    _highlight = GetCursor(typeof(ExtractCursors), "Resources.Highlight.cur");
                }

                return _highlight;
            }
        }

        /// <summary>
        /// Gets the rectangular highlight cursor.
        /// </summary>
        /// <value>The rectangular highlight cursor.</value>
        public static Cursor RectangularHighlight
        {
            get
            {
                if (_rectangularHighlight == null)
                {
                    _rectangularHighlight = GetCursor(typeof(ExtractCursors), 
                        "Resources.RectangularHighlight.cur");
                }

                return _rectangularHighlight;
            }
        }

        /// <summary>
        /// Gets the word highlighter cursor.
        /// </summary>
        /// <value>The word highlighter cursor.</value>
        public static Cursor WordHighlight
        {
            get
            {
                if (_wordHighlight == null)
                {
                    _wordHighlight = GetCursor(typeof(ExtractCursors),
                        "Resources.WordHighlight.cur");
                }

                return _wordHighlight;
            }
        }

        /// <summary>
        /// Gets the delete cursor.
        /// </summary>
        /// <value>The delete cursor.</value>
        public static Cursor Delete
        {
            get
            {
                if (_delete == null)
                {
                    _delete = GetCursor(typeof(ExtractCursors), "Resources.Delete.cur");
                }

                return _delete;
            }
        }

        /// <summary>
        /// Gets the edit text cursor.
        /// </summary>
        /// <value>The edit text cursor.</value>
        public static Cursor EditText
        {
            get
            {
                if (_editText == null)
                {
                    _editText = GetCursor(typeof(ExtractCursors), "Resources.EditText.cur");
                }

                return _editText;
            }
        }

        /// <summary>
        /// Gets the set height cursor.
        /// </summary>
        /// <value>The set height cursor.</value>
        public static Cursor SetHeight
        {
            get
            {
                if (_setHeight == null)
                {
                    _setHeight = GetCursor(typeof(ExtractCursors), "Resources.SetHeight.cur");
                }

                return _setHeight;
            }
        }

        /// <summary>
        /// Gets the pan cursor.
        /// </summary>
        /// <value>The pan cursor.</value>
        public static Cursor Pan
        {
            get
            {
                if (_pan == null)
                {
                    _pan = GetCursor(typeof(ExtractCursors), "Resources.Pan.cur");
                }

                return _pan;
            }
        }

        /// <summary>
        /// Gets the zoom window cursor.
        /// </summary>
        /// <value>The zoom window cursor.</value>
        public static Cursor ActivePan
        {
            get
            {
                if (_activePan == null)
                {
                    _activePan = GetCursor(typeof(ExtractCursors), "Resources.ActivePan.cur");
                }

                return _activePan;
            }
        }

        /// <summary>
        /// Gets the zoom window cursor.
        /// </summary>
        /// <value>The zoom window cursor.</value>
        public static Cursor ZoomWindow
        {
            get
            {
                if (_zoomWindow == null)
                {
                    _zoomWindow = GetCursor(typeof(ExtractCursors), "Resources.ZoomWindow.cur");
                }

                return _zoomWindow;
            }
        }

        #endregion ExtractCursors Properties

        #region ExtractCursors Methods

        /// <overloads>Creates a cursor object from the specified object(s).</overloads>
        /// <summary>
        /// Creates a cursor object from a stream of bytes.
        /// </summary>
        /// <param name="stream">A stream containing the cursor to load. Cannot be 
        /// <see langword="null"/>.</param>
        /// <returns>A <see cref="System.Windows.Forms.Cursor"/> loaded from the specified stream.
        /// </returns>
        public static Cursor GetCursor(Stream stream)
        {
            try
            {
                // Validate license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23140",
                    _OBJECT_NAME);

                ExtractException.Assert("ELI21217", "Stream must be specified.", stream != null);

                using (TemporaryFile tempFile = new TemporaryFile())
                {
                    // Write the stream to the temporary file
                    File.WriteAllBytes(tempFile.FileName,
                        StreamMethods.ConvertStreamToByteArray(stream));

                    // Load a cursor from the temporary file (using P/Invoke method)
                    IntPtr cursorHandle = NativeMethods.LoadCursorFromFile(tempFile.FileName);
                    if (cursorHandle == IntPtr.Zero)
                    {
                        throw new ExtractException("ELI21594", "Unable to load cursor handle.",
                            new Win32Exception(Marshal.GetLastWin32Error()));
                    }


                    Cursor cursor = new Cursor(cursorHandle);

                    // Return the new cursor
                    return cursor;
                }
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21207", "Unable to load cursor from stream.", e);
            }
        }

        /// <summary>
        /// Creates a cursor object from an embedded resource.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> whose namespace is used to scope the 
        /// cursor resource.</param>
        /// <param name="name">The case-sensitive name of the embedded cursor resource.</param>
        /// <returns>The cursor or <see langword="null"/> if the resource does not exist.
        /// </returns>
        public static Cursor GetCursor(Type type, string name)
        {
            try
            {
                // Validate license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23141",
                    _OBJECT_NAME);

                // Get the executing assembly
                Assembly thisAssembly = Assembly.GetExecutingAssembly();

                // Load the cursor from the resource stream
                using (Stream cursorStream = thisAssembly.GetManifestResourceStream(type, name))
                {
                    return cursorStream == null ? null : ExtractCursors.GetCursor(cursorStream);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23142", ex);
            }
        }

        #endregion ExtractCursors Methods
    }
}
