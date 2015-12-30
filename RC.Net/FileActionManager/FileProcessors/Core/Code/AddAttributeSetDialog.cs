using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    public partial class AddAttributeSetDialog : Form
    {
        /// <summary>
        /// Name of the newly created attribute, iff DialogResult.IDOK
        /// </summary>
        public String AttributeSetName { get; private set; }

        /// <summary>
        /// CTOR
        /// </summary>
        public AddAttributeSetDialog()
        {
            InitializeComponent();
            OkButton.Enabled = false;
        }

        #region Event Handlers
        private void OkButton_Click(object sender, EventArgs e)
        {
            AttributeSetName = AttributeSetNameTextBox.Text;
        }

        private void HandleTextChanged_AttributeSetNameTextBox(object sender, EventArgs e)
        {
            var text = AttributeSetNameTextBox.Text;
            OkButton.Enabled = !String.IsNullOrWhiteSpace(text);
        }
        #endregion
    }
}
