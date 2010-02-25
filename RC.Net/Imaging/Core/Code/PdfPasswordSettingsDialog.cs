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

            // Flashing error icons are unnecessary
            _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
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

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The data associated with this event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // Get the user password
                string userPassword = _settings.UserPassword;
                bool userPasswordSpecified = !string.IsNullOrEmpty(userPassword);

                // Get the owner password
                string ownerPassword = _settings.OwnerPassword;
                bool ownerPasswordSpecified = !string.IsNullOrEmpty(ownerPassword);

                // If both passwords are required then check both boxes
                // and disable them
                if (_settings.RequireUserAndOwnerPassword)
                {
                    _enableUserPasswordCheckBox.Checked = true;
                    _enableUserPasswordCheckBox.Enabled = false;
                    _enableOwnerPasswordCheckBox.Checked = true;
                    _enableOwnerPasswordCheckBox.Enabled = false;
                }
                else
                {
                    _enableUserPasswordCheckBox.Checked = userPasswordSpecified;
                    _enableOwnerPasswordCheckBox.Checked = ownerPasswordSpecified;
                }

                if (userPasswordSpecified)
                {
                    _userPasswordText1.Text = userPassword;
                    _userPasswordText2.Text = userPassword;
                }
                if (ownerPasswordSpecified)
                {
                    _ownerPasswordText1.Text = ownerPassword;
                    _ownerPasswordText2.Text = ownerPassword;

                    // Get the permissions
                    PdfOwnerPermissions permissions = _settings.OwnerPermissions;

                    bool allowHighQualityPrint =
                        (permissions & PdfOwnerPermissions.AllowHighQualityPrinting) == PdfOwnerPermissions.AllowHighQualityPrinting;
                    bool allowPrinting = allowHighQualityPrint
                        && (permissions & PdfOwnerPermissions.AllowLowQualityPrinting) == PdfOwnerPermissions.AllowLowQualityPrinting;
                    _allowAccessibilityCheck.Checked =
                        (permissions & PdfOwnerPermissions.AllowContentCopyingForAccessibility) == PdfOwnerPermissions.AllowContentCopyingForAccessibility;
                    _allowAddOrModifyAnnotationsCheck.Checked =
                        (permissions & PdfOwnerPermissions.AllowAddingModifyingAnnotations) == PdfOwnerPermissions.AllowAddingModifyingAnnotations;
                    _allowCopyAndExtractionCheck.Checked =
                        (permissions & PdfOwnerPermissions.AllowContentCopying) == PdfOwnerPermissions.AllowContentCopying;
                    _allowDocumentAssemblyCheck.Checked =
                        (permissions & PdfOwnerPermissions.AllowDocumentAssembly) == PdfOwnerPermissions.AllowDocumentAssembly;
                    _allowDocumentModificationsCheck.Checked =
                        (permissions & PdfOwnerPermissions.AllowDocumentModifications) == PdfOwnerPermissions.AllowDocumentModifications;
                    _allowFillInFormFieldsCheck.Checked =
                        (permissions & PdfOwnerPermissions.AllowFillingInFields) == PdfOwnerPermissions.AllowFillingInFields;
                    _allowHighQualityPrintingCheck.Checked = allowHighQualityPrint;
                    _allowLowQualityPrintCheck.Checked = allowPrinting;
                }

                // Update the controls to set appropriate enabled/disabled states
                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29751", ex);
            }
        }

        #endregion Overrides

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
        void HandleOkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                bool userChecked = _enableUserPasswordCheckBox.Checked;
                bool ownerChecked = _enableOwnerPasswordCheckBox.Checked;
                if (_settings.RequireUserAndOwnerPassword)
                {
                    if (!userChecked || !ownerChecked)
                    {
                        MessageBox.Show(
                            "This object requires both the user and owner passwords to be set.",
                            "Define both Passwords",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _enableUserPasswordCheckBox.Focus();
                        return;
                    }
                }
                else if (!userChecked && !ownerChecked)
                {
                    MessageBox.Show("At least one password must be specified.", "No Passwords",
                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);
                    _enableUserPasswordCheckBox.Focus();
                    return;
                }

                string userPassword = "";
                if (userChecked)
                {
                    string pass1 = _userPasswordText1.Text;
                    string pass2 = _userPasswordText2.Text;
                    if (string.IsNullOrEmpty(pass1) && string.IsNullOrEmpty(pass2))
                    {
                        MessageBox.Show("Please specify a user password.", "No User Password",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _userPasswordText1.Focus();
                        return;
                    }
                    if (pass1 != pass2)
                    {
                        MessageBox.Show("Passwords do not match.", "Password Mismatch",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _userPasswordText1.Focus();
                        return;
                    }

                    userPassword = pass1;
                }

                string ownerPassword = "";
                PdfOwnerPermissions permissions = PdfOwnerPermissions.DisallowAll;

                if (ownerChecked)
                {
                    string pass1 = _ownerPasswordText1.Text;
                    string pass2 = _ownerPasswordText2.Text;
                    if (string.IsNullOrEmpty(pass1) && string.IsNullOrEmpty(pass2))
                    {
                        MessageBox.Show("Please specify an owner password.", "No Owner Password",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _ownerPasswordText1.Focus();
                        return;
                    }
                    if (pass1 != pass2)
                    {
                        MessageBox.Show("Passwords do not match.", "Password Mismatch",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _ownerPasswordText1.Focus();
                        return;
                    }

                    ownerPassword = pass1;

                    // Get the check box settings
                    if (_allowAccessibilityCheck.Checked)
                    {
                        permissions |= PdfOwnerPermissions.AllowContentCopyingForAccessibility;
                    }
                    if (_allowAddOrModifyAnnotationsCheck.Checked)
                    {
                        permissions |= PdfOwnerPermissions.AllowAddingModifyingAnnotations;
                    }
                    if (_allowCopyAndExtractionCheck.Checked)
                    {
                        permissions |= PdfOwnerPermissions.AllowContentCopying;
                    }
                    if (_allowDocumentAssemblyCheck.Checked)
                    {
                        permissions |= PdfOwnerPermissions.AllowDocumentAssembly;
                    }
                    if (_allowDocumentModificationsCheck.Checked)
                    {
                        permissions |= PdfOwnerPermissions.AllowDocumentModifications;
                    }
                    if (_allowFillInFormFieldsCheck.Checked)
                    {
                        permissions |= PdfOwnerPermissions.AllowFillingInFields;
                    }
                    if (_allowHighQualityPrintingCheck.Checked)
                    {
                        permissions |= PdfOwnerPermissions.AllowHighQualityPrinting;
                    }
                    if (_allowLowQualityPrintCheck.Checked)
                    {
                        permissions |= PdfOwnerPermissions.AllowLowQualityPrinting;
                    }
                }

                // Ensure that both passwords are different
                if (userPassword == ownerPassword)
                {
                    MessageBox.Show("User and owner passwords must be different.",
                        "User and Owner Passwords Match",
                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);
                    _userPasswordText1.Focus();
                    return;
                }

                // Store the settings
                _settings.UserPassword = userPassword;
                _settings.OwnerPassword = ownerPassword;
                _settings.OwnerPermissions = permissions;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29733", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event for
        /// checkboxes that change.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleCheckBoxChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29794", ex);
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

                _allowCopyAndExtractionCheck.Enabled = permissionEnabled;
                _allowDocumentAssemblyCheck.Enabled = permissionEnabled;
                _allowDocumentModificationsCheck.Enabled = permissionEnabled;
                _allowHighQualityPrintingCheck.Enabled = permissionEnabled;

                // If high quality printing is allowed then need to allow low quality
                if (_allowHighQualityPrintingCheck.Checked)
                {
                    _allowLowQualityPrintCheck.Checked = true;
                    _allowLowQualityPrintCheck.Enabled = false;
                }
                else
                {
                    _allowLowQualityPrintCheck.Enabled = permissionEnabled;
                }

                // If document modifications are allowed then need to allow
                // adding/modifying annotations
                if (_allowDocumentModificationsCheck.Checked)
                {
                    _allowAddOrModifyAnnotationsCheck.Checked = true;
                    _allowAddOrModifyAnnotationsCheck.Enabled = false;
                }
                else
                {
                    _allowAddOrModifyAnnotationsCheck.Enabled = permissionEnabled;
                }

                // If adding/modifying annotations is allowed then need to
                // allow fill in form fields
                if (_allowAddOrModifyAnnotationsCheck.Checked)
                {
                    _allowFillInFormFieldsCheck.Checked = true;
                    _allowFillInFormFieldsCheck.Enabled = false;
                }
                else
                {
                    _allowFillInFormFieldsCheck.Enabled = permissionEnabled;
                }

                // If allow copy and extraction then need to allow accessibility
                if (_allowCopyAndExtractionCheck.Checked)
                {
                    _allowAccessibilityCheck.Checked = true;
                    _allowAccessibilityCheck.Enabled = false;
                }
                else
                {
                    _allowAccessibilityCheck.Enabled = permissionEnabled;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29734", ex);
            }
        }

        #endregion Methods
    }
}