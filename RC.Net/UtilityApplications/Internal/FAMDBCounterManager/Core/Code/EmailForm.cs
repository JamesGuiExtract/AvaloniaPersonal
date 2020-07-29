using Extract.Licensing.Internal;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    ///  A <see cref="Form"/> to view, edit and send a FAM DB counter update email.
    /// </summary>
    internal partial class EmailForm : Form
    {
        #region Fields

        /// <summary>
        /// The email settings to be used.
        /// </summary>
        EmailSettingsManager _settings = new EmailSettingsManager();

        /// <summary>
        /// Indicates whether the controls with the email body text are in the process of being
        /// sized to avoid inappropriate recursion.
        /// </summary>
        bool _sizingEmailBody = false;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailForm"/> class.
        /// </summary>
        /// <param name="databaseInfo">Represents counter related info for FAM DB secure counters.</param>
        /// <param name="counterOperationInfo">A <see cref="CounterOperationInfo"/> containing
        /// information about the counter update to be applied.</param>
        public EmailForm(DatabaseInfo databaseInfo, CounterOperationInfo counterOperationInfo)
        {
            _settings.DatabaseInfo = databaseInfo;
            _settings.CounterOperationInfo = counterOperationInfo;

            InitializeComponent();
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _flexLicenseEmailTextBox.Text = _settings.SenderName + ";";
                _subjectTextBox.Text = _settings.GetSubject();
                _editableBodyTextBox.Text = _settings.GetEditableBody();
                _readOnlyBodyTextBox.Text = _settings.GetReadonlyBody();
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> event.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Check that all email fields have been filled in correctly.
                if (WarnIfInvalid())
                {
                    return;
                }

                // Send the email represented by this form's current data.
                var email = new EmailMessage();
                email.EmailSettings = _settings;
                email.Recipients = ParseDelimitedList(_recipientTextBox.Text);
                email.CarbonCopyRecipients = ParseDelimitedList(
                    _settings.SenderAddress, _carbonCopyRecipientTextBox.Text);
                email.Subject = _settings.GetSubject();
                email.Body = _editableBodyTextBox.Text + "\r\n" + _readOnlyBodyTextBox.Text;
                email.Send();

                UtilityMethods.ShowMessageBox("The email has been sent.", "Email Sent", false);

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event of the
        /// <see cref="_flexLicenseEmailTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleFlexLicenseEmailTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var textBox = (TextBoxBase)sender;
                var table = (TableLayoutPanel)textBox.Parent;
                int index = table.GetRow(textBox);

                using (Graphics graphics = CreateGraphics())
                {
                    SizeF size = graphics.MeasureString(textBox.Text, textBox.Font);
                    _ccTableLayoutPanel.ColumnStyles[index].Width = (int)Math.Ceiling(size.Width);
                }
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event of the <see cref="_editableBodyTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleEditableBodyTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                SizeEmailBodyTable();
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.SizeChanged"/> event of the <see cref="_bodyPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleBodyPanel_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                SizeEmailBodyTable();
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragEnter"/> event of the <see cref="_recipientTextBox"/>
        /// or <see cref="_carbonCopyRecipientTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        void HandleRecipient_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragDrop"/> event of the <see cref="_recipientTextBox"/>
        /// or <see cref="_carbonCopyRecipientTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        void HandleRecipient_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    var textBox = (TextBox)sender;
                    string droppedText = (string)e.Data.GetData(DataFormats.Text);
                    textBox.Text = string.Join(";", ParseDelimitedList(textBox.Text, droppedText));
                }
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        /// <summary>
        /// Handles the click event for the "Copy to clipboard" button
        /// </summary>
        private void CopyToClipboardButton_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(_readOnlyBodyTextBox.Text);
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(_recipientTextBox.Text))
            {
                _recipientTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify the email recipient.",
                    "Missing recipient", true);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_subjectTextBox.Text))
            {
                _recipientTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify the email subject.",
                    "Missing subject", true);
                return true;
            }

            if (string.IsNullOrEmpty(_settings.Server))
            {
                UtilityMethods.ShowMessageBox("The outgoing email server settings have not " +
                    "been specified in the corresponding config file.",
                    "Outgoing email server not configured", true);
            }   

            return false;
        }

        /// <summary>
        /// Sizes the <see cref="_bodyTableLayoutPanel"/> rows based on the text contained in
        /// <see cref="_editableBodyTextBox"/> and <see cref="_readOnlyBodyTextBox"/> such that each
        /// control is large enough to fix the text in contains without scrolling and the only
        /// scroll bar shown is in <see cref="_bodyPanel"/> which will scroll the body as a whole,
        /// not just the individual pieces.
        /// </summary>
        void SizeEmailBodyTable()
        {
            // Prevent recursion method would otherwise cause.
            if (_sizingEmailBody)
            {
                return;
            }

            try
            {
                _sizingEmailBody = true;
                // Suspend layout to prevent flicker from scroll bars appearing/disappearing in the
                // midst of the resizing.
                _bodyPanel.SuspendLayout();
                SizeMultilineTextBoxRow(_editableBodyTextBox, false);
                // Allow _readOnlyBodyTextBox to fill the balance of any extra space left over after
                // the text controls have been sized.
                SizeMultilineTextBoxRow(_readOnlyBodyTextBox, true);
            }
            finally
            {
                _sizingEmailBody = false;
                _bodyPanel.ResumeLayout(true);
            }
        }

        /// <summary>
        /// Updates the size of the <see cref="_bodyTableLayoutPanel"/> row containing the specified
        /// <see cref="textBox"/> such that it is large enough to fit all the text it contains
        /// without scrolling.
        /// </summary>
        /// <param name="textBox">Should be either <see cref="_editableBodyTextBox"/> or
        /// <see cref="_readOnlyBodyTextBox"/>.</param>
        /// <param name="fill"><see langword="true"/> if <see paramref="textBox"/> should be sized
        /// to fill any leftover vertical space in _bodyPanel; <see langword="false"/> if it should
        /// always only be just large enough to fit its content.</param>
        void SizeMultilineTextBoxRow(TextBoxBase textBox, bool fill)
        {
            var table = (TableLayoutPanel)textBox.Parent;
            int index = table.GetRow(textBox);

            using (Graphics graphics = textBox.CreateGraphics())
            {
                // Measure the height of a line of text in the textbox's font & graphics.
                int lineHeight = (int)Math.Ceiling(textBox.Font.GetHeight(graphics));

                int newHeight;
                if (textBox.Lines.Length <= 1)
                {
                    // There is only a single line of text.
                    newHeight = lineHeight;
                }
                else
                {
                    // If there are multiple lines of text, multiplying the number of lines by line
                    // height may become inaccurate due to round-off (or so I fear). The required
                    // height should essentially be the Y position of the last row of text - the Y
                    // position of the first row + lineHeight.
                    newHeight = textBox.GetPositionFromCharIndex(textBox.Text.Length - 1).Y
                        + lineHeight;

                    // Except if the text ends with a newline, GetPositionFromCharIndex will return
                    // 0; the Y position needs to be manually accounted for.
                    if (textBox.Text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        newHeight += lineHeight;
                    }

                    int firstCharPos = textBox.GetPositionFromCharIndex(0).Y;
                    if (firstCharPos < 1)
                    {
                        newHeight += -firstCharPos + 1;
                    }
                }

                // I found I need to add a bit of extra padding to the calculated height, probably
                // to account for the padding above and below the text and the edge of the client
                // area.
                newHeight += (textBox.Size.Height - textBox.ClientSize.Height) + 3;
                
                // If fill is true, calculate the unused vertical real estate and add it to
                // newHeight.
                if (fill)
                {
                    // Height used so far is newHeight plus the height of the rows other than this
                    // one.
                    int usedTableHeight = newHeight +
                        (int)Math.Ceiling(table.RowStyles
                            .OfType<RowStyle>()
                            .Except(new[] { table.RowStyles[index] })
                            .Sum(rowStyle => rowStyle.Height));

                    // Account for text control borders (client height vs control height).
                    usedTableHeight += table.Controls
                        .OfType<Control>()
                        .Sum(control => control.Height - control.ClientSize.Height);

                    // If there is any extra vertical area available in _bodyPanel, use it.
                    if (usedTableHeight < _bodyPanel.ClientSize.Height)
                    {
                        newHeight += (_bodyPanel.ClientSize.Height - usedTableHeight);
                    }
                }

                table.RowStyles[index].Height = newHeight;
            }
        }

        /// <summary>
        /// Parses the delimited list.
        /// </summary>
        /// <param name="delimitedLists">The delimited list.</param>
        /// <returns></returns>
        string[] ParseDelimitedList(params string[] delimitedLists)
        {
            // Get all values delimited by , or ; that aren't 100% whitespace.
            var items = delimitedLists[0]
                .Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s));

            if (delimitedLists.Length > 1)
            {
                // Recurs to combine results with additional lists to be parsed.
                return items.Concat(
                    ParseDelimitedList(delimitedLists.Skip(1).ToArray()))
                    .ToArray();
            }
            else
            {
                return items.ToArray();
            }
        }

        #endregion Private Members
    }
}
