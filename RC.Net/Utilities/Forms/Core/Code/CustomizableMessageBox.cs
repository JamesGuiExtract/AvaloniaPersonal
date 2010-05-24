// Code from the following article by Sijin Joseph:
// http://www.codeproject.com/KB/dialog/CustomizableMessageBox.aspx
// It has been modified to meet our standards and changed slightly to better fit
// what we need it to do.  Removed the loading of string resources that would display
// the standard buttons in either English, German, or French depending on your locale.
// The buttons are now only in English.  Modified to use ExtractExceptions and to throw
// ExtractExceptions from all publicly visible properties and methods.  Modified to allow
// specifying which button should be considered the default button.
using Extract;
using Extract.Licensing;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// An extended MessageBox with lots of customizing capabilities.
    /// </summary>
    public class CustomizableMessageBox : IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(CustomizableMessageBox).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The message box form object.
        /// </summary>
        private CustomizableMessageBoxForm _msgBox = new CustomizableMessageBoxForm();

        /// <summary>
        /// Specifies whether the message box should use a saved response.
        /// </summary>
        private bool _useSavedResponse = true;

        /// <summary>
        /// The name for this message box.
        /// </summary>
        private string _name;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of this message box.
        /// </summary>
        /// <value>The name for this message box</value>
        /// <return>The name for this message box</return>
        internal string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Sets the caption of the message box.
        /// </summary>
        /// <value>The caption for this message box.</value>
        /// <return>The caption for this message box.</return>
        public string Caption
        {
            get
            {
                return _msgBox.Caption;
            }
            set
            {
                try
                {
                    _msgBox.Caption = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21621", ex);
                    ee.AddDebugData("Message box caption", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets the text of the message box.
        /// </summary>
        /// <value>The text of the message box.</value>
        /// <return>The text of the message box.</return>
        public string Text
        {
            get
            {
                return _msgBox.Message;
            }
            set
            {
                try
                {
                    _msgBox.Message = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21622", ex);
                    ee.AddDebugData("Message box text", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets the icon to show in the message box
        /// </summary>
        /// <value>The custom icon to display in the message box.</value>
        /// <return>The custom icon to display in the message box.</return>
        public Icon CustomIcon
        {
            get
            {
                return _msgBox.CustomIcon;
            }
            set
            {
                try
                {
                    _msgBox.CustomIcon = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21623", ex);
                    if (value != null)
                    {
                        ee.AddDebugData("Message box custom icon", value.ToString(), false);
                        ee.AddDebugData("Custom icon width", value.Width, false);
                        ee.AddDebugData("Custom icon height", value.Height, false);
                    }
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets the standard icon to show in the message box.
        /// </summary>
        /// <value>The standard icon to display in the message box.</value>
        /// <return>The standard icon to display in the message box.</return>
        public MessageBoxIcon StandardIcon
        {
            get
            {
                return _msgBox.StandardIcon;
            }
            set
            {
                try
                {
                    _msgBox.StandardIcon = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21624", ex);
                    ee.AddDebugData("Message box standard icon", value.ToString(), false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets the font for the text of the message box.
        /// </summary>
        /// <value>The font for the text of the message box.</value>
        /// <return>The font for the text of the message box.</return>
        public Font Font
        {
            get
            {
                return _msgBox.Font;
            }
            set
            {
                try
                {
                    _msgBox.Font = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21625", ex);
                    if (value != null)
                    {
                        ee.AddDebugData("Message box font name", value.Name, false);
                        ee.AddDebugData("Font size", value.Size, false);
                    }
                    throw ee;
                }

            }
        }

        /// <summary>
        /// Sets or gets the ability of the user to save his/her response.
        /// </summary>
        /// <value>Whether to save the users response.</value>
        /// <return>Whether the users response should/will be saved.</return>
        public bool AllowSaveResponse
        {
            get
            {
                return _msgBox.AllowSaveResponse;
            }
            set
            {
                try
                {
                    _msgBox.AllowSaveResponse = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21627", ex);
                    ee.AddDebugData("Message box AllowSaveResponse", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets the text to show to the user next to the save response checkbox.
        /// </summary>
        /// <value>The text to be displayed next to the save response checkbox.</value>
        /// <return>The text to be displayed next to the save response checkbox.</return>
        public string SaveResponseText
        {
            get
            {
                return _msgBox.SaveResponseText;
            }
            set
            {
                try
                {
                    _msgBox.SaveResponseText = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21628", ex);
                    ee.AddDebugData("Message box SaveResponseText", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets or gets whether the saved response (if available) should be used.
        /// </summary>
        /// <remarks>
        /// This property will determine if the message box should be shown to the user
        /// again or not depending on whether the user has saved a response to the
        /// message box.<para/>
        /// For example: The user is shown a message box with a checkbox allowing to
        /// save their response.  If they check the checkbox their response will be
        /// recorded.  If the UseSavedResponse property is set to true the next time
        /// <see cref="CustomizableMessageBox.Show()"/> is called the message box will
        /// not be displayed to the user and the result of the message box show will be the
        /// saved response.
        /// </remarks>
        /// <value>Whether to use the saved response (if available).</value>
        /// <return>Whether the saved response (if available) will be used.</return>
        public bool UseSavedResponse
        {
            get
            {
                return _useSavedResponse;
            }
            set
            {
                try
                {
                    _useSavedResponse = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21630", ex);
                    ee.AddDebugData("Message box UseSavedResponse", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets or gets whether an alert sound is played while showing the message box.
        /// The sound played depends on the the Icon selected for the message box.
        /// </summary>
        /// <value>Whether to play an alert sound when the message box is displayed.</value>
        /// <return>Whether or not a sound will be played when the message box is displayed.
        /// </return>
        public bool PlayAlertSound
        {
            get
            {
                return _msgBox.PlayAlertSound;
            }
            set
            {
                try
                {
                    _msgBox.PlayAlertSound = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21632", ex);
                    ee.AddDebugData("Message box PlayAlertSound", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets or gets the time in milliseconds for which the message box is displayed.
        /// If timeout value is zero then the message box will not timeout.
        /// </summary>
        /// <value>The timeout value (in milliseconds).</value>
        /// <return>The timeout value (in milliseconds).</return>
        public int Timeout
        {
            get
            {
                return _msgBox.Timeout;
            }
            set
            {
                try
                {
                    _msgBox.Timeout = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21634", ex);
                    ee.AddDebugData("Message box Timeout", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Controls the result that will be returned when the message box times out.
        /// </summary>
        /// <value>The result if dialog closes due to time out.</value>
        /// <return>The result if dialog closes due to time out.</return>
        public TimeoutResult TimeoutResult
        {
            get
            {
                return _msgBox.TimeoutResult;
            }
            set
            {
                try
                {
                    _msgBox.TimeoutResult = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21636", ex);
                    ee.AddDebugData("Message box TimeoutResult", value.ToString(), false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets the result from the message box.
        /// </summary>
        /// <return>The result from the message box.</return>
        public string Result
        {
            get
            {
                return _msgBox.Result;
            }
        }

        /// <summary>
        /// Gets whether the message box is currently visible or not.
        /// </summary>
        /// <return>Whether the message box is visible or not.</return>
        public bool IsVisible
        {
            get
            {
                return _msgBox.Visible;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <returns>The message box result.</returns>
        public string Show()
        {
            try
            {
                return Show(null);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21637", ex);
            }
        }

        /// <summary>
        /// Shows the messsage box with the specified owner
        /// </summary>
        /// <param name="owner">The window which owns this dialog</param>
        /// <returns>The message box result.</returns>
        public string Show(IWin32Window owner)
        {
            try
            {
                // If use saved response, just get response and return it
                if (_useSavedResponse && this.Name != null)
                {
                    // Get the saved response
                    string savedResponse = CustomizableMessageBoxManager.GetSavedResponse(this);

                    // If there is a saved response, just return it
                    if (savedResponse != null)
                    {
                        return savedResponse;
                    }
                }

                if (owner == null)
                {
                    _msgBox.ShowDialog();
                }
                else
                {
                    _msgBox.StartPosition = FormStartPosition.CenterParent;
                    _msgBox.ShowDialog(owner);
                }

                // Get the result from the message box
                string result = _msgBox.Result;

                // If this dialog is named (multiple use stored in the manager)
                // handle response saving if needed
                if (this.Name != null)
                {
                    if (_msgBox.AllowSaveResponse && _msgBox.SaveResponse)
                    {
                        // Set the saved response
                        CustomizableMessageBoxManager.SetSavedResponse(this, result);
                    }
                    else
                    {
                        // Reset the saved response
                        CustomizableMessageBoxManager.ResetSavedResponse(this.Name);
                    }
                }

                // Return the message box result
                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21638", ex);
            }
        }

        /// <summary>
        /// Shows the message box in a modeless fashion.
        /// </summary>
        public void ShowModeless()
        {
            try
            {
                ShowModeless(null);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21933", ex);
            }
        }

        /// <summary>
        /// Shows the messsage box with the specified owner in a modeless fashion.
        /// </summary>
        /// <param name="owner">The window which owns this dialog.</param>
        public void ShowModeless(IWin32Window owner)
        {
            try
            {
                if (owner == null)
                {
                    _msgBox.Show();
                }
                else
                {
                    _msgBox.Show(owner);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21934", ex);
            }
        }
        /// <summary>
        /// Add a custom button to the message box.
        /// <para><b>Note:</b></para>
        /// If more than one button is set as the default button, then the first
        /// button marked as default will be the default button on the message box.
        /// </summary>
        /// <param name="button">The button to add</param>
        /// <param name="defaultButton">Sets whether this button should be the
        /// default button.</param>
        public void AddButton(CustomizableMessageBoxButton button, bool defaultButton)
        {
            try
            {
                ExtractException.Assert("ELI21620", "Cannot add null button!", button != null);

                // Set the default button property
                button.IsDefaultButton = defaultButton;

                // Add the button
                _msgBox.Buttons.Add(button);

                // If this is a custom cancel button, set the custom cancel button property
                if (button.IsCancelButton)
                {
                    _msgBox.CustomCancelButton = button;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21619", ex);
            }
        }

        /// <summary>
        /// Add a custom button to the message box.
        /// <para><b>Note:</b></para>
        /// If more than one button is set as the default button, then the first
        /// button marked as default will be the default button on the message box.
        /// </summary>
        /// <param name="text">The text of the button. This must not be <see langword="null"/>
        /// </param>
        /// <param name="value">The return value if this button is clicked. This must not
        /// be <see langword="null"/></param>
        /// <param name="defaultButton">Sets whether this button should be the
        /// default button.</param>
        /// <exception cref="ExtractException"><paramref name="text"/> is
        /// <see langword="null"/></exception>
        /// <exception cref="ExtractException"><paramref name="value"/> is
        /// <see langword="null"/></exception>
        public void AddButton(string text, string value, bool defaultButton)
        {
            try
            {
                ExtractException.Assert("ELI21640", "Text cannot be null!", text != null);
                ExtractException.Assert("ELI21641",
                    "Value of a button cannot be null", value != null);

                CustomizableMessageBoxButton button = new CustomizableMessageBoxButton();
                button.Text = text;
                button.Value = value;

                AddButton(button, defaultButton);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21639", ex);
            }
        }

        /// <summary>
        /// Add a standard button to the message box.
        /// <para><b>Note:</b></para>
        /// If more than one button is set as the default button, then the first
        /// button marked as default will be the default button on the message box.
        /// </summary>
        /// <param name="button">The standard button to add.</param>
        /// <param name="defaultButton">Sets whether this button should be the
        /// default button.</param>
        public void AddButton(CustomizableMessageBoxButtons button, bool defaultButton)
        {
            try
            {
                CustomizableMessageBoxButton btn = new CustomizableMessageBoxButton();
                string text = "";

                // Set text for default buttons
                switch (button)
                {
                    case CustomizableMessageBoxButtons.Ok:
                        text = "&OK";
                        break;

                    case CustomizableMessageBoxButtons.Cancel:
                        text = "&Cancel";

                        // For the cancel button also need to set IsCancelButton to true
                        btn.IsCancelButton = true;
                        break;

                    case CustomizableMessageBoxButtons.Yes:
                        text = "&Yes";
                        break;

                    case CustomizableMessageBoxButtons.No:
                        text = "&No";
                        break;

                    case CustomizableMessageBoxButtons.Abort:
                        text = "&Abort";
                        break;

                    case CustomizableMessageBoxButtons.Retry:
                        text = "&Retry";
                        break;

                    case CustomizableMessageBoxButtons.Ignore:
                        text = "&Ignore";
                        break;

                    default:
                        ExtractException.ThrowLogicException("ELI21629");
                        break;
                }

                btn.Text = text;
                btn.Value = button.ToString();

                AddButton(btn, defaultButton);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21642", ex);
            }
        }

        /// <summary>
        /// Add standard <see cref="MessageBoxButtons"/> to the message box.
        /// </summary>
        /// <param name="buttons">The standard buttons to add.</param>
        public void AddStandardButtons(MessageBoxButtons buttons)
        {
            try
            {
                switch (buttons)
                {
                    case MessageBoxButtons.OK:
                        AddButton(CustomizableMessageBoxButtons.Ok, true);
                        break;

                    case MessageBoxButtons.AbortRetryIgnore:
                        AddButton(CustomizableMessageBoxButtons.Abort, true);
                        AddButton(CustomizableMessageBoxButtons.Retry, false);
                        AddButton(CustomizableMessageBoxButtons.Ignore, false);
                        break;

                    case MessageBoxButtons.OKCancel:
                        AddButton(CustomizableMessageBoxButtons.Ok, true);
                        AddButton(CustomizableMessageBoxButtons.Cancel, false);
                        break;

                    case MessageBoxButtons.RetryCancel:
                        AddButton(CustomizableMessageBoxButtons.Retry, true);
                        AddButton(CustomizableMessageBoxButtons.Cancel, false);
                        break;

                    case MessageBoxButtons.YesNo:
                        AddButton(CustomizableMessageBoxButtons.Yes, true);
                        AddButton(CustomizableMessageBoxButtons.No, false);
                        break;

                    case MessageBoxButtons.YesNoCancel:
                        AddButton(CustomizableMessageBoxButtons.Yes, true);
                        AddButton(CustomizableMessageBoxButtons.No, false);
                        AddButton(CustomizableMessageBoxButtons.Cancel, false);
                        break;

                    default:
                        ExtractException.ThrowLogicException("ELI21645");
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21643", ex);
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="CustomizableMessageBox"/> class.
        /// </summary>
        public CustomizableMessageBox()
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23137",
                    _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23138", ex);
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Dispose method for the class.
        /// </summary>
        /// <param name="disposing">If <see langword="true"/> then will dispose of
        /// resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_msgBox != null)
                {
                    _msgBox.Dispose();
                }
            }
        }

        /// <summary>
        /// Called by the <see cref="CustomizableMessageBoxManager"/> when it is disposed.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
