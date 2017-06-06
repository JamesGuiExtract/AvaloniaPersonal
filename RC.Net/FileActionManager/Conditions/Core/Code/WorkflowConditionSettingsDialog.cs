using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;
using static System.FormattableString;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// A <see cref="Form"/> that allows for configuration of an <see cref="WorkflowCondition"/>
    /// instance.
    /// </summary>
    public partial class WorkflowConditionSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(WorkflowConditionSettingsDialog).ToString();

        #endregion Constants

        #region Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowConditionSettingsDialog"/> class.
        /// </summary>
        public WorkflowConditionSettingsDialog(WorkflowCondition settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI43469",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43470");
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
        public WorkflowCondition Settings { get; set; }

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
                    _includeComboBox.SelectedIndex = Settings.Inclusive ? 0 : 1;

                    var fileProcessingDb = new FileProcessingDB();
                    fileProcessingDb.ConnectLastUsedDBThisProcess();

                    if (!fileProcessingDb.UsingWorkflows)
                    {
                        UtilityMethods.ShowMessageBox("This condition requires workflows in the database.",
                            "No workflows", true);
                        Close();
                        return;
                    }

                    var workflowNames = fileProcessingDb.GetWorkflows()
                        .ComToDictionary()
                        .Keys
                        .ToArray();

                    var selectedWorkflows = new HashSet<string>(
                        Settings.SelectedWorkflows.ToIEnumerable<string>(), StringComparer.OrdinalIgnoreCase);
                    var missingWorkflows = selectedWorkflows.Except(workflowNames);
                    if (missingWorkflows.Any())
                    {
                        UtilityMethods.ShowMessageBox(
                            Invariant($"The action(s) {string.Join(", ", missingWorkflows)} no longer exist(s) in the database.\r\n\r\n") +
                                $"If this condition is saved, these workflows will be removed from the settings.",
                            "Missing workflows", true);
                    }

                    foreach (var workflowName in workflowNames)
                    {
                        int index = _workflowCheckListBox.Items.Add(workflowName);
                        _workflowCheckListBox.SetItemCheckState(index,
                            selectedWorkflows.Contains(workflowName)
                                ? CheckState.Checked
                                : CheckState.Unchecked);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43468");
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

                Settings.Inclusive = (_includeComboBox.SelectedIndex == 0);
                Settings.SelectedWorkflows = _workflowCheckListBox.CheckedItems
                    .OfType<string>()
                    .ToVariantVector();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43471");
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
            ExtractException.Assert("ELI43472",
                "Workflow condition settings have not been provided.", Settings != null);

            if (_workflowCheckListBox.CheckedIndices.Count == 0)
            {
                UtilityMethods.ShowMessageBox("At least one workflow must be selected.",
                    "Select a workflow", true);

                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
