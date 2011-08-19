using Extract.Licensing;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
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
        /// Maintains references to all created cursors by resource name so that no cursor gets
        /// loaded more than once.
        /// </summary>
        static ConcurrentDictionary<string, Cursor> _loadedCursors =
            new ConcurrentDictionary<string, Cursor>();

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
                return GetCursor("Resources.Rotate.cur");
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
                return GetCursor("Resources.ActiveRotate.cur");
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
                return GetCursor("Resources.Highlight.cur");
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
                return GetCursor("Resources.RectangularHighlight.cur");
            }
        }

        /// <summary>
        /// Gets the rectangular highlight cursor for the shift (line fit) state.
        /// </summary>
        /// <value>The rectangular highlight cursor for the shift (line fit) state.</value>
        public static Cursor ShiftRectangularHighlight
        {
            get
            {
                return GetCursor("Resources.ShiftRectangularHighlight.cur");
            }
        }

        /// <summary>
        /// Gets the rectangular highlight cursor for the ctrl-shift (block fit) state.
        /// </summary>
        /// <value>The rectangular highlight cursor for the ctrl-shift (block fit) state.</value>
        public static Cursor CtrlShiftRectangularHighlight
        {
            get
            {
                return GetCursor("Resources.CtrlShiftRectangularHighlight.cur");
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
                return GetCursor("Resources.WordHighlight.cur");
            }
        }

        /// <summary>
        /// Gets the word highlight cursor for the shift (auto fit) state.
        /// </summary>
        /// <value>The word highlight cursor for the shift (auto fit) state.</value>
        public static Cursor ShiftWordHighlight
        {
            get
            {
                return GetCursor("Resources.ShiftWordHighlight.cur");
            }
        }

        /// <summary>
        /// Gets the word redaction cursor.
        /// </summary>
        /// <value>The word redaction cursor.</value>
        public static Cursor WordRedaction
        {
            get
            {
                return GetCursor("Resources.WordRedaction.cur");
            }
        }

        /// <summary>
        /// Gets the word redaction cursor in the shift (auto fit) state.
        /// </summary>
        /// <value>The word redaction cursor in the shift (auto fit) state.</value>
        public static Cursor ShiftWordRedaction
        {
            get
            {
                return GetCursor("Resources.ShiftWordRedaction.cur");
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
                return GetCursor("Resources.Delete.cur");
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
                return GetCursor("Resources.EditText.cur");
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
                return GetCursor("Resources.SetHeight.cur");
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
                return GetCursor("Resources.Pan.cur");
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
                return GetCursor("Resources.ActivePan.cur");
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
                return GetCursor("Resources.ZoomWindow.cur");
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

                using (TemporaryFile tempFile = new TemporaryFile(false))
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
        /// <param name="name">The case-sensitive name of the embedded cursor resource.</param>
        /// <returns>The cursor or <see langword="null"/> if the resource does not exist.
        /// </returns>
        public static Cursor GetCursor(string name)
        {
            try
            {
                // Validate license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23141",
                    _OBJECT_NAME);

                Cursor cursor;
                if (!_loadedCursors.TryGetValue(name, out cursor))
                {
                    // Get the executing assembly
                    Assembly thisAssembly = Assembly.GetExecutingAssembly();

                    // Load the cursor from the resource stream
                    using (Stream cursorStream =
                        thisAssembly.GetManifestResourceStream(typeof(ExtractCursors), name))
                    {
                        cursor = (cursorStream == null ? null : ExtractCursors.GetCursor(cursorStream));
                    }

                    _loadedCursors[name] = cursor;
                }

                return cursor;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23142", ex);
            }
        }

        #endregion ExtractCursors Methods
    }
}
