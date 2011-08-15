using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Extract.Redaction.CustomComponentsHelper
{
    /// <summary>
    /// Settings dialog used to configure the
    /// <see cref="CreateRedactedImageAdvancedLabelSettings"/>.
    /// </summary>
    public partial class CreateRedactedImageAdvanceLabelingDialog : Form
    {
        #region Constants

        /// <summary>
        /// The file filter used in the save and load dialogs.
        /// </summary>
        const string _FILE_FILTER = "List files (*.lst)|*.lst|All files (*.*)|*.*||";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Gets the replacement strings.
        /// </summary>
        List<StringPair> _replacementStrings = new List<StringPair>();

        /// <summary>
        /// Gets a value indicating whether text casing should be auto adjusted.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if casing should be adjusted; otherwise, <see langword="false"/>.
        /// </value>
        public bool AutoAdjustCase { get; private set; }

        /// <summary>
        /// Gets the prefix text for the first type instance.
        /// </summary>
        public string PrefixText { get; private set; }

        /// <summary>
        /// Gets the suffix text for the first type instance.
        /// </summary>
        public string SuffixText { get; private set; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRedactedImageAdvanceLabelingDialog"/> class.
        /// </summary>
        public CreateRedactedImageAdvanceLabelingDialog()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRedactedImageAdvanceLabelingDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public CreateRedactedImageAdvanceLabelingDialog(CreateRedactedImageAdvancedLabelSettings settings)
        {
            try
            {
                InitializeComponent();

                // Set the appropriate path tags class for the prefix and suffix tags
                _prefixTags.PathTags = new RedactionTextPathTags();
                _suffixTags.PathTags = new RedactionTextPathTags();

                if (settings != null)
                {
                    FillListView(settings.ReplacementStrings);
                    _checkAutoCase.Checked = settings.AutoAdjustCase;
                    string temp = settings.PrefixFirstInstance;
                    if (!string.IsNullOrEmpty(temp))
                    {
                        _checkPrefixText.Checked = true;
                        _textPrefix.Text = temp;
                    }
                    temp = settings.SuffixFirstInstance;
                    if (!string.IsNullOrEmpty(temp))
                    {
                        _checkSuffixText.Checked = true;
                        _textSuffix.Text = temp;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31748");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the collection of replacement string pairs
        /// </summary>
        public ReadOnlyCollection<StringPair> ReplacementStrings
        {
            get
            {
                return _replacementStrings.AsReadOnly();
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                UpdatePrefixEnabledState();
                UpdateSuffixEnabledState();
                UpdateListButtonEnabledState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31796");
            }
        }

        /// <summary>
        /// Handles the add replacement clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleAddReplacementClicked(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = CreateTwoEntryDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Get the list of selected indexes
                        var selected = _listReplacements.SelectedIndices;

                        // Find the max selected index
                        // OR get the index for the last item
                        var index = selected.Count > 0
                            ? selected.Cast<int>().Max() + 1 : _listReplacements.Items.Count;

                        var item = new ListViewItem(
                            new string[] { dialog.FirstValue, dialog.SecondValue });
                        _listReplacements.Items.Insert(index, item);

                        // Select the new item
                        SelectListItem(index);
                        _listReplacements.Invalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31738");
            }
        }

        /// <summary>
        /// Handles the remove replacement clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleRemoveReplacementClicked(object sender, EventArgs e)
        {
            try
            {
                var selected = _listReplacements.SelectedIndices;
                if (selected.Count > 0)
                {
                    var result = MessageBox.Show(
                        "Are you sure you want to delete the selected item(s)?",
                         "Delete Items", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                         MessageBoxDefaultButton.Button2, 0);
                    if (result == DialogResult.Yes)
                    {
                        foreach (var index in selected.Cast<int>().OrderByDescending(x => x))
                        {
                            _listReplacements.Items.RemoveAt(index);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31739");
            }
        }

        /// <summary>
        /// Handles the modify replacement clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleModifyReplacementClicked(object sender, EventArgs e)
        {
            try
            {
                var selected = _listReplacements.SelectedIndices;
                if (selected.Count == 1)
                {
                    var index = selected[0];
                    var item = _listReplacements.Items[index];
                    var key = item.SubItems[0].Text;
                    var value = item.SubItems[1].Text;
                    using (var dialog = CreateTwoEntryDialog())
                    {
                        dialog.FirstValue = key;
                        dialog.SecondValue = value;

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            item.SubItems[0].Text = dialog.FirstValue;
                            item.SubItems[1].Text = dialog.SecondValue;
                            _listReplacements.Invalidate();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31795");
            }
        }

        /// <summary>
        /// Handles up clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleUpClicked(object sender, EventArgs e)
        {
            try
            {
                var selected = _listReplacements.SelectedIndices;
                if (selected.Count == 1)
                {
                    var index = selected[0];
                    if (index > 0)
                    {
                        var item = _listReplacements.Items[index];
                        _listReplacements.Items.RemoveAt(index);
                        index--;
                        _listReplacements.Items.Insert(index, item);

                        SelectListItem(index);
                        _listReplacements.Invalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31740");
            }
        }

        /// <summary>
        /// Handles down clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleDownClicked(object sender, EventArgs e)
        {
            try
            {
                var selected = _listReplacements.SelectedIndices;
                if (selected.Count == 1)
                {
                    var index = selected[0];
                    if (index < _listReplacements.Items.Count - 1)
                    {
                        var item = _listReplacements.Items[index];
                        _listReplacements.Items.RemoveAt(index);
                        index++;
                        _listReplacements.Items.Insert(index, item);

                        SelectListItem(index);
                        _listReplacements.Invalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31741");
            }
        }

        /// <summary>
        /// Handles the load list clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleLoadListClicked(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("This will clear all items in the list. Are you sure?",
                     "Clear List And Load From File", MessageBoxButtons.YesNo,
                     MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, 0) == DialogResult.No)
                {
                    return;
                }

                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = _FILE_FILTER;
                    dialog.CheckFileExists = true;
                    dialog.Multiselect = false;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string[] lines = File.ReadAllLines(dialog.FileName);
                        int length = lines.Length;
                        if (length % 2 != 0)
                        {
                            MessageBox.Show(
                                "Invalid replacement list file. Must contain an even number of lines",
                                "Invalid Replacement File", MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                            return;
                        }

                        // Clear the list and add the new items
                        _listReplacements.Items.Clear();
                        for (int i = 0; i < length; i++)
                        {
                            _listReplacements.Items.Add(new ListViewItem(
                                new string[] { lines[i], lines[++i] }));
                        }

                        // If an item was added, select the top item
                        if (length > 0)
                        {
                            SelectListItem(0);
                        }
                        _listReplacements.Invalidate();

                        // Update the enabled states
                        UpdateListButtonEnabledState();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31742");
            }
        }

        /// <summary>
        /// Handles the save list clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleSaveListClicked(object sender, EventArgs e)
        {
            try
            {
                var list = BuildListFromListView();
                if (list.Count > 0)
                {
                    using (var dialog = new SaveFileDialog())
                    {
                        dialog.CheckPathExists = true;
                        dialog.DefaultExt = ".lst";
                        dialog.Filter = _FILE_FILTER;
                        dialog.AddExtension = true;
                        dialog.ValidateNames = true;
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            var sb = new StringBuilder(list.Count * 80);
                            foreach (var item in list)
                            {
                                sb.AppendLine(item.First);
                                sb.AppendLine(item.Second);
                            }
                            File.WriteAllText(dialog.FileName, sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31743");
            }
        }

        /// <summary>
        /// Handles the replacement list selection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleReplacementListSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateListButtonEnabledState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31744");
            }
        }

        /// <summary>
        /// Handles the ok clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleOkClicked(object sender, EventArgs e)
        {
            try
            {
                if (_checkPrefixText.Checked && string.IsNullOrEmpty(_textPrefix.Text))
                {
                    MessageBox.Show("Must specify prefix text.",
                        "Prefix Text Empty", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);
                    _textPrefix.Focus();
                    return;
                }
                if (_checkSuffixText.Checked && string.IsNullOrEmpty(_textSuffix.Text))
                {
                    MessageBox.Show("Must specify suffix text.",
                        "Suffix Text Empty", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);
                    _textSuffix.Focus();
                    return;
                }

                // Get the auto adjust and prefix/suffix settings
                AutoAdjustCase = _checkAutoCase.Checked;
                PrefixText = _checkPrefixText.Checked ? _textPrefix.Text : string.Empty;
                SuffixText = _checkSuffixText.Checked ? _textSuffix.Text : string.Empty;

                _replacementStrings = BuildListFromListView();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31745");
            }
        }

        /// <summary>
        /// Handles the prefix check changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandlePrefixCheckChanged(object sender, EventArgs e)
        {
            try
            {
                UpdatePrefixEnabledState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31746");
            }
        }

        /// <summary>
        /// Handles the suffix check changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleSuffixCheckChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateSuffixEnabledState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31747");
            }
        }

        #endregion EventHandlers

        #region Methods

        /// <summary>
        /// Creates the two entry dialog.
        /// </summary>
        /// <returns>A new two entry dialog.</returns>
        static TwoValueEntryDialog CreateTwoEntryDialog()
        {
            return new TwoValueEntryDialog(false,
                  "Enter replacement string pair",
                  "Specify string to be replaced:",
                  "Specify replacement string:");
        }

        /// <summary>
        /// Updates the enabled state of the prefix edit box and tags button.
        /// </summary>
        void UpdatePrefixEnabledState()
        {
            var isChecked = _checkPrefixText.Checked;
            _textPrefix.Enabled = isChecked;
            _prefixTags.Enabled = isChecked;
        }

        /// <summary>
        /// Updates the enabled state of the suffix edit box and tags button.
        /// </summary>
        void UpdateSuffixEnabledState()
        {
            var isChecked = _checkSuffixText.Checked;
            _textSuffix.Enabled = isChecked;
            _suffixTags.Enabled = isChecked;
        }

        /// <summary>
        /// Updates the enabled state of the list buttons.
        /// </summary>
        void UpdateListButtonEnabledState()
        {
            var selected = _listReplacements.SelectedIndices;
            var count = selected.Count;
            _buttonModify.Enabled = count == 1;
            _buttonRemove.Enabled = count > 0;
            _buttonUp.Enabled = count == 1 && selected[0] > 0;
            _buttonDown.Enabled = count == 1 && selected[0] < (_listReplacements.Items.Count - 1);
        }

        /// <summary>
        /// Builds the list from list view.
        /// </summary>
        /// <returns></returns>
        List<StringPair> BuildListFromListView()
        {
            // Fill a list with the values from the list control
            var list = new List<StringPair>(_listReplacements.Items.Count);
            foreach (ListViewItem item in _listReplacements.Items)
            {
                list.Add(new StringPair()
                    {
                        First = item.SubItems[0].Text,
                        Second = item.SubItems[1].Text
                    });
            }

            return list;
        }

        /// <summary>
        /// Fills the list view with the specified list.
        /// </summary>
        /// <param name="list">The list to fill the view with.</param>
        void FillListView(IList<StringPair> list)
        {
            _listReplacements.Items.Clear();
            foreach (var item in list)
            {
                _listReplacements.Items.Add(
                    new ListViewItem(new string[] { item.First, item.Second }));
            }
            _listReplacements.Invalidate();
        }

        /// <summary>
        /// Selects the list item.
        /// </summary>
        /// <param name="index">The index.</param>
        void SelectListItem(int index)
        {
            _listReplacements.SelectedIndices.Clear();
            _listReplacements.SelectedIndices.Add(index);
        }

        #endregion Methods
    }
}
