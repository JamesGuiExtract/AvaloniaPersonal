using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Extract.Redaction.Verification
{
	/// <summary>
	/// Represents a dialog that allows the user to select a list of exemption codes.
	/// </summary>
	public partial class ExemptionCodeListDialog: Form
	{
		#region Fields

        /// <summary>
        /// The master list of the all valid exemption categories and codes.
        /// </summary>
        readonly MasterExemptionCodeList _masterCodes;

		/// <summary>
		/// The exemption codes the user selected.
		/// </summary>
		ExemptionCodeList _codes;

        /// <summary>
        /// <see langword="true"/> if the Apply Last button should be enabled; 
        /// <see langword="false"/> if the button should be disabled.
        /// </summary>
        bool _enableApplyLast;

        /// <summary>
        /// The last applied exemption code.
        /// </summary>
        ExemptionCodeList _lastAppliedCodes = new ExemptionCodeList();

        /// <summary>
        /// The last displayed exemption code category. This is the category displayed when 
        /// <see cref="_codes"/> is empty.
        /// </summary>
        string _lastCategory;

        /// <summary>
        /// If greater than zero, the user interface is not updated; if zero the user interface is 
        /// updated.
        /// </summary>
        int _updating;
 
		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new <see cref="ExemptionCodeListDialog"/> class.
		/// </summary>
		// Don't fight with auto-generated code.
		//[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
		public ExemptionCodeListDialog(MasterExemptionCodeList masterCodes) 
            : this(masterCodes, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExemptionCodeListDialog"/> class.
		/// </summary>
        public ExemptionCodeListDialog(MasterExemptionCodeList masterCodes, ExemptionCodeList codes)
		{
			InitializeComponent();

            _masterCodes = masterCodes;
            _codes = codes ?? new ExemptionCodeList();

            // Add the names of the categories to combo box
            foreach (string category in _masterCodes.Categories)
            {
                _categoryComboBox.Items.Add(category);
            }
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Gets or sets the <see cref="ExemptionCodeList"/>.
		/// </summary>
		/// <value>The <see cref="ExemptionCodeList"/>.</value>
		/// <returns>The <see cref="ExemptionCodeList"/>.</returns>
		public ExemptionCodeList Exemptions
		{
			get
			{
                return _codes;
			}
			set
			{
                _codes = value;
			}
		}

        /// <summary>
        /// Gets or sets the last applied exemption code.
        /// </summary>
        /// <value>The last applied exemption code.</value>
        /// <returns>The last applied exemption code.</returns>
        public ExemptionCodeList LastExemptionCodeList
        {
            get
            {
                return _lastAppliedCodes;
            }
            set
            {
                _lastAppliedCodes = value ?? new ExemptionCodeList();
            }
        }

        /// <summary>
        /// Gets or sets whether the Apply Last button should be enabled.
        /// </summary>
        /// <value><see langword="true"/> if the Apply Last button should be enabled;
        /// <see langword="false"/> if the Apply Last button should be disabled.</value>
        /// <returns><see langword="true"/> if the Apply Last button should be enabled;
        /// <see langword="false"/> if the Apply Last button should be disabled.</returns>
        public bool EnableApplyLast
        {
            get
            {
                return _enableApplyLast;
            }
            set
            {
                _enableApplyLast = value;
            }
        }
        
		#endregion Properties

		#region Methods

        /// <summary>
        /// Prevents updates of the user interface until the <see cref="EndUpdate"/> method is 
        /// called.
        /// </summary>
        void BeginUpdate()
        {
            _updating++;
            _codesListView.BeginUpdate();
        }

        /// <summary>
        /// Resumes updating the user interface after it has been suspended by a call to 
        /// <see cref="BeginUpdate"/>.
        /// </summary>
        void EndUpdate()
        {
            _updating--;
            _codesListView.EndUpdate();

            if (_updating <= 0)
            {
                UpdateSample();
            }
        }

		/// <summary>
        /// Gets the <see cref="ExemptionCodeList"/> from the user interface.
		/// </summary>
        /// <returns>The <see cref="ExemptionCodeList"/> from the user interface.</returns>
        ExemptionCodeList GetSelectedExemptionCodeList()
		{
            // Get the abbreviated form of the currently selected exemption category
            string category = _masterCodes.GetCategoryAbbreviation(_categoryComboBox.Text);

            // Get the selected codes in this category
            string[] codes = GetSelectedCodeArray();

            // Get other text if the checkbox is checked
            string otherText = _otherTextCheckBox.Checked ? _otherTextTextBox.Text : "";

            // Return the result
            return new ExemptionCodeList(category, codes, otherText);
		}

        /// <summary>
        /// Gets the currently selected exemption codes.
        /// </summary>
        /// <returns>The currently selected exemption codes.</returns>
        string[] GetSelectedCodeArray()
        {
            List<string> codes = new List<string>();
            foreach (ListViewItem item in _codesListView.Items)
            {
                // Not sure why, but sometimes item is null. If null, skip it.
                if (item == null)
                {
                    continue;
                }

                if (item.Checked)
                {
                    // Add this code to the list
                    string code = GetCode(item);
                    codes.Add(code);
                }
            }

            return codes.ToArray();
        }

        /// <summary>
        /// Selects the specified exemption codes in the user interface.
        /// </summary>
        /// <param name="codes">The exemption codes to select.</param>
        void SelectExemptionCodes(ExemptionCodeList codes)
        {
            BeginUpdate();

            try
            {
                // Check if the selected codes are from a category in the master list
                bool hasCategory = codes.HasCategory;
                int selectedIndex = 0;
                if (hasCategory)
                {
                    // Check for the category in the combo box
                    string category = _masterCodes.GetFullCategoryName(codes.Category);
                    selectedIndex = _categoryComboBox.FindStringExact(category);

                    if (selectedIndex < 0)
                    {
                        // The category was not found in the master list
                        hasCategory = false;

                        // Default to the first category
                        selectedIndex = 0;
                    }
                }

                // Select the appropriate category in the combo box
                if (_categoryComboBox.Items.Count > 0)
                {
                    _categoryComboBox.SelectedIndex = selectedIndex;
                    UpdateListView();
                }

                // Check if the category wasn't found
                if (hasCategory)
                {
                    // Set the checkboxes for the selected codes
                    foreach (ListViewItem item in _codesListView.Items)
                    {
                        // Check this checkbox if the item is in the code list
                        string code = GetCode(item);
                        item.Checked = codes.HasCode(code);
                    }
                }

                // If other text is specified, check the checkbox
                _otherTextCheckBox.Checked = !string.IsNullOrEmpty(codes.OtherText);

                // Set the other text edit box
                _otherTextTextBox.Text = codes.OtherText;
                _otherTextTextBox.Enabled = _otherTextCheckBox.Checked;
            }
            finally
            {
                EndUpdate();
            }
        }

        /// <summary>
        /// Updates the exemption code description text box.
        /// </summary>
        void UpdateDescription()
        {
            // If no item is selected clear the description
            string description = "";
            if (_codesListView.SelectedItems.Count > 0)
            {
                // Get the code of the first selected item
                ListViewItem item = _codesListView.SelectedItems[0];
                string code = GetCode(item);

                // Get the exemption code description
                description = _masterCodes.GetDescription(_categoryComboBox.Text, code);
            }

            // Update the description
            _descriptionTextBox.Text = description;
        }

        /// <summary>
        /// Gets the exemption code corresponding to the specified <see cref="ListViewItem"/>.
        /// </summary>
        /// <param name="item">The <see cref="ListViewItem"/> from which to get an exemption code.
        /// </param>
        /// <returns>The exemption code corresponding to <paramref name="item"/>.</returns>
        string GetCode(ListViewItem item)
        {
            return item.SubItems[_codeColumnHeader.Index].Text;
        }

		/// <summary>
		/// Updates the sample exemption code text box.
		/// </summary>
		void UpdateSample()
		{
            if (_updating <= 0)
            {
                ExemptionCodeList codes = GetSelectedExemptionCodeList();
                _sampleTextBox.Text = codes.ToString();
            }
		}

        /// <summary>
        /// Gets the exemption codes to display when <see cref="ExemptionCodeListDialog"/> is 
        /// first shown.
        /// </summary>
        /// <returns>The exemption codes to display when <see cref="ExemptionCodeListDialog"/> is 
        /// first shown.</returns>
        ExemptionCodeList GetInitialExemptionCodes()
        {
            // Use last used exemption code category for empty exemption codes [FIDSC #5232]
            if (_codes.IsEmpty && !string.IsNullOrEmpty(_lastCategory))
            {
                return new ExemptionCodeList(_lastCategory, null, null);
            }

            return _codes;
        }

        /// <summary>
        /// Updates the list view based on the selected category combo box.
        /// </summary>
        void UpdateListView()
        {
            BeginUpdate();

            try
            {
                // Get the currently selected category
                string category = _categoryComboBox.Text;

                // Populate the list view
                _codesListView.Items.Clear();
                foreach (ExemptionCode code in _masterCodes.GetCodesInCategory(category))
                {
                    string[] cells = new string[] { "", code.Name, code.Summary };
                    _codesListView.Items.Add(new ListViewItem(cells));
                }

                // Select the first item in the list
                if (_codesListView.Items.Count > 0)
                {
                    _codesListView.Items[0].Selected = true;
                }
            }
            finally
            {
                EndUpdate();
            }
        }

		#endregion Methods

		#region Overrides

		/// <summary>
		/// Raises the <see cref="Form.Load"/> event.
		/// </summary>
		/// <param name="e">The event data associated with the <see cref="Form.Load"/> event.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
		        // Enable/disable the 'apply last' button
		        _applyLastButton.Enabled = _enableApplyLast;

		        // Set the control states to match the initial exemption codes
                SelectExemptionCodes( GetInitialExemptionCodes() );
			}
			catch (Exception ex)
			{
				ExtractException.Display("ELI26697", ex);
			}
		}
		 
		#endregion Overrides

		#region Event Handlers

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ComboBox.SelectedIndexChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ComboBox.SelectedIndexChanged"/> event.</param>
        void HandleCategoryComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateListView();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26700", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

	    /// <summary>
        /// Handles the <see cref="ListView.SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ListView.SelectedIndexChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ListView.SelectedIndexChanged"/> event.</param>
        void HandleCodesListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26701", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ListView.ItemChecked"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ListView.ItemChecked"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ListView.ItemChecked"/> event.</param>
        void HandleCodesListViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            try
            {
                UpdateSample();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26707", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        void HandleOtherTextCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Enable/disable the "other text" edit box based on the state of corresponding check box
                _otherTextTextBox.Enabled = _otherTextCheckBox.Checked;

                // Update the sample exemption code edit box
                UpdateSample();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26702", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        void HandleOtherTextTextBoxTextChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateSample();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26703", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleClearButtonClick(object sender, EventArgs e)
        {
            try
            {
                BeginUpdate();

                try
                {
                    // Uncheck all the codes
                    foreach (ListViewItem item in _codesListView.Items)
                    {
                        item.Checked = false;
                    }

                    // Uncheck the other text box
                    _otherTextCheckBox.Checked = false;
                }
                finally
                {
                    EndUpdate();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26704", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleApplyLastButtonClick(object sender, EventArgs e)
        {
            try
            {
                SelectExemptionCodes(_lastAppliedCodes);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26705", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Store codes
                _codes = GetSelectedExemptionCodeList();

                // Store the last used exemption code category
                if (!_codes.IsEmpty)
                {
                    _lastCategory = _codes.Category;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26698", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

		#endregion Event Handlers
	}
}