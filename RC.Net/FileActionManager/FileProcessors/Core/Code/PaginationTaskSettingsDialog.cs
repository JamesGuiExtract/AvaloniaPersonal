using Extract.FileActionManager.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Displays and allows editing of a <see cref="PaginationTask"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class PaginationTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// A special action name option that 
        /// </summary>
        const string _NO_ACTION = "<None>";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="IFileProcessingDB"/> this instance is associated with.
        /// </summary>
        IFileProcessingDB _fileProcessingDB;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="PaginationTask"/> instance that is being
        /// configured.</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> this instance is
        /// associated with.</param>
        public PaginationTaskSettingsDialog(IPaginationTask settings,
            IFileProcessingDB fileProcessingDB)
        {
            try
            {
                InitializeComponent();

                var outputPathTags = new FileActionManagerPathTags();
                outputPathTags.AddTag(PaginationSettings.SubDocIndexTag, "");
                outputPathTags.AddTag(PaginationSettings.FirstPageTag, "");
                outputPathTags.AddTag(PaginationSettings.LastPageTag, "");

                _outputPathPathTags.PathTags = outputPathTags;
                _documentDataAssemblyPathTags.PathTags = new FileActionManagerPathTags();
                _expectedPaginationAttributesPathTagButton.PathTags = new FileActionManagerPathTags();

                Settings = settings;
                _fileProcessingDB = fileProcessingDB;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40142");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IPaginationTask"/> represented in the configuration dialog.
        /// </summary>
        /// <value>
        /// The <see cref="IPaginationTask"/>.
        /// </value>
        public IPaginationTask Settings
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

                _sourceActionComboBox.Items.Add(_NO_ACTION);

                foreach (var actionName in _fileProcessingDB
                    .GetActions()
                    .GetKeys()
                    .ToIEnumerable<string>())
                {
                    _sourceActionComboBox.Items.Add(actionName);
                    _outputActionComboBox.Items.Add(actionName);
                }

                _outputPathTextBox.Text = Settings.OutputPath;
                _sourceActionComboBox.Text = Settings.SourceAction;
                _outputActionComboBox.Text = Settings.OutputAction;
                _documentDataAssemblyTextBox.Text = Settings.DocumentDataPanelAssembly;
                _expectedPaginationAttributesCheckBox.Checked = Settings.OutputExpectedPaginationAttributesFiles;
                _expectedPaginationAttributesTextBox.Text = Settings.ExpectedPaginationAttributesOutputPath;
                _expectedPaginationAttributesTextBox.Enabled
                    = _expectedPaginationAttributesBrowseButton.Enabled
                    = _expectedPaginationAttributesPathTagButton.Enabled
                    = _expectedPaginationAttributesCheckBox.Checked;
                _singleDocModeCheckBox.Checked = Settings.SingleSourceDocumentMode;
                _defaultToCollapsedCheckBox.Checked = Settings.DefaultToCollapsed;
                _autoRotateCheckBox.Checked = Settings.AutoRotateImages;
                _selectAllVisibleCheckBox.Checked = Settings.SelectAllCheckBoxVisible;
                _loadNextDocumentVisibleCheckBox.Checked = Settings.LoadNextDocumentVisible;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40143");
            }
        }

        #endregion Overrides

        #region Event Handlers

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
                if (string.IsNullOrWhiteSpace(_outputPathTextBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "The pagination output path must be specified.",
                        "Invalid configuration", true);

                    return;
                }

                if (!string.IsNullOrWhiteSpace(_sourceActionComboBox.Text) &&
                    !_sourceActionComboBox.Items.Contains(_sourceActionComboBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "The action to set source files to is not valid.",
                        "Invalid configuration", true);

                    return;
                }

                if (string.IsNullOrWhiteSpace(_outputActionComboBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "An action must be specified for output documents; this action will be " +
                        "responsible for the creation of the document via the \"Pagination: Create output\" task.",
                        "Invalid configuration", true);

                    return;
                }
                else if (!_outputActionComboBox.Items.Contains(_outputActionComboBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "The action to set pagination output files to is not valid.",
                        "Invalid configuration", true);

                    return;
                }

                Settings.SourceAction = _sourceActionComboBox.Text;
                Settings.OutputPath = _outputPathTextBox.Text;
                Settings.OutputAction = _outputActionComboBox.Text;
                Settings.DocumentDataPanelAssembly = _documentDataAssemblyTextBox.Text;
                Settings.OutputExpectedPaginationAttributesFiles = _expectedPaginationAttributesCheckBox.Checked;
                Settings.ExpectedPaginationAttributesOutputPath = _expectedPaginationAttributesTextBox.Text;
                Settings.SingleSourceDocumentMode = _singleDocModeCheckBox.Checked;
                Settings.DefaultToCollapsed = _defaultToCollapsedCheckBox.Checked;
                Settings.AutoRotateImages = _autoRotateCheckBox.Checked;
                Settings.SelectAllCheckBoxVisible = _selectAllVisibleCheckBox.Checked;
                Settings.LoadNextDocumentVisible = _loadNextDocumentVisibleCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40144");
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of
        /// <see cref="_sourceActionComboBox"/> and  <see cref="_outputActionComboBox"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleActionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBox comboBox = (ComboBox)sender;
                if (comboBox.Text == _NO_ACTION)
                {
                    // If "<None>" was selected, interpret that to mean the text should be cleared.
                    this.SafeBeginInvoke("ELI40145", () => comboBox.Text = "");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40146");
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the _expectedPaginationAttributesCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleExpectedPaginationAttributesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _expectedPaginationAttributesTextBox.Enabled
                    = _expectedPaginationAttributesBrowseButton.Enabled
                    = _expectedPaginationAttributesPathTagButton.Enabled
                    = _expectedPaginationAttributesCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40194");
            }
        }

        #endregion Event Handlers
    }
}
