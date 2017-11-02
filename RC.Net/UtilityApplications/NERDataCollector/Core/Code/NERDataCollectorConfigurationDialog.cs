using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.NERDataCollector
{
    /// <summary>
    /// Dialog to configure and run attribute labeling
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class NERDataCollectorConfigurationDialog : Form
    {
        #region Fields

        // Flag to short-circuit value-changed handler
        private bool _suspendUpdatesToSettingsObject;

        private NERDataCollector _settings;
        private bool _dirty;


        private static readonly string _TITLE_TEXT = "NER training data collector";
        private static readonly string _TITLE_TEXT_DIRTY = "*" + _TITLE_TEXT;
        private string _databaseServer;
        private string _databaseName;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether anything has been modified since loading
        /// </summary>
        private bool Dirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                _dirty = value;
                Text = _dirty
                    ? _TITLE_TEXT_DIRTY
                    : _TITLE_TEXT;
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates a configuration dialogue for an <see cref="NERDataCollector"/>
        /// </summary>
        /// <param name="collector">The instance to configure</param>
        /// <param name="databaseServer">The server to use to resolve MLModel.Names and AttributeSetNames</param>
        /// <param name="databaseName">The database to use to resolve MLModel.Names and AttributeSetNames</param>
        public NERDataCollectorConfigurationDialog(NERDataCollector collector, string databaseServer, string databaseName)
        {
            try
            {
                _settings = collector;
                _databaseServer = databaseServer;
                _databaseName = databaseName;

                InitializeComponent();

                SetControlValues();
            }
            catch (Exception ex)
            {
                _settings = new NERDataCollector();
                ex.ExtractDisplay("ELI45043");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                if (string.IsNullOrWhiteSpace(_databaseServer) || string.IsNullOrWhiteSpace(_databaseName))
                {
                    var fpdb = new FileProcessingDB
                    {
                        DatabaseServer = _databaseServer,
                        DatabaseName = _databaseName
                    };
                    fpdb.ShowSelectDB("Select database", false, false);
                    _databaseServer = fpdb.DatabaseServer;
                    _databaseName = fpdb.DatabaseName;
                }
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI45045").Display();
            }
        }

        #endregion Overrides

        #region Event Handlers


        /// <summary>
        /// Writes the field values to the settings object
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                _settings.ModelName = _modelNameTextBox.Text;
                _settings.AttributeSetName = _attributeSetNameTextBox.Text;
                _settings.AnnotatorSettingsPath = _annotatorSettingsPathTextBox.Text;
                _settings.LastIDProcessed = (int)_lastIDProcessedNumericUpDown.Value;

                Dirty = false;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45047");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }


        /// <summary>
        /// Update settings from UI controls
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_suspendUpdatesToSettingsObject)
                {
                    return;
                }

                Dirty = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45048");
            }
        }


        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Sets the control values from the settings object.
        /// </summary>
        private void SetControlValues()
        {
            try
            {
                _suspendUpdatesToSettingsObject = true;

                _attributeSetNameTextBox.Text = _settings.AttributeSetName;
                _modelNameTextBox.Text = _settings.ModelName;
                _annotatorSettingsPathTextBox.Text = _settings.AnnotatorSettingsPath;
                _lastIDProcessedNumericUpDown.Value = _settings.LastIDProcessed;

                Dirty = false;

            }
            finally
            {
                _suspendUpdatesToSettingsObject = false;
            }
        }

        #endregion Private Methods
    }
}