using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Indicates how to determine if a VOA file should be created when redacted output is created
    /// for verification in stand-alone mode and a VOA file doesn't already exist.
    /// </summary>
    public enum OnDemandCreateVOAFileMode
    {
        /// <summary>
        /// Do not created a VOA file.
        /// </summary>
        DoNotCreate,
        
        /// <summary>
        /// Prompt for whether to create a VOA file.
        /// </summary>
        Prompt,

        /// <summary>
        /// Always create a VOA file.
        /// </summary>
        Create
    }

    /// <summary>
    /// Represents a dialog that allows the user to select verification options.
    /// </summary>
    public partial class VerificationOptionsDialog : Form
    {
        #region VerificationOptionsDialog Fields

        /// <summary>
        /// Settings for the verification task.
        /// </summary>
        VerificationSettings _taskSettings;

        /// <summary>
        /// The config file used to retrieve/store the options for the slideshow.
        /// </summary>
        readonly ConfigSettings<Properties.Settings> _config =
            new ConfigSettings<Properties.Settings>();

        /// <summary>
        /// The set of keys that should be available for the user to select as the slideshow run key.
        /// </summary>
        static readonly Dictionary<Keys, string> _runKeyOptions = InitializeRunKeyOptions();

        #endregion VerificationOptionsDialog Fields

        #region VerificationOptionsDialog Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationOptionsDialog"/> class.
        /// </summary>
        /// <param name="taskSettings">The task settings.</param>
        /// <param name="standAloneMode"><see langword="true"/> if the verification session is being
        /// run independent of the FAM and database; <see langword="false"/> otherwise.</param>
        public VerificationOptionsDialog(VerificationSettings taskSettings, bool standAloneMode)
        {
            InitializeComponent();

            _taskSettings = taskSettings;

            // Fill the tool combo box from the enum
            var autoToolType = typeof(AutoTool);
            var names = new List<string>(Enum.GetNames(autoToolType));
            names.Remove(Enum.GetName(autoToolType, AutoTool.None));
            _autoToolComboBox.Items.AddRange(names.ToArray());

            _slideshowAutoStartCheckBox.Enabled = !_taskSettings.SlideshowSettings.RequireRunKey;
            _slideshowAutoStartCheckBox.Checked = _config.Settings.AutoStartSlideshow;
            _slideshowIntervalUpDown.Value = _config.Settings.SlideshowInterval;
            _slideshowIntervalUpDown.UserTextCorrected += HandleSlideshowIntervalCorrected;

            // If the slideshow is not enabled, remove the slideshow settings tab.
            if (!_taskSettings.SlideshowSettings.SlideshowEnabled)
            {
                this._tabControl.TabPages.Remove(_slideshowTabPage);
                _slideshowTabPage.Dispose();
                _slideshowTabPage = null;
            }
            
            // If not running in standalone mode, removed the OnDemand settings tab.
            if (!standAloneMode)
            {
                _tabControl.Controls.Remove(_onDemandTabPage);
                _onDemandTabPage.Dispose();
                _onDemandTabPage = null;
            }
        }

        #endregion VerificationOptionsDialog Constructors

        #region VerificationOptionsDialog Methods

        /// <summary>
        /// Gets the <see cref="AutoTool"/> specified by the user interface.
        /// </summary>
        /// <returns>The <see cref="AutoTool"/> specified by the user interface.</returns>
        AutoTool GetAutoTool()
        {
            if (_autoToolCheckBox.Checked)
            {
                var tool = (AutoTool)Enum.Parse(typeof(AutoTool), _autoToolComboBox.Text, true);
                return tool;
            }

            return AutoTool.None;
        }

        /// <summary>
        /// Gets the <see cref="OnDemandCreateVOAFileMode"/> setting specified in the UI.
        /// </summary>
        /// <returns>The <see cref="OnDemandCreateVOAFileMode"/> settings specified in the UI.
        /// </returns>
        OnDemandCreateVOAFileMode GetOnDemandCreateVOAFileMode()
        {
            if (_createVOAFileRadioButton.Checked)
            {
                return OnDemandCreateVOAFileMode.Create;
            }
            else if (_promptVOAFileRadioButton.Checked)
            {
                return OnDemandCreateVOAFileMode.Prompt;
            }
            
            return OnDemandCreateVOAFileMode.DoNotCreate;
        }

        /// <summary>
        /// Updates the enabled state of the controls.
        /// </summary>
        void UpdateControls()
        {
            // Enable/disable auto zoom
            bool autoZoom = _autoZoomCheckBox.Checked;
            _autoZoomScaleTrackBar.Enabled = autoZoom;
            _autoZoomScaleTextBox.Enabled = autoZoom;

            // Set the auto zoom scale
            int value = _autoZoomScaleTrackBar.Value;
            _autoZoomScaleTextBox.Text = value.ToString(CultureInfo.CurrentCulture);

            // Set the auto tool
            _autoToolComboBox.Enabled = _autoToolCheckBox.Checked;

            _ocrTradeOffLabel.Enabled = _OCRCheckBox.Checked;
            _ocrTradeOffLabel2.Enabled = _OCRCheckBox.Checked;
            _ocrTradeOffComboBox.Enabled = _OCRCheckBox.Checked;
        }

        #endregion VerificationOptionsDialog Methods

        #region VerificationOptionsDialog Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Initialize UI controls based on persisted values.
                AutoTool autoTool = _config.Settings.AutoTool;
                _autoToolCheckBox.Checked = autoTool != AutoTool.None;
                _autoToolComboBox.Text = Enum.GetName(typeof(AutoTool), autoTool);
                _autoZoomCheckBox.Checked = _config.Settings.AutoZoom;
                _autoZoomScaleTrackBar.Value = _config.Settings.AutoZoomScale;
                _OCRCheckBox.Checked = _config.Settings.AutoOcr;

                switch (_config.Settings.OcrTradeoff)
                {
                    case Imaging.OcrTradeoff.Accurate:
                        _ocrTradeOffComboBox.SelectedItem = "Accurate";
                        break;

                    case Imaging.OcrTradeoff.Balanced:
                        _ocrTradeOffComboBox.SelectedItem = "Balanced";
                        break;

                    case Imaging.OcrTradeoff.Fast:
                        _ocrTradeOffComboBox.SelectedItem = "Fast";
                        break;

                    default:
                        _ocrTradeOffComboBox.SelectedItem = "Balanced";
                        break;
                }

                // If the slideshow is enabled, populate all _runKeyOptions which can be recognized
                // on this system.
                if (_taskSettings.SlideshowSettings.SlideshowEnabled)
                {
                    foreach (string value in _runKeyOptions.Values)
                    {
                        _slideshowRunKeyComboBox.Items.Add(value);
                    }

                    _slideshowRunKeyComboBox.Text = _runKeyOptions[_config.Settings.SlideshowRunKey];
                }

                // If OnDemand tab is available initialize the OnDemand settings.
                if (_onDemandTabPage != null)
                {
                    _createVOAFileRadioButton.Checked =
                        _config.Settings.OnDemandCreateVOAFileMode == OnDemandCreateVOAFileMode.Create;
                    _promptVOAFileRadioButton.Checked =
                        _config.Settings.OnDemandCreateVOAFileMode == OnDemandCreateVOAFileMode.Prompt;
                    _doNotCreateVOAFileRadioButton.Checked =
                        _config.Settings.OnDemandCreateVOAFileMode == OnDemandCreateVOAFileMode.DoNotCreate;
                }

                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27407", ex);
            }
        }

        #endregion VerificationOptionsDialog Overrides

        #region VerificationOptionsDialog Event Handlers

        /// <summary>
        /// Handles the case that the user entered a slideshow interval that the control corrected
        /// due to being out of the valid range, etc.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSlideshowIntervalCorrected(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("The number of seconds must be a value between 1 and 99",
                        "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.None,
                        MessageBoxDefaultButton.Button1, 0);

                // Re-select the _slideshowIntervalUpDown control, but only after any other events
                // in the message queue have been processed so those event don't undo this selection.
                BeginInvoke((MethodInvoker)(() =>
                {
                    _tabControl.SelectedTab = _slideshowTabPage;
                    _slideshowIntervalUpDown.Focus();
                }));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31592");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            bool slideshowIntervalValid = true;
            EventHandler<EventArgs> handleUserTextCorrected =
                ((tempSender, tempEventArgs) => slideshowIntervalValid = false);

            try
            {
                if (_taskSettings.SlideshowSettings.SlideshowEnabled)
                {
                    // Before checking the _slideshowIntervalUpDown value, register
                    // slideshowIntervalValid to be set to false in the case that UserTextCorrected is
                    // raised.
                    _slideshowIntervalUpDown.UserTextCorrected += handleUserTextCorrected;
                    _config.Settings.SlideshowInterval = (int)_slideshowIntervalUpDown.Value;
                }

                // Store settings and close the dialog only if slideshowIntervalValid.
                if (slideshowIntervalValid)
                {
                    _config.Settings.AutoTool = GetAutoTool();
                    _config.Settings.AutoZoom = _autoZoomCheckBox.Checked;
                    _config.Settings.AutoZoomScale = _autoZoomScaleTrackBar.Value;
                    _config.Settings.OnDemandCreateVOAFileMode = GetOnDemandCreateVOAFileMode();
                    _config.Settings.AutoStartSlideshow = _slideshowAutoStartCheckBox.Checked;
                    _config.Settings.AutoOcr = _OCRCheckBox.Checked;

                    if (string.Compare(_ocrTradeOffComboBox.Text, "Accurate",
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        _config.Settings.OcrTradeoff = Imaging.OcrTradeoff.Accurate;
                    }
                    else if (string.Compare(_ocrTradeOffComboBox.Text, "Fast",
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        _config.Settings.OcrTradeoff = Imaging.OcrTradeoff.Fast;
                    }
                    else
                    {
                        _config.Settings.OcrTradeoff = Imaging.OcrTradeoff.Balanced;
                    }

                    // Apply any newly selected slideshow run key.
                    if (_taskSettings.SlideshowSettings.SlideshowEnabled &&
                        _slideshowRunKeyComboBox.Text != _runKeyOptions[_config.Settings.SlideshowRunKey])
                    {
                        _config.Settings.SlideshowRunKey = _runKeyOptions
                            .Where(option => option.Value == _slideshowRunKeyComboBox.Text)
                            .Single()
                            .Key;
                    }

                    _config.Save();

                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI27409");
            }
            finally
            {
                _slideshowIntervalUpDown.UserTextCorrected -= handleUserTextCorrected;
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        void HandleAutoZoomCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI27410");
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        void HandleAutoToolCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI27411");
            }
        }

        /// <summary>
        /// Handles the <see cref="TrackBar.ValueChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="TrackBar.ValueChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="TrackBar.ValueChanged"/> event.</param>
        void HandleAutoZoomScaleTrackBarValueChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI27412");
            }
        }

        /// <summary>
        /// Handles the CheckChanged event for the auto OCR checkbox.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOcrCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32629");
            }
        }

        #endregion VerificationOptionsDialog Event Handlers

        #region Private Members

        static Dictionary<Keys, string> InitializeRunKeyOptions()
        {
            Dictionary<Keys, string> runKeyOptions = new Dictionary<Keys, string>();

            runKeyOptions[Keys.LShiftKey] = "Left shift key";
            runKeyOptions[Keys.RShiftKey] = "Right shift key";
            runKeyOptions[Keys.LControlKey] = "Left control key";
            runKeyOptions[Keys.RControlKey] = "Right control key";
            runKeyOptions[Keys.LMenu] = "Left alt key";
            runKeyOptions[Keys.RMenu] = "Right alt key";

            Dictionary<Keys, string> result = new Dictionary<Keys, string>();

            foreach (var option in runKeyOptions
                        .Where(option => KeyMethods.IsRecognizedKey(option.Key, true)))
            {
                result[option.Key] = option.Value;
            }

            return result;
        }

        #endregion Private Members
    }
}