using Extract.FileActionManager.Forms;
using Extract.Utilities;
using System;
using System.Threading;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// A <see cref="Form"/> used to configure settings for <see cref="ComClass"/>.
    /// </summary>
    public partial class ConfigurationForm : Form
    {
        #region Fields

        /// <summary>
        /// The <see cref="VerificationSettings"/> instance for which configuration is being
        /// performed.
        /// </summary>
        VerificationSettings _settings;

        /// <summary>
        /// Specifies which tags should be available to the users.
        /// </summary>
        FileTagSelectionSettings _tagSettings;

        /// <summary>
        /// The <see cref="FileProcessingDB"/>.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ConfigurationForm"/> instance for the specified
        /// <see cref="VerificationSettings"/> instance.
        /// </summary>
        /// <param name="settings">The <see cref="VerificationSettings"/> instance which is to be
        /// configured.
        /// </param>
        public ConfigurationForm(VerificationSettings settings)
        {
            try
            {
                ExtractException.Assert("ELI25475", "Null argument exception!", settings != null);

                InitializeComponent();

                FAMDBUtils dbUtils = new FAMDBUtils();
                Type mgrType = Type.GetTypeFromProgID(dbUtils.GetFAMDBProgId());
                _fileProcessingDB = (FileProcessingDB)Activator.CreateInstance(mgrType);

                _fileProcessingDB.ConnectLastUsedDBThisProcess();

                _fileNameTagsButton.PathTags = new FileActionManagerPathTags();
                _settings = new VerificationSettings(settings);
                _tagSettings = _settings.TagSettings;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25492", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="VerificationSettings"/> represented in the configuration dialog.
        /// </summary>
        /// <value>
        /// The <see cref="VerificationSettings"/> represented in the configuration dialog.
        /// </value>
        public VerificationSettings Settings
        {
            get
            {
                return _settings;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Displays the form to allow the user to specify settings.
        /// </summary>
        /// <returns><see langword="true"/> if the user applied the settings or
        /// <see langworld="false"/> if configuration was cancelled.</returns>
        public bool Configure()
        {
            try
            {
                // Initialize the form's controls
                _configFileNameTextBox.Text = _settings.ConfigFileName;
                _enableInputTrackingCheckBox.Checked = _settings.InputEventTrackingEnabled;
                _enableCountersCheckBox.Checked = _settings.CountersEnabled;
                _allowTagsCheckBox.Checked = _settings.AllowTags;
                _tagSettingsButton.Enabled = _allowTagsCheckBox.Checked;

                // Display the form modally and wait for the result
                if (ShowDialog() == DialogResult.OK)
                {
                    _settings = new VerificationSettings(
                        _configFileNameTextBox.Text,
                        _enableInputTrackingCheckBox.Checked,
                        _enableCountersCheckBox.Checked,
                        _allowTagsCheckBox.Checked,
                        _tagSettings);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI25468");
            }
        }

        #endregion Methods

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
                ex.ExtractDisplay("ELI37238");
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
                    new FileTagSelectionDialog(_settings.TagSettings, _fileProcessingDB))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _tagSettings = new FileTagSelectionSettings(dialog.Settings);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37220");
            }
        }

        /// <summary>
        /// Handles the case that the user clicked the browse button for the configuration file.
        /// An open file dialog is created and displayed to allow the user to browse to the
        /// configuration file of their choosing.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleFileBrowseButtonClick(object sender, EventArgs e)
        {
            try
            {
                // The OpenFileDialog cless will not work in a multithreaded apartment. Initialize a
                // new thread in single thread apartment mode for the open file dialog.
                Thread browseForFileThread = new Thread(new ParameterizedThreadStart(BrowseForFile));
                browseForFileThread.SetApartmentState(ApartmentState.STA);

                // Run the thread to allow selection of the parameter for _configFileNameTextBox.
                browseForFileThread.Start(_configFileNameTextBox);

                // Allow the parent's UI to be updated while the open file dialog is open.
                while (browseForFileThread.IsAlive)
                {
                    Application.DoEvents();

                    // Prevent the loop from using to much CPU.
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25477", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays an <see cref="OpenFileDialog"/> to allow the user to select a filename to
        /// populate the provided <see cref="TextBox"/>
        /// </summary>
        /// <param name="fileNameTextBoxObject">A <see cref="TextBox"/> whose value should be
        /// updated via the <see cref="OpenFileDialog"/>.</param>
        void BrowseForFile(Object fileNameTextBoxObject)
        {
            try
            {
                TextBox fileNameTextBox = fileNameTextBoxObject as TextBox;
                ExtractException.Assert("ELI25484", "Null argument exception!", 
                    fileNameTextBox != null);

                // Create and initialized the OpenFileDialog
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = fileNameTextBox.Text;
                openFileDialog.Filter = "Configuration files (*.config)|*.config|All files (*.*)|*.*";

                // Display the dialog and check the result
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // If the user pressed OK on the file open dialog, apply the result.
                    UpdateTextBoxValue(fileNameTextBox, openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25483", ex);
            }
        }

        /// <summary>
        /// Updates a <see cref="TextBox"/> value from any thread.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to updated.</param>
        /// <param name="value">The value to apply to the <see cref="TextBox"/>.</param>
        void UpdateTextBoxValue(TextBox textBox, string value)
        {
            // If not running in the UI thread than an invoke is required
            if (base.InvokeRequired)
            {
                // Invoke this call in the UI thread.
                base.Invoke(new UpdateTextBoxParameterDelegate(UpdateTextBoxValue), 
                    new object[] { textBox, value });

                // Just return as the Invoke will take care of updating the UI
                return;
            }

            // If in the UI thread, apply the specified value.
            textBox.Text = value;
        }

        #endregion Private Members

        #region Delegates

        /// <summary>
        /// Delegate for <see cref="UpdateTextBoxValue"/>.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to updated.</param>
        /// <param name="value">The value to apply to the <see cref="TextBox"/>.</param>
        delegate void UpdateTextBoxParameterDelegate(TextBox textBox, string value);

        #endregion Delegates
    }
}
