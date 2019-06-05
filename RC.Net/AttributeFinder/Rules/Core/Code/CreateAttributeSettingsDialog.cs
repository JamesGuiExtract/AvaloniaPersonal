using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Property page for xpath-based attribute creation
    /// </summary>
    [CLSCompliant(false)]
    public partial class CreateAttributeSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(CreateAttributeSettingsDialog).ToString();

        static readonly string _INVALID_XPATH_ERROR_PROMPT =
            "Only underscore, letters, digits and valid XPath characters can be used";

        static readonly string _INVALID_NONXPATH_ERROR_PROMPT = "Only underscore, letters and digits can be used";

        #endregion Constants

        #region Fields

        /// <summary>
        /// When the user clicks on a row in the data grid view, an event handler changes the 
        /// text and check boxes to track the selected row. At this time it is important to 
        /// suspend the event handling for the text and check box changed events, as it is not
        /// desired to post an update of the subattribute components at that time.
        /// </summary>
        bool _suspendTextAndCheckboxChangedEvents;

        /// <summary>
        /// Used to populate import/export file dialogs with last filename imported/exported for convenience
        /// </summary>
        private string _fileName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAttributeSettingsDialog"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="CreateAttribute"/> instance to configure.
        /// </param>
        public CreateAttributeSettingsDialog(CreateAttribute settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                                                 "ELI39409",
                                                 _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                // Without this, shift+select doesn't select in-between rows.
                // Added here as well as in the designer in case the generated code gets regenerated.
                // https://extract.atlassian.net/browse/ISSUE-16448
                _nameDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                _suspendTextAndCheckboxChangedEvents = true;

                _rootTextBox.SetError(_errorProvider, String.Empty);
                _rootTextBox.SetErrorGlyphPosition(_errorProvider);

                _attributeNameTextBox.SetError(_errorProvider, String.Empty);
                _attributeNameTextBox.SetErrorGlyphPosition(_errorProvider, ErrorIconAlignment.TopRight);

                _attributeValueTextBox.SetError(_errorProvider, String.Empty);
                _attributeValueTextBox.SetErrorGlyphPosition(_errorProvider, ErrorIconAlignment.TopRight);

                _attributeTypeTextBox.SetError(_errorProvider, String.Empty);
                _attributeTypeTextBox.SetErrorGlyphPosition(_errorProvider, ErrorIconAlignment.TopRight);

                SetTextBoxEnabledStates(false);

                SetDoNotCreateCheckBoxStates();

                _removeButton.Enabled = false;
                _duplicateButton.Enabled = false;
                _upButton.Enabled = false;
                _downButton.Enabled = false;

                _suspendTextAndCheckboxChangedEvents = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39410");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="CreateAttribute"/> to configure.
        /// </summary>
        /// <value>The <see cref="CreateAttribute"/> to configure.</value>
        public CreateAttribute Settings
        {
            get; 
            set; 
        }

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
                    LoadPageControls();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39411");
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
                if (Settings.SubAttributeComponentCount < 1)
                {
                    var caption = "Subattributes are required";
                    var text = "At least one Subattribute definition is required.\n" +
                             "Click on the Add... button to add a subattribute definition.";
                    DisplayAlert(caption, text);

                    this.DialogResult = DialogResult.None;
                    return;
                }

                var rootText = _rootTextBox.TextValue();
                if (!UtilityMethods.IsValidXPathExpression(rootText))
                {
                    SetRootTextBoxErrorText();
                    _rootTextBox.Focus();
                    this.DialogResult = DialogResult.None;
                    return;
                }

                for (int i = 0; i < Settings.SubAttributeComponentCount; ++i)
                {
                    var valid = Settings.AttributeIsValid(i);
                    if (!valid.Item1)
                    {
                        SetErrorOrRequiredCue(i, invalidComponent: valid.Item2);

                        this.DialogResult = DialogResult.None;
                        return;
                    }
                }

                Settings.Root = _rootTextBox.TextValue();

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39434");
            }
        }

        /// <summary>
        /// Handles the Click event of the _AddButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _AddButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_attributeNameTextBox.Enabled)
                {
                    SetTextBoxEnabledStates(true);
                }

                ClearNameAndValueAndType();

                Settings.AddSubAttributeComponents(_attributeNameTextBox.TextValue(),
                                                   _attributeValueTextBox.TextValue(),
                                                   _attributeTypeTextBox.TextValue(),
                                                   _nameCheckBox.Checked,
                                                   _valueCheckBox.Checked,
                                                   _typeCheckBox.Checked,
                                                   _nameDoNotCreateIfEmptyCheckBox.Checked,
                                                   _valueDoNotCreateIfEmptyCheckBox.Checked,
                                                   _typeDoNotCreateIfEmptyCheckBox.Checked);

                int lastRow = _nameDataGridView.Rows.Count;

                // Add a blank row to the data grid view...
                // NOTE that the value can't be empty, or the row isn't added.
                _suspendTextAndCheckboxChangedEvents = true;
                _nameDataGridView.Rows.Add(" ");

                // and set the current cell to prevent an ugly multiple cell selection...
                _nameDataGridView.CurrentCell = _nameDataGridView.Rows[lastRow].Cells[0];
                _suspendTextAndCheckboxChangedEvents = false;

                // and position the current row on the newly added blank row...
                SetSelectedRow(lastRow);

                _AddButton.Enabled = false;
                _duplicateButton.Enabled = false;
                _removeButton.Enabled = true;
                UpdateUpDownButtons(lastRow);

                // and set focus to the name text box so user doesn't need to do this manually
                _attributeNameTextBox.Focus();
                _attributeNameTextBox.ForeColor = System.Drawing.Color.Black;

                // and set required fields - not in the Name textbox, as the focus is there, not 
                // the Type textbox, only required when use xpath is checked, by default it is not checked.
                _suspendTextAndCheckboxChangedEvents = true;
                _attributeValueTextBox.SetRequiredMarker();
                _suspendTextAndCheckboxChangedEvents = false;

                SetUseXpathCheckBoxEnabledStates();
                _valueDoNotCreateIfEmptyCheckBox.Enabled = _valueCheckBox.Checked && _valueCheckBox.Enabled;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39426");
            }
        }

        /// <summary>
        /// Handles the RowEnter event of the _nameDataGridView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
        private void _nameDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (_suspendTextAndCheckboxChangedEvents)
                {
                    return;
                }

                int numberOfRows = _nameDataGridView.Rows.Count;
                if (numberOfRows < 1)
                {
                    return;
                }

                // Use RowIndex to get the index of the row that WILL be the new current row.
                int index = e.RowIndex;
                SetSelectedRow(index);

                var valid = Settings.AttributeIsValid(index);
                bool isValid = valid.Item1;
                _duplicateButton.Enabled = isValid;
                _removeButton.Enabled = true;
                UpdateUpDownButtons(index);

                SetTextAndChecksFromSubAttribute(index);

                if (!isValid)
                {
                    SetErrorOrRequiredCue(index, invalidComponent: valid.Item2, suspendRowOperations: true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39428");
            }
        }
        
        /// <summary>
        /// Handles the checkboxes changed event - updates the modified subattribute.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckBoxChanged(object sender, EventArgs e)
        {
            try
            {
                if (_suspendTextAndCheckboxChangedEvents)
                {
                    return;
                }

                SetDoNotCreateCheckBoxStates();
                UpdateActiveSubattribute();
                SetAddAndDuplicateButtonsEnabledState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39435");
            }
        }
        

        /// <summary>
        /// Handles the TextChanged event for all TextBox controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_suspendTextAndCheckboxChangedEvents)
                {
                    return;
                }

                var textBox = (TextBox)sender;
                var text = textBox.TextValue();

                if (String.IsNullOrWhiteSpace(text))
                {
                    _suspendTextAndCheckboxChangedEvents = true;
                    textBox.SetError(_errorProvider, String.Empty);
                    _suspendTextAndCheckboxChangedEvents = false;
                }
                else
                {
                    var checkBox = GetAssociatedXpathCheckBox(textBox);

                    // NOTE: if checkBox is null, then it is the _rootTextBox, which always takes an xpath statement,
                    // hence the "use xpath" flag is set to true below if null.
                    bool checkd = checkBox == null ? true : checkBox.Checked;
                    bool isValid = false;

                    if (checkBox == null && UtilityMethods.IsValidXPathExpression(text)
                        || checkBox == _nameCheckBox && CreateAttribute.NameIsValid(text, checkd)
                        || checkBox == _typeCheckBox && CreateAttribute.TypeIsValid(text, checkd)
                        || checkBox == _valueCheckBox && CreateAttribute.ValueIsValid(text, checkd))
                    {
                        isValid = true;
                    }

                    var msg = isValid ? "" : "invalid characters in statement";

                    _suspendTextAndCheckboxChangedEvents = true;
                    textBox.SetError(_errorProvider, msg);
                    _suspendTextAndCheckboxChangedEvents = false;
                }

                UpdateActiveSubattribute();
                SetAddAndDuplicateButtonsEnabledState();
                if (null != _nameDataGridView.CurrentRow)
                {
                    UpdateUpDownButtons(_nameDataGridView.CurrentRow.Index);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39635");
            }
        }

        /// <summary>
        /// Handles the Click event of the _removeButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _removeButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in _nameDataGridView.SelectedRows)
                {
                    Settings.DeleteSubAttributeComponents(row.Index);
                    _nameDataGridView.Rows.RemoveAt(row.Index);
                }

                ClearAndDisableNameAndValueAndType();

                _AddButton.Enabled = true;

                var gridRowsEmpty = Settings.SubAttributeComponentCount == 0;
                _removeButton.Enabled = !gridRowsEmpty;
                _duplicateButton.Enabled = !gridRowsEmpty;

                if (null != _nameDataGridView.CurrentRow)
                {
                    SetSelectedRow(_nameDataGridView.CurrentRow.Index);
                    UpdateUpDownButtons(_nameDataGridView.CurrentRow.Index);
                }

                SetUseXpathCheckBoxEnabledStates();
                SetDoNotCreateCheckBoxStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39439");
            }
        }

        /// <summary>
        /// Handles the Click event of the _upButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _upButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in _nameDataGridView.SelectedRows)
                {
                    int index = row.Index;
                    int indexUp = index - 1;

                    if (indexUp >= 0)
                    {
                        Settings.SwapAttributeComponents(index, indexUp);
                        SetTextAndChecksFromSubAttribute(indexUp);

                        SetDataGridView_RowName(index);
                        SetDataGridView_RowName(indexUp);

                        _nameDataGridView.CurrentCell = _nameDataGridView.Rows[indexUp].Cells[0];
                        SetSelectedRow(indexUp);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39454");
            }
        }

        /// <summary>
        /// Handles the Click event of the _downButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _downButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in _nameDataGridView.SelectedRows)
                {
                    int index = row.Index;
                    int indexDown = index + 1;

                    if (row.Index + 1 < Settings.SubAttributeComponentCount)
                    {
                        Settings.SwapAttributeComponents(index, indexDown);
                        SetTextAndChecksFromSubAttribute(indexDown);

                        SetDataGridView_RowName(index);
                        SetDataGridView_RowName(indexDown);

                        _nameDataGridView.CurrentCell = _nameDataGridView.Rows[indexDown].Cells[0];
                        SetSelectedRow(indexDown);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40370");
            }
        }

        /// <summary>
        /// Handles the Click event of the _duplicateButton - duplicates a subsattribute
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _duplicateButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var row in _nameDataGridView.SelectedRows
                    .Cast<DataGridViewRow>()
                    .OrderBy(row => row.Index))
                {
                    Settings.DuplicateSubattribute(row.Index);
                }

                // Now update the grid view
                int currentNumberOfRows = _nameDataGridView.Rows.Count;
                int currentNumberOfSubattrs = Settings.SubAttributeComponentCount;
                for (int i = currentNumberOfRows; i < currentNumberOfSubattrs; ++i)
                {
                    var subattr = Settings.GetComponents(i);
                    _nameDataGridView.Rows.Add(subattr.Name);
                }

                int lastRow = _nameDataGridView.Rows.Count - 1;
                _nameDataGridView.CurrentCell = _nameDataGridView.Rows[lastRow].Cells[0];
                SetSelectedRow(lastRow);

                UpdateUpDownButtons(lastRow);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39497");
            }
        }

        /// <summary>
        /// Handles the CheckStateChanged event for _typeCheckBox and _nameCheckBox controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (_suspendTextAndCheckboxChangedEvents)
                {
                    return;
                }

                // here to validate associated text box when the check state changes
                var checkBox = (CheckBox)sender;
                TextBox textBox = GetAssociatedTextBox(checkBox);

                var text = textBox.TextValue();
                var checkd = checkBox.Checked;
                if (   checkBox == _nameCheckBox && CreateAttribute.NameIsValid(text, checkd)
                    || checkBox == _typeCheckBox && CreateAttribute.TypeIsValid(text, checkd)
                    || checkBox == _valueCheckBox && CreateAttribute.ValueIsValid(text, checkd))
                {
                    textBox.SetError(_errorProvider, String.Empty);
                    textBox.RemoveRequiredMarker();
                    UpdateActiveSubattribute();
                }
                else if (checkd && String.IsNullOrWhiteSpace(text))
                {
                    textBox.SetRequiredMarker();
                }
                else
                {
                    string error = checkd ? _INVALID_XPATH_ERROR_PROMPT : _INVALID_NONXPATH_ERROR_PROMPT;
                    textBox.SetError(_errorProvider, error);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39598");
            }
        }

        /// <summary>
        /// Handles (focus) enter event, to remove the required field marker while user is entering text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleFocusEnter(object sender, EventArgs e)
        {
            try
            {
                var textbox = (TextBox)sender;
                if (textbox.IsRequiredMarkerSet())
                {
                    textbox.RemoveRequiredMarker();
                    textbox.SafeBeginInvoke("ELI39495", () => SetTextBoxCursorPosition(textbox, start: 0));
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39644");
            }
        }

        /// <summary>
        /// Handles the leave (focus) event to mark the field as required iff no other text exists in the field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleFocusLeave(object sender, EventArgs e)
        {
            try
            {
                var textbox = (TextBox)sender;
                var text = textbox.Text;
                if (String.IsNullOrWhiteSpace(text))
                {
                    if (textbox == _attributeTypeTextBox ||
                        textbox == _attributeValueTextBox)
                    {
                        var checkBox = GetAssociatedXpathCheckBox(textbox);

                        if (checkBox.Checked)
                        {
                            textbox.SetRequiredMarker();
                        }
                    }
                    else
                    {
                        // Name or Root text boxes
                        textbox.SetRequiredMarker();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39645");
            }
        }


        private void _exportButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "YAML files|*.yml|All files|*.*";
                    saveDialog.FileName = "export.yml";
                    if (!string.IsNullOrEmpty(_fileName))
                    {
                        saveDialog.InitialDirectory = Path.GetDirectoryName(_fileName);
                    }

                    var result = saveDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        Settings.SaveToYAML(saveDialog.FileName);
                        _fileName = saveDialog.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46883");
            }
        }

        private void _importButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "YAML files|*.yml|All files|*.*";
                    openDialog.FileName = "export.yml";
                    if (!string.IsNullOrEmpty(_fileName))
                    {
                        openDialog.FileName = Path.GetFileName(_fileName);
                        openDialog.InitialDirectory = Path.GetDirectoryName(_fileName);
                    }

                    var result = openDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        Settings.LoadFromYAML(openDialog.FileName);

                        _fileName = openDialog.FileName;

                        // Workaround for crappy error provider code
                        if (!string.IsNullOrEmpty(Settings.Root))
                        {
                            _rootTextBox.ForeColor = Color.Black;
                        }

                        LoadPageControls();

                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46884");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the page controls - iff there are subattributes defined, then all the relevant
        /// controls are populated (datagridview, text boxes, and check boxes).
        /// </summary>
        void LoadPageControls()
        {
            _nameDataGridView.Rows.Clear();

            if (null == Settings)
            {
                return;
            }

            _rootTextBox.Text = Settings.Root;

            bool enableState = Settings.SubAttributeComponentCount > 0 ? true : false;
            SetTextBoxEnabledStates(enableState);
            _duplicateButton.Enabled = enableState;
            _removeButton.Enabled = enableState;

            for (int i = 0; i < Settings.SubAttributeComponentCount; ++i)
            {
                var antv = Settings.GetComponents(i);
                _nameDataGridView.Rows.Add(antv.Name);
            }

            SetSelectedRow(0);
            UpdateUpDownButtons(0);

            SetTextAndChecksFromSubAttribute(0);
            SetUseXpathCheckBoxEnabledStates();
            SetTextAndChecks();
        }

        /// <summary>
        /// Sets the selected row in the data grid
        /// </summary>
        /// <param name="rowIndex">Index of the row to set.</param>
        void SetSelectedRow(int rowIndex)
        {
            if (rowIndex >= _nameDataGridView.Rows.Count)
            {
                return;
            }

            _nameDataGridView.Rows[rowIndex].Selected = true;
            // Don't do the following, causes an exception due to recursion in some cases...
            //_nameDataGridView.CurrentCell = _nameDataGridView.Rows[rowIndex].Cells[0];
        }

        /// <summary>
        /// Sets the current row in the data grid.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        void SetCurrentRow(int rowIndex)
        {
            if (rowIndex >= _nameDataGridView.Rows.Count)
            {
                return;
            }

            _suspendTextAndCheckboxChangedEvents = true;
            _nameDataGridView.CurrentCell = _nameDataGridView.Rows[rowIndex].Cells[0];
            _suspendTextAndCheckboxChangedEvents = false;
        }

        /// <summary>
        /// Sets the text boxes and check boxes from the specified sub attribute values.
        /// </summary>
        /// <param name="subAttrIndex">Index of the sub attribute.</param>
        void SetTextAndChecksFromSubAttribute(int subAttrIndex)
        {
            if (null == Settings || subAttrIndex >= Settings.SubAttributeComponentCount)
            {
                return;
            }

            var nameTypeValue = Settings.GetComponents(subAttrIndex);

            _suspendTextAndCheckboxChangedEvents = true;

            _attributeNameTextBox.ForeColor = Color.Black;
            _attributeNameTextBox.Text = nameTypeValue.Name;

            _attributeTypeTextBox.ForeColor = Color.Black;
            _attributeTypeTextBox.Text = nameTypeValue.TypeOfAttribute;

            _attributeValueTextBox.ForeColor = Color.Black;
            _attributeValueTextBox.Text = nameTypeValue.Value;

            _nameCheckBox.Checked = nameTypeValue.NameContainsXPath;
            _valueCheckBox.Checked = nameTypeValue.ValueContainsXPath;
            _typeCheckBox.Checked = nameTypeValue.TypeContainsXPath;

            _nameDoNotCreateIfEmptyCheckBox.Checked = nameTypeValue.DoNotCreateIfNameIsEmpty;
            _valueDoNotCreateIfEmptyCheckBox.Checked = nameTypeValue.DoNotCreateIfValueIsEmpty;
            _typeDoNotCreateIfEmptyCheckBox.Checked = nameTypeValue.DoNotCreateIfTypeIsEmpty;

            _suspendTextAndCheckboxChangedEvents = false;
        }

        /// <summary>
        /// Sets the text and checks, not from specified subattribute.
        /// </summary>
        void SetTextAndChecks()
        {
            _suspendTextAndCheckboxChangedEvents = true;

            SetDoNotCreateCheckBoxStates();

            if (Settings.Root != null)
            {
                _rootTextBox.Text = Settings.Root;
            }

            _suspendTextAndCheckboxChangedEvents = false;
        }

        /// <summary>
        /// Sets the "use xpath" CheckBox enabled states.
        /// </summary>
        void SetUseXpathCheckBoxEnabledStates()
        {
            bool enableState = Settings.SubAttributeComponentCount > 0;

            _nameCheckBox.Enabled = enableState;
            _valueCheckBox.Enabled = enableState;
            _typeCheckBox.Enabled = enableState;
        }


        /// <summary>
        /// Displays an alert.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="text">The text.</param>
        void DisplayAlert(string caption, string text)
        {
            CustomizableMessageBox cmb = new CustomizableMessageBox();
            cmb.Caption = caption;
            cmb.Text = text;
            cmb.UseDefaultOkButton = true;
            cmb.Show(this);
        }

        /// <summary>
        /// Sets the name displayed in the data grid view row.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        void SetDataGridView_RowName(int rowIndex)
        {
            if (rowIndex >= _nameDataGridView.Rows.Count)
            {
                return;
            }

            var nameTypeValue = Settings.GetComponents(rowIndex);
            _nameDataGridView.Rows[rowIndex].Cells[0].Value = nameTypeValue.Name;
        }

        /// <summary>
        /// Sets the "do not create" CheckBox states, enabling/disabling the controls depending on 
        /// the state of the "[] use xpath" checkboxes ("[] do not create" is enabled only when the 
        /// corresponding "[] use xpath" is checked; otherwise the user is entering literal text and
        /// the attribute should always be created).
        /// </summary>
        void SetDoNotCreateCheckBoxStates()
        {
            _suspendTextAndCheckboxChangedEvents = true;

            _nameDoNotCreateIfEmptyCheckBox.Enabled = _nameCheckBox.Checked && _nameCheckBox.Enabled;
            _valueDoNotCreateIfEmptyCheckBox.Enabled = _valueCheckBox.Checked && _valueCheckBox.Enabled;
            _typeDoNotCreateIfEmptyCheckBox.Enabled = _typeCheckBox.Checked && _typeCheckBox.Enabled;

            _suspendTextAndCheckboxChangedEvents = false;
        }

        /// <summary>
        /// Sets the root text box error text, if the text box is empty.
        /// </summary>
        /// <returns></returns>
        bool SetRootTextBoxErrorText()
        {
            _suspendTextAndCheckboxChangedEvents = true;

            if (String.IsNullOrWhiteSpace(_rootTextBox.TextValue()))
            {
                _rootTextBox.RemoveRequiredMarker();
                _rootTextBox.SetError(_errorProvider, "This value is required");
                _rootTextBox.Focus();

                _suspendTextAndCheckboxChangedEvents = false;
                return true;
            }

            _suspendTextAndCheckboxChangedEvents = false;
            return false;
        }

        /// <summary>
        /// Sets the text box cursor position.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="start">The start position of the cursor.</param>
        static void SetTextBoxCursorPosition(TextBox textBox, int start)
        {
            textBox.SelectionStart = start;
            textBox.SelectionLength = 0;
            textBox.Focus();
        }

        /// <summary>
        /// Sets the text box enabled states to the specified state.
        /// </summary>
        /// <param name="state">if set to <c>true</c> [state].</param>
        void SetTextBoxEnabledStates(bool state)
        {
            _attributeNameTextBox.Enabled = state;
            _attributeValueTextBox.Enabled = state;
            _attributeTypeTextBox.Enabled = state;
        }

        /// <summary>
        /// Clears the name and value and type text, and sets states of associated check boxes.
        /// </summary>
        void ClearNameAndValueAndType()
        {
            _suspendTextAndCheckboxChangedEvents = true;

            _attributeNameTextBox.Text = "";
            _attributeValueTextBox.Text = "";
            _attributeTypeTextBox.Text = "";

            _nameCheckBox.Checked = false;
            _valueCheckBox.Checked = true;
            _typeCheckBox.Checked = false;
            SetDoNotCreateCheckBoxStates();

            _suspendTextAndCheckboxChangedEvents = false;
        }

        /// <summary>
        /// Clears and disables name and value and type.
        /// </summary>
        void ClearAndDisableNameAndValueAndType()
        {
            if (Settings.SubAttributeComponentCount == 0)
            {
                _suspendTextAndCheckboxChangedEvents = true;

                _attributeNameTextBox.Text = "";
                _attributeValueTextBox.Text = "";
                _attributeTypeTextBox.Text = "";

                _attributeNameTextBox.Enabled = false;
                _attributeValueTextBox.Enabled = false;
                _attributeTypeTextBox.Enabled = false;

                _suspendTextAndCheckboxChangedEvents = false;
            }
        }

        /// <summary>
        /// Updates the state (enable/disable) of the up and down buttons.
        /// </summary>
        /// <param name="rowIndex">Index of the current row in the data grid.</param>
        void UpdateUpDownButtons(int rowIndex)
        {
            try
            {
                if (null == _nameDataGridView.CurrentRow)
                {
                    return;
                }

                int rowCount = _nameDataGridView.Rows.Count;
                int lastRow = rowCount == 0 ? 0 : rowCount - 1;

                bool valid = AttributeAtGridRowIsValid(rowIndex);
                bool upState = rowCount > 1 && rowIndex != 0 && valid;
                bool downState = rowCount > 1 && rowIndex != lastRow && valid;

                _upButton.Enabled = upState;
                _downButton.Enabled = downState;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39594");
            }
        }

        /// <summary>
        /// Updates the active subattribute components, and updates the grid.
        /// </summary>
        void UpdateActiveSubattribute()
        {
            try
            {
                if (_nameDataGridView == null || _nameDataGridView.CurrentRow == null)
                {
                    return;
                }

                int rowIndex = _nameDataGridView.CurrentRow.Index;

                Settings.UpdateSubattributeComponents(_attributeNameTextBox.TextValue(),
                                                      _attributeValueTextBox.TextValue(),
                                                      _attributeTypeTextBox.TextValue(),
                                                      _nameCheckBox.Checked,
                                                      _valueCheckBox.Checked,
                                                      _typeCheckBox.Checked,
                                                      _nameDoNotCreateIfEmptyCheckBox.Checked,
                                                      _valueDoNotCreateIfEmptyCheckBox.Checked,
                                                      _typeDoNotCreateIfEmptyCheckBox.Checked,
                                                      rowIndex);

                _nameDataGridView.Rows[rowIndex].Cells[0].Value = _attributeNameTextBox.TextValue();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39600");
            }
        }

        /// <summary>
        /// Sets the enabled state of the add and duplicate buttons.
        /// </summary>
        /// <param name="validChar">Some uses involve whether a valid key was input, if so
        /// pass a flag value here</param>
        void SetAddAndDuplicateButtonsEnabledState(bool validChar = true)
        {
            if (null == _nameDataGridView || null == _nameDataGridView.CurrentRow)
            {
                return;
            }

            int index = _nameDataGridView.CurrentRow.Index;
            var enabled = validChar && Settings.AttributeIsValid(index).Item1;
            _AddButton.Enabled = enabled;
            _duplicateButton.Enabled = enabled;
        }

        /// <summary>
        /// Gets the associated xpath CheckBox.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <returns>The associated check box, or null</returns>
        CheckBox GetAssociatedXpathCheckBox(TextBox textBox)
        {
            if (textBox == _attributeNameTextBox)
            {
                return _nameCheckBox;
            }
            else if (textBox == _attributeValueTextBox)
            {
                return _valueCheckBox;
            }
            else if (textBox == _attributeTypeTextBox)
            {
                return _typeCheckBox;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the text box associated with the specified check box.
        /// </summary>
        /// <param name="checkBox">The check box.</param>
        /// <returns>The associated text box, or null</returns>
        TextBox GetAssociatedTextBox(CheckBox checkBox)
        {
            if (checkBox == _nameCheckBox || checkBox == _nameDoNotCreateIfEmptyCheckBox)
            {
                return _attributeNameTextBox;
            }
            else if (checkBox == _valueCheckBox || checkBox == _valueDoNotCreateIfEmptyCheckBox)
            {
                return _attributeValueTextBox;
            }
            else if (checkBox == _typeCheckBox || checkBox == _typeDoNotCreateIfEmptyCheckBox)
            {
                return _attributeTypeTextBox;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Determines if the attribute at current grid row is valid.
        /// </summary>
        /// <returns>true or false</returns>
        bool AttributeAtCurrentGridRowIsValid()
        {
            if (null == _nameDataGridView.CurrentRow)
            {
                return false;
            }

            int index = _nameDataGridView.CurrentRow.Index;
            return Settings.AttributeIsValid(index).Item1;
        }

        /// <summary>
        /// Is the attribute at the specified grid row valid?
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <returns>true if valid, false otherwise</returns>
        bool AttributeAtGridRowIsValid(int rowIndex)
        {
            if (null == _nameDataGridView.Rows)
            {
                return false;
            }

            return Settings.AttributeIsValid(rowIndex).Item1;
        }

        /// <summary>
        /// Sets the error or required cue for a specified attribute.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="invalidComponent">The invalid component.</param>
        /// <param name="suspendRowOperations">when called from within a grid row event handler, it
        /// is important to not perform grid row operations</param>
        void SetErrorOrRequiredCue(int rowIndex, 
                                   CreateAttribute.AttributeComponentId invalidComponent, 
                                   bool suspendRowOperations = false)
        {
            if (!suspendRowOperations)
            {
                SetCurrentRow(rowIndex);
                SetSelectedRow(rowIndex);
                SetTextAndChecksFromSubAttribute(rowIndex);
            }

            _suspendTextAndCheckboxChangedEvents = true;

            if (invalidComponent == CreateAttribute.AttributeComponentId.Name)
            {
                _attributeNameTextBox.Focus();
                if (!String.IsNullOrWhiteSpace(_attributeNameTextBox.TextValue()))
                {
                    string error = _nameCheckBox.Checked ? _INVALID_XPATH_ERROR_PROMPT : _INVALID_NONXPATH_ERROR_PROMPT;
                    _attributeNameTextBox.SetError(_errorProvider, error);
                }
            }
            else
            {
                if (_nameCheckBox.Checked && String.IsNullOrWhiteSpace(_attributeNameTextBox.Text))
                {
                    _attributeNameTextBox.SetRequiredMarker();
                }
            }

            if (invalidComponent == CreateAttribute.AttributeComponentId.Value)
            {
                _attributeValueTextBox.Focus();
                if (!String.IsNullOrWhiteSpace(_attributeValueTextBox.TextValue()))
                {
                    string error = _valueCheckBox.Checked ? _INVALID_XPATH_ERROR_PROMPT : _INVALID_NONXPATH_ERROR_PROMPT;
                    _attributeValueTextBox.SetError(_errorProvider, error);
                }
            }
            else
            {
                if (_valueCheckBox.Checked && String.IsNullOrWhiteSpace(_attributeValueTextBox.Text))
                {
                    _attributeValueTextBox.SetRequiredMarker();
                }
            }

            if (invalidComponent == CreateAttribute.AttributeComponentId.Type)
            {
                _attributeTypeTextBox.Focus();
                if (!String.IsNullOrWhiteSpace(_attributeTypeTextBox.TextValue()))
                {
                    string error = _typeCheckBox.Checked ? _INVALID_XPATH_ERROR_PROMPT : _INVALID_NONXPATH_ERROR_PROMPT;
                    _attributeTypeTextBox.SetError(_errorProvider, error);
                }
            }
            else
            {
                if (_typeCheckBox.Checked && String.IsNullOrWhiteSpace(_attributeTypeTextBox.Text))
                {
                    _attributeTypeTextBox.SetRequiredMarker();
                }
            }

            _suspendTextAndCheckboxChangedEvents = false;
        }

        #endregion Private Methods
    }
}
