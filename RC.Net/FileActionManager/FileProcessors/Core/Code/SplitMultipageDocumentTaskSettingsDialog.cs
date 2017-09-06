using Extract.FileActionManager.Forms;
using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify settings for an
    /// <see cref="SplitMultipageDocumentTask"/> instance.
    /// </summary>
    public partial class SplitMultipageDocumentTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name used for license validation.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(SplitMultipageDocumentTaskSettingsDialog).ToString();


        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitMultipageDocumentTaskSettingsDialog"/> class.
        /// </summary>
        public SplitMultipageDocumentTaskSettingsDialog()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitMultipageDocumentTaskSettingsDialog"/> class.
        /// </summary>
        public SplitMultipageDocumentTaskSettingsDialog(SplitMultipageDocumentTask settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI44854",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                var outputFileNamePathTags = new FileActionManagerPathTags();
                outputFileNamePathTags.AddTag(SplitMultipageDocumentTask.PageNumberTag, "");
                _outputPathTagButton.PathTags = outputFileNamePathTags;

                _voaPathTagButton.PathTags = new FileActionManagerPathTags();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44855");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public SplitMultipageDocumentTask Settings
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

                _outputPathTextBox.Text = Settings.OutputPath;
                _voaPathTextBox.Text = Settings.VOAPath;

                Invalidate(true);
                Refresh();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44851");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> event.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (WarnIfInvalid())
                {
                    return;
                }

                Settings.OutputPath = _outputPathTextBox.Text;
                Settings.VOAPath = _voaPathTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI44852", ex);
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
            if (string.IsNullOrWhiteSpace(_outputPathTextBox.Text))
            {
                _outputPathTextBox.Focus();
                MessageBox.Show("Please specify the path to use for output files.",
                    "Missing output path", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
