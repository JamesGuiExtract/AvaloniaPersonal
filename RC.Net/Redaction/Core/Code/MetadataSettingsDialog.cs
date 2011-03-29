using System;
using System.Windows.Forms;
using Extract.Utilities.Forms;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a dialog that allows the user to select metadata settings.
    /// </summary>
    public partial class MetadataSettingsDialog : Form
    {
        #region Fields
		
        /// <summary>
        /// Settings for creating redaction metadata xml.
        /// </summary>
        MetadataSettings _settings;
 
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="MetadataSettingsDialog"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public MetadataSettingsDialog() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">Settings for creating redaction metadata xml.</param>
        public MetadataSettingsDialog(MetadataSettings settings)
        {
            InitializeComponent();

            _settings = settings ?? new MetadataSettings();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="MetadataSettings"/>.
        /// </summary>
        /// <value>Settings for creating redaction metadata xml.</value>
        public MetadataSettings MetadataSettings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
            }
        }
		 
        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the <see cref="MetadataSettings"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="MetadataSettings"/> from the user interface.</returns>
        MetadataSettings GetMetadataSettings()
        {
            // Get the settings
            string dataFile = _dataFileControl.DataFile;
            string metadataFile = _metadataFileTextBox.Text;

            return new MetadataSettings(dataFile, metadataFile);
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if 
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            bool isEmpty = string.IsNullOrEmpty(_metadataFileTextBox.Text);
            if (isEmpty)
            {
                MessageBox.Show("Please enter the output file.", "Invalid output file",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                _metadataFileTextBox.Focus();
            }

            return isEmpty;
        }
		 
        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                _dataFileControl.DataFile = _settings.DataFile;
                _metadataFileTextBox.Text = _settings.MetadataFile;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28509", ex);
            }
        }
		 
        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="BrowseButton.PathSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="BrowseButton.PathSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="BrowseButton.PathSelected"/> event.</param>
        void HandleMetadataBrowseButtonPathSelected(object sender, PathSelectedEventArgs e)
        {
            try
            {
                _metadataFileTextBox.Text = e.Path;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26309", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
            try
            {
                if (WarnIfInvalid())
                {
                    return;
                }

                // Store settings
                _settings = GetMetadataSettings();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28510", ex);
            }
        }
		 
        #endregion Event Handlers
    }
}