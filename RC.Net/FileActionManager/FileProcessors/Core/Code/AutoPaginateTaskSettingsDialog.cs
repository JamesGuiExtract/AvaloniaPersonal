﻿using Extract.FileActionManager.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Linq;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Displays and allows editing of a <see cref="AutoPaginateTask"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class AutoPaginateTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// A special action name option that will clear any previously specified action.
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
        /// Initializes a new instance of the <see cref="AutoPaginateTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="AutoPaginateTask"/> instance that is being
        /// configured.</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> this instance is
        /// associated with.</param>
        public AutoPaginateTaskSettingsDialog(IAutoPaginateTask settings,
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

                Settings = settings;
                _fileProcessingDB = fileProcessingDB;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47033");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IAutoPaginateTask"/> represented in the configuration dialog.
        /// </summary>
        /// <value>
        /// The <see cref="IAutoPaginateTask"/>.
        /// </value>
        public IAutoPaginateTask Settings
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

                _sourceIfFullyPaginatedActionComboBox.Items.Add(_NO_ACTION);
                _outputActionComboBox.Items.Add(_NO_ACTION);

                var actionNames = _fileProcessingDB
                    .GetActions()
                    .GetKeys()
                    .ToIEnumerable<string>()
                    .ToList();

                foreach (var actionName in actionNames)
                {
                    _sourceIfFullyPaginatedActionComboBox.Items.Add(actionName);
                    _sourceIfNotFullyPaginatedActionComboBox.Items.Add(actionName);
                    _outputActionComboBox.Items.Add(actionName);
                }

                _outputPathTextBox.Text = Settings.OutputPath;
                _sourceIfFullyPaginatedActionComboBox.Text = Settings.SourceActionIfFullyPaginated;
                _sourceIfNotFullyPaginatedActionComboBox.Text = Settings.SourceActionIfNotFullyPaginated;
                _outputActionComboBox.Text = Settings.OutputAction;
                _documentDataAssemblyTextBox.Text = Settings.DocumentDataPanelAssembly;
                _qualifierConditionConfigurableObjectControl.ConfigurableObject =
                    Settings.AutoPaginateQualifier as ICategorizedComponent;
                _autoPaginatedTagComboBox.Text = Settings.AutoPaginatedTag;
                _autoRotateCheckBox.Checked = Settings.AutoRotatePages;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47034");
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

                if (!string.IsNullOrWhiteSpace(_sourceIfFullyPaginatedActionComboBox.Text) &&
                    !_sourceIfFullyPaginatedActionComboBox.Items.Contains(_sourceIfFullyPaginatedActionComboBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "The action to set fully paginated source source files to is not valid.",
                        "Invalid configuration", true);
                    _sourceIfFullyPaginatedActionComboBox.Focus();

                    return;
                }

                if (string.IsNullOrWhiteSpace(_sourceIfNotFullyPaginatedActionComboBox.Text) ||
                    !_sourceIfNotFullyPaginatedActionComboBox.Items.Contains(_sourceIfNotFullyPaginatedActionComboBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "The action to set source files that are not fully paginated to is not valid.",
                        "Invalid configuration", true);
                    _sourceIfNotFullyPaginatedActionComboBox.Focus();

                    return;
                }

                if (string.IsNullOrWhiteSpace(_outputActionComboBox.Text) ||
                    !_outputActionComboBox.Items.Contains(_outputActionComboBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "The action to set pagination output files to is not valid.",
                        "Invalid configuration", true);

                    return;
                }

                Settings.SourceActionIfFullyPaginated = _sourceIfFullyPaginatedActionComboBox.Text;
                Settings.SourceActionIfNotFullyPaginated = _sourceIfNotFullyPaginatedActionComboBox.Text;
                Settings.OutputPath = _outputPathTextBox.Text;
                Settings.OutputAction = _outputActionComboBox.Text;
                Settings.DocumentDataPanelAssembly = _documentDataAssemblyTextBox.Text;
                Settings.AutoPaginateQualifier =
                    _qualifierConditionConfigurableObjectControl.ConfigurableObject as IPaginationCondition;
                Settings.AutoPaginatedTag = _autoPaginatedTagComboBox.Text;
                Settings.AutoRotatePages = _autoRotateCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47035");
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of _sourceIfFullyPaginatedActionComboBox in order to clear the
        /// combo if NONE is selected.
        /// </summary>
        void HandleActionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBox comboBox = (ComboBox)sender;
                if (comboBox.Text == _NO_ACTION)
                {
                    // If "<None>" was selected, interpret that to mean the text should be cleared.
                    this.SafeBeginInvoke("ELI47036", () => comboBox.Text = "");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47037");
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of _qualifierConditionConfigurableObjectControl
        /// in order to set the status of the selected condition as a pagination condition.
        /// </summary>
        void HandleQualifier_SelectedObjectTypeChanged(object sender, EventArgs e)
        {
            try
            {
                if (_qualifierConditionConfigurableObjectControl.ConfigurableObject
                    is IPaginationCondition paginationCondition)
                {
                    paginationCondition.IsPaginationCondition = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47086");
            }
        }

        #endregion Event Handlers
    }
}