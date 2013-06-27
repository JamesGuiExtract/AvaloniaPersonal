using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Allows the advanced settings of a <see cref="SendEmailTask"/> instance to be configured.
    /// </summary>
    public partial class SendEmailTaskAdvancedDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(SendEmailTaskAdvancedDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailTaskAdvancedDialog"/> class.
        /// </summary>
        public SendEmailTaskAdvancedDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailTaskAttachmentsDialog"/> class.
        /// </summary>
        /// <param name="settings"><see cref="SendEmailTask"/> for which attachments are to be
        /// selected.</param>
        /// <param name="pathTags">The <see cref="IPathTags"/> instance to be used for all path tags
        /// buttons in the dialog.</param>
        public SendEmailTaskAdvancedDialog(SendEmailTask settings, IPathTags pathTags)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI35973",
                    _OBJECT_NAME);

                InitializeComponent();
                
                Settings = settings;
                _pathTagsButton.PathTags = pathTags;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35974");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public SendEmailTask Settings
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
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35975");
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
                Settings.DataFileName = _dataFileTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35976", ex);
            }
        }

        #endregion Event Handlers
    }
}
