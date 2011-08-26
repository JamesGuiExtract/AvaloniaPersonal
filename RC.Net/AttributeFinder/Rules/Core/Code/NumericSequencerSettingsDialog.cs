using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extract.Licensing;
using System.Globalization;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="NumericSequencer"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class NumericSequencerSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(NumericSequencerSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericSequencerSettingsDialog"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="NumericSequencer"/> instance to configure.</param>
        public NumericSequencerSettingsDialog(NumericSequencer settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI33429",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                // Define control update behavior.
                _sortCheckBox.CheckedChanged += ((sender, args) =>
                    _sortComboBox.Enabled = _sortCheckBox.Checked);

                _contractRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        if (_contractRadioButton.Checked)
                        {
                            _sortCheckBox.Checked = true;
                            _sortCheckBox.Enabled = false;
                            _sortComboBox.Enabled = true;
                        }
                        else
                        {
                            _sortCheckBox.Enabled = true;
                            _sortComboBox.Enabled = _sortComboBox.Enabled;
                        }
                    });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33430");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="NumericSequencer"/> to configure.
        /// </summary>
        /// <value>The <see cref="NumericSequencer"/> to configure.</value>
        public NumericSequencer Settings
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
                    _expandRadioButton.Checked = Settings.ExpandSequence;
                    _contractRadioButton.Checked = !Settings.ExpandSequence;
                    _sortCheckBox.Checked = Settings.Sort;
                    _sortComboBox.SelectedIndex = Settings.AscendingSortOrder ? 0 : 1;
                    _sortComboBox.Enabled = _sortCheckBox.Checked;
                    _eliminateDuplicatesCheckBox.Checked = Settings.EliminateDuplicates;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33431");
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
                Settings.ExpandSequence = _expandRadioButton.Checked;
                Settings.Sort = _sortCheckBox.Checked;
                Settings.AscendingSortOrder = (_sortComboBox.SelectedIndex == 0);
                Settings.EliminateDuplicates = _eliminateDuplicatesCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33432");
            }
        }

        #endregion Event Handlers
    }
}
