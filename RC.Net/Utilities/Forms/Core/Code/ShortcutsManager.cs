using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents the method that is called when a shortcut key is found and processed by the
    /// <see cref="ShortcutsManager.ProcessKey"/> method.
    /// </summary>
    public delegate void ShortcutHandler();

    /// <summary>
    /// Manages a collection of shortcut <see cref="Keys"/> and <see cref="ShortcutHandler"/> 
    /// classes.
    /// </summary>
    public class ShortcutsManager
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(ShortcutsManager).ToString();

        #endregion Constants

        #region Shortcuts Manager Fields

        /// <summary>
        /// A dictionary of shortcut <see cref="Keys"/> mapped to <see cref="ShortcutHandler"/> 
        /// classes.
        /// </summary>
        private Dictionary<Keys, ShortcutHandler> _shortcuts;

        #endregion

        #region Shortcuts Manager Event Handlers

        /// <summary>
        /// Occurs when a <see cref="ShortcutHandler"/> is specified for a particular shortcut
        /// <see cref="Keys"/> value.
        /// </summary>
        /// <seealso cref="ShortcutsManager.this"/>
        /// <seealso cref="ShortcutsManager.Clear"/>
        public event EventHandler<ShortcutKeyChangedEventArgs> ShortcutKeyChanged;

        /// <summary>
        /// Raised when a registered shortcut key or key combo is entered, before calling the registered
        /// ShortcutHandler.
        /// </summary>
        public event EventHandler<EventArgs> ProcessingShortcut;

        #endregion

        #region Shortcuts Manager Constructors

        /// <summary>
        /// Creates a ShortcutsManager where all shortcut <see cref="Keys"/> map to a 
        /// <see langword="null"/> <see cref="ShortcutHandler"/>.
        /// </summary>
        public ShortcutsManager()
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23155",
                    _OBJECT_NAME);

                _shortcuts = new Dictionary<Keys, ShortcutHandler>();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23156", ex);
            }
        }

        #endregion

        #region Shortcut Manager Properties

        /// <summary>
        /// Gets or sets the <see cref="ShortcutHandler"/> associated with the specified 
        /// <see cref="Keys"/> value.
        /// </summary>
        /// <param name="key">The key of the <see cref="ShortcutHandler"/> to get or set. Cannot be 
        /// <see cref="Keys.None"/></param>
        /// <value>The <see cref="ShortcutHandler"/> associated with the specified key. May be 
        /// <see langword="null"/> to remove a key from being handled by the 
        /// <see cref="ShortcutsManager"/>.</value>
        /// <returns>The <see cref="ShortcutHandler"/> associated with the specified 
        /// <see cref="Keys"/> value. If no <see cref="ShortcutHandler"/> is associated with the 
        /// specified key, <see langword="null"/> is returned.</returns>
        /// <event cref="ShortcutKeyChanged">Raised for each successful set operation.</event>
        /// <exception cref="ExtractException"><paramref name="key"/> is <see cref="Keys.None"/>
        /// </exception>
        public ShortcutHandler this[Keys key]
        {
            get
            {
                // Return the shortcut handler for the specified key.
                ShortcutHandler shortcutHandler;
                return _shortcuts.TryGetValue(key, out shortcutHandler) ? shortcutHandler : null;
            }
            set
            {
                try
                {
                    // Key must be valid
                    ExtractException.Assert("ELI21190", "Shortcut key cannot be none.", 
                        key != Keys.None);

                    // Check whether the specified key exists
                    if (_shortcuts.ContainsKey(key))
                    {
                        // Check whether the value exists
                        if (value == null)
                        {
                            // Remove the shortcut handler from the shortcuts collection
                            _shortcuts.Remove(key);
                        }
                        else
                        {
                            // Replace the shortcut handler specified for this key.
                            _shortcuts[key] = value;
                        }
                    }
                    else
                    {
                        // The specified shortcut handler doesn't exist. Add it only if it is non-null.
                        // NOTE: If it is null, there is no need to remove it since it doesn't exist.
                        if (value != null)
                        {
                            // Add the new key.
                            _shortcuts[key] = value;
                        }
                    }

                    // Raise the shortcut key changed event.
                    OnShortcutKeyChanged(new ShortcutKeyChangedEventArgs(key, value));
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21191",
                        "Cannot set shortcut handler.", e);
                    ee.AddDebugData("Key", key, false);
                    ee.AddDebugData("Shortcut Handler", value == null ? "null" : value.ToString(), 
                        false);
                    throw ee;
                }
            }
        }

        #endregion

        #region Shortcuts Manager Methods

        /// <summary>
        /// Removes the specified handler from keys.
        /// </summary>
        /// <param name="shortcutHandler">The shortcut handler.</param>
        public void RemoveHandlerFromKeys(ShortcutHandler shortcutHandler)
        {
            try
            {
                // Remove the handler for each key associated with the handler
                foreach (var key in GetKeys(shortcutHandler))
                {
                    this[key] = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32579");
            }
        }

        /// <summary>
        /// Gets an array of shortcut <see cref="Keys"/> associated with the specified 
        /// <see cref="ShortcutHandler"/>.
        /// </summary>
        /// <param name="shortcutHandler">The <see cref="ShortcutHandler"/> associated with the 
        /// array of shortcut <see cref="Keys"/> to get.</param>
        /// <returns>An array of shortcut <see cref="Keys"/> associated with the specified 
        /// <see cref="ShortcutHandler"/>. The array will be empty if no shortcut 
        /// <see cref="Keys"/> are found.</returns>
        /// <remarks><para>The order of the values in the returned array is unspecified.</para>
        /// <para>Performing this method is an O(n) operation, where n is the number of shortcut
        /// <see cref="Keys"/> stored in the <see cref="ShortcutsManager"/></para></remarks>
        public Keys[] GetKeys(ShortcutHandler shortcutHandler)
        {
            try
            {
                // Return an array of keys associated with the handler
                return _shortcuts
                    .Where(p => p.Value == shortcutHandler)
                    .Select(p => p.Key)
                    .ToArray();
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21134",
                    "Unable to get keys.", e);
                ee.AddDebugData("Shortcut handler", 
                    shortcutHandler == null ? "null" : shortcutHandler.ToString(), false);
                throw ee;
            }
        }

        /// <summary>
        /// Runs the <see cref="ShortcutHandler"/> associated with the specified key.
        /// </summary>
        /// <param name="key">The shortcut <see cref="Keys"/> value to process.</param>
        /// <returns><see langword="true"/> if the <see cref="ShortcutHandler"/> runs successfully.
        /// <see langword="false"/> if the specified key does not have a 
        /// <see cref="ShortcutHandler"/> associated with it.</returns>
        public bool ProcessKey(Keys key)
        {
            try
            {
                // Check if there is a shortcut handler associated with this key
                ShortcutHandler shortcutHandler;
                if (_shortcuts.TryGetValue(key, out shortcutHandler))
                {
                    ProcessingShortcut?.Invoke(this, new EventArgs());

                    // Run the shortcut handler and return true
                    shortcutHandler();
                    return true;
                }

                // This key does not have a shortcut handler
                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26500", ex);
            }
        }

        /// <summary>
        /// Removes all shortcut <see cref="Keys"/> and each associated 
        /// <see cref="ShortcutHandler"/> from the <see cref="ShortcutsManager"/> class.
        /// </summary>
        /// <event cref="ShortcutKeyChanged">Raised for each item removed.</event>
        public void Clear()
        {
            try
            {
                // If the shortcuts collection is empty, we are done.
                if (_shortcuts.Count == 0)
                {
                    return;
                }

                // Create an array of the keys in the shortcuts collection
                Keys[] keys = new Keys[_shortcuts.Count];
                _shortcuts.Keys.CopyTo(keys, 0);

                // Remove all the keys
                _shortcuts.Clear();

                // Raise an event for each key removed
                foreach (Keys key in keys)
                {
                    OnShortcutKeyChanged(new ShortcutKeyChangedEventArgs(key, null));
                }
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21195",
                    "Unable to clear shortcuts manager.", e);
            }
        }

        /// <summary>
        /// Retrieves a list of the specified key combinations as a comma-separated string 
        /// suitable for display to an end user.
        /// </summary>
        /// <param name="keys">The key combinations from which the display string should be 
        /// generated.</param>
        /// <returns><paramref name="keys"/> as a comma-separated string suitable for display to 
        /// an end user.</returns>
        public static string GetDisplayString(Keys[] keys)
        {
            try
            {
                // Handle the trivial case of the empty display string
                if (keys == null || keys.Length == 0)
                {
                    return "";
                }

                // Construct a list to hold the results
                List<string> displayStrings = new List<string>(keys.Length);

                // Get type converter for Keys object
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Keys));

                // Iterate through all the keys
                for (int i = 0; i < keys.Length; i++)
                {
                    // Add this display string, if it is not already added
                    string displayString = GetDisplayString(keys[i], converter);
                    if (!displayStrings.Contains(displayString))
                    {
                        displayStrings.Add(displayString);
                    }
                }

                // Return the result as a comma-separated list
                return String.Join(", ", displayStrings.ToArray());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26499", ex);
            }
        }

        /// <summary>
        /// Retrieves the specified key or key combination as text suitable for display to an end 
        /// user.
        /// </summary>
        /// <param name="key">The key or key combination from which the display string should be 
        /// generated.</param>
        /// <returns>The specified key or key combination as text suitable for display to an end 
        /// user.</returns>
        public static string GetDisplayString(Keys key)
        {
            return GetDisplayString(key, TypeDescriptor.GetConverter(typeof(Keys)));
        }

        /// <summary>
        /// Retrieves the specified key or key combination as text suitable for display to an end 
        /// user.
        /// </summary>
        /// <param name="key">The key or key combination from which the display string should be 
        /// generated.</param>
        /// <param name="converter">A converter for the <see cref="Keys"/> enumeration.</param>
        /// <returns>The specified key or key combination as text suitable for display to an end 
        /// user.</returns>
        private static string GetDisplayString(Keys key, TypeConverter converter)
        {
            // Get the display string from the string converter
            string displayString = converter.ConvertToString(key);

            // Check if the key code needs additional processing
            switch (key & Keys.KeyCode)
            {
                case Keys.Add:
                    return displayString.Replace("Add", "+");

                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                    return displayString.Replace("D", "");

                case Keys.Decimal:
                    return displayString.Replace("Decimal", ".");

                case Keys.Delete:
                    return displayString.Replace("Delete", "Del");

                case Keys.Divide:
                    return displayString.Replace("Divide", "/");

                case Keys.Escape:
                    return displayString.Replace("Escape", "Esc");

                case Keys.Insert:
                    return displayString.Replace("Insert", "Ins");

                case Keys.Multiply:
                    return displayString.Replace("Multiply", "*");

                case Keys.NumPad0:
                case Keys.NumPad1:
                case Keys.NumPad2:
                case Keys.NumPad3:
                case Keys.NumPad4:
                case Keys.NumPad5:
                case Keys.NumPad6:
                case Keys.NumPad7:
                case Keys.NumPad8:
                case Keys.NumPad9:
                    return displayString.Replace("NumPad", "");

                case Keys.Oem8:
                case Keys.OemClear:
                    return displayString.Replace("Oem", "");

                case Keys.OemBackslash:
                    return displayString.Replace("OemBackslash", "\\");

                case Keys.OemCloseBrackets:
                    return displayString.Replace("OemCloseBrackets", "}");

                case Keys.OemOpenBrackets:
                    return displayString.Replace("OemOpenBrackets", "{");

                case Keys.OemMinus:
                    return displayString.Replace("OemMinus", "-");

                case Keys.OemPeriod:
                    return displayString.Replace("OemPeriod", ".");

                case Keys.OemPipe:
                    return displayString.Replace("OemPipe", "|");

                case Keys.OemQuestion:
                    return displayString.Replace("OemQuestion", "?");

                case Keys.OemQuotes:
                    return displayString.Replace("OemQuotes", "\"");

                case Keys.OemSemicolon:
                    return displayString.Replace("OemSemicolon", ";");

                case Keys.Oemcomma:
                    return displayString.Replace("Oemcomma", ",");

                case Keys.Oemplus:
                    return displayString.Replace("Oemplus", "+");

                case Keys.Oemtilde:
                    return displayString.Replace("Oemtilde", "~");

                case Keys.Subtract:
                    return displayString.Replace("Subtract", "-");
            }

            // Return the result
            return displayString;
        }

        #endregion

        #region Shortcuts Manager Events

        /// <summary>
        /// Raises the <see cref="ShortcutKeyChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ShortcutKeyChangedEventArgs"/> that contains the event 
        /// data.</param>
        /// <seealso cref="ShortcutsManager.this"/>
        /// <seealso cref="ShortcutsManager.Clear"/>
        protected virtual void OnShortcutKeyChanged(ShortcutKeyChangedEventArgs e)
        {
            if (ShortcutKeyChanged != null)
            {
                ShortcutKeyChanged(this, e);
            }
        }

        #endregion
    }
}
