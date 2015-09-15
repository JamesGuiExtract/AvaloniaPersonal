using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UCLID_AttributeDbMgrComponentsLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify settings for an
    /// <see cref="StoreAttributesInDBTask"/> instance.
    /// </summary>
    public partial class StoreAttributesInDBTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name used for license validation.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(StoreAttributesInDBTaskSettingsDialog).ToString();

        /// <summary>
        /// Special value in the attribute set name drop down to allow a new attribute set name to
        /// be added.
        /// </summary>
        static readonly string _ADD_NEW = "<Add new...>";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Provides the ability to retrieve and add additional attribute set names.
        /// </summary>
        AttributeDBMgr _attributeDBManager;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAttributesInDBTaskSettingsDialog"/> class.
        /// </summary>
        public StoreAttributesInDBTaskSettingsDialog()
            : this(null)
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38656");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAttributesInDBTaskSettingsDialog"/> class.
        /// </summary>
        public StoreAttributesInDBTaskSettingsDialog(StoreAttributesInDBTask settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI38657",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38658");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public StoreAttributesInDBTask Settings
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

                _voaFileNameTextBox.Text = Settings.VOAFileName;

                var fileProcessingDB = new FileProcessingDB();
                fileProcessingDB.ConnectLastUsedDBThisProcess();
                _attributeDBManager = new AttributeDBMgr();
                _attributeDBManager.FAMDB = fileProcessingDB;

// TODO
//                _attributeSetNameComboBox.Items.AddRange(
//                    _attributeDBManager.GetAttributeSetNames().ToIEnumerable());
                _attributeSetNameComboBox.Items.Add(_ADD_NEW);

                if (!string.IsNullOrWhiteSpace(Settings.AttributeSetName))
                {
                    var selectedItem = _attributeSetNameComboBox.Items.OfType<object>()
                        .Where(item => Settings.AttributeSetName.Equals(item.ToString(),
                                StringComparison.OrdinalIgnoreCase))
                        .SingleOrDefault();

                    if (selectedItem != null)
                    {
                        _attributeSetNameComboBox.SelectedItem = selectedItem;
                    }
                    else
                    {
                        UtilityMethods.ShowMessageBox(
                            string.Format(CultureInfo.CurrentCulture,
                            "The attribute set name \"{0}\" no longer exists in the database.",
                            Settings.AttributeSetName), "Invalid attribute set name", true);
                    }
                }

                _storeRasterZonesCheckBox.Checked = Settings.StoreRasterZones;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38659");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of the
        /// <see cref="_attributeSetNameComboBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleAttributeSetNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_attributeSetNameComboBox.Text == _ADD_NEW)
                {
                    UtilityMethods.ShowMessageBox("TODO: UI to add attribute set name", "TODO", true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38660");
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
                if (WarnIfInvalid())
                {
                    return;
                }

                Settings.VOAFileName = _voaFileNameTextBox.Text;
                Settings.AttributeSetName = _attributeSetNameComboBox.Text;
                Settings.StoreRasterZones = _storeRasterZonesCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI38661", ex);
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
            if (string.IsNullOrWhiteSpace(_voaFileNameTextBox.Text))
            {
                _voaFileNameTextBox.Focus();
                MessageBox.Show("Please specify the name of the VOA file containing the attributes to store.",
                    "Missing VOA filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_attributeSetNameComboBox.Text.ToString()))
            {
                _attributeSetNameComboBox.Focus();
                MessageBox.Show("Please specify the attribute set name the attributes should be stored under.",
                    "Missing attribute set name", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
