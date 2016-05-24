using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Displays and allows editing of a <see cref="PaginationSettings"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class PaginationSettingsDialog : Form
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
        /// Initializes readable names for the <see cref="EFilePriority"/> enum.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static PaginationSettingsDialog()
        {
            EFilePriority.kPriorityHigh.SetReadableValue("High");
            EFilePriority.kPriorityAboveNormal.SetReadableValue("Above normal");
            EFilePriority.kPriorityNormal.SetReadableValue("Normal");
            EFilePriority.kPriorityBelowNormal.SetReadableValue("Below normal");
            EFilePriority.kPriorityLow.SetReadableValue("Low");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="PaginationSettings"/> instance that is being
        /// configured.</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> this instance is
        /// associated with.</param>
        public PaginationSettingsDialog(PaginationSettings settings,
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

                Settings = settings;
                _fileProcessingDB = fileProcessingDB;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39858");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="PaginationSettings"/> represented in the configuration
        /// dialog.
        /// </summary>
        /// <value>
        /// The <see cref="PaginationSettings"/>.
        /// </value>
        public PaginationSettings Settings
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
                _outputActionComboBox.Items.Add(_NO_ACTION);

                foreach (var actionName in _fileProcessingDB
                    .GetActions()
                    .GetKeys()
                    .ToIEnumerable<string>())
                {
                    _sourceActionComboBox.Items.Add(actionName);
                    _outputActionComboBox.Items.Add(actionName);
                }

                _outputPriorityComboBox.InitializeWithReadableEnum<EFilePriority>(true);

                _outputPathTextBox.Text = Settings.PaginationOutputPath;
                _sourceActionComboBox.Text = Settings.PaginationSourceAction;
                _outputActionComboBox.Text = Settings.PaginationOutputAction;
                _outputPriorityComboBox.SelectEnumValue<EFilePriority>(Settings.PaginatedOutputPriority);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39857");
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
                var newSettings = new PaginationSettings(
                    _sourceActionComboBox.Text,
                    _outputPathTextBox.Text,
                    _outputActionComboBox.Text,
                    _outputPriorityComboBox.ToEnumValue<EFilePriority>());

                if (string.IsNullOrWhiteSpace(_outputPathTextBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "The pagination output path must be specified.",
                        "Invalid configuration", true);

                    return;
                }

                Settings = newSettings;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39859");
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
                    this.SafeBeginInvoke("ELI39893", () => comboBox.Text = "");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39888");
            }
        }

        #endregion Event Handlers
    }
}
