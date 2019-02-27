using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows for configuration of an <see cref="MicrFinder"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class MicrFinderSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(MicrFinderSettingsDialog).ToString();

        #endregion Constants

        #region Fields

        MiscUtils _miscUtils = new MiscUtils();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrFinderSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="MicrFinder"/> instance to configure.</param>
        public MicrFinderSettingsDialog(MicrFinder settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI46909", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46910");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="MicrFinder"/> to configure.
        /// </summary>
        /// <value>The <see cref="MicrFinder"/> to configure.</value>
        public MicrFinder Settings
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

                _highConfidenceUpDown.Value = Settings.HighConfidenceThreshold;
                _lowConfidenceCheckBox.Checked = Settings.UseLowConfidenceThreshold;
                _lowConfidenceUpDown.Value = Settings.LowConfidenceThreshold;
                _lowConfidenceUpDown.Enabled = _lowConfidenceCheckBox.Checked;
                _filterRegExTextBox.Text = Settings.FilterRegex;
                _splitRoutingCheckBox.Checked = Settings.SplitRoutingNumber;
                _splitAccountCheckBox.Checked = Settings.SplitAccountNumber;
                _splitCheckCheckBox.Checked = Settings.SplitCheckNumber;
                _splitAmountCheckBox.Checked = Settings.SplitAmount;
                _micrSplitterRegexTextBox.Text = Settings.MicrSplitterRegex;
                _filterCharsWhenSplittingCheckBox.Checked = Settings.FilterCharsWhenSplitting;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46908");
            }
        }

        #endregion Overrides

        #region Event Handlers


        void HandleLowConfidenceCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                _lowConfidenceUpDown.Enabled = _lowConfidenceCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46907");
            }
        }

        void HandleFilterRegExComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Trim off the description preceding pre-populated regular expressions.
                var parsedSelection = _filterRegExTextBox.Text.Split(':');
                if (parsedSelection.Length > 1)
                {
                    this.SafeBeginInvoke("ELI46914", () =>
                    {
                        _filterRegExTextBox.Text = string.Join(":", parsedSelection
                            .Skip(1))
                            .Trim();
                        _filterRegExTextBox.SelectAll();
                    });
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46913");
            }
        }

        void HandlRegexFilterFileNameBrowseButton_PathSelected(object sender, PathSelectedEventArgs e)
        {
            try
            {
                _filterRegExTextBox.Text = "file://" + _filterRegExTextBox.Text;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46955");
            }
        }

        void HandleMicrSplitterRegexBrowseButton_PathSelected(object sender, PathSelectedEventArgs e)
        {
            try
            {
                _micrSplitterRegexTextBox.Text = "file://" + _micrSplitterRegexTextBox.Text;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46956");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for <see cref="_okButton"/>
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

                Settings.HighConfidenceThreshold = decimal.ToInt32(_highConfidenceUpDown.Value);
                Settings.UseLowConfidenceThreshold = _lowConfidenceCheckBox.Checked;
                Settings.LowConfidenceThreshold = decimal.ToInt32(_lowConfidenceUpDown.Value);
                Settings.FilterRegex = _filterRegExTextBox.Text;
                Settings.SplitRoutingNumber = _splitRoutingCheckBox.Checked;
                Settings.SplitAccountNumber = _splitAccountCheckBox.Checked;
                Settings.SplitCheckNumber = _splitCheckCheckBox.Checked;
                Settings.SplitAmount = _splitAmountCheckBox.Checked;
                Settings.MicrSplitterRegex = _micrSplitterRegexTextBox.Text;
                Settings.FilterCharsWhenSplitting = _filterCharsWhenSplittingCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46921");
            }
        }

        #endregion Event Handlers

        #region  Private Members

        // It is intentional to no use the results of the Regex construction; this simply to test if the specified pattern is valid.
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Text.RegularExpressions.Regex")]
        bool WarnIfInvalid()
        {
            if (_lowConfidenceCheckBox.Checked &&
                _highConfidenceUpDown.Value < _lowConfidenceUpDown.Value)
            {
                UtilityMethods.ShowMessageBox(
                    "Comparisons to standard OCR are only done in cased where MICR confidence does not " +
                    "meet the basic confidence threshold. Please adjust the confidence levels so the " +
                    "basic threshold is at least as high as the secondary threshold.",
                    "Invalid confidence thresholds",
                    false);
                return true;
            }

            var filterRegex = GetRegex(_filterRegExTextBox.Text);
            if (!string.IsNullOrWhiteSpace(filterRegex))
            {
                try
                {
                    new Regex(filterRegex);
                }
                catch (Exception ex)
                {
                    UtilityMethods.ShowMessageBox(ex.Message, "Invalid Regular Expression Filter", false);
                    return true;
                }
            }

            if (string.IsNullOrWhiteSpace(_micrSplitterRegexTextBox.Text)
                && (_splitRoutingCheckBox.Checked
                    || _splitAccountCheckBox.Checked
                    || _splitCheckCheckBox.Checked
                    || _splitAmountCheckBox.Checked))
            {
                UtilityMethods.ShowMessageBox(
                    "If MICR components are to be split out, a regex must be specified to accomplish the splitting",
                    "Missing splitter regex", false);
                return true;
            }

            var splitterRegex = GetRegex(_micrSplitterRegexTextBox.Text);
            if (!string.IsNullOrWhiteSpace(splitterRegex))
            {
                Regex regex;
                try
                {
                    regex = new Regex(splitterRegex);
                }
                catch (Exception ex)
                {
                    UtilityMethods.ShowMessageBox(ex.Message, "Invalid Regular Expression Filter", false);
                    return true;
                }

                var groupNames = regex.GetGroupNames();

                if (_splitRoutingCheckBox.Checked && !groupNames.Any(n => n == "Routing"))
                {
                    UtilityMethods.ShowMessageBox(
                        "Routing number cannot be split; regular expression does not specify 'Routing' group",
                        "Splitter configuration error", false);
                    return true;
                }

                if (_splitAccountCheckBox.Checked && !groupNames.Any(n => n == "Account"))
                {
                    UtilityMethods.ShowMessageBox(
                        "Account number cannot be split; regular expression does not specify 'Account' group",
                        "Splitter configuration error", false);
                    return true;
                }

                if (_splitCheckCheckBox.Checked && !groupNames.Any(n => n == "CheckNumber"))
                {
                    UtilityMethods.ShowMessageBox(
                        "Check number cannot be split; regular expression does not specify 'CheckNumber' group",
                        "Splitter configuration error", false);
                    return true;
                }

                if (_splitAmountCheckBox.Checked && !groupNames.Any(n => n == "Amount"))
                {
                    UtilityMethods.ShowMessageBox(
                        "Amount cannot be split; regular expression does not specify 'Amount' group",
                        "Splitter configuration error", false);
                    return true;
                }
            }

            return false;
        }

        string GetRegex(string regexSpecification)
        {
            if (regexSpecification.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                var pathTags = new AttributeFinderPathTags(new AFDocument());
                var expandedSpec = pathTags.Expand(regexSpecification.Substring(7));
                if (File.Exists(expandedSpec))
                {
                    return _miscUtils.GetStringOptionallyFromFile(expandedSpec);
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return regexSpecification;
            }
        }

        #endregion Private Members
    }
}
