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
    /// Internal data structure used to represent a message box button.
    /// </summary>
    public class CustomizableMessageBoxButton
    {
        #region Fields

        /// <summary>
        /// The text displayed on this button.
        /// </summary>
        private string _text;

        /// <summary>
        /// The value returned by the message box if this button is clicked.
        /// </summary>
        private string _value;

        /// <summary>
        /// The tooltip text for this button.
        /// </summary>
        private string _toolTipText;

        /// <summary>
        /// Whether this button should be treated as the cancel button.
        /// </summary>
        private bool _isCancelButton;

        /// <summary>
        /// Whether this button should be treated as the default button.
        /// </summary>
        private bool _isDefaultButton;

        #endregion Fields

        /// <summary>
        /// Gets or sets the text of the button.
        /// </summary>
        /// <value>The text to display on this button.</value>
        /// <return>The text displayed on this button.</return>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                try
                {
                    _text = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21648", ex);
                    ee.AddDebugData("Button text", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets the return value when this button is clicked.
        /// </summary>
        /// <value>The text returned when the button is clicked.</value>
        /// <return>The text returned when the button is clicked.</return>
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                try
                {
                    _value = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21650", ex);
                    ee.AddDebugData("Button return value", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets the tooltip that is displayed for this button
        /// </summary>
        /// <value>The tooltip text.</value>
        /// <return>The tooltip text.</return>
        public string ToolTipText
        {
            get
            {
                return _toolTipText;
            }
            set
            {
                try
                {
                    _toolTipText = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21652", ex);
                    ee.AddDebugData("Button tooltip text", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this button is a cancel button. i.e. the button
        /// that will be assumed to have been clicked if the user closes the message box
        /// without pressing any button.
        /// </summary>
        /// <value>Whether this button should be treated as the cancel button.</value>
        /// <return>Whether this button should be treated as the cancel button.</return>
        public bool IsCancelButton
        {
            get
            {
                return _isCancelButton;
            }
            set
            {
                try
                {
                    _isCancelButton = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21654", ex);
                    ee.AddDebugData("Button isCancelButton", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this button should be treated as the default button
        /// for the message box.
        /// </summary>
        /// <value>Whether this button should be treated as the default button.</value>
        /// <return>Whether this button should be treated as the default button.</return>
        internal bool IsDefaultButton
        {
            get
            {
                return _isDefaultButton;
            }
            set
            {
                try
                {
                    _isDefaultButton = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21626", ex);
                    ee.AddDebugData("Button isDefaultButton", value, false);

                    throw ee;
                }
            }
        }
    }
}
