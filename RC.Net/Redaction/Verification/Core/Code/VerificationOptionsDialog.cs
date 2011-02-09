using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a dialog that allows the user to select <see cref="VerificationOptions"/>.
    /// </summary>
    public partial class VerificationOptionsDialog : Form
    {
        #region VerificationOptionsDialog Fields

        /// <summary>
        /// Settings for the <see cref="VerificationTaskForm"/> user interface.
        /// </summary>
        VerificationOptions _options;

        /// <summary>
        /// The config file used to retrieve/store the options for the slideshow.
        /// </summary>
        readonly ConfigSettings<Properties.Settings> _config =
            new ConfigSettings<Properties.Settings>();

        #endregion VerificationOptionsDialog Fields

        #region VerificationOptionsDialog Constructors

        /// <summary>
        /// Initializes a new <see cref="VerificationOptionsDialog"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public VerificationOptionsDialog()
            : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationOptionsDialog"/> class.
        /// </summary>
        public VerificationOptionsDialog(VerificationOptions options)
        {
            InitializeComponent();

            _options = options ?? new VerificationOptions();

            // Fill the tool combo box from the enum
            var autoToolType = typeof(AutoTool);
            var names = new List<string>(Enum.GetNames(autoToolType));
            names.Remove(Enum.GetName(autoToolType, AutoTool.None));
            _autoToolComboBox.Items.AddRange(names.ToArray());

            _slideshowAutoStartCheckBox.Checked = _config.Settings.AutoStartSlideshow;
            _slideshowIntervalUpDown.Value = _config.Settings.SlideshowInterval;
            _slideshowIntervalUpDown.UserTextCorrected += HandleSlideshowIntervalCorrected;

        }

        #endregion VerificationOptionsDialog Constructors

        #region VerificationOptionsDialog Properties

        /// <summary>
        /// Gets or sets the <see cref="VerificationOptions"/>.
        /// </summary>
        /// <value>The <see cref="VerificationOptions"/>.</value>
        /// <returns>The <see cref="VerificationOptions"/>.</returns>
        public VerificationOptions VerificationOptions
        {
            get
            {
                return _options;
            }
            set
            {
                _options = value;
            }
        }

        #endregion VerificationOptionsDialog Properties

        #region VerificationOptionsDialog Methods

        /// <summary>
        /// Gets the <see cref="VerificationOptions"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="VerificationOptions"/> from the user interface.</returns>
        VerificationOptions GetVerificationOptions()
        {
            bool autoZoom = _autoZoomCheckBox.Checked;
            int autoZoomScale = _autoZoomScaleTrackBar.Value;
            AutoTool autoTool = GetAutoTool();

            return new VerificationOptions(autoZoom, autoZoomScale, autoTool);
        }

        /// <summary>
        /// Gets the <see cref="AutoTool"/> specified by the user interface.
        /// </summary>
        /// <returns>The <see cref="AutoTool"/> specified by the user interface.</returns>
        AutoTool GetAutoTool()
        {
            if (_autoToolCheckBox.Checked)
            {
                var tool = (AutoTool) Enum.Parse(typeof(AutoTool), _autoToolComboBox.Text, true);
                return tool;
            }

            return AutoTool.None;
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

                // Set auto zoom settings
                _autoZoomCheckBox.Checked = _options.AutoZoom;
                _autoZoomScaleTrackBar.Value = _options.AutoZoomScale;

                // Set auto tool settings
                AutoTool autoTool = _options.AutoTool;
                _autoToolCheckBox.Checked = autoTool != AutoTool.None;
                _autoToolComboBox.Text = Enum.GetName(typeof(AutoTool), autoTool);

                // [FlexIDSCore:4528]
                // With an Aero theme, the tab control the _autoZoomScaleTrackBar is drawn on
                // is transparent and, therefore, inherits the SystemColors.Window color. Otherwise,
                // ensure the _autoZoomScaleTrackBar assumes the same color as the _generalTabPage.
                if (_generalTabPage.BackColor == Color.Transparent)
                {
                    _autoZoomScaleTrackBar.BackColor = SystemColors.Window;
                }
                else
                {
                    _autoZoomScaleTrackBar.BackColor = _generalTabPage.BackColor;
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
                ExtractException.Display("ELI31592", ex);
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
                // Before checking the _slideshowIntervalUpDown value, register
                // slideshowIntervalValid to be set to false in the case that UserTextCorrected is
                // raised.
                _slideshowIntervalUpDown.UserTextCorrected += handleUserTextCorrected;
                _config.Settings.AutoStartSlideshow = _slideshowAutoStartCheckBox.Checked;
                _config.Settings.SlideshowInterval = (int)_slideshowIntervalUpDown.Value;

                // Store settings and close the dialog only if slideshowIntervalValid.
                if (slideshowIntervalValid)
                {
                    _options = GetVerificationOptions();
                    _config.Save();

                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27409", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                ExtractException ee = ExtractException.AsExtractException("ELI27410", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                ExtractException ee = ExtractException.AsExtractException("ELI27411", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                ExtractException ee = ExtractException.AsExtractException("ELI27412", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion VerificationOptionsDialog Event Handlers
    }
}