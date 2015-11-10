using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// A <see cref="Form"/> that allows secure counter update or unlock codes from Extract to be
    /// applied.
    /// </summary>
    internal partial class ApplyCounterUpdateForm : Form, IMessageFilter
    {
        #region Fields

        /// <summary>
        /// The <see cref="FileProcessingDB"/> to which the update/unlock code will be applied.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// <see langword="true"/> if the form is to accept an unlock code; <see langword="false"/>
        /// if it is to accept an update code.
        /// </summary>
        bool _unlockCode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyCounterUpdateForm"/> class.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> to which the
        /// update/unlock code will be applied.</param>
        /// <param name="unlockCode"><see langword="true"/> if the form is to accept an unlock code;
        /// <see langword="false"/> if it is to accept an update code.</param>
        public ApplyCounterUpdateForm(FileProcessingDB fileProcessingDB, bool unlockCode)
        {
            try
            {
                _fileProcessingDB = fileProcessingDB;

                InitializeComponent();

                _unlockCode = unlockCode;
                if (unlockCode)
                {
                    Text = "Apply Counter Unlock Code";
                    _updateCodeLabel.Text = "Counter unlock code:";
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39044");
            }
        }

        #endregion Constructors

        #region IMessageFilter

        /// <summary>
        /// Filters out a message before it is dispatched.
        /// </summary>
        /// <param name="m">The message to be dispatched. You cannot modify this message.</param>
        /// <returns>
        /// true to filter the message and stop it from being dispatched; false to allow the message
        /// to continue to the next filter or control.
        /// </returns>
        public bool PreFilterMessage(ref Message m)
        {
            try
            {
                // Handle Ctrl + V into the _counterUpdateCodeTextBox as a paste operation.
                if (m.Msg == 0x100 && m.WParam == (IntPtr)Keys.V &&
                   Control.ModifierKeys.HasFlag(Keys.Control) &&
                   ActiveControl == _counterUpdateCodeTextBox)
                {
                    ProcessInputText(Clipboard.GetText(), "pasted text");
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39045");
            }

            return false;
        }

        #endregion IMessageFilter

        #region Overloads

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // So that PreFilterMessage is called.
                Application.AddMessageFilter(this);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39046");
            }
        }

        #endregion Overloads

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.DragEnter"/> event of the
        /// <see cref="_counterUpdateCodeTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing
        /// the event data.</param>
        void HandleCounterUpdateCodeTextBox_DragEnter(object sender, DragEventArgs e)
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
                ex.ExtractDisplay("ELI39047");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragDrop"/> event of the
        /// <see cref="_counterUpdateCodeTextBox"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing
        /// the event data.</param>
        void HandleCounterUpdateCodeTextBox_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    string droppedText = (string)e.Data.GetData(DataFormats.Text);
                    ProcessInputText(droppedText, "dropped text");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39048");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_pasteCodeButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePasteCodeButton_Click(object sender, EventArgs e)
        {
            try
            {
                string pastedText = Clipboard.GetText();
                ProcessInputText(pastedText, "pasted text");
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39049");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_loadCodeFromFileButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleLoadCodeFromFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = FormsMethods.BrowseForFile(
                    "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));

                _counterUpdateCodeTextBox.Text = "";
                _applyButton.Enabled = false;

                if (!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
                {
                    string fileText = File.ReadAllText(fileName);
                    ProcessInputText(fileText, "file text");
                }
                else
                {
                    UtilityMethods.ShowMessageBox("The specified file could not be found.",
                        "File not found", true);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39050");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_applyButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleApplyButton_Click(object sender, EventArgs e)
        {
            try
            {
                string appliedChanges =
                    _fileProcessingDB.ApplySecureCounterUpdateCode(_counterUpdateCodeTextBox.Text);
                appliedChanges = appliedChanges.Trim();

                using (var sendConfirmationForm =
                    new SendCounterConfirmationForm(_fileProcessingDB, appliedChanges, _unlockCode))
                {
                    sendConfirmationForm.ShowDialog(this);

                    // After sending confirmation of the update, close this apply code window.
                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39051");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Attempts to parse a counter update/unlock code out of the specified
        /// <see paramref="inputText"/>.
        /// </summary>
        /// <param name="inputText">The text from which the code should be parsed.</param>
        /// <param name="inputSource">The source of the text (for use in message box text).</param>
        void ProcessInputText(string inputText, string inputSource)
        {
            string code = SecureCounterTextManipulator.ParseLicenseCode(inputText);

            if (string.IsNullOrWhiteSpace(code))
            {
                UtilityMethods.ShowMessageBox(
                    "Could not find a valid code in " + inputSource + ".", "No code found", true);
                _counterUpdateCodeTextBox.Text = "";
                _applyButton.Enabled = false;
            }
            else
            {
                _counterUpdateCodeTextBox.Text = code;
                _applyButton.Enabled = true;
            }
        }

        #endregion Private Members
    }
}
