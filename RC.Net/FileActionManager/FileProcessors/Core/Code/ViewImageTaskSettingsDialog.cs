using Extract.FileActionManager.Forms;
using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    ///  A <see cref="Form"/> to view and modify settings for an <see cref="ViewImageTask"/> instance.
    /// </summary>
    public partial class ViewImageTaskSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// Specifies which tags should be available to the users.
        /// </summary>
        FileTagSelectionSettings _tagSettings;

        /// <summary>
        /// The <see cref="FileProcessingDB"/> the task is to be run against.
        /// </summary>
        IFileProcessingDB _fileProcessingDB;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewImageTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">A <see cref="ViewImageTask"/> instance containing the settings
        /// to use.</param>
        public ViewImageTaskSettingsDialog(ViewImageTask settings)
        {
            try
            {
                InitializeComponent();

                FAMDBUtils dbUtils = new FAMDBUtils();
                Type mgrType = Type.GetTypeFromProgID(dbUtils.GetFAMDBProgId());
                _fileProcessingDB = (FileProcessingDB)Activator.CreateInstance(mgrType);

                _fileProcessingDB.ConnectLastUsedDBThisProcess();

                Settings = settings;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37252");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="ViewImageTask"/> to configure.
        /// </summary>
        /// <value>
        /// The <see cref="ViewImageTask"/> to configure.
        /// </value>
        public ViewImageTask Settings
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

                _allowTagsCheckBox.Checked = Settings.AllowTags;
                _tagSettings = new FileTagSelectionSettings(Settings.TagSettings);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37253");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_allowTagsCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleAllowTagsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _tagSettingsButton.Enabled = _allowTagsCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37254");
            }
        }

        /// <summary>
        /// Handles the <see cref="_tagSettingsButton"/> <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTagSettingsButtonClick(object sender, EventArgs e)
        {
            try
            {
                using (FileTagSelectionDialog dialog =
                    new FileTagSelectionDialog(_tagSettings, _fileProcessingDB))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _tagSettings = dialog.Settings;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37255");
            }
        }

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
                Settings.AllowTags = _allowTagsCheckBox.Checked;
                Settings.TagSettings = _tagSettings;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37256");
            }
        }

        #endregion Event Handlers
    }
}
