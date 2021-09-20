using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.Forms
{
    public partial class PasswordComplexityRequirementsDialog : Form
    {
        public PasswordComplexityRequirements Settings { get; private set; }

        public PasswordComplexityRequirementsDialog()
            : this(new PasswordComplexityRequirements())
        {
        }

        public PasswordComplexityRequirementsDialog(PasswordComplexityRequirements requirements)
        {
            InitializeComponent();

            _lengthNumericUpDown.Minimum = 1;
            _lengthNumericUpDown.Maximum = Math.Max(100, requirements.LengthRequirement);
            Settings = requirements;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _lengthNumericUpDown.Value = Settings.LengthRequirement;
            _uppercaseCheckBox.Checked = Settings.RequireUppercase;
            _lowercaseCheckBox.Checked = Settings.RequireLowercase;
            _numberCheckBox.Checked = Settings.RequireDigit;
            _punctuationCheckBox.Checked = Settings.RequirePunctuation;
        }

        private void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                Settings =
                    new(lengthRequirement: (int)_lengthNumericUpDown.Value,
                        requireLowercase: _lowercaseCheckBox.Checked,
                        requireDigit: _numberCheckBox.Checked,
                        requirePunctuation: _punctuationCheckBox.Checked,
                        requireUppercase: _uppercaseCheckBox.Checked);
            }
            catch (Exception ex)
            {
                DialogResult = DialogResult.None;
                ex.ExtractDisplay("ELI51875");
            }
        }
    }
}
