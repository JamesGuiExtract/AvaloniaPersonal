using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging
{
    /// <summary>
    /// Dialog for configuring <see cref="PdfPasswordSettings"/>.
    /// </summary>
    public partial class PdfPasswordSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// The settings object that will be modified by this dialog.
        /// </summary>
        readonly PdfPasswordSettings _settings;

        /// <summary>
        /// The error provider used to indicate that password fields do not match.
        /// </summary>
        ErrorProvider _errorProvider = new ErrorProvider();

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="PdfPasswordSettingsDialog"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPasswordSettingsDialog"/> class.
        /// </summary>
        public PdfPasswordSettingsDialog()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPasswordSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings to initialize the dialog with.</param>
        public PdfPasswordSettingsDialog(PdfPasswordSettings settings)
        {
            InitializeComponent();

            _settings = new PdfPasswordSettings(settings);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the settings from the dialog.
        /// </summary>
        public PdfPasswordSettings Settings
        {
            get
            {
                return _settings;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleEnablePasswordCheckboxClicked(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29729", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleUserPasswordTextChanged(object sender, EventArgs e)
        {
            try
            {
                string errorString = "";
                if ((_userPasswordText1.Text.Length != 0 || _userPasswordText2.Text.Length != 0)
                    && _userPasswordText1.Text != _userPasswordText2.Text)
                {
                    errorString = "Passwords do not match";
                }

                _errorProvider.SetError(_userPasswordText1, errorString);
                _errorProvider.SetError(_userPasswordText2, errorString);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29730", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleOwnerPasswordTextChanged(object sender, EventArgs e)
        {
            try
            {
                string errorString = "";
                if ((_ownerPasswordText1.Text.Length != 0 || _ownerPasswordText2.Text.Length != 0)
                    && _ownerPasswordText1.Text != _ownerPasswordText2.Text)
                {
                    errorString = "Passwords do not match";
                }

                _errorProvider.SetError(_ownerPasswordText1, errorString);
                _errorProvider.SetError(_ownerPasswordText2, errorString);

                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29731", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleDisplayPasswordsChecked(object sender, EventArgs e)
        {
            try
            {
                bool display = !_displayPasswordsCheck.Checked;

                _userPasswordText1.UseSystemPasswordChar = display;
                _userPasswordText2.UseSystemPasswordChar = display;
                _ownerPasswordText1.UseSystemPasswordChar = display;
                _ownerPasswordText2.UseSystemPasswordChar = display;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29732", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleOkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29733", ex);
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Updates the controls to the proper enabled/disabled state.
        /// </summary>
        void UpdateControls()
        {
            try
            {
                bool userEnabled = _enableUserPasswordCheckBox.Checked;
                bool ownerEnabled = _enableOwnerPasswordCheckBox.Checked;
                bool permissionEnabled = ownerEnabled
                    && _ownerPasswordText1.Text.Length > 0
                    && _ownerPasswordText2.Text == _ownerPasswordText1.Text;

                _userPasswordText1.Enabled = userEnabled;
                _userPasswordText2.Enabled = userEnabled;

                _ownerPasswordText1.Enabled = ownerEnabled;
                _ownerPasswordText2.Enabled = ownerEnabled;

                _allowAccessibilityCheck.Enabled = permissionEnabled;
                _allowAddOrModifyAnnotationsCheck.Enabled = permissionEnabled;
                _allowCopyAndExtractionCheck.Enabled = permissionEnabled;
                _allowDocumentAssemblyCheck.Enabled = permissionEnabled;
                _allowDocumentModificationsCheck.Enabled = permissionEnabled;
                _allowFillInFormFieldsCheck.Enabled = permissionEnabled;
                _allowHighQualityPrintingCheck.Enabled = permissionEnabled;
                _allowLowQualityPrintCheck.Enabled = permissionEnabled;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29734", ex);
            }
        }

        #endregion Methods
    }
}