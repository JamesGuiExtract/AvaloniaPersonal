using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify settings for an <see cref="ExtractImageAreaTask"/>
    /// instance.
    /// </summary>
    public partial class ExtractImageAreaTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(ExtractImageAreaTaskSettingsDialog).ToString();

        #endregion Constants

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractImageAreaTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="ExtractImageAreaTask"/> instance to configure.</param>
        public ExtractImageAreaTaskSettingsDialog(ExtractImageAreaTask settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI33208",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                _dataFilePathTagsButton.PathTags = new FileActionManagerPathTags();
                _outputFilePathTagsButton.PathTags = Settings.OutputPathTags;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33209");
            }
        }

        #endregion Contructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="ExtractImageAreaTask"/> to configure.
        /// </summary>
        /// <value>
        /// The <see cref="ExtractImageAreaTask"/> to configure.
        /// </value>
        public ExtractImageAreaTask Settings
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
                    _dataFileTextBox.Text = Settings.DataFileName;
                    _attributeQueryTextBox.Text = Settings.AttributeQuery;

                    if (Settings.UseOverallBounds)
                    {
                        _overallBoundsRadioButton.Checked = true;
                    }
                    else
                    {
                        _separateZonesRadioButton.Checked = true;
                    }

                    if (Settings.OutputAllAreas)
                    {
                        _allAreasRadioButton.Checked = true;
                    }
                    else
                    {
                        _firstAreaRadioButton.Checked = true;
                    }

                    _outputFileTextBox.Text = Settings.OutputFileName;
                    _allowOutputAppendCheckBox.Checked = Settings.AllowOutputAppend;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33201");
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

                Settings.DataFileName = _dataFileTextBox.Text;
                Settings.AttributeQuery = _attributeQueryTextBox.Text;
                Settings.UseOverallBounds = _overallBoundsRadioButton.Checked;
                Settings.OutputAllAreas = _allAreasRadioButton.Checked;
                Settings.OutputFileName = _outputFileTextBox.Text;
                Settings.AllowOutputAppend = _allowOutputAppendCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33202");
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
            ExtractException.Assert("ELI33203",
                "Extract image area task settings have not been provided.", Settings != null);

            if (string.IsNullOrWhiteSpace(_dataFileTextBox.Text))
            {
                _dataFileTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify the data file.",
                    "Data file not specified", false);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_outputFileTextBox.Text))
            {
                _outputFileTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify the output file.",
                    "Output file not specified", false);
                return true;
            }

            if (!_allowOutputAppendCheckBox.Checked)
            {
                if (!_outputFileTextBox.Text.Contains("<") && !_outputFileTextBox.Text.Contains("$"))
                {
                    _outputFileTextBox.Focus();
                    UtilityMethods.ShowMessageBox(
                        "Either path tags or functions must be used to define a unique filename " +
                        "for each image area or image areas should be allowed to be appended to " +
                        "an existing output file",
                        "Invalid output setting", false);
                    return true;
                }
                else if (_allAreasRadioButton.Checked &&
                         !_outputFileTextBox.Text.Contains(ExtractImageAreaTask._AREA_ID_TAG))
                {
                    _outputFileTextBox.Focus();
                    UtilityMethods.ShowMessageBox(
                        "If more than one area may be extracted per image and image areas may not " +
                        "be appended to an existing output file, the <AreaID> tag must be used " +
                        "to guarantee an unique output file name.",
                        "Output file must use <AreaID>", false);
                    return true;
                }
            }

            return false;
        }

        #endregion Private Members
    }
}
