using AttributeDbMgrComponentsLib;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Allows dynamic creation of attribute set names for <see cref="StoreAttributesInDBTask"/>.
    /// </summary>
    public partial class AddAttributeSetDialog : Form
    {
        #region Fields

        /// <summary>
        /// The <see cref="AttributeDBMgr"/> to use to create new attribute set names.
        /// </summary>
        AttributeDBMgr _attributeDBManager;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Name of the newly created attribute, iff DialogResult.IDOK
        /// </summary>
        public String AttributeSetName { get; private set; }

        #endregion Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="AddAttributeSetDialog"/> class.
        /// </summary>
        /// <param name="attributeDBManager">The <see cref="AttributeDBMgr"/> to use to create new
        /// attribute set names.</param>
        [CLSCompliant(false)]
        public AddAttributeSetDialog(AttributeDBMgr attributeDBManager)
        {
            try
            {
                _attributeDBManager = attributeDBManager;
                InitializeComponent();
                OkButton.Enabled = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39209");
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="OkButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                _attributeDBManager.CreateNewAttributeSetName(AttributeSetNameTextBox.Text);

                AttributeSetName = AttributeSetNameTextBox.Text;
            }
            catch (Exception ex)
            {
                DialogResult = DialogResult.None;
                ex.ExtractDisplay("ELI39210");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event of the
        /// <see cref="AttributeSetNameTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        private void HandleTextChanged_AttributeSetNameTextBox(object sender, EventArgs e)
        {
            var text = AttributeSetNameTextBox.Text;
            OkButton.Enabled = !String.IsNullOrWhiteSpace(text);
        }

        #endregion Event Handlers
    }
}
