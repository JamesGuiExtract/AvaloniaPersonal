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
	/// Standard MessageBoxEx results
	/// </summary>
	public static class CustomizableMessageBoxResult
    {
        #region Constants

        /// <summary>
        /// Result when the Ok button is clicked.
        /// </summary>
		public static readonly string Ok = "Ok";

        /// <summary>
        /// Result when the Cancel button is clicked.
        /// </summary>
		public static readonly string Cancel = "Cancel";

        /// <summary>
        /// Result when the Yes button is clicked.
        /// </summary>
		public static readonly string Yes = "Yes";

        /// <summary>
        /// Result when the No button is clicked.
        /// </summary>
		public static readonly string No = "No";

        /// <summary>
        /// Result when the Abort button is clicked.
        /// </summary>
		public static readonly string Abort = "Abort";

        /// <summary>
        /// Result when the Retry button is clicked.
        /// </summary>
		public static readonly string Retry = "Retry";

        /// <summary>
        /// Result when the Ignore button is clicked.
        /// </summary>
		public static readonly string Ignore = "Ignore";

        /// <summary>
        /// Result when the Timeout event occurs.
        /// </summary>
        public static readonly string Timeout = "Timeout";

        #endregion
    }
}
