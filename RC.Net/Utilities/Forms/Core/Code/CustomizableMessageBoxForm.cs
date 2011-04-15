// Code from the following article by Sijin Joseph:
// http://www.codeproject.com/KB/dialog/CustomizableMessageBox.aspx
// It has been modified to meet our standards and changed slightly to better fit
// what we need it to do.  Removed the loading of string resources that would display
// the standard buttons in either English, German, or French depending on your locale.
// The buttons are now only in English.  Modified to use ExtractExceptions and to throw
// ExtractExceptions from all publicly visible properties and methods.  Modified to allow
// specifying which button should be considered the default button.
using Extract.Licensing;
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// An advanced MessageBox that supports customizations like Font, Icon,
    /// Buttons and Saved Responses.
    /// </summary>
    internal class CustomizableMessageBoxForm : System.Windows.Forms.Form
    {
        #region Constants

        const int LEFT_PADDING = 12;
        const int RIGHT_PADDING = 12;
        const int TOP_PADDING = 12;
        const int BOTTOM_PADDING = 12;

        const int BUTTON_LEFT_PADDING = 4;
        const int BUTTON_RIGHT_PADDING = 4;
        const int BUTTON_TOP_PADDING = 4;
        const int BUTTON_BOTTOM_PADDING = 4;

        const int MIN_BUTTON_HEIGHT = 23;
        const int MIN_BUTTON_WIDTH = 74;

        const int ITEM_PADDING = 10;
        const int ICON_MESSAGE_PADDING = 15;

        const int BUTTON_PADDING = 5;

        const int CHECKBOX_WIDTH = 20;

        const int IMAGE_INDEX_EXCLAMATION = 0;
        const int IMAGE_INDEX_QUESTION = 1;
        const int IMAGE_INDEX_STOP = 2;
        const int IMAGE_INDEX_INFORMATION = 3;

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(CustomizableMessageBoxForm).ToString();

        #endregion Constants

        #region Fields

        System.ComponentModel.IContainer components;
        System.Windows.Forms.CheckBox chbSaveResponse;
        System.Windows.Forms.ImageList imageListIcons;
        System.Windows.Forms.ToolTip buttonToolTip;

        List<CustomizableMessageBoxButton> _buttons = 
            new List<CustomizableMessageBoxButton>();
        bool _allowSaveResponse;
        bool _playAlert = true;
        CustomizableMessageBoxButton _cancelButton;
        Button _defaultButtonControl;

        int _maxLayoutWidth;

        int _maxWidth;
        int _maxHeight;

        bool _allowCancel = true;
        string _result;

        /// <summary>
        /// Used to determine the alert sound to play
        /// </summary>
        MessageBoxIcon _standardIcon = MessageBoxIcon.None;
        Icon _iconImage;

        Timer _timeoutTimer;
        int _timeout;
        TimeoutResult _timeoutResult = TimeoutResult.Default;
        System.Windows.Forms.Panel _panelIcon;
        Extract.Utilities.Forms.BetterRichTextBox _rtbMessage;

        /// <summary>
        /// Maps CustomizableMessageBox buttons to Button controls
        /// </summary>
        Dictionary<CustomizableMessageBoxButton, Button> _buttonControlsTable =
            new Dictionary<CustomizableMessageBoxButton, Button>();

        #endregion Fields

        #region Properties

        /// <summary>
        /// Sets the message that will be displayed on this message box.
        /// </summary>
        /// <value>The message to display on this message box.</value>
        /// <return>The message to display on this message box.</return>
        public string Message
        {
            get
            {
                return _rtbMessage.Text;
            }
            set
            {
                try
                {
                    _rtbMessage.Text = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ex.AsExtract("ELI21655");
                    ee.AddDebugData("Message box text", value, false);

                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets the caption that will be displayed by this message box.
        /// </summary>
        /// <value>The caption to display on this message box.</value>
        /// <return>The caption to display on this message box.</return>
        public string Caption
        {
            get
            {
                return this.Text;
            }
            set
            {
                try
                {
                    this.Text = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ex.AsExtract("ELI21656");
                    ee.AddDebugData("Message box caption", value, false);

                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets the <see cref="Font"/> that will be used for this message box.
        /// </summary>
        /// <value>The <see cref="Font"/> for this message box.</value>
        /// <return>The <see cref="Font"/> for this message box.</return>
        public Font CustomFont
        {
            get
            {
                return this.Font;
            }
            set
            {
                try
                {
                    this.Font = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ex.AsExtract("ELI21657");
                    if (value != null)
                    {
                        ee.AddDebugData("Message box font", Font.Name, false);
                        ee.AddDebugData("Message box font size", Font.Size, false);
                    }

                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="CustomizableMessageBoxButton"/> that will
        /// be displayed in this message box.
        /// </summary>
        /// <return>A <see cref="List{T}"/> of <see cref="CustomizableMessageBoxButton"/>
        /// to be displayed in the message box.</return>
        public List<CustomizableMessageBoxButton> Buttons
        {
            get
            {
                return _buttons;
            }
        }

        /// <summary>
        /// Gets or sets whether the message box should allow the user to save their response.
        /// </summary>
        /// <value>Whether to allow the user to save their response.</value>
        /// <return>Whether to allow the user to save their response.</return>
        public bool AllowSaveResponse
        {
            get
            {
                return _allowSaveResponse;
            }
            set
            {
                try
                {
                    _allowSaveResponse = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ex.AsExtract("ELI21660");
                    ee.AddDebugData("Message box AllowSaveResponse", value, false);

                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets whether the user checked the Save response check box.
        /// </summary>
        /// <return>Whether the save response checkbox is checked.</return>
        public bool SaveResponse
        {
            get
            {
                try
                {
                    return chbSaveResponse.Checked;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI21661");
                }
            }
        }

        /// <summary>
        /// Sets the saved response text.
        /// </summary>
        /// <value>The saved response text.</value>
        /// <return>The saved response text.</return>
        public string SaveResponseText
        {
            get
            {
                try
                {
                    return chbSaveResponse.Text;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI21708");
                }
            }
            set
            {
                try
                {
                    chbSaveResponse.Text = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ex.AsExtract("ELI21662");
                    ee.AddDebugData("Message box SaveResponseText", value, false);

                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets the <see cref="MessageBoxIcon"/> to display in this message box.
        /// </summary>
        /// <value>The <see cref="MessageBoxIcon"/> to display.</value>
        /// <return>The <see cref="MessageBoxIcon"/> to display.</return>
        public MessageBoxIcon StandardIcon
        {
            get
            {
                return _standardIcon;
            }
            set
            {
                try
                {
                    SetStandardIcon(value);
                }
                catch (Exception ex)
                {
                    ExtractException ee = ex.AsExtract("ELI21663");
                    ee.AddDebugData("Message box standard icon", value.ToString(), false);

                    throw ee;
                }
            }
        }

        /// <summary>
        /// Sets the <see cref="Icon"/> to display in this message box.
        /// <para>Note:</para>
        /// This will clear out any standard (<see cref="MessageBoxIcon"/>) that
        /// may already have been set.
        /// </summary>
        /// <value>The custom <see cref="Icon"/> for this message box.</value>
        /// <return>The custom <see cref="Icon"/> for this message box if it has been set,
        /// otherwise returns <see langword="null"/>.</return>
        public Icon CustomIcon
        {
            get
            {
                if (_standardIcon == MessageBoxIcon.None)
                {
                    return _iconImage;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    // Set the standard icon to none
                    _standardIcon = MessageBoxIcon.None;

                    // Set the icon image to the custom icon
                    _iconImage = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI21664");
                }
            }
        }

        /// <summary>
        /// Sets the specified <see cref="CustomizableMessageBoxButton"/> to act as the cancel button.
        /// </summary>
        /// <value>The <see cref="CustomizableMessageBoxButton"/> that should act as the cancel button.
        /// </value>
        /// <return>The <see cref="CustomizableMessageBoxButton"/> that is serving as the cancel button.
        /// </return>
        public CustomizableMessageBoxButton CustomCancelButton
        {
            get
            {
                return _cancelButton;
            }
            set
            {
                try
                {
                    ExtractException.Assert("ELI21666",
                        "Cannot set Cancel button to a button not on the message box!",
                        _buttons.Contains(value));

                    _cancelButton = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI21665");
                }
            }
        }

        /// <summary>
        /// Gets the message box result.
        /// </summary>
        /// <value>The message box result.</value>
        public string Result
        {
            get
            {
                return _result;
            }
        }

        /// <summary>
        /// Gets or sets whether the message box should play an alert sound.
        /// </summary>
        /// <value>Whether to play an alert sound or not.</value>
        /// <return>Whether to play an alert sound or not.</return>
        public bool PlayAlertSound
        {
            get
            {
                return _playAlert;
            }
            set
            {
                try
                {
                    _playAlert = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ex.AsExtract("ELI21669");
                    ee.AddDebugData("Messagbox PlayAlertSound", value, false);

                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets the message box timeout value in milliseconds.
        /// </summary>
        /// <value>The number of milliseconds for the message box timeout.</value>
        /// <return>The number of milliseconds for the message box timeout.</return>
        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                try
                {
                    _timeout = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ex.AsExtract("ELI21671");
                    ee.AddDebugData("Message box timeout", value, false);

                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets the TimeoutResult value.
        /// </summary>
        /// <value>The <see cref="TimeoutResult"/> value for this message box.</value>
        /// <return>The <see cref="TimeoutResult"/> value for this message box.</return>
        public TimeoutResult TimeoutResult
        {
            get
            {
                return _timeoutResult;
            }
            set
            {
                try
                {
                    _timeoutResult = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ex.AsExtract("ELI21674");
                    ee.AddDebugData("Message box timeout result", value, false);

                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets/sets whether selection is allowed in the <see cref="RichTextBox"/> control.
        /// </summary>
        internal bool AllowSelection
        {
            get
            {
                return _rtbMessage.AllowSelection;
            }
            set
            {
                _rtbMessage.AllowSelection = value;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        public new event EventHandler<KeyEventArgs> KeyPress;

        #endregion Events 

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="CustomizableMessageBoxForm"/> class.
        /// </summary>
        public CustomizableMessageBoxForm()
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23139",
                    _OBJECT_NAME);

                //
                // Required for Windows Form Designer support
                //
                InitializeComponent();

                _maxWidth = (int)(SystemInformation.WorkingArea.Width * 0.60);
                _maxHeight = (int)(SystemInformation.WorkingArea.Height * 0.90);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21677",
                    "Unable to construct new CustomizableMessageBoxForm!", ex);
            }
        }

        #endregion Constructors

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CustomizableMessageBoxForm));
            this._panelIcon = new System.Windows.Forms.Panel();
            this.chbSaveResponse = new System.Windows.Forms.CheckBox();
            this.imageListIcons = new System.Windows.Forms.ImageList(this.components);
            this.buttonToolTip = new System.Windows.Forms.ToolTip(this.components);
            this._rtbMessage = new Extract.Utilities.Forms.BetterRichTextBox();
            this.SuspendLayout();
            // 
            // panelIcon
            // 
            this._panelIcon.BackColor = System.Drawing.Color.Transparent;
            this._panelIcon.Location = new System.Drawing.Point(8, 8);
            this._panelIcon.Name = "panelIcon";
            this._panelIcon.Size = new System.Drawing.Size(32, 32);
            this._panelIcon.TabIndex = 3;
            this._panelIcon.Visible = false;
            // 
            // chbSaveResponse
            // 
            this.chbSaveResponse.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chbSaveResponse.Location = new System.Drawing.Point(56, 56);
            this.chbSaveResponse.Name = "chbSaveResponse";
            this.chbSaveResponse.Size = new System.Drawing.Size(104, 16);
            this.chbSaveResponse.TabIndex = 0;
            // 
            // imageListIcons
            // 
            this.imageListIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.imageListIcons.ImageSize = new System.Drawing.Size(32, 32);
            this.imageListIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListIcons.ImageStream")));
            this.imageListIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // rtbMessage
            // 
            this._rtbMessage.AllowSelection = true;
            this._rtbMessage.BackColor = System.Drawing.SystemColors.Control;
            this._rtbMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._rtbMessage.Location = new System.Drawing.Point(200, 8);
            this._rtbMessage.Name = "rtbMessage";
            this._rtbMessage.ReadOnly = true;
            this._rtbMessage.Size = new System.Drawing.Size(100, 48);
            this._rtbMessage.TabIndex = 4;
            this._rtbMessage.TabStop = false;
            this._rtbMessage.Text = "";
            this._rtbMessage.Visible = false;
            // 
            // CustomizableMessageBoxForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(322, 224);
            this.Controls.Add(this._rtbMessage);
            this.Controls.Add(this.chbSaveResponse);
            this.Controls.Add(this._panelIcon);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomizableMessageBoxForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);

        }

        #endregion Windows Form Designer generated code

        #region Overrides

        /// <summary>
        /// This will get called everytime we call ShowDialog on the form
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> associated with this event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                //Reset result
                _result = null;

                this.Size = new Size(_maxWidth, _maxHeight);

                //This is the rectangle in which all items will be laid out
                _maxLayoutWidth = this.ClientSize.Width - LEFT_PADDING - RIGHT_PADDING;

                AddOkButtonIfNoButtonsPresent();
                DisableCloseIfMultipleButtonsAndNoCancelButton();

                SetIconSizeAndVisibility();
                SetMessageSizeAndVisibility();
                SetCheckboxSizeAndVisibility();

                SetOptimumSize();

                LayoutControls();

                CenterForm();

                PlayAlert();

                SelectDefaultButton();

                StartTimerIfTimeoutGreaterThanZero();

                base.OnLoad(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI21675");
            }
        }

        /// <summary>
        /// Handles command keys <see cref="Control.ProcessCmdKey"/>.
        /// </summary>
        /// <param name="msg">The <see cref="Message"/> that represents the window message
        /// to process.</param>
        /// <param name="keyData">The <see cref="Keys"/> values that represents the key
        /// to process.</param>
        /// <returns>Whether the character was processed by the control or not.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                if (!_allowCancel && keyData == (Keys.Alt | Keys.F4))
                {
                    return true;
                }
                else if (_allowCancel && keyData == Keys.Escape)
                {
                    // Set the dialog result
                    DialogResult = DialogResult.Cancel;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI21678");
                ee.AddDebugData("KeyData", keyData.ToString(), false);

                if (msg != null)
                {
                    ee.AddDebugData("Message", msg.ToString(), false);
                    ee.AddDebugData("MessageID", msg.Msg, false);
                }

                ee.Display();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Handles the OnClosing event.
        /// </summary>
        /// <param name="e">The <see cref="CancelEventArgs"/> associated with this event.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (_result == null)
                {
                    if (_allowCancel)
                    {
                        _result = _cancelButton.Value;
                    }
                    else
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                if (_timeoutTimer != null)
                {
                    _timeoutTimer.Stop();
                }

                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI21681");
            }
        }

        /// <summary>
        /// Handles the OnPaint event so that the specified icon is drawn.
        /// </summary>
        /// <param name="e">The <see cref="PaintEventArgs"/> associated with this event.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                if (_iconImage != null)
                {
                    e.Graphics.DrawIcon(_iconImage,
                        new Rectangle(_panelIcon.Location, new Size(32, 32)));
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI21682");
            }
        }

        /// <summary>
        /// Processes a dialog box key.
        /// </summary>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that
        /// represents the key to process.</param>
        /// <returns>
        /// <see langword="true"/> if the keystroke was processed and consumed by the control;
        /// otherwise, <see langword="true"/> to allow further processing.
        /// </returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            bool result = false;

            try
            {
                if (!IsInputKey(keyData))
                {
                    OnKeyPress(new KeyEventArgs(keyData));
                }

                result = base.ProcessDialogKey(keyData);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32355");
            }

            return result;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                OnKeyPress(e);

                base.OnKeyDown(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32354");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Methods

        /// <summary>
        /// Measures a string using the Graphics object for this form with
        /// the specified font
        /// </summary>
        /// <param name="stringToMeasure">The string to measure</param>
        /// <param name="maxWidth">The maximum width available to display the string</param>
        /// <param name="font">The font with which to measure the string</param>
        /// <returns>The <see cref="Size"/> needed to contain the
        /// <paramref name="stringToMeasure"/> when drawn with the specified <see cref="Font"/>.
        /// </returns>
        Size MeasureString(string stringToMeasure, int maxWidth, Font font)
        {
            using (Graphics g = this.CreateGraphics())
            {
                SizeF strRectSizeF = g.MeasureString(stringToMeasure, font, maxWidth);

                return new Size((int)Math.Ceiling(strRectSizeF.Width),
                    (int)Math.Ceiling(strRectSizeF.Height));
            }
        }

        /// <summary>
        /// Measures a string using the Graphics object for this form and the
        /// font of this form
        /// </summary>
        /// <param name="stringToMeasure">The string to measure</param>
        /// <param name="maxWidth">The maximum width available to display the string</param>
        /// <returns>The <see cref="Size"/> needed to contain the
        /// <paramref name="stringToMeasure"/> when drawn with the currently selected
        /// message box <see cref="Font"/>.
        /// </returns>
        Size MeasureString(string stringToMeasure, int maxWidth)
        {
            return MeasureString(stringToMeasure, maxWidth, this.Font);
        }

        /// <summary>
        /// Gets the longest button text.
        /// </summary>
        /// <returns>The longest <see cref="string"/> from the <see cref="List{T}"/>
        /// of <see cref="CustomizableMessageBoxButton"/></returns>
        string GetLongestButtonText()
        {
            int maxLen = 0;
            string maxStr = null;
            foreach (CustomizableMessageBoxButton button in _buttons)
            {
                if (button.Text != null && button.Text.Length > maxLen)
                {
                    maxLen = button.Text.Length;
                    maxStr = button.Text;
                }
            }

            return maxStr;
        }

        /// <summary>
        /// Sets the size and visibility of the Message
        /// </summary>
        void SetMessageSizeAndVisibility()
        {
            if (_rtbMessage.Text == null || _rtbMessage.Text.Trim().Length == 0)
            {
                _rtbMessage.Size = Size.Empty;
                _rtbMessage.Visible = false;
            }
            else
            {
                // Add space for the icon
                int maxWidth = _maxLayoutWidth;
                if (_panelIcon.Size.Width != 0)
                {
                    maxWidth = maxWidth - (_panelIcon.Size.Width + ICON_MESSAGE_PADDING);
                }

                //We need to account for scroll bar width and height, otherwise for certain
                //kinds of text the scroll bar shows up unnecessarily
                maxWidth = maxWidth - SystemInformation.VerticalScrollBarWidth;
                Size messageRectSize = MeasureString(_rtbMessage.Text, maxWidth);

                messageRectSize.Width += SystemInformation.VerticalScrollBarWidth;
                messageRectSize.Height = Math.Max(_panelIcon.Height, messageRectSize.Height)
                    + SystemInformation.HorizontalScrollBarHeight;

                _rtbMessage.Size = messageRectSize;
                _rtbMessage.Visible = true;
            }
        }

        /// <summary>
        /// Sets the size and visibility of the Icon
        /// </summary>
        void SetIconSizeAndVisibility()
        {
            if (_iconImage == null)
            {
                _panelIcon.Visible = false;
                _panelIcon.Size = Size.Empty;
            }
            else
            {
                _panelIcon.Size = new Size(32, 32);
                _panelIcon.Visible = true;
            }
        }

        /// <summary>
        /// Sets the size and visibility of the save response checkbox
        /// </summary>
        void SetCheckboxSizeAndVisibility()
        {
            if (!AllowSaveResponse)
            {
                chbSaveResponse.Visible = false;
                chbSaveResponse.Size = Size.Empty;
            }
            else
            {
                Size saveResponseTextSize = MeasureString(chbSaveResponse.Text, _maxLayoutWidth);
                saveResponseTextSize.Width += CHECKBOX_WIDTH;
                chbSaveResponse.Size = saveResponseTextSize;
                chbSaveResponse.Visible = true;
            }
        }

        /// <summary>
        /// Calculates the button size based on the text of the longest
        /// button text
        /// </summary>
        /// <returns></returns>
        Size GetButtonSize()
        {
            // If GetLongestButtonText() returns null then set the text string to "Ok"
            string longestButtonText = GetLongestButtonText() ?? "Ok";

            Size buttonTextSize = MeasureString(longestButtonText, _maxLayoutWidth);
            Size buttonSize = new Size(buttonTextSize.Width
                + BUTTON_LEFT_PADDING + BUTTON_RIGHT_PADDING, buttonTextSize.Height
                + BUTTON_TOP_PADDING + BUTTON_BOTTOM_PADDING);

            // Enforce minimum button width and height
            buttonSize.Width = Math.Max(buttonSize.Width, MIN_BUTTON_WIDTH);
            buttonSize.Height = Math.Max(buttonSize.Height, MIN_BUTTON_HEIGHT);

            return buttonSize;
        }

        /// <summary>
        /// Set the icon
        /// </summary>
        /// <param name="icon">The standard <see cref="MessageBoxIcon"/> to
        /// display in this message box.</param>
        /// <exception cref="ExtractException">Thrown if unrecognized
        /// <see cref="MessageBoxIcon"/> value is passed in.</exception>
        void SetStandardIcon(MessageBoxIcon icon)
        {
            _standardIcon = icon;

            switch (icon)
            {
                case MessageBoxIcon.Asterisk:
                    _iconImage = SystemIcons.Asterisk;
                    break;

                case MessageBoxIcon.Error:
                    _iconImage = SystemIcons.Error;
                    break;

                case MessageBoxIcon.Exclamation:
                    _iconImage = SystemIcons.Exclamation;
                    break;

                case MessageBoxIcon.Question:
                    _iconImage = SystemIcons.Question;
                    break;

                case MessageBoxIcon.None:
                    _iconImage = null;
                    break;

                default:
                    ExtractException.ThrowLogicException("ELI21683");
                    break;
            }
        }

        /// <summary>
        /// Will ensure that there is at least one button on the message box.
        /// </summary>
        void AddOkButtonIfNoButtonsPresent()
        {
            if (_buttons.Count == 0)
            {
                CustomizableMessageBoxButton okButton = new CustomizableMessageBoxButton();
                okButton.Text = "&Ok";
                okButton.Value = CustomizableMessageBoxButtons.Ok.ToString();

                _buttons.Add(okButton);
            }
        }


        /// <summary>
        /// Centers the form on the screen
        /// </summary>
        void CenterForm()
        {
            int x = (SystemInformation.WorkingArea.Width - this.Width) / 2;
            int y = (SystemInformation.WorkingArea.Height - this.Height) / 2;

            this.Location = new Point(x, y);
        }

        /// <summary>
        /// Sets the optimum size for the form based on the controls that
        /// need to be displayed
        /// </summary>
        void SetOptimumSize()
        {
            int ncWidth = this.Width - this.ClientSize.Width;
            int ncHeight = this.Height - this.ClientSize.Height;

            // Get the width of all of the controls
            int iconAndMessageRowWidth = _rtbMessage.Width + ICON_MESSAGE_PADDING + _panelIcon.Width;
            int saveResponseRowWidth = chbSaveResponse.Width + (int)(_panelIcon.Width / 2);
            int buttonsRowWidth = GetWidthOfAllButtons();
            int captionWidth = GetCaptionSize().Width;

            // Compute the maximum required width
            int maxItemWidth = Math.Max(saveResponseRowWidth,
                Math.Max(iconAndMessageRowWidth, buttonsRowWidth));

            // Compute the required width for the message box
            int requiredWidth = LEFT_PADDING + maxItemWidth + RIGHT_PADDING + ncWidth;

            //Since Caption width is not client width, we do the check here
            if (requiredWidth < captionWidth)
            {
                requiredWidth = captionWidth;
            }

            // Get the required height of the message box
            int requiredHeight = TOP_PADDING + Math.Max(_rtbMessage.Height, _panelIcon.Height)
                + ITEM_PADDING + GetButtonSize().Height + BOTTOM_PADDING + ncHeight
                + (chbSaveResponse.Height != 0 ? chbSaveResponse.Height + ITEM_PADDING : 0);

            //Fix the bug where if the message text is huge then the buttons are overwritten.
            //In case the required height is more than the max height then adjust that in the
            //message height
            if (requiredHeight > _maxHeight)
            {
                _rtbMessage.Height -= requiredHeight - _maxHeight;
            }

            // Compute the new size for the message box
            int height = Math.Min(requiredHeight, _maxHeight);
            int width = Math.Min(requiredWidth, _maxWidth);
            this.Size = new Size(width, height);
        }

        /// <summary>
        /// Returns the width that will be occupied by all buttons including
        /// the inter-button padding
        /// </summary>
        /// <returns>The total width required for the buttons along with padding.</returns>
        int GetWidthOfAllButtons()
        {
            Size buttonSize = GetButtonSize();
            int allButtonsWidth = buttonSize.Width * _buttons.Count 
                + BUTTON_PADDING * (_buttons.Count - 1);

            return allButtonsWidth;
        }

        /// <summary>
        /// Gets the width of the caption
        /// </summary>
        Size GetCaptionSize()
        {
            Font captionFont = NativeMethods.GetCaptionFont();
            if (captionFont == null)
            {
                //some error occured while determining system font
                captionFont = new Font("Tahoma", 11);
            }

            int availableWidth = _maxWidth - SystemInformation.CaptionButtonSize.Width
                - SystemInformation.Border3DSize.Width * 2;
            Size captionSize = MeasureString(this.Text, availableWidth, captionFont);

            captionSize.Width += SystemInformation.CaptionButtonSize.Width
                + SystemInformation.Border3DSize.Width * 2;
            return captionSize;
        }

        /// <summary>
        /// Layout all the controls 
        /// </summary>
        void LayoutControls()
        {
            // Layout the icon panel
            _panelIcon.Location = new Point(LEFT_PADDING, TOP_PADDING);

            // Compute the location of the message text
            _rtbMessage.Location = new Point(LEFT_PADDING + _panelIcon.Width
                + ICON_MESSAGE_PADDING * (_panelIcon.Width == 0 ? 0 : 1), TOP_PADDING);

            // Compute the location of the save response checkbox
            chbSaveResponse.Location = new Point(LEFT_PADDING + (int)(_panelIcon.Width / 2),
                TOP_PADDING + Math.Max(_panelIcon.Height, _rtbMessage.Height) + ITEM_PADDING);

            // Get the button size
            Size buttonSize = GetButtonSize();

            // Get the width of all buttons
            int allButtonsWidth = GetWidthOfAllButtons();

            // Compute the location for the first button
            int firstButtonX = ((int)(this.ClientSize.Width - allButtonsWidth) / 2);
            int firstButtonY = this.ClientSize.Height - BOTTOM_PADDING - buttonSize.Height;
            Point nextButtonLocation = new Point(firstButtonX, firstButtonY);

            // Place the first button on the message box and store its value
            // so that if no button has been defined as the default button
            // the first button can be marked as the default.  There will always be at
            // least one button on the message box.
            Button firstButton = GetButton(_buttons[0], buttonSize, nextButtonLocation);
            nextButtonLocation.X += buttonSize.Width + BUTTON_PADDING;

            // Iterate through each button and set its location
            bool foundDefaultButton = false;
            for (int i = 1; i < _buttons.Count; i++)
            {
                CustomizableMessageBoxButton button = _buttons[i];

                // Get the button
                Button buttonCtrl = GetButton(button, buttonSize, nextButtonLocation);

                // Set the default button for the message box.
                // NOTE: If more than one button is marked default, only the first one
                // will get the default property.
                if (!foundDefaultButton && button.IsDefaultButton)
                {
                    _defaultButtonControl = buttonCtrl;
                    foundDefaultButton = true;
                }

                // Move the location for next button
                nextButtonLocation.X += buttonSize.Width + BUTTON_PADDING;
            }

            // If no default button was found, set the first button as the default
            if (!foundDefaultButton)
            {
                _defaultButtonControl = firstButton;
            }
        }

        /// <summary>
        /// Gets the button control for the specified CustomizableMessageBoxButton, if the
        /// control has not been created this method creates the control
        /// </summary>
        /// <param name="button">The <see cref="CustomizableMessageBoxButton"/> struct that
        /// defines the button to get.</param>
        /// <param name="size">The <see cref="Size"/> for this button.</param>
        /// <param name="location">The location on the form for this button.</param>
        /// <returns>The <see cref="Button"/> described by the <see cref="CustomizableMessageBoxButton"/>
        /// struct.</returns>
        Button GetButton(CustomizableMessageBoxButton button, Size size, Point location)
        {
            // Try to get the button from the list, if it is not there then create a new button
            Button buttonCtrl = null;
            if (_buttonControlsTable.TryGetValue(button, out buttonCtrl))
            {
                buttonCtrl.Size = size;
                buttonCtrl.Location = location;
            }
            else
            {
                buttonCtrl = CreateButton(button, size, location);
                _buttonControlsTable.Add(button, buttonCtrl);
                this.Controls.Add(buttonCtrl);
            }

            return buttonCtrl;
        }

        /// <summary>
        /// Creates a button control based on info from CustomizableMessageBoxButton
        /// </summary>
        /// <param name="button">The <see cref="CustomizableMessageBoxButton"/> struct that
        /// defines the button to create.</param>
        /// <param name="size">The <see cref="Size"/> for this button.</param>
        /// <param name="location">The location on the form for this button.</param>
        /// <returns>The <see cref="Button"/> described by the <see cref="CustomizableMessageBoxButton"/>
        /// struct.</returns>
        Button CreateButton(CustomizableMessageBoxButton button, Size size, Point location)
        {
            // Create a new button with the specified size and text
            Button buttonCtrl = new Button();
            buttonCtrl.Size = size;
            buttonCtrl.Text = button.Text;

            // Set the text alignment and flatstyle
            buttonCtrl.TextAlign = ContentAlignment.MiddleCenter;
            buttonCtrl.FlatStyle = FlatStyle.System;

            // Set tooltip text if it has been specified
            if (!string.IsNullOrEmpty(button.ToolTipText))
            {
                buttonToolTip.SetToolTip(buttonCtrl, button.ToolTipText);
            }

            // Set the location for the button on the form
            buttonCtrl.Location = location;

            // Add the return value for the button
            buttonCtrl.Tag = button.Value;

            // Add the event handler to the button
            buttonCtrl.Click += HandleButtonClicked;

            return buttonCtrl;
        }

        /// <summary>
        /// Will disable the close event if there are multiple buttons and no cancel button.
        /// </summary>
        void DisableCloseIfMultipleButtonsAndNoCancelButton()
        {
            if (_buttons.Count > 1)
            {
                if (_cancelButton != null)
                    return;

                //See if standard cancel button is present
                foreach (CustomizableMessageBoxButton button in _buttons)
                {
                    if (button.Text == CustomizableMessageBoxButtons.Cancel.ToString()
                        && button.Value == CustomizableMessageBoxButtons.Cancel.ToString())
                    {
                        _cancelButton = button;
                        return;
                    }
                }

                //Standard cancel button is not present, Disable
                //close button
                NativeMethods.DisableCloseButton(this);
                _allowCancel = false;

            }
            else if (_buttons.Count == 1)
            {
                _cancelButton = _buttons[0] as CustomizableMessageBoxButton;
            }
            else
            {
                //This condition should never get called
                _allowCancel = false;
            }
        }

        /// <summary>
        /// Plays the alert sound based on the icon set for the message box
        /// </summary>
        void PlayAlert()
        {
            if (_playAlert)
            {
                // Play the alert sound, display an exception if an error occurs
                if (!NativeMethods.MessageBeep(_standardIcon != MessageBoxIcon.None ?
                    (uint)_standardIcon : 0))
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI21687",
                        new Win32Exception(Marshal.GetLastWin32Error()));
                    ee.Display();
                }
            }
        }

        /// <summary>
        /// Sets the default button control to the selected state.
        /// </summary>
        void SelectDefaultButton()
        {
            if (_defaultButtonControl != null)
            {
                _defaultButtonControl.Select();
            }
        }

        /// <summary>
        /// If the timeout value has been set greater than 0 then this function will
        /// start a timer and add the event handler to handle the timeout event.
        /// </summary>
        void StartTimerIfTimeoutGreaterThanZero()
        {
            if (_timeout > 0)
            {
                if (_timeoutTimer == null)
                {
                    _timeoutTimer = new Timer(this.components);
                    _timeoutTimer.Tick += HandleTimeoutTimerTick;
                }

                if (!_timeoutTimer.Enabled)
                {
                    _timeoutTimer.Interval = _timeout;
                    _timeoutTimer.Start();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:KeyPress"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing
        /// the event data.</param>
        void OnKeyPress(KeyEventArgs e)
        {
            if (KeyPress != null)
            {
                KeyPress(this, e);
            }
        }

        /// <summary>
        /// Will set the result value for the message box and close it.
        /// </summary>
        /// <param name="result">The value to set as the message box result.</param>
        internal void SetResultAndClose(string result)
        {
            try
            {
                _result = result;
                this.Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32350");
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for each of the buttons
        /// on the message box.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The data associated with this event.</param>
        void HandleButtonClicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn == null || btn.Tag == null)
                {
                    return;
                }

                string result = btn.Tag as string;
                SetResultAndClose(result);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI21686");
            }
        }

        /// <summary>
        /// Handles the <see cref="Timer.Tick"/> event.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleTimeoutTimerTick(object sender, EventArgs e)
        {
            try
            {
                _timeoutTimer.Stop();

                switch (_timeoutResult)
                {
                    case TimeoutResult.Default:
                        _defaultButtonControl.PerformClick();
                        break;

                    case TimeoutResult.Cancel:
                        if (_cancelButton != null)
                        {
                            SetResultAndClose(_cancelButton.Value);
                        }
                        else
                        {
                            _defaultButtonControl.PerformClick();
                        }
                        break;

                    case TimeoutResult.Timeout:
                        SetResultAndClose(CustomizableMessageBoxResult.Timeout);
                        break;

                    default:
                        ExtractException.ThrowLogicException("ELI21691");
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI21690");
            }
        }

        #endregion Event Handlers
    }
}
