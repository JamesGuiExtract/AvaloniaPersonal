using Extract.Utilities;
using System;
using System.IO;
using System.Windows.Forms;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// A dialog to allow for the configuration of redaction verification slideshow user options.
    /// </summary>
    public partial class SlideshowUserOptionsDialog : Form
    {
        #region Fields

        /// <summary>
        /// The config file used to retrieve/store the options.
        /// </summary>
        readonly ConfigSettings<Properties.Settings> _config =
            new ConfigSettings<Properties.Settings>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SlideshowUserOptionsDialog"/> class.
        /// </summary>
        public SlideshowUserOptionsDialog()
        {
            try
            {
                InitializeComponent();

                _autoStartCheckBox.Checked = _config.Settings.AutoStartSlideshow;
                _intervalUpDown.Value = _config.Settings.SlideshowInterval;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31115", ex);
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the ok button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_intervalUpDown.Value < 1 || _intervalUpDown.Value > 999)
                {
                    MessageBox.Show("The number of seconds must be a value between 1 and 999",
                        "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.None,
                        MessageBoxDefaultButton.Button1, 0);
                    _intervalUpDown.Focus();
                    return;
                }

                _config.Settings.AutoStartSlideshow = _autoStartCheckBox.Checked;
                _config.Settings.SlideshowInterval = (int)_intervalUpDown.Value;
                _config.Save();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31116", ex);
            }
        }

        #endregion Event Handlers
    }
}
