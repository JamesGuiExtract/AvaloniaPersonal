using Extract.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Allows configuration of a <see cref="FileTagSelectionSettings"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class FileTagSelectionDialog : Form
    {
        #region Fields

        /// <summary>
        /// The <see cref="IFileProcessingDB"/> instance to which the settings pertain.
        /// </summary>
        IFileProcessingDB _fileProcessingDB;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTagSelectionDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="FileTagSelectionSettings"/> to be configured.
        /// </param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> instance to which the
        /// settings pertain.</param>
        public FileTagSelectionDialog(FileTagSelectionSettings settings,
            IFileProcessingDB fileProcessingDB)
        {
            try
            {
                InitializeComponent();

                Settings = settings ?? new FileTagSelectionSettings();
                _fileProcessingDB = fileProcessingDB;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37247");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="FileTagSelectionSettings"/> represented in the configuration
        /// dialog.
        /// </summary>
        /// <value>
        /// The <see cref="FileTagSelectionSettings"/>.
        /// </value>
        public FileTagSelectionSettings Settings
        {
            get;
            private set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                var tagNames = _fileProcessingDB.GetTagNames().ToIEnumerable<string>();
                _selectedTagsCheckedListBox.Items.AddRange(tagNames.ToArray()); 

                _allTagsCheckBox.Checked = Settings.UseAllTags;
                _selectedTagsCheckBox.Checked = Settings.UseSelectedTags;

                if (Settings.SelectedTags != null)
                {
                    foreach (string tagName in Settings.SelectedTags)
                    {
                        int index = _selectedTagsCheckedListBox.FindStringExact(tagName);
                        if (index >= 0)
                        {
                            _selectedTagsCheckedListBox.SetItemChecked(index, true);
                        }
                        else
                        {
                            UtilityMethods.ShowMessageBox(
                                "Tag \"" + tagName + "\" is no longer available.",
                                "Missing tag", false);
                        }
                    }
                }

                _tagFilterCheckBox.Checked = Settings.UseTagFilter;
                _tagFilterTextBox.Text = Settings.TagFilter;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37225");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_allTagsCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAllTagsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_allTagsCheckBox.Checked)
                {
                    _selectedTagsCheckBox.Checked = false;
                    _tagFilterCheckBox.Checked = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37248");
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_selectedTagsCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSelectedTagsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _selectedTagsCheckedListBox.Enabled = _selectedTagsCheckBox.Checked;

                if (_selectedTagsCheckBox.Checked)
                {
                    _allTagsCheckBox.Checked = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37249");
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_tagFilterCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTagFilterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _tagFilterTextBox.Enabled = _tagFilterCheckBox.Checked;

                if (_tagFilterCheckBox.Checked)
                {
                    _allTagsCheckBox.Checked = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37250");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                var newSettings = new FileTagSelectionSettings(
                    _allTagsCheckBox.Checked,
                    _selectedTagsCheckBox.Checked,
                    _selectedTagsCheckedListBox.CheckedItems.OfType<string>().ToArray(),
                    _tagFilterCheckBox.Checked,
                    _tagFilterTextBox.Text);

                if (!newSettings.UseAllTags &&
                    !newSettings.UseSelectedTags &&
                    !newSettings.UseTagFilter)
                {
                    UtilityMethods.ShowMessageBox(
                        "Select at least one category of tags to include.",
                        "Invalid configuration", true);
                    return;
                }

                if (!newSettings.UseAllTags)
                {
                    var availableTags = _selectedTagsCheckedListBox.Items.OfType<string>();

                    if (newSettings.UseSelectedTags && !newSettings.SelectedTags.Any())
                    {
                        _selectedTagsCheckedListBox.Focus();
                        UtilityMethods.ShowMessageBox(
                            "Select at least one tag to include in the list box.", 
                            "Invalid configuration", true);
                        return;
                    }

                    if (newSettings.UseTagFilter)
                    {
                        if (string.IsNullOrWhiteSpace(newSettings.TagFilter))
                        {
                            _tagFilterTextBox.Focus();
                            UtilityMethods.ShowMessageBox("Specify the tag filter.",
                                "Invalid configuration", true);
                            return;
                        }
                        else if (!newSettings.GetTagsMatchingFilter(availableTags).Any())
                        {
                            DialogResult response = MessageBox.Show(
                                "There are not currently any tags that match the filter \"" +
                                newSettings.TagFilter +
                                "\". Use this filter anyway?", "Warning", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, 0);

                            if (response == DialogResult.No)
                            {
                                return;
                            }
                        }
                    }
                }

                Settings = newSettings;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37226");
            }
        }

        #endregion Event Handlers
    }
}
