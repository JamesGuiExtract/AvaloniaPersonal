using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Dialog to configure <see cref="MoveCopyAttributes"/> object
    /// </summary>
    [CLSCompliant(false)]
    public partial class MoveCopyAttributesSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(MoveCopyAttributesSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes dialog with <see cref="MoveCopyAttributes"/> settings
        /// </summary>
        /// <param name="settings">The <see cref="MoveCopyAttributes"/> object that has the settings</param>
        public MoveCopyAttributesSettingsDialog(MoveCopyAttributes settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                                                 "ELI46935",
                                                 _OBJECT_NAME);
                Settings = settings;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46932");
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="MoveCopyAttributes"/> to configure
        /// </summary>
        public MoveCopyAttributes Settings { get; set; }

        #endregion

        #region Overrides

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _sourceXPathTextBox.Text = Settings.SourceAttributeTreeXPath;
                _destinationXPathtextBox.Text = Settings.DestinationAttributeTreeXPath;
                _copyRadioButton.Checked = Settings.CopyAttributes;
                _moveRadioButton.Checked = !Settings.CopyAttributes;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46933");
            }
        }

        #endregion

        #region Event Handlers

        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_sourceXPathTextBox.Text) || !UtilityMethods.IsValidXPathExpression(_sourceXPathTextBox.Text))
                {
                    CustomizableMessageBox cmb = new CustomizableMessageBox
                    {
                        Caption = "Invalid",
                        Text = "Source XPath is not valid",
                        UseDefaultOkButton = true
                    };
                    cmb.Show(this);
                    DialogResult = DialogResult.None;
                    _sourceXPathTextBox.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(_destinationXPathtextBox.Text) || !UtilityMethods.IsValidXPathExpression(_destinationXPathtextBox.Text))
                {
                    CustomizableMessageBox cmb = new CustomizableMessageBox
                    {
                        Caption = "Invalid",
                        Text = "Destination XPath is not valid",
                        UseDefaultOkButton = true
                    };
                    cmb.Show(this);
                    DialogResult = DialogResult.None;
                    _destinationXPathtextBox.Focus();
                    return;
                }

                if (_sourceXPathTextBox.Text.Equals(_destinationXPathtextBox.Text, StringComparison.OrdinalIgnoreCase))
                {
                    CustomizableMessageBox cmb = new CustomizableMessageBox
                    {
                        Caption = "Invalid",
                        Text = "Source and destination XPath is cannot be the same.",
                        UseDefaultOkButton = true
                    };
                    cmb.Show(this);
                    DialogResult = DialogResult.None;
                    _destinationXPathtextBox.Focus();
                    return;
                }

                Settings.SourceAttributeTreeXPath = _sourceXPathTextBox.Text;
                Settings.DestinationAttributeTreeXPath = _destinationXPathtextBox.Text;
                Settings.CopyAttributes = _copyRadioButton.Checked;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46934");
            }
        }

        #endregion
    }
}
