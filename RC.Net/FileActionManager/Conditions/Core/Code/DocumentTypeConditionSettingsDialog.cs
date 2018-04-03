﻿using Extract.FileActionManager.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// Allows for configuration of a <see cref="DocumentTypeCondition"/> instance.
    /// </summary>
    public partial class DocumentTypeConditionSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(DocumentTypeConditionSettingsDialog).ToString();

        #endregion Constants

        #region Fields

        string _documentClassifiersPath;

        /// <summary>
        /// The default industry to use when displaying available document types in configuration.
        /// </summary>
        string _industry;

        /// <summary>
        /// Provides a dialog from which to choose from available document types.
        /// </summary>
        IDocumentClassificationUtils _documentClassifier =
            (IDocumentClassificationUtils)new DocumentClassifier();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTypeConditionSettingsDialog"/> class.
        /// </summary>
        public DocumentTypeConditionSettingsDialog(DocumentTypeCondition settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI32750",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                _voaFilePathTags.PathTags = new FileActionManagerPathTags();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32751");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public DocumentTypeCondition Settings { get; set; }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _metComboBox.Text = Settings.MetIfTrue ? "met" : "not met";
                    _voaFileTextBox.Text = Settings.VOAFileName;
                    _documentTypeListBox.Items.AddRange(Settings.DocumentTypes);
                    _documentClassifiersPath = Settings.DocumentClassifiersPath;
                    _industry = Settings.Industry;
                }

                UpdateButtons();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32752");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// In the case that the OK button is clicked, validates the settings, applies them, and
        /// closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // If there are invalid settings, prompt and return without closing.
                if (WarnIfInvalid())
                {
                    return;
                }

                // Apply the UI values to the Settings instance.
                Settings.MetIfTrue = _metComboBox.Text == "met";
                Settings.VOAFileName = _voaFileTextBox.Text;
                Settings.DocumentTypes = _documentTypeListBox.Items.Cast<string>().ToArray();
                Settings.DocumentClassifiersPath = _documentClassifiersPath;
                Settings.Industry = _industry;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32753");
            }
        }

        /// <summary>
        /// Handles the doc type select button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDocTypeSelectButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_documentTypeListBox.Items.Count > 0)
                {
                    using (CustomizableMessageBox cmb = new CustomizableMessageBox())
                    {
                        cmb.Caption = "Warning: selections will be discarded...";
                        cmb.Text = "If you re-select document types, curerntly selected " +
                            "document types will be discarded. Do you want to continue " +
                            "(and discard selections)?";
                        cmb.AddButton("OK", "OK", defaultButton: false);
                        cmb.AddButton("Cancel", "Cancel", defaultButton: true);

                        string result = cmb.Show(this);
                        if (result == "Cancel")
                        {
                            return;
                        }
                    }
                }

                using (new TemporaryWaitCursor())
                {
                    // If the industry is not set default it to the first in the industry list
                    if (string.IsNullOrWhiteSpace(_industry))
                    {
                        // Default it to the first industry category in the category list
                        VariantVector industriesVector = _documentClassifier.GetDocumentIndustries(_documentClassifiersPath);

                        // Make sure there is at least one industry
                        if (industriesVector.Size > 0)
                        {
                            // Get the first industry in the list
                            _industry = industriesVector[0].AsString();
                        }
                        else
                        {
                            throw new ExtractException("ELI32754", "No industry categories defined");
                        }
                    }
                }

                // Display the dialog - with variable industry, multiple selection and special types
                VariantVector selectedTypes =
                    _documentClassifier.GetDocTypeSelection(ref _documentClassifiersPath, ref _industry, true, true, true, true);

                if (_documentTypeListBox.Items.Count > 0 && selectedTypes.Size > 0)
                {
                    _documentTypeListBox.Items.Clear();
                }

                // Add any selected categories that are not already in _documentTypeListBox.
                for (int i = 0; i < selectedTypes.Size; i++)
                {
                    string documentType = selectedTypes[i].AsString();
                    _documentTypeListBox.Items.Add(documentType);
                }

                UpdateButtons();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32755");
            }
        }

        /// <summary>
        /// Handles the list remove button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleListRemoveButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_documentTypeListBox.SelectedIndex != -1)
                {
                    _documentTypeListBox.Items.RemoveAt(_documentTypeListBox.SelectedIndex);
                }

                UpdateButtons();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32756");
            }
        }

        /// <summary>
        /// Handles the list clear button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleListClearButtonClick(object sender, EventArgs e)
        {
            try
            {
                _documentTypeListBox.Items.Clear();

                UpdateButtons();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32757");
            }
        }

        /// <summary>
        /// Handles the case that selected index of <see cref="_documentTypeListBox"/> changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateButtons();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34203");
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
            ExtractException.Assert("ELI32758", "Document type condition settings have not been provided.",
                Settings != null);

            if (string.IsNullOrWhiteSpace(_voaFileTextBox.Text))
            {
                UtilityMethods.ShowMessageBox("You must specify a VOA file.",
                    "VOA file not specified", true);
                _voaFileTextBox.Focus();

                return true;
            }

            if (_documentTypeListBox.Items.Count == 0)
            {
                UtilityMethods.ShowMessageBox("You must specify at least one document type.",
                    "Document type not specified", true);
                _selectButton.Focus();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the button enabled state based on the items in _documentTypeListBox.
        /// </summary>
        void UpdateButtons()
        {
            bool itemSelected = _documentTypeListBox.SelectedItems.Count == 1;
            _removeButton.Enabled = itemSelected;
            _clearButton.Enabled = _documentTypeListBox.Items.Count > 0;
        }

        #endregion Private Members
    }
}
