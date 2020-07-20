using Extract.FileActionManager.Forms;
using Extract.Utilities;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    ///  A <see cref="Form"/> to view and modify settings for an <see cref="SetMetadataTask"/> instance.
    /// </summary>
    public partial class SetMetadataTaskSettingsDialog : Form
    {
        /// <summary>
        /// The file processing database
        /// </summary>
        IFileProcessingDB _fileProcessingDB = new FileProcessingDB();

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SetMetadataTaskSettingsDialog"/> class.
        /// </summary>
        public SetMetadataTaskSettingsDialog()
        {
            try
            {
                InitializeComponent();

                _fileProcessingDB.ConnectLastUsedDBThisProcess();

                _fieldNameTagsButton.PathTags = new FileActionManagerPathTags();
                _valueTagsButton.PathTags = new FileActionManagerPathTags();

                _okButton.Click += HandleOkButtonClick;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49966");
            }
        }

        #endregion Constructors

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _fieldNameTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                _fieldNameTextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                _fieldNameTextBox.AutoCompleteCustomSource = new AutoCompleteStringCollection();

                var fieldNames = _fileProcessingDB.GetMetadataFieldNames()
                    .ToIEnumerable<string>()
                    .ToArray();

                // Add an extra copy of every value with a pre-pended space char so that
                // pressing space will bring up a list with all available field names.
                _fieldNameTextBox.AutoCompleteCustomSource.AddRange(
                    fieldNames
                        .Select(name => " " + name)
                        .Union(fieldNames)
                        .ToArray());
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43515");
            }
        }

        #region Properties

        /// <summary>
        /// Gets or sets the name of the file to be generated.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FieldName
        {
            get
            {
                return _fieldNameTextBox.Text;
            }

            set
            {
                _fieldNameTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the contents of the file to be generated.
        /// </summary>
        /// <value>
        /// The contents of the file.
        /// </value>
        public string Value
        {
            get
            {
                return _valueTextBox.Text;
            }

            set
            {
                _valueTextBox.Text = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="LostFocus"/> event of the <see cref="_fieldNameTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void HandleFieldNameTextBox_LostFocus(object sender, System.EventArgs e)
        {
            try
            {
                // Remove any leading whitespace that may have been left via an auto-complete selection.
                _fieldNameTextBox.Text = _fieldNameTextBox.Text.TrimStart(' ');
            }
            catch (System.Exception ex)
            {
                ex.ExtractDisplay("ELI43527");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> event.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Remove any leading whitespace that may have been left via an auto-complete selection.
                _fieldNameTextBox.Text = _fieldNameTextBox.Text.TrimStart(' ');

                if (WarnIfInvalid())
                {
                    return;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31842", ex);
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
            if (string.IsNullOrWhiteSpace(_fieldNameTextBox.Text))
            {
                _fieldNameTextBox.Focus();
                MessageBox.Show("Please specify the name of the metadata field to set.",
                    "Missing field name", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (!Regex.IsMatch(_fieldNameTextBox.Text, @"<|>|(\$[\s\S]+?\([\s\S]*?\))"))
            {
                if (!_fileProcessingDB.GetMetadataFieldNames()
                    .ToIEnumerable<string>()
                    .Any(fieldName => fieldName.Equals(_fieldNameTextBox.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    _fieldNameTextBox.Focus();

                    MessageBox.Show("The specified metadata field does not exist in the database.",
                        "Unknown field.", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                    return true;
                }
            }

            return false;
        }

        #endregion Private Members
    }
}
