// Code from the following article by Sijin Joseph:
// http://www.codeproject.com/KB/dialog/CustomizableMessageBox.aspx
// It has been modified to meet our standards and changed slightly to better fit
// what we need it to do.  Removed the loading of string resources that would display
// the standard buttons in either English, German, or French depending on your locale.
// The buttons are now only in English.  Modified to use ExtractExceptions and to throw
// ExtractExceptions from all publicly visible properties and methods.  Modified to allow
// specifying which button should be considered the default button.
using System;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Enumerates the kind of results that can be returned when a
    /// message box times out
    /// </summary>
    public enum TimeoutResult
    {
        /// <summary>
        /// On timeout the value associated with the default button is set as the result.
        /// This is the default action on timeout.
        /// </summary>
        Default,

        /// <summary>
        /// On timeout the value associated with the cancel button is set as the result. If
        /// the messagebox does not have a cancel button then the value associated with 
        /// the default button is set as the result.
        /// </summary>
        Cancel,

        /// <summary>
        /// On timeout CustomizableMessageBoxResult.Timeout is set as the result.
        /// </summary>
        Timeout
    }
}
