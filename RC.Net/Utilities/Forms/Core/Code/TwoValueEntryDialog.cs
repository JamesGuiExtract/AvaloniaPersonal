using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Dialog class for prompting the user for two values.
    /// </summary>
    public partial class TwoValueEntryDialog : Form
    {
        #region Constants

        /// <summary>
        /// Name used in licensing validation.
        /// </summary>
        static readonly string _COMPONENT_NAME = typeof(TwoValueEntryDialog).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Whether or not empty string is valid in the entry boxes
        /// </summary>
        bool _allowEmptyString;

        /// <summary>
        /// Gets or sets the first value.
        /// </summary>
        /// <value>
        /// The first value.
        /// </value>
        public string FirstValue { get; set; }

        /// <summary>
        /// Gets or sets the second value.
        /// </summary>
        /// <value>
        /// The second value.
        /// </value>
        public string SecondValue { get; set; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoValueEntryDialog"/> class.
        /// </summary>
        public TwoValueEntryDialog() : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoValueEntryDialog"/> class.
        /// </summary>
        /// <param name="allowEmptyString">if set to <see langword="true"/> will
        /// allow <see cref="String.Empty"/> as a valid value in the entry boxes.</param>
        public TwoValueEntryDialog(bool allowEmptyString)
            : this(allowEmptyString, "", "", "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoValueEntryDialog"/> class.
        /// </summary>
        /// <param name="caption">The caption to display.</param>
        /// <param name="firstLabel">The label for the first entry box.</param>
        /// <param name="secondLabel">The label for the second entry box.</param>
        public TwoValueEntryDialog(string caption, string firstLabel, string secondLabel)
            : this(true, caption, firstLabel, secondLabel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoValueEntryDialog"/> class.
        /// </summary>
        /// <param name="allowEmptyString">if set to <see langword="true"/> will
        /// allow <see cref="String.Empty"/> as a valid value in the entry boxes.</param>
        /// <param name="caption">The caption to display.</param>
        /// <param name="firstLabel">The label for the first entry box.</param>
        /// <param name="secondLabel">The label for the second entry box.</param>
        public TwoValueEntryDialog(bool allowEmptyString,
            string caption, string firstLabel, string secondLabel)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI31703", _COMPONENT_NAME);

                InitializeComponent();

                _allowEmptyString = allowEmptyString;

                _label1.Text = firstLabel ?? string.Empty;
                _label2.Text = secondLabel ?? string.Empty;

                Text = caption ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31704");
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _textValue1.Text = FirstValue ?? string.Empty;
                _textValue2.Text = SecondValue ?? string.Empty;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31705");
            }
        }

        /// <summary>
        /// Handles the ok clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOkClicked(object sender, EventArgs e)
        {
            try
            {
                if (!_allowEmptyString)
                {
                    bool empty = false;
                    if (string.IsNullOrEmpty(_textValue1.Text))
                    {
                        empty = true;
                        _textValue1.Focus();
                    }
                    else if (string.IsNullOrEmpty(_textValue2.Text))
                    {
                        empty = true;
                        _textValue2.Focus();
                    }

                    if (empty)
                    {
                        MessageBox.Show("Empty entries are not allowed. Please specify a value.",
                            "Cannot Leave Data Empty", MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                        return;
                    }
                }

                FirstValue = _textValue1.Text;
                SecondValue = _textValue2.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31706");
            }
        }

        #endregion Event Handlers
    }
}
