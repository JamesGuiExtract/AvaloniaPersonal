// Code from the following article by Sijin Joseph:
// http://www.codeproject.com/KB/dialog/CustomizableMessageBox.aspx
// It has been modified to meet our standards and changed slightly to better fit
// what we need it to do.  Removed the loading of string resources that would display
// the standard buttons in either English, German, or French depending on your locale.
// The buttons are now only in English.  Modified to use ExtractExceptions and to throw
// ExtractExceptions from all publicly visible properties and methods.  Modified to allow
// specifying which button should be considered the default button.
using System;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Utilities.Forms
{
	/// <summary>
	/// Standard MessageBoxEx buttons.
	/// </summary>
    // In order to be consistent with the MessageBoxButtons enum the 0 value here should
    // be named Ok (even though FX Cop wants it to be called none)
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [FlagsAttribute]
	public enum CustomizableMessageBoxButtons
	{
        /// <summary>
        /// The Ok button.
        /// </summary>
		Ok = 0,

        /// <summary>
        /// The Cancel button.
        /// </summary>
		Cancel = 1,
        
        /// <summary>
        /// The Yes button.
        /// </summary>
		Yes = 2,

        /// <summary>
        /// The No button
        /// </summary>
		No = 4,

        /// <summary>
        /// The Abort button.
        /// </summary>
		Abort = 8,

        /// <summary>
        /// The Retry button.
        /// </summary>
		Retry = 16,

        /// <summary>
        /// The Ignore button.
        /// </summary>
		Ignore = 32
	}
}
